namespace MyGameServer.Data {
	public class WeaponModule {
		public uint ID { get; set; }
		public byte[] UnkBytes { get; protected set; }

		public WeaponModule(uint id) {
			ID = id;
			UnkBytes = new byte[] { 0xff, 0x00, 0x00 };
		}

		public static implicit operator Packets.GSS.Character.BaseController.KeyFrame.WeaponModule( WeaponModule o ) {
			return new Packets.GSS.Character.BaseController.KeyFrame.WeaponModule {
				ID = o.ID,
				Unk = o.UnkBytes
			};
		}
	}
}