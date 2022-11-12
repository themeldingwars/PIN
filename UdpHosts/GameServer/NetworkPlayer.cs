﻿using GameServer.Data;
using GameServer.Packets.GSS;
using GameServer.Packets.GSS.Character.BaseController.PartialUpdates;
using GameServer.Packets.GSS.Character.CombatController;
using GameServer.Packets.GSS.Character.ObserverView;
using GameServer.Packets.Matrix;
using GameServer.Test;
using System.Net;
using System.Numerics;
using System.Threading;
using Character = GameServer.Entities.Character;

namespace GameServer
{
    public class NetworkPlayer : NetworkClient, INetworkPlayer
    {
        private double lastKeyFrame;

        public NetworkPlayer(IPEndPoint ep, uint socketId)
            : base(ep, socketId)
        {
            CharacterEntity = null;
            Status = IPlayer.PlayerStatus.Connecting;
        }

        public ulong CharacterId { get; protected set; }
        public Character CharacterEntity { get; protected set; }
        public IPlayer.PlayerStatus Status { get; protected set; }
        public Zone CurrentZone { get; protected set; }
        public uint LastRequestedUpdate { get; set; }
        public uint RequestedClientTime { get; set; }
        public bool FirstUpdateRequested { get; set; }
        public ulong SteamUserId { get; set; }

        public void Init(IShard shard)
        {
            Init(this, shard, shard);
        }

        public void Login(ulong charId)
        {
            CharacterId = charId;
            CharacterEntity = new Character(AssignedShard, charId & 0xffffffffffffff00);
            CharacterEntity.Load(charId);
            Status = IPlayer.PlayerStatus.LoggedIn;

            // WelcomeToTheMatrix
            var wel = new WelcomeToTheMatrix { InstanceID = AssignedShard.InstanceID };
            NetChans[ChannelType.Matrix].SendClass(wel);

            var zone = (uint)(charId & 0x000000000000ffff);

            Program.Logger.Verbose("Zone {0}", zone);

            EnterZone(DataUtils.GetZone(zone));
        }

        public void Respawn()
        {
            var p = DataUtils.GetZone(CurrentZone.ID).POIs["spawn"];
            CharacterEntity.Position = p;

            var forcedMove = new ForcedMovement
                             {
                                 Type = 1,
                                 Unk1 = 0,
                                 Position = p,
                                 AimDirection = CharacterEntity.AimDirection,
                                 Velocity = Vector3.Zero,
                                 GameTime = AssignedShard.CurrentTime,
                                 KeyFrame = AssignedShard.CurrentShortTime
                             };

            NetChans[ChannelType.ReliableGss].SendGSSClass(forcedMove, CharacterEntity.EntityID);

            var respawnMsg = new Respawned { LastUpdateTime = AssignedShard.CurrentTime - 2, Unk1 = 0x0100 };

            NetChans[ChannelType.ReliableGss].SendGSSClass(respawnMsg, CharacterEntity.EntityID);

            var upd = new PartialUpdate();

            upd.Add(new MovementView
                    {
                        LastUpdateTime = AssignedShard.CurrentTime,
                        Position = CharacterEntity.Position,
                        Rotation = CharacterEntity.Rotation,
                        AimDirection = CharacterEntity.AimDirection,
                        Velocity = CharacterEntity.Velocity,
                        State = (ushort)CharacterEntity.MovementState,
                        Unk1 = 0,
                        Jets = (ushort)CharacterEntity.CharData.JumpJetEnergy,
                        AirGroundTimer = 0x3a7f,
                        JumpTimer = 0x3cdb,
                        Unk2 = 0
                    });

            upd.Add(new CharacterState { State = 3, LastUpdateTime = AssignedShard.CurrentTime - 1 });

            upd.Add(new Unknown1());

            upd.Add(new Unknown2 { Unk1 = 0x0100, Unk2 = 0x0100, LastUpdateTime = AssignedShard.CurrentTime });

            NetChans[ChannelType.ReliableGss].SendGSSClass(upd, CharacterEntity.EntityID, Enums.GSS.Controllers.Character_BaseController);

            upd = new PartialUpdate();

            upd.Add(new Unknown3 { Unk1 = 0, LastUpdateTime = AssignedShard.CurrentTime });

            upd.Add(new CharacterState { State = 6, LastUpdateTime = AssignedShard.CurrentTime });

            upd.Add(new CurrentHealth { Value = CharacterEntity.CharData.MaxHealth });

            upd.Add(new Unknown4 { Unk1 = 0 });

            upd.Add(new Unknown1());

            upd.Add(new RegionUnlocks { Bitfield = 0xFFFFFFFFFFFFFFFFUL });

            NetChans[ChannelType.ReliableGss].SendGSSClass(upd, CharacterEntity.EntityID, Enums.GSS.Controllers.Character_BaseController);

            upd = new PartialUpdate();

            upd.Add(new Unknown5 { LastUpdateTime = AssignedShard.CurrentTime });

            NetChans[ChannelType.ReliableGss].SendGSSClass(upd, CharacterEntity.EntityID, Enums.GSS.Controllers.Character_BaseController);
        }

