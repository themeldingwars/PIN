// Double-buffered snapshot of the extracted physics debug geometry.
//
// The shard (physics) thread fills a FrameBuilder and atomically publishes it; the window
// (GL) thread reads the published frame and renders it. A published PhysicsDebugFrame is
// immutable - the producer builds a fresh frame each tick and swaps the reference, so the
// window never reads memory the producer is writing. The frame holds no Bepu Simulation
// references; only plain instance data.
#nullable enable
using System;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using DemoRenderer.Constraints;

namespace DemoRenderer
{
    /// <summary>
    /// One published frame: dynamic/kinematic body shapes, constraint/contact/bounding-box
    /// lines, and the show flags. Static (terrain) shapes live in a separate, rarely-updated
    /// frame (see StaticFrame) because they only change on zone load.
    /// </summary>
    public sealed class PhysicsDebugFrame
    {
        public ShapeDrawing.ShapeCache Bodies;
        public QuickList<LineInstance> Lines;
        public bool ShowConstraints;
        public bool ShowContacts;
        public bool ShowBoundingBoxes;

        public PhysicsDebugFrame(ShapeDrawing.ShapeCache bodies, QuickList<LineInstance> lines)
        {
            Bodies = bodies;
            Lines = lines;
        }

        public void Dispose(BufferPool pool)
        {
            Bodies.Dispose(pool);
            Lines.Dispose(pool);
        }
    }

    /// <summary>
    /// Snapshot of the static/terrain shapes. Published once per zone load.
    /// </summary>
    public sealed class StaticFrame
    {
        public ShapeDrawing.ShapeCache Statics;

        public StaticFrame(ShapeDrawing.ShapeCache statics)
        {
            Statics = statics;
        }

        public void Dispose(BufferPool pool)
        {
            Statics.Dispose(pool);
        }
    }

    /// <summary>
    /// Producer side of the body frame double buffer. Holds two frame objects and alternates
    /// between them: Begin() returns the "back" frame to fill; Publish() atomically makes it the
    /// published (visible) frame and flips to the other buffer. The window therefore always reads a
    /// frame the producer is not writing, so a physics tick can never mutate geometry mid-render.
    /// </summary>
    public sealed class FrameBuilder : IDisposable
    {
        private readonly BufferPool pool;
        private readonly DoubleBufferedFrame<PhysicsDebugFrame> published;
        private PhysicsDebugFrame? frameA;
        private PhysicsDebugFrame? frameB;
        private bool fillA;

        public FrameBuilder(BufferPool pool, DoubleBufferedFrame<PhysicsDebugFrame> published)
        {
            this.pool = pool;
            this.published = published;
        }

        public BufferPool Pool => pool;

        /// <summary>
        /// Starts a new frame fill on the back buffer, resetting its instance counts. The returned
        /// frame is then filled by the extractors before Publish().
        /// </summary>
        public PhysicsDebugFrame Begin()
        {
            if (fillA)
            {
                frameA = frameA ?? CreateFrame();
                frameA.Bodies.Clear();
                frameA.Lines.Count = 0;
                return frameA;
            }

            frameB = frameB ?? CreateFrame();
            frameB.Bodies.Clear();
            frameB.Lines.Count = 0;
            return frameB;
        }

        /// <summary>
        /// Publishes the currently-filling frame and flips to the other buffer as the new back.
        /// </summary>
        public void Publish()
        {
            var frame = fillA ? frameA : frameB;
            if (frame != null)
            {
                published.Publish(frame);
            }
            fillA = !fillA;
        }

        private PhysicsDebugFrame CreateFrame()
        {
            const int initialPerShape = 1024;
            const int initialLines = 8192;
            return new PhysicsDebugFrame(
                new ShapeDrawing.ShapeCache(initialPerShape, pool),
                new QuickList<LineInstance>(initialLines, pool));
        }

        public void Dispose()
        {
            frameA?.Dispose(pool);
            frameB?.Dispose(pool);
            frameA = null;
            frameB = null;
        }
    }

    /// <summary>
    /// Double buffer: the producer fills a frame and calls Publish (atomic swap); consumers
    /// read the currently published frame via Current. Safe across threads with no locking on
    /// the hot path.
    /// </summary>
    public sealed class DoubleBufferedFrame<TFrame> where TFrame : class
    {
        private TFrame? published;

        /// <summary>
        /// Atomically publishes a new frame as the currently visible one.
        /// </summary>
        public void Publish(TFrame frame)
        {
            System.Threading.Interlocked.Exchange(ref published, frame);
        }

        /// <summary>
        /// The currently published frame (may be null before the first publish).
        /// </summary>
        public TFrame? Current
        {
            get { return System.Threading.Volatile.Read(ref published); }
        }
    }

    /// <summary>
    /// Double buffer for the static/terrain frame.
    /// </summary>
    public sealed class DoubleBufferedStaticFrame
    {
        private StaticFrame? published;

        public void Publish(StaticFrame frame)
        {
            System.Threading.Interlocked.Exchange(ref published, frame);
        }

        public StaticFrame? Current
        {
            get { return System.Threading.Volatile.Read(ref published); }
        }
    }
}
