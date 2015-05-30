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

using System;
using Microsoft.Xna.Framework.Input;

using XnaKeyboard = Microsoft.Xna.Framework.Input.Keyboard;
using System.Collections.Generic;

namespace Jypeli.Controls
{
    /// <summary>
    /// Näppäimistö peliohjaimena.
    /// </summary>
    public class Keyboard : Controller<KeyboardState>
    {
#if WINDOWS
        internal static KeyboardTextBuffer Buffer = new KeyboardTextBuffer();
#endif

        internal Keyboard()
        {
            oldState = XnaKeyboard.GetState();
        }

        /// <summary>
        /// Tarkistaa, liittyykö näppäimeen k merkkiä.
        /// </summary>
        /// <param name="k">Näppäin</param>
        /// <returns>boolean</returns>
        public static bool IsChar( Key k )
        {
            return ToMaybeChar( k ).HasValue;
        }

        /// <summary>
        /// Palauttaa näppäimen merkille c.
        /// Jos merkille ei ole näppäintä, palautetaan Key.None.
        /// </summary>
        /// <param name="c">Merkki</param>
        /// <returns>Näppäin</returns>
        public static Key FromChar( char c )
        {
            Key result = Key.None;
#if WINDOWS
            Enum.TryParse<Key>( c.ToString(), true, out result );
#endif
            return result;
        }

        /// <summary>
        /// Palauttaa merkin, joka liittyy näppäimeen k.
        /// Jos näppäimeen ei liity merkkiä, tapahtuu poikkeus ArgumentException.
        /// </summary>
        /// <param name="k">Näppäin</param>
        /// <returns>Merkki</returns>
        public static char ToChar( Key k )
        {
            char? ch = ToMaybeChar( k );
            if ( !ch.HasValue ) throw new ArgumentException( k.ToString() + " has no character attached to it." );
            return ch.Value;
        }

        private static char? ToMaybeChar( Key k )
        {
            if ( k == Key.Space ) return ' ';

            string keyStr = k.ToString();
            if ( keyStr.Length == 1 ) return char.ToLower( keyStr[0] );
            if ( keyStr.Length == 2 && keyStr[0] == 'D' ) return keyStr[1];
            if ( keyStr.Length == 7 && keyStr.StartsWith( "NumPad" ) ) return keyStr[6];

            if ( k == Key.OemQuotes ) return 'ä';
            if ( k == Key.OemTilde ) return 'ö';
            if ( k == Key.OemPlus || k == Key.Add ) return '+';
            if ( k == Key.Subtract ) return '-';
            if ( k == Key.Multiply ) return '*';
            if ( k == Key.Divide ) return '/';
            if ( k == Key.Aring ) return 'å';
            if ( k == Key.LessOrGreater ) return '<';

            return null;
        }

        private static Keys ToXnaKeys( Key k )
        {
            return (Keys)k;
        }

        protected override bool IsTriggered( Listener listener )
        {
            if ( listener.Type == ListeningType.KeyboardKey )
            {
                return KeyTriggered( listener );
            }

            if ( listener.Type == ListeningType.KeyboardAll )
            {
                List<Key> keys = KeysTriggered( listener.State );
                return keys.Count > 0;
            }

            return base.IsTriggered( listener );
        }

        private bool KeyTriggered( Listener listener )
        {
            Keys k = ToXnaKeys( listener.Key );

            switch ( listener.State )
            {
                case ButtonState.Irrelevant:
                    return true;

                case ButtonState.Released:
                    return ( oldState.IsKeyDown( k ) && newState.IsKeyUp( k ) );

                case ButtonState.Pressed:
                    return ( oldState.IsKeyUp( k ) && newState.IsKeyDown( k ) );

                case ButtonState.Up:
                    return ( newState.IsKeyUp( k ) );

                case ButtonState.Down:
                    return ( newState.IsKeyDown( k ) );
            }

            return false;
        }

        internal List<Key> KeysTriggered( ButtonState state )
        {
            List<Keys> pressedBefore = new List<Keys>( oldState.GetPressedKeys() );
            List<Keys> pressedNow = new List<Keys>( newState.GetPressedKeys() );
            List<Key> triggered = new List<Key>();

            switch ( state )
            {
                case ButtonState.Released:
                    for ( int i = 0; i < pressedBefore.Count; i++ )
                    {
                        if ( !pressedNow.Contains( pressedBefore[i] ) )
                            triggered.Add( (Key)pressedBefore[i] );
                    }
                    break;

                case ButtonState.Pressed:
                    for ( int i = 0; i < pressedNow.Count; i++ )
                    {
                        if ( !pressedBefore.Contains( pressedNow[i] ) )
                            triggered.Add( (Key)pressedNow[i] );
                    }
                    break;

                case ButtonState.Up:
                    throw new NotImplementedException( "Not implemented" );

                case ButtonState.Down:
                    for ( int i = 0; i < pressedNow.Count; i++ )
                    {
                        triggered.Add( (Key)pressedNow[i] );
                    }
                    break;
            }

            return triggered;
        }

