using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ShowRewardScreenCommand : ICommand
{
    private ShowRewardScreenCommandDef Params;

    public ShowRewardScreenCommand(ShowRewardScreenCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}