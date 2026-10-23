// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using BepuUtilities.Memory;
using DemoContentLoader;
using DemoRenderer.Background;
using DemoRenderer.Constraints;
using DemoRenderer.PostProcessing;
using DemoRenderer.ShapeDrawing;
using DemoRenderer.UI;
using DemoUtilities;
using OpenTK.Graphics.OpenGL4;

namespace DemoRenderer
{
    public class Renderer : Disposable
    {
        public readonly RenderSurface Surface;
        private readonly ParallelLooper _looper = new ParallelLooper();
        private readonly BufferPool _pool = new BufferPool();

        private readonly int depthBuffer = GL.GenTexture();
        private readonly int colorBuffer = GL.GenTexture();
        private readonly int framebuffer = GL.GenFramebuffer();
        private readonly int resolvedColorBuffer = GL.GenTexture();
        private readonly int resolvedFramebuffer = GL.GenFramebuffer();
        private int width = 0;
        private int height = 0;

        private readonly MeshCache meshCache;
        private readonly bool ownsMeshCache;

        // Optional GPU-accurate per-pass profiling. When enabled, each Render pass is followed by a
        // GL.Finish() so the GPU work issued by that pass is accounted against it (render is only CPU
        // issue time; the GPU execution is otherwise hidden inside the SwapBuffers sync point).
        public bool Profile { get; set; }
        private readonly Stopwatch _profSw = new Stopwatch();
        private readonly Dictionary<string, (double sum, int count)> _prof = new Dictionary<string, (double, int)>();
        private double _profLastLog;

        public ContentArchive Content { get; private set; }

        public Renderer(RenderSurface surface)
        {
            Surface = surface;
            ContentArchive content;

            using (var stream = GetType().Assembly.GetManifestResourceStream("PhysicsDebugRenderer.DemoRenderer.contentarchive"))
            {
                content = ContentArchive.Load(stream);
            }
            Content = content;

            Shapes = new ShapesExtractor(_looper, _pool);
            meshCache = Shapes.MeshCache;
            ownsMeshCache = true;
            SphereRenderer = new SphereRenderer(content);
            CapsuleRenderer = new CapsuleRenderer(content);
            CylinderRenderer = new CylinderRenderer(content);
            BoxRenderer = new BoxRenderer(content);
            TriangleRenderer = new TriangleRenderer(content);
            MeshRenderer = new MeshRenderer(Shapes.MeshCache, content);
            LineRenderer = new LineRenderer(content);
            Background = new BackgroundRenderer(content);
            CompressToSwap = new CompressToSwap(content);

            ImageRenderer = new ImageRenderer(content);
            ImageBatcher = new ImageBatcher(_pool);
            GlyphRenderer = new GlyphRenderer(content);
            TextBatcher = new TextBatcher();
            UILineRenderer = new UILineRenderer(content);
            UILineBatcher = new UILineBatcher();

            OnResize();
        }

        /// <summary>
        /// Creates a renderer that binds its <see cref="MeshRenderer"/> to an externally-owned
        /// <see cref="MeshCache"/>. Used by the PIN debug window so the mesh triangle data (and GPU
        /// buffer) that the physics (shard) thread fills during extraction is the very same one the
        /// renderer uploads and draws - a second, private cache would leave the vertex buffer empty
        /// and meshes/convex hulls would not render. The external cache is not disposed by the
        /// renderer (only its GPU buffer, which is created on this GL thread).
        /// </summary>
        public Renderer(RenderSurface surface, MeshCache sharedMeshCache)
        {
            Surface = surface;
            ContentArchive content;

            using (var stream = GetType().Assembly.GetManifestResourceStream("PhysicsDebugRenderer.DemoRenderer.contentarchive"))
            {
                content = ContentArchive.Load(stream);
            }
            Content = content;

            meshCache = sharedMeshCache;
            ownsMeshCache = false;
            SphereRenderer = new SphereRenderer(content);
            CapsuleRenderer = new CapsuleRenderer(content);
            CylinderRenderer = new CylinderRenderer(content);
            BoxRenderer = new BoxRenderer(content);
            TriangleRenderer = new TriangleRenderer(content);
            MeshRenderer = new MeshRenderer(meshCache, content);
            LineRenderer = new LineRenderer(content);
            Background = new BackgroundRenderer(content);
            CompressToSwap = new CompressToSwap(content);

            ImageRenderer = new ImageRenderer(content);
            ImageBatcher = new ImageBatcher(_pool);
            GlyphRenderer = new GlyphRenderer(content);
            TextBatcher = new TextBatcher();
            UILineRenderer = new UILineRenderer(content);
            UILineBatcher = new UILineBatcher();

            OnResize();
        }

