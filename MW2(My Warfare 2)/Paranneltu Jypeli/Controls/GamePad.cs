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

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Physics2DDotNet;

using XnaGamePad = Microsoft.Xna.Framework.Input.GamePad;

namespace Jypeli.Controls
{
    /// <summary>
    /// Xbox-peliohjain.
    /// </summary>
    public class GamePad : Controller<GamePadState>
    {
        internal class Vibration
        {
            public PlayerIndex player { get; private set; }
            public double leftMotor { get; private set; }
            public double rightMotor { get; private set; }
            public double leftAccel { get; set; }
            public double rightAccel { get; set; }
            public Lifespan lifetime { get; set; }

            public Vibration( PlayerIndex p, double lmotor, double rmotor, double laccel, double raccel, Lifespan ltime )
            {
                player = p;
                leftMotor = lmotor;
                rightMotor = rmotor;
                leftAccel = laccel;
                rightAccel = raccel;
                lifetime = ltime;
            }

            public void Update( Time time )
            {
                if ( lifetime.IsExpired )
                {
                    leftMotor = 0;
                    rightMotor = 0;
                    return;
                }

                // Acceleration
                leftMotor += time.SinceLastUpdate.TotalSeconds * leftAccel;
                rightMotor += time.SinceLastUpdate.TotalSeconds * rightAccel;

                // Lifetime progression
                lifetime.Age += (float)time.SinceLastUpdate.TotalSeconds;
            }

            public static void UpdateAll( Time time, List<Vibration> vibrations )
            {
                for ( int i = 0; i < vibrations.Count; i++ )
                {
                    vibrations[i].Update( time );
                }
            }

            public static void Execute( PlayerIndex p, List<Vibration> vibrations )
            {
                // Total vibrations
                double lmotort = 0;
                double rmotort = 0;

                for ( int i = 0; i < vibrations.Count; i++ )
                {
                    // Get the sum of all vibrations
                    if ( vibrations[i].player == p )
                    {
                        lmotort += vibrations[i].leftMotor;
                        rmotort += vibrations[i].rightMotor;
                    }
                }

                // Clamp the results between 0 and 1
                lmotort = AdvanceMath.MathHelper.Clamp( (float)lmotort, 0, 1 );
                rmotort = AdvanceMath.MathHelper.Clamp( (float)rmotort, 0, 1 );

                // Set the vibration
                XnaGamePad.SetVibration( p, (float)lmotort, (float)rmotort );
            }
        }

        private List<Vibration> vibrations;

        private PlayerIndex playerIndex;

        /// <summary>
        /// Vasemman tatin suuntavektori.
        /// Vaihtelee välillä (-1, -1) - (1, 1)
        /// </summary>
        public Vector LeftThumbDirection
        {
            get 
            {
                Vector2 v = newState.ThumbSticks.Left;
                return new Vector( v.X, v.Y );
            }
        }

        /// <summary>
        /// Oikean tatin suuntavektori.
        /// Vaihtelee välillä (-1, -1) - (1, 1)
        /// </summary>
        public Vector RightThumbDirection
        {
            get 
            {
                Vector2 v = newState.ThumbSticks.Right;
                return new Vector( v.X, v.Y );
            }
        }

        /// <summary>
        /// Vasemman liipaisimen tila.
        /// Vaihtelee välillä 0 - 1.
        /// </summary>
        public double LeftTriggerState
        {
            get { return newState.Triggers.Left; }
        }

        /// <summary>
        /// Oikean liipaisimen tila.
        /// Vaihtelee välillä 0 - 1.
        /// </summary>
        public double RightTriggerState
        {
            get { return newState.Triggers.Right; }
        }

        internal GamePad( PlayerIndex player )
            : base()
        {
            oldState = XnaGamePad.GetState( player );
            vibrations = new List<Vibration>();
            playerIndex = player;
        }

        /// <summary>
        /// Ottaa käytöstä poistetun napin takaisin käyttöön.
        /// </summary>
        public void Enable( Button button )
        {
            base.Enable( listener => ( listener.Button == button ) );
        }

        /// <summary>
        /// Ottaa käytöstä poistetun analogiohjaimen takaisin käyttöön.
        /// </summary>
        public void Enable( AnalogControl control )
        {
            base.Enable( listener => ( listener.AnalogControl == control ) );
        }

        /// <summary>
        /// Poistaa napin käytöstä.
        /// </summary>
        public void Disable( Button button )
        {
            base.Disable( listener => ( listener.Button == button ) );
        }

