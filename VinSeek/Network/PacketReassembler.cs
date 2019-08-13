using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VinSeek.Utilities;

namespace VinSeek.Network
{
    public class PacketReassembler : IDisposable
    {
        private byte[] _buffer;
        private byte[] _packetBuffer;
        private byte[] _decryptedBuffer;
        private byte[] _newPacketBuffer = null;
        private int _packetLength;
        private bool _firstPacket = true;
        private Transformer _transformer = new Transformer();

        public byte[] AnalyzePacket(byte[] data)
        {
            if (PacketReassembled(data))
                return _packetBuffer;
            else
                return null;
        }

        private bool PacketReassembled(byte[] data)
        {
            _buffer = data;

            if (_firstPacket)
            {
                // set salt
                _transformer.Decrypt(_buffer, IPAddress.NetworkToHostOrder(BitConverter.ToInt64(_buffer, 0)));
                _decryptedBuffer = _buffer;
                _firstPacket = false;
            }
            else
            {
                _transformer.Decrypt(_buffer);

                // append current buffer to the leftover part of previous buffer
                _decryptedBuffer = new byte[_newPacketBuffer.Length + _buffer.Length];
                Buffer.BlockCopy(_newPacketBuffer, 0, _decryptedBuffer, 0, _newPacketBuffer.Length);
                Buffer.BlockCopy(_buffer, 0, _decryptedBuffer, _newPacketBuffer.Length, _buffer.Length);

            }

            // get packet length
            _packetLength = this.GetExpectedLength(_decryptedBuffer);

            // if current buffer not = real packet length
            if (_decryptedBuffer.Length <= _packetLength)
            {
                _newPacketBuffer = new byte[_decryptedBuffer.Length];
                Buffer.BlockCopy(_decryptedBuffer, 0, _newPacketBuffer, 0, _decryptedBuffer.Length);
                return false;
            }

            // get all bytes of the completed packet
            _packetBuffer = new byte[_packetLength];
            Buffer.BlockCopy(_decryptedBuffer, 0, _packetBuffer, 0, _packetLength);

            // copy all bytes belong to the new packet to another buffer
            _newPacketBuffer = new byte[_decryptedBuffer.Length - _packetLength];
            Buffer.BlockCopy(_decryptedBuffer, _packetLength, _newPacketBuffer, 0, _newPacketBuffer.Length);

            return true;
        }

        public int GetExpectedLength(byte[] buffer)
        {
            var opcodeBytesCount = Util.GetBytesCount(buffer, sizeof(long));
            var lengthBytesCount = Util.GetBytesCount(buffer, sizeof(long) + opcodeBytesCount);
            var packetBodyLength = Util.GetInt32(buffer, sizeof(long) + opcodeBytesCount);
            var packetLength = sizeof(long) + opcodeBytesCount + lengthBytesCount + packetBodyLength;
            return packetLength;
        }

        public void Dispose()
        {
            _buffer = null;
            _transformer = null;
            _packetBuffer = null;
            _decryptedBuffer = null;
        }
    }
}
