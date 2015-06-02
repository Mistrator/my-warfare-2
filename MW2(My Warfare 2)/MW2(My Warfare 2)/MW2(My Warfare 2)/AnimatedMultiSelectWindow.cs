using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Widgets;

/// <summary>
/// Monivalintaikkuna, joka on animoitu.
/// </summary>
public class AnimatedMultiSelectWindow : MultiSelectWindow
{
    /// <summary>
    /// Valinnan korostaja.
    /// </summary>
    private PhysicsObject SelectionHighlighter { get; set; }

    /// <summary>
    /// Korostajan liikkumisnopeus.
    /// </summary>
    public double MovementSpeed { get; set; }

    /// <summary>
    /// Luodaan uusi animoitu monivalintaikkuna.
    /// </summary>
    /// <param name="question">Ikkunan otsikko.</param>
    /// <param name="selectionHighlight">Animoidun korostajan kuva.</param>
    /// <param name="buttonTexts">Painikkeiden tekstit.</param>
    public AnimatedMultiSelectWindow(string question, Image selectionHighlight, params string[] buttonTexts)
        : base(question, buttonTexts)
    {
        SelectionColor = Color.Transparent;

        SelectionHighlighter = new PhysicsObject(selectionHighlight.Width, selectionHighlight.Height);
        SelectionHighlighter.Image = selectionHighlight;
        SelectionHighlighter.IgnoresExplosions = true;
        SelectionHighlighter.IgnoresGravity = true;
        SelectionHighlighter.IgnoresCollisionResponse = true;

        this.AddedToGame += delegate {
            Timer.SingleShot(0.01, delegate
            {
                MW2_My_Warfare_2_.Peli.Add(SelectionHighlighter);
                SelectionHighlighter.Position = this.Buttons[0].Position;
            });
        };

        this.Closed += delegate { SelectionHighlighter.Destroy(); };
        MovementSpeed = 450;

        foreach (PushButton button in this.Buttons)
        {
            button.Hovered += MoveHighlighter;
        }
    }

    /// <summary>
    /// Liikutetaan korostinta korostetun napin kohdalle.
    /// </summary>
    /// <param name="button"></param>
    private void MoveHighlighter(PushButton button)
    {
        SelectionHighlighter.MoveTo(button.Position, MovementSpeed);
    }
}