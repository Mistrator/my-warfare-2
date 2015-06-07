using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

public class InfiniteMapGenTile
{
    public Image Tile { get; private set; }

    public bool HasTopExit { get; private set; }
    public bool HasBottomExit { get; private set; }
    public bool HasLeftExit { get; private set; }
    public bool HasRightExit { get; private set; }

    private int teStart;
    private int teEnd;
    private int beStart;
    private int beEnd;
    private int leStart;
    private int leEnd;
    private int reStart;
    private int reEnd;

    public bool HasPlayer1Spawn { get; private set; }
    public bool HasPlayer2Spawn { get; private set; }
    public bool HasAlternatePlayerSpawn { get; private set; }

    public bool HasEnemySpawn { get; private set; }

    public bool HasWeaponCrate { get; private set; }

    public InfiniteMapGenTile(Image img, int topExitStart, int topExitEnd, int bottomExitStart, int bottomExitEnd, int leftExitStart, int leftExitEnd, int rightExitStart, int rightExitEnd)
    {
        Tile = img;

        teStart = topExitStart;
        teEnd = topExitEnd;
        beStart = bottomExitStart;
        beEnd = bottomExitEnd;
        leStart = leftExitStart;
        leEnd = leftExitEnd;
        reStart = rightExitStart;
        reEnd = rightExitEnd;

        AnalyzeImage(Tile);
    }

    private void AnalyzeImage(Image img)
    {
        Color[,] data = img.GetData();

        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                CheckForSpecialObjects(data[i, j]);
            }
        }

        CheckForExits(img);
    }

    /// <summary>
    /// Tutkii, onko ruudussa erityisesineitä tai spawnipaikkoja.
    /// </summary>
    /// <param name="c"></param>
    private void CheckForSpecialObjects(Color c)
    {
        if (c == Vakiot.PELAAJAN_1_SPAWNI)
            HasPlayer1Spawn = true;
        if (c == Vakiot.PELAAJAN_2_SPAWNI)
            HasPlayer2Spawn = true;
        if (c == Vakiot.PELAAJIEN_VAIHTOEHTOINEN_SPAWNI)
            HasAlternatePlayerSpawn = true;
        if (c == Vakiot.VIHOLLISTEN_FIXED_SPAWN)
            HasEnemySpawn = true;
        if (c == Vakiot.ASELAATIKKO)
            HasWeaponCrate = true;
    }

    /// <summary>
    /// Tutkii, onko ruudussa uloskäyntejä.
    /// </summary>
    /// <param name="img"></param>
    private void CheckForExits(Image kuva)
    {
        Color[,] img = RotateImage90ClockWise(kuva.GetData());

        HasTopExit = true;
        HasBottomExit = true;
        HasLeftExit = true;
        HasRightExit = true;

        for (int i = teStart; i <= teEnd; i++)
        {
            int j = 0;
            if (img[i, j] != Vakiot.TYHJA)
            {
                HasTopExit = false;
                break;
            }
        }

        for (int i = beStart; i <= beEnd; i++)
        {
            int j = img.GetLength(1) - 1;
            if (img[i, j] != Vakiot.TYHJA)
            {
                HasBottomExit = false;
                break;
            }
        }

        for (int j = leStart; j <= leEnd; j++)
        {
            int i = 0;
            if (img[i, j] != Vakiot.TYHJA)
            {
                HasLeftExit = false;
                break;
            }
        }

        for (int j = reStart; j <= reEnd; j++)
        {
            int i = img.GetLength(0) - 1;
            if (img[i, j] != Vakiot.TYHJA)
            {
                HasRightExit = false;
                break;
            }
        }
    }

    private Color[,] RotateImage90ClockWise(Color[,] img)
    {
        Color[,] result = new Color[img.GetLength(1), img.GetLength(0)];

        for (int i = 0; i < img.GetLength(0); i++)
        {
            for (int j = img.GetLength(1) - 1; j >= 0; j--)
            {
                result[i, result.GetLength(1) - 1 - j] = img[j, i];
            }
        }
        return result;
    }
}