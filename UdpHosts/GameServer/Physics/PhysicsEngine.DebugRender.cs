#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using DemoRenderer;
using DemoRenderer.Constraints;
using DemoRenderer.ShapeDrawing;
using GameServer.Navigation;
using Serilog;

namespace GameServer.Physics;

/// <summary>
///     Optional in-process physics debug renderer (producer side).
///
///     When enabled by <see cref="PhysicsEngineSettings.EnablePhysicsDebugRenderer" />, this partial
///     fills double-buffered frames with the extracted Bepu body/static/line geometry after each
///     physics step and on zone load. The frames are published by atomic reference swap and consumed
///     by a GL window (the <see cref="PhysicsDebugWindow" />) on its own thread, which never touches
///     the simulation. All simulation reads happen here, on the shard thread, immediately after
///     <c>Simulation.Timestep</c> - never on the render thread.
/// </summary>
public partial class PhysicsEngine
{
    private const float _debugStaticStreamRadius = 512f;
    private const float _debugStaticResampleDistance = 256f;

    private readonly ILogger _debugRenderLogger = Log.Logger.ForContext<PhysicsEngine>();
    private readonly Stopwatch _debugExtractStopwatch = new Stopwatch();

    private bool _debugRendererEnabled;
    private bool _debugRendererShowConstraints;
    private bool _debugRendererShowContacts;
    private bool _debugRendererShowBoundingBoxes;
    private int _debugRendererFrameRate = 60;

    private DoubleBufferedFrame<PhysicsDebugFrame>? _debugBodyFrames;
    private DoubleBufferedStaticFrame? _debugStaticFrames;
    private FrameBuilder? _debugFrameBuilder;
    private LineExtractor? _debugLineExtractor;
    private ShapesExtractor? _debugShapesExtractor;

    // Camera-relative static streaming state. A zone's static mesh data is far too large to cache
    // all at once, so instead of extracting every static's geometry up front we keep a lightweight index of all
    // statics (position + shape ref + bounding radius, no vertex data) and (re)extract only the
    // statics within a radius of the debug camera each time the camera moves far enough. The vertex
    // cache then only ever holds what is actually being looked at.
    private DebugStaticIndex? _debugStaticIndex;
    private DebugCameraState? _debugCameraState;
    private DebugShowFlagsState? _debugShowFlagsState;
    private Vector3 _debugLastExtractedCamera = new Vector3(float.NaN, float.NaN, float.NaN);

    // One-shot navmesh overlay mesh (walkable surface, NOT a Bepu shape). Extracted once from the
    // loaded DtNavMesh and uploaded once to the shared vertex ring; drawn only while the window's
    // navmesh toggle is on. Kept outside the camera-streamed static frame so it is never cleared or
    // re-extracted when the camera moves.
    private DebugNavMeshMesh? _debugNavMeshMesh;

    // Double-buffered, reused static frames. Two frames alternate so the shard can refill the back
    // frame while the GL thread renders the front one; reusing (not recreating) them each
    // (re)extraction avoids returning a frame the GL thread may still be reading to the pool.
    private StaticFrame? _debugStaticFrameA;
    private StaticFrame? _debugStaticFrameB;
    private bool _debugStaticFillA;

    // Extraction timing (shard thread) - accumulated per tick and logged ~1x/second so we can tell
    // how much of the tick budget the debug extraction consumes.
    private double _debugExtractMsSum;
    private int _debugExtractCount;
    private double _debugExtractLastLog;

    /// <summary>
    ///     The double buffer of body/line frames, exposed so the shard can spawn a window that
    ///     consumes it. Null when the renderer is disabled.
    /// </summary>
    public DoubleBufferedFrame<PhysicsDebugFrame>? DebugBodyFrames => _debugBodyFrames;

    /// <summary>
    ///     The double buffer of static frames, exposed so the shard can spawn a window. Null when
    ///     the renderer is disabled.
    /// </summary>
    public DoubleBufferedStaticFrame? DebugStaticFrames => _debugStaticFrames;

    public int DebugRendererFrameRate => _debugRendererFrameRate;

