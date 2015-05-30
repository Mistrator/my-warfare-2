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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

#if !XBOX && !WINDOWS_PHONE
using System.Runtime.Serialization.Formatters.Binary;
#endif

using Microsoft.Xna.Framework;
using Jypeli.Controls;
using System.Reflection;


namespace Jypeli
{
    /// <summary>
    /// Pelialueella liikkuva olio.
    /// Käytä fysiikkapeleissä <c>PhysicsObject</c>-olioita.
    /// </summary>
#if !XBOX && !WINDOWS_PHONE
    [Serializable]
#endif
    [Save]
    public class GameObject : GameObjects.GameObjectBase, IGameObjectInternal
    {
        #region Fields

#if !XBOX && !WINDOWS_PHONE
        [NonSerialized]
#endif
        private Color color = Color.White;
        private Vector size;
        private Shape shape;
        private Vector textureWrapSize = new Vector( 1, 1 );

        internal List<IGameObject> childObjects = null;
        private Queue<IGameObject> childObjectsToRemove = null;

        public List<Listener> AssociatedListeners { get; private set; }

        private bool textureFillsShape = false;
        private bool isAddedToGame = false;

        protected Timer moveTimer = null;
        protected Vector? moveTarget = null;
        protected double moveSpeed;

        #endregion

        /// <summary>
        /// Olion lapsioliot. Ei voi muokata.
        /// </summary>
        public IEnumerable<IGameObject> Objects
        {
            get { InitializeObjectLists(); return childObjects; } 
        }

        /// <summary>
        /// Piirretäänkö oliota ruudulle.
        /// </summary>
        [Save] public bool IsVisible { get; set; }

        /// <summary>
        /// Olion koko pelimaailmassa.
        /// Kertoo olion äärirajat, ei muotoa.
        /// </summary>
        public override Vector Size
        {
            get { return size; }
            set
            {
                if ( value.X < 0.0 || value.Y < 0.0 )
                    throw new ArgumentException( "The size must be positive." );

                // TODO: this doesn't work properly.

                Vector oldSize = size;
                Vector newSize = value;
                size = value;

                double xFactor = newSize.X / oldSize.X;
                double yFactor = newSize.Y / oldSize.Y;
                if (childObjects != null)
                {
                    foreach (var o in childObjects)
                    {
                        Vector oldChildSize = o.Size;
                        o.Size = new Vector(oldChildSize.X * xFactor, oldChildSize.Y * yFactor);

                        //                    Vector direction = o.Position.Normalize();
                        //                    double distance = o.Position.Magnitude;
                        Vector oldChildPosition = o.Position;
                        o.Position = new Vector(oldChildPosition.X * xFactor, oldChildPosition.Y * yFactor);
                    }
                }
            }
        }

        /// <summary>
        /// Olion kulma tai rintamasuunta.
        /// Nolla = osoittaa oikealle.
        /// </summary>      
        public override Angle Angle { get; set; }

        /// <summary>
        /// Animaatio. Voi olla <c>null</c>, jolloin piirretään vain väri.
        /// </summary>
        public override Animation Animation { get; set; }

        /// <summary>
        /// Määrittää kuinka moneen kertaan kuva piirretään. Esimerkiksi (3.0, 2.0) piirtää
        /// kuvan 3 kertaa vaakasuunnassa ja 2 kertaa pystysuunnassa.
        /// </summary>
        public Vector TextureWrapSize
        {
            get { return textureWrapSize; }
            set { textureWrapSize = value; }
            }

        /// <summary>
        /// Väri, jonka värisenä olio piirretään, jos tekstuuria ei ole määritelty.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Jos <c>true</c>, kuva piirretään niin, ettei se mene olion muodon
        /// ääriviivojen yli. Toisin sanoen kuva piirretään tarkasti vain
        /// muodon määrittämälle alueelle.
        /// </summary>
        /// <remarks>
        /// Tämän asettaminen tekee olion piirtämisestä hitaamman. Jos
        /// muoto on yksinkertainen, harkitse voisiko kuvan piirtää niin, että
        /// läpinäkyvyyttä käyttämällä saadaan kuvasta halutun muotoinen.
        /// </remarks>
        public bool TextureFillsShape
        {
            get { return textureFillsShape; }
            set { textureFillsShape = value; }
        }

        /// <summary>
        /// Olion muoto.
        /// </summary>
        public virtual Shape Shape
        {
            get { return shape; }
            set { shape = value; }
        }

        /// <summary>
        /// Olion muoto merkkijonona (kenttäeditorin käyttöön)
        /// </summary>
        internal string ShapeString
        {
            get { return Shape.GetType().Name; }
            set { Shape = Shape.FromString( value ); }
        }

