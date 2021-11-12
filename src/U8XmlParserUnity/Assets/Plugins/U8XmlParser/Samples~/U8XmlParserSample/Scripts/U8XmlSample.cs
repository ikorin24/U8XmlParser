using UnityEngine;

namespace U8Xml.Samples
{
    public class U8XmlSample : MonoBehaviour
    {
        private const string SampleXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- This is a sample xml. -->
<foo>
  <bar aaa=""10""/>
  <baz>The quick brown fox jumps over the lazy dog</baz>
</foo>
";

        private void Start()
        {
            string rootNodeName;

            Debug.Log(SampleXml);
            using(var xml = XmlParser.Parse(SampleXml)) {

                var root = xml.Root;
                rootNodeName = root.Name.ToString();

                if(root.TryGetFirstChild(out var node1)) {
                    var attr = node1.FindAttribute("aaa");
                    string attrName = attr.Name.ToString();
                    int attrValue = attr.Value.ToInt32();
                    Debug.Log($"node1: {node1.Name}, [{attrName} = {attrValue}]");
                }

                if(root.TryFindChild("baz", out var node2)) {
                    Debug.Log(node2.InnerText);
                }
            }

            // [NOTE]
            // Don't use XmlObject, RawString, or any objects from XmlParser after Dispose().
            // All thier memory are already released.
            // Evaluate them into int, float, string, etc... if you use after disposed.

            Debug.Log($"root node name: {rootNodeName}");
        }
    }
}
