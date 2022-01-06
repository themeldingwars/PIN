using System.Numerics;

namespace GameServer.Test.GSS;

public static class ConfirmedPoseUpdate
{
    public static Packets.GSS.ConfirmedPoseUpdate GetPreSpawnConfirmedPoseUpdate()
    {
        var ret = new Packets.GSS.ConfirmedPoseUpdate
                  {
                      Key = 0x2cda,
                      Type = 0,
                      Velocity = Vector3.Zero,
                      Unk1 = 0,
                      Unk2 = 0,
                      Unk3 = 0,
                      KeyTime = 0x2d31
                  };

        return ret;
    }
}