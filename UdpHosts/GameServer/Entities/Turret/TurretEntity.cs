using System;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Turret.Controller;
using AeroMessages.GSS.V66.Turret.View;

namespace GameServer.Entities.Turret;

public class TurretEntity : BaseEntity
{
    public TurretEntity(IShard shard, ulong eid, uint type, BaseEntity parent, byte parentChildIndex, byte posture)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Turret };
        Scoping = parent.Scoping;
        Type = type;
        Parent = parent;
        ParentChildIndex = parentChildIndex;
        Posture = posture;
        Position = parent.Position;
        InitControllers();
        InitViews();
    }

    public BaseController Turret_BaseController { get; set; }
    public ObserverView Turret_ObserverView { get; set; }

    public INetworkPlayer ControllingPlayer { get; set; }
    public bool IsPlayerControlled => ControllingPlayer != null;

    public uint Type { get; set; }
    public BaseEntity Parent { get; set; }
    public byte ParentChildIndex { get; set; }
    public byte Posture { get; set; }

    public void SetControllingPlayer(INetworkPlayer player)
    {
        if (player == null)
        {
            Turret_ObserverView.GunnerIdProp = new EntityId() { Backing = 0 };

            if (ControllingPlayer != null)
            {
                ControllingPlayer.AssignedShard.EntityMan.RemoveControllers(ControllingPlayer, this);
                ControllingPlayer = null;
            }
        }
        else
        {
            var character = player.CharacterEntity;

            character.SetAttachedTo(new AttachedToData
                {
                    Id1 = AeroEntityId,
                    Id2 = Parent.AeroEntityId,
                    Role = AttachedToData.AttachmentRoleType.Turret,
                    Unk2 = Posture,
                    Unk3 = 0,
                }, this);

            if (character.IsPlayerControlled)
            {
                ControllingPlayer = player;
                InitControllers();
                InitViews();
                Shard.EntityMan.ScopeIn(character.Player, this);
            }
        }
    }

    public void SetFireBurst(uint time)
    {
        Turret_ObserverView.WeaponBurstFiredProp = time;
    }

    public void SetFireEnd(uint time)
    {
        Turret_ObserverView.WeaponBurstEndedProp = time;
    }

    private void InitControllers()
    {
        Turret_BaseController = new BaseController()
            {
                TypeProp = Type,
                ParentObjIdProp = Parent.AeroEntityId,
                ParentChildIndexProp = ParentChildIndex,
                GunnerIdProp = ControllingPlayer?.CharacterEntity?.AeroEntityId ?? new EntityId() { Backing = 0 },
                SpawnPoseProp = new SpawnPoseData() { Rotation = Quaternion.Identity, Time = Shard.CurrentTime },
                ProcessDelayProp = new ProcessDelayData() { Unk1 = 30721, Unk2 = 236 },
                WeaponFireBaseTimeProp = new WeaponFireBaseTimeData() { ChangeTime = 0, Unk = 0 },
                AmmoProp = new AmmoData() { Ammo = new ushort[] { } },
                FireRateModifierProp = 1.0f,
                HostilityInfoProp = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 },
                PersonalFactionStanceProp = null,
                ScalingLevelProp = 1,
            };
    }

    private void InitViews()
    {
        Turret_ObserverView = new ObserverView()
            {
                TypeProp = Type,
                ParentObjIdProp = Parent.AeroEntityId,
                ParentChildIndexProp = ParentChildIndex,
                GunnerIdProp = ControllingPlayer?.CharacterEntity?.AeroEntityId ?? new EntityId() { Backing = 0 },
                CurrentPoseProp = new CurrentPoseStruct() { Rotation = Quaternion.Identity, ShortTime = Shard.CurrentShortTime },
                ProcessDelayProp = new ProcessDelayData() { Unk1 = 30721, Unk2 = 236 },
                WeaponBurstFiredProp = Shard.CurrentTime,
                WeaponBurstEndedProp = Shard.CurrentTime,
                AmmoProp = new AmmoStruct() { AmmoIndex = new ushort[] { } },
                FireRateModifierProp = 1.0f,
                HostilityInfoProp = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 },
                PersonalFactionStanceProp = null,
            };
    }
}