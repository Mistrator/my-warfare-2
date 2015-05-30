using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows;


public static class Ohjelma
{
#if WINDOWS || XBOX
    public static void Main(string[] args)
    {
        using (MW2_My_Warfare_2_ game = new MW2_My_Warfare_2_())
        {
#if !DEBUG // K‰ynnistet‰‰n peli Release-tilassa crashlogien kanssa.
            game.IsFullScreen = true;
            try
            {
                game.Run();
            }
            catch (Exception ex)
            {
                TallennaCrashLog(ex);
            }
#endif

#if DEBUG // Ei k‰ytet‰ crashlogeja Debug-tilassa.
            game.Run();
#endif
        }
    }
#endif

    /// <summary>
    /// K‰ynnistet‰‰n peli ja alustetaan tarvittavat asiat.
    /// </summary>
    public static void StartGame()
    {
        using (MW2_My_Warfare_2_ game = new MW2_My_Warfare_2_())
        {
            
#if !DEBUG // K‰ynnistet‰‰n peli Release-tilassa crashlogien kanssa.
            game.IsFullScreen = true;
            try
            {
                game.Run();
            }
            catch (Exception ex)
            {
                TallennaCrashLog(ex);
            }
#endif

#if DEBUG // Ei k‰ytet‰ crashlogeja Debug-tilassa.
            game.Run();
#endif
        }
    }

    /// <summary>
    /// Luodaan tekstitiedosto, joka sis‰lt‰‰ tiedot ohjelman kaatumisesta.
    /// </summary>
    /// <param name="ex">Sattunut virhe.</param>
    public static void TallennaCrashLog(Exception ex)
    {
        string pelinHakemistoPolku = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Asetukset)).CodeBase).Remove(0, 6);
        string tallennusPaikkaTiedostonKanssa = pelinHakemistoPolku + "\\Crash-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.TimeOfDay.Hours.ToString() + "-" + DateTime.Now.TimeOfDay.Minutes.ToString() + ".txt";

        FileStream fs;
        if (!File.Exists(tallennusPaikkaTiedostonKanssa))
        {
            fs = File.Create(tallennusPaikkaTiedostonKanssa);
        }
        else fs = File.OpenWrite(tallennusPaikkaTiedostonKanssa);

        StreamWriter fOutStream = new StreamWriter(fs);
        fOutStream.WriteLine("-------------------------");
        fOutStream.WriteLine("Crash log - My Warfare 2");
        fOutStream.WriteLine(DateTime.Now.ToString());
        fOutStream.WriteLine("-------------------------");
        fOutStream.WriteLine();
        fOutStream.WriteLine("Virhe: ");
        fOutStream.WriteLine(ex.Message);
        fOutStream.WriteLine();
        fOutStream.WriteLine("Miss‰ virhe tapahtui:");
        fOutStream.WriteLine(ex.TargetSite);
        fOutStream.WriteLine();
        fOutStream.WriteLine("Call Stack virhehetkell‰:");
        fOutStream.WriteLine(ex.StackTrace);
        fOutStream.Flush();
        fOutStream.Close();
    }
}