# U8XmlParser

[![GitHub license](https://img.shields.io/github/license/ikorin24/U8XmlParser?color=FF8821)](https://github.com/ikorin24/U8XmlParser/blob/master/LICENSE)
[![nuget](https://img.shields.io/badge/nuget-v1.5.0-FF8821)](https://www.nuget.org/packages/U8XmlParser)
[![openupm](https://img.shields.io/npm/v/com.ikorin24.u8xmlparser?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.ikorin24.u8xmlparser/)
[![Build and Test](https://github.com/ikorin24/U8XmlParser/actions/workflows/test.yml/badge.svg)](https://github.com/ikorin24/U8XmlParser/actions/workflows/test.yml)

High performance, thread-safe and IL2CPP-safe C# xml parser.

U8XmlParser is faster than any other xml libraries. (See the following benchmark for more info)

## Install

### For .NET

.net standard2.0, 2.1, .net framework4.8, .net core3.1, .net5, .net6 are supported.

Install from [Nuget package](https://www.nuget.org/packages/U8XmlParser/)

```sh
$ dotnet add package U8XmlParser
```

### For Unity

#### **Unity 2020 or newer**

Install the package from OpenUPM.
See [OpenUPM](https://openupm.com/packages/com.ikorin24.u8xmlparser/) for details.

```sh
$ openupm add com.ikorin24.u8xmlparser
```

Or install the package from UPM by git URL.

git URL: (https://github.com/ikorin24/U8XmlParser.git?path=src/U8XmlParserUnity/Assets/Plugins#v1.5.0)

#### **Unity 2019 or older**

Add the following libraries built for .net standard2.0 to your project.
You can get them at the [release page](https://github.com/ikorin24/U8XmlParser/releases).

- U8XmlParser.dll (built for .net standard 2.0)
- System.Buffers.dll
- System.Memory.dll
- System.Runtime.CompilerServices.Unsafe.dll

## How to Build

It requires dotnet cli tools. (or Visual Studio)

```sh
$ git clone https://github.com/ikorin24/U8XmlParser.git
$ cd U8XmlParser
$ dotnet src/U8XmlParser/U8XmlParser.csproj -c Release
```

## How to Use

### General usage

```cs
/* ------- your_file.xml ----------

<?xml version="1.0" encoding="UTF-8">
<SomeData>
    <Data aa="20">bbb</Data>
    <Data aa="30">ccc</Data>
</SomeData>

----------------------------------- */

// using System.IO;
// using U8Xml;

using (XmlObject xml = XmlParser.ParseFile("your_file.xml"))
{
    XmlNode root = xml.Root;
    string rootName = root.Name.ToString();         // "SomeData"

    // node
    XmlNode child = root.Children.First();
    string childName = child.Name.ToString();       // "Data"
    string innerText = child.InnerText.ToString();  // "bbb"
    
    // attribute
    XmlAttribute attr = child.Attributes.First();
    string attrName = attr.Name.ToString();         // "aa"
    int attrValue = attr.Value.ToInt32();           // 20

    // children nodes enumeration
    foreach(XmlNode node in root.Children)
    {
        // do something
    }
}
// *** DO NOT use any object from the parser. ***
// XmlObject, XmlNode, RawString, etc... are no longer accessible here.
// Their all memories are released when XmlObject.Dispose() called !!
// They must be evaluated and converted to string, int, and so on.
```

### Entity resolving

Some xml has DTD; Document Type Declaration, and it can contain Entity Declaration. Entity defines alias of string in the xml. (For example, `'lt'` is defined as alias of `'<'` by default. We have to resolve it when  appears `&lt;` in xml.)

```cs
/* ------- your_file.xml ----------

<?xml version="1.0" encoding="UTF-8">
<SomeData>
    &lt;foo&gt;
</SomeData>

----------------------------------- */

// using System.IO;
// using U8Xml;

using (XmlObject xml = XmlParser.ParseFile("your_file.xml"))
{
    RawString innerText = xml.Root.InnerText;   // "&lt;foo&gt;"
    XmlEntityTable entities = xml.EntityTable;
    string resolvedText = entities.ResolveToString(innerText);  // "<foo>"
}
```

## Benchmark

### Benchmarking by yourself

```cs
$ cd src/Benchmark
$ dotnet run -c Release -f net6.0
```

### Results (on .net6)

Benchmarked by [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).

#### Large XML File (about 1MB)

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1348 (20H2/October2020Update)
Intel Core i7-10700 CPU 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  Job-BZPFMV : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Jit=RyuJit  Platform=X64  IterationCount=100  

```
|                     Method |      Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 |     Gen 2 |    Allocated |
|--------------------------- |----------:|---------:|---------:|------:|--------:|-----------:|----------:|----------:|-------------:|
| &#39;U8Xml.XmlParser (my lib)&#39; |  20.45 ms | 0.053 ms | 0.150 ms |  1.00 |    0.00 |          - |         - |         - |         90 B |
|  System.Xml.Linq.XDocument | 124.51 ms | 0.863 ms | 2.543 ms |  6.10 |    0.13 |  7200.0000 | 4000.0000 | 1200.0000 | 51,899,029 B |
|     System.Xml.XmlDocument | 172.12 ms | 0.842 ms | 2.415 ms |  8.42 |    0.15 | 10000.0000 | 5333.3333 | 1666.6667 | 76,710,869 B |
|       System.Xml.XmlReader |  26.10 ms | 0.041 ms | 0.115 ms |  1.28 |    0.01 |          - |         - |         - |    132,726 B |

---

#### Samll XML File (about 100KB)

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1348 (20H2/October2020Update)
Intel Core i7-10700 CPU 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  Job-VXFJEN : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Jit=RyuJit  Platform=X64  IterationCount=100  

```
|                     Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen 0 |   Gen 1 | Allocated |
|--------------------------- |---------:|--------:|--------:|------:|--------:|--------:|--------:|----------:|
| &#39;U8Xml.XmlParser (my lib)&#39; | 163.9 μs | 0.22 μs | 0.63 μs |  1.00 |    0.00 |       - |       - |      64 B |
|  System.Xml.Linq.XDocument | 604.8 μs | 0.41 μs | 1.17 μs |  3.69 |    0.02 | 64.4531 | 13.6719 | 546,186 B |
|     System.Xml.XmlDocument | 794.1 μs | 0.77 μs | 2.18 μs |  4.84 |    0.02 | 94.7266 | 46.8750 | 796,905 B |
|       System.Xml.XmlReader | 271.6 μs | 0.28 μs | 0.81 μs |  1.66 |    0.01 |  3.4180 |       - |  29,352 B |



## License

Author: [ikorin24](https://github.com/ikorin24), and all contributors.

This repository is licensed under [MIT](https://github.com/ikorin24/U8XmlParser/blob/master/LICENSE).

See Licenses of other libraries this repository contains from [here](https://github.com/ikorin24/U8XmlParser/blob/master/NOTICE.md).

