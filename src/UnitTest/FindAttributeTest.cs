#nullable enable
using System;
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
        public void Test_FindAttribute()
        {
            using var xml = XmlParser.Parse(SampleXml1);
            var foo = xml.Root;
            var bar = foo.FindChild("bar");
            bar.FindAttribute("test_b", "hoge").Value.ToInt32().ShouldBe(1);
            bar.FindAttribute("test_a", "hoge").Value.ToInt32().ShouldBe(2);

            var hoge = foo.FindChild("hoge");
            hoge.FindAttribute("test_a2", "piyo").Value.ToInt32().ShouldBe(3);
            hoge.FindAttribute("piyo").Value.ToInt32().ShouldBe(3);

            var baz = hoge.FindChild("baz");
            baz.FindAttribute("test_b", "abc").Value.ToInt32().ShouldBe(4);
            baz.FindAttribute("test_a2", "xyz").Value.ToInt32().ShouldBe(5);
            baz.FindAttribute("xyz").Value.ToInt32().ShouldBe(5);

            var ccc = foo.FindChild("aaa").FindChild("bbb").FindChild("ccc");
            ccc.FindAttribute("test_b", "aaaa").Value.ToInt32().ShouldBe(6);

            Assert.Throws<InvalidOperationException>(() => hoge.FindAttribute("test_a", "piyo"));
        }
    }
}
