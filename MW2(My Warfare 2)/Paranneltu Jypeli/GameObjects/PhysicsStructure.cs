using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Physics2DDotNet;
using Physics2DDotNet.Ignorers;
using AdvanceMath;
using Jypeli.Controls;

namespace Jypeli
{
    /// <summary>
    /// Rakenne, joka pitää fysiikkaoliot kiinteän matkan päässä toisistaan.
    /// </summary>
    public class PhysicsStructure : GameObjects.GameObjectBase, IPhysicsObjectInternal
    {
        private double _softness = 0;
        private List<PhysicsObject> objects;

        /// <summary>
        /// Onko rakenne lisätty peliin.
        /// </summary>
        public bool IsAddedToGame { get; set; }

        /// <summary>
        /// Rakenteeseen kuuluvat oliot.
        /// </summary>
        public IList<PhysicsObject> Objects
        {
            get { return objects.AsReadOnly(); }
        }

        /// <summary>
        /// Rakenteeseen kuuluvat liitokset.
        /// </summary>
        internal List<AxleJoint> Joints { get; private set; }

        /// <summary>
        /// Olioiden välisten liitosten pehmeys.
        /// </summary>
        public double Softness
        {
            get { return _softness; }
            set
            {
                _softness = value;

                for ( int i = 0; i < Joints.Count; i++ )
                {
                    Joints[i].Softness = value;
                }
            }
        }

        public BoundingRectangle BoundingRectangle
        {
            get
            {
                if ( objects.Count == 0 )
                    return new BoundingRectangle();

                double top = objects[0].Top;
                double left = objects[0].Left;
                double bottom = objects[0].Bottom;
                double right = objects[0].Right;

                for ( int i = 1; i < objects.Count; i++ )
                {
                    if ( objects[i].Top > top ) top = objects[i].Top;
                    if ( objects[i].Left < left ) left = objects[i].Left;
                    if ( objects[i].Bottom < bottom ) bottom = objects[i].Bottom;
                    if ( objects[i].Right > right ) right = objects[i].Right;
                }

                return new BoundingRectangle( new Vector( left, top ), new Vector( right, bottom ) );
            }
        }

        #region Tagged
        
        public object Tag { get; set; }

        #endregion

        #region IGameObject

        public event Action AddedToGame;

        bool _isVisible = true;
        PhysicsObject centerObject;

        public bool IsVisible
        {
            get { return false; }
            set
            {
                foreach ( var obj in objects )
                {
                    obj.IsVisible = value;
                }

                _isVisible = value;
            }
        }

        public List<Listener> AssociatedListeners { get; private set; }

        /// <summary>
        /// Rakenteen paikka pelimaailmassa.
        /// </summary>
        public override Vector Position
        {
            get
            {
                return centerObject.Position;
            }
            set
            {
                Vector delta = value - Position;

                foreach ( var obj in objects )
                {
                    obj.Position += delta;
                }

                centerObject.Position = value;
            }
        }

        public override Vector Size
        {
            get
            {
                return BoundingRectangle.Size;
            }
            set
            {
                throw new NotImplementedException( "Setting size for a structure is not implemented." );
            }
        }

        public override Animation Animation
        {
            get { return null; }
            set
            {
                throw new InvalidOperationException( "Physics structure has no image or animation." );
            }
        }

        public Vector TextureWrapSize
        {
            get { return new Vector( 1, 1 ); }
            set { throw new NotImplementedException(); }
        }

