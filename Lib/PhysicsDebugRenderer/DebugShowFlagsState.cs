// Thread-safe hand-off of the debug show-flags (constraint lines / contact points / bounding boxes)
// from the GL (window) thread to the physics (shard) thread. The GL thread publishes the current
// flags when a toggle key is pressed; the shard thread reads them before each body/line extraction so
// a toggle takes effect on the next physics tick. A published snapshot is immutable and swapped
// atomically, so the shard never observes a torn read.
#nullable enable
using System.Threading;

namespace DemoRenderer
{
    /// <summary>
    ///     An immutable snapshot of the debug show-flags.
    /// </summary>
    public readonly struct DebugShowFlagsSnapshot
    {
        public readonly bool ShowConstraints;
        public readonly bool ShowContacts;
        public readonly bool ShowBoundingBoxes;

        public DebugShowFlagsSnapshot(bool showConstraints, bool showContacts, bool showBoundingBoxes)
        {
            ShowConstraints = showConstraints;
            ShowContacts = showContacts;
            ShowBoundingBoxes = showBoundingBoxes;
        }
    }

    /// <summary>
    ///     Cross-thread slot for the debug show-flags. The writer (GL thread) calls <see cref="Publish" />
    ///     when a toggle key is pressed; the reader (shard thread) calls <see cref="Read" /> before each
    ///     extraction. Safe for one writer / one reader; the snapshot swap is atomic.
    /// </summary>
    public sealed class DebugShowFlagsState
    {
        // Holds a boxed DebugShowFlagsSnapshot. Volatile reads give the cross-thread memory barrier so a
        // reader sees a fully-published snapshot, never a torn one.
        private object? box;

        /// <summary>
        ///     Seeds the slot with the initial flags (from the engine settings) so the first extraction and
        ///     the window's on-screen state agree before the GL thread presses a key. Called once on the
        ///     shard thread when the renderer is initialized.
        /// </summary>
        public void SetInitial(bool showConstraints, bool showContacts, bool showBoundingBoxes)
        {
            box = new DebugShowFlagsSnapshot(showConstraints, showContacts, showBoundingBoxes);
        }

        /// <summary>
        ///     Atomically publishes a new show-flags snapshot. Called by the GL (window) thread on a toggle
        ///     key press. One small boxed allocation per press; negligible for a debug aid.
        /// </summary>
        public void Publish(bool showConstraints, bool showContacts, bool showBoundingBoxes)
        {
            box = new DebugShowFlagsSnapshot(showConstraints, showContacts, showBoundingBoxes);
        }

        /// <summary>
        ///     Reads the latest published snapshot. Falls back to the settings defaults (constraints on,
        ///     contacts off, bounding boxes on) before the first publish.
        /// </summary>
        public DebugShowFlagsSnapshot Read()
        {
            var current = Volatile.Read(ref box);
            return current is DebugShowFlagsSnapshot s ? s : new DebugShowFlagsSnapshot(true, false, true);
        }
    }
}
