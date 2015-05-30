using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My_Warfare_2_Server
{
    /// <summary>
    /// Paketin tyyppi (käytännössä sisältö).
    /// </summary>
    public enum PacketType
    {
        Connect,
        AcceptConnect,
        Disconnect,
        PlayerUpdate,
        Shoot,
        ChatMessage
    }
}
