#nullable enable
using Xunit;
using U8Xml;
using StringLiteral;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace UnitTest
{
    public class RawStringTest
    {
        [Fact]
        public void Int()
        {
            var checks = new Check<int>[]
            {
                new(1, RawStringSource.Get("1")),
                new(-1, RawStringSource.Get("-1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(0, RawStringSource.Get("-0")),
                new(-3, RawStringSource.Get("-03")),
                new(1234, RawStringSource.Get("1234")),
                new(-1234, RawStringSource.Get("-1234")),
                new(int.MaxValue, RawStringSource.Get("2147483647")),
                new(int.MinValue, RawStringSource.Get("-2147483648")),
                new(95, RawStringSource.Get("+95")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToInt32(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToInt32());
            }
        }

        [Fact]
        public void UInt()
        {
            var checks = new Check<uint>[]
            {
                new(1, RawStringSource.Get("1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(1234, RawStringSource.Get("1234")),
                new(uint.MaxValue, RawStringSource.Get("4294967295")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToUInt32(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToUInt32());
            }
        }

        [Fact]
        public void Long()
        {
            var checks = new Check<long>[]
            {
                new(1, RawStringSource.Get("1")),
                new(-1, RawStringSource.Get("-1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(0, RawStringSource.Get("-0")),
                new(-3, RawStringSource.Get("-03")),
                new(1234, RawStringSource.Get("1234")),
                new(-1234, RawStringSource.Get("-1234")),
                new(long.MaxValue, RawStringSource.Get("9223372036854775807")),
                new(long.MinValue, RawStringSource.Get("-9223372036854775808")),
                new(95, RawStringSource.Get("+95")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToInt64(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToInt64());
            }
        }

        [Fact]
        public void ULong()
        {
            var checks = new Check<ulong>[]
            {
                new(1, RawStringSource.Get("1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(1234, RawStringSource.Get("1234")),
                new(ulong.MaxValue, RawStringSource.Get("18446744073709551615")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToUInt64(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToUInt64());
            }
        }

        [Fact]
        public void Short()
        {
            var checks = new Check<short>[]
            {
                new(1, RawStringSource.Get("1")),
                new(-1, RawStringSource.Get("-1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(0, RawStringSource.Get("-0")),
                new(-3, RawStringSource.Get("-03")),
                new(1234, RawStringSource.Get("1234")),
                new(-1234, RawStringSource.Get("-1234")),
                new(short.MaxValue, RawStringSource.Get("32767")),
                new(short.MinValue, RawStringSource.Get("-32768")),
                new(95, RawStringSource.Get("+95")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToInt16(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToInt16());
            }
        }

        [Fact]
        public void UShort()
        {
            var checks = new Check<ushort>[]
            {
                new(1, RawStringSource.Get("1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(1234, RawStringSource.Get("1234")),
                new(ushort.MaxValue, RawStringSource.Get("65535")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToUInt16(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToUInt16());
            }
        }

        [Fact]
        public void SByte()
        {
            var checks = new Check<sbyte>[]
            {
                new(1, RawStringSource.Get("1")),
                new(-1, RawStringSource.Get("-1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(0, RawStringSource.Get("-0")),
                new(-3, RawStringSource.Get("-03")),
                new(sbyte.MaxValue, RawStringSource.Get("127")),
                new(sbyte.MinValue, RawStringSource.Get("-128")),
                new(95, RawStringSource.Get("+95")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToInt8(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToInt8());
            }
        }

        [Fact]
        public void Byte()
        {
            var checks = new Check<byte>[]
            {
                new(1, RawStringSource.Get("1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(byte.MaxValue, RawStringSource.Get("255")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToUInt8(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToUInt8());
            }
        }

        [Fact]
        public void Float()
        {
            Assert.True(float.IsNegativeInfinity(RawStringSource.Get("-∞").ToFloat32()));
            Assert.True(float.IsPositiveInfinity(RawStringSource.Get("+∞").ToFloat32()));
            Assert.True(float.IsPositiveInfinity(RawStringSource.Get("∞").ToFloat32()));

            var checks = new Check<float>[]
            {
                new(1, RawStringSource.Get("1")),
                new(-1, RawStringSource.Get("-1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(0, RawStringSource.Get("-0")),
                new(-3, RawStringSource.Get("-03")),
                new(1234, RawStringSource.Get("1234")),
                new(-1234, RawStringSource.Get("-1234")),
                new(long.MaxValue, RawStringSource.Get("9223372036854775807")),
                new(long.MinValue, RawStringSource.Get("-9223372036854775808")),
                new(95, RawStringSource.Get("+95")),
                new(-4.8e-9f, RawStringSource.Get("-4.8e-9")),
                new(+0.4E+9f, RawStringSource.Get("+0.4E+9")),
                new(-0e0f, RawStringSource.Get("-0e0")),
                new(03e008f, RawStringSource.Get("03e008")),
                new(03e-008f, RawStringSource.Get("03e-008")),
                new(1E-45f, RawStringSource.Get("1E-45")),
                new(0.34E+39f, RawStringSource.Get("0.34E+39")),
                new(4e9f, RawStringSource.Get("4e9")),
                new(float.NaN, RawStringSource.Get("nan")),
                new(-float.NaN, RawStringSource.Get("-NAN")),
                new(float.NaN, RawStringSource.Get("+NaN")),
            };

            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToFloat32(out var result));
                Assert.Equal(ans, result, 5);
                Assert.Equal(ans, input.ToFloat32(), 5);
            }

            return;
        }

        [Fact]
        public void Double()
        {
            Assert.True(double.IsNegativeInfinity(RawStringSource.Get("-∞").ToFloat32()));
            Assert.True(double.IsPositiveInfinity(RawStringSource.Get("+∞").ToFloat32()));
            Assert.True(double.IsPositiveInfinity(RawStringSource.Get("∞").ToFloat32()));

            var checks = new Check<double>[]
            {
                new(1, RawStringSource.Get("1")),
                new(-1, RawStringSource.Get("-1")),
                new(0, RawStringSource.Get("0")),
                new(0, RawStringSource.Get("000")),
                new(0, RawStringSource.Get("-0")),
                new(-3, RawStringSource.Get("-03")),
                new(1234, RawStringSource.Get("1234")),
                new(-1234, RawStringSource.Get("-1234")),
                new(long.MaxValue, RawStringSource.Get("9223372036854775807")),
                new(long.MinValue, RawStringSource.Get("-9223372036854775808")),
                new(95, RawStringSource.Get("+95")),
                new(-4.8e-9, RawStringSource.Get("-4.8e-9")),
                new(+0.4E+9, RawStringSource.Get("+0.4E+9")),
                new(-0e0, RawStringSource.Get("-0e0")),
                new(03e008, RawStringSource.Get("03e008")),
                new(03e-008, RawStringSource.Get("03e-008")),
                new(1E-45, RawStringSource.Get("1E-45")),
                new(0.34E+39, RawStringSource.Get("0.34E+39")),
                new(4e9, RawStringSource.Get("4e9")),
                new(double.NaN, RawStringSource.Get("nan")),
                new(-double.NaN, RawStringSource.Get("-NAN")),
                new(double.NaN, RawStringSource.Get("+NaN")),
                new(17E+307, RawStringSource.Get("17E+307")),
            };

            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToFloat64(out var result));
                Assert.Equal(ans, result, 12);
                Assert.Equal(ans, input.ToFloat64(), 12);
            }

            return;
        }

        [Fact]
        public void ToUpper()
        {
            var testCases = new (RawString input, RawString expected)[]
            {
                (RawStringSource.Get("あいうえお"), RawStringSource.Get("あいうえお")),
                (RawStringSource.Get("あa0"), RawStringSource.Get("あA0")),
                (RawStringSource.Get("abcde"), RawStringSource.Get("ABCDE")),
            };

            foreach(var (input, expected) in testCases) {
                Assert.True(expected.SequenceEqual(input.ToUpper()));
            }

            foreach(var (input, expected) in testCases) {
                var buf = new byte[input.Length];
                input.ToUpper(buf);
                Assert.True(expected.SequenceEqual(buf));
            }
        }

        [Fact]
        public void ToLower()
        {
            var testCases = new (RawString input, RawString expected)[]
            {
                (RawStringSource.Get("あいうえお"), RawStringSource.Get("あいうえお")),
                (RawStringSource.Get("あA0"), RawStringSource.Get("あa0")),
                (RawStringSource.Get("ABCDE"), RawStringSource.Get("abcde")),
            };

            foreach(var (input, expected) in testCases) {
                Assert.True(expected.SequenceEqual(input.ToLower()));
            }

            foreach(var (input, expected) in testCases) {
                var buf = new byte[input.Length];
                input.ToLower(buf);
                Assert.True(expected.SequenceEqual(buf));
            }
        }

        [Fact]
        public void Slice()
        {
            var hoge = RawStringSource.Get("hoge");
            Assert.True(hoge.Slice(0, 4) == hoge);
            Assert.True(hoge.Slice(0) == hoge);
            Assert.True(hoge.Slice(3).Length == 1);
            Assert.True(hoge.Slice(3)[0] == 'e');
            Assert.True(hoge.Slice(4).IsEmpty);
        }

        [Fact]
        public unsafe void Split()
        {
            {
                var ab_cd = RawStringSource.Get("ab cd");
                var (ab, cd) = ab_cd.Split2((byte)' ');
                Assert.True(ab == ab_cd.Slice(0, 2));
                Assert.True(cd == ab_cd.Slice(3, 2));
            }
            {
                var ab_cd_ef_gh = RawStringSource.Get("ab cd ef gh");
                var (ab, cd_ef_gh) = ab_cd_ef_gh.Split2((byte)' ');
                Assert.True(ab == ab_cd_ef_gh.Slice(0, 2));
                Assert.True(cd_ef_gh == ab_cd_ef_gh.Slice(3));
            }
            {
                var hoge = RawStringSource.Get("hoge");
                var (a, b) = hoge.Split2((byte)' ');
                Assert.True(a == hoge);
                Assert.True(hoge.Slice(4).IsEmpty);
                Assert.True(b.IsEmpty);
            }
        }

        [Fact]
        public void Trim()
        {
            {
                var foo = RawStringSource.Get(" \r\n\t foo \r\n\t ");
                Assert.True(foo.TrimEnd() == " \r\n\t foo");
            }
            {
                var foo = RawStringSource.Get(" \r\n\t foo \r\n\t ");
                Assert.True(foo.TrimStart() == "foo \r\n\t ");
            }
            {
                var foo = RawStringSource.Get(" \r\n\t foo \r\n\t ");
                Assert.True(foo.Trim() == "foo");
            }
        }

        [Fact]
        public void StartsWith()
        {
            const string str1 = "あいうえお";
            const string str2 = "あいう";
            const string str3 = "えお";
            var rawStr1 = RawStringSource.Get("あいうえお");
            var rawStr2 = RawStringSource.Get("あいう");
            var rawStr3 = RawStringSource.Get("えお");

            // [Assert true]
            {
                // "あいうえお" starts with "あいうえお"
                Assert.True(rawStr1.StartsWith(rawStr1));                    // RawString -- RawString
                Assert.True(rawStr1.StartsWith(rawStr1.AsSpan()));           // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr1.StartsWith(str1));                       // RawString -- string
                Assert.True(rawStr1.StartsWith(str1.AsSpan()));              // RawString -- ReadOnlySpan<char>

                // "あいうえお" starts with "あいう"
                Assert.True(rawStr1.StartsWith(rawStr2));                    // RawString -- RawString
                Assert.True(rawStr1.StartsWith(rawStr2.AsSpan()));           // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr1.StartsWith(str2));                       // RawString -- string
                Assert.True(rawStr1.StartsWith(str2.AsSpan()));              // RawString -- ReadOnlySpan<char>

                // "あいうえお" starts with ""
                Assert.True(rawStr1.StartsWith(RawString.Empty));            // RawString -- RawString
                Assert.True(rawStr1.StartsWith(ReadOnlySpan<byte>.Empty));   // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr1.StartsWith(string.Empty));               // RawString -- string
                Assert.True(rawStr1.StartsWith(ReadOnlySpan<char>.Empty));   // RawString -- ReadOnlySpan<char>

                // "あいう" starts with "あいう"
                Assert.True(rawStr2.StartsWith(rawStr2));                    // RawString -- RawString
                Assert.True(rawStr2.StartsWith(rawStr2.AsSpan()));           // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr2.StartsWith(str2));                       // RawString -- string
                Assert.True(rawStr2.StartsWith(str2.AsSpan()));              // RawString -- ReadOnlySpan<char>

                // "あいう" starts with ""
                Assert.True(rawStr2.StartsWith(RawString.Empty));            // RawString -- RawString
                Assert.True(rawStr2.StartsWith(ReadOnlySpan<byte>.Empty));   // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr2.StartsWith(string.Empty));               // RawString -- string
                Assert.True(rawStr2.StartsWith(ReadOnlySpan<char>.Empty));   // RawString -- ReadOnlySpan<char>
            }

            // [Assert false]
            {
                // "あいうえお" does not start with "えお"
                Assert.False(rawStr1.StartsWith(rawStr3));                    // RawString -- RawString
                Assert.False(rawStr1.StartsWith(rawStr3.AsSpan()));           // RawString -- ReadOnlySpan<byte>
                Assert.False(rawStr1.StartsWith(str3));                       // RawString -- string
                Assert.False(rawStr1.StartsWith(str3.AsSpan()));              // RawString -- ReadOnlySpan<char>

                // "あいう" does not start with "えお"
                Assert.False(rawStr2.StartsWith(rawStr3));                    // RawString -- RawString
                Assert.False(rawStr2.StartsWith(rawStr3.AsSpan()));           // RawString -- ReadOnlySpan<byte>
                Assert.False(rawStr2.StartsWith(str3));                       // RawString -- string
                Assert.False(rawStr2.StartsWith(str3.AsSpan()));              // RawString -- ReadOnlySpan<char>
            }
        }

        [Fact]
        public void EndsWith()
        {
            const string str1 = "あいうえお";
            const string str2 = "あいう";
            const string str3 = "えお";
            var rawStr1 = RawStringSource.Get("あいうえお");
            var rawStr2 = RawStringSource.Get("あいう");
            var rawStr3 = RawStringSource.Get("えお");

            // [Assert true]
            {
                // "あいうえお" ends with "あいうえお"
                Assert.True(rawStr1.EndsWith(rawStr1));                     // RawString -- RawString
                Assert.True(rawStr1.EndsWith(rawStr1.AsSpan()));            // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr1.EndsWith(str1));                        // RawString -- string
                Assert.True(rawStr1.EndsWith(str1.AsSpan()));               // RawString -- ReadOnlySpan<char>

                // "あいうえお" ends with "えお"
                Assert.True(rawStr1.EndsWith(rawStr3));                     // RawString -- RawString
                Assert.True(rawStr1.EndsWith(rawStr3.AsSpan()));            // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr1.EndsWith(str3));                        // RawString -- string
                Assert.True(rawStr1.EndsWith(str3.AsSpan()));               // RawString -- ReadOnlySpan<char>

                // "あいうえお" ends with ""
                Assert.True(rawStr1.EndsWith(RawString.Empty));             // RawString -- RawString
                Assert.True(rawStr1.EndsWith(ReadOnlySpan<byte>.Empty));    // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr1.EndsWith(string.Empty));                // RawString -- string
                Assert.True(rawStr1.EndsWith(ReadOnlySpan<char>.Empty));    // RawString -- ReadOnlySpan<char>

                // "あいう" ends with "あいう"
                Assert.True(rawStr2.EndsWith(rawStr2));                     // RawString -- RawString
                Assert.True(rawStr2.EndsWith(rawStr2.AsSpan()));            // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr2.EndsWith(str2));                        // RawString -- string
                Assert.True(rawStr2.EndsWith(str2.AsSpan()));               // RawString -- ReadOnlySpan<char>

                // "あいう" ends with ""
                Assert.True(rawStr2.EndsWith(RawString.Empty));             // RawString -- RawString
                Assert.True(rawStr2.EndsWith(ReadOnlySpan<byte>.Empty));    // RawString -- ReadOnlySpan<byte>
                Assert.True(rawStr2.EndsWith(string.Empty));                // RawString -- string
                Assert.True(rawStr2.EndsWith(ReadOnlySpan<char>.Empty));    // RawString -- ReadOnlySpan<char>
            }

            // [Assert false]
            {
                // "あいうえお" does not end with "あいう"
                Assert.False(rawStr1.EndsWith(rawStr2));                    // RawString -- RawString
                Assert.False(rawStr1.EndsWith(rawStr2.AsSpan()));           // RawString -- ReadOnlySpan<byte>
                Assert.False(rawStr1.EndsWith(str2));                       // RawString -- string
                Assert.False(rawStr1.EndsWith(str2.AsSpan()));              // RawString -- ReadOnlySpan<char>

                // "あいう" does not end with "えお"
                Assert.False(rawStr2.EndsWith(rawStr3));                    // RawString -- RawString
                Assert.False(rawStr2.EndsWith(rawStr3.AsSpan()));           // RawString -- ReadOnlySpan<byte>
                Assert.False(rawStr2.EndsWith(str3));                       // RawString -- string
                Assert.False(rawStr2.EndsWith(str3.AsSpan()));              // RawString -- ReadOnlySpan<char>
            }
        }

        private readonly struct Check<T>
        {
            public readonly T Answer;
            public readonly RawString Input;

            public Check(T ans, RawString input) => (Answer, Input) = (ans, input);
            public void Deconstruct(out T ans, out RawString input) => (ans, input) = (Answer, Input);
        }
    }
    
    internal static partial class RawStringSource
    {
        private static readonly Dictionary<string, RawString> _dic;

        [Utf8("ab cd")]
        private static partial ReadOnlySpan<byte> Str1();
        [Utf8("ab cd ef gh")]
        private static partial ReadOnlySpan<byte> Str2();
        [Utf8("hoge")]
        private static partial ReadOnlySpan<byte> Str3();
        [Utf8(" \r\n\t foo \r\n\t ")]
        private static partial ReadOnlySpan<byte> Str4();
        [Utf8("1")]
        private static partial ReadOnlySpan<byte> Str5();
        [Utf8("-1")]
        private static partial ReadOnlySpan<byte> Str6();
        [Utf8("0")]
        private static partial ReadOnlySpan<byte> Str7();
        [Utf8("000")]
        private static partial ReadOnlySpan<byte> Str8();
        [Utf8("-0")]
        private static partial ReadOnlySpan<byte> Str9();
        [Utf8("-03")]
        private static partial ReadOnlySpan<byte> Str10();
        [Utf8("1234")]
        private static partial ReadOnlySpan<byte> Str11();
        [Utf8("-1234")]
        private static partial ReadOnlySpan<byte> Str12();
        [Utf8("2147483647")]
        private static partial ReadOnlySpan<byte> Str13();
        [Utf8("-2147483648")]
        private static partial ReadOnlySpan<byte> Str14();
        [Utf8("4294967295")]
        private static partial ReadOnlySpan<byte> Str15();
        [Utf8("+95")]
        private static partial ReadOnlySpan<byte> Str16();

        [Utf8("9223372036854775807")]
        private static partial ReadOnlySpan<byte> Str17();
        [Utf8("-9223372036854775808")]
        private static partial ReadOnlySpan<byte> Str18();
        [Utf8("18446744073709551615")]
        private static partial ReadOnlySpan<byte> Str19();
        [Utf8("32767")]
        private static partial ReadOnlySpan<byte> Str20();
        [Utf8("-32768")]
        private static partial ReadOnlySpan<byte> Str21();
        [Utf8("65535")]
        private static partial ReadOnlySpan<byte> Str22();
        [Utf8("127")]
        private static partial ReadOnlySpan<byte> Str23();
        [Utf8("-128")]
        private static partial ReadOnlySpan<byte> Str24();
        [Utf8("255")]
        private static partial ReadOnlySpan<byte> Str25();
        [Utf8("∞")]
        private static partial ReadOnlySpan<byte> Str26();
        [Utf8("+∞")]
        private static partial ReadOnlySpan<byte> Str27();
        [Utf8("-∞")]
        private static partial ReadOnlySpan<byte> Str28();

        [Utf8("あいうえお")]
        private static partial ReadOnlySpan<byte> Str29();
        [Utf8("あa0")]
        private static partial ReadOnlySpan<byte> Str30();
        [Utf8("あA0")]
        private static partial ReadOnlySpan<byte> Str31();
        [Utf8("abcde")]
        private static partial ReadOnlySpan<byte> Str32();
        [Utf8("ABCDE")]
        private static partial ReadOnlySpan<byte> Str33();
        [Utf8("-4.8e-9")]
        private static partial ReadOnlySpan<byte> Str34();
        [Utf8("+0.4E+9")]
        private static partial ReadOnlySpan<byte> Str35();
        [Utf8("-0e0")]
        private static partial ReadOnlySpan<byte> Str36();
        [Utf8("03e008")]
        private static partial ReadOnlySpan<byte> Str37();
        [Utf8("03e-008")]
        private static partial ReadOnlySpan<byte> Str38();
        [Utf8("1E-45")]
        private static partial ReadOnlySpan<byte> Str39();
        [Utf8("0.34E+39")]
        private static partial ReadOnlySpan<byte> Str40();
        [Utf8("4e9")]
        private static partial ReadOnlySpan<byte> Str41();
        [Utf8("nan")]
        private static partial ReadOnlySpan<byte> Str42();
        [Utf8("-NAN")]
        private static partial ReadOnlySpan<byte> Str43();
        [Utf8("+NaN")]
        private static partial ReadOnlySpan<byte> Str44();
        [Utf8("17E+307")]
        private static partial ReadOnlySpan<byte> Str45();
        [Utf8("あいう")]
        private static partial ReadOnlySpan<byte> Str46();
        [Utf8("えお")]
        private static partial ReadOnlySpan<byte> Str47();

        static RawStringSource()
        {
            _dic = new Dictionary<string, RawString>();
            Register(Str1());
            Register(Str2());
            Register(Str3());
            Register(Str4());
            Register(Str5());
            Register(Str6());
            Register(Str7());
            Register(Str8());
            Register(Str9());
            Register(Str10());
            Register(Str11());
            Register(Str12());
            Register(Str13());
            Register(Str14());
            Register(Str15());
            Register(Str16());
            Register(Str17());
            Register(Str18());
            Register(Str19());
            Register(Str20());
            Register(Str21());
            Register(Str22());
            Register(Str23());
            Register(Str24());
            Register(Str25());
            Register(Str26());
            Register(Str27());
            Register(Str28());
            Register(Str29());
            Register(Str30());
            Register(Str31());
            Register(Str32());
            Register(Str33());
            Register(Str34());
            Register(Str35());
            Register(Str36());
            Register(Str37());
            Register(Str38());
            Register(Str39());
            Register(Str40());
            Register(Str41());
            Register(Str42());
            Register(Str43());
            Register(Str44());
            Register(Str45());
            Register(Str46());
            Register(Str47());

            static unsafe void Register(ReadOnlySpan<byte> s)
            {
                var ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(s));
                var str = Encoding.UTF8.GetString(s.ToArray());
                _dic[str] = new RawString(ptr, s.Length);
            }
        }

        public static RawString Get(string str)
        {
            return _dic[str];
        }
    }
}
