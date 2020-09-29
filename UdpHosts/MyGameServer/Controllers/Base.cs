using System;
using System.Collections.Generic;
using System.Linq;

using MyGameServer.Extensions;
using MyGameServer.Packets;

using Serilog;

namespace MyGameServer.Controllers {
	public abstract class Base {
		protected ILogger Log { get { return Program.Logger; } }
		public Enums.GSS.Controllers ControllerID { get; private set; }

		protected Base() {
			try {
				ControllerID = GetType().GetAttribute<ControllerIDAttribute>().ControllerID;
			} catch {
				throw new MissingMemberException(this.GetType().FullName, "Missing required ControllerID attribute");
			}
		}

		public abstract void Init( INetworkClient client, IPlayer player, IShard shard );

		public void HandlePacket(INetworkClient client, IPlayer player, ulong EntityID, byte MsgID, GamePacket packet) {
			var method = ReflectionUtils.FindMethodsByAttribute<MessageIDAttribute>(this).Where(( mi ) => mi.GetAttribute<MessageIDAttribute>().MsgID == MsgID).FirstOrDefault();

			if( method == null ) {
				Log.Verbose("---> Unrecognized MsgID for GSS Packet; Controller = {0} Entity = 0x{1:X8} MsgID = {2}!", ControllerID, EntityID, MsgID);
				Program.Logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
				return;
			}

			_ = method.Invoke(this, new object[] { client, player, EntityID, packet });
		}
	}
}
