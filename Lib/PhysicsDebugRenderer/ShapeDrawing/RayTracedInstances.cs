// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using System.Numerics;
using DemoContentLoader;

namespace DemoRenderer.ShapeDrawing
{
    /// <summary>
    /// Concrete sphere renderer that supplies a tight world bounding sphere (its own center/radius)
    /// for CPU frustum culling.
    /// </summary>
    public class SphereRenderer : RayTracedRenderer<SphereInstance>
    {
        public SphereRenderer(ContentArchive content, int maximumInstancesPerDraw = 10000) : base(content, @"ShapeDrawing\RenderSpheres", maximumInstancesPerDraw) { }
        protected override void GetBoundingSphere(in SphereInstance instance, out Vector3 center, out float radius)
        {
            center = instance.Position;
            radius = instance.Radius;
        }
    }

    /// <summary>
    /// Concrete capsule renderer. The capsule's world bounding sphere is centered at its position with
    /// radius = Radius + HalfLength (the capsule's two end-cap spheres plus the connecting cylinder all
    /// fit inside that sphere).
    /// </summary>
    public class CapsuleRenderer : RayTracedRenderer<CapsuleInstance>
    {
        public CapsuleRenderer(ContentArchive content, int maximumInstancesPerDraw = 10000) : base(content, @"ShapeDrawing\RenderCapsules", maximumInstancesPerDraw) { }
        protected override void GetBoundingSphere(in CapsuleInstance instance, out Vector3 center, out float radius)
        {
            center = instance.Position;
            radius = instance.Radius + instance.HalfLength;
        }
    }

    /// <summary>
    /// Concrete cylinder renderer. Same world bounding sphere as a capsule (Radius + HalfLength about
    /// the center).
    /// </summary>
    public class CylinderRenderer : RayTracedRenderer<CylinderInstance>
    {
        public CylinderRenderer(ContentArchive content, int maximumInstancesPerDraw = 10000) : base(content, @"ShapeDrawing\RenderCylinders", maximumInstancesPerDraw) { }
        protected override void GetBoundingSphere(in CylinderInstance instance, out Vector3 center, out float radius)
        {
            center = instance.Position;
            radius = instance.Radius + instance.HalfLength;
        }
    }
    /// <summary>
    /// GPU-relevant information for the rendering of a single sphere instance.
    /// </summary>
    public struct SphereInstance
    {
        public Vector3 Position;
        public float Radius;
        public Vector3 PackedOrientation;
        public uint PackedColor;
    }

    /// <summary>
    /// GPU-relevant information for the rendering of a single capsule instance.
    /// </summary>
    public struct CapsuleInstance
    {
        public Vector3 Position;
        public float Radius;
        public ulong PackedOrientation;
        public float HalfLength;
        public uint PackedColor;
    }

    /// <summary>
    /// GPU-relevant information for the rendering of a single cylinder instance.
    /// </summary>
    public struct CylinderInstance
    {
        public Vector3 Position;
        public float Radius;
        public ulong PackedOrientation;
        public float HalfLength;
        public uint PackedColor;
    }

}