        public BackgroundRenderer Background { get; private set; }
        public RayTracedRenderer<SphereInstance> SphereRenderer { get; private set; }
        public RayTracedRenderer<CapsuleInstance> CapsuleRenderer { get; private set; }
        public RayTracedRenderer<CylinderInstance> CylinderRenderer { get; private set; }
        public BoxRenderer BoxRenderer { get; private set; }
        public TriangleRenderer TriangleRenderer { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }
        public ShapesExtractor Shapes { get; private set; }
        public LineRenderer LineRenderer { get; private set; }
        public ImageRenderer ImageRenderer { get; private set; }
        public GlyphRenderer GlyphRenderer { get; private set; }
        public UILineRenderer UILineRenderer { get; private set; }
        public CompressToSwap CompressToSwap { get; private set; }
        public ImageBatcher ImageBatcher { get; private set; }
        public TextBatcher TextBatcher { get; private set; }
        public UILineBatcher UILineBatcher { get; private set; }

        public void OnResize()
        {
            var resolution = Surface.Resolution;
            width = resolution.X;
            height = resolution.Y;

            TextBatcher.Resolution = resolution;
            ImageBatcher.Resolution = resolution;
            UILineBatcher.Resolution = resolution;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.BindTexture(TextureTarget.Texture2DMultisample, depthBuffer);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, RenderSurface.SampleCount, PixelInternalFormat.DepthComponent, width, height, false);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2DMultisample, depthBuffer, 0);

