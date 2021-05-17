using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();
        static void _onAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Init(clientSocket);

                string msg = "Welcome to Kyeongmin's server!";
                session.Send(msg);

                Thread.Sleep(1000);

                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Console.WriteLine(host);
            _listener.Init(endPoint, _onAcceptHandler);

            while (true)
            {
                ;
            }

        }
    }
}