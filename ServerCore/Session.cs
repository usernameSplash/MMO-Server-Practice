using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new Object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract void OnReceive(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);

        public void Init(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterReceive();
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }


        #region 네트워크 통신
        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }
        public void Send(string msg)
        {
            byte[] sendBuff = Encoding.UTF8.GetBytes(msg);
            Send(sendBuff);
        }

        void RegisterSend()
        {
            _pendingList.Clear();
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && (args.SocketError == SocketError.Success))
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();
                        Console.WriteLine(_sendQueue.Count);
                        OnSend(args.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterReceive()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnReceiveCompleted(null, _recvArgs);
        }

        void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && (args.SocketError == SocketError.Success))
            {
                try
                {
                    OnReceive(new ArraySegment<byte>(args.Buffer, 0, args.BytesTransferred));
                    RegisterReceive();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnReceiveCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
