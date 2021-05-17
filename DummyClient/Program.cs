using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while (true)
            {
                // Socket Setting
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");

                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuffer = Encoding.UTF8.GetBytes($"Hello, World!{i} ");
                        int sendBytes = socket.Send(sendBuffer);
                    }

                    byte[] receiveBuffer = new byte[1024];
                    int receiveBytes = socket.Receive(receiveBuffer);
                    string receiveData = Encoding.UTF8.GetString(receiveBuffer, 0, receiveBytes);
                    Console.WriteLine($"[From Server]: {receiveData}");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(200);
            }
        }
    }
}