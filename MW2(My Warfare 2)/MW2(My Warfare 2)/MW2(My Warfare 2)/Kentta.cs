using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Tietorakenne pelikentän seinille.
/// </summary>
public class Kentta
{
    private GameObject[,] KentanOsat { get; set; }
    public MapNode[,] Nodes { get; private set; }

    private String ValeSeinienTag { get; set; }

    /// <summary>
    /// Luodaan uusi kentän tietorakenne.
    /// </summary>
    /// <param name="riveja">Kentän rivien määrä.</param>
    /// <param name="sarakkeita">Kentän sarakkeiden määrä.</param>
    public Kentta(int riveja, int sarakkeita, String valeSeinienTag)
    {
        KentanOsat = new GameObject[riveja, sarakkeita];
        ValeSeinienTag = valeSeinienTag;

        Nodes = new MapNode[riveja, sarakkeita];

        for (int i = 0; i < riveja; i++)
            for (int j = 0; j < sarakkeita; j++)
                Nodes[i, j] = new MapNode(false, new IntPoint(i, j));
    }


    #region valeseinat

    /// <summary>
    /// Lisätään seinä tietorakenteeseen.
    /// </summary>
    /// <param name="rivi">Seinän rivi tietorakenteessa.</param>
    /// <param name="sarake">Seinän sarake tietorakenteessa.</param>
    /// <param name="lisattava">Lisättävä seinä.</param>
    public void LisaaSeina(int rivi, int sarake, GameObject lisattava)
    {
        if (!TarkistaOlemassaolo(rivi, sarake, true)) return;            
        KentanOsat[rivi, sarake] = lisattava;

        if (lisattava.Tag.ToString() != ValeSeinienTag && lisattava.Tag.ToString() != Vakiot.PIIKKILANKA_TAG) // kiinteä seinä reitinhaulle
            Nodes[rivi, sarake].IsFull = true;
        
    }

    /// <summary>
    /// Poistetaan seinän osa tietorakenteesta.
    /// </summary>
    /// <param name="x">Seinän rivi.</param>
    /// <param name="y">Seinän sarake.</param>
    public void TuhoaSeina(int rivi, int sarake)
    {
        if (!TarkistaOlemassaolo(rivi, sarake)) return; // ei tuhota olematonta
        MuutaNaapuritKiinteiksi(rivi, sarake);

        KentanOsat[rivi, sarake].Destroy();
        KentanOsat[rivi, sarake] = null;

        Nodes[rivi, sarake].IsFull = false;
    }

    /// <summary>
    /// Tarkistetaan, ettei jokin paikka ole null ja se on taulukon sisäpuolella.
    /// </summary>
    /// <param name="x">X taulukossa</param>
    /// <param name="y">Y taulukossa</param>
    /// <param name="saakoOllaVarattu">Jos true, ruutu saa olla jo varattu. Taulukon sisäpuolella olo tarkistetaan silti.</param>
    /// <returns>True jos paikka on varattu ja taulukon sisäpuolella, muuten false.</returns>
    private bool TarkistaOlemassaolo(int x, int y, bool saakoOllaVarattu = false)
    {
        if (x < 0 || x >= KentanOsat.GetLength(0)) return false;
        if (y < 0 || y >= KentanOsat.GetLength(1)) return false;
        if (!saakoOllaVarattu) // voidaan ohittaa vaatimus tyhjästä ruudusta
            if (KentanOsat[x, y] == null) return false;
        return true;
    }

    /// <summary>
    /// Muutetaan tuhotun kentän osan naapurit kiinteiksi, jos ne eivät jo ole.
    /// </summary>
    /// <param name="x">Tuhotun osan X taulukossa</param>
    /// <param name="y">Tuhotun osan Y taulukossa</param>
    private void MuutaNaapuritKiinteiksi(int x, int y)
    {
        if (TarkistaOlemassaolo(x + 1, y))
            if (KentanOsat[x + 1, y].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x + 1, y);

        if (TarkistaOlemassaolo(x - 1, y))
            if (KentanOsat[x - 1, y].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x - 1, y);

        if (TarkistaOlemassaolo(x, y + 1))
            if (KentanOsat[x, y + 1].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x, y + 1);

        if (TarkistaOlemassaolo(x, y - 1))
            if (KentanOsat[x, y - 1].Tag.ToString() == ValeSeinienTag) VaihdaSeinaKiinteaksi(x, y - 1);
    }

