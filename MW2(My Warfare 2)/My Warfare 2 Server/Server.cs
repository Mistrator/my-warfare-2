using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Controls;

namespace My_Warfare_2_Server
{
    /// <summary>
    /// My Warfare 2:n multiplayer server.
    /// </summary>
    internal static class Server
    {
        /// <summary>
        /// Onko serveri käynnissä.
        /// </summary>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// Serverin tiedon lähettäjä.
        /// </summary>
        static Socket SendingSocket { get; set; }

        /// <summary>
        /// Serverin kuuntelija.
        /// </summary>
        static UdpClient Listener { get; set; }

        /// <summary>
        /// Kuuntelusäie.
        /// </summary>
        static Thread ListenThread { get; set; }

        /// <summary>
        /// Tällä hetkellä serverillä olevat pelaajat.
        /// </summary>
        static List<Player> ConnectedPlayers { get; set; }

        static int UsedPlayerIDs { get; set; }

        static IPAddress ServerIP { get; set; }

        static int ServerPort { get; set; }

        /// <summary>
        /// Serverin osoite (IP, portti).
        /// </summary>
        static IPEndPoint ServerAddress { get; set; }

        static ScrollViewer ChatWindow { get; set; }

        /// <summary>
        /// Serverin salasana.
        /// </summary>
        static String Password { get; set; }

        /// <summary>
        /// Pelimuoto.
        /// </summary>
        static int GameMode { get; set; }

        /// <summary>
        /// Käynnistetään serveri ja aloitetaan yhteydenottojen kuuntelu.
        /// </summary>
        public static void StartServer(int port, ScrollViewer chatWindow, string password, int gameMode)
        {
            IsRunning = true;
            ChatWindow = chatWindow;
            ChatWindow.Content = "";
            ShowMessage("SERVER", "Server starting...");
            SendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ConnectedPlayers = new List<Player>();
            Listener = new UdpClient(port);
            //Listener.Client = SendingSocket;
            //Listener.AllowNatTraversal(true);
            ServerIP = IPAddress.Parse(GetPublicIP());
            ServerPort = port;
            ServerAddress = new IPEndPoint(ServerIP, ServerPort);

            IPEndPoint listenedAddresses = new IPEndPoint(IPAddress.Any, port);
            Password = password;
            GameMode = gameMode;

            ListenThread = new Thread(new ThreadStart(delegate { Listen(listenedAddresses); }));
            ListenThread.Start();

            ShowMessage("SERVER", "Server started!");
            ShowMessage("SERVER", "Server IP: " + ServerIP);
            ShowMessage("SERVER", "Server port: " + ServerPort);
        }

        /// <summary>
        /// Sammutetaan serveri.
        /// </summary>
        public static void StopServer()
        {
            ShowMessage("SERVER", "Server stopping...");

            for (int i = 0; i < ConnectedPlayers.Count; i++) // disconnectataan pelaajat
            {
                Send(ConnectedPlayers[i].PelaajanOsoite, new Packet(PacketType.Disconnect, 0, ServerAddress, new Disconnect("Server closed!")));
            }

            IsRunning = false;
            ListenThread.Abort();
            SendingSocket.Close();
            Listener.Close();
            UsedPlayerIDs = 0;
            Password = "";

            ShowMessage("SERVER", "Server stopped!");
        }

