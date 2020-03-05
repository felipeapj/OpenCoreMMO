﻿using NeoServer.Networking.Packets;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Handlers;
using NeoServer.Server.Handlers.Authentication;
using NeoServer.Server.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoServer.Networking.Protocols
{
    public class LoginProtocol : OpenTibiaProtocol
    {
        public override bool KeepConnectionOpen => false;
        // private Func<GameIncomingPacketType, IEventHandler> _handlerFactory;
        private Func<NetworkMessage, IncomingPacket> _packetFactory;
        public LoginProtocol(Func<NetworkMessage, IncomingPacket> packetFactory)
        {
            _packetFactory = packetFactory;
        }

        public override void ProcessMessage(object sender, ConnectionEventArgs args)
        {
            var packet = _packetFactory(args.Connection.InMessage);
            args.Connection.SetXtea(packet.Xtea);

            var eventArgs = new ServerEventArgs(packet.Model, args.Connection, packet.SuccessFunc);

            packet.OnIncomingPacket(args.Connection, eventArgs);   
        }
    }
}
