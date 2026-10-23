using System;
using Aero.Protocol;

namespace GameServer.Handlers;

/// <summary>
///     Marks a static handler class with the universal (version agnostic) GSS route it handles:
///     either a namespace route or the route of a view within a namespace.
///     Wire typecodes are resolved against the configured protocol version at dispatch time.
/// </summary>
public class TypecodeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypecodeAttribute"/> class.
    ///     Namespace route: the typecode of the namespace itself (view ordinal -1).
    /// </summary>
    public TypecodeAttribute(int ns)
    {
        Namespace = ns;
        ViewOrdinal = -1;
        TypecodeName = ns switch
        {
            GssTables.Ns.Root => "Root",
            GssTables.Ns.Character => "Character",
            _ => "Namespace" + ns
        };
    }

    public TypecodeAttribute(GssCharacterView view)
    {
        Namespace = GssTables.Ns.Character;
        ViewOrdinal = (int)view;
        TypecodeName = view.ToString();
    }

    public TypecodeAttribute(GssVehicleView view)
    {
        Namespace = GssTables.Ns.Vehicle;
        ViewOrdinal = (int)view;
        TypecodeName = view.ToString();
    }

    public TypecodeAttribute(GssTurretView view)
    {
        Namespace = GssTables.Ns.Turret;
        ViewOrdinal = (int)view;
        TypecodeName = view.ToString();
    }

    public int Namespace { get; }
    public int ViewOrdinal { get; }
    public string TypecodeName { get; }
}
