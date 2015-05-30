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
    public partial class ListPage : PhoneApplicationPage
    {
        public static Uri URI = new Uri( "/ListPage.xaml", UriKind.Relative );

        public ListPage()
        {
            InitializeComponent();
        }

        private void buttonAddText_Click( object sender, RoutedEventArgs e )
        {
            listBox.Items.Add( textBox.Text );
        }

        private void buttonClear_Click( object sender, RoutedEventArgs e )
        {
            listBox.Items.Clear();
        }

        private void buttonLoad_Click( object sender, RoutedEventArgs e )
        {
            if ( !DataStorage.Instance.Exists( textFileName.Text ) )
            {
                textBox.Text = "File not found!";
                return;
            }

            List<string> itemList = DataStorage.Instance.Load<List<string>>( new List<string>(), textFileName.Text );
            listBox.Items.Clear();
            foreach ( string item in itemList ) listBox.Items.Add( item );
        }

        private void buttonSave_Click( object sender, RoutedEventArgs e )
        {
            List<string> itemList = listBox.Items.OfType<string>().ToList<string>();
            DataStorage.Instance.Save<List<string>>( itemList, textFileName.Text );
        }

        private void buttonDelete_Click( object sender, RoutedEventArgs e )
        {
            DataStorage.Instance.Delete( textFileName.Text );
        }
    }
}
