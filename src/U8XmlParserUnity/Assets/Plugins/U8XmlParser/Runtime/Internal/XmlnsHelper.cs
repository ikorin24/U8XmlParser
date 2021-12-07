#nullable enable
using System;
using System.Diagnostics;

namespace U8Xml.Internal
{
    internal static class XmlnsHelper
    {
        private const uint Bytes_xmln = (byte)'x' + ((byte)'m' << 8) + ((byte)'l' << 16) + ((byte)'n' << 24);
        private const uint Bytes_s_colon = (byte)'s' + ((byte)':' << 8);

        public unsafe static bool TryResolveNamespaceAlias(ReadOnlySpan<byte> nsName, XmlNode node, out RawString alias)
        {
            // xmlns:[alias]="[nsName]"
            // nsName -> alias

            var currentNode = node;
            while(true) {
                if(currentNode.HasXmlNamespaceAttr) {
                    foreach(var attr in currentNode.Attributes) {
                        var attrName = attr.Name;
                        if((attrName.Length >= 5) && (*(uint*)attrName.GetPtr() == Bytes_xmln)
                                                  && (attrName.At(4) == (byte)'s')) {
                            if(attrName.Length == 5 && attr.Value == nsName) {
                                // xmlns="[nsName]"
                                return Validate(RawString.Empty, nsName, node, out alias);
                            }
                            else if(attrName.Length >= 7 && attrName[5] == (byte)':' && attr.Value == nsName) {
                                // xmlns:[alias]="[nsName]"
                                return Validate(attrName.Slice(6), nsName, node, out alias);
                            }
                        }
                    }
                }
                if(currentNode.TryGetParent(out currentNode) == false) {
                    alias = RawString.Empty;
                    return false;
                }
            }

            static bool Validate(RawString aliasToValidate, ReadOnlySpan<byte> nsName, XmlNode targetNode, out RawString alias)
            {
                if(TryFindXmlnsRecursively(targetNode, aliasToValidate.AsSpan(), out var nsn)) {
                    if(nsn == nsName) {
                        alias = aliasToValidate;
                        return true;
                    }
                    else {
                        alias = RawString.Empty;
                        return false;
                    }
                }
                else {
                    Debug.Fail("Why are you getting here?");
                    alias = RawString.Empty;
                    return false;
                }
            }
        }

        private static bool TryFindXmlnsRecursively(XmlNode target, ReadOnlySpan<byte> alias, out RawString nsName)
        {
            // xmlns:[alias]="[nsName]"
            // alias -> nsName (recursive)

            var current = target;

            while(true) {
                if(TryFindXmlns(current, alias, out var nsn)) {
                    nsName = nsn;
                    return true;
                }

                if(current.TryGetParent(out current) == false) {
                    nsName = RawString.Empty;
                    return false;
                }
            }
        }

        private unsafe static bool TryFindXmlns(XmlNode target, ReadOnlySpan<byte> alias, out RawString nsName)
        {
            // xmlns:[alias]="[nsName]"
            // alias -> nsName (not recursive)

            if(alias.IsEmpty) {
                foreach(var attr in target.Attributes) {
                    var attrName = attr.Name;
                    if((attrName.Length == 5) && (*(uint*)attrName.GetPtr() == Bytes_xmln)
                                              && (attrName.At(4) == (byte)'s')) {
                        nsName = attr.Value;
                        return true;
                    }
                }
            }
            else {
                foreach(var attr in target.Attributes) {
                    var attrName = attr.Name;
                    if((attrName.Length >= 7) && (*(uint*)attrName.GetPtr() == Bytes_xmln)
                                              && (*(ushort*)(attrName.GetPtr() + 4) == Bytes_s_colon)
                                              && attrName.Slice(6) == alias) {
                        nsName = attr.Value;
                        return true;
                    }
                }
            }
            nsName = RawString.Empty;
            return false;
        }
    }
}
