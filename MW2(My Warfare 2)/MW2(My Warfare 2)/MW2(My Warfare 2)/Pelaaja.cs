using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// <summary>
/// Pelaajan ohjaama pelihahmo.
/// </summary>
public class Pelaaja : Elava
{
    private List<Ase> aseet;
    /// <summary>
    /// Pelaajalla olevat aseet.
    /// </summary>
    public List<Ase> Aseet { get { return aseet; } }

    private Ase valittuAse = null;
    /// <summary>
    /// Pelaajan tällä hetkellä valittu ase.
    /// </summary>
    public Ase ValittuAse { get { return valittuAse; } }

    /// <summary>
    /// Pelaajan aseiden määrä.
    /// </summary>
    public int AseidenMaara { get { return aseet.Count; } }

    /// <summary>
    /// Ei voi ampua, eikä vaihtaa asetta, jos true.
    /// </summary>
    public bool LadataankoAsetta { get; set; }

    /// <summary>
    /// Pelaajan kranaattien määrä.
    /// </summary>
    public int KranaattienMaara { get; set; }

    /// <summary>
    /// Onko pelaajalla loputtomat kranaatit.
    /// </summary>
    public bool LoputtomatKranaatit { get; set; }

    /// <summary>
    /// Pelaajan numero.
    /// </summary>
    public int Numero { get; set; }

    /// <summary>
    /// Survivalissa käytettävien seinien määrä.
    /// </summary>
    public int SeinienMaara { get; set; }

    /// <summary>
    /// Montako seinää pelaajalla on Survivalissa.
    /// </summary>
    public Label SeinienMaaraNaytto { get; set; }

    public bool kaytetaankoLasertahtainta = false;
    public bool kaytetaankoPalloTahtainta = true;
    public PhysicsObject tahtain;
    public GameObject Lasertahtain;
    public GameObject Taskulamppu;

    public ProgressBar elamaPalkki;
    public ProgressBar kuntoPalkki;

    /// <summary>
    /// Näyttö, joka kertoo, että pelaaja on esim. poistunut kentältä.
    /// </summary>
    public Label infoRuutu;

    /// <summary>
    /// Näyttö, joka näyttää, mikä ase on valittuna.
    /// </summary>
    public Label ValittuAseNaytto { get; set; }

    /// <summary>
    /// Näyttö, joka näyttää valitun aseen jäljellä olevien ammusten määrän.
    /// </summary>
    public Label ammusMaaraNaytto;

    /// <summary>
    /// Näyttö, joka näyttää pelaajan tappojen määrän.
    /// </summary>
    public Label TappojenMaaraNaytto { get; set; }

    public bool IsShooting { get; set; }

    private Timer elamanPalautin;

    private Timer kentaltaPoistumisAjastin;

    /// <summary>
    /// Kauanko pelaaja on ollut poissa kentältä.
    /// </summary>
    public DoubleMeter aikaPoissaKentalta;

    /// <summary>
    /// Onko pelaaja poissa kentältä.
    /// </summary>
    public bool OllaankoPoissaKentalta { get; protected set; }

    /// <summary>
    /// Pelaajan kuolemien määrä, lisääntyy yhdellä pelaajan kuollessa.
    /// </summary>
    public IntMeter Kuolemat { get; set; }

    /// <summary>
    /// Pelaajan tappojen määrä, lisääntyy yhdellä toisen pelaajan kuollessa.
    /// </summary>
    public IntMeter Tapot { get; set; }

    /// <summary>
    /// Mihin paikkoihin pelaaja voi spawnata.
    /// </summary>
    public List<Vector> SpawnausPaikat { get; set; }

    /// <summary>
    /// Missä pelaaja kuoli viimeksi, käytetään spawnkillien estämisessä.
    /// </summary>
    private Vector KuolemaPaikka { get; set; }

    public Color objektienVari;

    /// <summary>
    /// Pelaajan nopeus, oletuksena 300.0.
    /// </summary>
    public double Nopeus { get; set; }

    /// <summary>
    /// Riittäkö kunto juoksuun.
    /// </summary>
    public bool VoikoJuosta { get; private set; }

    /// <summary>
    /// Riittäkö kunto nopeaan kävelyyn.
    /// </summary>
    public bool VoikoKavella { get; private set; }

