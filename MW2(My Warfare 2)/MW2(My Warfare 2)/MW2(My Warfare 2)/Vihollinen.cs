using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Effects;

/// <summary>
/// Mikä tahansa pelaajalle vihamielinen pelihahmo.
/// </summary>
public class Vihollinen : Elava
{
    /// <summary>
    /// Paljonko vihollinen tekee vahinkoa eläviä kohtaan.
    /// </summary>
    public double TuhovoimaElaviaKohtaan { get; set; }

    /// <summary>
    /// Mitä vihollinen yrittää paikantaa ja tuhota.
    /// </summary>
    public Pelaaja[] Kohteet { get; set; }

    /// <summary>
    /// Miten nopeasti vihollinen liikkuu.
    /// </summary>
    public double LiikkumisNopeus { get; set; }

    private double todennakoisyysSpawnata;

    /// <summary>
    /// Todennäköisyys, miten usein tämä vihollinen spawnaa. Väliltä 0 - 1.
    /// 0 = ei spawnaa koskaan, 1 = spawnaa aina, kun tätä yritetään spawnata. 
    /// </summary>
    public double TodennakoisyysSpawnata { get { return todennakoisyysSpawnata; } set { if (value > 0 && value < 1) todennakoisyysSpawnata = value; } }

    /// <summary>
    /// Onko vihollisella ampuma-asetta.
    /// </summary>
    public bool OnkoAsetta { get; set; }

    /// <summary>
    /// Tuhoaako vihollinen itsensä törmätessään pelaajaan.
    /// </summary>
    public bool OnkoSuicideAttacker { get; set; }

    /// <summary>
    /// Mikä ase vihollisella on. Voi olla null, jolloin vahinkoa syntyy vain kosketuksesta
    /// TuhovoimaElaviaKohtaan-arvon verran.
    /// </summary>
    public Ase ValittuAse { get; set; }

    /// <summary>
    /// Miltä etäisyydeltä vihollinen ampuu, jos sillä on ase.
    /// </summary>
    public double EtaisyysJoltaAmpuu { get; set; }

    /// <summary>
    /// Paljonko aseen tavallista tarkkuutta kasvatetaan tai vähennetään.
    /// 1.0 ei tee mitään, pienempi tarkentaa laukauksia ja suurempi epätarkentaa.
    /// </summary>
    public double AmpumisTarkkuus { get; set; }

    /// <summary>
    /// Peli, jossa vihollinen on.
    /// </summary>
    private PhysicsGame peliJossaOn;

    private Infinite infiniteJossaOn;

    /// <summary>
    /// Ääni hylsyille.
    /// </summary>
    private static readonly SoundEffect hylsynPutoamisAani = MW2_My_Warfare_2_.LoadSoundEffect("gun_shell_drop");

    /// <summary>
    /// Mitä kuvaa käytetään kuolemaefektissä.
    /// </summary>
    public Image KuolemaEfektinPartikkeliKuva { get; set; }


    public FollowerBrain HyokkaysAivot { get; set; }
    public PathFollowerBrain ReittiAivot { get; set; }

    /// <summary>
    /// Luodaan uusi vihollinen.
    /// </summary>
    /// <param name="width">Leveys.</param>
    /// <param name="height">Korkeus.</param>
    /// <param name="kuva">Kuva.</param>
    /// <param name="elamienMaara">Hitpointien määrä, pelaajalla on 20.</param>
    /// <param name="tuhovoimaElaviaKohtaan">Paljonko tekee vahinkoa pelaajaa kohtaan meleehyökkäyksellä. Pelaajalla on 20 hp:tä.</param>
    public Vihollinen(double width, double height, Image kuva, double elamienMaara, double tuhovoimaElaviaKohtaan, double liikkumisNopeus, Pelaaja[] kohteet, bool onkoAsetta, bool onkoSuicideAttacker, PhysicsGame peli, Infinite infinite)
        : base(width, height)
    {
        this.Elamat.Value = elamienMaara;
        this.Elamat.LowerLimit += Kuolema;
        this.TuhovoimaElaviaKohtaan = tuhovoimaElaviaKohtaan;
        this.TodennakoisyysSpawnata = 1.0;
        this.Image = kuva;
        this.ValittuAse = new Ase(30, 10);
        this.Add(this.ValittuAse);
        this.LiikkumisNopeus = liikkumisNopeus;
        this.OnkoAsetta = onkoAsetta;
        this.HyokkaysAivot = LuoHyokkaysAivot(kohteet, liikkumisNopeus);
        this.ReittiAivot = LuoReittiAivot(liikkumisNopeus);
        this.Brain = HyokkaysAivot;
        this.Kohteet = kohteet;
        this.peliJossaOn = peli;
        this.infiniteJossaOn = infinite;
        this.LinearDamping = 0.95;
        this.AngularDamping = 0.95;
        this.OnkoSuicideAttacker = onkoSuicideAttacker;
        //peliJossaOn.AddCollisionHandler(this, "pelaaja", TormaaPelaajaan);
        this.CollisionIgnoreGroup = 5; // se vaan on 5
        this.Mass = 1000;
        this.Tag = "vihollinen";
    }

