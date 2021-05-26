using System;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;


namespace Server
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetID;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerID;
        public string name;

        public PlayerInfoReq()
        {
            packetID = (ushort)PacketID.PlayerInfoReq;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort count = 0;
            bool success = true;

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetID);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerID);
            count += sizeof(long);

            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);

            Encoding.Unicode.GetByteCount(this.name);
            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            // ushort size = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);
            // ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += sizeof(ushort);

            this.playerID = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2,
    }

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            // Packet packet = new Packet() { size = 4, packetId = 7 };

            // ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            // byte[] buffer = BitConverter.GetBytes(packet.size);
            // byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            // Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            // Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            // ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

            // Send(sendBuff);
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);

                        Console.WriteLine($"PlayerInfoReq: {p.playerID} {p.name}");
                    }
                    break;
            }

            Console.WriteLine($"ReceivePacketID: {id}, Size: {size} ");
        }
    }
}