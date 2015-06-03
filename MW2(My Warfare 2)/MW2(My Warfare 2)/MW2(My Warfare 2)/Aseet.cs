using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Aseiden luonti.
/// </summary>
public static class Aseet
{
    public static readonly SoundEffect singonAani = MW2_My_Warfare_2_.LoadSoundEffect("Blast");
    public static readonly SoundEffect pistoolinAani = MW2_My_Warfare_2_.LoadSoundEffect("pistol");
    public static readonly SoundEffect snipanAani = MW2_My_Warfare_2_.LoadSoundEffect("awp1");
    public static readonly SoundEffect rynkynAani = MW2_My_Warfare_2_.LoadSoundEffect("ak47-1");
    public static readonly SoundEffect singonLaukausAani = MW2_My_Warfare_2_.LoadSoundEffect("rpg7");
    public static readonly SoundEffect minigunAani = MW2_My_Warfare_2_.LoadSoundEffect("m249SAW_aani");
    public static readonly SoundEffect magnumAani = MW2_My_Warfare_2_.LoadSoundEffect("Magnum44");
    public static readonly SoundEffect nyrkkiAani = MW2_My_Warfare_2_.LoadSoundEffect("nyrkki");
    public static readonly SoundEffect vintorezAani = MW2_My_Warfare_2_.LoadSoundEffect("vintorez_shot");
    public static readonly SoundEffect haulikkoAani = MW2_My_Warfare_2_.LoadSoundEffect("shotgun_shot");
    public static readonly SoundEffect flaregunAani = MW2_My_Warfare_2_.LoadSoundEffect("flaregun_shot");

    public static Image pelaaja1pistooliKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1pistooli");
    public static Image pelaaja1rynkkyKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1rynkky");
    public static Image pelaaja1haulikkoKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1haulikko1");
    public static Image pelaaja1minigunKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1SAW");
    public static Image pelaaja1snipaKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1dragunov");
    public static Image pelaaja1varsijousiKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1varsijousi1");
    public static Image pelaaja1rpgKuva1 = MW2_My_Warfare_2_.LoadImage("pelaaja1rpg1");
    public static Image pelaaja1rpgKuva2 = MW2_My_Warfare_2_.LoadImage("pelaaja1rpg2");
    public static Image pelaaja1magnumKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1magnum");
    public static Image pelaaja1nyrkkiKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1nyrkki1");
    public static Image pelaaja1vintorezKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1vintorez");
    public static Image pelaaja1smgKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1ppsh41");
    public static Image pelaaja1sarjahaulikkoKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1aa12");
    public static Image pelaaja1liekinheitinKuva = MW2_My_Warfare_2_.LoadImage("pelaaja1liekinheitin");

    public static Image pelaaja2pistooliKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2pistooli");
    public static Image pelaaja2rynkkyKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2rynkky");
    public static Image pelaaja2haulikkoKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2haulikko1");
    public static Image pelaaja2minigunKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2SAW");
    public static Image pelaaja2snipaKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2dragunov");
    public static Image pelaaja2varsijousiKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2varsijousi1");
    public static Image pelaaja2rpgKuva1 = MW2_My_Warfare_2_.LoadImage("pelaaja2rpg1");
    public static Image pelaaja2rpgKuva2 = MW2_My_Warfare_2_.LoadImage("pelaaja2rpg2");
    public static Image pelaaja2magnumKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2magnum");
    public static Image pelaaja2nyrkkiKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2nyrkki1");
    public static Image pelaaja2vintorezKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2vintorez");
    public static Image pelaaja2smgKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2ppsh41");
    public static Image pelaaja2sarjahaulikkoKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2aa12");
    public static Image pelaaja2liekinheitinKuva = MW2_My_Warfare_2_.LoadImage("pelaaja2liekinheitin");


    public static Image rpgAmmusKuva = MW2_My_Warfare_2_.LoadImage("rpgammus");
    public static Image varsijousenNuolenKuva = MW2_My_Warfare_2_.LoadImage("varsijousennuoli");
    public static Image haulikonHylsyKuva = MW2_My_Warfare_2_.LoadImage("haulikonhylsy_ammuttu");
    public static Image binaryAmmusKuva = MW2_My_Warfare_2_.LoadImage("binaryluoti");

