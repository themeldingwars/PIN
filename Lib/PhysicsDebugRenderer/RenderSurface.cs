// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform;
using BepuUtilities;

namespace DemoRenderer
{
    /// <summary>
    /// The swap chain, drawing surfaces, and the device contexts associated with a window.
    /// </summary>
    public class RenderSurface : Disposable
    {
        private readonly IWindowInfo window;
        private readonly IGraphicsContext context;

        // MSAA sample count. Overridable via PIN_DEBUG_SAMPLES (default 4) to test the samples=1
        // (no MSAA) approach for the debug renderer, where antialiasing is pure overhead. Must be
        // read before any context is created; used by both the GraphicsMode and the Renderer's
        // TexImage2DMultisample calls so they stay consistent.
        public static int SampleCount;
        static RenderSurface()
        {
            var samples = 4;
            var env = Environment.GetEnvironmentVariable("PIN_DEBUG_SAMPLES");
            if (env != null && int.TryParse(env, out var parsed) && parsed >= 1)
            {
                samples = parsed;
            }
            SampleCount = samples;
        }

        /// <summary>
        /// Gets the current resolution of the render surface. To change the resolution, use Resize.
        /// </summary>
        public Int2 Resolution { get; private set; }

        /// <summary>
        /// Constructs a new swap surface.
        /// </summary> 
        /// <param name="window">Window to build a swap chain and drawing surface for.</param>
        /// <param name="resolution">Resolution of the rendering surface.</param>
        /// <param name="enableDeviceDebugLayer">Whether to use the debug layer for this window's graphics device.</param>
        public RenderSurface(IWindowInfo window, Int2 resolution, bool fullScreen = false, bool enableDeviceDebugLayer = false)
        {
            this.window = window;
            context = new GraphicsContext(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, SampleCount), window, 4, 6, GraphicsContextFlags.Default);
            context.MakeCurrent(window);
            context.LoadAll();
            //Swap interval (vsync). 0 = unlimited (no vsync), 1 = vsync on. Overridable via the
            //PIN_DEBUG_SWAP_INTERVAL env var for A/B testing whether the SwapBuffers cost is the
            //driver pacing the swap to the display's vertical blank (in which case 1 snaps it to one
            //refresh period) or genuine GPU work. The obsolete IGraphicsContext.VSync property does
            //not reliably set the interval on Linux/OpenGL drivers, so use SwapInterval.
            var swapInterval = 0;
            var swapIntervalEnv = Environment.GetEnvironmentVariable("PIN_DEBUG_SWAP_INTERVAL");
            if (swapIntervalEnv != null && int.TryParse(swapIntervalEnv, out var parsedSwapInterval))
            {
                swapInterval = parsedSwapInterval;
            }
            context.SwapInterval = swapInterval;
            if (enableDeviceDebugLayer)
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
                {
                    Console.Error.WriteLine($"{source}, {type}, {id}, {severity}, {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message)}");
                    if (type == DebugType.DebugTypeError) throw new Exception();
                }, IntPtr.Zero);
            }
            Resolution = resolution;
            GL.Viewport(0, 0, Resolution.X, Resolution.Y);
        }

        public void Resize(Int2 resolution, bool fullScreen)
        {
            Resolution = resolution;
            context.MakeCurrent(window);
            GL.Viewport(0, 0, Resolution.X, Resolution.Y);
        }

        public void Present() => context.SwapBuffers();

        protected override void DoDispose() => context.Dispose();
    }
}
