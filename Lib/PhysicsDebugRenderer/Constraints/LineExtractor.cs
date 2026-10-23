// Originally from bepuphysics2 (https://github.com/bepu/bepuphysics2)
// Adapted for the PIN physics debug renderer: extraction writes into a caller-provided
// QuickList<LineInstance> (part of a double-buffered frame) instead of a long-lived member list,
// so the physics (shard) thread can fill a snapshot the GL (window) thread renders independently.
using System;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;

namespace DemoRenderer.Constraints
{
    /// <summary>
    /// Extracts constraint, contact, and bounding-box debug lines from a Bepu <see cref="Simulation"/>
    /// into a caller-provided <see cref="QuickList{T}"/> of <see cref="LineInstance"/>. Reads the
    /// simulation - must run on the thread that owns it (the shard thread), never the GL thread.
    /// </summary>
    public class LineExtractor : IDisposable
    {
        ConstraintLineExtractor constraints;
        BoundingBoxLineExtractor boundingBoxes;

        BufferPool pool;
        ParallelLooper looper;
        LooperAction executeJobAction;

        public bool ShowConstraints = true;
        public bool ShowContacts;
        public bool ShowBoundingBoxes;

        public LineExtractor(BufferPool pool, ParallelLooper looper)
        {
            constraints = new ConstraintLineExtractor();
            boundingBoxes = new BoundingBoxLineExtractor();
            this.pool = pool;
            this.looper = looper;
            executeJobAction = ExecuteJob;
        }

        //The target line list is held by value for the duration of an extraction so the
        //multithreaded job callbacks can write into it; it is copied back to the caller at the end.
        QuickList<LineInstance> lines;
        Simulation simulation;
        QuickList<ConstraintLineExtractor.ThreadJob> constraintJobs;
        QuickList<BoundingBoxLineExtractor.ThreadJob> boundingBoxJobs;

        void ExecuteJob(int jobIndex, int workerIndex)
        {
            if (jobIndex >= constraintJobs.Count)
            {
                //This is a bounding box job.
                var job = boundingBoxJobs[jobIndex - constraintJobs.Count];
                boundingBoxes.ExecuteJob(lines.Span, simulation, job);
            }
            else
            {
                //This is a constraint job.
                var job = constraintJobs[jobIndex];
                constraints.ExecuteJob(lines.Span, simulation, job);
            }
        }

        /// <summary>
        /// Extracts constraint/contact/bounding-box lines into the given target list.
        /// Reads the simulation - must be called on the thread that owns it.
        /// </summary>
        public void Extract(Simulation simulation, ref QuickList<LineInstance> target, IThreadDispatcher threadDispatcher = null)
        {
            this.lines = target;
            this.simulation = simulation;
            if (ShowConstraints || ShowContacts)
            {
                constraintJobs = new QuickList<ConstraintLineExtractor.ThreadJob>(128, pool);
                constraints.CreateJobs(simulation, 0, ShowConstraints, ShowContacts, ref this.lines, ref constraintJobs, pool);
            }
            if (ShowBoundingBoxes)
            {
                boundingBoxJobs = new QuickList<BoundingBoxLineExtractor.ThreadJob>(128, pool);
                boundingBoxes.CreateJobs(simulation, 0, ref this.lines, ref boundingBoxJobs, pool);
            }
            looper.Dispatcher = threadDispatcher;
            looper.For(0, constraintJobs.Count + boundingBoxJobs.Count, executeJobAction);
            looper.Dispatcher = null;

            if (constraintJobs.Span.Allocated)
            {
                constraintJobs.Dispose(pool);
                constraintJobs = default;
            }
            if (boundingBoxJobs.Span.Allocated)
            {
                boundingBoxJobs.Dispose(pool);
                boundingBoxJobs = default;
            }
            this.simulation = null;
            target = this.lines;
        }

        public void ClearInstances(ref QuickList<LineInstance> target)
        {
            target.Count = 0;
        }

        //The line list is owned by the caller (the frame), not by this extractor, so there is
        //nothing to dispose here.
        public void Dispose()
        {
        }
    }
}
