using System;
using System.Collections.Generic;
using System.Text;

using MyGameServer.Packets.GSS.Character.BaseController;

using Shared.Udp;

namespace MyGameServer.Controllers.Character {
	[ControllerID( Enums.GSS.Controllers.Character_BaseController )]
	public class BaseController : Base {
		public override void Init( INetworkClient client, IPlayer player, IShard shard ) {
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( Test.GSS.Character.BaseController.KeyFrame.Test( player, shard ), player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new Packets.GSS.Character.CombatController.KeyFrame( shard ) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new Packets.GSS.Character.LocalEffectsController.KeyFrame( shard ) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new Packets.GSS.Character.MissionAndMarkerController.KeyFrame( shard ) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
			client.NetChans[ChannelType.ReliableGss].SendGSSClass( new CharacterLoaded(), player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );
		}

		[MessageID( (byte)Enums.GSS.Character.Commands.FetchQueueInfo )]
		public void FetchQueueInfo( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
		}

		[MessageID( (byte)Enums.GSS.Character.Commands.PlayerReady )]
		public void PlayerReady( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
			player.Ready();
		}

		[MessageID( (byte)Enums.GSS.Character.Commands.MovementInput )]
		public void MovementInput( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
			if( packet.BytesRemaining < 64 )
				return;

			var pkt = packet.Read<MovementInput>();

			if( !player.CharacterEntity.Alive )
				return;

			player.CharacterEntity.Position = pkt.Position;
			player.CharacterEntity.Rotation = pkt.Rotation;
			player.CharacterEntity.Velocity = pkt.Velocity;
			player.CharacterEntity.AimDirection = pkt.AimDirection;
			player.CharacterEntity.MovementState = (Entities.Character.CharMovement)pkt.State;

			if( player.CharacterEntity.LastJumpTime == null )
				player.CharacterEntity.LastJumpTime = pkt.LastJumpTimer;

			//Program.Logger.Warning( "Movement Unk1: {0:X4} {1:X4} {2:X4} {3:X4} {4:X4}", pkt.UnkUShort1, pkt.UnkUShort2, pkt.UnkUShort3, pkt.UnkUShort4, pkt.LastJumpTimer );

			var resp = new ConfirmedPoseUpdate {
				ShortTime = pkt.ShortTime,
				UnkByte1 = 1,
				UnkByte2 = 0,
				Position = player.CharacterEntity.Position,
				Rotation = player.CharacterEntity.Rotation,
				State = (ushort)player.CharacterEntity.MovementState,
				Velocity = player.CharacterEntity.Velocity,
				UnkUShort1 = pkt.UnkUShort3,
				UnkUShort2 = pkt.UnkUShort4,    // Somehow affects gravity
				LastJumpTimer = pkt.LastJumpTimer,
				UnkByte3 = 0,
				NextShortTime = unchecked((ushort)(pkt.ShortTime + 90))
			};

			client.NetChans[ChannelType.UnreliableGss].SendGSSClass( resp, player.EntityID, msgEnumType: typeof( Enums.GSS.Character.Events ) );

			if( player.CharacterEntity.LastJumpTime.HasValue && pkt.LastJumpTimer > player.CharacterEntity.LastJumpTime.Value )
				player.Jump();
		}

		[MessageID( (byte)Enums.GSS.Character.Commands.SetMovementSimulation )]
		public void SetMovementSimulation( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
		}

		[MessageID( (byte)Enums.GSS.Character.Commands.BagInventorySettings )]
		public void BagInventorySettings( INetworkClient client, IPlayer player, ulong EntityID, Packets.GamePacket packet ) {
		}
	}
}