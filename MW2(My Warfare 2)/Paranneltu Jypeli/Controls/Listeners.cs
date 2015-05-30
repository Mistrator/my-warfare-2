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
using Jypeli.WP7;

namespace Jypeli.Controls
{
    public enum ListeningType
    {
        Null,
        KeyboardKey,
        KeyboardAll,
        MouseButton,
        MouseMovement,
        MouseWheel,
        ControllerButton,
        ControllerAnalogMovement,
        Touch,
        TouchGesture,
        Accelerometer,
        PhoneButton,
    }

    /// <summary>
    /// Analoginen ohjain. Tämä voi olla joko painike, jota voi painaa
    /// eri voimakkuuksilla (padiohjaimen liipainäppäin), ohjaustikku
    /// tai puhelimen kiihtyvyysanturi
    /// </summary>
    public enum AnalogControl
    {
        /// <summary>
        /// Ohjaimen tavallisemmin käytettävä analogitikku. Padissa, jossa on kaksi tikkua, käytetään vasenta.
        /// </summary>
        DefaultStick,

        /// <summary>
        /// Ohjaimen vasen analogitikku.
        /// </summary>
        LeftStick,

        /// <summary>
        /// Ohjaimen oikea analogitikku.
        /// </summary>
        RightStick,

        /// <summary>
        /// Ohjaimen vasen liipasin.
        /// </summary>
        LeftTrigger,

        /// <summary>
        /// Ohjaimen oikea liipasin.
        /// </summary>
        RightTrigger,

        /// <summary>
        /// Puhelimen kiihtyvyysanturi
        /// </summary>
        Accelerometer,
    }

    [Save]
    public abstract class Listener : Destroyable
    {
        private static NullListener _nullListener = new NullListener();
        internal static NullListener Null
        {
            get { return _nullListener; }
        }

        [Save] internal ListeningType Type;
        [Save] internal String HelpText;
        [Save] internal ButtonState State;
        internal Controller Controller;

        private bool dynamicContext = false;
        private ListenContext context;
        private ControlContexted contextedObject;

        internal ListenContext Context
        {
            get { return ( dynamicContext ? contextedObject.ControlContext : context ); }
        }

        // Keyboard parametrers
        [Save] internal Key Key;

        // Game controller parameters
        internal AnalogControl AnalogControl;
        internal Button Button;
        internal double AnalogTrigger;

        // Accelerometer parameters
        internal AccelerometerDirection AccelerometerDirection;
        internal double AccelerometerSensitivity;
        internal int AccelerometerGestureCount;
        internal double AccelerometerLastGesture;
        internal double AccelerometerGestureSamplingStartTime;

        // Mouse parameters
        internal MouseButton MouseButton;
        internal double MovementTrigger;

        // Touchpanel parameters
        internal GestureType? gestureType;

        private bool disassociated = false;
        private IGameObjectInternal _gameObject;

        internal IGameObject GameObject
        {
            get { return _gameObject; }
            set
            {
                if ( !disassociated && _gameObject != null )
                    _gameObject.AssociatedListeners.Remove( this );

                _gameObject = (IGameObjectInternal)value;
                _gameObject.AssociatedListeners.Add( this );
                disassociated = false;
            }
        }

        internal bool HasDefaultContext { get { return ( Context == Game.Instance.ControlContext ); } }

        internal Listener( Controller controller, ListeningType type, String helpText )
        {
            this.Controller = controller;
            this.context = Game.Instance.ControlContext;
            this.Type = type;
            this.HelpText = helpText;
        }

        #region Destroyable

        /// <summary>
        /// Onko olio tuhottu.
        /// </summary>
        /// <returns></returns>
        public bool IsDestroyed { get; private set; }

        public void Destroy()
        {
            if ( Controller != null ) Controller.Remove( this );
            IsDestroyed = true;
            OnDestroyed();
        }

        /// <summary> 
        /// Tapahtuu, kun olio tuhotaan. 
        /// </summary> 
        public event Action Destroyed;

        private void OnDestroyed()
        {
            if ( Destroyed != null )
                Destroyed();
        }

        #endregion

        internal void Disassociate()
        {
            if ( _gameObject != null )
                _gameObject.AssociatedListeners.Remove( this );
            disassociated = true;
        }

        internal void Associate()
        {
            if ( !disassociated ) return;

            if ( _gameObject != null )
                _gameObject.AssociatedListeners.Add( this );

            disassociated = false;
        }