    /// <summary>
    ///     The mesh vertex cache (triangle data + GPU buffer) shared with the render window. The
    ///     renderer must bind to this same instance so that the <c>VertexStart</c>/<c>VertexCount</c>
    ///     written by extraction map onto the vertex data uploaded here. Null when the renderer is
    ///     disabled.
    /// </summary>
    public MeshCache? DebugRendererMeshCache => _debugShapesExtractor?.MeshCache;

    /// <summary>
    ///     The zone chunk/subchunk spatial index, rebuilt on each zone load, so the debug renderer can
    ///     resolve the camera world position to a chunk/subchunk name. Always non-null (the zone loader
    ///     owns it); empty until the first zone load completes.
    /// </summary>
    public Shared.Collision.ZoneLoading.ZoneSpatialIndex DebugZoneSpatialIndex => _zoneLoader.SpatialIndex;

    /// <summary>
    ///     The camera slot the debug window publishes its live position into, so the shard can decide
    ///     when to (re)extract statics near the camera. Null when the renderer is disabled.
    /// </summary>
    public DemoRenderer.DebugCameraState? DebugCameraState => _debugCameraState;

    /// <summary>
    ///     The show-flags slot the debug window publishes toggles into (J/K/L) and the shard reads before
    ///     each body/line extraction. Null when the renderer is disabled.
    /// </summary>
    public DemoRenderer.DebugShowFlagsState? DebugShowFlagsState => _debugShowFlagsState;

    /// <summary>
    ///     The one-shot navmesh overlay mesh (a single <see cref="MeshInstance" /> pointing at the uploaded
    ///     walkable-surface triangles), or null until it has been built / when the renderer is disabled. The
    ///     debug window reads the <c>Instance</c> (a PhysicsDebugRenderer type) while its navmesh toggle is on.
    /// </summary>
    public DebugNavMeshMesh? DebugNavMesh => _debugNavMeshMesh;

    /// <summary>
    ///     Extracts the current body + line geometry into a new frame and publishes it. Called on the
    ///     shard thread after each physics step. No-op when the renderer is disabled.
    /// </summary>
    public void DebugRenderExtractBodies()
    {
        if (!_debugRendererEnabled || _debugBodyFrames == null || _debugFrameBuilder == null ||
            _debugLineExtractor == null || _debugShapesExtractor == null)
        {
            return;
        }

        _debugExtractStopwatch.Restart();

        // Pick up any show-flag toggles the debug window published (J/K/L) and apply them to the line
        // extractor before extraction so the toggle takes effect on this tick.
        if (_debugShowFlagsState != null)
        {
            var flags = _debugShowFlagsState.Read();
            _debugLineExtractor.ShowConstraints = flags.ShowConstraints;
            _debugLineExtractor.ShowContacts = flags.ShowContacts;
            _debugLineExtractor.ShowBoundingBoxes = flags.ShowBoundingBoxes;
        }

        var frame = _debugFrameBuilder.Begin();
        _debugShapesExtractor.AddInstances(Simulation, ref frame.Bodies, ThreadDispatcher);
        _debugLineExtractor.Extract(Simulation, ref frame.Lines, ThreadDispatcher);
        frame.ShowConstraints = _debugRendererShowConstraints;
        frame.ShowContacts = _debugRendererShowContacts;
        frame.ShowBoundingBoxes = _debugRendererShowBoundingBoxes;
        _debugFrameBuilder.Publish();

        _debugExtractMsSum += _debugExtractStopwatch.Elapsed.TotalMilliseconds;
        _debugExtractCount++;
        var now = Environment.TickCount64 / 1000.0;
        if (now - _debugExtractLastLog >= 1.0 && _debugExtractCount > 0)
        {
            _debugRenderLogger.Information(
                "Physics debug extract (bodies): {AvgMs:0.00} ms avg over {Count} ticks",
                _debugExtractMsSum / _debugExtractCount,
                _debugExtractCount);
            _debugExtractMsSum = 0;
            _debugExtractCount = 0;
            _debugExtractLastLog = now;
        }
    }

