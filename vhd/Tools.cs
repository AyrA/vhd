using System.Net;

namespace vhd
{
    public static class Tools
    {
        public static bool InRange(int low,int current,int high)
        {
            return current <= high && current >= low;
        }

        public static ushort ToNetwork(ushort value)
        {
            return (ushort)ToNetwork((short)value);
        }

        public static short ToNetwork(short value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }

        public static uint ToNetwork(uint value)
        {
            return (uint)ToNetwork((int)value);
        }

        public static int ToNetwork(int value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }

        public static ulong ToNetwork(ulong value)
        {
            return (ulong)ToNetwork((long)value);
        }

        public static long ToNetwork(long value)
        {
            return IPAddress.HostToNetworkOrder(value);
        }

    }
}
