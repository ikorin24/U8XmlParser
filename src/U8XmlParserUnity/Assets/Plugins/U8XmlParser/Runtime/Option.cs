#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>A type that represents a value that may exist.</summary>
    /// <typeparam name="T">type of the value</typeparam>
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct Option<T> : IEquatable<Option<T>> where T : unmanaged, IReference
    {
        private readonly T _v;

        /// <summary>Create the instance of <see cref="Option{T}"/>.</summary>
        /// <param name="v">original value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option(in T v)
        {
            _v = v;
        }

        /// <summary>Get the value if exists, or it throws <see cref="InvalidOperationException"/>.</summary>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(_v.IsNull) { ThrowHelper.ThrowInvalidOperation("No value exist."); }
                return _v;
            }
        }

        /// <summary>Get whether the value exists or not.</summary>
        public bool HasValue => _v.IsNull == false;

        /// <summary>Try to get a value if exists, or the method returns false.</summary>
        /// <param name="value">a value if exists. (Don't use it if the method returns false.)</param>
        /// <returns>succeed or not</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out T value)
        {
            value = _v;
            return _v.IsNull == false;
        }

        /// <inheritdoc/>
        public override string ToString() => _v.IsNull ? "null" : (_v.ToString() ?? "");

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Option<T> option && Equals(option);

        /// <summary>Returns whether the value is same as the specified instance.</summary>
        /// <param name="other">an instance to check</param>
        /// <returns>equal or not</returns>
        public bool Equals(Option<T> other) => EqualityComparer<T>.Default.Equals(_v, other._v);

        /// <summary>Returns whether the value is same as the specified instance.</summary>
        /// <param name="other">an instance to check</param>
        /// <returns>equal or not</returns>
        public bool Equals(in T other) => EqualityComparer<T>.Default.Equals(_v, other);

        /// <inheritdoc/>
        public override int GetHashCode() => _v.GetHashCode();

        /// <summary>implicit cast operation <typeparamref name="T"/> to <see cref="Option{T}"/></summary>
        /// <param name="value">a value to cast</param>
        public static implicit operator Option<T>(in T value) => new Option<T>(value);
    }


    public interface IReference
    {
        public bool IsNull { get; }
    }
}
