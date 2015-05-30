using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Jypeli.LevelEditor
{
    [Save]
    public class LevelData
    {
        [Save] public List<Template> Templates { get; private set; }
        [Save] public List<LevelObject> Objects { get; private set; }

        public LevelData()
        {
            Templates = new List<Template>();
            Objects = new List<LevelObject>();
        }

        private int GetTemplateIndex( Template template )
        {
            int index = Templates.IndexOf( template );
            if ( index >= 0 ) return index;

            Templates.Add( template );
            return Templates.Count - 1;
        }

        public LevelObject CreateObject( Template t )
        {
            LevelObject newObj = new LevelObject( this, GetTemplateIndex( t ) );
            Objects.Add( newObj );
            return newObj;
        }

        public void RemoveObject(LevelObject obj)
        {
            Objects.Remove( obj );
        }

        public void Clear()
        {
            Templates.Clear();
            Objects.Clear();
        }
    }
}