    public const double KRANAATIN_MAKSIMI_DAMAGE = 30;
    public const double SINGON_MAKSIMI_DAMAGE = 45;

    /*
     * Luomisajat:
     * Pistooli: 0
     * Rynkky: 15
     * Haulikko: 30
     * Magnum: 45
     * SMG, Flare Gun: 60
     * LMG: 75
     * Snipa: 90
     * Vintorez: 120
     * Sarjahaulikko: 150
     * Sinko: 180
     * Therminator: 240
     */

    const double PISTOOLI_AIKA = 0;
    const double NYRKKI_AIKA = 0;
    const double RYNKKY_AIKA = 30;
    const double PUMPPUHAULIKKO_AIKA = 60;
    const double MAGNUM_AIKA = 90;
    const double SMG_AIKA = 120;
    const double FLAREGUN_AIKA = 135;
    const double LMG_AIKA = 150;
    const double SNIPA_AIKA = 180;
    const double VINTOREZ_AIKA = 210;
    const double SARJAHAULIKKO_AIKA = 240;
    const double SINKO_AIKA = 270;
    const double THERMINATOR_AIKA = 300;

    /// <summary>
    /// Luodaan pistooli.
    /// </summary>
    /// <returns>Pistooli.</returns>
    public static Ase LuoPistooli()
    {
        Ase pistooli = new Ase(30, 10);
        pistooli.aikaJolloinVoiLuoda = PISTOOLI_AIKA;

        pistooli.Ammo.Value = 20;
        pistooli.Ammo.MaxValue = 20;
        pistooli.MaxAmmo = new IntMeter(350);
        pistooli.MaxAmmo.MaxValue = 350;
       
        pistooli.Power.DefaultValue = 550; // 400
        pistooli.Power.Value = 550;
        
        pistooli.TuhovoimaElaviaVastaan = 1.5; //2.5
        pistooli.TuhovoimaTuhoutuviaVastaan = 0.5; //0.25

        pistooli.LataukseenKuluvaAika = 1.0;
        pistooli.FireRate = 16; // 12
        pistooli.MaxAmmoLifetime = TimeSpan.FromSeconds(1.0);
        pistooli.AseenHajoama = new Vector(-40.0, 40.0); // 25      
        
        pistooli.Tag = "pistooli";
        pistooli.AseenNimi = "Glock 18";
        
        pistooli.LatausAnimaatio1 = MW2_My_Warfare_2_.LoadImages("pelaaja1pistooli", "pelaaja1pistooli1", "pelaaja1pistooli2", "pelaaja1pistooli3", "pelaaja1pistooli4", "pelaaja1pistooli3", "pelaaja1pistooli2", "pelaaja1pistooli1", "pelaaja1pistooli5", "pelaaja1pistooli6", "pelaaja1pistooli5", "pelaaja1pistooli");
        pistooli.LatausAnimaatio2 = MW2_My_Warfare_2_.LoadImages("pelaaja2pistooli", "pelaaja2pistooli1", "pelaaja2pistooli2", "pelaaja2pistooli3", "pelaaja2pistooli4", "pelaaja2pistooli3", "pelaaja2pistooli2", "pelaaja2pistooli1", "pelaaja2pistooli5", "pelaaja2pistooli6", "pelaaja2pistooli5", "pelaaja2pistooli");
        pistooli.Pelaajan1AseKuva = pelaaja1pistooliKuva;
        pistooli.Pelaajan2AseKuva = pelaaja2pistooliKuva;
        pistooli.AttackSound = pistoolinAani;

        pistooli.IsVisible = false;
        pistooli.UsesTracers = true;
        return pistooli;
    }

