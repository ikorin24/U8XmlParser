# U8XmlParser

[![GitHub license](https://img.shields.io/github/license/ikorin24/U8XmlParser?color=FF8821)](https://github.com/ikorin24/U8XmlParser/blob/master/LICENSE)
[![nuget](https://img.shields.io/badge/nuget-v1.0.1-FF8821)](https://www.nuget.org/packages/U8XmlParser)

High performance, thread-safe and IL2CPP-safe C# xml parser.

U8XmlParser is faster than any other xml libraries. (See the following benchmark for more info)

## Install

### For .NET

.net standard2.0, 2.1, .net framework4.8, .net core3.1, .net5 are supported.

Install from [Nuget package](https://www.nuget.org/packages/U8XmlParser/)

```sh
$ dotnet add package U8XmlParser
```

### For Unity

If Unity 2020 or newer, install the package from UPM by git URL.

git URL: (https://github.com/ikorin24/U8XmlParser.git?path=src/U8XmlParserUnity/Assets/Plugins#v1.0.1)

For Unity 2019 or older, add the following libraries built for .net standard2.0 to your project.
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

using (Stream stream = File.OpenRead("your_file.xml"))
using (XmlObject xml = XmlParser.Parse(stream))
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

using (Stream stream = File.OpenRead("your_file.xml"))
using (XmlObject xml = XmlParser.Parse(stream))
{
    RawString innerText = xml.Root.InnerText;   // "&lt;foo&gt;"
    XmlEntityTable entities = xml.EntityTable;
    string resolvedText = entities.ResolveToString(innerText);  // "<foo>"
}
```

## Benchmark

### Benchmarking Yourself

```cs
$ cd src/Benchmark
$ dotnet run -c Release -f net5.0
```

### Results (on .net5)

Benchmarked by [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).

#### Large XML File (about 1MB)

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-10700 CPU 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=5.0.202
  [Host]     : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  Job-KJNRQJ : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Jit=RyuJit  Platform=X64  InvocationCount=1  
IterationCount=100  UnrollFactor=1  

```
|                     Method |      Mean |    Error |   StdDev | Ratio | RatioSD | Rank |      Gen 0 |     Gen 1 |     Gen 2 |  Allocated |
|--------------------------- |----------:|---------:|---------:|------:|--------:|-----:|-----------:|----------:|----------:|-----------:|
| &#39;U8Xml.XmlParser (my lib)&#39; |  21.86 ms | 0.057 ms | 0.152 ms |  1.00 |    0.00 |    1 |          - |         - |         - |       64 B |
|  System.Xml.Linq.XDocument | 117.67 ms | 1.979 ms | 5.834 ms |  5.38 |    0.28 |    3 |  7000.0000 | 4000.0000 | 1000.0000 | 51898640 B |
|     System.Xml.XmlDocument | 175.50 ms | 1.960 ms | 5.779 ms |  8.03 |    0.28 |    4 | 10000.0000 | 5000.0000 | 1000.0000 | 76709520 B |
|       System.Xml.XmlReader |  34.11 ms | 0.066 ms | 0.186 ms |  1.56 |    0.01 |    2 |          - |         - |         - |   132712 B |

---

#### Samll XML File (about 100KB)

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-10700 CPU 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=5.0.202
  [Host]     : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  Job-FURHHZ : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Jit=RyuJit  Platform=X64  InvocationCount=1  
IterationCount=100  UnrollFactor=1  

```
|                     Method |       Mean |    Error |    StdDev |     Median | Ratio | RatioSD | Rank | Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |-----------:|---------:|----------:|-----------:|------:|--------:|-----:|------:|------:|------:|----------:|
| &#39;U8Xml.XmlParser (my lib)&#39; |   245.7 μs |  2.46 μs |   6.90 μs |   243.4 μs |  1.00 |    0.00 |    1 |     - |     - |     - |      64 B |
|  System.Xml.Linq.XDocument | 1,110.2 μs |  2.44 μs |   7.05 μs | 1,108.9 μs |  4.52 |    0.13 |    3 |     - |     - |     - |  547336 B |
|     System.Xml.XmlDocument | 1,330.4 μs | 69.72 μs | 204.47 μs | 1,474.9 μs |  5.53 |    0.87 |    4 |     - |     - |     - |  796912 B |
|       System.Xml.XmlReader |   580.4 μs |  0.30 μs |   0.86 μs |   580.3 μs |  2.36 |    0.06 |    2 |     - |     - |     - |   29360 B |



## License

Author: [ikorin24](https://github.com/ikorin24), and all contributors.

This repository is licensed under [MIT](https://github.com/ikorin24/U8XmlParser/blob/master/LICENSE).

See Licenses of other libraries this repository contains from [here](https://github.com/ikorin24/U8XmlParser/blob/master/NOTICE.md).