    /// <summary>
    /// Juokseeko pelaaja.
    /// </summary>
    public bool Juokseeko { get; private set; }

    /// <summary>
    /// Pelaajan kunto, eli kauanko voi juosta.
    /// </summary>
    public DoubleMeter Vasymys { get; set; }

    /// <summary>
    /// Käytetään väsymyksen hallintaan.
    /// </summary>
    private Timer VasymysAjastin;

    /// <summary>
    /// Kutsutaan, kun pelaaja kuolee.
    /// </summary>
    public new event Action Kuoli;

    public delegate void PlayerHandler(Pelaaja pelaaja);
    public event PlayerHandler Voitti;
    //public PlayerHandler StoppedShooting;

    /// <summary>
    /// Alustetaan uusi pelaaja ja kaikki pelaajalle kuuluvat asiat.
    /// </summary>
    /// <param name="width">Pelaajan leveys.</param>
    /// <param name="height">Pelaajan korkeus.</param>
    public Pelaaja(double width, double height, bool kaytetaankoLaseria, bool kaytetaankoTaskuLamppua)
        : base(width, height)
    {
        aseet = new List<Ase>();
        SpawnausPaikat = new List<Vector>();
        KranaattienMaara = Vakiot.PELAAJAN_KRANAATTIEN_OLETUSMAARA;
        LoputtomatKranaatit = false;

        Elamat = new DoubleMeter(Vakiot.PELAAJIEN_ELAMIEN_MAARA);
        Elamat.MaxValue = Vakiot.PELAAJIEN_ELAMIEN_MAARA;
        Elamat.LowerLimit += Kuolema;

        Vasymys = new DoubleMeter(100, 0, 100);
        Vasymys.AddTrigger(30, TriggerDirection.Up, delegate() { this.VoikoJuosta = true; });
        Vasymys.AddTrigger(25, TriggerDirection.Down, delegate() { this.VoikoJuosta = false; this.Nopeus = 300.0; });
        Vasymys.AddTrigger(10, TriggerDirection.Down, delegate() { this.Nopeus = 50.0; this.VoikoKavella = false; });
        Vasymys.AddTrigger(15, TriggerDirection.Up, delegate() { this.Nopeus = 300.0; this.VoikoKavella = true; });

        VasymysAjastin = new Timer();
        VasymysAjastin.Interval = 0.1;
        VasymysAjastin.Timeout += delegate { PaivitaKunnonTila(); };
        VasymysAjastin.Start();

        VoikoJuosta = true;
        VoikoKavella = true;

        elamaPalkki = new ProgressBar(75, 5); //75, 10
        elamaPalkki.BorderColor = Color.White;
        elamaPalkki.BindTo(this.Elamat);
        elamaPalkki.Tag = "HUD";
        Game.Add(elamaPalkki, 0);

        kuntoPalkki = new ProgressBar(75, 5);
        kuntoPalkki.BorderColor = Color.White;
        kuntoPalkki.BarColor = Color.Lighter(Color.Blue, 40);
        kuntoPalkki.BindTo(this.Vasymys);
        kuntoPalkki.Tag = "HUD";
        Game.Add(kuntoPalkki, 0);

        infoRuutu = new Label();
        infoRuutu.BorderColor = Color.Transparent;
        infoRuutu.TextColor = Color.White;
        Game.Add(infoRuutu, 0);

        elamanPalautin = new Timer();
        elamanPalautin.Interval = Vakiot.PELAAJAN_ELAMIEN_REGENEROITUMISVAUHTI;
        elamanPalautin.Timeout += delegate { this.Elamat.Value++; };
        elamanPalautin.Start();

        aikaPoissaKentalta = new DoubleMeter(5.00);
        aikaPoissaKentalta.AddTrigger(0.0, TriggerDirection.Down, Kuolema);

        kentaltaPoistumisAjastin = new Timer();
        kentaltaPoistumisAjastin.Interval = 0.01;
        kentaltaPoistumisAjastin.Timeout += delegate
        {
            aikaPoissaKentalta.Value -= 0.01;
        };

        Kuolemat = new IntMeter(0);
        Tapot = new IntMeter(0, 0, MW2_My_Warfare_2_.MonestakoVoittaa);
        Tapot.Changed += delegate { this.TappojenMaaraNaytto.Text = "Tappoja: " + this.Tapot.Value; };
        Tapot.UpperLimit += delegate { Voitti(this); };
        this.kaytetaankoLasertahtainta = kaytetaankoLaseria;

        if (this.kaytetaankoLasertahtainta)
        {
            Lasertahtain = new GameObject(4000.0, 1.0);
            Lasertahtain.Left = this.Position.X + 10;
            Lasertahtain.Color = Color.Red;
            this.Add(Lasertahtain);
        }
        Nopeus = 300.0;


        Tag = "pelaaja";
        Mass = 10000;
        IsUpdated = true;

        if (kaytetaankoTaskuLamppua)
        {
            Taskulamppu = new GameObject(3072, 3072);
            Taskulamppu.Image = MW2_My_Warfare_2_.LoadImage("taskulampunvalotesti");
            Taskulamppu.Position = this.Position - new Vector(25.0, 0.0);
            this.Add(Taskulamppu);
            //MW2_My_Warfare_2_.Peli.Add(Taskulamppu, 0);
        }
    }

