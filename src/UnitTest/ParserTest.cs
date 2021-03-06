#nullable enable
using System;
using System.Linq;
using System.Text;
using Xunit;
using U8Xml;
using U8Xml.Internal;
using System.Collections.Generic;

namespace UnitTest
{
    public class ParserTest
    {
        [Fact]
        public void Parse()
        {
            foreach(var func in TestCases.GetTestCases(Data.Sample1)) {
                using(var xml = func()) {
                    TestSample1Contents(xml);
                }
                AllocationSafety.Ensure();
            }
            foreach(var func in TestCases.GetTestCases(Data.ErrorSample1)) {
                Assert.Throws<FormatException>(() =>
                {
                    using var xml = func();
                });
                AllocationSafety.Ensure();
            }
            foreach(var func in TestCases.GetTestCases(Data.ErrorSample2)) {
                Assert.Throws<FormatException>(() =>
                {
                    using var xml = func();
                });
                AllocationSafety.Ensure();
            }
            foreach(var func in TestCases.GetTestCases(Data.ErrorSample3)) {
                Assert.Throws<FormatException>(() =>
                {
                    using var xml = func();
                });
                AllocationSafety.Ensure();
            }

            foreach(var func in TestCases.GetUnsafeTestCases(Data.Sample1)) {
                using(var xml = func()) {
                    TestSample1Contents(xml);
                }
                AllocationSafety.Ensure();
            }
            foreach(var func in TestCases.GetUnsafeTestCases(Data.ErrorSample1)) {
                Assert.Throws<FormatException>(() =>
                {
                    using var xml = func();
                });
                AllocationSafety.Ensure();
            }
            foreach(var func in TestCases.GetUnsafeTestCases(Data.ErrorSample2)) {
                Assert.Throws<FormatException>(() =>
                {
                    using var xml = func();
                });
                AllocationSafety.Ensure();
            }
            foreach(var func in TestCases.GetUnsafeTestCases(Data.ErrorSample3)) {
                Assert.Throws<FormatException>(() =>
                {
                    using var xml = func();
                });
                AllocationSafety.Ensure();
            }
            return;
        }

        [Fact]
        public void TreeTest()
        {
            var ans = new NodeInfo("?????????", "", new[] { ("?????????", "?????????") },
                new NodeInfo("???????????????????????????MAX", "", null,
                    new NodeInfo("?????????????????????????????????", "", new[] { ("??????", "Koi") },
                        new NodeInfo("?????????????????????", "", new[] { ("??????", "?????????") },
                            new NodeInfo("????????????", "", new[] { ("age", "13"), ("tall", "144") }),
                            new NodeInfo("????????????", "", new[] { ("age", "15"), ("tall", "154") })
                        )
                    )
                ),
                new NodeInfo("??????????????????????????????????????????", "", null,
                    new NodeInfo("?????????????????????", "", new[] { ("??????", "???????????????") },
                        new NodeInfo("?????????", "", null,
                            new NodeInfo("????????????", "????????????????????????????????????", new[] { ("??????", "????????????") }),
                            new NodeInfo("????????????", "??????????????????????????????", new[] { ("??????", "??????") })
                        )
                    )
                )
            );
            using(var xml = XmlParser.Parse(Data.Sample2)) {
                var tree = new NodeInfo(xml.Root);
                Assert.True(NodeInfoComparer.Default.Equals(tree, ans));
            }
            AllocationSafety.Ensure();
        }

        [Fact]
        public void GetAllNodes()
        {
            using(var xml = XmlParser.Parse(Data.Sample2)) {
                var answer = new[] { xml.Root }.Concat(GetChildrenRecursively(xml.Root));

                // Enumerate descendants via foreach
                {
                    var list = new List<XmlNode>();
                    foreach(var node in xml.GetAllNodes()) {
                        list.Add(node);
                    }
                    Assert.True(list.SequenceEqual(answer));
                }

                // Enumerate descendants via IEnumerable<T>
                {
                    Assert.True(xml.GetAllNodes().AsEnumerable().SequenceEqual(answer));
                }
            }
        }

        [Fact]
        public void Descendants()
        {
            using(var xml = XmlParser.Parse(Data.Sample2)) {
                foreach(var node in xml.GetAllNodes()) {
                    CheckDescendantNodes(node);
                }
            }
            AllocationSafety.Ensure();
            return;

            static void CheckDescendantNodes(XmlNode targetNode)
            {
                // Enumerate descendants via foreach
                var answer = GetChildrenRecursively(targetNode).ToArray();
                int i = 0;
                foreach(var node in targetNode.Descendants) {
                    Assert.Equal(answer[i], node);
                    i++;
                }
                Assert.Equal(i, answer.Length);

                // Enumerate descendants via IEnumerable<T>
                Assert.True(targetNode.Descendants.AsEnumerable().SequenceEqual(answer));
            }
        }

