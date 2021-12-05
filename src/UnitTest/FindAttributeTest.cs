#nullable enable
using System;
using System.Text;
using U8Xml;
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

                var nsName_ROSbyte = Encoding.UTF8.GetBytes(nsName).AsSpan();
                var name_ROSbyte = Encoding.UTF8.GetBytes(name).AsSpan();
                fixed(byte* p = nsName_ROSbyte)
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    attr.IsName(nsName_ROSbyte, name_ROSbyte).ShouldBe(true);
                    attr.IsName(nsName_RS, name_ROSbyte).ShouldBe(true);
                    attr.IsName(nsName_ROSbyte, name_RS).ShouldBe(true);
                    attr.IsName(nsName_RS, name_RS).ShouldBe(true);
                }
            }
        }

        [Fact]
        public void Test_FindAttribute()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var foo = xml.Root;
            var bar = foo.FindChild("bar");

            CheckInnerValue_FindAttribtue(bar, "test_b", "hoge", 1);
            CheckInnerValue_FindAttribtue(bar, "test_a", "hoge", 2);

            var hoge = foo.FindChild("hoge");
            CheckInnerValue_FindAttribtue(hoge, "test_a2", "piyo", 3);
            CheckInnerValue_FindAttribtue(hoge, "piyo", 3);

            var baz = hoge.FindChild("baz");
            CheckInnerValue_FindAttribtue(baz, "test_b", "abc", 4);
            CheckInnerValue_FindAttribtue(baz, "test_a2", "xyz", 5);
            CheckInnerValue_FindAttribtue(baz, "xyz", 5);

            var ccc = foo.FindChild("aaa").FindChild("bbb").FindChild("ccc");
            CheckInnerValue_FindAttribtue(ccc, "test_b", "aaaa", 6);

            Throw_FindAttribtue<InvalidOperationException>(hoge, "test_a", "piyo");
        }

        private unsafe static void CheckInnerValue_FindAttribtue(XmlNode target, string? name, int value)
        {
            target.FindAttribute(name!).Value.ToInt32().ShouldBe(value);
            target.FindAttribute(name.AsSpan()).Value.ToInt32().ShouldBe(value);

            target.Attributes.Find(name!).Value.ToInt32().ShouldBe(value);
            target.Attributes.Find(name.AsSpan()).Value.ToInt32().ShouldBe(value);

            var name_ROSbyte = Encoding.UTF8.GetBytes(name ?? "");
            fixed(byte* p = name_ROSbyte) {
                var name_RS = new RawString(p, name_ROSbyte.Length);

                target.FindAttribute(name_ROSbyte).Value.ToInt32().ShouldBe(value);
                target.FindAttribute(name_RS).Value.ToInt32().ShouldBe(value);

                target.Attributes.Find(name_ROSbyte).Value.ToInt32().ShouldBe(value);
                target.Attributes.Find(name_RS).Value.ToInt32().ShouldBe(value);
            }
        }

        private unsafe static void CheckInnerValue_FindAttribtue(XmlNode target, string? nsName, string? name, int value)
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

        internal class AttrName
        {
            public string? NsName { get; }
            public string? Name { get; }

            public ReadOnlySpan<char> NsName_ROSchar => NsName.AsSpan();
            public ReadOnlySpan<byte> NsName_ROSbyte => Encoding.UTF8.GetBytes(NsName?.ToCharArray() ?? Array.Empty<char>());
            public ReadOnlySpan<char> Name_ROSchar => Name.AsSpan();
            public ReadOnlySpan<byte> Name_ROSbyte => Encoding.UTF8.GetBytes(Name?.ToCharArray() ?? Array.Empty<char>());

            public AttrName(string? nsName, string? name)
            {
                NsName = nsName;
                Name = name;
            }
        }
    }
}
