#nullable enable
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    public readonly struct Option<T> where T : unmanaged, IReference
    {
        private readonly T _v;

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(_v.IsNull) { ThrowHelper.ThrowInvalidOperation("No value exist."); }
                return _v;
            }
        }

        public bool HasValue => _v.IsNull == false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out T value)
        {
            value = _v;
            return _v.IsNull == false;
        }
    }
    

    public interface IReference
    {
        public bool IsNull { get; }
    }
}
