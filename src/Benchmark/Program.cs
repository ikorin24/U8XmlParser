#nullable enable
using System;
using System.IO;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

namespace U8Xml.Benchmark
{
    class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<ParserBenchmark>();
        }
    }

    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [RyuJitX64Job]
    [IterationCount(100)]
    [RankColumn]
    public class ParserBenchmark
    {
        private readonly string _filePath;
        private Stream? _stream;


        public ParserBenchmark()
        {
            // Remove comment-out to switch the file

            //var name = "small.xml";
            var name = "large.xml";

#if NET48
            _filePath = Path.Combine(AppContext.BaseDirectory, "../../../data/", name);
#else
            _filePath = Path.Combine(AppContext.BaseDirectory, "../../../../../../../data/", name);
#endif
        }

        [IterationSetup]
        public void Setup()
        {
            _stream = File.OpenRead(_filePath);
        }

        [IterationCleanup]
        public void Cleanup()
        {
            _stream?.Dispose();
            _stream = null;
        }

        [Benchmark(Baseline = true, Description = "U8Xml.XmlParser (my lib)")]
        public void U8XmlParser()
        {
            var stream = _stream!;
            using var xml = U8Xml.XmlParser.Parse(stream);
        }

        [Benchmark(Description = "System.Xml.Linq.XDocument")]
        public void XDocument()
        {
            var stream = _stream!;
            var xml = System.Xml.Linq.XDocument.Load(stream);
        }

        [Benchmark(Description = "System.Xml.XmlDocument")]
        public void XmlDocument()
        {
            var stream = _stream!;
            var xml = new System.Xml.XmlDocument();
            xml.Load(stream);
        }

        [Benchmark(Description = "System.Xml.XmlReader")]
        public void XmlReader()
        {
            var stream = _stream!;
            using var reader = System.Xml.XmlReader.Create(stream);
            while(reader.Read()) {
            }
        }
    }
}
