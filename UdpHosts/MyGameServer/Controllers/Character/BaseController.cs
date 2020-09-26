using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets.GSS.Character.BaseController;

using Shared.Udp;

namespace MyGameServer.Controllers.Character {
	[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
	public class BaseController : Base {
		public override void Init( INetworkClient client, IPlayer player, IShard shard ) {
			client.NetChans[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(player, shard), player.EntityID, msgEnumType: typeof(Enums.GSS.Character.Events));
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new Packets.GSS.Character.CombatController.KeyFrame( shard ) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new Packets.GSS.Character.LocalEffectsController.KeyFrame( shard ) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new Packets.GSS.Character.MissionAndMarkerController.KeyFrame(shard) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new CharacterLoaded(), player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
		}

		[MessageID((byte)Enums.GSS.Character.Commands.FetchQueueInfo)]
		public void FetchQueueInfo( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
		}

		[MessageID((byte)Enums.GSS.Character.Commands.PlayerReady)]
		public void PlayerReady( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
			player.Ready();
		}

		[MessageID((byte)Enums.GSS.Character.Commands.MovementInput)]
		public void MovementInput( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
			if( packet.BytesRemaining < 64 )
				return;

			var pkt = packet.Read<MovementInput>();

			if( player.CharacterEntity.Alive ) {
				player.CharacterEntity.Position = pkt.Position;
				player.CharacterEntity.Rotation = pkt.Rotation;
				player.CharacterEntity.Velocity = pkt.Velocity;
				player.CharacterEntity.AimDirection = pkt.AimDirection;
				player.CharacterEntity.MovementState = (Entities.Character.CharMovement)pkt.State;
			}
			//Program.Logger.Warning( "Movement State: {0} {1:X4}", player.CharacterEntity.MovementState, pkt.State );

			var resp = new ConfirmedPoseUpdate {
				Key = pkt.Key,
				UnkByte1 = 1,
				UnkByte2 = 0,
				Position = player.CharacterEntity.Position,
				Rotation = player.CharacterEntity.Rotation,
				State = (ushort)player.CharacterEntity.MovementState,
				Velocity = player.CharacterEntity.Velocity,
				UnkUShort1 = pkt.UnkUShort1,
				UnkShort1 = pkt.UnkShort2,
				LastJumpTimer = pkt.LastJumpTimer,
				UnkByte3 = 0,
				NextKey = unchecked((ushort)(pkt.Key + 90))
			};

			client.NetChans[ChannelType.UnreliableGss].SendGSSClass(resp, player.EntityID, msgEnumType: typeof(Enums.GSS.Character.Events));
		}

		[MessageID((byte)Enums.GSS.Character.Commands.SetMovementSimulation)]
		public void SetMovementSimulation( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
		}

		[MessageID((byte)Enums.GSS.Character.Commands.BagInventorySettings)]
		public void BagInventorySettings( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
		}
	}
}