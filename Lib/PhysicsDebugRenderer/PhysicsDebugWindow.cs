// Window-thread host for the PIN physics debug renderer.
//
// Runs the OpenGL window and render loop on its own thread at an independent frame rate.
// It consumes the double-buffered frames published by the physics (shard) thread and never
// touches the Bepu simulation. Freecam only (v1): mouse-look + WASD/Space/Shift.
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using DemoContentLoader;
using DemoRenderer.Constraints;
using DemoRenderer.UI;
using DemoUtilities;
using OpenTK.Input;

namespace DemoRenderer
{
    public class PhysicsDebugWindow : IDisposable
    {
        private readonly Window window;
        private readonly Input input;
        private readonly RenderSurface surface;
        private readonly Renderer renderer;
        private readonly Camera camera;
        private readonly BufferPool pool;

        private readonly DoubleBufferedFrame<PhysicsDebugFrame> bodyFrames;
        private readonly DoubleBufferedStaticFrame staticFrames;
        private readonly PhysicsDebugFrame emptyBodyFrame;
        private readonly StaticFrame emptyStaticFrame;

        private readonly int frameRate;
        private readonly string title;
        // Maps a world position to a display string (e.g. "chunk 448_0004_0003 / sub 2x1"). Supplied by the
        // host (GameServer) so this vendored renderer project stays decoupled from the collision/zone types.
        private readonly Func<Vector3, string> positionResolver;
        // Optional GL->shard camera hand-off. When supplied (by the host), the window publishes the
        // live camera position here each frame so the shard can (re)extract statics near the camera.
        private readonly DebugCameraState cameraPublisher;
        // Optional GL->shard hand-off for the show-flags toggled by key (J/K/L). When supplied, the
        // window publishes the current flags on a toggle so the shard's next extraction applies them.
        private readonly DebugShowFlagsState showFlags;
        // Host-supplied provider for the one-shot navmesh overlay mesh (the zone's walkable surface,
        // built by the shard from the loaded DtNavMesh). Returns null until it is built. This is a
        // delegate so this vendored renderer project stays free of the GameServer's navmesh types.
        private readonly Func<ShapeDrawing.MeshInstance?> navMeshProvider;
        // Local (GL-thread-owned) copies of the show-flags. These are the source of truth for what is
        // displayed and what is published on a toggle; the shard reads the published snapshot.
        private bool showConstraints;
        private bool showContacts;
        private bool showBoundingBoxes;
        // GL-thread-owned toggle for the navmesh overlay (key N). Purely a render decision: the mesh is
        // uploaded once by the shard and this flag only controls whether it is drawn this frame.
        private bool showNavMesh;
        private readonly TextBuilder uiText;
        private readonly Font font;
        private const float MouseSensitivity = 0.002f;
        private const float BaseMoveSpeed = 20f;
        // The freecam's starting position, shared with the host so the first camera-relative static
        // extraction (which runs on the shard thread before the GL thread's first publish) targets the
        // same spot the window opens on rather than the origin.
        public static readonly Vector3 DefaultCameraPosition = new Vector3(0f, -50f, 30f);
        private bool disposed;
        //Temporary diagnostic companion to BoundingBoxLineExtractor: dump the camera pose ~1x/sec
        //when PIN_DEBUG_RENDERER_LOG_BOXES=1 so world-space box data can be compared to the view.
        private static readonly bool s_logCamera =
            System.Environment.GetEnvironmentVariable("PIN_DEBUG_RENDERER_LOG_BOXES") == "1";
        private double _lastCameraLog;

