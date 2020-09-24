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

		ulong CharacterID { get; }
		ulong EntityID { get { return (CharacterID & 0xffffffffffffff00); } } // Ignore last byte
		Entities.Character CharacterEntity { get; }
		PlayerStatus Status { get; }
		Zone CurrentZone { get; }

		void Init(IShard shard);

		void Login( ulong charID );
		void Ready();

		void Tick( double deltaTime, double currTime );
	}
}
