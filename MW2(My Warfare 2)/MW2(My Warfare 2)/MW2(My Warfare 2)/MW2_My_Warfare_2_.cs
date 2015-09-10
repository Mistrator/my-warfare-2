// MY WARFARE 2
// ohjelmointi: Miska Kananen
// tekstuurit: Miska Kananen, Leevi Kujanpää, Kalle Pienmäki
// musiikit: Kalle Pienmäki

using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;
using My_Warfare_2_Server;

/// @author Miska Kananen
/// @version 7.6.2015
///
/// <summary>
/// My Warfare 2:n pääluokka. Peli toimii tämän luokan sisällä.
/// </summary>
public class MW2_My_Warfare_2_ : PhysicsGame
{
    #region attribuutit, kuvat ja äänet
    #region peliattribuutit

    public static MW2_My_Warfare_2_ Peli { get; private set; }
    public Pelaaja[] pelaajat = new Pelaaja[Vakiot.PELAAJIEN_MAARA];
    public Efektit Efektit = new Efektit();
    public Kentta KentanOsat { get; set; }
    private String ValittuKenttaTiedosto { get; set; }
    private Timer AikaKentanAlusta = new Timer();
    private Timer FireUpdateTick = new Timer();
    public Gamemode CurrentGameMode { get; set; }

    /// <summary>
    /// Tuleeko pelaajalle ilmoitus, jos tämä menee liian kauas.
    /// </summary>
    private bool KaytetaankoRajattuaAluetta = false;

    /// <summary>
    /// Monestako taposta voittaa pelin.
    /// </summary>
    public static IntMeter MonestakoVoittaa = new IntMeter(Int32.MaxValue);

    /// <summary>
    /// Kauanko Escapessa puolustetaan kopteria.
    /// </summary>
    public static DoubleMeter KauankoPuolustetaan = new DoubleMeter(120.0);

    private static readonly Func<Ase>[] aseidenLuontimetodit = 
    { 
        Aseet.LuoRynkky, Aseet.LuoHaulikko, Aseet.LuoMinigun, 
        Aseet.LuoSnipa, Aseet.LuoSinko, Aseet.LuoMagnum, 
        Aseet.LuoVintorez, Aseet.LuoSMG, Aseet.LuoAutomaattiHaulikko, 
        Aseet.LuoFlareGun, Aseet.LuoOhjus 
    };

    static readonly SoundEffect aseTyhjäAani = LoadSoundEffect("dry-fire-gun");
    static readonly SoundEffect aseLatausAani = LoadSoundEffect("Pump Shotgun");
    static readonly SoundEffect lippaanPoistoAani = LoadSoundEffect("clip-remove");
    static readonly SoundEffect hylsynPutoamisAani = LoadSoundEffect("gun_shell_drop");
    static readonly SoundEffect luotiOsuu = LoadSoundEffect("bullet_hit");
    static readonly SoundEffect luotiOsuu2 = LoadSoundEffect("bullet_hit2");
    static readonly SoundEffect luotiOsuu3 = LoadSoundEffect("bullet_hit3");
    static readonly SoundEffect luotiOsuu4 = LoadSoundEffect("bullet_hit4");
    static readonly SoundEffect luotiOsuu5 = LoadSoundEffect("bullet_hit5");
    public static readonly SoundEffect osuma1 = LoadSoundEffect("hit_1");

    private Light valo = new Light();
    #endregion

    #region Kuvat

    #region asekuvat
    static readonly Image kranaatinKuva = LoadImage("Kranaatti_F1");
    static readonly Image pistoolinMaaKuva = LoadImage("GLOCK18_maa");
    static readonly Image rynkynMaaKuva = LoadImage("AK-74M_maa");
    static readonly Image pumpparinMaaKuva = LoadImage("REMINGTON870_maa");
    static readonly Image sawinMaaKuva = LoadImage("M249SAW_maa");
    static readonly Image singonMaaKuva = LoadImage("RPG-7_maa");
    static readonly Image snipanMaaKuva = LoadImage("SVDDRAGUNOV_maa");
    static readonly Image kranaatinMaaKuva = LoadImage("F1FRAG_maa");
    static readonly Image magnuminMaaKuva = LoadImage("Magnum_maa");
    static readonly Image ppshMaaKuva = LoadImage("ppsh41_maa");
    static readonly Image aa12MaaKuva = LoadImage("AA12_maa");
    static readonly Image vintorezMaaKuva = LoadImage("VSS_maa");
    #endregion

    #region kentänkuvat
    // Kentän osat
    static readonly Image valikonKuva = LoadImage("valikkotausta");
    static readonly Image valikonPainikeKuva = LoadImage("menu_buttontest");
    static GameObject menuGradient;
    static readonly Image kivenKuva = LoadImage("kivi");
    static readonly Image piikkilankaKuva = LoadImage("piikkilanka");
    static readonly Image pystypiikkilankaKuva = LoadImage("piikkilankapysty");
    static readonly Image vaakapuunKuva = LoadImage("puuvaaka");
    static readonly Image naamioverkonKuva = LoadImage("naamioverkko");
    static readonly Image pystypuunKuva = LoadImage("puupysty");
    static readonly Image bussinkuva = LoadImage("bussi");
    static readonly Image poistuvanseinänKuva = LoadImage("poistuvaseinä");
    static readonly Image crack1 = LoadImage("crack1");
    static readonly Image kiviRikki0 = LoadImage("kivirikki0");
    static readonly Image kiviRikki1 = LoadImage("kivirikki1");
    static readonly Image kiviRikki2 = LoadImage("kivirikki2");
    static readonly Image kiviRikki3 = LoadImage("kivirikki3");
    static readonly Image pystypuuRikki1 = LoadImage("puupystyrikki1");
    static readonly Image pystypuuRikki2 = LoadImage("puupystyrikki2");
    static readonly Image edustanKuva = LoadImage("edusta1");
    static readonly Image seinäSoihdunKuva = LoadImage("seinäsoihtu");
    static readonly Image valonKuva = LoadImage("valo1");
    static readonly Image tynnyrinKuva = LoadImage("tynnyri");
    static readonly Image killballPohja = LoadImage("killball_pohja");
    static readonly Image killballKeski = LoadImage("killball_keski");
    static readonly Image killballYla = LoadImage("killball_ylä");
    static readonly Image helikopteriLaskeutumisAlusta = LoadImage("kopteri_laskeutumisalusta");
    static readonly Image kuusi = LoadImage("kuusi");

    static readonly Image hudKuva = LoadImage("hudtausta");
    static readonly Image hudTäysi = LoadImage("hudtaustatäysi");
    static readonly Image hudKuva2 = LoadImage("hudtausta2");
    static readonly Image hudTäysi2 = LoadImage("hudtaustatäysi2");

    static readonly Image laatikonKuva = LoadImage("kamaa");
    static readonly Image laatikkoTyhjä = LoadImage("kamaatyhjä");
    static readonly Image kamaaNäkymätön = LoadImage("kamaanäkymätön");
    static readonly Image tyhjä = LoadImage("tyhjä");
    static readonly Image[] laatikonAnimaatioKuvat = LoadImages("kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa", "kamaa1", "kamaa2", "kamaa3", "kamaa4");

    static readonly Image zombinKuva = LoadImage("zombi");
    static readonly Image[] zombinKuvat = LoadImages("zombi1", "zombi2", "zombi1", "zombi3");
    static readonly Image korpinKuva = LoadImage("korppitesti");
    #endregion

    #region infinitekuvat
    static readonly Image normaaliZombiKuva = LoadImage("zombi_tavallinen");
    static readonly Image[] normaaliZombiAnimaatio = LoadImages("zombi_tavallinen1", "zombi_tavallinen2", "zombi_tavallinen3", "zombi_tavallinen2", "zombi_tavallinen1", "zombi_tavallinen4", "zombi_tavallinen5", "zombi_tavallinen4");
    static readonly Image ampuvaZombiKuva = LoadImage("zombi_ak");
    static readonly Image[] parasiittienKuvat = LoadImages("Parasiitti1", "Parasiitti2", "Parasiitti3");
    static readonly Image[] parasiittienPartikkeliKuvat = LoadImages("paras_sirpale1", "paras_sirpale2", "paras_sirpale3", "paras_sirpale4", "paras_sirpale5");
    #endregion

    #endregion

    #endregion

    #region yleiset toiminnot

    /// <summary>
    /// Aloitetaan peli luomalla alkuvalikko.
    /// </summary>
    public override void Begin()
    {
        SoitaMusiikkia(0); // valikkotheme valikkoon
        LuoAlkuValikko();
    }

    /// <summary>
    /// Päivitetään pelaajiin liittyviä asioita.
    /// </summary>
    /// <param name="time">Aika pelin alusta ja edellisestä päivityksestä.</param>
    protected override void Update(Time time)
    {
        base.Update(time);

        // Päivitetään pelaajiin liittyviä juttuja.
        for (int i = 0; i < pelaajat.Length; i++)
        {
            if (pelaajat[i] != null)
            {
                if (pelaajat[i].kaytetaankoPalloTahtainta)
                {
                    Vector suunta = (pelaajat[i].tahtain.Position - pelaajat[i].Position).Normalize();
                    pelaajat[i].Angle = suunta.Angle;
                }

                if (pelaajat[i].tahtayksenKohde != null && pelaajat[i].tahtayksenKohde.IsAddedToGame)
                    pelaajat[i].Angle = (pelaajat[i].tahtayksenKohde.Position - pelaajat[i].Position).Normalize().Angle;

                pelaajat[i].elamaPalkki.Position = new Vector(pelaajat[i].X, pelaajat[i].Y + 35); // 30
                pelaajat[i].kuntoPalkki.Position = new Vector(pelaajat[i].X, pelaajat[i].Y + 25);

                pelaajat[i].ammusMaaraNaytto.Position = pelaajat[i].kuntoPalkki.Position + new Vector(0.0, 30.0);
                if (pelaajat[i].ammusMaaraNaytto.Text == String.Empty || pelaajat[i].ammusMaaraNaytto.IsVisible == false)
                    pelaajat[i].ValittuAseNaytto.Position = pelaajat[i].kuntoPalkki.Position + new Vector(0.0, 30.0);
                else pelaajat[i].ValittuAseNaytto.Position = pelaajat[i].ammusMaaraNaytto.Position + new Vector(0.0, 20.0);
                pelaajat[i].infoRuutu.Position = pelaajat[i].ValittuAseNaytto.Position + new Vector(0.0, 20.0);


                // Tarkistetaan, ovatko pelaajat sallitun pelialueen ulkopuolella. Ei tarvita kentillä, joissa on piikkilangat reunoina.
                if (KaytetaankoRajattuaAluetta)
                {
                    if (pelaajat[i].Position.X > Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA || pelaajat[i].Position.Y > Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA)
                    {
                        pelaajat[i].infoRuutu.Text = String.Format("Palaa taistelukentälle {0:0.00} sekunnin sisällä.", pelaajat[i].aikaPoissaKentalta.Value);
                        if (pelaajat[i].OllaankoPoissaKentalta == false) pelaajat[i].PoistuttiinKentalta();
                    }
                    else if (pelaajat[i].Position.X < Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA || pelaajat[i].Position.Y < Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA)
                    {
                        pelaajat[i].infoRuutu.Text = String.Format("Palaa taistelukentälle {0:0.00} sekunnin sisällä.", pelaajat[i].aikaPoissaKentalta.Value);
                        if (pelaajat[i].OllaankoPoissaKentalta == false) pelaajat[i].PoistuttiinKentalta();
                    }
                    else
                    {
                        if (pelaajat[i].OllaankoPoissaKentalta == true)
                        {
                            pelaajat[i].infoRuutu.Text = "";
                            pelaajat[i].PalattiinKentalle();
                        }
                    }
                }
            }
        }
    }

    protected override void Paint(Canvas canvas)
    {
        if (CurrentGameMode == Gamemode.SurvivalSingle || CurrentGameMode == Gamemode.SurvivalMulti)
        {
            canvas.BrushColor = Color.Darker(Color.Red, 192);
            Vector vasenYla = new Vector(Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA);
            Vector vasenAla = new Vector(Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA);
            Vector oikeaYla = new Vector(Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA);
            Vector oikeaAla = new Vector(Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA);

            canvas.DrawLine(vasenYla, oikeaYla);
            canvas.DrawLine(oikeaYla, oikeaAla);
            canvas.DrawLine(oikeaAla, vasenAla);
            canvas.DrawLine(vasenAla, vasenYla);
        }

        base.Paint(canvas);
    }

    /// <summary>
    /// Tehdään vahinkoa kohteelle.
    /// </summary>
    /// <param name="damagenKohde">Kohde, jota vahingoitetaan.</param>
    /// <param name="damagenMäärä">Paljonko vahinkoa tehdään.</param>
    public void Damagea(Elava damagenKohde, double damagenMäärä, bool tuleekoVerta = true)
    {
        if (damagenKohde.Elamat.Value <= 0) return;

        damagenKohde.Elamat.Value -= damagenMäärä;

        if (!tuleekoVerta) return;
        GameObject[] effects = Blood.AddNormalBlood(damagenKohde.Position, 3, 0.3);
        foreach (GameObject blood in effects)
        {
            Efektit.LisaaTehosteObjekti(blood);
        }
    }

    /// <summary>
    /// Tehdään vahinkoa kohteelle.
    /// </summary>
    /// <param name="damagenKohde">Kohde, jota vahingoitetaan.</param>
    /// <param name="damagenMäärä">Paljonko vahinkoa tehdään.</param>
    public void Damagea(Tuhoutuva damagenKohde, double damagenMaara)
    {
        damagenKohde.Kesto.Value -= damagenMaara;
    }

    /// <summary>
    /// Aloittaa pelin alusta ja palaa päävalikkoon. Poistaa kaiken tähän mennessä tehdyn.
    /// </summary>
    void PaaValikkoon()
    {
        ClearAll();
        Camera.Reset();
        Efektit.Tyhjenna();
        Level.AmbientLight = 0.8;
        SoitaMusiikkia(0); // valikkotheme valikkoon
        Partikkelit.InitializeParticles(this);
        CurrentGameMode = Gamemode.None;
        KaytetaankoRajattuaAluetta = false;
        LuoAlkuValikko(true);
    }

    /// <summary>
    /// Kutsutaan, kun toinen pelaaja voittaa pelin.
    /// </summary>
    /// <param name="voittanutPelaaja">Pelaaja, joka voitti pelin.</param>
    void Voitto(Pelaaja voittanutPelaaja)
    {
        if (CurrentGameMode == Gamemode.Sandbox) return; // ihan vaan ettei käy mitään outoa
        // MediaPlayer.Play("EPIC VICTORY THEME SONG");
        PimennaRuutua(0.01);
        MultiSelectWindow voittoIkkuna = new MultiSelectWindow("Pelaaja " + voittanutPelaaja.Numero + " voitti pelin!\n\n" +
            "Pelaajan 1 tapot: " + pelaajat[0].Tapot + "\n" +
            "Pelaajan 2 tapot: " + pelaajat[1].Tapot, Color.White, "Päävalikkoon", "Lopeta peli");
        voittoIkkuna.ItemSelected += delegate(int valinta)
        {
            switch (valinta)
            {
                case 0:
                    PaaValikkoon();
                    break;
                case 1:
                    Exit();
                    break;
            }
        };
        voittoIkkuna.Color = Color.Transparent;
        voittoIkkuna.BorderColor = Color.Transparent;
        voittoIkkuna.SelectionColor = Color.Green;
        voittoIkkuna.DefaultCancel = 0;
        voittoIkkuna.IsModal = true;
        Add(voittoIkkuna);
    }

    /// <summary>
    /// Kutsutaan, kun Infinite hävitään.
    /// </summary>
    /// <param name="havittyPeli">Peli, joka on hävitty.</param>
    void InfiniteHavio(Infinite havittyPeli)
    {
        MediaPlayer.Play("ZombieGameOverFIX");
        PimennaRuutua(0.01);
        MultiSelectWindow havioIkkuna = new MultiSelectWindow("Peli päättyi!\nVihollisia tapettu " + havittyPeli.VihollisiaTapettu + " kpl.\nSelvisit " + String.Format("{0:00}", havittyPeli.AikaPelinAlusta.CurrentTime) + " sekuntia.", Color.White, "Päävalikkoon", "Lopeta peli");
        havioIkkuna.ItemSelected += delegate(int valinta)
        {
            switch (valinta)
            {
                case 0:
                    PaaValikkoon();
                    Infinite.OnkoPeliKaynnissa = false;
                    break;
                case 1:
                    Infinite.OnkoPeliKaynnissa = false;
                    Exit();
                    break;
            }
        };
        havioIkkuna.Color = Color.Transparent;
        havioIkkuna.BorderColor = Color.Transparent;
        havioIkkuna.SelectionColor = Color.Green;
        havioIkkuna.DefaultCancel = 0;
        havioIkkuna.IsModal = true;
        Add(havioIkkuna);

        havittyPeli.SpawnataankoVihollisia = false;

        foreach (Pelaaja p in pelaajat)
        {
            if (p != null && p.Taskulamppu != null) p.Taskulamppu.Destroy();
        }

        foreach (Vihollinen vihu in havittyPeli.VihollisetKentalla)
        {
            vihu.Destroy();
        }
    }

    /// <summary>
    /// Tarkistetaan, onko piste näyttöruudun sisäpuolella.
    /// </summary>
    /// <param name="piste">Piste.</param>
    /// <returns>Onko piste näytön alueella.</returns>
    public bool OnkoNaytonAlueella(Vector piste)
    {
        Vector vasenYlakulma = Camera.ScreenToWorld(new Vector(Screen.Left, Screen.Top));
        Vector oikeaYlaKulma = Camera.ScreenToWorld(new Vector(Screen.Right, Screen.Top));
        Vector vasenAlaKulma = Camera.ScreenToWorld(new Vector(Screen.Left, Screen.Bottom));

        if (piste.X > vasenYlakulma.X && piste.X < oikeaYlaKulma.X)
            if (piste.Y < vasenYlakulma.Y && piste.Y > vasenAlaKulma.Y)
                return true;
        return false;
    }

    public Pelaaja LahinPelaaja(Vector position, Pelaaja[] pelaajat)
    {
        if (pelaajat.Length == 0) return null;

        double pieninEtaisyys = double.MaxValue;
        int lahin = 0;

        for (int i = 0; i < pelaajat.Length; i++)
        {
            double etaisyys = Vector.Distance(position, pelaajat[i].Position);

            if (etaisyys < pieninEtaisyys)
            {
                pieninEtaisyys = etaisyys;
                lahin = i;
            }
        }
        return pelaajat[lahin];
    }

    /// <summary>
    /// Asettaa ikkunan koon halutuksi ja palaa päävalikkoon.
    /// </summary>
    /// <param name="leveys">Ikkunan leveys.</param>
    /// <param name="korkeus">Ikkunan korkeus.</param>
    void AsetaIkkunanKoko(int leveys, int korkeus)
    {
        SetWindowSize(leveys, korkeus);
        PaaValikkoon();
    }

    /// <summary>
    /// Poistetaan kentältä kaikki asiat, paitsi pelaajat.
    /// </summary>
    void RemoveAllEntities()
    {
        List<GameObject> pelioliot = GetObjects(x => x.Tag.ToString() != "pelaaja" && x.Tag.ToString() != "tahtain" && x.Tag.ToString() != "HUD");

        foreach (GameObject olio in pelioliot)
        {
            olio.Destroy();
        }
        pelioliot.Clear();
    }

    /// <summary>
    /// Pimennetään ruutua pikku hiljaa.
    /// Oletusnopeus 0.01.
    /// </summary>
    public void PimennaRuutua(double nopeus)
    {
        Timer ajastin = new Timer();
        ajastin.Interval = 0.1;
        ajastin.Timeout += delegate
        {
            Level.AmbientLight -= nopeus;
            if (Level.AmbientLight <= 0.0)
            {
                ajastin.Stop();
                ajastin.Reset();
            }
        };
        ajastin.Start();
    }

    /// <summary>
    /// Kirkastetaan ruutua pikku hiljaa.
    /// </summary>
    public void KirkastaRuutua(double nopeus, double tavoite)
    {
        Timer ajastin = new Timer();
        ajastin.Interval = 0.1;
        ajastin.Timeout += delegate
        {
            //if (Level.AmbientLight > 0.1) return;
            Level.AmbientLight += nopeus;
            if (Level.AmbientLight > tavoite)
            {
                ajastin.Stop();
                ajastin.Reset();
            }
        };
        ajastin.Start();
    }

    /// <summary>
    /// Päivitetään palamistilanne.
    /// </summary>
    void FireUpdate()
    {
        List<PhysicsObject> pobjects = this.GetPhysicsObjects();

        for (int i = 0; i < pobjects.Count; i++)
        {
            if (pobjects[i].OnFire)
            {
                pobjects[i].Burn();
                SpreadFire(pobjects, i);
            }
        }
    }

