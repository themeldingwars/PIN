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

    public bool Execute(Context context)
    {
        var self = (BaseEntity)context.Self;

        if (self.Encounter != null && self.Encounter.Handles(EncounterComponent.Event.Signal))
        {
            self.Encounter.Instance.OnSignal();
        }

        return true;
    }
}