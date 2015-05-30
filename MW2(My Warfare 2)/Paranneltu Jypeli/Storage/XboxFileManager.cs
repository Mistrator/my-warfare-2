using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.Reflection;
using System.IO;

namespace Jypeli
{
    public class XboxFileManager : FileManager
    {
        delegate void AssertCallback( string fileName );

        StorageDevice device = null;
        StorageContainer container = null;

        public XboxFileManager()
        {
            StorageDevice.DeviceChanged += new EventHandler<EventArgs>( StorageDevice_DeviceChanged );
        }

        void StorageDevice_DeviceChanged( object sender, EventArgs e )
        {
            if ( device == null || device.IsConnected )
                return;

            device = null;
            container = null;
        }

        void AssertDevice( AssertCallback callback, string fileName )
        {
            if ( device == null )
            {
                AsyncCallback asCallback = delegate( IAsyncResult result )
                {
                    StorageDevice.EndShowSelector( result );
                    callback( fileName );
                };

                StorageDevice.BeginShowSelector( asCallback, null );
            }
            else
                callback( fileName );
        }

        void AssertContainer( AssertCallback callback, string fileName )
        {
            if ( container == null )
            {
                AsyncCallback asCallback = delegate( IAsyncResult result )
                {
                    device.EndOpenContainer( result );
                    callback( fileName );
                };

                device.BeginOpenContainer( Game.Name, asCallback, null );
            }
            else
                callback( fileName );
        }

        void AssertDevContainer( AssertCallback callback, string fileName )
        {
            if ( container != null )
                callback( fileName );
            else if ( device != null )
                AssertContainer( callback, fileName );
            else
                AssertDevice( delegate { AssertContainer( callback, fileName ); }, fileName );
        }

        void AssertDevContainerWait()
        {
            if ( device == null )
            {
                IAsyncResult result = StorageDevice.BeginShowSelector( null, null );
                result.AsyncWaitHandle.WaitOne();
            }

            if ( container == null )
            {
                IAsyncResult result = device.BeginOpenContainer( Game.Name, null, null );
                result.AsyncWaitHandle.WaitOne();
            }
        }

        public override bool Exists( string fileName )
        {
            MakeAbsolute(ref fileName);
            AssertDevContainerWait();
            return container.FileExists( fileName );
        }

        public override StorageFile Open( string fileName, bool write )
        {
            MakeAbsolute( ref fileName );
            AssertDevContainerWait();

            FileMode fileMode = write ? FileMode.Create : FileMode.Open;
            FileAccess fileAccess = write ? FileAccess.ReadWrite : FileAccess.Read;
            return new StorageFile( fileName, container.OpenFile( fileName, fileMode, fileAccess ) );
        }

        public override void Delete( string fileName )
        {
            MakeAbsolute( ref fileName );
            AssertDevContainer( doDeleteFile, fileName );
        }

        public override bool ChDir( string path )
        {
            MakeAbsolute( ref path );
            AssertDevContainer( doChangeDir, path );
            return true;
        }

        public override void MkDir( string path )
        {
            MakeAbsolute( ref path );
            AssertDevContainer( doMakeDir, path );
        }

        public override void RmDir( string path )
        {
            MakeAbsolute( ref path );
            AssertDevContainer( doRmDir, path );
        }

        public override IList<string> GetFileList()
        {
            AssertDevContainer( doMakeDir, _currentDir );
            string[] fileList = container.GetFileNames( _currentDir + "\\*" );
            return fileList.ToList<string>().AsReadOnly();
        }

        private void doDeleteFile( string fileName )
        {
            MakeAbsolute( ref fileName );
            container.DeleteFile( fileName );
        }

        private void doChangeDir( string path )
        {
            MakeAbsolute( ref path );

            if ( !container.DirectoryExists( path ) )
                throw new DirectoryNotFoundException( "Directory " + path + " was not found." );

            _currentDir = path;
        }

        private void doMakeDir( string path )
        {
            MakeAbsolute( ref path );
            container.CreateDirectory( path );
        }

        private void doRmDir( string path )
        {
            MakeAbsolute( ref path );
            container.DeleteDirectory( path );
        }
  }
}
