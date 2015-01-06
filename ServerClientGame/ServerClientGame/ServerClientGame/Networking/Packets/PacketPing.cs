using ProtoBuf;

namespace ServerClientGame.Networking.Packets
{
    [ProtoContract]
    public class PacketPing : Packet {

        public PacketPing()
        {
            Type = PacketType.Ping;
        }
    }
}


