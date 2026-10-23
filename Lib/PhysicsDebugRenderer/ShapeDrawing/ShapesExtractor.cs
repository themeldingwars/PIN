// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
// Adapted for the PIN physics debug renderer: extraction writes into a caller-provided
// ShapeCache (part of a double-buffered frame) instead of a long-lived member cache, so the
// physics (shard) thread can fill a snapshot that the GL (window) thread renders independently.
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;

namespace DemoRenderer.ShapeDrawing
{
    public struct ShapeCache
    {
        public QuickList<SphereInstance> Spheres;
        public QuickList<CapsuleInstance> Capsules;
        public QuickList<CylinderInstance> Cylinders;
        public QuickList<BoxInstance> Boxes;
        public QuickList<TriangleInstance> Triangles;
        public QuickList<MeshInstance> Meshes;

        public ShapeCache(int initialCapacityPerShapeType, BufferPool pool)
        {
            Spheres = new QuickList<SphereInstance>(initialCapacityPerShapeType, pool);
            Capsules = new QuickList<CapsuleInstance>(initialCapacityPerShapeType, pool);
            Cylinders = new QuickList<CylinderInstance>(initialCapacityPerShapeType, pool);
            Boxes = new QuickList<BoxInstance>(initialCapacityPerShapeType, pool);
            Triangles = new QuickList<TriangleInstance>(initialCapacityPerShapeType, pool);
            Meshes = new QuickList<MeshInstance>(initialCapacityPerShapeType, pool);
        }
        public void Clear()
        {
            Spheres.Count = 0;
            Capsules.Count = 0;
            Cylinders.Count = 0;
            Boxes.Count = 0;
            Triangles.Count = 0;
            Meshes.Count = 0;
        }
        public void Dispose(BufferPool pool)
        {
            Spheres.Dispose(pool);
            Capsules.Dispose(pool);
            Cylinders.Dispose(pool);
            Boxes.Dispose(pool);
            Triangles.Dispose(pool);
            Meshes.Dispose(pool);
        }
    }

    /// <summary>
    /// Extracts dynamic/kinematic body shapes and static shapes from a Bepu <see cref="Simulation"/>
    /// into a caller-provided <see cref="ShapeCache"/>. Reads the simulation (Bepu) - so it must run
    /// on the thread that owns the simulation (the shard thread), never the GL thread.
    /// </summary>
    public class ShapesExtractor : IDisposable
    {
        BufferPool pool;
        public MeshCache MeshCache;

        ParallelLooper looper;
        public ShapesExtractor(ParallelLooper looper, BufferPool pool)
        {
            this.MeshCache = new MeshCache(pool);
            this.pool = pool;
            this.looper = looper;
        }