    /// <summary>
    /// Luodaan rynkky.
    /// </summary>
    /// <returns>Rynkky.</returns>
    public static Ase LuoRynkky()
    {
        Ase rynkky = new Ase(30, 10);
        rynkky.aikaJolloinVoiLuoda = RYNKKY_AIKA;

        rynkky.Ammo.MaxValue = 30;
        rynkky.MaxAmmo = new IntMeter(60); // 120
        rynkky.MaxAmmo.MaxValue = 60;

        rynkky.Power.Value = 700; //450
        rynkky.Power.DefaultValue = 700;

        rynkky.TuhovoimaTuhoutuviaVastaan = 2.5;
        rynkky.TuhovoimaElaviaVastaan = 5;

        rynkky.LataukseenKuluvaAika = 1.5;
        rynkky.FireRate = 10.8; // 8
        rynkky.MaxAmmoLifetime = TimeSpan.FromSeconds(1.5);
        rynkky.AseenHajoama = new Vector(-20.0, 20.0); // 15
        rynkky.AseenLapaisy = 0.01;

        rynkky.Tag = "rynkky";
        rynkky.AseenNimi = "AK-74M";
       
        rynkky.LatausAnimaatio1 = MW2_My_Warfare_2_.LoadImages("pelaaja1rynkky", "pelaaja1rynkky1", "pelaaja1rynkky2", "pelaaja1rynkky3", "pelaaja1rynkky4", "pelaaja1rynkky5", "pelaaja1rynkky6", "pelaaja1rynkky7", "pelaaja1rynkky8", "pelaaja1rynkky1", "pelaaja1rynkky9", "pelaaja1rynkky10", "pelaaja1rynkky9", "pelaaja1rynkky");
        rynkky.LatausAnimaatio2 = MW2_My_Warfare_2_.LoadImages("pelaaja2rynkky", "pelaaja2rynkky1", "pelaaja2rynkky2", "pelaaja2rynkky3", "pelaaja2rynkky4", "pelaaja2rynkky5", "pelaaja2rynkky6", "pelaaja2rynkky7", "pelaaja2rynkky8", "pelaaja2rynkky1", "pelaaja2rynkky9", "pelaaja2rynkky10", "pelaaja2rynkky9", "pelaaja2rynkky");
        rynkky.Pelaajan1AseKuva = pelaaja1rynkkyKuva;
        rynkky.Pelaajan2AseKuva = pelaaja2rynkkyKuva;
        rynkky.AttackSound = rynkynAani;

        rynkky.IsVisible = false;
        rynkky.UsesTracers = true;

        return rynkky;
    }

    /// <summary>
    /// Luodaan minigun.
    /// </summary>
    /// <returns>Minigun.</returns>
    public static Ase LuoMinigun()
    {
        Ase minigun = new Ase(30, 10);
        minigun.TuhovoimaTuhoutuviaVastaan = 1.0;//0.2
        minigun.TuhovoimaElaviaVastaan = 3.5; // 2.0
        minigun.AseenHajoama = new Vector(-60.0, 60.0); // (-25.0, 25.0)
        minigun.aikaJolloinVoiLuoda = LMG_AIKA;
        minigun.Ammo.Value = 200;
        minigun.Ammo.MaxValue = 200;
        minigun.AttackSound = minigunAani;
        minigun.MaxAmmo = new IntMeter(200);
        minigun.MaxAmmo.MaxValue = 200;
        minigun.Power.Value = 900; // 450
        minigun.Power.DefaultValue = 900;
        minigun.LataukseenKuluvaAika = 5.0;
        minigun.FireRate = 16.7; // 30
        minigun.IsVisible = false;
        minigun.MaxAmmoLifetime = TimeSpan.FromSeconds(1.0);
        minigun.Tag = "minigun";
        minigun.AseenNimi = "M249 SAW";
        minigun.Pelaajan1AseKuva = pelaaja1minigunKuva;
        minigun.Pelaajan2AseKuva = pelaaja2minigunKuva;
        minigun.UsesTracers = true;
        minigun.TracerBrightness = 2.0;
        minigun.TracerLength = 0.4;
        return minigun;
    }

