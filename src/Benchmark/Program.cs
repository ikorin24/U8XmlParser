﻿#nullable enable
using System.IO;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

namespace U8Xml.Benchmark
{
    class Program
    {
        public static void Main()
        {
            // Switch benchmarks

            //BenchmarkRunner.Run<ParserStreamBenchmark>();
            BenchmarkRunner.Run<ParserFileBenchmark>();
        }
    }

    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [RyuJitX64Job]
    [IterationCount(100)]
    public class ParserStreamBenchmark
    {
        private Stream? _stream;

        public ParserStreamBenchmark()
        {
            // Remove comment-out to switch the file

            //var name = "small.xml";
            var name = "large.xml";

            var filePath = Path.Combine("data", name);

            using(var file = File.OpenRead(filePath)) {
                var ms = new MemoryStream();
                file.CopyTo(ms);
                _stream = ms;
            }
        }

        // *** NOTE ***
        // Don't use IterationSetup and IterationCleanup. This benchmark is shorter than 100ms.
        // See https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html
        // 
        // > It's not recommended to use this attribute in microbenchmarks because it can spoil the results.
        // > However, if you are writing a macrobenchmark (e.g. a benchmark which takes at least 100ms)
        // > and you want to prepare some data before each invocation, [IterationSetup] can be useful.

        [Benchmark(Baseline = true, Description = "U8Xml.XmlParser (my lib)")]
        public void U8XmlParser()
        {
            var stream = _stream!;
            stream.Position = 0;
            using var xml = U8Xml.XmlParser.Parse(stream);
        }

        [Benchmark(Description = "System.Xml.Linq.XDocument")]
        public void XDocument()
        {
            var stream = _stream!;
            stream.Position = 0;
            var xml = System.Xml.Linq.XDocument.Load(stream);
        }

        [Benchmark(Description = "System.Xml.XmlDocument")]
        public void XmlDocument()
        {
            var stream = _stream!;
            stream.Position = 0;
            var xml = new System.Xml.XmlDocument();
            xml.Load(stream);
        }

        [Benchmark(Description = "System.Xml.XmlReader")]
        public void XmlReader()
        {
            var stream = _stream!;
            stream.Position = 0;
            using var reader = System.Xml.XmlReader.Create(stream);
            while(reader.Read()) {
            }
        }
    }

    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [RyuJitX64Job]
    [IterationCount(100)]
    public class ParserFileBenchmark
    {
        private string _filePath;

        public ParserFileBenchmark()
        {
            // Remove comment-out to switch the file

            //var name = "small.xml";
            var name = "large.xml";

            _filePath = Path.Combine("data", name);
        }

        // *** NOTE ***
        // Don't use IterationSetup and IterationCleanup. This benchmark is shorter than 100ms.
        // See https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html
        // 
        // > It's not recommended to use this attribute in microbenchmarks because it can spoil the results.
        // > However, if you are writing a macrobenchmark (e.g. a benchmark which takes at least 100ms)
        // > and you want to prepare some data before each invocation, [IterationSetup] can be useful.

        [Benchmark(Baseline = true, Description = "U8Xml.XmlParser (my lib)")]
        public void U8XmlParser_File()
        {
            using var xml = U8Xml.XmlParser.ParseFile(_filePath);
        }

        [Benchmark(Description = "System.Xml.Linq.XDocument")]
        public void XDocument_File()
        {
            var xml = System.Xml.Linq.XDocument.Load(_filePath);
        }

        [Benchmark(Description = "System.Xml.XmlDocument")]
        public void XmlDocument_File()
        {
            var xml = new System.Xml.XmlDocument();
            xml.Load(_filePath);
        }

        [Benchmark(Description = "System.Xml.XmlReader")]
        public void XmlReader_File()
        {
            using var reader = System.Xml.XmlReader.Create(_filePath);
            while(reader.Read()) {
            }
        }
    }

}
