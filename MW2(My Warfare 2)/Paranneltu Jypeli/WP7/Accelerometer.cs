using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

#if WINDOWS_PHONE

using Microsoft.Devices.Sensors;
using XNAaccelerometer = Microsoft.Devices.Sensors.Accelerometer;

#endif

using Jypeli.Controls;

/*
 * Authors: Jaakko Kosonen
 */

namespace Jypeli.WP7
{   
    /// <summary>
    /// Suunta/ele joka tunnistetaan.
    /// </summary>
    public enum AccelerometerDirection
    {
        /// <summary>
        /// kallistetaan mihin tahansa suuntaan.
        /// </summary>
        Any,

        /// <summary>
        /// Kallistetaan vasemalle.
        /// </summary>
        Left,

        /// <summary>
        /// Kallistetaan oikealle.
        /// </summary>
        Right,

        /// <summary>
        /// Kallistetaan ylös.
        /// </summary>
        Up,

        /// <summary>
        /// Kallistetaan alas.
        /// </summary>
        Down,

        /// <summary>
        /// Puhelimen ravistusele.
        /// </summary>
        Shake,
        
        /// <summary>
        /// Puhelimen "nopea liike"-ele, esim. näpäytys tai tärähdys.
        /// </summary>
        Tap
    }

    /// <summary>
    /// Herkkyys jolla kallistus/ele halutaan tunnistaa.
    /// </summary>
    public enum AccelerometerSensitivity : int
    {
        /// <summary>
        /// Kallistus/ele tunnistetaan nopeasti.
        /// </summary>
        Realtime = 1,

        /// <summary>
        /// Kallistus/ele tunnistetaan melko nopeasti.
        /// </summary>
        High = 20,

        /// <summary>
        /// Kallistus/ele tunnistetaan melko myöhään.
        /// </summary>
        Medium = 50,

        /// <summary>
        /// Kallistus/ele tunnistetaan myöhään.
        /// </summary>
        Low = 70,        
    }

    /// <summary>
    /// Kalibrointi puhelimen kallistuksen nollakohdalle.
    /// (Asento missä puhelinta ei ole kallistettu yhtään)
    /// </summary>
    public enum AccelerometerCalibration
    {
        /// <summary>
        /// Puhelin on vaakatasossa näyttö ylöspäin.
        /// </summary>
        ZeroAngle,

        /// <summary>
        /// Puhelin on 45-asteen kulmassa.
        /// </summary>
        HalfRightAngle,

        /// <summary>
        /// Puhelin on pystysuorassa.
        /// </summary>
        RightAngle,
    }
    
    /// <summary>
    /// Puhelimen kiihtyvyysanturi.
    /// </summary>
    public class Accelerometer : Controller<Vector3>
    {
        private double deltaX;
        private double deltaY;
        private double deltaZ;

        /// <summary>
        /// Tämänhetkinen, synkronoimaton tila.
        /// </summary>
        private Vector3 memoryState = Vector3.Zero;

        /// <summary>
        /// Puhelimen kallistuksen tämänhetkinen suunta.
        /// </summary>
        public Vector Reading
        {
            get 
            {
                switch (Calibration)
                {
                    case AccelerometerCalibration.ZeroAngle:
                        return new Vector(newState.X, newState.Y);                         
                    case AccelerometerCalibration.HalfRightAngle:
                        return new Vector(newState.X, newState.Y - newState.Z);                         
                    case AccelerometerCalibration.RightAngle:
                        return new Vector(newState.X, -newState.Z); 
                    default:
                        return new Vector(newState.X, newState.Y); 
                }                
            }
        }

        /// <summary>
        /// Kiihtyvyysanturin koko suuntavektori.
        /// </summary>
        public Vector3 State
        {
            get { return newState; }
        }        

        /// <summary>
        /// Puhelimen kallistuksen nollakohta.
        /// </summary>
        public AccelerometerCalibration Calibration { get; set; }

