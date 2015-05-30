using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Effects;

/// <summary>
/// Pelissä käytettävät partikkelit.
/// </summary>
public static class Partikkelit
{
    public static ExplosionSystem KiviPartikkelit { get; private set; }
    public static ExplosionSystem KiviHajoaa { get; private set; }

    public static ExplosionSystem PuuPartikkelit { get; private set; }
    public static ExplosionSystem PuuHajoaa { get; private set; }

    public static ExplosionSystem KipinaPartikkelit { get; private set; }

    public static ExplosionSystem VeriPartikkelit { get; private set; }

    public static ExplosionSystem RajahdysPartikkelit1 { get; private set; }
    public static ExplosionSystem RajahdysPartikkelit2 { get; private set; }
    public static ExplosionSystem RajahdysPartikkelit3 { get; private set; }
    public static ExplosionSystem RajahdysPartikkelit4 { get; private set; }

    public static Flame Liekinheitin { get; private set; }

    public static List<Smoke> ValikkoSavut { get; private set; }

    public static Flame Tuli { get; private set; }

    private static readonly Image kivisirpale1 = MW2_My_Warfare_2_.LoadImage("kivisirpale1");
    private static readonly Image kivisirpale2 = MW2_My_Warfare_2_.LoadImage("kivisirpale2");
    private static readonly Image kipinä1 = MW2_My_Warfare_2_.LoadImage("kipinä1");
    private static readonly Image veriroiske1 = MW2_My_Warfare_2_.LoadImage("veriroiske1");
    private static readonly Image puusirpale1 = MW2_My_Warfare_2_.LoadImage("puusirpale1");
    private static readonly Image savuHiukkanen1 = MW2_My_Warfare_2_.LoadImage("savuhiukkanen1");
    private static readonly Image savuHiukkanen2 = MW2_My_Warfare_2_.LoadImage("savuhiukkanen2");
    private static readonly Image liekkiHiukkanen1 = MW2_My_Warfare_2_.LoadImage("liekkihiukkanen1");
    private static readonly Image liekkiHiukkanen2 = MW2_My_Warfare_2_.LoadImage("liekkihiukkanen2");
    private static readonly Image liekkiHiukkanen3 = MW2_My_Warfare_2_.LoadImage("liekkihiukkanen4");
    private static readonly Image flaregunhiukkanen = MW2_My_Warfare_2_.LoadImage("flaregunhiukkanen");
    private static readonly Image valikkopartikkeli1 = MW2_My_Warfare_2_.LoadImage("valikkopartikkeli1");
    private static readonly Image valikkopartikkeli2 = MW2_My_Warfare_2_.LoadImage("valikkopartikkeli2");

    //private static readonly Image rajahdysPartikkeli1 = MW2_My_Warfare_2_.LoadImage("Räjähdys1");
    //private static readonly Image rajahdysPartikkeli2 = MW2_My_Warfare_2_.LoadImage("räjähdyspartikkeli1");

    private static Image rajahdys1 = MW2_My_Warfare_2_.LoadImage("rajahdys1");
    private static Image rajahdys2 = MW2_My_Warfare_2_.LoadImage("rajahdys2");
    private static Image rajahdys3 = MW2_My_Warfare_2_.LoadImage("rajahdys3");
    private static Image rajahdys4 = MW2_My_Warfare_2_.LoadImage("rajahdys4");


    static Partikkelit()
    {
        InitializeParticles(MW2_My_Warfare_2_.Peli);
    }

