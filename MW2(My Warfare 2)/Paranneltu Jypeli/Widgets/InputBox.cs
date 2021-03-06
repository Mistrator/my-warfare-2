﻿#region MIT License
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
 * Authors: Tomi Karppinen, Tero Jäntti
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Jypeli.Controls;
using Jypeli.GameObjects;
using Jypeli.WP7;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Laatikko, johon käyttäjä voi syöttää tekstiä.
    /// </summary>
    public class InputBox : Label
    {
#if WINDOWS
        private static int InputBoxes = 0;
        const long eraseDelay = 1000000;

        long remainingDelay = 0;
#endif

        Timer cursorBlinkTimer;
        Color cursorColor = new Color( 255, 0, 0, 100 );

        /// <summary>
        /// Tekstilaatikon pituus kirjaimissa.
        /// </summary>
        public int WidthInCharacters { get; set; }

        /// <summary>
        /// Suurin määrä merkkejä joita tekstilaatikkoon voi kirjoittaa.
        /// </summary>
        public int MaxCharacters { get; set; }

        /// <summary>
        /// Kursori.
        /// </summary>
        public Widget Cursor { get; set; }

        public override Vector PreferredSize
        {
            get
            {
                return new Vector( Font.CharacterWidth * WidthInCharacters + 2 * XMargin, Font.CharacterHeight + 2 * YMargin );
            }
        }

        public override Vector Size
        {
            get { return base.Size; }
            set
            {
                base.Size = value;
                if ( Cursor != null )
                    UpdateCursorPosition();
            }
        }

        public override string Text
        {
            get { return base.Text; }
            set
            {
                if ( value.Length <= MaxCharacters )
                {
                    base.Text = value;

                }
                else
                {
                    base.Text = value.Substring( 0, MaxCharacters );
                }
                UpdateCursorPosition();
            }
        }

        /// <summary>
        /// Tapahtuma tekstin muuttumiselle.
        /// </summary>
        public event Action<string> TextChanged;

        protected void OnTextChanged()
        {
            if ( TextChanged != null )
                TextChanged( Text );
        }

        /// <summary>
        /// Alustaa uuden syöttökentän.
        /// </summary>
        /// <param name="characters">
        /// Kentän leveys merkkeinä. Tämä ei rajoita kirjoitettavan tekstin pituutta.
        /// <see cref="MaxCharacters"/>
        /// </param>
        public InputBox( int characters )
            : base()
        {
            MaxCharacters = int.MaxValue;
            WidthInCharacters = characters;
            HorizontalAlignment = HorizontalAlignment.Left;
            HorizontalSizing = Sizing.Expanding;
            XMargin = 7;
            YMargin = 2;
            TextColor = Color.Black;
            Color = new Color( 0, 255, 255, 150 );
            BorderColor = new Color( 200, 200, 200 );
            SizeMode = TextSizeMode.None;
            Size = PreferredSize;

            Cursor = new Widget( Font.CharacterWidth, Font.CharacterHeight );
            Cursor.Color = cursorColor;
            Add( Cursor );
            AddedToGame += UpdateCursorPosition;

            cursorBlinkTimer = new Timer();
            cursorBlinkTimer.Interval = 0.5;
            cursorBlinkTimer.Timeout += blinkCursor;

            AddedToGame += onAdded;
            Removed += onRemoved;
            
#if WINDOWS_PHONE
            AddedToGame += AddTouchListener;
#endif
        }

        private void onAdded()
        {
            cursorBlinkTimer.Start();
#if WINDOWS
            InputBoxes++;
            Keyboard.Buffer.Enabled = true;
            Keyboard.Buffer.BackspaceEnabled = true;
            Keyboard.Buffer.Clear();
            Keyboard.Buffer.TextChanged += updateText;
#endif
        }

        private void onRemoved()
        {
            cursorBlinkTimer.Stop();
#if WINDOWS
            InputBoxes--;
            Keyboard.Buffer.Enabled = InputBoxes > 0;
            Keyboard.Buffer.TextChanged -= updateText;
#endif
        }

        private void blinkCursor()
        {
            Cursor.Color = ( Cursor.Color != cursorColor ) ? cursorColor : Color.Transparent;
        }

        void UpdateCursorPosition()
        {
            Cursor.Left = Math.Min( -Width / 2 + XMargin + TextSize.X, Width / 2 - Font.CharacterWidth );
        }

#if WINDOWS
        private void updateText()
        {
            if ( !ControlContext.Active ) return;
            if ( Keyboard.Buffer.TextLength == 0 || Keyboard.Buffer.Text.Length == 0 ) return;

            bool changed = false;

            if ( Keyboard.Buffer.Text[0] == '\b' )
            {
                // Erase
                if ( remainingDelay <= 0 )
                {
                    if ( Text.Length > 0 )
                    {
                        Text = Text.Remove( Text.Length - 1 );
                        changed = true;
                    }
                    remainingDelay = InputBox.eraseDelay;
                }
            }
            else if ( Text.Length < MaxCharacters )
            {
                // Type
                Text += Keyboard.Buffer.Text;
                changed = true;
            }

            Keyboard.Buffer.Clear();

            UpdateCursorPosition();

            if ( changed )
                OnTextChanged();
        }
#endif

#if WINDOWS_PHONE
        void AddTouchListener()
        {
            Game.Instance.TouchPanel.ListenOn(this, ButtonState.Pressed, ShowTouchKeyboard, null).InContext( this );
        }

        void ShowTouchKeyboard( Touch touch )
        {
            if ( !Guide.IsVisible )
                Guide.BeginShowKeyboardInput( PlayerIndex.One, "", "", "", TouchTextEntered, this );
        }

        void TouchTextEntered( IAsyncResult result )
        {
            string typedText = Guide.EndShowKeyboardInput( result );
            if ( typedText != null )
            {
                Text = ( typedText.Length <= MaxCharacters ) ? typedText : typedText.Substring( 0, MaxCharacters );
                UpdateCursorPosition();
                OnTextChanged();
            }
        }
#endif

        /// <summary>
        /// Alustaa uuden syöttökentän.
        /// </summary>
        public InputBox()
            : this( 15 )
        {
        }

        public override void Update( Time time )
        {
#if WINDOWS
            if ( remainingDelay > 0 )
            {
                remainingDelay -= time.SinceLastUpdate.Ticks;
            }
#endif

            base.Update( time );
        }

        protected override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            if ( ! IsTruncated )
                base.Draw( parentTransformation, transformation, Text );
            else
            {
                String shownText = "";

                for ( int i = Text.Length - 1; i >= 0; i-- )
                {
                    String newText = Text[i] + shownText.ToString();

                    if ( Font.XnaFont.MeasureString( newText ).X >= Width )
                        break;

                    shownText = newText;
                }

                base.Draw( parentTransformation, transformation, shownText );
            }
        }
    }
}
