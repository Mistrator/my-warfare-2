#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Physics2DDotNet;
using Jypeli.WP7;

using XnaKeyboard = Microsoft.Xna.Framework.Input.Keyboard;
using XnaMouse = Microsoft.Xna.Framework.Input.Mouse;
using XnaGamePad = Microsoft.Xna.Framework.Input.GamePad;

namespace Jypeli.Controls
{
    /// <summary>
    /// Ohjaintapahtumankäsittelijä ilman parametreja.
    /// </summary>
    public delegate void Handler();

    /// <summary>
    /// Ohjaintapahtumankäsittelijä yhdellä parametrilla.
    /// </summary>
    public delegate void Handler<T1>( T1 p1 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kahdella parametrilla.
    /// </summary>
    public delegate void Handler<T1, T2>( T1 p1, T2 p2 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kolmella parametrilla.
    /// </summary>
    public delegate void Handler<T1, T2, T3>( T1 p1, T2 p2, T3 p3 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä neljällä parametrilla.
    /// </summary>
    public delegate void Handler<T1, T2, T3, T4>( T1 p1, T2 p2, T3 p3, T4 p4 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä ilman parametreja.
    /// </summary>
    public delegate void MultiKeyHandler( List<Key> keys );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä yhdellä parametrilla.
    /// </summary>
    public delegate void MultiKeyHandler<T1>( List<Key> keys, T1 p1 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kahdella parametrilla.
    /// </summary>
    public delegate void MultiKeyHandler<T1, T2>( List<Key> keys, T1 p1, T2 p2 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kolmella parametrilla.
    /// </summary>
    public delegate void MultiKeyHandler<T1, T2, T3>( List<Key> keys, T1 p1, T2 p2, T3 p3 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä neljällä parametrilla.
    /// </summary>
    public delegate void MultiKeyHandler<T1, T2, T3, T4>( List<Key> keys, T1 p1, T2 p2, T3 p3, T4 p4 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä ilman parametreja.
    /// </summary>
    public delegate void AnalogHandler( AnalogState analogState );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä yhdellä parametrilla.
    /// </summary>
    public delegate void AnalogHandler<T1>( AnalogState analogState, T1 p1 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kahdella parametrilla.
    /// </summary>
    public delegate void AnalogHandler<T1, T2>( AnalogState analogState, T1 p1, T2 p2 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kolmella parametrilla.
    /// </summary>
    public delegate void AnalogHandler<T1, T2, T3>( AnalogState analogState, T1 p1, T2 p2, T3 p3 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä neljällä parametrilla.
    /// </summary>
    public delegate void AnalogHandler<T1, T2, T3, T4>( AnalogState analogState, T1 p1, T2 p2, T3 p3, T4 p4 );

    /// <summary>
    /// Ohjaintapahtumankäsittelijä ilman parametreja.
    /// </summary>
    public delegate void TouchHandler(Touch touch);

    /// <summary>
    /// Ohjaintapahtumankäsittelijä yhdellä parametrilla.
    /// </summary>
    public delegate void TouchHandler<T1>(Touch touch, T1 p1);

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kahdella parametrilla.
    /// </summary>
    public delegate void TouchHandler<T1, T2>(Touch touch, T1 p1, T2 p2);

    /// <summary>
    /// Ohjaintapahtumankäsittelijä kolmella parametrilla.
    /// </summary>
    public delegate void TouchHandler<T1, T2, T3>(Touch touch, T1 p1, T2 p2, T3 p3);

    /// <summary>
    /// Ohjaintapahtumankäsittelijä neljällä parametrilla.
    /// </summary>
    public delegate void TouchHandler<T1, T2, T3, T4>(Touch touch, T1 p1, T2 p2, T3 p3, T4 p4);

    /// <summary>
    /// Sisältää ohjaimet.
    /// </summary>
    public class Controls
    {
        List<Controller> controllers = new List<Controller>();

        /// <summary>
        /// Näppäimistö.
        /// </summary>
        public Keyboard Keyboard { get; private set; }

        /// <summary>
        /// Hiiri.
        /// </summary>
        public Mouse Mouse { get; private set; }

        /// <summary>
        /// Kiihtyvyysanturi (Windows Phone 7)
        /// </summary>
        public Accelerometer Accelerometer { get; private set; }

        /// <summary>
        /// Kosketusnäyttö (Windows Phone 7)
        /// </summary>
        public TouchPanel TouchPanel { get; private set; }

        /// <summary>
        /// Back-nappi (Windows Phone 7)
        /// </summary>
        public PhoneBackButton PhoneBackButton { get; private set; }

        public bool Enabled { get; set; }

        /// <summary>
        /// Peliohjaimet 1-4.
        /// </summary>
        public GamePad[] GameControllers { get; private set; }

        /// <summary>
        /// Kaikkien ohjainten puskurien tila.
        /// </summary>
        /// <value><c>false</c> jos näppäimenpainalluksia on jonossa käsittelemättä, muuten <c>true</c>.</value>
        internal bool BufferEmpty
        {
            get
            {
                // TODO: Apply to game pads and mouse as well
                return !Keyboard.Enabled || Microsoft.Xna.Framework.Input.Keyboard.GetState().GetPressedKeys().Length == 0;
            }
        }

        /// <summary>
        /// Luo uuden kontrolli-olion.
        /// Jos mahdollista, käytä mieluummin peliluokan omaa Controls-ominaisuutta.
        /// </summary>
        public Controls()
        {
            Enabled = true;
            Mouse = new Mouse();
            Keyboard = new Keyboard();
            Accelerometer = new Accelerometer();
            TouchPanel = new TouchPanel();
            PhoneBackButton = new PhoneBackButton();

            GameControllers = new GamePad[]
            {
                new GamePad( PlayerIndex.One ),
                new GamePad( PlayerIndex.Two ),
                new GamePad( PlayerIndex.Three ),
                new GamePad( PlayerIndex.Four ),
            };

            controllers.Add( Keyboard );
#if WINDOWS
            controllers.Add( Mouse );
#elif WINDOWS_PHONE
            controllers.Add( Accelerometer );
            controllers.Add( TouchPanel );
            controllers.Add( PhoneBackButton );
#endif

#if WINDOWS || XBOX
            controllers.AddRange( GameControllers );
#endif
        }

        internal void Update()
        {
            if ( !Enabled )
            {
                return;
            }

            foreach ( Controller controller in controllers )
            {
                controller.Update();
            }
        }

        /// <summary>
        /// Tyhjentää kaikkien peliohjaimien ohjauspuskurit, eli jonossa odottavat
        /// näppäimenpainallukset ja muut ohjaintapahtumat.
        /// </summary>
        public void PurgeBuffer()
        {
            foreach ( Controller controller in controllers )
            {
                controller.PurgeBuffer();
            }
        }

        /// <summary>
        /// Palauttaa kaikki ohjaimien ohjetekstit listana.
        /// </summary>
        /// <returns></returns>
        public List<String> GetHelpTexts()
        {
            var texts = new List<string>();

            foreach ( Controller controller in controllers )
            {
                if ( controller is GamePad )
                    continue;

                controller.GetHelpTexts( texts );
            }

            // Assuming here that the other three gamepads would have the same texts.
            GameControllers[0].GetHelpTexts( texts );

            return texts;
        }

        /// <summary>
        /// Tyhjentää kaikki kontrollit.
        /// </summary>
        public void Clear()
        {
            foreach ( Controller controller in controllers )
            {
                controller.Clear();
            }

            // helpTexts.Clear();
        }
    }
}
