#nullable enable
using System.Diagnostics;

namespace U8Xml.Internal
{
    internal static unsafe class DataOffsetHelper
    {
        public static bool CheckContainsMemory(byte* dataHead, int dataLen, byte* targetHead, int targetLen)
        {
            Debug.Assert(targetLen >= 0);
            Debug.Assert(dataLen >= 0);
            return (dataHead <= targetHead) && (targetHead + targetLen) <= (dataHead + dataLen);
        }

        public static int? GetOffset(byte* dataHead, int dataLen, byte* target)
        {
            if(dataLen < 0) { ThrowHelper.ThrowArgOutOfRange(nameof(dataLen)); }
            if(CheckContainsMemory(dataHead, dataLen, target, 1) == false) {
                return null;
            }
            long offset = target - dataHead;
            return checked((int)(uint)offset);
        }

        public static DataLocation? GetLocation(byte* dataHead, int dataLen, byte* targetHead, int targetLen, bool useZeroBasedNum)
        {
            if(dataLen < 0) { ThrowHelper.ThrowArgOutOfRange(nameof(dataLen)); }
            if(targetLen < 0) { ThrowHelper.ThrowArgOutOfRange(nameof(targetLen)); }
            if(CheckContainsMemory(dataHead, dataLen, targetHead, targetLen) == false) {
                return null;
            }

            var start = GetLineAndPosition(dataHead, dataLen, targetHead, true);
            var endOffset = GetLineAndPosition(targetHead, targetLen, targetHead + targetLen, true);
            var end = new DataLinePosition(
                line: start.Line + endOffset.Line,
                position: (endOffset.Line == 0) ? (start.Position + endOffset.Position) : endOffset.Position
            );

            if(useZeroBasedNum == false) {
                start = new DataLinePosition(start.Line + 1, start.Position + 1);
                end = new DataLinePosition(end.Line + 1, end.Position + 1);
            }
            int byteOffset = checked((int)(uint)(targetHead - dataHead));
            return new DataLocation(start, end, new DataRange(byteOffset, targetLen));
        }

        private static DataLinePosition GetLineAndPosition(byte* dataHead, int dataLen, byte* target, bool useZeroBasedNum)
        {
            Debug.Assert(dataLen >= 0);
            Debug.Assert(dataHead <= target);

            int lineNum = 0;
            int pos;
            byte* lineHead = dataHead;
            for(byte* ptr = dataHead; ptr < target; ptr++) {
                if(*ptr == '\n') {
                    lineNum++;
                    lineHead = ptr + 1;
                }
            }

            checked {
                pos = (int)(target - lineHead);
                if(useZeroBasedNum == false) {
                    lineNum += 1;
                    pos += 1;
                }
            }
            return new DataLinePosition(lineNum, pos);
        }
    }
}
