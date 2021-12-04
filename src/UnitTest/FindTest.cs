#nullable enable
using System;
using U8Xml;
using Xunit;

namespace UnitTest
{
    public class FindTest
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
        public void FindChild()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            root.FindChild("test_b", "bar").InnerText.ToInt32().ShouldBe(1);
            root.FindChild("test_a2", "hoge").InnerText.ToInt32().ShouldBe(2);
            root.FindChild("test_a", "hoge").InnerText.ToInt32().ShouldBe(3);
            root.FindChild("test_b2", "piyo").InnerText.ToInt32().ShouldBe(4);

            root.FindChild("aaa")
                .FindChild("bbb")
                .FindChild("test_b", "ccc").InnerText.ToInt32().ShouldBe(5);

            Assert.Throws<InvalidOperationException>(() => root.FindChild("xxxx", "xxxx"));
            Assert.Throws<InvalidOperationException>(() => root.FindChild("xxxx"));
        }

        [Fact]
        public void FindChildOrDefault()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            root.FindChildOrDefault("test_b", "bar").Value.InnerText.ToInt32().ShouldBe(1);
            root.FindChildOrDefault("test_a2", "hoge").Value.InnerText.ToInt32().ShouldBe(2);
            root.FindChildOrDefault("test_a", "hoge").Value.InnerText.ToInt32().ShouldBe(3);
            root.FindChildOrDefault("test_b2", "piyo").Value.InnerText.ToInt32().ShouldBe(4);

            root.FindChildOrDefault("aaa").Value
                .FindChildOrDefault("bbb").Value
                .FindChildOrDefault("test_b", "ccc").Value.InnerText.ToInt32().ShouldBe(5);

            root.FindChildOrDefault("xxxx", "xxxx").HasValue.ShouldBe(false);
            root.FindChildOrDefault("xxxx").HasValue.ShouldBe(false);
        }

        [Fact]
        public void TryFindChild()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            {
                root.TryFindChild("test_b", "bar", out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(1);
            }
            {
                root.TryFindChild("test_a2", "hoge", out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(2);
            }
            {
                root.TryFindChild("test_a", "hoge", out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(3);
            }
            {
                root.TryFindChild("test_b2", "piyo", out var value).ShouldBe(true);
                value.InnerText.ToInt32().ShouldBe(4);
            }
            {
                root.TryFindChild("aaa", out var node1).ShouldBe(true);
                node1.TryFindChild("bbb", out var node2).ShouldBe(true);
                node2.TryFindChild("test_b", "ccc", out var node3).ShouldBe(true);
                node3.InnerText.ToInt32().ShouldBe(5);
            }


            {
                root.TryFindChild("xxxx", "xxxx", out _).ShouldBe(false);
                root.TryFindChild("xxxx", out _).ShouldBe(false);
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