        /// <summary>
        /// Serverin kuuntelu ja tiedon vastaanotto.
        /// </summary>
        private static void Listen(IPEndPoint listenedAddresses)
        {
            byte[] receivedData;
            Packet receivedPacket;
            try
            {
                while (true)
                {
                    if (!IsRunning) break;
                    receivedData = Listener.Receive(ref listenedAddresses);
                    receivedPacket = Packet.ToPacket(receivedData);

                    HandlePacket(receivedPacket);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // jaa?
            }
        }

        /// <summary>
        /// Lähetetään tietoa kohteelle.
        /// </summary>
        /// <param name="target">Kohde.</param>
        private static void Send(IPEndPoint target, Packet packet)
        {
            byte[] packetBuffer = Packet.ToBytes(packet);
            try
            {
                SendingSocket.SendTo(packetBuffer, target); // SendingSocket.SendTo
            }

            catch (SocketException se)
            {
                ShowMessage("[ERROR] " + se.Message);
            }
        }

        /// <summary>
        /// Lähetetään tietoa kaikille pelaajille.
        /// </summary>
        private static void Send(Packet packet)
        {
            byte[] packetBuffer = Packet.ToBytes(packet);
            try
            {
                for (int i = 0; i < ConnectedPlayers.Count; i++)
                {
                    SendingSocket.SendTo(packetBuffer, ConnectedPlayers[i].PelaajanOsoite); // SendingSocket.SendTo
                }
            }

            catch (SocketException se)
            {
                ShowMessage("[ERROR] " + se.Message);
            }
        }


        /// <summary>
        /// Käsitellään saapunut paketti.
        /// </summary>
        /// <param name="packet">Paketti.</param>
        private static void HandlePacket(Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.Connect: // pelaaja yrittää liittyä peliin
                    if ((packet.Data as Connect).Password != Password)
                    {
                        Send(packet.SenderAddress, new Packet(PacketType.Disconnect, 0, ServerAddress, new Disconnect("Virheellinen salasana!")));
                        ShowMessage("INFO", (packet.Data as Connect).PlayerName + " tried to connect with invalid password!");
                    }

                    else
                    {
                        int ID = AddPlayerToGame((packet.Data as Connect).PlayerName, packet.SenderAddress);
                        Send(packet.SenderAddress, new Packet(PacketType.AcceptConnect, 0, ServerAddress, new AcceptConnect(ConnectedPlayers, ID, GameMode)));
                        //ShowMessage((packet.Data as Connect).PlayerName + " joined the battle!");
                    }
                    break;

                case PacketType.AcceptConnect: // ei pitäisi tulla servulle
                    ShowMessage("[WUT] Miksi servulle tuli AcceptConnect-paketti?");
                    break;

                case PacketType.Disconnect:
                    ShowMessage(packet.SenderName + " left the battle.");
                    RemovePlayerFromGame(packet.SenderID);
                    break;

                case PacketType.PlayerUpdate: // pelaajan päivittyessä päivitetään servun tiedot ja lähetetään päivitys kaikille pelaajille.
                     int i = ConnectedPlayers.FindIndex(player => player == packet.Data as Player);
                     ConnectedPlayers[i] = (packet.Data as PlayerUpdate).UpdatedPlayerData;
                     Send(new Packet(PacketType.PlayerUpdate, packet.SenderID, packet.SenderAddress, new PlayerUpdate((packet.Data as PlayerUpdate).UpdatedPlayerID, (packet.Data as PlayerUpdate).UpdatedPlayerData)));
                    break;

                case PacketType.Shoot:
                    break;

                case PacketType.ChatMessage:
                    ShowMessage((packet.Data as ChatMessage).Message);
                    SendMessage((packet.Data as ChatMessage).Message, packet.SenderID, packet.SenderAddress); 
                    break;

                default:
                    ShowMessage("[ERROR] Unrecognized packet received! Is server out of date?");
                    break;
            }
        }

        /// <summary>
        /// Selvitetään serverin julkinen IP-osoite.
        /// </summary>
        /// <returns>Serverin external IP.</returns>
        private static string GetPublicIP()
        {
            String direction = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                direction = stream.ReadToEnd();
            }

            //Search for the ip in the html
            int first = direction.IndexOf("Address: ") + 9;
            int last = direction.LastIndexOf("</body>");
            direction = direction.Substring(first, last - first);

            return direction;
        }

        /// <summary>
        /// Näytetään viesti serverin chatti-ikkunassa.
        /// </summary>
        /// <param name="sender">Lähettäjä.</param>
        /// <param name="message">Viesti.</param>
        public static void ShowMessage(string sender, string message)
        {
            ChatWindow.Content += "[" + sender + "] " + message + "\n";
        }

        /// <summary>
        /// Näytetään viesti serverin chatti-ikkunassa.
        /// </summary>
        /// <param name="message">Viesti.</param>
        public static void ShowMessage(string message)
        {
            ChatWindow.Content += message + "\n";
        }

        /// <summary>
        /// Lähetetään viesti kaikille pelaajille serverin tiedoilla.
        /// </summary>
        /// <param name="message">Viesti.</param>
        public static void SendMessage(string message)
        {
            Send(new Packet(PacketType.ChatMessage, 0, ServerAddress, new ChatMessage(message)));
        }

        /// <summary>
        /// Lähetetään viesti kaikille pelaajille.
        /// </summary>
        /// <param name="message">Viesti.</param>
        /// <param name="senderID">Lähettäjän ID.</param>
        /// <param name="senderAddress">Lähettäjän osoite.</param>
        public static void SendMessage(string message, int senderID, IPEndPoint senderAddress)
        {
            Send(new Packet(PacketType.ChatMessage, senderID, senderAddress, new ChatMessage(message)));
        }


        /// <summary>
        /// Lisätään pelaaja peliin.
        /// </summary>
        /// <param name="playerName">Pelaajan nimi.</param>
        /// <param name="playerAddress">Pelaajan osoite.</param
        /// <returns>Pelaajalle tullut ID.</returns>
        private static int AddPlayerToGame(String playerName, IPEndPoint playerAddress)
        {
            ConnectedPlayers.Add(new Player(playerName, ++UsedPlayerIDs, playerAddress));
            return UsedPlayerIDs;
        }

        /// <summary>
        /// Poistetaan pelaaja pelistä.
        /// </summary>
        /// <param name="playerID">Poistettavan pelaajan ID.</param>
        private static void RemovePlayerFromGame(int playerID)
        {
            for (int i = 0; i < ConnectedPlayers.Count; i++)
            {
                if (ConnectedPlayers[i].PlayerID == playerID) ConnectedPlayers.RemoveAt(i);
            }
        }
    }
}
