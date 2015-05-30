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
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using XnaMouse = Microsoft.Xna.Framework.Input.Mouse;
using XnaMouseState = Microsoft.Xna.Framework.Input.MouseState;
using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace Jypeli.Controls
{
    /// <summary>
    /// Hiiri peliohjaimena.
    /// </summary>
    public class Mouse : Controller<MouseState>, PointingDevice
    {
        private static int wcx = 800;
        private static int wcy = 600;

        /// <summary>
        /// Multitouchia varten.
        /// </summary>
        public int ActiveChannels { get { return 1; } }

        /// <summary>
        /// Käytetäänkö hiiren kursoria.
        /// Jos käytetään, hiiren paikka ruudulla on mitattavissa, mutta hiiri ei
        /// voi liikkua ruudun ulkopuolelle.
        /// Jos ei käytetä, hiirtä voidaan liikuttaa rajatta, mutta sen paikkaa
        /// ruudulla ei voida määrittää.
        /// </summary>
        public bool IsCursorVisible
        {
            get 
            {
#if WINDOWS_PHONE
                return false;
#else
                return Game.Instance.IsMouseVisible; 
#endif
            }
            set { Game.Instance.IsMouseVisible = value; }
        }

        internal static Viewport Viewport
        {
            set
            {
                wcx = value.X + value.Width / 2;
                wcy = value.Y + value.Height / 2;
            }
        }

        /// <summary>
        /// Kursorin paikka ruutukoordinaateissa.
        /// </summary>
        public Vector PositionOnScreen
        {
            get
            {
                return newState.Position;
            }
            set
            {
                newState.Position = value;
                XnaMouse.SetPosition( wcx + (int)value.X, wcy - (int)value.Y );
            }
        }

        /// <summary>
        /// Kursorin paikka maailmankoordinaateissa.
        /// </summary>
        public Vector PositionOnWorld
        {
            get
            {
                return Game.Instance.Camera.ScreenToWorld(PositionOnScreen);
            }
            set
            {
                PositionOnScreen = Game.Instance.Camera.WorldToScreen(value);
            }
        }

        /// <summary>
        /// Rullan asento. Vähenee alaspäin ja kasvaa ylöspäin rullattaessa.
        /// </summary>
        public int WheelState
        {
            get
            {
                return newState.Wheel / 120;
            }
        }

        /// <summary>
        /// Rullan asennon muutos viime tarkistuksesta. Vähenee alaspäin ja kasvaa ylöspäin rullattaessa.
        /// Nolla jos rullaa ei ole käytetty.
        /// </summary>
        public int WheelChange
        {
            get
            {
                return (newState.Wheel - oldState.Wheel) / 120;
            }
        }

        internal Mouse()
        {
            XnaMouse.SetPosition( wcx, wcy );
        }

        internal override bool IsBufferEmpty()
        {
            // TODO: "Dead" tolerance for movement
            return newState.AnyButtonDown;
        }

        public void Enable( MouseButton button )
        {
            base.Enable( listener => ( listener.MouseButton == button ) );
        }

        public void Disable( MouseButton button )
        {
            base.Disable( listener => ( listener.MouseButton == button ) );
        }

        public bool IsTriggeredOnChannel( int channel, Listener listener )
        {
            return IsTriggered( listener );
        }

        protected override bool IsTriggered( Listener listener )
        {
            if ( listener.Type == ListeningType.MouseMovement )
            {
                return false;
            }

            if ( listener.Type == ListeningType.MouseWheel )
            {
                return IsWheelTriggered( listener );
            }

            if ( listener.Type == ListeningType.MouseButton )
            {
                return IsButtonTriggered( listener );
            }

            return base.IsTriggered( listener );
        }

        protected override bool IsAnalogTriggered( Listener listener, out AnalogState state )
        {
            if ( listener.Type == ListeningType.MouseMovement )
            {
                return IsMotionTriggered(listener, out state);
            }

            return base.IsAnalogTriggered(listener, out state);
        }

        private bool IsButtonTriggered( Listener listener )
        {
            Debug.Assert( listener.Type == ListeningType.MouseButton );
            MouseButton button = listener.MouseButton;

            if ( listener.GameObject != null && !IsCursorOn( listener.GameObject ) )
                return false;

            return listener.State == ButtonState.Irrelevant || MouseState.GetButtonState( oldState, newState, button ) == listener.State;
        }

        private bool IsWheelTriggered( Listener listener )
        {
            return WheelChange != 0;
        }

        private bool IsMotionTriggered( Listener listener, out AnalogState state )
        {
            Debug.Assert( listener.Type == ListeningType.MouseMovement );
            double xMovement = newState.X - oldState.X;
            double yMovement = newState.Y - oldState.Y;
            Vector movement = new Vector( xMovement, yMovement );

            if ( movement.Magnitude < listener.MovementTrigger )
            {
                state = new AnalogState();
                return false;
            }

            state = new AnalogState( listener.State, movement );
            return true;
        }

        internal override MouseState GetCurrentState()
        {
            MouseState state = GetMouseState();

            if ( !IsCursorVisible )
            {
                // Accumulate relative coordinates to get absolute ones
                state.X += oldState.X;
                state.Y += oldState.Y;
            }

            return state;
        }

        internal MouseState GetMouseState()
        {
            XnaMouseState xnaState = XnaMouse.GetState();
            MouseState state = new MouseState();

            state.X = xnaState.X - wcx;
            state.Y = -( xnaState.Y - wcy );
            state.LeftDown = xnaState.LeftButton == XnaButtonState.Pressed;
            state.RightDown = xnaState.RightButton == XnaButtonState.Pressed;
            state.MiddleDown = xnaState.MiddleButton == XnaButtonState.Pressed;
            state.X1Down = xnaState.XButton1 == XnaButtonState.Pressed;
            state.X2Down = xnaState.XButton2 == XnaButtonState.Pressed;
            state.Wheel = xnaState.ScrollWheelValue;

            if ( !IsCursorVisible )
            {
                // Reset the mouse to the center of the screen
                XnaMouse.SetPosition( wcx, wcy );
            }

            return state;
        }

        internal override string GetControlText( Listener listener )
        {
            switch ( listener.Type )
            {
                case ListeningType.MouseButton:
                    return listener.MouseButton.ToString() + " mouse";
                case ListeningType.MouseMovement:
                    return "Mouse";
                case ListeningType.MouseWheel:
                    return "Mouse wheel";
                default:
                    Debug.Assert( false, "Bad listener type for mouse" );
                    return null;
            }
        }

        /// <summary>
        /// Onko hiiren kursori annetun olion päällä.
        /// </summary>
        public bool IsCursorOn( IGameObject obj )
        {
#if WINDOWS_PHONE
            return false;
#else
            return obj.IsInside( Game.Instance.Camera.ScreenToWorld(this.PositionOnScreen, obj.Layer) );
#endif
        }


        #region Listen with no parameters

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <param name="b">Kuunneltava nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public Listener Listen( MouseButton b, ButtonState state, Handler handler, string helpText )
        {
#if WINDOWS
            Listener l = new SimpleListener( this, ListeningType.MouseButton, helpText, handler );
            l.MouseButton = b;
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="b">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public Listener ListenOn( IGameObject obj, MouseButton b, ButtonState state, Handler handler, String helpText )
        {
#if WINDOWS
            Listener l = new SimpleListener( this, ListeningType.MouseButton, helpText, handler );
            l.MouseButton = b;
            l.State = state;
            l.GameObject = obj;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <param name="trigger">Kuinka pitkän matkan hiiren tulisi liikkua, että tulee tapahtuma.</param>
        /// <param name="handler">Hiiren tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public Listener ListenMovement( double trigger, AnalogHandler handler, string helpText )
        {
#if WINDOWS
            Listener l = new AnalogListener( this, ListeningType.MouseMovement, helpText, handler );
            l.MovementTrigger = trigger;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        /// <summary>
        /// Kuuntelee hiiren rullaa.
        /// </summary>
        /// <param name="handler">Hiiren rullan tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public Listener ListenWheel(Handler handler, string helpText)
        {
#if WINDOWS
            Listener l = new SimpleListener(this, ListeningType.MouseWheel, helpText, handler);
            Add(l);
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 1 parameter

        public Listener Listen<T1>( MouseButton b, ButtonState state, Handler<T1> handler, string helpText, T1 p1 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1>( this, ListeningType.MouseButton, helpText, handler, p1 );
            l.MouseButton = b;
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1>( IGameObject obj, MouseButton b, ButtonState state, Handler<T1> handler, String helpText, T1 p1 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1>( this, ListeningType.MouseButton, helpText, handler, p1 );
            l.MouseButton = b;
            l.State = state;
            l.GameObject = obj;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenWheel<T1>(Handler<T1> handler, string helpText, T1 p1)
        {
#if WINDOWS
            Listener l = new SimpleListener<T1>(this, ListeningType.MouseWheel, helpText, handler, p1);
            Add(l);
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenMovement<T1>( double trigger, AnalogHandler<T1> handler, string helpText, T1 p1 )
        {
#if WINDOWS
            Listener l = new AnalogListener<T1>( this, ListeningType.MouseMovement, helpText, handler, p1 );
            l.MovementTrigger = trigger;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 2 parameters

        public Listener Listen<T1, T2>( MouseButton b, ButtonState state, Handler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2>( this, ListeningType.MouseButton, helpText, handler, p1, p2 );
            l.MouseButton = b;
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1, T2>( IGameObject obj, MouseButton b, ButtonState state, Handler<T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2>( this, ListeningType.MouseButton, helpText, handler, p1, p2 );
            l.MouseButton = b;
            l.State = state;
            l.GameObject = obj;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenWheel<T1, T2>(Handler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2>(this, ListeningType.MouseWheel, helpText, handler, p1, p2);
            Add(l);
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenMovement<T1, T2>( double trigger, AnalogHandler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
#if WINDOWS
            Listener l = new AnalogListener<T1, T2>( this, ListeningType.MouseMovement, helpText, handler, p1, p2 );
            l.MovementTrigger = trigger;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 3 parameters

        public Listener Listen<T1, T2, T3>( MouseButton b, ButtonState state, Handler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2, T3>( this, ListeningType.MouseButton, helpText, handler, p1, p2, p3 );
            l.MouseButton = b;
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1, T2, T3>( IGameObject obj, MouseButton b, ButtonState state, Handler<T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2, T3>( this, ListeningType.MouseButton, helpText, handler, p1, p2, p3 );
            l.MouseButton = b;
            l.State = state;
            l.GameObject = obj;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenWheel<T1, T2, T3>(Handler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2, T3>(this, ListeningType.MouseWheel, helpText, handler, p1, p2, p3);
            Add(l);
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenMovement<T1, T2, T3>( double trigger, AnalogHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
#if WINDOWS
            Listener l = new AnalogListener<T1, T2, T3>( this, ListeningType.MouseMovement, helpText, handler, p1, p2, p3 );
            l.MovementTrigger = trigger;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 4 parameters

        public Listener Listen<T1, T2, T3, T4>( MouseButton b, ButtonState state, Handler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2, T3, T4>( this, ListeningType.MouseButton, helpText, handler, p1, p2, p3, p4 );
            l.MouseButton = b;
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1, T2, T3, T4>( IGameObject obj, MouseButton b, ButtonState state, Handler<T1, T2, T3, T4> handler, String helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2, T3, T4>( this, ListeningType.MouseButton, helpText, handler, p1, p2, p3, p4 );
            l.MouseButton = b;
            l.State = state;
            l.GameObject = obj;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenWheel<T1, T2, T3, T4>(Handler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
#if WINDOWS
            Listener l = new SimpleListener<T1, T2, T3, T4>(this, ListeningType.MouseWheel, helpText, handler, p1, p2, p3, p4);
            Add(l);
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenMovement<T1, T2, T3, T4>( double trigger, AnalogHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
#if WINDOWS
            Listener l = new AnalogListener<T1, T2, T3, T4>( this, ListeningType.MouseMovement, helpText, handler, p1, p2, p3, p4 );
            l.MovementTrigger = trigger;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion
    } 

    public struct MouseState
    {
        static MouseButton[] buttons =
            { MouseButton.Left, MouseButton.Right, MouseButton.Middle,
              MouseButton.XButton1, MouseButton.XButton2 };

        static ButtonState[] states =
            { ButtonState.Up, ButtonState.Pressed,
                ButtonState.Released, ButtonState.Down };

        public double X;
        public double Y;
        public int ButtonMask;
        public int Wheel;

        public Vector Position
        {
            get { return new Vector( X, Y ); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public bool LeftDown
        {
            get { return ( ButtonMask & 1 ) > 0; }
            set { if ( value ) ButtonMask |= 1; else ButtonMask &= ~1; }
        }

        public bool RightDown
        {
            get { return ( ButtonMask & 2 ) > 0; }
            set { if ( value ) ButtonMask |= 2; else ButtonMask &= ~2; }
        }

        public bool MiddleDown
        {
            get { return ( ButtonMask & 4 ) > 0; }
            set { if ( value ) ButtonMask |= 4; else ButtonMask &= ~4; }
        }

        public bool X1Down
        {
            get { return ( ButtonMask & 8 ) > 0; }
            set { if ( value ) ButtonMask |= 8; else ButtonMask &= ~8; }
        }

        public bool X2Down
        {
            get { return ( ButtonMask & 16 ) > 0; }
            set { if ( value ) ButtonMask |= 16; else ButtonMask &= ~16; }
        }

        public bool AnyButtonDown
        {
            get { return ButtonMask > 0; }
        }               

        internal bool IsButtonDown( MouseButton button )
        {
            int bi = 1;

            for ( int i = 0; i < 5; i++ )
            {
                if ( button == buttons[i] )
                    return ( ButtonMask & bi ) > 0;

                bi *= 2;
            }

            return false;
        }

        internal static ButtonState GetButtonState( MouseState oldState, MouseState newState, MouseButton button )
        {
            int oldDown = oldState.IsButtonDown( button ) ? 2 : 0;
            int newDown = newState.IsButtonDown( button ) ? 1 : 0;
            return states[oldDown + newDown];
        }
    }
}
