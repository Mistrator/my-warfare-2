using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jypeli.LevelEditor
{
    [Save]
    public class PropertySet
    {
        [Save] public List<Property> Items { get; private set; }

        public PropertySet()
        {
            Items = new List<Property>();
        }

        private void Add( string name, object value )
        {
            Items.Add( new Property( name, value ) );
        }

        public void Add( PropertySet propSet )
        {
            for ( int i = 0; i < propSet.Items.Count; i++ )
            {
                SetValue( propSet.Items[i].Name, propSet.Items[i].Value );
            }
        }

        public void Remove( string propName )
        {
            Remove( item => item.Name == propName );
        }

        private void Remove( Predicate<Property> pred )
        {
            for ( int i = Items.Count - 1; i >= 0; i-- )
            {
                if ( pred( Items[i] ) )
                    Items.RemoveAt( i );
            }
        }

        public int IndexOf( string propName )
        {
            for ( int i = 0; i < Items.Count; i++ )
            {
                if ( Items[i].Name == propName )
                    return i;
            }

            return -1;
        }

        public bool Contains( string propName )
        {
            return IndexOf( propName ) >= 0;
        }

        public object GetValue( string propName )
        {
            int propIndex = IndexOf( propName );
            if ( propIndex < 0 ) throw new KeyNotFoundException();
            return Items[propIndex].Value;
        }

        public T GetValue<T>( string propName )
        {
            return (T)GetValue( propName );
        }

        public object TryGetValue( string propName )
        {
            try
            {
                return GetValue( propName );
            }
            catch ( KeyNotFoundException )
            {
                return null;
            }
        }

        public void SetValue( string propName, object propValue )
        {
            int propIndex = IndexOf( propName );

            if ( propIndex >= 0 )
            {
                Property property = Items[propIndex];
                property.Value = propValue;
                Items[propIndex] = property;
            }
            else
                Add( propName, propValue );
        }

        public void Apply( ref GameObject obj )
        {
            Type type = obj.GetType();

            for ( int i = 0; i < Items.Count; i++ )
            {
                if ( Items[i].Name == "Type" )
                    continue;

                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty;
                PropertyInfo prop = type.GetProperty( Items[i].Name, flags );

                if ( prop != null )
                    prop.SetValue( obj, Items[i].Value, null );
            }
        }

        public static PropertySet Merge( params PropertySet[] sets )
        {
            PropertySet result = new PropertySet();

            for ( int i = 0; i < sets.Length; i++ )
            {
                result.Add( sets[i] );
            }

            return result;
        }
    }

    [Save]
    public struct Property
    {
        [Save] public string Name;
        [Save] public object Value;

        public Property( string name, object value )
        {
            Name = name;
            Value  = value;
        }
    }
}
