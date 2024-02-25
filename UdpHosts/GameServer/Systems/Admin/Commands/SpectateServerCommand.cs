namespace GameServer.Admin;

[ServerCommand("Toggle (hacky) spectator mode", "spectate", "flycam")]
public class SpectateServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var character = context.SourcePlayer?.CharacterEntity;
        if (character == null)
        {
            SourceFeedback("Cannot spectate without a valid player character", context);
            return;
        }

        byte newValue = (byte)(character.Character_BaseController.SpectatorModeProp == 0 ? 2 : 0);
        character.Character_BaseController.SpectatorModeProp = newValue;

        if (newValue != 0)
        {
            context.Shard.EntityMan.ScopeOut(context.SourcePlayer, character);
            
            if (character.Character_SpectatorController == null)
            {
                character.Character_SpectatorController = new AeroMessages.GSS.V66.Character.Controller.SpectatorController()
                {
                    SpectatorModeProp = newValue,
                };
            }

            character.Character_CombatController.CombatFlagsProp = new AeroMessages.GSS.V66.Character.CombatFlagsData()
            {
                Time = context.Shard.CurrentTime,
                Value = (AeroMessages.GSS.V66.Character.CombatFlagsData.CharacterCombatFlags)0xFFFFFFFF,
            };
            context.Shard.EntityMan.ScopeIn(context.SourcePlayer, character);
        }
        else
        {
            context.Shard.EntityMan.ScopeOut(context.SourcePlayer, character);
            character.Character_SpectatorController = null;
            character.Character_CombatController.CombatFlagsProp = new AeroMessages.GSS.V66.Character.CombatFlagsData()
            {
                Time = context.Shard.CurrentTime,
                Value = (AeroMessages.GSS.V66.Character.CombatFlagsData.CharacterCombatFlags)0,
            };
            character.SetSpawnPose();
            character.SetSpawnTime(context.Shard.CurrentTime);
            context.Shard.EntityMan.ScopeIn(context.SourcePlayer, character);
        }

        if (newValue != 0)
        {
            SourceFeedback("Spectate ON", context);
        }
        else
        {
            SourceFeedback("Spectate OFF", context);
        }
    }
}
