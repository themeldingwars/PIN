using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Data {
	public class Character {
		public static Character Load(ulong charID) {
			// TODO: Pull from database or w/e
			return new Character();
		}

		protected Character() {

		}
	}
}
