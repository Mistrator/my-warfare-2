using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


using XnaGamepad = Microsoft.Xna.Framework.Input.GamePad;

namespace Jypeli.Controls
{
    /// <summary>
    /// Windows Phonen Back-nappi
    /// </summary>
    public class PhoneBackButton : Controller
    {
        internal override bool IsBufferEmpty()
        {
            return true;
        }

        internal override string GetControlText(Listener listener)
        {
            return "Phone's back button";
        }

        internal override void Update()
        {
            if (XnaGamepad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                bool listenerInvoked = false;

                foreach (Listener listener in listeners)
                {
                    if ( listener.Context.Active )
                    {
                        listener.Invoke();
                        listenerInvoked = true;
                    }
                }

                if ( !listenerInvoked && Game.Instance != null )
                {
                    // This is the main game context, and there are no listeners.
                    // Let's be good hosts and give our player an exit anyway.
                    Game.Instance.ConfirmExit();
                }
            }

            base.Update();
        }

        /// <summary>
        /// Kuuntelee Windows Phonen Back-nappia.
        /// </summary>
        /// <param name="handler">Aliohjelma joka suoritetaan kun painetaan nappia.</param>
        /// <param name="helpText">Teksti, joka kertoo mitä napin painallus tekee</param>
        /// <returns>Palauttaa kuuntelijan</returns>
        public Listener Listen(Handler handler, string helpText)
        {
            Listener l = new SimpleListener(this, ListeningType.PhoneButton, helpText, handler);
            //l.Button = button;
            //l.State = state;
            Add(l);
            return l;
        }

        /// <summary>
        /// Kuuntelee Windows Phonen Back-nappia
        /// </summary>
        /// <typeparam name="T1">Valinnaisen parametrin tyyppi</typeparam>
        /// <param name="handler">Aliohjelma joka suoritetaan kun painetaan nappia.</param>
        /// <param name="helpText">Teksti, joka kertoo mitä napin painallus tekee</param>
        /// <param name="p1">Valinnainen parametri</param>
        /// <returns>Palauttaa kuuntelijan</returns>
        public Listener Listen<T1>(Handler handler, string helpText, T1 p1)
        {
            Listener l = new SimpleListener(this, ListeningType.PhoneButton, helpText, handler);
            Add(l);
            return l;
        }

        /// <summary>
        /// Kuuntelee Windows Phonen Back-nappia
        /// </summary>
        /// <typeparam name="T1">Valinnaisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Valinnaisen parametrin tyyppi</typeparam>
        /// <param name="handler">Aliohjelma joka suoritetaan kun painetaan nappia.</param>
        /// <param name="helpText">Teksti, joka kertoo mitä napin painallus tekee</param>
        /// <param name="p1">Valinnainen parametri</param>
        /// <param name="p2">Valinnainen parametri</param>
        /// <returns>Palauttaa kuuntelijan</returns>
        public Listener Listen<T1, T2>(Handler handler, string helpText, T1 p1, T2 p2)
        {
            Listener l = new SimpleListener(this, ListeningType.PhoneButton, helpText, handler);
            Add(l);
            return l;
        }

        /// <summary>
        /// Kuuntelee Windows Phonen Back-nappia
        /// </summary>
        /// <typeparam name="T1">Valinnaisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Valinnaisen parametrin tyyppi</typeparam>
        /// <typeparam name="T3">Valinnaisen parametrin tyyppi</typeparam>
        /// <param name="handler">Aliohjelma joka suoritetaan kun painetaan nappia.</param>
        /// <param name="helpText">Teksti, joka kertoo mitä napin painallus tekee</param>
        /// <param name="p1">Valinnainen parametri</param>
        /// <param name="p2">Valinnainen parametri</param>
        /// <param name="p3">Valinnainen parametri</param>
        /// <returns>Palauttaa kuuntelijan</returns>
        public Listener Listen<T1, T2, T3>(Handler handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            Listener l = new SimpleListener(this, ListeningType.PhoneButton, helpText, handler);
            Add(l);
            return l;
        }

        /// <summary>
        /// Kuuntelee Windows Phonen Back-nappia
        /// </summary>
        /// <typeparam name="T1">Valinnaisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Valinnaisen parametrin tyyppi</typeparam>
        /// <typeparam name="T3">Valinnaisen parametrin tyyppi</typeparam>
        /// <typeparam name="T4">Valinnaisen parametrin tyyppi</typeparam>
        /// <param name="handler">Aliohjelma joka suoritetaan kun painetaan nappia.</param>
        /// <param name="helpText">Teksti, joka kertoo mitä napin painallus tekee</param>
        /// <param name="p1">Valinnainen parametri</param>
        /// <param name="p2">Valinnainen parametri</param>
        /// <param name="p3">Valinnainen parametri</param>
        /// <param name="p4">Valinnainen parametri</param>
        /// <returns>Palauttaa kuuntelijan</returns>
        public Listener Listen<T1, T2, T3, T4>(Handler handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            Listener l = new SimpleListener(this, ListeningType.PhoneButton, helpText, handler);
            Add(l);
            return l;
        }
    }
}
