using Networking.Packets;

namespace Networking.Commands
{
    class IdentifyCommand : Command, ICommandFactory
    {
        public string Name { get; set; }
        public RemoteClient RemoteClientToIdentify { get; set; }

        public override CommandResult Execute()
        {
            if (Server == null)
                Client.Send(new PacketConsoleCommand(CommandType, new[] { Name }));
            else if (RemoteClientToIdentify != null)
            {
                if (Server.Clients.ContainsKey(Name))
                {
                    Console.Output("Failed to update identifation of " + RemoteClientToIdentify.IP);
                    RemoteClientToIdentify.Send(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", "Identify Failed: A user already exists with the name: " + Name }));
                    return CommandResult.Failed;
                }

                RemoteClient outRemoteClient;
                if (Server.Clients.TryRemove(RemoteClientToIdentify.Name, out outRemoteClient))
                {
                    var oldName = RemoteClientToIdentify.Name;
                    RemoteClientToIdentify.Name = Name;
                    Server.Clients.TryAdd(RemoteClientToIdentify.Name, RemoteClientToIdentify);

                    Console.Output("Client " + RemoteClientToIdentify.IP + " - Identified as " + Name);
                    Server.Broadcast(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", " * " + oldName + " identified as " + RemoteClientToIdentify.Name + " *" }));

                    return CommandResult.Success;
                }
                Console.Output("Failed to update identifation of " + RemoteClientToIdentify.IP);
                return CommandResult.Failed;
            }
            else if (Server.LocalClient != null)
                Server.LocalClient.Send(new PacketConsoleCommand(CommandType, Name));

            return CommandResult.Success;
        }

        public string CommandName { get { return "Identify"; } }
        public string Description { get { return "/identify NAME - Identify client as NAME"; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Identify; } }

        public Command MakeCommand(string[] args)
        {
            if (args.Length > 2) throw new UnexpectedCommandArgumentException("Identify failed: Name contained spaces");
            if (args.Length == 1) throw new UnexpectedCommandArgumentException("Identify failed: No Name supplied");

            return new IdentifyCommand { Name = args[1]};
        }

        public Command MakeCommand(string[] args, RemoteClient remoteClient)
        {
            var command = MakeCommand(new[] {CommandName.ToLower(), args[0]});

            if (command is IdentifyCommand)
                (command as IdentifyCommand).RemoteClientToIdentify = remoteClient;

            return command;
        }
    }
}