        internal abstract Delegate Handler { get; }

        internal static void Invoke( Listener l )
        {
            if ( !l.IsDestroyed && l.Context != null && l.Context.Active )
                l.Invoke();
        }

        internal virtual void Invoke()
        {
            throw new NotImplementedException();
        }

        internal virtual void Invoke( List<Key> keys )
        {
            throw new NotImplementedException();
        }

        internal virtual void Invoke( AnalogState state )
        {
            throw new NotImplementedException();
        }

        internal virtual void Invoke( int channel )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// </summary>
        /// <param name="context"></param>
        public Listener InContext( ListenContext context )
        {
            this.dynamicContext = false;
            this.context = context;
            return this;
        }

        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// Esim. Keyboard.Listen(parametrit).InContext(omaIkkuna) kuuntelee
        /// haluttua näppäimistötapahtumaa ainoastaan kun ikkuna on näkyvissä ja päällimmäisenä.
        /// </summary>
        /// <param name="context"></param>
        public Listener InContext( ControlContexted obj )
        {
            this.dynamicContext = true;
            this.contextedObject = obj;
            return this;
        }

        /// <summary>
        /// Kuuntelee tapahtumaa tietyn olion päällä.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Listener OnObject(GameObject obj)
        {
            this.GameObject = obj;
            return this;
        }
    }

    public class NullListener : Listener
    {
        internal override Delegate Handler { get { return null; } }

        internal NullListener()
            : base( null, ListeningType.Null, null )
        {
        }

        internal override void Invoke()
        {
        }

        internal override void Invoke( AnalogState state )
        {
        }

        internal override void Invoke( List<Key> keys )
        {
        }

        internal override void Invoke( int channel )
        {
        }
    }

    public class SimpleListener : Listener
    {
        internal Handler handler;

        internal override Delegate Handler { get { return handler; } }

        internal SimpleListener( Controller controller, ListeningType type, string helpText, Handler handler )
            : base( controller, type, helpText )
        {
            this.handler = handler;
        }

        internal override void Invoke()
        {
            handler();
        }

        internal override void Invoke( int channel )
        {
            handler();
        }
    }

    internal class SimpleListener<T1> : Listener
    {
        internal Handler<T1> handler;
        internal T1 p1;

        internal override Delegate Handler { get { return handler; } }

        internal SimpleListener( Controller controller, ListeningType type, string helpText, Handler<T1> handler, T1 p1 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
        }

        internal override void Invoke()
        {
            handler( p1 );
        }

        internal override void Invoke( int channel )
        {
            handler( p1 );
        }
    }

    public class SimpleListener<T1, T2> : Listener
    {
        internal Handler<T1, T2> handler;
        internal T1 p1;
        internal T2 p2;

        internal override Delegate Handler { get { return handler; } }

        internal SimpleListener( Controller controller, ListeningType type, string helpText, Handler<T1, T2> handler, T1 p1, T2 p2 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
        }

        internal override void Invoke()
        {
            handler( p1, p2 );
        }

        internal override void Invoke( int channel )
        {
            handler( p1, p2 );
        }
    }

    public class SimpleListener<T1, T2, T3> : Listener
    {
        internal Handler<T1, T2, T3> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;

        internal override Delegate Handler { get { return handler; } }

        internal SimpleListener( Controller controller, ListeningType type, string helpText, Handler<T1, T2, T3> handler, T1 p1, T2 p2, T3 p3 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        internal override void Invoke()
        {
            handler( p1, p2, p3 );
        }

        internal override void Invoke( int channel )
        {
            handler( p1, p2, p3 );
        }
    }

    public class SimpleListener<T1, T2, T3, T4> : Listener
    {
        internal Handler<T1, T2, T3, T4> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;
        internal T4 p4;

        internal override Delegate Handler { get { return handler; } }

        internal SimpleListener( Controller controller, ListeningType type, string helpText, Handler<T1, T2, T3, T4> handler, T1 p1, T2 p2, T3 p3, T4 p4 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
        }

        internal override void Invoke()
        {
            handler( p1, p2, p3, p4 );
        }

        internal override void Invoke( int channel )
        {
            handler( p1, p2, p3, p4 );
        }
    }

    public class MultiKeyListener : Listener
    {
        private Keyboard keyboard;
        internal MultiKeyHandler handler;