        /// <summary>
        /// Näytön suunta.
        /// </summary>
        public WP7.DisplayOrientation DisplayOrientation { get; set; }

        private double _defaultSensitivity;
        /// <summary>
        /// Herkkyys jos kuunnellaan suuntia ja eleitä ilman erikseen annettua herkkyyttä.
        /// </summary>
        public double DefaultSensitivity { get { return _defaultSensitivity; } }

        private double _defaultAnalogSensitivity;
        /// <summary>
        /// Herkkyys jos kuunnellaan suuntia analogisesti ilman erikseen annettua herkkyyttä.
        /// </summary>
        public double DefaultAnalogSensitivity { get { return _defaultAnalogSensitivity; } }

        /// <summary>
        /// Määrittää onko Shake ja Tap käytössä.
        /// </summary>
        public Boolean GesturesEnabled { get; set; }

        /// <summary>
        /// Aika millisekunteina joka pitää kulua napautusten välissä.
        /// </summary>
        public int TimeBetweenTaps { get; set; }

        /// <summary>
        /// Aika millisekunteina joka pitää kulua ravistusten välissä.
        /// </summary>
        public int TimeBetweenShakes { get; set; }        
        
#if WINDOWS_PHONE

        private bool accMeterStarted;
        private XNAaccelerometer accelerometer;

        /// <summary>
        /// Konstruktori.
        /// </summary>
        public Accelerometer()
        {                        
            accelerometer = new XNAaccelerometer();
            accelerometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(accelerometer_ReadingChanged);
            Calibration = AccelerometerCalibration.ZeroAngle;
            DisplayOrientation = WP7.DisplayOrientation.Landscape;
            _defaultSensitivity = 0.2;
            _defaultAnalogSensitivity = 0.01;
            GesturesEnabled = true;
            TimeBetweenTaps = 300;
            TimeBetweenShakes = 500;
            accMeterStarted = false;
        }  
        
#endif
        /// <summary>
        /// Käynnistää kiihtyvyysanturin.
        /// </summary>
        public void Start()
        {
#if WINDOWS_PHONE
            if (!accMeterStarted)
            {
                accelerometer.Start();
                accMeterStarted = true;
            }
#endif
        }

        /// <summary>
        /// Pysäyttää kiihtyvyysanturin.
        /// </summary>
        public void Stop()
        {
#if WINDOWS_PHONE
            accelerometer.Stop();
            accMeterStarted = false;
#endif
        }

        /// <summary>
        /// Pysäyttää kiihtyvyysanturin annetuksi ajaksi.
        /// </summary>
        /// <param name="seconds">Aika sekunteina</param>
        public void PauseForDuration(double seconds)
        {
            Stop();
            Timer.SingleShot(seconds, Start);            
        }       

        /// <summary>
        /// Asettaa vakioherkkyydeksi annetun arvon.
        /// </summary>
        /// <param name="sensitivity">Herkkyys</param>        
        public void SetDefaultSensitivity(double sensitivity)
        {
            SetDefaultSensitivity(sensitivity, ListeningType.Accelerometer);                        
        }

        /// <summary>
        /// Asettaa vakioherkkyydeksi annetun arvon.
        /// </summary>
        /// <param name="sensitivity">Herkkyys</param>        
        public void SetDefaultSensitivity(AccelerometerSensitivity sensitivity)
        {
            SetDefaultSensitivity((double)sensitivity / 100, ListeningType.Accelerometer);                       
        }

        /// <summary>
        /// Asettaa vakioherkkyydeksi (analoginen) annetun arvon.
        /// </summary>
        /// <param name="sensitivity">Herkkyys</param>        
        public void SetDefaultAnalogSensitivity(double sensitivity)
        {
            SetDefaultSensitivity(sensitivity, ListeningType.ControllerAnalogMovement);
        }

