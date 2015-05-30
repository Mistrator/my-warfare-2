using System;
using System.Collections;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Effects;
using Jypeli.GameObjects;

/// @author Miska Kananen
/// @version 22.2.2013
/// 
/// <summary>
/// Game of Life.
/// </summary>
public class GameOfLife : PhysicsGame
{
    private const int NY = 60;
    private static int[,] sukupolvi;
    private static int[,] seuraavaSukupolvi;

    public static int[,] ArvoSukupolvia(int[,] lähtötilanne, int rivejä, int sarakkeita, int arvottaviaSukupolvia)
    {
        sukupolvi = lähtötilanne; //  Luodaan taulukot
        seuraavaSukupolvi = new int[rivejä, sarakkeita];
        ArvoSukupolvi();
        for (int i = 0; i < arvottaviaSukupolvia; i++)
        {
            LaskeSeuraavaSukupolvi();
        }
        return sukupolvi;
    }

    /// <summary>
    /// Arvotaan ensimmäinen sukupolvi.
    /// </summary>
    private static void ArvoSukupolvi()
    {
        Sopulit.Arvo(sukupolvi, 0, 1);
    }

    /// <summary>
    /// Lasketaan seuraava sukupolvi ja päivitetään neliöiden värit.
    /// </summary>
    private static void LaskeSeuraavaSukupolvi()
    {
        Sopulit.SeuraavaSukupolvi(sukupolvi, seuraavaSukupolvi);
        sukupolvi = seuraavaSukupolvi;
    }
}