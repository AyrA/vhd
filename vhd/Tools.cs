using System.Net;

namespace vhd
{
    public static class Tools
    {
        /// <summary>
        /// Converts CHS to LBA
        /// </summary>
        /// <param name="CurrentCylinder">Current cylinder to access (0=first)</param>
        /// <param name="CurrentHead">Current head to access (0=first)</param>
        /// <param name="CurrentSector">Current sector to access (1=first)</param>
        /// <param name="NumHeads">Number of heads on the disk (1=1)</param>
        /// <param name="NumSectors">Number of sectors in each track (1=1)</param>
        /// <remarks><paramref name="CurrentSector"/> is historically 1 based and not 0 based</remarks>
        /// <returns>LBA value</returns>
        public static long CHSToLBA(int CurrentCylinder, int CurrentHead, int CurrentSector, int NumHeads, int NumSectors)
        {
            return ((long)CurrentCylinder * NumHeads + CurrentHead) * NumSectors + (CurrentSector - 1L);
        }

        public static bool InRange(int low, int current, int high)
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