    /// <summary>
    /// Poistetaan läpi mentävä seinä ja laitetaan tilalle kiinteä kopio.
    /// </summary>
    /// <param name="x">Muutettavan X taulukossa</param>
    /// <param name="y">Muutettavan Y taulukossa</param>
    private void VaihdaSeinaKiinteaksi(int x, int y)
    {
        Tuhoutuva kiinteaseina = new Tuhoutuva(KentanOsat[x, y].Width, KentanOsat[x, y].Height, 20); // äh, kesto fixattu...
        kiinteaseina.Position = KentanOsat[x, y].Position;
        kiinteaseina.Image = KentanOsat[x, y].Image;
        kiinteaseina.PositionInLevelArray = new IntPoint(x, y);
        kiinteaseina.CollisionIgnoreGroup = 1;
        kiinteaseina.MakeStatic();
        kiinteaseina.Kesto.LowerLimit += delegate
        {
            kiinteaseina.Destroy();
            TuhoaSeina(x, y);
            // tänne jotain ?
        };
        MW2_My_Warfare_2_.Peli.Add(kiinteaseina);
        KentanOsat[x, y].Destroy();
        KentanOsat[x, y] = kiinteaseina;

        Nodes[x, y].IsFull = true;
    }

    #endregion

    #region reitinhaku

    public void SetNode(int i, int j, MapNode node)
    {
        Nodes[i, j] = node;
    }

    /// <summary>
    /// Etsitään lyhin reitti kahden pisteen välillä.
    /// </summary>
    /// <param name="startingPosition">Aloituspiste.</param>
    /// <param name="targetPosition">Lopetuspiste.</param>
    /// <returns>Lyhin reitti.</returns>
    public MapNode[] DijkstraSearch(IntPoint startingPosition, IntPoint targetPosition)
    {
        List<MapNode> unvisitedNodes = new List<MapNode>();

        const int NodeDistance = 1;

        for (int i = 0; i < Nodes.GetLength(0); i++)
        {
            for (int j = 0; j < Nodes.GetLength(1); j++)
            {
                if (i != startingPosition.X || j != startingPosition.Y)
                {
                    Nodes[i, j].IsExamined = false;
                    Nodes[i, j].Distance = int.MaxValue;
                    Nodes[i, j].PreviousNode = null;
                    unvisitedNodes.Add(Nodes[i, j]);
                }
                else
                {
                    Nodes[i, j].Distance = 0;
                    Nodes[i, j].PreviousNode = null;
                }
            }
        }

        MapNode currentNode = Nodes[startingPosition.X, startingPosition.Y];
        bool firstIteration = true;

        int iterations = 0;
        while (unvisitedNodes.Count != 0)
        {
            iterations++;
            if (!firstIteration)
            {
                currentNode = GetNodeWithSmallestDistance(unvisitedNodes);
                // unvisitedNodes.RemoveAt(unvisitedNodes.FindIndex(x => x.Position.X == currentNode.Position.X && x.Position.Y == currentNode.Position.Y));
                unvisitedNodes.Remove(currentNode);
            }
            firstIteration = false;
            currentNode.IsExamined = true;
            if (currentNode.Position == targetPosition) // se tunne kun unohtaa overloadata == :n... i just got cancer
                break;

            MapNode[] neighbours = GetEmptyNotExaminedNeighbours(currentNode);
            for (int i = 0; i < neighbours.Length; i++)
            {
                int distance = currentNode.Distance + NodeDistance;
                if (distance < neighbours[i].Distance)
                {
                    neighbours[i].Distance = distance;
                    neighbours[i].PreviousNode = currentNode;
                }
            }
        }
        List<MapNode> shortestRoute = new List<MapNode>();
        shortestRoute.Add(currentNode);

        while (shortestRoute[0].PreviousNode != null)
        {
            shortestRoute.Insert(0, shortestRoute[0].PreviousNode);
        }
        return shortestRoute.ToArray();
    }