    /// <summary>
    /// Alustetaan partikkelit käyttöä varten. Kutsu manuaalisesti
    /// vain ClearAllin jälkeen.
    /// </summary>
    /// <param name="peli">Peli, johon partikkelit lisätään.</param>
    public static void InitializeParticles(MW2_My_Warfare_2_ peli)
    {
        KiviPartikkelit = new ExplosionSystem(kivisirpale1, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        KiviPartikkelit.MinScale = 1;
        KiviPartikkelit.MaxScale = 2;
        KiviPartikkelit.MaxLifetime = 0.5;
        KiviPartikkelit.MinLifetime = 0.2;
        KiviPartikkelit.MaxVelocity = 1;
        peli.Add(KiviPartikkelit);

        KiviHajoaa = new ExplosionSystem(kivisirpale1, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        KiviHajoaa.MinScale = 5;
        KiviHajoaa.MaxScale = 10;
        KiviHajoaa.MaxLifetime = 1.0;
        KiviHajoaa.MinLifetime = 0.2;
        KiviHajoaa.MaxVelocity = 1;
        peli.Add(KiviHajoaa);

        PuuPartikkelit = new ExplosionSystem(puusirpale1, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        PuuPartikkelit.MinScale = 1;
        PuuPartikkelit.MaxScale = 2;
        PuuPartikkelit.MaxLifetime = 0.5;
        PuuPartikkelit.MinLifetime = 0.2;
        PuuPartikkelit.MaxVelocity = 1;
        peli.Add(PuuPartikkelit);

        PuuHajoaa = new ExplosionSystem(puusirpale1, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        PuuHajoaa.MinScale = 5;
        PuuHajoaa.MaxScale = 10;
        PuuHajoaa.MaxLifetime = 1.0;
        PuuHajoaa.MinLifetime = 0.2;
        PuuHajoaa.MaxVelocity = 0.5;
        peli.Add(PuuHajoaa);

        KipinaPartikkelit = new ExplosionSystem(kipinä1, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        KipinaPartikkelit.MinScale = 1;
        KipinaPartikkelit.MaxScale = 2;
        KipinaPartikkelit.MaxLifetime = 0.5;
        KipinaPartikkelit.MinLifetime = 0.2;
        KipinaPartikkelit.MaxVelocity = 0.3;
        peli.Add(KipinaPartikkelit);

        VeriPartikkelit = new ExplosionSystem(veriroiske1, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        VeriPartikkelit.MinScale = 1;
        VeriPartikkelit.MaxScale = 2;
        VeriPartikkelit.MaxLifetime = 0.5;//0.5
        VeriPartikkelit.MinLifetime = 0.2;
        VeriPartikkelit.MaxVelocity = 1;
        peli.Add(VeriPartikkelit);

        RajahdysPartikkelit1 = new ExplosionSystem(rajahdys1, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        RajahdysPartikkelit1.MinVelocity = 1.0;
        RajahdysPartikkelit1.MaxVelocity = 90.0; // 45
        RajahdysPartikkelit1.MaxScale = 20;
        RajahdysPartikkelit1.MaxLifetime = 0.4; // Jypelin oletus 0.8
        peli.Add(RajahdysPartikkelit1);

        RajahdysPartikkelit2 = new ExplosionSystem(rajahdys2, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        RajahdysPartikkelit2.MinVelocity = 25.0;
        RajahdysPartikkelit2.MaxVelocity = 140.0; // 70
        RajahdysPartikkelit2.MaxScale = 20;
        RajahdysPartikkelit2.MaxLifetime = 0.4;
        peli.Add(RajahdysPartikkelit2);

        RajahdysPartikkelit3 = new ExplosionSystem(rajahdys3, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        RajahdysPartikkelit3.MinVelocity = 60.0;
        RajahdysPartikkelit3.MaxVelocity = 200.0; // 100
        RajahdysPartikkelit3.MaxScale = 20;
        RajahdysPartikkelit3.MaxLifetime = 0.4;
        peli.Add(RajahdysPartikkelit3);

        RajahdysPartikkelit4 = new ExplosionSystem(rajahdys4, Vakiot.PARTIKKELEIDEN_MAX_MAARA);
        RajahdysPartikkelit4.MinVelocity = 50.0;
        RajahdysPartikkelit4.MaxVelocity = 220.0; // 110
        RajahdysPartikkelit4.MaxScale = 35;
        RajahdysPartikkelit4.MaxLifetime = 0.6;
        RajahdysPartikkelit4.AlphaAmount = 0.7;
        peli.Add(RajahdysPartikkelit4);

    }

    /// <summary>
    /// Lisätään partikkeliefekti kentälle.
    /// </summary>
    /// <param name="kaytettavatPartikkelit">Mitä partikkeleita lisätään.</param>
    /// <param name="paikka">Mihin partikkelit lisätään.</param>
    /// <param name="maara">Paljonko partikkeleita lisätään.</param>
    public static void AddParticleEffect(ExplosionSystem kaytettavatPartikkelit, Vector paikka, int maara)
    {
        kaytettavatPartikkelit.AddEffect(paikka, maara);
    }

    /// <summary>
    /// Lisätään valmis räjähdysefekti kentälle.
    /// </summary>
    /// <param name="paikka">Räjähdyksen paikka.</param>
    /// <param name="maara">Käytettävien partikkelien määrä.</param>
    public static void AddExplosionEffect(Vector paikka, int maara)
    {
        AddParticleEffect(RajahdysPartikkelit1, paikka, maara / 4);
        AddParticleEffect(RajahdysPartikkelit2, paikka, maara / 4);
        AddParticleEffect(RajahdysPartikkelit3, paikka, maara / 4);
        AddParticleEffect(RajahdysPartikkelit4, paikka, maara / 4);
    }

    /// <summary>
    /// Luodaan liekkiefekti ja lisätään se kentälle.
    /// </summary>
    /// <param name="paikka">Efektin paikka.</param>
    /// <returns>Flame.</returns>
    public static Flame CreateFlames(Vector paikka, int maara, double lapinakyvyys = 0.5)
    {
        Flame f = new Flame(liekkiHiukkanen3);
        f.Position = paikka;
        f.AlphaAmount = lapinakyvyys;
        MW2_My_Warfare_2_.Peli.Add(f, 1);
        f.AddEffect(paikka.X, paikka.Y, maara);
        return f;
    }

    public static Flame CreateDirectionalFlame(Vector paikka, Angle suunta, int maara, double lapinakyvyys = 0.5)
    {
        Flame f = new Flame(liekkiHiukkanen3);
        f.IsDirectional = true;
        f.FlameDirection = suunta;
        f.Position = paikka;
        f.MinVelocity = 40;
        f.MaxVelocity = 200;
        f.MinScale = 50;
        f.MaxScale = 100;
        f.AlphaAmount = lapinakyvyys;
        MW2_My_Warfare_2_.Peli.Add(f, 1);
        f.AddEffect(paikka.X, paikka.Y, maara);
        return f;
    }

    public static Flame CreateFlareGunParticles(Vector paikka, int maara, double lapinakyvyys = 0.5)
    {
        Flame f = new Flame(flaregunhiukkanen);
        f.Position = paikka;
        f.AlphaAmount = lapinakyvyys;
        f.MinScale = 30;
        f.MaxScale = 50;
        f.MinVelocity = 10;
        f.MaxVelocity = 60;
        f.MaxLifetime = 0.2;
        f.MinLifetime = 0.1;
        MW2_My_Warfare_2_.Peli.Add(f, 1);
        f.AddEffect(paikka.X, paikka.Y, maara);
        return f;
    }

    public static Smoke CreateSmoke(Image kuva, Vector paikka, double leveys, Angle suunta, double elinaika)
    {
        Smoke savu = new Smoke(kuva, leveys, suunta);
        savu.MaximumLifetime = TimeSpan.FromSeconds(100000);
        savu.MinLifetime = elinaika;
        savu.MaxLifetime = 40.0;
        savu.Position = paikka;
        savu.MinAcceleration = 50.0;
        return savu;
    }

    public static void LisaaValikkoTaustaPartikkelit()
    {
        ValikkoSavut = new List<Smoke>();
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 100, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli2, new Vector(MW2_My_Warfare_2_.Screen.Left + 200, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 300, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli2, new Vector(MW2_My_Warfare_2_.Screen.Left + 500, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 700, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 800, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli2, new Vector(MW2_My_Warfare_2_.Screen.Left + 1000, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 1100, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli2, new Vector(MW2_My_Warfare_2_.Screen.Left + 1200, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 1400, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 1500, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli2, new Vector(MW2_My_Warfare_2_.Screen.Left + 1600, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));
        ValikkoSavut.Add(CreateSmoke(valikkopartikkeli1, new Vector(MW2_My_Warfare_2_.Screen.Left + 1800, MW2_My_Warfare_2_.Screen.Bottom), 15, Angle.FromDegrees(90), RandomGen.NextInt(1, 20)));

        foreach (Smoke savu in ValikkoSavut)
        {
            MW2_My_Warfare_2_.Peli.Add(savu);
        }
    }

    public static void PoistaValikkoTaustaPartikkelit()
    {
        /*
        foreach (Smoke savu in ValikkoSavut)
        {
            savu.Destroy();
        }
        ValikkoSavut.Clear();*/ 
    }
}