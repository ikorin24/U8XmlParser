#nullable enable
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
</foo>";

        [Fact]
        public void FindChild()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var root = xml.Root;

            //root.FindChild("test_b", "bar").InnerText.ToInt32().ShouldBe(1);
            //root.FindChild("test_a2", "hoge").InnerText.ToInt32().ShouldBe(2);
            root.FindChild("test_a", "hoge").InnerText.ToInt32().ShouldBe(3);
            //root.FindChild("test_b2", "piyo").InnerText.ToInt32().ShouldBe(4);
        }
    }

    internal static class AssertExtension
    {
        public static void ShouldBe(this int actual, int expected)
        {
            Assert.Equal(expected, actual);
        }
    }
}