        private void AddCompoundChildren(ref Buffer<CompoundChild> children, Shapes shapes, RigidPose pose, Vector3 color, ref ShapeCache shapeCache, BufferPool pool)
        {
            for (int i = 0; i < children.Length; ++i)
            {
                ref var child = ref children[i];
                RigidPose childPose;
                Compound.GetRotatedChildPose(child.LocalPosition, child.LocalOrientation, pose.Orientation, out childPose.Position, out childPose.Orientation);
                childPose.Position += pose.Position;
                AddShape(shapes, child.ShapeIndex, childPose, color, ref shapeCache, pool);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void AddShape(void* shapeData, int shapeType, Shapes shapes, RigidPose pose, Vector3 color, ref ShapeCache shapeCache, BufferPool pool)
        {
            switch (shapeType)
            {
                case Sphere.Id:
                    {
                        SphereInstance instance;
                        instance.Position = pose.Position;
                        instance.Radius = Unsafe.AsRef<Sphere>(shapeData).Radius;
                        Helpers.PackOrientation(pose.Orientation, out instance.PackedOrientation);
                        instance.PackedColor = Helpers.PackColor(color);
                        shapeCache.Spheres.Add(instance, pool);
                    }
                    break;
                case Capsule.Id:
                    {
                        CapsuleInstance instance;
                        instance.Position = pose.Position;
                        ref var capsule = ref Unsafe.AsRef<Capsule>(shapeData);
                        instance.Radius = capsule.Radius;
                        instance.HalfLength = capsule.HalfLength;
                        instance.PackedOrientation = Helpers.PackOrientationU64(pose.Orientation);
                        instance.PackedColor = Helpers.PackColor(color);
                        shapeCache.Capsules.Add(instance, pool);
                    }
                    break;
                case Box.Id:
                    {
                        BoxInstance instance;
                        instance.Position = pose.Position;
                        ref var box = ref Unsafe.AsRef<Box>(shapeData);
                        instance.PackedColor = Helpers.PackColor(color);
                        instance.Orientation = pose.Orientation;
                        instance.HalfWidth = box.HalfWidth;
                        instance.HalfHeight = box.HalfHeight;
                        instance.HalfLength = box.HalfLength;
                        shapeCache.Boxes.Add(instance, pool);
                    }
                    break;
                case Triangle.Id:
                    {
                        ref var triangle = ref Unsafe.AsRef<Triangle>(shapeData);
                        TriangleInstance instance;
                        instance.A = triangle.A;
                        instance.PackedColor = Helpers.PackColor(color);
                        instance.B = triangle.B;
                        instance.C = triangle.C;
                        instance.PackedOrientation = Helpers.PackOrientationU64(pose.Orientation);
                        instance.X = pose.Position.X;
                        instance.Y = pose.Position.Y;
                        instance.Z = pose.Position.Z;
                        shapeCache.Triangles.Add(instance, pool);
                    }
                    break;
                case Cylinder.Id:
                    {
                        CylinderInstance instance;
                        instance.Position = pose.Position;
                        ref var cylinder = ref Unsafe.AsRef<Cylinder>(shapeData);
                        instance.Radius = cylinder.Radius;
                        instance.HalfLength = cylinder.HalfLength;
                        instance.PackedOrientation = Helpers.PackOrientationU64(pose.Orientation);
                        instance.PackedColor = Helpers.PackColor(color);
                        shapeCache.Cylinders.Add(instance, pool);
                    }
                    break;
                case ConvexHull.Id:
                    {
                        ref var hull = ref Unsafe.AsRef<ConvexHull>(shapeData);
                        MeshInstance instance;
                        instance.Position = pose.Position;
                        instance.PackedColor = Helpers.PackColor(color);
                        instance.PackedOrientation = Helpers.PackOrientationU64(pose.Orientation);
                        instance.Scale = Vector3.One;
                        //Memory can be reused, so we slightly reduce the probability of a bad reuse by taking the first 64 bits of data into the hash.
                        var id = (ulong)hull.Points.Memory ^ (ulong)hull.Points.Length ^ (*(ulong*)hull.Points.Memory);
                        bool meshExisted;
                        Buffer<Vector4> vertices;
                        lock (MeshCache)
                        {
                            meshExisted = MeshCache.TryGetExistingMesh(id, out instance.VertexStart, out vertices);
                        }
                        bool instanceValid = true;
                        if (!meshExisted)
                        {
                            int triangleCount = 0;
                            for (int i = 0; i < hull.FaceToVertexIndicesStart.Length; ++i)
                            {
                                hull.GetVertexIndicesForFace(i, out var faceVertexIndices);
                                triangleCount += faceVertexIndices.Length - 2;
                            }
                            instance.VertexCount = triangleCount * 3;
                            //Allocate AND upload the vertex data inside the same MeshCache lock. The
                            //upload writes into the shared ring buffer that the GL thread mutates
                            //concurrently (FlushPendingUploads -> IncrementalCompact/Deallocate); doing
                            //it outside the lock let the write race that mutation and corrupt the
                            //buffer. Serializing the write with the allocation keeps the buffer valid.
                            lock (MeshCache)
                            {
                                bool needsUpload;
                                instanceValid = MeshCache.Allocate(id, instance.VertexCount, out instance.VertexStart, out vertices, out needsUpload);
                                //Only a fresh allocation (needsUpload) has vertex data that must be
                                //written. A mesh already uploaded earlier in this pass needs no write,
                                //and a failed allocation (cache full) must be skipped so we never write
                                //into an empty/invalid region (that would be an out-of-bounds write).
                                if (instanceValid && needsUpload)
                                {
                                    int targetVertexIndex = 0;
                                    for (int i = 0; i < hull.FaceToVertexIndicesStart.Length; ++i)
                                    {
                                        hull.GetVertexIndicesForFace(i, out var faceVertexIndices);
                                        hull.GetPoint(faceVertexIndices[0], out var faceOrigin);
                                        hull.GetPoint(faceVertexIndices[1], out var previousEdgeEnd);
                                        for (int j = 2; j < faceVertexIndices.Length; ++j)
                                        {
                                            vertices[targetVertexIndex++] = new Vector4(faceOrigin, 1.0f);
                                            vertices[targetVertexIndex++] = new Vector4(previousEdgeEnd, 1.0f);
                                            hull.GetPoint(faceVertexIndices[j], out previousEdgeEnd);
                                            vertices[targetVertexIndex++] = new Vector4(previousEdgeEnd, 1.0f);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            instance.VertexCount = vertices.Length;
                        }
                        //Local bounding radius (distance from the local origin to the farthest hull
                        //point), used on the CPU for frustum culling. Walk the face vertices (which
                        //cover every hull point) via GetPoint, since the Points buffer is SIMD-packed.
                        float maxRadiusSq = 0f;
                        for (int i = 0; i < hull.FaceToVertexIndicesStart.Length; ++i)
                        {
                            hull.GetVertexIndicesForFace(i, out var radiusFaceIndices);
                            for (int j = 0; j < radiusFaceIndices.Length; ++j)
                            {
                                hull.GetPoint(radiusFaceIndices[j], out var radiusPoint);
                                maxRadiusSq = MathF.Max(maxRadiusSq, radiusPoint.LengthSquared());
                            }
                        }
                        instance.BoundingRadius = MathF.Sqrt(maxRadiusSq);
                        if (instanceValid)
                        {
                            shapeCache.Meshes.Add(instance, pool);
                        }
                    }
                    break;
                case Compound.Id:
                    {
                        AddCompoundChildren(ref Unsafe.AsRef<Compound>(shapeData).Children, shapes, pose, color, ref shapeCache, pool);
                    }
                    break;
                case BigCompound.Id:
                    {
                        AddCompoundChildren(ref Unsafe.AsRef<BigCompound>(shapeData).Children, shapes, pose, color, ref shapeCache, pool);
                    }
                    break;
                case Mesh.Id:
                    {
                        ref var mesh = ref Unsafe.AsRef<Mesh>(shapeData);
                        MeshInstance instance;
                        instance.Position = pose.Position;
                        instance.PackedColor = Helpers.PackColor(color);
                        instance.PackedOrientation = Helpers.PackOrientationU64(pose.Orientation);
                        instance.Scale = mesh.Scale;
                        //Memory can be reused, so we slightly reduce the probability of a bad reuse by taking the first 64 bits of data into the hash.
                        var id = (ulong)mesh.Triangles.Memory ^ (ulong)mesh.Triangles.Length ^ (*(ulong*)mesh.Triangles.Memory);
                        instance.VertexCount = mesh.Triangles.Length * 3;
                        bool instanceValid;
                        Buffer<Vector4> vertices;
                        //Allocate AND (below) upload inside the same MeshCache lock so the write into the
                        //shared ring buffer is serialized with the GL thread's FlushPendingUploads
                        //mutation (see the ConvexHull case above).
                        lock (MeshCache)
                        {
                            bool needsUpload;
                            instanceValid = MeshCache.Allocate(id, instance.VertexCount, out instance.VertexStart, out vertices, out needsUpload);
                            //Only a fresh allocation (needsUpload) has vertex data that must be written.
                            //A mesh already uploaded earlier in this pass needs no write, and a failed
                            //allocation (cache full) must be skipped so the instance never points at an
                            //invalid region.
                            if (instanceValid && needsUpload)
                            {
                                for (int i = 0; i < mesh.Triangles.Length; ++i)
                                {
                                    ref var triangle = ref mesh.Triangles[i];
                                    var baseVertexIndex = i * 3;
                                    //Note winding flip for rendering.
                                    vertices[baseVertexIndex] = new Vector4(triangle.A, 1.0f);
                                    vertices[baseVertexIndex + 1] = new Vector4(triangle.C, 1.0f);
                                    vertices[baseVertexIndex + 2] = new Vector4(triangle.B, 1.0f);
                                }
                            }
                        }
                        //Local bounding radius (distance from the local origin to the farthest vertex),
                        //used on the CPU for frustum culling.
                        float maxRadiusSq = 0f;
                        for (int i = 0; i < mesh.Triangles.Length; ++i)
                        {
                            ref var triangle = ref mesh.Triangles[i];
                            maxRadiusSq = MathF.Max(maxRadiusSq, triangle.A.LengthSquared());
                            maxRadiusSq = MathF.Max(maxRadiusSq, triangle.B.LengthSquared());
                            maxRadiusSq = MathF.Max(maxRadiusSq, triangle.C.LengthSquared());
                        }
                        instance.BoundingRadius = MathF.Sqrt(maxRadiusSq);
                        if (instanceValid)
                        {
                            shapeCache.Meshes.Add(instance, pool);
                        }
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void AddShape(Shapes shapes, TypedIndex shapeIndex, RigidPose pose, Vector3 color, ref ShapeCache shapeCache, BufferPool pool)
        {
            if (shapeIndex.Exists)
            {
                shapes[shapeIndex.Type].GetShapeData(shapeIndex.Index, out var shapeData, out _);
                AddShape(shapeData, shapeIndex.Type, shapes, pose, color, ref shapeCache, pool);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddBodyShape(Shapes shapes, Bodies bodies, int setIndex, int indexInSet, ref ShapeCache shapeCache, BufferPool pool)
        {
            ref var set = ref bodies.Sets[setIndex];
            var handle = set.IndexToHandle[indexInSet];
            //Body color is based on three factors:
            //1) Handle as a hash seed that is unpacked into a color
            //2) Dynamics vs kinematic state
            //3) Activity state
            //The handle is hashed to get variation.
            ref var activity = ref set.Activity[indexInSet];
            Vector3 color;
            Helpers.UnpackColor((uint)HashHelper.Rehash(handle.Value), out Vector3 colorVariation);
            ref var state = ref set.DynamicsState[indexInSet];
            if (Bodies.IsKinematic(state.Inertia.Local))
            {
                var kinematicBase = new Vector3(0, 0.609f, 0.37f);
                var kinematicVariationSpan = new Vector3(0.1f, 0.1f, 0.1f);
                color = kinematicBase + kinematicVariationSpan * colorVariation;
            }
            else
            {
                var dynamicBase = new Vector3(0.8f, 0.1f, 0.566f);
                var dynamicVariationSpan = new Vector3(0.2f, 0.2f, 0.2f);
                color = dynamicBase + dynamicVariationSpan * colorVariation;
            }

            if (setIndex == 0)
            {
                if (activity.SleepCandidate)
                {
                    var sleepCandidateTint = new Vector3(0.35f, 0.35f, 0.7f);
                    color *= sleepCandidateTint;
                }
            }
            else
            {
                var sleepTint = new Vector3(0.2f, 0.2f, 0.4f);
                color *= sleepTint;
            }

            AddShape(shapes, set.Collidables[indexInSet].Shape, state.Motion.Pose, color, ref shapeCache, pool);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddStaticShape(Shapes shapes, Statics statics, int index, ref ShapeCache shapeCache, BufferPool pool)
        {
            var handle = statics.IndexToHandle[index];
            //Statics don't have any activity states. Just some simple variation on a central static color.
            Helpers.UnpackColor((uint)HashHelper.Rehash(handle.Value), out Vector3 colorVariation);
            var staticBase = new Vector3(0.1f, 0.057f, 0.014f);
            var staticVariationSpan = new Vector3(0.07f, 0.07f, 0.03f);
            var color = staticBase + staticVariationSpan * colorVariation;
            ref var collidable = ref statics[index];
            AddShape(shapes, collidable.Shape, collidable.Pose, color, ref shapeCache, pool);
        }

        /// <summary>
        ///     Extracts a single static (given its shape reference and pose) into the target cache,
        ///     without needing a <see cref="Statics" /> collection. Used by the camera-relative static
        ///     streaming path, which iterates a lightweight spatial index of position + shape + radius
        ///     rather than the full statics collection. Reads the simulation's shapes - must run on the
        ///     thread that owns the simulation.
        /// </summary>
        public void AddStaticByShape(Shapes shapes, TypedIndex shapeIndex, RigidPose pose, ref ShapeCache shapeCache, BufferPool pool)
        {
            //Statics have no activity states; use a stable per-shape color variation so repeated
            //extraction of the same static produces a consistent color.
            Helpers.UnpackColor((uint)HashHelper.Rehash(shapeIndex.Type * 397 + shapeIndex.Index), out Vector3 colorVariation);
            var staticBase = new Vector3(0.1f, 0.057f, 0.014f);
            var staticVariationSpan = new Vector3(0.07f, 0.07f, 0.03f);
            var color = staticBase + staticVariationSpan * colorVariation;
            AddShape(shapes, shapeIndex, pose, color, ref shapeCache, pool);
        }

        struct Job
        {
            public int SetIndex;
            public int StartIndex;
            public int Count;
            //If the job is about statics, the set index will be -1.
        }
        QuickList<Job> jobs;

        //The target cache is passed in by reference for the duration of an extraction. A by-value
        //copy is held here so the multithreaded worker-done callbacks can merge into it; the copy is
        //written back to the caller's target at the end of the extraction (resized QuickLists carry
        //their new buffers back to the frame).
        ShapeCache target;
        Simulation simulation;
        Buffer<ShapeCache> workerCaches;

        void PrepareForMultithreadedExecution(IThreadDispatcher threadDispatcher)
        {
            jobs = new QuickList<Job>(128, pool);
            looper.Dispatcher = threadDispatcher;
            pool.Take(threadDispatcher.ThreadCount, out workerCaches);
            for (int i = 0; i < workerCaches.Length; ++i)
            {
                workerCaches[i] = new ShapeCache(128, threadDispatcher.WorkerPools[i]);
            }
        }

        void EndMultithreadedExecution()
        {
            jobs.Dispose(pool);
            for (int i = 0; i < workerCaches.Length; ++i)
            {
                workerCaches[i].Dispose(looper.Dispatcher.WorkerPools[i]);
            }
            looper.Dispatcher = null;
            pool.Return(ref workerCaches);
        }

        static void CreateBodyJobs(Simulation simulation, ref QuickList<Job> jobs, BufferPool pool)
        {
            const int targetBodiesPerJob = 1024;
            for (int setIndex = 0; setIndex < simulation.Bodies.Sets.Length; ++setIndex)
            {
                ref var set = ref simulation.Bodies.Sets[setIndex];
                if (set.Allocated && set.Count > 0) //active set can be allocated and have no bodies in it.
                {
                    var jobCount = (set.Count + targetBodiesPerJob - 1) / targetBodiesPerJob;
                    var bodiesPerJob = set.Count / jobCount;
                    var remainder = set.Count - bodiesPerJob * jobCount;
                    var previousEnd = 0;
                    for (int j = 0; j < jobCount; ++j)
                    {
                        var count = j < remainder ? bodiesPerJob + 1 : bodiesPerJob;
                        jobs.Allocate(pool) = new Job { SetIndex = setIndex, Count = count, StartIndex = previousEnd };
                        previousEnd += count;
                    }
                }
            }
        }

        static void CreateStaticJobs(Simulation simulation, ref QuickList<Job> jobs, BufferPool pool)
        {
            const int targetBodiesPerJob = 1024;
            if (simulation.Statics.Count > 0)
            {
                var jobCount = (simulation.Statics.Count + targetBodiesPerJob - 1) / targetBodiesPerJob;
                var bodiesPerJob = simulation.Statics.Count / jobCount;
                var remainder = simulation.Statics.Count - bodiesPerJob * jobCount;
                var previousEnd = 0;
                for (int j = 0; j < jobCount; ++j)
                {
                    var count = j < remainder ? bodiesPerJob + 1 : bodiesPerJob;
                    jobs.Allocate(pool) = new Job { SetIndex = -1, Count = count, StartIndex = previousEnd };
                    previousEnd += count;
                }
            }
        }

        void AddBodyShapesForJob(int jobIndex, int workerIndex)
        {
            var job = jobs[jobIndex];
            var pool = looper.Dispatcher.WorkerPools[workerIndex];
            ref var set = ref simulation.Bodies.Sets[job.SetIndex];
            var endIndex = job.StartIndex + job.Count;
            Debug.Assert(endIndex <= set.Count);
            for (int bodyIndex = job.StartIndex; bodyIndex < endIndex; ++bodyIndex)
            {
                AddBodyShape(simulation.Shapes, simulation.Bodies, job.SetIndex, bodyIndex, ref workerCaches[workerIndex], pool);
            }
        }

        void AddStaticShapesForJob(int jobIndex, int workerIndex)
        {
            var job = jobs[jobIndex];
            var pool = looper.Dispatcher.WorkerPools[workerIndex];
            var endIndex = job.StartIndex + job.Count;
            Debug.Assert(endIndex <= simulation.Statics.Count);
            for (int staticIndex = job.StartIndex; staticIndex < endIndex; ++staticIndex)
            {
                AddStaticShape(simulation.Shapes, simulation.Statics, staticIndex, ref workerCaches[workerIndex], pool);
            }
        }

        object workerShapeMergeLocker = new object();

        void CopyWorkerCacheToMainCache<TShape>(ref QuickList<TShape> workerCache, ref QuickList<TShape> mainCache) where TShape : unmanaged
        {
            if (workerCache.Count > 0)
            {
                int copyStartLocation;
                lock (workerShapeMergeLocker)
                {
                    var newCount = mainCache.Count + workerCache.Count;
                    mainCache.EnsureCapacity(newCount, pool);
                    copyStartLocation = mainCache.Count;
                    mainCache.Count = newCount;
                }
                workerCache.Span.CopyTo(0, mainCache.Span, copyStartLocation, workerCache.Count);
            }
        }

        void WorkerDone(int workerIndex)
        {
            //This fires when a worker finishes its work. We should copy the results into the main buffer.
            ref var workerCache = ref workerCaches[workerIndex];
            CopyWorkerCacheToMainCache(ref workerCache.Spheres, ref target.Spheres);
            CopyWorkerCacheToMainCache(ref workerCache.Capsules, ref target.Capsules);
            CopyWorkerCacheToMainCache(ref workerCache.Boxes, ref target.Boxes);
            CopyWorkerCacheToMainCache(ref workerCache.Cylinders, ref target.Cylinders);
            CopyWorkerCacheToMainCache(ref workerCache.Triangles, ref target.Triangles);
            CopyWorkerCacheToMainCache(ref workerCache.Meshes, ref target.Meshes);
        }

        void AddBodiesSequentially(Simulation simulation, ref ShapeCache shapeCache, BufferPool pool)
        {
            for (int i = 0; i < simulation.Bodies.Sets.Length; ++i)
            {
                ref var set = ref simulation.Bodies.Sets[i];
                if (set.Allocated) //Islands are stored noncontiguously; skip those which have been deallocated.
                {
                    for (int bodyIndex = 0; bodyIndex < set.Count; ++bodyIndex)
                    {
                        AddBodyShape(simulation.Shapes, simulation.Bodies, i, bodyIndex, ref shapeCache, pool);
                    }
                }
            }
        }

        void AddStaticsSequentially(Simulation simulation, ref ShapeCache shapeCache, BufferPool pool)
        {
            for (int i = 0; i < simulation.Statics.Count; ++i)
            {
                AddStaticShape(simulation.Shapes, simulation.Statics, i, ref shapeCache, pool);
            }
        }

        /// <summary>
        /// Extracts the dynamic/kinematic body shapes into the given target cache.
        /// Reads the simulation - must be called on the thread that owns it.
        /// </summary>
        public void AddInstances(Simulation simulation, ref ShapeCache target, IThreadDispatcher threadDispatcher = null)
        {
            this.target = target;
            if (threadDispatcher != null && threadDispatcher.ThreadCount > 1)
            {
                this.simulation = simulation;
                PrepareForMultithreadedExecution(threadDispatcher);
                CreateBodyJobs(simulation, ref jobs, pool);
                looper.For(0, jobs.Count, AddBodyShapesForJob, WorkerDone);
                EndMultithreadedExecution();
                this.simulation = null;
            }
            else
            {
                AddBodiesSequentially(simulation, ref this.target, pool);
            }
            target = this.target;
        }

        /// <summary>
        /// Extracts the static/terrain shapes into the given target cache.
        /// Reads the simulation - must be called on the thread that owns it.
        /// </summary>
        public void AddStaticInstances(Simulation simulation, ref ShapeCache target, IThreadDispatcher threadDispatcher = null)
        {
            this.target = target;
            if (threadDispatcher != null && threadDispatcher.ThreadCount > 1)
            {
                this.simulation = simulation;
                PrepareForMultithreadedExecution(threadDispatcher);
                CreateStaticJobs(simulation, ref jobs, pool);
                looper.For(0, jobs.Count, AddStaticShapesForJob, WorkerDone);
                EndMultithreadedExecution();
                this.simulation = null;
            }
            else
            {
                AddStaticsSequentially(simulation, ref this.target, pool);
            }
            target = this.target;
        }

        public void Dispose()
        {
            MeshCache.Dispose();
        }
    }
}
