using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinSeek.Network
{
    public class Transformer
    {
        private static Random _rand = new Random();
        private Isaac _send;
        private Isaac _recv;

        public bool Decrypt(byte[] buffer, long salt)
        {
            bool flag = _recv == null;

            if (_recv == null)
                _recv = new Isaac((uint)salt);

            this.Decrypt(buffer);

            return flag;
        }

        public void Decrypt(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] ^= (byte)_recv.Next();
        }

        public bool Encrypt(byte[] buffer, out long salt)
        {
            bool flag = _send == null;

            if (_send == null)
            {
                salt = (long)((ulong)_rand.Next());
                _send = new Isaac((uint)salt);
            }
            else
            {
                salt = 0;
            }

            this.Encrypt(buffer);

            return flag;
        }

        public void Encrypt(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] ^= (byte)_send.Next();
        }
    }
}
