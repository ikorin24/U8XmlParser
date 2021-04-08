#nullable enable
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace U8Xml.Internal
{
    // [NOTE]
    // - Only provides 'Add an item' and 'Dispose the list'.
    // - Address of the items are always fixed even if the list extends.
    // - It is not supported to get items. You can access the item from the address which 'Add' method returns.
    // - Max items count is 2^31 - 16.

    //  CustomList<T>
    // +------------+
    // |  ListInfo* |
    // |    Info    |    ListInfo
    // +-----|------+   +-----------+-----------------+--------------+----------+----------+-----+---------+
    //       └-------->|    int    |       int       |     Line*    |   Line*  |   Line*  | ... |  Line*  |
    //                  |   Count   |  CurrentLineNum |  CurrentLine |    L0    |    L1    | ... |   L26   |
    //                  +-----------+-----------------+--------------+----|-----+----|-----+-----+---------+
    //                                                                    |          |
    //                     ┌---------------------------------------------┘         |
    //                     |     Line (Capacity = 16)                                |
    //                     |    +-----------+---------+-------+-----+--------+       |
    //                     └-->|    int    |   int   |   T   | ... |   T    |       |
    //                          |  Capacity |  Count  |  [0]  | ... |  [15]  |       |
    //                          +-----------+---------+-------+-----+--------+       |
    //                                                                               |
    //                    ┌---------------------------------------------------------┘
    //                    |     Line (Capacity = 32)
    //                    |     +-----------+---------+-------+-------------+--------+
    //                    └--->|    int    |   int   |   T   | ........... |   T    |
    //                          |  Capacity |  Count  |  [0]  | ........... |  [31]  |
    //                          +-----------+---------+-------+-------------+--------+

    internal unsafe readonly struct CustomList<T> : IEnumerable<T>, IDisposable where T : unmanaged
    {
        private static readonly int[] _lineCapacity;
        private readonly IntPtr _p;     // ListInfo*

        private ListInfo* Info => (ListInfo*)_p;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Info != null ? Info->Count : 0;
        }

        public bool IsDisposed => _p == IntPtr.Zero;

        public T* FirstItem
        {
            get
            {
                var l0 = Info->L0;
                return l0 != null ? &l0->FirstItem : null;
            }
        }

        static CustomList()
        {
            _lineCapacity = new int[ListInfo.BucketCount];
            for(int i = 0; i < _lineCapacity.Length; i++) {
                _lineCapacity[i] = (1 << (i + 4));
            }
        }

        private CustomList(ListInfo* listInfo)
        {
            _p = (IntPtr)listInfo;
        }

        public static CustomList<T> Create()
        {
            var listInfo = (ListInfo*)Marshal.AllocHGlobal(ListInfo.GetSizeToAllocate());
            listInfo->CurrentLineNum = -1;
            listInfo->Count = 0;
            for(int i = 0; i < ListInfo.BucketCount; i++) {
                (&listInfo->L0)[i] = null;      // must be null
            }
            NewLine(listInfo);
            return new CustomList<T>(listInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Add(T item)
        {
            Debug.Assert(Info != null);     // I don't check it because CustomList<T> is internal.
            if(Info->Count >= ListInfo.MaxItemCount) { ThrowHelper.ThrowInvalidOperation("Can not add any more."); }
            var currentLine = Info->CurrentLine;
            if(currentLine->Count == currentLine->Capacity) {
                currentLine = NewLine(Info);
            }
            var addr = &currentLine->FirstItem + currentLine->Count;
            *addr = item;
            currentLine->Count++;
            Info->Count++;
            return addr;
        }

        public void Dispose()
        {
            for(int i = 0; i < ListInfo.BucketCount; i++) {
                Marshal.FreeHGlobal((IntPtr)((&Info->L0)[i]));
            }
            Marshal.FreeHGlobal((IntPtr)Info);
            Unsafe.AsRef(_p) = IntPtr.Zero;
        }

        public CustomListEnumerator<T> GetEnumerator() => new CustomListEnumerator<T>(Info);

        internal CustomListEnumeratorClass<T> GetEnumeratorClass() => new CustomListEnumeratorClass<T>(Info);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new CustomListEnumeratorClass<T>(Info);

        IEnumerator IEnumerable.GetEnumerator() => new CustomListEnumeratorClass<T>(Info);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Line* NewLine(ListInfo* listInfo)
        {
            int* newLineNum = &listInfo->CurrentLineNum;
            *newLineNum = *newLineNum + 1;
            var lineCapacity = _lineCapacity[*newLineNum];
            var line = (Line*)Marshal.AllocHGlobal(Line.GetLineSizeToAllocate(lineCapacity));
            line->Count = 0;
            line->Capacity = lineCapacity;

            listInfo->CurrentLine = line;
            (&listInfo->L0)[*newLineNum] = line;
            return line;
        }

        internal struct ListInfo
        {
            public const int BucketCount = 27;
            public const int MaxItemCount = (int)((1L << 31) - 16); // 2^31 - 16

            public int Count;
            public int CurrentLineNum;
            public Line* CurrentLine;

            public Line* L0;   // capacity = 2^4 = 16

            public static int GetSizeToAllocate()
            {
                return sizeof(int) + sizeof(int) + sizeof(Line*) + sizeof(Line*) * BucketCount;
            }
        }

        internal struct Line
        {
            public int Capacity;
            public int Count;
            public T FirstItem;

            public static int GetLineSizeToAllocate(int capacity)
            {
                return sizeof(int) + sizeof(int) + sizeof(T) * capacity;
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe struct CustomListEnumerator<T> : IEnumerator<T> where T : unmanaged
    {
        private readonly CustomList<T>.ListInfo* _listInfo;
        private readonly int _count;
        private int _lineNum;
        private CustomList<T>.Line* _line;
        private int _offset;
        private int _pos;
        private int _i;

        public T Current => (&_line->FirstItem)[_pos];

        object IEnumerator.Current => Current;

        internal CustomListEnumerator(CustomList<T>.ListInfo* listInfo)
        {
            _listInfo = listInfo;
            _count = listInfo->Count;
            _lineNum = 0;
            _line = _listInfo->L0;
            _offset = 0;
            _pos = 0;
            _i = 0;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if(_i >= _count) { return false; }
            _pos = _i - _offset;
            if(_pos >= _line->Capacity) {
                NextLine();
            }
            _i++;
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void NextLine()
        {
            _offset = _i;
            _lineNum++;
            _line = (&_listInfo->L0)[_lineNum];
            _pos = _i - _offset;
        }

        public void Reset()
        {
            _lineNum = 0;
            _line = _listInfo->L0;
            _offset = 0;
            _pos = 0;
            _i = 0;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed unsafe class CustomListEnumeratorClass<T> : IEnumerator<T> where T : unmanaged
    {
        private readonly CustomList<T>.ListInfo* _listInfo;
        private readonly int _count;
        private int _lineNum;
        private CustomList<T>.Line* _line;
        private int _offset;
        private int _pos;
        private int _i;

        public T Current => (&_line->FirstItem)[_pos];

        object IEnumerator.Current => Current;

        internal CustomListEnumeratorClass(CustomList<T>.ListInfo* listInfo)
        {
            _listInfo = listInfo;
            _count = listInfo->Count;
            _lineNum = 0;
            _line = _listInfo->L0;
            _offset = 0;
            _pos = 0;
            _i = 0;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if(_i >= _count) { return false; }
            _pos = _i - _offset;
            if(_pos >= _line->Capacity) {
                NextLine();
            }
            _i++;
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void NextLine()
        {
            _offset = _i;
            _lineNum++;
            _line = (&_listInfo->L0)[_lineNum];
            _pos = _i - _offset;
        }

        public void Reset()
        {
            _lineNum = 0;
            _line = _listInfo->L0;
            _offset = 0;
            _pos = 0;
            _i = 0;
        }
    }
}