    /// <summary>
    /// Levitetään tulta.
    /// </summary>
    /// <param name="burnableObjs">Palamaan pystyvät oliot.</param>
    /// <param name="spreader">Tulta levittävä olio.</param>
    void SpreadFire(List<PhysicsObject> burnableObjs, int spreader)
    {
        for (int i = 0; i < burnableObjs.Count; i++)
        {
            if (Vector.Distance(burnableObjs[i].Position, burnableObjs[spreader].Position) < Vakiot.TULEN_LEVIAMIS_ETAISYYS && i != spreader)
            {
                /*if (Wind != Vector.Zero)
                {
                    Vector leviamisSuunta = (burnableObjs[i].Position - burnableObjs[spreader].Position).Normalize();
                    Vector.DotProduct(Wind.Normalize(), (burnableObjs[i].Position - burnableObjs[spreader].Position).Normalize())
                }
                else*/
                if (!burnableObjs[i].OnFire)
                    burnableObjs[i].IgnitionHP.Value--;
            }
        }
    }


    /// <summary>
    /// Soitetaan musiikkia.
    /// 0:Menu Background Theme 
    /// 1:Dark Lurk 
    /// 2:Zombin taustamusiikki
    /// 3:OrcsCome 
    /// 4:OrcsCome Special 
    /// 5:MoozE-Radwind
    /// </summary>
    /// <param name="valinta">Mikä biisi soitetaan.</param>
    void SoitaMusiikkia(int valinta)
    {
        MediaPlayer.IsRepeating = true;
        switch (valinta)
        {
            case 0:
                MediaPlayer.Play("Menu_BackGround");
                break;
            case 1:
                MediaPlayer.Play("DarkLurk");
                break;
            case 2:
                MediaPlayer.Play("zombi_taustamusa_1");
                break;
            case 3:
                MediaPlayer.Play("OrcsCome");
                break;
            case 4:
                MediaPlayer.Play("OrcsCome_special");
                break;
            case 5:
                MediaPlayer.Play("03 MoozE - Radwind Pt1");
                break;
        }
    }

    /// <summary>
    /// Piirretään valojuovia.
    /// </summary>
    /// <param name="position">Valojuovan tämänhetkinen sijainti.</param>
    /// <param name="color">Valojuovan väri.</param>
    /// <param name="lifetimeInSeconds">Kauanko valojuova pysyy kentällä.</param>
    void DrawTracers(Vector position, Color color, double lifetimeInSeconds, double brightnessFactor)
    {
        if (brightnessFactor > byte.MaxValue)
            color.AlphaComponent = byte.MaxValue;
        else
            color.AlphaComponent = (byte)(brightnessFactor); 
        GameObject tracer = new GameObject(2, 2);
        tracer.Shape = Shape.Circle;
        tracer.Color = color;
        tracer.Position = position;
        tracer.MaximumLifetime = TimeSpan.FromSeconds(lifetimeInSeconds);
        Add(tracer);
    }

    /// <summary>
    /// Piirretään valojuovia.
    /// </summary>
    /// <param name="tracingObject">Olio, josta valojuovat tulevat.</param>
    /// <param name="color">Valojuovan väri.</param>
    /// <param name="lifetimeInSeconds">Kauanko valojuova pysyy kentällä.</param>
    /// <param name="brightnessFactor">Valojuovan kirkkaus.</param>
    void DrawTracers(PhysicsObject tracingObject, Color color, double lifetimeInSeconds, double brightnessFactor)
    {
        if (brightnessFactor > byte.MaxValue)
            color.AlphaComponent = byte.MaxValue;
        else
            color.AlphaComponent = (byte)(brightnessFactor);
        GameObject tracer = new GameObject((1.0 / 60.0) * 1.5 * tracingObject.Velocity.Magnitude, 2);
        tracer.Shape = Shape.Circle;
        tracer.Color = color;
        tracer.Position = tracingObject.Position;
        tracer.Angle = tracingObject.Angle;
        tracer.MaximumLifetime = TimeSpan.FromSeconds(lifetimeInSeconds);
        Add(tracer);
    }

    #endregion

    #region kenttä

    /// <summary>
    /// Luodaan kenttä ja kaikki siihen liittyvä, kuten pelaajat.
    /// </summary>
    /// <param name="kenttaTyyppi">Kentän luomistapa. 0: kenttä kuvasta, 1: satunnaisgeneroitu kenttä, 2: tyhjä kenttä.</param>
    /// <param name="pelaajienMaara">Montako pelaajaa peliin tulee.</param>
    void LuoKentta(KenttaTyyppi kenttaTyyppi, int pelaajienMaara, bool onkoInfinite, Image ruutukartta = null)
    {
        Mouse.IsCursorVisible = false;
        bool onkoSandbox = false;
        AikaKentanAlusta.Reset();
        AikaKentanAlusta.Start();
        MessageDisplay.BackgroundColor = Color.Transparent;
        MessageDisplay.TextColor = Color.White;
        MessageDisplay.MessageTime = TimeSpan.FromSeconds(1.5);
        Level.Size = new Vector(Vakiot.KENTAN_LEVEYS * Vakiot.KENTAN_RUUDUN_LEVEYS, Vakiot.KENTAN_KORKEUS * Vakiot.KENTAN_RUUDUN_KORKEUS);
        Level.AmbientLight = 0.8;
        FireUpdateTick.Interval = 0.1;
        FireUpdateTick.Timeout += FireUpdate;
        FireUpdateTick.Start();
        Add(valo);
        menuGradient.Destroy();
        menuGradient = null;

        Vector pelaajan1spawni = new Vector(-700.0, 0.0);
        Vector pelaajan2spawni = new Vector(700.0, 0.0);
        List<Vector> vaihtoehtoisetSpawnit = new List<Vector>();

        switch (kenttaTyyppi)
        {
            case KenttaTyyppi.Ruutukartta:
                List<Vector> spawns = LuoTileMapKentta(ruutukartta);
                pelaajan1spawni = spawns[0];
                pelaajan2spawni = spawns[1];
                if (spawns.Count > 2)
                    vaihtoehtoisetSpawnit.AddRange(spawns.GetRange(2, spawns.Count - 2));
                LuoPilvet((onkoInfinite && pelaajienMaara == 1));
                break;
            case KenttaTyyppi.GameOfLifeGeneroitu:
                string[] satunnaisKenttäTiedosto = LuoSatunnainenKenttäTiedosto(Vakiot.KENTAN_LEVEYS, Vakiot.KENTAN_KORKEUS);
                TileMap satunnaisKenttä = TileMap.FromStringArray(satunnaisKenttäTiedosto);
                satunnaisKenttä.SetTileMethod('1', LuoTuhoutuvaKentanOsa, kivenKuva, "kivi", 20, 1.0, 1.0);
                satunnaisKenttä.Optimize();
                satunnaisKenttä.Execute(Vakiot.KENTAN_RUUDUN_LEVEYS, Vakiot.KENTAN_RUUDUN_KORKEUS);

                string[] pickupTiedosto = LuoSatunnainenPickupTiedosto(satunnaisKenttäTiedosto, Vakiot.PICKUPIEN_MAARA);
                TileMap pickupit = TileMap.FromStringArray(pickupTiedosto);
                pickupit.SetTileMethod('1', LuoKerattavaEsine);
                pickupit.Execute(Vakiot.KENTAN_RUUDUN_LEVEYS, Vakiot.KENTAN_RUUDUN_KORKEUS);
                LuoPilvet(false);
                break;
            case KenttaTyyppi.Sandbox:
                onkoSandbox = true;
                break;
            case KenttaTyyppi.Survival:
                onkoInfinite = true;
                KentanOsat = new Kentta(Vakiot.KENTAN_LEVEYS, Vakiot.KENTAN_KORKEUS, "valekivi");
                break;
        }

        #region pelaajien spawnaus ja asettelu

        bool kaytetaankoTaskuLamppua = false;

        if (CurrentGameMode == Gamemode.InfiniteSingle)
            kaytetaankoTaskuLamppua = true;
        if (CurrentGameMode == Gamemode.SurvivalSingle || CurrentGameMode == Gamemode.SurvivalMulti)
        {
            for (int i = 0; i < 10; i++)
            {
                vaihtoehtoisetSpawnit.Add(RandomGen.NextVector(Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA));
            }
        }

        pelaajat[0] = LuoPelaaja(pelaajan1spawni, 43.5, 21.75, 1, Aseet.pelaaja1pistooliKuva, Color.Green, AsetustenKaytto.Asetukset.OnkoPelaajalla1Lasertahtainta, new Vector(Screen.Left + 130, Screen.Top - 70), kaytetaankoTaskuLamppua, true);
        Add(pelaajat[0], 1);
        pelaajat[0].SpawnausPaikat.AddRange(vaihtoehtoisetSpawnit);

        pelaajat[1] = LuoPelaaja(pelaajan2spawni, 43.5, 21.75, 2, Aseet.pelaaja2pistooliKuva, Color.Red, AsetustenKaytto.Asetukset.OnkoPelaajalla2Lasertahtainta, new Vector(Screen.Right - 130, Screen.Top - 70), kaytetaankoTaskuLamppua, false);
        pelaajat[1].SpawnausPaikat.AddRange(vaihtoehtoisetSpawnit);

        valo.Position = Vector.Zero;
        valo.Intensity = 1.0;
        valo.Intensity = 0.0;

        if (!onkoInfinite)
        {
            pelaajat[0].Voitti += Voitto;
            pelaajat[0].Kuoli += delegate 
            { 
                pelaajat[1].Tapot.Value++;
                if (MonestakoVoittaa.MaxValue == Int32.MaxValue)
                    pelaajat[1].NaytaViesti("Tappoja " + pelaajat[1].Tapot.Value, 1.0);
                else
                    pelaajat[1].NaytaViesti("Tappoja " + pelaajat[1].Tapot.Value + "/" + pelaajat[1].Tapot.MaxValue, 1.0);
            };
            pelaajat[1].Voitti += Voitto;
            pelaajat[1].Kuoli += delegate 
            { 
                pelaajat[0].Tapot.Value++;
                if (MonestakoVoittaa.MaxValue == Int32.MaxValue)
                    pelaajat[0].NaytaViesti("Tappoja " + pelaajat[0].Tapot.Value, 1.0);
                else
                    pelaajat[0].NaytaViesti("Tappoja " + pelaajat[0].Tapot.Value + "/" + pelaajat[0].Tapot.MaxValue, 1.0);
            };
        }

        if (pelaajienMaara == 2)
        {
            Add(pelaajat[1], 1);
            Camera.Follow(pelaajat[0], pelaajat[1]);
        }
        else
        {
            Camera.Zoom(1);
            Camera.Follow(pelaajat[0]);
            pelaajat[1].elamaPalkki.IsVisible = false;
            pelaajat[1].ValittuAseNaytto.IsVisible = false;
            pelaajat[1].kuntoPalkki.IsVisible = false;
            pelaajat[1].ammusMaaraNaytto.IsVisible = false;
        }

        if (onkoSandbox) // laitetaan kaikki aseet, loputtomat ammukset ja loputtomat kranaatit
        {
            for (int i = 0; i < pelaajat.Length; i++)
            {
                pelaajat[i].LisaaAse(Aseet.LuoRynkky());
                pelaajat[i].LisaaAse(Aseet.LuoMinigun());
                pelaajat[i].LisaaAse(Aseet.LuoHaulikko());
                pelaajat[i].LisaaAse(Aseet.LuoMagnum());
                pelaajat[i].LisaaAse(Aseet.LuoSnipa());
                pelaajat[i].LisaaAse(Aseet.LuoSinko());
                pelaajat[i].LisaaAse(Aseet.LuoOhjus());
                pelaajat[i].LisaaAse(Aseet.LuoVintorez());
                pelaajat[i].LisaaAse(Aseet.LuoAutomaattiHaulikko());
                pelaajat[i].LisaaAse(Aseet.LuoSMG());
                pelaajat[i].LisaaAse(Aseet.LuoFlareGun());
#if DEBUG
                pelaajat[i].LisaaAse(Aseet.LuoBinaryRifle());
                pelaajat[i].LisaaAse(Aseet.LuoVarsijousi());
                pelaajat[i].LisaaAse(Aseet.LuoLiekinheitin());
#endif

                for (int j = 0; j < pelaajat[i].Aseet.Count; j++)
                {
                    pelaajat[i].Aseet[j].Ammo.MaxValue = Int32.MaxValue;
                    pelaajat[i].Aseet[j].Ammo.Value = Int32.MaxValue;
                    pelaajat[i].Aseet[j].MaxAmmo.MaxValue = Int32.MaxValue;
                    pelaajat[i].Aseet[j].MaxAmmo.Value = Int32.MaxValue;

                    pelaajat[i].KranaattienMaara = Int32.MaxValue;
                    pelaajat[i].LoputtomatKranaatit = true;
                }
                pelaajat[i].ammusMaaraNaytto.IsVisible = false;
            }
        }

        if (kenttaTyyppi == KenttaTyyppi.Ruutukartta)
        {
            AjastaAseidenAvautumisIlmoitukset();
        }

        #endregion

    }

    /// <summary>
    /// Luo kentän ColorTileMapilla kuvasta. Jos kuva on null, käytetään ValittuKenttaTiedostoa.
    /// Palauttaa listan, jossa [0] = pelaajan 1 spawni, [1] = pelaajan 2 spawni, [2->...] = pelaajien vaihtoehtoiset spawnit
    /// </summary>
    /// <param name="tilemap"></param>
    List<Vector> LuoTileMapKentta(Image tilemap)
    {
        List<Vector> positions = new List<Vector>();
        positions.Add(Vector.Zero);
        positions.Add(Vector.Zero);

        ColorTileMap ruudut;
        if (tilemap == null)
            ruudut = ColorTileMap.FromLevelAsset(ValittuKenttaTiedosto);
        else
            ruudut = new ColorTileMap(tilemap);

        KentanOsat = new Kentta(ruudut.ColumnCount, ruudut.RowCount, "valekivi");

        ruudut.SetTileMethod(Vakiot.VAAKASUORA_PIIKKILANKA, LuoKentanOsa, piikkilankaKuva, Vakiot.PIIKKILANKA_TAG, 5);
        ruudut.SetTileMethod(Vakiot.KIVI, LuoTuhoutuvaKentanOsa, kivenKuva, "kivi", 20, 1.0, 1.0);
        ruudut.SetTileMethod(Vakiot.VALEKIVI, LuoLapiMentavaKentanOsa, kivenKuva, "valekivi", 1.0, 1.0, 1, false);
        ruudut.SetTileMethod(Vakiot.PYSTYSUORA_PUU, LuoTuhoutuvaKentanOsa, pystypuunKuva, "puu", 10, 0.3, 1.0);
        ruudut.SetTileMethod(Vakiot.PYSTYSUORA_PIIKKILANKA, LuoKentanOsa, pystypiikkilankaKuva, Vakiot.PIIKKILANKA_TAG, 5);
        ruudut.SetTileMethod(Vakiot.VAAKASUORA_PUU, LuoTuhoutuvaKentanOsa, vaakapuunKuva, "puu", 10, 1.0, 0.3);
        //ruudut.SetTileMethod(Color.ForestGreen, LuoLapiMentavaKentanOsa, naamioverkonKuva, "naamioverkko", 1);
        ruudut.SetTileMethod(Vakiot.ASELAATIKKO, LuoLaatikko);
        ruudut.SetTileMethod(Color.Yellow, LuoLapiMentavaKentanOsa, seinäSoihdunKuva, "seinäsoihtu", 1.0, 1.0, -1, false);
        ruudut.SetTileMethod(Color.Olive, LuoLapiMentavaKentanOsa, valonKuva, "valo", 6.0, 6.0, 1, false);
        ruudut.SetTileMethod(Vakiot.TYNNYRI, LuoTynnyri);
        ruudut.SetTileMethod(Vakiot.VIHOLLISTEN_FIXED_SPAWN, delegate(Vector p, double w, double h, IntPoint posInLevel)
        {
            if (Infinite.CurrentGame != null)
                Infinite.CurrentGame.LisaaFixedSpawni(p);
        });
        ruudut.SetTileMethod(Vakiot.PELAAJAN_1_SPAWNI, delegate(Vector p, double w, double h, IntPoint posInLevel)
        {
            positions[0] = p;
        });
        ruudut.SetTileMethod(Vakiot.PELAAJAN_2_SPAWNI, delegate(Vector p, double w, double h, IntPoint posInLevel)
        {
            positions[1] = p;
        });
        ruudut.SetTileMethod(Vakiot.PELAAJIEN_VAIHTOEHTOINEN_SPAWNI, delegate(Vector p, double w, double h, IntPoint posInLevel)
        {
            positions.Add(p);
        });
        // ruudut.SetTileMethod(Color.YellowGreen, LuoLapiMentavaKentanOsa, helikopteriLaskeutumisAlusta, "laskeutumisalusta", 1.0, 1.0, -1);
        ruudut.SetTileMethod(Color.YellowGreen, delegate(Vector p, double w, double h, IntPoint posInLevel)
        {
            LuoLapiMentavaKentanOsa(p, w, h, posInLevel, helikopteriLaskeutumisAlusta, "laskeutumisalusta", 6.0, 6.0, -1, false);
            if (Escape.Peli != null)
                Escape.Peli.LaskeutumisPaikka = p;
        });
        ruudut.SetTileMethod(Color.ForestGreen, LuoLapiMentavaKentanOsa, kuusi, "kuusi", 4.0, 4.0, 1, true);
        ruudut.SetTileMethod(Vakiot.VERILAIKKA, delegate(Vector p, double w, double h, IntPoint posInLevel) { Blood.AddNormalBlood(p, 5, 1.5); });

        ruudut.Execute(Vakiot.KENTAN_RUUDUN_LEVEYS, Vakiot.KENTAN_RUUDUN_KORKEUS);
        return positions;
    }

    /// <summary>
    /// Luodaan kenttä onlinepelille.
    /// </summary>
    /// <param name="pelaajat">Pelaajat pelissä.</param>
    /// <param name="kenttaTyyppi">Kentän tyyppi. 0: klassinen mäppi, 1: satunnaisgeneroitu mäppi, 2: tyhjä mäppi.</param>
    void LuoOnlineKentta(List<Player> onlinePelaajat, int kenttaTyyppi)
    {
        Level.Background.Image = null;
        Mouse.IsCursorVisible = false;
        AikaKentanAlusta.Reset();
        AikaKentanAlusta.Start();
        MessageDisplay.BackgroundColor = Color.Transparent;
        MessageDisplay.TextColor = Color.White;
        MessageDisplay.MessageTime = TimeSpan.FromSeconds(1.5);
        Level.Size = new Vector(Vakiot.KENTAN_LEVEYS * Vakiot.KENTAN_RUUDUN_LEVEYS, Vakiot.KENTAN_KORKEUS * Vakiot.KENTAN_RUUDUN_KORKEUS);
        Level.AmbientLight = 0.8;
        Add(valo);

        foreach (Player pelaaja in onlinePelaajat)
        {
            Pelaaja p = new Pelaaja(43.5, 21.75, false, false);
            p.Position = pelaaja.Position;
            p.Angle = pelaaja.Angle;
            p.Velocity = pelaaja.Velocity;
            p.Elamat.Value = pelaaja.Health;
            Add(p);
            OnlineGame.PhysicalPlayers.Add(p);
        }

        pelaajat[0] = LuoPelaaja(new Vector(0.0, 0.0), 43.5, 21.75, 1, Aseet.pelaaja1pistooliKuva, Color.Green, AsetustenKaytto.Asetukset.OnkoPelaajalla1Lasertahtainta, new Vector(Screen.Left + 130, Screen.Top - 70), false, true); // false = onko taskulamppua
        Add(pelaajat[0], 1);
        ClearControls();
        AsetaOhjaimet();
        SoitaMusiikkia(4); // OrcsCome Special nettipeliin
    }

    void AjastaAseidenAvautumisIlmoitukset()
    {
        for (int i = 0; i < aseidenLuontimetodit.Length; i++)
        {
            Ase ase = aseidenLuontimetodit[i]();
            AikaKentanAlusta.SecondCounter.AddTrigger(ase.aikaJolloinVoiLuoda, TriggerDirection.Up, delegate() 
            {
                foreach (Pelaaja p in pelaajat)
                {
                    p.NaytaViesti(ase.AseenNimi + " saatavilla", 1.0);
                }
            });
            //AjastaIlmoitusPelaajille(ase.AseenNimi + " saatavilla", ase.aikaJolloinVoiLuoda);
        }
    }

    #region GameOfLife-satunnaisgeneraatio

