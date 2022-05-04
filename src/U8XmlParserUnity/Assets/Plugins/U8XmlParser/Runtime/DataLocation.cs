#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("{DebugView,nq}")]
    public readonly struct DataLocation : IEquatable<DataLocation>
    {
        public readonly DataLinePosition Start;
        public readonly DataLinePosition End;
        public readonly DataRange Range;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugView => (Start == End) ? Start.DebugView : $"{Start.DebugView} - {End.DebugView}";

        public DataLocation(DataLinePosition start, DataLinePosition end, DataRange range)
        {
            Start = start;
            End = end;
            Range = range;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out DataLinePosition start, out DataLinePosition end, out DataRange range)
        {
            start = Start;
            end = End;
            range = Range;
        }

        public override bool Equals(object? obj) => obj is DataLocation location && Equals(location);

        public bool Equals(DataLocation other) => Start.Equals(other.Start) &&
                                                  End.Equals(other.End) &&
                                                  Range.Equals(other.Range);

        // Can not use System.HashCode because the project doesn't depend on Microsoft.Bcl.HashCode package. (on netstandard2.0 / net48)
        public override int GetHashCode() => XXHash32.ComputeHash(this);

        public static bool operator ==(in DataLocation left, in DataLocation right) => left.Equals(right);

        public static bool operator !=(in DataLocation left, in DataLocation right) => !(left == right);

        public override string ToString() => DebugView;
    }

    [DebuggerDisplay("{DebugView,nq}")]
    public readonly struct DataLinePosition : IEquatable<DataLinePosition>
    {
        /// <summary>Line number (zero-based numbering)</summary>
        public readonly int Line;
        /// <summary>Position offset which is number of characters, NOT number of bytes. (zero-based numbering)</summary>
        public readonly int Position;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebugView => $"(L.{Line}, {Position})";

        public DataLinePosition(int line, int position)
        {
            Line = line;
            Position = position;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int line, out int position)
        {
            line = Line;
            position = Position;
        }

        public override bool Equals(object? obj) => obj is DataLinePosition position && Equals(position);

        public bool Equals(DataLinePosition other) => Line == other.Line && Position == other.Position;

        // Can not use System.HashCode because the project doesn't depend on Microsoft.Bcl.HashCode package. (on netstandard2.0 / net48)
        public override int GetHashCode() => XXHash32.ComputeHash(this);

        public static bool operator ==(DataLinePosition left, DataLinePosition right) => left.Equals(right);

        public static bool operator !=(DataLinePosition left, DataLinePosition right) => !(left == right);

        public override string ToString() => DebugView;
    }

    public readonly struct DataRange : IEquatable<DataRange>
    {
        public readonly int Start;
        public readonly int Length;

        public DataRange(int byteOffset, int byteLength)
        {
            Start = byteOffset;
            Length = byteLength;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int byteOffset, out int byteLength)
        {
            byteOffset = Start;
            byteLength = Length;
        }

        public override bool Equals(object? obj) => obj is DataRange range && Equals(range);

        public bool Equals(DataRange other) => Start == other.Start && Length == other.Length;

        // Can not use System.HashCode because the project doesn't depend on Microsoft.Bcl.HashCode package. (on netstandard2.0 / net48)
        public override int GetHashCode() => XXHash32.ComputeHash(this);

        public static bool operator ==(DataRange left, DataRange right) => left.Equals(right);

        public static bool operator !=(DataRange left, DataRange right) => !(left == right);
    }
}