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
    public partial class FilePage : PhoneApplicationPage
    {
        public static Uri URI = new Uri( "/FilePage.xaml", UriKind.Relative );

        public FilePage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_GotFocus( object sender, RoutedEventArgs e )
        {
        }

        private void PhoneApplicationPage_Loaded( object sender, RoutedEventArgs e )
        {
            UpdateFileList();
        }

        private void buttonView_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                FileContentPage.FileName = GetSelectedFileName();
                NavigationService.Navigate( FileContentPage.URI );
            }
            catch ( IndexOutOfRangeException )
            {
            }
        }

        private void buttonDelete_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                DataStorage.Instance.Delete( GetSelectedFileName() );
                UpdateFileList();
            }
            catch ( IndexOutOfRangeException )
            {
            }
        }

        string GetSelectedFileName()
        {
            if ( fileList.SelectedIndex < 0 ) throw new IndexOutOfRangeException();
            return fileList.SelectedItem as string;
        }

        void UpdateFileList()
        {
            fileList.Items.Clear();
            foreach ( string file in DataStorage.Instance.GetFileList() )
                fileList.Items.Add( file );
        }
    }
}
