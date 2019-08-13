using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VinSeek.Utilities;
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

        [XmlIgnore]
        public byte[] BufferWithDirection { get; set; }

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
            }
            else
                this.Guid = string.Empty;

            this.PacketName = "UNKNOWN";
            foreach (var knownOpcode in Enum.GetValues(typeof(KnownOpcodes)))
            {
                if (Opcode == (int)knownOpcode)
                {
                    var pName = (KnownOpcodes)knownOpcode;
                    this.PacketName = pName.ToString();
                    break;
                }
            }
        }

        #region Opcodes
        enum KnownOpcodes
        {
            UNKNOWN = 0,
            CM_CHAR_NAME_CHECK = 456,
            CM_ENTER_TOWN = 464,
            CM_CLIENT_INFO = 526,
            CM_VERIFY_PIN = 586,
            CM_LOGIN_REQUEST = 659,
            CM_SELECT_CHARACTER = 662,
            CM_CREATE_CHARACTER = 663,
            CM_REQUEST_CHAR_INFO = 716,
            CM_SET_PIN = 766,
            SM_REQUEST_PIN = 420,
            SM_CASH_SHOP_CASH = 436,
            SM_CASH_SHOP_PRODUCTS = 440,
            SM_CASH_SHOP_SECTIONS = 441,
            SM_GAME_ENV = 522,
            SM_HAS_PIN = 594,
            SM_CHARACTER_OUTFIT = 627,
            SM_CHARACTER_LIST = 661,
            SM_LOGIN_SUCCESS = 664,
            SM_CHARACTER_MAILBOX = 681,
            SM_NPC_LOCATIONS = 693,
            SM_NPC_DIALOGUE = 694,
            SM_QUEST_STATUS = 735,
            SM_CHARACTER_QUICK_SLOTS = 741,
            SM_PIN_RESULT = 756,
            SM_CHARACTER_SP_SKILL = 780,
            SM_CHARACTER_STATUS_EFFECTS = 792,
            SM_STORY_STATUS = 794,
            SM_SYSTEM_MSG = 798,
            SM_CHARACTER_INFO = 800,
            SM_CHARACTER_STATS = 810,
        }
        #endregion
    }
}
