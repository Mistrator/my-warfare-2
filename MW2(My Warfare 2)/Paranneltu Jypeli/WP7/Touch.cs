using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace Jypeli.WP7
{
    /// <summary>
    /// Kosketuspaneelin kosketus.
    /// </summary>
    public class Touch
    {
        protected Vector2 _previousPosition;
        protected Vector2 _position;
        protected Vector2 _movement;

        /// <summary>
        /// Id-tunnus tälle kosketukselle.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Kosketuksen paikka ruudulla.
        /// </summary>
        public Vector PositionOnScreen
        {
            get
            {
                return Game.Screen.FromXnaScreenCoordinates( _position );
            }
        }

        /// <summary>
        /// Kosketuksen paikka pelimaailmassa.
        /// </summary>
        public Vector PositionOnWorld
        {
            get
            {
                return Game.Instance.Camera.ScreenToWorld( PositionOnScreen );
            }
        }

        /// <summary>
        /// Kosketuksen liike ruudulla.
        /// </summary>
        public Vector MovementOnScreen
        {
            get
            {
                return new Vector( (double)_movement.X, -(double)_movement.Y );
            }
        }

        /// <summary>
        /// Kosketuksen liike pelimaailmassa.
        /// </summary>
        public Vector MovementOnWorld
        {
            get
            {
                return MovementOnScreen / Game.Instance.Camera.ZoomFactor;
            }
        }

#if WINDOWS_PHONE
        public TouchLocationState State { get; internal set; }

        internal Touch( Vector2 position, Vector2 movement )
        {
            this._position = position;
            this._movement = movement;
        }

        internal Touch( int id, Vector2 initialPosition, TouchLocationState initialState )
        {
            this.Id = id;
            this._position = this._previousPosition = initialPosition;
            this._movement = Vector2.Zero;
            this.State = initialState;
        }

        internal void Update( Vector2 newPosition, TouchLocationState newState )
        {
            _previousPosition = _position;
            _position = newPosition;
            _movement = newPosition - _previousPosition;
            State = newState;
        }
#endif

    }
}
