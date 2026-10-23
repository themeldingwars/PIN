// Thread-safe hand-off of the debug camera position from the GL (window) thread to the physics
// (shard) thread. The GL thread publishes the current camera position once per frame; the shard
// thread reads it to decide when to (re)extract the statics near the camera for the debug renderer.
// A published snapshot is immutable and swapped atomically, so the shard never observes a torn read.
#nullable enable
using System;
using System.Numerics;
using System.Threading;

namespace DemoRenderer
{
    /// <summary>
    ///     An immutable snapshot of a debug camera position.
    /// </summary>
    public readonly struct DebugCameraSnapshot
    {
        public readonly Vector3 Position;

        public DebugCameraSnapshot(Vector3 position)
        {
            Position = position;
        }
    }

    /// <summary>
    ///     Cross-thread camera slot. The writer (GL thread) calls <see cref="Publish" /> with the
    ///     current position; the reader (shard thread) calls <see cref="Read" /> to get the latest
    ///     snapshot. Safe for one writer / one reader; the snapshot swap is atomic.
    /// </summary>
    public sealed class DebugCameraState
    {
        // Holds a boxed DebugCameraSnapshot. Volatile reads give the cross-thread memory barrier so a
        // reader sees a fully-published snapshot, never a torn one.
        private object? box;

        /// <summary>
        ///     Seeds the slot with an initial camera position so the first (shard-side) streaming
        ///     extraction targets a meaningful spot even before the GL thread's first publish. Called
        ///     once on the shard thread at zone load with the debug window's default camera position.
        /// </summary>
        public void SetInitial(Vector3 position)
        {
            box = new DebugCameraSnapshot(position);
        }

        /// <summary>
        ///     Atomically publishes a new camera snapshot. Called by the GL (window) thread each frame.
        ///     One small boxed allocation per published frame (the GL frame rate); negligible for a
        ///     debug aid.
        /// </summary>
        public void Publish(Vector3 position)
        {
            box = new DebugCameraSnapshot(position);
        }

        /// <summary>
        ///     Reads the latest published snapshot. Returns the default (zero) snapshot before the
        ///     first publish.
        /// </summary>
        public DebugCameraSnapshot Read()
        {
            var current = Volatile.Read(ref box);
            return current is DebugCameraSnapshot s ? s : default;
        }
    }
}
