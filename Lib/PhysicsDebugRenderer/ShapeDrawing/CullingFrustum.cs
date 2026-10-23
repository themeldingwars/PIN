// Originally adapted for the PIN physics debug renderer.
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuUtilities;

namespace DemoRenderer.ShapeDrawing
{
    /// <summary>
    /// CPU-side view frustum for culling. The six planes are derived from the SAME
    /// camera-relative view the GPU rasterizes (a camera-relative point p is mapped to view
    /// space as (dot(p, Right), dot(p, Up), dot(p, Backward)) and then multiplied by the
    /// Projection). Because the Bepu Projection (rows X,Y,Z,W) is uploaded as a column-major
    /// GLSL mat4 (its transpose), the combined view-projection in camera-relative space is
    /// C = Projection^T * [Right|Up|Backward|1], and row i of C is
    /// (P.X[i]*Right + P.Y[i]*Up + P.Z[i]*Backward, P.W[i]). Using camera.ViewProjection here
    /// instead would describe a frustum rotated relative to what is drawn (the "culling wedge"
    /// bug). Each plane (nx,ny,nz,nw) keeps the visible half-space
    /// dot(worldPos, n) + nw &gt;= 0, so camera-relative offsets are shifted by -dot(n, camera.Position).
    /// </summary>
    public readonly struct CullingFrustum
    {
        private readonly Vector4 left;
        private readonly Vector4 right;
        private readonly Vector4 bottom;
        private readonly Vector4 top;
        private readonly Vector4 near;
        private readonly Vector4 far;

        public CullingFrustum(Camera camera)
        {
            var projection = camera.Projection;
            var camRight = camera.Right;
            var camUp = camera.Up;
            var camBackward = camera.Backward;
            var camPosition = camera.Position;
            var px = projection.X;
            var py = projection.Y;
            var pz = projection.Z;
            var pw = projection.W;
            var r0 = CombineProjectionRow(px.X, py.X, pz.X, pw.X, camRight, camUp, camBackward, camPosition);
            var r1 = CombineProjectionRow(px.Y, py.Y, pz.Y, pw.Y, camRight, camUp, camBackward, camPosition);
            var r2 = CombineProjectionRow(px.Z, py.Z, pz.Z, pw.Z, camRight, camUp, camBackward, camPosition);
            var r3 = CombineProjectionRow(px.W, py.W, pz.W, pw.W, camRight, camUp, camBackward, camPosition);
            //The projection is reversed-Z (near -> z/w = +1, far -> z/w = -1), so the depth planes
            //are the mirror of the standard Gribb-Hartmann: near = r3 - r2, far = r3 + r2.
            left = NormalizePlane(r3 + r0);
            right = NormalizePlane(r3 - r0);
            bottom = NormalizePlane(r3 + r1);
            top = NormalizePlane(r3 - r1);
            near = NormalizePlane(r3 - r2);
            far = NormalizePlane(r3 + r2);
        }

        //Builds one row of the combined view-projection C = Projection^T * [Right|Up|Backward|1]
        //(camera-relative space), shifted into world space. The visible half-space is
        //dot(p - Position, n) + w >= 0 for a WORLD point p, i.e. dot(p, n) + (w - dot(Position, n))
        //>= 0, hence the offset shift.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 CombineProjectionRow(float cX, float cY, float cZ, float cW, Vector3 right, Vector3 up, Vector3 backward, Vector3 position)
        {
            var n = cX * right + cY * up + cZ * backward;
            var offset = cW - Vector3.Dot(n, position);
            return new Vector4(n, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 NormalizePlane(Vector4 plane)
        {
            var normalLength = MathF.Sqrt(plane.X * plane.X + plane.Y * plane.Y + plane.Z * plane.Z);
            return new Vector4(plane.X / normalLength, plane.Y / normalLength, plane.Z / normalLength, plane.W / normalLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SignedDistance(in Vector4 plane, in Vector3 point) =>
            plane.X * point.X + plane.Y * point.Y + plane.Z * point.Z + plane.W;

        /// <summary>
        /// True if the sphere (centered at <paramref name="center"/>, radius <paramref name="radius"/>)
        /// lies entirely outside the frustum. The center is more than a radius beyond a plane iff the
        /// sphere is fully outside.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSphereOutside(in Vector3 center, float radius)
        {
            return SignedDistance(left, center) < -radius ||
                   SignedDistance(right, center) < -radius ||
                   SignedDistance(bottom, center) < -radius ||
                   SignedDistance(top, center) < -radius ||
                   SignedDistance(near, center) < -radius ||
                   SignedDistance(far, center) < -radius;
        }

        /// <summary>
        /// True if the line segment [a, b] (in world space) lies entirely outside the frustum. A
        /// segment is fully outside when BOTH endpoints are beyond the same plane (conservative - a
        /// segment crossing the frustum is kept). The caller is responsible for any thickness
        /// inflation before calling.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSegmentOutside(in Vector3 a, in Vector3 b)
        {
            return (SignedDistance(left, a) < 0 && SignedDistance(left, b) < 0) ||
                   (SignedDistance(right, a) < 0 && SignedDistance(right, b) < 0) ||
                   (SignedDistance(bottom, a) < 0 && SignedDistance(bottom, b) < 0) ||
                   (SignedDistance(top, a) < 0 && SignedDistance(top, b) < 0) ||
                   (SignedDistance(near, a) < 0 && SignedDistance(near, b) < 0) ||
                   (SignedDistance(far, a) < 0 && SignedDistance(far, b) < 0);
        }
    }
}
