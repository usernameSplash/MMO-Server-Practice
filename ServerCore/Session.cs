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

        ReceiveBuffer _recvBuffer = new ReceiveBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract int OnReceive(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);

        public void Init(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
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
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
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
                        OnSend(_sendArgs.BytesTransferred);

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
            _recvBuffer.Clear();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

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
                    // Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                    int processLength = OnReceive(_recvBuffer.ReadSegment);
                    if (processLength < 0 || _recvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

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
