using GameServer.Enums.GSS.Character;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GameServer.Packets.GSS;

[GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Events.ConfirmedPoseUpdate)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ConfirmedPoseUpdate
{
    public ushort Key;
    public ushort Type;
    private float vel_x;
    private float vel_y;
    private float vel_z;
    public ushort Unk1;
    public uint Unk2;
    public byte Unk3;
    public ushort KeyTime;

    public Vector3 Velocity
    {
        get => new(vel_x, vel_y, vel_z);
        set
        {
            vel_x = value.X;
            vel_y = value.Y;
            vel_z = value.Z;
        }
    }
}