        internal override Delegate Handler { get { return handler; } }

        internal MultiKeyListener( Controller controller, string helpText, MultiKeyHandler handler )
            : base( controller, ListeningType.KeyboardAll, helpText )
        {
            this.handler = handler;
            
            keyboard = controller as Keyboard;
            if ( keyboard == null ) throw new InvalidOperationException( "MultiKeyListener not bound to a keyboard" );
        }

        internal override void Invoke()
        {
            handler( keyboard.KeysTriggered( this.State ) );
        }
    }

    public class MultiKeyListener<T1> : Listener
    {
        internal MultiKeyHandler<T1> handler;
        internal T1 p1;

        internal override Delegate Handler { get { return handler; } }

        internal MultiKeyListener( Controller controller, string helpText, MultiKeyHandler<T1> handler, T1 p1 )
            : base( controller, ListeningType.KeyboardAll, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
        }

        internal override void Invoke( List<Key> keys )
        {
            handler( keys, p1 );
        }
    }

    public class MultiKeyListener<T1, T2> : Listener
    {
        internal MultiKeyHandler<T1, T2> handler;
        internal T1 p1;
        internal T2 p2;

        internal override Delegate Handler { get { return handler; } }

        internal MultiKeyListener( Controller controller, string helpText, MultiKeyHandler<T1, T2> handler, T1 p1, T2 p2 )
            : base( controller, ListeningType.KeyboardAll, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
        }

        internal override void Invoke( List<Key> keys )
        {
            handler( keys, p1, p2 );
        }
    }

    public class MultiKeyListener<T1, T2, T3> : Listener
    {
        internal MultiKeyHandler<T1, T2, T3> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;

        internal override Delegate Handler { get { return handler; } }

        internal MultiKeyListener( Controller controller, string helpText, MultiKeyHandler<T1, T2, T3> handler, T1 p1, T2 p2, T3 p3 )
            : base( controller, ListeningType.KeyboardAll, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        internal override void Invoke( List<Key> keys )
        {
            handler( keys, p1, p2, p3 );
        }
    }

    public class MultiKeyListener<T1, T2, T3, T4> : Listener
    {
        internal MultiKeyHandler<T1, T2, T3, T4> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;
        internal T4 p4;

        internal override Delegate Handler { get { return handler; } }

        internal MultiKeyListener( Controller controller, string helpText, MultiKeyHandler<T1, T2, T3, T4> handler, T1 p1, T2 p2, T3 p3, T4 p4 )
            : base( controller, ListeningType.KeyboardAll, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
        }

        internal override void Invoke( List<Key> keys )
        {
            handler( keys, p1, p2, p3, p4 );
        }
    }

    public interface IAnalogListener
    {
    }

    public class AnalogListener : Listener, IAnalogListener
    {
        internal AnalogHandler handler;

        internal override Delegate Handler { get { return handler; } }

        internal AnalogListener( Controller controller, ListeningType type, string helpText, AnalogHandler handler )
            : base( controller, type, helpText )
        {
            this.handler = handler;
        }

        internal override void Invoke( AnalogState state )
        {
            handler( state );
        }
    }

    public class AnalogListener<T1> : Listener, IAnalogListener
    {
        internal AnalogHandler<T1> handler;
        internal T1 p1;

        internal override Delegate Handler { get { return handler; } }

        internal AnalogListener( Controller controller, ListeningType type, string helpText, AnalogHandler<T1> handler, T1 p1 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
        }

        internal override void Invoke( AnalogState state )
        {
            handler( state, p1 );
        }
    }

    public class AnalogListener<T1, T2> : Listener, IAnalogListener
    {
        internal AnalogHandler<T1, T2> handler;
        internal T1 p1;
        internal T2 p2;

        internal override Delegate Handler { get { return handler; } }

        internal AnalogListener( Controller controller, ListeningType type, string helpText, AnalogHandler<T1, T2> handler, T1 p1, T2 p2 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
        }

        internal override void Invoke( AnalogState state )
        {
            handler( state, p1, p2 );
        }
    }

    public class AnalogListener<T1, T2, T3> : Listener, IAnalogListener
    {
        internal AnalogHandler<T1, T2, T3> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;

        internal override Delegate Handler { get { return handler; } }

