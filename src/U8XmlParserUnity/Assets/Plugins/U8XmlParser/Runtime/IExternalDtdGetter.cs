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

    public readonly struct ExternalDtdGetterState
    {
        private readonly RawString _uri;
        private readonly XmlDtdParser _dtdParser;

        public readonly RawString Uri => _uri;
        public readonly XmlDtdParser DtdParser => _dtdParser;
    }
}