    /// <summary>
    /// Luodaan kentälle kerättävä esine. 
    /// </summary>
    /// <param name="mikaLuodaan">Mikä esine luodaan, välillä 0..6.
    /// (0:Pistooli, 1:Rynkky, 2:Haulikko, 3:Minigun, 4:Varsijousi, 5:Snipa, 6:Sinko, 7:Kranaatti)</param>
    /// <param name="paikka">Paikka, johon esine luodaan.</param>
    void LuoKerattavaEsine(Vector paikka, double leveys, double korkeus, IntPoint posInLevel)
    {
        int mikaLuodaan = RandomGen.NextIntWithProbabilities(0.20, 0.15, 0.10, 0.10, 0.10, 0.10, 0.05, 0.05, 0.15);
        switch (mikaLuodaan)
        {
            // Pistooli
            case 0:
                Kerattava pistooli = LuoKerattava(paikka, Aseet.LuoPistooli(), pistoolinMaaKuva, 0.5);
                pistooli.Kerattiin += JotainKerattiin;
                Add(pistooli);
                break;

            // Rynkky
            case 1:
                Kerattava rynkky = LuoKerattava(paikka, Aseet.LuoRynkky(), rynkynMaaKuva);
                rynkky.Kerattiin += JotainKerattiin;
                Add(rynkky);
                break;

            // Haulikko
            case 2:
                Kerattava haulikko = LuoKerattava(paikka, Aseet.LuoHaulikko(), pumpparinMaaKuva);
                haulikko.Kerattiin += JotainKerattiin;
                Add(haulikko);
                break;
            case 3:
                Kerattava magnum = LuoKerattava(paikka, Aseet.LuoMagnum(), magnuminMaaKuva);
                magnum.Kerattiin += JotainKerattiin;
                Add(magnum);
                break;

            // Minigun
            case 4:
                Kerattava minigun = LuoKerattava(paikka, Aseet.LuoMinigun(), sawinMaaKuva);
                minigun.Kerattiin += JotainKerattiin;
                Add(minigun);
                break;

            // Varsijousi
            case 5:
                /*Kerattava varsijousi = LuoKerattava(paikka, Aseet.LuoVarsijousi(), new Image(512, 512, Color.White));
                varsijousi.Kerattiin += JotainKerattiin;
                Add(varsijousi);*/
                break;
            // Snipa
            case 6:
                Kerattava snipa = LuoKerattava(paikka, Aseet.LuoSnipa(), snipanMaaKuva);
                snipa.Kerattiin += JotainKerattiin;
                Add(snipa);
                break;

            // Sinko
            case 7:
                Kerattava sinko = LuoKerattava(paikka, Aseet.LuoSinko(), singonMaaKuva);
                sinko.Kerattiin += JotainKerattiin;
                Add(sinko);
                break;

            case 8:
                Kerattava kranaatti = LuoKerattava(paikka, "kranaatti", kranaatinMaaKuva, 0.8);
                kranaatti.Kerattiin += JotainKerattiin;
                Add(kranaatti);
                break;
            case 9:
                /*Kerattava vintorez = LuoKerattava(paikka, Aseet.LuoVintorez(), vintorezMaaKuva);
                vintorez.Kerattiin += JotainKerattiin;
                Add(vintorez);*/
                break;
        }
    }

    /// <summary>
    /// Luodaan kerättävä esine.
    /// </summary>
    /// <param name="paikka">Esineen paikka.</param>
    /// <param name="mitaKerattiin">Mitä esineestä saa.</param>
    /// <param name="kuva">Esineen kuva. Voi olla null.</param>
    /// <param name="kuvanKoonMuutosKerroin">Paljonko kuvan kokoa muutetaan piirrettäessä. 1.0 ei tee mitään, suurempi kasvattaa ja pienempi pienentää.</param>
    /// <returns>Esine.</returns>
    Kerattava LuoKerattava(Vector paikka, object mitaKerattiin, Image kuva, double kuvanKoonMuutosKerroin = 1.0)
    {
        Kerattava k = new Kerattava(kuva.Width / 8 * kuvanKoonMuutosKerroin, kuva.Height / 8 * kuvanKoonMuutosKerroin, mitaKerattiin, paikka);
        if (kuva != null)
            k.Image = kuva;
        return k;
    }

    /// <summary>
    /// Kutsutaan, kun pelaaja törmää kerättävään esineeseen.
    /// </summary>
    /// <param name="p">Pelaaja, joka törmäsi.</param>
    /// <param name="keratty">Se, mitä pelaaja sai kerättävästä esineestä.</param>
    void JotainKerattiin(Pelaaja p, object keratty)
    {
        if (keratty is Ase)
        {
            Ase a = keratty as Ase;
            if (!p.OnkoPelaajallaAse(a.Tag.ToString()))
            {
                p.LisaaAse(a);
                p.NaytaViesti(Vakiot.HUD_SAIT_ASEEN + a.AseenNimi, 1.0);
            }
            else
            {
                for (int i = 0; i < p.Aseet.Count; i++)
                {
                    if (p.Aseet[i].Tag.ToString() == a.Tag.ToString()) p.Aseet[i].MaxAmmo.Value += p.Aseet[i].Ammo.MaxValue; // jos pelaajalla on jo ase, annetaan aseeseen lippaallinen ammuksia
                    p.NaytaViesti(Vakiot.HUD_SAIT_AMMUKSIA_ASEESEEN + a.AseenNimi, 1.0);
                }
            }
        }
        if (keratty is String)
        {
            if (keratty.ToString() == "kranaatti")
            {
                p.KranaattienMaara++;
                p.NaytaViesti(Vakiot.HUD_SAIT_KRANAATIN + p.KranaattienMaara.ToString(), 1.0);
            }
            if (keratty.ToString() == "kivi")
            {
                p.SeinienMaara += Vakiot.SEINIA_KERRALLA;
                p.NaytaViesti("Sait " + Vakiot.SEINIA_KERRALLA.ToString() + " seinää", 1.0);
            }
        }
    }

    /// <summary>
    /// Luodaan satunnainen String-taulukko nollista ja ykkösistä, jota voidaan käyttää kentän luontiin.
    /// </summary>
    /// <param name="leveys">Kentän leveys.</param>
    /// <param name="korkeus">Kentän korkeus.</param>
    /// <returns>Kenttä binäärimuodossa.</returns>
    String[] LuoSatunnainenKenttäTiedosto(int leveys, int korkeus)
    {
        const int ARVOTTAVIA_SUKUPOLVIA = 2;
        int[,] kentanArvot = new int[leveys, korkeus];
        for (int i = 0; i < kentanArvot.GetLength(0); i++)
        {
            for (int j = 0; j < kentanArvot.GetLength(1); j++)
            {
                kentanArvot[i, j] = RandomGen.NextIntWithProbabilities(0.1);
            }
        }
        int[,] kenttaLukuina = GameOfLife.ArvoSukupolvia(kentanArvot, leveys, korkeus, ARVOTTAVIA_SUKUPOLVIA);
        String[] tulos = TaulukkoStringiksi(kenttaLukuina);
        return tulos;
    }

    /// <summary>
    /// Muunnetaan kaksiulotteinen int-taulukko string-taulukoksi.
    /// </summary>
    /// <param name="taulukko">Alkuperäinen int-taulukko.</param>
    /// <returns>Int-taulukko string-taulukkona.</returns>
    String[] TaulukkoStringiksi(int[,] taulukko)
    {
        String[] tulos = new String[taulukko.GetLength(1)];
        for (int rivi = 0; rivi < taulukko.GetLength(0); rivi++)
        {
            for (int sarake = 0; sarake < taulukko.GetLength(1); sarake++)
            {
                tulos[rivi] += taulukko[rivi, sarake];
            }
        }
        return tulos;
    }

    /// <summary>
    /// Palauttaa string-lukutaulukon muunnettuna 2D-int-taulukoksi. Taulukon rivien pitää
    /// olla samanpituisia.
    /// </summary>
    /// <param name="taulukko">Muunnettava taulukko.</param>
    /// <returns>Taulukko int-muodossa.<returns>
    int[,] TaulukkoIntiksi(String[] taulukko)
    {
        int[,] tulos = new int[taulukko.Length, taulukko[0].Length];
        for (int rivi = 0; rivi < taulukko.Length; rivi++)
        {
            for (int sarake = 0; sarake < taulukko[0].Length; sarake++)
            {
                tulos[rivi, sarake] = int.Parse(taulukko[rivi][sarake].ToString());
            }
        }
        return tulos;
    }

    /// <summary>
    /// Muuntaa int-taulukon bool-taulukoksi. Luvusta 0 tulee true, muista false.
    /// </summary>
    /// <param name="taulukkoIntina">Muunnettava taulukko.</param>
    /// <returns>Taulukko boolina.</returns>
    bool[,] TaulukkoBooliksi(int[,] taulukkoIntina)
    {
        bool[,] tulos = new bool[taulukkoIntina.GetLength(0), taulukkoIntina.GetLength(1)];
        for (int i = 0; i < taulukkoIntina.GetLength(0); i++)
        {
            for (int j = 0; j < taulukkoIntina.GetLength(1); j++)
            {
                if (taulukkoIntina[i, j] == 0) tulos[i, j] = true;
                else tulos[i, j] = false;
            }
        }
        return tulos;
    }

    /// <summary>
    /// Palauttaa int-taulukon, jonka alkioiden arvot voivat olla VAIN 0 tai 1 käännettynä siten, että alkioiden
    /// arvot ovat päinvastaiset.
    /// </summary>
    /// <param name="taulukko">Käännettävä taulukko.</param>
    /// <returns>Käännetty taulukko.</returns>
    int[,] KaannaTaulukko(int[,] taulukko)
    {
        for (int i = 0; i < taulukko.GetLength(0); i++)
        {
            for (int j = 0; j < taulukko.GetLength(1); j++)
            {
                if (taulukko[i, j] == 0) taulukko[i, j] = 1;
                else taulukko[i, j] = 0;
            }
        }
        return taulukko;
    }

    /// <summary>
    /// Palauttaa niiden taulukon alkioiden indeksit, joissa on etsittävä luku.
    /// </summary>
    /// <param name="taulukko">Taulukko, josta lukua etsitään.</param>
    /// <param name="mitäEtsitään">Mitä lukua etsitään.</param>
    /// <returns>Indeksit vektorilistana.</returns>
    List<Vector> TaulukonAlkioidenIndeksit(int[,] taulukko, int mitäEtsitään)
    {
        List<Vector> indeksit = new List<Vector>();
        for (int i = 0; i < taulukko.GetLength(0); i++)
        {
            for (int j = 0; j < taulukko.GetLength(1); j++)
            {
                if (taulukko[i, j] == mitäEtsitään) indeksit.Add(new Vector(i, j));
            }
        }
        return indeksit;
    }

    /// <summary>
    /// Luodaan tiedosto pickupien luontia varten siten, ettei pickupeja mene seinien päälle.
    /// </summary>
    /// <param name="kentanKiinteatOsat">Kentän seinät.</param>
    /// <returns>String-taulukko, jossa on pickupien paikat.</returns>
    String[] LuoSatunnainenPickupTiedosto(String[] kentanKiinteatOsat, int pickupienMaara)
    {
        int[,] pickupienPaikat = new int[kentanKiinteatOsat.Length, kentanKiinteatOsat[0].Length];

        // Ne paikat, jotka ovat tyhjiä kenttägeneraation jäljiltä.
        int[,] pickupienMahdollisetPaikat = TaulukkoIntiksi(kentanKiinteatOsat);
        pickupienMahdollisetPaikat = KaannaTaulukko(pickupienMahdollisetPaikat);
        List<Vector> indeksitJoissaOnTyhjää = TaulukonAlkioidenIndeksit(pickupienMahdollisetPaikat, 1);

        for (int i = 0; i < pickupienMaara; i++)
        {
            Vector paikka = RandomGen.SelectOne<Vector>(indeksitJoissaOnTyhjää);
            int x = (int)paikka.X;
            int y = (int)paikka.Y;
            pickupienPaikat[x, y] = 1;
        }
        String[] pickupienPaikatStringinä = TaulukkoStringiksi(pickupienPaikat);
        return pickupienPaikatStringinä;
    }
    #endregion

    #region ruutusatunnaisgeneraatio

    /// <summary>
    /// Luo satunnaisen ColorTileMap-ruutukentän yhdistelemällä pienempiä ColorTileMap-ruutuja.
    /// </summary>
    /// <param name="tiles">Yksittäiset ruudut.</param>
    /// <returns></returns>
    Image LuoSatunnainenRuutuKentta(string filename, int startIndex, int endIndex)
    {
        Image[] tiles = LoadImages(filename, startIndex, endIndex);
        InfiniteMapGen generator = new InfiniteMapGen(tiles, Vakiot.TILES_HORIZONTAL, Vakiot.TILES_VERTICAL, Vakiot.EXIT_START_PIXEL, Vakiot.EXIT_END_PIXEL,
            Vakiot.EXIT_START_PIXEL, Vakiot.EXIT_END_PIXEL, Vakiot.EXIT_START_PIXEL, Vakiot.EXIT_END_PIXEL, Vakiot.EXIT_START_PIXEL, Vakiot.EXIT_END_PIXEL);
        return generator.GenerateMap();
    }

    #endregion

    #region kentän osat

    /// <summary>
    /// Luo staattisen kentän osan, joka voi tuhoutua.
    /// </summary>
    /// <param name="paikka">Kappaleen paikka.</param>
    /// <param name="leveys">Kappaleen leveys.</param>
    /// <param name="korkeus">Kappaleen korkeus.</param>
    /// <param name="kuva">Kappaleen kuva.</param>
    /// <param name="tag">Kappaleen tagi.</param>
    /// <param name="kesto">Kappaleen kesto.</param>
    public void LuoTuhoutuvaKentanOsa(Vector paikka, double leveys, double korkeus, IntPoint positionInLevelArray, Image kuva, string tag, int kesto, double leveydenKerroin, double korkeudenKerroin)
    {
        Tuhoutuva kentanosa = new Tuhoutuva(leveys * leveydenKerroin, korkeus * korkeudenKerroin, kesto);
        kentanosa.Position = paikka;
        kentanosa.MakeStatic();
        kentanosa.Image = kuva;
        kentanosa.Tag = tag;
        kentanosa.CollisionIgnoreGroup = 1;
        kentanosa.PositionInLevelArray = positionInLevelArray;
        kentanosa.Kesto.LowerLimit += delegate
        {
            if (KentanOsat != null)
                KentanOsat.TuhoaSeina(kentanosa.PositionInLevelArray.X, kentanosa.PositionInLevelArray.Y);
            kentanosa.Destroy();
        };
        Add(kentanosa);

        if (KentanOsat != null)
            KentanOsat.LisaaSeina(positionInLevelArray.X, positionInLevelArray.Y, kentanosa);
    }

    /// <summary>
    /// Luo staattisen kentän osan, joka ei voi tuhoutua.
    /// </summary>
    /// <param name="paikka">Kappaleen paikka.</param>
    /// <param name="leveys">Kappaleen leveys.</param>
    /// <param name="korkeus">Kappaleen korkeus.</param>
    /// <param name="kuva">Kappaleen kuva.</param>
    /// <param name="tag">Kappaleen tagi.</param>
    /// <param name="collisionIgnoreGroup">Kappaleen törmäysryhmä.</param>
    void LuoKentanOsa(Vector paikka, double leveys, double korkeus, IntPoint positionInLevelArray, Image kuva, string tag, int collisionIgnoreGroup)
    {
        PhysicsObject kentanosa = new PhysicsObject(leveys, korkeus);
        kentanosa.Position = paikka;
        kentanosa.MakeStatic();
        kentanosa.Image = kuva;
        kentanosa.Tag = tag;
        kentanosa.CollisionIgnoreGroup = collisionIgnoreGroup;
        kentanosa.PositionInLevelArray = positionInLevelArray;
        Add(kentanosa);

        KentanOsat.LisaaSeina(positionInLevelArray.X, positionInLevelArray.Y, kentanosa);
    }

    /// <summary>
    /// Luo liikkumattoman kentän osan, johon ei voi törmätä.
    /// </summary>
    /// <param name="paikka">Kappaleen paikka.</param>
    /// <param name="leveys">Kappaleen leveys.</param>
    /// <param name="korkeus">Kappaleen korkeus.</param>
    /// <param name="kuva">Kappaleen kuva.</param>
    /// <param name="tag">Kappaleen tagi.</param>
    void LuoLapiMentavaKentanOsa(Vector paikka, double leveys, double korkeus, IntPoint positionInLevelArray, Image kuva, string tag, double leveydenKerroin, double korkeudenKerroin, int layer, bool satunnainenKulma)
    {
        GameObject kentanosa = new GameObject(leveys * leveydenKerroin, korkeus * korkeudenKerroin);
        kentanosa.Image = kuva;
        kentanosa.Tag = tag;
        kentanosa.Position = paikka;
        kentanosa.PositionInLevelArray = positionInLevelArray;
        if (satunnainenKulma)
            kentanosa.Angle = RandomGen.NextAngle();
        Add(kentanosa, layer);

        KentanOsat.LisaaSeina(positionInLevelArray.X, positionInLevelArray.Y, kentanosa);
    }

    /// <summary>
    /// Luodaan liikkuva kentän osa.
    /// </summary>
    /// <param name="paikka">Paikka.</param>
    /// <param name="leveys">Leveys.</param>
    /// <param name="korkeus">Korkeus.</param>
    /// <param name="kuva">Kuva.</param>
    /// <param name="tag">Tagi.</param>
    /// <param name="shape">Muoto.</param>
    /// <returns>Kentän osa.</returns>
    PhysicsObject LuoLiikkuvaKentanOsa(Vector paikka, double leveys, double korkeus, Image kuva, string tag, Shape shape)
    {
        PhysicsObject kentanosa = new PhysicsObject(leveys, korkeus);
        kentanosa.Position = paikka;
        kentanosa.Tag = tag;
        if (kuva != null) kentanosa.Image = kuva;
        kentanosa.Shape = shape;
        Add(kentanosa);
        return kentanosa;
    }

    /// <summary>
    /// Luodaan räjähtävä tynnyri.
    /// </summary>
    /// <param name="paikka">Tynnyrin paikka.</param>
    /// <param name="leveys">Tynnyrin leveys.</param>
    /// <param name="korkeus">Tynnyrin korkeus.</param>
    void LuoTynnyri(Vector paikka, double leveys, double korkeus, IntPoint posInLevel)
    {
        Tuhoutuva tynnyri = new Tuhoutuva(leveys / 1.5, korkeus / 1.5, 1);
        tynnyri.Image = tynnyrinKuva;
        tynnyri.Position = paikka;
        tynnyri.Shape = Shape.Circle;
        tynnyri.Mass = 600;
        tynnyri.LinearDamping = 0.90;
        tynnyri.AngularDamping = 0.90;
        tynnyri.Angle = RandomGen.NextAngle();
        tynnyri.Shatters = false;
        tynnyri.Extinguished += delegate
        {
            Partikkelit.AddExplosionEffect(tynnyri.Position, 600);
            //Flame f = Partikkelit.CreateFlames(tynnyri.Position, 30);
            //Add(f);

            Explosion rajahdys = new Explosion(200);
            rajahdys.Position = tynnyri.Position;
            rajahdys.Image = tyhjä;
            rajahdys.Sound = Aseet.singonAani;
            rajahdys.Force = 400;
            rajahdys.Speed = 1000;
            rajahdys.ShockwaveColor = Color.Black;
            rajahdys.ShockwaveReachesObject += delegate(IPhysicsObject paineaallonKohde, Vector shokki) { RajahdysOsuu(paineaallonKohde, shokki, tynnyri.Position, Aseet.SINGON_MAKSIMI_DAMAGE); };
            Add(rajahdys);
            tynnyri.Destroy();

        };

        tynnyri.Kesto.LowerLimit += delegate
        {
            if (tynnyri.IsDestroying || tynnyri.IsDestroyed) return;
            Partikkelit.AddExplosionEffect(tynnyri.Position, 600);
            //Flame f = Partikkelit.CreateFlames(tynnyri.Position, 30);
            //Add(f);

            Explosion rajahdys = new Explosion(200);
            rajahdys.Position = tynnyri.Position;
            rajahdys.Image = tyhjä;
            rajahdys.Sound = Aseet.singonAani;
            rajahdys.Force = 400;
            rajahdys.Speed = 1000;
            rajahdys.ShockwaveColor = Color.Black;
            rajahdys.ShockwaveReachesObject += delegate(IPhysicsObject paineaallonKohde, Vector shokki) { RajahdysOsuu(paineaallonKohde, shokki, tynnyri.Position, Aseet.SINGON_MAKSIMI_DAMAGE); };
            Add(rajahdys);
            tynnyri.Destroy();
        };
        Add(tynnyri);
        //tynnyri.Hit(RandomGen.NextVector(100.0, 1000.0));
    }

    /// <summary>
    /// Luodaan kentän päälle pilviä, jotka liikkuvat hiljalleen.
    /// </summary>
    void LuoPilvet(bool onkoInfinite)
    {
        GameObject pilvet = new GameObject(8192, 8192);
        if (onkoInfinite) pilvet.Image = LoadImage("himmeatpilvet");
        else pilvet.Image = LoadImage("pilvet2");
        pilvet.Tag = "pilvet";
        Add(pilvet, 3);
        Layers[3].RelativeTransition = new Vector(2.0, 2.0);
        RandomMoverBrain rb = new RandomMoverBrain(0.3);
        pilvet.Brain = rb;
        rb.WanderRadius = 1000;
        rb.ChangeMovementSeconds = 10.0;
        rb.TurnWhileMoving = false;
    }

    /// <summary>
    /// Luodaan laatikko, josta voi kerätä kamaa.
    /// </summary>
    /// <param name="paikka">Laatikon paikka.</param>
    /// <param name="leveys">Laatikon leveys.</param>
    /// <param name="korkeus">Laatikon korkeus.</param>
    void LuoLaatikko(Vector paikka, double leveys, double korkeus, IntPoint posInLevel)
    {
        Laatikko laatikko = new Laatikko(leveys, korkeus);
        laatikko.Animation = new Animation(laatikonAnimaatioKuvat);
        laatikko.Animation.Start();
        laatikko.MakeStatic();
        laatikko.Animation.FPS = 10;
        laatikko.Position = paikka;
        laatikko.Tag = "laatikko";
        AddCollisionHandler(laatikko, AnnaKamaa);
        Add(laatikko);

        if (KentanOsat != null)
            KentanOsat.LisaaSeina(posInLevel.X, posInLevel.Y, laatikko);
    }

