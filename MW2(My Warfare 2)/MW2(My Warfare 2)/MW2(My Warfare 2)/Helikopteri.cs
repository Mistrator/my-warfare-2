using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Widgets;
using Jypeli.Effects;
using Jypeli.Assets;


/// <summary>
/// Kopteri, joka nousee, laskeutuu, lentää paikasta toiseen ja voi tuhoutua.
/// </summary>
public class Helikopteri : Elava
{
    /// <summary>
    /// Kopteri, josta puuttuu lavat.
    /// </summary>
    Image kopteriIlmanLapojaKuva = MW2_My_Warfare_2_.LoadImage("kopteri_ilmanlapoja");

    /// <summary>
    /// Irtonaiset kopterin lavat.
    /// </summary>
    Image kopterinLavatKuva = MW2_My_Warfare_2_.LoadImage("kopteri_lavat");

    /// <summary>
    /// Kopterin lentoanimaatio.
    /// </summary>
    Image[] kopteriIlmassa = MW2_My_Warfare_2_.LoadImages("kopteri_ilmassa1", "kopteri_ilmassa2", "kopteri_ilmassa3", "kopteri_ilmassa4");

    public bool OnkoIlmassa { get; private set; }

    private PhysicsObject KopterinLavat { get; set; }

    private Timer NousuAjastin;
    private Timer KasvuAjastin;
    private Timer PienennysAjastin;

    public ProgressBar ElamaMittari { get; private set; }

    private PathFollowerBrain ReittiAivot { get; set; }

    private const double LAPOJEN_MAX_PYORIMISNOPEUS = 20;
    private const double KOPTERIN_KOON_MUUTOS_AIKA = 6;
    private const double KOPTERIN_MINIMI_LEVEYS = 330;

    public Helikopteri(double width, double height, double kesto, bool aloittaakoIlmasta)
        : base(width, height, kesto)
    {
        KopterinLavat = new PhysicsObject(kopterinLavatKuva);
        KopterinLavat.IgnoresCollisionResponse = true;
        KopterinLavat.Position = new Vector(this.Position.X, this.Size.Y / 4);
        KopterinLavat.AngularDamping = 1.00;
        KopterinLavat.Shape = Shape.FromImage(kopterinLavatKuva);
        MW2_My_Warfare_2_.Peli.Add(KopterinLavat, 2);
        this.Mass = 10000;
        this.LinearDamping = 0.90;
        this.AngularDamping = 0.90;
        //this.MakeStatic();
        this.Image = kopteriIlmanLapojaKuva;
        //this.Add(KopterinLavat);
        this.OnkoIlmassa = false;
        NousuAjastin = new Timer();
        KasvuAjastin = new Timer();
        PienennysAjastin = new Timer();

        Elamat = new DoubleMeter(50.0);
        Elamat.MinValue = 0;
        Elamat.MaxValue = 50;
        Elamat.LowerLimit += delegate
        {
            KopteriTuhoutui();
        };

        ElamaMittari = new ProgressBar(150, 10);
        ElamaMittari.BindTo(this.Elamat);
        ElamaMittari.BarColor = Color.Blue;
        ElamaMittari.BorderColor = Color.White;
        ElamaMittari.Angle += Angle.FromDegrees(90);
        //Add(ElamaMittari);

        IsUpdated = true;

        if (aloittaakoIlmasta)
        {
            this.KopterinLavat.AngularVelocity = 20;
            this.OnkoIlmassa = true;
            for (int i = 0; i < 120; i++)
            {
                this.Size *= 1.005;
                this.KopterinLavat.Size *= 1.005;
                this.IgnoresCollisionResponse = true;
            }
        }
        ReittiAivot = new PathFollowerBrain();
        this.Brain = ReittiAivot;
        this.Destroyed += delegate { KopterinLavat.Destroy(); };
    }

    /// <summary>
    /// Nostetaan kopteri ilmaan.
    /// </summary>
    public void NouseIlmaan()
    {
        this.KopterinLavat.AngularDamping = 1.0;
        NousuAjastin.Interval = 1.0;
        NousuAjastin.Timeout += delegate
        {
            this.KopterinLavat.AngularVelocity = NousuAjastin.SecondCounter.Value;

            if (this.KopterinLavat.AngularVelocity >= LAPOJEN_MAX_PYORIMISNOPEUS)
            {
                NousuAjastin.Stop();
                NousuAjastin.Reset();
                NostaKopteria();
                return;
            }
        };
        NousuAjastin.Start();
    }

    private void NostaKopteria()
    {
        KasvuAjastin.Interval = 0.05;
        KasvuAjastin.Start();
        KasvuAjastin.Timeout += delegate
        {
            if (KasvuAjastin.SecondCounter.Value >= KOPTERIN_KOON_MUUTOS_AIKA)
            {
                KasvuAjastin.Stop();
                KasvuAjastin.Reset();
                this.IgnoresCollisionResponse = true;
                this.OnkoIlmassa = true;
                return;
            }

            this.Size *= 1.005;
            this.KopterinLavat.Size *= 1.005;
        };

    }

    private void LaskeKopteria()
    {
        PienennysAjastin.Interval = 0.05;
        PienennysAjastin.Start();
        PienennysAjastin.Timeout += delegate
        {
            if (this.Size.X <= KOPTERIN_MINIMI_LEVEYS)
            {
                PienennysAjastin.Stop();
                PienennysAjastin.Reset();
                ViimeisteleLasku();
                return;
            }
            this.Size /= 1.005;
            this.KopterinLavat.Size /= 1.005;
        };
    }

    public void Laskeudu()
    {
        LaskeKopteria();
    }

    private void ViimeisteleLasku()
    {
        this.OnkoIlmassa = false;
        this.IgnoresCollisionResponse = false;
        this.KopterinLavat.AngularDamping = 0.95;
    }

    private void KopteriTuhoutui()
    {
        Partikkelit.AddExplosionEffect(this.Position, 500);
        this.Destroy();
    }

    public override void Update(Time time)
    {
        //this.ElamaMittari.Position = new Vector(this.X, this.Y + this.Size.Y / 2 + 30);
        //KopterinLavat.Position = new Vector(this.Position.X, this.Size.Y / 4.3);
        KopterinLavat.Position = this.Position;
        if (OnkoIlmassa)
            this.KopterinLavat.AngularVelocity = LAPOJEN_MAX_PYORIMISNOPEUS;

        base.Update(time);
    }

    /// <summary>
    /// Liikutetaan kopteria aloittaen sen nykyisestä paikasta. Ei voi liikuttaa, jos kopteri ei ole ilmassa.
    /// </summary>
    /// <param name="reitti">Reitti, jota kopteri seuraa.</param>
    public void LiikutaKopteria(double nopeus, bool laskeutuukoPerilla, params Vector[] reitti)
    {
        if (!OnkoIlmassa) return;

        ReittiAivot.Speed = nopeus;
        ReittiAivot.Loop = false;
        ReittiAivot.TurnWhileMoving = true;
        ReittiAivot.Path = reitti;

        ReittiAivot.ArrivedAtEnd += delegate
        {
            ReittiAivot.Active = false;
            if (laskeutuukoPerilla)
                this.Laskeudu();
        };

    }
}