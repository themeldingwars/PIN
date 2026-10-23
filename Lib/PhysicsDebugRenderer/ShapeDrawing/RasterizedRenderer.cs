// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using BepuUtilities;
using DemoContentLoader;
using OpenTK.Graphics.OpenGL4;

namespace DemoRenderer.ShapeDrawing
{
    [StructLayout(LayoutKind.Explicit)]
    struct RasterizedVertexConstants
    {
        [FieldOffset(0)]
        public Matrix Projection;
        [FieldOffset(64)]
        public Vector3 CameraPosition;
        [FieldOffset(80)]
        public Vector3 CameraRight;
        [FieldOffset(96)]
        public Vector3 CameraUp;
        [FieldOffset(112)]
        public Vector3 CameraBackward;
    }
    public class RasterizedRenderer<TInstance> : Shader where TInstance : struct
    {
        private readonly StructuredBuffer<TInstance> instances;
        private readonly ConstantsBuffer<RasterizedVertexConstants> vertexConstants;
        private TInstance[] cullScratch;

        public RasterizedRenderer(ContentArchive content, string shaderPath, int maximumInstancesPerDraw = 10000) : base(
            content.Load<GLSLContent>($"{shaderPath}.glvs").Source,
            content.Load<GLSLContent>($"{shaderPath}.glfs").Source
        )
        {
            string instanceTypeName = typeof(TInstance).Name;
            instances = new StructuredBuffer<TInstance>(BufferTarget.ShaderStorageBuffer, maximumInstancesPerDraw, $"{instanceTypeName} Instances");
            vertexConstants = new ConstantsBuffer<RasterizedVertexConstants>(BufferTarget.UniformBuffer, debugName: $"{instanceTypeName} Renderer Vertex Constants");
        }

        protected virtual void OnDrawSetup() { }
        protected virtual void OnBatchDraw(int batchCount) { }

        /// <summary>
        /// Computes the world-space bounding sphere of a single instance for CPU frustum culling. The
        /// base implementation reports a degenerate (never-culled) bound; concrete shape renderers
        /// override this to supply a tight bound.
        /// </summary>
        protected virtual void GetBoundingSphere(in TInstance instance, out Vector3 center, out float radius)
        {
            center = Vector3.Zero;
            radius = float.MaxValue;
        }

        //Grows the culling compaction buffer to hold at least <paramref name="needed"/> instances.
        private void EnsureCullScratchCapacity(int needed)
        {
            if (cullScratch == null || cullScratch.Length < needed)
            {
                Array.Resize(ref cullScratch, needed);
            }
        }

        public void Render(Camera camera, Int2 screenResolution, Span<TInstance> instances, int start, int count)
        {
            Use();
            vertexConstants.Bind(0);
            this.instances.Bind(0);
            OnDrawSetup();

            var vertexConstantsData = new RasterizedVertexConstants
            {
                Projection = camera.Projection,
                CameraPosition = camera.Position,
                CameraRight = camera.Right,
                CameraUp = camera.Up,
                CameraBackward = camera.Backward,
            };
            vertexConstants.Update(ref vertexConstantsData);

            //Frustum-cull instances whose world bounding sphere lies entirely outside the view, then
            //upload only the survivors (compacted) to the GPU buffer.
            var frustum = new CullingFrustum(camera);
            EnsureCullScratchCapacity(count);
            var end = start + count;
            var kept = 0;
            for (int i = start; i < end; ++i)
            {
                ref var instance = ref instances[i];
                GetBoundingSphere(instance, out var center, out var radius);
                if (frustum.IsSphereOutside(center, radius))
                {
                    continue;
                }
                cullScratch[kept++] = instance;
            }

            var offset = 0;
            while (kept > 0)
            {
                var batchCount = Math.Min(this.instances.Capacity, kept);
                this.instances.Update(cullScratch.AsSpan(offset, batchCount));
                OnBatchDraw(batchCount);
                kept -= batchCount;
                offset += batchCount;
            }
        }

        protected override void DoDispose()
        {
            instances.Dispose();
            vertexConstants.Dispose();
            base.DoDispose();
        }
    }
}
