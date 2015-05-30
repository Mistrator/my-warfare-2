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
using System.Text;
using System.IO;

namespace SilverStorageTest
{
    public partial class FileContentPage : PhoneApplicationPage
    {
        public static Uri URI = new Uri( "/FileContentPage.xaml", UriKind.Relative );
        public static string FileName { private get; set; }

        public FileContentPage()
        {
            InitializeComponent();
            PageTitle.Text = FileName;

            StorageFile file = DataStorage.Instance.Open( FileName, false );
            StreamReader reader = new StreamReader( file.Stream );
            FileContent.Text = reader.ReadToEnd();
            reader.Close();
            file.Close();
        }
    }
}