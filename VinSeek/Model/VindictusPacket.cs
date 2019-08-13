using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VinSeek.Utilities;
using VinSeek.Network;
using System.Xml.Serialization;

namespace VinSeek.Model
{
    public class VindictusPacket : INotifyPropertyChanged
    {
        private string _note;
        
        public string Direction { get; set; }
        
        public string Time { get; set; }
        
        public string PacketName { get; set; }

        [XmlIgnore]
        public byte[] Buffer { get; set; }

        [XmlElement(ElementName = "Buffer")]
        public string BufferString { get; set; }
        
        [XmlIgnore]
        public int PacketLength
        {
            get
            {
                return Buffer.Length;
            }
        }

        [XmlIgnore]
        public int OpcodeBytesCount { get; private set; }

        [XmlIgnore]
        public int Opcode { get; private set; }

        [XmlIgnore]
        public int LengthBytesCount { get; private set; }

        [XmlIgnore]
        public int Length { get; private set; }

        [XmlIgnore]
        public int PacketOffset { get; private set; }

        [XmlIgnore]
        public string Guid { get; set; }

        [XmlIgnore]
        public byte[] Body
        {
            get
            {
                var buffer = new byte[Buffer.Length - PacketOffset];
                System.Buffer.BlockCopy(Buffer, PacketOffset, buffer, 0, buffer.Length);

                return buffer;
            }
        }

        [XmlIgnore]
        public string Note
        {
            get { return _note; }
            set
            {
                if (_note != value)
                {
                    _note = value;
                    this.OnPropertyChanged("Note");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Non-parameter constructor for XML Serialize purpose
        /// </summary>
        public VindictusPacket()
        {
            this.PacketName = String.Empty;
        }

        /// <summary>
        /// Create a new vindictus packet
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="time">buffer received timestamp</param>
        /// <param name="flag">flag indicate if buffer contains packet direction information</param>
        public VindictusPacket(byte[] buffer, string time, string direction)
        {
            this.Buffer = buffer;
            this.Time = time;
            this.Direction = direction;
            this.OpcodeBytesCount = Util.GetBytesCount(Buffer, 8);
            this.Opcode = Util.GetInt32(Buffer, 8);
            this.LengthBytesCount = Util.GetBytesCount(Buffer, 8 + OpcodeBytesCount);
            this.Length = Util.GetInt32(Buffer, LengthBytesCount);
            this.PacketOffset = 8 + OpcodeBytesCount + LengthBytesCount;

            if (this.Opcode == 0)
            {
                var guidBytes = new byte[16];
                System.Buffer.BlockCopy(this.Buffer, this.PacketOffset, guidBytes, 0, 16);
                this.Guid = BitConverter.ToString(guidBytes).Replace("-", string.Empty);
                foreach (KeyValuePair<string, int> item in PacketIdentifier.Guids)
                {
                    if (this.Guid == item.Key)
                        this.Opcode = item.Value;
                }
            }
            else
                this.Guid = string.Empty;

            this.PacketName = "UNKNOWN";
            foreach (var knownOpcode in Enum.GetValues(typeof(PacketIdentifier.Opcodes)))
            {
                if (Opcode == (int)knownOpcode)
                {
                    var pName = (PacketIdentifier.Opcodes)knownOpcode;
                    this.PacketName = pName.ToString();
                    break;
                }
            }
        }
    }
}