    /// <summary>
    /// Luodaan snipa.
    /// </summary>
    /// <returns>Snipa.</returns>
    public static Ase LuoSnipa()
    {
        Ase snipa = new Ase(30, 10);
        snipa.Ammo.MaxValue = 5;
        snipa.MaxAmmo = new IntMeter(10);
        snipa.MaxAmmo.MaxValue = 10;
        snipa.FireRate = 1.0;
        snipa.AttackSound = snipanAani;
        snipa.TuhovoimaTuhoutuviaVastaan = 10;
        snipa.TuhovoimaElaviaVastaan = 20;
        snipa.LataukseenKuluvaAika = 2.0;
        snipa.AseenHajoama = new Vector(-3.0, 3.0);
        snipa.aikaJolloinVoiLuoda = SNIPA_AIKA;
        snipa.Power.Value = 800;
        snipa.Power.DefaultValue = 800;
        snipa.IsVisible = false;
        snipa.MaxAmmoLifetime = TimeSpan.FromSeconds(5);
        snipa.Tag = "snipa";
        snipa.AseenNimi = "SVD Dragunov";
        snipa.Pelaajan1AseKuva = pelaaja1snipaKuva;
        snipa.Pelaajan2AseKuva = pelaaja2snipaKuva;
        snipa.UsesTracers = true;
        snipa.TracerBrightness = 3.0;
        snipa.TracerLength = 1.0;
        snipa.AseenLapaisy = 0.1;
        return snipa;
    }

    /// <summary>
    /// Luodaan sinko.
    /// </summary>
    /// <returns>Sinko.</returns>
    public static Ase LuoSinko()
    {
        Ase sinko = new Ase(30, 10);
        sinko.AttackSound = singonLaukausAani;
        sinko.Ammo.MaxValue = 1;//2
        sinko.MaxAmmo = new IntMeter(2);
        sinko.MaxAmmo.MaxValue = 2;
        sinko.Power.Value = 450;
        sinko.Power.DefaultValue = 450;
        sinko.LataukseenKuluvaAika = 2.0;
        sinko.AseenHajoama = new Vector(-10.0, 10.0);
        sinko.IsVisible = false;
        sinko.FireRate = 0.5; // 0.5
        sinko.MaxAmmoLifetime = TimeSpan.FromSeconds(5);
        sinko.ProjectileCollision = delegate(PhysicsObject ammus, PhysicsObject kohde) { MW2_My_Warfare_2_.Peli.SingonAmmusRajahtaa(ammus, kohde, sinko); };
        sinko.aikaJolloinVoiLuoda = SINKO_AIKA;
        sinko.Tag = "sinko";
        sinko.AseenNimi = "RPG-7";
        sinko.luodinKuva = rpgAmmusKuva;
        sinko.TuleekoHylsya = false;
        sinko.Pelaajan1AseKuva = pelaaja1rpgKuva1;
        sinko.Pelaajan2AseKuva = pelaaja2rpgKuva1;
        sinko.IsIncendiary = true;
        sinko.IgnitionChance = 50;
        return sinko;
    }

    /// <summary>
    /// Luodaan haulikko.
    /// </summary>
    /// <returns>Haulikko.</returns>
    public static Ase LuoHaulikko()
    {
        Ase haulikko = new Ase(30, 10);
        haulikko.Ammo.MaxValue = 5;
        haulikko.Power.Value = 450; //350
        haulikko.Power.DefaultValue = 450;
        haulikko.AseenHajoama = new Vector(-40.0, 40.0);
        haulikko.MaxAmmoLifetime = TimeSpan.FromSeconds(0.25);
        haulikko.TuhovoimaElaviaVastaan = 4;
        haulikko.TuhovoimaTuhoutuviaVastaan = 2;
        haulikko.LataukseenKuluvaAika = 3.5;
        haulikko.MaxAmmo = new IntMeter(60);
        haulikko.MaxAmmo.MaxValue = 60;
        haulikko.hylsynKuva = haulikonHylsyKuva;
        haulikko.FireRate = 1;
        haulikko.BulletsPerShot = 10;
        haulikko.IsVisible = false;
        haulikko.Tag = "haulikko";
        haulikko.AseenNimi = "Remington 870";
        haulikko.AttackSound = haulikkoAani;
        haulikko.aikaJolloinVoiLuoda = PUMPPUHAULIKKO_AIKA;
        haulikko.Pelaajan1AseKuva = pelaaja1haulikkoKuva;
        haulikko.Pelaajan2AseKuva = pelaaja2haulikkoKuva;
        haulikko.TuleekoHylsya = false;
        haulikko.UsesTracers = true;
        return haulikko;
    }

