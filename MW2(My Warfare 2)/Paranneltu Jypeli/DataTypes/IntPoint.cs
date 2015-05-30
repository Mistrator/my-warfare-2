using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Piste kokonaislukuruudukossa.
    /// </summary>
    public class IntPoint
    {
        /// <summary>
        /// X-koordinaatti.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y-koordinaatti.
        /// </summary>
        public int Y { get; set; }

        public IntPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
