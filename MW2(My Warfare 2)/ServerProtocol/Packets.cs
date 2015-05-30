using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My_Warfare_2_Server
{
    /// <summary>
    /// Yhteyden muodostamisyritys clientiltä serverille.
    /// </summary>
    [Serializable]
    public class Connect
    {
        /// <summary>
        /// Pelaajan nimi.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Pelaajan antama salasana.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Luodaan Connect-paketti.
        /// </summary>
        /// <param name="name">Pelaajan nimi.</param>
        /// <param name="password">Pelaajan antama salasana.</param>
        public Connect(string name, string password)
        {
            this.PlayerName = name;
            this.Password = password;
        }
    }

    /// <summary>
    /// Yhteyden katkaisu syystä tai toisesta.
    /// </summary>
    [Serializable]
    public class Disconnect
    {
        /// <summary>
        /// Syy yhteyden katkaisuun.
        /// </summary>
        public string Reason { get; set; }

        public Disconnect(string reason)
        {
            this.Reason = reason;
        }
    }

    /// <summary>
    /// Yhteyden muodostuksen hyväksyminen.
    /// </summary>
    [Serializable]
    public class AcceptConnect
    {
        /// <summary>
        /// Tällä hetkellä pelissä olevat pelaajat.
        /// </summary>
        public List<Player> ConnectedPlayers { get; set; }

        /// <summary>
        /// Mikä ID paketin kohteelle tulee.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Pelityyppi.
        /// </summary>
        public int GameMode { get; set; }

        public AcceptConnect(List<Player> connectedPlayers, int ID, int gameMode)
        {
            this.ConnectedPlayers = connectedPlayers;
            this.ID = ID;
            this.GameMode = gameMode;
        }
    }

    /// <summary>
    /// Chattiviesti.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <summary>
        /// Viesti.
        /// </summary>
        public String Message { get; set; }

        public ChatMessage(string message)
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// Pelaajan tilan muutos.
    /// </summary>
    [Serializable]
    public class PlayerUpdate
    {
        /// <summary>
        /// Päivitetyn pelaajan ID.
        /// </summary>
        public int UpdatedPlayerID { get; set; }

        /// <summary>
        /// Päivitetyn pelaajan data.
        /// </summary>
        public Player UpdatedPlayerData { get; set; }

        public PlayerUpdate(int updatedPlayerID, Player updatedPlayerData)
        {
            this.UpdatedPlayerID = updatedPlayerID;
            this.UpdatedPlayerData = updatedPlayerData;
        }
    }
}
