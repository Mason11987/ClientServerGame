namespace Networking.Commands
{
    public class HelpCommand : Command, ICommandFactory
    {
        public override CommandResult Execute()
        {
            Console.Output("Commands:");
            foreach (var command in AvailableCommands(NetworkManager.Server != null))
                Console.Output(string.Format("  {0}", command.Description));
            return CommandResult.Success;
        }

        public string CommandName { get { return "Help"; } }
        public string Description { get { return "/help - Displays this message "; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Help; } }


        public Command MakeCommand(string[] args)
        {
            return new HelpCommand();
        }
    }
}
