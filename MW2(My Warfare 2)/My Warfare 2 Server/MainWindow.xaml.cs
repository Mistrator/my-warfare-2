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

namespace My_Warfare_2_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Pelimuoto.
        /// </summary>
        public static int GameType { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (!Server.IsRunning)
            {
                Server.StartServer(int.Parse(portBox.Text), chatWindow, passwordBox.Text, 0);
                startButton.Content = "Stop!";
            }
            else
            {
                Server.StopServer();
                startButton.Content = "Start!";
            }
        }

        private void chatWindow_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Server.IsRunning) return;
            Server.ShowMessage("SERVER", messageBox.Text);
            Server.SendMessage("[SERVER] " + messageBox.Text);
            messageBox.Text = "Type here...";
        }

        private void messageBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void messageBox_GotFocus(object sender, RoutedEventArgs e)
        {
            messageBox.Text = "";
        }

        private void messageBox_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void customizeButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Server.IsRunning) Server.StopServer();
        }
    }
}
