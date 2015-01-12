using ProtoBuf;

namespace Networking.Packets
{
    [ProtoContract]
    public class PacketPing : Packet {

        public PacketPing()
        {
            Type = PacketType.Ping;
        }
    }
}


