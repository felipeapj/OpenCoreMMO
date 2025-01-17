﻿using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class RemoveTileThingPacket : OutgoingPacket
{
    private readonly byte stackPosition;
    private readonly ITile tile;

    public RemoveTileThingPacket(ITile tile, byte stackPosition)
    {
        this.tile = tile;
        this.stackPosition = stackPosition;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.RemoveAtStackpos);
        message.AddLocation(tile.Location);
        message.AddByte(stackPosition);
    }
}