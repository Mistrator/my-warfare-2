using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Veriefektit maassa.
/// </summary>
public class Blood
{
    static Image[] NormalBloodImages = MW2_My_Warfare_2_.LoadImages("Blood1", "Blood2", "Blood3", "Blood4", "Blood5");
    static Image[] DeathSplatterBloodImages = MW2_My_Warfare_2_.LoadImages("Ultisplat");

    /// <summary>
    /// Lisätään verta kentälle.
    /// </summary>
    /// <param name="position">Paikka.</param>
    /// <param name="amount">Määrä.</param>
    /// <param name="scale">Koko. 1.0 on normaali, suurempi kasvattaa ja pienempi pienentää.</param>
    public static GameObject[] AddNormalBlood(Vector position, int amount, double scale)
    {
        GameObject[] bloodSplatters = new GameObject[amount];
        for (int i = 0; i < amount; i++)
        {
            GameObject blood = new GameObject(RandomGen.SelectOne<Image>(NormalBloodImages));
            blood.Position = position += RandomGen.NextVector(0.0, 30.0);
            blood.Angle = RandomGen.NextAngle();
            blood.Size *= scale;
            MW2_My_Warfare_2_.Peli.Add(blood, -3);
            bloodSplatters[i] = blood;
        }
        return bloodSplatters;
    }

    /// <summary>
    /// Lisätään kuolemaefekti kentälle.
    /// </summary>
    /// <param name="position">Paikka.</param>
    /// <param name="amount">Määrä.</param>
    /// <param name="scale">Koko. 1.0 on normaali, suurempi kasvattaa ja pienempi pienentää.</param>
    public static GameObject[] AddDeathSplatter(Vector position, int amount, double scale)
    {
        GameObject[] deathSplatters = new GameObject[amount];
        for (int i = 0; i < amount; i++)
        {
            GameObject deathsplatter = new GameObject(RandomGen.SelectOne<Image>(DeathSplatterBloodImages));
            deathsplatter.Position = position += RandomGen.NextVector(0.0, 10.0);
            deathsplatter.Size *= scale;
            deathsplatter.Angle = RandomGen.NextAngle();
            MW2_My_Warfare_2_.Peli.Add(deathsplatter, -3);
            deathSplatters[i] = deathsplatter;
        }
        return deathSplatters;
    }
}