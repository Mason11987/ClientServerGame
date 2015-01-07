using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Microsoft.Xna.Framework;
using ServerClientGame;
using ServerClientGame.Networking.Packets;
using ProtoBuf;
using System.IO;
using System.Diagnostics;
using ServerClientGame.Commands;

namespace ServerClientGame.Networking
{
    /// <summary>
    /// Accepts game data sent from remote Server object for remote Game and sends client data back.
    /// </summary>
    public class Client : GameComponent
    {
        public TcpClient tcpClient;
        private CustomConsole console;
        public Game1 GameRef;
        public Thread commThread;
        public NetworkStream CommStream;
        public double lastServerPing;
        public GameTime lastGameTime;
        public Dictionary<string, string> Settings = new Dictionary<string, string>() { { "showping", "no" }, { "showsuccess", "no" } };

        public bool hasLocalServer { get { return GameRef.Server != null; } }

        public bool Alive { get; set; }
        
        public Client(Game1 game)
            : base(game)
        {
            GameRef = game;
            if (!hasLocalServer)
            {
                console = new CustomConsole(game, this);
                Command.console = console;
                game.Components.Add(console);
            }
            else
            {
                GameRef.Server.console.client = this;
                this.console = GameRef.Server.console;
            }
                
                
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
                Connect(GameRef.Server);

            if (console != null)
            {
                console.Enabled = Alive;
                if (console.hasInput)
                    RespondToConsoleInput(console.ReadInput());
            }

            base.Update(gameTime);
        }

        public void Connect(Server server)
        {
            ExecuteCommand(new ConnectCommand() { IP = "127.0.0.1", Port = server.Port });
        }

        public void HandleServerComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;

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

            if (tcpClient.Connected)
            {
                ExecuteCommand(new DisconnectCommand());
                Console.WriteLine("Server Lost");
            }
        }

        private void HandleServerData()
        {
            Packet p;
            p = Serializer.DeserializeWithLengthPrefix<Packet>(CommStream, PrefixStyle.Base128);

            lastServerPing = lastGameTime.TotalGameTime.TotalSeconds;
            if (p is PacketPing)
            {
                Send(new PacketPing()); //Return Ping
                if (Settings["showping"] == "yes")
                {
                    if (!hasLocalServer)
                        console.Output("Server Ping");
                    else
                        GameRef.Server.console.Output("Server Ping");
                }
            }
            else if (p is PacketConsoleCommand)
            {
                PacketConsoleCommand packet = p as PacketConsoleCommand;
                HandleServerCommand(packet.CommandType, packet.Arguments);
            }
        }

        private void HandleServerCommand(ConsoleCommandType commandType, string[] args)
        {
            switch (commandType)
            {
                case ConsoleCommandType.Text:
                    if (!hasLocalServer)
                        console.Output(String.Format("{0}: {1}", args));
                    break;
                case ConsoleCommandType.Say:
                    if (!hasLocalServer)
                        console.Output(string.Format("[{0}]: {1}", args[0], args[1]));
                    break;
                case ConsoleCommandType.Disconnect:
                    ExecuteCommand(new DisconnectCommand());
                    break;
                default:
                    console.Output("Unexpected Command from Server: " + commandType.ToString());
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
                console.Output("Not Connected to server");
            }
           
        }

        public void ExecuteCommand(Command command)
        {
            CommandResult result = command.Execute();

            if (result == CommandResult.Failed)
                console.Output("*Command Failed*");
            else if (result == CommandResult.NotImplemented)
                console.Output("*Command Not Implemented*");
            else if (result == CommandResult.Success && Settings["showsuccess"] == "yes")
                console.Output("*Command Succeeded*");
        }

        private void RespondToConsoleInput(string input)
        {
            ExecuteCommand(console.GetCommand(input));
        }
    }
}

