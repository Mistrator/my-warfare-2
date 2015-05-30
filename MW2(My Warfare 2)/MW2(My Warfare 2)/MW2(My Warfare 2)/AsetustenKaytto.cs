using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Reflection;

/// <summary>
/// Asetusten lataaminen ja niiden tallentaminen tiedostoon.
/// </summary>
public static class AsetustenKaytto
{
    /// <summary>
    /// Pelin asetukset.
    /// </summary>
    public static Asetukset Asetukset { get; set; }

    /// <summary>
    /// Asetustiedoston nimi.
    /// </summary>
    public static string TiedostonNimi = "settings.dat";

    /// <summary>
    /// Pelin hakemistopolku.
    /// </summary>
    private static string PelinHakemistoPolku = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Asetukset)).CodeBase).Remove(0, 6);

    /// <summary>
    /// Mihin asetustiedosto tallennetaan.
    /// </summary>
    private static string TallennusPaikkaTiedostonKanssa = PelinHakemistoPolku + "\\" + TiedostonNimi;

    static AsetustenKaytto()
    {
        Asetukset = new Asetukset(); // tehdään instanssi Asetuksista käyttöä varten.
    }

    /// <summary>
    /// Ladataan asetukset tiedostosta.
    /// </summary>
    public static void LataaAsetukset()
    {
        FileStream fInStream;
        if (!File.Exists(TallennusPaikkaTiedostonKanssa))
        {
            fInStream = File.Create(TallennusPaikkaTiedostonKanssa);
        }
        else fInStream = File.OpenRead(TallennusPaikkaTiedostonKanssa);
        SoapFormatter soapFormatter = new SoapFormatter();
        while (fInStream.Position != fInStream.Length) Asetukset = (Asetukset)soapFormatter.Deserialize(fInStream);
        fInStream.Close();
    }

    /// <summary>
    /// Tallennetaan asetukset tiedostoon.
    /// </summary>
    public static void TallennaAsetukset()
    {
        if (!File.Exists(TallennusPaikkaTiedostonKanssa)) File.Create(TallennusPaikkaTiedostonKanssa);
        FileStream fOutStream = File.Open(TallennusPaikkaTiedostonKanssa, FileMode.Create, FileAccess.Write);
        SoapFormatter soapFormatter = new SoapFormatter();
        soapFormatter.Serialize(fOutStream, Asetukset);
        fOutStream.Close();
    }

    /// <summary>
    /// Palautetaan oletusasetukset ja tallennetaan ne.
    /// </summary>
    public static void PalautaOletusAsetukset()
    {
        // Oletusasetukset.
        Asetukset.OnkoFullscreen = true;
        Asetukset.OnkoAntialiasing = true;
        Asetukset.OnkoPelaajalla1Lasertahtainta = false;
        Asetukset.OnkoPelaajalla2Lasertahtainta = true;
        Asetukset.KayttajaNimi = "N00b";
        TallennaAsetukset();
    }
}