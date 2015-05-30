using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli.LevelEditor
{
    [Save]
    public class LevelObject
    {
        [Save] internal int TemplateIndex;
        [Save] public PropertySet OverridingProperties { get; private set; }

        private LevelData levelData;

        public Template Template
        {
            get { return levelData.Templates[TemplateIndex]; }
        }

        public LevelObject( LevelData levelData, int templateIndex )
        {
            this.levelData = levelData;
            this.TemplateIndex = templateIndex;
            this.OverridingProperties = new PropertySet();
        }

        public T GetPropertyValue<T>( string propName )
        {
            try
            {
                return (T)OverridingProperties.GetValue( propName );
            }
            catch ( KeyNotFoundException )
            {
                return (T)Template.Properties.GetValue( propName );
            }
        }

        public T GetPropertyValue<T>( string propName, T defaultValue )
        {
            try
            {
                return GetPropertyValue<T>( propName );
            }
            catch ( KeyNotFoundException )
            {
                return defaultValue;
            }
        }

        public GameObject ConstructObject()
        {
            //GameObject obj = new GameObject( 10, 10 );
            Type type = GetPropertyValue<Type>( "Type" );
            object[] args = { 10, 10 };
            GameObject obj = (GameObject)Activator.CreateInstance( type, args );
            Template.Properties.Apply( ref obj );
            OverridingProperties.Apply( ref obj );
            return (GameObject)obj;
        }

        public void ResetTemplate()
        {
            TemplateIndex = 0;
        }
    }
}
