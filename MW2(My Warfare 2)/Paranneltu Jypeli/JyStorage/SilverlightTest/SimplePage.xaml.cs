using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Jypeli;

namespace SilverStorageTest
{
    public partial class SimplePage : PhoneApplicationPage
    {
        public static Uri URI = new Uri( "/SimplePage.xaml", UriKind.Relative );

        // Constructor
        public SimplePage()
        {
            InitializeComponent();
        }

        private void buttonSave_Click( object sender, RoutedEventArgs e )
        {
            using ( SaveState state = DataStorage.Instance.BeginSave( textFileName.Text ) )
            {
                state.Save<bool>( checkBox1.IsChecked.Value, "Check1" );
                state.Save<bool>( checkBox2.IsChecked.Value, "Check2" );
                state.Save<bool>( checkBox3.IsChecked.Value, "Check3" );
                state.Save<string>( textBox1.Text, "TextBox" );
            }
        }

        private void buttonLoad_Click( object sender, RoutedEventArgs e )
        {
            if ( !DataStorage.Instance.Exists( textFileName.Text ) )
            {
                textBox1.Text = "File not found.";
                return;
            }

            using ( Jypeli.LoadState state = DataStorage.Instance.BeginLoad( textFileName.Text ) )
            {
                checkBox1.IsChecked = state.Load<bool>( false, "Check1" );
                checkBox2.IsChecked = state.Load<bool>( false, "Check2" );
                checkBox3.IsChecked = state.Load<bool>( false, "Check3" );
                textBox1.Text = state.Load<string>( textBox1.Text, "TextBox" );
            }
        }

        private void buttonDelete_Click( object sender, RoutedEventArgs e )
        {
            DataStorage.Instance.Delete( textFileName.Text );
        }

        private void buttonClear_Click( object sender, RoutedEventArgs e )
        {
            checkBox1.IsChecked = false;
            checkBox2.IsChecked = false;
            checkBox3.IsChecked = false;
            textBox1.Text = "";
        }

        private void simpleTestPage_BackKeyPress( object sender, System.ComponentModel.CancelEventArgs e )
        {
            NavigationService.GoBack();
        }
    }
}