            // Using a 64 bit texture in the demos for lighting is pretty silly. But we gon do it.
            GL.BindTexture(TextureTarget.Texture2DMultisample, colorBuffer);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, RenderSurface.SampleCount, PixelInternalFormat.Rgba16f, width, height, false);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, colorBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, resolvedFramebuffer);
            GL.BindTexture(TextureTarget.Texture2D, resolvedColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, resolvedColorBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Marks the end of a render pass for GPU-accurate profiling. Drains the GPU (GL.Finish) so
        /// the work issued since the previous mark is attributed to the named pass, and accumulates
        /// the elapsed time. No-op (and no GL.Finish) when Profile is disabled.
        /// </summary>
        private void ProfileSection(string name)
        {
            if (!Profile)
            {
                return;
            }
            GL.Finish();
            var elapsed = _profSw.Elapsed.TotalMilliseconds;
            if (_prof.TryGetValue(name, out var acc))
            {
                _prof[name] = (acc.sum + elapsed, acc.count + 1);
            }
            else
            {
                _prof[name] = (elapsed, 1);
            }
            _profSw.Restart();

            var now = _profSw.Elapsed.TotalSeconds + elapsed / 1000.0;
            if (now - _profLastLog >= 1.0)
            {
                _profLastLog = now;
                var parts = new List<string>();
                foreach (var kv in _prof)
                {
                    parts.Add($"{kv.Key} {kv.Value.sum / kv.Value.count:0.0}");
                }
                Console.WriteLine("[PhysicsDebugRenderer GPU] " + string.Join(" | ", parts) + " (ms, GPU-accurate)");
                foreach (var kv in _prof)
                {
                    _prof[kv.Key] = (0.0, 0);
                }
            }
        }

        /// <param name="navMesh">
        /// Optional one-shot mesh (the zone navmesh overlay) drawn as a single opaque instance before the
        /// sky. Null to skip. It is not part of the double-buffered frames because it is never
        /// re-extracted, so it is passed in directly rather than through a ShapeCache.
        /// </param>
        public void Render(Camera camera, PhysicsDebugFrame bodyFrame, StaticFrame staticFrame, MeshInstance? navMesh = null)
        {
            if (Profile)
            {
                _profSw.Restart();
            }
            if (Surface.Resolution.X != width || Surface.Resolution.Y != height)
            {
                OnResize();
            }

            //The mesh cache is shared with the physics (shard) thread, which allocates new mesh
            //geometry during extraction. Serialize the CPU-side pending-upload bookkeeping + GPU
            //upload with the extraction's lock (MeshCache) so the two threads don't race on the
            //vertex buffer / pending-upload list.
            lock (meshCache)
            {
                meshCache.FlushPendingUploads();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            // Note reversed depth.
            GL.ClearDepth(0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Greater);
            ProfileSection("upload+clear");

            // All ray traced shapes use analytic coverage writes to get antialiasing.
            GL.Enable(EnableCap.SampleAlphaToCoverage);
            ref var statics = ref staticFrame.Statics;
            SphereRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(statics.Spheres.Span), 0, statics.Spheres.Count);
            CapsuleRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(statics.Capsules.Span), 0, statics.Capsules.Count);
            CylinderRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(statics.Cylinders.Span), 0, statics.Cylinders.Count);
            ref var bodies = ref bodyFrame.Bodies;
            SphereRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(bodies.Spheres.Span), 0, bodies.Spheres.Count);
            CapsuleRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(bodies.Capsules.Span), 0, bodies.Capsules.Count);
            CylinderRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(bodies.Cylinders.Span), 0, bodies.Cylinders.Count);

            // Non-raytraced shapes just use regular opaque rendering.
            GL.Disable(EnableCap.SampleAlphaToCoverage);
            BoxRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(statics.Boxes.Span), 0, statics.Boxes.Count);
            BoxRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(bodies.Boxes.Span), 0, bodies.Boxes.Count);
            TriangleRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(statics.Triangles.Span), 0, statics.Triangles.Count);
            TriangleRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(bodies.Triangles.Span), 0, bodies.Triangles.Count);
            GL.Disable(EnableCap.CullFace);
            MeshRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(statics.Meshes.Span), 0, statics.Meshes.Count, _pool);
            MeshRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(bodies.Meshes.Span), 0, bodies.Meshes.Count, _pool);
            GL.Enable(EnableCap.CullFace);
            LineRenderer.Render(camera, Surface.Resolution, SpanConverter.AsSpan(bodyFrame.Lines.Span), 0, bodyFrame.Lines.Count);
            ProfileSection("shapes");

            // The one-shot navmesh overlay (walkable surface), drawn as a single mesh instance with culling
            // disabled (both faces) and before the sky so it is not overdrawn by the background.
            if (navMesh.HasValue)
            {
                GL.Disable(EnableCap.CullFace);
                MeshInstance[] navMeshSpan = { navMesh.Value };
                MeshRenderer.Render(camera, Surface.Resolution, navMeshSpan, 0, 1, _pool);
                GL.Enable(EnableCap.CullFace);
            }

            Background.Render(camera);
            ProfileSection("background");
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Resolve MSAA rendering down to a single sample buffer for screenspace work.
            // Note that we're not bothering to properly handle tonemapping during the resolve. That's going to hurt quality a little, but the demos don't make use of very wide ranges.
            // (If for some reason you end up expanding the demos to make use of wider HDR, you can make this a custom resolve pretty easily.)
            GL.BlitNamedFramebuffer(framebuffer, resolvedFramebuffer, 0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            ProfileSection("msaa-resolve");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, resolvedFramebuffer);

            // Glyph and screenspace line drawing rely on the same premultiplied alpha blending transparency. We'll handle their state out here.
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            ImageRenderer.PreparePipeline();
            ImageBatcher.Flush(Surface.Resolution, ImageRenderer);
            UILineBatcher.Flush(Surface.Resolution, UILineRenderer);
            GlyphRenderer.PreparePipeline();
            TextBatcher.Flush(Surface.Resolution, GlyphRenderer);
            GL.Disable(EnableCap.Blend);
            ProfileSection("ui");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            CompressToSwap.Render(resolvedColorBuffer);
            ProfileSection("post");
        }

        protected override void DoDispose()
        {
            Background.Dispose();
            CompressToSwap.Dispose();

            SphereRenderer.Dispose();
            CapsuleRenderer.Dispose();
            CylinderRenderer.Dispose();
            BoxRenderer.Dispose();
            TriangleRenderer.Dispose();
            MeshRenderer.Dispose();

            UILineRenderer.Dispose();
            GlyphRenderer.Dispose();

            GL.DeleteFramebuffer(framebuffer);
            GL.DeleteTexture(depthBuffer);
            GL.DeleteTexture(colorBuffer);
            GL.DeleteFramebuffer(resolvedFramebuffer);
            GL.DeleteTexture(resolvedColorBuffer);

            if (ownsMeshCache)
            {
                Shapes.Dispose();
            }
            else
            {
                // The MeshCache CPU state (vertex buffer, allocator, id sets) is owned by the
                // physics (shard) thread and disposed there. Only the GPU triangle buffer - which
                // was created on this GL thread - is disposed here.
                meshCache.DisposeGpuResources();
            }
        }
    }
}
