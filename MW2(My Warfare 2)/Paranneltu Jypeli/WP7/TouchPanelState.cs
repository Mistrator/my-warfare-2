using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
#endif

namespace Jypeli.WP7
{
    public class TouchPanelState
    {
        /// <summary>
        /// Aktiiviset kosketukset ruudulla
        /// </summary>
        public List<Touch> ActiveTouches;

        /// <summary>
        /// Aktiiviset eleet ruudulla
        /// </summary>
        public List<Gesture> ActiveGestures;

#if WINDOWS_PHONE
        internal List<GestureSample> samples = new List<GestureSample>();
#endif

        public TouchPanelState()
        {
            ActiveTouches = new List<Touch>( 5 );
            ActiveGestures = new List<Gesture>( 5 );
        }

        internal Touch GetObjectByChannel( int channel )
        {
            // TODO: Specific gesture handlers
            Touch touch;
            int touches = ActiveTouches.Count;

            if ( channel >= touches )
            {
                // Gesture
                return ActiveGestures[channel - touches];
            }

            // Normal touch
            return ActiveTouches[channel];
        }

#if WINDOWS_PHONE
        public TouchPanelState( TouchCollection touchCollection, List<GestureSample> samples, TouchPanelState prevState )
        {
            ActiveTouches = new List<Touch>();
            ActiveGestures = new List<Gesture>( 5 );
            this.samples = samples;

            for ( int i = 0; i < touchCollection.Count; i++ )
            {
                GetTouchObject( touchCollection[i], prevState );
            }

            for ( int i = 0; i < samples.Count; i++ )
            {
                GetGestureObject( samples[i] );
            }
        }

        private Touch GetGestureObject( GestureSample gestureSample )
        {
            Gesture gesture = new Gesture( gestureSample.Position, gestureSample.Delta, gestureSample.Position2, gestureSample.Delta2 );
            ActiveGestures.Add( gesture );
            return gesture;
        }

        private Touch GetTouchObject( TouchLocation location, TouchPanelState prevState )
        {
            Touch touch = null;

            for ( int i = 0; i < prevState.ActiveTouches.Count; i++ )
            {
                if ( location.Id == prevState.ActiveTouches[i].Id )
                {
                    touch = prevState.ActiveTouches[i];
                    touch.Update( location.Position, location.State );

                    ActiveTouches.Add( touch );
                    return touch;
                }
            }

            touch = new Touch( location.Id, location.Position, location.State );
            ActiveTouches.Add( touch );

            return touch;
        }
#endif
    }
}