        public PhysicsDebugWindow(
            string title,
            Int2 resolution,
            DoubleBufferedFrame<PhysicsDebugFrame> bodyFrames,
            DoubleBufferedStaticFrame staticFrames,
            int frameRate = 30,
            Vector3? initialCameraPosition = null,
            ShapeDrawing.MeshCache sharedMeshCache = null,
            Func<Vector3, string> positionResolver = null,
            DebugCameraState cameraPublisher = null,
            DebugShowFlagsState showFlags = null,
            Func<ShapeDrawing.MeshInstance?> navMeshProvider = null,
            bool showNavMesh = false)
        {
            this.bodyFrames = bodyFrames;
            this.staticFrames = staticFrames;
            this.frameRate = Math.Max(1, frameRate);
            this.title = title;
            this.positionResolver = positionResolver ?? (_ => string.Empty);
            this.cameraPublisher = cameraPublisher;
            this.showFlags = showFlags;
            this.navMeshProvider = navMeshProvider;
            this.showNavMesh = showNavMesh;
            // Seed the local show-flags from the shared state (already SetInitial'd by the host from the
            // engine settings) so the on-screen state and the shard's first extraction agree. When no
            // state is supplied (standalone window) fall back to the settings defaults.
            var initialFlags = showFlags?.Read() ?? new DebugShowFlagsSnapshot(true, false, true);
            showConstraints = initialFlags.ShowConstraints;
            showContacts = initialFlags.ShowContacts;
            showBoundingBoxes = initialFlags.ShowBoundingBoxes;
            pool = new BufferPool();
            window = new Window(title, resolution, WindowMode.Windowed);
            input = new Input(window, pool);
            surface = new RenderSurface(window.WindowInfo, window.Resolution, enableDeviceDebugLayer: false);
            renderer = sharedMeshCache != null
                ? new Renderer(surface, sharedMeshCache)
                : new Renderer(surface);
            //Optional GPU-accurate per-pass profiling (expensive: GL.Finish per pass). Enable with
            //PIN_DEBUG_RENDERER_PROFILE=1 to attribute the render+present GPU cost to each pass.
            renderer.Profile = Environment.GetEnvironmentVariable("PIN_DEBUG_RENDERER_PROFILE") == "1";
            camera = new Camera(window.Resolution.X / (float)window.Resolution.Y, (float)Math.PI / 2, 0.01f, 512);
            camera.Position = initialCameraPosition ?? DefaultCameraPosition;
            uiText = new TextBuilder(128);

            //Load the debug overlay font from the embedded content archive. If it is missing (e.g. an
            //older archive) the overlay simply falls back to coordinates only - never fail the window.
            font = null;
            try
            {
                var fontContent = renderer.Content.Load<FontContent>(@"Content\Carlito-Regular.ttf");
                font = new Font(fontContent);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[PhysicsDebugWindow] Could not load debug font, overlay shows coordinates only: {e.Message}");
            }

            //Empty fallback frames used until the first real frame is published.
            emptyBodyFrame = new PhysicsDebugFrame(new ShapeDrawing.ShapeCache(16, pool), new QuickList<LineInstance>(16, pool));
            emptyStaticFrame = new StaticFrame(new ShapeDrawing.ShapeCache(16, pool));
        }

