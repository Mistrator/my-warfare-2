#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyväskylä, Department of Mathematical
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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;

using XnaTouchPanel = Microsoft.Xna.Framework.Input.Touch.TouchPanel;
using XnaGestureType = Microsoft.Xna.Framework.Input.Touch.GestureType;
#endif

using Jypeli.Controls;
using System.Linq;


namespace Jypeli.WP7
{
    /// <summary>
    /// Kosketuspaneeli.
    /// </summary>
    public class TouchPanel : Controller<TouchPanelState>, PointingDevice
    {
        private ListenContext _snipContext = null;
        private ListenContext _pinchContext = null;
        private ListenContext _keyEmulationContext = null;

        /// <summary>
        /// Kuinka monta kosketusta näytöllä on aktiivisena.
        /// </summary>
        public int ActiveChannels
        {
            get { return newState.ActiveTouches.Count + newState.ActiveGestures.Count; }
        }

        /// <summary>
        /// Kuinka monta kosketusta näytöllä on aktiivisena.
        /// </summary>
        public int NumTouches
        {
            get { return newState.ActiveTouches.Count; }
        }

        /// <summary>
        /// Kuinka monta elettä näytöllä on aktiivisena.
        /// </summary>
        public int NumGestures
        {
            get { return newState.ActiveGestures.Count; }
        }

        internal override bool IsBufferEmpty()
        {
            return true;
        }

        /// <summary>
        /// Seurataanko kosketusta kameralla.
        /// </summary>
        public bool FollowSnipping
        {
            get { return _snipContext != null; }
            set
            {
                TouchHandler handler = delegate( Touch t ) { Game.Instance.Camera.Position -= t.MovementOnWorld; };
                ContextHandler op = delegate( ListenContext ctx ) { Listen( ButtonState.Down, handler, "Move around" ).InContext( ctx ); };
                setContext( ref _snipContext, value, op );
            }
        }

        /// <summary>
        /// Zoomataanko kameralla kun käyttäjä tekee nipistyseleen
        /// </summary>
        public bool FollowPinching
        {
            get { return _pinchContext != null; }
            set
            {
                TouchHandler handler = delegate( Touch t )
                {
                    Gesture g = (Gesture)t;
                    Game.Instance.Camera.Zoom( g.WorldDistanceAfter.Magnitude / g.WorldDistanceBefore.Magnitude );
                };
                ContextHandler op = delegate( ListenContext ctx ) { ListenGesture( GestureType.Pinch, handler, "Zoom" ); };
                setContext( ref _pinchContext, value, op );
            }
        }

        /// <summary>
        /// Matkitaanko näppäimistöä. Jos päällä, ruudun reunojen kosketus vastaa nuolinäppäinten
        /// painalluksia ja ruudun keskeltä painaminen välilyöntiä.
        /// </summary>
        public bool EmulateKeyboard
        {
            get { return _keyEmulationContext != null; }
            set
            {
                ContextHandler op = delegate( ListenContext ctx ) { Listen( ButtonState.Down, DoEmulateKeyboard, null ).InContext( ctx ); };
                setContext( ref _keyEmulationContext, value, op );
            }
        }

        delegate void ContextHandler( ListenContext ctx );

        private void setContext( ref ListenContext context, bool enable, ContextHandler operation )
        {
            if ( ( context != null ) == enable ) return;
            if ( enable )
            {
                context = Game.Instance.ControlContext.CreateSubcontext();
                context.Active = true;
                operation( context );
            }
            else
            {
                context.Destroy();
                context = null;
            }
        }

        public override void Clear()
        {
            bool pinch = this.FollowPinching;
            bool snip = this.FollowSnipping;
            bool kbem = this.EmulateKeyboard;

            this.FollowPinching = false;
            this.FollowSnipping = false;
            this.EmulateKeyboard = false;

            base.Clear();

            this.FollowPinching = pinch;
            this.FollowSnipping = snip;
            this.EmulateKeyboard = kbem;
        }

        /// <summary>
        /// Ottaa käytöstä poistetun kosketuskontrollin <c>k</c> takaisin käyttöön.
        /// </summary>
        /// <param name="state">Kosketuksen tila</param>
        public void Enable( ButtonState state )
        {
            base.Enable( listener => ( listener.State == state ) );
        }

