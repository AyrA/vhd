using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd.VHD
{
    public class DynamicHeader
    {
        /// <summary>
        /// Default cookie value
        /// </summary>
        public const string DEFAULT_COOKIE = "cxsparse";
        /// <summary>
        /// Default value for the data offset
        /// </summary>
        public const uint DEFAULT_DATA_OFFSET = 0xFFFFFFFF;
        /// <summary>
        /// Default value for the blocksize
        /// </summary>
        public uint DEFAULT_BLOCK_SIZE = 0x00200000;
        /// <summary>
        /// Default value for the major version
        /// </summary>
        public const int V_MAJOR_DEFAULT = 1;
        /// <summary>
        /// Default value for the minor version
        /// </summary>
        public const int V_MINOR_DEFAULT = 0;
        /// <summary>
        /// Size of the first reserved field
        /// </summary>
        public const int RESERVED1_FIELD_SIZE = 4;
        /// <summary>
        /// Size of the second reserved field
        /// </summary>
        public const int RESERVED2_FIELD_SIZE = 256;
        /// <summary>
        /// Total header size
        /// </summary>
        public const int HEADER_LENGTH = 1024;
        /// <summary>
        /// Size of the parent file name length in bytes
        /// </summary>
        /// <remarks>The string is treated as UTF-16</remarks>
        public const int UNICODE_PARENT_LENGTH = 512;
        /// <summary>
        /// Number of entries in the locator table
        /// </summary>
        public const int LOCATOR_ENTRY_COUNT = 8;

        /// <summary>
        /// Identifier of this dynamic disk.
        /// As of now, it must always be the value of <see cref="DEFAULT_COOKIE"/>
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// This field contains the absolute byte offset to the next structure in the hard disk image.
        /// It is currently unused by existing formats and should be set to 0xFFFFFFFF
        /// </summary>
        public ulong DataOffset { get; set; }
        /// <summary>
        /// This field stores the absolute byte offset of the Block Allocation Table (BAT) in the file
        /// </summary>
        public ulong TableOffset { get; set; }
        /// <summary>
        /// This field stores the version of the dynamic disk header.
        /// For this specification, this field must be initialized to Version 1.0. 
        /// The major version will be incremented only when the header format is modified in such a way
        /// that it is no longer compatible with older versions of the product.
        /// </summary>
        public Version HeaderVersion { get; set; }
        /// <summary>
        /// This field holds the maximum entries present in the BAT. This should be equal to the number of blocks in the disk (that is, the disk size divided by the block size)
        /// </summary>
        public uint MaxTableEntries { get; set; }
        /// <summary>
        /// A block is a unit of expansion for dynamic and differencing hard disks.
        /// It is stored in bytes. This size does not include the size of the block bitmap.
        /// It is only the size of the data section of the block.
        /// The sectors per block must always be a power of two.
        /// The default value is 0x00200000 (indicating a block size of 2 MB)
        /// </summary>
        public uint BlockSize { get; set; }
        /// <summary>
        /// Checksum of the dynamic header portion
        /// </summary>
        /// <remarks>
        /// The checksum is a ones complement of the sum of the header bytes.
        /// During sum calculation, the checksum field assumes a value of zero
        /// </remarks>
        public int Checksum { get; set; }
        /// <summary>
        /// This field is used for differencing hard disks.
        /// A differencing hard disk stores a 128-bit UUID of the parent hard disk.
        /// </summary>
        /// <remarks>
        /// Set to <see cref="Guid.Empty"/> if not a differencing disk
        /// </remarks>
        public Guid ParentUniqueId { get; set; }
        /// <summary>
        /// This field stores the modification time stamp of the parent hard disk.
        /// </summary>
        public DateTime ParentTimeStamp { get; set; }
        /// <summary>
        /// This field should be set to zero
        /// </summary>
        public byte[] Reserved1 { get; set; }
        /// <summary>
        /// This field contains a Unicode string (UTF-16) of the parent hard disk filename
        /// </summary>
        /// <remarks>It's set to nullbytes for non differencing disks</remarks>
        public string ParentUnicodeName { get; set; }
        /// <summary>
        /// These entries store an absolute byte offset in the file
        /// where the parent locator for a differencing hard disk is stored.
        /// </summary>
        /// <remarks>
        /// This field is used only for differencing disks and should be set to zero for dynamic disks.
        /// This field always contains 8 entries
        /// </remarks>
        public LocatorEntry[] ParentLocatorEntries { get; set; }
        /// <summary>
        /// Must be set to zero
        /// </summary>
        public byte[] Reserved2 { get; set; }

        /// <summary>
        /// Creates dynamic VHD disk with most values set to their defaults
        /// </summary>
        public DynamicHeader(ulong Size)
        {
            Cookie = DEFAULT_COOKIE;
            HeaderVersion = new Version(V_MAJOR_DEFAULT, V_MINOR_DEFAULT);
            ParentUniqueId = Guid.Empty;
            //Default sizes
            //
            Reserved1 = new byte[RESERVED1_FIELD_SIZE];
            Reserved2 = new byte[RESERVED2_FIELD_SIZE];

            Checksum = ComputeChecksum();
        }

        /// <summary>
        /// Creates a VHD header from given data
        /// </summary>
        /// <param name="Data">Data, must be of at least <see cref="HEADER_LENGTH"/> size</param>
        /// <remarks>The data must start with the header but is allowed to contain excessive bytes which are ignored.</remarks>
        public DynamicHeader(byte[] Data)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }
            if (Data.Length < HEADER_LENGTH)
            {
                throw new ArgumentOutOfRangeException(nameof(Data), $"{HEADER_LENGTH} bytes expected, {Data.Length} given");
            }
            using (var MS = new MemoryStream(Data, false))
            {
                FromStream(MS);
            }
        }

        /// <summary>
        /// Creates a VHD header
        /// </summary>
        /// <param name="S">Stream to read header from</param>
        /// <remarks>The stream must be positioned at the header start</remarks>
        public DynamicHeader(Stream S)
        {
            FromStream(S);
        }

        /// <summary>
        /// Reads a stream into a VHD header
        /// </summary>
        /// <param name="S">Stream</param>
        private void FromStream(Stream S)
        {
            Encoding E = Encoding.Default;
            //The comments indicate the offset from the header start
            using (var BR = new BinaryReader(S, E, true))
            {
                //0
                Cookie = E.GetString(BR.ReadBytes(8));
                //8
                DataOffset = Tools.ToNetwork(BR.ReadUInt64());
                //16
                TableOffset = Tools.ToNetwork(BR.ReadUInt64());
                //24
                HeaderVersion = new Version(Tools.ToNetwork(BR.ReadUInt16()), Tools.ToNetwork(BR.ReadUInt16()));
                //28
                MaxTableEntries = Tools.ToNetwork(BR.ReadUInt32());
                //32
                BlockSize = Tools.ToNetwork(BR.ReadUInt32());
                //36
                Checksum = Tools.ToNetwork(BR.ReadInt32());
                //40
                ParentUniqueId = new Guid(BR.ReadBytes(16));
                //56
                ParentTimeStamp = VHD.FromDiskTimestamp(Tools.ToNetwork(BR.ReadUInt32()));
                //60
                Reserved1 = BR.ReadBytes(RESERVED1_FIELD_SIZE);
                //64
                ParentUnicodeName = Encoding.Unicode.GetString(BR.ReadBytes(UNICODE_PARENT_LENGTH));
                //576
                ParentLocatorEntries = Enumerable
                    .Range(0, LOCATOR_ENTRY_COUNT)
                    .Select(m => new LocatorEntry(BR.ReadBytes(LocatorEntry.ENTRY_SIZE)))
                    .ToArray();
                //768
                Reserved2 = BR.ReadBytes(RESERVED2_FIELD_SIZE);
                //1024
            }
        }

        /// <summary>
        /// Validates the header and throws an exception if it's invalid
        /// </summary>
        public void Validate()
        {
            var E = Encoding.Default;
            if (Cookie == null || E.GetBytes(Cookie).Length != 8)
            {
                throw new ValidationException(nameof(Cookie), "Must be an 8 byte (ANSI) string");
            }
            if (DataOffset != DEFAULT_DATA_OFFSET)
            {
                throw new ValidationException(nameof(DataOffset), $"Is currently unused and must be set to {DEFAULT_DATA_OFFSET}");
            }
            if (TableOffset < HEADER_LENGTH + Footer.HEADER_LENGTH)
            {
                throw new ValidationException(nameof(TableOffset), $"Can't be less than the footer size plus the dynamic header size");
            }
            if (HeaderVersion == null)
            {
                throw new ValidationException(nameof(HeaderVersion), "Must be defined");
            }
            if (HeaderVersion.Major < ushort.MinValue || HeaderVersion.Minor < ushort.MinValue || HeaderVersion.Major > ushort.MaxValue || HeaderVersion.Minor > ushort.MaxValue)
            {
                throw new ValidationException(nameof(HeaderVersion), $"Major and minor must range from {ushort.MinValue}-{ushort.MaxValue}");
            }
            if (MaxTableEntries == 0)
            {
                throw new ValidationException(nameof(MaxTableEntries), "Can't be zero. It should be equal to the number of blocks in the disk");
            }
            if (BlockSize == 0)
            {
                throw new ValidationException(nameof(BlockSize), "Can't be zero.");
            }
            if (BlockSize % 512 != 0)
            {
                throw new ValidationException(nameof(BlockSize), "Must be a multiple of 512");
            }
            if (VHD.ToDiskTimestamp(ParentTimeStamp) < int.MinValue || VHD.ToDiskTimestamp(ParentTimeStamp) > int.MaxValue)
            {
                throw new ValidationException(nameof(ParentTimeStamp), $"Must be at most {int.MinValue} - {int.MaxValue} seconds away from 2000-01-01 00:00:00 UTC");
            }
            if (Reserved1 == null || Reserved1.Length != RESERVED1_FIELD_SIZE || Reserved1.Any(m => m > 0))
            {
                throw new ValidationException(nameof(Reserved1), $"Must be {RESERVED1_FIELD_SIZE} nullbytes");
            }
            if (Checksum != ComputeChecksum())
            {
                throw new ValidationException(nameof(Checksum), $"Wrong checksum. Expected: {ComputeChecksum()}");
            }

            if (ParentUnicodeName == null || Encoding.Unicode.GetBytes(ParentUnicodeName).Length != UNICODE_PARENT_LENGTH)
            {
                throw new ValidationException(nameof(ParentUnicodeName), $"Must be a {UNICODE_PARENT_LENGTH} byte UTF-16 string. Pad with nullbytes as needed");
            }
            if (ParentLocatorEntries == null || ParentLocatorEntries.Length != LOCATOR_ENTRY_COUNT)
            {
                throw new ValidationException(nameof(ParentLocatorEntries), $"Must contain {LOCATOR_ENTRY_COUNT} entries");
            }
            for (var i = 0; i < LOCATOR_ENTRY_COUNT;i++)
            {
                try
                {
                    ParentLocatorEntries[i].Validate();
                }
                catch (Exception ex)
                {
                    throw new ValidationException(nameof(ParentLocatorEntries), $"Element at index {i} failed to validate. See inner exception for details.", ex);
                }
            }
            if (Reserved2 == null || Reserved2.Length != RESERVED2_FIELD_SIZE || Reserved2.Any(m => m > 0))
            {
                throw new ValidationException(nameof(Reserved2), $"Must be {RESERVED2_FIELD_SIZE} nullbytes");
            }
        }

        /// <summary>
        /// Computes the expected checksum for this header without touching the stored checksum
        /// </summary>
        /// <returns>Checksum</returns>
        public int ComputeChecksum()
        {
            var Data = Export();
            var Sum = 0;
            //Checksum is calculated by assuming the checksum field is zero
            Data[36] = Data[37] = Data[38] = Data[39] = 0;
            for (var i = 0; i < Data.Length; i++)
            {
                Sum += Data[i];
            }
            return ~Sum;
        }

        /// <summary>
        /// Exports the current header to a 1024 byte array
        /// </summary>
        /// <remarks>
        /// This will not validate the disk or update the header.
        /// Use <see cref="Validate"/> and <see cref="ComputeChecksum"/> before exporting
        /// </remarks>
        /// <returns>Raw header data</returns>
        public byte[] Export()
        {
            using (var MS = new MemoryStream())
            {
                using (var BW = new BinaryWriter(MS))
                {
                    BW.Write(Encoding.Default.GetBytes(Cookie));
                    BW.Write(Tools.ToNetwork(DataOffset));
                    BW.Write(Tools.ToNetwork(TableOffset));
                    BW.Write(Tools.ToNetwork((ushort)HeaderVersion.Major));
                    BW.Write(Tools.ToNetwork((ushort)HeaderVersion.Minor));
                    BW.Write(Tools.ToNetwork(MaxTableEntries));
                    BW.Write(Tools.ToNetwork(BlockSize));
                    BW.Write(Tools.ToNetwork(Checksum));
                    BW.Write(ParentUniqueId.ToByteArray());
                    BW.Write(Tools.ToNetwork((uint)VHD.ToDiskTimestamp(ParentTimeStamp)));
                    BW.Write(Reserved1);
                    BW.Write(Encoding.Unicode.GetBytes(ParentUnicodeName));
                    foreach (var E in ParentLocatorEntries)
                    {
                        BW.Write(E.Export());
                    }
                    BW.Write(Reserved2);
                    BW.Flush();
                    return MS.ToArray();
                }
            }
        }

        /// <summary>
        /// Checks if this header equals another header
        /// </summary>
        /// <param name="obj">Reference header</param>
        /// <returns>true, if identical headers</returns>
        public override bool Equals(object obj)
        {
            //Note: Do not use "a is b" expression for type checking because this matches for derived types too.
            if (obj == null || obj.GetType() != typeof(DynamicHeader))
            {
                return base.Equals(obj);
            }
            return GetHashCode() == obj.GetHashCode();
        }

        /// <summary>
        /// Gets the hash code of this instance
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            //Magic constant. The purpose is to avoid the hash code being zero if all fields are zero.
            //You can change this as much as you want without any negative effects.
            //By default it's "1234" and "FEDC" interleaved to make it obvious that this is not special.
            //If you don't fear hell you can set it to zero,
            //but don't complain if false and zero suddenly equal a blank VHD header in a hashmap.
            int Code = 0x1F2E3D4C;
            foreach (var Prop in typeof(DynamicHeader).GetProperties())
            {
                var v = Prop.GetValue(this);
                if (v != null)
                {
                    if (Prop.PropertyType.IsArray)
                    {
                        foreach (var e in (Array)v)
                        {
                            if (e != null)
                            {
                                Code ^= e.GetHashCode();
                            }
                        }
                    }
                    else
                    {
                        Code ^= v.GetHashCode();
                    }
                }
            }
            return Code;
        }
    }
}
