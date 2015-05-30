using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Jypeli;

namespace My_Warfare_2_Server
{
    /// <summary>
    /// Yhteyden ottanut pelaaja.
    /// </summary>
    [Serializable]
    public class Player
    {
        /// <summary>
        /// Pelaajan IP-osoite ilman porttia.
        /// </summary>
        private IPAddress PelaajanIPOsoite { get; set; }

        /// <summary>
        /// Pelaajan koko osoite (IP, portti)
        /// </summary>
        public IPEndPoint PelaajanOsoite { get; set; }

        /// <summary>
        /// Pelaajan ID, joka ei muutu yhden pelikerran aikana.
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// Pelaajan sijainti kentällä.
        /// </summary>
        public Vector Position { get; set; }

        /// <summary>
        /// Pelaajan nopeus ja liikkeen suunta.
        /// </summary>
        public Vector Velocity { get; set; }

        /// <summary>
        /// Pelaajan kulma.
        /// </summary>
        public Angle Angle { get; set; }

        /// <summary>
        /// Pelaajan näytettävä nimi.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Pelaajan HP:t.
        /// </summary>
        public double Health { get; set; }

        public Player(String nimi, int ID, IPEndPoint playerAddress)
        {
            this.Name = nimi;
            this.PlayerID = ID;
            this.PelaajanOsoite = playerAddress;
        }
    }
}