    /// <summary>
    /// Luodaan vihollinen toisen pohjalta.
    /// </summary>
    /// <param name="kopioitavaVihollinen">Vihollinen, jonka pohjalta kopioidaan uusi.</param>
    public Vihollinen(Vihollinen kopioitavaVihollinen)
        : base(kopioitavaVihollinen.Width, kopioitavaVihollinen.Height)
    {
        this.Elamat.Value = kopioitavaVihollinen.Elamat.Value;
        this.Elamat.LowerLimit += Kuolema;
        this.TuhovoimaElaviaKohtaan = kopioitavaVihollinen.TuhovoimaElaviaKohtaan;
        this.TodennakoisyysSpawnata = kopioitavaVihollinen.TodennakoisyysSpawnata;
        this.Image = kopioitavaVihollinen.Image;
        if (kopioitavaVihollinen.Animation != null)
        {
            this.Animation = kopioitavaVihollinen.Animation;
            this.Animation.Start();
        }
        this.ValittuAse = Aseet.LuoRynkky(); // fixattu, masentavaa
        this.Add(this.ValittuAse);
        this.LiikkumisNopeus = kopioitavaVihollinen.LiikkumisNopeus;
        this.Kohteet = kopioitavaVihollinen.Kohteet;
        this.EtaisyysJoltaAmpuu = kopioitavaVihollinen.EtaisyysJoltaAmpuu;
        this.AmpumisTarkkuus = kopioitavaVihollinen.AmpumisTarkkuus;
        this.OnkoAsetta = kopioitavaVihollinen.OnkoAsetta;
        this.HyokkaysAivot = LuoHyokkaysAivot(kopioitavaVihollinen.Kohteet, kopioitavaVihollinen.LiikkumisNopeus);
        this.ReittiAivot = LuoReittiAivot(kopioitavaVihollinen.LiikkumisNopeus);
        this.Brain = HyokkaysAivot;
        this.peliJossaOn = kopioitavaVihollinen.peliJossaOn;
        this.infiniteJossaOn = kopioitavaVihollinen.infiniteJossaOn;
        this.OnkoSuicideAttacker = kopioitavaVihollinen.OnkoSuicideAttacker;
        this.CollisionIgnoreGroup = kopioitavaVihollinen.CollisionIgnoreGroup;
        //peliJossaOn.AddCollisionHandler(this, "pelaaja", TormaaPelaajaan);
        this.Mass = kopioitavaVihollinen.Mass;
        this.Tag = "vihollinen";
    }

    /// <summary>
    /// Luodaan viholliselle aivot.
    /// </summary>
    /// <param name="kohteet">Mitä kohteita vihollinen seuraa.</param>
    /// <param name="nopeus">Miten nopeasti vihollinen liikkuu.</param>
    /// <returns></returns>
    private FollowerBrain LuoHyokkaysAivot(Pelaaja[] kohteet, double nopeus)
    {
        FollowerBrain seuraamisAivot;
        if (kohteet[1] == null) seuraamisAivot = new FollowerBrain(kohteet[0]);
        else seuraamisAivot = new FollowerBrain(kohteet);
        seuraamisAivot.Speed = nopeus;
        seuraamisAivot.TurnWhileMoving = true;
        if (this.OnkoAsetta)
        {
            seuraamisAivot.DistanceClose = this.EtaisyysJoltaAmpuu;
            seuraamisAivot.TargetClose += delegate { Ammu(this.AmpumisTarkkuus); };
        }
        return seuraamisAivot;
    }

    private PathFollowerBrain LuoReittiAivot(double nopeus)
    {
        PathFollowerBrain reittiAivot = new PathFollowerBrain(nopeus);
        reittiAivot.Loop = true;
        reittiAivot.TurnWhileMoving = true;
        return reittiAivot;
    }

