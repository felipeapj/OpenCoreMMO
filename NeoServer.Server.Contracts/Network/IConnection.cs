using System;
using System.Collections.Generic;
using System.Text;

namespace NeoServer.Server.Contracts.Network
{
    public interface IConnection
    {
        void Send(IOutgoingPacket packet);
        void Send(string text);
    }
}
