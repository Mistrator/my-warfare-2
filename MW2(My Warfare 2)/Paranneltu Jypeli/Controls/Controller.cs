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
using System.Collections.Generic;
using System.Text;

namespace Jypeli.Controls
{
    /// <summary>
    /// Yleinen peliohjainluokka.
    /// </summary>
    [Save]
    public abstract class Controller
    {
        protected enum ListenerActionType { Add, Remove, Enable, Disable, RemoveAll };

        protected struct ListenerAction
        {
            public ListenerActionType actionType;
            public Listener listener;

            public ListenerAction( Listener l, ListenerActionType a )
            {
                actionType = a;
                listener = l;
            }
        }

        protected class ListenerPrecedenceComparer : IComparer<Listener>
        {
            public int Compare( Listener x, Listener y )
            {
                // 1. Gestures first
                if ( x.gestureType != null && y.gestureType == null ) return -1;
                if ( x.gestureType == null && y.gestureType != null ) return 1;

                // 2. Then the game objects, top-down layerwise
                if ( x.GameObject != null && y.GameObject != null )
                {
                    int xi = Game.Instance.Layers.IndexOf( x.GameObject.Layer );
                    int yi = Game.Instance.Layers.IndexOf( y.GameObject.Layer );
                    return yi.CompareTo( xi );
                }
                if ( x.GameObject != null ) return -1;
                if ( y.GameObject != null ) return 1;

                return 0;
            }
        }

        protected static ListenerPrecedenceComparer ListenerComparer = new ListenerPrecedenceComparer();

        [Save]
        internal List<Listener> listeners = new List<Listener>();
        protected Queue<ListenerAction> listenerActions = new Queue<ListenerAction>();
        protected List<Listener> disabledListeners = new List<Listener>();
        protected Dictionary<Delegate, string> helpTextsForHandlers = new Dictionary<Delegate, string>();

        /// <summary>
        /// Onko puskuri parhaillaan tyhjentymässä.
        /// </summary>
        internal bool BufferPurging { get; set; }

        /// <summary>
        /// Kuunnellaanko ohjainta.
        /// </summary>
        public bool Enabled { get; set; }

        internal Controller()
        {
            Enabled = true;
        }

        internal void Add( Listener l )
        {
            listenerActions.Enqueue( new ListenerAction( l, ListenerActionType.Add ) );
        }

        internal void Remove( Listener l )
        {
            listenerActions.Enqueue( new ListenerAction( l, ListenerActionType.Remove ) );
        }

