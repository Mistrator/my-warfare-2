using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class Botti : Elävä
{
    private string botinnimi;
    public string BotinNimi
    {
        get { return botinnimi; }
        set { botinnimi = value; }
    }

    public Botti(double width, double height)
        : base(width, height)
    {
        this.Elämät = new DoubleMeter(20);
        this.Elämät.MinValue = 0;
        this.botinnimi = "N00b";
    }
}