    /// <summary>
    /// Lisätään pelaajan nopeutta.
    /// </summary>
    public void Juokse()
    {
        if (!VoikoJuosta) return;

        this.Nopeus = 450.0;
        this.Juokseeko = true;
    }

    public void LopetaJuoksu()
    {
        if (this.VoikoKavella)
            this.Nopeus = 300.0;
        this.Juokseeko = false;
    }

    private void PaivitaKunnonTila()
    {
        if (this.Juokseeko)
        {
            this.Vasymys.Value -= Vakiot.PELAAJAN_VASYMIS_NOPEUS;
        }

        else
        {
            this.Vasymys.Value += Vakiot.PELAAJAN_KUNNON_PALAUTUMIS_NOPEUS;
        }
    }


    /// <summary>
    /// Respawnataan pelaaja spawnipaikkaan.
    /// </summary>
    public void Respawnaa()
    {
        Game.Add(this, 1);
        this.Elamat.Value = Vakiot.PELAAJIEN_ELAMIEN_MAARA;
        int i = 0;
        while (Vector.Distance(this.KuolemaPaikka, this.SpawnausPaikat[i]) < Vakiot.PELAAJAN_MINIMIRESPAUSETAISYYS_KUOLEMAPAIKASTA)
        {
            if (this.SpawnausPaikat.Count == 1) break;
            i = RandomGen.NextInt(0, this.SpawnausPaikat.Count);
        }
        this.Velocity = Vector.Zero;
        this.Position = this.SpawnausPaikat[i];
        this.tahtain.Position = this.SpawnausPaikat[i];
        this.Hit(new Vector(0.01, 0.01));
        KranaattienMaara = Vakiot.PELAAJAN_KRANAATTIEN_OLETUSMAARA;
        if (this.Taskulamppu != null)
        {
            if (MW2_My_Warfare_2_.Peli.CurrentGameMode == Gamemode.InfiniteSingle)
                MW2_My_Warfare_2_.Peli.KirkastaRuutua(0.02, 0.7);
            if (MW2_My_Warfare_2_.Peli.CurrentGameMode == Gamemode.SurvivalSingle)
                MW2_My_Warfare_2_.Peli.KirkastaRuutua(0.03, 1.0);
        }
    }

    /// <summary>
    /// Poistetaan pelaaja ruudulta pelaajan elämien mennessä nollaan, ja kasvatetaan
    /// kuolemien määrää yhdellä.
    /// </summary>
    private void Kuolema()
    {
        this.Kuolemat.Value++;
        KuolemaEfekti(this.X, this.Y, 300);
        Blood.AddDeathSplatter(this.Position, 3, 0.4);
        Timer.SingleShot(Vakiot.PELAAJAN_RESPAUS_AIKA, delegate { this.Respawnaa(); });
        Kuoli();
        this.KuolemaPaikka = this.Position;
        this.Extinquish();
        Game.Remove(this);
        if (this.Taskulamppu != null)
            MW2_My_Warfare_2_.Peli.PimennaRuutua(0.03);
    }

    /// <summary>
    /// Tarkistetaan, onko pelaajalla jokin tietty ase.
    /// </summary>
    /// <param name="tag">Aseen tagi, jota etsitään.</param>
    /// <returns>Onko pelaajalla asetta.</returns>
    public bool OnkoPelaajallaAse(String tag)
    {
        return aseet.Exists(ase => ase.Tag.ToString() == tag);
    }

