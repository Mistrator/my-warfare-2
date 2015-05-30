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

namespace Jypeli
{
    /// <summary>
    /// Parametrit analogisen ohjauksen (hiiren tai ohjaustikun) tapahtumalle.
    /// </summary>    
    public struct AnalogState
    {
        /// <summary>
        /// Peliohjaimen analoginäppäimen paikkakoordinaatti.
        /// Arvo on välillä 0.0 - 1.0.
        /// </summary>
        public double State;

        /// <summary>
        /// Muutos peliohjaimen analoginäppäimen paikassa.
        /// </summary>
        public double AnalogChange;

        /// <summary>
        /// Analogisen Ohjainsauvan paikka tai puhelimen asento.
        /// Arvo on (0, 0) kun sauva on keskellä tai puhelinta ei ole kallistettu.
        /// X- sekä Y-koordinaattien arvot ovat välillä -1.0 - 1.0.
        /// </summary>
        public Vector StateVector;

        /// <summary>
        /// Hiiren liikevektori.
        /// </summary>
        public Vector MouseMovement;

        /// <summary>
        /// Parameters for mouse events.
        /// </summary>
        internal AnalogState( ButtonState s, Vector movement )
        {
            AnalogChange = movement.Magnitude;
            MouseMovement = movement;
            State = ( s == ButtonState.Down ) ? 1 : 0;
            StateVector = Vector.Zero;
        }

        /// <summary>
        /// Parameters for gamecontroller events.
        /// </summary>
        internal AnalogState( double state, double analogChange, Vector stateVector )
        {
            AnalogChange = analogChange;
            State = state;
            StateVector = stateVector;
            MouseMovement = Vector.Zero;
        }
    }
}
