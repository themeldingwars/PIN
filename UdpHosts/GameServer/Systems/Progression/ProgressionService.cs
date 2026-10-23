using System;
using System.Collections.Generic;
using System.Linq;
using Aero.Protocol;
using AeroMessages.GSS;
using AeroMessages.GSS.Character.Controller;
using AeroMessages.GSS.Character.Event;
using GameServer.Data;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Turret;
using GameServer.Entities.Vehicle;
using GameServer.Enums;
using GameServer.Extensions;
using GameServer.Protocol;
using GameServer.StaticDB;
using GameServer.Systems.Aptitude;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.SystemEvents;
using Serilog;
using SdbCharCreateLoadoutSlots = GameServer.StaticDB.Records.dbcharacter.CharCreateLoadoutSlots;
using SdbDeployable = GameServer.StaticDB.Records.dbcharacter.Deployable;
using SdbMonster = GameServer.StaticDB.Records.dbcharacter.Monster;
using SdbRootItem = GameServer.StaticDB.Records.dbitems.RootItem;

namespace GameServer.Systems.Progression;

/// <summary>
///     Awards XP and runs battleframe level ups. A kill reward is the <c>dbcharacter::XPRewardType</c> tier's
///     <c>base_xp</c> scaled by a per-level factor and by the player's own XP modifiers. The modified and the
///     unmodified number both go to the client, in <c>RewardInfo</c> and in the matching <c>PrivateCombatLog</c>
///     RewardEvent row, and the two have to agree.
/// </summary>
public class ProgressionService
{
    private const uint _levelUpEffectId = 399;
    private const uint _eliteLevelUpEffectId = 13238;

    private static readonly ILogger _logger = Log.ForContext<ProgressionService>();

    private readonly IShard _shard;
    private readonly IProgressionRules _rules;

    public ProgressionService(IShard shard, IEventBus eventBus, IProgressionRules rules)
    {
        _shard = shard;
        _rules = rules;
        MaxFrameLevel = ResolveMaxFrameLevel();

        eventBus.Subscribe<CharacterDiedEvent>(OnCharacterDied);
        eventBus.Subscribe<DeployableDestroyedEvent>(OnDeployableDestroyed);
    }

    /// <summary>The highest battleframe level the dbitems::FrameProgressionLevel curve runs out to.</summary>
    public uint MaxFrameLevel { get; }

    /// <summary>
    ///     The XP cost of advancing out of one battleframe level, from dbitems::FrameProgressionLevel.
    ///     0 means the level is the top of the curve.
    /// </summary>
    /// <param name="level">The level whose cost to look up.</param>
    /// <returns>The XP needed to reach level + 1, or 0 when the curve has no row for the level.</returns>
    public static uint GetXpCost(uint level) => SDBInterface.GetFrameProgressionLevel(level)?.XpCost ?? 0;

    /// <summary>
    ///     Awards a flat amount of XP to the battleframe the character is wearing, without a kill behind it.
    /// </summary>
    /// <param name="player">The player earning the XP.</param>
    /// <param name="xp">How much XP to award, already boosted.</param>
    /// <param name="rewardTypeId">The dbcharacter::XPRewardType id to report, 0 for none.</param>
    /// <param name="uiType">
    ///     How the client labels the reward pop. It defaults to the encounter kind because a flat award has no
    ///     kill behind it, and the client reads the kill kind as a kill feed entry.
    /// </param>
    /// <returns>The progression state after the award, or null when there is no character to award to.</returns>
    public ProgressionUpdate? AwardXp(INetworkPlayer player, uint xp, uint rewardTypeId = 0, XpRewardUiType uiType = XpRewardUiType.FromEncounter)
    {
        var character = player?.CharacterEntity;

        if (character == null || xp == 0)
        {
            return null;
        }

        SendRewardInfo(player, null, xp, xp, uiType);
        SendRewardCombatLog(player, null, rewardTypeId, (int)xp, (int)xp, false);

        return AddXp(player, character.CurrentLoadout.ChassisID, xp);
    }