        internal override bool IsBufferEmpty()
        {
            return ( newState.GetPressedKeys().Length == 0 );
        }

        /// <summary>
        /// Ottaa käytöstä poistetun napin <c>k</c> takaisin käyttöön.
        /// </summary>
        public void Enable( Key k )
        {
            base.Enable( listener => ( listener.Key == k ) );
        }

        /// <summary>
        /// Poistaa napin <c>k</c> käytöstä.
        /// </summary>
        public void Disable( Key k )
        {
            base.Disable( listener => ( listener.Key == k ) );
        }

        internal override KeyboardState GetCurrentState()
        {
            return XnaKeyboard.GetState();
        }

        /// <summary>
        /// Palauttaa annetun näppäimen tilan (ks. <c>ButtonState</c>).
        /// </summary>
        /// <param name="k">Näppäin.</param>
        /// <returns>Näppäimen tila</returns>
        public ButtonState GetKeyState( Key k )
        {
            Keys key = ToXnaKeys( k );
            bool down = newState.IsKeyDown( key );
            bool lastdown = oldState.IsKeyDown( key );

            if ( lastdown && down )
                return ButtonState.Down;
            if ( !lastdown && down )
                return ButtonState.Pressed;
            if ( lastdown && !down )
                return ButtonState.Released;

            return ButtonState.Up;
        }

        /// <summary>
        /// Tarkistaa, onko kumpikaan shift-näppäimistä painettuna.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> jos alhaalla, muuten <c>painettuna</c>.
        /// </returns>
        public bool IsShiftDown()
        {
            return newState.IsKeyDown( Keys.LeftShift ) || newState.IsKeyDown( Keys.RightShift );
        }

        /// <summary>
        /// Tarkistaa, onko kumpikaan ctrl-näppäimistä painettuna.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> jos alhaalla, muuten <c>painettuna</c>.
        /// </returns>
        public bool IsCtrlDown()
        {
            return newState.IsKeyDown( Keys.LeftControl ) || newState.IsKeyDown( Keys.RightControl );
        }

        /// <summary>
        /// Tarkistaa, onko kumpikaan alt-näppäimistä painettuna.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> jos alhaalla, muuten <c>painettuna</c>.
        /// </returns>
        public bool IsAltDown()
        {
            return newState.IsKeyDown( Keys.LeftAlt ) || newState.IsKeyDown( Keys.RightAlt );
        }

        internal override string GetControlText( Listener listener )
        {
            if ( listener.Key == Key.Add ) return "NumPad+";
            if ( listener.Key == Key.Subtract ) return "NumPad-";
            if ( listener.Key == Key.Multiply ) return "NumPad*";
            if ( listener.Key == Key.Divide ) return "NumPad/";
            if ( listener.Key == Key.OemQuotes ) return "Ä";
            if ( listener.Key == Key.OemTilde ) return "Ö";
            if ( listener.Key == Key.Left ) return "Left arrow";
            if ( listener.Key == Key.Right ) return "Right arrow";
            if ( listener.Key == Key.Up ) return "Up arrow";
            if ( listener.Key == Key.Down ) return "Down arrow";

            return listener.Key.ToString();
        }

