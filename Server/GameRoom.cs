using System;
using System.Collections.Generic;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerID = session.SessionID;
            packet.chat = $"{chat}, packetID: {packet.playerID}";
            ArraySegment<byte> segment = packet.Write();

            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                {
                    s.Send(segment);
                }
            }
        }
    }
}