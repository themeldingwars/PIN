using System.Numerics;

namespace GameServer.Entities.Character;

public class Character : BaseEntity
{
    public Character(IShard owner, ulong eid)
        : base(owner, eid)
    {
        Position = new Vector3();
        Rotation = Quaternion.Identity;
        Velocity = new Vector3();
        AimDirection = Vector3.UnitZ;

        Alive = false;
        TimeSinceLastJump = null;

        /*RegisterController(Enums.GSS.Controllers.Character_BaseController);
        RegisterController(Enums.GSS.Controllers.Character_CombatController);
        RegisterController(Enums.GSS.Controllers.Character_MissionAndMarkerController);
        RegisterController(Enums.GSS.Controllers.Character_LocalEffectsController);*/
    }

    public Data.Character CharData { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AimDirection { get; set; }
    public bool Alive { get; set; }
    public ushort? TimeSinceLastJump { get; set; }
    internal MovementStateContainer MovementStateContainer { get; set; } = new();

    public void Load(ulong characterId)
    {
        CharData = Data.Character.Load(characterId);
    }
}