using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;


namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionID { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            Program.Room.Push(
                () => Program.Room.Enter(this)
            );
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            if (Room != null)
            {
                GameRoom room = Room;
                room.Push(
                    () => room.Leave(this)
                );

                Room = null;
            }

            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnReceivePacket(this, buffer);
        }
    }
}