using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Turret.Controller;
using AeroMessages.GSS.Turret.View;
using GameServer.Entities.Character;

namespace GameServer.Entities.Turret;

public sealed class TurretEntity : BaseEntity
{
    /// <summary>
    ///     Status effect whose apply/remove chains run the TurretControl command, binding an NPC gunner to this turret.
    /// </summary>
    public const uint TurretControlEffectId = 1368;

    public TurretEntity(IShard shard, ulong eid, uint type, BaseEntity parent, byte parentChildIndex, byte posture, uint gunnerPoseId, Vector3 gunnerPoseOffset)
        : base(shard, eid)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.Turret };
        Scoping = parent.Scoping;
        Type = type;
        Parent = parent;
        ParentChildIndex = parentChildIndex;
        Posture = posture;
        GunnerPoseId = gunnerPoseId;
        GunnerPoseOffset = gunnerPoseOffset;

        // BaseEntity has the Position prop and other systems may wish to use that
        // when processing this entity, EntityManagers scoping in/out logic is one such system.
        // The turret rides on its deployable's transform: CurrentPose is a rotation in this frame, so the gun's
        // rest direction is the deployable's +Y axis.
        Position = parent.Position;
        Orientation = parent.Orientation;

        HostilityInfo = parent.HostilityInfo;
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
    public uint GunnerPoseId { get; set; }
    public Vector3 GunnerPoseOffset { get; set; }
    public CharacterEntity NpcGunner { get; private set; }

    public bool IsNpcControlled => NpcGunner != null;

    public void SetNpcGunner(CharacterEntity gunner)
    {
        NpcGunner = gunner;

        var gunnerId = gunner?.AeroEntityId ?? new EntityId() { Backing = 0 };
        Turret_ObserverView.GunnerIdProp = gunnerId;

        if (Turret_BaseController != null)
        {
            Turret_BaseController.GunnerIdProp = gunnerId;
        }
    }

    /// <summary>
    ///     Points the turret's gun. Mirrors what the manned turret controller does with its PoseUpdate, so an
    ///     NPC-driven turret animates on the client exactly like a manned one.
    /// </summary>
    /// <param name="rotation">The gun pose rotation to apply.</param>
    public void SetAimRotation(Quaternion rotation)
    {
        Turret_ObserverView.CurrentPoseProp = new CurrentPoseStruct()
        {
            Rotation = rotation,
            ShortTime = Shard.CurrentShortTime,
        };
    }

    public void SetAmmo(ushort[] ammo)
    {
        Turret_ObserverView.AmmoProp = new AmmoStruct() { AmmoIndex = ammo };

        if (Turret_BaseController != null)
        {
            Turret_BaseController.AmmoProp = new AmmoData() { Ammo = ammo };
        }
    }

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
                Posture = Posture,
                Unk3 = 0,
            },
            this,
            GunnerPoseId,
            GunnerPoseOffset);

            ControllingPlayer = player;
            InitControllers();
            InitViews();
            Shard.EntityMan.ScopeIn(character.Player, this);
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

    public void SetHostilityInfo(HostilityInfoData newValue)
    {
        HostilityInfo = newValue;
        Turret_ObserverView.HostilityInfoProp = HostilityInfo;

        if (Turret_BaseController != null)
        {
            Turret_BaseController.HostilityInfoProp = HostilityInfo;
        }
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
            ProcessDelayProp = new ProcessDelayData() { ChangeTime = 30721, DelayMs = 236 },
            WeaponFireBaseTimeProp = new WeaponFireBaseTimeData() { ChangeTime = 0, Unk = 0 },
            AmmoProp = new AmmoData() { Ammo = [] },
            FireRateModifierProp = 1.0f,
            HostilityInfoProp = HostilityInfo,
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
            ProcessDelayProp = new ProcessDelayData() { ChangeTime = 30721, DelayMs = 236 },
            WeaponBurstFiredProp = Shard.CurrentTime,
            WeaponBurstEndedProp = Shard.CurrentTime,
            AmmoProp = new AmmoStruct() { AmmoIndex = [] },
            FireRateModifierProp = 1.0f,
            HostilityInfoProp = HostilityInfo,
            PersonalFactionStanceProp = null,
        };
    }
}