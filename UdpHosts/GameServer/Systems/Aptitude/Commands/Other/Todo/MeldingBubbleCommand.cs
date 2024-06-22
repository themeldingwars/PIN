using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class MeldingBubbleCommand : ICommand
{
    private MeldingBubbleCommandDef Params;

    public MeldingBubbleCommand(MeldingBubbleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}