using System;
using System.Net;
using System.Threading.Tasks;

namespace Shared.Udp;

public interface IPacketSender
{
    Task<bool> SendAsync(Memory<byte> packet, IPEndPoint endPoint);
}