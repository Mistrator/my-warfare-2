﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli.Controls;
using Jypeli.GameObjects;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Ikkuna, joka antaa käyttäjän valita yhden annetuista vaihtoehdoista.
    /// </summary>
    public class MultiSelectWindow : Window
    {
        static readonly Key[] keys = { Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0 };

        private int _defaultCancel = 0;
        private Listener _escListener = null;
        private Listener _xboxListener = null;
        private Listener _backListener = null;

        private int _selectedIndex = -1;
        private Color _selectedColor = Color.Black;
        private Color _selectionColor = Color.Cyan;
        private bool _buttonColorSet = false;

        /// <summary>
        /// Painonappulat järjestyksessä.
        /// </summary>
        public PushButton[] Buttons { get; private set; }

        /// <summary>
        /// Mitä valitaan kun käyttäjä painaa esc tai takaisin-näppäintä.
        /// Laittomalla arvolla (esim. negatiivinen) em. näppäimistä ei tapahdu mitään.
        /// </summary>
        public int DefaultCancel
        {
            get { return _defaultCancel; }
            set
            {
                _defaultCancel = value;
                if ( Game.Instance != null ) AddDefaultControls();
            }
        }

        /// <summary>
        /// Valittu nappula.
        /// </summary>
        public PushButton SelectedButton
        {
            get { return _selectedIndex >= 0 || _selectedIndex >= Buttons.Length ? Buttons[_selectedIndex] : null; }
        }

        /// <summary>
        /// Nappulan oletusväri.
        /// </summary>
        public override Color Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                if ( !_buttonColorSet ) _setButtonColor( Color.Darker( value, 40 ) );
                base.Color = value;
            }
        }

        /// <summary>
        /// Valitun nappulan väri.
        /// </summary>
        public Color SelectionColor
        {
            get { return _selectionColor; }
            set
            {
                _selectionColor = value;
                SelectButton( _selectedIndex );
            }
        }

        /// <summary>
        /// Tapahtuma joka tapahtuu kun nappia painetaan.
        /// Ottaa parametrikseen painonapin indeksin (alkaen nollasta).
        /// </summary>
        public event Action<int> ItemSelected;

        /// <summary>
        /// Luo uuden monivalintaikkunan.
        /// </summary>
        /// <param name="question">Kysymys.</param>
        /// <param name="buttonTexts">Nappien tekstit merkkijonoina.</param>
        public MultiSelectWindow(string question, params string[] buttonTexts)
            : base()
        {
            if ( buttonTexts.Length == 0 ) throw new InvalidOperationException( "You must add at least one button" );

            VerticalScrollLayout layout = new VerticalScrollLayout();
            layout.LeftPadding = layout.RightPadding = 20;
            layout.BottomPadding = 30;
            layout.Spacing = 20;
            this.Layout = layout;

            Label kysymysLabel = new Label( question );
            Add( kysymysLabel );

            Buttons = new PushButton[buttonTexts.Length];

            for ( int i = 0; i < buttonTexts.Length; i++ )
            {
                PushButton button = new PushButton( buttonTexts[i] );
                button.Tag = i;
                button.Clicked += new Action( delegate { button_Clicked( (int)button.Tag ); } );
#if WINDOWS_PHONE
            if ( Game.Instance.Phone.DisplayResolution == WP7.DisplayResolution.Large )
                button.TextScale = new Vector(2, 2);
#endif
                Add( button );
                Buttons[i] = button;
            }

            Game.AssertInitialized( delegate { AddControls(); AddDefaultControls(); } );
            AddedToGame += delegate { SelectButton( 0 ); };
        }

        /// <summary>
        /// Luo uuden monivalintaikkunan.
        /// </summary>
        /// <param name="question">Kysymys.</param>
        /// <param name="titleColor">Otsikon tekstin väri.</param>
        /// <param name="buttonTexts">Nappien tekstit merkkijonoina.</param>
        public MultiSelectWindow(string question, Color titleColor, params string[] buttonTexts)
            : base()
        {
            if (buttonTexts.Length == 0) throw new InvalidOperationException("You must add at least one button");

            VerticalScrollLayout layout = new VerticalScrollLayout();
            layout.LeftPadding = layout.RightPadding = 20;
            layout.BottomPadding = 30;
            layout.Spacing = 20;
            this.Layout = layout;

            Label kysymysLabel = new Label(question);
            kysymysLabel.TextColor = titleColor;
            Add(kysymysLabel);

            Buttons = new PushButton[buttonTexts.Length];

            for (int i = 0; i < buttonTexts.Length; i++)
            {
                PushButton button = new PushButton(buttonTexts[i]);
                button.Tag = i;
                button.Clicked += new Action(delegate { button_Clicked((int)button.Tag); });
#if WINDOWS_PHONE
            if ( Game.Instance.Phone.DisplayResolution == WP7.DisplayResolution.Large )
                button.TextScale = new Vector(2, 2);
#endif
                Add(button);
                Buttons[i] = button;
            }

            Game.AssertInitialized(delegate { AddControls(); AddDefaultControls(); });
            AddedToGame += delegate { SelectButton(0); };
        }


        private void SelectButton( int p )
        {
#if !WINDOWS_PHONE
            UnselectButton();
            if ( p < 0 || p >= Buttons.Length ) return;

            _selectedIndex = p;
            _selectedColor = SelectedButton.Color;
            SelectedButton.Color = SelectionColor;
#endif
        }

        private void UnselectButton()
        {
            if ( _selectedIndex < 0 ) return;
            SelectedButton.Color = _selectedColor;
            _selectedIndex = -1;
        }

        public void AddItemHandler( int item, Action handler )
        {
            Buttons[item].Clicked += handler;
        }

        public void AddItemHandler<T1>(int item, Action<T1> handler, T1 p1)
        {
            Buttons[item].Clicked += delegate { handler(p1); };
        }

        public void AddItemHandler<T1, T2>(int item, Action<T1, T2> handler, T1 p1, T2 p2)
        {
            Buttons[item].Clicked += delegate { handler(p1, p2); };
        }

        public void AddItemHandler<T1, T2, T3>(int item, Action<T1, T2, T3> handler, T1 p1, T2 p2, T3 p3)
        {
            Buttons[item].Clicked += delegate { handler(p1, p2, p3); };
        }

        public void RemoveItemHandler( int item, Action handler )
        {
            Buttons[item].Clicked -= handler;
        }

        private void _setButtonColor( Color color )
        {
            for ( int i = 0; i < Buttons.Length; i++ )
            {
                Buttons[i].Color = color;
            }

            // Re-set the color for selected item and reselect it
            _selectedColor = color;
            SelectButton( _selectedIndex );
        }

        public void SetButtonColor( Color color )
        {
            _setButtonColor( color );
            _buttonColorSet = true;
        }

        public void SetButtonTextColor(Color color)
        {
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].TextColor = color;
            }
        }

        void AddControls()
        {
            for ( int i = 0; i < Math.Min( Buttons.Length, keys.Length ); i++ )
            {
                Jypeli.Game.Instance.Keyboard.Listen( keys[i], ButtonState.Pressed, button_Clicked, null, i ).InContext( this );
            }

            Handler selectPrev = delegate { SelectButton( _selectedIndex > 0 ? _selectedIndex - 1 : Buttons.Length - 1 ); };
            Handler selectNext = delegate { SelectButton( _selectedIndex < Buttons.Length - 1 ? _selectedIndex + 1 : 0 ); };
            Handler confirmSelect = delegate { SelectedButton.Click(); };
            
            Jypeli.Game.Instance.Keyboard.Listen( Key.Up, ButtonState.Pressed, selectPrev, null ).InContext( this );
            Jypeli.Game.Instance.Keyboard.Listen( Key.Down, ButtonState.Pressed, selectNext, null ).InContext( this );
            Jypeli.Game.Instance.Keyboard.Listen( Key.Enter, ButtonState.Pressed, confirmSelect, null ).InContext( this );

            Jypeli.Game.Instance.ControllerOne.Listen( Button.DPadUp, ButtonState.Pressed, selectPrev, null ).InContext( this );
            Jypeli.Game.Instance.ControllerOne.Listen( Button.DPadDown, ButtonState.Pressed, selectNext, null ).InContext( this );
            Jypeli.Game.Instance.ControllerOne.Listen( Button.A, ButtonState.Pressed, confirmSelect, null ).InContext( this );
        }

        void AddDefaultControls()
        {
            if ( _backListener != null ) Jypeli.Game.Instance.PhoneBackButton.Remove( _backListener );
            if ( _escListener != null ) Jypeli.Game.Instance.Keyboard.Remove( _escListener );
            if ( _xboxListener != null ) Jypeli.Game.Instance.Keyboard.Remove( _xboxListener );

            if ( _defaultCancel >= 0 && _defaultCancel < Buttons.Length )
            {
                _backListener = Jypeli.Game.Instance.PhoneBackButton.Listen( Buttons[_defaultCancel].Click, null ).InContext( this );
                _escListener = Jypeli.Game.Instance.Keyboard.Listen( Key.Escape, ButtonState.Pressed, Buttons[_defaultCancel].Click, null ).InContext( this );
                _xboxListener = Jypeli.Game.Instance.ControllerOne.Listen( Button.B, ButtonState.Pressed, Buttons[_defaultCancel].Click, null ).InContext( this );
            }
        }

        void button_Clicked( int index )
        {
            if ( ItemSelected != null )
                ItemSelected( index );

            Close();
        }
    }
}