        internal AnalogListener( Controller controller, ListeningType type, string helpText, AnalogHandler<T1, T2, T3> handler, T1 p1, T2 p2, T3 p3 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        internal override void Invoke( AnalogState state )
        {
            handler( state, p1, p2, p3 );
        }
    }

    public class AnalogListener<T1, T2, T3, T4> : Listener, IAnalogListener
    {
        internal AnalogHandler<T1, T2, T3, T4> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;
        internal T4 p4;

        internal override Delegate Handler { get { return handler; } }

        internal AnalogListener( Controller controller, ListeningType type, string helpText, AnalogHandler<T1, T2, T3, T4> handler, T1 p1, T2 p2, T3 p3, T4 p4 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
        }

        internal override void Invoke( AnalogState state )
        {
            handler( state, p1, p2, p3, p4 );
        }
    }

    public class TouchListener : Listener
    {
        internal TouchHandler handler;

        internal override Delegate Handler { get { return handler; } }

        internal TouchListener( Controller controller, ListeningType type, string helpText, TouchHandler handler )
            : base( controller, type, helpText )
        {
            this.handler = handler;
        }

        internal override void Invoke( int channel )
        {
            TouchPanel touchController = Controller as TouchPanel;

            if ( touchController != null )
                handler( touchController.newState.GetObjectByChannel( channel ) );
            else
                throw new InvalidOperationException( "TouchListener.Invoke(ch): Controller is not a TouchPanel" );
        }
    }

    public class TouchListener<T1> : Listener
    {
        internal TouchHandler<T1> handler;
        internal T1 p1;

        internal override Delegate Handler { get { return handler; } }

        internal TouchListener( Controller controller, ListeningType type, string helpText, TouchHandler<T1> handler, T1 p1 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
        }

        internal override void Invoke( int channel )
        {
            TouchPanel touchController = Controller as TouchPanel;

            if ( touchController != null )
                handler( touchController.newState.GetObjectByChannel( channel ), p1 );
            else
                throw new InvalidOperationException( "TouchListener.Invoke(ch): Controller is not a TouchPanel" );
        }
    }

    public class TouchListener<T1, T2> : Listener
    {
        internal TouchHandler<T1, T2> handler;
        internal T1 p1;
        internal T2 p2;

        internal override Delegate Handler { get { return handler; } }

        internal TouchListener( Controller controller, ListeningType type, string helpText, TouchHandler<T1, T2> handler, T1 p1, T2 p2 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
        }

        internal override void Invoke( int channel )
        {
            TouchPanel touchController = Controller as TouchPanel;

            if ( touchController != null )
                handler( touchController.newState.GetObjectByChannel( channel ), p1, p2 );
            else
                throw new InvalidOperationException( "TouchListener.Invoke(ch): Controller is not a TouchPanel" );
        }
    }

    public class TouchListener<T1, T2, T3> : Listener
    {
        internal TouchHandler<T1, T2, T3> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;

        internal override Delegate Handler { get { return handler; } }

        internal TouchListener( Controller controller, ListeningType type, string helpText, TouchHandler<T1, T2, T3> handler, T1 p1, T2 p2, T3 p3 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        internal override void Invoke( int channel )
        {
            TouchPanel touchController = Controller as TouchPanel;

            if ( touchController != null )
                handler( touchController.newState.GetObjectByChannel( channel ), p1, p2, p3 );
            else
                throw new InvalidOperationException( "TouchListener.Invoke(ch): Controller is not a TouchPanel" );
        }
    }

    public class TouchListener<T1, T2, T3, T4> : Listener
    {
        internal TouchHandler<T1, T2, T3, T4> handler;
        internal T1 p1;
        internal T2 p2;
        internal T3 p3;
        internal T4 p4;

        internal override Delegate Handler { get { return handler; } }

        internal TouchListener( Controller controller, ListeningType type, string helpText, TouchHandler<T1, T2, T3, T4> handler, T1 p1, T2 p2, T3 p3, T4 p4 )
            : base( controller, type, helpText )
        {
            this.handler = handler;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
        }

        internal override void Invoke( int channel )
        {
            TouchPanel touchController = Controller as TouchPanel;

            if ( touchController != null )
                handler( touchController.newState.GetObjectByChannel( channel ), p1, p2, p3, p4 );
            else
                throw new InvalidOperationException( "TouchListener.Invoke(ch): Controller is not a TouchPanel" );
        }
    }
}
