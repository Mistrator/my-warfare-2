using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Luokka pelin vakioiden helppoa käyttöä varten.
/// </summary>
public abstract class Vakiot
{
    #region Peli

    public static readonly TimeSpan MINIMUM_FPS = TimeSpan.FromSeconds(1.0 / 59.0); // pyritään pyörittämään peliä min 30 fps

    public const int PELAAJIEN_MAARA = 2;
    public const double PELAAJIEN_ELAMIEN_MAARA = 20.0;
    public const double PELAAJAN_ELAMIEN_REGENEROITUMISVAUHTI = 3.0;
    public const double PELAAJAN_RESPAUS_AIKA = 2.0;
    public const double PELAAJAN_MINIMIRESPAUSETAISYYS_KUOLEMAPAIKASTA = 200.0;
    public const int PELAAJAN_KRANAATTIEN_OLETUSMAARA = 3;
    public const double PELAAJAN_VASYMIS_NOPEUS = 2.0;
    public const double PELAAJAN_KUNNON_PALAUTUMIS_NOPEUS = 1;
    public const double PELAAJAN_PYORIMIS_NOPEUS_OHJAIMELLA = 2.0;
    public const double OHJAIMEN_DEAD_ZONE_ASTEINA = 2.0;

    //public static readonly Vector[] PELAAJIEN_VAIHTOEHTOISET_SPAWNAUSPAIKAT = { new Vector(0, -600), new Vector(0, 700), new Vector(-1030, 1450), new Vector(1030, 1450) };
          
    public const int PARTIKKELEIDEN_MAX_MAARA = 4000;
    public const int TEHOSTEOBJEKTIEN_MAX_MAARA = 750;
    public const int SIRPALEIDEN_MAX_MAARA = 200;
    public const int PICKUPIEN_MAARA = 20;
    public const int KENTAN_LEVEYS = 60;
    public const int KENTAN_KORKEUS = 60;
    public const int KENTAN_RUUDUN_LEVEYS = 50;
    public const int KENTAN_RUUDUN_KORKEUS = 50;
    public const int KIELLETYN_ALUEEN_LEVEYS_KENTAN_REUNOILLA = 10;
    public const double PELAAJAN_SUURIN_SALLITTU_X_KENTALLA = (KENTAN_RUUDUN_LEVEYS * (KENTAN_LEVEYS - KIELLETYN_ALUEEN_LEVEYS_KENTAN_REUNOILLA)) / 2;
    public const double PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA = (KENTAN_RUUDUN_KORKEUS * (KENTAN_KORKEUS - KIELLETYN_ALUEEN_LEVEYS_KENTAN_REUNOILLA)) / 2;
    public const double PELAAJAN_PIENIN_SALLITTU_X_KENTALLA = -PELAAJAN_SUURIN_SALLITTU_X_KENTALLA;
    public const double PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA = -PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA;
    public const int TULEN_LEVIAMIS_ETAISYYS = 100; // 100
    public const double TULEN_DAMAGE_ELAVIA_VASTAAN = 0.4;
    public const string PIIKKILANKA_TAG = "piikkilanka"; // pelaajille kiinteä, zombit menevät läpi
    public const double SHATTER_SIZE = 10;
    public static readonly TimeSpan ENEMY_PART_LIFETIME = TimeSpan.FromSeconds(1.0);

    #endregion

    #region Infinite

    public const double SPAWNAUS_VAUHTI = 4.0; // 4.0
    public const int RESPAUSTEN_OLETUSMAARA = 5;
    public const int VIHOLLISTEN_OLETUS_MAX_MAARA_SPAWNAUSKIERROSTA_KOHTI = 3; // 3
    public const double PALJONKO_VIHOLLISET_SPAWNAAVAT_KENTAN_RAJOJEN_ULKOPUOLELLE = 100.0;
    public const int VIHOLLISTEN_MAX_MAARA_PELISSA = 30; // 30

    public const double AI_REFRESH_RATE = 1.0;
    public const double DISTANCE_TO_REFRESH_ROUTE = 750.0; // jos pelaaja tätä kauempana dijkstra-reitin päästä, lasketaan uusi reitti

    // miten jaksotetaan jonossa olevat reitinhakupyynnöt
    public const double DIJKSTRA_PATHFINDING_REFRESH_RATE = 1 / 3.0;
    public const int AI_PATHFINDING_UPDATES_PER_TICK = 4;

    #endregion

    #region Survival

    public const double KAMAN_SPAWNAUS_VAUHTI = 2.0;
    public const double KAMAN_MAX_MAARA = 25;

    #endregion
}