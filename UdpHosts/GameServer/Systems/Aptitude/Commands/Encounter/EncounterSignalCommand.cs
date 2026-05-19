using GameServer.Entities;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Encounter;

public class EncounterSignalCommand : Command, ICommand
{
    private EncounterSignalCommandDef Params;

    public EncounterSignalCommand(EncounterSignalCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var self = (BaseEntity)context.Self;

        if (self.Encounter != null && self.Encounter.Handles(EncounterComponent.Event.Signal))
        {
            self.Encounter.Instance.OnSignal();
        }

        result.SetPass();
        return;
    }
}