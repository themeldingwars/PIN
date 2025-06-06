using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class InstantActivationCommand : Command, ICommand
{
    private InstantActivationCommandDef Params;

    public InstantActivationCommand(InstantActivationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        /*
        if (context.Self is CharacterEntity { IsPlayerControlled: true } character)
        {
            var player = character.Player;
            var message = new AbilityActivated
            {
                ActivatedAbilityId = context.AbilityId,
                ActivatedTime = context.InitTime,
                AbilityCooldownsData = new AbilityCooldownsData
                {
                    ActiveCooldowns_Group1 = Array.Empty<ActiveCooldown>(),
                    ActiveCooldowns_Group2 = Array.Empty<ActiveCooldown>(),
                    Unk = 0,
                    GlobalCooldown_Activated_Time = context.InitTime,
                    GlobalCooldown_ReadyAgain_Time = context.InitTime + Params.GlobalCooldown,
                }
            };
            Console.WriteLine($"ActivateAbility {message.ActivatedAbilityId} at {message.ActivatedTime}");
            player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }
        */

        return true;
    }
}