        /// <summary>
        /// Runs the window/render loop on the calling thread until the window is closed or the
        /// cancellation token fires. Must be called on a dedicated thread (the GL context is made
        /// current on this thread in the constructor).
        /// </summary>
        public void Run(CancellationToken cancellationToken)
        {
            var targetFrameTime = 1.0 / frameRate;
            //Measure the real (capped) frame rate and the per-phase time breakdown so we can tell
            //whether the window is GPU-bound (render), vsync-bound (present), sleeping (cap), or
            //otherwise. Camera movement is dt-scaled, so the movement speed alone does not reveal
            //the render rate.
            var fpsStopwatch = new Stopwatch();
            fpsStopwatch.Start();
            var frameCount = 0;
            var lastFpsUpdate = fpsStopwatch.Elapsed.TotalSeconds;

            var phaseStopwatch = new Stopwatch();
            double updateMsSum = 0;
            double renderMsSum = 0;
            double presentMsSum = 0;
            double sleepMsSum = 0;
            double frameMsSum = 0;
            var phaseFrameCount = 0;
            var lastPhaseUpdate = fpsStopwatch.Elapsed.TotalSeconds;

            window.Run(
                dt =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        window.Close();
                        return;
                    }

                    phaseStopwatch.Restart();

                    input.Start();
                    UpdateCamera(dt);
                    ProcessToggles();
                    if (s_logCamera && fpsStopwatch.Elapsed.TotalSeconds - _lastCameraLog >= 1.0)
                    {
                        _lastCameraLog = fpsStopwatch.Elapsed.TotalSeconds;
                        Console.WriteLine($"[dbgcam] pos={camera.Position} fwd={camera.Forward} right={camera.Right} up={camera.Up} yaw={camera.Yaw:0.000} pitch={camera.Pitch:0.000}");
                    }
                    var bodyFrame = bodyFrames.Current ?? emptyBodyFrame;
                    var staticFrame = staticFrames.Current ?? emptyStaticFrame;
                    var updateMs = phaseStopwatch.Elapsed.TotalMilliseconds;

                    //Hand the live camera position to the shard so it can (re)extract statics near the
                    //camera for the debug renderer (see DebugCameraState). No-op when not supplied.
                    cameraPublisher?.Publish(camera.Position);

                    //Queue the camera-position overlay into the text batcher; renderer.Render flushes it.
                    DrawOverlay(camera.Position);
                    //Draw the one-shot navmesh overlay while the toggle (N) is on and the shard has built it.
                    ShapeDrawing.MeshInstance? navInstance = showNavMesh && navMeshProvider != null
                        ? navMeshProvider()
                        : null;
                    renderer.Render(camera, bodyFrame, staticFrame, navInstance);
                    var renderMs = phaseStopwatch.Elapsed.TotalMilliseconds - updateMs;

                    surface.Present();
                    var presentMs = phaseStopwatch.Elapsed.TotalMilliseconds - updateMs - renderMs;

                    input.End();

                    //Frame pacing: sleep only the time remaining until the target frame duration, so
                    //the cap bounds the rate without stacking on top of work that already exceeded the
                    //target. If render+present took longer than the frame budget, sleep 0 and let the
                    //frame run as fast as the work allows (the rate is then limited by the work itself).
                    var workMs = phaseStopwatch.Elapsed.TotalMilliseconds;
                    var remainingMs = targetFrameTime * 1000.0 - workMs;
                    var sleepMs = 0.0;
                    if (remainingMs > 0)
                    {
                        Thread.Sleep((int)Math.Ceiling(remainingMs));
                        sleepMs = phaseStopwatch.Elapsed.TotalMilliseconds - workMs;
                    }
                    var frameMs = phaseStopwatch.Elapsed.TotalMilliseconds;

                    frameCount++;
                    updateMsSum += updateMs;
                    renderMsSum += renderMs;
                    presentMsSum += presentMs;
                    sleepMsSum += sleepMs;
                    frameMsSum += frameMs;
                    phaseFrameCount++;

                    var now = fpsStopwatch.Elapsed.TotalSeconds;
                    if (now - lastFpsUpdate >= 1.0)
                    {
                        var measuredFps = frameCount / (now - lastFpsUpdate);
                        window.SetTitle($"{title}  [{measuredFps:0.#} fps / target {frameRate}]");
                        frameCount = 0;
                        lastFpsUpdate = now;
                    }
                    if (now - lastPhaseUpdate >= 1.0 && phaseFrameCount > 0)
                    {
                        Console.WriteLine(
                            $"[PhysicsDebugWindow] {frameRate:0}fps cap | " +
                            $"update {updateMsSum / phaseFrameCount:0.0}ms | " +
                            $"render {renderMsSum / phaseFrameCount:0.0}ms | " +
                            $"present {presentMsSum / phaseFrameCount:0.0}ms | " +
                            $"sleep {sleepMsSum / phaseFrameCount:0.0}ms | " +
                            $"frame {frameMsSum / phaseFrameCount:0.0}ms");
                        updateMsSum = renderMsSum = presentMsSum = sleepMsSum = frameMsSum = 0;
                        phaseFrameCount = 0;
                        lastPhaseUpdate = now;
                    }
                },
                resolution => OnResize(resolution));
        }

        private void UpdateCamera(float dt)
        {
            if (input.WasPushed(MouseButton.Left))
            {
                input.MouseLocked = !input.MouseLocked;
            }
            if (input.WasPushed(Key.Escape) && !input.MouseLocked)
            {
                window.Close();
                return;
            }

            if (input.MouseLocked)
            {
                var delta = input.MouseDelta;
                camera.Yaw += delta.X * MouseSensitivity;
                camera.Pitch += delta.Y * MouseSensitivity;
            }

            var speed = BaseMoveSpeed * dt;
            if (input.IsDown(Key.W))
                camera.Position += camera.Forward * speed;
            if (input.IsDown(Key.S))
                camera.Position -= camera.Forward * speed;
            if (input.IsDown(Key.A))
                camera.Position -= camera.Right * speed;
            if (input.IsDown(Key.D))
                camera.Position += camera.Right * speed;
            if (input.IsDown(Key.Space))
                camera.Position += Vector3.UnitZ * speed;
            if (input.IsDown(Key.C))
                camera.Position -= Vector3.UnitZ * speed;
        }

        /// <summary>
        ///     Handles the show-flag toggle keys (J = constraints, K = contacts, L = bounding boxes,
        ///     matching the old demo harness) and publishes the new flags to the shard so the next body
        ///     extraction applies them. No-op when no shared show-flags state was supplied.
        /// </summary>
        private void ProcessToggles()
        {
            // The navmesh toggle is a GL-local render flag (the mesh is uploaded once by the shard); it
            // works even in a standalone window with no shared show-flags state, so handle it first.
            if (input.WasPushed(Key.N))
            {
                showNavMesh = !showNavMesh;
            }

            if (showFlags == null)
            {
                return;
            }

            var changed = false;
            if (input.WasPushed(Key.J))
            {
                showConstraints = !showConstraints;
                changed = true;
            }
            if (input.WasPushed(Key.K))
            {
                showContacts = !showContacts;
                changed = true;
            }
            if (input.WasPushed(Key.L))
            {
                showBoundingBoxes = !showBoundingBoxes;
                changed = true;
            }
            if (changed)
            {
                showFlags.Publish(showConstraints, showContacts, showBoundingBoxes);
            }
        }

        /// <summary>
        ///     Queues the on-screen camera-position overlay (world x/y/z plus the resolved zone chunk /
        ///     subchunk) into the text batcher. <c>Renderer.Render</c> flushes it at the end of the
        ///     frame. The resolver is a host-supplied delegate so this project stays free of the
        ///     collision/zone types.
        /// </summary>
        private void DrawOverlay(Vector3 position)
        {
            if (font == null)
            {
                return;
            }

            const float textHeight = 16f;
            const float inset = 16f;
            var lineSpacing = textHeight * 1.3f;
            var varRes = window.Resolution;

            var posText = uiText.Clear()
                .Append("Pos: ")
                .Append(position.X, 2)
                .Append("  ")
                .Append(position.Y, 2)
                .Append("  ")
                .Append(position.Z, 2);
            renderer.TextBatcher.Write(posText, new Vector2(inset, varRes.Y - inset - textHeight), textHeight, new Vector3(1f, 1f, 0.85f), font);

            var resolved = positionResolver(position);
            if (!string.IsNullOrEmpty(resolved))
            {
                renderer.TextBatcher.Write(
                    uiText.Clear().Append(resolved),
                    new Vector2(inset, varRes.Y - inset - textHeight - lineSpacing),
                    textHeight,
                    new Vector3(0.65f, 1f, 0.65f),
                    font);
            }

            // Show the current show-flag states with their toggle keys so the user can see and change
            // them at a glance (mirrors the old harness' timing-display flags).
            var flagsText = uiText.Clear()
                .Append("Constraints [J]: ").Append(showConstraints ? "on" : "off")
                .Append("   Contacts [K]: ").Append(showContacts ? "on" : "off")
                .Append("   BoundingBoxes [L]: ").Append(showBoundingBoxes ? "on" : "off")
                .Append("   NavMesh [N]: ").Append(showNavMesh ? "on" : "off");
            renderer.TextBatcher.Write(
                flagsText,
                new Vector2(inset, varRes.Y - inset - textHeight - lineSpacing * 2),
                textHeight,
                new Vector3(0.85f, 0.85f, 1f),
                font);
        }

        private void OnResize(Int2 resolution)
        {
            renderer.Surface.Resize(resolution, false);
            camera.AspectRatio = resolution.X / (float)resolution.Y;
        }

        public void Dispose()
        {
            if (!disposed)
            {
        disposed = true;
            input.Dispose();
            renderer.Dispose();
            font?.Dispose();
            emptyBodyFrame.Dispose(pool);
                emptyStaticFrame.Dispose(pool);
                pool.Clear();
                window.Dispose();
            }
        }
    }
}