    /// <summary>
    ///     Moves the battleframe the character is wearing to an exact level, skipping the XP curve.
    ///     Level up rewards are granted for every level the frame moves up through.
    /// </summary>
    /// <param name="player">The player whose frame changes level.</param>
    /// <param name="level">The level to move to, clamped to [1, <see cref="MaxFrameLevel"/>].</param>
    /// <returns>The progression state after the change, or null when there is no character to change.</returns>
    public ProgressionUpdate? SetLevel(INetworkPlayer player, uint level)
    {
        if (player?.CharacterEntity == null)
        {
            return null;
        }

        var chassisId = player.CharacterEntity.CurrentLoadout.ChassisID;
        var frame = player.Battleframes.GetOrAdd(chassisId);
        var previousLevel = frame.Level;

        frame.Level = Math.Clamp(level, 1u, MaxFrameLevel);
        frame.CurrentXp = 0;

        return ApplyProgressionChange(player, chassisId, previousLevel, frame.Level);
    }

    /// <summary>
    ///     Advances the battleframe the character is wearing by a number of levels without paying XP for them.
    /// </summary>
    /// <param name="player">The player whose frame levels up.</param>
    /// <param name="levels">How many levels to advance, at least one.</param>
    /// <returns>The progression state after the change, or null when there is no character to change.</returns>
    public ProgressionUpdate? LevelUp(INetworkPlayer player, uint levels = 1)
    {
        if (player?.CharacterEntity == null)
        {
            return null;
        }

        var chassisId = player.CharacterEntity.CurrentLoadout.ChassisID;
        var frame = player.Battleframes.GetOrAdd(chassisId);
        var previousLevel = frame.Level;

        frame.Level = Math.Min(previousLevel + Math.Max(1, levels), MaxFrameLevel);

        return ApplyProgressionChange(player, chassisId, previousLevel, frame.Level);
    }

    /// <summary>
    ///     Sends the client the XP and level of one of the player's battleframes.
    /// </summary>
    /// <param name="player">The player whose frame to report.</param>
    /// <param name="chassisId">The chassis whose XP and level to report.</param>
    public void SendXpRefresh(INetworkPlayer player, uint chassisId)
    {
        if (player?.CharacterEntity is not { } character)
        {
            return;
        }

        var frame = player.Battleframes.Get(chassisId);

        player.NetChannels[ChannelType.ReliableGss].SendMessage(
            new ProgressionXPRefresh
            {
                Frames =
                [
                    new ProgressionFrameInfo
                    {
                        ChassisID = chassisId,
                        CurrentXp = (uint)(frame?.CurrentXp ?? 0),
                        LifetimeXp = (uint)(frame?.LifetimeXp ?? 0),
                        CurrentLevel = frame?.Level ?? character.Level,
                        Unk = 0,
                    },
                ],
            },
            character.EntityId);
    }

    /// <summary>
    ///     Sends the client the XP it earned, as the RewardInfo message that drives the on-screen reward pop.
    /// </summary>
    /// <param name="player">The player earning the XP.</param>
    /// <param name="source">The entity the XP came from, or null when nothing specific did it.</param>
    /// <param name="total">The XP awarded, boost included.</param>
    /// <param name="unmodified">The XP awarded before the boost.</param>
    /// <param name="uiType">The kind the client shows the reward as.</param>
    private static void SendRewardInfo(INetworkPlayer player, IEntity source, uint total, uint unmodified, XpRewardUiType uiType)
    {
        var bonus = total > unmodified ? total - unmodified : 0;

        player.NetChannels[ChannelType.ReliableGss].SendMessage(
            new RewardInfo
            {
                RewardType = (byte)uiType,

                // BonusType mirrors RewardType and is only 0 when there is no bonus part at all.
                BonusType = bonus > 0 ? (byte)uiType : (byte)0,
                RelatedEntity = source?.AeroEntityId ?? default,
                RewardValue = total,
                KillCount = 0,
                BonusValue = bonus,
            },
            player.CharacterEntity.EntityId);
    }

