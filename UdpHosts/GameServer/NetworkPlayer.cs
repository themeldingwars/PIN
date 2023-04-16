using GameServer.Data;
using GameServer.Packets.GSS;
using GameServer.Packets.GSS.Character.BaseController.PartialUpdates;
using GameServer.Packets.GSS.Character.CombatController;
using GameServer.Packets.GSS.Character.ObserverView;
using GameServer.Packets.Matrix;
using GameServer.Test;
using System.Net;
using System.Numerics;
using System.Threading;
using Character = GameServer.Entities.Character.Character;
using EnterZone = AeroMessages.Matrix.V25.EnterZone;

namespace GameServer;

public class NetworkPlayer : NetworkClient, INetworkPlayer
{
    private double lastKeyFrame;

    public NetworkPlayer(IPEndPoint endPoint, uint socketId)
        : base(endPoint, socketId)
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

    public void Login(ulong characterId)
    {
        CharacterId = characterId;
        CharacterEntity = new Character(AssignedShard, characterId & 0xffffffffffffff00);
        CharacterEntity.Load(characterId);
        Status = IPlayer.PlayerStatus.LoggedIn;

        // WelcomeToTheMatrix
        var welcomeToTheMatrix = new WelcomeToTheMatrix { InstanceID = AssignedShard.InstanceId };
        NetChannels[ChannelType.Matrix].SendClass(welcomeToTheMatrix);

        var zone = (uint)(characterId & 0x000000000000ffff);

        Program.Logger.Verbose("Zone {0}", zone);

        EnterZone(DataUtils.GetZone(zone));
    }

    public void Respawn()
    {
        var pointOfInterestPosition = DataUtils.GetZone(CurrentZone.ID).POIs["spawn"];
        CharacterEntity.Position = pointOfInterestPosition;

        var forcedMove = new ForcedMovement
                         {
                             Type = 1,
                             Unknown1 = 0,
                             Position = pointOfInterestPosition,
                             AimDirection = CharacterEntity.AimDirection,
                             Velocity = Vector3.Zero,
                             GameTime = AssignedShard.CurrentTime,
                             KeyFrame = AssignedShard.CurrentShortTime
                         };

        NetChannels[ChannelType.ReliableGss].SendGSSClass(forcedMove, CharacterEntity.EntityId);

        var respawnMessage = new Respawned { LastUpdateTime = AssignedShard.CurrentTime - 2, Unknown1 = 0x0100 };

        NetChannels[ChannelType.ReliableGss].SendGSSClass(respawnMessage, CharacterEntity.EntityId);

        var update = new PartialUpdate();

        update.Add(new MovementView
                   {
                       LastUpdateTime = AssignedShard.CurrentTime,
                       Position = CharacterEntity.Position,
                       Rotation = CharacterEntity.Rotation,
                       AimDirection = CharacterEntity.AimDirection,
                       Velocity = CharacterEntity.Velocity,
                       State = (ushort)CharacterEntity.MovementStateContainer.MovementState,
                       Unknown1 = 0,
                       Jets = (ushort)CharacterEntity.CharData.JumpJetEnergy,
                       AirGroundTimer = 0x3a7f,
                       JumpTimer = 0x3cdb,
                       Unknown2 = 0
                   });

        update.Add(new CharacterState { State = 3, LastUpdateTime = AssignedShard.CurrentTime - 1 });

        update.Add(new Unknown1());

        update.Add(new UnknownUpdate2 { Unknown1 = 0x0100, Unknown2 = 0x0100, LastUpdateTime = AssignedShard.CurrentTime });

        NetChannels[ChannelType.ReliableGss].SendGSSClass(update, CharacterEntity.EntityId, Enums.GSS.Controllers.Character_BaseController);

        update = new PartialUpdate();

        update.Add(new Unknown3 { Unknown1 = 0, LastUpdateTime = AssignedShard.CurrentTime });

        update.Add(new CharacterState { State = 6, LastUpdateTime = AssignedShard.CurrentTime });

        update.Add(new CurrentHealth { Value = CharacterEntity.CharData.MaxHealth });

        update.Add(new Unknown4 { Unknown1 = 0 });

        update.Add(new Unknown1());

        update.Add(new RegionUnlocks { Bitfield = 0xFFFFFFFFFFFFFFFFUL });

        NetChannels[ChannelType.ReliableGss].SendGSSClass(update, CharacterEntity.EntityId, Enums.GSS.Controllers.Character_BaseController);

        update = new PartialUpdate();

        update.Add(new Unknown5 { LastUpdateTime = AssignedShard.CurrentTime });

        NetChannels[ChannelType.ReliableGss].SendGSSClass(update, CharacterEntity.EntityId, Enums.GSS.Controllers.Character_BaseController);
    }

    public void Ready()
    {
        Status = IPlayer.PlayerStatus.Playing;
        CharacterEntity.Alive = true;
    }

    public void Jump()
    {
        //NetChannels[ChannelType.UnreliableGss].SendGSSClass( new JumpActioned { JumpTime = AssignedShard.CurrentShortTime }, CharacterEntity.EntityID, Enums.GSS.Controllers.Character_BaseController );
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
                //NetChannels[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(this, this), this.InstanceID, msgEnumType: typeof(Enums.GSS.Character.Events));
            }
        }
    }

    public void EnterZone(Zone z)
    {
        CurrentZone = z;
        CharacterEntity.Position = CurrentZone.POIs["spawn"];
        
        var msg = new EnterZone
           {
               InstanceId = AssignedShard.InstanceId,
               ZoneId = CurrentZone.ID,
               ZoneTimestamp = CurrentZone.Timestamp,
               PreviewModeFlag = 0,
               ZoneOwner = "r5_exec",
               Unk1_1 = 0x4c5f,
               Unk1_2 = 0x0c9f5,
               HotfixLevel = 0,
               MatchId = 0,
               Unk2 = 0,
               Unk3_Millis = 0x63e2db5e,
               ZoneName = CurrentZone.Name,
               HaveDevZoneInfo = 0,
               ZoneTimeSyncInfo = new AeroMessages.Matrix.V25.ZoneTimeSyncData {
                    FictionDateTimeOffsetMicros = 0,
                    DayLengthFactor = 12.0F,
                    DayPhaseOffset = 0.896445870399F,
               },
               GameClockInfo = new AeroMessages.Matrix.V25.GameClockInfoData {
                    MicroUnix_1 = 1478970208392232,
                    MicroUnix_2 = 1478774752697322,
                    Timescale = 1.0,
                    Unk3 = 0,
                    Unk4 = 0,
                    Paused = 0,
               },
               SpectatorModeFlag = 0,
           };

        NetChannels[ChannelType.Matrix].SendIAero(msg);
        
        Status = IPlayer.PlayerStatus.Loading;
        lastKeyFrame = AssignedShard.CurrentTime;
    }
}