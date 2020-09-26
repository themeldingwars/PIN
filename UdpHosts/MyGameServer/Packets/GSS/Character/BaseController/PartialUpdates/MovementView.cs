using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets.Common;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.BaseController.PartialUpdates {
    [PartialUpdate.PartialShadowField(0x0c)]
    public class MovementView {
        [Field]
        public uint LastUpdateTime;
        [Field]
        public Vector Position;
        [Field]
        public Quaternion Rotation;
        [Field]
        public Vector AimDirection;
        [Field]
        public Vector Velocity;
        [Field]
        public ushort State;
        [Field]
        public ushort Unk1;
        [Field]
        public ushort Jets;
        [Field]
        public ushort AirGroundTimer;
        [Field]
        public ushort JumpTimer;
        [Field]
        public byte Unk2;
    }
}
