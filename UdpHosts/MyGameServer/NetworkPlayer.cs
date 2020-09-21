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
			CharacterEntity = new Entities.Character();
			Status = IPlayer.PlayerStatus.Connecting;
		}

		public void Init(IShard shard) {
			Init( this, shard, shard );
		}

		public void Login( ulong charID ) {
			CharacterID = charID;
			CharacterEntity.Load( charID );
			Status = IPlayer.PlayerStatus.LoggedIn;

			// WelcomeToTheMatrix
			var wel = new Packets.Matrix.WelcomeToTheMatrix(AssignedShard.InstanceID, 0, 0);
			NetChans[ChannelType.Matrix].Send( wel );

			EnterZone( Test.DataUtils.GetZone( 448 ) );

			/*var syncReq = new Packets.Matrix.SynchronizationRequest() {
				Unk = new byte[] {
					0xb4, 0x50, 0xaf, 0xa8, 0x1c, 0xa5, 0x45, 0x15, 0x00, 0x0d, 0x0c, 0x0c, 0xcc, 0xcc, 0xc6, 0x1b,
					0xcd, 0x05, 0xc6, 0xcc, 0xcc, 0xcc, 0x21, 0xe3, 0x9e, 0x37, 0x71, 0x0d, 0x3f, 0xb9, 0xb4, 0xd6,
					0x9c, 0x20, 0x36, 0xf7, 0x4a, 0x01
				}
			};

			Channels[ChannelType.Matrix].Send(syncReq);*/
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
			enterZone.Unk4 = 0x2894E7951D410500u;
			enterZone.Unk5 = 0xEA5BD613F0400500u;
			enterZone.Unk6 = 0x000000000000F03Fu;
			enterZone.Unk7 = 0x0000000000000000u;
			enterZone.Unk8 = 0x0000000000000000u;
			enterZone.Unk9 = 0;
			enterZone.SpectatorModeFlag = 0;

			NetChans[ChannelType.Matrix].Send( enterZone );
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
