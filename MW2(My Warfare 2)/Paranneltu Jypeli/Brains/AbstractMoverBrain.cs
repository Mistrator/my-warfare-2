using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Jypeli
{
    /// <summary>
    /// Yleiset liikkumiseen tarkoitetut aivot.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public abstract class AbstractMoverBrain : Brain
    {
        private double _speed = 100;

        /// <summary>
        /// Nopeus, jolla liikutaan.
        /// </summary>
        /// <value>Nopeus.</value>
        public virtual double Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Käännytäänkö siihen suuntaan mihin liikutaan.
        /// </summary>
        public bool TurnWhileMoving { get; set; }

        public AbstractMoverBrain()
        {
        }

        public AbstractMoverBrain( double speed )
        {
            this.Speed = speed;
        }

        protected void Move( Vector direction )
        {
            if ( Owner == null || direction == Vector.Zero ) return;
            double d = Math.Min( direction.Magnitude, Speed );
            Owner.Move( Vector.FromLengthAndAngle( d, direction.Angle ) );

            if (TurnWhileMoving)
            {
                Owner.Angle = direction.Angle;
            }
        }

        protected void Move( Angle direction )
        {
            if ( Owner == null ) return;
            Owner.Move( Vector.FromLengthAndAngle( Speed, direction ) );

            if (TurnWhileMoving)
            {
                Owner.Angle = direction;
            }
        }
    }
}
