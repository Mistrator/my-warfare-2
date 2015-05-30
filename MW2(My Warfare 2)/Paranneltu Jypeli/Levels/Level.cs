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
using System.Linq;
using System.Collections.Generic;
using Physics2DDotNet.Ignorers;
using Physics2DDotNet.Shapes;
using Jypeli.LevelEditor;
using Jypeli.Widgets;
using AdvanceMath;

namespace Jypeli
{
    public class ObjectLoadMethods
    {
        internal static readonly ObjectLoadMethods Empty = new ObjectLoadMethods();

        internal Dictionary<string, Func<GameObject, GameObject>> MethodsByTag = new Dictionary<string, Func<GameObject, GameObject>>();
        internal Dictionary<string, Func<GameObject, GameObject>> MethodsByTemplate = new Dictionary<string, Func<GameObject, GameObject>>();

        /// <summary>
        /// Lisää metodin ladatun olion muokkaamiseksi.
        /// Metodin tulee palauttaa muokkaamansa olio. Metodi voi myös palauttaa uuden olion, jos
        /// haluttu tyyppi ei ole tuettu editorissa.
        /// </summary>
        /// <param name="tag">Tagi, joka oliolla tulee olla.</param>
        /// <param name="method">Metodi, joka muokkaa oliota tai palauttaa uuden.</param>
        public void AddByTag( string tag, Func<GameObject, GameObject> method )
        {
            MethodsByTag.Add( tag, method );
        }

        /// <summary>
        /// Lisää metodin ladatun olion muokkaamiseksi.
        /// Metodin tulee palauttaa muokkaamansa olio. Metodi voi myös palauttaa uuden olion, jos
        /// haluttu olion tyyppi ei ole tuettu editorissa.
        /// </summary>
        /// <param name="tag">Template, jonka pohjalta olio on luotu.</param>
        /// <param name="method">Metodi, joka muokkaa oliota tai palauttaa uuden.</param>
        public void AddByTemplate( string template, Func<GameObject, GameObject> method )
        {
            MethodsByTemplate.Add( template, method );
        }
    }


    /// <summary>
    /// Pelikenttä, johon voi lisätä olioita. Kentällä voi myös olla reunat ja taustaväri tai taustakuva.
    /// </summary>
    [Save]
    public class Level
    {
        [Save] double _width = 1000;
        [Save] double _height = 800;

        private Game game;
        private ObjectIgnorer ignorerForBorders = new ObjectIgnorer();
#if WINDOWS
        private WindowsFileManager fileManager = new WindowsFileManager();
#endif

        public double AmbientLight { get; set; }

        Surface ground = null;

        /// <summary>
        /// Kentän keskipiste.
        /// </summary>
        public readonly Vector Center = Vector.Zero;

        /// <summary>
        /// Kentän taustaväri.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Kentän taustakuva.
        /// </summary>
        public Background Background { get; set; }

        /// <summary>
        /// Kentän leveys.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Kentän korkeus.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Kentän koko (leveys ja korkeus).
        /// </summary>
        public Vector Size
        {
            get { return new Vector( _width, _height ); }
            set { _width = value.X; _height = value.Y; }
        }

        /// <summary>
        /// Kentän vasemman reunan x-koordinaatti.
        /// </summary>
        public double Left
        {
            get { return -Width / 2; }
        }

        /// <summary>
        /// Kentän oikean reunan x-koordinaatti.
        /// </summary>
        public double Right
        {
            get { return Width / 2; }
        }

        /// <summary>
        /// Kentän yläreunan y-koordinaatti.
        /// </summary>
        public double Top
        {
            get { return Height / 2; }
        }

        /// <summary>
        /// Kentän alareunan y-koordinaatti.
        /// </summary>
        public double Bottom
        {
            get { return -Height / 2; }
        }

        internal Level( Game game )
        {
            this.game = game;
            AmbientLight = 1.0;
            BackgroundColor = Color.LightBlue; // default color

            // creates a null background
            this.Background = new Jypeli.Widgets.Background( Vector.Zero );
        }

        internal void Clear()
        {
            Background.Image = null;
        }

