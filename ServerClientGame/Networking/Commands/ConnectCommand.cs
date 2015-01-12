using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Networking.Commands
{
    class ConnectCommand : Command, ICommandFactory
    {
        public string IP { get; set; }
        public int Port { get; set; }

        public override CommandResult Execute()
        {
            return Connect(IP, Port);
        }

        public CommandResult Connect(string ip, int port)
        {
            if (Client.tcpClient.Connected)
            {
                Console.Output("Already connected to server.  Disconnect first");
                return CommandResult.Failed;
            }

            var serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            try
            {

                Client.tcpClient.Connect(serverEndPoint);

                Client.lastServerPing = Client.lastGameTime.TotalGameTime.TotalSeconds;
                Client.commThread = new Thread(client => Client.HandleServerComm());
                Client.commThread.Start(Client.tcpClient);
                if (!Client.hasLocalServer)
                    Console.Output("Connected to server at: " + ip + ":" + port);
                else
                    NetworkManager.Server.LocalClient = Client;
                
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                if (e is ThreadStartException ||
                    e is SocketException)
                {
                    if (!Client.hasLocalServer)
                        Console.Output("Couldn't Connect to server at: " + ip + ":" + port);
                    return CommandResult.Failed;
                }
                if (e is InvalidOperationException)
                {

                    throw;
                }
                throw;
            }
        }


        public string CommandName { get { return "Connect"; } }
        public string Description { get { return "/connect [ip:port|ip port]- Connects to the server, optionally supply ip and port "; } }
        public ConsoleCommandType CommandType { get { return ConsoleCommandType.Connect; } }

        public Command MakeCommand(string[] args)
        {
            if (args.Length > 3) throw new UnexpectedCommandArgumentException("Connect command failed, supply ip and port only");

		    var ip = "127.0.0.1";
            var port = 3000;
            try 
	        {
                switch (args.Length)
                {
                    case 2:
                        if (args[1].Contains(':'))
                        {
                            ip = args[1].Split(':')[0];
                            port = int.Parse(args[1].Split(':')[1]);
                        }
                        else
                            ip = args[1];
                        break;
                    case 3:
                        ip = args[1];
                        port = int.Parse(args[2]);
                        break;
                }
	        }
	        catch (Exception)
	        {
                throw new UnexpectedCommandArgumentException("Connect command failed, could not parse ip and port");
	        }

            return new ConnectCommand {IP = ip, Port = port};
        }
    }
}
