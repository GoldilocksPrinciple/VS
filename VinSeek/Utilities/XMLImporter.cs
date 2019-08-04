using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinSeek.Model;
using System.Xml.Serialization;

namespace VinSeek.Utilities
{
    // Thanks to FFXIVMon for this class
    public static class XMLImporter
    {
        public static void SaveCapture(Capture capture, string path)
        {
            var filestream = new FileStream(path, FileMode.Create);
            if (capture != null)
            {
                foreach (var packet in capture.Packets)
                {
                    packet.BufferString = Util.ByteArrayToString(packet.BufferWithDirection);
                }
            }
            var serializer = new XmlSerializer(typeof(Capture));
            serializer.Serialize(filestream, capture);
        }

        public static Capture LoadCapture(string path)
        {
            try
            {
                var fileStream = new FileStream(path, FileMode.Open);
                var serializer = new XmlSerializer(typeof(Capture));
                var capture = (Capture)serializer.Deserialize(fileStream);
                if (capture != null)
                {
                    foreach (var packet in capture.Packets)
                    {
                        packet.BufferWithDirection = Util.StringToByteArray(packet.BufferString);
                    }
                }

                return capture;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
