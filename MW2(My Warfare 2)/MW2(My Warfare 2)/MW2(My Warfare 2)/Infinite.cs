using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Pelimuoto, jossa on tarkoitus taistella jatkuvaa vihollisten
/// vyöryä vastaan.
/// </summary>
public class Infinite
{
    public static Infinite CurrentGame { get; set; }

    public static bool OnkoPeliKaynnissa { get; set; }

    /// <summary>
    /// Kaikki tällä hetkellä pelikentällä olevat viholliset.
    /// </summary>
    public List<Vihollinen> VihollisetKentalla { get; protected set; }

    /// <summary>
    /// Mitä eri vihollistyyppejä on olemassa. Viholliset spawnataan tästä listasta.
    /// </summary>
    public List<Vihollinen> VihollisTyypit { get; set; }

    /// <summary>
    /// Tällä spawnauskerralla spawnattavat viholliset.
    /// </summary>
    public List<Vihollinen> TallaSpawnauskerrallaSpawnattavatViholliset { get; set; }

    /// <summary>
    /// Jos KaytetaankoFixedSpawneja on true, viholliset spawnaavat satunnaisesti näihin paikkoihin.
    /// </summary>
    public List<Vector> VihollistenFixedSpawnit { get; set; }

    /// <summary>
    /// Aika tämän pelikerran alusta.
    /// </summary>
    public Timer AikaPelinAlusta { get; protected set; }

    /// <summary>
    /// Miten usein vihollisia spawnataan. Ajastimen lauetessa spawnataan vihollisia
    /// VihollisTyypit-listasta.
    /// </summary>
    public Timer VihollistenSpawnausAjastin { get; set; }

    /// <summary>
    /// Miten usein spawnataan tavaraa, kuten aseita tai seiniä. Käytetään Survivalissa.
    /// </summary>
    public Timer KamanSpawnausAjastin { get; set; }

    /// <summary>
    /// Montako vihollista voi spawnata maksimissaan yhtä spawnauskierrosta kohti.
    /// </summary>
    public int VihollistenMaxMaaraYhdellaSpawnauskierroksella { get; set; }

    /// <summary>
    /// Montako vihollista kentällä voi olla maksimissaan kerralla.
    /// </summary>
    public int VihollistenMaxMaaraPelissa { get; set; }

    /// <summary>
    /// Montako kertaa pelaaja(t) voivat vielä respata, ennen kuin peli päättyy.
    /// </summary>
    public IntMeter RespauksiaJaljellaYhteensa { get; set; }

    /// <summary>
    /// Montako kertaa pelaaja(t) ovat respanneet tämän pelin aikana.
    /// </summary>
    public IntMeter MonestikoRespattuYhteensa { get; protected set; }

    /// <summary>
    /// Montako vihollista on tapettu tämän pelikerran aikana.
    /// </summary>
    public IntMeter VihollisiaTapettu { get; set; }

    /// <summary>
    /// Spawnataanko vihollisia tällä hetkellä ollenkaan. Oletuksena true.
    /// </summary>
    public bool SpawnataankoVihollisia { get; set; }

    /// <summary>
    /// Spawnaavatko viholliset aina samoista määritellyistä paikoista.
    /// </summary>
    public bool KaytetaankoFixedSpawneja { get; set; }

    public delegate void InfiniteHandler(Infinite infinite);

    /// <summary>
    /// Kutsutaan, kun peli päättyy.
    /// </summary>
    public event InfiniteHandler PeliPaattyi;

