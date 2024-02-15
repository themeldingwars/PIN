using System;
using GameServer.Data.SDB.Records.aptfs;
using static AeroMessages.GSS.V66.Character.CharacterStateData;

namespace GameServer.Aptitude;

public class RequirementServerCommand : ICommand
{
    private RequirementServerCommandDef Params;

    public RequirementServerCommand(RequirementServerCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // TODO: Investigate Params.Local, Params.LocalInit
        if (Params.Server == 1)
        {
            return true;
        }
        else
        {
            Console.WriteLine($"RequirementServerCommand returns false for ${Params.Id}");
            return false;
        }
    }
}