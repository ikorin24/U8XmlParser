#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace U8Xml
{
    public readonly ref struct SplitRawStrings
    {
        private readonly RawString _str;
        private readonly bool _isSeparatorSingleByte;
        private readonly byte _separator;
        private readonly ReadOnlySpan<byte> _spanSeparator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitRawStrings(RawString str, byte separator)
        {
            _str = str;
            _isSeparatorSingleByte = true;
            _separator = separator;
            _spanSeparator = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitRawStrings(RawString str, ReadOnlySpan<byte> separator)
        {
            _str = str;
            _isSeparatorSingleByte = false;
            _separator = default;
            _spanSeparator = separator;
        }

        public IEnumerable<RawString> AsEnumerable()
        {
            var list = new List<RawString>();
            foreach(var s in this) {
                list.Add(s);
            }
            return list;
        }

        public RawString[] ToArray()
        {
            return AsEnumerable().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => _isSeparatorSingleByte ? new Enumerator(_str, _separator) : new Enumerator(_str, _spanSeparator);

        public ref struct Enumerator
        {
            private RawString _str;
            private RawString _current;
            private readonly bool _isSeparatorChar;
            private readonly byte _separatorChar;
            private readonly ReadOnlySpan<byte> _separatorStr;

            public RawString Current => _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(RawString str, byte separator)
            {
                _str = str;
                _current = RawString.Empty;
                _isSeparatorChar = true;
                _separatorChar = separator;
                _separatorStr = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(RawString str, ReadOnlySpan<byte> separator)
            {
                _str = str;
                _current = RawString.Empty;
                _isSeparatorChar = false;
                _separatorChar = default;
                _separatorStr = separator;
            }

            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if(_str.IsEmpty) {
                    return false;
                }
                if(_isSeparatorChar) {
                    (_current, _str) = _str.Split2(_separatorChar);
                }
                else {
                    (_current, _str) = _str.Split2(_separatorStr);
                }
                return true;
            }
        }
    }
}
