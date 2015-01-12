using ProtoBuf;

namespace Networking.Packets
{
    public enum PacketType : byte 
    {
        Data,
        Ping,
        ConsoleCommand
    }

    [ProtoContract]
    [ProtoInclude(7, typeof(PacketPing))]
    [ProtoInclude(8, typeof(PacketConsoleCommand))]
    public abstract class Packet
    {
        [ProtoMember(1)]
        public PacketType Type { get; set; }


        //public Packet(PacketType packetType, byte[] data)
        //{
        //    Type = packetType;
        //    //Data = data;
        //}
    }
}