    /// <summary>
    ///     Sends the RewardEvent combat log row that pairs with the RewardInfo message.
    /// </summary>
    /// <param name="player">The player earning the XP.</param>
    /// <param name="source">The entity the XP came from, or null when nothing specific did it.</param>
    /// <param name="rewardTypeId">The dbcharacter::XPRewardType id of the reward tier.</param>
    /// <param name="total">The XP awarded, boost included.</param>
    /// <param name="unmodified">The XP awarded before the boost.</param>
    /// <param name="isNpc">Whether the source was an NPC, which is what the client's kill feed filters on.</param>
    private static void SendRewardCombatLog(INetworkPlayer player, IEntity source, uint rewardTypeId, int total, int unmodified, bool isNpc)
    {
        var character = player.CharacterEntity;

        var row = new CombatLogRow
        {
            SourceType = CombatLogRow.CombatSourceType.Weapon,
            LogType = CombatLogRow.CombatLogType.RewardEvent,
            Type34Data = new CombatLogType34Data
            {
                T34_XpGain = total,
                T34_XpUnmodifiedGain = unmodified,
                T34_XpRewardType = rewardTypeId,
                T34_ChassisId = character.CurrentLoadout.ChassisID,
                T34_Level = character.Level,
                T34_FrameLevel = character.Level,

                // Mirrors T34_FrameLevel in 302 of the 311 captured rows.
                T34_Unk7 = character.Level,

                // T34_Entity carries the controller id in its low byte, so it is the full EntityId backing.
                T34_Entity = source?.AeroEntityId.Backing ?? 0,
                T34_SourceMonsterId = source is CharacterEntity npc ? npc.StaticInfo.CharacterTypeId : 0,
                T34_SourceIsNPC = (byte)(isNpc ? 1 : 0),
            },
        };

        var message = new PrivateCombatLog { HaveData = 1, Data = new CombatLogMessage { Entries = [row] } };

        // Bytes is the length of the entry array that follows it: everything the message holds apart from
        // the HaveData flag and the field itself.
        message.Data.Bytes = (ushort)(message.GetPackedSize() - sizeof(int) - sizeof(ushort));

        player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
    }

    private static float ModifierValue(StatModifierData modifier) => modifier.StatValue > 0 ? modifier.StatValue : 1f;

    /// <summary>
    ///     Works out the reward tier of a monster kill.
    /// </summary>
    /// <param name="monster">The monster that died.</param>
    /// <returns>The reward tier id, its base_xp, and the tier multiplier to apply.</returns>
    private static (uint RewardTypeId, uint BaseXp, float RewardMult) ResolveMonsterReward(SdbMonster monster)
    {
        var battleframe = SDBInterface.GetBattleframe(monster.ChassisId);
        var rewardTypeId = monster.XprewardType != 0 ? monster.XprewardType : battleframe?.XprewardType ?? 0;
        var baseXp = SDBInterface.GetXPRewardType(rewardTypeId)?.BaseXp ?? 0;

        // xpreward_mult is 1 on every battleframe row, but it is the column the tier is scaled by.
        var rewardMult = battleframe is { XprewardMult: > 0 } ? battleframe.XprewardMult : 1f;

        return (rewardTypeId, baseXp, rewardMult);
    }

    /// <summary>
    ///     Works out the reward tier of a destroyed deployable. Deployables carry their own tier and multiplier
    ///     outright, with no chassis to inherit from.
    /// </summary>
    /// <param name="deployable">The deployable that was destroyed.</param>
    /// <returns>The reward tier id, its base_xp, and the tier multiplier to apply.</returns>
    private static (uint RewardTypeId, uint BaseXp, float RewardMult) ResolveDeployableReward(SdbDeployable deployable)
    {
        var baseXp = SDBInterface.GetXPRewardType(deployable.XprewardType)?.BaseXp ?? 0;
        var rewardMult = deployable.XprewardMult > 0 ? deployable.XprewardMult : 1f;

        return (deployable.XprewardType, baseXp, rewardMult);
    }

    /// <summary>
    ///     Resolves the player a kill is credited to.
    /// </summary>
    /// <param name="source">The entity that landed the killing blow.</param>
    /// <returns>The player to reward, or null when the kill is not attributable to one.</returns>
    private static INetworkPlayer ResolveRewardPlayer(IEntity source)
    {
        return source switch
        {
            CharacterEntity character when character.IsPlayerControlled => character.Player,
            VehicleEntity vehicle when vehicle.IsPlayerControlled => vehicle.ControllingPlayer,
            TurretEntity turret when turret.IsPlayerControlled => turret.ControllingPlayer,
            DeployableEntity deployable => deployable.IsPlayerOwned ? deployable.Player : deployable.Owner?.Player,
            _ => null,
        };
    }

