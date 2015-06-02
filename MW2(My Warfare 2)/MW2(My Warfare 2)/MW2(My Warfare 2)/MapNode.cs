using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

/// <summary>
/// Reitinhaussa käytettävä solmukohta.
/// </summary>
public class MapNode
{
    /// <summary>
    /// Onko ruudussa este, jonka läpi ei voi kulkea.
    /// </summary>
    public bool IsFull;

    /// <summary>
    /// Used in search functions.
    /// </summary>
    public bool IsExamined;

    public int Distance;

    public MapNode PreviousNode;

    public IntPoint Position;

    public MapNode(bool isFull, IntPoint position)
    {
        this.IsFull = isFull;
        this.IsExamined = false;
        this.Position = position;
        this.Distance = 0;
    }
}