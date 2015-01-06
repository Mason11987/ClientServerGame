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

namespace ServerClientGame.Networking
{
    /// <summary>
    /// Accepts game data sent from remote Server object for remote Game and sends client data back.
    /// </summary>
    public class Client : GameComponent
    {
        TcpClient client;
        private CustomConsole console;
        private Game1 GameRef;
        private Thread commThread;
        private NetworkStream CommStream;
        private double lastServerPing;
        private GameTime lastGameTime;

        public bool hasLocalServer { get { return GameRef.Server != null; } }

        public bool Alive { get; set; }
        
        public Client(Game1 game)
            : base(game)
        {
            GameRef = game;
            if (!hasLocalServer)
            {
                console = new CustomConsole(game);
                game.Components.Add(console);
            }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            client = new TcpClient();
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
            if (hasLocalServer && !client.Connected)
                Connect(GameRef.Server);

            // TODO: Add your update code here
            if (console != null)
            {
                console.Enabled = Alive;
                if (console.hasInput)
                    RespondToConsoleCommand(console.ReadInput());
            }
            // Do something more useful  

            base.Update(gameTime);
        }

        public void Connect(Server server)
        {
            Connect("127.0.0.1", server.Port);
        }

        public void Connect(string ip, int port)
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            try
            {
                client.Connect(serverEndPoint);

                lastServerPing = lastGameTime.TotalGameTime.TotalSeconds;
                commThread = new Thread(new ParameterizedThreadStart(HandleServerComm));
                commThread.Start(client);
                if (!hasLocalServer)
                    console.Output("Connected to server at: " + ip + ":" + port);
                else
                    GameRef.Server.LocalClient = this;
            }
            catch (Exception e)
            {
                if (e is ThreadStartException ||
                    e is SocketException)
                { 
                    if (!hasLocalServer)
                        console.Output("Couldn't Connect to server at: " + ip + ":" + port);
                }
                else
                {
                    throw;
                }
            }
        }

        private void HandleServerComm(object client)
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
                DisconnectFromServer();
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
//#if DEBUG
//                if (!hasLocalServer)
//                    console.Output("Server Ping");
//                else
//                    GameRef.Server.console.Output("Server Ping");
//#endif
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
                    DisconnectFromServer();
                    break;
                default:
                    console.Output("Unexpected Command from Server: " + commandType.ToString());
                    break;
            }
        }

        public void Send(Packet packet)
        {
            if (client.Connected)
            {
                try
                {
                    Serializer.SerializeWithLengthPrefix(CommStream, packet, PrefixStyle.Base128);
                }
                catch (IOException)
                {
                    DisconnectFromServer();
                }
            }
            else
            {
                console.Output("Not Connected to server");
            }
           
        }

        private void RespondToConsoleCommand(string input)
        {
            
            string[] args;
            ConsoleCommandType commandType = CustomConsole.GetCommandArgsFromString(input, out args);
            
            switch (commandType)
            {
                case ConsoleCommandType.Disconnect:
                    if (client.Connected)
                        DisconnectFromServer();
                    else
                        console.Output("Not Connected to Server");
                    break;
                case ConsoleCommandType.Connect:
                    if (args.Length == 0)
                        Connect("127.0.0.1", 3000);
                    else if (args.Length == 1 && args[0].Contains(':'))
                        Connect(args[0].Split(':')[0], Convert.ToInt32(args[0].Split(':')[1]));
                    else if (args.Length == 2)
                        Connect(args[0], Convert.ToInt32(args[1]));
                    break;
                case ConsoleCommandType.Text:
                    Send(new PacketConsoleCommand(commandType, String.Join(" ",args)));
                    break;
                case ConsoleCommandType.Identify:
                    if (args.Length == 1)
                        Send(new PacketConsoleCommand(commandType, args));
                    else if (args.Length == 0)
                    {
                        console.Output("Identify failed: No Name supplied");
                        console.Output("Usage: /identify Name");
                    }
                    else if (args.Length > 1)
                    {
                        console.Output("Identify failed: Name contained spaces");
                        console.Output("Usage: /identify Name");
                    }
                    break;
                case ConsoleCommandType.Say:
                    if (args.Length >= 2)
                    {
                        args = new string[] { args[0], string.Join(" ", args).Substring(args[0].Length + 1) };
                        Send(new PacketConsoleCommand(commandType, args));
                    }
                    else 
                        console.Output("Usage: /say Name Message");
                    break;
                case ConsoleCommandType.Exit:
                    console.Output("Exiting");
                    if (commThread != null)
                    {
                        commThread.Abort();
                        client.GetStream().Close();
                    }
                    client.Close();
                    Alive = false;
                    break;
                case ConsoleCommandType.Clients:
                    Send(new PacketConsoleCommand(commandType, args));
                    break;
                case ConsoleCommandType.Help:
                case ConsoleCommandType.Unknown:
                default:
                    console.Output("Commands: ");
                    console.Output("/identify NAME - Identify local client as NAME");
                    console.Output("/say NAME message  - whisper message to NAME");
                    console.Output("/connect - Connects to the server ");
                    console.Output("/disconnect - Disconnects from the server ");
                    console.Output("/exit - Ends the application ");
                    console.Output("/help - Displays this message ");
                    break;
            }

        }

        public void DisconnectFromServer()
        {
            if (hasLocalServer)
            {
                commThread.Abort();
                CommStream.Close();
                client.Close();
                Alive = false;
            }
            else
            {
                if (client.Connected)
                    Send(new PacketConsoleCommand(ConsoleCommandType.Disconnect)); 
                client.Close();
                Initialize();
            }
        }
    }
}