        #region Listen
        public Listener Listen( Key k, ButtonState state, Handler handler, String helpText )
        {
            Listener l = new SimpleListener( this, ListeningType.KeyboardKey, helpText, handler );
            l.Key = k;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener Listen<T1>( Key k, ButtonState state, Handler<T1> handler, String helpText, T1 p1 )
        {
            Listener l = new SimpleListener<T1>( this, ListeningType.KeyboardKey, helpText, handler, p1 );
            l.Key = k;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener Listen<T1, T2>( Key k, ButtonState state, Handler<T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
            Listener l = new SimpleListener<T1, T2>( this, ListeningType.KeyboardKey, helpText, handler, p1, p2 );
            l.Key = k;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener Listen<T1, T2, T3>( Key k, ButtonState state, Handler<T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listener l = new SimpleListener<T1, T2, T3>( this, ListeningType.KeyboardKey, helpText, handler, p1, p2, p3 );
            l.Key = k;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener Listen<T1, T2, T3, T4>( Key k, ButtonState state, Handler<T1, T2, T3, T4> handler, String helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
            Listener l = new SimpleListener<T1, T2, T3, T4>( this, ListeningType.KeyboardKey, helpText, handler, p1, p2, p3, p4 );
            l.Key = k;
            l.State = state;
            Add( l );
            return l;
        }
        #endregion

        #region ListenWSAD
        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public void ListenWSAD( ButtonState state, Handler<Vector> handler, String helpText )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX );
            Listen( Key.D, state, handler, helpText, Vector.UnitX );
        }

        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        public void ListenWSAD<T1>( ButtonState state, Handler<Vector, T1> handler, String helpText, T1 p1 )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY, p1 );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY, p1 );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX, p1 );
            Listen( Key.D, state, handler, helpText, Vector.UnitX, p1 );
        }

        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenWSAD<T1, T2>( ButtonState state, Handler<Vector, T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY, p1, p2 );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY, p1, p2 );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX, p1, p2 );
            Listen( Key.D, state, handler, helpText, Vector.UnitX, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenWSAD<T1, T2, T3>( ButtonState state, Handler<Vector, T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY, p1, p2, p3 );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY, p1, p2, p3 );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX, p1, p2, p3 );
            Listen( Key.D, state, handler, helpText, Vector.UnitX, p1, p2, p3 );
        }
        #endregion

        #region ListenArrows
        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public void ListenArrows( ButtonState state, Handler<Vector> handler, String helpText )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX );
        }

        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        public void ListenArrows<T1>( ButtonState state, Handler<Vector, T1> handler, String helpText, T1 p1 )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY, p1 );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY, p1 );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX, p1 );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX, p1 );
        }

        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenArrows<T1, T2>( ButtonState state, Handler<Vector, T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY, p1, p2 );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY, p1, p2 );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX, p1, p2 );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenArrows<T1, T2, T3>( ButtonState state, Handler<Vector, T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY, p1, p2, p3 );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY, p1, p2, p3 );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX, p1, p2, p3 );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX, p1, p2, p3 );
        }
        #endregion

        #region ListenAll
        public Listener ListenAll( ButtonState state, MultiKeyHandler handler, String helpText )
        {
            Listener l = new MultiKeyListener( this, helpText, handler );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll<T1>( ButtonState state, MultiKeyHandler<T1> handler, String helpText, T1 p1 )
        {
            Listener l = new MultiKeyListener<T1>( this, helpText, handler, p1 );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll<T1, T2>( ButtonState state, MultiKeyHandler<T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
            Listener l = new MultiKeyListener<T1, T2>( this, helpText, handler, p1, p2 );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll<T1, T2, T3>( ButtonState state, MultiKeyHandler<T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listener l = new MultiKeyListener<T1, T2, T3>( this, helpText, handler, p1, p2, p3 );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll<T1, T2, T3, T4>( ButtonState state, MultiKeyHandler<T1, T2, T3, T4> handler, String helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
            Listener l = new MultiKeyListener<T1, T2, T3, T4>( this, helpText, handler, p1, p2, p3, p4 );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll( ButtonState state, Action<Key> handler )
        {
            Listener l = new MultiKeyListener( this, null, delegate( List<Key> keys ) { keys.ForEach( handler ); } );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll<T1>( ButtonState state, Action<Key, T1> handler, T1 p1 )
        {
            Listener l = new MultiKeyListener( this, null, delegate( List<Key> keys ) { foreach ( var k in keys ) handler( k, p1 ); } );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll<T1, T2>( ButtonState state, Action<Key, T1, T2> handler, T1 p1, T2 p2 )
        {
            Listener l = new MultiKeyListener( this, null, delegate( List<Key> keys ) { foreach ( var k in keys ) handler( k, p1, p2 ); } );
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAll<T1, T2, T3>( ButtonState state, Action<Key, T1, T2, T3> handler, T1 p1, T2 p2, T3 p3 )
        {
            Listener l = new MultiKeyListener( this, null, delegate( List<Key> keys ) { foreach ( var k in keys ) handler( k, p1, p2, p3 ); } );
            l.State = state;
            Add( l );
            return l;
        }
        #endregion
    }
}
