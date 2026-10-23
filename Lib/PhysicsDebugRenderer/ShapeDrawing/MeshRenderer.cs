// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BepuUtilities;
using BepuUtilities.Collections;
using DemoContentLoader;
using OpenTK.Graphics.OpenGL4;

namespace DemoRenderer.ShapeDrawing
{
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct MeshInstance
    {
        [FieldOffset(0)]
        public Vector3 Position;
        [FieldOffset(12)]
        public uint PackedColor;
        [FieldOffset(16)]
        public ulong PackedOrientation;
        [FieldOffset(24)]
        public int VertexStart;
        [FieldOffset(28)]
        public int VertexCount;
        [FieldOffset(32)]
        public Vector3 Scale;
        //Local bounding radius (distance from the local origin to the farthest vertex). Used only on
        //the CPU for frustum culling; the vertex shader reads this slot as its unused 'Padding' float,
        //so adding it here changes neither the 48-byte stride nor any other field's offset.
        [FieldOffset(44)]
        public float BoundingRadius;
    }
    public class MeshRenderer : Shader
    {
        private readonly MeshCache meshCache;
        private readonly ConstantsBuffer<RasterizedVertexConstants> vertexConstants;
        private readonly StructuredBuffer<MeshInstance> instances;

        public MeshRenderer(MeshCache meshCache, ContentArchive content, int maximumInstancesPerDraw = 10000) : base(
            content.Load<GLSLContent>(@"ShapeDrawing\RenderMeshes.glvs").Source,
            content.Load<GLSLContent>(@"ShapeDrawing\RenderMeshes.glfs").Source
        )
        {
            this.meshCache = meshCache;
            instances = new StructuredBuffer<MeshInstance>(BufferTarget.ShaderStorageBuffer, maximumInstancesPerDraw, $"Mesh Instances");
            vertexConstants = new ConstantsBuffer<RasterizedVertexConstants>(BufferTarget.UniformBuffer, debugName: $"Mesh Renderer Vertex Constants");
        }

        /// <param name="allocationPool">
        /// Pool used for the transient per-draw batching lists. When the <see cref="MeshCache"/> is
        /// shared with the physics (shard) thread (as in the PIN debug window), pass the GL thread's
        /// own pool so these window-thread allocations never touch the shard's pool - which is
        /// concurrently in use by the simulation and would otherwise be corrupted. When null, the
        /// cache's pool is used (the original single-threaded demo case).
        /// </param>
        public unsafe void Render(Camera camera, Int2 screenResolution, Span<MeshInstance> instances, int start, int count, BepuUtilities.Memory.BufferPool allocationPool = null)
        {
            Use();
            vertexConstants.Bind(0);
            this.instances.Bind(0);
            meshCache.TriangleBuffer.Bind(1);

            //The batching bookkeeping lives on this (GL) thread, so it must use a pool that is not
            //simultaneously in use by the thread that owns the mesh cache (the physics shard).
            var pool = allocationPool ?? meshCache.Pool;

            //Frustum culling of instances whose world bounding sphere lies entirely outside the view.
            //The planes describe the SAME frustum the GPU rasterizes (see CullingFrustum).
            var frustum = new CullingFrustum(camera);

            //Examine the set of instances and batch them into groups using the same mesh data.
            var batches = new QuickDictionary<ulong, QuickList<MeshInstance>, PrimitiveComparer<ulong>>(16, pool);
            var end = start + count;
            for (int i = start; i < end; ++i)
            {
                ref var instance = ref instances[i];

                //Frustum cull: skip instances whose world bounding sphere (centered at the instance
                //position, radius = local bounding radius * max scale) lies entirely outside the view.
                var scale = instance.Scale;
                var worldRadius = instance.BoundingRadius * MathF.Max(scale.X, MathF.Max(scale.Y, scale.Z));
                if (frustum.IsSphereOutside(instance.Position, worldRadius))
                {
                    continue;
                }

                ref var id = ref Unsafe.As<int, ulong>(ref instance.VertexStart);

                if (batches.GetTableIndices(ref id, out var tableIndex, out var elementIndex))
                {
                    //The id was already present.
                    batches.Values[elementIndex].Add(instance, pool);
                }
                else
                {
                    //There is no batch for this vertex region, so create one.
                    var newCount = batches.Count + 1;
                    if (newCount > batches.Keys.Length)
                    {
                        batches.Resize(newCount, pool);
                        //Resizing will change the table indices, so we have to grab it again.
                        batches.GetTableIndices(ref id, out tableIndex, out _);
                    }
                    batches.Keys[batches.Count] = id;
                    ref var listSlot = ref batches.Values[batches.Count];
                    listSlot = new QuickList<MeshInstance>(64, pool);
                    listSlot.Add(instance, pool);
                    batches.Table[tableIndex] = newCount;
                    batches.Count = newCount;
                }
            }

            var vertexConstantsData = new RasterizedVertexConstants
            {
                Projection = camera.Projection,
                CameraPosition = camera.Position,
                CameraRight = camera.Right,
                CameraUp = camera.Up,
                CameraBackward = camera.Backward,
            };
            vertexConstants.Update(ref vertexConstantsData);

            for (int i = 0; i < batches.Count; ++i)
            {
                ref var batch = ref batches.Values[i];
                var batchVertexCount = batch[0].VertexCount;
                while (batch.Count > 0)
                {
                    var subbatchStart = Math.Max(0, batch.Count - this.instances.Capacity);
                    var subbatchCount = batch.Count - subbatchStart;
                    this.instances.Update(new Span<MeshInstance>(batch.Span.Memory + subbatchStart, subbatchCount));
                    GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, batchVertexCount, subbatchCount);
                    batch.Count -= subbatchCount;
                }
                batch.Dispose(pool);
            }
            batches.Dispose(pool);
        }

        protected override void DoDispose()
        {
            instances.Dispose();
            vertexConstants.Dispose();
            base.DoDispose();
        }
    }
}
