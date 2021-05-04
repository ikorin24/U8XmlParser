#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Option<T> : IEquatable<Option<T>> where T : unmanaged, IReference
    {
        private readonly T _v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option(in T v)
        {
            _v = v;
        }

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

        public override string ToString() => _v.IsNull ? "null" : (_v.ToString() ?? "");

        public override bool Equals(object? obj) => obj is Option<T> option && Equals(option);

        public bool Equals(Option<T> other) => EqualityComparer<T>.Default.Equals(_v, other._v);

        public bool Equals(in T other) => EqualityComparer<T>.Default.Equals(_v, other);

        public override int GetHashCode() => _v.GetHashCode();

        public static implicit operator Option<T>(in T value) => new Option<T>(value);
    }
    

    public interface IReference
    {
        public bool IsNull { get; }
    }
}
