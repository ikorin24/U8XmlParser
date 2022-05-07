#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace U8Xml.Internal
{
    [DebuggerDisplay("NodeStack[{Count}]")]
    [DebuggerTypeProxy(typeof(NodeStackDebuggerTypeProxy))]
    internal unsafe struct NodeStack : IDisposable
    {
        private XmlNode_** _ptr;
        private int _capacity;
        private int _count;

        public int Capacity => _capacity;

        public int Count => _count;

        public NodeStack(int capacity)
        {
            Debug.Assert(capacity >= 0);
            _ptr = (XmlNode_**)Marshal.AllocHGlobal(capacity * sizeof(XmlNode_*));
            AllocationSafety.Add(capacity * sizeof(XmlNode_*));
            _capacity = capacity;
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(XmlNode_* value)
        {
            if(_capacity == _count) {
                GrowUp();
            }
            Debug.Assert(_capacity > _count);
            _ptr[_count] = value;
            _count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode_* Pop()
        {
            if(_count == 0) { ThrowHelper.ThrowInvalidOperation("Stack has no items."); }
            _count--;
            return _ptr[_count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode_* Peek()
        {
            if(_count == 0) { ThrowHelper.ThrowInvalidOperation("Stack has no items."); }
            return _ptr[_count - 1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out XmlNode_* item)
        {
            if(_count == 0) {
                item = null;
                return false;
            }
            item = _ptr[_count - 1];
            return true;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)_ptr);
            AllocationSafety.Remove(_capacity * sizeof(XmlNode_*));
            _capacity = 0;
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]  // uncommon path, no inlining
        private void GrowUp()
        {
            var newCapacity = Math.Max(4, _capacity * 2);
            var ptr = (XmlNode_**)Marshal.AllocHGlobal(newCapacity * sizeof(XmlNode_*));
            AllocationSafety.Add(newCapacity * sizeof(XmlNode_*));
            try {
                SpanHelper.CreateSpan<IntPtr>(_ptr, _count).CopyTo(SpanHelper.CreateSpan<IntPtr>(ptr, newCapacity));
                Marshal.FreeHGlobal((IntPtr)_ptr);
                AllocationSafety.Remove(_capacity * sizeof(XmlNode_*));
                _ptr = ptr;
                _capacity = newCapacity;
            }
            catch {
                Marshal.FreeHGlobal((IntPtr)ptr);
                AllocationSafety.Remove(newCapacity * sizeof(XmlNode_*));
                throw;
            }
        }


        private class NodeStackDebuggerTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private NodeStack _entity;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public unsafe XmlNode_*[] Items
            {
                get
                {
                    var array = new XmlNode_*[_entity.Count];
                    for(int i = 0; i < array.Length; i++) {
                        array[i] = _entity._ptr[i];
                    }
                    return array;
                }
            }

            public NodeStackDebuggerTypeProxy(NodeStack entity)
            {
                _entity = entity;
            }
        }
    }
}
