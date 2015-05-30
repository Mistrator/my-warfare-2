using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class Zombi : Elava
{
    private int spawnauspaikka;
    public int SpawnausPaikka
    {
        get { return spawnauspaikka; }
        set { spawnauspaikka = value; }
    }

    public Zombi(double width, double height)
        : base(width, height)
    {
        this.Elamat = new DoubleMeter(20);
        this.Elamat.MinValue = 0;
        this.Elamat.LowerLimit += Kuole;
    }

    private void Kuole()
    {
        this.Destroy();
    }
}