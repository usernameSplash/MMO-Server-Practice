using System;
using System.Collections.Generic;

namespace DummyClient
{
    class SessionManager
    {
        static SessionManager _session = new SessionManager();

        public static SessionManager Instance { get { return _session; } }

        List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();

        static int count = 0;

        public ServerSession Generate()
        {
            lock (_lock)
            {

                ServerSession session = new ServerSession();
                _sessions.Add(session);
                return session;
            }
        }

        public void SendForEach()
        {
            foreach (ServerSession session in _sessions)
            {
                C_Chat packet = new C_Chat();
                packet.chat = $"Hello, Server!";

                ArraySegment<byte> segment = packet.Write();
                session.Send(segment);
            }
        }
    }
}