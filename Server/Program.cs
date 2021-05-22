using System;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buffer = Encoding.UTF8.GetBytes("Buffer1");
            byte[] buffer2 = Encoding.UTF8.GetBytes("Buffer2");
            Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset, buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

            Send(sendBuff);
            Thread.Sleep(2000);
            Disconnect();
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }
        public override int OnReceive(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, 0, buffer.Count);
            Console.WriteLine($"[From Client]: {recvData}");

            return buffer.Count;
        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            Console.WriteLine(ipAddr);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine(host);

            while (true)
            {
                ;
            }

        }
    }
}