    /// <summary>
    ///     The default loadout slots of a chassis, from dbcharacter::CharCreateLoadout and its slots table.
    /// </summary>
    /// <param name="chassisId">The dbitems::RootItem sdb_id of the chassis.</param>
    /// <returns>The slot rows by slot index, or null when the chassis has no loadout rows.</returns>
    private static Dictionary<byte, SdbCharCreateLoadoutSlots> GetFrameDefaultLoadoutSlots(uint chassisId)
    {
        var loadout = SDBInterface.GetCharCreateLoadoutsByFrame(chassisId)
            .Where(entry => entry.IsDev == 0 && entry.IsExperimental == 0)
            .OrderByDescending(entry => entry.IsStartingLoadout)
            .ThenBy(entry => entry.Id)
            .FirstOrDefault();

        return loadout == null ? null : SDBInterface.GetCharCreateLoadoutSlots(loadout.Id);
    }

    /// <summary>
    ///     The top of the dbitems::FrameProgressionLevel curve. The table runs 1 to 50 and the last row has
    ///     xp_cost 0, so the highest level is one past the last row that still costs something to leave.
    /// </summary>
    private static uint ResolveMaxFrameLevel()
    {
        var highest = 0u;

        for (var level = 1u; level <= byte.MaxValue; level++)
        {
            if (GetXpCost(level) > 0)
            {
                highest = level + 1;
            }
        }

        return highest > 0 ? highest : byte.MaxValue;
    }

    /// <summary>
    ///     The product of the five XP modifier fields retail keeps on the character's BaseController.
    /// </summary>
    /// <param name="character">The character earning the XP.</param>
    /// <returns>The XP multiplier to apply on top of the unmodified reward.</returns>
    private float GetXpBoost(CharacterEntity character)
    {
        var controller = character.Character_BaseController;

        if (controller == null)
        {
            return _rules.XpBoost;
        }

        return _rules.XpBoost
               * ModifierValue(controller.XpBoostModifierProp)
               * ModifierValue(controller.XpPermanentModifierProp)
               * ModifierValue(controller.XpZoneModifierProp)
               * ModifierValue(controller.XpVipModifierProp)
               * ModifierValue(controller.XpEventModifierProp);
    }

    private void OnCharacterDied(CharacterDiedEvent evt)
    {
        var victim = evt.Target;

        if (victim.IsPlayerControlled)
        {
            return;
        }

        // XP only ever lands on a player, so a kill no player can be credited for pays nothing.
        var player = ResolveRewardPlayer(evt.Source);

        if (player == null)
        {
            return;
        }

        var monster = SDBInterface.GetMonster(victim.StaticInfo.CharacterTypeId);

        if (monster == null)
        {
            _logger.Debug("No Monster row for {Victim}, cannot work out its XP reward", victim);
            return;
        }

        var (rewardTypeId, baseXp, rewardMult) = ResolveMonsterReward(monster);

        // Tier 2 is the friendly and neutral population and pays 0 on purpose, so there is nothing to send.
        if (baseXp == 0)
        {
            return;
        }

        AwardKill(player, victim, victim.Level, rewardTypeId, baseXp, rewardMult, true);
    }

    private void OnDeployableDestroyed(DeployableDestroyedEvent evt)
    {
        var player = ResolveRewardPlayer(evt.Source);

        if (player == null)
        {
            return;
        }

        var deployable = SDBInterface.GetDeployable(evt.Target.Type);

        if (deployable == null)
        {
            _logger.Debug("No Deployable row for {Victim}, cannot work out its XP reward", evt.Target);
            return;
        }

        var (rewardTypeId, baseXp, rewardMult) = ResolveDeployableReward(deployable);

        if (baseXp == 0)
        {
            return;
        }

        AwardKill(player, evt.Target, deployable.DeployableLevel, rewardTypeId, baseXp, rewardMult, false);
    }

