// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
#nullable enable
using System;
using System.Numerics;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using OpenTK.Graphics.OpenGL4;

namespace DemoRenderer.ShapeDrawing
{
    /// <summary>
    /// Stores references to triangle data between usages to avoid the need to regather it every frame.
    /// </summary>
    public class MeshCache : Disposable
    {
        Buffer<Vector4> vertices;
        //The GPU buffer is created lazily on the GL (window) thread, because this cache is
        //constructed on the physics (shard) thread which has no current GL context.
        private StructuredBuffer<Vector4>? triangleBuffer;
        private readonly int initialCapacity;
        private readonly object glLock = new object();
        //All mesh ids currently occupying a region in the ring allocator (for enumeration when
        //reclaiming). Mutated on the shard thread during Allocate / ReclaimStaleMeshes under the
        //cache lock.
        QuickSet<ulong, PrimitiveComparer<ulong>> allocatedIds;
        //The ids referenced by the most recent extraction (the frame being filled) and the one
        //before it (the frame still possibly being rendered by the GL thread, due to double
        //buffering). A region is only reclaimable once it is in NEITHER set. Kept as sets so
        //membership is O(1) - the reclaim must not run an O(n^2) diff on the render thread.
        QuickSet<ulong, PrimitiveComparer<ulong>> currentLiveIds;
        QuickSet<ulong, PrimitiveComparer<ulong>> previousLiveIds;
        //The id bookkeeping is mutated on the shard thread (Allocate / ReclaimStaleMeshes) and read
        //on the GL thread (FlushPendingUploads only reads the GPU-relevant state). It is kept on its
        //own cache-private pool - never the simulation's pool - so concurrent simulation
        //allocations cannot corrupt it. The set objects themselves are serialized by the cache lock.
        private readonly BufferPool bookkeepingPool = new BufferPool();

        struct UploadRequest
        {
            public int Start;
            public int Count;
        }
        QuickList<UploadRequest> pendingUploads;

        public BufferPool Pool { get; private set; }
        Allocator allocator;
        //Largest power-of-two vertex count whose byte size (x sizeof(Vector4)) still fits in a single
        //BufferPool suballocation (2^MaximumSpanSizePower = 2^30 bytes). Beyond this a single
        //Buffer<Vector4> cannot be allocated, so the cache hard-caps here. 2^27 * 16 = 2^30.
        private const int maxCapacity = 1 << 27;
        public MeshCache(BufferPool pool, int initialSizeInVertices = 1 << 23)
        {
            Pool = pool;
            this.initialCapacity = initialSizeInVertices;
            pool.TakeAtLeast(initialSizeInVertices, out vertices);
            allocator = new Allocator(initialSizeInVertices, pool);

            //Bookkeeping collections live on the cache-private pool (see bookkeepingPool), not the
            //simulation's pool, so cross-thread mutation (shard Allocate vs GL FlushPendingUploads)
            //cannot corrupt the simulation's pool state.
            pendingUploads = new QuickList<UploadRequest>(128, bookkeepingPool);
            allocatedIds = new QuickSet<ulong, PrimitiveComparer<ulong>>(10000, bookkeepingPool);
            currentLiveIds = new QuickSet<ulong, PrimitiveComparer<ulong>>(10000, bookkeepingPool);
            previousLiveIds = new QuickSet<ulong, PrimitiveComparer<ulong>>(10000, bookkeepingPool);
            uploadedThisGeneration = new QuickSet<ulong, PrimitiveComparer<ulong>>(10000, bookkeepingPool);
        }

        /// <summary>
        /// The GPU triangle buffer. Must be accessed from the GL thread; created on first use via
        /// EnsureTriangleBuffer.
        /// </summary>
        public StructuredBuffer<Vector4> TriangleBuffer => EnsureTriangleBuffer(allocator.Capacity);

        /// <summary>
        /// Creates the GPU triangle buffer if needed (on the GL thread) and grows it to at least the
        /// given capacity. Safe to call from the GL thread; guarded so a concurrent shard-thread
        /// Allocate (which only reads capacity) cannot observe a half-built buffer.
        /// </summary>
        private StructuredBuffer<Vector4> EnsureTriangleBuffer(long requiredCapacity)
        {
            var buffer = triangleBuffer;
            if (buffer == null)
            {
                lock (glLock)
                {
                    if (triangleBuffer == null)
                    {
                        triangleBuffer = new StructuredBuffer<Vector4>(BufferTarget.ShaderStorageBuffer,
                            (int)Math.Max(requiredCapacity, initialCapacity), "Mesh Cache Vertex Buffer");
                    }
                    buffer = triangleBuffer;
                }
            }
            else if (requiredCapacity > buffer.Capacity)
            {
                buffer.SetCapacityWithoutCopy((int)requiredCapacity);
            }
            return buffer;
        }