    void LuoKillBall(Vector paikka)
    {
        PhysicsObject killball = LuoLiikkuvaKentanOsa(paikka, 200.0, 200.0, killballPohja, "killball", Shape.Circle);
        killball.AngularDamping = 1.0;
        killball.AngularVelocity = 5.0;

        PhysicsObject killballKeskiOsa = new PhysicsObject(200.0, 200.0);
        killballKeskiOsa.Image = killballKeski;
        killballKeskiOsa.MakeStatic();
        killballKeskiOsa.Shape = Shape.Circle;
        killballKeskiOsa.AngularDamping = 1.0;
        killballKeskiOsa.AngularVelocity = -6.0;
        killball.Add(killballKeskiOsa);

        PhysicsObject killballYlaOsa = new PhysicsObject(200.0, 200.0);
        killballYlaOsa.Image = killballYla;
        killballYlaOsa.MakeStatic();
        killballYlaOsa.Shape = Shape.Circle;
        killballYlaOsa.AngularDamping = 1.0;
        killballYlaOsa.AngularVelocity = 8.0;
        killballKeskiOsa.Add(killballYlaOsa);

        AddCollisionHandler<PhysicsObject, Pelaaja>(
            killball, delegate(PhysicsObject kb, Pelaaja pelaaja)
        {
            Damagea(pelaaja, 20);
            PlaySound("KillBall_Kill");
        });
    }

    void LuoHelikopteri(Vector paikka)
    {
        Helikopteri kopteri = new Helikopteri(330, 73, 50, false); // 211, 171
        kopteri.Position = paikka;
        Add(kopteri, 2);

#if DEBUG
        Vector[] reitti = { new Vector(200.0, 200.0), new Vector(100.0, -500.0), new Vector(0.0, -100.0) };
        Keyboard.Listen(Key.I, ButtonState.Pressed, kopteri.NouseIlmaan, null);
        Keyboard.Listen(Key.U, ButtonState.Pressed, kopteri.Laskeudu, null);
        Keyboard.Listen(Key.Y, ButtonState.Pressed, delegate { kopteri.LiikutaKopteria(100.0, true, reitti); }, null);
#endif
    }

    /// <summary>
    /// Asetetaan pelaajan lisäämät seinät siten, että se eivät mene sisäkkäin.
    /// </summary>
    /// <param name="realPos">Paikka, johon seinää yritetään lisätä.</param>
    Vector SijoitaKentanosaRuudukkoon(Vector mousePos)
    {
        int merkki = Math.Sign(mousePos.X);
        double x = mousePos.X - mousePos.X % Vakiot.KENTAN_RUUDUN_LEVEYS + merkki * Vakiot.KENTAN_RUUDUN_LEVEYS / 2;
        merkki = Math.Sign(mousePos.Y);
        double y = mousePos.Y - (mousePos.Y % Vakiot.KENTAN_RUUDUN_KORKEUS) + merkki * Vakiot.KENTAN_RUUDUN_KORKEUS / 2;

        Vector posRuudukossa = new Vector(x, y);
        return posRuudukossa;
    }

    #endregion

    #endregion

    #region valikot
    /// <summary>
    /// Luodaan pelille alkuvalikko.
    /// </summary>
    /// <param name="isEverythingCleared">Jos kutsutaan ClearAllin jälkeen.</param>
    void LuoAlkuValikko(bool isEverythingCleared = false)
    {
        MW2_My_Warfare_2_.Peli = this;
        AsetustenKaytto.LataaAsetukset();
        Infinite.OnkoPeliKaynnissa = false;
        CurrentGameMode = Gamemode.None;
#if DEBUG
        AnimatedMultiSelectWindow valikko = new AnimatedMultiSelectWindow("MY WARFARE 2", valikonPainikeKuva,
        "Uusi peli", "Online", "Asetukset", "Lopeta peli");
#else
        AnimatedMultiSelectWindow valikko = new AnimatedMultiSelectWindow("MY WARFARE 2", valikonPainikeKuva,
        "Uusi peli", "Asetukset", "Lopeta peli");
#endif
        valikko.ItemSelected += PainettiinAlkuValikonNappia;
        valikko.Color = Color.Transparent;
        valikko.BorderColor = Color.Transparent;
        valikko.MovementSpeed = 450;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        Level.AmbientLight = 1.0;
        valikko.DefaultCancel = 4;
        //Partikkelit.LisaaValikkoTaustaPartikkelit();
        Add(valikko);

        if (menuGradient == null || menuGradient.IsDestroyed || !menuGradient.IsAddedToGame || isEverythingCleared) // jos null tai poistettu pelistä tai jos tullaan ClearAllin kautta
        {
            menuGradient = new GameObject(1920, 1080);
            menuGradient.Image = LoadImage("menu_gradient");
            Add(menuGradient);
        }
    }

    /// <summary>
    /// Alkuvalikon nappien painalluksen käsittely.
    /// </summary>
    /// <param name="valinta">Painettu nappi.</param>
    void PainettiinAlkuValikonNappia(int valinta)
    {
        switch (valinta)
        {
#if DEBUG
            case 0:
                LuoPelimuodonValintaValikko();
                break;
            case 1:
                LuoOnlineValikko();
                break;
            case 2:
                LuoAsetusIkkuna();
                break;
            case 3:
                Exit();
                break;
#else
            case 0:
                LuoPelimuodonValintaValikko();
                break;
            case 1:
                LuoAsetusIkkuna();
                break;
            case 2:
                Exit();
                break;
#endif
        }
    }

    void LuoOnlineValikko()
    {
        AnimatedMultiSelectWindow onlineValikko = new AnimatedMultiSelectWindow("Valitse pelimuoto", valikonPainikeKuva, "Liity peliin", "Syötä IP", "Syötä portti", "Syötä salasana", "Takaisin");
        onlineValikko.ItemSelected += PainettiinOnlineValikonNappia;
        onlineValikko.Color = Color.Transparent;
        onlineValikko.BorderColor = Color.Transparent;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        onlineValikko.DefaultCancel = 2;
        //onlineValikko.Position = new Vector(0, Screen.Top - 100);
        Add(onlineValikko);

    }

