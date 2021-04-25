#nullable enable
using System;
using StringLiteral;

namespace UnitTest
{
    internal static partial class Data
    {
        [Utf8(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<あいうえお ほげ=""3"">
    <かきくけこ>さしすせそ</かきくけこ>
    <abc>
        <![CDATA[15 / 3 > A && -3 < B]]>
    </abc>
</あいうえお>")]
        private static partial ReadOnlySpan<byte> Xml1();
        public static ReadOnlySpan<byte> Sample1 => Xml1();

        [Utf8(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- This is a comment. -->
<きらら 出版社=""芳文社"">
    <まんがタイムきららMAX>
        <ご注文はうさぎですか？ 作者=""Koi"">
            <ラビットハウス 種類=""カフェ"">
                <香風智乃 age=""13"" tall=""144""/>
                <保登心愛 age='15' tall='154'/>
            </ラビットハウス>
        </ご注文はうさぎですか？>
    </まんがタイムきららMAX>
    <まんがタイムきららキャラット>
        <まちカドまぞく 作者='伊藤いづも'>
            <多魔市>
                <吉田優子 愛称=""シャミ子"">これで勝ったと思うなよぉ</吉田優子>
                <千代田桃 愛称=""モモ"">シャミ子が悪いんだよ</千代田桃>
            </多魔市>
        </まちカドまぞく>
    </まんがタイムきららキャラット>
</きらら>")]
        private static partial ReadOnlySpan<byte> Xml2();
        public static ReadOnlySpan<byte> Sample2 => Xml2();
    }
}
