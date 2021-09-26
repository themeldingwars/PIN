using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Shared.Udp
{
    public static class Utils
    {
        public static Thread RunThread(ThreadStart action)
        {
            var t = new Thread(action);
            t.Start();

            return t;
        }

        public static Thread RunThread(Action<CancellationToken> action, CancellationToken cancellationToken)
        {
            var t = new Thread(o => action((CancellationToken)o));
            t.Start(cancellationToken);

            return t;
        }

        public static T SimpleFixEndianness<T>(T value)
            where T : struct
        {
            var s = MemoryMarshal.Cast<T, byte>(new[] { value });
            s.Reverse();
            return MemoryMarshal.Cast<byte, T>(s).ToArray().FirstOrDefault();
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }

        public static Memory<byte> Combine(IList<Memory<byte>> memoryList)
        {
            var totalSize = memoryList.Sum(m => m.Length);

            return Combine(memoryList, totalSize);
        }

        public static Memory<byte> Combine(IList<Memory<byte>> memoryList, int totalSize)
        {
            var combinedMemory = new Memory<byte>(new byte[totalSize]);
            var index = 0;
            foreach (var memory in memoryList)
            {
                memory.CopyTo(combinedMemory.Slice(index));
                index += memory.Length;
            }

            return combinedMemory;
        }
    }
}