        /// <summary>
        /// Poistaa analogiohjaimen (tikku tai nappi) käytöstä.
        /// </summary>
        public void Disable( AnalogControl control )
        {
            base.Disable( listener => ( listener.AnalogControl == control ) );
        }

        protected override bool IsTriggered( Listener listener )
        {
            if ( listener.Type == ListeningType.ControllerButton )
                return IsButtonTriggered( listener );

            return base.IsTriggered( listener );
        }

        protected override bool IsAnalogTriggered( Listener listener, out AnalogState state )
        {
            if ( listener.Type == ListeningType.ControllerAnalogMovement )
                return IsMovementTriggered( listener, out state );

            return base.IsAnalogTriggered( listener, out state );
        }

        private bool IsButtonTriggered( Listener listener )
        {
            Buttons b = ToXnaButtons( listener.Button );

            switch ( listener.State )
            {
                case ButtonState.Irrelevant:
                    return true;
                case ButtonState.Released:
                    return oldState.IsButtonDown( b ) && newState.IsButtonUp( b );
                case ButtonState.Pressed:
                    return oldState.IsButtonUp( b ) && newState.IsButtonDown( b );
                case ButtonState.Up:
                    return newState.IsButtonUp( b );
                case ButtonState.Down:
                    return newState.IsButtonDown( b );
                default:
                    return false;
            }
        }

        private bool IsMovementTriggered( Listener listener, out AnalogState analogState )
        {
            switch ( listener.AnalogControl )
            {
                case AnalogControl.DefaultStick:
                case AnalogControl.LeftStick:
                    {
                        Vector2 current = newState.ThumbSticks.Left;
                        if ( current.Length() > listener.AnalogTrigger )
                        {
                            Vector2 previous = oldState.ThumbSticks.Left;
                            double change = ( current - previous ).Length();
                            analogState = new AnalogState( 0.0, change, new Vector( current.X, current.Y ) );
                            return true;
                        }
                        break;
                    }
                case AnalogControl.RightStick:
                    {
                        Vector2 current = newState.ThumbSticks.Right;
                        if ( current.Length() > listener.AnalogTrigger )
                        {
                            Vector2 previous = oldState.ThumbSticks.Right;
                            double change = ( current - previous ).Length();
                            analogState = new AnalogState( 0.0, change, new Vector( current.X, current.Y ) );
                            return true;
                        }
                        break;
                    }
                case AnalogControl.LeftTrigger:
                    {
                        double currentState = newState.Triggers.Left;
                        if ( currentState > listener.AnalogTrigger )
                        {
                            double change = currentState - oldState.Triggers.Left;
                            analogState = new AnalogState( currentState, change, Vector.Zero );
                            return true;
                        }
                        break;
                    }
                case AnalogControl.RightTrigger:
                    {
                        double currentState = newState.Triggers.Right;
                        if ( currentState > listener.AnalogTrigger )
                        {
                            double change = currentState - oldState.Triggers.Right;
                            analogState = new AnalogState( currentState, change, Vector.Zero );
                            return true;
                        }
                        break;
                    }
                default:
                    break;
            }

            analogState = new AnalogState();
            return false;
        }

        internal override GamePadState GetCurrentState()
        {
            return XnaGamePad.GetState( playerIndex );
        }

        private static Buttons ToXnaButtons( Button b )
        {
            return (Buttons)b;
        }

        internal override bool IsBufferEmpty()
        {
            // TODO: "Dead" tolerance for axes and triggers
            return AnyButtonDown();
        }

        internal override void Update()
        {
            if ( !Enabled )
                return;

            Vibration.UpdateAll( Game.Time, vibrations );
            Vibration.Execute( playerIndex, vibrations );

            for ( int i = 0; i < vibrations.Count; i++ )
            {
                if ( vibrations[i].lifetime.IsExpired )
                {
                    vibrations.RemoveAt( i );
                    i--;
                }
            }

            base.Update();
        }

        internal override string GetControlText( Listener listener )
        {
            switch ( listener.Type )
            {
                case ListeningType.ControllerButton:
                    return listener.Button.ToString();
                case ListeningType.ControllerAnalogMovement:
                    return listener.AnalogControl.ToString();
                default:
                    Debug.Assert( false, "Bad listener type for gamepad" );
                    return null;
            }
        }

