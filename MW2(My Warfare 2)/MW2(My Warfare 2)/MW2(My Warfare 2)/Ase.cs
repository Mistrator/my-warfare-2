using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// <summary>
/// Paranneltu AssaultRifle, jolla on mm. tuhovoimamäärät.
/// </summary>
public class Ase : AssaultRifle
{
    /// <summary>
    /// Aseen tuhovoima seiniä vastaan.
    /// </summary>
    public double TuhovoimaTuhoutuviaVastaan { get; set; }

    /// <summary>
    /// Aseen tuhovoima pelaajaa ja zombeja vastaan. (joo joo, zombi ei ole elävä xD)
    /// </summary>
    public double TuhovoimaElaviaVastaan { get; set; }

    public double aikaJolloinVoiLuoda;

    /// <summary>
    /// Kauanko aseen latauksessa kestää.
    /// </summary>
    public double LataukseenKuluvaAika { get; set; }

    /// <summary>
    /// Aseen näytöllä näkyvä nimi.
    /// </summary>
    public string AseenNimi { get; set; }

    /// <summary>
    /// Paljonko aseen luodit hajoavat ammuttaessa eri suuntiin.
    /// X- ja Y-komponentit mielellään samat, jotta luotisuihku olisi tasainen. 
    /// </summary>
    public Vector AseenHajoama { get; set; }


    private double aseenLapaisy;
    public bool LapaiseekoMateriaaleja { get; private set; }

    /// <summary>
    /// Paljonko ase läpäisee materiaaleja.
    /// </summary>
    public double AseenLapaisy
    {
        get { return aseenLapaisy; }
        set 
        {
            if (value > 0)
            {
                aseenLapaisy = value;
                LapaiseekoMateriaaleja = true;
            }
            else LapaiseekoMateriaaleja = false;
        }
    }

    public bool OnkoMeleeAse = false;
    public bool TuleekoHylsya = true;
    public bool NakyykoAmmus = true;
    public bool TuleekoOsumastaEfektiä = true;
    public bool KuulukoOsumastaAanta = true;
    public bool HaviaakoAmmusOsumasta = true;

    /// <summary>
    /// Käytetäänkö valojuovia. Muista myös TracerBrightness.
    /// </summary>
    public bool UsesTracers { get; set; }

    /// <summary>
    /// Mahdollisen valojuovan kirkkaus. 1.0 on oletus, suurempi kirkastaa ja pienempi himmentää.
    /// </summary>
    public double TracerBrightness { get; set; }

    /// <summary>
    /// Määrittelee ajan, jonka valojuova pysyy kentällä.
    /// </summary>
    public double TracerLength { get; set; }

    /// <summary>
    /// Ylikirjoittaa oletusvalojuovavärin tämän aseen kohdalla. Jos null, ei tehdä mitään.
    /// </summary>
    public Color OverrideTracerColor { get; set; }

    /// <summary>
    /// Hakeutuuko ammus kohteeseensa.
    /// </summary>
    public bool IsHoming { get; set; }

    /// <summary>
    /// Sytyttävätkö ammukset asioita palamaan.
    /// </summary>
    public bool IsIncendiary { get; set; }

    /// <summary>
    /// Sytyttämistodennäköisyys väliltä 0-100.
    /// </summary>
    public int IgnitionChance { get; set; }

    public Image hylsynKuva = MW2_My_Warfare_2_.LoadImage("rynkynhylsy");
    public Image luodinKuva = MW2_My_Warfare_2_.LoadImage("isoluoti");

    /// <summary>
    /// Kuva, joka on pelaajalla 1, kun tämä ase on valittuna.
    /// </summary>
    public Image Pelaajan1AseKuva { get; set; }

    /// <summary>
    /// Kuva, joka on pelaajalla 2, kun tämä ase on valittuna.
    /// </summary>
    public Image Pelaajan2AseKuva { get; set; }

    /// <summary>
    /// Aseen latausanimaatio.
    /// </summary>
    public Image[] LatausAnimaatio1 { get; set; }

    /// <summary>
    /// Aseen latausanimaatio.
    /// </summary>
    public Image[] LatausAnimaatio2 { get; set; }

    /// <summary>
    /// Aseen ampumisanimaatio pelaajalle 1.
    /// </summary>
    public Animation HyokkaysAnimaatio1 { get; set; }

    /// <summary>
    /// Aseen ampumisanimaatio pelaajalle 2.
    /// </summary>
    public Animation HyokkaysAnimaatio2 { get; set; }

    /// <summary>
    /// Suurin mahdollinen määrä ammuksia mukana kerralla aseeseen.
    /// </summary>
    public IntMeter MaxAmmo { get; set; }

    /// <summary>
    /// Montako ammusta aseesta lähtee yhdellä ampumiskerralla.
    /// </summary>
    public int BulletsPerShot { get; set; }

    public Ase(double leveys, double korkeus)
        : base(leveys, korkeus)
    {
        this.IsVisible = false;
        this.TracerBrightness = 1.0;
        this.TracerLength = 0.1;
        this.OverrideTracerColor = Color.Transparent;
        this.BulletsPerShot = 1;
    }
}