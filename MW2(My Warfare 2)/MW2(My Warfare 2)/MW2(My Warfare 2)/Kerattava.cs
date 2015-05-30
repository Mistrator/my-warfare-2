using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Kentällä oleva kerättävä esine.
/// </summary>
class Kerattava : PhysicsObject
{
    public delegate void KeraysKasittelija(Pelaaja pelaaja, object mitaKerattiin);

    /// <summary>
    /// Kutsutaan, kun esineeseen törmätään.
    /// </summary>
    public event KeraysKasittelija Kerattiin;

    /// <summary>
    /// Mitä esineestä saa, kun se on kerätty.
    /// </summary>
    public object MitaSaaKerattyaan { get; set; }

    /// <summary>
    /// Mikä ääni kuuluu kerättäessä.
    /// </summary>
    private SoundEffect keraysAani = MW2_My_Warfare_2_.LoadSoundEffect("pickup_1");

    /// <summary>
    /// Luodaan uusi kerättävä esine.
    /// </summary>
    /// <param name="width">Esineen leveys.</param>
    /// <param name="height">Esineen korkeus.</param>
    /// <param name="MitaSaa">Mitä esineestä saa.</param>
    public Kerattava(double width, double height, object MitaSaa)
        :base(width, height)
    {
        this.MitaSaaKerattyaan = MitaSaa;
        this.Collided += KohdeKerattiin;
        this.IgnoresCollisionResponse = true;
    }

    /// <summary>
    /// Luodaan uusi kerättävä esine.
    /// </summary>
    /// <param name="width">Esineen leveys.</param>
    /// <param name="height">Esineen korkeus.</param>
    /// <param name="MitaSaa">Mitä esineestä saa.</param>
    /// <param name="paikka">Esineen paikka.</param>
    public Kerattava(double width, double height, object MitaSaa, Vector paikka)
        : base(width, height)
    {
        this.MitaSaaKerattyaan = MitaSaa;
        this.Collided += KohdeKerattiin;
        this.Position = paikka;
        this.Mass = 40;
        this.LinearDamping = 0.95;
        this.AngularVelocity = 0.5;
        this.Angle = Angle.FromDegrees(RandomGen.NextDouble(0, 360));
        this.IgnoresCollisionResponse = true;
    }

    /// <summary>
    /// Kutsutaan, kun jokin törmää kerättävään esineeseen.
    /// </summary>
    /// <param name="kerattava">Kerättävä esine.</param>
    /// <param name="tormaaja">Törmääjä.</param>
    private void KohdeKerattiin(IPhysicsObject kerattava, IPhysicsObject tormaaja)
    {
        if (tormaaja is Pelaaja)
        {
            Pelaaja p = tormaaja as Pelaaja;
            Kerattiin(p, MitaSaaKerattyaan);
            keraysAani.Play();
            this.Destroy();
        }
    }
}