    /// <summary>
    ///     Turns a kill into XP
    /// </summary>
    private void AwardKill(INetworkPlayer player, IEntity victim, uint victimLevel, uint rewardTypeId, uint baseXp, float rewardMult, bool isNpc)
    {
        var character = player.CharacterEntity;
        var levelScale = _rules.GetLevelScale(victimLevel);
        var boost = GetXpBoost(character);

        var reward = baseXp * levelScale * rewardMult;
        var unmodified = (int)Math.Floor(reward);
        var total = (int)Math.Floor(reward * boost);

        if (total == 0)
        {
            return;
        }

        SendRewardInfo(player, victim, (uint)total, (uint)unmodified, XpRewardUiType.Kill);
        SendRewardCombatLog(player, victim, rewardTypeId, total, unmodified, isNpc);

        var update = AddXp(player, character.CurrentLoadout.ChassisID, (uint)total);

        _logger.Debug(
            "{Player} earns {Total} xp for killing {Victim} at level {Level}: base {BaseXp} of tier {RewardType} x {Scale:F4} level scale x {Boost:F4} boost, {Levels} level(s) gained",
            character,
            total,
            victim,
            victimLevel,
            baseXp,
            rewardTypeId,
            levelScale,
            boost,
            update.LevelsGained);
    }

    /// <summary>
    ///     Adds XP to one battleframe and walks the FrameProgressionLevel curve, spending the current level's
    ///     cost for every level the new total covers.
    /// </summary>
    private ProgressionUpdate AddXp(INetworkPlayer player, uint chassisId, uint xp)
    {
        var frame = player.Battleframes.GetOrAdd(chassisId);
        var previousLevel = frame.Level;

        frame.CurrentXp += xp;
        frame.LifetimeXp += xp;

        while (frame.Level < MaxFrameLevel)
        {
            var cost = GetXpCost(frame.Level);

            if (cost == 0 || frame.CurrentXp < cost)
            {
                break;
            }

            frame.CurrentXp -= cost;
            frame.Level++;
        }

        return ApplyProgressionChange(player, chassisId, previousLevel, frame.Level);
    }

    /// <summary>
    ///     Consumes progression change.
    /// </summary>
    /// <param name="player">The player whose progression changed.</param>
    /// <param name="chassisId">The chassis whose progression changed.</param>
    /// <param name="previousLevel">The level the frame started the change on.</param>
    /// <param name="newLevel">The level the frame reached.</param>
    /// <returns>The progression state after the change.</returns>
    private ProgressionUpdate ApplyProgressionChange(INetworkPlayer player, uint chassisId, uint previousLevel, uint newLevel)
    {
        var character = player.CharacterEntity;
        var frame = player.Battleframes.GetOrAdd(chassisId);
        var levelsGained = newLevel > previousLevel ? newLevel - previousLevel : 0;

        if (levelsGained == 0)
        {
            player.Battleframes.ApplyFrameXp(character, chassisId);
        }
        else
        {
            ApplyLevelUp(player, character, chassisId, previousLevel, newLevel, levelsGained);
        }

        SendXpRefresh(player, chassisId);

        player.Battleframes.Persist(chassisId);

        return new ProgressionUpdate(chassisId, frame.Level, frame.CurrentXp, frame.LifetimeXp, levelsGained, GetXpCost(frame.Level));
    }

    /// <summary>
    ///     Puts a level the frame just reached onto the character and tells everyone about it.
    /// </summary>
    /// <param name="player">The player whose frame leveled.</param>
    /// <param name="character">The character wearing the frame.</param>
    /// <param name="chassisId">The chassis that leveled up.</param>
    /// <param name="previousLevel">The level the frame was on before.</param>
    /// <param name="newLevel">The level the frame reached.</param>
    /// <param name="levelsGained">How many levels the frame advanced.</param>
    private void ApplyLevelUp(INetworkPlayer player, CharacterEntity character, uint chassisId, uint previousLevel, uint newLevel, uint levelsGained)
    {
        player.Battleframes.ApplyFrameProgression(character, chassisId);

        // Health and weapon scaling are derived off the level by ApplyLoadout, so the loadout goes through
        // again to move them onto the new level.
        character.ApplyLoadout(character.CurrentLoadout);

        SendLevelUpEvent(player, character, chassisId, levelsGained, newLevel);
        ApplyLevelEffect(character, _levelUpEffectId);

        GrantFrameLevelRewards(player, chassisId, previousLevel, newLevel);
    }

