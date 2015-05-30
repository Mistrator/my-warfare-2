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
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

#if XBOX
using Microsoft.Xna.Framework.GamerServices;
#endif

using Jypeli.Widgets;
using Jypeli.Effects;
using Jypeli.Controls;
using Jypeli.WP7;

using XnaColor = Microsoft.Xna.Framework.Color;
using XnaSoundEffect = Microsoft.Xna.Framework.Audio.SoundEffect;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using System.Reflection;
using System.Diagnostics;


namespace Jypeli
{
    /// <summary>
    /// Peliluokka reaaliaikaisille peleille.
    /// </summary>
    [Save]
    public class Game : Microsoft.Xna.Framework.Game, ControlContexted, IDisposable
    {
        Queue<Action> PendingActions = new Queue<Action>();

        /// <summary>
        /// Onko peli pysähdyksissä.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Kerrokset, joilla pelioliot viihtyvät.
        /// </summary>
        public SynchronousList<Layer> Layers { get; private set; }

        /// <summary>
        /// Kerrokset, joilla olevat pelioliot eivät liiku kameran mukana.
        /// </summary>
        public IList<Layer> StaticLayers
        {
            get { return Layers.FindAll( l => l.IgnoresZoom && l.RelativeTransition == Vector.Zero ).AsReadOnly(); }
        }

        /// <summary>
        /// Kerrokset, joilla olevat pelioliot liikkuvat kameran mukana.
        /// </summary>
        public IList<Layer> DynamicLayers
        {
            get { return Layers.FindAll( l => !l.IgnoresZoom || l.RelativeTransition != Vector.Zero ).AsReadOnly(); }
        }

        /// <summary>
        /// Pienin mahdollinen kerros.
        /// </summary>
        public int MinLayer
        {
            get { return Layers.FirstIndex; }
        }

        /// <summary>
        /// Suurin mahdollinen kerros.
        /// </summary>
        public int MaxLayer
        {
            get { return Layers.LastIndex; }
        }

        /// <summary>
        /// Kerrosten määrä.
        /// </summary>
        public int LayerCount
        {
            get { return Layers.Count; }
        }

        /// <summary>
        /// Pelin nimi.
        /// </summary>
        public static string Name { get; private set; }

        // TJ: Since there is only one isntance of the
        // game class in practice, its useful to have it available as
        // a static member. This way, we avoid passing a reference to the
        // game in lots of places.
        //
        // In addition, some often-used things, such as Time or GraphicsDevice,
        // are available as static properties (public or internal) as well.
        public static Game Instance { get; private set; }

        /// <summary>
        /// Tapahtuu kun Game.Instance on alustettu.
        /// </summary>
        public static event Action InstanceInitialized;

        /// <summary>
        /// Tapahtuu kun peli lopetetaan.
        /// </summary>
        public static new event Action Exiting;

        /// <summary>
        /// Kamera, joka näyttää ruudulla näkyvän osan kentästä.
        /// Kameraa voidaan siirtää, zoomata tai asettaa seuraamaan tiettyä oliota.
        /// </summary>
        [Save]
        public Camera Camera { get; set; }

        /// <summary>
        /// Kentän reunat näkyvissä tai pois näkyvistä.
        /// Huomaa, että tämä ominaisuus ei vaikuta reunojen törmäyskäsittelyyn.
        /// </summary>
        public bool DrawPerimeter { get; set; }

        /// <summary>
        /// Väri, jolla kentän reunat piirretään.
        /// </summary>
        public Color PerimeterColor { get; set; }

        /// <summary>
        /// Tekstuurien (kuvien) reunanpehmennys skaalattaessa (oletus päällä).
        /// </summary>
        public static bool SmoothTextures { get; set; }

        /// <summary>
        /// Kirjaston mukana tuleva sisältö.
        /// Voidaan käyttää esimerkiksi tekstuurien lataamiseen.
        /// </summary>
        internal static ResourceContentManager ResourceContent { get; private set; }

        /// <summary>
        /// Näytön dimensiot, eli koko ja reunat.
        /// </summary>
        public static ScreenView Screen
        {
            get { return Instance.screen; }
        }

        /// <summary>
        /// Pelin kontrollit.
        /// </summary>
        public static Jypeli.Controls.Controls Controls
        {
            get { return Instance.controls; }
        }

        private ListenContext _context = new ListenContext() { Active = true };

        /// <summary>
        /// Pelin pääohjainkonteksti.
        /// </summary>
        public ListenContext ControlContext
        {
            get { return Instance._context; }
        }

        public bool IsModal
        {
            get { return false; }
        }

        /// <summary>
        /// Viestinäyttö, johon voi laittaa viestejä.
        /// </summary>
        /// <value>Viestinäyttö.</value>
        public MessageDisplay MessageDisplay { get; set; }

        /// <summary>
        /// Tietovarasto, johon voi tallentaa tiedostoja pidempiaikaisesti.
        /// Sopii esimerkiksi pelitilanteen lataamiseen ja tallentamiseen.
        /// </summary>
        public static FileManager DataStorage { get { return Instance.dataStorage; } }

        /// <summary>
        /// Onko olio valittavissa.
        /// Vain valittu (fokusoitu) olio voii kuunnella näppäimistöä ja muita ohjainlaitteita.
        /// Peliolio on aina valittavissa.
        /// </summary>
        public bool AcceptsFocus { get { return true; } }

        /// <summary>
        /// Näppäimistö.
        /// </summary>
        public Keyboard Keyboard { get { return controls.Keyboard; } }

        /// <summary>
        /// Hiiri.
        /// </summary>
        public Mouse Mouse { get { return controls.Mouse; } }

        /// <summary>
        /// Kosketusnäyttö. Vain kännykässä.
        /// </summary>
        public TouchPanel TouchPanel { get { return controls.TouchPanel; } }

