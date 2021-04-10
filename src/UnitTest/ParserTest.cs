#nullable enable
using System.Linq;
using Xunit;
using U8Xml;
using U8Xml.Internal;

namespace UnitTest
{
    public partial class ParserTest
    {
        [Fact]
        public void Parse()
        {
            using(var obj = XmlParser.Parse(Data.Sample1)) {
                Assert.NotNull(obj);
                var root = obj.Root;
                Assert.True(root.Name == "あいうえお");
                Assert.True(root.InnerText.IsEmpty);
                Assert.True(root.HasAttribute);
                Assert.True(root.Attributes.Count == 1);
                Assert.True(root.HasChildren);
                Assert.True(root.Children.Count == 1);

                // Test attributus enumeration via interface
                Assert.True(root.Attributes.First().Name == "ほげ");
                Assert.True(root.Attributes.First().Value == "3");
                Assert.True(root.Attributes.First() == ("ほげ", "3"));

                // Test attributes enumeration directly
                foreach(var attr in root.Attributes) {
                    Assert.True(attr.Name == "ほげ");
                    Assert.True(attr.Value == "3");
                    var (name, value) = attr;
                    Assert.True(name == "ほげ");
                    Assert.True(value == "3");
                    break;
                }

                // Test children enumeration via interface
                Assert.True(root.Children.First().Name == "かきくけこ");
                Assert.True(root.Children.First().InnerText == "さしすせそ");
                Assert.True(root.Children.First().HasAttribute == false);
                Assert.True(root.Children.First().Attributes.Count == 0);
                Assert.True(root.Children.First().HasChildren == false);
                Assert.True(root.Children.First().Children.Count == 0);

                // Test children enumeration directly
                foreach(var child in root.Children) {
                    Assert.True(child.Name == "かきくけこ");
                    Assert.True(child.InnerText == "さしすせそ");
                    Assert.True(child.HasAttribute == false);
                    Assert.True(child.Attributes.Count == 0);
                    Assert.True(child.HasChildren == false);
                    Assert.True(child.Children.Count == 0);
                    break;
                }
            }
            AllocationSafety.Ensure();
        }
    }
}