    /// <summary>
    /// Luodaan nyrkki-meleease.
    /// </summary>
    /// <returns>Nyrkki.</returns>
    public static Ase LuoNyrkki()
    {
        Ase nyrkki = new Ase(30, 10);
        nyrkki.Ammo.MaxValue = Int32.MaxValue;
        nyrkki.Power.Value = 450;
        nyrkki.InfiniteAmmo = true;
        nyrkki.Power.DefaultValue = 450;
        nyrkki.AseenHajoama = new Vector(0.0, 0.0);
        nyrkki.MaxAmmoLifetime = TimeSpan.FromSeconds(0.035); // 0.035
        nyrkki.TuhovoimaElaviaVastaan = 10;
        nyrkki.TuhovoimaTuhoutuviaVastaan = 10;
        nyrkki.LataukseenKuluvaAika = 1;
        nyrkki.AttackSound = nyrkkiAani;
        nyrkki.MaxAmmo = new IntMeter(Int32.MaxValue);
        nyrkki.MaxAmmo.MaxValue = Int32.MaxValue;
        nyrkki.FireRate = 1; // 1
        nyrkki.IsVisible = false;
        nyrkki.Tag = "nyrkki";
        nyrkki.AseenNimi = "Luuvitonen";
        nyrkki.aikaJolloinVoiLuoda = NYRKKI_AIKA;
        nyrkki.OnkoMeleeAse = true;
        nyrkki.Pelaajan1AseKuva = pelaaja1nyrkkiKuva;
        nyrkki.Pelaajan2AseKuva = pelaaja2nyrkkiKuva;
        nyrkki.HyokkaysAnimaatio1 = new Animation(MW2_My_Warfare_2_.LoadImages("pelaaja1nyrkki1", "pelaaja1nyrkki2", "pelaaja1nyrkki3", "pelaaja1nyrkki4", "pelaaja1nyrkki3", "pelaaja1nyrkki2", "pelaaja1nyrkki1"));
        nyrkki.HyokkaysAnimaatio2 = new Animation(MW2_My_Warfare_2_.LoadImages("pelaaja2nyrkki1", "pelaaja2nyrkki2", "pelaaja2nyrkki3", "pelaaja2nyrkki4", "pelaaja2nyrkki3", "pelaaja2nyrkki2", "pelaaja2nyrkki1"));
        return nyrkki;
    }

    /// <summary>
    /// Luodaan varsijousi.
    /// </summary>
    /// <returns>Varsijousi.</returns>
    public static Ase LuoVarsijousi()
    {
        Ase varsijousi = new Ase(30, 10);
        varsijousi.Ammo.MaxValue = 1; // 1
        varsijousi.Power.Value = 400;
        varsijousi.Power.DefaultValue = 400;
        varsijousi.AseenHajoama = new Vector(0.0, 0.0);
        varsijousi.MaxAmmoLifetime = TimeSpan.FromSeconds(60); //60
        varsijousi.TuhovoimaElaviaVastaan = 20;
        varsijousi.TuhovoimaTuhoutuviaVastaan = 0;
        varsijousi.LataukseenKuluvaAika = 2;
        varsijousi.AttackSound = null; 
        varsijousi.MaxAmmo = new IntMeter(5);
        varsijousi.MaxAmmo.MaxValue = 10;
        varsijousi.FireRate = 1; //1
        varsijousi.IsVisible = false;
        varsijousi.Tag = "varsijousi";
        varsijousi.AseenNimi = "Varsijousi";
        varsijousi.OnkoMeleeAse = false;
        varsijousi.HaviaakoAmmusOsumasta = false;
        varsijousi.TuleekoHylsya = false;
        varsijousi.luodinKuva = varsijousenNuolenKuva;
        varsijousi.CanHitOwner = false;
        varsijousi.Pelaajan1AseKuva = pelaaja1varsijousiKuva;
        varsijousi.Pelaajan2AseKuva = pelaaja2varsijousiKuva;
        varsijousi.aikaJolloinVoiLuoda = 75;
        return varsijousi;
    }

