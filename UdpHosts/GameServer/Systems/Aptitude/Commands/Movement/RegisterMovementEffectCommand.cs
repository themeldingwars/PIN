using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class RegisterMovementEffectCommand : Command, ICommand
{
    private const ushort _sprintFlag = 0x10;

    private RegisterMovementEffectCommandDef Params;

    public RegisterMovementEffectCommand(RegisterMovementEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    private ushort Flags => Params.Sprinting != 0 ? _sprintFlag : (ushort)0;

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.OnServer != 0 && context.Self is CharacterEntity character)
        {
            if (Params.MovestateIndex is 0 or > 13)
            {
                Logger.Warning("{Command} {CommandId} has movestate index {Index} outside 1 to 13", nameof(RegisterMovementEffectCommand), Params.Id, Params.MovestateIndex);
            }
            else
            {
                character.RegisterMovementEffect(Params.StatusfxId, (byte)Params.MovestateIndex, Flags, Params.Reapply != 0);
            }
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
        if (Params.OnServer != 0 && Params.MovestateIndex is > 0 and <= 13 && context.Self is CharacterEntity character)
        {
            character.UnregisterMovementEffect(Params.StatusfxId, (byte)Params.MovestateIndex, Flags, Params.Reapply != 0);
        }
    }
}