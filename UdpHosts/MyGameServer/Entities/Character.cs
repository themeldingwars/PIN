using System;
using System.Numerics;

namespace MyGameServer.Entities
{
    public class Character : BaseEntity
    {
        [Flags]
        public enum CharMovement : ushort
        {
            None,
            Crouch = 1,
            MovementKeys = 4,
            Sprint = 1 << 4,
            TryingToMove = 1 << 12,
            IsMoving = 2 << 12,
            Unk = 4 << 12
        }

        public Character(IShard owner, ulong eid) : base(owner, eid)
        {
            Position = new Vector3();
            Rotation = Quaternion.Identity;
            Velocity = new Vector3();
            AimDirection = Vector3.UnitZ;

            Alive = false;
            LastJumpTime = null;

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
        public CharMovement MovementState { get; set; }
        public bool Alive { get; set; }
        public ushort? LastJumpTime { get; set; }

        public void Load(ulong charID)
        {
            CharData = Data.Character.Load(charID);
        }
    }
}