        /// <summary>
        /// Asettaa vakioherkkyydeksi (analoginen) annetun arvon.
        /// </summary>
        /// <param name="sensitivity">Herkkyys</param>        
        public void SetDefaultAnalogSensitivity(AccelerometerSensitivity sensitivity)
        {
            SetDefaultSensitivity((double)sensitivity / 100, ListeningType.ControllerAnalogMovement);
        }

        private void SetDefaultSensitivity(double sens, ListeningType type)
        {            
            switch (type)
            {
                case ListeningType.ControllerAnalogMovement:
                    _defaultAnalogSensitivity = sens;
                    break;
                case ListeningType.Accelerometer:
                    _defaultSensitivity = sens;
                    break;
                default:
                    break;
            }
        }

        internal override bool IsBufferEmpty()
        {
            return true;
        }

        /// <summary>
        /// Ottaa käytöstä poistetun kiihtyvyyskontrollin analogikontrollit <c>k</c> takaisin käyttöön.
        /// </summary>
        public void EnableAnalog()
        {
            base.Enable( listener => ( listener.Type == ListeningType.ControllerAnalogMovement ) );
        }

        /// <summary>
        /// Poistaa kiihtyvyysanturin analogikontrollit <c>k</c> käytöstä.
        /// </summary>
        public void DisableAnalog()
        {
            base.Disable( listener => ( listener.Type == ListeningType.ControllerAnalogMovement ) );
        }

        /// <summary>
        /// Ottaa käytöstä poistetun kiihtyvyyskontrollin <c>k</c> takaisin käyttöön.
        /// </summary>
        /// <param name="direction">Kiihtyvyyssuunta</param>
        public void Enable( AccelerometerDirection direction )
        {
            base.Enable( listener => ( listener.AccelerometerDirection == direction ) );
        }

        /// <summary>
        /// Poistaa kiihtyvyyskontrollin <c>k</c> käytöstä.
        /// </summary>
        /// <param name="direction">Kiihtyvyyssuunta</param>
        public void Disable( AccelerometerDirection direction )
        {
            base.Disable( listener => ( listener.AccelerometerDirection == direction ) );
        }

        internal override string GetControlText(Listener listener)
        {
            if (listener.Type != ListeningType.Accelerometer && listener.Type != ListeningType.ControllerAnalogMovement)
                Debug.Assert(false, "Bad listener type for accelerometer");
            return "Accelerometer";
        }

#if WINDOWS_PHONE

        private void accelerometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            switch (DisplayOrientation)
            {
                case WP7.DisplayOrientation.Landscape:                    
                    memoryState.X = -(float)e.Y;
                    memoryState.Y = (float)e.X;
                    memoryState.Z = (float)e.Z;
                    break;
                case WP7.DisplayOrientation.LandscapeLeft:
                    memoryState.X = -(float)e.Y;
                    memoryState.Y = (float)e.X;
                    memoryState.Z = (float)e.Z;
                    break;
                case WP7.DisplayOrientation.LandscapeRight:
                    memoryState.X = -(float)e.Y;
                    memoryState.Y = -(float)e.X;
                    memoryState.Z = (float)e.Z;
                    break;
                case WP7.DisplayOrientation.Portrait:
                    memoryState.X = (float)e.X;
                    memoryState.Y = (float)e.Y;
                    memoryState.Z = (float)e.Z;
                    break;
                default:
                    memoryState.X = (float)e.X;
                    memoryState.Y = (float)e.Y;
                    memoryState.Z = (float)e.Z;
                    break;
            }            
        }

        internal override void Update()
        {
            if ( !accMeterStarted )
                return;

            base.Update();
        }

#endif

        internal override Vector3 GetCurrentState()
        {
            return memoryState;
        }

        protected override void UpdateState()
        {
            deltaY = Math.Abs( newState.Y - oldState.Y );
            deltaX = Math.Abs( newState.X - oldState.X );
            deltaZ = Math.Abs( newState.Z - oldState.Z );
        }

#if WINDOWS_PHONE

        protected override bool IsTriggered( Listener listener )
        {
            if ( listener.Type == ListeningType.Accelerometer )
                return IsAccTriggered( listener, Reading );

            return base.IsTriggered( listener );
        }