    private MapNode GetNodeWithSmallestDistance(List<MapNode> nodes)
    {
        //if (nodes.Count == 0) return
        int smallestDistance = nodes[0].Distance;
        int smallestIndex = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Distance < smallestDistance)
            {
                smallestDistance = nodes[i].Distance;
                smallestIndex = i;
            }
        }
        return nodes[smallestIndex];
    }

    public MapNode[] GetEmptyNeighbours(MapNode node)
    {
        List<MapNode> results = new List<MapNode>(4);

        if (node.Position.X - 1 >= 0)
            if (!(Nodes[node.Position.X - 1, node.Position.Y].IsFull))
                results.Add(Nodes[node.Position.X - 1, node.Position.Y]);

        if (node.Position.X + 1 < Nodes.GetLength(0))
            if (!(Nodes[node.Position.X + 1, node.Position.Y].IsFull))
                results.Add(Nodes[node.Position.X + 1, node.Position.Y]);

        if (node.Position.Y - 1 >= 0)
            if (!(Nodes[node.Position.X, node.Position.Y - 1].IsFull))
                results.Add(Nodes[node.Position.X, node.Position.Y - 1]);

        if (node.Position.Y + 1 < Nodes.GetLength(1))
            if (!(Nodes[node.Position.X, node.Position.Y + 1].IsFull))
                results.Add(Nodes[node.Position.X, node.Position.Y + 1]);

        if (results.Count == 0)
            return null;
        return results.ToArray();
    }

    public MapNode[] GetEmptyNotExaminedNeighbours(MapNode node)
    {
        List<MapNode> empties = new List<MapNode>();
        MapNode[] emptyNeighbours = GetEmptyNeighbours(node);
        if (emptyNeighbours != null)
            empties.AddRange(emptyNeighbours);

        return empties.FindAll(x => x.IsExamined == false).ToArray();
    }

    public MapNode GetNodeFromIndex(IntPoint index)
    {
        //if (index.X < 0 || index.X > nodes.GetLength(0)) return null;
        //if (index.Y < 0 || index.Y > nodes.GetLength(1)) return null;
        return Nodes[index.X, index.Y];
    }

    /// <summary>
    /// Palauttaa ruudun keskipisteen koordinaatit.
    /// </summary>
    /// <param name="node">Ruudun indeksi.</param>
    /// <returns>Ruudun keskipisteen koordinaatit.</returns>
    public Vector GetPositionOnWorld(IntPoint node)
    {
        double x = -(Nodes.GetLength(0) * Vakiot.KENTAN_RUUDUN_LEVEYS / 2) + Vakiot.KENTAN_RUUDUN_LEVEYS / 2 + node.X * Vakiot.KENTAN_RUUDUN_LEVEYS;
        double y = Nodes.GetLength(1) * Vakiot.KENTAN_RUUDUN_KORKEUS / 2 - Vakiot.KENTAN_RUUDUN_KORKEUS / 2 - node.Y * Vakiot.KENTAN_RUUDUN_KORKEUS;
        return new Vector(x, y);
    }

    /// <summary>
    /// Palauttaa ruudun indeksin, jossa piste sijaitsee.
    /// Jos piste on ruudukon ulkopuolella, heitetään poikkeus.
    /// </summary>
    /// <param name="position">Koordinaatti</param>
    /// <returns>Lähimmän ruudun indeksi.</returns>
    public IntPoint GetCorrespondingNode(Vector position)
    {
        if (position.X < -(Nodes.GetLength(0) * Vakiot.KENTAN_RUUDUN_LEVEYS / 2) || position.X > Nodes.GetLength(0) * Vakiot.KENTAN_RUUDUN_LEVEYS / 2)
            throw new ArgumentOutOfRangeException("position", "X-coordinate was outside the grid.");
        // on leveyssuunnassa ruudukon ulkopuolella
        if (position.Y > Nodes.GetLength(1) * Vakiot.KENTAN_RUUDUN_KORKEUS / 2 || position.Y < -(Nodes.GetLength(1) * Vakiot.KENTAN_RUUDUN_KORKEUS / 2))
            throw new ArgumentOutOfRangeException("position", "Y-coordinate was outside the grid.");
        // on korkeussuunnassa ruudukon ulkopuolella

        int x;
        if (position.X >= 0)
            x = ((int)Math.Ceiling(position.X / Vakiot.KENTAN_RUUDUN_LEVEYS) + Nodes.GetLength(0) / 2) - 1;
        else
            x = (int)Math.Floor(position.X / Vakiot.KENTAN_RUUDUN_LEVEYS) + Nodes.GetLength(0) / 2;

        int y;
        if (position.Y >= 0)
            y = -((int)Math.Ceiling(position.Y / Vakiot.KENTAN_RUUDUN_KORKEUS) - Nodes.GetLength(1) / 2);
        else
            y = -(((int)Math.Floor(position.Y / Vakiot.KENTAN_RUUDUN_KORKEUS) - Nodes.GetLength(1) / 2) + 1);

        return new IntPoint(x, y);
    }

    /// <summary>
    /// Palautetaan MapNode-taulukon muodostama reitti maailmakoordinaatteina.
    /// </summary>
    /// <param name="route">Reitti.</param>
    /// <returns>Reitti maailmakoordinaatteina.</returns>
    public List<Vector> GetRouteCoordinates(MapNode[] route)
    {
        List<Vector> reitti = new List<Vector>();

        for (int i = 0; i < route.Length; i++)
        {
            reitti.Add(GetPositionOnWorld(route[i].Position));
        }
        return reitti;
    }

    private int ToNearestInt(double d)
    {
        return (int)(d + 0.5);
    }


    #endregion
}