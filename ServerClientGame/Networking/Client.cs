using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;
using Networking.Commands;
using Networking.Packets;
using ProtoBuf;

namespace Networking
{
    /// <summary>
    /// Accepts game data sent from remote Server object for remote Game and sends client data back.
    /// </summary>
    public class Client : GameComponent
    {
        public Server Server { get { return NetworkManager.Server; } }
        public CustomConsole Console { get { return NetworkManager.Console; }}
        

        public TcpClient tcpClient;

        public Thread commThread;
        public NetworkStream CommStream;
        public double lastServerPing;
        public GameTime lastGameTime;
        public Dictionary<string, string> Settings = new Dictionary<string, string> { { "showping", "no" }, { "showsuccess", "no" } };

        public bool hasLocalServer { get { return Server != null; } }

        public bool Alive { get; set; }
        
        public Client(Game game)
            : base(game)
        {
                
                
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            tcpClient = new TcpClient();
            Alive = true;

            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            lastGameTime = gameTime;
            if (hasLocalServer && !tcpClient.Connected)
                Connect(Server);

            if (Console != null)
            {
                Console.Enabled = Alive;
                if (Console.hasInput)
                    RespondToConsoleInput(Console.ReadInput());
            }

            base.Update(gameTime);
        }

        public void Connect(Server server)
        {
            ExecuteCommand(new ConnectCommand { IP = "127.0.0.1", Port = server.Port });
        }

        public void HandleServerComm()
        {

            CommStream = tcpClient.GetStream();

            while (tcpClient.Connected && (lastGameTime == null || lastGameTime.TotalGameTime.TotalSeconds - lastServerPing < (Server.PingRate * 2 + 1)))
            {
                try
                {
                    if (CommStream.DataAvailable)
                        HandleServerData();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

            }

            if (!tcpClient.Connected) 
                return;
            ExecuteCommand(new DisconnectCommand());
            Console.Output("Server Lost");
        }

        private void HandleServerData()
        {
            var p = Serializer.DeserializeWithLengthPrefix<Packet>(CommStream, PrefixStyle.Base128);

            lastServerPing = lastGameTime.TotalGameTime.TotalSeconds;
            if (p is PacketPing)
            {
                Send(new PacketPing()); //Return Ping
                if (Settings["showping"] != "yes") 
                    return;
                if (!hasLocalServer)
                    Console.Output("Server Ping");
                else
                    Console.Output("Server Ping");
            }
            else if (p is PacketConsoleCommand)
            {
                var packet = p as PacketConsoleCommand;
                HandleServerCommand(packet.CommandType, packet.Arguments);
            }
        }

        private void HandleServerCommand(ConsoleCommandType commandType, string[] args)
        {
            switch (commandType)
            {
                case ConsoleCommandType.Text:
                    if (!hasLocalServer)
                        Console.Output(String.Format("{0}: {1}", args[0], args[1]));
                    break;
                case ConsoleCommandType.Say:
                    if (!hasLocalServer)
                        Console.Output(string.Format("[{0}]: {1}", args[0], args[1]));
                    break;
                case ConsoleCommandType.Disconnect:
                    ExecuteCommand(new DisconnectCommand());
                    break;
                default:
                    Console.Output("Unexpected Command from Server: " + commandType);
                    break;
            }
        }

        public void Send(Packet packet)
        {
            if (tcpClient.Connected)
            {
                try
                {
                    Serializer.SerializeWithLengthPrefix(CommStream, packet, PrefixStyle.Base128);
                }
                catch (IOException)
                {
                    ExecuteCommand(new DisconnectCommand());
                }
            }
            else
            {
                Console.Output("Not Connected to server");
            }
           
        }

        public void ExecuteCommand(Command command)
        {
            var result = command.Execute();

            if (result == CommandResult.Failed)
                Console.Output("*Command Failed*");
            else if (result == CommandResult.NotImplemented)
                Console.Output("*Command Not Implemented*");
            else if (result == CommandResult.Success && Settings["showsuccess"] == "yes")
                Console.Output("*Command Succeeded*");
        }

        private void RespondToConsoleInput(string input)
        {
            ExecuteCommand(Console.GetCommand(input));
        }
    }
}

