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
using System.ComponentModel;
using Jypeli.Controls;
using Jypeli.GameObjects;


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
    public partial class GameObject : GameObjects.GameObjectBase, IGameObjectInternal
    {
        public List<Listener> AssociatedListeners { get; private set; }
        Timer fadeTimer;

        #region Destroyable

        /// <summary>
        /// Tuhoaa olion. Tuhottu olio poistuu pelistä.
        /// </summary>
        public override void Destroy()
        {
            this.MaximumLifetime = TimeSpan.Zero;

            DestroyChildren();

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
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto.</param>
        public GameObject( double width, double height, Shape shape )
            : base()
        {
            InitDimensions( width, height, shape );
            InitAppearance();
            InitListeners();
            InitLayout( width, height );
        }

        /// <summary>
        /// Alustaa uuden peliolion.
        /// Kappaleen koko ja ulkonäkö ladataan parametrina annetusta kuvasta.
        /// </summary>
        /// <param name="animation">Kuva</param>
        public GameObject( Animation animation )
            : base()
        {
            InitDimensions( animation.Width, animation.Height, Shape.Rectangle );
            InitAppearance( animation );
            InitListeners();
            InitLayout( animation.Width, animation.Height );
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        public GameObject( ILayout layout )
            : base()
        {
            Vector defaultSize = new Vector( 100, 100 );
            InitDimensions( defaultSize.X, defaultSize.Y, Shape.Rectangle );
            InitAppearance();
            InitListeners();
            InitLayout( defaultSize.X, defaultSize.Y, layout );
        }

        private void InitListeners()
        {
            this.AssociatedListeners = new List<Listener>();
        }

        /// <summary>
        /// Peliolion päivitys. Tätä kutsutaan, kun <c>IsUpdated</c>-ominaisuuden
        /// arvoksi on asetettu <c>true</c> ja olio on lisätty peliin.
        /// <see cref="IsUpdated"/>
        /// </summary>
        /// <param name="time">Peliaika.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public override void Update( Time time )
        {
            base.Update( time );
            UpdateChildren( time );
            if ( _layoutNeedsRefreshing )
            {
                RefreshLayout();
                _layoutNeedsRefreshing = false;
            }
            if ( oscillators != null )
                oscillators.Update( time );
        }

        /// <summary>
        /// Näkeekö olio toisen.
        /// </summary>
        /// <param name="obj">Toinen olio</param>
        /// <returns></returns>
        public bool SeesObject(GameObject obj)
        {
            return Game.Instance.GetFirstObject(obstacle => obstacle != this && obstacle != obj && !(obstacle is Widget) && obstacle.IsBlocking(this.Position, obj.Position)) == null;
        }

        /// <summary>
        /// Näkeekö olio toisen.
        /// </summary>
        /// <param name="obj">Toinen olio</param>
        /// <param name="isObstacle">Ehto sille mikä lasketaan esteeksi</param>
        /// <returns></returns>
        public bool SeesObject(GameObject obj, Predicate<GameObject> isObstacle)
        {
            return Game.Instance.GetFirstObject(obstacle => obstacle != this && obstacle != obj && isObstacle(obstacle) && obstacle.IsBlocking(this.Position, obj.Position)) == null;
        }

        /// <summary>
        /// Näkeekö olio toisen.
        /// </summary>
        /// <param name="obj">Toinen olio</param>
        /// <param name="obstacleTag">Tagi esteelle</param>
        /// <returns></returns>
        public bool SeesObject(GameObject obj, object obstacleTag)
        {
            return SeesObject(obj, o => o.Tag == obstacleTag);
        }

        /// <summary>
        /// Näkeekö olio paikkaan.
        /// </summary>
        /// <param name="targetPosition">Paikka</param>
        /// <returns></returns>
        public bool SeesTarget(Vector targetPosition)
        {
            return Game.Instance.GetFirstObject(obstacle => obstacle != this && !(obstacle is Widget) && obstacle.IsBlocking(this.Position, targetPosition)) == null;
        }

        /// <summary>
        /// Näkeekö olio paikkaan.
        /// </summary>
        /// <param name="targetPosition">Paikka</param>
        /// <param name="isObstacle">Ehto sille mikä lasketaan esteeksi</param>
        /// <returns></returns>
        public bool SeesTarget(Vector targetPosition, Predicate<GameObject> isObstacle)
        {
            return Game.Instance.GetFirstObject(obstacle => obstacle != this && isObstacle(obstacle) && obstacle.IsBlocking(this.Position, targetPosition)) == null;
        }

        /// <summary>
        /// Näkeekö olio paikkaan.
        /// </summary>
        /// <param name="targetPosition">Paikka</param>
        /// <param name="obstacleTag">Tagi esteelle</param>
        /// <returns></returns>
        public bool SeesTarget(Vector targetPosition, object obstacleTag)
        {
            return SeesTarget(targetPosition, o => o.Tag == obstacleTag);
        }

        /// <summary>
        /// Onko olio kahden paikan välissä.
        /// </summary>
        /// <param name="obj">Olio</param>
        /// <param name="pos1">Paikka 1</param>
        /// <param name="pos2">Paikka 2</param>
        /// <returns></returns>
        public bool IsBlocking(Vector pos1, Vector pos2)
        {
            Vector normal = (pos2 - pos1).Normalize();
            double ep = this.AbsolutePosition.ScalarProjection(normal);
            double p1p = pos1.ScalarProjection(normal);
            double p2p = pos2.ScalarProjection(normal);

            if (ep < p1p || ep > p2p)
                return false;

            double pn = pos1.ScalarProjection(normal.RightNormal);
            double en = this.AbsolutePosition.ScalarProjection(normal.RightNormal);
            return Math.Abs(en - pn) <= 0.5 * Math.Sqrt(this.Width * this.Width + this.Height * this.Height);
        }

        /// <summary>
        /// Muuttaa olion väriä toiseen hitaasti liukumalla.
        /// </summary>
        /// <param name="targetColor">Väri johon muutetaan</param>
        /// <param name="seconds">Aika jossa muutos valmistuu</param>
        public void FadeColorTo(Color targetColor, double seconds)
        {
            if (fadeTimer != null && fadeTimer.Enabled)
            {
                fadeTimer.Stop();
            }

            double timeLeft = seconds;
            double dt = GetPrecision(seconds);

            int counter = 1;
            Color original = this.Color;
            int rd = targetColor.RedComponent - this.Color.RedComponent;
            int gd = targetColor.GreenComponent - this.Color.GreenComponent;
            int bd = targetColor.BlueComponent - this.Color.BlueComponent;
            int ad = targetColor.AlphaComponent - this.Color.AlphaComponent;

            fadeTimer = new Timer();
            fadeTimer.Interval = dt;
            fadeTimer.Timeout += delegate
            {
                byte r = (byte)(original.RedComponent + rd * counter * dt / seconds);
                byte g = (byte)(original.GreenComponent + gd * counter * dt / seconds);
                byte b = (byte)(original.BlueComponent + bd * counter * dt / seconds);
                byte a = (byte)(original.AlphaComponent + ad * counter * dt / seconds);
                this.Color = new Color(r, g, b, a);
                counter++;
            };
            fadeTimer.Start((int)Math.Ceiling(seconds / dt));
        }

        private double GetPrecision(double seconds)
        {
            double dt = 0.01;
            while (dt > seconds) dt /= 10;
            return dt;
        }
    }
}
