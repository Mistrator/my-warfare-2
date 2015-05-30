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
    public partial class ArrayPage : PhoneApplicationPage
    {
        public static Uri URI = new Uri( "/ArrayPage.xaml", UriKind.Relative );

        [Save] string[] array = new string[4];
        TextBox[] textBoxes = new TextBox[4];

        public ArrayPage()
        {
            InitializeComponent();

            textBoxes[0] = textBox1;
            textBoxes[1] = textBox2;
            textBoxes[2] = textBox3;
            textBoxes[3] = textBox4;

            BoxesToArray();
        }

        private void ArrayToBoxes()
        {
            for ( int i = 0; i < array.Length; i++ )
                textBoxes[i].Text = array[i];
        }

        private void BoxesToArray()
        {
            for ( int i = 0; i < array.Length; i++ )
                array[i] = textBoxes[i].Text;
        }

        private void buttonDelete_Click( object sender, RoutedEventArgs e )
        {
            for ( int i = 0; i < 4; i++ )
                textBoxes[i].Text = "";

            DataStorage.Instance.Delete( "Array.xml" );
        }

        private void buttonSave_Click( object sender, RoutedEventArgs e )
        {
            BoxesToArray();
            DataStorage.Instance.Save<string[]>( array, textFileName.Text );
        }

        private void buttonLoad_Click( object sender, RoutedEventArgs e )
        {
            if ( !DataStorage.Instance.Exists( textFileName.Text ) )
            {
                textBox1.Text = "File not found!";
                return;
            }

            array = DataStorage.Instance.Load<string[]>( array, textFileName.Text );
            ArrayToBoxes();
        }

        private void buttonDelete_Click_1( object sender, RoutedEventArgs e )
        {
            DataStorage.Instance.Delete( textFileName.Text );
        }

        private void buttonClear_Click( object sender, RoutedEventArgs e )
        {
            for ( int i = 0; i < textBoxes.Length; i++ )
                textBoxes[i].Text = "";
        }
    }
}
