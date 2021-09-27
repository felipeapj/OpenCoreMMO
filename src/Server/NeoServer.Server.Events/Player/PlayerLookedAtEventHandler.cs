﻿using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player
{
    public class PlayerLookedAtEventHandler
    {
        private readonly IGameServer _game;

        public PlayerLookedAtEventHandler(IGameServer game)
        {
            _game = game;
        }

        public void Execute(IPlayer player, IThing thing, bool isClose)
        {
            if (_game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection) is false) return;

            var text = thing.GetLookText(isClose);

            connection.OutgoingPackets.Enqueue(new TextMessagePacket(text, TextMessageOutgoingType.Description));
            connection.Send();
        }
    }
}