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
using ServerClientGame.Commands;


namespace ServerClientGame.Networking
{
    /// <summary>
    /// Routes Game Data as necessary through RemoteClient to Client on remote machine.
    /// </summary>
    public class Server : GameComponent
    {
        public static int PingRate = 2;

        public TcpListener tcpListener;
        public Thread listenThread;
        internal CustomConsole console;
        public Client LocalClient;
        public GameTime lastGameTime;
        public double LastPing;
        public Dictionary<string, string> Settings = new Dictionary<string, string>() { { "showping", "no" } };

        public ConcurrentDictionary<string, RemoteClient> Clients = new ConcurrentDictionary<string, RemoteClient>();

        public string LocalIP { get { return tcpListener.LocalEndpoint.ToString().Split(':')[1]; } }
        public int Port { get; private set; }
        public string PublicIP { get; private set; }

        public bool Alive { get; set; }

        public Server(Game game, int port)
            : base(game)
        {
            Port = port;
            console = new CustomConsole(game, this);
            Command.console = console;
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
                RespondToConsoleInput(console.ReadInput());

            if (gameTime.TotalGameTime.TotalSeconds - LastPing > Server.PingRate)
            {
                LastPing = gameTime.TotalGameTime.TotalSeconds;
                Broadcast(new PacketPing());
            }

        }

        public void ExecuteCommand(Command command)
        {
            CommandResult result = command.Execute();

            if (result == CommandResult.Failed)
                console.Output("Command Failed");
            else if (result == CommandResult.NotImplemented)
                console.Output("Command Not Implemented");
            else if (result == CommandResult.Success)
                console.Output("Command Succeeded");
        }

        private void RespondToConsoleInput(string input)
        {
            ExecuteCommand(console.GetCommand(input));
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
        public void Broadcast(Packet packet, RemoteClient ignoreClient = null)
        {
            foreach (var client in Clients.Values)
            {
                if (client != ignoreClient && client.Connected)
                    client.Send(packet);
            }
        }

        #region Helper Functions
        public string GetClientDisplayList()
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