        /// <summary>
        /// Laskee pienimmän alueen, jonka sisälle kaikki kentän oliot mahtuvat.
        /// </summary>
        public BoundingRectangle FindObjectLimits()
        {
            var objectsAboutToBeAdded = Game.GetObjectsAboutToBeAdded();

            if ( ( Game.Instance.ObjectCount + objectsAboutToBeAdded.Count ) == 0 )
            {
                throw new InvalidOperationException( "There must be at least one object" );
            }

            double left = double.PositiveInfinity;
            double right = double.NegativeInfinity;
            double top = double.NegativeInfinity;
            double bottom = double.PositiveInfinity;

            foreach ( var layer in Game.Instance.Layers )
            {
                if (layer.IgnoresZoom)
                    continue;

                foreach ( var o in layer.Objects )
                {
                    if ( o.Left < left )
                        left = o.Left * layer.RelativeTransition.X;
                    if ( o.Right > right )
                        right = o.Right * layer.RelativeTransition.X;
                    if ( o.Top > top )
                        top = o.Top * layer.RelativeTransition.Y;
                    if ( o.Bottom < bottom )
                        bottom = o.Bottom * layer.RelativeTransition.Y;
                }
            }

            foreach ( var o in objectsAboutToBeAdded )
            {
                if ( o.Left < left )
                    left = o.Left;
                if ( o.Right > right )
                    right = o.Right;
                if ( o.Top > top )
                    top = o.Top;
                if ( o.Bottom < bottom )
                    bottom = o.Bottom;
            }

            return new BoundingRectangle( new Vector( left, top ), new Vector( right, bottom ) );
        }

        #region Borders

        private Surface CreateBorder( Direction direction, double restitution, bool isVisible, Image borderImage, Color borderColor )
        {
            Surface s = Surface.Create( this, direction );
            s.Restitution = restitution;
            s.IsVisible = isVisible;
            s.Image = borderImage;
            s.Color = borderColor;
            game.Add( s );
            return s;
        }

