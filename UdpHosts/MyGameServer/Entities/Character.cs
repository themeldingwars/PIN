using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Entities {
	public class Character : BaseEntity {
		public Data.Character CharData { get; set; }
		public System.Numerics.Vector3 Position { get; set; }
		public Character( IShard owner, ulong eid ) : base(owner, eid) {
			Position = new System.Numerics.Vector3();

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
