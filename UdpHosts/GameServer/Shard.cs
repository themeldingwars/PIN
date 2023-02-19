using GameServer.Entities;
using Shared.Common;
using Shared.Udp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer;

public class Shard : IShard, IPacketSender
{
    public const double NetworkTickRate = 1.0 / 20.0;
    protected long startTime;
    protected double lastNetTick;
    private ushort LastEntityRefId;
    protected Thread runThread;

    public Shard(double gameTickRate, ulong instanceId, IPacketSender sender)
    {
        Clients = new ConcurrentDictionary<uint, INetworkPlayer>();
        Entities = new ConcurrentDictionary<ulong, IEntity>();
        Physics = new PhysicsEngine(gameTickRate);
        AI = new AIEngine();
        InstanceId = instanceId;
        Sender = sender;
        EntityRefMap = new ConcurrentDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>>();
        LastEntityRefId = 0;
    }

    public DateTime StartTime => DateTimeExtensions.Epoch.AddSeconds(startTime);
    public IDictionary<ulong, IEntity> Entities { get; protected set; }
    protected IPacketSender Sender { get; }

    public IDictionary<uint, INetworkPlayer> Clients { get; protected set; }
    public PhysicsEngine Physics { get; protected set; }
    public AIEngine AI { get; protected set; }
    public ulong InstanceId { get; }
    public ulong CurrentTimeLong { get; protected set; }
    public IDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }

    public void Run(CancellationToken ct)
    {
        runThread = Utils.RunThread(RunThread, ct);
    }

    public void Stop()
    {
        runThread.Abort();
    }

    public bool Tick(double deltaTime, ulong currTime, CancellationToken ct)
    {
        CurrentTimeLong = currTime;
        foreach (var c in Clients.Values)
        {
            c.Tick(deltaTime, currTime, ct);
        }

        AI.Tick(deltaTime, currTime, ct);
        Physics.Tick(deltaTime, currTime, ct);

        return true;
    }

    public void NetworkTick(double deltaTime, ulong currTime, CancellationToken ct)
    {
        // Handle timeout, reliable retransmission, normal rx/tx
        foreach (var c in Clients.Values)
        {
            c.NetworkTick(deltaTime, currTime, ct);
        }
    }

    public bool MigrateOut(INetworkPlayer player) { return false; }

    public bool MigrateIn(INetworkPlayer player)
    {
        if (Clients.ContainsKey(player.SocketId))
        {
            return true;
        }

        player.Init(this);

        Clients.Add(player.SocketId, player);
        //Entities.Add(player.CharacterEntity.EntityID, player.CharacterEntity);

        return true;
    }

    public async Task<bool> Send(Memory<byte> p, IPEndPoint ep)
    {
        return await Sender.Send(p, ep);
    }

    public ushort AssignNewRefId(IEntity entity, Enums.GSS.Controllers controller)
    {
        while (EntityRefMap.ContainsKey(unchecked(++LastEntityRefId)) || LastEntityRefId == 0 || LastEntityRefId == 0xffff)
        {
            ;
        }

        EntityRefMap.Add(LastEntityRefId, new Tuple<IEntity, Enums.GSS.Controllers>(entity, controller));

        return unchecked(LastEntityRefId++);
    }

    public void RunThread(CancellationToken ct)
    {
        startTime = (long)DateTime.Now.UnixTimestamp();
        lastNetTick = 0;

        var stopwatch = new Stopwatch();
        var lastTime = 0.0;
        ulong currentTime;
        double delta;

        stopwatch.Start();

        while (!ct.IsCancellationRequested)
        {
            var currentUnixTimestamp = (ulong)(DateTime.Now.UnixTimestamp() * 1000);
            currentTime = unchecked((ulong)stopwatch.Elapsed.TotalMilliseconds);
            delta = currentTime - lastTime;

            if (ShouldNetworkTick(currentTime - lastNetTick, currentUnixTimestamp))
            {
                NetworkTick(currentTime - lastNetTick, currentUnixTimestamp, ct);
                lastNetTick = currentTime;
            }

            if (!Tick(delta, currentUnixTimestamp, ct))
            {
                break;
            }

            lastTime = currentTime;
            _ = Thread.Yield();
        }

        stopwatch.Stop();
    }

    protected virtual bool ShouldNetworkTick(double deltaTime, ulong currentTime)
    {
        return deltaTime >= NetworkTickRate;
    }
}