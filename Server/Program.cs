﻿using System;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = "localhost";
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Console.WriteLine(ipAddr);
            try
            {
                _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            int roomTick = 0;
            while (true)
            {
                int now = System.Environment.TickCount;
                if (roomTick < now)
                {
                    Room.Push(() => Room.Flush());
                    roomTick = now + 250;
                }
            }

        }
    }
}