    /// <summary>
    ///     Builds the lightweight statics spatial index on zone load. This records each static's
    ///     position, shape reference, and bounding radius, but allocates NO mesh vertex data. The
    ///     (very large) vertex data is instead streamed camera-relative via
    ///     <see cref="DebugRenderExtractStaticsNear" />, so the vertex cache never has to hold the
    ///     whole zone at once. Called on the shard thread after a zone load. No-op when disabled.
    /// </summary>
    public void DebugRenderExtractStatics()
    {
        if (!_debugRendererEnabled || _debugStaticFrames == null)
        {
            return;
        }

        _debugExtractStopwatch.Restart();
        _debugStaticIndex = DebugStaticIndex.Build(Simulation, BufferPool);
        _debugLastExtractedCamera = new Vector3(float.NaN, float.NaN, float.NaN);

        // Seed the camera hand-off with the window's default freecam position so the first streaming
        // extraction (fired on the next tick by the NaN check) targets that spot rather than the
        // origin, which the GL thread may not have published yet.
        _debugCameraState?.SetInitial(PhysicsDebugWindow.DefaultCameraPosition);

        // Create the two reused static frames. The back frame (fillA ? A : B) is the one the shard
        // will refill first, so publish the OTHER one (empty) as the initially-visible frame: this
        // keeps the frame being refilled distinct from the one the GL thread renders at all times.
        _debugStaticFrameA = new StaticFrame(new ShapeCache(4096, BufferPool));
        _debugStaticFrameB = new StaticFrame(new ShapeCache(4096, BufferPool));
        _debugStaticFillA = true;
        _debugStaticFrames.Publish(_debugStaticFrameB);
        _debugRenderLogger.Information(
            "Physics debug static index built: {Count} statics (camera-relative streaming, radius {Radius}m)",
            _debugStaticIndex.Count,
            _debugStaticStreamRadius);
    }

    /// <summary>
    ///     (Re)extracts the statics within <see cref="_debugStaticStreamRadius" /> of the given camera
    ///     position into a new static frame and publishes it. Only the nearby statics' (large) mesh
    ///     vertex data is allocated into the shared vertex cache, keeping it bounded to what is being
    ///     looked at. Called on the shard thread when the debug camera has moved far enough from the
    ///     last extraction. No-op when the renderer is disabled or the index is not built yet.
    /// </summary>
    /// <param name="cameraPosition">The world-space camera position the statics are streamed around.</param>
    public void DebugRenderExtractStaticsNear(Vector3 cameraPosition)
    {
        if (!_debugRendererEnabled || _debugStaticFrames == null || _debugShapesExtractor == null ||
            _debugStaticIndex == null)
        {
            return;
        }

        if (_debugStaticFrameA == null || _debugStaticFrameB == null)
        {
            return;
        }

        _debugExtractStopwatch.Restart();

        // A fresh extraction pass: the vertex cache will only upload meshes not already uploaded this
        // pass, and the previous pass's out-of-view meshes are reclaimed by the GL thread's flush.
        _debugShapesExtractor.MeshCache.BeginExtraction();

        // Fill the back frame (clearing it) and publish it, flipping to the other buffer. The frame
        // being refilled is the one the GL thread is NOT currently rendering.
        var frame = _debugStaticFillA ? _debugStaticFrameA : _debugStaticFrameB;

        // Work on the frame's ShapeCache BY REFERENCE. Copying the struct and mutating the copy would
        // (a) never publish the filled instances, and (b) let a QuickList growth inside the copy return
        // the buffer the frame still references (QuickList.Resize disposes the old span) - a dangling
        // buffer that is returned a second time on the next refill and trips the pool's
        // "must have been taken from the pool previously" assertion.
        ref var statics = ref frame.Statics;
        statics.Clear();

        var shapes = Simulation.Shapes;
        var pool = BufferPool;
        var matches = new List<(Vector3 Position, Quaternion Orientation, TypedIndex Shape, float Radius)>();
        _debugStaticIndex.Query(
            cameraPosition,
            _debugStaticStreamRadius,
            (position, orientation, shape, radius) => matches.Add((position, orientation, shape, radius)));
        for (var i = 0; i < matches.Count; ++i)
        {
            var m = matches[i];
            _debugShapesExtractor.AddStaticByShape(
                shapes, m.Shape, new RigidPose(m.Position, m.Orientation), ref statics, pool);
        }

        // Reclaim ring regions that fell out of the last two extractions (the frame being rendered
        // plus the one being filled). Done on the shard, once per extraction, so the render thread
        // never pays an id-membership scan. Safe because the GL thread's flush only uploads here and
        // the front frame's meshes are kept alive by the "previous" live set.
        lock (_debugShapesExtractor.MeshCache)
        {
            _debugShapesExtractor.MeshCache.ReclaimStaleMeshes();
        }

        _debugStaticFrames.Publish(frame);
        _debugStaticFillA = !_debugStaticFillA;
        _debugLastExtractedCamera = cameraPosition;
        var total = statics.Spheres.Count + statics.Capsules.Count + statics.Cylinders.Count +
                    statics.Boxes.Count + statics.Triangles.Count + statics.Meshes.Count;
        _debugRenderLogger.Information(
            "Physics debug statics near camera: {Ms:0.0} ms, {Total} instances ({Meshes} meshes)",
            _debugExtractStopwatch.Elapsed.TotalMilliseconds,
            total,
            statics.Meshes.Count);
    }

