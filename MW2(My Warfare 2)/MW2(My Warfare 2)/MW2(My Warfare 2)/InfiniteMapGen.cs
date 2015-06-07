using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

public class InfiniteMapGen
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }

    public int TilesHorizontal { get; set; }
    public int TilesVertical { get; set; }

    private InfiniteMapGenTile[,] generatedMap;

    private List<InfiniteMapGenTile> allTiles;

    public int MapWidth
    {
        get
        {
            return TileWidth * TilesHorizontal;
        }
    }

    public int MapHeight
    {
        get
        {
            return TileHeight * TilesVertical;
        }
    }

    /// <summary>
    /// Luo uusi satunnaisgeneraattori Infinite-karttoja varten.
    /// </summary>
    /// <param name="tileMaps">Ruudut, joista kenttä koostuu. Kaikkien pitää olla pikseleinä saman kokoisia.</param>
    /// <param name="tilesHorizontal">Montako ruutua vaakasuorassa</param>
    /// <param name="tilesVertical">Montako ruutua pystysuorassa</param>
    /// <param name="topExitStart">Yläuloskäynnin alkamispikselin indeksi (inclusive)</param>
    /// <param name="topExitEnd">Yläuloskäynnin loppumispikselin indeksi (inclusive)</param>
    /// <param name="bottomExitStart">Sama kuin edellinen</param>
    /// <param name="bottomExitEnd"></param>
    /// <param name="leftExitStart"></param>
    /// <param name="leftExitEnd"></param>
    /// <param name="rightExitStart"></param>
    /// <param name="rightExitEnd"></param>
    public InfiniteMapGen(Image[] tileMaps, int tilesHorizontal, int tilesVertical, 
        int topExitStart, int topExitEnd, int bottomExitStart, int bottomExitEnd, int leftExitStart, int leftExitEnd, int rightExitStart, int rightExitEnd)
    {
        if (tileMaps == null || tileMaps.Length == 0) return;

        TilesHorizontal = tilesHorizontal;
        TilesVertical = tilesVertical;

        TileWidth = tileMaps[0].Width;
        TileHeight = tileMaps[0].Height;


        allTiles = new List<InfiniteMapGenTile>();

        for (int i = 0; i < tileMaps.Length; i++)
        {
            allTiles.Add(new InfiniteMapGenTile(tileMaps[i], topExitStart, topExitEnd, bottomExitStart, bottomExitEnd, leftExitStart, leftExitEnd, rightExitStart, rightExitEnd));
        }
    }

    /// <summary>
    /// Luo satunnaisen ruutukartan yhdistelemällä ruutuja uloskäyntien avulla.
    /// </summary>
    /// <returns>Ruutukartta käytettäväksi ColorTileMapin kanssa</returns>
    public Image GenerateMap()
    {
        generatedMap = new InfiniteMapGenTile[TilesHorizontal, TilesVertical];

        List<InfiniteMapGenTile> spawnTiles = allTiles.FindAll(x => x.HasPlayer1Spawn && x.HasPlayer2Spawn);
        allTiles.RemoveAll(x => x.HasPlayer1Spawn && x.HasPlayer2Spawn); // ettei tule useampia aloitusspawnpointeja

        int startingX = RandomGen.NextInt(0, generatedMap.GetLength(0));
        int startingY = RandomGen.NextInt(0, generatedMap.GetLength(1));
        generatedMap[startingX, startingY] = RandomGen.SelectOne<InfiniteMapGenTile>(spawnTiles);

        // rekursiivinen, generoi koko kentän
        SetNeighbourTiles(startingX, startingY);

        return GetTileMap();
    }

    private void SetNeighbourTiles(int x, int y)
    {
        IntPoint[] nbCoords = GetEmptyNeighbours(x, y);

        for (int i = 0; i < nbCoords.Length; i++)
        {
            SetTile(nbCoords[i].X, nbCoords[i].Y);
        }

        RandomGen.Shuffle<IntPoint>(nbCoords);

        for (int i = 0; i < nbCoords.Length; i++)
        {
            SetNeighbourTiles(nbCoords[i].X, nbCoords[i].Y);
        }
    }

    /// <summary>
    /// Valitsee sopivan ruudun sen naapurien perusteella.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void SetTile(int x, int y)
    {
        bool needsTopExit = false;
        bool needsBottomExit = false;
        bool needsLeftExit = false;
        bool needsRightExit = false;

        if (x - 1 >= 0)
            if (generatedMap[x - 1, y] != null)
                if (generatedMap[x - 1, y].HasRightExit)
                    needsLeftExit = true;

        if (x + 1 < generatedMap.GetLength(0))
             if (generatedMap[x + 1, y] != null)
                if (generatedMap[x + 1, y].HasLeftExit)
                    needsRightExit = true;

        if (y - 1 >= 0)
            if (generatedMap[x, y - 1] != null)
                if (generatedMap[x, y - 1].HasBottomExit)
                    needsTopExit = true;

        if (y + 1 < generatedMap.GetLength(1))
            if (generatedMap[x, y + 1] != null)
                if (generatedMap[x, y + 1].HasTopExit)
                    needsBottomExit = true;

        // etsitään kaikki missä on sopivat uloskäynnit
        List<InfiniteMapGenTile> validTiles = allTiles.FindAll(delegate(InfiniteMapGenTile t)
        {
            if (needsTopExit && t.HasTopExit == false)
                return false;
            if (needsBottomExit && t.HasBottomExit == false)
                return false;
            if (needsLeftExit && t.HasLeftExit == false)
                return false;
            if (needsRightExit && t.HasRightExit == false)
                return false;

            return true;
        });

        // tähän lisää ehtoja, esim laatikoita, spawneja yms?

        if (validTiles.Count != 0)
            generatedMap[x, y] = RandomGen.SelectOne<InfiniteMapGenTile>(validTiles);
        else
            generatedMap[x, y] = RandomGen.SelectOne<InfiniteMapGenTile>(allTiles);
    }

    private IntPoint[] GetEmptyNeighbours(int x, int y)
    {
        if (x < 0 || x > generatedMap.GetLength(0)) return null;
        if (y < 0 || y > generatedMap.GetLength(1)) return null;

        List<IntPoint> nb = new List<IntPoint>();

        if (x - 1 >= 0)
        {
            if (generatedMap[x - 1, y] == null)
                nb.Add(new IntPoint(x - 1, y));
        }

        if (x + 1 < generatedMap.GetLength(0))
        {
            if (generatedMap[x + 1, y] == null)
                nb.Add(new IntPoint(x + 1, y));
        }

        if (y - 1 >= 0)
        {
            if (generatedMap[x, y - 1] == null)
                nb.Add(new IntPoint(x, y - 1));
        }

        if (y + 1 < generatedMap.GetLength(1))
        {
            if (generatedMap[x, y + 1] == null)
                nb.Add(new IntPoint(x, y + 1));
        }
        return nb.ToArray();
    }

    /// <summary>
    /// Muodostaa Jypeli-ColorTileMapin yhdistämällä kaikki ruudut.
    /// </summary>
    /// <returns></returns>
    public Image GetTileMap()
    {
        Image tilemap = new Image(MapWidth, MapHeight, Vakiot.TYHJA);
        Color[,] pixels = new Color[MapWidth, MapHeight];

        for (int i = 0; i < generatedMap.GetLength(0); i++)
        {
            for (int j = 0; j < generatedMap.GetLength(1); j++)
            {
                // [i, j] ruudun indeksi
                Color[,] currentPixels = generatedMap[i, j].Tile.GetData();
                CopyArrayTo(currentPixels, pixels, i * TileWidth, j * TileHeight);
            }
        }

        tilemap.SetData(pixels);
        return tilemap;
    }

    private void CopyArrayTo(Color[,] source, Color[,] destination, int startX, int startY)
    {
        if (source.GetLength(0) > destination.GetLength(0) - startX) return;
        if (source.GetLength(1) > destination.GetLength(1) - startY) return;


        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int j = 0; j < source.GetLength(1); j++)
            {
                destination[i + startX, j + startY] = source[i, j];
            }
        }
    }
}