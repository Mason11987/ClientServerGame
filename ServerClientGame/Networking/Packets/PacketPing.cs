using ProtoBuf;

namespace Networking.Networking.Packets
{
    [ProtoContract]
    public class PacketPing : Packet {

        public PacketPing()
        {
            Type = PacketType.Ping;
        }
    }
}