        public void Ready()
        {
            Status = IPlayer.PlayerStatus.Playing;
            CharacterEntity.Alive = true;
        }

        public void Jump()
        {
            //NetChans[ChannelType.UnreliableGss].SendGSSClass( new JumpActioned { JumpTime = AssignedShard.CurrentShortTime }, CharacterEntity.EntityID, Enums.GSS.Controllers.Character_BaseController );
            CharacterEntity.TimeSinceLastJump = AssignedShard.CurrentShortTime;
        }

        public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
        {
            // TODO: Implement FSM here to move player thru log in process to connecting to a shard to playing
            if (Status == IPlayer.PlayerStatus.Connected)
            {
                Status = IPlayer.PlayerStatus.LoggingIn;
            }
            else if (Status == IPlayer.PlayerStatus.Loading)
            {
            }
            else if (Status == IPlayer.PlayerStatus.Playing)
            {
                if (AssignedShard.CurrentTime - lastKeyFrame > 0.5)
                {
                    //NetChans[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(this, this), this.InstanceID, msgEnumType: typeof(Enums.GSS.Character.Events));
                }
            }
        }

        public void EnterZone(Zone z)
        {
            CurrentZone = z;
            CharacterEntity.Position = CurrentZone.POIs["spawn"];

            // EnterZone
            var enterZone = new EnterZone();
            enterZone.InstanceId = AssignedShard.InstanceID;
            enterZone.ZoneId = CurrentZone.ID;
            enterZone.ZoneTimestamp = CurrentZone.Timestamp;
            enterZone.PreviewModeFlag = 0;
            enterZone.ZoneOwner = "r5_exec";
            enterZone.Unk1 = new byte[] { 0x5F, 0x4C, 0xF5, 0xC9, 0x01, 0x00 };
            enterZone.HotfixLevel = 0;
            enterZone.MatchId = 0;
            enterZone.Unk2 = new byte[] { 0x00, 0x5e, 0xdb, 0xe2, 0x63 };
            enterZone.ZoneName = CurrentZone.Name;
            enterZone.Unk3 = 0;
            enterZone.Unk_ZoneTime = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x41, 0x7A, 0x7D, 0x65, 0x3F };
            enterZone.Unk4 = 0x0005411D95E79428u;
            enterZone.Unk5 = 0x000540F013D65BEAu;
            enterZone.Unk6 = 0x3FF0000000000000u;
            enterZone.Unk7 = 0x0000000000000000u;
            enterZone.Unk8 = 0x0000000000000000u;
            enterZone.Unk9 = 0;
            enterZone.SpectatorModeFlag = 0;

            NetChans[ChannelType.Matrix].SendClass(enterZone);
            Status = IPlayer.PlayerStatus.Loading;

            lastKeyFrame = AssignedShard.CurrentTime;
        }
    }
}