        /// <summary>
        /// Poistaa kosketuskontrollin <c>k</c> käytöstä.
        /// </summary>
        /// <param name="state">Kosketuksen tila</param>
        public void Disable( ButtonState state )
        {
            base.Disable( listener => ( listener.State == state ) );
        }

        internal override string GetControlText( Listener listener )
        {
            if ( listener.Type != ListeningType.Touch && listener.Type != ListeningType.TouchGesture )
                Debug.Assert( false, "Bad listener type for touch panel" );
            return "Touch panel";
        }

        internal override TouchPanelState GetCurrentState()
        {
#if WINDOWS_PHONE
            List<GestureSample> samples = new List<GestureSample>();
            if ( XnaTouchPanel.EnabledGestures != XnaGestureType.None )
            {
                while ( XnaTouchPanel.IsGestureAvailable )
                    samples.Add( XnaTouchPanel.ReadGesture() );

            }
            
            return new TouchPanelState( XnaTouchPanel.GetState(), samples, newState );
#else
            return new TouchPanelState();
#endif
        }

        public bool IsTriggeredOnChannel( int channel, Listener listener )
        {
#if WINDOWS_PHONE
            Debug.Assert( ( listener.Type == ListeningType.Touch ) || ( listener.Type == ListeningType.TouchGesture ) );

            if ( !listener.Context.Active )
                return false;

            if ( listener.gestureType == null )
            {
                // Normal touch
                if ( newState.ActiveTouches.Count <= channel )
                    return false;

                if ( listener.GameObject != null )
                {
                    Vector touchOnScreen = newState.ActiveTouches[channel].PositionOnScreen;
                    Vector touchOnWorld = Game.Instance.Camera.ScreenToWorld( touchOnScreen, listener.GameObject.Layer );

                    if ( !listener.GameObject.IsInside( touchOnWorld ) )
                        return false;
                }

                switch ( newState.ActiveTouches[channel].State )
                {
                    case TouchLocationState.Moved:
                        return listener.State == ButtonState.Down;

                    case TouchLocationState.Pressed:
                        return listener.State == ButtonState.Pressed;

                    case TouchLocationState.Released:
                        return listener.State == ButtonState.Released;
                }
            }
            else
            {
                // Gesture
                int gIndex = channel - newState.ActiveTouches.Count;
                if ( gIndex < 0 || newState.ActiveGestures.Count <= gIndex )
                    return false;

                if ( listener.GameObject != null && !listener.GameObject.IsInside( newState.ActiveGestures[gIndex].PositionOnWorld ) )
                    return false;

                return newState.samples.Any( s => s.GestureType == (XnaGestureType)listener.gestureType.Value );
            }
#endif

            return false;
        }
        
        private void DoEmulateKeyboard( Touch touch )
        {
            if ( Game.Instance == null ) return;

            if ( touch.PositionOnScreen.X > Game.Screen.Width / 4 )
            {
                // Right
                Game.Controls.Keyboard.listeners.FindAll( l => l.Key == Key.Right ).ForEach( Listener.Invoke );
            }

            else if ( touch.PositionOnScreen.X < -Game.Screen.Width / 4 )
            {
                // Left
                Game.Controls.Keyboard.listeners.FindAll( l => l.Key == Key.Left ).ForEach( Listener.Invoke );
            }

            if ( touch.PositionOnScreen.Y > Game.Screen.Height / 4 )
            {
                // Up
                Game.Controls.Keyboard.listeners.FindAll( l => l.Key == Key.Up ).ForEach( Listener.Invoke );
            }

            else if ( touch.PositionOnScreen.Y < -Game.Screen.Height / 4 )
            {
                // Down
                Game.Controls.Keyboard.listeners.FindAll( l => l.Key == Key.Down ).ForEach( Listener.Invoke );
            }

            else if ( touch.PositionOnScreen.Magnitude < 150 )
            {
                // Space
                Game.Controls.Keyboard.listeners.FindAll( l => l.Key == Key.Space ).ForEach( Listener.Invoke );
            }
        }

