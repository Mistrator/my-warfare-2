using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// @author Vesa Lappalainen, Antti-Jussi Lakanen
/// @version 18.2.2013
///
/// <summary>
/// Tutkitaan elämäpeliä. 
/// </summary>
/// <remarks>
/// Säännöt:
/// <code>
/// Jos ruudussa on sopuli (eli alkion arvo on 1)
///   - Jos sillä on naapureita (ykkösiä) yksi tai 
///     nolla kappaletta, se kuolee yksinäisyyteen 
///     (muuttuu nollaksi).
///   - Jos sillä on neljä tai enemmän naapureita, 
///     se kuolee ylikansoitukseen (muuttuu nollaksi).
///   - Sellainen, jolla on täsmälleen kaksi tai kolme 
///     naapuria, selviää hengissä.
/// Jos ruutu on tyhjä (eli arvo on 0)
///   - Jos on täsmälleen kolme naapuria, ruutu "herää 
///     eloon" (muuttuu ykköseksi).
/// </code>
/// Tutkitaan kahdella eri tavalla:
/// <list type="number">
///  <item>Uusi tilanne tehdään edellisestä uuteen taulukkon.</item>
///  <item>Muutos vaikuttaa seuraavaan ruutuun</item>
///  </list>
/// </remarks>
public class GameOfLifeLaskenta
{
    /// <summary>
    /// Palauttaa taulukon merkkijonona niin, että alkioiden välissä on erotin.
    /// </summary>
    /// <param name="luvut">Taulukko josta tehdään merkkijono</param>
    /// <param name="sarakeErotin">Jono, jolla rivin alkiot erotetaan toisistaan</param>
    /// <param name="riviErotin">Jono, jolla rivit erotetaan toisistaan</param>
    /// <param name="riviformaatti">C# tyylinen formaattojono, jolla yksi rivi käsitellään.
    /// Jos formaattijonossa ei ole lainkaan {-merkkiä, käytetään jonoa sellaisenaan rivien
    /// erottomina
    /// </param>
    /// <example>
    /// <pre name="test">
    ///  int[,] luvut = {{1,2,3}, {4,5,6}, {7,8,9}};
    ///  Sopulit.Jonoksi(luvut) === "1 2 3\n4 5 6\n7 8 9";
    ///  Sopulit.Jonoksi(luvut," ",",") === "1 2 3,4 5 6,7 8 9";
    ///  Sopulit.Jonoksi(luvut,":","|","[ {0} ]") === "[ 1:2:3 ]|[ 4:5:6 ]|[ 7:8:9 ]";
    /// </pre>
    /// </example>
    public static String Jonoksi(int[,] luvut, string sarakeErotin = " ",
                                 string riviErotin = "\n", string riviformaatti = "{0}")
    {
        StringBuilder tulos = new StringBuilder();
        String rivivali = "";
        for (int iy = 0; iy < luvut.GetLength(0); iy++)
        {
            String vali = "";
            StringBuilder rivi = new StringBuilder();
            for (int ix = 0; ix < luvut.GetLength(1); ix++)
            {
                rivi.Append(vali + luvut[iy, ix]);
                vali = sarakeErotin;
            }
            tulos.Append(rivivali + String.Format(riviformaatti, rivi));
            rivivali = riviErotin;
        }
        return tulos.ToString();
    }


    /// <summary>
    /// Muodostaa seuraavan vaiheen taulukosta luomalla
    /// uuden taulukon ja tekee sinne 
    /// </summary>
    /// <param name="sukupolvi">Nykyinen vaihe, josta tehdään seuraava vaihe</param>
    /// <returns>Elämäpelin seuraava vaihe</returns>
    /// <example>
    /// <pre name="test">
    ///   int[,] alku = {
    ///                { 1,0,1,1 },
    ///                { 0,1,1,0 },
    ///                { 1,0,0,0 },
    ///                { 1,0,0,1 }
    ///              };
    ///    int[,] seuraava;
    ///    seuraava = Sopulit.MuodostaUusiSukupolvi(alku);
    ///    Sopulit.Jonoksi(seuraava," ",",") === "0 0 1 1,1 0 1 1,1 0 1 0,0 0 0 0";
    ///    seuraava = Sopulit.MuodostaUusiSukupolvi(seuraava);
    ///    Sopulit.Jonoksi(seuraava," ",",") === "0 1 1 1,0 0 0 0,0 0 1 1,0 0 0 0";
    ///    seuraava = Sopulit.MuodostaUusiSukupolvi(seuraava);
    ///    Sopulit.Jonoksi(seuraava," ",",") === "0 0 1 0,0 1 0 0,0 0 0 0,0 0 0 0";
    /// </pre>
    /// </example>
    public static int[,] MuodostaUusiSukupolvi(int[,] sukupolvi)
    {
        int[,] uusi = new int[sukupolvi.GetLength(0), sukupolvi.GetLength(1)];
        SeuraavaSukupolvi(sukupolvi, uusi);
        return uusi;
    }


