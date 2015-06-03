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

    public FollowerBrain SeuraamisAivot;
    public PathFollowerBrain ReittiAivot;

    public bool HasRoute = false;

    public Pelaaja NextTarget;

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
        this.SeuraamisAivot = LuoHyokkaysAivot(liikkumisNopeus);
        this.ReittiAivot = LuoReittiAivot(liikkumisNopeus);
        this.CreateShard += Shard;

        if (MW2_My_Warfare_2_.Peli.KentanOsat == null)  
            this.PaivitaTekoaly();
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
        this.peliJossaOn = kopioitavaVihollinen.peliJossaOn;
        this.infiniteJossaOn = kopioitavaVihollinen.infiniteJossaOn;
        this.OnkoSuicideAttacker = kopioitavaVihollinen.OnkoSuicideAttacker;
        this.CollisionIgnoreGroup = kopioitavaVihollinen.CollisionIgnoreGroup;
        //peliJossaOn.AddCollisionHandler(this, "pelaaja", TormaaPelaajaan);
        this.Mass = kopioitavaVihollinen.Mass;
        this.Tag = "vihollinen";
        this.SeuraamisAivot = LuoHyokkaysAivot(kopioitavaVihollinen.LiikkumisNopeus);
        this.ReittiAivot = LuoReittiAivot(kopioitavaVihollinen.LiikkumisNopeus);
        this.CreateShard += Shard;

        if (MW2_My_Warfare_2_.Peli.KentanOsat == null)
            this.PaivitaTekoaly();
    }

    #region tekoaly
    /// <summary>
    /// Luodaan viholliselle aivot.
    /// </summary>
    /// <param name="kohteet">Mitä kohteita vihollinen seuraa.</param>
    /// <param name="nopeus">Miten nopeasti vihollinen liikkuu.</param>
    /// <returns></returns>
    private FollowerBrain LuoHyokkaysAivot(double nopeus)
    {
        FollowerBrain seuraamisAivot = new FollowerBrain();
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
        reittiAivot.TurnWhileMoving = true;
        return reittiAivot;
    }

    public void PaivitaTekoaly()
    {
        /* Jos näkyvissä on pelaaja/pelaajia, vihollinen seuraa sitä (lähintä).
         * Jos ei, lasketaan reitti lähimpään Dijkstralla.
         * Kun reitti on laskettu, tarkistetaan usein, näkyykö pelaajia tai onko reitti vanhentunut (pelaajat liian kaukana päätepisteestä).
         * -> Jos reitti on vanhentunut, lasketaan uusi.
         * -> Jos pelaaja näkyy, vihollinen lähtee seuraamaan sitä.
         */
        if (Kohteet == null || Kohteet.Length == 0) return;
        if (double.IsNaN(this.Position.X)) return; // karua mutta joskus tuli nan

        List<Pelaaja> targets = new List<Pelaaja>();
        for (int i = 0; i < Kohteet.Length; i++)
        {
            if (Kohteet[i] != null && Kohteet[i].IsAddedToGame)
                targets.Add(Kohteet[i]);
        }

        if (targets.Count == 0) return;

        // jos ei ole navmeshiä, käytetään followerbrainia
        if (MW2_My_Warfare_2_.Peli.KentanOsat == null)
        {
            ActivateFollowerBrain(targets.ToArray());
            return;
        }


        Tuple<Pelaaja, bool> closestData = EtsiLahinNakyvaPelaaja(this.Position, targets);
        if (closestData.Item2) // oli näkyvissä oleva pelaaja
        {
            ActivateFollowerBrain(closestData.Item1);
            return;
        }
        /* Tutkitaan, miten kaukana pelaajat ovat tämänhetkisen reitin päätepisteestä. Jos matkaa on liikaa, lasketaan uusi reitti.
         * Uusi reitti lasketaan kuitenkin vihollista lähimpään pelaajaan, ei päätepistettä lähimpään.
         */

        if (this.HasRoute)
        {
            Pelaaja lahin = EtsiLahinPelaaja(this.ReittiAivot.Path[this.ReittiAivot.Path.Count - 1], targets);
            double d = Vector.Distance(this.ReittiAivot.Path[this.ReittiAivot.Path.Count - 1], lahin.Position);

            if (d > Vakiot.DISTANCE_TO_REFRESH_ROUTE)
            {
                this.HasRoute = false;
            }
            else return;
        }

        // lisätään vihollinen jonoon reitin laskentaa varten
        NextTarget = closestData.Item1;
        infiniteJossaOn.aiUpdater.EnqueueForPathfindingUpdate(this);
    }

    /// <summary>
    /// Lasketaan Dijkstralla reitti kohteeseen ja liikutaan sinne.
    /// Kohde on NextTarget.
    /// </summary>
    public void FindAndUseRoute()
    {
        List<Vector> reitti = new List<Vector>();
        try
        {
            reitti = MW2_My_Warfare_2_.Peli.KentanOsat.GetRouteCoordinates(
                        MW2_My_Warfare_2_.Peli.KentanOsat.DijkstraSearch(
                        MW2_My_Warfare_2_.Peli.KentanOsat.GetCorrespondingNode(this.Position),
                        MW2_My_Warfare_2_.Peli.KentanOsat.GetCorrespondingNode(NextTarget.Position)));
        }
        catch (ArgumentOutOfRangeException)
        {
            // tähän tullaan jos reitti menee navmeshin ulkopuolelle
            // laitetaan followerbrain päälle jos ei saada reittiä laskettua
            ActivateFollowerBrain(NextTarget);
            return;
        }
        ActivatePathfindingBrain(reitti);
        NextTarget = null;
    }

    void ActivateFollowerBrain(Pelaaja target)
    {
        this.ReittiAivot.Active = false;
        this.SeuraamisAivot.Active = true;
        this.Brain = SeuraamisAivot;
        this.SeuraamisAivot.ObjectsToFollow.Clear();
        this.SeuraamisAivot.ObjectsToFollow.Add(target);

        this.HasRoute = false; // etsitään dijkstralla uusi reitti otettaessa se käyttöön
    }

    void ActivateFollowerBrain(Pelaaja[] targets)
    {
        this.ReittiAivot.Active = false;
        this.SeuraamisAivot.Active = true;
        this.Brain = SeuraamisAivot;
        this.SeuraamisAivot.ObjectsToFollow.Clear();
        this.SeuraamisAivot.ObjectsToFollow.AddRange(targets);

        this.HasRoute = false; // etsitään dijkstralla uusi reitti otettaessa se käyttöön
    }


    void ActivatePathfindingBrain(List<Vector> route)
    {
        this.SeuraamisAivot.Active = false;
        this.ReittiAivot.Active = true;
        this.Brain = ReittiAivot;
        this.ReittiAivot.Path = route;
        this.HasRoute = true;
        this.ReittiAivot.ArrivedAtEnd += delegate { this.HasRoute = false; };

#if DEBUG
        for (int i = 0; i < route.Count; i++)
        {
            GameObject marker = new GameObject(10, 10);
            marker.Color = Color.Red;
            marker.Position = route[i];
            marker.MaximumLifetime = TimeSpan.FromSeconds(3.0);
            MW2_My_Warfare_2_.Peli.Add(marker);
        }
#endif

    }


    /// <summary>
    /// Etsii lähimmän näkyvän pelaajan. Jos ei ole näkyviä, palauttaa lähimmän piilossa olevan pelaajan.
    /// </summary>
    /// <returns>Pelaaja-lähin pelaaja, bool-oliko pelaaja näkyvissä</returns>
    Tuple<Pelaaja, bool> EtsiLahinNakyvaPelaaja(Vector observerPosition, List<Pelaaja> targets)
    {
        double seenDistance = double.MaxValue;
        double generalDistance = double.MaxValue;
        int closestSeen = -1;
        int closestGeneral = -1;

        for (int i = 0; i < targets.Count; i++)
        {
            double currentDistance = Vector.Distance(targets[i].Position, observerPosition);
            if (currentDistance < generalDistance)
            {
                generalDistance = currentDistance;
                closestGeneral = i;
            }

            if (SeesObject(targets[i], x => x is PhysicsObject && ((PhysicsObject)x).IsStatic)) // ainoastaan staattiset objektit estävät kulkemisen
            {
                if (currentDistance < seenDistance)
                {
                    seenDistance = currentDistance;
                    closestSeen = i;
                }
            }
        }
        if (closestSeen != -1)
        {
            return new Tuple<Pelaaja, bool>(targets[closestSeen], true);
        }
        else return new Tuple<Pelaaja, bool>(targets[closestGeneral], false);
    }

    /// <summary>
    /// Etsii pistettä lähimmän pelaajan.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="targets"></param>
    /// <returns></returns>
    Pelaaja EtsiLahinPelaaja(Vector position, List<Pelaaja> targets)
    {
        if (targets == null || targets.Count == 0) return null;

        double distance = double.MaxValue;
        int index = -1;

        for (int i = 0; i < targets.Count; i++)
        {
            double d = Vector.Distance(position, targets[i].Position);
            if (d < distance)
            {
                distance = d;
                index = i;
            }
        }
        return targets[index];
    }

    #endregion

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
        this.Shatter(Vakiot.SHATTER_SIZE, Vector.Zero);
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

    public void HajotaPaloiksi()
    {
        if (this.IsShattered) return;

        Shatter(Vakiot.SHATTER_SIZE, Vector.Zero);
    }

    public PhysicsObject Shard(double width, double height, Vector position)
    {
        PhysicsObject shard = new PhysicsObject(width, height);
        shard.Position = position;
        MW2_My_Warfare_2_.Peli.Efektit.LisaaAjastettuSirpale(shard, Vakiot.ENEMY_PART_LIFETIME);

        shard.LinearDamping = RandomGen.NextDouble(0.80, 0.90);
        shard.AngularDamping = 0.90;
        shard.CanBurn = false;
        shard.Mass = 1;
        shard.Hit(RandomGen.NextVector(100.0, 500.0));

        return shard;

    }
}