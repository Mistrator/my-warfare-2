using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class Laatikko : PhysicsObject
{
    //Laatikon kertakäyttöisyys, voi ottaa 15 sekunnin päästä uudestaan
    Image laatikonKuva = MW2_My_Warfare_2_.LoadImage("kamaa");

    private bool onkokäytössä = true;
    public bool OnkoKäytössä
    {
        get { return onkokäytössä; }
        set { onkokäytössä = value; }
    }
    private Timer laatikonaika;
    public Timer LaatikonAika
    {
        get { return laatikonaika; }
        set { laatikonaika = value; }
    }

    public Laatikko(double width, double height)
        : base(width, height)
    {
        this.laatikonaika = new Timer();
        this.laatikonaika.Interval = 15;
        this.laatikonaika.Timeout += delegate { this.onkokäytössä = true; this.laatikonaika.Reset(); this.Image = laatikonKuva; this.IgnoresCollisionResponse = false; };
    }
}