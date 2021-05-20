using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ServerCore;

namespace DummyClient
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            for (int i = 0; i < 5; i++)
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes($"Hello, World!{i} ");
                Send(sendBuffer);
            }
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }
        public override void OnReceive(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, 0, buffer.Count);
            Console.WriteLine($"[From Server]: {recvData}");
        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            Thread.Sleep(200);
            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                Thread.Sleep(200);
            }
        }
    }
}