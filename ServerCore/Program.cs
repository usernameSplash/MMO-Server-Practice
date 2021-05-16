using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
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

            Console.WriteLine(host);

            // Socket Setting
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Socket에 ip 주소 연동
                listenSocket.Bind(endPoint);

                // 통신 시작
                // backlog : 최대 대기 수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Listening...");

                    // Client 접속
                    Socket clientSocket = listenSocket.Accept();

                    // Client 요청 받기
                    byte[] receiveBuffer = new byte[1024];
                    int receiveBytes = clientSocket.Receive(receiveBuffer);
                    string receiveData = Encoding.UTF8.GetString(receiveBuffer, 0, receiveBytes);
                    Console.WriteLine($"[From Client]: {receiveData} {DateTime.Now}");

                    // Client에게 보내기
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to Kyeongmin's server!");
                    clientSocket.Send(sendBuffer);

                    // Client 퇴장
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}