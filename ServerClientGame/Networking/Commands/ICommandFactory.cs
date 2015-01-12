namespace Networking.Commands
{
    public interface ICommandFactory
    {
        string CommandName { get; }
        string Description { get; }
        ConsoleCommandType CommandType { get; }

        Command MakeCommand(string[] arguments);
    }
}
