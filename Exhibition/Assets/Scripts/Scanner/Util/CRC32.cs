using System.Collections;
using System.Collections.Generic;
using System;
namespace Scanner.Util
{
    public class CRC32
    {

        private readonly UInt32 CRC32_POLYNOMIAL = 0x04c11db7;
        private readonly UInt32 CRC32_INITIAL_REMAINDER = 0xFFFFFFFF;
        private readonly UInt32 CRC32_FINAL_XOR_VALUE = 0xFFFFFFFF;
        private readonly byte CRC32_WIDTH = 32;
        private readonly UInt32 CRC32_NULL = 0;


        bool sCRC32IsInitialized = false;

        UInt32 mLength;

        UInt32 mRemainder;

        UInt32[] sCRCTable = new UInt32[256];

        public CRC32()
        {
            this.mLength = 0;
            this.mRemainder = CRC32_INITIAL_REMAINDER;

            if (false == sCRC32IsInitialized)
            {
                for (UInt32 iCodes = 0; iCodes <= 0xFF; iCodes++)
                {
                    sCRCTable[iCodes] = reflect(iCodes, 8) << (CRC32_WIDTH - 8);

                    for (int iPos = 0; iPos < 8; iPos++)
                    {
                        sCRCTable[iCodes] = (sCRCTable[iCodes] << 1) ^ (System.Convert.ToBoolean(sCRCTable[iCodes] & (1 << 31)) ? CRC32_POLYNOMIAL : CRC32_NULL);
                    }

                    sCRCTable[iCodes] = reflect(sCRCTable[iCodes], CRC32_WIDTH);


                }
                sCRC32IsInitialized = true;
            }
        }

        public UInt32 reflect(UInt32 value, byte theBits)
        {
            UInt32 result = 0;
            for (Int32 bit = 1; bit < (theBits + 1); bit++)
            {
                if (System.Convert.ToBoolean(value & 1))
                {
                    result = (UInt32)(result | (1 << (theBits - bit)));
                }
                value >>= 1;
            }

            return result;
        }

        private UInt32 add(byte[] data,int length)
        {
            if (0 == length)
            {
                return 0;
            }

            for (int l = 0; l < length; l++)
            {
                mRemainder = (mRemainder >> 8) ^ sCRCTable[(mRemainder & 0xFF) ^ data[l]];
            }

            mLength += (UInt32)length;
            return (mRemainder ^ CRC32_FINAL_XOR_VALUE);
        }

        private void clear()
        {
            mLength = 0;
            mRemainder = CRC32_INITIAL_REMAINDER;
        }

        public UInt32 get()
        {
            return (mRemainder ^ CRC32_FINAL_XOR_VALUE);
        }

        public UInt32 get(byte[] data,int length)
        {
            clear();
            return add(data,length);
        }
    }
}