    public static Ase LuoMagnum()
    {
        Ase magnum = new Ase(30, 10);
        magnum.aikaJolloinVoiLuoda = MAGNUM_AIKA;
        magnum.Ammo.Value = 6;
        magnum.Ammo.MaxValue = 6;
        magnum.IsVisible = false;
        magnum.Power.DefaultValue = 600;
        magnum.Power.Value = 600;
        magnum.AttackSound = magnumAani;
        magnum.TuhovoimaElaviaVastaan = 12;
        magnum.TuhovoimaTuhoutuviaVastaan = 5;
        magnum.LataukseenKuluvaAika = 3.0;
        magnum.AseenHajoama = new Vector(-10.0, 10.0);
        magnum.FireRate = 7;
        magnum.MaxAmmoLifetime = TimeSpan.FromSeconds(2.0);
        magnum.Tag = "magnum";
        magnum.AseenNimi = ".44 Magnum";
        magnum.MaxAmmo = new IntMeter(50);
        magnum.MaxAmmo.MaxValue = 50;
        magnum.Pelaajan1AseKuva = pelaaja1magnumKuva;
        magnum.Pelaajan2AseKuva = pelaaja2magnumKuva;
        magnum.TuleekoHylsya = false;
        magnum.UsesTracers = true;
        magnum.TracerBrightness = 2.0;
        return magnum;
    }

    public static Ase LuoOhjus()
    {
        Ase ohjus = new Ase(30, 10);
        ohjus.aikaJolloinVoiLuoda = THERMINATOR_AIKA;
        ohjus.Ammo.Value = 1;
        ohjus.Ammo.MaxValue = 1;
        ohjus.IsVisible = false;
        ohjus.Power.DefaultValue = 300;
        ohjus.Power.Value = 300;
        ohjus.AttackSound = singonLaukausAani;
        ohjus.LataukseenKuluvaAika = 3.0;
        ohjus.AseenHajoama = new Vector(-10.0, 10.0);
        ohjus.FireRate = 0.5; //0.25
        ohjus.MaxAmmoLifetime = TimeSpan.FromSeconds(10.0);
        ohjus.Tag = "ohjus";
        ohjus.AseenNimi = "Therminator";
        ohjus.MaxAmmo = new IntMeter(3);
        ohjus.MaxAmmo.MaxValue = 3;
        ohjus.Pelaajan1AseKuva = pelaaja1rpgKuva1;
        ohjus.Pelaajan2AseKuva = pelaaja2rpgKuva1;
        ohjus.TuleekoHylsya = false;
        ohjus.UsesTracers = true;
        ohjus.TracerBrightness = 3.0;
        ohjus.TracerLength = 0.5;
        ohjus.IsHoming = true;
        ohjus.ProjectileCollision = delegate(PhysicsObject ammus, PhysicsObject kohde) { MW2_My_Warfare_2_.Peli.SingonAmmusRajahtaa(ammus, kohde, ohjus); };
        ohjus.IsIncendiary = true;
        ohjus.IgnitionChance = 20;
        return ohjus;
    }

    public static Ase LuoBinaryRifle()
    {
        Ase binaryrifle = new Ase(30, 10);

        binaryrifle.Ammo.MaxValue = 2;
        binaryrifle.Ammo.Value = 2;
        binaryrifle.MaxAmmo = new IntMeter(12);
        binaryrifle.MaxAmmo.MaxValue = 12;

        binaryrifle.LataukseenKuluvaAika = 2.0;
        binaryrifle.FireRate = 1.0;
        binaryrifle.AttackSound = snipanAani;
        binaryrifle.TuhovoimaTuhoutuviaVastaan = 50;
        binaryrifle.TuhovoimaElaviaVastaan = 50;
        binaryrifle.Power.Value = 2000;
        binaryrifle.Power.DefaultValue = 2000;
        binaryrifle.MaxAmmoLifetime = TimeSpan.FromSeconds(5);
        binaryrifle.AseenHajoama = new Vector(0.0, 0.0);

        binaryrifle.aikaJolloinVoiLuoda = Int32.MaxValue;//90
        binaryrifle.IsVisible = false;
        
        binaryrifle.Tag = "binaryrifle";
        binaryrifle.AseenNimi = "Binary Rifle";
        binaryrifle.Pelaajan1AseKuva = pelaaja1snipaKuva;
        binaryrifle.Pelaajan2AseKuva = pelaaja2snipaKuva;
        binaryrifle.luodinKuva = binaryAmmusKuva;

        binaryrifle.UsesTracers = true;
        binaryrifle.TracerBrightness = 3.0;
        binaryrifle.TracerLength = 1.0;
        binaryrifle.OverrideTracerColor = Color.OrangeRed;
        return binaryrifle;
    }

