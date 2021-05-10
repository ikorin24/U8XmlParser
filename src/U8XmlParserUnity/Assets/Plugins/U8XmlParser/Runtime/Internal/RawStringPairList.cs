#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace U8Xml.Internal
{
    // [NOTE]
    // - This list re-allocate and copy memory when capacity increases, the address of memories can be changed.

    [DebuggerDisplay("{DebugDisplay,nq}")]
    [DebuggerTypeProxy(typeof(RawStringPairListTypeProxy))]
    internal unsafe ref struct RawStringPairList
    {
        private Pair* _ptr;
        private int _capacity;
        private int _count;

        public int Count => _count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugDisplay => $"{nameof(RawStringPairList)} (Count={_count})";

        public ref readonly Pair this[int index]
        {
            get
            {
                // Check index boundary only when DEBUG because this is internal.
                Debug.Assert((uint)index < (uint)_count);
                return ref _ptr[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in RawString key, in RawString value)
        {
            if(_capacity <= _count) {
                Growup();   // no inlining, uncommon path
            }
            Debug.Assert(_capacity > _count);
            ref var p = ref _ptr[_count++];
            p.Key = key;
            p.Value = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]      // no inlining, uncommon path
        private void Growup()
        {
            if(_capacity == 0) {
                const int InitialCapacity = 32;
                var size = InitialCapacity * sizeof(Pair);

                // It does not need to be cleared to zero.
                _ptr = (Pair*)Marshal.AllocHGlobal(size);
                AllocationSafety.Add(size);
                _capacity = InitialCapacity;
                _count = 0;
            }
            else {
                Debug.Assert(_capacity > 0);
                var newCapacity = _capacity * 2;
                var newSize = newCapacity * sizeof(Pair);
                var newPtr = (Pair*)Marshal.AllocHGlobal(newSize);
                AllocationSafety.Add(newSize);

                var sizeToCopy = _count * sizeof(Pair);
                Buffer.MemoryCopy(_ptr, newPtr, sizeToCopy, sizeToCopy);
                Marshal.FreeHGlobal((IntPtr)_ptr);
                AllocationSafety.Remove(_capacity * sizeof(Pair));
                _capacity = newCapacity;
                _ptr = newPtr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)_ptr);
            AllocationSafety.Remove(_capacity * sizeof(Pair));
            _capacity = 0;
            _count = 0;
        }

        [DebuggerDisplay("Key={Key}, Value={Value}")]
        public struct Pair
        {
            public RawString Key;
            public RawString Value;
        }

        private class RawStringPairListTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Pair[] _items;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Pair[] Items => _items;

            public RawStringPairListTypeProxy(RawStringPairList entity)
            {
                var items = new Pair[entity.Count];
                for(int i = 0; i < items.Length; i++) {
                    items[i] = entity[i];
                }
                _items = items;
            }
        }
    }
}
