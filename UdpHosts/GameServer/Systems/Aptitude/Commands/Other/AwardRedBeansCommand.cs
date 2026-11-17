using GameServer.Entities.Character;
using GameServer.GRPC;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class AwardRedBeansCommand : Command, ICommand
{
    private AwardRedBeansCommandDef Params;

    public AwardRedBeansCommand(AwardRedBeansCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self;

        if (target is CharacterEntity { IsPlayerControlled: true } character)
        {
            // Shown straight away; RIN reports the real account balance back once it has added them
            character.SetRedBeans((long)character.Wallet.Beans + Params.Amount);

            var characterGuid = character.EntityId + (byte)GuidService.AdditionalTypes.Character;
            _ = GRPCService.SendCommandAsync(new GrpcGameServerAPIClient.Command
            {
                AwardRedBeans = new GrpcGameServerAPIClient.AwardRedBeans { CharacterGuid = characterGuid, Amount = Params.Amount }
            });
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because self is not a character (why is it running on something other than a character?)", nameof(AwardRedBeansCommand), Params.Id);
            result.SetFail();
            return;
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}