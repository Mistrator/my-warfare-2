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

namespace SilverStorageTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void buttonSimpleTest_Click( object sender, RoutedEventArgs e )
        {
            NavigationService.Navigate( SimplePage.URI );
        }

        private void buttonArrayTest_Click( object sender, RoutedEventArgs e )
        {
            NavigationService.Navigate( ArrayPage.URI );
        }

        private void buttonListTest_Click( object sender, RoutedEventArgs e )
        {
            NavigationService.Navigate( ListPage.URI );
        }

        private void buttonFileExplorer_Click( object sender, RoutedEventArgs e )
        {
            NavigationService.Navigate( FilePage.URI );
        }
    }
}