    public static Ase LuoVintorez()
    {
        Ase vintorez = new Ase(30, 10);
        vintorez.IsVisible = false;
        vintorez.aikaJolloinVoiLuoda = VINTOREZ_AIKA;
        vintorez.FireRate = 15;
        vintorez.TuhovoimaTuhoutuviaVastaan = 2;
        vintorez.TuhovoimaElaviaVastaan = 6;
        vintorez.AseenHajoama = new Vector(-2.0, 2.0);
        vintorez.Power.Value = 600;
        vintorez.Power.DefaultValue = 600;
        vintorez.LataukseenKuluvaAika = 1.0;
        vintorez.AttackSound = vintorezAani;
        vintorez.Ammo.MaxValue = 10;
        vintorez.MaxAmmo = new IntMeter(60);
        vintorez.MaxAmmo.MaxValue = 60;
        vintorez.MaxAmmoLifetime = TimeSpan.FromSeconds(3);
        vintorez.Tag = "vintorez";
        vintorez.AseenNimi = "VSS Vintorez";
        vintorez.Pelaajan1AseKuva = pelaaja1vintorezKuva;
        vintorez.Pelaajan2AseKuva = pelaaja2vintorezKuva;
        vintorez.UsesTracers = true;
        vintorez.TracerLength = 0.3;
        vintorez.AseenLapaisy = 0.01;
        return vintorez;
    }

    /// <summary>
    /// Luodaan haulikko.
    /// </summary>
    /// <returns>Haulikko.</returns>
    public static Ase LuoAutomaattiHaulikko()
    {
        Ase automaattihaulikko = new Ase(30, 10);
        automaattihaulikko.Ammo.MaxValue = 8;
        automaattihaulikko.Power.Value = 450; //350
        automaattihaulikko.Power.DefaultValue = 450;
        automaattihaulikko.AseenHajoama = new Vector(-60.0, 60.0); //60
        automaattihaulikko.MaxAmmoLifetime = TimeSpan.FromSeconds(0.45);
        automaattihaulikko.TuhovoimaElaviaVastaan = 1;
        automaattihaulikko.TuhovoimaTuhoutuviaVastaan = 1;
        automaattihaulikko.LataukseenKuluvaAika = 1.5; // 2.5
        automaattihaulikko.MaxAmmo = new IntMeter(60);
        automaattihaulikko.MaxAmmo.MaxValue = 60;
        automaattihaulikko.hylsynKuva = haulikonHylsyKuva;
        automaattihaulikko.FireRate = 5;
        automaattihaulikko.BulletsPerShot = 10;
        automaattihaulikko.IsVisible = false;
        automaattihaulikko.Tag = "automaattihaulikko";
        automaattihaulikko.AseenNimi = "AA-12";
        automaattihaulikko.AttackSound = haulikkoAani;
        automaattihaulikko.aikaJolloinVoiLuoda = SARJAHAULIKKO_AIKA;
        automaattihaulikko.Pelaajan1AseKuva = pelaaja1sarjahaulikkoKuva;
        automaattihaulikko.Pelaajan2AseKuva = pelaaja2sarjahaulikkoKuva;
        automaattihaulikko.TuleekoHylsya = false;
        return automaattihaulikko;
    }