    /// <summary>
    /// Lisätään pelaajalle ase, jos pelaajalla ei vielä ole sitä.
    /// </summary>
    /// <param name="w">Ase, jota ollaan lisäämässä.</param>
    public void LisaaAse(Ase w)
    {
        // Ei lisätä samaa asetta aseisiin montaa kertaa
        if (!aseet.Exists(x => x.Tag.ToString() == w.Tag.ToString()))
        {
            aseet.Add(w);
            this.Add(w);
            if (aseet.Count == 1)
                valittuAse = aseet[0];
        }
    }

    /// <summary>
    /// Poistetaan pelaajalta tietty ase, jos pelaajalla on se.
    /// </summary>
    /// <param name="w">Ase, joka poistetaan.</param>
    public void PoistaAse(Ase w)
    {
        if (!aseet.Contains(w))
            aseet.Remove(w);
    }

    /// <summary>
    /// Vaihtaa asetta.
    /// </summary>
    /// <param name="suunta">Montako hiiren rullan naksua ollaan liikutettu, pos (rulla ylöspäin)
    /// tai neg (rulla alaspäin)</param>
    public void VaihdaAse(int suunta)
    {
        if (LadataankoAsetta) return; // ei voi vaihtaa asetta, jos lataus on käynnissä

        int tamanHetkinenValinta = aseet.IndexOf(valittuAse);

        if (aseet != null && aseet.Count > 1)
        {
            foreach (Weapon ase in aseet)
            {
                ase.IsVisible = false;
            }
            if (tamanHetkinenValinta + suunta >= aseet.Count) valittuAse = aseet[0]; // ympäri loppupäästä
            else if (tamanHetkinenValinta + suunta < 0) valittuAse = aseet[aseet.Count - 1]; // ympäri alkupäästä
            else valittuAse = aseet[tamanHetkinenValinta + suunta];
            valittuAse.IsVisible = true;
        }
    }

    /// <summary>
    /// Valitaan ase tagin perusteella.
    /// </summary>
    /// <param name="tag">Aseen tag.</param>
    public void ValitseAse(String tag)
    {
        if (this.Aseet == null || this.Aseet.Count == 0) return;

        for (int i = 0; i < this.Aseet.Count; i++)
        {
            if (aseet[i].Tag.ToString() == tag) valittuAse = aseet[i];
        }
    }

    /// <summary>
    /// Tehdään veriroiske pelaajan kuollessa.
    /// </summary>
    /// <param name="x">Efektin X-koordinaatti.</param>
    /// <param name="y">Efektin Y-koordinaatti.</param>
    /// <param name="pMaara">Efektiin käytettävien partikkelien määrä.</param>
    private void KuolemaEfekti(double x, double y, int pMaara)
    {
        Image veriRoiske = MW2_My_Warfare_2_.LoadImage("veriroiske1");
        ExplosionSystem kuolemaefekti = new ExplosionSystem(veriRoiske, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        Game.Add(kuolemaefekti);

        kuolemaefekti.MinScale = 2;
        kuolemaefekti.MaxScale = 4;
        kuolemaefekti.MaxLifetime = 3.0;
        kuolemaefekti.MinLifetime = 1.0;
        kuolemaefekti.MaxVelocity = 5.0;

        kuolemaefekti.AddEffect(x, y, pMaara);
    }

    /// <summary>
    /// Kutsutaan, kun pelaaja poistuu kentän sallitulta alueelta.
    /// </summary>
    public void PoistuttiinKentalta()
    {
        this.OllaankoPoissaKentalta = true;
        this.kentaltaPoistumisAjastin.Start();
    }

    /// <summary>
    /// Kutsutaan, kun pelaaja palaa kentän sallitulle alueelle.
    /// </summary>
    public void PalattiinKentalle()
    {
        this.OllaankoPoissaKentalta = false;
        this.kentaltaPoistumisAjastin.Stop();
        this.kentaltaPoistumisAjastin.Reset();
        this.aikaPoissaKentalta.Value = 5.00;
    }

    public override void Update(Time time)
    {
        //Taskulamppu.Position = this.Position + Vector.FromLengthAndAngle(-25.0, this.Angle);
        base.Update(time);
    }
}