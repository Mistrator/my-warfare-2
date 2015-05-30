using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Effects;

namespace Jypeli
{
    public partial class PhysicsObject
    {
        /// <summary>
        /// Onko olio tulessa.
        /// </summary>
        public bool OnFire { get; protected set; }

        /// <summary>
        /// Voiko olio palaa.
        /// </summary>
        public bool CanBurn { get; set; }

        /// <summary>
        /// Olion tulenkestävyys. Jos menee alle nollan, syttyy palamaan.
        /// Yrittäessä syttyä palamaan vähennetään.
        /// </summary>
        public IntMeter IgnitionHP { get; set; }

        /// <summary>
        /// Olion paloaika, vähenee palamisen aikana. Jos menee alle nollan, olio sammuu.
        /// </summary>
        public IntMeter BurningHP { get; set; }

        /// <summary>
        /// Palamisnopeus.
        /// </summary>
        private const int BURNING_SPEED = 1;

        public delegate void FireEvent(PhysicsObject obj);

        public event FireEvent Ignited;
        public event FireEvent Extinguished;
        public event FireEvent BurnUpdate;

        /// <summary>
        /// Sytyttää olion palamaan.
        /// </summary>
        public virtual void Ignite()
        {
            if (OnFire) return;
            if (!CanBurn) return;
            if (BurningHP.Value <= 0) return;

            OnFire = true;

            if (Ignited != null)
                Ignited(this);
        }

        /// <summary>
        /// Sammuttaa olion.
        /// </summary>
        public virtual void Extinquish()
        {
            if (!OnFire) return;
            OnFire = false;
            if (Extinguished != null)
                Extinguished(this);
        }

        /// <summary>
        /// Kutsutaan palamisen aikana jatkuvasti, päivittää tilannetta.
        /// </summary>
        public virtual void Burn()
        {
            if (!CanBurn) return;
            if (!OnFire) return;

            BurningHP.Value -= BURNING_SPEED;
            if (BurnUpdate != null)
                BurnUpdate(this);
        }

        public virtual void ResetFireSystem()
        {
            if (IgnitionHP != null)
                IgnitionHP.Value = 50;
            if (BurningHP != null)
                BurningHP.Value = 75;
            OnFire = false;
        }

        private void InitializeFireSystem()
        {
            IgnitionHP = new IntMeter(50);
            IgnitionHP.MinValue = 0;
            IgnitionHP.LowerLimit += Ignite;

            BurningHP = new IntMeter(75);
            BurningHP.MinValue = 0;
            BurningHP.LowerLimit += Extinquish;

            CanBurn = true;
        }
    }
}
