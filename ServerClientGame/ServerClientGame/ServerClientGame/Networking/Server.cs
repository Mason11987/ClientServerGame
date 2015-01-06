using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Net;
using System.IO;
using ServerClientGame.Networking.Packets;
using System.Collections.Concurrent;


namespace ServerClientGame.Networking
{
    /// <summary>
    /// Routes Game Data as necessary through RemoteClient to Client on remote machine.
    /// </summary>
    public class Server : GameComponent
    {
        public static int PingRate = 2;

        private TcpListener tcpListener;
        private Thread listenThread;
        internal CustomConsole console;
        public Client LocalClient;
        public GameTime lastGameTime;
        public double LastPing;

        private ConcurrentDictionary<string, RemoteClient> Clients = new ConcurrentDictionary<string, RemoteClient>();

        public string LocalIP { get { return tcpListener.LocalEndpoint.ToString().Split(':')[1]; } }
        public int Port { get; private set; }
        public string PublicIP { get; private set; }

        public bool Alive { get; set; }

        public Server(Game game, int port)
            : base(game)
        {
            Port = port;
            console = new CustomConsole(game);
            game.Components.Add(console);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            Alive = true;
            this.tcpListener = new TcpListener(IPAddress.Any, Port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            PublicIP = GetPublicIpAddress();
            console.Output("Public IP: " + GetPublicIpAddress() + ":" + Port);


            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            lastGameTime = gameTime;

            base.Update(gameTime);

            console.Enabled = Alive;
            if (console.hasInput)
                RespondToConsoleCommand(console.ReadInput());
            if (gameTime.TotalGameTime.TotalSeconds - LastPing > Server.PingRate)
            {
                LastPing = gameTime.TotalGameTime.TotalSeconds;
                Broadcast(new PacketPing());
            }

        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (Alive)
            {
                try
                {
                    //blocks until a client has connected to the server
                    TcpClient connection = this.tcpListener.AcceptTcpClient();

                    if (!Alive) //When exiting
                        break;
                    string name = "Client" + (Clients.Count + 1);
                    RemoteClient newClient = new RemoteClient(name, connection, this);
                    Clients.TryAdd(name, newClient);
                    console.Output("Client Connected: " + newClient.IP);
                    Broadcast(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", " * " + newClient.Name + " Connected *" }), newClient);
                }
                catch (SocketException)
                {
                    Alive = false;
                }
            }
        }

        /// <summary>
        /// Send a packet to all current clients
        /// </summary>
        private void Broadcast(Packet packet, RemoteClient ignoreClient = null)
        {
            foreach (var client in Clients.Values)
            {
                if (client != ignoreClient && client.Connected)
                    client.Send(packet);
            }
        }

        /// <summary>
        /// Execute commands inputed into the console
        /// </summary>
        public void RespondToConsoleCommand(string input)
        {
            string[] args;
            ConsoleCommandType commandType = CustomConsole.GetCommandArgsFromString(input, out args);

            switch (commandType)
            {
                case ConsoleCommandType.Text:
                    Broadcast(new PacketConsoleCommand(commandType, new [] {"SERVER", args[0]}));
                    break;
                case ConsoleCommandType.Identify:
                    if (LocalClient != null)
                        LocalClient.Send(new PacketConsoleCommand(commandType, args));
                    break;
                case ConsoleCommandType.Exit:
                    console.Output("Exiting");
                    Broadcast(new PacketConsoleCommand(ConsoleCommandType.Disconnect));

                    Alive = false;
                    foreach (var client in Clients.Values)
                        client.Close();
                    tcpListener.Stop();
                    listenThread.Abort();
                    Game.Exit();
                    break;
                case ConsoleCommandType.Clients:
                    console.Output("List of Clients");
                    console.Output(GetClientDisplayList());
                    break;
                case ConsoleCommandType.Say:
                    if (args.Length >= 2)
                    {
                        args = new string[] { args[0], string.Join(" ", args).Substring(args[0].Length + 1) };
                        LocalClient.Send(new PacketConsoleCommand(commandType, args));
                    }
                    else
                        console.Output("Usage: /say Name Message");
                    break;
                case ConsoleCommandType.Disconnect: //Server can't input Disconnect Command
                case ConsoleCommandType.Unknown:
                default:
                        console.Output("Commands: ");
                        console.Output("/identify name - Identify local client as name");
                        console.Output("/say NAME message  - whisper message to NAME from local client");
                        console.Output("/clients - Lists all clients ");
                        console.Output("/exit - Ends the application ");
                        console.Output("/help - Displays this message ");
                    break;
            }
        }

        #region Helper Functions
        private string GetClientDisplayList()
        {
            StringBuilder list = new StringBuilder();
            foreach (var client in Clients)
                list.AppendLine(client.Key + "\t" + client.Value.IP);

            return list.ToString().Trim();
        }

        public string GetPublicIpAddress()
        {
            try
            {
                IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());

                foreach (var addrList in IPHost.AddressList)
                {
                    if (addrList.AddressFamily == AddressFamily.InterNetwork)
                        return addrList.ToString();
                }
                return "";
            }
            catch (Exception)
            {
                return "Unknown";
            }

        }
        #endregion

        #region Respond To Client Command
        internal void OnClientDisconnectedCommand(RemoteClient remoteClient, bool acknowledged)
        {
            if (acknowledged)
                console.Output(remoteClient.Name + " Disconnected");
            else
                console.Output(remoteClient.Name + " Lost");

            RemoteClient outRemoteClient;
            if (!Clients.TryRemove(remoteClient.Name, out outRemoteClient))
            {
                if (Clients.ContainsKey(remoteClient.Name))
                    throw new Exception("Couldn't remove client from list on server");
                else
                {

                }
            }
            Broadcast(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", " * " + remoteClient.Name + " Disconnected *" }));
            
        }

        internal void OnClientIdentifiedCommand(RemoteClient remoteClient, string newName)
        {
            if (Clients.ContainsKey(newName))
            {
                console.Output("Failed to update identifation of " + remoteClient.IP);
                remoteClient.Send(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", "Identify Failed: A user already exists with the name: " + newName}));
                return;
            }

            RemoteClient outRemoteClient;
            if (Clients.TryRemove(remoteClient.Name, out outRemoteClient))
            {

                string oldName = remoteClient.Name;
                remoteClient.Name = newName;
                Clients.TryAdd(remoteClient.Name, remoteClient);

                console.Output("Client " + remoteClient.IP + " - Identified as " + newName);
                Broadcast(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", " * " + oldName + " identified as " + remoteClient.Name + " *" }));


            }
            else
            {

                console.Output("Failed to update identifation of " + remoteClient.IP);
            }
        }

        internal void OnClientTextCommand(RemoteClient remoteClient, string text)
        {
            console.Output(remoteClient.Name + ": " + text);

            Broadcast(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { remoteClient.Name, text }));
        }

        internal void OnClientUnexpectedCommand(RemoteClient remoteClient, ConsoleCommandType commandType)
        {
            console.Output(String.Format("Unexpected Command from Client ({0}): {1}", remoteClient.Name, commandType.ToString()));
        }

        internal void OnClientSay(RemoteClient remoteClient, string Target, string Text)
        {
            foreach (var client in Clients.Values)
            {
                if (client.Name == Target)
                {
                    console.Output(String.Format("{0} to {1}: {2}", remoteClient.Name, client.Name, Text));
                    client.Send(new PacketConsoleCommand(ConsoleCommandType.Say, new [] {remoteClient.Name, Text }));
                    return;
                }
            }
            console.Output(String.Format("{0} failed say to {1}: {2}", remoteClient.Name, Target, Text));
            remoteClient.Send(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", string.Format("Say Failed. {0} is not connected.", Target) }));
        }

        internal void OnClientRequestClientList(RemoteClient remoteClient)
        {
            remoteClient.Send(new PacketConsoleCommand(ConsoleCommandType.Text, new[] { "Server", "List of Clients\n" + GetClientDisplayList() }));
        }
        
        #endregion


    }

}




