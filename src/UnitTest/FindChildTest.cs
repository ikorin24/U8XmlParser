#nullable enable
using System;
using System.Text;
using U8Xml;
using Xunit;

namespace UnitTest
{
    public class FindTestChild
    {
        private const string SampleXml1 =
@"<foo xmlns='test_a' xmlns:b='test_b'>
    <b:bar>1</b:bar>
    <hoge xmlns='test_a2'>2</hoge>
    <hoge>3</hoge>
    <b:piyo xmlns:b='test_b2'>4</b:piyo>
    <aaa>
        <bbb>
            <b:ccc>5</b:ccc>
        </bbb>
    </aaa>
</foo>";

        [Fact]
        public void Test_FindChild()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            CheckInnerValue_FindChild(root, "test_b", "bar", 1);
            CheckInnerValue_FindChild(root, "test_a2", "hoge", 2);
            CheckInnerValue_FindChild(root, "test_a", "hoge", 3);
            CheckInnerValue_FindChild(root, "test_b2", "piyo", 4);

            var bbb = root.FindChild("aaa").FindChild("bbb");
            CheckInnerValue_FindChild(bbb, "test_b", "ccc", 5);

            CheckThrow_FindChild<InvalidOperationException>(root, "xxxx", "xxxx");
            CheckThrow_FindChild<InvalidOperationException>(root, "xxxx", null);
            CheckThrow_FindChild<InvalidOperationException>(root, null, "xxxx");
            CheckThrow_FindChild<InvalidOperationException>(root, null, null);
        }

