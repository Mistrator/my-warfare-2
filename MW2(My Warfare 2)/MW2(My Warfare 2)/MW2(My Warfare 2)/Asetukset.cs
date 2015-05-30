using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Serialisoitava asetustiedosto.
/// </summary>
[Serializable]
public class Asetukset
{
    /// <summary>
    /// Onko peli fullscreenissa.
    /// </summary>
    public bool OnkoFullscreen { get; set; }

    /// <summary>
    /// Käytetäänkö antialiasingia.
    /// </summary>
    public bool OnkoAntialiasing { get; set; }

    /// <summary>
    /// Onko pelaajalla 1 lasertähtäintä.
    /// </summary>
    public bool OnkoPelaajalla1Lasertahtainta { get; set; }

    /// <summary>
    /// Onko pelaajalla 2 lasertähtäintä.
    /// </summary>
    public bool OnkoPelaajalla2Lasertahtainta { get; set; }

    /// <summary>
    /// Pelaajan online-käyttäjänimi.
    /// </summary>
    public String KayttajaNimi { get; set; }
}