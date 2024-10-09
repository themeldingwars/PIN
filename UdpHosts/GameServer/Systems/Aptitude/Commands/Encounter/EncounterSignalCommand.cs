using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities;

namespace GameServer.Aptitude;

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
