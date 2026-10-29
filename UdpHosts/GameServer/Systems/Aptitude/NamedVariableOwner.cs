using GameServer.Entities;

namespace GameServer.Systems.Aptitude;

internal static class NamedVariableOwner
{
    /// <summary>
    ///     The entity whose named variables a command reads or writes. Which entity var_srctype names isn't known, 1 is
    ///     taken as the initiator and the rest as self, the glider abilities run with both being the player.
    /// </summary>
    public static BaseAptitudeEntity Of(Context context, byte varSrctype)
    {
        return (varSrctype == 1 ? context.Initiator : context.Self) as BaseAptitudeEntity;
    }
}
