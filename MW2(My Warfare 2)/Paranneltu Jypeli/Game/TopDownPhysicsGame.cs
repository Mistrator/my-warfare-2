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
using Physics2DDotNet;
using Physics2DDotNet.Joints;
using Physics2DDotNet.Solvers;
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Peli, johon voi lisätä pintoja, joiden päällä oliot voivat liukua. Peliin lisätyt <code>PhysicsObject</code>-oliot
    /// käyttäytyvät fysiikan lakien mukaan.
    /// </summary>
    public class TopDownPhysicsGame : PhysicsGameBase
    {
        private FrictionLogic frictionlogic;
        private List<PhysicsObject> surfaces = new List<PhysicsObject>();

        private SequentialImpulsesSolver solver;

        /// <summary>
        /// The surfaces on which other objects may slide.
        /// </summary>
        /// <remarks>
        /// The purpose of having surfaces in a separate list is to
        /// have less checks between objects and thus keep the performance
        /// good. Although the worst-case scenario is still O(n^2), this
        /// happens only when there are a lot of surfaces. One way to
        /// optimize that might be to store the surfaces in a spatial hash.
        /// 
        /// It might make sense not to add the surfaces to the physics engine at all.
        /// Or to have only IShape objects as surfaces instead of physics objects.
        /// Although someone might of course want to listen collisions for surfaces.
        /// </remarks>
        internal List<PhysicsObject> Surfaces { get { return surfaces; } }
       
        /// <summary>
        /// Painovoima. Mitä suurempi painovoima, sitä suurempi liikekitka
        /// kaikille olioille.
        /// </summary>
        public double Gravity { get; set; }

        /// <summary>
        /// Liikekitka pinnalla. Tätä arvoa käytetään, kun liikkuva kappale
        /// ei ole minkään lisätyn pinnan päällä. Arvot tyypillisesti välillä
        /// <c>0.0</c>-<c>1.0</c>.
        /// </summary>
        public double KineticFriction { get; set; }

        /// <summary>
        /// Alustaa uuden fysiikkapelin.
        /// </summary>
        public TopDownPhysicsGame()
            : this( 1 )
        {
        }

        /// <summary>
        /// Alustaa uuden fysiikkapelin.
        /// </summary>
        /// <param name="device">Mikä monitori käytössä, 1=ensimmäinen</param>
        public TopDownPhysicsGame( int device )
            : base( device )
        {
            phsEngine.BroadPhase = new Physics2DDotNet.Detectors.SelectiveSweepDetector();
            solver = new SequentialImpulsesSolver();
            phsEngine.Solver = (CollisionSolver)solver;

            Gravity = 1.0;
            KineticFriction = 1.0;

            SetPhysics();
        }

        private void SetPhysics()
        {
            solver.Iterations = 12;
            solver.SplitImpulse = true;
            //solver.BiasFactor = 0.7f;
            solver.BiasFactor = 0.0f;
            //solver.AllowedPenetration = 0.1f;
            solver.AllowedPenetration = 0.01f;

            frictionlogic = new FrictionLogic( this, new Lifespan() );
            phsEngine.AddLogic( frictionlogic );
        }

        /// <summary>
        /// Lisää peliin pinnan, jonka päällä muut oliot voivat liukua.
        /// </summary>
        /// <remarks>
        /// Pinnalle asetetaan automaattisesti <c>IgnoresCollisionResponse</c> arvoon <c>true</c>.
        /// </remarks>
        /// <param name="surface"></param>
        public void AddSurface( PhysicsObject surface )
        {
            surface.IgnoresCollisionResponse = true;
            surfaces.Add( surface );
            Add( surface );
        }

        /// <summary>
        /// Nollaa kaiken (kontrollit, näyttöobjektit, ajastimet ja fysiikkamoottorin).
        /// </summary>
        public override void ClearAll()
        {
            surfaces.Clear();
            base.ClearAll();
        }
    }
}
