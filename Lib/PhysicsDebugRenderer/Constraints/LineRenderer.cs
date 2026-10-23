// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BepuUtilities;
using DemoContentLoader;
using DemoRenderer.ShapeDrawing;
using OpenTK.Graphics.OpenGL4;

namespace DemoRenderer.Constraints
{
    /// <summary>
    /// GPU-relevant information for the rendering of a single line instance.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct LineInstance
    {
        [FieldOffset(0)]
        public Vector3 Start;
        [FieldOffset(12)]
        public uint PackedBackgroundColor;
        [FieldOffset(16)]
        public Vector3 End;
        [FieldOffset(28)]
        public uint PackedColor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LineInstance(in Vector3 start, in Vector3 end, in Vector3 color, in Vector3 backgroundColor)
        {
            Start = start;
            PackedBackgroundColor = Helpers.PackColor(backgroundColor);
            End = end;
            PackedColor = Helpers.PackColor(color);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LineInstance(in Vector3 start, in Vector3 end, uint packedColor, uint packedBackgroundColor)
        {
            Start = start;
            PackedBackgroundColor = packedBackgroundColor;
            End = end;
            PackedColor = packedColor;
        }
    }

    public class LineRenderer : Shader
    {
        [StructLayout(LayoutKind.Explicit)]
        struct VertexConstants
        {
            [FieldOffset(0)]
            public Matrix Projection;
            [FieldOffset(64)]
            public Vector2 NDCToScreenScale;
            [FieldOffset(80)]
            public Vector3 CameraForward;
            [FieldOffset(92)]
            public float TanAnglePerPixel;
            [FieldOffset(96)]
            public Vector3 CameraRight;
            [FieldOffset(112)]
            public Vector3 CameraPosition;
            [FieldOffset(128)]
            public Vector3 CameraUp;
            [FieldOffset(144)]
            public Vector3 CameraBackward;
        }

        private readonly ConstantsBuffer<VertexConstants> vertexConstants;
        private readonly StructuredBuffer<LineInstance> instances;
        private readonly IndexBuffer indices;
        private LineInstance[] cullScratch;

        public LineRenderer(ContentArchive content, int maximumInstancesPerDraw = 16384) : base(
            content.Load<GLSLContent>(@"Constraints\RenderLines.glvs").Source,
            content.Load<GLSLContent>(@"Constraints\RenderLines.glfs").Source
        )
        {
            instances = new StructuredBuffer<LineInstance>(BufferTarget.ShaderStorageBuffer, maximumInstancesPerDraw, "Line Instances");
            indices = new IndexBuffer(Helpers.GetBoxIndices(maximumInstancesPerDraw), "Line Quad Indices");

            vertexConstants = new ConstantsBuffer<VertexConstants>(BufferTarget.UniformBuffer, debugName: "Line Renderer Vertex Constants");
        }

        //Grows the culling compaction buffer to hold at least <paramref name="needed"/> instances.
        private void EnsureCullScratchCapacity(int needed)
        {
            if (cullScratch == null || cullScratch.Length < needed)
            {
                Array.Resize(ref cullScratch, needed);
            }
        }

        public void Render(Camera camera, Int2 resolution, Span<LineInstance> instances, int start, int count)
        {
            Use();
            indices.Bind();
            vertexConstants.Bind(0);
            this.instances.Bind(0);

            var tanPerPixel = (float)Math.Tan(camera.FieldOfView / resolution.Y);
            var vertexConstantsData = new VertexConstants
            {
                Projection = camera.Projection,
                NDCToScreenScale = new Vector2(resolution.X / 2f, resolution.Y / 2f),
                CameraForward = camera.Forward,
                TanAnglePerPixel = tanPerPixel,
                CameraRight = camera.Right,
                CameraPosition = camera.Position,
                CameraUp = camera.Up,
                CameraBackward = camera.Backward,
            };
            vertexConstants.Update(ref vertexConstantsData);

            //Frustum-cull lines whose world bounding sphere lies entirely outside the view, then
            //upload only the survivors (compacted). The bound inflates the segment by the maximum
            //perspective thickness (distance to camera * per-pixel angle) so a visible line is never
            //culled.
            var frustum = new CullingFrustum(camera);
            var camPosition = camera.Position;
            EnsureCullScratchCapacity(count);
            var end = start + count;
            var kept = 0;
            for (int i = start; i < end; ++i)
            {
                ref var line = ref instances[i];
                var center = (line.Start + line.End) * 0.5f;
                var halfLength = Vector3.Distance(line.Start, line.End) * 0.5f;
                var maxDist = MathF.Max(
                    Vector3.Distance(line.Start, camPosition),
                    Vector3.Distance(line.End, camPosition));
                var radius = halfLength + maxDist * tanPerPixel;
                if (frustum.IsSphereOutside(center, radius))
                {
                    continue;
                }
                cullScratch[kept++] = line;
            }

            var offset = 0;
            while (kept > 0)
            {
                var batchCount = Math.Min(this.instances.Capacity, kept);
                this.instances.Update(cullScratch.AsSpan(offset, batchCount));
                GL.DrawElements(PrimitiveType.Triangles, batchCount * 36, indices.Type, 0);
                kept -= batchCount;
                offset += batchCount;
            }
        }

        protected override void DoDispose()
        {
            instances.Dispose();
            indices.Dispose();
            vertexConstants.Dispose();
            base.DoDispose();
        }
    }
}
