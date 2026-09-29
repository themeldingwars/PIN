# Pirate Intelligence Network - Documention

PIN runs the UDP side of Firefall, see [UdpHosts](#udphosts).
The HTTP side lives in [RIN.WebAPI](https://github.com/themeldingwars/RIN.WebAPI), see [Web API](#web-api).

## UdpHosts

As the name suggests these are the server hosts that talk via UDP to the client.
They are further more split into two servers:
- [MatrixServer](#matrixserver)
- [GameServer](#gameserver)

### MatrixServer

The MatrixServer handles the whole initial connection of a new client and tells it where to find the GameServer to connect to.
The handshake protocol is quite simple and as such the server setup is also rather basic.

![](MatrixServer.png)

### GameServer

The GameServer is really the heart of the whole operation, which handles player connections, active shaders and receives and sends packets in the Game Server Socket (GSS) protocol.

The GameServer runs three different threads:
- ListenThread
- ServerRunThread
- SendThread

Packets are fetch from the network socket in the `ListenThread` and pushed to a `BufferBlock` of incoming packets.
The `ServerRunThread` is responsible to picking the latest packet on the `BufferBlock` and start handling it.
The `SendThread` reads from a separate `BufferBlock` of outgoing packets and pushes them onto the network socket.

A GameServer hosts exactly one `Shard`. The `Shard` has its own `RunThread` that triggers a timed network tick, which then causes the queued packets to be sent and pending packets to be proccessed. Each connecting client is migrated into the `Shard`.

The GSS protocol has four different channels:
- Control - Connection and time sync handling
- Matrix - Shard / zone related events and commands
- Reliable GSS - Message delivery is ensured with resend functionality
- Unreliable GSS - Message delivery is not guaranteed

The `NetworkClient` (`NetworkPlayer` as concrete implementation) subscribes via delegate onto the different channels and during the `Shard` network tick, the channel processing triggers these delegates.

![](GameServer.png)

### References

- [Game Server Protocol Overview](https://github.com/themeldingwars/Documentation/wiki/Game-Server-Protocol-Overview)
- [AeroMessages](https://github.com/themeldingwars/AeroMessages)
