using GameServer.StaticDB.Records;

namespace GameServer.Systems.Aptitude.Commands;

/// <summary>
///  Game client design has this base class use a 6th method.
///  It is assumed that all implementors will have params with regop and 11 values.
///  The execute command then runs the 6th method to get the index to use.
///  It then gets that reg value from the params, along with regop, and applies this to register.
///  The result of the register op is stored in the reigster.
///  The result of the command is always pass.
/// </summary>
public abstract class LoadRegisterBaseCommand : BaseRegisterOpCommand
{
    protected LoadRegisterBaseCommand(ICommandDef def)
        : base(def)
    {
    }
}