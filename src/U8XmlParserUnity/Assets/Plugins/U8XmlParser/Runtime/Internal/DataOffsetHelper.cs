#nullable enable
using System.Diagnostics;

namespace U8Xml
{
    internal static unsafe class DataOffsetHelper
    {
        public static bool CheckContainsMemory(byte* dataHead, int dataLen, byte* targetHead, int targetLen)
        {
            return (dataHead <= targetHead) && (targetHead + targetLen) <= (dataHead + dataLen);
        }

        public static (int Line, int Position) GetLineAndPosition(byte* dataHead, int dataLen, byte* targetHead, int targetLen, bool useZeroBasedNum)
        {
            Debug.Assert(targetLen >= 0);
            Debug.Assert(dataLen >= 0);
            Debug.Assert(dataHead <= targetHead);
            Debug.Assert((targetHead + targetLen) <= (dataHead + dataLen));

            int lineNum = 0;
            int pos;
            byte* lineHead = dataHead;
            for(byte* ptr = dataHead; ptr < targetHead; ptr++) {
                if(*ptr == '\n') {
                    lineNum++;
                    lineHead = ptr + 1;
                }
            }

            checked {
                pos = (int)(targetHead - lineHead);
                if(useZeroBasedNum == false) {
                    lineNum += 1;
                    pos += 1;
                }
            }
            return (Line: lineNum, Position: pos);
        }
    }
}
