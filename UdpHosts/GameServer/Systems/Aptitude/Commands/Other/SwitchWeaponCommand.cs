using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Protocol;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SwitchWeaponCommand : Command, ICommand
{
    private readonly SwitchWeaponCommandDef Params;

    public SwitchWeaponCommand(SwitchWeaponCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity character)
        {
            result.SetPass();
            return;
        }

        context.PreviousWeaponIndex = character.WeaponIndex.Index;
        Switch(context, character, TargetIndex);
        result.SetPass();
    }

    public override void Reset(Context context)
    {
        if (Params.RestoreOnRollback == 1
            && context.Self is CharacterEntity character
            && context.PreviousWeaponIndex is { } previous
            && previous != TargetIndex)
        {
            Switch(context, character, previous);
        }
    }

    private byte TargetIndex => (byte)(Params.TargetWeaponSlot - 1);

    private void Switch(Context context, CharacterEntity character, byte index)
    {
        if (character.WeaponIndex.Index == index)
        {
            return;
        }

        character.SetWeaponIndex(new WeaponIndexData
        {
            Index = index,
            PreviousIndex = character.WeaponIndex.Index,
            PlaySwapAnimation = Params.PlayAnimation,
            Time = context.Shard.CurrentTime,
        });

        if (character is { IsPlayerControlled: true, Player: { } player })
        {
            player.NetChannels[ChannelType.ReliableGss].SendMessage(new ForcedWeaponSwap { WeaponIndex = index, ShortTime = context.Shard.CurrentShortTime }, character.EntityId);
        }
    }
}
