using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Security;

namespace Jypeli
{
    public class WindowsFileManager : FileManager
    {
        private string[] pathCandidates;

        public WindowsFileManager( params string[] pathCandidates )
        {
            this.pathCandidates = pathCandidates;
        }

        protected override void Initialize()
        {
            for ( int i = 0; i < pathCandidates.Length; i++ )
            {
                _currentDir = pathCandidates[i];

                if ( Directory.Exists( _currentDir ) )
                    return;
                else
                {
                    var parent = Directory.GetParent( _currentDir );
                    if ( !parent.Exists ) continue;
                    Directory.CreateDirectory( _currentDir );
                }
            }
        }

        public WindowsFileManager()
            : this( WindowsLocation.ExePath, Path.Combine( WindowsLocation.MyDocuments, Application.ProductName ) )
        {
        }

        /// <summary>
        /// Kertoo onko tiedosto tai hakemisto olemassa.
        /// </summary>
        /// <param name="fileName">Tiedoston tai hakemiston nimi</param>
        /// <returns>Onko olemassa</returns>
        public override bool Exists( string fileName )
        {
            Initialize();
            MakeAbsolute( ref fileName );
            return FMAssert( File.Exists, false, false, fileName ) || FMAssert( Directory.Exists, false, false, fileName );
        }

        /// <summary>
        /// Vaihtaa työhakemistoa.
        /// </summary>
        /// <param name="path">Hakemistopolku.</param>
        /// <returns>Onnistuiko hakemiston vaihtaminen (palauttaa false jos hakemistoa ei ole).</returns>
        public override bool ChDir( string path )
        {
            Initialize();
            MakeAbsolute( ref path );

            if ( !FMAssert( Directory.Exists, false, false, path ) )
                return false;

            _currentDir = path;
            return true;
        }

        /// <summary>
        /// Luo uuden hakemiston.
        /// </summary>
        /// <param name="path">Hakemistopolku.</param>
        public override void MkDir( string path )
        {
            Initialize();
            MakeAbsolute( ref path );
            FMAssert( Directory.CreateDirectory, true, null, path );
        }

        /// <summary>
        /// Tuhoaa hakemiston. Heittää poikkeuksen jos hakemisto ei ole tyhjä.
        /// Ei heitä poikkeusta, jos hakemistoa ei ole olemassa.
        /// </summary>
        /// <param name="path"></param>
        public override void RmDir( string path )
        {
            Initialize();
            MakeAbsolute( ref path );
            FMAssert( Directory.Delete, true, path );
        }

        /// <summary>
        /// Antaa listan nykyisessä hakemistossa olevista tiedostoista.
        /// </summary>
        /// <returns></returns>
        public override IList<string> GetFileList()
        {
            Initialize();
            string[] fileList = FMAssert( Directory.GetFiles, false, new string[] { }, _currentDir );
            return fileList.ToList<string>().AsReadOnly();
        }

        /// <summary>
        /// Avaa tiedoston.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi.</param>
        /// <param name="write">Tarvitaanko tiedostoon kirjoitusoikeus.</param>
        /// <returns>StorageFile-olio</returns>
        public override StorageFile Open( string fileName, bool write )
        {
            Initialize();
            MakeAbsolute( ref fileName );
            Stream stream = FMAssert( openFileStream, write, null, fileName, write );
            return new StorageFile( fileName, stream );
        }

        private Stream openFileStream(string fileName, bool write)
        {
            FileMode mode = write ? FileMode.Create : FileMode.Open;
            FileAccess access = write ? FileAccess.ReadWrite : FileAccess.Read;
            return new FileStream( fileName, mode, access );
        }

        /// <summary>
        /// Poistaa tiedoston.
        /// Ei heitä poikkeusta, jos tiedostoa ei ole olemassa.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi</param>
        public override void Delete( string fileName )
        {
            Initialize();
            MakeAbsolute( ref fileName );
            FMAssert( File.Delete, true, fileName );
        }
    }

    /// <summary>
    /// Usein käytettyjä polkuja Windowsissa.
    /// </summary>
    public static class WindowsLocation
    {
        /// <summary>
        /// Omat tiedostot.
        /// </summary>
        public static readonly string MyDocuments = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

        /// <summary>
        /// Oma musiikki.
        /// </summary>
        public static readonly string MyMusic = Environment.GetFolderPath( Environment.SpecialFolder.MyMusic );

        /// <summary>
        /// Omat kuvatiedostot.
        /// </summary>
        public static readonly string MyPictures = Environment.GetFolderPath( Environment.SpecialFolder.MyPictures );

        /// <summary>
        /// Omat videot.
        /// </summary>
        public static readonly string MyVideos = Environment.GetFolderPath( Environment.SpecialFolder.MyVideos );

        /// <summary>
        /// Ohjelman hakemisto.
        /// </summary>
        public static readonly string ExePath = Path.GetDirectoryName( Application.ExecutablePath );

        /// <summary>
        /// Ohjelman data-alihakemisto.
        /// </summary>
        public static readonly string DataPath = Path.Combine( Path.GetDirectoryName( Application.ExecutablePath ), "Data" );

        /// <summary>
        /// Ohjelman content-alihakemisto.
        /// </summary>
        public static readonly string ContentPath = Path.Combine( Path.GetDirectoryName( Application.ExecutablePath ), "Content" );
    }
}
