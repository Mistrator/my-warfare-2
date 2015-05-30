#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */ 

/*
 * Directions
 *  
 * 14.4.2009 (Tomi Karppinen)
 *    - Class is now serializable
 * 14.4.2009 (Tomi Karppinen)
 *    - Added the Direction enumeration
 * 14.4.2009 Initial version (Tomi Karppinen)
 */

using System;
using System.Collections.Generic;
using AdvanceMath;
using Physics2DDotNet;

namespace Jypeli
{
    /// <summary>
    /// Perussuunta tasossa.
    /// </summary>
    public struct Direction
    {
        /// <summary>
        /// Ei suuntaa.
        /// </summary>
        public static Direction None = new Direction( "None", 0, 0 );

        /// <summary>
        /// Suunta ylös.
        /// </summary>
        public static Direction Up = new Direction( "Up", 0, 1 );

        /// <summary>
        /// Suunta alas.
        /// </summary>
        public static Direction Down = new Direction( "Down", 0, -1 );

        /// <summary>
        /// Suunta vasemmalle.
        /// </summary>
        public static Direction Left = new Direction( "Left", -1, 0 );

        /// <summary>
        /// Suunta oikealle.
        /// </summary>
        public static Direction Right = new Direction( "Right", 1, 0 );

        /// <summary>
        /// Suuntaa vastaava yksikkövektori.
        /// </summary>
        private Vector Vector;

        /// <summary>
        /// Suuntaa vastaava kulma.
        /// </summary>
        public Angle Angle
        {
            get { return Vector.Angle; }
        }

        /// <summary>
        /// Suunnan nimi.
        /// </summary>
        public string Name;

        private Direction( string name, int x, int y )
        {            
            Name = name;
            Vector = new Vector(x, y);
        }

        /// <summary>
        /// Palauttaa vastakkaisen suunnan annetulle suunnalle.
        /// </summary>
        /// <param name="d">Suunta.</param>
        /// <returns>Vastakkainen suunta.</returns>
        public static Direction Inverse( Direction d )
        {
            if ( d == Direction.Up ) return Direction.Down;
            if ( d == Direction.Down ) return Direction.Up;
            if ( d == Direction.Left ) return Direction.Right;
            if ( d == Direction.Right ) return Direction.Left;

            return Direction.None;
        }

        public Vector GetVector()
        {
            return Vector;
        }

        public override bool Equals( object obj )
        {
            if ( !( obj is Direction ) ) return false;
            return this == ( (Direction)obj );
        }

        public static bool operator ==(Direction left, Direction right)
        {
            return (left.Vector == right.Vector && left.Name == right.Name);
        }

        public static bool operator !=(Direction left, Direction right)
        {
            return (left.Vector != right.Vector || left.Name != right.Name);
        }
    }
}
