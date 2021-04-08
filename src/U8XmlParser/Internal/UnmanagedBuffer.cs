#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace U8Xml.Internal
{
    internal unsafe readonly struct UnmanagedBuffer : IDisposable
    {
        private readonly IntPtr _ptr;    // byte*
        private readonly int _length;

        public bool IsEmpty => _length == 0;

        public int Length => _length;

        public IntPtr Ptr => _ptr;

        public UnmanagedBuffer(int length)
        {
            if(length == 0) {
                this = default;
                return;
            }
            _ptr = Marshal.AllocHGlobal(length);
            AllocationSafety.Add(length);
            _length = length;
        }

        public UnmanagedBuffer(ReadOnlySpan<byte> source)
        {
            if(source.IsEmpty) {
                this = default;
                return;
            }
            _ptr = Marshal.AllocHGlobal(source.Length);
            AllocationSafety.Add(source.Length);
            _length = source.Length;
            source.CopyTo(SpanHelper.CreateSpan<byte>(_ptr.ToPointer(), _length));
        }

        public void TransferMemoryOwnership(out IntPtr ptr, out int length)
        {
            ptr = _ptr;
            length = _length;
            Unsafe.AsRef(_ptr) = default;
            Unsafe.AsRef(_length) = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Marshal.FreeHGlobal(_ptr);
            AllocationSafety.Remove(_length);
            Unsafe.AsRef(_ptr) = default;
            Unsafe.AsRef(_length) = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => SpanHelper.CreateSpan<byte>(_ptr.ToPointer(), _length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start) => AsSpan().Slice(start);   // check boundary

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => AsSpan().Slice(start, length);   // check boundary
    }
}