    /// <summary>
    /// Initialisoidaan uusi SurvivalGame ja aloitetaan peli.
    /// </summary>
    public Infinite(int respaustenMaara)
    {
        if (Infinite.OnkoPeliKaynnissa) throw new NotSupportedException("Vain yksi Infinite voi olla käynnissä kerrallaan!");

        Infinite.OnkoPeliKaynnissa = true;
        Infinite.CurrentGame = this;
        this.VihollisetKentalla = new List<Vihollinen>();
        this.VihollisTyypit = new List<Vihollinen>();
        this.VihollisiaTapettu = new IntMeter(0);

        this.AikaPelinAlusta = new Timer();
        this.AikaPelinAlusta.Start();

        this.VihollistenSpawnausAjastin = new Timer();
        this.VihollistenSpawnausAjastin.Interval = Vakiot.SPAWNAUS_VAUHTI;
        this.VihollistenSpawnausAjastin.Timeout += MaaritteleTallaKierroksellaSpawnattavatViholliset;
        this.VihollistenSpawnausAjastin.Start();

        this.KamanSpawnausAjastin = new Timer();
        this.KamanSpawnausAjastin.Interval = Vakiot.KAMAN_SPAWNAUS_VAUHTI;

        this.VihollistenMaxMaaraYhdellaSpawnauskierroksella = Vakiot.VIHOLLISTEN_OLETUS_MAX_MAARA_SPAWNAUSKIERROSTA_KOHTI;
        this.VihollistenMaxMaaraPelissa = Vakiot.VIHOLLISTEN_MAX_MAARA_PELISSA;
        this.TallaSpawnauskerrallaSpawnattavatViholliset = new List<Vihollinen>();
        this.SpawnataankoVihollisia = true;
        this.VihollistenFixedSpawnit = new List<Vector>();
        this.KaytetaankoFixedSpawneja = false;

        this.RespauksiaJaljellaYhteensa = new IntMeter(respaustenMaara, 0, Int32.MaxValue);
        this.RespauksiaJaljellaYhteensa.LowerLimit += delegate { PeliPaattyi(this); };
        this.MonestikoRespattuYhteensa = new IntMeter(0);
    }

    /// <summary>
    /// Lisää vihollisen pelin spawnauslistaan niin, että se voi spawnata.
    /// </summary>
    /// <param name="lisattavaVihollinen"></param>
    public void LisaaVihollinenPeliin(Vihollinen lisattavaVihollinen)
    {
        this.VihollisTyypit.Add(lisattavaVihollinen);
        lisattavaVihollinen.Kuoli += delegate { this.VihollisiaTapettu.Value++; };
    }

    /// <summary>
    /// Määritellään, mitä vihollisia pitäisi spawnata tällä kierroksella ja lisätään ne spawnauslistaan.
    /// </summary>
    private void MaaritteleTallaKierroksellaSpawnattavatViholliset()
    {
        this.TallaSpawnauskerrallaSpawnattavatViholliset.Clear();
        if (VihollisTyypit.Count == 0) return; // ei spawnata, jos ei ole spawnattavaa
        if (SpawnataankoVihollisia == false) return; // ei spawnata, jos ei pidä spawnata
        if (VihollisetKentalla.Count >= VihollistenMaxMaaraPelissa) return; // ei spawnata, jos on liikaa vihuja ruudulla

        int vihollistenMaaraTallaKierroksella = RandomGen.NextInt(1, this.VihollistenMaxMaaraYhdellaSpawnauskierroksella + 1); // montako vihollista spawnataan tällä kierroksella

        for (int i = 0; i < vihollistenMaaraTallaKierroksella; i++)
        {
            Vihollinen spawnattavaVihollinen;
            spawnattavaVihollinen = new Vihollinen(RandomGen.SelectOneWithProbabilities<Vihollinen>(VihollisTyypit, 1.0)); // valitaan joku vihollistyyppi, PITÄÄ TULLA YHTEENSÄ 100 %!
            TallaSpawnauskerrallaSpawnattavatViholliset.Add(spawnattavaVihollinen);
        }
    }

    public void PelaajaKuoli()
    {
        this.MonestikoRespattuYhteensa.Value++;
        this.RespauksiaJaljellaYhteensa.Value--;
    }

    /// <summary>
    /// Lisätään uusi spawnipaikka zombeille.
    /// </summary>
    /// <param name="spawniPaikka">Spawnipaikka.</param>
    public void LisaaFixedSpawni(Vector spawniPaikka)
    {
        this.VihollistenFixedSpawnit.Add(spawniPaikka);
    }
} 