#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace U8Xml.Internal
{
    // [NOTE]
    // - This list re-allocate and copy memory when capacity increases, the address of memories can be changed.

    internal unsafe ref struct RawStringList
    {
        private RawString* _ptr;
        private int _capacity;
        private int _count;

        public int Count => _count;

        public ref readonly RawString this[int index]
        {
            get
            {
                // Check index boundary only when DEBUG because this is internal.
                Debug.Assert((uint)index < (uint)_count);
                return ref _ptr[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in RawString item)
        {
            if(_capacity <= _count) {
                Growup();   // no inlining, uncommon path
            }
            Debug.Assert(_capacity > _count);
            _ptr[_count++] = item;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]      // no inlining, uncommon path
        private void Growup()
        {
            if(_capacity == 0) {
                const int InitialCapacity = 64;
                var size = InitialCapacity * sizeof(RawString);

                // It does not need to be cleared to zero.
                _ptr = (RawString*)Marshal.AllocHGlobal(size);
                AllocationSafety.Add(size);
                _capacity = InitialCapacity;
                _count = 0;
            }
            else {
                Debug.Assert(_capacity > 0);
                var newCapacity = _capacity * 2;
                var newSize = newCapacity * sizeof(RawString);
                var newPtr = (RawString*)Marshal.AllocHGlobal(newSize);
                AllocationSafety.Add(newSize);

                var sizeToCopy = _count * sizeof(RawString);
                Buffer.MemoryCopy(_ptr, newPtr, sizeToCopy, sizeToCopy);
                Marshal.FreeHGlobal((IntPtr)_ptr);
                AllocationSafety.Remove(_capacity * sizeof(RawString));
                _capacity = newCapacity;
                _ptr = newPtr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)_ptr);
            AllocationSafety.Remove(_capacity * sizeof(RawString));
            _capacity = 0;
            _count = 0;
        }
    }
}
