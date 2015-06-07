using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RadioButtonGroup
{
    public List<OnOffButton> buttons { get; set; }

    public int Selected { get; private set; }

    public RadioButtonGroup()
    {
        buttons = new List<OnOffButton>();
    }

    public void AddButton(OnOffButton button)
    {
        buttons.Add(button);
        button.Clicked += delegate { ButtonClicked(button); };

        if (buttons.Count == 1)
            Selected = 0;
    }

    private void ButtonClicked(OnOffButton clicked)
    {
        if (clicked.IsPressed == false)
            clicked.Pressed();
        buttons.FindAll(x => x != clicked).ForEach(delegate(OnOffButton b)
        {
            if (b.IsPressed)
                b.Pressed(); // vaihdetaan tila mutta ei kutsuta Clicked-actionia
        });

        Selected = buttons.IndexOf(clicked);
    }
}