using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class InterruptCommand : Command, ICommand
{
    private InterruptCommandDef Params;

    public InterruptCommand(InterruptCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}