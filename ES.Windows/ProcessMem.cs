using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ES.Windows
{
    public static class extenstions
    {
        public static List<IntPtr> IndexOfSequence(this byte[] buffer, byte[] pattern, int startIndex)
        {
            List<IntPtr> positions = new List<IntPtr>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    positions.Add(new IntPtr(i));
                i = Array.IndexOf<byte>(buffer, pattern[0], i + pattern.Length);
            }
            return positions;
        }
    }
    public class ProcessMem
    {
        [DllImport("kernel32.dll")]
        protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        [In, Out] byte[] buffer,
        Int64 size,
        out IntPtr lpNumberOfBytesRead
        );

        [StructLayout(LayoutKind.Sequential)]
        protected struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public uint __alignment1;
            public Int64 RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
            public uint __alignment2;
        }
        List<MEMORY_BASIC_INFORMATION> MemReg { get; set; }

        public void MemInfo(IntPtr pHandle)
        {
            IntPtr Addy = new IntPtr();
            while (true)
            {
                MEMORY_BASIC_INFORMATION MemInfo = new MEMORY_BASIC_INFORMATION();
                int MemDump = VirtualQueryEx(pHandle, Addy, out  MemInfo, Marshal.SizeOf(MemInfo));
                if (MemDump == 0) break;
                if ((MemInfo.State & 0x1000) != 0 && (MemInfo.Protect & 0x100) == 0)
                    MemReg.Add(MemInfo);
                var x = IntPtr.Size;
                Addy = new IntPtr(MemInfo.BaseAddress.ToInt64() + MemInfo.RegionSize);
            }
        }
        //public List<IntPtr> _Scan(byte[] sIn, byte[] sFor)
        //{
        //    var retVal = new List<IntPtr>();
        //    int[] sBytes = new int[256]; int Pool = 0;
        //    int End = sFor.Length - 1;
        //    for (int i = 0; i < 256; i++)
        //        sBytes[i] = sFor.Length;
        //    for (int i = 0; i < End; i++)
        //        sBytes[sFor[i]] = End - i;
        //    while (Pool <= sIn.Length - sFor.Length)
        //    {
        //        for (Int64 i = End; sIn[Pool + i] == sFor[i]; i--)
        //            if (i == 0) retVal.Add(new IntPtr(Pool));
        //        Pool += sBytes[sIn[Pool + End]];
        //    }
        //    return retVal;
        //}

        public List<IntPtr> AobScan(Process p, byte[] Pattern)
        {
            var retVal = new List<IntPtr>();
            MemReg = new List<MEMORY_BASIC_INFORMATION>();
            MemInfo(p.Handle);
            for (int i = 0; i < MemReg.Count; i++)
            {
                byte[] buff = new byte[MemReg[i].RegionSize];
                IntPtr ptrBytesReaded;
                ReadProcessMemory(p.Handle, MemReg[i].BaseAddress, buff, MemReg[i].RegionSize, out ptrBytesReaded);

                var Result = buff.IndexOfSequence(Pattern,0);
                
                if (Result.Count > 0)
                    foreach (var r in Result)
                    {
                        retVal.Add(new IntPtr(MemReg[i].BaseAddress.ToInt64() + r.ToInt64()));
                    }
            }
            return retVal;
        }

        public byte[] ReadAdress(Process p, IntPtr MemoryAddress, uint bytesToRead, out int bytesReaded)
        {

            byte[] buffer = new byte[bytesToRead];
            IntPtr ptrBytesReaded;

            ReadProcessMemory(p.Handle, MemoryAddress, buffer, bytesToRead, out ptrBytesReaded);

            bytesReaded = ptrBytesReaded.ToInt32();

            return buffer;

        }
    }
}
