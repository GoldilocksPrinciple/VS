using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinSeek.Network
{
    public class Isaac
    {
        private int _count;
        private uint[] _rsl = new uint[256];
        private uint[] _mm = new uint[256];
        private uint _aa, _bb, _cc;

        public Isaac()
        {
            this.Init(false);
        }

        public Isaac(uint seed)
        {
            seed *= 9U;
            _rsl[0] = seed;
            _rsl[seed % 64U] = seed;

            this.Init(true);
        }

        public byte Next()
        {
            if (_count == 0)
            {
                this.Generate();
                _count = 256;
            }
            _count--;

            return (byte)_rsl[_count];
        }

        public void Generate()
        {
            uint i, x, y;
            _cc++;
            _bb += _cc;

            for (i = 0; i <= 255; i++)
            {
                x = _mm[i];
                switch (i & 3)
                {
                    case 0:
                        _aa = _aa ^ (_aa << 13);
                        break;
                    case 1:
                        _aa = _aa ^ (_aa >> 6);
                        break;
                    case 2:
                        _aa = _aa ^ (_aa << 2);
                        break;
                    case 3:
                        _aa = _aa ^ (_aa >> 16);
                        break;
                }
                _aa = _mm[(i + 128) & 255] + _aa;
                y = _mm[(x >> 2) & 255] + _aa + _bb;
                _mm[i] = y;
                _bb = _mm[(y >> 10) & 255] + x;
                _rsl[i] = _bb;
            }
        }

        void Mix(ref uint a, ref uint b, ref uint c, ref uint d, ref uint e, ref uint f, ref uint g, ref uint h)
        {
            a = a ^ b << 11; d += a; b += c;
            b = b ^ c >> 2; e += b; c += d;
            c = c ^ d << 8; f += c; d += e;
            d = d ^ e >> 16; g += d; e += f;
            e = e ^ f << 10; h += e; f += g;
            f = f ^ g >> 4; a += f; g += h;
            g = g ^ h << 8; b += g; h += a;
            h = h ^ a >> 9; c += h; a += b;
        }

        void Init(bool flag)
        {
            short i; uint a, b, c, d, e, f, g, h;

            _aa = 0; _bb = 0; _cc = 0;
            a = 0x9e3779b9;
            b = a; c = a; d = a;
            e = a; f = a; g = a; h = a;

            for (i = 0; i <= 3; i++)
                this.Mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);

            i = 0;
            do
            {
                if (flag)
                {
                    a += _rsl[i]; b += _rsl[i + 1]; c += _rsl[i + 2]; d += _rsl[i + 3];
                    e += _rsl[i + 4]; f += _rsl[i + 5]; g += _rsl[i + 6]; h += _rsl[i + 7];
                }

                this.Mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);

                _mm[i] = a; _mm[i + 1] = b; _mm[i + 2] = c; _mm[i + 3] = d;
                _mm[i + 4] = e; _mm[i + 5] = f; _mm[i + 6] = g; _mm[i + 7] = h;
                i += 8;
            }
            while (i < 255);

            if (flag)
            {
                i = 0;
                do
                {
                    a += _mm[i]; b += _mm[i + 1]; c += _mm[i + 2]; d += _mm[i + 3];
                    e += _mm[i + 4]; f += _mm[i + 5]; g += _mm[i + 6]; h += _mm[i + 7];

                    this.Mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);

                    _mm[i] = a; _mm[i + 1] = b; _mm[i + 2] = c; _mm[i + 3] = d;
                    _mm[i + 4] = e; _mm[i + 5] = f; _mm[i + 6] = g; _mm[i + 7] = h;
                    i += 8;
                }
                while (i < 255);
            }

            this.Generate();

            _count = 256;
        }
    }
}
