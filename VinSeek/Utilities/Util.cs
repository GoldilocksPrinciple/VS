using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinSeek.Utilities
{
    public class Util
    {
        public static int GetBytes(int value, byte[] buffer, int index)
        {
            if (value <= 0x7F)
            {
                buffer[index] = (byte)value;

                return 1;
            }
            else if (value <= 0x7FF)
            {
                buffer[index + 0] = (byte)((value >> 6) | 0xC0);
                buffer[index + 1] = (byte)((value & 63) | 0x80);

                return 2;
            }
            else if (value <= 0xFFFF)
            {
                buffer[index + 0] = (byte)((value >> 12) | 0xE0);
                buffer[index + 1] = (byte)((value >> 06 & 63) | 0x80);
                buffer[index + 2] = (byte)((value >> 00 & 63) | 0x80);

                return 3;
            }
            else if (value <= 0x1FFFFF)
            {
                buffer[index + 0] = (byte)((value >> 18) | 0xF0);
                buffer[index + 1] = (byte)((value >> 12 & 63) | 0x80);
                buffer[index + 2] = (byte)((value >> 06 & 63) | 0x80);
                buffer[index + 3] = (byte)((value >> 00 & 63) | 0x80);

                return 4;
            }
            else if (value <= 0x3FFFFFF)
            {
                buffer[index + 0] = (byte)((value >> 24) | 0xF8);
                buffer[index + 1] = (byte)((value >> 18 & 63) | 0x80);
                buffer[index + 2] = (byte)((value >> 12 & 63) | 0x80);
                buffer[index + 3] = (byte)((value >> 06 & 63) | 0x80);
                buffer[index + 4] = (byte)((value >> 00 & 63) | 0x80);

                return 5;
            }
            else
            {
                buffer[index + 0] = (byte)((value >> 30) | 0xFC);
                buffer[index + 1] = (byte)((value >> 24 & 63) | 0x80);
                buffer[index + 2] = (byte)((value >> 18 & 63) | 0x80);
                buffer[index + 3] = (byte)((value >> 12 & 63) | 0x80);
                buffer[index + 4] = (byte)((value >> 06 & 63) | 0x80);
                buffer[index + 5] = (byte)((value >> 00 & 63) | 0x80);

                return 6;
            }
        }

        public static int GetBytesCount(int value)
        {
            if (value <= 0x000007F)
                return 1;

            if (value <= 0x00007FF)
                return 2;

            if (value <= 0x000FFFF)
                return 3;

            if (value <= 0x01FFFFF)
                return 4;

            if (value <= 0x3FFFFFF)
                return 5;

            return 6;
        }

        public static int GetBytesCount(byte[] buffer, int index)
        {
            byte b = buffer[index];

            if ((b & 0x80) == 0x00)
                return 1;

            if ((b & 0xE0) == 0xC0)
                return 2;

            if ((b & 0xF0) == 0xE0)
                return 3;

            if ((b & 0xF8) == 0xF0)
                return 4;

            if ((b & 0xFC) == 0xF8)
                return 5;

            if ((b & 0xFC) == 0xFC)
                return 6;

            return -1;
        }
        
        // TODO: Rewrite this mess
        public static int GetInt32(byte[] bytes, int index)
        {
            byte num1 = bytes[index++];
            int num2;
            if ((num1 & 128) == 0)
            {
                num2 = num1;
            }
            else
            {
                int num3;
                if ((num1 & 224) == 192)
                {
                    num3 = num1 << 6 & 1984;
                }
                else
                {
                    int num4;
                    if ((num1 & 240) == 224)
                    {
                        num4 = num1 << 12 & 61440;
                    }
                    else
                    {
                        int num5;
                        if ((num1 & 248) == 240)
                        {
                            num5 = num1 << 18 & 1835008;
                        }
                        else
                        {
                            int num6;
                            if ((num1 & 252) == 248)
                            {
                                num6 = num1 << 24 & 50331648;
                            }
                            else
                            {
                                if ((num1 & 252) != 252)
                                    return -1;
                                int num7 = num1 << 30 & -1073741824;
                                byte num8 = bytes[index++];
                                if ((num8 & 192) != 128)
                                    return -1;
                                num6 = num7 | num8 << 24 & 1056964608;
                                if ((uint)num6 <= 67108863U)
                                    return -1;
                            }
                            byte num9 = bytes[index++];
                            if ((num9 & 192) != 128)
                                return -1;
                            num5 = num6 | num9 << 18 & 16515072;
                            if ((uint)num5 <= 2097151U)
                                return -1;
                        }
                        byte num10 = bytes[index++];
                        if ((num10 & 192) != 128)
                            return -1;
                        num4 = num5 | num10 << 12 & 258048;
                        if ((uint)num4 <= ushort.MaxValue)
                            return -1;
                    }
                    byte num11 = bytes[index++];
                    if ((num11 & 192) != 128)
                        return -1;
                    num3 = num4 | num11 << 6 & 4032;
                    if ((uint)num3 <= 2047U)
                        return -1;
                }
                byte num12 = bytes[index++];
                if ((num12 & 192) != 128)
                    return -1;
                num2 = num3 | num12 & 63;
                if ((uint)num2 <= (uint)sbyte.MaxValue)
                    return -1;
            }
            return num2;
        }
    }
}
