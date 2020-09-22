using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using MyGameServer.Data;

using Shared.Udp;

namespace MyGameServer {
	public class NetworkPlayer : NetworkClient, INetworkPlayer {
		public ulong CharacterID { get; protected set; }
		public Entities.Character CharacterEntity { get; protected set; }
		public IPlayer.PlayerStatus Status { get; protected set; }
		public Zone CurrentZone { get; protected set; }

		private double lastKeyFrame;

		public NetworkPlayer( IPEndPoint ep, uint socketID )
			: base( ep, socketID ) {
			CharacterEntity = null;
			Status = IPlayer.PlayerStatus.Connecting;
		}

		public void Init(IShard shard) {
			Init( this, shard, shard );
		}

		public void Login( ulong charID ) {
			CharacterID = charID;
			CharacterEntity = new Entities.Character(AssignedShard, charID & 0xffffffffffffff00);
			CharacterEntity.Load( charID );
			Status = IPlayer.PlayerStatus.LoggedIn;

			// WelcomeToTheMatrix
			var wel = new Packets.Matrix.WelcomeToTheMatrix {
				InstanceID = AssignedShard.InstanceID
			};
			NetChans[ChannelType.Matrix].SendClass( wel );

			EnterZone( Test.DataUtils.GetZone( 448 ) );
		}

		public void EnterZone(Zone z) {
			CurrentZone = z;
			CharacterEntity.Position = CurrentZone.POIs["watchtower"];

			// EnterZone
			var enterZone = new Packets.Matrix.EnterZone();
			enterZone.InstanceId = AssignedShard.InstanceID;
			enterZone.ZoneId = CurrentZone.ID;
			enterZone.ZoneTimestamp = CurrentZone.Timestamp;
			enterZone.PreviewModeFlag = 0;
			enterZone.ZoneOwner = "r5_exec";
			enterZone.Unk1 = new byte[] { 0x5F, 0x4C, 0xF5, 0xC9, 0x01, 0x00 };
			enterZone.HotfixLevel = 0;
			enterZone.MatchId = 0;
			enterZone.Unk2 = new byte[] { 0x00, 0x5e, 0xdb, 0xe2, 0x63 };
			enterZone.ZoneName = CurrentZone.Name;
			enterZone.Unk3 = 0;
			enterZone.Unk_ZoneTime = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x41, 0x7A, 0x7D, 0x65, 0x3F };
			enterZone.Unk4 = 0x0005411D95E79428u;
			enterZone.Unk5 = 0x000540F013D65BEAu;
			enterZone.Unk6 = 0x3FF0000000000000u;
			enterZone.Unk7 = 0x0000000000000000u;
			enterZone.Unk8 = 0x0000000000000000u;
			enterZone.Unk9 = 0;
			enterZone.SpectatorModeFlag = 0;

			NetChans[ChannelType.Matrix].SendClass( enterZone );
			Status = IPlayer.PlayerStatus.Loading;

			lastKeyFrame = AssignedShard.CurrentTick;
		}

		public void Ready() {
			Status = IPlayer.PlayerStatus.Playing;
		}

		public void Tick( double deltaTime, double currTime ) {
			// TODO: Implement FSM here to move player thru log in process to connecting to a shard to playing
			if( Status == IPlayer.PlayerStatus.Connected ) {
				Status = IPlayer.PlayerStatus.LoggingIn;
			} else if( Status == IPlayer.PlayerStatus.Loading ) {
				
			} else if( Status == IPlayer.PlayerStatus.Playing ) {
				if( AssignedShard.CurrentTick - lastKeyFrame > 0.5 ) {
					//NetChans[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(this, this), this.InstanceID, msgEnumType: typeof(Enums.GSS.Character.Events));
				}
			}
		}
	}
}
