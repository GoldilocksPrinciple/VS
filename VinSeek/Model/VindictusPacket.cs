using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VinSeek.Utilities;

namespace VinSeek.Model
{
    public class VindictusPacket : INotifyPropertyChanged
    {
        private string _note;

        public string Direction { get; set; }

        public string Time { get; set; }

        public string PacketName { get; private set; }

        public byte[] Buffer { get; private set; }


        public int PacketLength
        {
            get
            {
                return Buffer.Length;
            }
        }

        public int OpcodeBytesCount { get; private set; }

        public int Opcode { get; private set; }

        public int LengthBytesCount { get; private set; }

        public int Length { get; private set; }

        public int PacketOffset { get; private set; }


        public byte[] Body
        {
            get
            {
                var buffer = new byte[Buffer.Length - PacketOffset];
                System.Buffer.BlockCopy(Buffer, PacketOffset, buffer, 0, buffer.Length);

                return buffer;
            }
        }

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

        public VindictusPacket(byte[] buffer, string time)
        {
            var d = new byte[1];
            System.Buffer.BlockCopy(buffer, 0, d, 0, 1);
            this.Direction = System.Text.Encoding.ASCII.GetString(d);
            Debug.WriteLine(Direction);

            var packBuffer = new byte[buffer.Length - 1];
            System.Buffer.BlockCopy(buffer, 1, packBuffer, 0, buffer.Length - 1);
            this.Buffer = packBuffer;
            this.Time = time;
            this.OpcodeBytesCount = Util.GetBytesCount(Buffer, 8);
            this.Opcode = Util.GetInt32(Buffer, 8);
            this.LengthBytesCount = Util.GetBytesCount(Buffer, 8 + OpcodeBytesCount);
            this.Length = Util.GetInt32(Buffer, LengthBytesCount);
            this.PacketOffset = 8 + OpcodeBytesCount + LengthBytesCount;
            this.PacketName = "UNKNOWN";
            foreach (var clientOpcode in Enum.GetValues(typeof(ClientOpcode)))
            {
                if (Opcode == (int)clientOpcode)
                {
                    var pName = (ClientOpcode)clientOpcode;
                    this.PacketName = pName.ToString();
                    break;
                }
            }
            if (this.PacketName == "UNKNOWN")
            {
                foreach (var serverOpcode in Enum.GetValues(typeof(ServerOpcode)))
                {
                    if (Opcode == (int)serverOpcode)
                    {
                        var pName = (ServerOpcode)serverOpcode;
                        this.PacketName = pName.ToString();
                        break;
                    }
                }
            }
        }

        #region Opcodes
        enum ClientOpcode
        {
            CM_CHAR_NAME_CHECK = 456,
            CM_ENTER_TOWN = 464,
            CM_CLIENT_INFO = 526,
            CM_VERIFY_PIN = 586,
            CM_LOGIN_REQUEST = 659,
            CM_SELECT_CHARACTER = 662,
            CM_CREATE_CHARACTER = 663,
            CM_REQUEST_CHAR_INFO = 716,
            CM_SET_PIN = 766,
        }

        enum ServerOpcode
        {
            SM_REQUEST_PIN = 420,
            SM_CASH_SHOP_CASH = 436,
            SM_CASH_SHOP_PRODUCTS = 440,
            SM_CASH_SHOP_SECTIONS = 441,
            SM_GAME_ENV = 522,
            SM_HAS_PIN = 594,
            SM_CHARACTER_OUTFIT = 627,
            SM_CHARACTER_LIST = 660,
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
