using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VinSeek.Model;
using Machina;
using System.Text.RegularExpressions;

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

        public static CapturedPacketInfo ReadPacket(byte[] buffer)
        {
            byte[] sourceIPBytes = new byte[15];
            byte[] destIPBytes = new byte[15];
            byte[] sourcePortBytes = new byte[5];
            byte[] destPortBytes = new byte[5];
            byte[] data = new byte[buffer.Length - 44];

            Array.Copy(buffer, sourceIPBytes, 14);
            var sourceIP = Encoding.ASCII.GetString(sourceIPBytes);
            sourceIP = Regex.Replace(sourceIP, @"[^\u0020-\u007E]", string.Empty);

            Array.Copy(buffer, 16, destIPBytes, 0, 15);
            var destIP = Encoding.ASCII.GetString(destIPBytes);
            destIP = Regex.Replace(destIP, @"[^\u0020-\u007E]", string.Empty);

            Array.Copy(buffer, 32, sourcePortBytes, 0, 5);
            var sourcePort = Encoding.ASCII.GetString(sourcePortBytes);
            sourcePort = Regex.Replace(sourcePort, @"[^\u0020-\u007E]", string.Empty);

            Array.Copy(buffer, 38, destPortBytes, 0, 5);
            var destPort = Encoding.ASCII.GetString(destPortBytes);
            sourcePort = Regex.Replace(sourcePort, @"[^\u0020-\u007E]", string.Empty);

            Array.Copy(buffer, 43, data, 0, buffer.Length - 44);

            string direction;

            if (sourcePort == "27015")
                direction = "Received";
            else
                direction = "Sent";
            var pack = new CapturedPacketInfo
            {
                Direction = direction,
                SourceIP = sourceIP.ToString(),
                DestIP = destIP.ToString(),
                SourcePort = sourcePort,
                DestPort = destPort,
                Protocol = IPProtocol.TCP,
                DataLength = data.Length,
                Data = data
            };

            return pack;
        }
    }
}
