#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using AdvanceMath;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Jypeli.Controls;
using Jypeli.GameObjects;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Aloitusruutu, joka voidaan näyttää ennen pelin käynnistämistä.
    /// </summary>
    public class SplashScreen : Window
    {
        private string _controlHelp = DefaultControlHelp;
        private string _loadingText = "Loading...";

        /// <summary>
        /// Pelin nimen näyttävä tekstikenttä.
        /// </summary>
        public Label NameLabel { get; private set; }

        /// <summary>
        /// Pelin tekijänoikeudet näyttävä tekstikenttä.
        /// </summary>
        public Label CopyrightLabel { get; private set; }

        /// <summary>
        /// Pelin tekijät näyttävä tekstikenttä.
        /// </summary>
        public Label AuthorsLabel { get; private set; }

        /// <summary>
        /// Päätekstikenttä.
        /// </summary>
        public StringListWidget TextBody { get; private set; }

        /// <summary>
        /// Aloita peli painamalla... -tekstin näyttävä tekstikenttä.
        /// Käytä ominaisuuksia ControlHelp ja LoadingText jos haluat muuttaa itse tekstiä.
        /// </summary>
        public Label StartLabel { get; private set; }

        /// <summary>
        /// Kontrolliohje (Aloita peli painamalla Enter / Xbox A).
        /// </summary>
        public string ControlHelp
        {
            get { return _controlHelp; }
            set
            {
                if ( StartLabel.Text == _controlHelp ) StartLabel.Text = value;
                _controlHelp = value;
            }
        }

        /// <summary>
        /// Latausteksti.
        /// </summary>
        public string LoadingText
        {
            get { return _loadingText; }
            set
            {
                if ( StartLabel.Text == _loadingText ) StartLabel.Text = value;
                _loadingText = value;
            }
        }

        /// <summary>
        /// Tapahtuu kun ruudusta poistutaan.
        /// Tee varsinaiset pelin alustukset tämän tapahtuman käsittelijässä.
        /// </summary>
        public event Action GameStarted;

        #region Default dimensions

        private static double DefaultWidth
        {
            get
            {
#if WINDOWS_PHONE
                return Game.Screen.Width;
#else
                return 600;
#endif
            }
        }

        private static double DefaultTextWidth
        {
            get
            {
#if WINDOWS_PHONE
                return Game.Instance.Phone.DisplayResolution == WP7.DisplayResolution.Small ? 300 : 500;
#else
                return 500;
#endif
            }
        }

        private static double DefaultHeight
        {
            get
            {
#if WINDOWS_PHONE
                return Game.Screen.Height;
#else
                return 600;
#endif
            }
        }

        private static double DefaultSpacing
        {
            get
            {
#if WINDOWS_PHONE
                return Game.Instance.Phone.DisplayResolution == WP7.DisplayResolution.Small ? 0 : 20;
#else
                return 50;
#endif
            }
        }

        private static string DefaultControlHelp
        {
            get
            {
#if WINDOWS_PHONE
                return "Start the game by tapping here";
#elif XBOX
                return "Start the game by pressing A";
#else
                return "Start the game by pressing Enter";
#endif
            }
        }

        #endregion

        /// <summary>
        /// Alustaa aloitusruudun.
        /// </summary>
        public SplashScreen( string gameName, string authors, string copyright, string textBody )
            : base( DefaultWidth, DefaultHeight )
        {
            Layout = new VerticalLayout() { Spacing = DefaultSpacing };

            NameLabel = InitializeTextDisplay( gameName, Color.Red );
            CopyrightLabel = InitializeTextDisplay( copyright, Color.Blue );
            AuthorsLabel = InitializeTextDisplay( authors, Color.Blue );
            StartLabel = InitializeTextDisplay( ControlHelp, Color.Green );
            
            double targetWidth = 2 * this.Width / 3;
            NameLabel.TextScale = new Vector( targetWidth / NameLabel.TextSize.X, 2 );
            NameLabel.SizeMode = TextSizeMode.Wrapped;
            CopyrightLabel.SizeMode = TextSizeMode.Wrapped;

            TextBody = new StringListWidget();
            TextBody.ItemAligment = HorizontalAlignment.Center;
            TextBody.Width = DefaultTextWidth;
            TextBody.TextColor = Color.Black;
            TextBody.Color = new Color( 0, 0, 255, 4 );
#if WINDOWS_PHONE
            if ( DefaultTextWidth < 500 )
                TextBody.Font = Font.DefaultSmall;
            else 
                TextBody.Font = Font.DefaultLarge;
#endif
            TextBody.Text = textBody;

            StartLabel.SizeMode = TextSizeMode.Wrapped;

            Game.Controls.Keyboard.Listen( Key.Enter, ButtonState.Pressed, BeginLoad, null,StartLabel ).InContext( this );
            Game.Controls.Keyboard.Listen( Key.Escape, ButtonState.Pressed, Game.Instance.Exit, null ).InContext( this ); ;
            Game.Controls.Mouse.Listen( MouseButton.Left, ButtonState.Down, BeginLoad, null, StartLabel ).InContext( this );
            
            for ( int i = 0; i < Game.Controls.GameControllers.Length; i++ )
            {
                Game.Controls.GameControllers[i].Listen( Button.A, ButtonState.Pressed, BeginLoad, null, StartLabel ).InContext( this );
                Game.Controls.GameControllers[i].Listen( Button.B, ButtonState.Pressed, Game.Instance.Exit, null ).InContext( this );
            }

            Game.Controls.TouchPanel.ListenOn( StartLabel, ButtonState.Pressed, delegate { BeginLoad( StartLabel ); }, null ).InContext( this );
            Game.Controls.PhoneBackButton.Listen( Game.Instance.Exit, null ).InContext( this );

            Add( NameLabel );
            Add( CopyrightLabel );
            Add( AuthorsLabel );
            Add( TextBody );
            Add( StartLabel );
        }

        private void BeginLoad( Label aloitusohje )
        {
            // Don't trigger twice
            if ( aloitusohje.Text == LoadingText )
                return;

            aloitusohje.TextColor = Color.Red;
            aloitusohje.Text = LoadingText;
            Timer.SingleShot( 0.1, ResumeLoad );
        }

        private void ResumeLoad()
        {
            Close();
            if ( GameStarted != null )
                GameStarted();
        }

        private Label InitializeTextDisplay( string text, Color textColor )
        {
            Label kentta = new Label(text);

            kentta.HorizontalAlignment = HorizontalAlignment.Center;
            kentta.VerticalAlignment = VerticalAlignment.Top;
            kentta.Width = DefaultTextWidth;
            kentta.Text = text;
            kentta.TextColor = textColor;

            return kentta;
        }
    }
}
