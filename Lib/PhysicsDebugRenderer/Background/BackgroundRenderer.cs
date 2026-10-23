// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using BepuUtilities;
using DemoContentLoader;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace DemoRenderer.Background
{
    public class BackgroundRenderer : Shader
    {
        private readonly ConstantsBuffer<Matrix> constants;

        public BackgroundRenderer(ContentArchive content)
            : base(
            content.Load<GLSLContent>(@"Background\RenderBackground.glvs").Source,
            AdaptBackgroundFragmentForZUp(content.Load<GLSLContent>(@"Background\RenderBackground.glfs").Source)) =>
            constants = new ConstantsBuffer<Matrix>(BufferTarget.UniformBuffer, debugName: "BackgroundRenderer Constants");

        // The vendored sky shader was authored for a Y-up world: it grades the sky by the view ray's
        // Y component and places the sun at a fixed Y-up direction. The PIN world is Z-up, so remap the
        // "up" axis to Z and re-orient the sun (swap the old Y and Z components, since the old up was Y
        // and is now Z). This keeps the horizon level under yaw rotation - the blue sky stays up and the
        // gray ground stays down no matter which way the camera faces. Done on the runtime GLSL source
        // (the contentarchive stores it as text, compiled by the driver on load) so the prebuilt
        // contentarchive binary does not need to be regenerated.
        private static string AdaptBackgroundFragmentForZUp(string source)
        {
            // Sky gradient "up" component of the view direction: Y-up -> Z-up.
            var adapted = source.Replace("_33.y", "_33.z");
            // Sun direction: (x, y, z) in Y-up becomes (x, z, y) in Z-up.
            adapted = adapted
                .Replace("vec3(0.37000000476837158203125, 0.930000007152557373046875, 0.300000011920928955078125)",
                         "vec3(0.37000000476837158203125, 0.300000011920928955078125, 0.930000007152557373046875)");
            return adapted;
        }

        public void Render(Camera camera)
        {
            Use();
            constants.Bind(0);

            // The un-projected ray must track the actual scene camera so the horizon stays level under
            // yaw/pitch. The geometry rasterizes in a camera-basis view whose COLUMNS are (Right, Up,
            // Backward) - see RenderMeshes.glvs: vec4(_156 * mat3(CameraRight, CameraUp, CameraBackward), 1)
            // - NOT the quaternion-inverted camera.View (which puts Forward in the view Y slot and Up in the
            // Z slot, so its un-projected center ray is not camera.Forward). Compositing that exact basis
            // view with the same camera.Projection the geometry uses, then inverting, gives the NDC->world
            // offset the sky shader needs; its center ray is camera.Forward at every yaw/pitch/position.
            // The Z-up sky gradient/sun orientation comes from AdaptBackgroundFragmentForZUp on the source.
            var right = camera.Right;
            var up = camera.Up;
            var backward = camera.Backward;
            var position = camera.Position;
            var basisView = new Matrix
            {
                X = new Vector4(right.X, up.X, backward.X, 0f),
                Y = new Vector4(right.Y, up.Y, backward.Y, 0f),
                Z = new Vector4(right.Z, up.Z, backward.Z, 0f),
                W = new Vector4(-Vector3.Dot(position, right), -Vector3.Dot(position, up), -Vector3.Dot(position, backward), 1f)
            };
            var constantsData = Matrix.Invert(basisView * camera.Projection);
            constants.Update(ref constantsData);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        protected override void DoDispose()
        {
            constants.Dispose();
            base.DoDispose();
        }
    }
}
