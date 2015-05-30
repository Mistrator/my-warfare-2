using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public static class Factory
    {
        struct FactoryKey
        {
            public Type type;
            public string tag;

            public FactoryKey( Type type, string tag )
            {
                this.type = type;
                this.tag = tag;
            }
        }

        public delegate object FactoryMethod();
        static Dictionary<FactoryKey, FactoryMethod> constructors = new Dictionary<FactoryKey, FactoryMethod>();

        public static void AddFactory<T>( string tag, FactoryMethod method )
        {
            foreach ( var key in constructors.Keys.FindAll( k => k.type == typeof( T ) && k.tag == tag ) )
            {
                // Overwrite an existing method
                constructors[key] = method;
                return;
            }

            FactoryKey newKey = new FactoryKey( typeof( T ), tag );
            constructors.Add( newKey, method );
        }

        public static void RemoveFactory<T>( string tag, FactoryMethod method )
        {
            foreach ( var key in constructors.Keys.FindAll( k => k.type == typeof( T ) && k.tag == tag ) )
                constructors.Remove( key );
        }

        public static T FactoryCreate<T>( string tag )
        {
            return (T)FactoryCreate( typeof( T ), tag );
        }

        internal static object FactoryCreate( Type type, string tag )
        {
            foreach ( FactoryKey key in constructors.Keys )
            {
                if ( key.type == type && key.tag == tag )
                    return constructors[key].Invoke();
            }

            throw new KeyNotFoundException( "Key " + tag + " for class " + type.Name + " was not found." );
        }
    }
}
