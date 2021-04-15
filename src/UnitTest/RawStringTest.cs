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
                new(95, RawStringSource.Get("+95")),
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
                new(95, RawStringSource.Get("+95")),
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
                new(95, RawStringSource.Get("+95")),
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
                new(95, RawStringSource.Get("+95")),
            };
            foreach(var (ans, input) in checks) {
                Assert.True(input.TryToUInt8(out var result));
                Assert.Equal(ans, result);
                Assert.Equal(ans, input.ToUInt8());
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
