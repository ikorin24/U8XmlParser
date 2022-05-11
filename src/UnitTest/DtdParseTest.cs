#nullable enable
using System;
using Xunit;
using U8Xml;

namespace UnitTest
{
    public class DtdParseTest
    {
        [Theory]
        [InlineData(
@"<?xml version=""1.0""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 3.2 Final//EN"" ""http://www.w3.org/MarkUp/Wilbur/HTML32.DTD"">
<html>
</html>")]
        [InlineData("<?xml version=\"1.0\"?><!DOCTYPE\thtml  PUBLIC \r\n'-//W3C//DTD HTML 3.2 Final//EN'\t  'http://www.w3.org/MarkUp/Wilbur/HTML32.DTD'  ><html></html>")]
        public void ExternalDtd_PUBLIC(string xmlString)
        {
            // No exceptions, but the external dtd is not read.

            using var xml = XmlParser.Parse(xmlString);
            Assert.True(xml.DocumentType.HasValue);
            var doctype = xml.DocumentType.Value;
            Assert.Equal("html", doctype.Name.ToString());
            Assert.Equal("", doctype.InternalSubset.ToString());
            Assert.False(doctype.AsRawString().IsEmpty);
        }

        [Theory]
        [InlineData(
@"<?xml version=""1.0""?>
<!DOCTYPE data SYSTEM ""http://github.com/ikorin24/foo.dtd"">
<data>
</data>")]
        [InlineData("<?xml version=\"1.0\"?><!DOCTYPE\t\t\tdata\t\t\tSYSTEM\r\n\t\t\t'http://github.com/ikorin24/foo.dtd'   ><data></data>")]
        public void ExternalDtd_SYSTEM(string xmlString)
        {
            // No exceptions, but the external dtd is not read.

            using var xml = XmlParser.Parse(xmlString);
            Assert.True(xml.DocumentType.HasValue);
            var doctype = xml.DocumentType.Value;
            Assert.Equal("data", doctype.Name.ToString());
            Assert.Equal("", doctype.InternalSubset.ToString());
            Assert.False(doctype.AsRawString().IsEmpty);
        }

        [Theory]
        [InlineData(
@"<?xml version=""1.0""?>
<!DOCTYPE data[]>
<data>
</data>")]
        [InlineData("<?xml version=\"1.0\"?><!DOCTYPE data[]><data></data>")]
        [InlineData("<?xml version=\"1.0\"?><!DOCTYPE data  []><data></data>")]
        public void Dtd_InternalSubset(string xmlString)
        {
            using var xml = XmlParser.Parse(xmlString);
            Assert.True(xml.DocumentType.HasValue);
            var doctype = xml.DocumentType.Value;
            Assert.Equal("data", doctype.Name.ToString());
            Assert.Equal("", doctype.InternalSubset.ToString());
            Assert.False(doctype.AsRawString().IsEmpty);
        }

        [Theory]
        [InlineData(
@"<?xml version=""1.0""?>
<!DOCTYPE data [
    <!ENTITY foo ""aaa"">
]>
<data>&foo;</data>")]
        public void Dtd_InternalSubset2(string xmlString)
        {
            using var xml = XmlParser.Parse(xmlString);
            Assert.True(xml.DocumentType.HasValue);
            var doctype = xml.DocumentType.Value;
            Assert.Equal("data", doctype.Name.ToString());
            Assert.Equal(@"<!ENTITY foo ""aaa"">", doctype.InternalSubset.Trim().ToString());
            Assert.False(doctype.AsRawString().IsEmpty);

            Assert.Equal("aaa", xml.EntityTable.ResolveToString(xml.Root.InnerText));
        }

        [Theory]
        [InlineData(
@"<?xml version=""1.0""?>
<!DOCTYPE data [
    <!ENTITY foo ""aaa"">
    <!ENTITY bar SYSTEM ""another_file.xml"">
]>
<data x=""&foo;"">&bar;</data>")]
        public void Dtd_ExternalEntity(string xmlString)
        {
            // U8XmlParser does not throw an exception even if xml containing external entity references.
            // However, it does not read the external file.
            // Thus, we cannot resolve the entity.


            // No exceptions to parse xml
            using var xml = XmlParser.Parse(xmlString);
            Assert.True(xml.DocumentType.HasValue);
            var doctype = xml.DocumentType.Value;
            Assert.Equal("data", doctype.Name.ToString());
            Assert.Equal(@"<!ENTITY foo ""aaa"">
    <!ENTITY bar SYSTEM ""another_file.xml"">", doctype.InternalSubset.Trim().ToString());
            Assert.False(doctype.AsRawString().IsEmpty);


            var entities = xml.EntityTable;
            var data = xml.Root;

            // We can resolve the entity in the internal subset.
            Assert.Equal("aaa", entities.ResolveToString(data.FindAttribute("x").Value));

            // External entity references can not be resolved.
            Assert.Equal(XmlEntityResolverState.CannotResolve, entities.CheckNeedToResolve(data.InnerText, out _));
            Assert.Throws<ArgumentException>(() => entities.ResolveToString(data.InnerText));
        }
    }
}
