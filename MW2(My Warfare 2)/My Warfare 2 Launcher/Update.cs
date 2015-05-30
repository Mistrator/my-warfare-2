using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Controls;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows;

namespace My_Warfare_2_Launcher
{
    /// <summary>
    /// Pelin päivitys.
    /// </summary>
    public class Update
    {
        /// <summary>
        /// Tiedoston osoite, josta löytyy linkki uusimpaan development buildiin.
        /// </summary>
        private static String DevelopmentBuildListUrl = "https://dl.dropboxusercontent.com/u/25754602/My%20Warfare%202%20Launcher%20Data/newest_development.txt";

        /// <summary>
        /// Tiedoston osoite, josta löytyy linkki uusimpaan stable buildiin.
        /// </summary>
        private static String StableBuildListUrl = "https://dl.dropboxusercontent.com/u/25754602/My%20Warfare%202%20Launcher%20Data/newest_stable.txt";

        private static ProgressBar ProgressBar { get; set; }

        private static Label StatusBox { get; set; }

        private static Button LaunchButton { get; set; }

        private static Button UpdateButton { get; set; }

        private static Label VersionNumber { get; set; }

        /// <summary>
        /// Launcherin osoite.
        /// </summary>
        private static String LauncherinHakemistoPolku = Path.GetDirectoryName(Assembly.GetAssembly(typeof(MainWindow)).CodeBase).Remove(0, 6);


        /// <summary>
        /// Ladataan uusin valitun tyyppinen versio ja asennetaan se.
        /// </summary>
        /// <param name="kaytetaankoDevelopmentBuildia">Ladataanko development build. False: Stable-versio, True: Development-versio.</param>
        /// <returns>Onnistuiko päivitys.</returns>
        public static void UpdateGame(bool kaytetaankoDevelopmentBuildia, ProgressBar etenemisPalkki, Label statusBox, Button startButton, Button updateButton, Label versionNumber)
        {
            string updateFileUrl; // uusimman version URL
            Update.ProgressBar = etenemisPalkki;
            Update.StatusBox = statusBox;
            Update.StatusBox.Content = "Starting update...";
            Update.LaunchButton = startButton;
            LaunchButton.IsEnabled = false;
            Update.UpdateButton = updateButton;
            UpdateButton.IsEnabled = false;
            Update.VersionNumber = versionNumber;

            if (kaytetaankoDevelopmentBuildia) updateFileUrl = LueTekstiaNetista(DevelopmentBuildListUrl);
            else updateFileUrl = LueTekstiaNetista(StableBuildListUrl);

            if (updateFileUrl == "ERROR") // jos osoitteen haku failaa
            {
                Update.StatusBox.Content = "Update failed!";
                LaunchButton.IsEnabled = File.Exists(LauncherinHakemistoPolku + "\\bin\\MW2(My Warfare 2)");
                UpdateButton.IsEnabled = true;
                return;
            }

            if (updateFileUrl == "" && kaytetaankoDevelopmentBuildia) // jos osoitetta ei ole
            {
                Update.StatusBox.Content = "No development versions available!";
                LaunchButton.IsEnabled = File.Exists(LauncherinHakemistoPolku + "\\bin\\MW2(My Warfare 2)");
                UpdateButton.IsEnabled = true;
                return;
            }

            if (updateFileUrl == "" && !kaytetaankoDevelopmentBuildia) // jos osoitetta ei ole
            {
                Update.StatusBox.Content = "No stable versions available!";
                LaunchButton.IsEnabled = File.Exists(LauncherinHakemistoPolku + "\\bin\\MW2(My Warfare 2)");
                UpdateButton.IsEnabled = true;
                return;
            }

            if (Directory.Exists(LauncherinHakemistoPolku + "\\bin")) Directory.Delete(LauncherinHakemistoPolku + "\\bin", true);

            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(updateFileUrl), LauncherinHakemistoPolku + "\\temp");
            Update.StatusBox.Content = "Downloading update...";
            Update.ProgressBar.Visibility = Visibility.Visible;
            return;
        }

        /// <summary>
        /// Luetaan tekstiä annetusta tekstitiedostosta.
        /// </summary>
        /// <param name="versiolistanUrl">Tiedoston URL.</param>
        /// <returns>Tiedoston sisältö tekstinä.</returns>
        public static String LueTekstiaNetista(string tiedostonURL)
        {
            HttpWebResponse response = null;
            StreamReader reader = null;
            StringBuilder teksti = new StringBuilder();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tiedostonURL);
                request.Method = "GET";
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                while (!reader.EndOfStream)
                {
                    teksti.Append(reader.ReadLine() + "\n");
                }
            }

            catch (WebException)
            {
                return "ERROR (No internet connection?)";
            }

            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
            return teksti.ToString();
        }

        private static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Update.ProgressBar.Value = e.ProgressPercentage;
        }

        private static void Completed(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Update.StatusBox.Content = "Extracting...";
            DecompressFiles(LauncherinHakemistoPolku + "\\temp", LauncherinHakemistoPolku + "\\bin", ""); // \bin luodaan DecompressFilesissa
            Update.StatusBox.Content = "Game updated";

            if (File.Exists(LauncherinHakemistoPolku + "\\temp")) File.Delete(LauncherinHakemistoPolku + "\\temp");

            Update.ProgressBar.Value = 0;
            Update.ProgressBar.Visibility = Visibility.Hidden;
            LaunchButton.IsEnabled = true;
            UpdateButton.IsEnabled = true;
            VersionNumber.Content = Update.CheckVersionNumber(LauncherinHakemistoPolku + "\\bin\\version.txt");
        }

        /// <summary>
        /// Puretaan Zip-tiedosto.
        /// </summary>
        /// <param name="ZipPath">Tiedoston polku.</param>
        /// <param name="Destination">Mihin tiedostot puretaan. Jos hakemistoa ei ole, se luodaan.</param>
        /// <param name="Password">Zipin salasana.</param>
        public static void DecompressFiles(String ZipPath, String Destination, String Password)
        {
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(ZipPath)))
            {
                if (Password != null && Password != String.Empty)
                {
                    s.Password = Password;
                }

                ZipEntry entry;
                String tmpEntry = String.Empty;

                while ((entry = s.GetNextEntry()) != null)
                {
                    String dirName = Destination;
                    String fileName = Path.GetFileName(entry.Name);

                    if (dirName != "")
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    if (fileName != String.Empty)
                    {
                        if (entry.Name.IndexOf(".ini") < 0)
                        {
                            String FileName = dirName + "\\" + entry.Name;
                            FileName = FileName.Replace("\\ ", "\\");

                            String FolderName = Path.GetDirectoryName(FileName);

                            if (Directory.Exists(FolderName) == false)
                            {
                                Directory.CreateDirectory(FolderName);
                            }

                            FileStream fStream = File.Create(FileName);
                            int StreamSize = 2048;
                            byte[] buffer = new byte[2048];

                            while (true)
                            {
                                StreamSize = s.Read(buffer, 0, buffer.Length);
                                if (StreamSize > 0)
                                {
                                    fStream.Write(buffer, 0, StreamSize);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            fStream.Close();
                        }
                    }
                }
                s.Close();
            }
        }

        /// <summary>
        /// Palauttaa annetun tiedoston sisällön.
        /// </summary>
        /// <param name="path">Tiedoston polku.</param>
        /// <returns>Tiedoston sisällön ensimmäinen rivi.</returns>
        public static String CheckVersionNumber(string path)
        {
            if (!File.Exists(path)) return "Not installed!";
            using (StreamReader inStream = new StreamReader(path))
                return inStream.ReadLine();
        }
    }
}
