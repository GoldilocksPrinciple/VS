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
        public static byte[] BuildPacket(string sourceIP, string destIP, string sourcePort, string destPort, byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] sourceIPBytes = Encoding.ASCII.GetBytes(sourceIP);
                byte[] destIPBytes = Encoding.ASCII.GetBytes(destIP);
                byte[] sourcePortBytes = Encoding.ASCII.GetBytes(sourcePort);
                byte[] destPortBytes = Encoding.ASCII.GetBytes(destPort);

                stream.Write(sourceIPBytes, 0, sourceIPBytes.Length);

                stream.Position = 0x10;
                stream.Write(destIPBytes, 0, destIPBytes.Length);

                stream.Position = 0x20;
                stream.Write(sourcePortBytes, 0, sourcePortBytes.Length);

                stream.Position = 0x26;
                stream.Write(destPortBytes, 0, destPortBytes.Length);

                stream.WriteByte(0x00);
                List<byte> output = new List<byte>(buffer);
                output.InsertRange(0, stream.ToArray());

                return output.ToArray();
            }
        }
    }
}
