using AeroMessages.GSS.V66.Character;
using GameServer.Entities.Character;
using System.Text;

namespace GameServer.Admin;

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

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("GetActiveWeaponDetails");
        stringBuilder.AppendLine($"Weapon: {info.Weapon.DebugName}");
        stringBuilder.AppendLine($"WeaponId: {info.WeaponId}");
        stringBuilder.AppendLine($"Calculated Spread Factor: {info.Spread}");
        stringBuilder.AppendLine($"Attribute RateOfFire: {info.RateOfFire}");

        stringBuilder.AppendLine($"----- Components");
        stringBuilder.AppendLine($"ScopeId: {info.Weapon.ScopeId}");
        stringBuilder.AppendLine($"UnderbarrelId: {info.Weapon.UnderbarrelId}");
        stringBuilder.AppendLine($"AmmoId: {info.Weapon.AmmoId} (From Template)");

        stringBuilder.AppendLine($"----- Properties");
        stringBuilder.AppendLine($"WeaponFlags: {info.Weapon.WeaponFlags}");
        stringBuilder.AppendLine($"FireType: {info.Weapon.FireType}");
        stringBuilder.AppendLine($"SlotIndex: {info.Weapon.SlotIndex}");
        stringBuilder.AppendLine($"Range: {info.Weapon.Range}");
        stringBuilder.AppendLine($"EquipEnterMs: {info.Weapon.EquipEnterMs}");
        stringBuilder.AppendLine($"EquipExitMs: {info.Weapon.EquipExitMs}");

        /*
        stringBuilder.AppendLine($"----- Abilities");
        stringBuilder.AppendLine($"MeleeAbility: {info.Weapon.MeleeAbility}");
        stringBuilder.AppendLine($"AttackAbility: {info.Weapon.AttackAbility}");
        stringBuilder.AppendLine($"OverchargeAbility: {info.Weapon.OverchargeAbility}");
        stringBuilder.AppendLine($"BurstAbility: {info.Weapon.BurstAbility}");
        stringBuilder.AppendLine($"ReloadAbility: {info.Weapon.ReloadAbility}");
        stringBuilder.AppendLine($"EmptyAbility: {info.Weapon.EmptyAbility}");
        */

        /*
        stringBuilder.AppendLine($"----- Ammo, Clip, Reload");
        stringBuilder.AppendLine($"BaseClipSize: {info.Weapon.BaseClipSize}");
        stringBuilder.AppendLine($"MaxAmmo: {info.Weapon.MaxAmmo}");
        stringBuilder.AppendLine($"AmmoPerBurst: {info.Weapon.AmmoPerBurst}");
        stringBuilder.AppendLine($"MinAmmoPerBurst: {info.Weapon.MinAmmoPerBurst}");
        stringBuilder.AppendLine($"RoundsPerBurst: {info.Weapon.RoundsPerBurst}");
        stringBuilder.AppendLine($"MinRoundsPerBurst: {info.Weapon.MinRoundsPerBurst}");
        stringBuilder.AppendLine($"RoundReload: {info.Weapon.RoundReload}");
        stringBuilder.AppendLine($"ClipRegenMs: {info.Weapon.ClipRegenMs}");
        stringBuilder.AppendLine($"ReloadTime: {info.Weapon.ReloadTime}");
        stringBuilder.AppendLine($"ReloadPenalty: {info.Weapon.ReloadPenalty}");
        */

        /*
        stringBuilder.AppendLine($"----- Targets");
        stringBuilder.AppendLine($"MaxTargets: {info.Weapon.MaxTargets}");
        stringBuilder.AppendLine($"BurstBonusPerTarget: {info.Weapon.BurstBonusPerTarget}");
        stringBuilder.AppendLine($"TargetingRange: {info.Weapon.TargetingRange}");
        */

        stringBuilder.AppendLine($"----- Burst");
        stringBuilder.AppendLine($"MsPerBurst: {info.Weapon.MsPerBurst}");
        stringBuilder.AppendLine($"MsBurstDuration: {info.Weapon.MsBurstDuration}");

        stringBuilder.AppendLine($"----- Chargeup, Overcharge");
        stringBuilder.AppendLine($"MsChargeUp: {info.Weapon.MsChargeUp}");
        stringBuilder.AppendLine($"MsChargeUpMax: {info.Weapon.MsChargeUpMax}");
        stringBuilder.AppendLine($"MsChargeUpMin: {info.Weapon.MsChargeUpMin}");
        stringBuilder.AppendLine($"MsOverchargeDelay: {info.Weapon.MsOverchargeDelay}");

        /*
        stringBuilder.AppendLine($"----- Damage");
        stringBuilder.AppendLine($"MinDamage: {info.Weapon.MinDamage}");
        stringBuilder.AppendLine($"DamagePerRound: {info.Weapon.DamagePerRound}");
        stringBuilder.AppendLine($"HeadshotMult: {info.Weapon.HeadshotMult}");
        */

        stringBuilder.AppendLine($"----- ?");
        stringBuilder.AppendLine($"MsReturn: {info.Weapon.MsReturn}");

        stringBuilder.AppendLine($"----- Spread");
        stringBuilder.AppendLine($"MinSpread: {info.Weapon.MinSpread}");
        stringBuilder.AppendLine($"MaxSpread: {info.Weapon.MaxSpread}");
        stringBuilder.AppendLine($"StartingSpread: {info.Weapon.StartingSpread}");
        stringBuilder.AppendLine($"SpreadPerBurst: {info.Weapon.SpreadPerBurst}");
        stringBuilder.AppendLine($"SpreadRampExponent: {info.Weapon.SpreadRampExponent}");
        stringBuilder.AppendLine($"SpreadRampTime: {info.Weapon.SpreadRampTime}");
        stringBuilder.AppendLine($"RunMinSpread: {info.Weapon.RunMinSpread}");
        stringBuilder.AppendLine($"JumpMinSpread: {info.Weapon.JumpMinSpread}");
        stringBuilder.AppendLine($"MsSpreadReturnDelay: {info.Weapon.MsSpreadReturnDelay}");
        stringBuilder.AppendLine($"MsSpreadReturn: {info.Weapon.MsSpreadReturn}");
        stringBuilder.AppendLine($"NoSpreadChance: {info.Weapon.NoSpreadChance}");

        /*
        stringBuilder.AppendLine($"----- Agility");
        stringBuilder.AppendLine($"Agility: {info.Weapon.Agility}");
        stringBuilder.AppendLine($"MsAgilityReturn: {info.Weapon.MsAgilityReturn}");
        stringBuilder.AppendLine($"MsAgilityReturnDelay: {info.Weapon.MsAgilityReturnDelay}");
        stringBuilder.AppendLine($"-----");
        */

        var message = stringBuilder.ToString();

        context.SourcePlayer.SendDebugLog(message);
        SourceFeedback($"Printing weapon info to console", context);
    }
}
