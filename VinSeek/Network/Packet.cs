using System;
using System.Runtime.InteropServices;

namespace VinSeek.Network
{
    public class Packet
    {
        #region DLL Imports

        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr _Transformer();
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Encrypt(IntPtr transformer, [In, Out] byte[] buf, int len);
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Decrypt(IntPtr transformer, [In, Out] byte[] buf, int len);
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Encrypt2(IntPtr transformer, [In, Out] byte[] buf, int len, IntPtr salt);
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Decrypt2(IntPtr transformer, [In, Out] byte[] buf, int len, IntPtr salt);
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr _Util();
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ToBytes(IntPtr util, int val, [In, Out] byte[] buf, int i);
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ToInt(IntPtr util, [In, Out] byte[] buf, int i);
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCount(IntPtr util, int j);
        [DllImport("libcore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCount2(IntPtr util, [In, Out] byte[] buf, int i);

        #endregion
    }
}
