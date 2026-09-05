using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using AeroMessages.GSS.Generic;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;
using Serilog;

namespace GameServer.Systems.WeaponSim;

public class WeaponSim
{
    private readonly Dictionary<ulong, WeaponPlayerSim> _weaponSimState;
    private readonly Shard _shard;
    private readonly ILogger _logger;
    private readonly ulong _updateIntervalMs = 50;
    private ulong _lastUpdate;

    public WeaponSim(Shard shard)
    {
        _shard = shard;
        _weaponSimState = [];
        _logger = shard.Logger.ForContext<WeaponSim>();
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > _lastUpdate + _updateIntervalMs)
        {
            _lastUpdate = currentTime;
            var entities = GetWeaponSimPlayersEntities();
            foreach (var entity in entities)
            {
                ProcessEntity(entity as CharacterEntity);
            }
        }
    }

    public void OnFireWeaponProjectile(CharacterEntity entity, uint time, Vector3 localAimDir, Vector3? shooterVelocity = null)
    {
        // Weapon
        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null || activeWeaponDetails.Weapon == null)
        {
            _logger.Warning("Will not fire projectile because failed to get active weapon from the entity");
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var spreadProfile = activeWeaponDetails.SpreadProfile;

        // Resolve weapon attributes: stats from weapon override template defaults
        var attrsDict = activeWeaponDetails.Attributes;
        float range = weapon.Range;
        if (attrsDict.TryGetValue((ushort)ItemAttributeId.WeaponRange, out var rangeAttr))
        {
            range = rangeAttr;
        }

        // Weapon Sim State
        var weaponSimState = GetOrCreateState(entity, activeWeaponDetails, time);

        // Ammo
        var ammo = SDBInterface.GetAmmo(weapon.AmmoId); // TODO: Handle ammo overrides

        // Ammo stat properties: stat ID from ammo points to weapon attribute to use
        float projectileSpeed = ammo.ProjectileSpeed;
        if (ammo.ProjectileSpeedStat != 0 && attrsDict.TryGetValue(ammo.ProjectileSpeedStat, out var speedAttr))
        {
            projectileSpeed = speedAttr;
        }

        float impactRadius = ammo.ImpactRadius;
        if (ammo.ImpactRadiusStat != 0 && attrsDict.TryGetValue(ammo.ImpactRadiusStat, out var impactAttr))
        {
            impactRadius = impactAttr;
        }

        float maxRadius = ammo.MaxRadius;
        if (ammo.MaxRadiusStat != 0 && attrsDict.TryGetValue(ammo.MaxRadiusStat, out var maxRadiusAttr))
        {
            maxRadius = maxRadiusAttr;
        }

        float damage = ResolveWeaponDamage(weapon, attrsDict);

        // Projectile origin at the fire time, interpolated/predicted from movement samples
        var origin = entity.GetProjectileOrigin(time, localAimDir, shooterVelocity);

        // Determine number of rounds to fire with this proj
        // If weapon has burst duration, we expect to receive multiple proj calls and only fire 1.
        byte roundsToFire = 1;
        if (weapon.MsBurstDuration == 0 && weapon.RoundsPerBurst > 1)
        {
            roundsToFire = weapon.RoundsPerBurst;
        }

        // Calculate spreadPct
        WeaponSpreadMath.UpdateMovementState(spreadProfile, weaponSimState, entity.MovementStateContainer.MovementStateValue, time);
        WeaponSpreadMath.RecoilInterpUpdate(spreadProfile, weaponSimState, time, snapshot: true);
        float spreadPct = WeaponSpreadMath.GetCurrentSpreadPct(spreadProfile, weaponSimState, time);

        // Fire rounds
        for (byte round = 0; round < roundsToFire; round++)
        {
            Vector3 aimForward = localAimDir; // entity.AimDirection;
            Vector3 aimRight = Vector3.Normalize(Vector3.Cross(aimForward, Vector3.UnitZ));
            Vector3 aimUp = Vector3.Normalize(Vector3.Cross(aimRight, aimForward));
            Vector3 lastSpreadDirection = weaponSimState.LastSpreadDirection;
            uint lastSpreadTime = weaponSimState.LastSpreadTime;
            PRNG.PRNG.Spread(time, weapon.SlotIndex, round, aimForward, aimRight, aimUp, spreadPct, lastSpreadDirection, lastSpreadTime, out Vector3 direction);
            uint trace = PRNG.PRNG.Trace(time, round);
            _shard.ProjectileSim.FireProjectile(entity, trace, origin, direction, ammo, range, projectileSpeed, impactRadius, maxRadius, damage);
            weaponSimState.LastSpreadDirection = direction;
            weaponSimState.LastSpreadTime = time;
        }

        // Add spread
        uint totalBurstRounds = (uint)(weapon.MsBurstDuration == 0 && weapon.RoundsPerBurst > 1 ? weapon.RoundsPerBurst : 1);
        WeaponSpreadMath.ApplyBurstSpreadUpdate(spreadProfile, weaponSimState, roundsToFire, totalBurstRounds);

        weaponSimState.LastBurstTime = time;
    }

    /// <summary>
    ///     Fires an ability projectile using the aim vector from the client's FireWeaponProjectile
    ///     TODO: Homing, aim-at-target and body/hardpoint origin offsets
    /// </summary>
    /// <param name="entity">Character that fires</param>
    /// <param name="pending">Ability projectile info</param>
    /// <param name="time">Time of firing</param>
    /// <param name="localAimDir">The aim direction at the time of firing</param>
    /// <param name="shooterVelocity">The shooter velocity at the time of firing</param>
    public void OnFireAbilityProjectile(CharacterEntity entity, PendingAbilityProjectile pending, uint time, Vector3 localAimDir, Vector3? shooterVelocity = null)
    {
        var ammo = SDBInterface.GetAmmo(pending.AmmoType);
        if (ammo == null)
        {
            _logger.Warning("Will not fire ability projectile because failed to get ammo {AmmoType} (exec {ExecId})", pending.AmmoType, pending.ExecutionId);
            return;
        }

        // Stub: spawn from the weapon muzzle origin (body/hardpoint offset not implemented yet).
        var origin = entity.GetProjectileOrigin(pending.QueueTime, localAimDir, shooterVelocity);

        // TODO: Should we really load from weapon?
        var attrsDict = entity.GetActiveWeaponDetails()?.Attributes;

        float projectileSpeed = ammo.ProjectileSpeed;
        if (ammo.ProjectileSpeedStat != 0 && attrsDict != null && attrsDict.TryGetValue(ammo.ProjectileSpeedStat, out var speedAttr))
        {
            projectileSpeed = speedAttr;
        }

        float impactRadius = ammo.ImpactRadius;
        if (ammo.ImpactRadiusStat != 0 && attrsDict != null && attrsDict.TryGetValue(ammo.ImpactRadiusStat, out var impactAttr))
        {
            impactRadius = impactAttr;
        }

        float maxRadius = ammo.MaxRadius;
        if (ammo.MaxRadiusStat != 0 && attrsDict != null && attrsDict.TryGetValue(ammo.MaxRadiusStat, out var maxRadiusAttr))
        {
            maxRadius = maxRadiusAttr;
        }

        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        float damage = pending.UseWeaponDamage && activeWeaponDetails != null && activeWeaponDetails.Weapon != null
            ? ResolveWeaponDamage(activeWeaponDetails.Weapon, attrsDict)
            : pending.Damage;

        byte roundsToFire = pending.BurstCount > 0 ? pending.BurstCount : (byte)1;
        Vector3 aimForward = Vector3.Normalize(localAimDir);
        Vector3 aimRight = Vector3.Normalize(Vector3.Cross(aimForward, Vector3.UnitZ));
        Vector3 aimUp = Vector3.Normalize(Vector3.Cross(aimRight, aimForward));

        for (byte round = 0; round < roundsToFire; round++)
        {
            PRNG.PRNG.Spread(time, 0, round, aimForward, aimRight, aimUp, pending.Spread, Vector3.Zero, 0, out Vector3 direction);
            uint trace = PRNG.PRNG.Trace(time, round);
            _shard.ProjectileSim.FireProjectile(entity, trace, origin, direction, ammo, pending.Range, projectileSpeed, impactRadius, maxRadius, damage, isAbilityProjectile: true);
        }

        _logger.Debug("Fired ability projectile ammo={AmmoType} range={Range} burst={Burst} entity={Entity} exec={ExecId}", pending.AmmoType, pending.Range, roundsToFire, entity.EntityId, pending.ExecutionId);
    }

    private static float ResolveWeaponDamage(WeaponTemplateResult weapon, IReadOnlyDictionary<ushort, float> attributes)
    {
        float damage = weapon.DamagePerRound;
        if (attributes != null && attributes.TryGetValue((ushort)ItemAttributeId.WeaponDamage, out var damageAttribute))
        {
            damage = damageAttribute;
        }

        return damage;
    }

    // Keeps one spread state per fire mode (mirrors the client's firstMode/secondMode), so each mode
    // carries its own accumulated spread/heat history. On a weapon or fire-mode change the newly
    // activated mode is re-seeded to match the client switch handler (see WeaponSpreadMath.SeedModeStateOnSwitch).
    private WeaponModeState GetOrCreateState(CharacterEntity entity, CharacterEntity.ActiveWeaponDetails activeWeaponDetails, uint time)
    {
        if (!_weaponSimState.TryGetValue(entity.EntityId, out var player))
        {
            player = new WeaponPlayerSim();
            _weaponSimState[entity.EntityId] = player;
        }

        uint weaponId = activeWeaponDetails.WeaponId;
        byte activeMode = entity.GetActiveFireModeIndex();

        bool weaponChanged = player.WeaponId != weaponId;
        if (weaponChanged)
        {
            player.WeaponId = weaponId;
            player.Modes[0] = new WeaponModeState { WeaponId = weaponId };
            player.Modes[1] = new WeaponModeState { WeaponId = weaponId };
        }

        bool modeChanged = player.ActiveMode != activeMode;
        if (weaponChanged || modeChanged)
        {
            byte otherMode = (byte)(1 - activeMode);
            var otherDetails = entity.GetWeaponDetails(otherMode);
            bool otherHasWeapon = otherDetails != null && otherDetails.Weapon != null;

            bool newHasScope = activeWeaponDetails.Weapon.ScopeId != 0;
            bool newLinked = (activeWeaponDetails.Weapon.WeaponFlags & 8) != 0;
            bool otherLinked = otherHasWeapon && (otherDetails.Weapon!.WeaponFlags & 8) != 0;

            WeaponSpreadMath.SeedModeStateOnSwitch(
                activeWeaponDetails.SpreadProfile,
                player.Modes[activeMode],
                otherHasWeapon ? otherDetails.SpreadProfile : default,
                otherHasWeapon ? player.Modes[otherMode] : null,
                newHasScope,
                newLinked,
                otherLinked,
                time);

            player.ActiveMode = activeMode;
        }

        return player.Modes[activeMode];
    }

    private void ProcessEntity(CharacterEntity entity)
    {
        ProcessWeaponSpread(entity);
        DebugWeaponSpread(entity);
    }

    private void ProcessWeaponSpread(CharacterEntity entity)
    {
        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null || activeWeaponDetails.Weapon == null)
        {
            return;
        }

        uint currentTime = _shard.CurrentTime;
        var weaponSimState = GetOrCreateState(entity, activeWeaponDetails, currentTime);

        WeaponSpreadMath.UpdateMovementState(activeWeaponDetails.SpreadProfile, weaponSimState, entity.MovementStateContainer.MovementStateValue, currentTime);
        WeaponSpreadMath.RecoilInterpUpdate(activeWeaponDetails.SpreadProfile, weaponSimState, currentTime, snapshot: false);
    }

    private void DebugWeaponSpread(CharacterEntity entity)
    {
        var client = entity.Player;

        if (client.Preferences.DebugWeapon == 0)
        {
            return;
        }

        if (!client.CanReceiveGSS)
        {
            return;
        }

        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null || activeWeaponDetails.Weapon == null)
        {
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var weaponId = activeWeaponDetails.WeaponId;
        uint time = _shard.CurrentTime;
        var weaponSimState = GetOrCreateState(entity, activeWeaponDetails, time);

        float spreadPct = WeaponSpreadMath.GetCurrentSpreadPct(activeWeaponDetails.SpreadProfile, weaponSimState, time);

        var ammo = SDBInterface.GetAmmo(weapon.AmmoId);
        var eventData = new DebugWeaponSimEventData()
        {
            WeaponName = weapon.DebugName,
            SpreadPct = spreadPct,
            WeaponId = weaponId,
            AccumulatedSpread = weaponSimState.AccumulatedSpread,
            SpreadHeat = weaponSimState.SpreadHeat,
            MovementStateValue = entity.MovementStateContainer.MovementStateValue,
            SpreadMovementState = weaponSimState.SpreadMovementState,
            AgilityFactor = weaponSimState.AgilityCurrent,
            BaseSpreadPct = activeWeaponDetails.SpreadProfile.BaseSpreadPct,
            OtherSpreadPct = activeWeaponDetails.SpreadProfile.OtherSpreadPct,
            Heat = weaponSimState.SpreadHeat,
            SpreadHeatAfterExponent = WeaponSpreadMath.ApplyRampExponent(weaponSimState.SpreadHeat, activeWeaponDetails.SpreadProfile.SpreadRampExponent),
            SpreadFactor = (WeaponSpreadMath.ApplyRampExponent(weaponSimState.SpreadHeat, activeWeaponDetails.SpreadProfile.SpreadRampExponent) * (1f - activeWeaponDetails.SpreadProfile.MinSpreadFrac)) + activeWeaponDetails.SpreadProfile.MinSpreadFrac,
            LastRecoilUpdate = weaponSimState.LastRecoilUpdate,
            AccumulatedWhenReturnStarted = weaponSimState.AccumulatedSpreadWhenReturnStarted,
            StartingSpread = activeWeaponDetails.SpreadProfile.StartingSpread,
            MinSpreadFrac = activeWeaponDetails.SpreadProfile.MinSpreadFrac,
            MsPerBurst = activeWeaponDetails.SpreadProfile.MsPerBurst,
            SpreadRampTime = activeWeaponDetails.SpreadProfile.SpreadRampTime,
            MsSpreadReturn = activeWeaponDetails.SpreadProfile.MsSpreadReturn,
            MsSpreadReturnDelay = activeWeaponDetails.SpreadProfile.MsSpreadReturnDelay,
            WeaponFlags = ((WeaponFlags)weapon.WeaponFlags).ToString(),
            AmmoFlags = ammo != null ? new AmmoFlags(ammo.Flags).ToString() : "[N/A]",
            AmmoName = ammo?.Name ?? "[N/A]",
            AmmoId = ammo?.Id ?? 0
        };

        try
        {
            var json = JsonSerializer.Serialize(eventData);

            var message = new TempConsoleMessage()
            {
                ConsoleNoticeMessage = string.Empty,
                ConsoleCommand = string.Empty,
                ChatNotification = string.Empty,
                DebugReportArgType = "WeaponSim.Spread",
                DebugReportArgData = json,
            };

            client.NetChannels[ChannelType.ReliableGss].SendMessage(message);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed DebugWeaponSpread");
        }
    }

    private IEnumerable<INetworkPlayer> GetWeaponSimPlayers()
    {
        return _shard.Clients.Values.Where((client) => client.CanReceiveGSS);
    }

    private IEnumerable<IEntity> GetWeaponSimPlayersEntities()
    {
        return _shard.Entities.Values.Where((entity) => entity is CharacterEntity character && character.IsPlayerControlled);
    }

    // Per-player weapon sim state: one spread state per fire mode (main/alt)
    private sealed class WeaponPlayerSim
    {
        public uint WeaponId;
        public byte ActiveMode = 255;
        public WeaponModeState[] Modes = new WeaponModeState[2];
    }

    private record class DebugWeaponSimEventData
    {
        public string WeaponName { get; set; }
        public float SpreadPct { get; set; }
        public uint WeaponId { get; set; }
        public float AccumulatedSpread { get; set; }
        public float SpreadHeat { get; set; }
        public ushort MovementStateValue { get; set; }
        public byte SpreadMovementState { get; set; }
        public float AgilityFactor { get; set; }
        public float BaseSpreadPct { get; set; }
        public float OtherSpreadPct { get; set; }
        public float Heat { get; set; }
        public float SpreadHeatAfterExponent { get; set; }
        public float SpreadFactor { get; set; }
        public uint LastRecoilUpdate { get; set; }
        public float AccumulatedWhenReturnStarted { get; set; }
        public float StartingSpread { get; set; }
        public float MinSpreadFrac { get; set; }
        public uint MsPerBurst { get; set; }
        public uint SpreadRampTime { get; set; }
        public uint MsSpreadReturn { get; set; }
        public uint MsSpreadReturnDelay { get; set; }
        public string WeaponFlags { get; set; }
        public string AmmoFlags { get; set; }
        public string AmmoName { get; set; }
        public uint AmmoId { get; set; }
    }
}