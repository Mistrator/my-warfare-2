using System;
using Microsoft.Xna.Framework;

#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.Reflection;
#endif


namespace Jypeli.WP7
{
    /// <summary>
    /// Puhelimen n�yt�n asemointi.
    /// </summary>
    public enum DisplayOrientation
    {
        /// <summary>
        /// Vaakasuuntainen. n�ytt� k��ntyy automaattisesti, jos puhelin k��nnet��n toisinp�in.
        /// </summary>
        Landscape,

        /// <summary>
        /// Vaakasuuntainen, vasemmalle k��nnetty.
        /// </summary>
        LandscapeLeft,

        /// <summary>
        /// Vaakasuuntainen, oikealle k��nnetty.
        /// </summary>
        LandscapeRight,

        /// <summary>
        /// Pystysuuntainen.
        /// </summary>
        Portrait,
    }

    /// <summary>
    /// Puhelimen n�yt�n tarkkuus.
    /// </summary>
    public enum DisplayResolution
    {
        /// <summary>
        /// Pieni tarkkuus. Paremi suorituskyky ja pienempi akun kulutus.
        /// </summary>
        Small,

        /// <summary>
        /// Suuri tarkkuus.
        /// </summary>
        Large,
    }    

    /// <summary>
    /// Aliohjelmia ja ominaisuuksia, jotka toimivat vain puhelimessa. Voidaan kutsua my�s PC:lle k��nnett�ess�,
    /// mutta t�ll�in mit��n ei yksinkertaisesti tapahdu.
    /// </summary>
    public class Phone
    {
        private DisplayOrientation _displayOrientation = DisplayOrientation.Landscape;
        private DisplayResolution _displayResolution = DisplayResolution.Small;

        /// <summary>
        /// Tapahtuu kun puhelin palaa tombstone-tilasta
        /// </summary>
        public event Action Activated;

        /// <summary>
        /// Tapahtuu kun puhelin siirtyy tombstone-tilaan
        /// </summary>
        public event Action Deactivated;

#if WINDOWS_PHONE
        private void OnActivated( object sender, ActivatedEventArgs args )
        {
            if ( Activated != null )
                Activated();
        }

        private void OnDeactivated( object sender, DeactivatedEventArgs args )
        {
            if ( Deactivated != null )
                Deactivated();
        }
#endif

        /// <summary>
        /// V�risytt�� puhelinta.
        /// </summary>
        /// <param name="milliSeconds">Aika kuinka kauan V�risytet��n millisekunteina</param>
        public void Vibrate(int milliSeconds)
        {
#if WINDOWS_PHONE
            Microsoft.Devices.VibrateController.Default.Start(TimeSpan.FromMilliseconds(milliSeconds));
#endif
        }

        /// <summary>
        /// Lopettaa puhelimen v�rin�n.
        /// </summary>
        public void StopVibrating()
        {
#if WINDOWS_PHONE
            Microsoft.Devices.VibrateController.Default.Stop();
#endif
        }

        private static void GetScreenSize(DisplayResolution resolution, out int width, out int height)
        {
            switch (resolution)
            {
                case DisplayResolution.Small:
                    width = 240;
                    height = 400;
                    break;
                default:
                    width = 480;
                    height = 800;
                    break;
            }
        }

        /// <summary>
        /// Puhelimen n�yt�n tarkkuus.
        /// </summary>
        public DisplayResolution DisplayResolution
        {
            get { return _displayResolution; }
            set
            {
                if (_displayResolution != value)
                {
                    _displayResolution = value;
                    ResetScreen(); 
                }
            }
        }

        /// <summary>
        /// Puhelimen N�yt�n asemointi.
        /// </summary>
        public DisplayOrientation DisplayOrientation
        {
            get { return _displayOrientation; }
            set
            {
                if (_displayOrientation != value)
                {
                    _displayOrientation = value;
                    Game.Instance.Accelerometer.DisplayOrientation = value;
                    ResetScreen();
                }
            }
        }

        private bool _tombstoning = false;

        /// <summary>
        /// Tallennetaanko pelin tilanne jos ruutu sammutetaan, vaihdetaan ohjelmaa tms.
        /// </summary>
        public bool Tombstoning
        {
            get { return _tombstoning; }
            set
            {
#if WINDOWS_PHONE
                if ( value && !_tombstoning )
                {
                    PhoneApplicationService.Current.Deactivated += SaveTombstone;
                    _tombstoning = true;
                }
                else if ( !value && _tombstoning )
                {
                    PhoneApplicationService.Current.Deactivated -= SaveTombstone;
                    _tombstoning = false;
                }
#endif
            }
        }

#if WINDOWS_PHONE
        internal Phone()
        {
            PhoneApplicationService.Current.Launching += NewGame;
            PhoneApplicationService.Current.Activated += LoadTombstone;
            PhoneApplicationService.Current.Activated += OnActivated;
            PhoneApplicationService.Current.Deactivated += OnDeactivated;
        }

        void NewGame( object sender, LaunchingEventArgs e )
        {
            Game.Instance.CallBegin();
        }

            internal void LoadTombstone( object sender, ActivatedEventArgs e )
            {
                Type gameType = Game.Instance.GetType();
                bool hasContinue = true; // gameType.GetMethod("Continue", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null;

                if ( hasContinue && e.IsApplicationInstancePreserved )
                    Game.Instance.Continue();
                else
                    Game.Instance.CallBegin();

                Game.Instance.LoadGame( "tombstone" );
            }

        internal void SaveTombstone( object sender, DeactivatedEventArgs e )
        {
            Game.Instance.SaveGame( "tombstone" );
        }
#endif


        internal void ResetScreen()
        {
#if WINDOWS_PHONE
            int screenWidth, screenHeight;
            GraphicsDeviceManager graphics = Game.GraphicsDeviceManager;
            GetScreenSize(_displayResolution, out screenWidth, out screenHeight);

            switch (_displayOrientation)
            {
                case DisplayOrientation.Landscape:
                    graphics.SupportedOrientations = Microsoft.Xna.Framework.DisplayOrientation.LandscapeLeft | Microsoft.Xna.Framework.DisplayOrientation.LandscapeRight;
                    Game.Instance.SetWindowSize(screenHeight, screenWidth);
                    break;
                case DisplayOrientation.LandscapeLeft:
                    graphics.SupportedOrientations = Microsoft.Xna.Framework.DisplayOrientation.LandscapeLeft;
                    Game.Instance.SetWindowSize(screenHeight, screenWidth);
                    break;
                case DisplayOrientation.LandscapeRight:
                    graphics.SupportedOrientations = Microsoft.Xna.Framework.DisplayOrientation.LandscapeRight;
                    Game.Instance.SetWindowSize(screenHeight, screenWidth);
                    break;
                case DisplayOrientation.Portrait:
                    graphics.SupportedOrientations = Microsoft.Xna.Framework.DisplayOrientation.Portrait;
                    Game.Instance.SetWindowSize(screenWidth, screenHeight);
                    break;
                default:
                    break;
            }

            graphics.ApplyChanges();
#endif
        }
    }
}