        public PhoneBackButton PhoneBackButton { get { return controls.PhoneBackButton; } }

        /// <summary>
        /// Peliohjain yksi.
        /// </summary>
        public GamePad ControllerOne { get { return controls.GameControllers[0]; } }

        /// <summary>
        /// Peliohjain kaksi.
        /// </summary>
        public GamePad ControllerTwo { get { return controls.GameControllers[1]; } }

        /// <summary>
        /// Peliohjain kolme.
        /// </summary>
        public GamePad ControllerThree { get { return controls.GameControllers[2]; } }

        /// <summary>
        /// Peliohjain neljä.
        /// </summary>
        public GamePad ControllerFour { get { return controls.GameControllers[3]; } }

        /// <summary>
        /// Kiihtyvyysanturi. Vain kännykässä.
        /// </summary>
        public Accelerometer Accelerometer { get { return controls.Accelerometer; } }

        private Phone phone;

        /// <summary>
        /// Phone-olio esim. puhelimen tärisyttämiseen.
        /// </summary>
        public Phone Phone { get { return phone; } }


        /// <summary>
        /// Aktiivinen kenttä.
        /// </summary>
        public Level Level
        {
            get { return theLevel; }
        }

        /// <summary>
        /// Peliaika. Sisältää tiedon siitä, kuinka kauan peliä on pelattu (Time.SinceStartOfGame)
        /// ja kuinka kauan on viimeisestä pelin päivityksestä (Time.SinceLastUpdate).
        /// Tätä päivitetään noin 30 kertaa sekunnissa kun peli ei ole pause-tilassa.
        /// </summary>
        public static Time Time
        {
            get { return currentTime; }
        }

        /// <summary>
        /// Todellinen peliaika. Sisältää tiedon siitä, kuinka kauan peliä on pelattu (Time.SinceStartOfGame)
        /// ja kuinka kauan on viimeisestä pelin päivityksestä (Time.SinceLastUpdate).
        /// Tätä päivitetään noin 30 kertaa sekunnissa, myös pause-tilassa.
        /// </summary>
        public static Time RealTime
        {
            get { return currentTime; }
        }

        /// <summary>
        /// Tuuli. Vaikuttaa vain efekteihin
        /// </summary>
        public static Vector Wind { get; set; }

        /// <summary>
        /// Teksti, joka näkyy pelin ikkunassa (jos peli ei ole koko ruudun tilassa).
        /// </summary>
        public string Title
        {
            get { return Window.Title; }
            set { Window.Title = value; }
        }

        /// <summary>
        /// Kuinka monta pelioliota pelissä on (ei laske widgettejä).
        /// </summary>
        internal int ObjectCount
        {
            get
            {
                return Layers.Sum<Layer>( l => l.Objects.Count );
            }
        }

        /// <summary>
        /// Onko peli kokoruututilassa.
        /// </summary>
        public bool IsFullScreen
        {
            get { return isFullScreenRequested; }
            set
            {
                if ( GraphicsDevice == null )
                {
                    // GraphicsDevice is not initialized yet.
                    isFullScreenRequested = value;
                }
                else if ( ( GraphicsDeviceManager.IsFullScreen != value ) )
                {
#if WINDOWS_PHONE
                    Phone.ResetScreen();
#else
                    SetWindowSize( GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height, value );
#endif
                }
            }
        }

        /// <summary>
        /// Mediasoitin.
        /// </summary>
        public MediaPlayer MediaPlayer { get; private set; }

        private Jypeli.Controls.Controls controls;
        private FileManager dataStorage;
        private ScreenView screen;

        [EditorBrowsable( EditorBrowsableState.Never )]
        public static new GraphicsDevice GraphicsDevice
        {
            get { return ( (Microsoft.Xna.Framework.Game)Instance ).GraphicsDevice; }
        }

        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

#if !WINDOWS_PHONE
        internal static List<Light> Lights { get { return lights; } }
#endif

#if DEBUG
        private BarGauge[] objectCountDisplays;
        private Label fpsDisplay;
        private string fpsText = "00";
        private int fpsSkipCounter;
        private bool isDebugScreenShown = false;
#endif

#if !WINDOWS_PHONE
        private static List<Light> lights = new List<Light>();
#endif

        private Level theLevel;

        private bool loadContentHasBeenCalled = false;
        private bool beginHasBeenCalled = false;

        // fullscreen isn't used as default, because debug mode doesn't work well with it
        private bool isFullScreenRequested = false;

        // Real time passed, including paused time
        private static Time currentRealTime = new Time();

        // Game time passed
        private static Time currentTime = new Time();


        /// <summary>
        /// Alustaa uuden peliluokan.
        /// </summary>
        public Game()
            : this( 1 )
        {
        }

        /// <summary>
        /// Alustaa uuden peliluokan.
        /// </summary>
        /// <param name="device">Mikä monitori käytössä, 1=ensimmäinen</param>
        public Game( int device )
        {
            if ( Instance != null )
                throw new Exception( "Only one instance of the Game class can be created." );

            Instance = this;
            Name = this.GetType().Assembly.FullName.Split( ',' )[0];

            InitializeLayers();
            InitializeContent();

            Camera = new Camera();
            controls = new Jypeli.Controls.Controls();

            InitializeGraphics( device );

#if WINDOWS
            this.dataStorage = new WindowsFileManager( WindowsLocation.DataPath, WindowsLocation.MyDocuments ); ;
#elif WINDOWS_PHONE
            dataStorage = new IsolatedStorageManager();
#elif XBOX
            Components.Add( new GamerServicesComponent( this ) );
            dataStorage = new XboxFileManager();
#endif

            dataStorage.ReadAccessDenied += delegate (Exception ex)
            {
                ShowErrorMessage("Could not read from data directory. " + ex.Message);
            };
            dataStorage.WriteAccessDenied += delegate (Exception ex)
            {
                ShowErrorMessage("Could not write to data directory. " + ex.Message);
            };

            DrawPerimeter = false;
            PerimeterColor = Color.DarkGray;

            phone = new Phone();
            Phone.Tombstoning = true;
        }