        /// <summary>
        /// Lisää ohjeteksti, joka on sama kaikille näppäimille tai muille ohjaimille,
        /// jotka käyttävät samaa aliohjelmaa ohjaintapahtuman käsittelyyn.
        /// </summary>
        /// <param name="controlHandler">Ohjaintapahtuman käsittelevä aliohjelma.</param>
        /// <param name="text">Ohjeteksti.</param>
        public void AddHelpText( Handler controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1>( Handler<T1> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1, T2>( Handler<T1, T2> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1, T2, T3>( Handler<T1, T2, T3> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1, T2, T3, T4>( Handler<T1, T2, T3, T4> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText( AnalogHandler controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1>( AnalogHandler<T1> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1, T2>( AnalogHandler<T1, T2> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1, T2, T3>( AnalogHandler<T1, T2, T3> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        public void AddHelpText<T1, T2, T3, T4>( AnalogHandler<T1, T2, T3, T4> controlHandler, string text )
        {
            helpTextsForHandlers.Add( controlHandler, text );
        }

        /// <summary>
        /// Returns a name for the key, button, or whatever control that is assigned to the
        /// given listener.
        /// </summary>
        internal abstract string GetControlText( Listener listener );


        internal void GetHelpTexts( List<string> texts )
        {
            var controlTextsForHandlerMethods = new Dictionary<Delegate, StringBuilder>();

            foreach ( var listener in listeners )
                GetHelpText( listener, controlTextsForHandlerMethods, texts );
            foreach ( var action in listenerActions )
            {
                if ( action.actionType == ListenerActionType.Add ||
                        action.actionType == ListenerActionType.Enable )
                    GetHelpText( action.listener, controlTextsForHandlerMethods, texts );
            }

            foreach ( var item in controlTextsForHandlerMethods )
            {
                texts.Add( item.Value + " - " + helpTextsForHandlers[item.Key] );
            }
        }

        private void GetHelpText(
            Listener listener,
            Dictionary<Delegate, StringBuilder> controlTextsForHandlerMethods,
            List<string> texts )
        {
            Delegate handlerMethod = listener.Handler;
            string controlText = this.GetControlText( listener );

            if ( helpTextsForHandlers.ContainsKey( handlerMethod ) )
            {
                // Helptext related to a given method:

                if ( !controlTextsForHandlerMethods.ContainsKey( handlerMethod ) )
                {
                    controlTextsForHandlerMethods[handlerMethod] = new StringBuilder( controlText );
                }
                else
                {
                    controlTextsForHandlerMethods[handlerMethod].Append( ", " + controlText );
                }
            }
            else if ( listener.HelpText != null )
            {
                // Helptext related to a single Listen-call:

                texts.Add( controlText + " - " + listener.HelpText );
            }
        }

        /// <summary>
        /// Poistaa tietyt kuuntelutapahtumat käytöstä.
        /// </summary>
        /// <param name="predicate">Ehto, jonka tapahtuman on toteutettava.</param>
        public void Disable( Predicate<Listener> predicate )
        {
            foreach ( Listener l in listeners )
            {
                if ( predicate( l ) )
                {
                    listenerActions.Enqueue( new ListenerAction( l, ListenerActionType.Disable ) );
                }
            }
        }

        /// <summary>
        /// Ottaa käytöstä poistetun kontrollin takaisin käyttöön.
        /// </summary>
        /// <param name="predicate">Ehto, jonka tapahtuman on toteutettava.</param>
        public void Enable( Predicate<Listener> predicate )
        {
            foreach ( Listener l in disabledListeners )
            {
                if ( predicate( l ) )
                {
                    listenerActions.Enqueue( new ListenerAction( l, ListenerActionType.Enable ) );
                }
            }
        }

        /// <summary>
        /// Ottaa takaisin käyttöön kaikki <c>Disable</c>-metodilla poistetut kontrollit.
        /// </summary>
        public void EnableAll()
        {
            Enable( x => true );
        }

        /// <summary>
        /// Poistaa kaikki kontrollit käytöstä.
        /// </summary>
        public void DisableAll()
        {
            Disable( x => true );
        }

        /// <summary>
        /// Poistaa tämän ohjaimen kaikki kuuntelijat.
        /// </summary>
        public virtual void Clear()
        {
            listenerActions.Enqueue( new ListenerAction( null, ListenerActionType.RemoveAll ) );
        }

        protected void ProcessListenerActions()
        {
            bool sortListeners = false;

            while ( listenerActions.Count > 0 )
            {
                ListenerAction action = listenerActions.Dequeue();

                switch ( action.actionType )
                {
                    case ListenerActionType.Add:
                        listeners.Add( action.listener );
                        sortListeners = true;
                        break;
                    case ListenerActionType.Remove:
                        listeners.Remove( action.listener );
                        disabledListeners.Remove( action.listener );
                        action.listener.Disassociate();
                        break;
                    case ListenerActionType.Enable:
                        listeners.Add( action.listener );
                        sortListeners = true;
                        disabledListeners.Remove( action.listener );
                        break;
                    case ListenerActionType.Disable:
                        foreach ( Listener listener in listeners )
                            listener.Disassociate();
                        listeners.Remove( action.listener );
                        disabledListeners.Add( action.listener );
                        break;
                    case ListenerActionType.RemoveAll:
                        listeners.Clear();
                        disabledListeners.Clear();
                        helpTextsForHandlers.Clear();
                        break;
                }
            }

            if ( sortListeners )
                listeners.Sort( Controller.ListenerComparer );
        }

        internal virtual void Update()
        {
            ProcessListenerActions();
        }

        protected static int CompareByContext( Listener l1, Listener l2 )
        {
            int v1 = 0;
            int v2 = 0;

            if ( !l1.HasDefaultContext ) v1 = 1;
            if ( !l2.HasDefaultContext ) v2 = 1;

            return v1 - v2;
        }

        /// <summary>
        /// Tarkistaa, onko ohjainpuskuri tyhjä.
        /// </summary>
        internal abstract bool IsBufferEmpty();

        /// <summary>
        /// Tyhjentää ohjainpuskurin.
        /// Huomaa, että puskuri ei tyhjenny automaattisesti kutsun jälkeen
        /// (kutsu <c>IsBufferEmpty</c> tarkastaaksesi)
        /// </summary>
        internal void PurgeBuffer()
        {
            // oldState = QueryState();
            BufferPurging = true;
        }
    }

    /// <summary>
    /// Yleinen peliohjainluokka tilatiedoilla.
    /// </summary>
    public abstract class Controller<ControllerState> : Controller
    {
        internal ControllerState oldState;
        internal ControllerState newState;

        internal abstract ControllerState GetCurrentState();
        
        protected virtual bool IsTriggered( Listener listener ) { return false; }
        protected virtual bool IsAnalogTriggered( Listener listener, out AnalogState state ) { state = new AnalogState();  return false; }

        protected virtual void UpdateState() {}

        internal override void Update()
        {
            if ( !Enabled )
                return;

            ProcessListenerActions();

            oldState = newState;
            newState = GetCurrentState();
            UpdateState();

            if ( BufferPurging )
            {
                if ( !IsBufferEmpty() )
                {
                    // Wait until the buffer empties itself
                    newState = oldState = GetCurrentState();
                    return;
                }

                // The buffer is empty, clear the flag
                BufferPurging = false;
            }

            for ( int i = listeners.Count - 1; i >= 0; i-- )
            {
                Listener l = listeners[i];

                if ( l == null || l.IsDestroyed || l.Context.IsDestroyed )
                {
                    listeners.RemoveAt( i );
                    continue;
                }
            }

            PointingDevice thisPointing = this as PointingDevice;
            int numChannels = thisPointing != null ? thisPointing.ActiveChannels : 1;

            for ( int ch = 0; ch < numChannels; ch++ )
            {
                List<Listener> mostPrecedent = null;

                for ( int i = 0; i < listeners.Count; i++ )
                {
                    Listener l = listeners[i];
                    AnalogState state;

                    if ( !l.Context.Active )
                        continue;

                    if ( l is IAnalogListener && IsAnalogTriggered( l, out state ) )
                        l.Invoke( state );

                    if ( thisPointing != null )
                    {
                        // Pointing device
                        if ( thisPointing.IsTriggeredOnChannel( ch, l ) )
                        {
                            if ( mostPrecedent == null )
                            {
                                mostPrecedent = new List<Listener>();
                                mostPrecedent.Add( l );
                            }
                            else
                            {
                                int cmp = ListenerComparer.Compare( l, mostPrecedent[0] );

                                if ( cmp == 0 )
                                {
                                    // This listener has equal precedence
                                    mostPrecedent.Add( l );
                                }
                                else if ( cmp < 0 )
                                {
                                    // This listener has greater precedence
                                    mostPrecedent.Clear();
                                    mostPrecedent.Add( l );
                                }
                            }
                        }
                    }
                    else if ( IsTriggered( l ) )
                    {
                        // Non-pointing device
                        l.Invoke();
                    }
                }

                if ( mostPrecedent != null )
                {
                    // Invoke only the most precedent listeners
                    foreach ( Listener l in mostPrecedent )
                        l.Invoke( ch );
                }
            }
        }
    }
}
