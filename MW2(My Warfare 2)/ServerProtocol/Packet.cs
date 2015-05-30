using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace My_Warfare_2_Server
{
    /// <summary>
    /// Lähetettävä paketti.
    /// </summary>
    [Serializable]
    public class Packet
    {
        /// <summary>
        /// Mitä paketin Datassa on.
        /// </summary>
        public PacketType Type { get; set; }

        /// <summary>
        /// Lähettäjän ID.
        /// </summary>
        public int SenderID { get; set; }

        /// <summary>
        /// Lähettäjän IP.
        /// </summary>
        public IPEndPoint SenderAddress { get; set; }

        /// <summary>
        /// Lähettäjän nimi.
        /// </summary>
        public String SenderName { get; set; }

        /// <summary>
        /// Itse sisältö.
        /// </summary>
        public Object Data { get; set; }

        /// <summary>
        /// Luodaan uusi paketti.
        /// </summary>
        /// <param name="type">Tyyppi.</param>
        /// <param name="senderID">Lähettäjän ID.</param>
        /// <param name="senderAddress">Lähettäjän IP.</param>
        /// <param name="data">Paketin sisältö.</param>
        public Packet(PacketType type, int senderID, IPEndPoint senderAddress, object data)
        {
            Type = type;
            SenderID = senderID;
            SenderAddress = senderAddress;
            Data = data;
        }

        /// <summary>
        /// Muunnetaan Packet byte-taulukoksi lähetystä varten.
        /// </summary>
        /// <param name="packet">Muunnettava paketti.</param>
        /// <returns>Paketti byte-taulukkona.</returns>
        public static byte[] ToBytes(Packet packet)
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, packet);
                buffer = stream.ToArray();
            }

            return buffer;
        }

        /// <summary>
        /// Muunnetaan Byte-taulukko takaisin Packetiksi.
        /// </summary>
        /// <param name="serializedPacket">Muunnettava taulukko.</param>
        /// <returns>Taulukko pakettina.</returns>
        public static Packet ToPacket(byte[] serializedPacket)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(serializedPacket, 0, serializedPacket.Length);
                stream.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();

                return (Packet)formatter.Deserialize(stream);
            }
        }
    }
}