        [Fact]
        public void Test_FindChild2()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            root.Children.Find("test_b", "bar").InnerText.ToInt32().ShouldBe(1);
        }

        [Fact]
        public void Test_FindChildOrDefault()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            CheckInnerValue_FindChildOrDefault(root, "test_b", "bar", 1);
            CheckInnerValue_FindChildOrDefault(root, "test_a2", "hoge", 2);
            CheckInnerValue_FindChildOrDefault(root, "test_a", "hoge", 3);
            CheckInnerValue_FindChildOrDefault(root, "test_b2", "piyo", 4);

            var bbb = root.FindChild("aaa").FindChild("bbb");
            CheckInnerValue_FindChildOrDefault(bbb, "test_b", "ccc", 5);

            CheckNotFound_FindChildOrDefault(root, "xxxx", "xxxx");
            CheckNotFound_FindChildOrDefault(root, "xxxx", null);
            CheckNotFound_FindChildOrDefault(root, null, "xxxx");
            CheckNotFound_FindChildOrDefault(root, null, null);
        }

        [Fact]
        public void Test_TryFindChild()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            CheckInnerValue_TryFindChild(root, "test_b", "bar", 1);
            CheckInnerValue_TryFindChild(root, "test_a2", "hoge", 2);
            CheckInnerValue_TryFindChild(root, "test_a", "hoge", 3);
            CheckInnerValue_TryFindChild(root, "test_b2", "piyo", 4);

            var bbb = root.FindChild("aaa").FindChild("bbb");
            CheckInnerValue_TryFindChild(bbb, "test_b", "ccc", 5);

            CheckNotFound_TryFindChild(root, "xxxx", "xxxx");
            CheckNotFound_TryFindChild(root, "xxxx", null);
            CheckNotFound_TryFindChild(root, null, "xxxx");
            CheckNotFound_TryFindChild(root, null, null);
        }

        private static unsafe void CheckInnerValue_FindChild(XmlNode target, string? nsName, string? name, int innerValue)
        {
            var nodeName = new NodeName(nsName, name);

            target.FindChild(nodeName.NsName!, nodeName.Name!).InnerText.ToInt32().ShouldBe(innerValue);
            target.FindChild(nodeName.NsName!, nodeName.Name_ROSchar).InnerText.ToInt32().ShouldBe(innerValue);
            target.FindChild(nodeName.NsName_ROSchar, nodeName.Name!).InnerText.ToInt32().ShouldBe(innerValue);
            target.FindChild(nodeName.NsName_ROSchar, nodeName.Name_ROSchar).InnerText.ToInt32().ShouldBe(innerValue);

            target.Children.Find(nodeName.NsName!, nodeName.Name!).InnerText.ToInt32().ShouldBe(innerValue);
            target.Children.Find(nodeName.NsName!, nodeName.Name_ROSchar).InnerText.ToInt32().ShouldBe(innerValue);
            target.Children.Find(nodeName.NsName_ROSchar, nodeName.Name!).InnerText.ToInt32().ShouldBe(innerValue);
            target.Children.Find(nodeName.NsName_ROSchar, nodeName.Name_ROSchar).InnerText.ToInt32().ShouldBe(innerValue);

            var nsName_ROSbyte = nodeName.NsName_ROSbyte;
            var name_ROSbyte = nodeName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.FindChild(nsName_ROSbyte, name_ROSbyte).InnerText.ToInt32().ShouldBe(innerValue);
                    target.FindChild(nsName_ROSbyte, name_RS).InnerText.ToInt32().ShouldBe(innerValue);
                    target.FindChild(nsName_RS, name_ROSbyte).InnerText.ToInt32().ShouldBe(innerValue);
                    target.FindChild(nsName_RS, name_RS).InnerText.ToInt32().ShouldBe(innerValue);

                    target.Children.Find(nsName_ROSbyte, name_ROSbyte).InnerText.ToInt32().ShouldBe(innerValue);
                    target.Children.Find(nsName_ROSbyte, name_RS).InnerText.ToInt32().ShouldBe(innerValue);
                    target.Children.Find(nsName_RS, name_ROSbyte).InnerText.ToInt32().ShouldBe(innerValue);
                    target.Children.Find(nsName_RS, name_RS).InnerText.ToInt32().ShouldBe(innerValue);
                }
            }
        }

        private static unsafe void CheckThrow_FindChild<TException>(XmlNode target, string? nsName, string? name) where TException : Exception
        {
            var nodeName = new NodeName(nsName, name);

            Assert.Throws<TException>(() => target.FindChild(nodeName.NsName!, nodeName.Name!));
            Assert.Throws<TException>(() => target.FindChild(nodeName.NsName!, nodeName.Name_ROSchar));
            Assert.Throws<TException>(() => target.FindChild(nodeName.NsName_ROSchar, nodeName.Name!));
            Assert.Throws<TException>(() => target.FindChild(nodeName.NsName_ROSchar, nodeName.Name_ROSchar));

            Assert.Throws<TException>(() => target.Children.Find(nodeName.NsName!, nodeName.Name!));
            Assert.Throws<TException>(() => target.Children.Find(nodeName.NsName!, nodeName.Name_ROSchar));
            Assert.Throws<TException>(() => target.Children.Find(nodeName.NsName_ROSchar, nodeName.Name!));
            Assert.Throws<TException>(() => target.Children.Find(nodeName.NsName_ROSchar, nodeName.Name_ROSchar));

            var nsName_ROSbyte = nodeName.NsName_ROSbyte;
            var name_ROSbyte = nodeName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    Assert.Throws<TException>(() => target.FindChild(nodeName.NsName_ROSbyte, nodeName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.FindChild(nodeName.NsName_ROSbyte, name_RS));
                    Assert.Throws<TException>(() => target.FindChild(nsName_RS, nodeName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.FindChild(nsName_RS, name_RS));

                    Assert.Throws<TException>(() => target.Children.Find(nodeName.NsName_ROSbyte, nodeName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.Children.Find(nodeName.NsName_ROSbyte, name_RS));
                    Assert.Throws<TException>(() => target.Children.Find(nsName_RS, nodeName.Name_ROSbyte));
                    Assert.Throws<TException>(() => target.Children.Find(nsName_RS, name_RS));
                }
            }
        }

        private static unsafe void CheckInnerValue_FindChildOrDefault(XmlNode target, string? nsName, string? name, int innerValue)
        {
            var nodeName = new NodeName(nsName, name);

            target.FindChildOrDefault(nodeName.NsName!, nodeName.Name!)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);

            target.FindChildOrDefault(nodeName.NsName!, nodeName.Name_ROSchar)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);

            target.FindChildOrDefault(nodeName.NsName_ROSchar, nodeName.Name!)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);

            target.FindChildOrDefault(nodeName.NsName_ROSchar, nodeName.Name_ROSchar)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);


            target.Children.FindOrDefault(nodeName.NsName!, nodeName.Name!)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);

            target.Children.FindOrDefault(nodeName.NsName!, nodeName.Name_ROSchar)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);

            target.Children.FindOrDefault(nodeName.NsName_ROSchar, nodeName.Name!)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);

            target.Children.FindOrDefault(nodeName.NsName_ROSchar, nodeName.Name_ROSchar)
                .Value.InnerText.ToInt32().ShouldBe(innerValue);


            var nsName_ROSbyte = nodeName.NsName_ROSbyte;
            var name_ROSbyte = nodeName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.FindChildOrDefault(nsName_ROSbyte, name_ROSbyte)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);

                    target.FindChildOrDefault(nsName_ROSbyte, name_RS)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);

                    target.FindChildOrDefault(nsName_RS, name_ROSbyte)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);

                    target.FindChildOrDefault(nsName_RS, name_RS)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);


                    target.Children.FindOrDefault(nsName_ROSbyte, name_ROSbyte)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);

                    target.Children.FindOrDefault(nsName_ROSbyte, name_RS)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);

                    target.Children.FindOrDefault(nsName_RS, name_ROSbyte)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);

                    target.Children.FindOrDefault(nsName_RS, name_RS)
                        .Value.InnerText.ToInt32().ShouldBe(innerValue);
                }
            }
        }

        private static unsafe void CheckNotFound_FindChildOrDefault(XmlNode target, string? nsName, string? name)
        {
            var nodeName = new NodeName(nsName, name);

            target.FindChildOrDefault(nodeName.NsName!, nodeName.Name!).HasValue.ShouldBe(false);
            target.FindChildOrDefault(nodeName.NsName!, nodeName.Name_ROSchar).HasValue.ShouldBe(false);
            target.FindChildOrDefault(nodeName.NsName_ROSchar, nodeName.Name!).HasValue.ShouldBe(false);
            target.FindChildOrDefault(nodeName.NsName_ROSchar, nodeName.Name_ROSchar).HasValue.ShouldBe(false);

            target.Children.FindOrDefault(nodeName.NsName!, nodeName.Name!).HasValue.ShouldBe(false);
            target.Children.FindOrDefault(nodeName.NsName!, nodeName.Name_ROSchar).HasValue.ShouldBe(false);
            target.Children.FindOrDefault(nodeName.NsName_ROSchar, nodeName.Name!).HasValue.ShouldBe(false);
            target.Children.FindOrDefault(nodeName.NsName_ROSchar, nodeName.Name_ROSchar).HasValue.ShouldBe(false);

            var nsName_ROSbyte = nodeName.NsName_ROSbyte;
            var name_ROSbyte = nodeName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.FindChildOrDefault(nsName_ROSbyte, name_ROSbyte).HasValue.ShouldBe(false);
                    target.FindChildOrDefault(nsName_ROSbyte, name_RS).HasValue.ShouldBe(false);
                    target.FindChildOrDefault(nsName_RS, name_ROSbyte).HasValue.ShouldBe(false);
                    target.FindChildOrDefault(nsName_RS, name_RS).HasValue.ShouldBe(false);

                    target.Children.FindOrDefault(nsName_ROSbyte, name_ROSbyte).HasValue.ShouldBe(false);
                    target.Children.FindOrDefault(nsName_ROSbyte, name_RS).HasValue.ShouldBe(false);
                    target.Children.FindOrDefault(nsName_RS, name_ROSbyte).HasValue.ShouldBe(false);
                    target.Children.FindOrDefault(nsName_RS, name_RS).HasValue.ShouldBe(false);
                }
            }
        }

        private static unsafe void CheckInnerValue_TryFindChild(XmlNode target, string? nsName, string? name, int innerValue)
        {
            var nodeName = new NodeName(nsName, name);

            {
                target.TryFindChild(nodeName.NsName!, nodeName.Name!, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }
            {
                target.TryFindChild(nodeName.NsName!, nodeName.Name_ROSchar, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }
            {
                target.TryFindChild(nodeName.NsName_ROSchar, nodeName.Name!, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }
            {
                target.TryFindChild(nodeName.NsName_ROSchar, nodeName.Name_ROSchar, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }

            {
                target.Children.TryFind(nodeName.NsName!, nodeName.Name!, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }
            {
                target.Children.TryFind(nodeName.NsName!, nodeName.Name_ROSchar, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }
            {
                target.Children.TryFind(nodeName.NsName_ROSchar, nodeName.Name!, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }
            {
                target.Children.TryFind(nodeName.NsName_ROSchar, nodeName.Name_ROSchar, out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(innerValue);
            }

            var nsName_ROSbyte = nodeName.NsName_ROSbyte;
            var name_ROSbyte = nodeName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    {
                        target.TryFindChild(nsName_ROSbyte, name_ROSbyte, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }
                    {
                        target.TryFindChild(nsName_ROSbyte, name_RS, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }
                    {
                        target.TryFindChild(nsName_RS, name_ROSbyte, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }
                    {
                        target.TryFindChild(nsName_RS, name_RS, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }

                    {
                        target.Children.TryFind(nsName_ROSbyte, name_ROSbyte, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }
                    {
                        target.Children.TryFind(nsName_ROSbyte, name_RS, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }
                    {
                        target.Children.TryFind(nsName_RS, name_ROSbyte, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }
                    {
                        target.Children.TryFind(nsName_RS, name_RS, out var value).ShouldBe(true);
                        value.InnerText.ToInt32().ShouldBe(innerValue);
                    }
                }
            }
        }

        private static unsafe void CheckNotFound_TryFindChild(XmlNode target, string? nsName, string? name)
        {
            var nodeName = new NodeName(nsName, name);

            target.TryFindChild(nodeName.NsName!, nodeName.Name!, out _).ShouldBe(false);
            target.TryFindChild(nodeName.NsName!, nodeName.Name_ROSchar, out _).ShouldBe(false);
            target.TryFindChild(nodeName.NsName_ROSchar, nodeName.Name!, out _).ShouldBe(false);
            target.TryFindChild(nodeName.NsName_ROSchar, nodeName.Name_ROSchar, out _).ShouldBe(false);

            target.Children.TryFind(nodeName.NsName!, nodeName.Name!, out _).ShouldBe(false);
            target.Children.TryFind(nodeName.NsName!, nodeName.Name_ROSchar, out _).ShouldBe(false);
            target.Children.TryFind(nodeName.NsName_ROSchar, nodeName.Name!, out _).ShouldBe(false);
            target.Children.TryFind(nodeName.NsName_ROSchar, nodeName.Name_ROSchar, out _).ShouldBe(false);

            var nsName_ROSbyte = nodeName.NsName_ROSbyte;
            var name_ROSbyte = nodeName.Name_ROSbyte;
            fixed(byte* p = nsName_ROSbyte) {
                fixed(byte* p2 = name_ROSbyte) {
                    var nsName_RS = new RawString(p, nsName_ROSbyte.Length);
                    var name_RS = new RawString(p2, name_ROSbyte.Length);

                    target.TryFindChild(nsName_ROSbyte, name_ROSbyte, out _).ShouldBe(false);
                    target.TryFindChild(nsName_ROSbyte, name_RS, out _).ShouldBe(false);
                    target.TryFindChild(nsName_RS, name_ROSbyte, out _).ShouldBe(false);
                    target.TryFindChild(nsName_RS, name_RS, out _).ShouldBe(false);

                    target.Children.TryFind(nsName_ROSbyte, name_ROSbyte, out _).ShouldBe(false);
                    target.Children.TryFind(nsName_ROSbyte, name_RS, out _).ShouldBe(false);
                    target.Children.TryFind(nsName_RS, name_ROSbyte, out _).ShouldBe(false);
                    target.Children.TryFind(nsName_RS, name_RS, out _).ShouldBe(false);
                }
            }
        }

        internal class NodeName
        {
            public string? NsName { get; }
            public string? Name { get; }

            public ReadOnlySpan<char> NsName_ROSchar => NsName.AsSpan();
            public ReadOnlySpan<byte> NsName_ROSbyte => Encoding.UTF8.GetBytes(NsName?.ToCharArray() ?? Array.Empty<char>());
            public ReadOnlySpan<char> Name_ROSchar => Name.AsSpan();
            public ReadOnlySpan<byte> Name_ROSbyte => Encoding.UTF8.GetBytes(Name?.ToCharArray() ?? Array.Empty<char>());

            public NodeName(string? nsName, string? name)
            {
                NsName = nsName;
                Name = name;
            }
        }
    }

    internal static class AssertExtension
    {
        public static void ShouldBe(this int actual, int expected)
        {
            Assert.Equal(expected, actual);
        }

        public static void ShouldBe(this bool actual, bool expected)
        {
            Assert.Equal(expected, actual);
        }
    }
}
