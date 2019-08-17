using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinSeek.Utilities
{
    public class Util
    {
        public static int ReadBytes(int value, byte[] buffer, int index)
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

        public static int ReadBytesCount(int value)
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

        public static int ReadBytesCount(byte[] buffer, int index)
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
        
        public static int ReadVarInt(byte[] bytes, int index, out int length)
        {
            byte b = bytes[index];
            length = ReadBytesCount(bytes, index);
            if (length == 1)
            {
                return b;
            }
            else if (length == 2)
            {
                var temp = b;
                var result = (temp << 6 & 0x7C0);

                temp = bytes[index + 1];
                result |= (temp & 63);

                return result;
            }
            else if (length == 3)
            {
                var temp = b;
                var result = (temp << 12 & 0xF000);

                temp = bytes[index + 1];
                result |= (temp << 6 & 0xFC0);
                temp = bytes[index + 2];
                result |= (temp & 63);

                return result;
            }
            else if (length == 4)
            {
                var temp = b;
                var result = (temp << 18 & 0x1C0000);

                temp = bytes[index + 1];
                result |= (temp << 12 & 0x3F000);
                temp = bytes[index + 2];
                result |= (temp << 6 & 0xFC0);
                temp = bytes[index + 3];
                result |= (temp & 63);

                return result;
            }
            else if (length == 5)
            {
                var temp = b;
                var result = (temp << 24 & 0x3000000);

                temp = bytes[index + 1];
                result |= (temp << 18 & 0xFC0000);
                temp = bytes[index + 2];
                result |= (temp << 12 & 0x3F000);
                temp = bytes[index + 3];
                result |= (temp << 6 & 0xFC0);
                temp = bytes[index + 4];
                result |= (temp & 63);
            }
            else if (length == 6)
            {
                var temp = b;
                var result = (temp << 30 & -0x40000000);

                temp = bytes[index + 1];
                result |= (temp << 24 & 0x3F000000);
                temp = bytes[index + 2];
                result |= (temp << 18 & 0xFC0000);
                temp = bytes[index + 3];
                result |= (temp << 12 & 0x3F000);
                temp = bytes[index + 4];
                result |= (temp << 6 & 0xFC0);
                temp = bytes[index + 5];
                result |= (temp & 63);
            }
            return -1;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
