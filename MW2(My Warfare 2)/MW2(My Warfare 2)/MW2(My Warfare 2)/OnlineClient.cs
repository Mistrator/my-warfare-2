using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using My_Warfare_2_Server;
using Jypeli;

/// <summary>
/// Verkkopelin verkkotoiminnot.
/// </summary>
public class OnlineClient
{
    public bool IsStarted { get; private set; }

    Socket SendingSocket { get; set; }

    UdpClient Listener { get; set; }

    Thread ListenThread { get; set; }

    IPAddress ClientIP { get; set; }

    int ClientPort { get; set; }

    IPEndPoint ClientAddress { get; set; }

    IPEndPoint ServerAddress { get; set; }

    String PlayerName { get; set; }

    String Password { get; set; }

    public Player LocalPlayer { get; set; }

    public List<Player> ConnectedPlayers { get; set; }

    /// <summary>
    /// Käynnistetään online-client. 
    /// </summary>
    /// <param name="serverAddress">Serverin IP ja portti, jota servu kuuntelee.</param>
    /// <param name="playerName">Pelaajan nimi.</param>
    /// <param name="password">Servun salasana.</param>
    /// <param name="clientPort">Portti, jota client kuuntelee. Välittyy servulle.</param>
    public void StartClient(IPEndPoint serverAddress, string playerName, string password, int clientPort = 13371)
    {
        IsStarted = true; 
        SendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ClientPort = clientPort;
        Listener = new UdpClient(ClientPort); // serverAddress.Port
        //Listener.AllowNatTraversal(true);
        //Listener.Client = SendingSocket;
        ServerAddress = serverAddress;

        ClientIP = IPAddress.Parse(GetPublicIP());
        ClientAddress = new IPEndPoint(ClientIP, ClientPort);

        LocalPlayer = new Player(playerName, -1, ClientAddress);
        ConnectedPlayers = new List<Player>();
        PlayerName = playerName;
        Password = password;

        Connect(ServerAddress);

        ListenThread = new Thread(new ThreadStart(delegate { Listen(new IPEndPoint(IPAddress.Any, ClientPort)); })); // serverin IP, clientin portti
        ListenThread.Start();
    }

    /// <summary>
    /// Sammutetaan online-client. 
    /// </summary>
    public void StopClient()
    {
        SendingSocket.Close();
        Listener.Close();
        ListenThread.Abort();
        IsStarted = false;
    }

    /// <summary>
    /// Muodostetaan yhteys serveriin.
    /// </summary>
    /// <param name="serverAddress">Serverin IP-osoite.</param>
    public void Connect(IPEndPoint serverAddress)
    {
        Send(new Packet(PacketType.Connect, -1, ClientAddress, new Connect(PlayerName, Password)));
    }

    /// <summary>
    /// Clientin kuuntelu ja tiedon vastaanotto.
    /// </summary>
    private void Listen(IPEndPoint listenedAddresses)
    {
        byte[] receivedData;
        Packet receivedPacket;
        try
        {
            while (true)
            {
                if (!IsStarted) break;
                receivedData = Listener.Receive(ref listenedAddresses);
                receivedPacket = Packet.ToPacket(receivedData);

                HandlePacket(receivedPacket);
            }
        }

        catch (Exception ex)
        {
            //ShowMessage("[ERROR] " + ex.Message); // kaataa servun sammuttaessa
        }
    }

    /// <summary>
    /// Lähetetään tietoa serverille.
    /// </summary>
    private void Send(Packet packet)
    {
        byte[] packetBuffer = Packet.ToBytes(packet);
        try
        {
            SendingSocket.SendTo(packetBuffer, ServerAddress); // SendingSocket.SendTo
        }

        catch (SocketException se)
        {
            //ShowMessage("[ERROR] " + se.Message);
        }
    }

    /// <summary>
    /// Käsitellään saapunut paketti.
    /// </summary>
    /// <param name="packet">Paketti.</param>
    private void HandlePacket(Packet packet)
    {
        switch (packet.Type)
        {
            case PacketType.Connect: // ei pitäisi tulla clientille
                break;

            case PacketType.AcceptConnect:
                AcceptConnect ac = packet.Data as AcceptConnect;
                LocalPlayer.PlayerID = ac.ID;
                ConnectedPlayers = ac.ConnectedPlayers;
                MW2_My_Warfare_2_.Peli.MessageDisplay.Add("[TESTI] Tuli AcceptConnect!");
                OnlineGame.StartOnlineGame(ac.GameMode);
                break;

            case PacketType.Disconnect:
                StopClient();
                MW2_My_Warfare_2_.Peli.MessageDisplay.Add((packet.Data as Disconnect).Reason);
                break;

            case PacketType.PlayerUpdate: // jonkun pelaajan datan muuttuessa päivitetään kyseisen pelaajan tiedot listasta
                PlayerUpdate pu = packet.Data as PlayerUpdate;
                for (int i = 0; i < ConnectedPlayers.Count; i++)
                {
                    if (ConnectedPlayers[i].PlayerID == pu.UpdatedPlayerID)
                        ConnectedPlayers[i] = pu.UpdatedPlayerData;
                }
                break;

            case PacketType.Shoot:
                break;
            case PacketType.ChatMessage:
                MW2_My_Warfare_2_.Peli.MessageDisplay.Add((packet.Data as ChatMessage).Message); // tähän parempi chatikkuna
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Selvitetään clientin julkinen IP-osoite.
    /// </summary>
    /// <returns>Clientin external IP.</returns>
    private string GetPublicIP()
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
    /// Vapautetaan resurssit.
    /// </summary>
    ~OnlineClient()
    {
        StopClient();
    }
}