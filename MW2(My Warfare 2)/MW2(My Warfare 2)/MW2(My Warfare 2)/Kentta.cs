using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Tietorakenne pelikentän seinille.
/// </summary>
public class Kentta
{
    private GameObject[,] KentanOsat { get; set; }

    private String ValeSeinienTag { get; set; }

    /// <summary>
    /// Luodaan uusi kentän tietorakenne.
    /// </summary>
    /// <param name="riveja">Kentän rivien määrä.</param>
    /// <param name="sarakkeita">Kentän sarakkeiden määrä.</param>
    public Kentta(int riveja, int sarakkeita, String valeSeinienTag)
    {
        KentanOsat = new GameObject[riveja, sarakkeita];
        ValeSeinienTag = valeSeinienTag;
    }

    /// <summary>
    /// Lisätään seinä tietorakenteeseen.
    /// </summary>
    /// <param name="rivi">Seinän rivi tietorakenteessa.</param>
    /// <param name="sarake">Seinän sarake tietorakenteessa.</param>
    /// <param name="lisattava">Lisättävä seinä.</param>
    public void LisaaSeina(int rivi, int sarake, GameObject lisattava)
    {
        if (!TarkistaOlemassaolo(rivi, sarake, true)) return;
            KentanOsat[rivi, sarake] = lisattava;
    }

    /// <summary>
    /// Poistetaan seinän osa tietorakenteesta.
    /// </summary>
    /// <param name="x">Seinän rivi.</param>
    /// <param name="y">Seinän sarake.</param>
    public void TuhoaSeina(int rivi, int sarake)
    {
        if (!TarkistaOlemassaolo(rivi, sarake)) return; // ei tuhota olematonta
        MuutaNaapuritKiinteiksi(rivi, sarake);

        KentanOsat[rivi, sarake].Destroy();
        KentanOsat[rivi, sarake] = null;
    }

    /// <summary>
    /// Tarkistetaan, ettei jokin paikka ole null ja se on taulukon sisäpuolella.
    /// </summary>
    /// <param name="x">X taulukossa</param>
    /// <param name="y">Y taulukossa</param>
    /// <param name="saakoOllaVarattu">Jos true, ruutu saa olla jo varattu. Taulukon sisäpuolella olo tarkistetaan silti.</param>
    /// <returns>True jos paikka on varattu ja taulukon sisäpuolella, muuten false.</returns>
    private bool TarkistaOlemassaolo(int x, int y, bool saakoOllaVarattu = false)
    {
        if (x < 0 || x >= KentanOsat.GetLength(0)) return false;
        if (y < 0 || y >= KentanOsat.GetLength(1)) return false;
        if (!saakoOllaVarattu) // voidaan ohittaa vaatimus tyhjästä ruudusta
            if (KentanOsat[x, y] == null) return false;
        return true;
    }

    /// <summary>
    /// Muutetaan tuhotun kentän osan naapurit kiinteiksi, jos ne eivät jo ole.
    /// </summary>
    /// <param name="x">Tuhotun osan X taulukossa</param>
    /// <param name="y">Tuhotun osan Y taulukossa</param>
    private void MuutaNaapuritKiinteiksi(int x, int y)
    {
        if (TarkistaOlemassaolo(x + 1, y))
            if (KentanOsat[x + 1, y].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x + 1, y);

        if (TarkistaOlemassaolo(x - 1, y))
            if (KentanOsat[x - 1, y].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x - 1, y);

        if (TarkistaOlemassaolo(x, y + 1))
            if (KentanOsat[x, y + 1].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x, y + 1);

        if (TarkistaOlemassaolo(x, y - 1))
            if (KentanOsat[x, y - 1].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x, y - 1);
    }

    /// <summary>
    /// Poistetaan läpi mentävä seinä ja laitetaan tilalle kiinteä kopio.
    /// </summary>
    /// <param name="x">Muutettavan X taulukossa</param>
    /// <param name="y">Muutettavan Y taulukossa</param>
    private void VaihdaSeinaKiinteaksi(int x, int y)
    {
        Tuhoutuva kiinteaseina = new Tuhoutuva(KentanOsat[x, y].Width, KentanOsat[x, y].Height, 20); // äh, kesto fixattu...
        kiinteaseina.Position = KentanOsat[x, y].Position;
        kiinteaseina.Image = KentanOsat[x, y].Image;
        kiinteaseina.PositionInLevelArray = new IntPoint(x, y);
        kiinteaseina.CollisionIgnoreGroup = 1;
        kiinteaseina.MakeStatic();
        kiinteaseina.Kesto.LowerLimit += delegate
        {
            kiinteaseina.Destroy();
            TuhoaSeina(x, y);
            // tänne jotain ?
        };
        MW2_My_Warfare_2_.Peli.Add(kiinteaseina);
        KentanOsat[x, y].Destroy();
        KentanOsat[x, y] = kiinteaseina;
    }
}