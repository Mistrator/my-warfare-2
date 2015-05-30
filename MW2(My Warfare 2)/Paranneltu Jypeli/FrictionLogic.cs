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
using AdvanceMath;
using AdvanceMath.Geometry2D;
using Physics2DDotNet;
using Physics2DDotNet.PhysicsLogics;

namespace Jypeli
{
    /// <summary>
    /// Fysiikkalogiikkaluokka ylhäältä päin kuvattuihin peleihin, joissa tarvitaan kitkaa.
    /// </summary>
#if !XBOX && !WINDOWS_PHONE
    [Serializable]
#endif
    internal sealed class FrictionLogic : PhysicsLogic
    {
        private TopDownPhysicsGame parent;

        public FrictionLogic( TopDownPhysicsGame parent, Lifespan lifetime )
            : base( lifetime )
        {
            this.parent = parent;
        }

        private bool Overlap( Body b, Body surface )
        {
            if ( !b.Shape.CanGetIntersection || !surface.Shape.CanGetIntersection )
                return false;

            Vector2D offset = b.State.Position.Linear - surface.State.Position.Linear;
            IntersectionInfo ii;
            var rect = surface.Rectangle;
            double surfaceWidth = Math.Abs( rect.Max.X - rect.Min.X );
            double surfaceHeight = Math.Abs( rect.Max.Y - rect.Min.Y );
            double targetRadius = Math.Sqrt( Math.Pow( surfaceWidth, 2 ) + Math.Pow( surfaceHeight, 2 ) );

            for ( int i = 0; i < b.Shape.Vertexes.Length; i++ )
            {
                Vector2D vec = b.Shape.Vertexes[i] + offset + Vector2D.FromLengthAndAngle( 10, b.Shape.Vertexes[i].Angle );

                if ( surface.Shape.TryGetIntersection( vec, out ii ) )
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Käyttää kitkalogiikkaa pelitilanteeseen.
        /// </summary>
        /// <param name="step">Aika viimeisestä päivityksestä.</param>
        protected internal override void RunLogic( TimeStep step )
        {
            double surfaceFriction = parent.KineticFriction;
            double gravityNormal = parent.Gravity;

            foreach ( Body e in Bodies )
            {
                if ( e.IgnoresPhysicsLogics ) continue;
                if ( !( e.Tag is PhysicsObject ) ) continue;

                double frictionCoefficient = 0;
                if ( Game.Instance is TopDownPhysicsGame )
                    frictionCoefficient = ( (TopDownPhysicsGame)Game.Instance ).KineticFriction;

                foreach ( var surface in parent.Surfaces )
                {
                    if ( surface.Body == e )
                        continue;

                    if ( Overlap( e, surface.Body ) )
                    {
                        // Use the object's friction
                        frictionCoefficient = Math.Sqrt( surface.KineticFriction * e.Coefficients.DynamicFriction );

                        // TODO: Instead of stopping on the first surface found, look for
                        // the surface with highest Z value.
                        break;
                    }
                    else
                    {
                        // Use the global friction
                        frictionCoefficient = Math.Sqrt( surfaceFriction * e.Coefficients.DynamicFriction );
                    }
                }

                float friction = (float)( frictionCoefficient * gravityNormal * e.Mass.MassInv * step.Dt );
                if ( e.State.Velocity.Linear.Magnitude <= friction )
                    e.State.Velocity.Linear = Vector2D.Zero;
                else
                    e.State.Velocity.Linear.Magnitude -= friction;
            }
        }
    }
}