        #region Listen with no parameters

        public Listener Listen( ButtonState state, TouchHandler handler, string helpText )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener( this, ListeningType.Touch, helpText, handler );
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGesture( GestureType type, TouchHandler handler, string helpText )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener( this, ListeningType.TouchGesture, helpText, handler );
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGestureOn( GameObject o, GestureType type, TouchHandler handler, string helpText )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener( this, ListeningType.TouchGesture, helpText, handler );
            l.GameObject = o;
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn( IGameObject o, ButtonState state, TouchHandler handler, String helpText )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener( this, ListeningType.Touch, helpText, handler );
            l.State = state;
            l.GameObject = o;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 1 parameter

        public Listener Listen<T1>( ButtonState state, TouchHandler<T1> handler, string helpText, T1 p1 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1>( this, ListeningType.Touch, helpText, handler, p1 );
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGesture<T1>( GestureType type, TouchHandler<T1> handler, string helpText, T1 p1 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1>( this, ListeningType.TouchGesture, helpText, handler, p1 );
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGestureOn<T1>( GameObject o, GestureType type, TouchHandler<T1> handler, string helpText, T1 p1 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1>( this, ListeningType.TouchGesture, helpText, handler, p1 );
            l.GameObject = o;
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1>( IGameObject o, ButtonState state, TouchHandler<T1> handler, String helpText, T1 p1 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1>( this, ListeningType.Touch, helpText, handler, p1 );
            l.State = state;
            l.GameObject = o;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 2 parameters

        public Listener Listen<T1, T2>( ButtonState state, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2>( this, ListeningType.Touch, helpText, handler, p1, p2 );
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGesture<T1, T2>( GestureType type, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2>( this, ListeningType.TouchGesture, helpText, handler, p1, p2 );
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGestureOn<T1, T2>( GameObject o, GestureType type, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2>( this, ListeningType.TouchGesture, helpText, handler, p1, p2 );
            l.GameObject = o;
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1, T2>( IGameObject o, ButtonState state, TouchHandler<T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2>( this, ListeningType.Touch, helpText, handler, p1, p2 );
            l.State = state;
            l.GameObject = o;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 3 parameters

        public Listener Listen<T1, T2, T3>( ButtonState state, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3>( this, ListeningType.Touch, helpText, handler, p1, p2, p3 );
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGesture<T1, T2, T3>( GestureType type, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3>( this, ListeningType.TouchGesture, helpText, handler, p1, p2, p3 );
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGestureOn<T1, T2, T3>( GameObject o, GestureType type, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3>( this, ListeningType.TouchGesture, helpText, handler, p1, p2, p3 );
            l.GameObject = o;
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1, T2, T3>( IGameObject o, ButtonState state, TouchHandler<T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3>( this, ListeningType.Touch, helpText, handler, p1, p2, p3 );
            l.State = state;
            l.GameObject = o;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion

        #region Listen with 4 parameters

        public Listener Listen<T1, T2, T3, T4>( ButtonState state, TouchHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3, T4>( this, ListeningType.Touch, helpText, handler, p1, p2, p3, p4 );
            l.State = state;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGesture<T1, T2, T3, T4>( GestureType type, TouchHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3, T4>( this, ListeningType.TouchGesture, helpText, handler, p1, p2, p3, p4 );
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenGestureOn<T1, T2, T3, T4>( GameObject o, GestureType type, TouchHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3, T4>( this, ListeningType.TouchGesture, helpText, handler, p1, p2, p3, p4 );
            l.GameObject = o;
            l.gestureType = type;
            Add( l );
            XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return l;
#else
            return Listener.Null;
#endif
        }

        public Listener ListenOn<T1, T2, T3, T4>( IGameObject o, ButtonState state, TouchHandler<T1, T2, T3, T4> handler, String helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
#if WINDOWS_PHONE
            Listener l = new TouchListener<T1, T2, T3, T4>( this, ListeningType.Touch, helpText, handler, p1, p2, p3, p4 );
            l.State = state;
            l.GameObject = o;
            Add( l );
            return l;
#else
            return Listener.Null;
#endif
        }

        #endregion
    }
}
