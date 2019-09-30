using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VinSeek.Utilities;
using VinSeek.Views;
using VinSeek.Model;

namespace VinSeek.Network
{
    public class PacketHandler
    {
        private readonly VinSeekMainTab _currentVinSeekTab;
        private byte[] _buffer;
        private byte[] _decryptedBuffer;
        private byte[] _newPacketBuffer;
        private bool _needSalt;
        private string _direction;
        public Transformer _transformer;

        /// <summary>
        /// Constructor with current used VinSeek tab
        /// </summary>
        /// <param name="currentTab">VinSeek tab</param>
        /// <param name="direction">direction of data stream (server/client)</param>
        public PacketHandler(VinSeekMainTab currentTab, string direction)
        {
            _currentVinSeekTab = currentTab;
            _transformer = new Transformer();
            _direction = direction;
            _buffer = null;
            _decryptedBuffer = null;
            _newPacketBuffer = null;
            _needSalt = true;
        }

        /// <summary>
        /// Public method for PacketHandler class
        /// </summary>
        /// <param name="data">buffer</param>
        /// <param name="isEncrypted">encrypted flag</param>
        public void AnalyzePacket(byte[] data, bool isEncrypted)
        {
            if (isEncrypted)
            {
                this.DecryptPacketBuffer(data);
            }
            else
            {
                this.ReassemblingRawPacket(data, false);
            }
        }

        /// <summary>
        /// Decrypt packet buffer
        /// </summary>
        /// <param name="data">encrypted packet buffer</param>
        private void DecryptPacketBuffer(byte[] data)
        {
            _buffer = new byte[data.Length];
            Buffer.BlockCopy(data, 0, _buffer, 0, data.Length);
            if (_needSalt)
            {
                // set salt
                _transformer.Decrypt(_buffer, IPAddress.NetworkToHostOrder(BitConverter.ToInt64(_buffer, 0)));

                _decryptedBuffer = _buffer;
                _needSalt = false;
            }
            else
            {
                _transformer.Decrypt(_buffer);

                if (_newPacketBuffer != null)
                {
                    // append current buffer to the leftover part of previous buffer
                    _decryptedBuffer = new byte[_newPacketBuffer.Length + _buffer.Length];
                    Buffer.BlockCopy(_newPacketBuffer, 0, _decryptedBuffer, 0, _newPacketBuffer.Length);
                    Buffer.BlockCopy(_buffer, 0, _decryptedBuffer, _newPacketBuffer.Length, _buffer.Length);
                }
                else
                {
                    _decryptedBuffer = new byte[_buffer.Length];
                    Buffer.BlockCopy(_buffer, 0, _decryptedBuffer, 0, _buffer.Length);
                }
            }

            // reassembling packet
            this.ReassemblingRawPacket(_decryptedBuffer, true);
        }


        /// <summary>
        /// Reassembling decrypted packet buffer
        /// </summary>
        /// <param name="data">decrypted buffer</param>
        private void ReassemblingRawPacket(byte[] data, bool isEncrypted)
        {
            if (data == null)
                return;

            byte[] tempBuffer;

            if (isEncrypted)
            {
                tempBuffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, tempBuffer, 0, data.Length);
            }
            else
            {
                if (_newPacketBuffer == null)
                {
                    tempBuffer = new byte[data.Length];
                    Buffer.BlockCopy(data, 0, tempBuffer, 0, data.Length);
                }
                else
                {
                    tempBuffer = new byte[data.Length + _newPacketBuffer.Length];
                    Buffer.BlockCopy(_newPacketBuffer, 0, tempBuffer, 0, _newPacketBuffer.Length);
                    Buffer.BlockCopy(data, 0, tempBuffer, _newPacketBuffer.Length, data.Length);
                }
            }

            while (true)
            {
                // get packet length
                var packetFullLength = this.GetExpectedLength(tempBuffer);
                Console.WriteLine(packetFullLength);

                // if current buffer not = real packet length
                if (packetFullLength == -1 || tempBuffer.Length < packetFullLength)
                {
                    _newPacketBuffer = new byte[tempBuffer.Length];
                    Buffer.BlockCopy(tempBuffer, 0, _newPacketBuffer, 0, tempBuffer.Length);
                    break;
                }
                else
                {
                    // get all bytes of the completed packet
                    var packetBuffer = new byte[packetFullLength];
                    Buffer.BlockCopy(tempBuffer, 0, packetBuffer, 0, packetFullLength);

                    // add to packet list
                    string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                    VindictusPacket packet;
                    if (isEncrypted)
                        packet = new VindictusPacket(packetBuffer, timestamp, _direction, "27015");
                    else
                        packet = new VindictusPacket(packetBuffer, timestamp, _direction, "27023");

                    _currentVinSeekTab.Dispatcher.Invoke(new Action(() =>
                    {
                        _currentVinSeekTab.PacketList.Add(packet);
                    }));

                    // copy all bytes belong to the new packet to tempBuffer, for the loop to continue
                    var tempBuffer2 = new byte[tempBuffer.Length - packetFullLength];
                    Buffer.BlockCopy(tempBuffer, packetFullLength, tempBuffer2, 0, tempBuffer2.Length);
                    tempBuffer = new byte[tempBuffer2.Length];
                    Buffer.BlockCopy(tempBuffer2, 0, tempBuffer, 0, tempBuffer2.Length);
                }
            }
        }


        /// <summary>
        /// Get packet body length from decrypted packet buffer
        /// </summary>
        /// <param name="buffer">decrypted packet buffer</param>
        /// <returns>packet body length</returns>
        private int GetExpectedLength(byte[] buffer)
        {
            // sizeof(long) + opcode byte
            if (buffer.Length < sizeof(long) + 1)
                return -1;

            try
            {
                var packetOpcode = Util.ReadVarInt(buffer, sizeof(long), out int opcodeBytesCount);
                if (buffer.Length < sizeof(long) + opcodeBytesCount)
                    return -1;

                var packetBodyLength = Util.ReadVarInt(buffer, sizeof(long) + opcodeBytesCount, out int lengthBytesCount);

                // return packet full length
                return sizeof(long) + opcodeBytesCount + lengthBytesCount + packetBodyLength;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
