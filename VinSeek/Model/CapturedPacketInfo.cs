using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using Machina;

namespace VinSeek.Model
{
    public class CapturedPacketInfo
    {
        public string Direction { get; set; }
        public string SourceIP { get; set; }
        public string DestIP { get; set; }
        public string SourcePort { get; set; }
        public string DestPort { get; set; }
        public IPProtocol Protocol { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; set; }
}
