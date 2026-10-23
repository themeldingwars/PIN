// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using DemoContentLoader;
using OpenTK.Graphics.OpenGL4;

namespace DemoRenderer.ShapeDrawing
{
    //This isn't exactly an efficient representation, but isolated triangle rendering is mainly just for small scale testing anyway.
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct TriangleInstance
    {
        [FieldOffset(0)]
        public Vector3 A;
        [FieldOffset(12)]
        public uint PackedColor;
        [FieldOffset(16)]
        public Vector3 B;
        [FieldOffset(28)]
        public float X;
        [FieldOffset(32)]
        public Vector3 C;
        [FieldOffset(44)]
        public float Y;
        [FieldOffset(48)]
        public ulong PackedOrientation;
        [FieldOffset(56)]
        public float Z;
    }
    public class TriangleRenderer : RasterizedRenderer<TriangleInstance>
    {
        public TriangleRenderer(ContentArchive content, int maximumInstancesPerDraw = 10000) : base(content, @"ShapeDrawing\RenderTriangles", maximumInstancesPerDraw)
        { }
        protected override void GetBoundingSphere(in TriangleInstance instance, out Vector3 center, out float radius)
        {
            //The three local vertices (A, B, C) are rotated about and translated by (X, Y, Z). Rotation
            //preserves length, so the world bounding sphere is centered at (X, Y, Z) with radius equal
            //to the farthest local vertex from the local origin.
            center = new Vector3(instance.X, instance.Y, instance.Z);
            var da = Vector3.Dot(instance.A, instance.A);
            var db = Vector3.Dot(instance.B, instance.B);
            var dc = Vector3.Dot(instance.C, instance.C);
            radius = MathF.Sqrt(MathF.Max(da, MathF.Max(db, dc)));
        }
        protected override void OnBatchDraw(int batchCount) =>
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3 * batchCount);
    }
}
