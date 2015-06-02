using System;
using System.Collections.Generic;

namespace Jypeli.Assets
{
    /// <summary>
    /// Aivot, jotka seuraavat annettua polkua.
    /// </summary>
    public class PathFollowerBrain : AbstractMoverBrain
    {
        private IList<Vector> path;
        private int wayPointIndex = 0;
        private int step = 1;
        private double _waypointRadius = 10;

        /// <summary>
        /// Polku, eli lista pisteistä joita aivot seuraa.
        /// </summary>
        public IList<Vector> Path
        {
            get { return path; }
            set
            {
                path = value;
                wayPointIndex = 0;
            }
        }

        /// <summary>
        /// Etäisyys, jonka sisällä ollaan perillä pisteessä.
        /// </summary>
        public double WaypointRadius
        {
            get { return _waypointRadius; }
            set { _waypointRadius = value; }
        }

        /// <summary>
        /// Etäisyys seuraavaan pisteeseen.
        /// </summary>
        public DoubleMeter DistanceToWaypoint { get; private set; }

        /// <summary>
        /// Jos true, palataan alkupisteeseen ja kierretään reittiä loputtomiin.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Palataanko samaa reittiä takaisin.
        /// </summary>
        public bool ReverseReturn { get; set; }

        /// <summary>
        /// Tapahtuu, kun saavutaan reitin päähän.
        /// </summary>
        public event Action ArrivedAtEnd;

        /// <summary>
        /// Luo uudet polunseuraaja-aivot.
        /// </summary>
        public PathFollowerBrain()
            : base()
        {
            Path = new Vector[] { };
            DistanceToWaypoint = new DoubleMeter(double.PositiveInfinity, 0, double.PositiveInfinity);
        }

        /// <summary>
        /// Luo uudet polunseuraaja-aivot ja asettaa niille nopeuden.
        /// </summary>
        public PathFollowerBrain( double speed )
            : this()
        {
            Speed = speed;
        }

        /// <summary>
        /// Luo aivot, jotka seuraavat polkua <c>path</c>.
        /// </summary>
        public PathFollowerBrain( params Vector[] path )
        {
            this.Path = path;
            DistanceToWaypoint = new DoubleMeter( double.PositiveInfinity, 0, double.PositiveInfinity );
        }

        protected override void Update(Time time)
        {
            if ( Owner == null || Path == null || Path.Count == 0 ) return;

            Vector target = path[wayPointIndex];
            Vector dist = target - Owner.AbsolutePosition;
            DistanceToWaypoint.Value = dist.Magnitude;

            if (DistanceToWaypoint.Value < WaypointRadius)
            {
                // Arrived at waypoint
                Arrived();
            }
            else
            {
                // Continue moving
                Move(dist.Angle);
            }

            base.Update(time);
        }

        private void OnArrivedAtEnd()
        {
            if ( ArrivedAtEnd != null )
                ArrivedAtEnd();
        }

        void Arrived()
        {
            if ( Path.Count == 0 || wayPointIndex >= path.Count ) return;

            int nextIndex = wayPointIndex + step;

            if ( nextIndex < 0 )
            {
                OnArrivedAtEnd();

                if ( Loop )
                {
                    step = Math.Abs( step );
                    wayPointIndex = 0;
                }
                else return;
            }
            else if ( nextIndex >= path.Count )
            {
                OnArrivedAtEnd();
                
                if ( ReverseReturn )
                {
                    step = -Math.Abs( step );
                    wayPointIndex += step;
                }
                else if ( Loop ) wayPointIndex = 0;
                else return;
            }
            else
                wayPointIndex = nextIndex;
        }
    }
}
