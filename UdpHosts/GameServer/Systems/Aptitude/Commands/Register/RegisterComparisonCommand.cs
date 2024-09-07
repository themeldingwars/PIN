using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.apt;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class RegisterComparisonCommand : Command, ICommand
{
    private RegisterComparisonCommandDef Params;

    public RegisterComparisonCommand(RegisterComparisonCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // NOTE: The params can have multiple set, need to double check if this implementation is appropriate. It currently returns true if any condition matches.
        bool result = false;

        if (Params.EqualTo == 1)
        {
            float minValue = Params.CompareVal - Params.EqualTol;
            float maxValue = Params.CompareVal + Params.EqualTol;
            
            if (context.Register >= minValue && context.Register <= maxValue)
            {
                result = true;
            }
        }

        if (Params.LessThan == 1)
        {
            if (context.Register < Params.CompareVal)
            {
                result = true;
            }
        }

        if (Params.GreaterThan == 1)
        {
            if (context.Register > Params.CompareVal)
            {
                result = true;
            }
        }

        return result;
    }
}