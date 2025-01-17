﻿using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands
{
    public class TeleportToTempleCommand : CommandSpell
    {
        public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
        {
            error = InvalidOperation.NotEnoughRoom;

            var playerName = Params?.Length > 0 ? Params[0].ToString() : actor.Name;
            var gameManager = Fabric.Return<IGameCreatureManager>();

            if (!gameManager.TryGetPlayer(playerName, out IPlayer player))
            {
                error = InvalidOperation.PlayerNotFound;
                return false;
            }

            var location = new Location(player.Town.Coordinate);
            player.TeleportTo(location);
            return true;
        }
    }
}