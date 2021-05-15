#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    internal unsafe readonly struct CustomList<T> : IDisposable, IReference where T : unmanaged
    {
        private static readonly int[] _lineCapacity;
        private readonly IntPtr _p;     // ListInfo*

        private ListInfo* Info => (ListInfo*)_p;

        public bool IsNull => _p == IntPtr.Zero;

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
            var allocSize = ListInfo.GetSizeToAllocate();
            var listInfo = (ListInfo*)Marshal.AllocHGlobal(allocSize);
            AllocationSafety.Add(allocSize);
            listInfo->CurrentLineNum = -1;
            listInfo->Count = 0;
            for(int i = 0; i < ListInfo.BucketCount; i++) {
                (&listInfo->L0)[i] = null;      // must be null
            }
            NewLine(listInfo);
            return new CustomList<T>(listInfo);
        }

        public T* GetPointerToAdd()
        {
            Debug.Assert(Info != null);     // I don't check it because CustomList<T> is internal.
            if(Info->Count >= ListInfo.MaxItemCount) { ThrowHelper.ThrowInvalidOperation("Can not add any more."); }
            var currentLine = Info->CurrentLine;
            if(currentLine->Count == currentLine->Capacity) {
                currentLine = NewLine(Info);
            }
            var addr = &currentLine->FirstItem + currentLine->Count;
            currentLine->Count++;
            Info->Count++;
            return addr;
        }

        public void CopyItemsPointer(T** buf, int bufLen, int start, int length)
        {
            if(bufLen < length) { ThrowHelper.ThrowArg($"{nameof(bufLen)} is too short to copy to."); }
            var count = Count;
            if((uint)start >= (uint)count) { ThrowHelper.ThrowArgOutOfRange(nameof(start)); }
            if((uint)length > (uint)(count - start)) { ThrowHelper.ThrowArgOutOfRange(nameof(length)); }
            if(length == 0) { return; }

            var enumerator = GetEnumerator(start, length);
            var i = 0;
            while(enumerator.MoveNext()) {
                buf[i] = enumerator.Current;
            }
        }

        public T* At(int i)
        {
            // No check because CustomList<T> is internal.
            Debug.Assert(Info != null);
            Debug.Assert((uint)i < (uint)Count);
            var (lineNum, posInLine) = GetLineNumAndIndex(i);
            var line = (&Info->L0)[lineNum];
            return &(&line->FirstItem)[posInLine];
        }

        public void Dispose()
        {
            for(int i = 0; i < ListInfo.BucketCount; i++) {
                var ptr = (IntPtr)((&Info->L0)[i]);
                Marshal.FreeHGlobal(ptr);
#if DEBUG
                if(ptr != IntPtr.Zero) {
                    AllocationSafety.Remove(Line.GetLineSizeToAllocate(_lineCapacity[i]));
                }
#endif
            }
            Marshal.FreeHGlobal((IntPtr)Info);
            AllocationSafety.Remove(ListInfo.GetSizeToAllocate());
            Unsafe.AsRef(_p) = IntPtr.Zero;
        }

        public Enumerator GetEnumerator() => new Enumerator(Info, 0, Count);

        public Enumerator GetEnumerator(int start, int count) => new Enumerator(Info, start, count);

        private static (int lineNum, int index) GetLineNumAndIndex(int i)
        {
            // This formula is correct, but the following is the same. I confirmed that.
            //var l = Math.Log2(index / 16 + 1);

            var l = BitOperationHelper.Log2((uint)((i >> 4) + 1));
            var offset = (1 << (l + 4)) - 16;
            return (l, i - offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Line* NewLine(ListInfo* listInfo)
        {
            int* newLineNum = &listInfo->CurrentLineNum;
            *newLineNum = *newLineNum + 1;
            var lineCapacity = _lineCapacity[*newLineNum];
            var allocSize = Line.GetLineSizeToAllocate(lineCapacity);
            var line = (Line*)Marshal.AllocHGlobal(allocSize);
            AllocationSafety.Add(allocSize);
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

        internal struct Enumerator
        {
            private readonly CustomList<T>.ListInfo* _listInfo;
            private readonly int _count;
            private int _i;
            private int _lineNum;
            private CustomList<T>.Line* _line;
            private T* _current;
            private int _posInLine;

            internal Enumerator(CustomList<T>.ListInfo* listInfo, int start, int count)
            {
                _listInfo = listInfo;
                _count = count;
                _i = 0;
                (_lineNum, _posInLine) = GetLineNumAndIndex(start);
                _line = (&_listInfo->L0)[_lineNum];
                _current = null;
            }

            public T* Current => _current;

            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if(_i >= _count) { return false; }
                _current = &(&_line->FirstItem)[_posInLine];
                _posInLine++;
                _i++;
                if(_posInLine >= _line->Capacity) {
                    NewLine();
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void NewLine()
            {
                _posInLine = 0;
                _lineNum++;
                _line = (&_listInfo->L0)[_lineNum];
            }

            public void Reset() => throw new NotSupportedException();
        }
    }
}
