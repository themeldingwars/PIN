using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using AeroMessages.GSS.V66.Generic;
using BepuUtilities;
using FauFau.Util;
using GameServer.Data.SDB;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;
using static GameServer.Entities.Character.CharacterEntity;

namespace GameServer;

public class WeaponSim
{
    private readonly Dictionary<ulong, WeaponSimState> _weaponSimState;
    private Shard _shard;
    private ulong LastUpdate = 0;
    private ulong UpdateIntervalMs = 50;

    public WeaponSim(Shard shard)
    {
        _shard = shard;
        _weaponSimState = new();
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > LastUpdate + UpdateIntervalMs)
        {
            LastUpdate = currentTime;
            var entities = GetWeaponSimPlayersEntities();
            foreach (var entity in entities)
            {
                ProcessEntity(entity as CharacterEntity);
            }
        }
    }

    public void OnFireWeaponProjectile(CharacterEntity entity, uint time, Vector3 localAimDir)
    {
        // Weapon
        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null)
        {
            Console.WriteLine($"Will not fire projectile because failed to get active weapon from the entity");
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var weaponId = activeWeaponDetails.WeaponId;
        var weaponSpreadFactor = activeWeaponDetails.Spread;
        var weaponAttributeRateOfFire = activeWeaponDetails.RateOfFire;

        // Weapon Sim State
        var weaponSimState = _weaponSimState.GetValueOrDefault(entity.EntityId, new WeaponSimState());
        if (weaponSimState.LastWeaponId == 0)
        {
            weaponSimState.LastWeaponId = weaponId;
        }
        else if (weaponSimState.LastWeaponId != weaponId)
        {
            // Console.WriteLine($"Reset {weaponId} sim state!");

            // Drop state if we swapped weapon   
            weaponSimState = new WeaponSimState
            {
                LastWeaponId = weaponId
            };
        }

        // Console.WriteLine($"Selected weapon {weapon.DebugName} and spread factor {weaponSpreadFactor}");

        // Ammo
        var ammo = SDBInterface.GetAmmo(weapon.AmmoId); // TODO: Handle ammo overrides
        
        // Projectile origin
        var origin = entity.GetProjectileOrigin(localAimDir);

        // Determine number of rounds to fire with this proj
        // If weapon has burst duration, we expect to receive multiple proj calls and only fire 1.
        byte roundsToFire = 1;
        if (weapon.MsBurstDuration == 0 && weapon.RoundsPerBurst > 1)
        {
            roundsToFire = weapon.RoundsPerBurst;
        }

        // Calculate spreadPct
        float spreadPct = GetCurrentSpreadPct(entity, weapon, weaponSimState, weaponSpreadFactor, time);
        // Console.WriteLine($"Firing with spreadPct {spreadPct}");

        // Fire rounds
        for (byte round = 0; round < roundsToFire; round++)
        {
            Vector3 aimForward = localAimDir; // entity.AimDirection;
            Vector3 aimRight = Vector3.Normalize(Vector3.Cross(aimForward, Vector3.UnitZ));
            Vector3 aimUp = Vector3.Normalize(Vector3.Cross(aimRight, aimForward));
            Vector3 lastSpreadDirection = weaponSimState.LastSpreadDirection;
            uint lastSpreadTime = weaponSimState.LastSpreadTime;
            PRNG.Spread(time, weapon.SlotIndex, round, aimForward, aimRight, aimUp, spreadPct, lastSpreadDirection, lastSpreadTime, out Vector3 direction);
            uint trace = PRNG.Trace(time, round);
            _shard.ProjectileSim.FireProjectile(entity, trace, origin, direction, ammo);
            weaponSimState.LastSpreadDirection = direction;
            weaponSimState.LastSpreadTime = time;
        }

        // Add spread
        if (weapon.SpreadRampTime != 0)
        {
            uint burstCost = weapon.MsPerBurst;
            var update = System.Math.Min(weapon.SpreadRampTime, weaponSimState.AccumulatedSpreadTime + burstCost);

            // Console.WriteLine($"SpreadRampTime {weapon.SpreadRampTime}, AccumulatedSpreadTime: {weaponSimState.AccumulatedSpreadTime}, MsPerBurst: {weapon.MsPerBurst}, Setting AccumulatedSpreadTime To : {update}");
            weaponSimState.AccumulatedSpreadTime = update;
        }

        weaponSimState.LastBurstTime = time;
        _weaponSimState[entity.EntityId] = weaponSimState;
    }

    private float GetCurrentSpreadPct(CharacterEntity entity, WeaponTemplateResult weapon, WeaponSimState weaponSimState, float weaponSpreadFactor, uint time)
    {
        // NOTE: Consider this whole thing a sham, needs further RE.
        float spreadValue = 0;

        // Start at max spread if spread should reduce when firing (HMG)
        bool inverse = weapon.SpreadPerBurst < 0;
        if (inverse)
        {
            spreadValue = weapon.MaxSpread;
        }

        // Ensure we are at least at minSpread
        if (spreadValue < weapon.MinSpread)
        {
            spreadValue = weapon.MinSpread;
        }

        // Add burst spread
        if (weapon.SpreadPerBurst != 0 && weapon.SpreadRampTime != 0)
        {
            // Shambles
            float mult = (float)weaponSimState.AccumulatedSpreadTime / weapon.SpreadRampTime;
            float spreadRange = weapon.MaxSpread - weapon.MinSpread;
            spreadValue += mult * spreadRange;
        }

        // If max spread is set, ensure we are not above maxSpread, even if minSpread > maxSpread (BioCrossbow, AR ADS)
        if (weapon.MaxSpread != 0 && spreadValue > weapon.MaxSpread)
        {
            spreadValue = weapon.MaxSpread;
        }

        // Calculate with spread factor
        float spreadPct = spreadValue * weaponSpreadFactor;

        // Add movement bonus
        spreadPct += weaponSimState.CurrentMovementSpreadBonus;

        return spreadPct;
    }

    private void ProcessEntity(CharacterEntity entity)
    {
        DebugWeaponSpread(entity);
        ProcessWeaponSpread(entity);
    }

    private void ProcessWeaponSpread(CharacterEntity entity)
    {
        // NOTE: Consider this whole thing a sham, needs further RE.
        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null)
        {
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var weaponSimState = _weaponSimState.GetValueOrDefault(entity.EntityId, new WeaponSimState());
        var currentTime = _shard.CurrentTime;

        // Process Accumulated Spread Time
        if (weaponSimState.AccumulatedSpreadTime > 0)
        {
            int timeCanReturn = (int)(currentTime - weaponSimState.LastBurstTime - weapon.MsSpreadReturnDelay);
            if (timeCanReturn > 0)
            {
                uint returnedTime = (uint)System.Math.Min(weapon.MsSpreadReturn, timeCanReturn);
                float ratioToReturn = (float)returnedTime / weapon.MsSpreadReturn;
                uint rampTimeToReturn = (uint)(weaponSimState.AccumulatedSpreadTime * ratioToReturn);
                uint update = (uint)System.Math.Max(0, (int)weaponSimState.AccumulatedSpreadTime - rampTimeToReturn);

                // Console.WriteLine($"returnedTime {returnedTime}, ratioToReturn: {ratioToReturn}, rampTimeToReturn: {rampTimeToReturn}, Setting AccumulatedSpreadTime To : {update}");
                weaponSimState.AccumulatedSpreadTime = update;
            }
        }

        // Process Movement Bonus
        if (entity.IsAirborne || entity.IsMoving)
        {
            // We are moving, so we should be at full value
            weaponSimState.LastMovementTime = currentTime;
            if (entity.IsAirborne)
            {
                weaponSimState.CurrentMovementSpreadBonus = weapon.JumpMinSpread;
            }
            else if (entity.IsMoving)
            {
                weaponSimState.CurrentMovementSpreadBonus = weapon.RunMinSpread;
            }
        }
        else if (weaponSimState.CurrentMovementSpreadBonus > 0)
        {
            if (currentTime > weaponSimState.LastMovementTime + weapon.MsSpreadReturnDelay)
            {
                // Delay passed, we should reduce
                uint elapsedTime = currentTime - (weaponSimState.LastMovementTime + weapon.MsSpreadReturnDelay);
                if (weapon.MsSpreadReturn > elapsedTime)
                {
                    // Within return time, reducing by ratio of passed time
                    float ratioToReturn = (float)elapsedTime / weapon.MsSpreadReturn;
                    float update = (float)(weaponSimState.CurrentMovementSpreadBonus * (1f - ratioToReturn));

                    // Console.WriteLine($"ReturnMovementSpread elapsedTime: {elapsedTime}, ratioToReturn: {ratioToReturn}, Setting CurrentMovementSpreadBonus To : {update}");
                    weaponSimState.CurrentMovementSpreadBonus = update;
                }
                else
                {
                    // Atter return time, finished reducing and setting 0
                    weaponSimState.CurrentMovementSpreadBonus = 0;
                }
            }
            else
            {
                // Within delay, keep full bonus
            }
        }

        _weaponSimState[entity.EntityId] = weaponSimState;
    }

    private void DebugWeaponSpread(CharacterEntity entity)
    {
        var client = entity.Player;

        if (client.Preferences.DebugWeapon == 0)
        {
            return;
        }

        if (!(client.Status.Equals(IPlayer.PlayerStatus.Playing) || client.Status.Equals(IPlayer.PlayerStatus.Loading)) && client.NetClientStatus.Equals(Status.Connected))
        {
            return;
        }

        var activeWeaponDetails = entity.GetActiveWeaponDetails();
        if (activeWeaponDetails == null)
        {
            return;
        }

        var weapon = activeWeaponDetails.Weapon;
        var weaponId = activeWeaponDetails.WeaponId;
        var weaponSpreadFactor = activeWeaponDetails.Spread;
        var weaponSimState = _weaponSimState.GetValueOrDefault(entity.EntityId, new WeaponSimState());
        var time = _shard.CurrentTime;

        var spreadPct = GetCurrentSpreadPct(entity, weapon, weaponSimState, weaponSpreadFactor, time);

        var eventData = new DebugWeaponSimEventData()
        {
            WeaponName = weapon.DebugName,
            SpreadPct = spreadPct,
            WeaponId = weaponId,
            AccumulatedSpreadTime = weaponSimState.AccumulatedSpreadTime,
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
            Console.WriteLine($"Failed DebugWeaponSpread", e);
        }
    }

    private IEnumerable<INetworkPlayer> GetWeaponSimPlayers()
    {
        return _shard.Clients.Values.Where((client) => (client.Status.Equals(IPlayer.PlayerStatus.Playing) || client.Status.Equals(IPlayer.PlayerStatus.Loading)) && client.NetClientStatus.Equals(Status.Connected));
    }

    private IEnumerable<IEntity> GetWeaponSimPlayersEntities()
    {
        return _shard.Entities.Values.Where((entity) => entity is CharacterEntity character && character.IsPlayerControlled);
    }

    public class WeaponSimState
    {
        public Vector3 LastSpreadDirection;
        public uint LastSpreadTime;
        public uint LastBurstTime;
        public float Ramp;
        public uint LastWeaponId;

        public uint AccumulatedSpreadTime;
        public uint LastMovementTime;
        public float CurrentMovementSpreadBonus;
    }

    public record class DebugWeaponSimEventData
    {
        public string WeaponName { get; set; }
        public float SpreadPct { get; set; }
        public uint WeaponId { get; set; }
        public uint AccumulatedSpreadTime { get; set; }
    }
}