    /// <summary>
    /// Ammutaan vihollisen aseella, jos sellainen on.
    /// </summary>
    /// <param name="tarkkuusKerroin">Paljonko aseen tavallista tarkkuutta kasvatetaan tai vähennetään.
    /// 1.0 ei tee mitään, pienempi tarkentaa laukauksia ja suurempi epätarkentaa.
    /// </param>
    public void Ammu(double tarkkuusKerroin)
    {
        // jos ei ole asetta, ei ammuta
        if (this.ValittuAse == null) return;
        if (this.ValittuAse.Ammo.Value == 0) Timer.SingleShot(this.ValittuAse.LataukseenKuluvaAika, delegate
        {
            this.ValittuAse.Ammo.Value = this.ValittuAse.Ammo.MaxValue;
        });
        PhysicsObject ammus = this.ValittuAse.Shoot();
        if (ammus == null) return; // jos jostain syystä ei voida ampua, keskeytetään

        // lisätään epätarkkuutta
        Vector random = RandomGen.NextVector(this.ValittuAse.AseenHajoama.X * tarkkuusKerroin, this.ValittuAse.AseenHajoama.Y * tarkkuusKerroin);
        ammus.Hit(random);

        ammus.CollisionIgnoreGroup = 4;
        ammus.Tag = "ammus";
        ammus.LinearDamping = 0.99;
        ammus.Size = new Vector(this.ValittuAse.luodinKuva.Width, this.ValittuAse.luodinKuva.Height);
        ammus.Image = this.ValittuAse.luodinKuva;
        peliJossaOn.AddCollisionHandler<PhysicsObject, Tuhoutuva>(
            ammus, delegate(PhysicsObject a, Tuhoutuva tuhoutuva)
            {
                a.Destroy();
                tuhoutuva.Kesto.Value -= this.ValittuAse.TuhovoimaTuhoutuviaVastaan;
            });

        peliJossaOn.AddCollisionHandler<PhysicsObject, Pelaaja>(
            ammus, delegate(PhysicsObject a, Pelaaja pelaaja)
            {
                a.Destroy();
                MW2_My_Warfare_2_.osuma1.Play();
                pelaaja.Elamat.Value -= this.ValittuAse.TuhovoimaElaviaVastaan / 1.5;
            });
        peliJossaOn.Add(ammus);
        //if (this.ValittuAse.TuleekoHylsya)
        //    Hylsy(this, this.ValittuAse.hylsynKuva);
    }

    /// <summary>
    /// Luo hylsyn vihollisen ampuessa.
    /// </summary>
    /// <param name="pelaaja">Vihollinen, jonka aseesta hylsy lentää.</param>
    void Hylsy(Vihollinen vihu, Image hylsynkuva)
    {
        PhysicsObject hylsy = new PhysicsObject(2, 5);
        hylsy.Image = hylsynkuva;
        hylsy.Position = this.Position + Vector.FromLengthAndAngle(15, this.Angle + Angle.FromDegrees(-90));
        hylsy.LinearDamping = 0.93;
        hylsy.Mass = 0.5;
        hylsy.Angle = this.Angle + Angle.FromDegrees(90);
        hylsy.AngularVelocity = RandomGen.NextDouble(-20.0, 20.0);
        hylsy.AngularDamping = 0.97;
        hylsy.MaximumLifetime = TimeSpan.FromSeconds(3.0);
        hylsy.Hit(Vector.FromLengthAndAngle(RandomGen.NextInt(150, 300), this.Angle + Angle.FromDegrees(RandomGen.NextDouble(-95, -85))));
        peliJossaOn.Add(hylsy);
        Timer.SingleShot(0.5, delegate { hylsynPutoamisAani.Play(0.7, 0.0, 0.0); });
    }

    /// <summary>
    /// Tuhotaan vihollinen Elamien loputtua.
    /// </summary>
    private void Kuolema()
    {
        if (infiniteJossaOn != null)
            infiniteJossaOn.VihollisetKentalla.Remove(this);
        Blood.AddDeathSplatter(this.Position, 3, 0.4);
        this.Destroy();
    }

    /// <summary>
    /// Kutsutaan, kun vihu törmää pelaajaan.
    /// </summary>
    /// <param name="tormaaja">Törmäävä vihollinen.</param>
    /// <param name="kohde">Pelaaja, johon törmätään.</param>
    private void TormaaPelaajaan(PhysicsObject tormaaja, PhysicsObject pelaaja)
    {
        Pelaaja p = pelaaja as Pelaaja;
        p.Elamat.Value -= this.TuhovoimaElaviaKohtaan;
        if (this.OnkoSuicideAttacker) // tuhoutuuko hyökätessään
        {
            KuolemaEfekti();
            Kuolema();
        }
    }

    private void KuolemaEfekti()
    {
        if (this.KuolemaEfektinPartikkeliKuva == null) return; // ettei tule NullPointeria

        ExplosionSystem kuolemaefekti = new ExplosionSystem(this.KuolemaEfektinPartikkeliKuva, 100);
        kuolemaefekti.Position = this.Position;
        peliJossaOn.Add(kuolemaefekti);
        kuolemaefekti.AddEffect(this.Position, 50);
    }
}