        private static void TestSample1Contents<TXmlObject>(TXmlObject xml) where TXmlObject : IXmlObject
        {
            Assert.NotNull(xml);
            var declaration = xml.Declaration.Value;
            Assert.True(declaration.AsRawString() == @"<?xml version=""1.0"" encoding=""UTF-8""?>");
            var version = declaration.Version.Value;
            Assert.True(version.Name == "version");
            Assert.True(version.Value == "1.0");
            var encoding = declaration.Encoding.Value;
            Assert.True(encoding.Name == "encoding");
            Assert.True(encoding.Value == "UTF-8");

            var root = xml.Root;
            Assert.True(root.Name == "???????????????");
            Assert.True(root.InnerText.IsEmpty);
            Assert.True(root.HasAttribute);
            Assert.True(root.Attributes.Count == 1);
            Assert.True(root.HasChildren);
            Assert.True(root.Children.Count == 2);

            // Test attributus enumeration via interface
            Assert.True(root.Attributes.First().Name == "??????");
            Assert.True(root.Attributes.First().Value == "3");
            Assert.True(root.Attributes.First() == ("??????", "3"));

            // Test attributes enumeration directly
            foreach(var attr in root.Attributes) {
                Assert.True(attr.Name == "??????");
                Assert.True(attr.Value == "3");
                var (name, value) = attr;
                Assert.True(name == "??????");
                Assert.True(value == "3");
                break;
            }

            // Test children enumeration via interface
            {
                var first = root.Children.First();
                Assert.True(first.Name == "???????????????");
                Assert.True(first.InnerText == "???????????????");
                Assert.True(first.HasAttribute == false);
                Assert.True(first.Attributes.Count == 0);
                Assert.True(first.HasChildren == false);
                Assert.True(first.Children.Count == 0);

                var second = root.Children.ElementAt(1);
                Assert.True(second.Name == "abc");
                Assert.True(second.InnerText == "15 / 3 > A && -3 < B");
                Assert.True(second.HasAttribute);
                Assert.True(second.Attributes.Count == 1);
                Assert.True(second.HasChildren == false);
                Assert.True(second.Children.Count == 0);

                // Resolve entity
                {
                    var entities = xml.EntityTable;
                    var attr = second.Attributes.First();
                    Assert.True(attr.Value == "123&foo;456&#8721;&#x2211;");
                    Assert.True(entities.CheckNeedToResolve(attr.Value, out var len) == XmlEntityResolverState.NeedToResolve);
                    Assert.True(entities.GetResolvedByteLength(attr.Value) == len);
                    var resolved = entities.Resolve(attr.Value);
                    Assert.True(resolved.Length == len);
                    var buf = new byte[len];
                    Assert.True(entities.Resolve(attr.Value, buf) == len);
                    Assert.True(buf.SequenceEqual(resolved));
                    Assert.True(UTF8ExceptionFallbackEncoding.Instance.GetString(resolved) == "123??????456??????");
                    Assert.True(entities.ResolveToString(attr.Value) == "123??????456??????");
                }
            }

            // Test children enumeration directly
            {
                int i = 0;
                foreach(var child in root.Children) {
                    if(i == 0) {
                        Assert.True(child.Name == "???????????????");
                        Assert.True(child.InnerText == "???????????????");
                        Assert.True(child.HasAttribute == false);
                        Assert.True(child.Attributes.Count == 0);
                        Assert.True(child.HasChildren == false);
                        Assert.True(child.Children.Count == 0);
                    }
                    else if(i == 1) {
                        Assert.True(child.Name == "abc");
                        Assert.True(child.InnerText == "15 / 3 > A && -3 < B");
                        Assert.True(child.HasAttribute);
                        Assert.True(child.Attributes.Count == 1);
                        Assert.True(child.HasChildren == false);
                        Assert.True(child.Children.Count == 0);

                        // Resolve entity
                        {
                            var entities = xml.EntityTable;
                            var attr = child.Attributes.First();
                            Assert.True(attr.Value == "123&foo;456&#8721;&#x2211;");
                            Assert.True(entities.CheckNeedToResolve(attr.Value, out var len) == XmlEntityResolverState.NeedToResolve);
                            Assert.True(entities.GetResolvedByteLength(attr.Value) == len);
                            var resolved = entities.Resolve(attr.Value);
                            Assert.True(resolved.Length == len);
                            var buf = new byte[len];
                            Assert.True(entities.Resolve(attr.Value, buf) == len);
                            Assert.True(buf.SequenceEqual(resolved));
                            Assert.True(UTF8ExceptionFallbackEncoding.Instance.GetString(resolved) == "123??????456??????");
                            Assert.True(entities.ResolveToString(attr.Value) == "123??????456??????");
                        }
                    }
                    i++;
                }
            }
        }

        private static IEnumerable<XmlNode> GetChildrenRecursively(XmlNode node)
        {
            // The order is depth first search.

            foreach(var child in node.Children) {
                yield return child;
                foreach(var c2 in GetChildrenRecursively(child)) {
                    yield return c2;
                }
            }
        }
    }
}
