using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Widgets;

/// <summary>
/// Nappi, jolla on kaksi asentoa. Asento vaihtuu painettaessa nappia.
/// </summary>
public class OnOffButton : PushButton
{
    /// <summary>
    /// Onko nappi päällä vai pois päältä.
    /// </summary>
    public bool IsPressed { get; protected set; }

    /// <summary>
    /// Teksti, kun nappi on päällä.
    /// </summary>
    public String TextWhileOn { get; set; }

    /// <summary>
    /// Teksti, kun nappi on pois päältä.
    /// </summary>
    public String TextWhileOff { get; set; }

    /// <summary>
    /// Väri, kun nappi on pohjassa.
    /// </summary>
    public Color ColorWhileOn { get; set; }

    /// <summary>
    /// Väri, kun nappi on ylhäällä.
    /// </summary>
    public Color ColorWhileOff { get; set; }


    /// <summary>
    /// Luodaan uusi kaksiasentoinen nappi.
    /// </summary>
    /// <param name="width">Leveys.</param>
    /// <param name="height">Korkeus.</param>
    public OnOffButton(double width, double height)
        : base(width, height)
    {
        this.TextWhileOn = "";
        this.TextWhileOff = "";
        this.ColorWhileOn = Color.Green;
        this.ColorWhileOff = Color.Red;
        this.Clicked += Pressed;
        this.Text = this.TextWhileOff;
        this.Color = this.ColorWhileOff;
    }


    /// <summary>
    /// Luodaan uusi kaksiasentoinen nappi.
    /// </summary>
    /// <param name="width">Leveys.</param>
    /// <param name="height">Korkeus.</param>
    /// <param name="isPressed">Onko nappi valmiiksi pohjassa vai ei.</param>
    public OnOffButton(double width, double height, bool isPressed)
        : base(width, height)
    {
        this.TextWhileOn = "";
        this.TextWhileOff = "";
        this.ColorWhileOn = Color.Green;
        this.ColorWhileOff = Color.Red;
        this.Clicked += Pressed;
        this.IsPressed = isPressed;
        if (this.IsPressed)
        {
            this.Text = this.TextWhileOn;
            this.Color = this.ColorWhileOn;
        }
        else
        {
            this.Text = this.TextWhileOff;
            this.Color = this.ColorWhileOff;
        }
    }


    /// <summary>
    /// Luodaan uusi kaksiasentoinen nappi.
    /// </summary>
    /// <param name="width">Leveys.</param>
    /// <param name="height">Korkeus.</param>
    /// <param name="textWhileOn">Teksti, kun nappi on pohjassa.</param>
    /// <param name="textWhileOff">Teksti, kun nappi on ylhäällä.</param>
    public OnOffButton(double width, double height, String textWhileOn, String textWhileOff)
        : base(width, height)
    {
        this.TextWhileOn = textWhileOn;
        this.TextWhileOff = textWhileOff;
        this.ColorWhileOn = Color.Green;
        this.ColorWhileOff = Color.Red;
        this.Clicked += Pressed;
        this.Text = this.TextWhileOff;
        this.Color = this.ColorWhileOff;
    }

    /// <summary>
    /// Luodaan uusi kaksiasentoinen nappi.
    /// </summary>
    /// <param name="width">Leveys.</param>
    /// <param name="height">Korkeus.</param>
    /// <param name="colorWhileOn">Väri, kun nappi on pohjassa.</param>
    /// <param name="colorWhileOff">Väri, kun nappi on ylhäällä.</param>
    public OnOffButton(double width, double height, Color colorWhileOn, Color colorWhileOff)
        :base(width, height)
    {
        this.TextWhileOn = "";
        this.TextWhileOff = "";
        this.ColorWhileOn = colorWhileOn;
        this.ColorWhileOff = colorWhileOff;
        this.Clicked += Pressed;
        this.Text = this.TextWhileOff;
        this.Color = this.ColorWhileOff;
    }

    private void Pressed()
    {
        this.IsPressed = !IsPressed;
        if (this.IsPressed)
        {
            this.Text = this.TextWhileOn;
            this.Color = this.ColorWhileOn;
        }
        else
        {
            this.Text = this.TextWhileOff;
            this.Color = this.ColorWhileOff;
        }
    }
}