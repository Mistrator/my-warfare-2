using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Widgets;

/// <summary>
/// Pelimuoto, jossa on tarkoituksena päästä kentän toiselle laidalle kopterin laskeutumispaikalle,
/// tyhjentää laskeutumisalue zombeista ja puolustaa kopteria nousuun asti.
/// </summary>
public class Escape : Infinite
{
    /// <summary>
    /// Tämänhetkinen peli.
    /// </summary>
    public static Escape Peli { get; set; }

    /// <summary>
    /// Mihin kopteri laskeutuu. Muokkaa muualta vain LuoKentta-kohdassa.
    /// </summary>
    public Vector LaskeutumisPaikka { get; set; }

    public const double LASKEUTUMISEEN_VAADITTAVAN_VAPAAN_ALUEEN_HALKAISIJA = 50.0;

    private List<Pelaaja> PelaajatPelissa { get; set; }

    /// <summary>
    /// Helikopterin puolustusaika maassa.
    /// </summary>
    private Timer PuolustusAjastin { get; set; }

    private Timer OnkoLaskeutumisAlueTyhjaTarkistus { get; set; }

    public DoubleMeter PuolustetutSekunnit { get; private set; }

    public ProgressBar PuolustusAikaPalkki { get; private set; }

    public Helikopteri Kopteri { get; set; }

    public Escape(int respaustenMaara, double puolustettavaAika, List<Pelaaja> pelaajat)
        : base(respaustenMaara)
    {
        //LaskeutumisPaikka = landingPosition;
        Peli = this;
        PelaajatPelissa = pelaajat;

        PuolustetutSekunnit = new DoubleMeter(0, 0, puolustettavaAika);

        PuolustusAjastin = new Timer();
        PuolustusAjastin.Interval = 1;
        PuolustusAjastin.Timeout += delegate { PuolustetutSekunnit.Value++; };

        PuolustusAikaPalkki = new ProgressBar(150, 10);
        PuolustusAikaPalkki.Color = Color.Blue;
        PuolustusAikaPalkki.BindTo(PuolustetutSekunnit);
        PuolustusAikaPalkki.BorderColor = Color.White;
        PuolustusAikaPalkki.Position = new Vector(LaskeutumisPaikka.X, LaskeutumisPaikka.Y + 150);

        OnkoLaskeutumisAlueTyhjaTarkistus = new Timer();
        OnkoLaskeutumisAlueTyhjaTarkistus.Interval = 1;
        OnkoLaskeutumisAlueTyhjaTarkistus.Timeout += OnkoLaskeutumisAlueVapaa;
        OnkoLaskeutumisAlueTyhjaTarkistus.Start();

        //Kopteri = new Helikopteri(233, 73, 50, false); // 211, 171
        //Kopteri.Position = 
        //SpawnataankoVihollisia = false;

    }

    /// <summary>
    /// Nostetaan kopteri ilmaan, poistetaan pelaajat, lennetään pois ja voitetaan peli.
    /// </summary>
    void LastausValmis()
    {
        Kopteri.NouseIlmaan();
    }

    void OnkoLaskeutumisAlueVapaa()
    {
        foreach (Pelaaja p in PelaajatPelissa)
        {
            if (p != null)
                if (!OnkoLaskeutumisAlueenSisapuolella(p.Position)) return;
        }

        //foreach (Vihollinen v in VihollisetKentalla)
        //{
        //    if (VihollisetKentalla.Count == 0) return;
        //    if (OnkoLaskeutumisAlueenSisapuolella(v.Position)) return;
        //}

        // Kopteri voi saapua
        //MW2_My_Warfare_2_.Peli.MessageDisplay.Add("[TESTI] Alue " + LASKEUTUMISEEN_VAADITTAVAN_VAPAAN_ALUEEN_HALKAISIJA + " yksikön säteeltä puhdas!");
    }

    /// <summary>
    /// Onko jokin piste laskeutumisalueen sisäpuolella.
    /// </summary>
    /// <param name="paikka">Tarkistettava piste.</param>
    /// <returns>Onko sisäpuolella.</returns>
    bool OnkoLaskeutumisAlueenSisapuolella(Vector paikka)
    {
        if (paikka.X < LaskeutumisPaikka.X - LASKEUTUMISEEN_VAADITTAVAN_VAPAAN_ALUEEN_HALKAISIJA / 2 || paikka.X > LaskeutumisPaikka.X + LASKEUTUMISEEN_VAADITTAVAN_VAPAAN_ALUEEN_HALKAISIJA / 2)
            return false;
        if (paikka.Y < LaskeutumisPaikka.Y - LASKEUTUMISEEN_VAADITTAVAN_VAPAAN_ALUEEN_HALKAISIJA / 2 || paikka.Y > LaskeutumisPaikka.Y + LASKEUTUMISEEN_VAADITTAVAN_VAPAAN_ALUEEN_HALKAISIJA / 2)
            return false;
        return true;
    }
}