        /// <summary>
        /// Peli, johon olio on lisätty. <c>null</c>, jos
        /// oliota ei ole lisätty peliin.
        /// </summary>
        [Obsolete("Käytä Game.Instance ja IsAddedToGame")]
        public Game Game
        {
            get { return isAddedToGame ? Game.Instance : null; }
        }

        /// <summary>
        /// Onko olio lisätty peliin.
        /// </summary>
        public bool IsAddedToGame
        {
            get { return isAddedToGame; }
            set
            {
                isAddedToGame = value;
                if ( childObjects != null )
                {
                    foreach ( var o in childObjects )
                        o.IsAddedToGame = value;
                }
            }
        }

        /// <summary>
        /// Tapahtuu, kun on saavuttu haluttuun paikkaan (MoveTo-metodi)
        /// </summary>
        [Obsolete("Käytä mieluummin Arrived-tapahtumaa (ottaa parametriksi IGameObject pelkän GameObjectin sijaan)")]
        public event Action<GameObject, Vector> ArrivedAt;

        /// <summary>
        /// Kutsutaan, kun on saavuttu haluttuun paikkaan (MoveTo-metodi)
        /// </summary>
        /// <param name="location"></param>
        [Obsolete( "Käytä mieluummin Arrived-tapahtumaa (ottaa parametriksi IGameObject pelkän GameObjectin sijaan)" )]
        public void OnArrivedAt( Vector location )
        {
            if ( ArrivedAt != null )
                ArrivedAt( this, location );
        }

        #region Destroyable

        /// <summary>
        /// Tuhoaa olion. Tuhottu olio poistuu pelistä.
        /// </summary>
        public override void Destroy()
        {
            this.MaximumLifetime = TimeSpan.Zero;

            if ( childObjects != null )
            {
                foreach ( GameObject child in childObjects )
                {
                    child.Destroy();
                }
            }

            if ( AssociatedListeners != null )
            {
                foreach ( Listener listener in AssociatedListeners )
                {
                    listener.Destroy();
                }
            }

            base.Destroy();
        }

        #endregion

        /// <summary>
        /// Alustaa uuden peliolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public GameObject( double width, double height )
            : this( width, height, Shape.Rectangle )
        {
        }

        /// <summary>
        /// Alustaa uuden peliolion.
        /// Kappaleen koko ja ulkonäkö ladataan parametrina annetusta kuvasta.
        /// </summary>
        /// <param name="image">Kuva</param>
        public GameObject(Image image)
            : this( image.Width, image.Height, Shape.Rectangle )
        {
            this.Image = image;
        }

        /// <summary>
        /// Alustaa uuden peliolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto.</param>
        public GameObject( double width, double height, Shape shape )
            : base()
        {
            this.IsVisible = true;
            this.size = new Vector( width, height );
            this.shape = shape;
            this.AssociatedListeners = new List<Listener>();
            this.Arrived += delegate( IGameObject obj, Vector location ) { OnArrivedAt( location ); };
        }

#if !XBOX && !WINDOWS_PHONE
        /// <summary>
        /// Tekee oliosta kopion.
        /// </summary>
        /// <returns>
        /// Uusi <see cref="GameObject"/>-olio samoilla arvoilla.
        /// </returns>
        public object Clone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize( ms, this );
            ms.Position = 0;

            object obj = bf.Deserialize( ms );
            ms.Close();

            // Copy nonserializable data
            ( (GameObject)obj ).Animation = new Animation( this.Animation );

            return obj;
        }
#endif

        /// <summary>
        /// Onko piste <c>p</c> tämän olion sisäpuolella.
        /// </summary>
        public bool IsInside(Vector point)
        {
            Vector p = this.AbsolutePosition;

            if ( AbsoluteAngle == Angle.Zero )
            {
                // A special (faster) case of the general case below
                if ( point.X >= ( p.X - Width / 2 )
                    && point.X <= ( p.X + Width / 2 )
                    && point.Y >= ( p.Y - Height / 2 )
                    && point.Y <= ( p.Y + Height / 2 ) ) return true;
            }
            else
            {
                Vector unitX = Vector.FromLengthAndAngle( 1, this.AbsoluteAngle );
                Vector unitY = unitX.LeftNormal;
                double pX = p.ScalarProjection( unitX );
                double pY = p.ScalarProjection( unitY );
                double pointX = point.ScalarProjection( unitX );
                double pointY = point.ScalarProjection( unitY );

                if ( pointX >= ( pX - Width / 2 )
                    && pointX <= ( pX + Width / 2 )
                    && pointY >= ( pY - Height / 2 )
                    && pointY <= ( pY + Height / 2 ) ) return true;
            }

            if ( childObjects == null ) return false;

            for ( int i = 0; i < childObjects.Count; i++ )
            {
                if ( childObjects[i].IsInside( point ) ) return true;
            }

            return false;
        }

