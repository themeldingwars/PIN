using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets.Common;

namespace MyGameServer.Entities {
	public class Character : BaseEntity {
		[Flags]
		public enum CharMovement : ushort {
			None,
			Crouch = 1,
			MovementKeys = 4,
			Sprint = 1 << 4,
			TryingToMove = 1 << 12,
			IsMoving = 2 << 12,
			Unk = 4 << 12,
        }

		public Data.Character CharData { get; set; }
		public System.Numerics.Vector3 Position { get; set; }
		public System.Numerics.Quaternion Rotation { get; set; }
		public System.Numerics.Vector3 Velocity { get; set; }
		public System.Numerics.Vector3 AimDirection { get; set; }
		public CharMovement MovementState { get; set; }
		public bool Alive { get; set; }
		public ushort? LastJumpTime { get; set; }

		public Character( IShard owner, ulong eid ) : base(owner, eid) {
			Position = new System.Numerics.Vector3();
			Rotation = System.Numerics.Quaternion.Identity;
			Velocity = new System.Numerics.Vector3();
			AimDirection = System.Numerics.Vector3.UnitZ;

			Alive = false;
			LastJumpTime = null;

			/*RegisterController(Enums.GSS.Controllers.Character_BaseController);
			RegisterController(Enums.GSS.Controllers.Character_CombatController);
			RegisterController(Enums.GSS.Controllers.Character_MissionAndMarkerController);
			RegisterController(Enums.GSS.Controllers.Character_LocalEffectsController);*/
		}

		public void Load(ulong charID) {
			CharData = Data.Character.Load(charID);
		}
	}
}
