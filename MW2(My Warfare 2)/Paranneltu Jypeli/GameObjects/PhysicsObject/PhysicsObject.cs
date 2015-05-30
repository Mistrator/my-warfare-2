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
using Physics2DDotNet;
using Physics2DDotNet.Shapes;

namespace Jypeli
{
    /// <summary>
    /// Peliolio, joka noudattaa fysiikkamoottorin määräämiä fysiikan lakeja.
    /// Voidaan kuitenkin myös laittaa noudattamaan lakeja valikoidusti.
    /// </summary>
    public partial class PhysicsObject : GameObject, IPhysicsObjectInternal
    {
        /// <summary>
        /// Olioon vaikuttavat voimat
        /// </summary>
        internal List<Force> ActiveForces = new List<Force>();

        /// <summary>
        /// Rakenneolio, johon tämä olio kuuluu.
        /// </summary>
        public PhysicsStructure ParentStructure { get; internal set; }

        /// <summary>
        /// Jättääkö olio räjähdyksen paineaallon huomiotta.
        /// </summary>
        public bool IgnoresExplosions { get; set; }

        /// <summary>
        /// Jättääkö olio painovoiman huomioimatta.
        /// </summary>
        public bool IgnoresGravity
        {
            get { return Body.IgnoresGravity; }
            set { Body.IgnoresGravity = value; }
        }

        /// <summary>
        /// Jättääkö olio kaikki fysiikkalogiikat (ks. <c>AddPhysicsLogic</c>)
        /// huomiotta. Vaikuttaa esim. painovoimaan, mutta ei törmäyksiin.
        /// </summary>
        public bool IgnoresPhysicsLogics
        {
            get { return Body.IgnoresPhysicsLogics; }
            set { Body.IgnoresPhysicsLogics = value; }
        }

        public delegate void UpdateHandler(PhysicsObject updatedObject);
        public event UpdateHandler Updated;
        public bool NeedsUpdateCall = false;

        #region Constructors

        /// <summary>
        /// Luo uuden fysiikkaolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public PhysicsObject( double width, double height )
            : this( width, height, Shape.Rectangle )
        {
        }

        /// <summary>
        /// Luo uuden fysiikkaolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto.</param>
        public PhysicsObject( double width, double height, Shape shape )
            : this( width, height, shape, CreatePhysicsShape( shape, new Vector( width, height ) ) )
        {
        }

        /// <summary>
        /// Luo uuden fysiikkaolion.
        /// Kappaleen koko ja ulkonäkö ladataan parametrina annetusta kuvasta.
        /// </summary>
        /// <param name="image">Kuva</param>
        public PhysicsObject( Image image )
            : this( image.Width, image.Height, Shape.Rectangle )
        {
            this.Image = image;
        }

        public PhysicsObject( double width, double height, Shape shape, CollisionShapeParameters shapeParameters )
            : this( width, height, shape, CreatePhysicsShape( shape, new Vector( width, height ), shapeParameters ) )
        {
        }

        [Obsolete( "Use CollisionShapeParameters or the PhysicsTemplates class." )]
        internal PhysicsObject( double width, double height, Shape shape, CollisionShapeQuality quality )
            : this( width, height, shape, CreatePhysicsShape( shape, new Vector( width, height ) ) )
        {
        }

        [Obsolete( "Use constructor with CollisionShapeParameters" )]
        internal PhysicsObject( double width, double height, Shape shape, double maxDistanceBetweenVertices, double gridSpacing )
            : this( width, height, shape, new CollisionShapeParameters( maxDistanceBetweenVertices, gridSpacing ) )
        {
        }

        /// <summary>
        /// Luo fysiikkaolion, jonka muotona on säde.
        /// </summary>
        /// <param name="raySegment">Säde.</param>
        public PhysicsObject( RaySegment raySegment )
            : this( 1, 1, raySegment )
        {
        }

        /// <summary>
        /// Initializes the object with the given physics shape. The size of
        /// the physicsShape must be the one given.
        /// </summary>
        internal PhysicsObject( double width, double height, Shape shape, IShape physicsShape )
            : base( width, height, shape )
        {
            Coefficients c = new Coefficients( DefaultCoefficients.Restitution, DefaultCoefficients.StaticFriction, DefaultCoefficients.DynamicFriction );
            Body = new Body( new PhysicsState( ALVector2D.Zero ), physicsShape, DefaultMass, c, new Lifespan() );
            Body.Tag = this;
            Body.Collided += this.OnCollided;
            InitializeFireSystem();
        }

        #endregion

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
            Body.Lifetime.IsExpired = true;
            base.Destroy();
        }

        #endregion

        public override void Update( Time time )
        {
            if ( Velocity.Magnitude > MaxVelocity )
                Velocity = Vector.FromLengthAndAngle( MaxVelocity, Velocity.Angle );
            if ( AngularVelocity > MaxAngularVelocity )
                AngularVelocity = MaxVelocity;

            for ( int i = ActiveForces.Count - 1; i >= 0; i-- )
            {
                if ( ActiveForces[i].IsDestroyed() )
                {
                    ActiveForces.RemoveAt( i );
                    continue;
                }

                // Apply the force
                Push( ActiveForces[i].Value * time.SinceLastUpdate.TotalSeconds );
            }
            if (this != null && this.IsAddedToGame && this.NeedsUpdateCall) Updated(this);
            base.Update( time );
        }
    }
}
