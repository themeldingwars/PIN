using System.Numerics;

namespace GameServer.Test.GSS;

public static class ConfirmedPoseUpdate
{
    public static Packets.GSS.ConfirmedPoseUpdate GetPreSpawnConfirmedPoseUpdate()
    {
        var ret = new Packets.GSS.ConfirmedPoseUpdate();

        ret.Key = 0x2cda;
        ret.Type = 0;
        ret.Velocity = Vector3.Zero;
        ret.Unknown1 = 0;
        ret.Unknown2 = 0;
        ret.Unknown3 = 0;
        ret.KeyTime = 0x2d31;

        return ret;
    }
}