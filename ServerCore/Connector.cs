using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        // 여러 개의 소켓이 연결하길 원할 수 있으므로 멤버변수 대신 이벤트 방식을 사용한다.
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectCompleted;
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                RegisterConnect(args);
            }
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);
            if (pending == false)
                OnConnectCompleted(null, args);
        }

        void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Init(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectComplted Fail: {args.SocketError}");
            }
        }
    }
}