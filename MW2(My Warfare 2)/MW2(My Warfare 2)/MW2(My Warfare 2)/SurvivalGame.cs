using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Pelimuoto, jossa on tarkoitus taistella jatkuvaa vihollisten
/// vyöryä vastaan.
/// </summary>
public class SurvivalGame
{
    /// <summary>
    /// Kaikki tällä hetkellä pelikentällä olevat viholliset.
    /// </summary>
    public List<Vihollinen> VihollisetKentalla { get; protected set; }

    /// <summary>
    /// Mitä eri vihollistyyppejä on olemassa. Viholliset spawnataan tästä listasta.
    /// </summary>
    public List<Vihollinen> VihollisTyypit { get; set; }

    /// <summary>
    /// Aika tämän pelikerran alusta.
    /// </summary>
    public Timer AikaPelinAlusta { get; protected set; }

    /// <summary>
    /// Miten usein vihollisia spawnataan.
    /// </summary>
    private Timer VihollistenSpawnausAjastin { get; set; }

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
    public IntMeter VihollisiaTapettu { get; protected set; }

    /// <summary>
    /// Initialisoidaan uusi SurvivalGame ja aloitetaan peli.
    /// </summary>
    public SurvivalGame()
    {
        this.VihollisetKentalla = new List<Vihollinen>();
        this.VihollisTyypit = new List<Vihollinen>();
        this.VihollisiaTapettu = new IntMeter(0);

        this.AikaPelinAlusta = new Timer();
        this.AikaPelinAlusta.Start();

        this.VihollistenSpawnausAjastin = new Timer();
        this.VihollistenSpawnausAjastin.Interval = Vakiot.SPAWNAUS_VAUHTI;
        this.VihollistenSpawnausAjastin.Timeout += SpawnaaVihollisia;
        this.VihollistenSpawnausAjastin.Start();

        this.RespauksiaJaljellaYhteensa = new IntMeter(Vakiot.RESPAUSTEN_OLETUSMAARA, 0, Int32.MaxValue);
        this.MonestikoRespattuYhteensa = new IntMeter(0);
    }

    /// <summary>
    /// Lisää vihollisen pelin spawnauslistaan niin, että se voi spawnata.
    /// </summary>
    /// <param name="lisattavaVihollinen"></param>
    public void LisaaVihollinenPeliin(Vihollinen lisattavaVihollinen)
    {
        this.VihollisTyypit.Add(lisattavaVihollinen);
    }

    /// <summary>
    /// Spawnataan vihollisia määritellyille alueille.
    /// </summary>
    private void SpawnaaVihollisia()
    {
        if (VihollisTyypit.Count == 0) return; // ei spawnata, jos ei ole spawnattavaa

        //Vihollinen spawnattavaVihollinen = new
    }
}