        public bool TryGetExistingMesh(ulong id, out int start, out Buffer<Vector4> vertices)
        {
            if (allocator.TryGetAllocationRegion(id, out var allocation))
            {
                start = (int)allocation.Start;
                vertices = this.vertices.Slice(start, (int)(allocation.End - start));
                return true;
            }
            start = default;
            vertices = default;
            return false;
        }

        /// <summary>
        ///     Starts a new extraction pass. Advances the generation counter so that the next
        ///     <see cref="Allocate" /> calls treat previously-allocated mesh ids as fresh (re-uploading
        ///     them) unless they were already uploaded earlier within this same pass. Called on the
        ///     shard thread before extracting a (re)extraction's statics.
        /// </summary>
        public void BeginExtraction()
        {
            uploadedThisGeneration.Clear();
            //Roll the live sets forward: the extraction we are about to build becomes "current", the
            //one we just built (still possibly on screen) becomes "previous". A region is only
            //reclaimable once it has fallen out of both (see ReclaimStaleMeshes). Swapping the two
            //set variables is O(1); only the set that becomes "current" is cleared.
            var tmp = currentLiveIds;
            currentLiveIds = previousLiveIds;
            previousLiveIds = tmp;
            currentLiveIds.FastClear();
        }

        private QuickSet<ulong, PrimitiveComparer<ulong>> uploadedThisGeneration;

        /// <summary>
        ///     Places a mesh's vertex region in the shared ring buffer. The id is always recorded as
        ///     requested (so the GL thread's flush can later reclaim meshes that stream out of view).
        ///     Three outcomes: (1) already uploaded earlier in this extraction generation - the region is
        ///     valid and no upload is needed (<c>needsUpload == false</c>); (2) freshly allocated (the
        ///     buffer may have grown) - the region is valid and the caller must upload it
        ///     (<c>needsUpload == true</c>); (3) the cache is full - the region is invalid and the caller
        ///     must skip the instance (returns <c>false</c>, <c>needsUpload == false</c>).
        /// </summary>
        public bool Allocate(ulong id, int vertexCount, out int start, out Buffer<Vector4> vertices, out bool needsUpload)
        {
            //Record the id as live for this extraction so ReclaimStaleMeshes keeps it (it is
            //referenced by the frame being built). Set additions are idempotent.
            currentLiveIds.Add(id, bookkeepingPool);
            //If the id already occupies a ring region, never re-allocate it (the allocator asserts on
            //duplicate ids). Two sub-cases: (a) it was uploaded in the CURRENT extraction generation -
            //its data is already in the GPU buffer and the region is stable, so reuse with no upload;
            //(b) it was uploaded by an OLDER generation that has not yet been reclaimed (its data is
            //stale and its slot may be reused), so re-upload into the existing region to refresh it.
            if (allocator.Contains(id))
            {
                if (TryGetExistingMesh(id, out start, out vertices))
                {
                    if (uploadedThisGeneration.Contains(id))
                    {
                        needsUpload = false;
                        return true;
                    }
                    //Stale region from a previous extraction: rewrite + re-upload into the same region.
                    pendingUploads.Add(new UploadRequest { Start = start, Count = vertexCount }, bookkeepingPool);
                    allocatedIds.Add(id, bookkeepingPool);
                    uploadedThisGeneration.Add(id, bookkeepingPool);
                    needsUpload = true;
                    return true;
                }
            }
            if (allocator.Allocate(id, vertexCount, out var longStart))
            {
                start = (int)longStart;
                vertices = this.vertices.Slice(start, vertexCount);
                pendingUploads.Add(new UploadRequest { Start = start, Count = vertexCount }, bookkeepingPool);
                allocatedIds.Add(id, bookkeepingPool);
                uploadedThisGeneration.Add(id, bookkeepingPool);
                needsUpload = true;
                return true;
            }
            //Didn't fit in the current capacity. Grow the ring (power of two) to hold the existing
            //allocations plus this one, but never past the pool's single-buffer hard cap.
            var oldCapacity = (long)allocator.Capacity;
            var needed = BitOperations.RoundUpToPowerOf2((ulong)(oldCapacity + vertexCount));
            if (needed > maxCapacity)
            {
                //The vertex data for this mesh would push the single Buffer<Vector4> past the pool's
                //2^30-byte limit (a power-of-two capacity above maxCapacity cannot be allocated). The
                //cache is full; report failure so the caller can skip this instance rather than
                //overflowing int in the resize math and corrupting the buffer.
                start = 0;
                vertices = default;
                needsUpload = false;
                return false;
            }
            var newSize = (int)needed;
            //ResizeToAtLeast copies `copyCount` existing elements into the new (larger) buffer. Passing
            //the whole used region preserves prior allocations; the ring allocator keeps them valid.
            Pool.ResizeToAtLeast(ref this.vertices, newSize, (int)oldCapacity);
            // The allocator is a ring buffer: it may wrap and place an allocation at a start where
            // start + count <= Capacity but > the buffer's Length if the two ever diverge (the pool
            // rounds the buffer length up to a power-of-2 allocation, which can differ from newSize).
            // Slice() validates against the buffer's Length, so pin the ring capacity to the actual
            // buffer length to guarantee every allocation stays within the buffer's extent.
            allocator.Capacity = this.vertices.Length;
            if (!allocator.Allocate(id, vertexCount, out longStart))
            {
                // The ring is fragmented (the free space is not contiguous), so the new region cannot be
                // placed even though the total free space is sufficient. Growing further risks pushing the
                // single Buffer<Vector4> past the pool's 2^30-byte cap and corrupting memory, so fail
                // safely and let the caller skip this instance.
                start = 0;
                vertices = default;
                needsUpload = false;
                return false;
            }
            start = (int)longStart;
            vertices = this.vertices.Slice(start, vertexCount);
            //A resize forces an upload of everything, so any previous pending uploads are unnecessary.
            pendingUploads.Count = 0;
            pendingUploads.Add(new UploadRequest { Start = 0, Count = (int)(oldCapacity + vertexCount) }, bookkeepingPool);
            allocatedIds.Add(id, bookkeepingPool);
            uploadedThisGeneration.Add(id, bookkeepingPool);
            needsUpload = true;
            return true;
        }

