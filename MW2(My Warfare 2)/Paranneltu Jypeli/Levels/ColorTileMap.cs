using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Jypeli
{
    /// <summary>
    /// Ruutukartta, jonka avulla olioita voidaan helposti asettaa tasavälein ruudukkoon.
    /// Ruutukartta koostuu kirjoitusmerkeistä (<c>char</c>), joihin voi liittää
    /// aliohjelman, joka luo merkkiä vastaavan olion.
    /// </summary>
    public class ColorTileMap : AbstractTileMap<Color>
    {
        private double _tolerance = 30;

        protected override Color Null
        {
            get { return Color.Transparent; }
        }

        /// <summary>
        /// Väritoleranssi. Mitä pienempi toleranssi, sitä tarkemmin eri värit erotellaan toisistaan.
        /// Nollatoleranssilla värit on annettava tarkkoina rgb-koodeina, suuremmilla toleransseilla
        /// riittää "sinne päin".
        /// </summary>
        public double ColorTolerance
        {
            get { return _tolerance; }
            set
            {
                if (value < 0) throw new ArgumentException("Tolerance must not be negative.");
                _tolerance = value;
            }
        }
        
        /// <summary>
        /// Luo uuden ruutukartan.
        /// </summary>
        /// <param name="img">Kuva, jossa jokainen pikseli vastaa oliota.</param>
        public ColorTileMap( Image img )
            : base( img.GetData() )
        {
        }

        /// <summary>
        /// Lukee ruutukentän Content-projektin kuvatiedostosta.
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>        
        public static ColorTileMap FromLevelAsset( string assetName )
        {
            return new ColorTileMap( Game.LoadImage( assetName ) );
        }

        protected virtual bool ItemEquals( Color a, Color b )
        {
            return ( a.AlphaComponent == b.AlphaComponent && Color.Distance( a, b ) <= ColorTolerance );
        }
    }
}