        protected override bool IsAnalogTriggered( Listener listener, out AnalogState state )
        {
            if ( listener.Type == ListeningType.ControllerAnalogMovement )
                return IsAccTriggeredAnalog( listener, out state, Reading );

            return base.IsAnalogTriggered( listener, out state );
        }

        private bool IsAccTriggered(Listener l, Vector reading)
        {
            double sensitivity = l.AccelerometerSensitivity;
            switch (l.AccelerometerDirection)
            {
                case AccelerometerDirection.Any:                    
                    return Math.Abs(reading.X) > sensitivity || Math.Abs(reading.Y) > sensitivity;
                case AccelerometerDirection.Left:
                    return reading.X < -sensitivity;
                case AccelerometerDirection.Right:
                    return reading.X > sensitivity;
                case AccelerometerDirection.Up:
                    return reading.Y > sensitivity;
                case AccelerometerDirection.Down:
                    return reading.Y < -sensitivity;
                case AccelerometerDirection.Shake:
                    return GestureTrickered(l, 3);
                case AccelerometerDirection.Tap:
                    return GestureTrickered(l, 1);
                default:
                    return false;
            }                        
        }

        private bool IsAccTriggeredAnalog(Listener l, out AnalogState state, Vector reading)
        {
            if (reading.Magnitude > l.AccelerometerSensitivity)
            {
                state = new AnalogState(0.0, 0.0, new Vector(reading.X, reading.Y));
                return true;
            }
            state = new AnalogState();
            return false;
        }

        #region Gestures

        private bool GestureTrickered(Listener listener, int minGestureCount)
        {
            if (!GesturesEnabled) return false;
            if (IsSamplingTimeOver(listener) && listener.AccelerometerGestureCount >= minGestureCount)
            {
                SetLastGesture(listener);
                return true;
            }
            else if (TimeToGetSample(listener))
            {
                bool gestureDetected;
                switch (listener.AccelerometerDirection)
                {                    
                    case AccelerometerDirection.Shake:
                        gestureDetected = Shaked(listener.AccelerometerSensitivity);
                        break;
                    case AccelerometerDirection.Tap:
                        gestureDetected = Tapped(listener.AccelerometerSensitivity);
                        break;
                    default:
                        gestureDetected = false;
                        break;
                }
                HandleSampling(gestureDetected, listener);
            }
            return false;
        }
        
        private bool Shaked(double trigger)
        {
            double threshold = (trigger + 1) * 2;
            return deltaX + deltaY > threshold ||
                   deltaX + deltaZ > threshold ||
                   deltaY + deltaZ > threshold;
        }

        private bool Tapped(double trigger)
        {
            double threshold = trigger + 0.2;
            return !Shaked(0) &&
                 ( deltaX > threshold ||
                   deltaY > threshold ||
                   deltaZ > threshold );            
        }        

        private void HandleSampling(bool gestureDetected, Listener listener)
        {            
            if (listener.AccelerometerGestureCount == 0 && gestureDetected)
            {
                listener.AccelerometerGestureSamplingStartTime = Game.Time.SinceStartOfGame.TotalMilliseconds;                
            }
            listener.AccelerometerGestureCount += gestureDetected ? 1 : 0;
        }

        private bool TimeToGetSample(Listener listener)
        {            
            switch (listener.AccelerometerDirection)
            {                               
                case AccelerometerDirection.Shake:
                    return Game.Time.SinceStartOfGame.TotalMilliseconds > listener.AccelerometerLastGesture + TimeBetweenShakes;
                case AccelerometerDirection.Tap:
                    return Game.Time.SinceStartOfGame.TotalMilliseconds > listener.AccelerometerLastGesture + TimeBetweenTaps;
                default:
                    return false;
            }            
        }

