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
        public static byte[] BuildPacket(string localIP, string remoteIP, string localPort, byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] localIPBytes = Encoding.ASCII.GetBytes(localIP);
                byte[] remoteIPBytes = Encoding.ASCII.GetBytes(remoteIP);
                byte[] localPortBytes = Encoding.ASCII.GetBytes(localPort);
                stream.Write(localIPBytes, 0, localIPBytes.Length);

                stream.Position = 0x10;
                stream.Write(remoteIPBytes, 0, remoteIPBytes.Length);

                stream.Position = 0x20;
                stream.Write(localPortBytes, 0, localPortBytes.Length);

                stream.WriteByte(0x00);

                List<byte> output = new List<byte>(buffer);
                output.InsertRange(0, stream.ToArray());

                return output.ToArray();
            }
        }
    }
}