        private void ShowErrorMessage(string message)
        {
            MessageDisplay.Add( "ERROR: " + message, Color.Red );

            /*bool mouseVisible = IsMouseVisible;
            IsMouseVisible = true;

            MessageWindow errWindow = new MessageWindow( message );
            errWindow.Color = Color.Red;
            errWindow.Message.TextColor = Color.White;
            errWindow.Closed += delegate { IsMouseVisible = mouseVisible; };
            Add( errWindow );*/
        }

        private void InitializeLayers()
        {
            Layers = new SynchronousList<Layer>( -3 );
            Layers.ItemAdded += OnLayerAdded;
            Layers.ItemRemoved += OnLayerRemoved;

            for ( int i = 0; i < 7; i++ )
            {
                Layers.Add( new Layer() );
            }

            // This is the widget layer
            Layers.Add( Layer.CreateStaticLayer() );

            Layers.UpdateChanges();
        }

        private void InitializeContent()
        {
#if WINDOWS_PHONE
            
            ResourceContent = new ResourceContentManager( this.Services, WindowsPhoneResources.ResourceManager );
#elif XBOX
            ResourceContent = new ResourceContentManager( this.Services, XBox360Resources.ResourceManager );
#else
            ResourceContent = new ResourceContentManager( this.Services, Resources.ResourceManager );
#endif

            Content.RootDirectory = "Content";
        }

        private void InitializeGraphics( int device )
        {
#if WINDOWS
            if ( device == 1 )
                GraphicsDeviceManager = new GraphicsDeviceManager( this );
            else
                GraphicsDeviceManager = new TargetedGraphicsDeviceManager( this, device );
#else
            GraphicsDeviceManager = new GraphicsDeviceManager( this );
#endif
            GraphicsDeviceManager.PreferredDepthStencilFormat = Jypeli.Graphics.SelectStencilMode();
            SmoothTextures = true;
        }

        private void ActivateObject( ControlContexted obj )
        {
            obj.ControlContext.Active = true;

            if ( obj.IsModal )
            {
                Game.Instance.ControlContext.SaveFocus();
                Game.Instance.ControlContext.Active = false;

                foreach ( Layer l in Layers )
                {
                    foreach ( IGameObject lo in l.Objects )
                    {
                        ControlContexted co = lo as ControlContexted;
                        if ( lo == obj || co == null )
                            continue;

                        co.ControlContext.SaveFocus();
                        co.ControlContext.Active = false;
                    }
                }
            }
        }

        private void DeactivateObject( ControlContexted obj )
        {
            obj.ControlContext.Active = false;

            if ( obj.IsModal )
            {
                Game.Instance.ControlContext.RestoreFocus();

                foreach ( Layer l in Layers )
                {
                    foreach ( IGameObject lo in l.Objects )
                    {
                        ControlContexted co = lo as ControlContexted;
                        if ( lo == obj || co == null )
                            continue;

                        co.ControlContext.RestoreFocus();
                    }
                }
            }
        }

        protected virtual void OnObjectAdded( IGameObject obj )
        {
            IGameObjectInternal iObj = obj as IGameObjectInternal;
            if ( iObj == null ) return;
            iObj.IsAddedToGame = true;
            iObj.OnAddedToGame();

            ControlContexted cObj = obj as ControlContexted;
            if ( cObj != null ) ActivateObject( cObj );
        }

        protected virtual void OnObjectRemoved( IGameObject obj )
        {
            IGameObjectInternal iObj = obj as IGameObjectInternal;
            if ( iObj == null ) return;
            iObj.IsAddedToGame = false;
            iObj.OnRemoved();

            ControlContexted cObj = obj as ControlContexted;
            if ( cObj != null ) DeactivateObject( cObj );
        }

        internal static void OnAddObject( IGameObject obj )
        {
            Debug.Assert( Instance != null );
            Instance.OnObjectAdded( obj );
        }

        internal static void OnRemoveObject( IGameObject obj )
        {
            Debug.Assert( Instance != null );
            Instance.OnObjectRemoved( obj );
        }

        private void OnLayerAdded( Layer l )
        {
            l.Objects.ItemAdded += this.OnObjectAdded;
            l.Objects.ItemRemoved += this.OnObjectRemoved;
        }

        private void OnLayerRemoved( Layer l )
        {
            l.Objects.ItemAdded -= this.OnObjectAdded;
            l.Objects.ItemRemoved -= this.OnObjectRemoved;
        }

        /// <summary>
        /// Suorittaa aliohjelman kun peli on varmasti alustettu.
        /// </summary>
        /// <param name="actionMethod">Suoritettava aliohjelma.</param>
        public static void AssertInitialized( Action actionMethod )
        {
            if ( Instance != null )
                actionMethod();
            else
                InstanceInitialized += actionMethod;
        }

        /// <summary>
        /// Suorittaa aliohjelman seuraavalla päivityksellä.
        /// </summary>
        /// <param name="action"></param>
        public static void DoNextUpdate( Action action )
        {
            if ( Instance != null )
                Instance.PendingActions.Enqueue( action );
            else
                InstanceInitialized += action;
        }

        /// <summary>
        /// Suorittaa aliohjelman seuraavalla päivityksellä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        public static void DoNextUpdate<T1>( Action<T1> action, T1 p1 )
        {
            DoNextUpdate( delegate { action( p1 ); } );
        }

