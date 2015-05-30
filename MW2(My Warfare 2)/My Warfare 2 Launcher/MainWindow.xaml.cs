using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Reflection;

namespace My_Warfare_2_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool kaytetaankoDevelopmentVersiota = false;

        private static String LauncherinHakemistoPolku = System.IO.Path.GetDirectoryName(Assembly.GetAssembly(typeof(MainWindow)).CodeBase).Remove(0, 6);

        private const String VERSION_NUMBER = "1.2";

        public MainWindow()
        {
            InitializeComponent();
            changelog.Content = Update.LueTekstiaNetista("https://dl.dropboxusercontent.com/u/25754602/My%20Warfare%202%20Launcher%20Data/changelog.txt");
            updateProgressBar.Visibility = Visibility.Hidden;
            statusBox.Content = "Ready";
            versionNumber.Content = Update.CheckVersionNumber(LauncherinHakemistoPolku + "\\bin\\version.txt");
            launcherVersion.Content = VERSION_NUMBER;
            if (!System.IO.Directory.Exists(LauncherinHakemistoPolku + "\\bin"))
            {
                launchButton.IsEnabled = false;
                updateButton.Content = "Download";
                statusBox.Content = "Game not installed, click Download!";
            }
            else launchButton.IsEnabled = true;
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            kaytetaankoDevelopmentVersiota = false;
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            kaytetaankoDevelopmentVersiota = true;
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(LauncherinHakemistoPolku + "\\bin\\MW2(My Warfare 2).exe")) Process.Start(LauncherinHakemistoPolku + "\\bin\\MW2(My Warfare 2)");
            else
            {
                launchButton.IsEnabled = false;
                statusBox.Content = "Game not installed, click Download!";
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            Update.UpdateGame(kaytetaankoDevelopmentVersiota, updateProgressBar, statusBox, launchButton, updateButton, versionNumber);
        }
    }
}
