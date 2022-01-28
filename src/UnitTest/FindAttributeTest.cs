#nullable enable
using System;
using System.Text;
using U8Xml;
using U8Xml.Internal;
using Xunit;

namespace UnitTest
{
    public class FindAttributeTest
    {
        private const string SampleXml1 =
@"<foo xmlns='test_a' xmlns:b='test_b'>
    <bar b:hoge='1' hoge='2'/>
    <hoge piyo='3' xmlns='test_a2'>
        <baz b:abc='4' xyz='5'/>
    </hoge>
    <aaa>
        <bbb>
            <ccc b:aaaa='6'/>
        </bbb>
    </aaa>
</foo>";

        [Fact]
        public unsafe void Test_IsName()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            var testCases = new[]
            {
                ("test_b", "hoge", root.FindChild("bar").FindAttribute("test_b", "hoge")),
                ("test_a", "hoge", root.FindChild("bar").FindAttribute("test_a", "hoge")),
                ("test_a2", "piyo", root.FindChild("hoge").FindAttribute("test_a2", "piyo")),
                ("test_b", "abc", root.FindChild("hoge").FindChild("baz").FindAttribute("test_b", "abc")),
                ("test_a2", "xyz", root.FindChild("hoge").FindChild("baz").FindAttribute("test_a2", "xyz")),
                ("test_b", "aaaa", root.FindChild("aaa").FindChild("bbb").FindChild("ccc").FindAttribute("test_b", "aaaa")),
            };

            foreach(var (nsName, name, attr) in testCases) {
                attr.IsName(nsName, name).ShouldBe(true);
                attr.IsName(nsName.AsSpan(), name).ShouldBe(true);
                attr.IsName(nsName.AsSpan(), name.AsSpan()).ShouldBe(true);
                attr.IsName(nsName, name.AsSpan()).ShouldBe(true);

                var nsName_ROSbyte = UTF8ExceptionFallbackEncoding.Instance.GetBytes(nsName).AsSpan();
                var name_ROSbyte = UTF8ExceptionFallbackEncoding.Instance.GetBytes(name).AsSpan();
                fixed(byte* p = nsName_ROSbyte)
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    attr.IsName(nsName_ROSbyte, name_ROSbyte).ShouldBe(true);
                    attr.IsName(nsName_RS, name_ROSbyte).ShouldBe(true);
                    attr.IsName(nsName_ROSbyte, name_RS).ShouldBe(true);
                    attr.IsName(nsName_RS, name_RS).ShouldBe(true);
                }

