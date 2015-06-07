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
            if (OnFire) return;
            Flame f = Partikkelit.CreateFlames(this.Position, 100, 0.3);
            this.Destroyed += delegate { Ext(f); };
#if DEBUG
            MW2_My_Warfare_2_.Peli.MessageDisplay.Add("[DEBUG] Added fire " + f.GetHashCode().ToString());
#endif
            NeedsUpdateCall = true;
            Updated += delegate { if (f != null) f.Position = this.Position; };
            Extinguished += delegate(PhysicsObject p2)
            {
                Ext(f);
            };
            BurnUpdate += delegate { MW2_My_Warfare_2_.Peli.Damagea(this, Vakiot.TULEN_DAMAGE_ELAVIA_VASTAAN, false); };
        };
    }

    private void Ext(Flame f)
    {
        f.FadeOut(2);
        Timer.SingleShot(2, delegate
        {
            f.Destroy();
#if DEBUG
            MW2_My_Warfare_2_.Peli.MessageDisplay.Add("[DEBUG] Removed fire " + f.GetHashCode().ToString());
#endif
            NeedsUpdateCall = false;
            ResetFireSystem();
        });
    }
}