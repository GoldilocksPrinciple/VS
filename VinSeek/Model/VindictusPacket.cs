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
        private string _comment;

        public string Time { get; set; }

        public string Direction { get; set; }
        
        public string ServerPort { get; set; }
        
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
        public int Opcode { get; private set; }

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
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
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
            this.PacketName = string.Empty;
        }

        /// <summary>
        /// Constructor for dungeon packet
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="time"></param>
        /// <param name="direction"></param>
        /// <param name="serverPort"></param>
        /// <param name="opcode"></param>
        public VindictusPacket(byte[] buffer, string time, string direction, string serverPort, int opcode)
        {
            this.Buffer = buffer;
            this.Time = time;
            this.Direction = direction;
            this.ServerPort = serverPort;
            this.Opcode = opcode;
            this.PacketName = "DUNGEON_SERVER";
        }

        /// <summary>
        /// Constructor for world server packet
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="time">buffer received timestamp</param>
        /// <param name="direction">packet direction</param>
        /// <param name="serverPort">server port that the packet was sent to</param>
        public VindictusPacket(byte[] buffer, string time, string direction, string serverPort)
        {
            this.Buffer = buffer;
            this.Time = time;
            this.Direction = direction;
            this.ServerPort = serverPort;
            try
            {
                this.Opcode = Util.ReadVarInt(Buffer, sizeof(long), out int opcodeBytesCount);
                this.Length = Util.ReadVarInt(Buffer, sizeof(long) + opcodeBytesCount, out int lengthBytesCount);
                this.PacketOffset = sizeof(long) + opcodeBytesCount + lengthBytesCount;

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

                if (this.ServerPort == "27023")
                    this.PacketName = "CHANNEL_SERVER";
                else
                {
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
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