                {
                    attr.TryGetFullName(out var nsNameResolved, out var nameResolved).ShouldBe(true);
                    (nsNameResolved == nsName).ShouldBe(true);
                    (nameResolved == name).ShouldBe(true);
                }
                {
                    var (nsNameResolved, nameResolved) = attr.GetFullName();
                    (nsNameResolved == nsName).ShouldBe(true);
                    (nameResolved == name).ShouldBe(true);
                }
            }
        }

        [Fact]
        public void Test_FindAttribute()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var foo = xml.Root;
            var bar = foo.FindChild("bar");

            CheckInnerValue_FindAttribute(bar, "test_b", "hoge", 1);
            CheckInnerValue_FindAttribute(bar, "test_a", "hoge", 2);

            var hoge = foo.FindChild("hoge");
            CheckInnerValue_FindAttribute(hoge, "test_a2", "piyo", 3);
            CheckInnerValue_FindAttribute(hoge, "piyo", 3);

            var baz = hoge.FindChild("baz");
            CheckInnerValue_FindAttribute(baz, "test_b", "abc", 4);
            CheckInnerValue_FindAttribute(baz, "test_a2", "xyz", 5);
            CheckInnerValue_FindAttribute(baz, "xyz", 5);

            var ccc = foo.FindChild("aaa").FindChild("bbb").FindChild("ccc");
            CheckInnerValue_FindAttribute(ccc, "test_b", "aaaa", 6);

            Throw_FindAttribtue<InvalidOperationException>(hoge, "test_a", "piyo");
        }

        [Fact]
        public void Test_TryFindAttribute()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var foo = xml.Root;
            var bar = foo.FindChild("bar");

            CheckInnerValue_TryFindAttribute(bar, "test_b", "hoge", 1);
            CheckInnerValue_TryFindAttribute(bar, "test_a", "hoge", 2);

            var hoge = foo.FindChild("hoge");
            CheckInnerValue_TryFindAttribute(hoge, "test_a2", "piyo", 3);
            CheckInnerValue_TryFindAttribute(hoge, "piyo", 3);

            var baz = hoge.FindChild("baz");
            CheckInnerValue_TryFindAttribute(baz, "test_b", "abc", 4);
            CheckInnerValue_TryFindAttribute(baz, "test_a2", "xyz", 5);
            CheckInnerValue_TryFindAttribute(baz, "xyz", 5);

            var ccc = foo.FindChild("aaa").FindChild("bbb").FindChild("ccc");
            CheckInnerValue_TryFindAttribute(ccc, "test_b", "aaaa", 6);

            NotFound_TryFindAttribtue(hoge, "test_a", "piyo");
        }

        [Fact]
        public void Test_FindAttributeOrDefault()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var foo = xml.Root;
            var bar = foo.FindChild("bar");

            CheckInnerValue_FindAttributeOrDefault(bar, "test_b", "hoge", 1);
            CheckInnerValue_FindAttributeOrDefault(bar, "test_a", "hoge", 2);

            var hoge = foo.FindChild("hoge");
            CheckInnerValue_FindAttributeOrDefault(hoge, "test_a2", "piyo", 3);
            CheckInnerValue_FindAttributeOrDefault(hoge, "piyo", 3);

            var baz = hoge.FindChild("baz");
            CheckInnerValue_FindAttributeOrDefault(baz, "test_b", "abc", 4);
            CheckInnerValue_FindAttributeOrDefault(baz, "test_a2", "xyz", 5);
            CheckInnerValue_FindAttributeOrDefault(baz, "xyz", 5);

            var ccc = foo.FindChild("aaa").FindChild("bbb").FindChild("ccc");
            CheckInnerValue_FindAttributeOrDefault(ccc, "test_b", "aaaa", 6);

            NotFound_FindAttribtueOrDefault(hoge, "test_a", "piyo");
        }

        private unsafe static void CheckInnerValue_FindAttribute(XmlNode target, string? name, int value)
        {
            target.FindAttribute(name!).Value.ToInt32().ShouldBe(value);
            target.FindAttribute(name.AsSpan()).Value.ToInt32().ShouldBe(value);

            target.Attributes.Find(name!).Value.ToInt32().ShouldBe(value);
            target.Attributes.Find(name.AsSpan()).Value.ToInt32().ShouldBe(value);

            var name_ROSbyte = UTF8ExceptionFallbackEncoding.Instance.GetBytes(name ?? "");
            fixed(byte* p = name_ROSbyte) {
                var name_RS = new RawString(p, name_ROSbyte.Length);

                target.FindAttribute(name_ROSbyte).Value.ToInt32().ShouldBe(value);
                target.FindAttribute(name_RS).Value.ToInt32().ShouldBe(value);

                target.Attributes.Find(name_ROSbyte).Value.ToInt32().ShouldBe(value);
                target.Attributes.Find(name_RS).Value.ToInt32().ShouldBe(value);
            }
        }

        private unsafe static void CheckInnerValue_FindAttribute(XmlNode target, string? nsName, string? name, int value)
        {
            var attrName = new AttrName(nsName, name);

            target.FindAttribute(attrName.NsName!, attrName.Name!).Value.ToInt32().ShouldBe(value);
            target.FindAttribute(attrName.NsName!, attrName.Name_ROSchar).Value.ToInt32().ShouldBe(value);
            target.FindAttribute(attrName.NsName_ROSchar, attrName.Name!).Value.ToInt32().ShouldBe(value);
            target.FindAttribute(attrName.NsName_ROSchar, attrName.Name_ROSchar).Value.ToInt32().ShouldBe(value);

            target.Attributes.Find(attrName.NsName!, attrName.Name!).Value.ToInt32().ShouldBe(value);
            target.Attributes.Find(attrName.NsName!, attrName.Name_ROSchar).Value.ToInt32().ShouldBe(value);
            target.Attributes.Find(attrName.NsName_ROSchar, attrName.Name!).Value.ToInt32().ShouldBe(value);
            target.Attributes.Find(attrName.NsName_ROSchar, attrName.Name_ROSchar).Value.ToInt32().ShouldBe(value);

            var nsName_ROSbyte = attrName.NsName_ROSbyte;
            var name_ROSbyte = attrName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.FindAttribute(nsName_ROSbyte, name_ROSbyte).Value.ToInt32().ShouldBe(value);
                    target.FindAttribute(nsName_ROSbyte, name_RS).Value.ToInt32().ShouldBe(value);
                    target.FindAttribute(nsName_RS, name_ROSbyte).Value.ToInt32().ShouldBe(value);
                    target.FindAttribute(nsName_RS, name_RS).Value.ToInt32().ShouldBe(value);

                    target.Attributes.Find(nsName_ROSbyte, name_ROSbyte).Value.ToInt32().ShouldBe(value);
                    target.Attributes.Find(nsName_ROSbyte, name_RS).Value.ToInt32().ShouldBe(value);
                    target.Attributes.Find(nsName_RS, name_ROSbyte).Value.ToInt32().ShouldBe(value);
                    target.Attributes.Find(nsName_RS, name_RS).Value.ToInt32().ShouldBe(value);
                }
            }
        }

        private unsafe static void CheckInnerValue_TryFindAttribute(XmlNode target, string? name, int value)
        {
            {
                target.TryFindAttribute(name!, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.TryFindAttribute(name.AsSpan(), out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }

            {
                target.Attributes.TryFind(name!, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.Attributes.TryFind(name.AsSpan(), out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }

            var name_ROSbyte = UTF8ExceptionFallbackEncoding.Instance.GetBytes(name ?? "");
            fixed(byte* p = name_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var name_RS = new RawString(p, name_ROSbyte.Length);

                    {
                        target.TryFindAttribute(name_ROSbyte, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.TryFindAttribute(name_RS, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }

                    {
                        target.Attributes.TryFind(name_ROSbyte, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.Attributes.TryFind(name_RS, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                }
            }
        }

        private unsafe static void CheckInnerValue_TryFindAttribute(XmlNode target, string? nsName, string? name, int value)
        {
            var attrName = new AttrName(nsName, name);

            {
                target.TryFindAttribute(attrName.NsName!, attrName.Name!, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.TryFindAttribute(attrName.NsName!, attrName.Name_ROSchar, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.TryFindAttribute(attrName.NsName_ROSchar, attrName.Name!, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.TryFindAttribute(attrName.NsName_ROSchar, attrName.Name_ROSchar, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }

            {
                target.Attributes.TryFind(attrName.NsName!, attrName.Name!, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.Attributes.TryFind(attrName.NsName!, attrName.Name_ROSchar, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.Attributes.TryFind(attrName.NsName_ROSchar, attrName.Name!, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }
            {
                target.Attributes.TryFind(attrName.NsName_ROSchar, attrName.Name_ROSchar, out var attr).ShouldBe(true);
                attr.Value.ToInt32().ShouldBe(value);
            }

            var nsName_ROSbyte = attrName.NsName_ROSbyte;
            var name_ROSbyte = attrName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    {
                        target.TryFindAttribute(nsName_ROSbyte, name_ROSbyte, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.TryFindAttribute(nsName_ROSbyte, name_RS, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.TryFindAttribute(nsName_RS, name_ROSbyte, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.TryFindAttribute(nsName_RS, name_RS, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }

                    {
                        target.Attributes.TryFind(nsName_ROSbyte, name_ROSbyte, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.Attributes.TryFind(nsName_ROSbyte, name_RS, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.Attributes.TryFind(nsName_RS, name_ROSbyte, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                    {
                        target.Attributes.TryFind(nsName_RS, name_RS, out var attr).ShouldBe(true);
                        attr.Value.ToInt32().ShouldBe(value);
                    }
                }
            }
        }

        private unsafe static void CheckInnerValue_FindAttributeOrDefault(XmlNode target, string? nsName, string? name, int value)
        {
            var attrName = new AttrName(nsName, name);

            target.FindAttributeOrDefault(attrName.NsName!, attrName.Name!).Value.Value.ToInt32().ShouldBe(value);
            target.FindAttributeOrDefault(attrName.NsName!, attrName.Name_ROSchar).Value.Value.ToInt32().ShouldBe(value);
            target.FindAttributeOrDefault(attrName.NsName_ROSchar, attrName.Name!).Value.Value.ToInt32().ShouldBe(value);
            target.FindAttributeOrDefault(attrName.NsName_ROSchar, attrName.Name_ROSchar).Value.Value.ToInt32().ShouldBe(value);

            target.Attributes.FindOrDefault(attrName.NsName!, attrName.Name!).Value.Value.ToInt32().ShouldBe(value);
            target.Attributes.FindOrDefault(attrName.NsName!, attrName.Name_ROSchar).Value.Value.ToInt32().ShouldBe(value);
            target.Attributes.FindOrDefault(attrName.NsName_ROSchar, attrName.Name!).Value.Value.ToInt32().ShouldBe(value);
            target.Attributes.FindOrDefault(attrName.NsName_ROSchar, attrName.Name_ROSchar).Value.Value.ToInt32().ShouldBe(value);

            var nsName_ROSbyte = attrName.NsName_ROSbyte;
            var name_ROSbyte = attrName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.FindAttributeOrDefault(nsName_ROSbyte, name_ROSbyte).Value.Value.ToInt32().ShouldBe(value);
                    target.FindAttributeOrDefault(nsName_ROSbyte, name_RS).Value.Value.ToInt32().ShouldBe(value);
                    target.FindAttributeOrDefault(nsName_RS, name_ROSbyte).Value.Value.ToInt32().ShouldBe(value);
                    target.FindAttributeOrDefault(nsName_RS, name_RS).Value.Value.ToInt32().ShouldBe(value);

                    target.Attributes.FindOrDefault(nsName_ROSbyte, name_ROSbyte).Value.Value.ToInt32().ShouldBe(value);
                    target.Attributes.FindOrDefault(nsName_ROSbyte, name_RS).Value.Value.ToInt32().ShouldBe(value);
                    target.Attributes.FindOrDefault(nsName_RS, name_ROSbyte).Value.Value.ToInt32().ShouldBe(value);
                    target.Attributes.FindOrDefault(nsName_RS, name_RS).Value.Value.ToInt32().ShouldBe(value);
                }
            }
        }

        private unsafe static void CheckInnerValue_FindAttributeOrDefault(XmlNode target, string? name, int value)
        {
            target.FindAttributeOrDefault(name!).Value.Value.ToInt32().ShouldBe(value);
            target.FindAttributeOrDefault(name.AsSpan()).Value.Value.ToInt32().ShouldBe(value);

            target.Attributes.FindOrDefault(name!).Value.Value.ToInt32().ShouldBe(value);
            target.Attributes.FindOrDefault(name.AsSpan()).Value.Value.ToInt32().ShouldBe(value);

            var name_ROSbyte = UTF8ExceptionFallbackEncoding.Instance.GetBytes(name ?? "");
            fixed(byte* p = name_ROSbyte) {
                var name_RS = new RawString(p, name_ROSbyte.Length);

                target.FindAttributeOrDefault(name_ROSbyte).Value.Value.ToInt32().ShouldBe(value);
                target.FindAttributeOrDefault(name_RS).Value.Value.ToInt32().ShouldBe(value);

                target.Attributes.FindOrDefault(name_ROSbyte).Value.Value.ToInt32().ShouldBe(value);
                target.Attributes.FindOrDefault(name_RS).Value.Value.ToInt32().ShouldBe(value);
            }
        }

        private unsafe static void Throw_FindAttribtue<TException>(XmlNode target, string? nsName, string? name) where TException : Exception
        {
            var attrName = new AttrName(nsName, name);

            Assert.Throws<TException>(() => target.FindAttribute(attrName.NsName!, attrName.Name!));
            Assert.Throws<TException>(() => target.FindAttribute(attrName.NsName!, attrName.Name_ROSchar));
            Assert.Throws<TException>(() => target.FindAttribute(attrName.NsName_ROSchar, attrName.Name!));
            Assert.Throws<TException>(() => target.FindAttribute(attrName.NsName_ROSchar, attrName.Name_ROSchar));

            Assert.Throws<TException>(() => target.Attributes.Find(attrName.NsName!, attrName.Name!));
            Assert.Throws<TException>(() => target.Attributes.Find(attrName.NsName!, attrName.Name_ROSchar));
            Assert.Throws<TException>(() => target.Attributes.Find(attrName.NsName_ROSchar, attrName.Name!));
            Assert.Throws<TException>(() => target.Attributes.Find(attrName.NsName_ROSchar, attrName.Name_ROSchar));

            var nsName_ROSbyte = attrName.NsName_ROSbyte;
            var name_ROSbyte = attrName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    Assert.Throws<TException>(() => target.FindAttribute(attrName.NsName_ROSbyte, attrName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.FindAttribute(attrName.NsName_ROSbyte, name_RS));
                    Assert.Throws<TException>(() => target.FindAttribute(nsName_RS, attrName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.FindAttribute(nsName_RS, name_RS));

                    Assert.Throws<TException>(() => target.Attributes.Find(attrName.NsName_ROSbyte, attrName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.Attributes.Find(attrName.NsName_ROSbyte, name_RS));
                    Assert.Throws<TException>(() => target.Attributes.Find(nsName_RS, attrName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.Attributes.Find(nsName_RS, name_RS));
                }
            }
        }

        private unsafe static void NotFound_TryFindAttribtue(XmlNode target, string? nsName, string? name)
        {
            var attrName = new AttrName(nsName, name);

            target.TryFindAttribute(attrName.NsName!, attrName.Name!, out _).ShouldBe(false);
            target.TryFindAttribute(attrName.NsName!, attrName.Name_ROSchar, out _).ShouldBe(false);
            target.TryFindAttribute(attrName.NsName_ROSchar, attrName.Name!, out _).ShouldBe(false);
            target.TryFindAttribute(attrName.NsName_ROSchar, attrName.Name_ROSchar, out _).ShouldBe(false);

            target.Attributes.TryFind(attrName.NsName!, attrName.Name!, out _).ShouldBe(false);
            target.Attributes.TryFind(attrName.NsName!, attrName.Name_ROSchar, out _).ShouldBe(false);
            target.Attributes.TryFind(attrName.NsName_ROSchar, attrName.Name!, out _).ShouldBe(false);
            target.Attributes.TryFind(attrName.NsName_ROSchar, attrName.Name_ROSchar, out _).ShouldBe(false);

            var nsName_ROSbyte = attrName.NsName_ROSbyte;
            var name_ROSbyte = attrName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.TryFindAttribute(attrName.NsName_ROSbyte, attrName.Name_ROSbyte, out _).ShouldBe(false);
                    target.TryFindAttribute(attrName.NsName_ROSbyte, name_RS, out _).ShouldBe(false);
                    target.TryFindAttribute(nsName_RS, attrName.Name_ROSbyte, out _).ShouldBe(false);
                    target.TryFindAttribute(nsName_RS, name_RS, out _).ShouldBe(false);

                    target.Attributes.TryFind(attrName.NsName_ROSbyte, attrName.Name_ROSbyte, out _).ShouldBe(false);
                    target.Attributes.TryFind(attrName.NsName_ROSbyte, name_RS, out _).ShouldBe(false);
                    target.Attributes.TryFind(nsName_RS, attrName.Name_ROSbyte, out _).ShouldBe(false);
                    target.Attributes.TryFind(nsName_RS, name_RS, out _).ShouldBe(false);
                }
            }
        }

        private unsafe static void NotFound_FindAttribtueOrDefault(XmlNode target, string? nsName, string? name)
        {
            var attrName = new AttrName(nsName, name);

            target.FindAttributeOrDefault(attrName.NsName!, attrName.Name!).HasValue.ShouldBe(false);
            target.FindAttributeOrDefault(attrName.NsName!, attrName.Name_ROSchar).HasValue.ShouldBe(false);
            target.FindAttributeOrDefault(attrName.NsName_ROSchar, attrName.Name!).HasValue.ShouldBe(false);
            target.FindAttributeOrDefault(attrName.NsName_ROSchar, attrName.Name_ROSchar).HasValue.ShouldBe(false);

            target.Attributes.FindOrDefault(attrName.NsName!, attrName.Name!).HasValue.ShouldBe(false);
            target.Attributes.FindOrDefault(attrName.NsName!, attrName.Name_ROSchar).HasValue.ShouldBe(false);
            target.Attributes.FindOrDefault(attrName.NsName_ROSchar, attrName.Name!).HasValue.ShouldBe(false);
            target.Attributes.FindOrDefault(attrName.NsName_ROSchar, attrName.Name_ROSchar).HasValue.ShouldBe(false);

            var nsName_ROSbyte = attrName.NsName_ROSbyte;
            var name_ROSbyte = attrName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.FindAttributeOrDefault(attrName.NsName_ROSbyte, attrName.Name_ROSbyte).HasValue.ShouldBe(false);
                    target.FindAttributeOrDefault(attrName.NsName_ROSbyte, name_RS).HasValue.ShouldBe(false);
                    target.FindAttributeOrDefault(nsName_RS, attrName.Name_ROSbyte).HasValue.ShouldBe(false);
                    target.FindAttributeOrDefault(nsName_RS, name_RS).HasValue.ShouldBe(false);

                    target.Attributes.FindOrDefault(attrName.NsName_ROSbyte, attrName.Name_ROSbyte).HasValue.ShouldBe(false);
                    target.Attributes.FindOrDefault(attrName.NsName_ROSbyte, name_RS).HasValue.ShouldBe(false);
                    target.Attributes.FindOrDefault(nsName_RS, attrName.Name_ROSbyte).HasValue.ShouldBe(false);
                    target.Attributes.FindOrDefault(nsName_RS, name_RS).HasValue.ShouldBe(false);
                }
            }
        }

        internal class AttrName
        {
            public string? NsName { get; }
            public string? Name { get; }

            public ReadOnlySpan<char> NsName_ROSchar => NsName.AsSpan();
            public ReadOnlySpan<byte> NsName_ROSbyte => UTF8ExceptionFallbackEncoding.Instance.GetBytes(NsName?.ToCharArray() ?? Array.Empty<char>());
            public ReadOnlySpan<char> Name_ROSchar => Name.AsSpan();
            public ReadOnlySpan<byte> Name_ROSbyte => UTF8ExceptionFallbackEncoding.Instance.GetBytes(Name?.ToCharArray() ?? Array.Empty<char>());

            public AttrName(string? nsName, string? name)
            {
                NsName = nsName;
                Name = name;
            }
        }
    }
}