    /// <summary>
    /// Muodostaa seuraavan vaiheen menemällä taulukkoa 
    /// rivi kerrallaan ylhäältä alas ja vasemmalta oikealle.
    /// Tulos tulee samaan taulukkoon jos seuraava = null.
    /// </summary>
    /// <param name="sukupolvi">Nykyinen vaihe, josta tilanne lasketaan</param>
    /// <param name="seuraava">Vaihe, johon uusi vaihe lasketaan.  Jos == null,
    /// niin vaihe lasketaan vaihe-taulukkoon.  JOs taulukoiden koot eivät
    /// ole samat, käyteään pienemmän kokoa</param>
    /// <example>
    /// <pre name="test">
    ///   int[,] vaihe = {
    ///                { 1,0,1,1 },
    ///                { 0,1,1,0 },
    ///                { 1,0,0,0 },
    ///                { 1,0,0,1 }
    ///              };
    ///    Sopulit.SeuraavaSukupolvi(vaihe);
    ///    Sopulit.Jonoksi(vaihe," ",",") === "0 1 0 0,1 0 0 0,1 1 0 0,1 1 1 0";
    ///    Sopulit.SeuraavaSukupolvi(vaihe);
    ///    Sopulit.Jonoksi(vaihe," ",",") === "0 0 0 0,1 1 0 0,0 0 1 0,0 1 1 0";
    ///    Sopulit.SeuraavaSukupolvi(vaihe);
    ///    Sopulit.Jonoksi(vaihe," ",",") === "0 0 0 0,0 0 0 0,0 1 1 0,0 1 1 0";
    /// </pre>
    /// </example>
    public static void SeuraavaSukupolvi(int[,] sukupolvi, int[,] seuraava = null)
    {
        if (seuraava == null) seuraava = sukupolvi;
        int maxy = Math.Min(sukupolvi.GetLength(0), seuraava.GetLength(0));
        int maxx = Math.Min(sukupolvi.GetLength(1), seuraava.GetLength(1));
        for (int iy = 0; iy < maxy; iy++)
        {
            for (int ix = 0; ix < maxx; ix++)
            {
                int arvo = sukupolvi[iy, ix];
                int uusiarvo = 0;
                int naapureita = LaskeNaapurit(sukupolvi, iy, ix);
                if (arvo == 1 && (naapureita == 2 || naapureita == 3)) uusiarvo = 1;
                if (arvo == 0 && naapureita == 3) uusiarvo = 1;
                seuraava[iy, ix] = uusiarvo;
            }
        }
    }


    /// <summary>
    /// Laskee montako nollasta poikkeavaa naapuria on valitulla alkiolla
    /// </summary>
    /// <param name="sukupolvi">taulukko josta naapureita etsitään</param>
    /// <param name="rivi">minkä rivin naapureita etsitään</param>
    /// <param name="sarake">minkä sarakkeen naapureita etsitään</param>
    /// <returns></returns>
    /// <example>
    /// <pre name="test">
    ///   int[,] alku = {
    ///                { 1,0,1,1 },
    ///                { 0,1,1,0 },
    ///                { 1,0,0,0 },
    ///                { 2,0,0,0 }
    ///              };
    ///   Sopulit.LaskeNaapurit(alku, 0, 0) === 1;
    ///   Sopulit.LaskeNaapurit(alku, 3, 0) === 1;
    ///   Sopulit.LaskeNaapurit(alku, 0, 1) === 4;
    ///   Sopulit.LaskeNaapurit(alku, 2, 2) === 2;
    ///   Sopulit.LaskeNaapurit(alku, 3, 2) === 0;
    /// </pre>
    /// </example>
    public static int LaskeNaapurit(int[,] sukupolvi, int rivi, int sarake)
    {
        int summa = 0;
        int maxy = Math.Min(sukupolvi.GetLength(0) - 1, rivi + 1);
        int maxx = Math.Min(sukupolvi.GetLength(1) - 1, sarake + 1);
        for (int iy = Math.Max(0, rivi - 1); iy <= maxy; iy++)
            for (int ix = Math.Max(0, sarake - 1); ix <= maxx; ix++)
            {
                if (iy != rivi || ix != sarake) // itseä ei lasketa naapuriksi
                    if (sukupolvi[iy, ix] > 0) summa++;
            }
        return summa;
    }


    /// <summary>
    /// Arvotaan taulukkoon lukuja [ala,yla] välille.
    /// </summary>
    /// <param name="taulukko">Taulukko johon lukuja arvotaan</param>
    /// <param name="ala">Pienen arvo joka taulukkoon voi tulla</param>
    /// <param name="yla">Suurin arvo joka taulukkon voi tulla</param>
    /// <example>
    /// <pre name="test">
    ///  int[,] luvut = new int[3,3];
    ///  Sopulit.Arvo(luvut,4,8);
    ///  foreach (int luku in luvut)
    ///     4 <= luku && luku <= 8 === true;
    /// </pre>
    /// </example>
    public static void Arvo(int[,] taulukko, int ala, int yla)
    {
        Random rnd = new Random();
        int ny = taulukko.GetLength(0);
        int nx = taulukko.GetLength(1);
        for (int iy = 0; iy < ny; iy++)
            for (int ix = 0; ix < nx; ix++)
                taulukko[iy, ix] = ala + rnd.Next(yla - ala + 1);
    }


    /// <summary>
    /// Täytetään taulukko valitulla arvolla
    /// </summary>
    /// <param name="taulukko">Täytettävä taulukko</param>
    /// <param name="arvo">Arvo, jolla taulukko täytetään</param>
    /// <example>
    /// <pre name="test">
    ///  int[,] luvut = new int[3,3];
    ///  Sopulit.Tayta(luvut,7);
    ///  foreach (int luku in luvut)
    ///     luku === 7;
    ///  Sopulit.Tayta(luvut,2);
    ///  foreach (int luku in luvut)
    ///     luku === 2;
    /// </pre>
    /// </example>
    public static void Tayta(int[,] taulukko, int arvo)
    {
        int ny = taulukko.GetLength(0);
        int nx = taulukko.GetLength(1);
        for (int iy = 0; iy < ny; iy++)
            for (int ix = 0; ix < nx; ix++)
                taulukko[iy, ix] = arvo;
    }

}