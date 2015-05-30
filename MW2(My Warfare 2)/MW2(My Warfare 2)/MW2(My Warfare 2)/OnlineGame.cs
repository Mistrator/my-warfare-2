using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Jypeli;
using Jypeli.Widgets;

/// <summary>
/// Luokka verkkopelin toiminnoille.
/// </summary>
public static class OnlineGame
{
    public static StringBuilder ServerIP { get; set; }

    public static int ServerPort { get; set; }

    public static StringBuilder ServerPassword { get; set; }

    public enum GameMode { Deathmatch, Infinite };

    public static Label ChatWindow { get; set; }

    public static OnlineClient CurrentGame { get; set; }

    public static List<Pelaaja> PhysicalPlayers { get; set; }

    static OnlineGame()
    {
        ServerIP = new StringBuilder();
        ServerPassword = new StringBuilder();
    }
    /// <summary>
    /// Liitytään peliin ja alustetaan verkkopeli clientin päässä.
    /// </summary>
    /// <param name="serverAddress">Serverin osoite.</param>
    public static void JoinGame(IPEndPoint serverAddress)
    {
        OnlineClient client = new OnlineClient();
        CurrentGame = client;
        client.StartClient(serverAddress, AsetustenKaytto.Asetukset.KayttajaNimi, ServerPassword.ToString());
    }

    /// <summary>
    /// Aloitetaan online-peli, luodaan kenttä ja pelaajat.
    /// </summary>
    /// <param name="gameMode">Pelin tyyppi.</param>
    public static void StartOnlineGame(int gameMode)
    {
        switch (gameMode)
        {
            case 0:
                break;
            case 1:
                break;
        }
    }
}