using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Vehicle;
using AeroMessages.GSS.V66.Vehicle.Command;
using AeroMessages.GSS.V66.Vehicle.Controller;
using GameServer.Enums.GSS.Vehicle;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using System.Numerics;

namespace GameServer.Controllers.Vehicle;

[ControllerID(Enums.GSS.Controllers.Vehicle_BaseController)]
public class BaseController : Base
{
    private ILogger _logger;

    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.MovementInput)]
    public void MovementInput(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var movementInput = packet.Unpack<MovementInput>();
        var movementUpdate = new AeroMessages.GSS.V66.Vehicle.Controller.BaseController
        {
            /*VehicleIdProp = 0,
            ConfigurationProp = new ConfigurationData { Data = null },
            FlagsProp = null,
            EngineStateProp = 0,
            PathStateProp = 0,
            OwnerIdProp = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OwnerNameProp = null,
            OwnerLocalStringProp = 0,
            OccupantIds_0Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_1Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_2Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_3Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_4Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_5Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            DeployableIds_0Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_1Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_2Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_3Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_4Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_5Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_6Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_7Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_8Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            DeployableIds_9Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 0 },
            SnapMountProp = 0,
            SpawnPoseProp = new SpawnPoseData
            {
                Position = new Vector3(0, 0, 0),
                Rotation = new Quaternion(0, 0, 0, 0),
                Direction = new Vector3(0, 0, 0),
                Time = 0
            },
            SpawnVelocityProp = new Vector3(0, 0, 0),
            CurrentPoseProp = new CurrentPoseData
            {
                Position = new Vector3(0, 0, 0),
                Rotation = new Quaternion(0, 0, 0, 0),
                Direction = new Vector3(0, 0, 0),
                State = 0,
                Time = 0
            },
            ProcessDelayProp = new ProcessDelayData { Unk1 = 0, Unk2 = 0 },
            HostilityInfoProp = new HostilityInfoData { Flags = 0, FactionId = 0, TeamId = 0, Unk2 = 0, Unk3 = 0, Unk4 = 0 },
            PersonalFactionStanceProp = null,
            CurrentHealthProp = 60673,
            MaxHealthProp = 0,
            CurrentShieldsProp = 0,
            MaxShieldsProp = 0,
            CurrentResourcesProp = 0,
            MaxResourcesProp = 0,
            WaterLevelAndDescProp = 0,
            SinFlagsProp = 0,
            SinFlagsPrivateProp = 0,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
            SinCardTypeProp = 0,
            SinCardFields_0Prop = null,
            SinCardFields_1Prop = null,
            SinCardFields_2Prop = null,
            SinCardFields_3Prop = null,
            SinCardFields_4Prop = null,
            SinCardFields_5Prop = null,
            SinCardFields_6Prop = null,
            SinCardFields_7Prop = null,
            SinCardFields_8Prop = null,
            SinCardFields_9Prop = null,
            SinCardFields_10Prop = null,
            SinCardFields_11Prop = null,
            SinCardFields_12Prop = null,
            SinCardFields_13Prop = null,
            SinCardFields_14Prop = null,
            SinCardFields_15Prop = null,
            SinCardFields_16Prop = null,
            SinCardFields_17Prop = null,
            SinCardFields_18Prop = null,
            SinCardFields_19Prop = null,
            SinCardFields_20Prop = null,
            SinCardFields_21Prop = null,
            SinCardFields_22Prop = null,
            ScopeBubbleInfoProp = new ScopeBubbleInfoData { Unk1 = 0, Unk2 = 0 },
            ScalingLevelProp = 0*/

            CurrentHealthProp = 60673
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAeroChanges(movementUpdate, entityId);
    }

    [MessageID((byte)Commands.SetWaterLevelAndDesc)]
    public void SetWaterLevelAndDesc(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID((byte)Commands.SetEffectsFlag)]
    public void SetEffectsFlag(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }
}