    public static Ase LuoSMG()
    {
        Ase smg = new Ase(30, 10);
        smg.aikaJolloinVoiLuoda = SMG_AIKA;
        smg.Ammo.Value = 71;
        smg.Ammo.MaxValue = 71;
        smg.IsVisible = false;
        smg.Power.DefaultValue = 550;
        smg.Power.Value = 550;
        smg.AttackSound = pistoolinAani;
        smg.TuhovoimaElaviaVastaan = 1.5; //2.5
        smg.TuhovoimaTuhoutuviaVastaan = 0.5; //0.25
        smg.LataukseenKuluvaAika = 1.5;
        smg.AseenHajoama = new Vector(-30.0, 30.0);
        smg.FireRate = 30;
        smg.MaxAmmoLifetime = TimeSpan.FromSeconds(1.2);
        smg.Tag = "smg";
        smg.AseenNimi = "PPSH-41";
        smg.MaxAmmo = new IntMeter(355);
        smg.MaxAmmo.MaxValue = 355;
        smg.Pelaajan1AseKuva = pelaaja1smgKuva;
        smg.Pelaajan2AseKuva = pelaaja2smgKuva;
        smg.UsesTracers = true;
        return smg;
    }

    public static Ase LuoFlareGun()
    {
        Ase flaregun = new Ase(30, 10);
        flaregun.aikaJolloinVoiLuoda = FLAREGUN_AIKA;
        flaregun.Ammo.Value = 1;
        flaregun.Ammo.MaxValue = 1;
        flaregun.IsVisible = false;
        flaregun.Power.DefaultValue = 350;
        flaregun.Power.Value = 350;
        flaregun.AttackSound = flaregunAani;
        flaregun.TuhovoimaElaviaVastaan = 1;
        flaregun.TuhovoimaTuhoutuviaVastaan = 1;
        flaregun.LataukseenKuluvaAika = 1.5;
        flaregun.AseenHajoama = new Vector(-15.0, 15.0);
        flaregun.FireRate = 1;
        flaregun.MaxAmmoLifetime = TimeSpan.FromSeconds(1.5);
        flaregun.Tag = "flaregun";
        flaregun.AseenNimi = "Flare Gun";
        flaregun.MaxAmmo = new IntMeter(6);
        flaregun.MaxAmmo.MaxValue = 6;
        flaregun.Pelaajan1AseKuva = pelaaja1pistooliKuva;
        flaregun.Pelaajan2AseKuva = pelaaja2pistooliKuva;
        /*flaregun.UsesTracers = true;
        flaregun.OverrideTracerColor = Color.OrangeRed;
        flaregun.TracerLength = 0.2;*/
        flaregun.IsIncendiary = true;
        flaregun.IgnitionChance = 100; // koska se on tarkoitettu sytyttämiseen
        flaregun.TuleekoHylsya = false;
        return flaregun;
    }

    public static Ase LuoLiekinheitin()
    {
        Ase liekinheitin = new Ase(30, 10);
        liekinheitin.TuhovoimaTuhoutuviaVastaan = 0.1;
        liekinheitin.TuhovoimaElaviaVastaan = 0.1;
        liekinheitin.AseenHajoama = new Vector(-40.0, 40.0);
        liekinheitin.aikaJolloinVoiLuoda = 75;
        liekinheitin.Ammo.Value = 200;
        liekinheitin.Ammo.MaxValue = 200;
        liekinheitin.AttackSound = null;
        liekinheitin.MaxAmmo = new IntMeter(200);
        liekinheitin.MaxAmmo.MaxValue = 200;
        liekinheitin.Power.Value = 200; 
        liekinheitin.Power.DefaultValue = 200;
        liekinheitin.LataukseenKuluvaAika = 5.0;
        liekinheitin.FireRate = 25; // 30
        liekinheitin.IsVisible = false;
        liekinheitin.MaxAmmoLifetime = TimeSpan.FromSeconds(0.3);
        liekinheitin.Tag = "liekinheitin";
        liekinheitin.AseenNimi = "Flamethrower";
        liekinheitin.Pelaajan1AseKuva = pelaaja1liekinheitinKuva;
        liekinheitin.Pelaajan2AseKuva = pelaaja2liekinheitinKuva;
        liekinheitin.UsesTracers = false;
        liekinheitin.IsIncendiary = true;
        liekinheitin.IgnitionChance = 10;
        liekinheitin.TuleekoHylsya = false;
        liekinheitin.NakyykoAmmus = false;
        liekinheitin.TuleekoOsumastaEfektiä = false;
        liekinheitin.KuulukoOsumastaAanta = false;
        return liekinheitin;
    }
}