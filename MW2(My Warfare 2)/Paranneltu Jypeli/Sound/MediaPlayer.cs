﻿using System;
using Microsoft.Xna.Framework.Content;

using XnaSong = Microsoft.Xna.Framework.Media.Song;
using System.Reflection;
using Microsoft.Xna.Framework.Media;

namespace Jypeli
{
    /// <summary>
    /// Mediasoitin, jolla voi soittaa musiikkikappaleita.
    /// </summary>
    public class MediaPlayer
    {
        private ContentManager content;

        /// <summary>
        /// Äänenvoimakkuus välillä 0.0 - 1.0.
        /// </summary>
        public double Volume
        {
            get { return Microsoft.Xna.Framework.Media.MediaPlayer.Volume; }
            set { Microsoft.Xna.Framework.Media.MediaPlayer.Volume = (float)value; }
        }

        /// <summary>
        /// Onko mediasoitin hiljaisella vai ei.
        /// </summary>
        public bool IsMuted
        {
            get { return Microsoft.Xna.Framework.Media.MediaPlayer.IsMuted; }
            set { Microsoft.Xna.Framework.Media.MediaPlayer.IsMuted = value; }
        }

        /// <summary>
        /// Toistetaanko kappaleita alusta.
        /// </summary>
        public bool IsRepeating
        {
            get { return Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating; }
            set { Microsoft.Xna.Framework.Media.MediaPlayer.IsRepeating = value; }
        }

        /// <summary>
        /// Soitetaanko tällä hetkellä mitään.
        /// </summary>
        public bool IsPlaying
        {
            get { return Microsoft.Xna.Framework.Media.MediaPlayer.State == MediaState.Playing; }
        }

        internal MediaPlayer( ContentManager content )
        {
            this.content = content;
        }

        /// <summary>
        /// Soittaa kappaleen.
        /// </summary>
        /// <param name="songName">Kappaleen nimi.</param>
        public void Play( string songName )
        {
            XnaSong song;
            try
            {
                song = content.Load<XnaSong>( songName );
            }
            catch ( ContentLoadException e )
            {
                throw new Exception(
                    "Could not play the song \"" + songName + "\".\n" +
                    "Please check that you added the song to the Content project and typed the name correctly.", e );
            }
            Microsoft.Xna.Framework.Media.MediaPlayer.Play( song );
        }

#if WINDOWS
        /// <summary>
        /// Soittaa kappaleen tiedostosta.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi.</param>
        public void PlayFromFile( string fileName )
        {
            // Use reflection to get the internal constructor of Song class
            // The public constructor does not take spaces well!
            var ctor = typeof( Song ).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { typeof( string ), typeof( string ), typeof( int ) }, null );

            Song song = (Song)ctor.Invoke( new object[] { "name", fileName, 0 } );
            Microsoft.Xna.Framework.Media.MediaPlayer.Play( song );
        }
#endif

        /// <summary>
        /// Soittaa kappaleen nettiosoitteesta.
        /// </summary>
        /// <param name="url">Nettiosoite (URL).</param>
        public void PlayFromURL( string url )
        {
            Song song = Song.FromUri( "song", new Uri( url ) );
            Microsoft.Xna.Framework.Media.MediaPlayer.Play( song );
        }

        /// <summary>
        /// Pysäyttää soiton.
        /// </summary>
        public void Pause()
        {
            Microsoft.Xna.Framework.Media.MediaPlayer.Pause();
        }

        /// <summary>
        /// Jatkaa pysäytettyä kappaletta.
        /// </summary>
        public void Resume()
        {
            Microsoft.Xna.Framework.Media.MediaPlayer.Resume();
        }

        /// <summary>
        /// Keskeyttää soiton.
        /// </summary>
        public void Stop()
        {
            Microsoft.Xna.Framework.Media.MediaPlayer.Stop();
        }
    }
}
