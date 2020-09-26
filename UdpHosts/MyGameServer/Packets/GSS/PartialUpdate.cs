using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS {
    [GSSMessage( (byte)Enums.GSS.Character.Events.PartialUpdate )]
    public class PartialUpdate {
        [Field]
        public IList<PartialView> Fields = new List<PartialView>();

        public void Add<T>(T data ) where T : class {
            Fields.Add( new PartialView<T> { ShadowFieldIndex = getIndex<T>(), Data = data } );
        }

        public T Get<T>() where T :class {
            return Get( typeof( T ) ) as T;
        }

        public object Get( Type t ) {
            var i = getIndex( t );
            return Fields.Where((pv) => pv.ShadowFieldIndex == i).FirstOrDefault()?.Data;
        }

        private byte getIndex<T>() {
            return getIndex( typeof( T ) );
        }

        private byte getIndex(Type t) {
            var attr = t.GetCustomAttributes( typeof( PartialShadowFieldAttribute ), false ).FirstOrDefault() as PartialShadowFieldAttribute;

            if( attr == null )
                throw new Exception( "Type " + t.FullName + " must have a PartialShadowField attribute." );

            return attr.ShadowFieldIndex;
        }

        public class PartialView {
            [Field]
            public byte ShadowFieldIndex;
            public object Data;
        }
        public class PartialView<T> : PartialView where T : class {
            [Field]
            public new T Data;
        }
        public class PartialShadowFieldAttribute : Attribute {
            public byte ShadowFieldIndex { get; }

            public PartialShadowFieldAttribute(byte index) {
                ShadowFieldIndex = index;
            }
        }
    }
}
