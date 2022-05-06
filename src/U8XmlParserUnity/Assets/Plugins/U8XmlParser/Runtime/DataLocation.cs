#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>Represents location of data</summary>
    [DebuggerDisplay("{DebugView,nq}")]
    public readonly struct DataLocation : IEquatable<DataLocation>
    {
        /// <summary>Start line number and character number of the data</summary>
        public readonly DataLinePosition Start;
        /// <summary>End line number and character number of the data</summary>
        public readonly DataLinePosition End;
        /// <summary>Data range of bytes</summary>
        public readonly DataRange Range;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugView => (Start == End) ? Start.DebugView : $"{Start.DebugView} - {End.DebugView}";

        /// <summary>Create location of data</summary>
        /// <param name="start">start line number and character number</param>
        /// <param name="end">end line number and character number</param>
        /// <param name="range">data range of bytes</param>
        public DataLocation(DataLinePosition start, DataLinePosition end, DataRange range)
        {
            Start = start;
            End = end;
            Range = range;
        }

        /// <summary>Deconstruct <see cref="DataLocation"/></summary>
        /// <param name="start">start line number and character number</param>
        /// <param name="end">end line number and character number</param>
        /// <param name="range">data range of bytes</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out DataLinePosition start, out DataLinePosition end, out DataRange range)
        {
            start = Start;
            end = End;
            range = Range;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is DataLocation location && Equals(location);

        public bool Equals(DataLocation other) => Start.Equals(other.Start) &&
                                                  End.Equals(other.End) &&
                                                  Range.Equals(other.Range);

        // Can not use System.HashCode because the project doesn't depend on Microsoft.Bcl.HashCode package. (on netstandard2.0 / net48)
        /// <inheritdoc/>
        public override int GetHashCode() => XXHash32.ComputeHash(this);

        public static bool operator ==(in DataLocation left, in DataLocation right) => left.Equals(right);

        public static bool operator !=(in DataLocation left, in DataLocation right) => !(left == right);

        /// <inheritdoc/>
        public override string ToString() => DebugView;
    }

    /// <summary>Represents line number and character number of data</summary>
    [DebuggerDisplay("{DebugView,nq}")]
    public readonly struct DataLinePosition : IEquatable<DataLinePosition>
    {
        /// <summary>Line number (zero-based numbering)</summary>
        public readonly int Line;
        /// <summary>Position offset which is number of characters, NOT number of bytes. (zero-based numbering)</summary>
        public readonly int Position;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string DebugView => $"(L.{Line}, {Position})";

        /// <summary>Create line position of data</summary>
        /// <param name="line">line number</param>
        /// <param name="position">character number in a line</param>
        public DataLinePosition(int line, int position)
        {
            Line = line;
            Position = position;
        }

        /// <summary>Deconstruct <see cref="DataLinePosition"/></summary>
        /// <param name="line">line number</param>
        /// <param name="position">character number in a line</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int line, out int position)
        {
            line = Line;
            position = Position;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is DataLinePosition position && Equals(position);

        public bool Equals(DataLinePosition other) => Line == other.Line && Position == other.Position;

        // Can not use System.HashCode because the project doesn't depend on Microsoft.Bcl.HashCode package. (on netstandard2.0 / net48)
        /// <inheritdoc/>
        public override int GetHashCode() => XXHash32.ComputeHash(this);

        public static bool operator ==(DataLinePosition left, DataLinePosition right) => left.Equals(right);

        public static bool operator !=(DataLinePosition left, DataLinePosition right) => !(left == right);

        /// <inheritdoc/>
        public override string ToString() => DebugView;
    }

    /// <summary>Represents bytes range of data</summary>
    [DebuggerDisplay("{DebugView,nq}")]
    public readonly struct DataRange : IEquatable<DataRange>
    {
        /// <summary>start offset in bytes</summary>
        public readonly int Start;
        /// <summary>length of bytes</summary>
        public readonly int Length;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugView => $"Start: {Start}, Length: {Length}";

        /// <summary>Create bytes range of data</summary>
        /// <param name="start">start offset in bytes</param>
        /// <param name="length">length of bytes</param>
        public DataRange(int start, int length)
        {
            Start = start;
            Length = length;
        }

        /// <summary>Deconstruct <see cref="DataRange"/></summary>
        /// <param name="start">start offset in bytes</param>
        /// <param name="length">length of bytes</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int start, out int length)
        {
            start = Start;
            length = Length;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is DataRange range && Equals(range);

        public bool Equals(DataRange other) => Start == other.Start && Length == other.Length;

        // Can not use System.HashCode because the project doesn't depend on Microsoft.Bcl.HashCode package. (on netstandard2.0 / net48)
        /// <inheritdoc/>
        public override int GetHashCode() => XXHash32.ComputeHash(this);

        public static bool operator ==(DataRange left, DataRange right) => left.Equals(right);

        public static bool operator !=(DataRange left, DataRange right) => !(left == right);

        /// <inheritdoc/>
        public override string ToString() => DebugView;
    }
}
