using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VinSeek.Utils
{
    public static class CustomPacketBuilder
    {
        public static byte[] BuildPacket(byte[] buffer)
        {
            byte[] head = new byte[0x18];
            using (MemoryStream stream = new MemoryStream(head))
            {
                stream.WriteByte(0x01);

                stream.Position = 0x04;
                stream.Write(BitConverter.GetBytes((UInt16)buffer.Length), 0, 2);

                stream.WriteByte(0x01);

                List<byte> output = new List<byte>(buffer);
                output.InsertRange(0, stream.ToArray());

                return output.ToArray();
            }
        }
    }
}
