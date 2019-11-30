using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd.MBR
{
    public class MBR
    {
        /// <summary>
        /// Size of the bootstrap code
        /// </summary>
        public const int BOOTSTRAP_SIZE = 446;
        /// <summary>
        /// Size of an MBR
        /// </summary>
        public const int MBR_SIZE = 512;

        /// <summary>
        /// Bootstrap executable code
        /// </summary>
        public byte[] BootstrapCode { get; set; }

        /// <summary>
        /// Partition table (Always has 4 entries)
        /// </summary>
        public PartitionTableEntry[] Partitions { get; set; }

        /// <summary>
        /// Boot sector signature
        /// </summary>
        public byte[] Signature { get; set; }



        public MBR()
        {

        }

        public MBR(Stream Source)
        {
            Read(Source);
        }

        public MBR(byte[] Data)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }
            if (Data.Length != 512)
            {
                throw new ArgumentOutOfRangeException(nameof(Data), $"MBR size must be {MBR_SIZE} bytes");
            }
            using (var MS = new MemoryStream(Data, false))
            {
                Read(MS);
            }
        }

        private void Read(Stream S)
        {
            using (var BR = new BinaryReader(S, Encoding.Default, true))
            {
                BootstrapCode = BR.ReadBytes(BOOTSTRAP_SIZE);
                Partitions = new PartitionTableEntry[] {
                    new PartitionTableEntry(BR.ReadBytes(16)),
                    new PartitionTableEntry(BR.ReadBytes(16)),
                    new PartitionTableEntry(BR.ReadBytes(16)),
                    new PartitionTableEntry(BR.ReadBytes(16))
                };
                Signature = BR.ReadBytes(2);
            }
        }
    }
}
