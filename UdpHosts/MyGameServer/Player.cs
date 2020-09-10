using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer {
	public class Player {
		private NetworkClient client;
		public ulong CharacterID { get; protected set; }
		public Entities.Character Character { get; protected set; }
		public ulong EntityID { get { return (CharacterID & 0x00ffffffffffffff); } }

		public Player(NetworkClient netclient) {
			client = netclient;
			Character = new Entities.Character();
		}

		public void Login(ulong charID ) {
			CharacterID = charID;
			Character.Load( charID );
		}
	}
}
