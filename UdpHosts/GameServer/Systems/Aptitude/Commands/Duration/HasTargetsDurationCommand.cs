﻿using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class HasTargetsDurationCommand : ICommand
{
    private HasTargetsDurationCommandDef Params;

    public HasTargetsDurationCommand(HasTargetsDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = context.Targets.Count >= Params.MinCount;

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}