    /// <summary>
    ///     Tells the clients about a level up
    /// </summary>
    /// <param name="player">The player whose frame leveled.</param>
    /// <param name="character">The character wearing the frame.</param>
    /// <param name="chassisId">The chassis that leveled up.</param>
    /// <param name="levelsGained">How many levels the frame advanced.</param>
    /// <param name="newLevel">The level the frame reached.</param>
    private void SendLevelUpEvent(INetworkPlayer player, CharacterEntity character, uint chassisId, uint levelsGained, uint newLevel)
    {
        var levelUp = new LevelUpEvent
        {
            FrameId = chassisId,
            NewLevel = (byte)Math.Min(newLevel, byte.MaxValue),
            NumberOfLevelsAdvanced = (byte)Math.Min(levelsGained, byte.MaxValue),

            // dbcharacter::LoadoutRareReplacements swaps gear at specific levels; not applied yet.
            RareReplacementLevels = [],
        };

        player.NetChannels[ChannelType.ReliableGss].SendMessage(levelUp, character.EntityId);

        SendLevelUpEventToObservers(character, levelUp);
    }

    /// <summary>
    ///     Sends the same event body to every player that has the character scoped in
    /// </summary>
    /// <param name="character">The character that leveled; its scoped-in players are the recipients.</param>
    /// <param name="levelUp">The event to send.</param>
    private void SendLevelUpEventToObservers(CharacterEntity character, LevelUpEvent levelUp)
    {
        if (!WireIds.TryGetGssWireIdsOnView(typeof(LevelUpEvent), _shard.Settings.GssProtocolVersion, GssCharacterView.ObserverView, out var typecode, out var messageId))
        {
            _logger.Debug(
                "LevelUpEvent has no ObserverView route in protocol version {Version}, leaving the level up of {Character} unreported to its observers",
                _shard.Settings.GssProtocolVersion,
                character);

            return;
        }

        levelUp.SerializeToMemory(out var messageData);
        _shard.EntityMan.SendToScopedReliable(character, messageId, typecode, messageData.Span);
    }

    /// <summary>
    ///     Puts a progression celebration effect on the character.
    /// </summary>
    /// <param name="character">The character to put the effect on.</param>
    /// <param name="effectId">The dbeffects::StatusEffectData id of the effect.</param>
    private void ApplyLevelEffect(CharacterEntity character, uint effectId)
    {
        if (SDBInterface.GetStatusEffectData(effectId) == null)
        {
            _logger.Debug("No StatusEffectData row for effect {EffectId}, leaving {Character} without its level up effect", effectId, character);
            return;
        }

        _shard.Abilities.DoApplyEffect(effectId, character, new Context(_shard, character)
        {
            InitTime = _shard.CurrentTime,
        });
    }

