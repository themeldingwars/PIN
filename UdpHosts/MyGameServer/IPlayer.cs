using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Data;

namespace MyGameServer {
	public interface IPlayer {
		public enum PlayerStatus {
			Invalid = -1,
			Unknown = 0,
			Connecting = 1,
			Connected,
			LoggingIn,
			LoggedIn,
			Loading,

			Playing = 999
		}

		public ulong CharacterID { get; }
		public Entities.Character CharacterEntity { get; }
		public ulong EntityID { get { return (CharacterID & 0xffffffffffffff00); } } // Ignore last byte
		public PlayerStatus Status { get; }
		public Zone CurrentZone { get; }

		public void Init(IShard shard);

		public void Login( ulong charID );
		void Ready();

		public void Tick( double deltaTime, double currTime );
	}
}
