using Networking.Commands;
using ProtoBuf;

namespace Networking.Packets
{

    [ProtoContract]
    class PacketConsoleCommand : Packet
    {

        [ProtoMember(1)]
        public ConsoleCommandType CommandType { get; set; }
        [ProtoMember(2)]
        public string[] Arguments {get;set;}



        public PacketConsoleCommand()
        {
            Type = PacketType.ConsoleCommand;
        }

        public PacketConsoleCommand(ConsoleCommandType type, params string[] args)
        {
            Type = PacketType.ConsoleCommand;
            CommandType = type;
            Arguments = args;
        }
    }
}