    /// <summary>
    ///     Hands out the gear a battleframe earns by leveling.
    /// </summary>
    /// <param name="player">The player whose frame leveled.</param>
    /// <param name="chassisId">The chassis that leveled up.</param>
    /// <param name="previousLevel">The level the frame was at before.</param>
    /// <param name="newLevel">The level the frame reached.</param>
    private void GrantFrameLevelRewards(INetworkPlayer player, uint chassisId, uint previousLevel, uint newLevel)
    {
        var character = player.CharacterEntity;

        if (!player.Battleframes.IsPersisted(chassisId))
        {
            _logger.Debug(
                "{Player} is on a temporary frame, leaving the level {Level} rewards of chassis {ChassisId} ungranted",
                character,
                newLevel,
                chassisId);

            return;
        }

        var defaultSlots = GetFrameDefaultLoadoutSlots(chassisId);

        if (defaultSlots == null || defaultSlots.Count == 0)
        {
            _logger.Debug("No CharCreateLoadoutSlots rows for chassis {ChassisId}, no level up rewards to grant", chassisId);
            return;
        }

        var loadoutId = player.Inventory.GetLoadoutIdForChassis(chassisId);

        if (loadoutId == 0)
        {
            _logger.Warning(
                "{Player} has no loadout for chassis {ChassisId}, cannot slot the level {Level} rewards",
                character,
                chassisId,
                newLevel);

            return;
        }

        var granted = new List<string>();
        var slottedCount = 0;

        foreach ((byte slotIndex, var slot) in defaultSlots)
        {
            var itemId = slot.DefaultPveModule;

            if (itemId == 0)
            {
                continue;
            }

            var item = SDBInterface.GetRootItem(itemId);

            if (item == null)
            {
                _logger.Warning("CharCreateLoadoutSlots slots item {ItemId}, which has no RootItem row", itemId);
                continue;
            }

            if (item.RequiredLevel <= previousLevel || item.RequiredLevel > newLevel)
            {
                continue;
            }

            var guid = player.Inventory.CreateItem(itemId);

            if (guid == 0)
            {
                continue;
            }

            granted.Add($"{slotIndex}={itemId}");

            if (TrySlotLevelReward(player, character, loadoutId, slotIndex, item, guid, newLevel))
            {
                slottedCount++;
            }
        }

        if (granted.Count == 0)
        {
            return;
        }

        if (slottedCount > 0)
        {
            character.ApplyLoadout(character.CurrentLoadout);
        }

        _logger.Information(
            "Granted {Count} level up reward(s) to {Player} on chassis {ChassisId} between levels {From} and {To}, {Slotted} of them slotted into the loadout: {Rewards}",
            granted.Count,
            character,
            chassisId,
            previousLevel + 1,
            newLevel,
            slottedCount,
            string.Join(", ", granted));
    }

    /// <summary>
    ///     Slots one level up reward into the frame's loadout when the slot can take it. The reward is already in
    ///     the inventory by the time this runs.
    /// </summary>
    /// <param name="player">The player who earned the reward.</param>
    /// <param name="character">The character wearing the frame.</param>
    /// <param name="loadoutId">The loadout of the frame that leveled.</param>
    /// <param name="slotIndex">The slot_type of the row the reward came from, a dbitems::LoadoutSlot id.</param>
    /// <param name="item">The reward's dbitems::RootItem row.</param>
    /// <param name="itemGuid">The guid the reward has in the inventory.</param>
    /// <param name="newLevel">The level the frame reached.</param>
    /// <returns>True when the item went into the slot, false when it stays in the inventory.</returns>
    private bool TrySlotLevelReward(INetworkPlayer player, CharacterEntity character, int loadoutId, byte slotIndex, SdbRootItem item, ulong itemGuid, uint newLevel)
    {
        var loadoutSlot = (LoadoutSlotType)slotIndex;

        if (!Enum.IsDefined(loadoutSlot))
        {
            _logger.Debug(
                "Leaving the level {Level} reward {ItemId} in the inventory: slot {SlotIndex} ({SlotName})",
                newLevel,
                item.SdbId,
                slotIndex,
                SDBInterface.GetLoadoutSlot(slotIndex)?.Name ?? "no LoadoutSlot row");

            return false;
        }

        var acceptedType = CharacterLoadout.GetAcceptedItemType(loadoutSlot);

        if (acceptedType == null || (ItemType)item.Type != acceptedType.Value)
        {
            _logger.Debug(
                "Leaving the level {Level} reward {ItemId} in the inventory: it is of type {ItemType}, which slot {Slot} does not accept",
                newLevel,
                item.SdbId,
                (ItemType)item.Type,
                loadoutSlot);

            return false;
        }

        if (character.CurrentLoadout.SlottedItems.GetValueOrDefault(loadoutSlot) != 0)
        {
            _logger.Debug(
                "Leaving the level {Level} reward {ItemId} in the inventory: slot {Slot} already holds an item",
                newLevel,
                item.SdbId,
                loadoutSlot);

            return false;
        }

        player.Inventory.EquipItemByGUID(loadoutId, loadoutSlot, itemGuid);
        return true;
    }
}
