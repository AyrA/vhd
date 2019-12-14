using System;
using System.IO;
using System.Text;

namespace vhd.MBR
{
    public class MasterBootRecord
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
        /// Default partition count in an MBR
        /// </summary>
        public const int PARTITION_COUNT = 4;

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

        public MasterBootRecord()
        {

        }

        public MasterBootRecord(Stream Source)
        {
            Read(Source);
        }

        public MasterBootRecord(byte[] Data)
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

        public void Validate()
        {
            if (Signature == null || Signature.Length != 2)
            {
                throw new FormatException($"{nameof(Signature)} is not a 2 byte value");
            }
            if (Signature[0]!=0x55 || Signature[1]!=0xAA)
            {
                throw new FormatException($"{nameof(Signature)} is not 55-AA");
            }
            if (BootstrapCode == null || BootstrapCode.Length != BOOTSTRAP_SIZE)
            {
                throw new FormatException($"{nameof(BootstrapCode)} is not {BOOTSTRAP_SIZE} bytes long");
            }
            if (Partitions == null || Partitions.Length != PARTITION_COUNT)
            {
                throw new FormatException($"{nameof(Partitions)} is not {PARTITION_COUNT} entries long");
            }
            for (var i = 0; i < PARTITION_COUNT; i++)
            {
                try
                {
                    Partitions[i].Validate();
                }
                catch (Exception ex)
                {
                    throw new FormatException($"{nameof(Partitions)}[{i}] failed to validate", ex);
                }
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