        /// <summary>
        ///     Frees ring regions that are no longer referenced by either the most recent extraction
        ///     (the frame being filled) or the one before it (the frame the GL thread may still be
        ///     rendering). Called on the shard thread once per extraction, after all <see cref="Allocate" />
        ///     calls, under the cache lock. Membership is O(1) (sets), so this is O(allocated) and never
        ///     runs an O(n^2) diff on the render thread.
        /// </summary>
        public void ReclaimStaleMeshes()
        {
            for (int i = 0; i < allocatedIds.Count; ++i)
            {
                var id = allocatedIds.Span[i];
                if (!currentLiveIds.Contains(id) && !previousLiveIds.Contains(id))
                {
                    allocator.Deallocate(id);
                }
            }
            //Shrink the allocated set to exactly the surviving ids so the next pass enumerates only
            //live regions. (Freed ids are dropped; surviving ids are re-added.)
            allocatedIds.FastClear();
            for (int i = 0; i < currentLiveIds.Count; ++i)
            {
                allocatedIds.Add(currentLiveIds.Span[i], bookkeepingPool);
            }
            for (int i = 0; i < previousLiveIds.Count; ++i)
            {
                allocatedIds.Add(previousLiveIds.Span[i], bookkeepingPool);
            }
        }

        public unsafe void FlushPendingUploads()
        {
            //Only GPU work: upload the regions the shard wrote this pass. Reclamation of out-of-view
            //meshes happens on the shard (ReclaimStaleMeshes), NOT here - doing an id-membership scan
            //on the render thread every frame is what made rendering O(n^2) in the live mesh count.
            var triangleBuffer = EnsureTriangleBuffer(allocator.Capacity);
            for (int i = 0; i < pendingUploads.Count; ++i)
            {
                ref var upload = ref pendingUploads[i];
                triangleBuffer.Update(new Span<Vector4>(vertices.Memory, vertices.Length), upload.Count, upload.Start, upload.Start);
            }
            pendingUploads.Count = 0;

            //Note: the original Bepu cache ran IncrementalCompact here to close ring gaps. That is
            //intentionally NOT done while a frame is live - compaction moves a region to a new offset,
            //invalidating the VertexStart values the published frame's instances reference (which are
            //never rewritten), and would render that frame's meshes at the wrong buffer offset.
        }

        /// <summary>
        /// Disposes only the GPU triangle buffer (created on the GL thread). Used when this cache is
        /// shared with a renderer: the renderer disposes the GPU side on the GL thread at teardown
        /// while the CPU state (vertex buffer, allocator, id sets) is disposed separately by the
        /// owning (shard) thread.
        /// </summary>
        public void DisposeGpuResources()
        {
            lock (glLock)
            {
                triangleBuffer?.Dispose();
                triangleBuffer = null;
            }
        }

        protected override void DoDispose()
        {
            //The GPU buffer is disposed on the GL thread (it was created there).
            lock (glLock)
            {
                triangleBuffer?.Dispose();
                triangleBuffer = null;
            }
            pendingUploads.Dispose(bookkeepingPool);
            Pool.Return(ref vertices);
            allocatedIds.Dispose(bookkeepingPool);
            currentLiveIds.Dispose(bookkeepingPool);
            previousLiveIds.Dispose(bookkeepingPool);
            uploadedThisGeneration.Dispose(bookkeepingPool);
            allocator.Dispose();
            bookkeepingPool.Clear();
        }
    }
}