        /// <summary>
        /// Palauttaa, onko yksikään ohjaimen nappi alhaalla.
        /// </summary>
        /// <returns><c>bool</c></returns>
        internal bool AnyButtonDown()
        {
            Button[] buttons =
            {
                Button.DPadUp, Button.DPadDown, Button.DPadLeft, Button.DPadRight,
                Button.A, Button.B, Button.X, Button.Y,
                Button.Start, Button.Back, Button.BigButton,
                Button.LeftShoulder, Button.RightShoulder
            };
            bool buttondown = false;

            for ( int i = 0; i < buttons.Length; i++ )
            {
                buttondown |= ( (GamePadState)newState ).IsButtonDown( ToXnaButtons( buttons[i] ) );
            }

            return buttondown;
        }

        /// <summary>
        /// Täristää peliohjainta.
        /// </summary>
        /// <param name="leftMotor">Vasemmanpuoleisen moottorin tärinän määrä (maksimi 1).</param>
        /// <param name="rightMotor">Oikeanpuoleisen moottorin tärinän määrä (maksimi 1) .</param>
        /// <param name="leftAcceleration">Vasemmanpuoleisen moottorin tärinäkiihtyvyys (yksikköä sekunnissa).</param>
        /// <param name="rightAcceleration">Oikeanpuoleisen moottorin tärinäkiihtyvyys (yksikköä sekunnissa).</param>
        /// <param name="time">Aika, jonka tärinä kestää (sekunteina).</param>
        public void Vibrate( double leftMotor, double rightMotor, double leftAcceleration, double rightAcceleration, double time )
        {
            vibrations.Add( new Vibration( playerIndex, leftMotor, rightMotor, leftAcceleration, rightAcceleration, new Lifespan( (float)time ) ) );
        }

        #region Listen with no parameters

        public Listener Listen( Button button, ButtonState state, Handler handler, string helpText )
        {
            Listener l = new SimpleListener( this, ListeningType.ControllerButton, helpText, handler );
            l.Button = button;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAnalog( AnalogControl control, double trigger, AnalogHandler handler, string helpText )
        {
            Listener l = new AnalogListener( this, ListeningType.ControllerAnalogMovement, helpText, handler );
            l.AnalogControl = control;
            l.AnalogTrigger = trigger;
            Add( l );
            return l;
        }

        #endregion

        #region Listen with 1 parameter

        public Listener Listen<T1>( Button button, ButtonState state, Handler<T1> handler, string helpText, T1 p1 )
        {
            Listener l = new SimpleListener<T1>( this, ListeningType.ControllerButton, helpText, handler, p1 );
            l.Button = button;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAnalog<T1>( AnalogControl control, double trigger, AnalogHandler<T1> handler, string helpText, T1 p1 )
        {
            Listener l = new AnalogListener<T1>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1 );
            l.AnalogControl = control;
            l.AnalogTrigger = trigger;
            Add( l );
            return l;
        }

        #endregion

        #region Listen with 2 parameters

        public Listener Listen<T1, T2>( Button button, ButtonState state, Handler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            Listener l = new SimpleListener<T1, T2>( this, ListeningType.ControllerButton, helpText, handler, p1, p2 );
            l.Button = button;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAnalog<T1, T2>( AnalogControl control, double trigger, AnalogHandler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            Listener l = new AnalogListener<T1, T2>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1, p2 );
            l.AnalogControl = control;
            l.AnalogTrigger = trigger;
            Add( l );
            return l;
        }

        #endregion

        #region Listen with 3 parameters

        public Listener Listen<T1, T2, T3>( Button button, ButtonState state, Handler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listener l = new SimpleListener<T1, T2, T3>( this, ListeningType.ControllerButton, helpText, handler, p1, p2, p3 );
            l.Button = button;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAnalog<T1, T2, T3>( AnalogControl control, double trigger, AnalogHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listener l = new AnalogListener<T1, T2, T3>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1, p2, p3 );
            l.AnalogControl = control;
            l.AnalogTrigger = trigger;
            Add( l );
            return l;
        }

        #endregion

        #region Listen with 4 parameters

        public Listener Listen<T1, T2, T3, T4>( Button button, ButtonState state, Handler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
            Listener l = new SimpleListener<T1, T2, T3, T4>( this, ListeningType.ControllerButton, helpText, handler, p1, p2, p3, p4 );
            l.Button = button;
            l.State = state;
            Add( l );
            return l;
        }

        public Listener ListenAnalog<T1, T2, T3, T4>( AnalogControl control, double trigger, AnalogHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
            Listener l = new AnalogListener<T1, T2, T3, T4>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1, p2, p3, p4 );
            l.AnalogControl = control;
            l.AnalogTrigger = trigger;
            Add( l );
            return l;
        }

        #endregion
    }
}