        private bool IsSamplingTimeOver(Listener listener)
        {
            switch (listener.AccelerometerDirection)
            {                
                case AccelerometerDirection.Shake:
                    return Game.Time.SinceStartOfGame.TotalMilliseconds > listener.AccelerometerGestureSamplingStartTime + TimeBetweenShakes / 5;            
                case AccelerometerDirection.Tap:
                    return Game.Time.SinceStartOfGame.TotalMilliseconds > listener.AccelerometerGestureSamplingStartTime + TimeBetweenTaps / 10;            
                default:
                    return false;
            }            
        }

        private void SetLastGesture(Listener listener)
        {
            listener.AccelerometerGestureCount = 0;
            listener.AccelerometerLastGesture = Game.Time.SinceStartOfGame.TotalMilliseconds;                        
        }

        #endregion

#endif

        #region Listen with no parameters

        public void Listen(AccelerometerDirection direction, Handler handler, string helpText)
        {
            this.Listen(direction, DefaultSensitivity, handler, helpText);
        }

        public void ListenAnalog(AnalogHandler handler, string helpText)
        {
            this.ListenAnalog(DefaultAnalogSensitivity, handler, helpText);
        }

        public void Listen(AccelerometerDirection direction, AccelerometerSensitivity sensitivity, Handler handler, string helpText)                     
        {
            this.Listen(direction, (int)sensitivity / 100, handler, helpText);
        }

        public void ListenAnalog(AccelerometerSensitivity sensitivity, AnalogHandler handler, string helpText)
        {
            this.ListenAnalog((int)sensitivity / 100, handler, helpText);
        }

        public void Listen(AccelerometerDirection direction, double trigger, Handler handler, string helpText)
        {
#if WINDOWS_PHONE
            Listener l = new SimpleListener(this, ListeningType.Accelerometer, helpText, handler);
            l.AccelerometerDirection = direction;
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }
        