        /// <summary>
        /// Suorittaa aliohjelman seuraavalla päivityksellä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public static void DoNextUpdate<T1, T2>( Action<T1, T2> action, T1 p1, T2 p2 )
        {
            DoNextUpdate( delegate { action( p1, p2 ); } );
        }

        /// <summary>
        /// Suorittaa aliohjelman kun peli on varmasti alustettu.
        /// </summary>
        /// <param name="actionMethod">Suoritettava aliohjelma.</param>
        public static void AssertInitialized<T1>( Action<T1> actionMethod, T1 o1 )
        {
            if ( Instance != null )
                actionMethod( o1 );
            else
                InstanceInitialized += delegate { actionMethod( o1 ); };
        }

        /// <summary>
        /// Lisää olion peliin.
        /// Tavalliset oliot tulevat automaattisesti kerrokselle 0
        /// ja ruutuoliot päällimmäiselle kerrokselle.
        /// </summary>
        public void Add( IGameObject o )
        {
            if ( o.Layer != null && o.Layer.Objects.WillContain( o ) )
            {
                if ( o.Layer == Layers[0] )
                {
                    throw new NotSupportedException( "Object cannot be added twice" );
                }
                else
                    throw new NotSupportedException( "Object cannot be added to multiple layers" );
            }

            if ( o is Widget ) Add( o, MaxLayer );
            else Add( o, 0 );
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Lisää valon peliin. Nykyisellään valoja voi olla ainoastaan
        /// yksi kappale.
        /// </summary>
        public void Add( Light light )
        {
            if ( light == null ) throw new NullReferenceException( "Tried to add a null light to game" );

            if ( lights.Count >= 1 )
                throw new NotSupportedException( "Only one light is supported" );

            lights.Add( light );
        }
#endif

        /// <summary>
        /// Lisää peliolion peliin, tiettyyn kerrokseen.
        /// </summary>
        /// <param name="o">Lisättävä olio.</param>
        /// <param name="layer">Kerros, luku väliltä [-3, 3].</param>
        public virtual void Add( IGameObject o, int layer )
        {
            if ( o == null ) throw new NullReferenceException( "Tried to add a null object to game" );
            Layers[layer].Add( o );
        }

        internal static IList<IGameObject> GetObjectsAboutToBeAdded()
        {
            List<IGameObject> result = new List<IGameObject>();

            foreach ( Layer layer in Game.Instance.Layers )
            {
                layer.GetObjectsAboutToBeAdded( result );
            }

            return result;
        }

        /// <summary>
        /// Lisää oliokerroksen peliin.
        /// </summary>
        /// <param name="l"></param>
        public void Add( Layer l )
        {
            Layers.Add( l );
            Layers.UpdateChanges();
        }

        /// <summary> 
        /// Poistaa olion pelistä. Jos haluat tuhota olion, 
        /// kutsu mielummin olion <c>Destroy</c>-metodia. 
        /// </summary> 
        /// <remarks> 
        /// Oliota ei poisteta välittömästi, vaan viimeistään seuraavan 
        /// päivityksen jälkeen. 
        /// </remarks> 
        public void Remove( IGameObject o )
        {
            if ( !o.IsAddedToGame )
                return;

            foreach ( Layer l in Layers )
                l.Remove( o );
        }

        /// <summary>
        /// Poistaa oliokerroksen pelistä.
        /// </summary>
        /// <param name="l"></param>
        public void Remove( Layer l )
        {
            Layers.Remove( l );
            Layers.UpdateChanges();
        }

        /// <summary>
        /// Palauttaa listan kaikista peliolioista jotka toteuttavat ehdon.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="condition">Ehto</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjects( Predicate<GameObject> condition )
        {
            List<GameObject> objs = new List<GameObject>();

            for ( int i = MaxLayer; i >= MinLayer; i-- )
            {
                foreach ( var obj in Layers[i].Objects )
                {
                    GameObject gobj = obj as GameObject;

                    if ( gobj != null && condition( gobj ) )
                        objs.Add( gobj );
                }
            }

            return objs;
        }

        /// <summary>
        /// Palauttaa listan kaikista fysiikkaolioista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="condition">Ehto</param>
        /// <returns>Lista olioista</returns>
        public List<PhysicsObject> GetPhysicsObjects()
        {
            List<PhysicsObject> objs = new List<PhysicsObject>();

            for (int i = MaxLayer; i >= MinLayer; i--)
            {
                foreach (var obj in Layers[i].Objects)
                {
                    GameObject gobj = obj as GameObject;

                    if (gobj != null && gobj is PhysicsObject)
                        objs.Add(gobj as PhysicsObject);
                }
            }

            return objs;
        }

        /// <summary>
        /// Palauttaa listan kaikista peliolioista joilla on tietty tagi.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="tags">Tagi(t)</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsWithTag( params string[] tags )
        {
            return GetObjects( o => tags.Contains<string>( o.Tag as string ) );
        }

        /// <summary>
        /// Palauttaa ensimmäisen peliolion joka toteuttaa ehdon (null jos mikään ei toteuta).
        /// </summary>
        /// <param name="condition">Ehto</param>
        /// <returns>Olio</returns>
        public GameObject GetFirstObject( Predicate<GameObject> condition )
        {
            for ( int i = MaxLayer; i >= MinLayer; i-- )
            {
                foreach ( var obj in Layers[i].Objects )
                {
                    GameObject gobj = obj as GameObject;

                    if ( gobj != null && condition( gobj ) )
                        return gobj;
                }
            }

            return null;
        }

        /// <summary>
        /// Palauttaa ensimmäisen ruutuolion joka toteuttaa ehdon (null jos mikään ei toteuta).
        /// </summary>
        /// <param name="condition">Ehto</param>
        /// <returns>Lista olioista</returns>
        public Widget GetFirstWidget( Predicate<Widget> condition )
        {
            return (Widget)GetFirstObject( obj => obj is Widget && condition( (Widget)obj ) );
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position )
        {
            return GetObjects( obj => obj.IsInside( position ) );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan päällimmäinen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position )
        {
            return GetFirstObject( obj => obj.IsInside( position ) && !(obj is MessageDisplay) );
        }

        /// <summary>
        /// Palauttaa ruutuolion, joka on annetussa paikassa.
        /// Jos paikassa ei ole mitään oliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan päällimmäinen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <returns>Mahdollinen ruutuolio</returns>
        public Widget GetWidgetAt( Vector position )
        {
            return (Widget)GetFirstObject( obj => obj is Widget && obj.IsInside( position ) && !( obj is MessageDisplay ) );
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position, double radius )
        {
            Predicate<GameObject> isInsideRadius = delegate ( GameObject obj )
            {
                if ( obj is MessageDisplay ) return false;

                Vector positionUp = new Vector( position.X, position.Y + radius );
                Vector positionDown = new Vector( position.X, position.Y - radius );
                Vector positionLeft = new Vector( position.X - radius, position.Y );
                Vector positionRight = new Vector( position.X + radius, position.Y );

                if ( obj.IsInside( position ) ) return true;
                if ( obj.IsInside( positionUp ) ) return true;
                if ( obj.IsInside( positionDown ) ) return true;
                if ( obj.IsInside( positionLeft ) ) return true;
                if ( obj.IsInside( positionRight ) ) return true;

                return false;
            };

            return GetObjects( isInsideRadius );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan ensin lisätty.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position, double radius )
        {
            var objs = GetObjectsAt( position, radius );
            return objs.Count > 0 ? objs[0] : null;
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position, object tag )
        {
            return GetObjectsAt( position ).FindAll( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan ensin lisätty.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position, object tag )
        {
            return GetObjectsAt( position ).Find( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position, object tag, double radius )
        {
            return GetObjectsAt( position, radius ).FindAll<GameObject>( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa tietyllä säteellä.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan ensin lisätty.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position, object tag, double radius )
        {
            return GetObjectsAt( position, radius ).Find( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Lataa animaation contentista.
        /// </summary>
        /// <param name="name">Animaation nimi (ei tarkennetta)</param>
        /// <returns>Animation-olio</returns>
        public static Animation LoadAnimation( string name )
        {
            return Instance.Content.Load<Animation>( name );
        }

        /// <summary>
        /// Lataa kuvan contentista.
        /// </summary>
        /// <param name="name">Kuvan nimi (ei tarkennetta)</param>
        /// <returns>Image-olio</returns>
        public static Image LoadImage( string name )
        {
            return new Image( name );
        }

        /// <summary>
        /// Lataa taulukon kuvia contentista.
        /// </summary>
        /// <param name="name">Kuvien nimet ilman tarkennetta pilkuin eroiteltuna</param>
        /// <returns>Taulukko Image-olioita</returns>
        public static Image[] LoadImages( params string[] names )
        {
            Image[] result = new Image[names.Length];
            for ( int i = 0; i < names.Length; i++ )
                result[i] = LoadImage( names[i] );
            return result;
        }

        /// <summary>
        /// Lataa taulukon kuvia contentista.
        /// </summary>
        /// <param name="baseName">Ennen numeroa tuleva osa nimestä.</param>
        /// <param name="startIndex">Ensimmäisen kuvan numero.</param>
        /// <param name="endIndex">Viimeisen kuvan numero.</param>
        /// <param name="zeroPad">Onko numeron edessä täytenollia.</param>
        /// <returns></returns>
        public static Image[] LoadImages( string baseName, int startIndex, int endIndex, bool zeroPad = false )
        {
            if ( startIndex > endIndex ) throw new ArgumentException("Starting index must be smaller than ending index.");

            Image[] result = new Image[endIndex - startIndex];
            string format;

            if ( zeroPad )
            {
                int digits = endIndex.ToString().Length;
                format = "{0}{1:" + "0".Repeat( digits ) + "}";
            }
            else
            {
                format = "{0}{1}";
            }

            for ( int i = startIndex; i < endIndex; i++ )
            {
                string imgName = String.Format( format, baseName, i );
                result[i - startIndex] = LoadImage( imgName );
            }

            return result;
        }

        /// <summary>
        /// Soittaa ääniefektin.
        /// </summary>
        /// <param name="name">Äänen nimi (ei tarkennetta)</param>
        public static void PlaySound( string name )
        {
            LoadSoundEffect( name ).Play();
        }

        /// <summary>
        /// Lataa ääniefektin contentista.
        /// </summary>
        /// <param name="name">Äänen nimi (ei tarkennetta)</param>
        /// <returns>SoundEffect-olio</returns>
        public static SoundEffect LoadSoundEffect( string name )
        {
            return new SoundEffect( name );
        }

        /// <summary>
        /// Lataa taulukon ääniefektejä contentista.
        /// </summary>
        /// <param name="names">Äänien nimet ilman tarkennetta pilkuin eroiteltuna</param>
        /// <returns>Taulukko SoundEffect-olioita</returns>
        public static SoundEffect[] LoadSoundEffects( params string[] names )
        {
            SoundEffect[] result = new SoundEffect[names.Length];
            for ( int i = 0; i < names.Length; i++ )
                result[i] = LoadSoundEffect( names[i] );
            return result;
        }

        /// <summary>
        /// Poistaa kaikki ajastimet.
        /// </summary>
        public void ClearTimers()
        {
            Timer.ClearAll();
        }

        /// <summary>
        /// Nollaa kaiken.
        /// </summary>
        public virtual void ClearAll()
        {
            Level.Clear();
            ResetLayers();
            ClearTimers();
#if !WINDOWS_PHONE
            ClearLights();
#endif
            ClearControls();
            GC.Collect();
            addMessageDisplay();
            ControlContext.Enable();
        }

        /// <summary>
        /// Nollaa oliokerrokset. Huom. tuhoaa kaikki pelioliot!
        /// </summary>
        /// <param name="l"></param>
        public void ResetLayers()
        {
            ClearGameObjects();
            InitializeLayers();
        }

        /// <summary>
        /// Poistaa kaikki oliokerrokset. Huom. tuhoaa kaikki pelioliot!
        /// </summary>
        /// <param name="l"></param>
        public void RemoveAllLayers()
        {
            ClearGameObjects();
            Layers.Clear();
        }

        /// <summary>
        /// Palauttaa kontrollit alkutilaansa.
        /// </summary>
        public void ClearControls()
        {
            controls.Clear();
            TouchPanel.Clear();
#if DEBUG && !WINDOWS_PHONE
            Keyboard.Listen( Key.F12, ButtonState.Pressed, ToggleDebugScreen, null );
#endif
        }

#if DEBUG
        private void ToggleDebugScreen()
        {
            if ( isDebugScreenShown )
                isDebugScreenShown = false;
            else
                isDebugScreenShown = true;
        }
#endif

        /// <summary>
        /// Tuhoaa ja poistaa pelistä kaikki pelioliot (ml. fysiikkaoliot).
        /// </summary>
        public void ClearGameObjects()
        {
            foreach ( var layer in Layers )
                layer.Clear();

            addMessageDisplay();
        }

#if !WINDOWS_PHONE
        public void ClearLights()
        {
            lights.Clear();
        }
#endif

        /// <summary>
        /// Ajetaan Updaten sijaan kun peli on pysähdyksissä.
        /// </summary>
        protected virtual void PausedUpdate( Time time )
        {
            foreach ( var layer in Layers )
            {
                // Update the UI components only
                layer.Objects.Update( time, o => o is Widget );
            }

            Timer.UpdateAll( time, t => t.IgnorePause );
        }

        /// <summary>
        /// Ajetaan kun pelin tilannetta päivitetään. Päivittämisen voi toteuttaa perityssä luokassa
        /// toteuttamalla tämän metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
        /// </summary>
        protected virtual void Update( Time time )
        {
            this.Camera.Update( time );
            Layers.Update( time );
            Timer.UpdateAll( time );

            while (PendingActions.Count > 0)
            {
                PendingActions.Dequeue()();
            }
        }
        /// <summary>
        /// This gets called after the GraphicsDevice has been created. So, this is
        /// the place to initialize the resources needed in the game. Except the graphics content,
        /// which should be called int LoadContent(), according to the XNA docs.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Initialize()
        {
            Jypeli.Graphics.Initialize();

#if WINDOWS_PHONE
            isFullScreenRequested = true;
            Phone.ResetScreen();
#else
            SetWindowSize( GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height, isFullScreenRequested );
#endif

            screen = new ScreenView( GraphicsDevice.Viewport );

            theLevel = new Level( this );

            MediaPlayer = new MediaPlayer( Content );

            addMessageDisplay();

#if DEBUG
            double barWidth = 20;
            double barHeight = Screen.Height;
            fpsDisplay = new Label( "00" );
            fpsDisplay.Color = Color.Gray;
            fpsDisplay.X = Level.Right - barWidth / 2 - fpsDisplay.Width;
            fpsDisplay.Y = Screen.Top - fpsDisplay.Height / 2;

            double left = Screen.Right - Layers.Count * barWidth;

            objectCountDisplays = new BarGauge[Layers.Count];

            for ( int i = 0; i < Layers.Count; i++ )
            {
                var gauge = new BarGauge( barWidth, Screen.Height );
                gauge.X = left + i * barWidth + barWidth / 2;
                gauge.Y = Screen.Center.Y;
                gauge.BarColor = Color.DarkGreen;
                gauge.BindTo( Layers[i + Layers.FirstIndex].ObjectCount );
                objectCountDisplays[i] = gauge;
            }

            Keyboard.Listen( Key.F12, ButtonState.Pressed, ToggleDebugScreen, null );
#endif

            base.Initialize();
        }

        /// <summary>
        /// XNA calls this when graphics resources need to be loaded.
        /// Note that this can be called multiple times (whenever the graphics device is reset).
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void LoadContent()
        {
            if ( !loadContentHasBeenCalled )
            {
                if ( InstanceInitialized != null )
                    InstanceInitialized();

#if !WINDOWS_PHONE
                CallBegin();
#endif
                loadContentHasBeenCalled = true;
            }

#if DEBUG && !WINDOWS_PHONE
            MessageDisplay.Add( "F12 - Debug view" );
#endif
            base.LoadContent();

            GC.Collect();
        }

        /// <summary>
        /// Aloittaa pelin kutsumalla Begin-metodia.
        /// Tärkeää: kutsu tätä, älä Beginiä suoraan, sillä muuten peli ei päivity!
        /// </summary>
        internal void CallBegin()
        {
            Begin();
            beginHasBeenCalled = true;
        }

        /// <summary>
        /// Tässä alustetaan peli.
        /// </summary>
        public virtual void Begin()
        {
        }

        /// <summary>
        /// Tässä alustetaan peli tombstoning-tilasta.
        /// Jos metodia ei ole määritelty, kutsutaan Begin.
        /// </summary>
        public virtual void Continue()
        {
        }

        protected override void OnExiting( object sender, EventArgs args )
        {
            if ( Exiting != null )
                Exiting();

            base.OnExiting( sender, args );
        }

        private void addMessageDisplay()
        {
            MessageDisplay = new MessageDisplay();
            MessageDisplay.BackgroundColor = Color.LightGray;
            Add( MessageDisplay );
        }

        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Update( GameTime gameTime )
        {
            if ( !loadContentHasBeenCalled || !beginHasBeenCalled )
            {
                // No updates until both LoadContent and Begin have been called
                base.Update( gameTime );
                return;
            }

            currentRealTime.Advance( gameTime );

            if ( this.IsActive ) controls.Update();
            if ( DataStorage.IsUpdated )
                DataStorage.Update( currentRealTime );

            // The update in derived classes.
            if ( !IsPaused )
            {
                currentTime.Advance( gameTime );
                this.Update( currentTime );
            }
            else
            {
                this.PausedUpdate( currentRealTime );
            }

            base.Update( gameTime );
        }

        protected virtual void Paint( Canvas canvas )
        {
        }

        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Draw( GameTime gameTime )
        {
            Time time = new Time( gameTime );

            GraphicsDevice.Clear( ClearOptions.Target, Level.BackgroundColor.AsXnaColor(), 1.0f, 0 );

            if ( Level.Background.Image != null && !Level.Background.MovesWithCamera )
            {
                SpriteBatch spriteBatch = Jypeli.Graphics.SpriteBatch;
                spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend ); // AlphaBlend
                spriteBatch.Draw( Level.Background.Image.XNATexture, new XnaRectangle( 0, 0, (int)screen.Width, (int)screen.Height ), XnaColor.White );
                spriteBatch.End();
            }

            // The world matrix adjusts the position and size of objects according to the camera angle.
            var worldMatrix =
                Matrix.CreateTranslation( (float)-Camera.Position.X, (float)-Camera.Position.Y, 0 )
                * Matrix.CreateScale( (float)Camera.ZoomFactor, (float)Camera.ZoomFactor, 1f );

            // If the background should move with camera, draw it here.
            Level.Background.Draw( worldMatrix, Matrix.Identity );

            if ( DrawPerimeter )
            {
                Matrix m = Matrix.CreateScale( (float)Level.Width, (float)Level.Height, 1 ) * worldMatrix;
                Renderer.DrawRectangle( ref m, PerimeterColor );
            }

            foreach ( var layer in Layers )
            {
                layer.Draw( Camera );
            }

            Graphics.LineBatch.Begin( ref worldMatrix );
            Graphics.Canvas.Reset( Level );
            Paint( Graphics.Canvas );
            Graphics.LineBatch.End();

#if DEBUG
            if ( isDebugScreenShown )
            {
                DrawDebugView();
            }
#endif

            base.Draw( gameTime );
        }

#if DEBUG
        private void DrawDebugView()
        {
            for ( int i = 0; i < Layers.Count; i++ )
            {
                objectCountDisplays[i].Draw( Matrix.Identity );
            }

            if ( fpsSkipCounter++ > 10 )
            {
                fpsSkipCounter = 0;
                fpsText = ( 1.0 / Time.SinceLastUpdate.TotalSeconds ).ToString( "F2" );
            }

            fpsDisplay.Text = fpsText;
            fpsDisplay.Draw( Matrix.Identity );
        }
#endif

        /// <summary>
        /// Asettaa ikkunan koon.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public bool SetWindowSize( int width, int height )
        {
            return SetWindowSize( width, height, IsFullScreen );
        }

        /// <summary>
        /// Asettaa ikkunan koon ja alustaa pelin käyttämään joko ikkunaa tai koko ruutua.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="fullscreen">Koko ruutu jos <c>true</c>, muuten ikkuna.</param>
        /// <returns></returns>
        public bool SetWindowSize( int width, int height, bool fullscreen )
        {
            // copy-paste code from: http://forums.xna.com/forums/p/1031/5461.aspx#5461
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            doSetWindowSize( GraphicsDeviceManager, width, height, fullscreen );
            return true;  // jostakin syystä näyttö voi olla isommassa resoluutiossa kuin Mode antaisi ymmärtää.

            if ( fullscreen == false )
            {
                if ( true || ( width <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width )
                    && ( height <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height ) )
                {
                    // The mode is supported, so set the buffer formats, apply changes and return
                    doSetWindowSize( GraphicsDeviceManager, width, height, false );
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach ( DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes )
                {
                    // Check the width and height of each mode against the passed values
                    if ( ( dm.Width == width ) && ( dm.Height == height ) )
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        doSetWindowSize( GraphicsDeviceManager, width, height, true );

                        return true;
                    }
                }
            }

            return false;
        }

        private void doSetWindowSize( GraphicsDeviceManager graphics, int width, int height, bool fullscreen )
        {
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.IsFullScreen = fullscreen;
            graphics.ApplyChanges();

            if ( GraphicsDevice != null )
            {
                Viewport viewPort = GraphicsDevice.Viewport;
                Mouse.Viewport = viewPort;
                if ( screen != null )
                {
                    screen.viewPort = viewPort;
                }
            }
        }

        /// <summary>
        /// Pysäyttää pelin tai jatkaa sitä, jos se on jo pysäytetty.
        /// </summary>
        public void Pause()
        {
            IsPaused = !IsPaused;
        }

        /// <summary>
        /// Lopettaa pelin.
        /// </summary>
        public void Exit()
        {
            Phone.StopVibrating();
            base.Exit();
        }

        /// <summary>
        /// Kysyy haluaako lopettaa pelin ja lopettaa jos vastataan kyllä.
        /// </summary>
        public void ConfirmExit()
        {
            bool cursorVisible = IsMouseVisible;
            IsMouseVisible = true;

            YesNoWindow kyselyIkkuna = new YesNoWindow( "Do you want to quit?" );
            kyselyIkkuna.Yes += Exit;
            kyselyIkkuna.Closed += delegate { IsMouseVisible = cursorVisible; IsPaused = false; };
            Add( kyselyIkkuna );

            IsPaused = true;
        }

        /// <summary>
        /// Tallentaa pelin.
        /// </summary>
        /// <param name="tagName">Pelitilanteen nimi.</param>
        public void SaveGame( string tagName )
        {
            Type gameType = this.GetType();
            SaveState state = DataStorage.BeginSave( tagName );

            foreach ( PropertyInfo property in gameType.GetProperties( BindingFlags.GetProperty | StorageFile.AllOfInstance ) )
            {
                if ( property.GetCustomAttributes( typeof( SaveAttribute ), true ).Length == 0 )
                    continue;

                object propValue = property.GetValue( this, null );
                Type propType = property.PropertyType;
                //DataStorage.Save( propValue, propType, tagName + FileManager.SanitizeFileName( property.Name ) );
                state.Save( propValue, propType, FileManager.SanitizeFileName( property.Name ) + "Property" );
            }

            foreach ( FieldInfo field in gameType.GetFields( BindingFlags.GetField | StorageFile.AllOfInstance ) )
            {
                if ( field.GetCustomAttributes( typeof( SaveAttribute ), true ).Length == 0 )
                    continue;

                object fieldValue = field.GetValue( this );
                Type fieldType = field.FieldType;
                //DataStorage.Save( fieldValue, fieldType, tagName + FileManager.SanitizeFileName( field.Name ) );
                state.Save( fieldValue, fieldType, FileManager.SanitizeFileName( field.Name ) + "Field" );
            }

            state.EndSave();
        }

        /// <summary>
        /// Lataa pelin.
        /// </summary>
        /// <param name="tagName">Pelitilanteen nimi.</param>
        public void LoadGame( string tagName )
        {
            Type gameType = this.GetType();

            LoadState state = DataStorage.BeginLoad( tagName );

            foreach ( PropertyInfo property in gameType.GetProperties( BindingFlags.GetProperty | StorageFile.AllOfInstance ) )
            {
                if ( property.GetCustomAttributes( typeof( SaveAttribute ), true ).Length == 0 )
                    continue;

                object oldValue = property.GetValue( this, null );
                Type propType = property.PropertyType;
                //object newValue = DataStorage.Load( oldValue, propType, tagName + FileManager.SanitizeFileName( property.Name ) );
                object newValue = state.Load( oldValue, propType, FileManager.SanitizeFileName( property.Name ) + "Property" );
                property.SetValue( this, newValue, null );
            }

            foreach ( FieldInfo field in gameType.GetFields( BindingFlags.GetField | StorageFile.AllOfInstance ) )
            {
                if ( field.GetCustomAttributes( typeof( SaveAttribute ), true ).Length == 0 )
                    continue;

                object oldValue = field.GetValue( this );
                Type fieldType = field.FieldType;
                //object newValue = DataStorage.Load( oldValue, fieldType, tagName + FileManager.SanitizeFileName( field.Name ) );
                object newValue = state.Load( oldValue, fieldType, FileManager.SanitizeFileName( field.Name ) + "Field" );
                field.SetValue( this, newValue );
            }

            state.EndLoad();
        }

        public void AddFactory<T>( string tag, Factory.FactoryMethod method )
        {
            Factory.AddFactory<T>( tag, method );
        }

        public void RemoveFactory<T>( string tag, Factory.FactoryMethod method )
        {
            Factory.RemoveFactory<T>( tag, method );
        }

        public T FactoryCreate<T>( string tag )
        {
            return Factory.FactoryCreate<T>( tag );
        }

        /// <summary>
        /// Näyttää kontrollien ohjetekstit.
        /// </summary>
        public void ShowControlHelp()
        {
            MessageDisplay.Add( "Ohjeet:" );

            foreach ( String message in controls.GetHelpTexts() )
            {
                MessageDisplay.Add( message );
            }
        }

        [EditorBrowsable( EditorBrowsableState.Never )]
        public void Dispose()
        {
        }

        /// <summary>
        /// Sitoo kontrollien ohjeet viestinäyttöön ja haluttuihin nappeihin.
        /// Tämän jälkeen nappeja painamalla pelaaja saa automaattisesti ohjeen esille.
        /// </summary>
        /// <param name="keysOrButtons">Napit, joita painamalla ohjeen saa näkyviin.</param>
        private void BindControlHelp( params object[] keysOrButtons )
        {
            String nappaimet = "";

            foreach ( object o in keysOrButtons )
            {
                if ( o is Key )
                {
                    Key k = (Key)o;
                    controls.Keyboard.Listen( k, ButtonState.Pressed, ShowControlHelp, null );

                    nappaimet += k.ToString();
                }

                if ( o is Button )
                {
                    Button b = (Button)o;
                    for ( int i = 0; i < controls.GameControllers.Length; i++ )
                    {
                        controls.GameControllers[i].Listen( b, ButtonState.Pressed, ShowControlHelp, null );
                    }

                    nappaimet += b.ToString();
                }

                nappaimet += " / ";
            }

            MessageDisplay.Add( "Katso näppäinohje painamalla " + nappaimet.Substring( 0, nappaimet.Length - 3 ) );
        }

        public static Image LoadImageFromResources( string name )
        {
            return new Image( ResourceContent.Load<Texture2D>( name ) );
        }

        public static SoundEffect LoadSoundEffectFromResources( string name )
        {
            return new SoundEffect( ResourceContent.Load<XnaSoundEffect>( name ) );
        }

        /// <summary>
        /// Lataa fontin. Fontin tulee olla lisätty content-hakemistoon.
        /// </summary>
        /// <param name="name">Fontin tiedoston nimi, ilman päätettä.</param>
        public static Font LoadFont( string name )
        {
            return new Font( name, ContentSource.GameContent );
        }
    }
}

