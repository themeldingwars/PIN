using System.Linq;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Extensions;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromItemStatCommand : Command, ICommand
{
    private LoadRegisterFromItemStatCommandDef Params;

    public LoadRegisterFromItemStatCommand(LoadRegisterFromItemStatCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (Params.FromTarget == 1)
        {
            // The target list of a proximity ability also holds the deployable that owns it (its area acquire
            // starts at the pad itself), so take the first *character* in it: the item stat that is being looked
            // up belongs to the player that walked into range.
            target = context.Targets.FirstOrDefault(candidate => candidate is CharacterEntity)
                     ?? (context.Targets.Count > 0 ? context.Targets.Peek() : null);

            if (target == null)
            {
                if (OnceLog.ShouldLog((nameof(LoadRegisterFromItemStatCommand), "no target", Params.Id)))
                {
                    Logger.Debug("[{Command} {CommandId}] FromTarget is set but the chain has no target, leaving the register alone", nameof(LoadRegisterFromItemStatCommand), Params.Id);
                }

                return true;
            }
        }

        if (Params.FromInitiator == 1)
        {
            target = context.Initiator;
        }

        if (target is not CharacterEntity character)
        {
            // Item stats belong to characters. A deployable owned chain (a glider pad reading the glider stat of
            // the player that walked onto it) has no character to read from here, and failing the command the way
            // the requirement commands used to would make the ability apply and lose its effect over and over.
            if (OnceLog.ShouldLog((nameof(LoadRegisterFromItemStatCommand), "not a character", Params.Id)))
            {
                Logger.Debug("[{Command} {CommandId}] target {TargetType} is not a Character, leaving the register alone",
                    nameof(LoadRegisterFromItemStatCommand), Params.Id, target?.GetType().Name ?? "nothing");
            }

            return true;
        }

        float prevValue = context.Register;
        float statValue = character.GetItemAttribute(Params.Stat);
        context.Register = AbilitySystem.RegistryOp(prevValue, statValue, (Operand)Params.Regop);

        // Duration chains re-run this on every effect update tick, so only report the value once per command
        // unless it actually changed. This is what used to fill the log with the same boomerang duration line
        // several times a second.
        if (OnceLog.ShouldLog((nameof(LoadRegisterFromItemStatCommand), "value", Params.Id, context.Register)))
        {
            var statInfo = SDBInterface.GetAttributeDefinition((uint)Params.Stat);
            Logger.Debug("{Command} {CommandId}: ({prevValue}, {statValue} ({statName}), {op}) => {register}", nameof(LoadRegisterFromItemStatCommand), Params.Id, prevValue, statValue, statInfo.Name.Trim(), (Operand)Params.Regop, context.Register);
        }

        return true;
    }
}