        /// <summary>
        /// Lisää annetun peliolion tämän olion lapseksi. Lapsiolio liikkuu tämän olion mukana,
        /// ja sen paikka ja koko ilmaistaan suhteessa tähän olioon.
        /// </summary>
        /// <remarks>
        /// <c>PhysicsObject</c>-tyyppisiä olioita ei voi lisätä lapsiolioksi.
        /// </remarks>
        public void Add( IGameObject childObject )
        {
            var childObjectInt = (IGameObjectInternal)childObject;

            InitializeObjectLists();

            if ( childObjects.Contains( childObject ) )
                throw new ArgumentException( "The child object has already been added" );
            if ( childObject is PhysicsObject )
                throw new NotImplementedException( "Having a PhysicsObject as a child object is not supported." );

            childObjects.Add( childObject );
            childObject.Parent = this;

            if ( IsAddedToGame )
            {
                childObject.IsAddedToGame = true;
                childObjectInt.OnAddedToGame();
            }
            else
                AddedToGame += childObjectInt.OnAddedToGame;


            // Let's keep it simple and assume that child objects need updating,
            // even if that is not the case.
            if ( !IsUpdated )
                IsUpdated = true;
        }

        private void InitializeObjectLists()
        {
            if ( childObjects == null )
            {
                childObjects = new List<IGameObject>();
                childObjectsToRemove = new Queue<IGameObject>();
            }
        }

        /// <summary> 
        /// Poistaa lapsiolion. Jos haluat tuhota olion, 
        /// kutsu mielummin olion <c>Destroy</c>-metodia. 
        /// </summary> 
        /// <remarks> 
        /// Oliota ei poisteta välittömästi, vaan viimeistään seuraavan 
        /// päivityksen jälkeen. 
        /// </remarks> 
        public void Remove( IGameObject childObject )
        {
            childObjectsToRemove.Enqueue( childObject );
        }

        /// <summary>
        /// Peliolion päivitys. Tätä kutsutaan, kun <c>IsUpdated</c>-ominaisuuden
        /// arvoksi on asetettu <c>true</c> ja olio on lisätty peliin.
        /// <see cref="IsUpdated"/>
        /// </summary>
        /// <param name="time">Peliaika.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public virtual void Update( Time time )
        {
            base.Update( time );

            if (childObjects != null)
            {
                foreach (GameObject child in childObjects)
                {
                    child.Update(time);
                    if (child.IsDestroyed)
                    {
                        childObjectsToRemove.Enqueue(child);
                    }
                }

                while (childObjectsToRemove.Count > 0)
                {
                    IGameObject o = childObjectsToRemove.Dequeue();
                    childObjects.Remove(o);
                    this.AddedToGame -= ( (IGameObjectInternal)o ).OnAddedToGame;
                    o.IsAddedToGame = false;
                }
            }
        }

        /// <summary>
        /// Lataa kuvan tiedostosta ja asettaa sen oliolle.
        /// </summary>
        /// <param name="file"></param>
        public void SetImage( StorageFile file )
        {
            this.Image = Image.FromStream( file.Stream );
        }

        /// <summary>
        /// Siirtää oliota.
        /// </summary>
        /// <param name="movement">Vektori, joka määrittää kuinka paljon siirretään.</param>
        public virtual void Move( Vector movement )
        {
            Position += movement;
        }

        /// <summary>
        /// Yrittää siirtyä annettuun paikkaan annetulla nopeudella.
        /// Laukaisee tapahtuman ArrivedAt, kun paikkaan on päästy.
        /// </summary>
        /// <param name="location">Paikka johon siirrytään</param>
        /// <param name="speed">
        /// Nopeus (paikkayksikköä sekunnissa) jolla liikutaan.
        /// Nopeus on maksiminopeus. Jos välissä on hitaampaa maastoa tai
        /// esteitä, liikkumisnopeus voi olla alle sen.
        /// </param>
        public virtual void MoveTo( Vector location, double speed )
        {
            if ( moveTimer == null )
            {
                moveTimer = new Timer();
                moveTimer.Timeout += MoveToTarget;
                moveTimer.Interval = 0.01;
            }
            else if ( moveTimer.Enabled )
                moveTimer.Stop();

            moveSpeed = speed;
            moveTarget = location;
            moveTimer.Start();
        }

        protected virtual void MoveToTarget()
        {
            if ( !moveTarget.HasValue )
            {
                moveTimer.Stop();
                return;
            }

            Vector d = moveTarget.Value - Position;
            double vt = moveSpeed * moveTimer.Interval;

            if ( d.Magnitude < vt )
            {
                Vector targetLoc = moveTarget.Value;
                moveTimer.Stop();
                Position = moveTarget.Value;
                moveTarget = null;
                OnArrived( targetLoc );
            }
            else
                //Position += d.Normalize() * moveSpeed * sender.Interval;
                Position += Vector.FromLengthAndAngle( vt, d.Angle );
        }
    }
}
