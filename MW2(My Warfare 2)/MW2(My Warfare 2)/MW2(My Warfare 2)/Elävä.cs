using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// <summary>
/// Kaikkien kuolevaisten olioiden yhteiset ominaisuudet, kuten elämät. Abstrakti luokka.
/// </summary>
public abstract class Elava : PhysicsObject
{
    public DoubleMeter Elamat { get; set; }

    public event Action Kuoli;

    public Elava(double width, double height)
        : this(width, height, 20)
    {
    }

    public Elava(double width, double height, double elamat)
        : base(width, height)
    {
        this.Elamat = new DoubleMeter(elamat);
        this.Elamat.MinValue = 0;
        this.Elamat.LowerLimit += delegate
        {
            if (OnFire) Extinquish();
        };
        this.Elamat.LowerLimit += this.Kuoli;

        Ignited += delegate(PhysicsObject p)
        {
            Flame f = Partikkelit.CreateFlames(this.Position, 100, 0.3);
            NeedsUpdateCall = true;
            Updated += delegate { if (f != null) f.Position = this.Position; };
            Extinguished += delegate(PhysicsObject p2)
            {
                f.FadeOut(2);
                Timer.SingleShot(2, delegate {
                    f.Destroy();
                    NeedsUpdateCall = false;
                    ResetFireSystem();
                });
            };
            BurnUpdate += delegate { MW2_My_Warfare_2_.Peli.Damagea(this, Vakiot.TULEN_DAMAGE_ELAVIA_VASTAAN, false); };
        };
    }
}