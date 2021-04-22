#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace U8Xml.Internal
{
    internal unsafe readonly struct OptionalNodeList : IDisposable
    {
        private readonly IntPtr _list;  // OptionalNodeList_*

        public XmlDeclaration_* Declaration => &((OptionalNodeList_*)_list)->Declaration;

        private OptionalNodeList(IntPtr list)
        {
            _list = list;
        }

        public static OptionalNodeList Create()
        {
            var ptr = (OptionalNodeList_*)Marshal.AllocHGlobal(sizeof(OptionalNodeList_));
            AllocationSafety.Add(sizeof(OptionalNodeList_));
            *ptr = default;
            return new OptionalNodeList((IntPtr)ptr);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_list);
            AllocationSafety.Remove(sizeof(OptionalNodeList_));
            Unsafe.AsRef(_list) = IntPtr.Zero;
        }


        private struct OptionalNodeList_
        {
            public XmlDeclaration_ Declaration;
        }
    }
}
