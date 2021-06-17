using System;
using System.Collections.Generic;
using System.Text;
using DummyClient;
using ServerCore;

class PacketHandler
{
    public static void S_ChatHandler(Session session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession s = session as ServerSession;

        // if (chatPacket.playerID == 1)
        // Console.WriteLine(chatPacket.chat);
    }
}