        public bool TextureFillsShape
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public Color Color
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                foreach ( var obj in objects )
                    obj.Color = value;
            }
        }

        public Shape Shape
        {
            get { return Shape.Rectangle; }
            set { throw new NotImplementedException(); }
        }

        public override Angle Angle
        {
            // TODO: Rotation
            get { return Angle.Zero; }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region IPhysicsObject

        bool _ignoresGravity = false;
        bool _ignoresCollisionResponse = false;
        bool _ignoresExplosions = false;
        bool _ignoresPhysicsLogics = false;
        Ignorer _collisionIgnorer = null;
        int _collisionIgnoreGroup = 0;
        Coefficients _coeffs = PhysicsObject.DefaultCoefficients.Duplicate();
        double _linearDamping = 1;
        double _angularDamping = 1;
        double? _setMomentOfInertia = null;
        double _calcMomentOfInertia = 0;

        /// <summary>
        /// Tapahtuu kun olio törmää toiseen.
        /// </summary>
        public event CollisionHandler<IPhysicsObject, IPhysicsObject> Collided;

        /// <summary>
        /// Rakenneolio, johon tämä olio kuuluu.
        /// </summary>
        public PhysicsStructure ParentStructure
        {
            // No nested physics structures for now
            get { return null; }
        }

        public double Mass
        {
            get
            {
                double totalMass = 0;
                foreach ( var part in objects )
                    totalMass += part.Mass;
                return totalMass;
            }
            set
            {
                throw new NotImplementedException( "Setting mass for a structure is not implemented." );
            }
        }

        public Body Body
        {
            get
            {
                throw new InvalidOperationException( "Structure has no body." );
            }
        }

        public bool IgnoresGravity
        {
            get { return _ignoresGravity; }
            set
            {
                foreach ( var obj in objects )
                    obj.IgnoresGravity = value;

                _ignoresGravity = value;
            }
        }

        public bool IgnoresCollisionResponse
        {
            get { return _ignoresCollisionResponse; }
            set
            {
                foreach ( var obj in objects )
                    obj.IgnoresCollisionResponse = value;

                _ignoresCollisionResponse = value;
            }
        }

        public bool IgnoresExplosions
        {
            get { return _ignoresExplosions; }
            set
            {
                foreach ( var obj in objects )
                    obj.IgnoresExplosions = value;

                _ignoresExplosions = value;
            }
        }

        public bool IgnoresPhysicsLogics
        {
            get { return _ignoresPhysicsLogics; }
            set
            {
                foreach ( var obj in objects )
                    obj.IgnoresPhysicsLogics = value;

                _ignoresPhysicsLogics = value;
            }
        }

        public Ignorer CollisionIgnorer
        {
            get { return _collisionIgnorer; }
            set
            {
                foreach ( var obj in objects )
                    obj.CollisionIgnorer = value;

                _collisionIgnorer = value;
            }
        }

        public int CollisionIgnoreGroup
        {
            get { return _collisionIgnoreGroup; }
            set
            {
                foreach ( var obj in objects )
                    obj.CollisionIgnoreGroup = value;

                _collisionIgnoreGroup = value;
            }
        }

        public double StaticFriction
        {
            get { return _coeffs.StaticFriction; }
            set
            {
                foreach ( var obj in objects )
                    obj.StaticFriction = value;

                _coeffs.StaticFriction = value;
            }
        }

        public double KineticFriction
        {
            get { return _coeffs.DynamicFriction; }
            set
            {
                foreach ( var obj in objects )
                    obj.KineticFriction = value;

                _coeffs.DynamicFriction = value;
            }
        }

        public double Restitution
        {
            get { return _coeffs.Restitution; }
            set
            {
                foreach ( var obj in objects )
                    obj.Restitution = value;

                _coeffs.Restitution = value;
            }
        }

        public double LinearDamping
        {
            get { return _linearDamping; }
            set
            {
                foreach ( var obj in objects )
                    obj.LinearDamping = value;

                _linearDamping = value;
            }
        }

        public double AngularDamping
        {
            get { return _angularDamping; }
            set
            {
                foreach ( var obj in objects )
                    obj.AngularDamping = value;

                _angularDamping = value;
            }
        }

        public Vector Velocity
        {
            get
            {
                var velocities = objects.ConvertAll<PhysicsObject, Vector>( delegate( PhysicsObject o ) { return o.Velocity; } );
                return Vector.Average( velocities );
            }
            set
            {
                foreach ( var obj in objects )
                    obj.Velocity = value;
            }
        }

        public double AngularVelocity
        {
            get
            {
                IEnumerable<double> velocities = objects.ConvertAll<PhysicsObject, double>( delegate( PhysicsObject o ) { return o.AngularVelocity; } );
                return velocities.Average();
            }
            set
            {
                foreach ( var obj in objects )
                    obj.AngularVelocity = value;
            }
        }

        public Vector Acceleration
        {
            get
            {
                var accs = objects.ConvertAll<PhysicsObject, Vector>( delegate( PhysicsObject o ) { return o.Acceleration; } );
                return Vector.Average( accs );
            }
            set
            {
                foreach ( var obj in objects )
                    obj.Acceleration = value;
            }
        }

        public double AngularAcceleration
        {
            get
            {
                IEnumerable<double> accs = objects.ConvertAll<PhysicsObject, double>( delegate( PhysicsObject o ) { return o.AngularAcceleration; } );
                return accs.Average();
            }
            set
            {
                foreach ( var obj in objects )
                    obj.AngularAcceleration = value;
            }
        }

        public double MomentOfInertia
        {
            get
            {
                return _setMomentOfInertia.HasValue ? _setMomentOfInertia.Value : _calcMomentOfInertia;
            }
            set
            {
                _setMomentOfInertia = value;
            }
        }

        /// <summary>
        /// Jos <c>false</c>, olio ei voi pyöriä.
        /// </summary>
        public bool CanRotate
        {
            get { return !double.IsPositiveInfinity( MomentOfInertia ); }
            set
            {
                if ( !value )
                {
                    MomentOfInertia = double.PositiveInfinity;
                }
                else
                {
                    CalculateMomentOfInertia();
                    _setMomentOfInertia = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Luo uuden tyhjän rakenteen.
        /// </summary>
        public PhysicsStructure()
        {
            centerObject = new PhysicsObject( 1, 1 ) { IgnoresPhysicsLogics = true, IsVisible = false };
            objects = new List<PhysicsObject>();
            Joints = new List<AxleJoint>();
            AssociatedListeners = new List<Listener>();
            AddedToGame += AddJoints;
            Removed += RemoveJoints;
        }

        private void AddJoints()
        {
            Joints.ForEach( ( (PhysicsGameBase)Game.Instance ).Add );
        }

        private void RemoveJoints()
        {
            Joints.ForEach( ( (PhysicsGameBase)Game.Instance ).Remove );
        }

        /// <summary>
        /// Luo uuden rakenteen ja varustaa sen fysiikkaolioilla.
        /// </summary>
        /// <param name="objs">Fysiikkaoliot</param>
        public PhysicsStructure( params PhysicsObject[] objs )
            : this()
        {
            for ( int i = 0; i < objs.Length; i++ )
            {
                Add( objs[i] );
            }
        }

        /// <summary>
        /// Kutsutaan kun olio lisätään peliin.
        /// </summary>
        public void OnAddedToGame()
        {
            if ( AddedToGame != null )
                AddedToGame();
            //brain.AddToGameEvent();
        }

        /// <summary>
        /// Kutsutaan kun törmätään.
        /// </summary>
        internal void OnCollided( IPhysicsObject part, IPhysicsObject target )
        {
            if ( Collided != null )
                Collided( this, target );
        }

        public void Update( Time time )
        {
            foreach ( var obj in objects )
            {
                if ( obj.IsUpdated )
                    obj.Update( time );
            }
        }

        /// <summary>
        /// Lisää olion rakenteeseen.
        /// </summary>
        /// <param name="obj">Lisättävä olio</param>
        public void Add( GameObject obj )
        {
            if ( !( obj is PhysicsObject ) )
                throw new NotImplementedException( "Currently only PhysicsObjects can be added to a structure." );

            if ( !IsAddedToGame )
            {
                // Add to game and use relative coordinates
                obj.Position += this.Position;
                Game.Instance.Add( obj );
            }

            PhysicsObject physObj = (PhysicsObject)obj;
            physObj.ParentStructure = this;
            physObj.IsVisible = _isVisible;
            physObj.IgnoresGravity = _ignoresGravity;
            physObj.IgnoresCollisionResponse = _ignoresCollisionResponse;
            physObj.IgnoresExplosions = _ignoresExplosions;
            physObj.IgnoresPhysicsLogics = _ignoresPhysicsLogics;
            physObj.CollisionIgnorer = _collisionIgnorer;
            physObj.CollisionIgnoreGroup = _collisionIgnoreGroup;
            physObj.Restitution = _coeffs.Restitution;
            physObj.StaticFriction = _coeffs.StaticFriction;
            physObj.KineticFriction = _coeffs.DynamicFriction;
            physObj.LinearDamping = _linearDamping;
            physObj.AngularDamping = _angularDamping;

            physObj.Collided += this.OnCollided;

            foreach ( var existing in objects )
            {
                AddJoint( physObj, existing );
            }

            objects.Add( physObj );
            CalculateMomentOfInertia();
        }

        private void AddJoint( PhysicsObject obj1, PhysicsObject obj2 )
        {
            AxleJoint joint = new AxleJoint( obj1, obj2 );
            joint.Softness = _softness;
            Joints.Add( joint );
            ( (PhysicsGameBase)Game.Instance ).Add( joint );
        }

        public void Remove( GameObject obj )
        {
            if ( !( obj is PhysicsObject ) )
                throw new NotImplementedException( "Currently only PhysicsObjects can be added to a structure." );

            PhysicsObject physObj = (PhysicsObject)obj;

            if ( !objects.Contains( physObj ) )
                return;

            physObj.ParentStructure = null;
            physObj.CollisionIgnorer = null;
            physObj.CollisionIgnoreGroup = 0;
            physObj.Collided -= this.OnCollided;

            foreach ( var joint in Joints.FindAll( j => j.Object1 == physObj || j.Object2 == physObj ) )
            {
                ( (PhysicsGameBase)Game.Instance ).Remove( joint );
            }

            objects.Remove( physObj );
            CalculateMomentOfInertia();
        }

        private void CalculateMomentOfInertia()
        {
            Vector center = this.Position;
            _calcMomentOfInertia = 0;

            foreach ( var part in objects )
            {
                double r = Vector.Distance( center, part.Position );
                _calcMomentOfInertia += part.Mass * r * r;
            }
        }

        public bool IsInside( Vector point )
        {
            foreach ( var obj in objects )
            {
                if ( obj.IsInside( point ) )
                    return true;
            }

            return false;
        }

        #region IPhysicsObject

        public void Hit( Vector impulse )
        {
            Vector velocity = impulse / Mass;

            foreach ( var obj in objects )
            {
                obj.Hit( velocity * obj.Mass );
            }
        }

        public void Push( Vector force )
        {
            Vector acceleration = force / Mass;

            foreach ( var obj in objects )
            {
                obj.Push( acceleration * obj.Mass );
            }
        }

        public void Push( Vector force, TimeSpan time )
        {
            Vector acceleration = force / Mass;

            foreach ( var obj in objects )
            {
                obj.Push( acceleration * obj.Mass, time );
            }
        }

        public void ApplyTorque( double torque )
        {
            if ( MomentOfInertia == 0 || double.IsInfinity( MomentOfInertia ) )
                return;

            double angularAcc = torque / MomentOfInertia;
            Vector center = this.Position;

            foreach ( var obj in objects )
            {
                Vector radius = obj.Position - center;
                double linearAcc = radius.Magnitude * angularAcc;
                obj.Push( linearAcc * obj.Mass * radius.LeftNormal );
            }
        }

        public void Move( Vector movement )
        {
            foreach ( var obj in objects )
            {
                obj.Move( movement );
            }
        }

        public override void MoveTo( Vector location, double speed, Action doWhenArrived )
        {
            centerObject.MoveTo( location, speed, doWhenArrived );

            foreach ( var obj in objects )
            {
                Vector displacement = obj.AbsolutePosition - centerObject.AbsolutePosition;
                obj.MoveTo( location + displacement, speed );
            }
        }

        public void StopMoveTo()
        {
            foreach ( var obj in objects )
                obj.StopMoveTo();
        }

        public void Stop()
        {
            foreach ( var obj in objects )
                obj.Stop();
        }

        public void StopHorizontal()
        {
            foreach ( var obj in objects )
                obj.StopHorizontal();
        }

        public void StopVertical()
        {
            foreach ( var obj in objects )
                obj.StopVertical();
        }

        #endregion

        #region DelayedDestroyable

        #region DelayedDestroyable

        /// <summary>
        /// Onko olio tuhoutumassa.
        /// </summary>
        public bool IsDestroying { get; private set; }

        /// <summary>
        /// Tapahtuu, kun olion tuhoaminen alkaa.
        /// </summary> 
        public event Action Destroying;

        protected void OnDestroying()
        {
            if ( Destroying != null )
                Destroying();
        }

        public override void Destroy()
        {
            IsDestroying = true;
            OnDestroying();
            Game.DoNextUpdate( ReallyDestroy );
        }

        protected virtual void ReallyDestroy()
        {
            foreach ( var joint in Joints ) joint.Destroy();
            foreach ( var obj in objects ) obj.Destroy();
            this.MaximumLifetime = new TimeSpan( 0 );
            base.Destroy();
        }

        #endregion

        #endregion

        #region Unimplemented Members / IGameObject

        public IGameObject Parent
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}
