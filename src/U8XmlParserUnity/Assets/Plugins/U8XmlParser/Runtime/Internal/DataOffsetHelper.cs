#nullable enable
using System.Diagnostics;

namespace U8Xml.Internal
{
    internal static unsafe class DataOffsetHelper
    {
        public static int? GetOffset(byte* dataHead, int dataLen, byte* target)
        {
            if(dataLen < 0) { ThrowHelper.ThrowArgOutOfRange(nameof(dataLen)); }
            if(CheckContainsMemory(dataHead, dataLen, target, 1) == false) {
                return null;
            }
            long offset = target - dataHead;
            return checked((int)(uint)offset);
        }

        public static DataLocation? GetLocation(byte* dataHead, int dataLen, byte* targetHead, int targetLen)
        {
            if(dataLen < 0) { ThrowHelper.ThrowArgOutOfRange(nameof(dataLen)); }
            if(targetLen < 0) { ThrowHelper.ThrowArgOutOfRange(nameof(targetLen)); }
            if(CheckContainsMemory(dataHead, dataLen, targetHead, targetLen) == false) {
                return null;
            }

            var start = GetLinePosition(dataHead, dataLen, targetHead);
            var endOffset = GetLinePosition(targetHead, targetLen, targetHead + targetLen);
            var end = new DataLinePosition(
                line: start.Line + endOffset.Line,
                position: (endOffset.Line == 0) ? (start.Position + endOffset.Position) : endOffset.Position
            );

            int byteOffset = checked((int)(uint)(targetHead - dataHead));
            var range = new DataRange(byteOffset, targetLen);
            return new DataLocation(start, end, range);
        }

        private static DataLinePosition GetLinePosition(byte* dataHead, int dataLen, byte* target)
        {
            Debug.Assert(dataLen >= 0);
            Debug.Assert(dataHead <= target);

            int lineNum = 0;
            byte* lastLineHead = dataHead;
            for(byte* p = dataHead; p < target; p++) {
                if(*p == '\n') {
                    lineNum++;
                    lastLineHead = p + 1;
                }
            }
            int byteCountInLastLine = checked((int)(uint)(target - lastLineHead));
            var utf8 = UTF8ExceptionFallbackEncoding.Instance;
            var pos = utf8.GetCharCount(lastLineHead, byteCountInLastLine);
            return new DataLinePosition(lineNum, pos);
        }

        private static bool CheckContainsMemory(byte* dataHead, int dataLen, byte* targetHead, int targetLen)
        {
            Debug.Assert(targetLen >= 0);
            Debug.Assert(dataLen >= 0);
            return (dataHead <= targetHead) && (targetHead + targetLen) <= (dataHead + dataLen);
        }
    }
}
