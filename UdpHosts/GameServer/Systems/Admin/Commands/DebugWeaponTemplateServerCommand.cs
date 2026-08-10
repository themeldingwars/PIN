#pragma warning disable CS8632
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbitems;
using GameServer.Systems.WeaponSim;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Print server weapon info", "dbg_weapon", "dbg_weapon")]
public class DebugWeaponTemplateServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot without a valid player character", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        if (context.Target != null && context.Target is CharacterEntity commandTarget)
        {
            character = commandTarget;
        }

        var info = character.GetActiveWeaponDetails();
        if (info == null || info.Weapon == null)
        {
            context.SourcePlayer.SendDebugLog("=== Weapon Debug ===\n(not a weapon)");
            return;
        }

        var fullInfo = SDBUtils.GetDetailedWeaponInfo(info.WeaponId);

        var sb = new StringBuilder();
        sb.AppendLine("=== Weapon Debug ===");
        sb.AppendLine($"WeaponId: {info.WeaponId}");
        sb.AppendLine($"Active: {info.Weapon.DebugName}");
        sb.AppendLine($"SpreadFactor: {info.Spread} | RateOfFire: {info.RateOfFire}");
        if (fullInfo.Alt != null)
        {
            sb.AppendLine($"Alt (Underbarrel): {fullInfo.Alt.DebugName}");
        }

        var slots = GetSlots(info.WeaponId, sb);
        var weapon = SDBInterface.GetWeapon(info.WeaponId);
        var template = SDBInterface.GetWeaponTemplate(weapon?.WeaponTypeId ?? 0);
        var wmod = SDBInterface.GetWeaponTemplateModifiers(info.WeaponId);
        var mods = GetModuleMods(slots);

        sb.AppendLine();
        AppendCascade(sb, "=== Main Weapon Cascade ===", info.Weapon, template, wmod, mods);

        if (fullInfo.Alt != null)
        {
            var ub = SDBInterface.GetWeaponUnderbarrel(fullInfo.Main.UnderbarrelId);
            var ubTemplate = ub != null ? SDBInterface.GetWeaponTemplate(ub.WeaponTypeId) : null;
            var ubMod = ub != null ? SDBInterface.GetWeaponTemplateModifiers(ub.Id) : (WeaponTemplateModifiers?)null;
            var ubSlots = ub != null ? (SDBInterface.GetWeaponSlots(ub.Id) ?? new List<WeaponSlot>()) : new List<WeaponSlot>();
            var ubMods = GetModuleMods(ubSlots);

            sb.AppendLine();
            AppendCascade(sb, "=== Underbarrel Cascade ===", fullInfo.Alt, ubTemplate, ubMod, ubMods);
        }

        sb.AppendLine();
        AppendAmmo(sb, info.Weapon.AmmoId);

        sb.AppendLine();
        AppendAttributes(sb, character);

        sb.AppendLine();
        AppendSpreadProfile(sb, info.SpreadProfile);

        context.SourcePlayer.SendDebugLog(sb.ToString());
        SourceFeedback("Printing weapon info to console", context);
    }

    private static List<WeaponSlot> GetSlots(uint weaponId, StringBuilder sb)
    {
        var weaponSlots = SDBInterface.GetWeaponSlots(weaponId);
        var slots = weaponSlots ?? new List<WeaponSlot>();
        if (slots.Count > 1)
        {
            sb.AppendLine($"WARNING - {slots.Count} WeaponSlot entries, modifiers merged sequentially");
        }

        sb.AppendLine();
        sb.AppendLine("--- WeaponSlots ---");
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot.DefaultAbility != 0)
            {
                var am = SDBInterface.GetAbilityModule(slot.DefaultAbility);
                sb.AppendLine($"  [{i}] DefaultAbility: {slot.DefaultAbility}{(am != null ? $" (ModuleType={am.ModuleType}, ChainId={am.AbilityChainId})" : " (no AbilityModule)")}");
            }
            else
            {
                sb.AppendLine($"  [{i}] DefaultAbility: 0");
            }
        }

        return slots;
    }

    private static List<WeaponTemplateModifiers?> GetModuleMods(List<WeaponSlot> slots)
    {
        var result = new List<WeaponTemplateModifiers?>();
        foreach (var slot in slots)
        {
            if (slot.DefaultAbility != 0)
            {
                result.Add(SDBInterface.GetWeaponTemplateModifiers(slot.DefaultAbility));
            }
            else
            {
                result.Add(null);
            }
        }

        return result;
    }

    private static void AppendCascade(StringBuilder sb, string title, WeaponTemplateResult w, WeaponTemplates? tpl, WeaponTemplateModifiers? wm, List<WeaponTemplateModifiers?> mods)
    {
        sb.AppendLine(title);
        if (tpl == null)
        {
            sb.AppendLine("  (template not found)");
            return;
        }

        sb.AppendLine();
        sb.AppendLine("  [Components]");
        OvOv(sb, "  AmmoId", w.AmmoId, tpl.DefaultAmmoId, wm?.DefaultAmmoId, mods, m => m.DefaultAmmoId, v => v == 0);
        OvOv(sb, "  ScopeId", w.ScopeId, tpl.DefaultScopeId, wm?.DefaultScopeId, mods, m => m.DefaultScopeId, v => v == 0);
        OvOv(sb, "  UnderbarrelId", w.UnderbarrelId, tpl.DefaultUnderbarrelId, wm?.DefaultUnderbarrelId, mods, m => m.DefaultUnderbarrelId, v => v == 0);

        sb.AppendLine();
        sb.AppendLine("  [Properties]");
        ModI(sb, "  WeaponFlags", w.WeaponFlags, tpl.WeaponFlags, (float)(wm?.WeaponFlags ?? 0), 1f, mods, m => (float)m.WeaponFlags);
        ModF(sb, "  FireType", (float)w.FireType, tpl.AnimFireType, (float)(wm?.AnimFireType ?? 0), 1f, mods, m => (float)m.FireType);
        ModF(sb, "  SlotIndex", (float)w.SlotIndex, tpl.SlotIndex, 0f, 1f, mods, m => 0f);
        ModFM(sb, "  Range", w.Range, tpl.Range, wm?.Range ?? 0f, wm?.RangeMult ?? 1f, mods, (m, a, u) => (m.Range, m.RangeMult));
        ModI(sb, "  EquipEnterMs", w.EquipEnterMs, tpl.EquipEnterMs, (float)(wm?.EquipEnterMs ?? 0), 1f, mods, m => (float)m.EquipEnterMs);
        ModI(sb, "  EquipExitMs", w.EquipExitMs, tpl.EquipExitMs, (float)(wm?.EquipExitMs ?? 0), 1f, mods, m => (float)m.EquipExitMs);

        sb.AppendLine();
        sb.AppendLine("  [Ammo, Clip, Reload]");
        ModFM(sb, "  BaseClipSize", (float)w.BaseClipSize, tpl.BaseClipSize, wm?.BaseClipSize ?? 0f, wm?.BaseClipSizeMult ?? 1f, mods, (m, a, u) => (m.BaseClipSize, m.BaseClipSizeMult));
        ModFM(sb, "  MaxAmmo", (float)w.MaxAmmo, tpl.MaxAmmo, wm?.MaxAmmo ?? 0f, wm?.MaxAmmoMult ?? 1f, mods, (m, a, u) => (m.MaxAmmo, m.MaxAmmoMult));
        ModI(sb, "  AmmoPerBurst", w.AmmoPerBurst, tpl.AmmoPerBurst, (float)(wm?.AmmoPerBurst ?? 0), 1f, mods, m => (float)m.AmmoPerBurst);
        ModI(sb, "  MinAmmoPerBurst", w.MinAmmoPerBurst, tpl.MinAmmoPerBurst, (float)(wm?.MinAmmoPerBurst ?? 0), 1f, mods, m => (float)m.MinAmmoPerBurst);
        ModFM(sb, "  RoundsPerBurst", (float)w.RoundsPerBurst, tpl.RoundsPerBurst, wm?.RoundsPerBurst ?? 0f, wm?.RoundsPerBurstMult ?? 1f, mods, (m, a, u) => (m.RoundsPerBurst, m.RoundsPerBurstMult));
        ModFM(sb, "  MinRoundsPerBurst", (float)w.MinRoundsPerBurst, tpl.MinRoundsPerBurst, wm?.MinRoundsPerBurst ?? 0f, wm?.MinRoundsPerBurstMult ?? 1f, mods, (m, a, u) => (m.MinRoundsPerBurst, m.MinRoundsPerBurstMult));
        ModI(sb, "  RoundReload", w.RoundReload, tpl.RoundReload, (float)(wm?.RoundReload ?? 0), 1f, mods, m => (float)m.RoundReload);
        ModFM(sb, "  ClipRegenMs", (float)w.ClipRegenMs, tpl.ClipRegenMs, wm?.ClipRegenMs ?? 0f, wm?.ClipRegenMsMult ?? 1f, mods, (m, a, u) => (m.ClipRegenMs, m.ClipRegenMsMult));
        ModFM(sb, "  ReloadTime", (float)w.ReloadTime, tpl.ReloadTime, wm?.ReloadTime ?? 0f, wm?.ReloadTimeMult ?? 1f, mods, (m, a, u) => (m.ReloadTime, m.ReloadTimeMult));
        ModFM(sb, "  ReloadPenalty", (float)w.ReloadPenalty, tpl.ReloadPenalty, wm?.ReloadPenalty ?? 0f, wm?.ReloadPenaltyMult ?? 1f, mods, (m, a, u) => (m.ReloadPenalty, m.ReloadPenaltyMult));

        sb.AppendLine();
        sb.AppendLine("  [Targets]");
        ModI(sb, "  MaxTargets", w.MaxTargets, tpl.MaxTargets, (float)(wm?.MaxTargets ?? 0), 1f, mods, m => (float)m.MaxTargets);
        ModI(sb, "  BurstBonusPerTarget", w.BurstBonusPerTarget, tpl.BurstbonusPerTarget, (float)(wm?.BurstbonusPerTarget ?? 0), 1f, mods, m => (float)m.BurstbonusPerTarget);
        ModFM(sb, "  TargetingRange", w.TargetingRange, tpl.TargetingRange, wm?.TargetingRange ?? 0f, wm?.TargetingRangeMult ?? 1f, mods, (m, a, u) => (m.TargetingRange, m.TargetingRangeMult));

        sb.AppendLine();
        sb.AppendLine("  [Burst]");
        ModFM(sb, "  MsPerBurst", (float)w.MsPerBurst, tpl.MsPerBurst, wm?.MsPerBurst ?? 0f, wm?.MsPerBurstMult ?? 1f, mods, (m, a, u) => (m.MsPerBurst, m.MsPerBurstMult));
        ModI(sb, "  MsBurstDuration", w.MsBurstDuration, tpl.MsBurstDuration, (float)(wm?.MsBurstDuration ?? 0), 1f, mods, m => (float)m.MsBurstDuration);

        sb.AppendLine();
        sb.AppendLine("  [Chargeup, Overcharge]");
        ModFM(sb, "  MsChargeUp", (float)w.MsChargeUp, tpl.MsChargeup, wm?.MsChargeup ?? 0f, wm?.MsChargeupMult ?? 1f, mods, (m, a, u) => (m.MsChargeup, m.MsChargeupMult));
        ModFM(sb, "  MsChargeUpMax", (float)w.MsChargeUpMax, tpl.MsChargeupMax, wm?.MsChargeupMax ?? 0f, wm?.MsChargeupMaxMult ?? 1f, mods, (m, a, u) => (m.MsChargeupMax, m.MsChargeupMaxMult));
        ModFM(sb, "  MsChargeUpMin", (float)w.MsChargeUpMin, tpl.MsChargeupMin, wm?.MsChargeupMin ?? 0f, wm?.MsChargeupMinMult ?? 1f, mods, (m, a, u) => (m.MsChargeupMin, m.MsChargeupMinMult));
        ModI(sb, "  MsOverchargeDelay", w.MsOverchargeDelay, tpl.MsOverchargeDelay, (float)(wm?.MsOverchargeDelay ?? 0), 1f, mods, m => (float)m.MsOverchargeDelay);

        sb.AppendLine();
        sb.AppendLine("  [Damage]");
        ModFM(sb, "  MinDamage", w.MinDamage, tpl.MinDamage, wm?.MinDamage ?? 0f, wm?.MinDamageMult ?? 1f, mods, (m, a, u) => (m.MinDamage, m.MinDamageMult));
        ModFM(sb, "  DamagePerRound", w.DamagePerRound, tpl.DamagePerRound, wm?.DamagePerRound ?? 0f, wm?.DamagePerRoundMult ?? 1f, mods, (m, a, u) => (m.DamagePerRound, m.DamagePerRoundMult));
        ModFM(sb, "  HeadshotMult", w.HeadshotMult, tpl.HeadshotMult, wm?.HeadshotMult ?? 0f, wm?.HeadshotMultMult ?? 1f, mods, (m, a, u) => (m.HeadshotMult, m.HeadshotMultMult));

        sb.AppendLine();
        sb.AppendLine("  [Spread]");
        ModFM(sb, "  MinSpread", w.MinSpread, tpl.MinSpread, wm?.MinSpread ?? 0f, wm?.MinSpreadMult ?? 1f, mods, (m, a, u) => (m.MinSpread, m.MinSpreadMult));
        ModFM(sb, "  MaxSpread", w.MaxSpread, tpl.MaxSpread, wm?.MaxSpread ?? 0f, wm?.MaxSpreadMult ?? 1f, mods, (m, a, u) => (m.MaxSpread, m.MaxSpreadMult));
        ModFM(sb, "  StartingSpread", w.StartingSpread, tpl.StartingSpread, wm?.StartingSpread ?? 0f, wm?.StartingSpreadMult ?? 1f, mods, (m, a, u) => (m.StartingSpread, m.StartingSpreadMult));
        ModFM(sb, "  SpreadPerBurst", w.SpreadPerBurst, tpl.SpreadPerBurst, wm?.SpreadPerBurst ?? 0f, wm?.SpreadPerBurstMult ?? 1f, mods, (m, a, u) => (m.SpreadPerBurst, m.SpreadPerBurstMult));
        ModF(sb, "  SpreadRampExponent", w.SpreadRampExponent, tpl.SpreadRampExponent, wm?.SpreadRampExponent ?? 0f, 1f, mods, m => m.SpreadRampExponent);
        ModI(sb, "  SpreadRampTime", w.SpreadRampTime, tpl.SpreadRampTime, wm?.SpreadRampTime ?? 0f, 1f, mods, m => (float)m.SpreadRampTime);
        ModF(sb, "  RunMinSpread", w.RunMinSpread, tpl.RunMinspreadAdd, wm?.RunMinspreadAdd ?? 0f, 1f, mods, m => m.RunMinspreadAdd);
        ModF(sb, "  JumpMinSpread", w.JumpMinSpread, tpl.JumpMinspreadAdd, wm?.JumpMinspreadAdd ?? 0f, 1f, mods, m => m.JumpMinspreadAdd);
        ModI(sb, "  MsSpreadReturnDelay", w.MsSpreadReturnDelay, tpl.MsSpreadReturnDelay, wm?.MsSpreadReturnDelay ?? 0f, 1f, mods, m => (float)m.MsSpreadReturnDelay);
        ModI(sb, "  MsSpreadReturn", w.MsSpreadReturn, tpl.MsSpreadReturn, wm?.MsSpreadReturn ?? 0f, 1f, mods, m => (float)m.MsSpreadReturn);
        ModFM(sb, "  NoSpreadChance", w.NoSpreadChance, tpl.NoSpreadChance, wm?.NoSpreadChance ?? 0f, wm?.NoSpreadChanceMult ?? 1f, mods, (m, a, u) => (m.NoSpreadChance, m.NoSpreadChanceMult));

        sb.AppendLine();
        sb.AppendLine("  [Agility]");
        ModF(sb, "  Agility", w.Agility, tpl.Agility, wm?.Agility ?? 0f, 1f, mods, m => m.Agility);
        ModI(sb, "  MsAgilityReturn", w.MsAgilityReturn, tpl.MsAgilityReturn, wm?.MsAgilityReturn ?? 0f, 1f, mods, m => (float)m.MsAgilityReturn);
        ModI(sb, "  MsAgilityReturnDelay", w.MsAgilityReturnDelay, tpl.MsAgilityReturnDelay, wm?.MsAgilityReturnDelay ?? 0f, 1f, mods, m => (float)m.MsAgilityReturnDelay);

        sb.AppendLine();
        sb.AppendLine("  [Misc]");
        ModI(sb, "  MsReturn", w.MsReturn, tpl.MsReturn, wm?.MsReturn ?? 0f, 1f, mods, m => (float)m.MsReturn);
    }

    private static void OvOv(StringBuilder sb, string name, uint final, uint tpl, uint? wm, List<WeaponTemplateModifiers?> mods, Func<WeaponTemplateModifiers, uint?> getter, Func<uint, bool> isDefault)
    {
        var p = new StringBuilder();
        p.AppendFormat("  {0}: {1} | Tpl: {2}", name, final, tpl);
        p.Append(" | WpnMod: ");
        if (wm != null && !isDefault(wm.Value))
        {
            p.Append(wm.Value);
        }
        else
        {
            p.Append("(none)");
        }

        uint computed = tpl;
        if (wm != null && !isDefault(wm.Value))
        {
            computed = wm.Value;
        }

        for (int i = 0; i < mods.Count; i++)
        {
            uint? v = mods[i] != null ? getter(mods[i]) : (uint?)null;
            p.AppendFormat(" | Mod[{0}]: ", i);
            if (v != null && !isDefault(v.Value))
            {
                p.Append(v.Value);
                computed = v.Value;
            }
            else
            {
                p.Append("(none)");
            }
        }

        if (computed != final)
        {
            p.Append(" MISMATCH (computed ");
            p.Append(computed);
            p.AppendLine(")");
        }
        else
        {
            p.AppendLine();
        }

        sb.Append(p);
    }

    private static void ModF(StringBuilder sb, string name, float final, float tpl, float wadd, float wmult, List<WeaponTemplateModifiers?> mods, Func<WeaponTemplateModifiers, float> getter)
    {
        float computed = (tpl + wadd) * wmult;
        var p = new StringBuilder();
        p.AppendFormat("  {0}: {1} | Tpl: {2}", name, FF(final), FF(tpl));
        p.Append(" | WpnMod: ");
        p.Append(MS(wadd, wmult));

        for (int i = 0; i < mods.Count; i++)
        {
            float mad = mods[i] != null ? getter(mods[i]) : 0f;
            p.AppendFormat(" | Mod[{0}]: ", i);
            p.Append(MS(mad, 1f));
            computed = (computed + mad) * 1f;
        }

        if (Math.Abs(computed - final) > 0.01f)
        {
            p.Append(" MISMATCH (computed ");
            p.Append(FF(computed));
            p.AppendLine(")");
        }
        else
        {
            p.AppendLine();
        }

        sb.Append(p);
    }

    private static void ModI(StringBuilder sb, string name, object final, float tpl, float wadd, float wmult, List<WeaponTemplateModifiers?> mods, Func<WeaponTemplateModifiers, float> getter)
    {
        float computed = (tpl + wadd) * wmult;
        var p = new StringBuilder();
        p.AppendFormat("  {0}: {1} | Tpl: {2}", name, final, FF(tpl));
        p.Append(" | WpnMod: ");
        p.Append(MS(wadd, wmult));

        for (int i = 0; i < mods.Count; i++)
        {
            float mad = mods[i] != null ? getter(mods[i]) : 0f;
            p.AppendFormat(" | Mod[{0}]: ", i);
            p.Append(MS(mad, 1f));
            computed = (computed + mad) * 1f;
        }

        var computedObj = Convert.ChangeType((int)computed, final.GetType());
        if (!object.Equals(computedObj, final))
        {
            p.Append(" MISMATCH (computed ");
            p.Append(computedObj);
            p.AppendLine(")");
        }
        else
        {
            p.AppendLine();
        }

        sb.Append(p);
    }

    private static void ModFM(StringBuilder sb, string name, float final, float tpl, float wadd, float wmult, List<WeaponTemplateModifiers?> mods, Func<WeaponTemplateModifiers, float, float, (float, float)> getter)
    {
        float computed = (tpl + wadd) * wmult;
        var p = new StringBuilder();
        p.AppendFormat("  {0}: {1} | Tpl: {2}", name, FF(final), FF(tpl));
        p.Append(" | WpnMod: ");
        p.Append(MS(wadd, wmult));

        for (int i = 0; i < mods.Count; i++)
        {
            var mod = mods[i];
            (float mad, float mmu) = mod != null ? getter(mod, wadd, wmult) : (0f, 1f);
            p.AppendFormat(" | Mod[{0}]: ", i);
            p.Append(MS(mad, mmu));
            computed = (computed + mad) * mmu;
        }

        if (Math.Abs(computed - final) > 0.01f)
        {
            p.Append(" MISMATCH (computed ");
            p.Append(FF(computed));
            p.AppendLine(")");
        }
        else
        {
            p.AppendLine();
        }

        sb.Append(p);
    }

    private static string MS(float add, float mult)
    {
        if (add == 0f && mult == 1f)
        {
            return "(none)";
        }

        if (add == 0f)
        {
            return $"x{mult:F2}";
        }

        if (mult == 1f)
        {
            return (add >= 0 ? "+" : string.Empty) + FS(add);
        }

        return (add >= 0 ? "+" : string.Empty) + FS(add) + "x" + FS(mult);
    }

    private static string FF(float v)
    {
        return Math.Abs(v - (float)(int)v) < 0.0001f ? v.ToString("F0") : v.ToString("F4");
    }

    private static string FS(float v)
    {
        return Math.Abs(v - (float)(int)v) < 0.0001f ? v.ToString("F0") : v.ToString("F2");
    }

    private static void AppendAmmo(StringBuilder sb, ushort ammoId)
    {
        sb.AppendLine("--- Ammo ---");
        var ammo = SDBInterface.GetAmmo(ammoId);
        if (ammo != null)
        {
            var af = new AmmoFlags(ammo.Flags);
            sb.AppendLine($"  AmmoId: {ammo.Id}");
            sb.AppendLine($"  Name: {ammo.Name}");
            sb.AppendLine($"  Flags: {af}");
            sb.AppendLine($"  ProjectileSpeed: {ammo.ProjectileSpeed}");
            sb.AppendLine($"  Gravity: {ammo.Gravity}");
            sb.AppendLine($"  BounceCos: {ammo.BounceCos}");
            sb.AppendLine($"  SlopeBounceCos: {ammo.SlopeBounceCos}");
            sb.AppendLine($"  MaxBounces: {ammo.MaxBounces}");
            sb.AppendLine($"  MaxHits: {ammo.MaxHits}");
            sb.AppendLine($"  BounceFriction: {ammo.BounceFriction}");
            sb.AppendLine($"  BounceElasticity: {ammo.BounceElasticity}");
            sb.AppendLine($"  BounceDuration: {ammo.BounceDuration}");
            sb.AppendLine($"  ConstLifetime: {ammo.ConstLifetime}");
        }
        else
        {
            sb.AppendLine($"  Ammo not found for AmmoId: {ammoId}");
        }
    }

    private static void AppendAttributes(StringBuilder sb, CharacterEntity character)
    {
        sb.AppendLine("--- Attributes ---");
        var attributes = character.GetActiveWeaponAttributes();
        foreach (var (id, value) in attributes)
        {
            var attr = SDBInterface.GetAttributeDefinition(id);
            sb.AppendLine($"  {id} {attr.Name}: {value}");
        }

        if (!attributes.ContainsKey((ushort)ItemAttributeId.WeaponSpread))
        {
            sb.AppendLine("  WARNING - missing WeaponSpread attribute");
        }

        if (!attributes.ContainsKey((ushort)ItemAttributeId.RateOfFire))
        {
            sb.AppendLine("  WARNING - missing RateOfFire attribute");
        }
    }

    private static void AppendSpreadProfile(StringBuilder sb, WeaponSpreadProfile wp)
    {
        sb.AppendLine("--- WeaponSpreadProfile---");
        sb.AppendLine($"  BaseSpreadPct: {wp.BaseSpreadPct}");
        sb.AppendLine($"  OtherSpreadPct: {wp.OtherSpreadPct}");
        sb.AppendLine($"  StartingSpread: {wp.StartingSpread}");
        sb.AppendLine($"  Agility: {wp.Agility}");
        sb.AppendLine($"  SpreadPerBurst: {wp.SpreadPerBurst}");
        sb.AppendLine($"  MinSpreadFrac: {wp.MinSpreadFrac}");
        sb.AppendLine($"  SpreadRampExponent: {wp.SpreadRampExponent}");
        sb.AppendLine($"  SpreadRampTime: {wp.SpreadRampTime}");
        sb.AppendLine($"  MsSpreadReturn: {wp.MsSpreadReturn}");
        sb.AppendLine($"  MsSpreadReturnDelay: {wp.MsSpreadReturnDelay}");
        sb.AppendLine($"  MsReturn: {wp.MsReturn}");
        sb.AppendLine($"  MsRiseReturnDelay: {wp.MsRiseReturnDelay}");
        sb.AppendLine($"  MsAgilityReturn: {wp.MsAgilityReturn}");
        sb.AppendLine($"  MsAgilityReturnDelay: {wp.MsAgilityReturnDelay}");
        sb.AppendLine($"  MsPerBurst: {wp.MsPerBurst}");
    }
}