    /// <summary>
    ///     Reads the live debug camera position and, when it has moved at least
    ///     <see cref="_debugStaticResampleDistance" /> from the last extraction, (re)extracts the statics
    ///     near it. The first extraction happens as soon as a camera position is available so terrain
    ///     appears without waiting for a big camera move. Called on the shard thread each physics tick.
    ///     No-op when the renderer is disabled or the static index is not built yet.
    /// </summary>
    public void DebugRenderUpdateStaticStream()
    {
        if (!_debugRendererEnabled || _debugCameraState == null || _debugStaticIndex == null)
        {
            return;
        }

        var cameraPosition = _debugCameraState.Read().Position;
        if (float.IsNaN(_debugLastExtractedCamera.X))
        {
            DebugRenderExtractStaticsNear(cameraPosition);
            return;
        }

        var delta = cameraPosition - _debugLastExtractedCamera;
        if (delta.LengthSquared() >= _debugStaticResampleDistance * _debugStaticResampleDistance)
        {
            DebugRenderExtractStaticsNear(cameraPosition);
        }
    }

    /// <summary>
    ///     Extracts the zone's navmesh walkable surface (a single render mesh, NOT a Bepu shape) into the
    ///     shared vertex cache exactly once and records a single <see cref="MeshInstance" /> for the debug
    ///     window to draw. Reads the loaded <see cref="NavMeshService" /> (DotRecast) - must run on the shard
    ///     thread. No-op when the renderer is disabled, the navmesh is not loaded, or the mesh is already
    ///     built (it is a one-shot and is never re-extracted).
    /// </summary>
    /// <param name="nav">The navmesh service whose walkable surface triangles are extracted; a null value is ignored.</param>
    public void DebugRenderExtractNavMesh(NavMeshService nav)
    {
        if (!_debugRendererEnabled || _debugShapesExtractor == null || _debugNavMeshMesh != null)
        {
            return;
        }

        if (nav == null || !nav.TryExtractTriangles(out var triangles, out var center, out var radius))
        {
            return;
        }

        var vertexCount = triangles.Count; // flat: 3 Vector3 per triangle

        // A stable, collision-free id for the single navmesh mesh: fold in the vertex count so a different
        // zone maps to a distinct ring region, and use a fixed seed so it stays well clear of the
        // data-hash-based ids the statics
        const ulong seed = 0x5EAD0001;
        var id = seed ^ (ulong)vertexCount;

        var instance = new MeshInstance
        {
            Position = center,
            Scale = Vector3.One,
            PackedOrientation = DemoRenderer.Helpers.PackOrientationU64(Quaternion.Identity),
            PackedColor = DemoRenderer.Helpers.PackColor(new Vector3(0.2f, 0.85f, 1f)),
            BoundingRadius = radius,
        };

        bool instanceValid;
        Buffer<Vector4> vertices;
        lock (_debugShapesExtractor.MeshCache)
        {
            bool needsUpload;
            instanceValid = _debugShapesExtractor.MeshCache.Allocate(id, vertexCount, out instance.VertexStart, out vertices, out needsUpload);

            // Only a fresh allocation (needsUpload) has vertex data to write. The navmesh is a one-shot
            // (never reclaimed), so it is written exactly once, here.
            //
            // The mesh vertex shader transforms each stored vertex as
            //   worldPos = Position + Orientation * (Scale * storedVertex),
            // i.e. stored vertices are LOCAL to the instance's Position (see RenderMeshes.glvs). The
            // extraction returns world-space positions, so subtract the AABB center (used as Position)
            // to make them local. Storing world coords here AND setting Position would double the offset
            // and float the whole navmesh up by half its bounding box. The triangle winding is left as
            // extracted; the navmesh draw pass disables culling, so it is irrelevant.
            if (instanceValid && needsUpload)
            {
                for (int i = 0; i < vertexCount; ++i)
                {
                    vertices[i] = new Vector4(triangles[i] - center, 1f);
                }
            }
        }

        if (!instanceValid)
        {
            _debugRenderLogger.Warning("Navmesh debug mesh skipped: vertex cache full ({Vertices} vertices)", vertexCount);
            return;
        }

        instance.VertexCount = vertexCount;
        _debugNavMeshMesh = new DebugNavMeshMesh { Instance = instance, Ready = true };
        _debugRenderLogger.Information("Navmesh debug mesh built: {Vertices} vertices, radius {Radius:0.0}m", vertexCount, radius);
    }

