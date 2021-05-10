#nullable enable
using System;
using System.Diagnostics;

namespace U8Xml.Internal
{
    internal static class AllocationSafety
    {
        [ThreadStatic]
        private static int _size;

        [Conditional("DEBUG")]
        public static void Add(int size)
        {
            _size += size;
        }

        [Conditional("DEBUG")]
        public static void Remove(int size)
        {
            _size -= size;
        }

        [Conditional("DEBUG")]
        public static void Ensure()
        {
            var size = _size;
            _size = 0;
            if(size != 0) {
                throw new Exception($"Memory leak happened or something wrong. ({_size} bytes)");
            }
        }
    }
}