        public void ListenAnalog(double trigger, AnalogHandler handler, string helpText)
        {
#if WINDOWS_PHONE
            Listener l = new AnalogListener( this, ListeningType.ControllerAnalogMovement, helpText, handler );
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        #endregion 

        #region Listen with 1 parameter

        public void Listen<T1>(AccelerometerDirection direction, Handler<T1> handler, string helpText, T1 p1)
        {
            this.Listen(direction, DefaultSensitivity, handler, helpText, p1);
        }

        public void ListenAnalog<T1>(AnalogHandler<T1> handler, string helpText, T1 p1)
        {
            this.ListenAnalog(DefaultAnalogSensitivity, handler, helpText, p1);
        }

        public void Listen<T1>(AccelerometerDirection direction, AccelerometerSensitivity sensitivity, Handler<T1> handler, string helpText, T1 p1)
        {
            this.Listen(direction, (double)sensitivity / 100, handler, helpText, p1);
        }

        public void ListenAnalog<T1>(AccelerometerSensitivity sensitivity, AnalogHandler<T1> handler, string helpText, T1 p1)
        {
            this.ListenAnalog((double)sensitivity / 100, handler, helpText, p1);
        }

        public void Listen<T1>(AccelerometerDirection direction, double trigger, Handler<T1> handler, string helpText, T1 p1)
        {
#if WINDOWS_PHONE
            Listener l = new SimpleListener<T1>( this, ListeningType.Accelerometer, helpText, handler, p1 );
            l.AccelerometerDirection = direction;
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        public void ListenAnalog<T1>(double trigger, AnalogHandler<T1> handler, string helpText, T1 p1)
        {
#if WINDOWS_PHONE
            Listener l = new AnalogListener<T1>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1 );
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        #endregion

        #region Listen with 2 parameters

        public void Listen<T1, T2>(AccelerometerDirection direction, Handler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            this.Listen(direction, DefaultSensitivity, handler, helpText, p1, p2);
        }

        public void ListenAnalog<T1, T2>(AnalogHandler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            this.ListenAnalog(DefaultAnalogSensitivity, handler, helpText, p1, p2);
        }

        public void Listen<T1, T2>(AccelerometerDirection direction, AccelerometerSensitivity sensitivity, Handler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            this.Listen(direction, (double)sensitivity / 100, handler, helpText, p1, p2);
        }

        public void ListenAnalog<T1, T2>(AccelerometerSensitivity sensitivity, AnalogHandler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            this.ListenAnalog((double)sensitivity / 100, handler, helpText, p1, p2);
        }

        public void Listen<T1, T2>(AccelerometerDirection direction, double trigger, Handler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
#if WINDOWS_PHONE
            Listener l = new SimpleListener<T1, T2>( this, ListeningType.Accelerometer, helpText, handler, p1, p2 );
            l.AccelerometerDirection = direction;
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        public void ListenAnalog<T1, T2>(double trigger, AnalogHandler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
#if WINDOWS_PHONE
            Listener l = new AnalogListener<T1, T2>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1, p2 );
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        #endregion

        #region Listen with 3 parameters

        public void Listen<T1, T2, T3>(AccelerometerDirection direction, Handler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            this.Listen(direction, DefaultSensitivity, handler, helpText, p1, p2, p3);
        }

        public void ListenAnalog<T1, T2, T3>(AnalogHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            this.ListenAnalog(DefaultAnalogSensitivity, handler, helpText, p1, p2, p3);
        }

        public void Listen<T1, T2, T3>(AccelerometerDirection direction, AccelerometerSensitivity sensitivity, Handler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            this.Listen(direction, (double)sensitivity / 100, handler, helpText, p1, p2, p3);
        }

        public void ListenAnalog<T1, T2, T3>(AccelerometerSensitivity sensitivity, AnalogHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            this.ListenAnalog((double)sensitivity / 100, handler, helpText, p1, p2, p3);
        }

        public void Listen<T1, T2, T3>(AccelerometerDirection direction, double trigger, Handler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
#if WINDOWS_PHONE
            Listener l = new SimpleListener<T1, T2, T3>( this, ListeningType.Accelerometer, helpText, handler, p1, p2, p3 );
            l.AccelerometerDirection = direction;
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        public void ListenAnalog<T1, T2, T3>(double trigger, AnalogHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
#if WINDOWS_PHONE
            Listener l = new AnalogListener<T1, T2, T3>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1, p2, p3 );
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        #endregion

        #region Listen with 4 parameters

        public void Listen<T1, T2, T3, T4>(AccelerometerDirection direction, Handler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            this.Listen(direction, DefaultSensitivity, handler, helpText, p1, p2, p3, p4);
        }

        public void ListenAnalog<T1, T2, T3, T4>(AnalogHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            this.ListenAnalog(DefaultAnalogSensitivity, handler, helpText, p1, p2, p3, p4);
        }

        public void Listen<T1, T2, T3, T4>(AccelerometerDirection direction, AccelerometerSensitivity sensitivity, Handler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            this.Listen(direction, (double)sensitivity / 100, handler, helpText, p1, p2, p3, p4);
        }

        public void ListenAnalog<T1, T2, T3, T4>(AccelerometerSensitivity sensitivity, AnalogHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            this.ListenAnalog((double)sensitivity / 100, handler, helpText, p1, p2, p3, p4);
        }

        public void Listen<T1, T2, T3, T4>(AccelerometerDirection direction, double trigger, Handler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
#if WINDOWS_PHONE
            Listener l = new SimpleListener<T1, T2, T3, T4>( this, ListeningType.Accelerometer, helpText, handler, p1, p2, p3, p4 );
            l.AccelerometerDirection = direction;
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        public void ListenAnalog<T1, T2, T3, T4>(double trigger, AnalogHandler<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
#if WINDOWS_PHONE
            Listener l = new AnalogListener<T1, T2, T3, T4>( this, ListeningType.ControllerAnalogMovement, helpText, handler, p1, p2, p3, p4 );
            l.AccelerometerSensitivity = trigger;
            Add(l);
            Start();
#endif
        }

        #endregion

    }
}
