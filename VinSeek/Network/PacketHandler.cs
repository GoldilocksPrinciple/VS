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
    public class PacketHandler
    {
        private byte[] _buffer;
        private byte[] _packetBuffer;
        private byte[] _decryptedBuffer;
        private byte[] _newPacketBuffer = null;
        private int _packetLength;
        private bool _needSalt = true;
        public Transformer HandlerTransformer;

        public PacketHandler(bool needSalt, Transformer transformer)
        {
            _needSalt = needSalt;
            this.HandlerTransformer = transformer;
        }
        
        public byte[] AnalyzePacket(byte[] data, bool isEncrypted)
        {
            if (isEncrypted)
            {
                if (this.EncryptedPacketReassembled(data))
                    return _packetBuffer;
                else
                    return null;
            }
            else
            {
                if (this.RawPacketReassembled(data))
                    return _packetBuffer;
                else
                    return null;
            }
        }
        
        private bool EncryptedPacketReassembled(byte[] data)
        {
            _buffer = new byte[data.Length];
            Buffer.BlockCopy(data, 0, _buffer, 0, data.Length);
            if (_needSalt)
            {
                // set salt
                HandlerTransformer.Decrypt(_buffer, IPAddress.NetworkToHostOrder(BitConverter.ToInt64(_buffer, 0)));

                _decryptedBuffer = _buffer;
                _needSalt = false;
            }
            else
            {
                HandlerTransformer.Decrypt(_buffer);

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

        private bool RawPacketReassembled(byte[] data)
        {
            if (_newPacketBuffer != null)
            {
                _buffer = new byte[_newPacketBuffer.Length + data.Length];
                Buffer.BlockCopy(_newPacketBuffer, 0, _buffer, 0, _newPacketBuffer.Length);
                Buffer.BlockCopy(data, 0, _buffer, _newPacketBuffer.Length, data.Length);
            }
            else
            {
                _buffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, _buffer, 0, data.Length);
            }

            // get packet length
            _packetLength = this.GetExpectedLength(_buffer);

            // if current buffer not = real packet length
            if (_buffer.Length <= _packetLength)
            {
                _newPacketBuffer = new byte[_buffer.Length];
                Buffer.BlockCopy(_buffer, 0, _newPacketBuffer, 0, _buffer.Length);
                return false;
            }

            // get all bytes of the completed packet
            _packetBuffer = new byte[_packetLength];
            Buffer.BlockCopy(_buffer, 0, _packetBuffer, 0, _packetLength);

            // copy all bytes belong to the new packet to another buffer
            _newPacketBuffer = new byte[_buffer.Length - _packetLength];
            Buffer.BlockCopy(_buffer, _packetLength, _newPacketBuffer, 0, _newPacketBuffer.Length);

            return true;
        }
        
        private int GetExpectedLength(byte[] buffer)
        {
            try
            {
                var opcodeBytesCount = Util.GetBytesCount(buffer, sizeof(long));
                var lengthBytesCount = Util.GetBytesCount(buffer, sizeof(long) + opcodeBytesCount);
                var packetBodyLength = Util.GetInt32(buffer, sizeof(long) + opcodeBytesCount);
                var packetLength = sizeof(long) + opcodeBytesCount + lengthBytesCount + packetBodyLength;
                return packetLength;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
