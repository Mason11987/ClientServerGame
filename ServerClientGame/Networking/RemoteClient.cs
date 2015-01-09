using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Networking.Networking.Packets;
using ProtoBuf;
using System.Diagnostics;
using System.IO;
using Networking.Commands;


namespace Networking
{
    /// <summary>
    /// Represents a Remote Client for a Server.  
    /// RemoteClient sends out data from servers to a connected Client object on another machine.
    /// RemoteClient accepts data sent from connected Client object on another machine, interprets it, and routes it to server.
    /// </summary>
    public class RemoteClient
    {
        public Server Server { get { return NetworkManager.Server; } }

        private TcpClient Connection;
        private Thread CommThread;
        NetworkStream CommStream;

        public string Name { get; set; }
        public bool Connected { get { return Connection != null ? Connection.Connected : false; } }

        private double lastClientPing;

        public string IP { get { return Connection.Client.RemoteEndPoint.ToString(); } }
        public bool RecentlyPinged { get { return Server.lastGameTime.TotalGameTime.TotalSeconds - lastClientPing < (Server.PingRate * 2 + 1); } }

        public RemoteClient(string name, TcpClient connection, Server server)
        {
            Connection = connection;
            CommThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
            Name = name;

            this.CommThread.Start(this);
        }

        #region Communication From Remote Client
        private void HandleClientCommunication(object sender)
        {

            RemoteClient commClient = (RemoteClient)sender;
            CommStream = commClient.Connection.GetStream();
            if (Server == null)
                lastClientPing = 0;
            else
                lastClientPing = Server.lastGameTime.TotalGameTime.TotalSeconds;

            while (Connection.Connected && RecentlyPinged)
            {
                if (CommStream.DataAvailable)
                    HandleClientData(CommStream);
            }

            Server.OnClientDisconnectedCommand(this, !Connection.Connected);

            if (Connection.Connected)
                Close();
        }

        private void HandleClientData(NetworkStream CommStream)
        {
            Packet p;
            try
            {
                p = Serializer.DeserializeWithLengthPrefix<Packet>(CommStream, PrefixStyle.Base128);


                lastClientPing = Server.lastGameTime.TotalGameTime.TotalSeconds;

                if (NetworkManager.Server.Settings["showping"] == "yes")
                    NetworkManager.Console.Output(Name + " Ping " + Math.Round((lastClientPing - Server.LastPing) * 1000) + "ms");

                if (p is PacketConsoleCommand)
                    HandleClientCommand((p as PacketConsoleCommand).CommandType, (p as PacketConsoleCommand).Arguments);

            }
            catch (Exception e)
            {
                if (e is IOException || e is ArgumentException)
                {
                    Server.OnClientDisconnectedCommand(this, !Connection.Connected);

                    if (Connection.Connected)
                        Close();
                }
                else
                    throw;
            }

        }

        private void HandleClientCommand(ConsoleCommandType commandType, params string[] args)
        {
            switch (commandType)
            {
                case ConsoleCommandType.Disconnect:
                    Server.OnClientDisconnectedCommand(this, true);
                    Close();
                    break;
                case ConsoleCommandType.Identify:
                    Server.ExecuteCommand(new IdentifyCommand().MakeCommand(args, this));
                    break;
                case ConsoleCommandType.Text:
                    Server.OnClientTextCommand(this, args[0]);
                    break;
                case ConsoleCommandType.Say:
                    Server.OnClientSay(this, args[0], args[1]);
                    break;
                case ConsoleCommandType.Clients:
                    Server.OnClientRequestClientList(this);
                    break;
                default:
                    Server.OnClientUnexpectedCommand(this, commandType);
                    break;
            }

        }
        #endregion

        internal void Send(Packet packet)
        {
            try
            {
                Serializer.SerializeWithLengthPrefix(CommStream, packet, PrefixStyle.Base128);
            }
            catch(Exception e)
            {
                if (e is IOException || e is ArgumentException)
                {
                    Server.OnClientDisconnectedCommand(this, !Connection.Connected);

                    if (Connection.Connected)
                        Close();
                }
                else
                    throw;
            }
        }

        internal void Close()
        {
            if (Connection.Connected)
                Connection.GetStream().Close();
            Connection.Close();
            CommThread.Abort();
        }
    }
}
