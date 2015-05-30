using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    internal class Force
    {
        /// <summary>
        /// Voiman suunta ja suuruus.
        /// </summary>
        public Vector Value { get; set; }

        /// <summary>
        /// Voiman luomisaika.
        /// </summary>
        public TimeSpan CreationTime { get; private set; }

        /// <summary>
        /// Elinaika. Lasketaan siitä lähtien, kun voima luodaan.
        /// </summary>
        public TimeSpan Lifetime
        {
            get { return Game.Time.SinceStartOfGame - CreationTime; }
        }

        /// <summary>
        /// Suurin mahdollinen elinaika.
        /// Kun <c>Lifetime</c> on suurempi kuin tämä, voima lopettaa vaikuttamasta.
        /// </summary>
        public TimeSpan MaximumLifetime { get; set; }

        public Force( Vector f )
        {
            Value = f;
            CreationTime = Game.Time.SinceStartOfGame;
            MaximumLifetime = TimeSpan.MaxValue;
        }

        public Force( Vector f, TimeSpan t )
            : this( f )
        {
            MaximumLifetime = CreationTime + t;
        }

        /// <summary>
        /// Lopettaa voiman vaikutuksen.
        /// </summary>
        public virtual void Destroy()
        {
            this.MaximumLifetime = TimeSpan.Zero;
        }

        public bool IsDestroyed()
        {
            return Lifetime > MaximumLifetime;
        }
    }
}
