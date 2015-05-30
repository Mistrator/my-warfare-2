using System;
using Microsoft.Xna.Framework;

namespace Jypeli.Effects
{
    /// <summary>
    /// Järjestelmä liekeille
    /// </summary>
    public class Flame : ParticleSystem
    {
        private double addTime;

        public bool IsDirectional;

        public Angle FlameDirection;

        /// <summary>
        /// Luo uuden liekin.
        /// </summary>
        /// <param name="image">Partikkelin kuva.</param>
        public Flame(Image image)
            : base(image, 100)
        {
            Angle = Angle.FromDegrees(90);
            BlendMode = BlendMode.Additive;
        }

        /// <summary>
        /// Määritetään oletusarvot efektille
        /// </summary>
        protected override void InitializeParticles()
        {
            MinLifetime = 0.8;
            MaxLifetime = 1.0;

            MinScale = 120; //70
            MaxScale = 180; // 100

            ScaleAmount = -2.0;
            AlphaAmount = 0.5;

            //MinVelocity = 40;
            //MaxVelocity = 70;
            MinVelocity = 5;
            MaxVelocity = 40;

            MinAcceleration = 1;
            MaxAcceleration = 2;

            MinRotationSpeed = -MathHelper.PiOver4;
            MaxRotationSpeed = MathHelper.PiOver4;
        }

        /// <summary>
        /// Lasketaan liekin suunnalle satunnaisuutta
        /// </summary>
        /// <returns>Partikkelin suunta</returns>
        protected override Vector GiveRandomDirection()
        {
            //return Vector.FromLengthAndAngle(1, Angle + Angle.FromDegrees(RandomGen.NextDouble(-5, 5)));
            if (!IsDirectional)
                return Vector.FromAngle(RandomGen.NextAngle());
            else 
                return Vector.FromLengthAndAngle(1, FlameDirection + Angle.FromDegrees(RandomGen.NextDouble(-5, 5)));
        }

        /// <summary>
        /// Päivitetään liekkiä
        /// </summary>
        /// <param name="time"></param>
        public override void Update(Time time)
        {
            double t = time.SinceLastUpdate.TotalSeconds;
            addTime += t;
            if (addTime > 0.05)
            {
                base.AddEffect(Position, 2);
                addTime = 0;
            }
            base.Update(time);
        }

        /// <summary>
        /// Alustetaan partikkeli
        /// </summary>
        /// <param name="p">Partikkeli joka alustetaan</param>
        /// <param name="position">Sijainti johon alustetaan</param>
        protected override void InitializeParticle(Particle p, Vector position)
        {
            base.InitializeParticle(p, position);
            if (!IgnoreWind)
            p.Acceleration = Game.Wind;
        }
    }
}
