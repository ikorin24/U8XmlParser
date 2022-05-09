#nullable enable
using System;

namespace U8Xml
{
    public interface IExternalDtdGetter
    {
        void GetDtd(ExternalDtdGetterState state);
    }

    internal sealed class DefaultExternalDtdGetter : IExternalDtdGetter
    {
        public static DefaultExternalDtdGetter Instance { get; } = new DefaultExternalDtdGetter();

        public void GetDtd(ExternalDtdGetterState state) => throw new NotSupportedException("External DTD is not supported by default.");
    }

    public unsafe readonly struct ExternalDtdGetterState : IEquatable<ExternalDtdGetterState>
    {
        private readonly IntPtr _ptr;    // ExternalDtdGetterState_*

        public readonly RawString Uri => ((ExternalDtdGetterState_*)_ptr)->Uri;
        public readonly RawString Body => ((ExternalDtdGetterState_*)_ptr)->Body;
        public readonly RawString PublicIdentifier => ((ExternalDtdGetterState_*)_ptr)->PublicIdentifier;
        public readonly ExternalDtdType DtdType => ((ExternalDtdGetterState_*)_ptr)->DtdType;

        internal ExternalDtdGetterState(ExternalDtdGetterState_* ptr)
        {
            _ptr = (IntPtr)ptr;
        }

        public override bool Equals(object? obj) => obj is ExternalDtdGetterState state && Equals(state);

        public bool Equals(ExternalDtdGetterState other) => _ptr.Equals(other._ptr);

        public override int GetHashCode() => _ptr.GetHashCode();
    }

    internal struct ExternalDtdGetterState_
    {
        public RawString Body;
        public RawString PublicIdentifier;
        public RawString Uri;
        public ExternalDtdType DtdType;
    }

    public enum ExternalDtdType : byte
    {
        Public = 0,
        System = 1,
    }
}