        private Surface CreateBorder( Direction direction, double min, double max, int points, double restitution, bool isVisible, Image borderImage, Color borderColor )
        {
            Surface s = Surface.Create( this, direction, min, max, points );
            s.Restitution = restitution;
            s.IsVisible = isVisible;
            s.Image = borderImage;
            s.Color = borderColor;
            game.Add( s );
            return s;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        public Surfaces CreateBorders()
        {
            return CreateBorders( PhysicsObject.DefaultCoefficients.Restitution, true );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateBorders( bool isVisible )
        {
            return CreateBorders( PhysicsObject.DefaultCoefficients.Restitution, isVisible );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateBorders( double restitution, bool isVisible )
        {
            return CreateBorders( restitution, isVisible, Color.Gray );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateBorders( double restitution, bool isVisible, Color borderColor )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, null, borderColor );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, null, borderColor );
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, null, borderColor );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, null, borderColor );
            return borders;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateBorders( double restitution, bool isVisible, Image borderImage )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, borderImage, Color.Gray );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, borderImage, Color.Gray );
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, borderImage, Color.Gray );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, borderImage, Color.Gray );
            return borders;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateHorizontalBorders( double restitution, bool isVisible, Color borderColor )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, null, borderColor );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, null, borderColor );
            return borders;
        }

        /// <summary>
        /// Lisää kentän pystysivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateVerticalBorders( double restitution, bool isVisible, Color borderColor )
        {
            Surfaces borders = new Surfaces();
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, null, borderColor );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, null, borderColor );
            return borders;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateHorizontalBorders( double restitution, bool isVisible, Image borderImage )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, borderImage, Color.Gray );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, borderImage, Color.Gray );
            return borders;
        }

        /// <summary>
        /// Lisää kentän pystysivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateVerticalBorders( double restitution, bool isVisible, Image borderImage )
        {
            Surfaces borders = new Surfaces();
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, borderImage, Color.Gray );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, borderImage, Color.Gray );
            return borders;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateBorders( double min, double max, int points, double restitution, Color borderColor )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, null, borderColor );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, null, borderColor );
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, null, borderColor );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, null, borderColor );
            return s;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateBorders( double min, double max, int points, double restitution, Image borderImage )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, borderImage, Color.Gray );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, borderImage, Color.Gray );
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, borderImage, Color.Gray );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, borderImage, Color.Gray );
            return s;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        public Surfaces CreateBorders( double min, double max, int points, double restitution )
        {
            return CreateBorders( min, max, points, restitution, Color.Gray );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        public Surfaces CreateBorders( double min, double max, int points )
        {
            return CreateBorders( min, max, points, PhysicsObject.DefaultCoefficients.Restitution, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points, double restitution )
        {
            return CreateHorizontalBorders( min, max, points, restitution, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points, double restitution, Image borderImage )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, borderImage, Color.Gray );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, borderImage, Color.Gray );
            return s;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points, double restitution, Color borderColor )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, null, borderColor );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, null, borderColor );
            return s;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points )
        {
            return CreateHorizontalBorders( min, max, points, PhysicsObject.DefaultCoefficients.Restitution );
        }

        /// <summary>
        /// Lisää kentän pystysivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points, double restitution )
        {
            return CreateVerticalBorders( min, max, points, restitution, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän pystysivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points, double restitution, Image borderImage )
        {
            Surfaces s = new Surfaces();
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, borderImage, Color.Gray );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, borderImage, Color.Gray );
            return s;
        }

        /// <summary>
        /// Lisää kentän pystysivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points, double restitution, Color borderColor )
        {
            Surfaces s = new Surfaces();
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, null, borderColor );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, null, borderColor );
            return s;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points )
        {
            return CreateVerticalBorders( min, max, points, PhysicsObject.DefaultCoefficients.Restitution );
        }

        private PhysicsObject CreateBorder( double width, double height )
        {
            PhysicsObject b = PhysicsObject.CreateStaticObject( width, height );
            b.Color = Color.Gray;
            b.Body.CollisionIgnorer = ignorerForBorders;
            game.Add( b );
            return b;
        }

        /// <summary>
        /// Lisää kenttään vasemman reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateLeftBorder( double restitution, bool isVisible )
        {
            double thickness = this.GetBorderThickness();
            PhysicsObject b = CreateBorder( this.Height, thickness );
            b.Angle = Angle.FromRadians( -Math.PI / 2 );
            b.Position = new Vector( Left - ( thickness / 2 ), Center.Y );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään oikean reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateRightBorder( double restitution, bool isVisible )
        {
            double thickness = this.GetBorderThickness();
            PhysicsObject b = CreateBorder( this.Height, thickness );
            b.Angle = Angle.FromRadians( Math.PI / 2 );
            b.Position = new Vector( Right + ( thickness / 2 ), Center.Y );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään yläreunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateTopBorder( double restitution, bool isVisible )
        {
            double thickness = this.GetBorderThickness();
            PhysicsObject b = CreateBorder( this.Width + ( 2 * thickness ), thickness );
            b.Angle = Angle.FromRadians( Math.PI );
            b.Position = new Vector( Center.X, Top + ( thickness / 2 ) );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään alareunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateBottomBorder( double restitution, bool isVisible )
        {
            double thickness = GetBorderThickness();
            PhysicsObject b = CreateBorder( Width + ( 2 * thickness ), thickness );
            b.Angle = Angle.Zero;
            b.Position = new Vector( Center.X, Bottom - ( thickness / 2 ) );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään vasemman reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateLeftBorder()
        {
            return CreateLeftBorder( PhysicsObject.DefaultCoefficients.Restitution, true );
        }

        /// <summary>
        /// Lisää kenttään oikean reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateRightBorder()
        {
            return CreateRightBorder( PhysicsObject.DefaultCoefficients.Restitution, true );
        }

        /// <summary>
        /// Lisää kenttään yläreunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateTopBorder()
        {
            return CreateTopBorder( PhysicsObject.DefaultCoefficients.Restitution, true );
        }

        /// <summary>
        /// Lisää kenttään alareunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateBottomBorder()
        {
            return CreateBottomBorder( PhysicsObject.DefaultCoefficients.Restitution, true );
        }

        internal double GetBorderThickness()
        {
            return this.Width / 10;
        }

        /// <summary>
        /// Helppo tapa lisätä kenttään epätasainen maasto.
        /// Maasto kuvataan luettelemalla Y-koordinaatteja vasemmalta oikealle lukien. Kahden Y-koordinaatin
        /// väli on aina sama.
        /// </summary>
        /// <param name="heights">Y-koordinaatit lueteltuna vasemmalta oikealle.</param>
        /// <param name="scale">Vakio, jolla jokainen Y-koordinaatti kerrotaan. Hyödyllinen,
        /// jos halutaan muuttaa koko maaston korkeutta muuttamatta jokaista pistettä yksitellen.
        /// Tavallisesti arvoksi kelpaa 1.0.</param>
        /// <remarks>
        /// Huomaa, että maastossa ei voi olla kahta pistettä päällekkäin.
        /// </remarks>
        public PhysicsObject CreateGround( double[] heights, double scale )
        {
            return CreateGround( heights, scale, null );
        }

        /// <summary>
        /// Helppo tapa lisätä kenttään epätasainen maasto.
        /// Maasto kuvataan luettelemalla Y-koordinaatteja vasemmalta oikealle lukien. Kahden Y-koordinaatin
        /// väli on aina sama.
        /// </summary>
        /// <param name="heights">Y-koordinaatit lueteltuna vasemmalta oikealle.</param>
        /// <param name="scale">Vakio, jolla jokainen Y-koordinaatti kerrotaan. Hyödyllinen,
        /// jos halutaan muuttaa koko maaston korkeutta muuttamatta jokaista pistettä yksitellen.
        /// Tavallisesti arvoksi kelpaa 1.0.</param>
        /// <param name="image">Maastossa käytettävä kuva.</param>
        /// <returns></returns>
        public PhysicsObject CreateGround( double[] heights, double scale, Image image )
        {
            ground = new Surface( this.Width, heights, scale );
            ground.Position = new Vector( Center.X, Bottom - ( MathHelper.Max( heights ) / 2 ) );
            ground.Image = image;
            game.Add( ground );
            return ground;
        }

        #endregion

        /// <summary>
        /// Palauttaa satunnaisen kohdan kentän reunojen sisältä.
        /// </summary>
        /// <returns>Vektori.</returns>
        public Vector GetRandomPosition()
        {
            return new Vector( RandomGen.NextDouble( Left, Right ), RandomGen.NextDouble( Bottom, Top ) );
        }

        /// <summary>
        /// Lataa kentän tiedostosta.
        /// Kenttätiedostoja voi tehdä Jypelin mukana tulevalle editorilla.
        /// </summary>
        /// <param name="fileName">Kenttätiedoston nimi</param>
        public void LoadFromFile( string fileName )
        {
            LoadFromFile( fileName, ObjectLoadMethods.Empty );
        }

        /// <summary>
        /// Lataa kentän tiedostosta.
        /// Kenttätiedostoja voi tehdä Jypelin mukana tulevalle editorilla.
        /// </summary>
        /// <param name="fileName">Kenttätiedoston nimi</param>
        /// <param name="methods">Metodit, joilla ladattuja olioita voidaan muokata</param>
        public void LoadFromFile( string fileName, ObjectLoadMethods methods )
        {
#if WINDOWS
            LevelData levelData = new LevelData();
            Factory.FactoryMethod createObject = delegate { return new LevelObject( levelData, 0 ); };
            Game.Instance.AddFactory<LevelObject>( null, createObject );
            levelData = fileManager.Load<LevelData>( levelData, fileName );
            Game.Instance.RemoveFactory<LevelObject>( null, createObject );
            DoLoadLevel( levelData, methods );
#else
            throw new NotImplementedException( "LoadFromFile not implemented for WP7 / Xbox360" );
#endif
        }

        /// <summary>
        /// Lataa kentän contentista.
        /// Kenttätiedostoja voi tehdä Jypelin mukana tulevalle editorilla.
        /// </summary>
        /// <param name="assetName">Kentän nimi contentissa</param>
        public void LoadFromContent( string assetName )
        {
            LoadFromContent( assetName, ObjectLoadMethods.Empty );
        }

        /// <summary>
        /// Lataa kentän contentista.
        /// Kenttätiedostoja voi tehdä Jypelin mukana tulevalle editorilla.
        /// </summary>
        /// <param name="assetName">Kentän nimi contentissa</param>
        /// <param name="methods">Metodit, joilla ladattuja olioita voidaan muokata</param>
        public void LoadFromContent( string assetName, ObjectLoadMethods methods )
        {
            LevelData levelData = new LevelData();
            Factory.FactoryMethod createObject = delegate { return new LevelObject( levelData, 0 ); };
            Game.Instance.AddFactory<LevelObject>( null, createObject );
            levelData = Game.DataStorage.LoadContent<LevelData>( levelData, assetName );
            Game.Instance.RemoveFactory<LevelObject>( null, createObject );
            DoLoadLevel( levelData, methods );
        }

        private void DoLoadLevel( LevelData levelData, ObjectLoadMethods methods )
        {
            foreach ( var levelObj in levelData.Objects )
            {
                var o = levelObj.ConstructObject();
                string tag = o.Tag.ToString();
                string template = levelObj.Template.Name;

                if ( methods.MethodsByTemplate.ContainsKey( template ) )
                {
                    o = methods.MethodsByTemplate[template]( o );
                    if ( o == null ) continue;
                }

                if ( methods.MethodsByTag.ContainsKey( tag ) )
                {
                    o = methods.MethodsByTag[tag]( o );
                    if ( o == null ) continue;
                }

                Game.Instance.Add( o );
            }
        }
    }
}
