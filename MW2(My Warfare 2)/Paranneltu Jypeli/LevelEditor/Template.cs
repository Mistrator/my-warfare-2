using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Jypeli.LevelEditor
{
    [Save]
    public class Template
    {
        [Save] public string Name { get; set; }
        [Save] public PropertySet Properties { get; set; }

        public Template( string name )
        {
            this.Name = name;
            this.Properties = new PropertySet();
            this.Properties.SetValue( "Type", typeof( GameObject ) );
            this.Properties.SetValue( "ShapeString", "Triangle" );
            this.Properties.SetValue( "Size", new Vector( 10, 10 ) );
            this.Properties.SetValue( "Color", Color.LightGreen );
        }

        public Template()
            : this( "Default" )
        {
        }
    }
}
