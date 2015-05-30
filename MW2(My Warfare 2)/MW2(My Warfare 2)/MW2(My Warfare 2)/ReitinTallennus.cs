using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows;


public static class ReitinTallennus
{
    static string pelinHakemistoPolku = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Asetukset)).CodeBase).Remove(0, 6);
    static string tallennusPaikka = pelinHakemistoPolku + "\\" + "reitti.txt";

    private static FileStream fs;
    private static StreamWriter fOutStream;

    private static bool IsInitialized = false;

    public static void AloitaTallennus()
    {       
        if (!File.Exists(tallennusPaikka))
        {
            fs = File.Create(tallennusPaikka);
        }
        else fs = File.OpenWrite(tallennusPaikka);

        fOutStream = new StreamWriter(fs);
        IsInitialized = true;
    }

    public static void TallennaPaikka(Jypeli.Vector position)
    {
        if (!IsInitialized) AloitaTallennus();

        fOutStream.WriteLine("reitti.Add(new Vector" + position.ToString() + ");");
        fOutStream.Flush();
    }

    public static void LopetaTallennus()
    {
        if (!IsInitialized) return;

        fOutStream.Flush();
        fOutStream.Close();
        fs.Close();
    }
}