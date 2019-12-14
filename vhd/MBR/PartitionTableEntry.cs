using System;
using System.IO;

namespace vhd.MBR
{
    /// <summary>
    /// Represents an entry in the MBR partition table
    /// </summary>
    public class PartitionTableEntry
    {
        /// <summary>
        /// Size of a partition entry
        /// </summary>
        public const int ENTRY_SIZE = 16;

        /// <summary>
        /// Status of the partition.
        /// Only one partition is supposed to be active
        /// </summary>
        public PartitionStatus Status { get; set; }
        /// <summary>
        /// First sector in CHS notation
        /// </summary>
        public CHS FirstSector { get; set; }
        /// <summary>
        /// Type of this partition
        /// </summary>
        public PartitionType Type { get; set; }
        /// <summary>
        /// Last sector in CHS notation
        /// </summary>
        public CHS LastSector { get; set; }
        /// <summary>
        /// First sector in LBA mode
        /// </summary>
        public uint LBAFirstSector { get; set; }
        /// <summary>
        /// Sector count in LBA mode
        /// </summary>
        public uint LBASectorCount { get; set; }

        /// <summary>
        /// Creates an empty MBR partition table entry
        /// </summary>
        public PartitionTableEntry()
        {
            Status = PartitionStatus.Inactive;
            FirstSector = new CHS();
            LastSector = new CHS();
            Type = PartitionType.Empty;
            LBAFirstSector = LBASectorCount = 0;
        }

        /// <summary>
        /// Creates a partition table entry from the given bytes
        /// </summary>
        /// <param name="RawData">Partition table bytes</param>
        public PartitionTableEntry(byte[] RawData)
        {
            if (RawData == null)
            {
                throw new ArgumentNullException(nameof(RawData));
            }
            if (RawData.Length != ENTRY_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(RawData), $"Partition table entry must be {ENTRY_SIZE} bytes long");
            }
            using (var MS = new MemoryStream(RawData, false))
            {
                using (var BR = new BinaryReader(MS))
                {
                    Status = (PartitionStatus)BR.ReadByte();
                    FirstSector = new CHS(BR.ReadBytes(3));
                    Type = (PartitionType)BR.ReadByte();
                    LastSector = new CHS(BR.ReadBytes(3));
                    LBAFirstSector = BR.ReadUInt32();
                    LBASectorCount = BR.ReadUInt32();
                }
            }
        }

        public void Validate()
        {
            //Don't care about unpartitioned segments
            if (Type == PartitionType.Empty)
            {
                return;
            }
            if (Status != PartitionStatus.Inactive && Status != PartitionStatus.Active)
            {
                throw new ValidationException(nameof(Status), $"Must be {nameof(PartitionStatus.Active)} or {nameof(PartitionStatus.Inactive)}");
            }
            try
            {
                FirstSector.Validate();
                if (FirstSector.IsEmpty)
                {
                    throw new ValidationException(nameof(FirstSector), "instance is empty");
                }
            }
            catch (Exception ex)
            {
                throw new ValidationException(nameof(FirstSector), "failed to validate", ex);
            }
            try
            {
                LastSector.Validate();
                if (LastSector.IsEmpty)
                {
                    throw new ValidationException(nameof(LastSector), "instance is empty");
                }
            }
            catch (Exception ex)
            {
                throw new ValidationException(nameof(LastSector), "failed to validate", ex);
            }
            if (LBAFirstSector == 0)
            {
                throw new ValidationException(nameof(LBAFirstSector),"is zero");
            }
        }
    }
}