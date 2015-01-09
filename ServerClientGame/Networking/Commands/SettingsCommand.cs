using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Networking.Commands
{
    class SettingsCommand : Command, ICommandFactory
    {
        public string Setting { get; set; }
        public IEnumerable<string> Value { get; set; }
        public bool Display { get; set; }

        public override CommandResult Execute()
        {
            //var args = new string[] { Name, string.Join(" ", Message) };
            //if (Server == null)
            //    Client.Send(new PacketConsoleCommand(CommandType, args));
            //else
            //    Server.LocalClient.Send(new PacketConsoleCommand(CommandType, args));

            Dictionary<string, string> settings;
            if (Server == null)
                settings = Client.Settings;
            else
                settings = Server.Settings;
            

            if (Display)
            {
                Console.Output("Settings");
                foreach (var setting in settings)
                    Console.Output(string.Format("  {0} - {1}", setting.Key, setting.Value));
                return CommandResult.Success;
            }

            if (Value.Count() > 1)
                return CommandResult.Failed;

            settings[Setting] = Value.First();    

            return CommandResult.Success;
        }

        public string CommandName { get { return "Settings"; } }
        public string Description { get { return "/settings [NAME VALUE]  - change setting Name to VALUE";  } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Say; } }

        public Command MakeCommand(string[] args)
        {
            if (args.Length == 1)
                return new SettingsCommand() { Display = true };

            Dictionary<string, string> settings;
            if (Server == null)
                settings = Client.Settings;
            else
                settings = Server.Settings;

            if (args.Length > 1)
                if (!settings.ContainsKey(args[1])) throw new UnexpectedCommandArgumentException("Settings change failed, no setting named: " + args[1]);
            if (args.Length == 2)
            {
                if (settings[args[1]] != "no" && settings[args[1]] != "yes") throw new UnexpectedCommandArgumentException("Settings change failed, setting is not a toggle: " + args[1]);
                return new SettingsCommand() {Setting = args[1], Value = new string[] {settings[args[1]] == "yes" ? "no" : "yes"} };
            }
            else
                return new SettingsCommand() { Setting = args[1], Value = args.Skip(2)};
        }
    }
}