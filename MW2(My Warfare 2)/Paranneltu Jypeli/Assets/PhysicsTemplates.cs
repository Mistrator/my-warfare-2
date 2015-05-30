using System;

namespace Jypeli.Assets
{
    /// <summary>
    /// Sisältää funktioita, joilla fysiikkaoliolle voidaan asettaa tiettyä tarkoitusta varten
    /// optimoidut ominaisuudet.
    /// </summary>
    public static class PhysicsTemplates
    {
        static readonly Vector[] rampRightVertices = new Vector[]
        {
            new Vector(-0.5, -0.5),
            new Vector(0.5, -0.5),
            new Vector(0.5, 0.5),
        };

        static readonly IndexTriangle[] rampRightTriangles = new IndexTriangle[]
        {
            new IndexTriangle( 2, 1, 0 )
        };

        static readonly Polygon rampRightShape = new Polygon(
            new ShapeCache( rampRightVertices, rampRightTriangles ) );

        /// <summary>
        /// Optimoi fysiikkaominaisuudet rampille, jonka päältä sivulta kuvattu
        /// auto voi ajaa.
        /// </summary>
        /// <param name="o">Olio, jolle asetetaan fysiikkaominaisuudet.</param>
        /// <param name="wheelDiameter">Auton pyörän halkaisija.</param>
        public static void ApplyRampRight( PhysicsObject o, double wheelDiameter )
        {
            o.SetShape( rampRightShape, GetDrivingSurfaceParameters( o, wheelDiameter ) );
        }

        private static CollisionShapeParameters GetDrivingSurfaceParameters( PhysicsObject o, double wheelDiameter )
        {
            CollisionShapeParameters p;
            p.DistanceGridSpacing = Math.Min( wheelDiameter / 3, o.Width / 8 );
            p.MaxVertexDistance = Math.Min( wheelDiameter / 2, o.Width / 4 );
            return p;
        }

        static readonly Vector[] rampLeftVertices = new Vector[]
        {
            new Vector(-0.5, -0.5),
            new Vector(0.5, -0.5),
            new Vector(-0.5, 0.5),
        };

        static readonly IndexTriangle[] rampLeftTriangles = new IndexTriangle[]
        {
            new IndexTriangle( 2, 1, 0 )
        };

        static readonly Polygon rampLeftShape = new Polygon(
            new ShapeCache( rampLeftVertices, rampLeftTriangles ) );

        /// <summary>
        /// Optimoi fysiikkaominaisuudet rampille, jonka päältä sivulta kuvattu
        /// auto voi ajaa.
        /// </summary>
        /// <param name="o">Olio, jolle asetetaan fysiikkaominaisuudet.</param>
        /// <param name="wheelDiameter">Auton pyörän halkaisija.</param>
        public static void ApplyRampLeft( PhysicsObject o, double wheelDiameter )
        {
            o.SetShape( rampLeftShape, GetDrivingSurfaceParameters( o, wheelDiameter ) );
        }

        /// <summary>
        /// Optimoi fysiikkaominaisuudet laatikolle, joita pinotaan monta päällekkäin,
        /// niin että ne pysyvät mahdollisimman vakaina.
        /// </summary>
        /// <param name="o">Olio, jolle asetetaan fysiikkaominaisuudet.</param>
        /// <param name="minBoxWidth">Pienimmän samassa pinossa olevan laatikon koko.</param>
        public static void ApplyStackableBox( PhysicsObject o, double minBoxWidth )
        {
            CollisionShapeParameters p = new CollisionShapeParameters();
            o.LinearDamping = 0.96;
            o.AngularDamping = 0.96;
            p.MaxVertexDistance = Math.Min( minBoxWidth / 4, o.Height / 2 );
            p.DistanceGridSpacing = Math.Min( minBoxWidth / 8, o.Height / 2 );
            o.SetShape( Shape.Rectangle, p );
        }
    }
}