    public void DebugRenderDispose()
    {
        if (!_debugRendererEnabled)
        {
            return;
        }

        _debugRendererEnabled = false;
        _debugLineExtractor?.Dispose();
        _debugLineExtractor = null;
        _debugFrameBuilder?.Dispose();
        _debugFrameBuilder = null;
        _debugShapesExtractor?.Dispose();
        _debugShapesExtractor = null;
        _debugBodyFrames = null;
        _debugStaticFrames = null;
        _debugStaticFrameA?.Dispose(BufferPool);
        _debugStaticFrameB?.Dispose(BufferPool);
        _debugStaticFrameA = null;
        _debugStaticFrameB = null;
        _debugStaticIndex = null;
        _debugCameraState = null;
        _debugShowFlagsState = null;

        // The navmesh mesh lives in the (shared) vertex ring owned by the shapes extractor, which is
        // disposed above; drop our reference so nothing dangles. The ring region itself is freed with
        // the extractor's cache.
        _debugNavMeshMesh = null;
    }

    partial void InitDebugRenderer()
    {
        _debugRendererEnabled = true;
        _debugRendererShowConstraints = _settings.PhysicsDebugRendererShowConstraints;
        _debugRendererShowContacts = _settings.PhysicsDebugRendererShowContacts;
        _debugRendererShowBoundingBoxes = _settings.PhysicsDebugRendererShowBoundingBoxes;
        _debugRendererFrameRate = _settings.PhysicsDebugRendererFrameRate;
        _debugBodyFrames = new DoubleBufferedFrame<PhysicsDebugFrame>();
        _debugFrameBuilder = new FrameBuilder(BufferPool, _debugBodyFrames);
        _debugStaticFrames = new DoubleBufferedStaticFrame();
        _debugLineExtractor = new LineExtractor(BufferPool, new ParallelLooper())
        {
            ShowConstraints = _debugRendererShowConstraints,
            ShowContacts = _debugRendererShowContacts,
            ShowBoundingBoxes = _debugRendererShowBoundingBoxes,
        };
        _debugShapesExtractor = new ShapesExtractor(new ParallelLooper(), BufferPool);
        _debugCameraState = new DebugCameraState();
        _debugShowFlagsState = new DebugShowFlagsState();
        _debugShowFlagsState.SetInitial(
            _debugRendererShowConstraints,
            _debugRendererShowContacts,
            _debugRendererShowBoundingBoxes);
        _debugRenderLogger.Information("Physics debug renderer enabled (frame rate {FrameRate})", _debugRendererFrameRate);
    }
}

/// <summary>
///     The one-shot navmesh overlay mesh: a single render-only <see cref="MeshInstance" /> (the zone's
///     walkable surface) plus a readiness flag. It is NOT a Bepu shape and never participates in the
///     simulation. The debug window reads <see cref="Instance" /> (a PhysicsDebugRenderer type) while its
///     navmesh toggle is on; the host supplies it via a delegate so the renderer project stays free of
///     GameServer types.
/// </summary>
public sealed class DebugNavMeshMesh
{
    public MeshInstance Instance;
    public bool Ready;
}
