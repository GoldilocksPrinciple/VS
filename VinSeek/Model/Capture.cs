using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinSeek.Model
{
    // Thanks to FFXIVMon for this class
    public class Capture
    {
        public VindictusPacket[] Packets { get; set; }

        public Capture()
        {
            Packets = new VindictusPacket[0];
        }
    }
}