    void PainettiinOnlineValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                OnlineGame.JoinGame(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(OnlineGame.ServerIP.ToString()), OnlineGame.ServerPort));
                LuoOnlineKentta(OnlineGame.CurrentGame.ConnectedPlayers, 2);

                break;
            case 1:
                InputWindow ipIkkuna = new InputWindow("Syötä serverin IP:");
                ipIkkuna.Color = Color.Transparent;
                ipIkkuna.InputBox.BorderColor = Color.Green;
                ipIkkuna.InputBox.Cursor.Color = Color.Green;
                ipIkkuna.MaxCharacters = 15;
                ipIkkuna.OKButton.Clicked += delegate
                {
                    OnlineGame.ServerIP.Clear();
                    OnlineGame.ServerIP.Append(ipIkkuna.InputBox.Text);
                    LuoOnlineValikko();
                };
                Add(ipIkkuna);
                break;
            case 2:
                InputWindow porttiIkkuna = new InputWindow("Syötä serverin portti:");
                porttiIkkuna.Color = Color.Transparent;
                porttiIkkuna.InputBox.BorderColor = Color.Green;
                porttiIkkuna.InputBox.Cursor.Color = Color.Green;
                porttiIkkuna.MaxCharacters = 5;
                porttiIkkuna.OKButton.Clicked += delegate
                {
                    OnlineGame.ServerPort = int.Parse(porttiIkkuna.InputBox.Text);
                    LuoOnlineValikko();
                };
                Add(porttiIkkuna);

                break;
            case 3:
                InputWindow salasanaIkkuna = new InputWindow("Syötä serverin portti:");
                salasanaIkkuna.Color = Color.Transparent;
                salasanaIkkuna.InputBox.BorderColor = Color.Green;
                salasanaIkkuna.InputBox.Cursor.Color = Color.Green;
                salasanaIkkuna.OKButton.Clicked += delegate
                {
                    OnlineGame.ServerPassword.Clear();
                    OnlineGame.ServerPassword.Append(salasanaIkkuna.InputBox.Text);
                    LuoOnlineValikko();
                };
                Add(salasanaIkkuna);
                break;
            case 4:
                LuoAlkuValikko();
                break;
        }
    }

    /// <summary>
    /// Asetukset, kuten fullscreen-valinta, antialiasing ja resoluutio.
    /// </summary>
    void LuoAsetusIkkuna()
    {
        AsetustenKaytto.LataaAsetukset();
        Mouse.IsCursorVisible = true;
        MultiSelectWindow paluu = new MultiSelectWindow("Asetukset", "Tallenna ja palaa päävalikkoon", "Palauta oletusasetukset");
        paluu.ItemSelected += delegate(int valinta)
        {
            switch (valinta)
            {
                case 0:
                    AsetustenKaytto.TallennaAsetukset();
                    PaaValikkoon();
                    break;
                case 1:
                    AsetustenKaytto.PalautaOletusAsetukset();
                    AsetustenKaytto.TallennaAsetukset();
                    PaaValikkoon();
                    break;
            }
        };
        paluu.Color = Color.Transparent;
        paluu.BorderColor = Color.Transparent;
        paluu.SelectionColor = Color.Green;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        paluu.DefaultCancel = 0;
        paluu.Position = new Vector(0, Screen.Top - 100);
        Add(paluu);

        OnOffButton fullscreenNappi = new OnOffButton(20, 20, AsetustenKaytto.Asetukset.OnkoFullscreen);
        fullscreenNappi.Position = new Vector(0.0, paluu.Y - 100);
        Add(fullscreenNappi);
        fullscreenNappi.Clicked += delegate { AsetustenKaytto.Asetukset.OnkoFullscreen = fullscreenNappi.IsPressed; this.IsFullScreen = AsetustenKaytto.Asetukset.OnkoFullscreen; };

        Label fullscreenTeksti = new Label("Fullscreen:");
        fullscreenTeksti.Position = new Vector(fullscreenNappi.X - 100, fullscreenNappi.Y);
        Add(fullscreenTeksti);

        OnOffButton antialiasingNappi = new OnOffButton(20, 20, AsetustenKaytto.Asetukset.OnkoAntialiasing);
        antialiasingNappi.Position = new Vector(0.0, fullscreenNappi.Y - 50);
        Add(antialiasingNappi);
        antialiasingNappi.Clicked += delegate { AsetustenKaytto.Asetukset.OnkoAntialiasing = antialiasingNappi.IsPressed; Game.SmoothTextures = AsetustenKaytto.Asetukset.OnkoAntialiasing; };

        Label antialiasingTeksti = new Label("Antialiasing:");
        antialiasingTeksti.Position = new Vector(antialiasingNappi.X - 100, antialiasingNappi.Y);
        Add(antialiasingTeksti);

        Label resoluutiotOtsikko = new Label("Resoluutio:");
        resoluutiotOtsikko.Position = new Vector(antialiasingTeksti.X, antialiasingTeksti.Y - 50);
        Add(resoluutiotOtsikko);

        PushButton resoluutio800x600 = new PushButton(20, 20);
        resoluutio800x600.Position = new Vector(100.0, resoluutiotOtsikko.Y - 50);
        Add(resoluutio800x600);
        resoluutio800x600.Clicked += delegate { AsetaIkkunanKoko(800, 600); };

        Label resoluutio800x600Teksti = new Label("800 x 600");
        resoluutio800x600Teksti.Position = new Vector(resoluutio800x600.X - 100, resoluutio800x600.Y);
        Add(resoluutio800x600Teksti);

        PushButton resoluutio1024x768 = new PushButton(20, 20);
        resoluutio1024x768.Position = new Vector(100.0, resoluutio800x600.Y - 50);
        Add(resoluutio1024x768);
        resoluutio1024x768.Clicked += delegate { AsetaIkkunanKoko(1024, 768); };

        Label resoluutio1024x768Teksti = new Label("1024 x 768");
        resoluutio1024x768Teksti.Position = new Vector(resoluutio1024x768.X - 100, resoluutio1024x768.Y);
        Add(resoluutio1024x768Teksti);

        PushButton resoluutio1280x1024 = new PushButton(20, 20);
        resoluutio1280x1024.Position = new Vector(100.0, resoluutio1024x768.Y - 50);
        Add(resoluutio1280x1024);
        resoluutio1280x1024.Clicked += delegate { AsetaIkkunanKoko(1280, 1024); };

        Label resoluutio1280x1024Teksti = new Label("1280 x 1024");
        resoluutio1280x1024Teksti.Position = new Vector(resoluutio1280x1024.X - 100, resoluutio1280x1024.Y);
        Add(resoluutio1280x1024Teksti);

        PushButton resoluutio1600x1200 = new PushButton(20, 20);
        resoluutio1600x1200.Position = new Vector(100.0, resoluutio1280x1024.Y - 50);
        Add(resoluutio1600x1200);
        resoluutio1600x1200.Clicked += delegate { AsetaIkkunanKoko(1600, 1200); };

        Label resoluutio1600x1200Teksti = new Label("1600 x 1200");
        resoluutio1600x1200Teksti.Position = new Vector(resoluutio1600x1200.X - 100, resoluutio1600x1200.Y);
        Add(resoluutio1600x1200Teksti);

        PushButton resoluutio1920x1080 = new PushButton(20, 20);
        resoluutio1920x1080.Position = new Vector(100.0, resoluutio1600x1200.Y - 50);
        Add(resoluutio1920x1080);
        resoluutio1920x1080.Clicked += delegate { AsetaIkkunanKoko(1920, 1080); };

        Label resoluutio1920x1080Teksti = new Label("1920 x 1080");
        resoluutio1920x1080Teksti.Position = new Vector(resoluutio1920x1080.X - 100, resoluutio1920x1080.Y);
        Add(resoluutio1920x1080Teksti);

        OnOffButton pelaaja1LaserNappi = new OnOffButton(20, 20, AsetustenKaytto.Asetukset.OnkoPelaajalla1Lasertahtainta);
        pelaaja1LaserNappi.Position = new Vector(0.0, resoluutio1920x1080.Y - 50);
        Add(pelaaja1LaserNappi);
        pelaaja1LaserNappi.Clicked += delegate { AsetustenKaytto.Asetukset.OnkoPelaajalla1Lasertahtainta = !AsetustenKaytto.Asetukset.OnkoPelaajalla1Lasertahtainta; };

        Label pelaaja1LaserTeksti = new Label("Pelaajan 1 lasertähtäin:");
        pelaaja1LaserTeksti.Position = new Vector(pelaaja1LaserNappi.X - 150, pelaaja1LaserNappi.Y);
        Add(pelaaja1LaserTeksti);

        OnOffButton pelaaja2LaserNappi = new OnOffButton(20, 20, AsetustenKaytto.Asetukset.OnkoPelaajalla2Lasertahtainta);
        pelaaja2LaserNappi.Position = new Vector(0.0, pelaaja1LaserNappi.Y - 50);
        Add(pelaaja2LaserNappi);
        pelaaja2LaserNappi.Clicked += delegate { AsetustenKaytto.Asetukset.OnkoPelaajalla2Lasertahtainta = !AsetustenKaytto.Asetukset.OnkoPelaajalla2Lasertahtainta; };

        Label pelaaja2LaserTeksti = new Label("Pelaajan 2 lasertähtäin:");
        pelaaja2LaserTeksti.Position = new Vector(pelaaja2LaserNappi.X - 150, pelaaja2LaserNappi.Y);
        Add(pelaaja2LaserTeksti);

#if DEBUG
        PushButton kayttajaNimiNappi = new PushButton("Muuta");
        kayttajaNimiNappi.Color = Color.Green;
        kayttajaNimiNappi.Position = new Vector(0.0, pelaaja2LaserNappi.Position.Y - 50);
        kayttajaNimiNappi.Clicked += delegate
        {
            InputWindow kayttajanimiIkkuna = new InputWindow("");
            kayttajanimiIkkuna.Color = Color.Transparent;
            kayttajanimiIkkuna.InputBox.BorderColor = Color.Green;
            kayttajanimiIkkuna.InputBox.Text = AsetustenKaytto.Asetukset.KayttajaNimi;
            kayttajanimiIkkuna.InputBox.Cursor.Color = Color.Green;
            kayttajanimiIkkuna.Position = new Vector(0.0, kayttajaNimiNappi.Y - 50);
            kayttajanimiIkkuna.OKButton.Color = Color.Green;
            kayttajanimiIkkuna.OKButton.Clicked += delegate
            {
                AsetustenKaytto.Asetukset.KayttajaNimi = kayttajanimiIkkuna.InputBox.Text;
                AsetustenKaytto.TallennaAsetukset();
                LuoAsetusIkkuna();
            };
            Add(kayttajanimiIkkuna);
        };
        Add(kayttajaNimiNappi);

        Label kayttajanimiTeksti = new Label("Käyttäjänimi: ");
        kayttajanimiTeksti.Position = new Vector(kayttajaNimiNappi.X - 150, kayttajaNimiNappi.Y);
        Add(kayttajanimiTeksti);
#endif
    }

    /// <summary>
    /// Pelimuotovalikon luonti.
    /// </summary>
    void LuoPelimuodonValintaValikko()
    {
        AnimatedMultiSelectWindow pelimuotoValikko = new AnimatedMultiSelectWindow("Valitse pelimuoto", valikonPainikeKuva, "Deathmatch", "Infinite", "Sandbox", "Takaisin");
        pelimuotoValikko.ItemSelected += PainettiinPelimuotoValikonNappia;
        pelimuotoValikko.Color = Color.Transparent;
        pelimuotoValikko.BorderColor = Color.Transparent;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        pelimuotoValikko.DefaultCancel = 2;
        Add(pelimuotoValikko);
    }

    /// <summary>
    /// Pelimuotovalikon nappien painalluksen käsittely.
    /// </summary>
    /// <param name="valinta">Painettu nappi.</param>
    void PainettiinPelimuotoValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                LuoPelinKustomointiIkkuna();
                break;
            case 1:
                LuoInfinitePeliMuodonValintaValikko();
                break;
            case 2:
                Level.Background.Image = null;
                ValittuKenttaTiedosto = "kenttä2"; // turha
                MonestakoVoittaa = new IntMeter(Int32.MaxValue);
                LuoKentta(KenttaTyyppi.Sandbox, 2, false);
                KaytetaankoRajattuaAluetta = false;
                ClearControls();
                AsetaOhjaimet();
                AsetaSandboxOhjaimet();
                SoitaMusiikkia(3); // Sandbox: vanha OrcsCome
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                CurrentGameMode = Gamemode.Sandbox;
                Level.AmbientLight = 0.0;
                KirkastaRuutua(0.02, 0.8);
                break;
            case 3:
                LuoAlkuValikko();
                break;
        }
    }

    void LuoInfinitePeliMuodonValintaValikko()
    {
        AnimatedMultiSelectWindow infinitePelimuotoValikko = new AnimatedMultiSelectWindow("Valitse pelimuoto", valikonPainikeKuva, "Infinite", "Survival", "Takaisin"); //("Valitse pelimuoto", valikonPainikeKuva, "Infinite", "Escape", "Takaisin")
        infinitePelimuotoValikko.ItemSelected += PainettiinInfinitePelimuotoValikonNappia;
        infinitePelimuotoValikko.Color = Color.Transparent;
        infinitePelimuotoValikko.BorderColor = Color.Transparent;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        infinitePelimuotoValikko.DefaultCancel = 2;
        Add(infinitePelimuotoValikko);
    }

    void PainettiinInfinitePelimuotoValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                LuoInfinitevalikko();
                break;
            case 1:
                LuoSurvivalValikko();
                break;
            case 2:
                LuoPelimuodonValintaValikko();
                break;
        }
    }

    void LuoSurvivalValikko()
    {
        List<Widget> ikkunanKomponentit = new List<Widget>();
        IntMeter monestakoHaviaa = new IntMeter(1, 0, 100);

        MultiSelectWindow valikko = new MultiSelectWindow("Survivalin kustomointi", "Yksinpeli", "Moninpeli", "Takaisin");
        valikko.ItemSelected += delegate(int valinta) { PainettiinSurvivalinKustomointiIkkunanNappia(valinta, monestakoHaviaa.Value, ikkunanKomponentit); };
        valikko.Color = Color.Transparent;
        valikko.SelectionColor = Color.Green;
        valikko.BorderColor = Color.Transparent;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        valikko.DefaultCancel = 2;
        valikko.Position = new Vector(0.0, Screen.Top - valikko.Height);
        Add(valikko);

        Slider kuolematJoillaHaviaa = new Slider(300.0, 20.0);
        kuolematJoillaHaviaa.Position = new Vector(valikko.X, valikko.Y - 200);
        Add(kuolematJoillaHaviaa);
        kuolematJoillaHaviaa.BorderColor = Color.Transparent;
        kuolematJoillaHaviaa.Color = Color.Transparent;
        kuolematJoillaHaviaa.Knob.Color = Color.Green;
        kuolematJoillaHaviaa.Track.Color = Color.DarkGreen;
        kuolematJoillaHaviaa.BindTo(monestakoHaviaa);
        ikkunanKomponentit.Add(kuolematJoillaHaviaa);

        Label kuolematJoillaHaviaaOtsikko = new Label("Respauksia käytössä:");
        kuolematJoillaHaviaaOtsikko.Position = new Vector(kuolematJoillaHaviaa.X - kuolematJoillaHaviaa.Width, kuolematJoillaHaviaa.Y);
        Add(kuolematJoillaHaviaaOtsikko);
        ikkunanKomponentit.Add(kuolematJoillaHaviaaOtsikko);

        Label kuolematJoillaHaviaaNaytto = new Label();
        kuolematJoillaHaviaaNaytto.BindTo(monestakoHaviaa);
        kuolematJoillaHaviaaNaytto.Position = new Vector(kuolematJoillaHaviaa.X + kuolematJoillaHaviaa.Width / 1.5, kuolematJoillaHaviaa.Y);
        Add(kuolematJoillaHaviaaNaytto);
        ikkunanKomponentit.Add(kuolematJoillaHaviaaNaytto);
    }

    void PainettiinSurvivalinKustomointiIkkunanNappia(int valinta, int monestakoPoikki, List<Widget> poistettavatJutut)
    {
        foreach (Widget poistettava in poistettavatJutut)
        {
            poistettava.Destroy();
        }

        switch (valinta)
        {
            case 0:
                Level.Background.Image = null;
                ClearControls();
                SoitaMusiikkia(4); // OrcsCome Special
                CurrentGameMode = Gamemode.SurvivalSingle;
                AloitaInfiniteGame(1, KenttaTyyppi.Survival, monestakoPoikki);
                AsetaOhjaimet();
                AsetaSurvivalOhjaimet();
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                KaytetaankoRajattuaAluetta = true;
                break;

            case 1:
                Level.Background.Image = null;
                ClearControls();
                SoitaMusiikkia(4); // OrcsCome Special
                CurrentGameMode = Gamemode.SurvivalMulti;
                AloitaInfiniteGame(2, KenttaTyyppi.Survival, monestakoPoikki);
                AsetaOhjaimet();
                AsetaSurvivalOhjaimet();
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                KaytetaankoRajattuaAluetta = true;
                break;
            case 2:
                LuoPelimuodonValintaValikko();
                break;
        }
    }

    void LuoEscapeValikko()
    {
        List<Widget> ikkunanKomponentit = new List<Widget>();

        MultiSelectWindow valikko = new MultiSelectWindow("Escapen kustomointi", "Yksinpeli", "Moninpeli", "Takaisin");
        valikko.ItemSelected += delegate(int valinta) { PainettiinEscapenKustomointiIkkunanNappia(valinta, ikkunanKomponentit); };
        valikko.Color = Color.Transparent;
        valikko.SelectionColor = Color.Green;
        valikko.BorderColor = Color.Transparent;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        valikko.DefaultCancel = 2;
        valikko.Position = new Vector(0.0, Screen.Top - valikko.Height);
        Add(valikko);

        MonestakoVoittaa = new IntMeter(5, 1, 600);

        Slider puolustusAika = new Slider(300.0, 20.0);
        puolustusAika.Position = new Vector(valikko.X, valikko.Y - 200);
        Add(puolustusAika);
        puolustusAika.BorderColor = Color.Transparent;
        puolustusAika.Color = Color.Transparent;
        puolustusAika.Knob.Color = Color.Green;
        puolustusAika.Track.Color = Color.DarkGreen;
        puolustusAika.BindTo(KauankoPuolustetaan);
        ikkunanKomponentit.Add(puolustusAika);

        Label puolustusAikaOtsikko = new Label("Kauanko puolustetaan:");
        puolustusAikaOtsikko.Position = new Vector(puolustusAika.X - puolustusAika.Width, puolustusAika.Y);
        Add(puolustusAikaOtsikko);
        ikkunanKomponentit.Add(puolustusAikaOtsikko);

        Label puolustusAikaNaytto = new Label();
        puolustusAikaNaytto.BindTo(MonestakoVoittaa);
        puolustusAikaNaytto.Position = new Vector(puolustusAika.X + puolustusAika.Width / 1.5, puolustusAika.Y);
        Add(puolustusAikaNaytto);
        ikkunanKomponentit.Add(puolustusAikaNaytto);
    }

    void PainettiinEscapenKustomointiIkkunanNappia(int valinta, List<Widget> ikkunanKomponentit)
    {
        foreach (Widget k in ikkunanKomponentit)
        {
            k.Destroy();
        }

        switch (valinta)
        {
            case 0:
                Level.Background.Image = null;
                ValittuKenttaTiedosto = "copter_rescue_map";
                ClearControls();
                SoitaMusiikkia(4); // OrcsCome Special
                AloitaEscapeGame(1, 1, KauankoPuolustetaan);
                AsetaOhjaimet();
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                break;
            case 1:
                Level.Background.Image = null;
                ValittuKenttaTiedosto = "copter_rescue_map"; // zombimäppi1
                ClearControls();
                SoitaMusiikkia(4); // OrcsCome Special
                AloitaEscapeGame(2, 1, KauankoPuolustetaan);
                AsetaOhjaimet();
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                break;
            case 2:
                LuoInfinitePeliMuodonValintaValikko();
                break;

        }
    }

    /// <summary>
    /// Pelin kustomointi-ikkunan luonti.
    /// </summary>
    void LuoPelinKustomointiIkkuna()
    {
        List<Widget> ikkunanKomponentit = new List<Widget>();
        OnOffButton ulkonakoValitsin = new OnOffButton(25, 25, false);


        MultiSelectWindow valikko = new MultiSelectWindow("Pelin kustomointi", "Klassinen mäppi", "Satunnaisgeneroitu mäppi", "Takaisin");
        valikko.ItemSelected += delegate(int valinta) { PainettiinPelinKustomointiIkkunanNappia(valinta, ikkunanKomponentit, ulkonakoValitsin.IsPressed); };
        valikko.Color = Color.Transparent;
        valikko.SelectionColor = Color.Green;
        valikko.BorderColor = Color.Transparent;
        Level.Background.Image = valikonKuva;
        Level.BackgroundColor = Color.Black;
        valikko.DefaultCancel = 2;
        valikko.Position = new Vector(0.0, Screen.Top - valikko.Height);
        Add(valikko);

        MonestakoVoittaa = new IntMeter(5, 1, 100);

        Slider tapotJoillaVoittaa = new Slider(300.0, 20.0);
        tapotJoillaVoittaa.Position = new Vector(valikko.X, valikko.Y - 200);
        Add(tapotJoillaVoittaa);
        tapotJoillaVoittaa.BorderColor = Color.Transparent;
        tapotJoillaVoittaa.Color = Color.Transparent;
        tapotJoillaVoittaa.Knob.Color = Color.Green;
        tapotJoillaVoittaa.Track.Color = Color.DarkGreen;
        tapotJoillaVoittaa.BindTo(MonestakoVoittaa);
        ikkunanKomponentit.Add(tapotJoillaVoittaa);

        Label tapotJoillaVoittaaOtsikko = new Label("Tappoja voittoon:");
        tapotJoillaVoittaaOtsikko.Position = new Vector(tapotJoillaVoittaa.X - tapotJoillaVoittaa.Width, tapotJoillaVoittaa.Y);
        Add(tapotJoillaVoittaaOtsikko);
        ikkunanKomponentit.Add(tapotJoillaVoittaaOtsikko);

        Label tapotJoillaVoittaaNaytto = new Label();
        tapotJoillaVoittaaNaytto.BindTo(MonestakoVoittaa);
        tapotJoillaVoittaaNaytto.Position = new Vector(tapotJoillaVoittaa.X + tapotJoillaVoittaa.Width / 1.5, tapotJoillaVoittaa.Y);
        Add(tapotJoillaVoittaaNaytto);
        ikkunanKomponentit.Add(tapotJoillaVoittaaNaytto);

#if DEBUG
        ulkonakoValitsin.ColorWhileOn = Color.Green;
        ulkonakoValitsin.ColorWhileOff = Color.Red;
        ulkonakoValitsin.Position = new Vector(tapotJoillaVoittaa.X, tapotJoillaVoittaa.Y - 50);
        Add(ulkonakoValitsin);
        ikkunanKomponentit.Add(ulkonakoValitsin);

        Label ulkoNakoValitsinOtsikko = new Label("Vaihtoehtoinen grafiikkateema:");
        ulkoNakoValitsinOtsikko.Y = ulkonakoValitsin.Y;
        ulkoNakoValitsinOtsikko.Left = tapotJoillaVoittaaOtsikko.Left;
        Add(ulkoNakoValitsinOtsikko);
        ikkunanKomponentit.Add(ulkoNakoValitsinOtsikko);
#endif
    }

    /// <summary>
    /// Handleri pelin kustomointinäytön napeille.
    /// </summary>
    /// <param name="valinta">Valittu nappi.</param>
    /// <param name="poistettavatIkkunanKomponentit">Mitkä kaikki poistetaan, kun nappia painetaan.</param>
    void PainettiinPelinKustomointiIkkunanNappia(int valinta, List<Widget> poistettavatIkkunanKomponentit, bool kaytetaankoVaihtoEhtoistaUlkonakoa)
    {
        foreach (Widget ikkunanKomponentti in poistettavatIkkunanKomponentit)
        {
            ikkunanKomponentti.Destroy();
        }

        switch (valinta)
        {
            case 0:
                Level.Background.Image = null;
                ValittuKenttaTiedosto = "kenttä2"; // kenttä2
                LuoKentta(KenttaTyyppi.Ruutukartta, 2, false);
                ClearControls();
                AsetaOhjaimet();
                SoitaMusiikkia(1); // DarkLurk deathmatchiin
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                if (kaytetaankoVaihtoEhtoistaUlkonakoa)
                {
                    Level.Background.Image = LoadImage("Pohja_testi");
                    Camera.StayInLevel = true;
                    Level.AmbientLight = 0.0;
                }
                CurrentGameMode = Gamemode.Deathmatch;
                Level.AmbientLight = 0.0;
                KirkastaRuutua(0.02, 0.8);
                break;
            case 1:
                Level.Background.Image = null;
                ValittuKenttaTiedosto = "kenttä2";
                LuoKentta(KenttaTyyppi.GameOfLifeGeneroitu, 2, false);
                KaytetaankoRajattuaAluetta = true;
                ClearControls();
                AsetaOhjaimet();
                SoitaMusiikkia(1); // DarkLurk deathmatchiin
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                CurrentGameMode = Gamemode.DeathmatchRandomMap;
                Level.AmbientLight = 0.0;
                KirkastaRuutua(0.02, 0.8);
                break;
            case 2:
                LuoPelimuodonValintaValikko();
                break;
        }
    }

    /// <summary>
    /// Infinitevalikon luonti.
    /// </summary>
    void LuoInfinitevalikko()
    {
        List<Widget> ikkunanKomponentit = new List<Widget>();
        IntMeter monestakoHaviaa = new IntMeter(1, 0, 100);
        RadioButtonGroup kenttaMenu = new RadioButtonGroup();


        Level.Background.Image = null;
        MultiSelectWindow infinitevalikko = new MultiSelectWindow("Valitse pelaajien määrä", "Yksinpeli", "Moninpeli", "Takaisin");
        infinitevalikko.ItemSelected += delegate(int valinta) { PainettiinInfiniteMenunNappia(valinta, monestakoHaviaa.Value, ikkunanKomponentit, kenttaMenu.Selected); };
        infinitevalikko.Color = Color.Transparent;
        infinitevalikko.SelectionColor = Color.Green;
        infinitevalikko.BorderColor = Color.Transparent;
        infinitevalikko.Position = new Vector(0.0, Screen.Top - infinitevalikko.Height);
        Level.Background.Image = valikonKuva;
        infinitevalikko.DefaultCancel = 2;
        Add(infinitevalikko);

        Slider kuolematJoillaHaviaa = new Slider(300.0, 20.0);
        kuolematJoillaHaviaa.Position = new Vector(infinitevalikko.X, infinitevalikko.Y - 200);
        Add(kuolematJoillaHaviaa);
        kuolematJoillaHaviaa.BorderColor = Color.Transparent;
        kuolematJoillaHaviaa.Color = Color.Transparent;
        kuolematJoillaHaviaa.Knob.Color = Color.Green;
        kuolematJoillaHaviaa.Track.Color = Color.DarkGreen;
        kuolematJoillaHaviaa.BindTo(monestakoHaviaa);
        ikkunanKomponentit.Add(kuolematJoillaHaviaa);

        Label kuolematJoillaHaviaaOtsikko = new Label("Respauksia käytössä:");
        kuolematJoillaHaviaaOtsikko.Position = new Vector(kuolematJoillaHaviaa.X - kuolematJoillaHaviaa.Width, kuolematJoillaHaviaa.Y);
        Add(kuolematJoillaHaviaaOtsikko);
        ikkunanKomponentit.Add(kuolematJoillaHaviaaOtsikko);

        Label kuolematJoillaHaviaaNaytto = new Label();
        kuolematJoillaHaviaaNaytto.BindTo(monestakoHaviaa);
        kuolematJoillaHaviaaNaytto.Position = new Vector(kuolematJoillaHaviaa.X + kuolematJoillaHaviaa.Width / 1.5, kuolematJoillaHaviaa.Y);
        Add(kuolematJoillaHaviaaNaytto);
        ikkunanKomponentit.Add(kuolematJoillaHaviaaNaytto);


        Label kentanvalintaOtsikko = new Label("Kenttä:");
        kentanvalintaOtsikko.Left = kuolematJoillaHaviaaOtsikko.Left;
        kentanvalintaOtsikko.Y = kuolematJoillaHaviaaOtsikko.Y - 100;
        Add(kentanvalintaOtsikko);
        ikkunanKomponentit.Add(kentanvalintaOtsikko);

        Label klassinenOtsikko = new Label("Klassinen:");
        klassinenOtsikko.Position = kentanvalintaOtsikko.Position + new Vector(300.0, 0.0);
        Add(klassinenOtsikko);
        ikkunanKomponentit.Add(klassinenOtsikko);

        Label randomOtsikko = new Label("Satunnainen:");
        randomOtsikko.Position = klassinenOtsikko.Position + new Vector(0.0, -50.0);
        Add(randomOtsikko);
        ikkunanKomponentit.Add(randomOtsikko);

        OnOffButton classic = new OnOffButton(20, 20, true);
        classic.Position = klassinenOtsikko.Position + new Vector(100.0, 0.0);
        Add(classic);
        kenttaMenu.AddButton(classic);

        OnOffButton random = new OnOffButton(20, 20, false);
        random.Position = randomOtsikko.Position + new Vector(100.0, 0.0);
        Add(random);
        kenttaMenu.AddButton(random);

        ikkunanKomponentit.AddRange(kenttaMenu.buttons);
    }

    /// <summary>
    /// Infinitevalikon napin painalluksen käsittely.
    /// </summary>
    /// <param name="valinta">Mitä painettiin.</param>
    /// <param name="monestakoPoikki">Monestako peli menee poikki.</param>
    /// <param name="poistettavatJutut">Pitä poistetaan, kun nappia painetaan.</param>
    void PainettiinInfiniteMenunNappia(int valinta, int monestakoPoikki, List<Widget> poistettavatJutut, int valittuKentta)
    {
        foreach (Widget poistettava in poistettavatJutut)
        {
            poistettava.Destroy();
        }

        switch (valinta)
        {
            case 0: // Infiniten yksinpeli
                Level.Background.Image = null;
                ValittuKenttaTiedosto = "zombimäppi1"; // zombimäppi1
                ClearControls();
                SoitaMusiikkia(4); // OrcsCome Special
                CurrentGameMode = Gamemode.InfiniteSingle;
                switch (valittuKentta)
                {
                    case 0:
                        AloitaInfiniteGame(1, KenttaTyyppi.Ruutukartta, monestakoPoikki);
                        break;
                    case 1:
                        AloitaInfiniteGame(1, KenttaTyyppi.SatunnainenRuutukartta, monestakoPoikki);
                        break;
                }
                AsetaOhjaimet();
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                break;

            case 1: // Infiniten kaksinpeli
                Level.Background.Image = null;
                ValittuKenttaTiedosto = "zombimäppi1";
                ClearControls();
                SoitaMusiikkia(4); // OrcsCome Special
                CurrentGameMode = Gamemode.InfiniteMulti;
                switch (valittuKentta)
                {
                    case 0:
                        AloitaInfiniteGame(2, KenttaTyyppi.Ruutukartta, monestakoPoikki);
                        break;
                    case 1:
                        AloitaInfiniteGame(2, KenttaTyyppi.SatunnainenRuutukartta, monestakoPoikki);
                        break;
                }
                AsetaOhjaimet();
                Partikkelit.PoistaValikkoTaustaPartikkelit();
                break;
            case 2:
                LuoPelimuodonValintaValikko();
                break;
        }
    }

    /// <summary>
    /// Luodaan pausevalikko.
    /// </summary>
    void LuoPauseValikko()
    {
        IsPaused = true;
        MultiSelectWindow valikko = new MultiSelectWindow("TAUKO", "Takaisin peliin", "Päävalikkoon", "Poistu pelistä");
        valikko.ItemSelected += PainettiinPauseValikonNappia;
        valikko.Color = Color.Transparent;
        valikko.BorderColor = Color.Transparent;
        valikko.SelectionColor = Color.Green;
        valikko.DefaultCancel = 0;
        Add(valikko);
    }

    /// <summary>
    /// Pausevalikon nappien painalluksen käsittely.
    /// </summary>
    /// <param name="valinta">Painettu nappi.</param>
    void PainettiinPauseValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                IsPaused = false;
                break;
            case 1:
                IsPaused = false;
                PaaValikkoon();
                break;
            case 2:
                Exit();
                break;
        }
    }

    #endregion

    #region infinite

    /// <summary>
    /// Aloitetaan uusi Infinite-peli.
    /// </summary>
    /// <param name="pelaajienMaara">Montako pelaajaa peliin tulee.</param>
    /// <param name="kenttaTyyppi">Millaista kenttää käytetään.</param>
    /// <param name="monestakoRespauksestaPoikki">Monestako respauksesta peli menee poikki.</param>
    void AloitaInfiniteGame(int pelaajienMaara, KenttaTyyppi kenttaTyyppi, int monestakoRespauksestaPoikki)
    {
        Infinite infinite = new Infinite(monestakoRespauksestaPoikki);
        if (kenttaTyyppi == KenttaTyyppi.SatunnainenRuutukartta)
        {
            LuoKentta(KenttaTyyppi.Ruutukartta, pelaajienMaara, true, LuoSatunnainenRuutuKentta(Vakiot.SATUNNAISRUUTUKENTAN_TIEDOSTONIMI, Vakiot.SATUNNAISRUUTUKENTAN_ALOITUSINDEKSI, Vakiot.SATUNNAISRUUTUKENTAN_LOPETUSINDEKSI));
            KaytetaankoRajattuaAluetta = true;
        }
        if (kenttaTyyppi == KenttaTyyppi.Ruutukartta)
        {
            LuoKentta(kenttaTyyppi, pelaajienMaara, true);
        }

        if (kenttaTyyppi == KenttaTyyppi.Survival)
        {
            LuoKentta(kenttaTyyppi, pelaajienMaara, true);
        }
        
        LuoVihollistenMallineet(infinite);
        infinite.PeliPaattyi += InfiniteHavio;
        infinite.VihollistenSpawnausAjastin.Timeout += delegate { SpawnaaInfinitenVihollisia(infinite); };
        foreach (Pelaaja p in pelaajat)
        {
            p.Kuoli += infinite.PelaajaKuoli;
        }

        if (kenttaTyyppi == KenttaTyyppi.Ruutukartta || kenttaTyyppi == KenttaTyyppi.SatunnainenRuutukartta)
        {
            infinite.KaytetaankoFixedSpawneja = true;
        }
        Level.AmbientLight = 0.0;
        if (CurrentGameMode == Gamemode.InfiniteMulti || CurrentGameMode == Gamemode.InfiniteSingle)
        {
            if (pelaajienMaara == 1)
                KirkastaRuutua(0.02, 0.7); // 0.7
            else KirkastaRuutua(0.02, 0.4);
        }
        if (CurrentGameMode == Gamemode.SurvivalMulti || CurrentGameMode == Gamemode.SurvivalSingle)
        {
            KirkastaRuutua(0.04, 1.0);
            infinite.KamanSpawnausAjastin.Timeout += SpawnaaSurvivalKamaa;
            infinite.KamanSpawnausAjastin.Start();
        }
    }

    /// <summary>
    /// Luodaan Infinitessä käytettävien vihollisten mallineet
    /// ja annetaan ne Infinite-pelimuodon spawnauslistaan.
    /// </summary>
    /// <param name="peli">Peli, johon viholliset lisätään.</param>
    void LuoVihollistenMallineet(Infinite peli)
    {
        // Tavallinen zombi
        Vihollinen zombi = new Vihollinen(normaaliZombiKuva.Width / 1.2, normaaliZombiKuva.Height / 1.2, normaaliZombiKuva, 20, 12, 100, pelaajat, false, false, this, peli); // nopeus oli 100
        //zombi.Animation = new Animation(normaaliZombiAnimaatio);
        //zombi.Animation.FPS = 2;
        peli.LisaaVihollinenPeliin(zombi);

        // Zombi, jolla on ase
        /* Vihollinen ampuvaZombi = new Vihollinen(ampuvaZombiKuva.Width / 1.2, ampuvaZombiKuva.Height / 1.2, ampuvaZombiKuva, 20, 4, 100, pelaajat, true, false, this, peli);
        ampuvaZombi.AmpumisTarkkuus = 6.0;
        ampuvaZombi.EtaisyysJoltaAmpuu = 300.0;
        ampuvaZombi.ValittuAse = Aseet.LuoRynkky();
        ampuvaZombi.ValittuAse.AanenVoimakkuus = 0.5;
        ampuvaZombi.TodennakoisyysSpawnata = 0.3;
        peli.LisaaVihollinenPeliin(ampuvaZombi);*/

        // Itsetuhoinen parasiitti
        /*Vihollinen parasiitti = new Vihollinen(10, 10, RandomGen.SelectOne<Image>(parasiittienKuvat), 1, 2, 200, pelaajat, false, true, this);
        parasiitti.KuolemaEfektinPartikkeliKuva = RandomGen.SelectOne<Image>(parasiittienPartikkeliKuvat);
        peli.LisaaVihollinenPeliin(parasiitti); */
    }

    /// <summary>
    /// Spawnataan viholliset Infiniten spawnauslistasta.
    /// </summary>
    /// <param name="peli">Peli, jonka spawnauslistasta viholliset spawnataan.</param>
    void SpawnaaInfinitenVihollisia(Infinite peli)
    {
        foreach (Vihollinen vihu in peli.TallaSpawnauskerrallaSpawnattavatViholliset)
        {
            vihu.Destroyed += delegate { peli.VihollisiaTapettu.Value++; };
            Add(vihu);
            Vector paikka;
            int spawnausYrityksia = 0;
            if (peli.KaytetaankoFixedSpawneja && peli.VihollistenFixedSpawnit.Count != 0)
            {
                do
                {
                    paikka = RandomGen.SelectOne<Vector>(peli.VihollistenFixedSpawnit);
                    spawnausYrityksia++;
                    if (spawnausYrityksia > peli.VihollistenFixedSpawnit.Count * 6) break; // ettei tule loputonta silmukkaa, jos ei ole spawnauspaikkoja
                    if (OnkoLiianLahellaPelaajia(paikka)) continue; // ettei spawnaa pelaajan päälle

                    if (!OnkoNaytonAlueella(paikka))
                    {
                        if (RandomGen.NextDouble(0.0, 100.0) <= Vakiot.TODENNAKOISYYS_SPAWNATA_RUUDUN_ULKOPUOLELLE) // näytön ulkopuolella kelpaa tietyllä todennäköisyydellä
                        {
                            break;
                        }
                    }
                    else break; // näytöllä kelpaa aina
                } while (true);
                vihu.Position = paikka;
                spawnausYrityksia = 0;
            }
            else vihu.Position = ArvoSpawnausPaikkaKentanLaidoilta(Vakiot.PALJONKO_VIHOLLISET_SPAWNAAVAT_KENTAN_RAJOJEN_ULKOPUOLELLE);

            peli.VihollisetKentalla.Add(vihu);
        }
    }

    bool OnkoLiianLahellaPelaajia(Vector paikka)
    {
        for (int i = 0; i < pelaajat.Length; i++)
        {
            if (pelaajat[i] != null && pelaajat[i].IsAddedToGame)
            {
                if (Vector.Distance(pelaajat[i].Position, paikka) < Vakiot.MINIMIETAISYYS_PELAAJIIN_SPAWNATESSA)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Arvotaan satunnainen paikka kentän rajojen ulkopuolelta.
    /// </summary>
    /// <param name="paljonkoLaitojenUlkopuolelle">Paljonko paikka tulee kentän rajojen ulkopuolelle.</param>
    /// <returns>Satunnainen paikka kentän laidoilta.</returns>
    Vector ArvoSpawnausPaikkaKentanLaidoilta(double paljonkoLaitojenUlkopuolelle)
    {
        //Arvotaan satunnainen vektori kentän joka laidalle, ja arvotaan joku niistä
        Vector[] vaihtoehdot = new Vector[4];
        vaihtoehdot[0] = RandomGen.NextVector(Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA - paljonkoLaitojenUlkopuolelle, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA,
            Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA - paljonkoLaitojenUlkopuolelle, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA); // vasen reuna

        vaihtoehdot[1] = RandomGen.NextVector(Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA + paljonkoLaitojenUlkopuolelle,
            Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA + paljonkoLaitojenUlkopuolelle); // yläreuna

        vaihtoehdot[2] = RandomGen.NextVector(Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA + paljonkoLaitojenUlkopuolelle, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA,
            Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA + paljonkoLaitojenUlkopuolelle, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA); // oikea reuna

        vaihtoehdot[3] = RandomGen.NextVector(Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA - paljonkoLaitojenUlkopuolelle,
            Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA - paljonkoLaitojenUlkopuolelle); // alareuna
        return RandomGen.SelectOne<Vector>(vaihtoehdot); // palautetaan yksi neljästä
    }

    #endregion

    #region survival

    /// <summary>
    /// Asetetaan seinä. Ottaa huomioon pelaajalla olevien seinien määrän.
    /// </summary>
    /// <param name="pelaaja">Seinää asettava pelaaja.</param>
    void AsetaSeina(Pelaaja pelaaja)
    {
        if (pelaaja.SeinienMaara <= 0) return;

        if (pelaaja.Numero == 1)
            LuoTuhoutuvaKentanOsa(SijoitaKentanosaRuudukkoon(pelaajat[0].tahtain.Position), 50, 50, KentanOsat.GetCorrespondingNode(pelaajat[0].tahtain.Position), kivenKuva, "kivi", 20, 1.0, 1.0);
        if (pelaaja.Numero == 2)
            ControllerOne.Listen(Button.B, ButtonState.Pressed, delegate { LuoTuhoutuvaKentanOsa(SijoitaKentanosaRuudukkoon(pelaajat[1].Position + Vector.FromLengthAndAngle(pelaajat[1].Width, pelaajat[1].Angle)), 50, 50, KentanOsat.GetCorrespondingNode(pelaajat[1].Position + Vector.FromLengthAndAngle(pelaajat[1].Width, pelaajat[1].Angle)), kivenKuva, "kivi", 20, 1.0, 1.0); }, null);
        pelaaja.SeinienMaara--;
    }

    /// <summary>
    /// Spawnataan jokin esine kentän rajojen sisäpuolelle.
    /// </summary>
    void SpawnaaSurvivalKamaa()
    {
        /* Pistooli: 5%
         * Rynkky: 7.5%
         * Haulikko: 4%
         * Magnum: 3%
         * SMG: 4%
         * Minigun: 3%
         * Snipa: 2%
         * Vintorez: 2%
         * Automaattihaulikko: 2%
         * Sinko: 2%
         * Flare gun: 3%
         * Kranaatti: 7.5%
         * Seinä: 55%
         */
        int mikaLuodaan = RandomGen.NextIntWithProbabilities(0.05, 0.075, 0.04, 0.03, 0.04, 0.03, 0.02, 0.02, 0.02, 0.02, 0.03, 0.075, 0.55);
        Vector paikka = RandomGen.NextVector(Vakiot.PELAAJAN_PIENIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_PIENIN_SALLITTU_Y_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_X_KENTALLA, Vakiot.PELAAJAN_SUURIN_SALLITTU_Y_KENTALLA);

        switch (mikaLuodaan)
        {
            case 0:
                Kerattava pistooli = LuoKerattava(paikka, Aseet.LuoPistooli(), pistoolinMaaKuva, 0.5);
                pistooli.Kerattiin += JotainKerattiin;
                Add(pistooli);
                break;

            case 1:
                Kerattava rynkky = LuoKerattava(paikka, Aseet.LuoRynkky(), rynkynMaaKuva);
                rynkky.Kerattiin += JotainKerattiin;
                Add(rynkky);
                break;

            case 2:
                Kerattava haulikko = LuoKerattava(paikka, Aseet.LuoHaulikko(), pumpparinMaaKuva);
                haulikko.Kerattiin += JotainKerattiin;
                Add(haulikko);
                break;

            case 3:
                Kerattava magnum = LuoKerattava(paikka, Aseet.LuoMagnum(), magnuminMaaKuva);
                magnum.Kerattiin += JotainKerattiin;
                Add(magnum);
                break;

            case 4:
                Kerattava smg = LuoKerattava(paikka, Aseet.LuoSMG(), ppshMaaKuva);
                smg.Kerattiin += JotainKerattiin;
                Add(smg);
                break;

            case 5:
                Kerattava minigun = LuoKerattava(paikka, Aseet.LuoMinigun(), sawinMaaKuva);
                minigun.Kerattiin += JotainKerattiin;
                Add(minigun);
                break;

            case 6:
                Kerattava snipa = LuoKerattava(paikka, Aseet.LuoSnipa(), snipanMaaKuva);
                snipa.Kerattiin += JotainKerattiin;
                Add(snipa);
                break;

            case 7:
                Kerattava vintorez = LuoKerattava(paikka, Aseet.LuoVintorez(), vintorezMaaKuva, 0.4);
                vintorez.Kerattiin += JotainKerattiin;
                Add(vintorez);
                break;

            case 8:
                Kerattava sarjahaulikko = LuoKerattava(paikka, Aseet.LuoAutomaattiHaulikko(), aa12MaaKuva, 0.4);
                sarjahaulikko.Kerattiin += JotainKerattiin;
                Add(sarjahaulikko);
                break;

            case 9:
                Kerattava sinko = LuoKerattava(paikka, Aseet.LuoSinko(), singonMaaKuva);
                sinko.Kerattiin += JotainKerattiin;
                Add(sinko);
                break;

            case 10:
                Kerattava flaregun = LuoKerattava(paikka, Aseet.LuoFlareGun(), pistoolinMaaKuva);
                flaregun.Kerattiin += JotainKerattiin;
                Add(flaregun);
                break;

            case 11:
                Kerattava kranaatti = LuoKerattava(paikka, "kranaatti", kranaatinMaaKuva, 0.8);
                kranaatti.Kerattiin += JotainKerattiin;
                Add(kranaatti);
                break;

            case 12:
                Kerattava kivi = LuoKerattava(paikka, "kivi", kivenKuva, 4.0);
                kivi.Kerattiin += JotainKerattiin;
                Add(kivi);
                break;

        }
    }

    #endregion

    #region Escape

    void AloitaEscapeGame(int pelaajienMaara, int respaustenMaara, double puolustettavaAika)
    {
        List<Pelaaja> pelaajatList = new List<Pelaaja>(pelaajat);

        Escape escapeGame = new Escape(respaustenMaara, puolustettavaAika, pelaajatList);
        LuoKentta(0, pelaajienMaara, true);
        LuoVihollistenMallineet(escapeGame);
        escapeGame.PeliPaattyi += InfiniteHavio;
        escapeGame.VihollistenSpawnausAjastin.Timeout += delegate { SpawnaaInfinitenVihollisia(escapeGame); };
        foreach (Pelaaja p in pelaajat)
        {
            p.Kuoli += escapeGame.PelaajaKuoli;
        }

        escapeGame.KaytetaankoFixedSpawneja = true;
        Level.AmbientLight = 0.4;
        if (pelaajienMaara == 1) Level.AmbientLight = 0.7; // koska taskulamppu
    }

    #endregion

    #region pelaaja

    /// <summary>
    /// Luodaan uusi pelaaja.
    /// </summary>
    /// <param name="paikka">Paikka, johon pelaaja luodaan ja johon se respawnaa.</param>
    /// <param name="leveys">Pelaajan leveys.</param>
    /// <param name="korkeus">Pelaajan korkeus.</param>
    /// <param name="numero">Pelaajan numero.</param>
    /// <param name="kuva">Pelaajan kuva.</param>
    /// <param name="kamanVari">Pelaajaan viittaavien objektien väri.</param>
    /// <param name="kaytetaankoLaseria">Onko pelaajalla lasertähtäin vai tavallinen piste.</param>
    /// <param name="hudinPaikka">Pelaajan HUD:in paikka.</param>
    /// <returns>Pelaaja.</returns>
    public Pelaaja LuoPelaaja(Vector paikka, double leveys, double korkeus, int numero, Image kuva, Color kamanVari, bool kaytetaankoLaseria, Vector hudinPaikka, bool kaytetaankoTaskulamppua, bool kaytetaankoPalloTahtainta)
    {
        Pelaaja pelaaja = new Pelaaja(leveys, korkeus, kaytetaankoLaseria, kaytetaankoTaskulamppua); // true: käytetäänkö taskulamppua
        pelaaja.CanRotate = false;
        pelaaja.Tag = "pelaaja";
        pelaaja.LisaaAse(Aseet.LuoPistooli());
        pelaaja.LisaaAse(Aseet.LuoNyrkki());
        pelaaja.Position = paikka;
        pelaaja.SpawnausPaikat.Add(paikka);
        pelaaja.Numero = numero;
        pelaaja.Image = kuva;
        pelaaja.LinearDamping = 0.80;
        pelaaja.objektienVari = kamanVari;
        pelaaja.elamaPalkki.BarColor = pelaaja.objektienVari;
        pelaaja.kaytetaankoPalloTahtainta = kaytetaankoPalloTahtainta;
        AddCollisionHandler<Pelaaja, Vihollinen>(pelaaja, delegate(Pelaaja p, Vihollinen v) { Damagea(pelaaja, v.TuhovoimaElaviaKohtaan); });
        PaivitaPelaajanKuvaJaHUD(pelaaja);

        if (pelaaja.kaytetaankoPalloTahtainta)
            pelaaja.tahtain = LuoTähtäin(pelaaja.objektienVari);

        return pelaaja;
    }

    /// <summary>
    /// Liikutetaan pelaajaa määrättyyn suuntaan näppäimistöllä.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jota liikutetaan.</param>
    /// <param name="suunta">Suunta, johon pelaajaa liikutetaan.</param>
    void LiikutaPelaajaa(Pelaaja pelaaja)
    {
        Vector w = new Vector(0.0, 1.0);
        Vector s = new Vector(0.0, -1.0);
        Vector a = new Vector(-1.0, 0.0);
        Vector d = new Vector(1.0, 0.0);

        Vector sum = Vector.Zero;
        if (Keyboard.GetKeyState(Key.W) == ButtonState.Down)
            sum += w;
        if (Keyboard.GetKeyState(Key.S) == ButtonState.Down)
            sum += s;
        if (Keyboard.GetKeyState(Key.A) == ButtonState.Down)
            sum += a;
        if (Keyboard.GetKeyState(Key.D) == ButtonState.Down)
            sum += d;

        pelaaja.Move(sum * pelaaja.Nopeus);
        pelaaja.tahtain.Move(sum * pelaaja.Nopeus);
    }

    /// <summary>
    /// Liikutetaan pelaajaa määrättyyn suuntaan Xbox-ohjaimella.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jota liikutetaan.</param>
    /// <param name="tatinTila">Xbox-ohjaimen analogitatin asento.</param>
    void LiikutaPelaajaaPadilla(AnalogState tatinTila, Pelaaja pelaaja)
    {
        Vector tatinAsento = tatinTila.StateVector;
        pelaaja.Move(tatinAsento * pelaaja.Nopeus);
        pelaaja.tahtain.Move(tatinAsento * pelaaja.Nopeus);
    }

    // Tässä kohdassa (LiikutaTahtainta, LiikutaTahtaintaPadilla) ongelmana samat parametrit, mutta eri sisältö.

    /// <summary>
    /// Liikutetaan pelaajan tähtäintä hiirellä.
    /// </summary>
    /// <param name="hiirenTila">Ei käytössä, pakollinen parametri.</param>
    /// <param name="pelaaja">Pelaaja, jonka tähtäintä liikutetaan.</param>
    void LiikutaTahtainta(AnalogState hiirenTila, Pelaaja pelaaja)
    {
        double tahtaimenEtaisyys = Vakiot.TAHTAIMEN_MAX_ETAISYYS - 5 * pelaaja.ValittuAse.AseenHajoama.Magnitude;

        if (tahtaimenEtaisyys < Vakiot.TAHTAIMEN_MIN_ETAISYYS)
            tahtaimenEtaisyys = Vakiot.TAHTAIMEN_MIN_ETAISYYS;

        Vector posRelativeToPlayer = (pelaaja.tahtain.Position + hiirenTila.MouseMovement) - pelaaja.Position;

        if (posRelativeToPlayer.Magnitude > tahtaimenEtaisyys)
        {
            posRelativeToPlayer = posRelativeToPlayer.Normalize();
            posRelativeToPlayer *= tahtaimenEtaisyys;
        }

        pelaaja.tahtain.AbsolutePosition = pelaaja.Position + posRelativeToPlayer;
    }

    /// <summary>
    /// Liikutetaan pelaajan tähtäintä Xbox-ohjaimella.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka tähtäintä liikutetaan.</param>
    /// <param name="tatinTila">Xbox-ohjaimen analogitatin asento.</param>
    void LiikutaTahtaintaPadilla(AnalogState tatinTila, Pelaaja pelaaja)
    {
        if (pelaaja.tahtayksenKohde != null) 
            pelaaja.tahtayksenKohde = null;

        if (tatinTila.StateVector.Magnitude > 0.95)
        {
            pelaaja.Angle = tatinTila.StateVector.Angle;
            return;
        }

        if (Math.Abs(pelaaja.Angle.Degrees - tatinTila.StateVector.Angle.Degrees) < Vakiot.OHJAIMEN_DEAD_ZONE_ASTEINA)
            return;
        if (Math.Abs(pelaaja.Angle.Degrees - tatinTila.StateVector.Angle.Degrees) > 360 - Vakiot.OHJAIMEN_DEAD_ZONE_ASTEINA)
            return;

        if (Math.Abs(pelaaja.Angle.Degrees - tatinTila.StateVector.Angle.Degrees) <= 180)
        {
            if (pelaaja.Angle.Degrees < tatinTila.StateVector.Angle.Degrees)
                pelaaja.Angle = Angle.FromDegrees(pelaaja.Angle.Degrees + (tatinTila.StateVector.Magnitude * Vakiot.PELAAJAN_PYORIMIS_NOPEUS_OHJAIMELLA));
            else
                pelaaja.Angle = Angle.FromDegrees(pelaaja.Angle.Degrees - (tatinTila.StateVector.Magnitude * Vakiot.PELAAJAN_PYORIMIS_NOPEUS_OHJAIMELLA));

        }
        else
        {
            if (pelaaja.Angle.Degrees < tatinTila.StateVector.Angle.Degrees)
                pelaaja.Angle = Angle.FromDegrees(pelaaja.Angle.Degrees - (tatinTila.StateVector.Magnitude * Vakiot.PELAAJAN_PYORIMIS_NOPEUS_OHJAIMELLA));
            else
                pelaaja.Angle = Angle.FromDegrees(pelaaja.Angle.Degrees + (tatinTila.StateVector.Magnitude * Vakiot.PELAAJAN_PYORIMIS_NOPEUS_OHJAIMELLA));
        }
    }

    void FindAimlockTarget(Pelaaja p)
    {
        if (p == null || !p.IsAddedToGame) return;

        Angle playerAngle = p.Angle;
        List<GameObject> kohteet = GetObjects(x => x is Elava && x != p);

        if (kohteet.Count == 0) return;

        double smallestAngle = 180.0;
        int closestIndex = -1;

        for (int i = 0; i < kohteet.Count; i++)
        {
            double currentAngle = GetDegreeAngleBetweenVectors(Vector.FromAngle(playerAngle), (kohteet[i].Position - p.Position));
            if (currentAngle < smallestAngle)
            {
                smallestAngle = currentAngle;
                closestIndex = i;
            }
        }

        if (smallestAngle < Vakiot.AIMLOCK_MAX_ANGLE.Degrees)
            p.tahtayksenKohde = (Elava)kohteet[closestIndex];
    }

    /// <summary>
    /// Laskee vektorien välisen kulman asteina.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    double GetDegreeAngleBetweenVectors(Vector first, Vector second)
    {
        double aRad = Math.Acos((Vector.DotProduct(first, second)) / (first.Magnitude * second.Magnitude));
        return RadiansToDegrees(aRad);
    }

    /// <summary>
    /// Muuntaa kulman radiaaneina asteiksi.
    /// </summary>
    /// <param name="angleInRadians"></param>
    /// <returns></returns>
    double RadiansToDegrees(double angleInRadians)
    {
        return angleInRadians * (180.0 / Math.PI);
    }

    /// <summary>
    /// Luodaan uusi tähtäin.
    /// </summary>
    /// <param name="vari">Tähtäimen väri.</param>
    /// <returns>Tähtäin.</returns>
    PhysicsObject LuoTähtäin(Color vari)
    {
        PhysicsObject tähtäin = new PhysicsObject(10.0, 10.0);
        tähtäin.Shape = Shape.Circle;
        tähtäin.Color = vari;
        tähtäin.IgnoresCollisionResponse = true;
        tähtäin.IgnoresExplosions = true;
        tähtäin.IgnoresPhysicsLogics = true;
        tähtäin.Body.IsCollidable = false;
        tähtäin.LinearDamping = 0.80;
        tähtäin.Tag = "tahtain";
        Add(tähtäin, 2);
        return tähtäin;
    }

    #endregion

    #region aseiden toiminnot

    /// <summary>
    /// Ammutaan pelaajan kulloinkin valittuna olevalla aseella ja
    /// luodaan ammukselle törmäyksenkäsittelijät.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka aseella ammutaan.</param>
    /// <param name="overridesIsReady">Annetaan aseen ampua, vaikka se ei oikeasti voisi. Jos true, ääntä ei toisteta.</param>
    void Ammu(Pelaaja pelaaja, bool overridesIsReady = false)
    {
        // pari tarpeellista tarkistusta
        if (!pelaaja.IsAddedToGame) return;
        if (pelaaja.ValittuAse == null) return;
        if (pelaaja.LadataankoAsetta) return;

        //pelaaja.ValittuAse.IsReady = true;
        PhysicsObject ammus = pelaaja.ValittuAse.Shoot(overridesIsReady);
        if (ammus == null)
        {
            if (pelaaja.ValittuAse.Ammo == 0)
                AloitaLataus(pelaaja);
            return;
        }

        /*if (pelaaja.IsShooting == false)
        {
            StartedShooting(pelaaja);
            pelaaja.IsShooting = true;
        }*/
        // laitetaan ammus satunnaiseen suuntaan aseen hajoaman mukaan
        Vector random = RandomGen.NextVector(pelaaja.ValittuAse.AseenHajoama.X, pelaaja.ValittuAse.AseenHajoama.Y);

        pelaaja.ammusMaaraNaytto.Text = pelaaja.ValittuAse.Ammo.Value.ToString() + " / " + pelaaja.ValittuAse.MaxAmmo.Value.ToString();
        if (pelaaja.ValittuAse.OnkoMeleeAse == true) pelaaja.ammusMaaraNaytto.Text = "";
        if (Array.IndexOf(pelaajat, pelaaja) == 0)
        {
            if (pelaaja.ValittuAse.HyokkaysAnimaatio1 != null)
            {
                pelaajat[0].Animation = pelaaja.ValittuAse.HyokkaysAnimaatio1;
                pelaajat[0].Animation.Start(1);
            }
        }
        else
        {
            if (pelaaja.ValittuAse.HyokkaysAnimaatio2 != null)
            {
                pelaajat[1].Animation = pelaaja.ValittuAse.HyokkaysAnimaatio2;
                pelaajat[1].Animation.Start(1);
            }

        }

        ammus.Hit(random);
        ammus.CollisionIgnoreGroup = 4;
        ammus.Tag = "ammus";
        ammus.LinearDamping = 0.99; // 0.99
        ammus.Size = new Vector(pelaaja.ValittuAse.luodinKuva.Width, pelaaja.ValittuAse.luodinKuva.Height);
        ammus.Color = Color.White;
        ammus.Mass = 50; // 0.01
        ammus.CanRotate = false;
        ammus.IsVisible = pelaaja.ValittuAse.NakyykoAmmus;

        // jos on meleease, ei näytetä ammusta
        if (pelaaja.ValittuAse.OnkoMeleeAse == true) ammus.IsVisible = false;

        // muuten ammutaan hylsy ja laitetaan luodille oikea kuva
        else
        {
            if (pelaaja.ValittuAse.TuleekoHylsya)
                Hylsy(pelaaja, pelaaja.ValittuAse.hylsynKuva);
            ammus.Image = pelaaja.ValittuAse.luodinKuva;
        }

        if (pelaaja.ValittuAse.UsesTracers)
        {
            ammus.IsUpdated = true;
            ammus.NeedsUpdateCall = true;
            Color c;
            if (pelaaja.ValittuAse.OverrideTracerColor != Color.Transparent)
                c = pelaaja.ValittuAse.OverrideTracerColor;
            else c = pelaaja.objektienVari;
            ammus.Updated += delegate(PhysicsObject o) { DrawTracers(o, c, pelaaja.ValittuAse.TracerLength, pelaaja.ValittuAse.TracerBrightness); };
        }

        if (pelaaja.ValittuAse.IsHoming)
        {
            ammus.IsUpdated = true;
            ammus.NeedsUpdateCall = true;
            if (pelaaja == pelaajat[0])
                ammus.Updated += delegate(PhysicsObject o) { HakeuduKohteeseen(o, EtsiLahinKohde(pelaajat[0].tahtain, pelaajat[0]), 750); }; //750
            else
                ammus.Updated += delegate(PhysicsObject o) { HakeuduKohteeseen(o, pelaajat[0], 750); };
        }

        if (pelaaja.ValittuAse.LapaiseekoMateriaaleja)
        {
            ammus.IgnoresCollisionResponse = true;
            ammus.Collided += delegate
            {
                if (pelaaja.ValittuAse.AseenLapaisy > 0)
                    Timer.SingleShot(pelaaja.ValittuAse.AseenLapaisy, delegate { ammus.IgnoresCollisionResponse = false; });
            };
        }

        if (pelaaja.ValittuAse.Tag.ToString() == "flaregun")
        {
            Flame f = Partikkelit.CreateFlareGunParticles(ammus.Position, 1);
            ammus.NeedsUpdateCall = true;
            ammus.Updated += delegate
            {
                f.Position = ammus.Position;
            };
            ammus.Destroyed += delegate
            {
                f.FadeOut(2);
                Timer.SingleShot(2.0, delegate { f.Destroy();});
            };
        }

        AddCollisionHandler<PhysicsObject, Pelaaja>(
            ammus, delegate(PhysicsObject a, Pelaaja kohdepelaaja)
            {
                Damagea(kohdepelaaja, pelaaja.ValittuAse.TuhovoimaElaviaVastaan);
                if (pelaaja.ValittuAse.TuleekoOsumastaEfektiä)
                    Partikkelit.AddParticleEffect(Partikkelit.VeriPartikkelit, new Vector(a.X, a.Y), 40);
                if (pelaaja.ValittuAse.KuulukoOsumastaAanta)
                    osuma1.Play();
                if (pelaaja.ValittuAse.IsIncendiary)
                    if (RandomGen.NextInt(0, 100) < pelaaja.ValittuAse.IgnitionChance)
                        kohdepelaaja.Ignite();


                if (ammus != null) ammus.Destroy();
            });

        AddCollisionHandler<PhysicsObject, Vihollinen>(
            ammus, delegate(PhysicsObject a, Vihollinen kohde)
            {
                Damagea(kohde, pelaaja.ValittuAse.TuhovoimaElaviaVastaan);
                if (pelaaja.ValittuAse.TuleekoOsumastaEfektiä)
                    Partikkelit.AddParticleEffect(Partikkelit.VeriPartikkelit, new Vector(a.X, a.Y), 40);
                if (pelaaja.ValittuAse.KuulukoOsumastaAanta)
                    osuma1.Play();
                if (pelaaja.ValittuAse.IsIncendiary)
                    if (RandomGen.NextInt(0, 100) < pelaaja.ValittuAse.IgnitionChance)
                        kohde.Ignite();

                if (ammus != null) ammus.Destroy();
            });

        AddCollisionHandler<PhysicsObject, Tuhoutuva>(
            ammus, delegate(PhysicsObject a, Tuhoutuva tuhoutuva)
        {
            if (tuhoutuva.IsStatic && pelaaja.ValittuAse.TuleekoOsumastaEfektiä)
            {
                GameObject luodinJalki = new GameObject(12, 13);
                luodinJalki.Image = crack1;
                luodinJalki.Position = a.Position;
                luodinJalki.Angle = RandomGen.NextAngle();
                Add(luodinJalki, 1);
                Efektit.LisaaTehosteObjekti(luodinJalki);
                tuhoutuva.Destroyed += delegate
                {
                    luodinJalki.Destroy();
                    Efektit.PoistaEfekti(luodinJalki);
                };
            }

            if (pelaaja.ValittuAse.HaviaakoAmmusOsumasta && ammus.IgnoresCollisionResponse == false)
            {
                a.Destroy();
                a.NeedsUpdateCall = false;
                if (pelaaja.ValittuAse.KuulukoOsumastaAanta)
                {
                    switch (RandomGen.NextInt(0, 5))
                    {
                        case 0:
                            luotiOsuu.Play();
                            break;
                        case 1:
                            luotiOsuu2.Play();
                            break;
                        case 2:
                            luotiOsuu3.Play();
                            break;
                        case 3:
                            luotiOsuu4.Play();
                            break;
                        case 4:
                            luotiOsuu5.Play();
                            break;
                    }
                }
            }
            else if (ammus.IgnoresCollisionResponse == true && pelaaja.ValittuAse.KuulukoOsumastaAanta) // ei tuhota
            {
                switch (RandomGen.NextInt(0, 5))
                {
                    case 0:
                        luotiOsuu.Play();
                        break;
                    case 1:
                        luotiOsuu2.Play();
                        break;
                    case 2:
                        luotiOsuu3.Play();
                        break;
                    case 3:
                        luotiOsuu4.Play();
                        break;
                    case 4:
                        luotiOsuu5.Play();
                        break;
                }
            }
            else
            {
                a.Velocity = new Vector(0.0, 0.0);
                a.Body.IsCollidable = false;
            }
            //tuhoutuva.Kesto.Value -= pelaaja.ValittuAse.TuhovoimaTuhoutuviaVastaan;
            Damagea(tuhoutuva, pelaaja.ValittuAse.TuhovoimaTuhoutuviaVastaan);
            if (pelaaja.ValittuAse.IsIncendiary)
                if (RandomGen.NextInt(0, 100) < pelaaja.ValittuAse.IgnitionChance)
                    tuhoutuva.Ignite();
        });

        AddCollisionHandler<PhysicsObject, Tuhoutuva>(
            ammus, "kivi", delegate(PhysicsObject a, Tuhoutuva kivi)
            {
                if (pelaaja.ValittuAse.TuleekoOsumastaEfektiä)
                {
                    Partikkelit.AddParticleEffect(Partikkelit.KiviPartikkelit, new Vector(a.X, a.Y), 30);
                    Partikkelit.AddParticleEffect(Partikkelit.KipinaPartikkelit, new Vector(a.X, a.Y), 5);
                }
            });

        AddCollisionHandler<PhysicsObject, Tuhoutuva>(
            ammus, "puu", delegate(PhysicsObject a, Tuhoutuva puu)
            {
                if (pelaaja.ValittuAse.TuleekoOsumastaEfektiä)
                    Partikkelit.AddParticleEffect(Partikkelit.PuuPartikkelit, new Vector(a.X, a.Y), 40);
            });
        AddCollisionHandler<PhysicsObject, Helikopteri>(
            ammus, delegate(PhysicsObject a, Helikopteri kopteri)
            {
                if (kopteri.OnkoIlmassa) return;

                Damagea(kopteri, pelaaja.ValittuAse.TuhovoimaElaviaVastaan);
                if (pelaaja.ValittuAse.TuleekoOsumastaEfektiä)
                    Partikkelit.AddParticleEffect(Partikkelit.KipinaPartikkelit, new Vector(a.X, a.Y), 40);
                a.Destroy();
            });

        AddCollisionHandler<PhysicsObject, PhysicsObject>(
            ammus, "piikkilanka", delegate(PhysicsObject a, PhysicsObject piikkilanka)
            {
                if (pelaaja.ValittuAse.TuleekoOsumastaEfektiä)
                    Partikkelit.AddParticleEffect(Partikkelit.KipinaPartikkelit, new Vector(a.X, a.Y), 40);
                //ammus.Destroy();
            });
        AddCollisionHandler<PhysicsObject, PhysicsObject>(
            ammus, delegate(PhysicsObject a, PhysicsObject kohde)
            {
                if (ammus.IgnoresCollisionResponse == false) a.Destroy();
                a.NeedsUpdateCall = false;
            });


    }

    /// <summary>
    /// Ammutaan haulikkotyyppisellä aseella.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka aseella ammutaan.</param>
    /// <param name="ammustenMaara">Montako ammusta ammutaan.</param>
    void AmmuHaulikolla(Pelaaja pelaaja, int ammustenMaara)
    {
        if (!pelaaja.ValittuAse.IsReady) return;

        pelaaja.ValittuAse.AttackSound.Play();
        pelaaja.ValittuAse.Ammo.Value--;
        Hylsy(pelaaja, pelaaja.ValittuAse.hylsynKuva);

        for (int i = 0; i < ammustenMaara; i++)
        {
            Ammu(pelaaja, true);
        }
    }

    void MeleeIsku(Pelaaja pelaaja)
    {
        Ase edellinenAse = pelaaja.ValittuAse;
        pelaaja.ValitseAse("nyrkki");
        Ammu(pelaaja);
        pelaaja.ValitseAse(edellinenAse.Tag.ToString());
        PaivitaPelaajanKuvaJaHUD(pelaaja);
    }


    public void AloitaLataus(Pelaaja pelaaja)
    {
        if (pelaaja.LadataankoAsetta) return;

        lippaanPoistoAani.Play();
        pelaaja.LadataankoAsetta = true;
        if (pelaaja.ValittuAse.LatausAnimaatio1 != null)
        {
            pelaaja.Animation = new Animation(pelaaja.ValittuAse.LatausAnimaatio1);
            pelaaja.Animation.Start(1);
            pelaaja.Animation.FPS = pelaaja.Animation.FrameCount / pelaaja.ValittuAse.LataukseenKuluvaAika;
        }
        Timer.SingleShot(pelaaja.ValittuAse.LataukseenKuluvaAika, delegate { Lataa(pelaaja); });
    }

    /// <summary>
    /// Ladataan pelaajan kulloinkin valittuna oleva ase.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka ase ladataan.</param>
    public void Lataa(Pelaaja pelaaja, bool playSound = true)
    {
        // Ammo.Value == valitun aseen senhetkinen lippaassa oleva ammusmäärä
        // Ammo.MaxValue == valitun aseen maksimimäärä ammuksia lippaassa
        // MaxAmmo.Value == valitun aseen kokonaisammusten maksimimäärä

        // jos ammukset täynnä, ei tehdä mitään
        if (pelaaja.ValittuAse.Ammo.Value == pelaaja.ValittuAse.Ammo.MaxValue)
        {
            pelaaja.LadataankoAsetta = false;
            return;
        }

        // montako ammusta on käytetty lippaasta
        int lippaastaKaytettyjaAmmuksia = pelaaja.ValittuAse.Ammo.MaxValue - pelaaja.ValittuAse.Ammo.Value;
        if (playSound)
            aseLatausAani.Play();
        if (pelaaja.ValittuAse.MaxAmmo.Value >= pelaaja.ValittuAse.Ammo.MaxValue) pelaaja.ValittuAse.Ammo.Value = pelaaja.ValittuAse.Ammo.MaxValue;
        else pelaaja.ValittuAse.Ammo.Value = pelaaja.ValittuAse.MaxAmmo.Value;

        pelaaja.ValittuAse.MaxAmmo.Value -= lippaastaKaytettyjaAmmuksia;

        pelaaja.ammusMaaraNaytto.Text = pelaaja.ValittuAse.Ammo.Value.ToString() + " / " + pelaaja.ValittuAse.MaxAmmo.Value.ToString();

        pelaaja.LadataankoAsetta = false;
    }

    /// <summary>
    /// Luo hylsyn pelaajan ampuessa ja lisää sen hylsyt-taulukkoon.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka aseesta hylsy lentää.</param>
    void Hylsy(Pelaaja pelaaja, Image hylsynkuva)
    {
        PhysicsObject hylsy = new PhysicsObject(2, 5);
        hylsy.Image = hylsynkuva;
        hylsy.Position = pelaaja.Position + Vector.FromLengthAndAngle(15, pelaaja.Angle + Angle.FromDegrees(-90));
        hylsy.LinearDamping = 0.93;
        hylsy.Mass = 0.5;
        hylsy.Angle = pelaaja.Angle + Angle.FromDegrees(90);
        hylsy.AngularVelocity = RandomGen.NextDouble(-20.0, 20.0);
        hylsy.AngularDamping = 0.97;
        Add(hylsy, -3);

        hylsy.MaximumLifetime = TimeSpan.FromSeconds(1.5);
        hylsy.Destroying += delegate
        {
            GameObject fakeHylsy = new GameObject(hylsy.Width, hylsy.Height); // samanlainen GameObject-hylsy samaan paikkaan lagin vähentämiseksi
            fakeHylsy.Image = hylsynkuva;
            fakeHylsy.Position = hylsy.Position;
            fakeHylsy.Angle = hylsy.Angle;
            Add(fakeHylsy, -1);
            Efektit.LisaaTehosteObjekti(fakeHylsy);
        };

        hylsy.Hit(Vector.FromLengthAndAngle(RandomGen.NextInt(150, 300), pelaaja.Angle + Angle.FromDegrees(RandomGen.NextDouble(-95, -85))));
        AddCollisionHandler(hylsy, HylsyOsuu);
        Timer.SingleShot(0.5, delegate { hylsynPutoamisAani.Play(0.7, 0.0, 0.0); });
    }

    /// <summary>
    /// CollisionHandleri hylsylle. Soitetaan kilahdusääni, jos hylsy osuu seinään.
    /// </summary>
    /// <param name="hylsy">Hylsy.</param>
    /// <param name="kohde">Kohde, johon hylsy osuu.</param>
    void HylsyOsuu(PhysicsObject hylsy, PhysicsObject kohde)
    {
        if (kohde is Tuhoutuva)
            hylsynPutoamisAani.Play(0.7, 0.0, 0.0);
    }

    /// <summary>
    /// Heitetään kranaatti, jos pelaaja on lisätty ruudulle ja pelaajalla on kranaatteja jäljellä.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, joka heittää kranaatin.</param>
    void HeitaKranaatti(Pelaaja pelaaja)
    {
        if (!pelaaja.IsAddedToGame || pelaaja.KranaattienMaara <= 0) return;
        const double KRANAATIN_NOPEUS = 25000;
        Grenade kranaatti = new Grenade(3.0, TimeSpan.FromSeconds(1.5));
        kranaatti.LinearDamping = 0.95;
        kranaatti.AngularVelocity = RandomGen.NextDouble(-5.0, 5.0);
        kranaatti.AngularDamping = 0.97;
        kranaatti.Image = kranaatinKuva;

        GameObject indicator = new GameObject(1.0, 1.0, Shape.Circle);
        indicator.Color = new Color(255, 165, 0, 64);
        indicator.Tag = true;
        kranaatti.Add(indicator);

        kranaatti.Explosion.Image = null;
        kranaatti.Explosion.ShockwaveColor = Color.Black;
        kranaatti.Explosion.IsShockwaveVisible = false;
        kranaatti.Explosion.Sound = Aseet.singonAani;
        kranaatti.Explosion.Speed = 2000;
        kranaatti.Explosion.MaxRadius = 500;
        kranaatti.Explosion.ShockwaveReachesObject += delegate(IPhysicsObject kohde, Vector shokki) { RajahdysOsuu(kohde, shokki, kranaatti.Position, Aseet.KRANAATIN_MAKSIMI_DAMAGE); };
        kranaatti.Destroying += delegate
        {
            Partikkelit.AddExplosionEffect(kranaatti.Position, 800);
            valo.Position = kranaatti.Position;
            valo.Intensity = 2.0;
            valo.Distance = 100;
            Timer.SingleShot(0.2, delegate { valo.Intensity = 1.5; });
            Timer.SingleShot(0.3, delegate { valo.Intensity = 1.0; });
            Timer.SingleShot(0.4, delegate { valo.Intensity = 0.5; });
            Timer.SingleShot(0.5, delegate { valo.Intensity = 0.0; });
        };
        kranaatti.Collided += delegate(IPhysicsObject k, IPhysicsObject kohde) { if (kohde.Tag.ToString() != "pelaaja" && kohde.Tag.ToString() != "tahtain") kranaatti.Velocity = Vector.Zero; };
        kranaatti.IsUpdated = true;
        kranaatti.NeedsUpdateCall = true;
        kranaatti.Updated += delegate(PhysicsObject p) { 
            DrawTracers(kranaatti.Position, Color.Orange, 0.30, 255);

            if ((bool)indicator.Tag == true && indicator.Width <= Vakiot.KRANAATTI_INDIKATTORIN_MAX_KOKO)
            {
                indicator.Width += Vakiot.KRANAATTI_INDIKAATTORIN_VALKKYMISNOPEUS;
                indicator.Height += Vakiot.KRANAATTI_INDIKAATTORIN_VALKKYMISNOPEUS;
                indicator.Tag = (indicator.Width < Vakiot.KRANAATTI_INDIKATTORIN_MAX_KOKO);
            }
            if ((bool)indicator.Tag == false && indicator.Width >= Vakiot.KRANAATTI_INDIKATTORIN_MIN_KOKO)
            {
                indicator.Width -= Vakiot.KRANAATTI_INDIKAATTORIN_VALKKYMISNOPEUS;
                indicator.Height -= Vakiot.KRANAATTI_INDIKAATTORIN_VALKKYMISNOPEUS;
                indicator.Tag = (indicator.Width < Vakiot.KRANAATTI_INDIKATTORIN_MIN_KOKO + Vakiot.KRANAATTI_INDIKAATTORIN_VALKKYMISNOPEUS);
            }

        }; // brightness 3
        pelaaja.Throw(kranaatti, Angle.FromDegrees(0), KRANAATIN_NOPEUS, 0, 0);
        if (!pelaaja.LoputtomatKranaatit)
        {
            pelaaja.KranaattienMaara--;
            pelaaja.NaytaViesti(Vakiot.HUD_KRANAATTEJA_JALJELLA + pelaaja.KranaattienMaara.ToString(), 1.0);
        }
    }

    /// <summary>
    /// Annetaan pelaajalle kamaa pelaajan törmätessä laatikkoon.
    /// </summary>
    /// <param name="laatikko">Laatikko, johon törmätään.</param>
    /// <param name="kohde">Pelaaja, joka törmää.</param>
    void AnnaKamaa(PhysicsObject laatikko, PhysicsObject kohde)
    {
        if (kohde.Tag.ToString() == "pelaaja")
        {
            Pelaaja pelaaja = (kohde as Pelaaja);
            Laatikko laat = (laatikko as Laatikko);

            if (laat.OnkoKäytössä == false)
            {
                //Ei voi ottaa kamaa käytetystä laatikosta
                return;
            }
            for (int i = 0; i < aseidenLuontimetodit.Length; i++)
            {
                Ase ase = aseidenLuontimetodit[i]();
                if (!pelaaja.OnkoPelaajallaAse(ase.Tag.ToString()))
                {
                    if (AikaKentanAlusta.SecondCounter.Value > ase.aikaJolloinVoiLuoda)
                    {
                        pelaaja.LisaaAse(ase);
                        //MessageDisplay.Add("Pelaaja " + pelaaja.Numero + " sai aseen: " + ase.AseenNimi);
                        pelaaja.NaytaViesti(Vakiot.HUD_SAIT_ASEEN + ase.AseenNimi, 1.0);
                        return;
                    }
                }
            }
            Ase satunnainenPelaajanAse;
            do
                satunnainenPelaajanAse = RandomGen.SelectOne<Ase>(pelaaja.Aseet);
            while (satunnainenPelaajanAse.OnkoMeleeAse);

            satunnainenPelaajanAse.MaxAmmo.Value = satunnainenPelaajanAse.MaxAmmo.MaxValue;
            //MessageDisplay.Add("Pelaaja " + pelaaja.Numero + " sai ammuksia aseeseen: " + satunnainenPelaajanAse.AseenNimi);
            pelaaja.NaytaViesti(Vakiot.HUD_SAIT_AMMUKSIA_ASEESEEN + satunnainenPelaajanAse.AseenNimi, 1.0);

            pelaaja.KranaattienMaara++;
            pelaaja.NaytaViesti(Vakiot.HUD_SAIT_KRANAATIN + pelaaja.KranaattienMaara.ToString(), 1.0);
            //MessageDisplay.Add("Pelaaja " + pelaaja.Numero + " sai kranaatin.");

            laatikko.Image = kamaaNäkymätön;
            laatikko.IgnoresCollisionResponse = true;
            laat.OnkoKäytössä = false;
            laat.LaatikonAika.Start();
        }
    }

    /// <summary>
    /// Vaihdetaan asetta ja pelaajan kuvaa.
    /// </summary>
    /// <param name="pelaaja">Pelaaja, jonka asetta vaihdetaan.</param>
    /// <param name="vaihtosuunta">Suunta, johon aselistaa pyöritetään.</param>
    void VaihdaAsetta(Pelaaja pelaaja, int vaihtosuunta = 0)
    {
        if (pelaaja.AseidenMaara < 1) return;
        pelaaja.VaihdaAse(Mouse.WheelChange);
        pelaaja.VaihdaAse(vaihtosuunta);

        PaivitaPelaajanKuvaJaHUD(pelaaja);
    }

    /*void StartedShooting(Pelaaja pelaaja)
    {
        if (pelaaja.ValittuAse.Tag.ToString() == "liekinheitin")
        {
            Flame f = Partikkelit.CreateDirectionalFlame(pelaaja.Position, pelaaja.Angle, 100);
            pelaaja.NeedsUpdateCall = true;
            pelaaja.Updated += delegate
            {
                f.Position = pelaaja.Position;
                f.FlameDirection = pelaaja.Angle;
                pelaaja.StoppedShooting += delegate
                {
                    f.FadeOut(2);
                    pelaaja.NeedsUpdateCall = false;
                    Timer.SingleShot(2.0, delegate
                    {
                        f.Destroy();
                    }); 
                };
            };
        }
    }

    void StoppedShooting(Pelaaja pelaaja)
    {
        pelaaja.IsShooting = false;
        if (pelaaja.StoppedShooting != null)
            pelaaja.StoppedShooting(pelaaja);
    }*/

    public void PaivitaPelaajanKuvaJaHUD(Pelaaja pelaaja)
    {
        pelaaja.ValittuAseNaytto.IsVisible = true;
        pelaaja.ValittuAseNaytto.Text = pelaaja.ValittuAse.AseenNimi;
        pelaaja.AsenaytonPiilotusAjastin.Reset();
        pelaaja.AsenaytonPiilotusAjastin.Start();

        if (pelaaja.ValittuAse.OnkoMeleeAse == true) pelaaja.ammusMaaraNaytto.Text = "";
        else pelaaja.ammusMaaraNaytto.Text = pelaaja.ValittuAse.Ammo.Value.ToString() + " / " + pelaaja.ValittuAse.MaxAmmo.Value.ToString();

        int i = Array.IndexOf(pelaajat, pelaaja);
        if (i == 0) pelaaja.Image = pelaaja.ValittuAse.Pelaajan1AseKuva;
        if (i == 1) pelaaja.Image = pelaaja.ValittuAse.Pelaajan2AseKuva;

        pelaaja.ValittuAse.IsVisible = false;
    }

    /// <summary>
    /// Ohjataan hakeutuvaa esinettä kohteeseen.
    /// </summary>
    /// <param name="hakeutuva">Hakeutuva esine.</param>
    /// <param name="kohde">Kohde, jota kohti hakeudutaan.</param>
    void HakeuduKohteeseen(PhysicsObject hakeutuva, PhysicsObject kohde, double nopeus)
    {
        if (kohde == null) return;

        const double HAKEUTUMISNOPEUS = 6; // 4

        Angle suunta = (kohde.AbsolutePosition - hakeutuva.AbsolutePosition).Angle;
        Vector suuntaKohteeseen = Vector.FromLengthAndAngle(nopeus, suunta);
        hakeutuva.Angle = hakeutuva.Velocity.Angle;
        if (hakeutuva.Velocity == suuntaKohteeseen) return; // ollaan oikeassa suunnassa

        if (hakeutuva.Velocity.X < suuntaKohteeseen.X) hakeutuva.Velocity = new Vector(hakeutuva.Velocity.X + HAKEUTUMISNOPEUS, hakeutuva.Velocity.Y);
        else hakeutuva.Velocity = new Vector(hakeutuva.Velocity.X - HAKEUTUMISNOPEUS, hakeutuva.Velocity.Y);

        if (hakeutuva.Velocity.Y < suuntaKohteeseen.Y) hakeutuva.Velocity = new Vector(hakeutuva.Velocity.X, hakeutuva.Velocity.Y + HAKEUTUMISNOPEUS);
        else hakeutuva.Velocity = new Vector(hakeutuva.Velocity.X, hakeutuva.Velocity.Y - HAKEUTUMISNOPEUS);
        hakeutuva.Velocity *= 1.01;
    }

    PhysicsObject EtsiLahinKohde(PhysicsObject hakeutuva, GameObject etsija)
    {
        List<GameObject> kohteet = GetObjects(x => x is Elava);

        double lahinEtaisyys = double.MaxValue;
        int lahin = 0;

        for (int i = 0; i < kohteet.Count; i++)
        {
            double etaisyys = Vector.Distance(hakeutuva.Position, kohteet[i].Position);
            if (etaisyys < lahinEtaisyys && kohteet[i] != etsija)
            {
                lahinEtaisyys = etaisyys;
                lahin = i;
            }
        }
        return (PhysicsObject)kohteet[lahin];
    }

    #endregion

    #region ohjaimet

    /// <summary>
    /// Asetetaan ohjaimet.
    /// </summary>
    void AsetaOhjaimet()
    {
        // Pelaajan 1 ohjaimet
        Keyboard.Listen(Key.Escape, Jypeli.ButtonState.Pressed, LuoPauseValikko, "Lopeta peli");
        Keyboard.Listen(Key.W, Jypeli.ButtonState.Down, LiikutaPelaajaa, "", pelaajat[0]);
        Keyboard.Listen(Key.S, Jypeli.ButtonState.Down, LiikutaPelaajaa, "", pelaajat[0]);
        Keyboard.Listen(Key.A, Jypeli.ButtonState.Down, LiikutaPelaajaa, "", pelaajat[0]);
        Keyboard.Listen(Key.D, Jypeli.ButtonState.Down, LiikutaPelaajaa, "", pelaajat[0]);
        Keyboard.Listen(Key.R, Jypeli.ButtonState.Pressed, delegate
        {
            AloitaLataus(pelaajat[0]);
        }, null);
        Keyboard.Listen(Key.LeftShift, Jypeli.ButtonState.Pressed, delegate { pelaajat[0].Juokse(); }, null);
        Keyboard.Listen(Key.LeftShift, Jypeli.ButtonState.Released, delegate { pelaajat[0].LopetaJuoksu(); }, null);
        Keyboard.Listen(Key.E, ButtonState.Pressed, MeleeIsku, null, pelaajat[0]);
        Mouse.ListenWheel(delegate { VaihdaAsetta(pelaajat[0]); }, null);
        Mouse.Listen(MouseButton.Left, Jypeli.ButtonState.Down, delegate
        {
            if (pelaajat[0].ValittuAse.BulletsPerShot != 1) AmmuHaulikolla(pelaajat[0], pelaajat[0].ValittuAse.BulletsPerShot);
            else Ammu(pelaajat[0]);
        }, null);
        //Mouse.Listen(MouseButton.Left, Jypeli.ButtonState.Released, delegate { StoppedShooting(pelaajat[0]); }, null);
        Mouse.Listen(MouseButton.Right, Jypeli.ButtonState.Pressed, HeitaKranaatti, null, pelaajat[0]);
        Mouse.ListenMovement(0.1, LiikutaTahtainta, null, pelaajat[0]);
#if DEBUG
        Keyboard.Listen(Key.RightControl, ButtonState.Pressed, delegate { MessageDisplay.Add(pelaajat[0].Position.ToString()); }, null);
        Keyboard.Listen(Key.RightShift, ButtonState.Pressed, delegate { ReitinTallennus.TallennaPaikka(pelaajat[0].Position); }, null); // tallentaa pelaajan 1 paikan vektorina tekstitiedostoon
        Keyboard.Listen(Key.End, ButtonState.Pressed, delegate { ReitinTallennus.LopetaTallennus(); }, null);
        Keyboard.Listen(Key.P, ButtonState.Pressed, delegate { MessageDisplay.Add(OnkoNaytonAlueella(new Vector(0.0, 0.0)).ToString()); }, null);
        Keyboard.Listen(Key.O, ButtonState.Pressed, LuoHelikopteri, null, pelaajat[0].tahtain.Position);
        Keyboard.Listen(Key.U, ButtonState.Pressed, delegate { AikaKentanAlusta.SecondCounter.Value = 1000; MessageDisplay.Add("[TESTI] Kaikki aseet avattu!"); }, null); // avataan kaikki aseet saataville laatikoista
#endif

        // Pelaajan 2 ohjaimet
        ControllerOne.ListenAnalog(AnalogControl.LeftStick, 0.1, LiikutaPelaajaaPadilla, null, pelaajat[1]);
        ControllerOne.Listen(Button.LeftStick, Jypeli.ButtonState.Pressed, delegate {
            pelaajat[1].tahtayksenKohde = null;
            pelaajat[1].Juokse(); 
        }, null);
        ControllerOne.Listen(Button.LeftStick, Jypeli.ButtonState.Released, delegate { pelaajat[1].LopetaJuoksu(); }, null);
        ControllerOne.ListenAnalog(AnalogControl.RightStick, 0.1, LiikutaTahtaintaPadilla, null, pelaajat[1]);
        ControllerOne.Listen(Button.RightTrigger, Jypeli.ButtonState.Down, delegate
        {
            if (pelaajat[1].ValittuAse.BulletsPerShot != 1) AmmuHaulikolla(pelaajat[1], pelaajat[1].ValittuAse.BulletsPerShot);
            else Ammu(pelaajat[1]);
        }, null);
        //ControllerOne.Listen(Button.RightTrigger, Jypeli.ButtonState.Released, delegate { StoppedShooting(pelaajat[1]); }, null);
        ControllerOne.Listen(Button.LeftTrigger, Jypeli.ButtonState.Pressed, FindAimlockTarget, null, pelaajat[1]);
        ControllerOne.Listen(Button.LeftTrigger, Jypeli.ButtonState.Released, delegate {
            pelaajat[1].tahtayksenKohde = null;
        }, null, pelaajat[1]);
        ControllerOne.Listen(Button.LeftTrigger, Jypeli.ButtonState.Down, delegate {
            if (pelaajat[1].tahtayksenKohde != null)
                pelaajat[1].Vasymys.Value -= Vakiot.PELAAJAN_VASYMISNOPEUS_TAHDATESSA;
        }, null, pelaajat[1]);


        ControllerOne.Listen(Button.LeftShoulder, Jypeli.ButtonState.Pressed, delegate { VaihdaAsetta(pelaajat[1], -1); }, null);
        ControllerOne.Listen(Button.RightShoulder, Jypeli.ButtonState.Pressed, delegate { VaihdaAsetta(pelaajat[1], 1); }, null);
        ControllerOne.Listen(Button.X, Jypeli.ButtonState.Pressed, delegate
        {
            AloitaLataus(pelaajat[1]);
        }, null);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, HeitaKranaatti, null, pelaajat[1]);
        ControllerOne.Listen(Button.RightStick, ButtonState.Pressed, MeleeIsku, null, pelaajat[1]);
    }

    /// <summary>
    /// Otetaan käyttöön sandbox-lisäohjaimet.
    /// </summary>
    void AsetaSandboxOhjaimet()
    {
        Keyboard.Listen(Key.Space, ButtonState.Pressed, delegate { 
            LuoTuhoutuvaKentanOsa(SijoitaKentanosaRuudukkoon(pelaajat[0].tahtain.Position), 50, 50, new IntPoint(0, 0), kivenKuva, "kivi", 20, 1.0, 1.0);           
        }, null);
        Keyboard.Listen(Key.K, ButtonState.Pressed, delegate { LuoKillBall(pelaajat[0].tahtain.Position); }, null);
        Keyboard.Listen(Key.T, ButtonState.Pressed, delegate { LuoTynnyri(SijoitaKentanosaRuudukkoon(pelaajat[0].tahtain.Position), 50.0, 50.0, null); }, null);
        Keyboard.Listen(Key.O, ButtonState.Pressed, LuoHelikopteri, null, pelaajat[0].tahtain.Position);
        Keyboard.Listen(Key.Delete, ButtonState.Pressed, RemoveAllEntities, null);
        Keyboard.Listen(Key.C, ButtonState.Pressed, delegate
        {
            Vihollinen zombi = new Vihollinen(normaaliZombiKuva.Width / 1.2, normaaliZombiKuva.Height / 1.2, normaaliZombiKuva, 20, 4, 100, pelaajat, false, false, this, null);
            Add(zombi);
            zombi.Position = pelaajat[0].tahtain.Position;

        }, null);
        ControllerOne.Listen(Button.B, ButtonState.Pressed, delegate { LuoTuhoutuvaKentanOsa(SijoitaKentanosaRuudukkoon(pelaajat[1].Position + Vector.FromLengthAndAngle(pelaajat[1].Width, pelaajat[1].Angle)), 50, 50, new IntPoint(0, 0), kivenKuva, "kivi", 20, 1.0, 1.0); }, null);
        ControllerOne.Listen(Button.Y, ButtonState.Pressed, delegate { LuoTynnyri(SijoitaKentanosaRuudukkoon(pelaajat[1].Position + Vector.FromLengthAndAngle(pelaajat[1].Width, pelaajat[1].Angle)), 50.0, 50.0, null); }, null);
        ControllerOne.Listen(Button.DPadUp, ButtonState.Pressed, delegate
        {
            Vihollinen zombi = new Vihollinen(normaaliZombiKuva.Width / 1.2, normaaliZombiKuva.Height / 1.2, normaaliZombiKuva, 20, 4, 100, pelaajat, false, false, this, null);
            Add(zombi);
            zombi.Position = pelaajat[1].Position + Vector.FromLengthAndAngle(pelaajat[1].Width * 3, pelaajat[1].Angle);

        }, null);
    }

    /// <summary>
    /// Asetetaan survivalissa tarvittavat lisäohjaimet.
    /// </summary>
    void AsetaSurvivalOhjaimet()
    {
        Keyboard.Listen(Key.Space, ButtonState.Pressed, delegate {
            AsetaSeina(pelaajat[0]);
            Infinite.CurrentGame.aiUpdater.UpdateRoutesForAddedWalls();
        }, null);
        ControllerOne.Listen(Button.B, ButtonState.Pressed, delegate {
            AsetaSeina(pelaajat[1]);
            Infinite.CurrentGame.aiUpdater.UpdateRoutesForAddedWalls();
        }, null);
    }

    #endregion

    #region aseiden osumat

    /// <summary>
    /// Kutsutaan, kun singon raketti osuu johonkin.
    /// </summary>
    /// <param name="ammus">Ammus, joka osuu.</param>
    /// <param name="kohde">Kohde, johon osutaan.</param>
    public void SingonAmmusRajahtaa(PhysicsObject ammus, PhysicsObject kohde, Ase ase)
    {
        Explosion sinkopaineaalto = new Explosion(200);
        sinkopaineaalto.Position = ammus.Position;
        sinkopaineaalto.Image = tyhjä;
        sinkopaineaalto.Sound = null;
        sinkopaineaalto.Force = 400; // 400
        sinkopaineaalto.Speed = 1000;
        sinkopaineaalto.ShockwaveColor = Color.Black;
        sinkopaineaalto.ShockwaveReachesObject += delegate(IPhysicsObject paineaallonKohde, Vector shokki) 
        { 
            RajahdysOsuu(paineaallonKohde, shokki, ammus.Position, Aseet.SINGON_MAKSIMI_DAMAGE);
            /*if (ase.IsIncendiary)
            {
                if (RandomGen.NextInt(0, 100) < ase.IgnitionChance)
                    kohde.Ignite();
            }*/
        };

        Add(sinkopaineaalto);
        Aseet.singonAani.Play();

        Partikkelit.AddParticleEffect(Partikkelit.RajahdysPartikkelit1, new Vector(kohde.X, kohde.Y), 500);
        Partikkelit.AddParticleEffect(Partikkelit.RajahdysPartikkelit2, new Vector(kohde.X, kohde.Y), 300);
        ammus.Destroy();

        valo.Position = ammus.Position;
        valo.Intensity = 2.0;
        valo.Distance = 100;
        Timer.SingleShot(0.2, delegate { valo.Intensity = 1.5; });
        Timer.SingleShot(0.3, delegate { valo.Intensity = 1.0; });
        Timer.SingleShot(0.4, delegate { valo.Intensity = 0.5; });
        Timer.SingleShot(0.5, delegate { valo.Intensity = 0.0; });
    }

    /// <summary>
    /// Lasketaan räjähdyksen aiheuttama vahinko etäisyyden mukaan.
    /// </summary>
    /// <param name="rajahdyksenKeskiPiste">Räjähdyksen keskipiste.</param>
    /// <param name="kohde">Kohde, johon räjähdys osuu.</param>
    /// <param name="maksimivahinko">Vahinko, jonka räjähdys tekee nollapisteessä.</param>
    /// <param name="nopeusJollaVahinkoVahenee">Nopeus, jolla räjähdyksen vahinko vähenee etäisyyden kasvaessa. Suurempi luku hidastaa, pienempi nopeuttaa.</param>
    /// <returns>Vahinko.</returns>
    double LaskeRajahdyksenDamage(Vector rajahdyksenKeskiPiste, Vector kohde, double maksimivahinko, double nopeusJollaVahinkoVahenee)
    {
        double pisteidenEtaisyys = Vector.Distance(rajahdyksenKeskiPiste, kohde) / nopeusJollaVahinkoVahenee;
        double damage = maksimivahinko - pisteidenEtaisyys;
        if (damage < 0) return 0;
        return damage;
    }

    /// <summary>
    /// Törmäyksenkuuntelija räjähdyksille.
    /// </summary>
    /// <param name="kohde">Kohde, johon räjähdys osuu.</param>
    /// <param name="shokki">Pakollinen parametri.</param>
    void RajahdysOsuu(IPhysicsObject kohde, Vector shokki, Vector rajahdyksenAlkamisPiste, double maksimivahinko)
    {
        double todellinenVahinko = LaskeRajahdyksenDamage(rajahdyksenAlkamisPiste, kohde.Position, maksimivahinko, 8);
        if (todellinenVahinko == 0) return;

        if (kohde is Tuhoutuva)
        {
            //Partikkelit.AddParticleEffect(Partikkelit.KiviHajoaa, new Vector(kohde.X, kohde.Y), 50);
            //Partikkelit.AddParticleEffect(Partikkelit.PuuHajoaa, new Vector(kohde.X, kohde.Y), 50);
            if (kohde != null) Damagea(kohde as Tuhoutuva, todellinenVahinko);
        }

        if (kohde is Elava)
        {
            Elava kohdePelaaja = kohde as Elava;
            Partikkelit.AddParticleEffect(Partikkelit.VeriPartikkelit, new Vector(kohde.X, kohde.Y), 40);
            GameObject[] effects = Blood.AddNormalBlood(kohde.Position, 3, 0.3);
            foreach (GameObject blood in effects)
            {
                Efektit.LisaaTehosteObjekti(blood);
            }
            Damagea(kohdePelaaja, todellinenVahinko);
        }
    }
    #endregion
}