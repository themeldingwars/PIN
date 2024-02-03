using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Aptitude;
using GameServer.Entities;
using Shared.Common;
using Shared.Udp;

namespace GameServer;

public class Shard : IShard
{
    private const double NetworkTickRate = 1.0 / 20.0;

    private long _startTime;
    private double _lastNetTick;
    private ushort _lastEntityRefId;

    public Shard(double gameTickRate, ulong instanceId, IPacketSender sender)
    {
        Clients = new ConcurrentDictionary<uint, INetworkPlayer>();
        Entities = new ConcurrentDictionary<ulong, IEntity>();
        Physics = new PhysicsEngine(gameTickRate);
        AI = new AIEngine();
        Movement = new MovementRelay(this);
        Abilities = new AbilitySystem(this);
        EntityMan = new EntityManager(this);
        InstanceId = instanceId;
        Sender = sender;
        EntityRefMap = new ConcurrentDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>>();
        _lastEntityRefId = 0;
    }

    public DateTime StartTime => DateTimeExtensions.Epoch.AddSeconds(_startTime);
    public IDictionary<ulong, IEntity> Entities { get; protected set; }
    public IDictionary<uint, INetworkPlayer> Clients { get; }
    public PhysicsEngine Physics { get; }
    public AIEngine AI { get; }
    public MovementRelay Movement { get; }
    public EntityManager EntityMan { get; }
    public AbilitySystem Abilities { get; }
    public ulong InstanceId { get; }
    public ulong CurrentTimeLong { get; private set; }
    public uint CurrentTime => unchecked((uint)CurrentTimeLong);
    public ushort CurrentShortTime => unchecked((ushort)CurrentTime);
    public IDictionary<ushort, Tuple<IEntity, Enums.GSS.Controllers>> EntityRefMap { get; }
    private IPacketSender Sender { get; }

    public void Run(CancellationToken ct)
    {
        Utils.RunThread(RunThread, ct);
    }

    public void NetworkTick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        // Handle timeout, reliable retransmission, normal rx/tx
        foreach (var client in Clients.Values)
        {
            client.NetworkTick(deltaTime, currentTime, ct);
        }
    }

    public bool Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        CurrentTimeLong = currentTime;

        AI.Tick(deltaTime, currentTime, ct);
        Physics.Tick(deltaTime, currentTime, ct);
        EntityMan.Tick(deltaTime, currentTime, ct);
        Abilities.Tick(deltaTime, currentTime, ct);

        return true;
    }

    public bool MigrateOut(INetworkPlayer player)
    {
        if (Clients.ContainsKey(player.SocketId))
        {
            Clients.Remove(player.SocketId);
            return true;
        }

        return false;
    }

    public bool MigrateIn(INetworkPlayer player)
    {
        if (Clients.ContainsKey(player.SocketId))
        {
            return true;
        }

        player.Init(this);

        Clients.Add(player.SocketId, player);

        return true;
    }

    public async Task<bool> SendAsync(Memory<byte> packet, IPEndPoint endPoint)
    {
        return await Sender.SendAsync(packet, endPoint);
    }

    public ushort AssignNewRefId(IEntity entity, Enums.GSS.Controllers controller)
    {
        while (EntityRefMap.ContainsKey(unchecked(++_lastEntityRefId)) || _lastEntityRefId is 0 or 0xffff)
        {
        }

        EntityRefMap.Add(_lastEntityRefId, new Tuple<IEntity, Enums.GSS.Controllers>(entity, controller));

        return unchecked(_lastEntityRefId++);
    }

    protected virtual bool ShouldNetworkTick(double deltaTime, ulong currentTime)
    {
        return deltaTime >= NetworkTickRate;
    }

    private void RunThread(CancellationToken ct)
    {
        _startTime = (long)DateTime.Now.UnixTimestamp();
        _lastNetTick = 0;

        var stopwatch = new Stopwatch();
        var lastTime = 0.0;

        stopwatch.Start();

        while (!ct.IsCancellationRequested)
        {
            // (ulong)(DateTime.Now.UnixTimestamp() * 1000);
            var currentUnixTimestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var currentTime = unchecked((ulong)stopwatch.Elapsed.TotalMilliseconds);
            var delta = currentTime - lastTime;

            if (ShouldNetworkTick(currentTime - _lastNetTick, currentUnixTimestamp))
            {
                NetworkTick(currentTime - _lastNetTick, currentUnixTimestamp, ct);
                _lastNetTick = currentTime;
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
}