using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd.VHD
{
    /// <summary>
    /// Represents a VHD header
    /// </summary>
    public class Footer
    {
        /// <summary>
        /// Default file format major version
        /// </summary>
        public const int V_MAJOR_DEFAULT = 1;
        /// <summary>
        /// Default file format minor version
        /// </summary>
        public const int V_MINOR_DEFAULT = 0;
        /// <summary>
        /// Default data offset value
        /// </summary>
        public const ulong OFFSET_NONE = ulong.MaxValue;
        /// <summary>
        /// Default value for the header cookie
        /// </summary>
        public const string DEFAULT_COOKIE = "conectix";
        /// <summary>
        /// Size of the reserved field data
        /// </summary>
        public const int RESERVED_FIELD_SIZE = 427;
        /// <summary>
        /// Size of a regular VHD Header
        /// </summary>
        public const int HEADER_LENGTH = 512;
        /// <summary>
        /// The size in bytes of a sector
        /// </summary>
        public const int SECTOR_SIZE = 512;

        /// <summary>
        /// Identifies the original creator of the disk
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// Enabled features of this disk
        /// </summary>
        public VhdFeatures Features { get; set; }

        /// <summary>
        /// File format version. Currently, only 1.0 is defined.
        /// Only the <see cref="Version.Major"/> and <see cref="Version.Minor"/> is defined
        /// </summary>
        /// <remarks>
        /// Major version is incremeneted whenever a change is incompatible
        /// </remarks>
        public Version FileFormatVersion { get; set; }

        /// <summary>
        /// Offset of the disk data from the file start.
        /// Must be set to <see cref="OFFSET_NONE"/> for fixed disks
        /// </summary>
        public ulong DataOffset { get; set; }

        /// <summary>
        /// Date this disk was created
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// The name of the application that creates the disk.
        /// 
        /// </summary>
        public string CreatorApplication { get; set; }

        /// <summary>
        /// Version of the creator application.
        /// Only the <see cref="Version.Major"/> and <see cref="Version.Minor"/> can be used.
        /// </summary>
        public Version CreatorVersion { get; set; }

        /// <summary>
        /// Host operating system this disk was created on
        /// </summary>
        public string CreatorHostOS { get; set; }

        /// <summary>
        /// Original disk size during creation.
        /// This field is for informational purposes.
        /// </summary>
        /// <remarks>This is from the perspective of the VM</remarks>
        public ulong OriginalSize { get; set; }

        /// <summary>
        /// Current disk size
        /// </summary>
        /// <remarks>This is from the perspective of the VM</remarks>
        public ulong CurrentSize { get; set; }

        /// <summary>
        /// Disk geometry for classical Cylinder-Heads-Sectors setup (ATA emulation)
        /// </summary>
        /// <remarks>
        /// When the user creates a hard disk of a certain size,
        /// the size of the hard disk image in the virtual machine is smaller than that created by the user.
        /// This is because CHS value calculated from the hard disk size is rounded down.
        /// </remarks>
        public CHS DiskGeometry { get; set; }

        /// <summary>
        /// Disk type of this VHD
        /// </summary>
        public VhdType DiskType { get; set; }

        /// <summary>
        /// Checksum of this disk
        /// </summary>
        /// <remarks>
        /// The checksum is a ones complement of the sum of the header bytes.
        /// During sum calculation, the checksum field assumes a value of zero
        /// </remarks>
        public int Checksum { get; set; }

        /// <summary>
        /// Unique Id of this disk
        /// </summary>
        /// <remarks>
        /// This field is used to associate a parent hard disk image with its differencing hard disk image(s).
        /// Changing this decouples the disks unless it's changed across them all.
        /// </remarks>
        public Guid DiskId { get; set; }

        /// <summary>
        /// Indicates whether the disk is in a saved state or not.
        /// If in a saved state, operations such as compaction or expansion can't be performed.
        /// </summary>
        /// <remarks>
        /// Resetting this value can cause unintended side effects.
        /// Do not change unless you are certain that the disk is "clean"
        /// </remarks>
        public bool SavedState { get; set; }

        /// <summary>
        /// Reserved header values.
        /// This should be all set to zeros
        /// </summary>
        public byte[] Reserved { get; set; }

        /// <summary>
        /// Creates a VHD Header with most values set to their defaults
        /// </summary>
        public Footer()
        {
            Cookie = DEFAULT_COOKIE;
            FileFormatVersion = new Version(V_MAJOR_DEFAULT, V_MINOR_DEFAULT);
            DataOffset = OFFSET_NONE;
            TimeStamp = DateTime.Now;
            CreatorApplication = "ayra";
            CreatorVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            CreatorHostOS = VhdCreatorHost.WINDOWS;
            OriginalSize = 0;
            CurrentSize = 0;
            DiskGeometry = new CHS(0, 0, 0);
            DiskType = VhdType.FixedDisk;
            DiskId = Guid.NewGuid();
            Features = VhdFeatures.Reserved;
            Reserved = new byte[RESERVED_FIELD_SIZE];
            Checksum = ComputeChecksum();
        }

        /// <summary>
        /// Creates a VHD header from given data
        /// </summary>
        /// <param name="Data">Data, must be of at least <see cref="HEADER_LENGTH"/> size</param>
        /// <remarks>The data must start with the header but is allowed to contain excessive bytes which are ignored.</remarks>
        public Footer(byte[] Data)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }
            if (Data.Length == HEADER_LENGTH - 1)
            {
                throw new ArgumentException($"The {HEADER_LENGTH - 1} byte header can't be processed. Add an additional nullbyte if you feel it's a valid header.", nameof(Data));
            }
            if (Data.Length < HEADER_LENGTH)
            {
                throw new ArgumentOutOfRangeException(nameof(Data), $"{HEADER_LENGTH} bytes expected, {Data.Length} given");
            }
            using (var MS = new MemoryStream(Data))
            {
                FromStream(MS);
            }
        }

        /// <summary>
        /// Creates a VHD header
        /// </summary>
        /// <param name="S">Stream to read header from</param>
        /// <remarks>The stream must be positioned at the header start</remarks>
        public Footer(Stream S)
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
                Features = (VhdFeatures)Tools.ToNetwork(BR.ReadUInt32());
                //12
                FileFormatVersion = new Version(Tools.ToNetwork(BR.ReadUInt16()), Tools.ToNetwork(BR.ReadUInt16()));
                //16
                DataOffset = Tools.ToNetwork(BR.ReadUInt64());
                //24
                TimeStamp = FromTimeStamp(Tools.ToNetwork(BR.ReadInt32()));
                //28
                CreatorApplication = E.GetString(BR.ReadBytes(4));
                //32
                CreatorVersion = new Version(Tools.ToNetwork(BR.ReadUInt16()), Tools.ToNetwork(BR.ReadUInt16()));
                //36
                CreatorHostOS = E.GetString(BR.ReadBytes(4));
                //40
                OriginalSize = Tools.ToNetwork(BR.ReadUInt64());
                //48
                CurrentSize = Tools.ToNetwork(BR.ReadUInt64());
                //56
                DiskGeometry = new CHS(
                    Tools.ToNetwork(BR.ReadUInt16()),
                    BR.ReadByte(),
                    BR.ReadByte()
                    );
                //60
                DiskType = (VhdType)Tools.ToNetwork(BR.ReadUInt32());
                //64
                Checksum = Tools.ToNetwork(BR.ReadInt32());
                //68
                DiskId = new Guid(BR.ReadBytes(16));
                //84
                SavedState = BR.ReadByte() == 1;
                //85
                Reserved = BR.ReadBytes(RESERVED_FIELD_SIZE);
                //512
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
                throw new FormatException($"{nameof(Cookie)} is not an 8 byte string");
            }
            if (!Enum.IsDefined(typeof(VhdFeatures), Features))
            {
                throw new FormatException($"{nameof(Features)} is not one or a combination of the defined enum values");
            }
            if (!Features.HasFlag(VhdFeatures.Reserved))
            {
                throw new FormatException($"{nameof(Features)} must have the reservedflag set");
            }
            if ((int)Features >= ((int)VhdFeatures.Reserved << 1))
            {
                throw new FormatException($"{nameof(Features)} must have undefined bits set to zero");
            }

            if (FileFormatVersion == null)
            {
                throw new FormatException($"{nameof(FileFormatVersion)} must be defined");
            }
            if (!Tools.InRange(ushort.MinValue, FileFormatVersion.Major, ushort.MaxValue))
            {
                throw new FormatException($"{nameof(FileFormatVersion)}.{nameof(Version.Major)} must be in the range of {ushort.MaxValue}-{ushort.MinValue}");
            }
            if (!Tools.InRange(ushort.MinValue, FileFormatVersion.Major, ushort.MaxValue))
            {
                throw new FormatException($"{nameof(FileFormatVersion)}.{nameof(Version.Minor)} must be in the range of {ushort.MaxValue}-{ushort.MinValue}");
            }

            if (DiskType == VhdType.FixedDisk && DataOffset != OFFSET_NONE)
            {
                throw new FormatException($"{nameof(DataOffset)} must be set to ({nameof(OFFSET_NONE)}){OFFSET_NONE} for a fixed vhd type");
            }

            if (ToTimestamp(TimeStamp) < int.MinValue || ToTimestamp(TimeStamp) > int.MaxValue)
            {
                throw new FormatException($"{nameof(TimeStamp)} can't be more than {int.MinValue} or {int.MaxValue} seconds away from Jan 1, 2000");
            }

            if (CreatorApplication == null || E.GetBytes(CreatorApplication).Length != 4)
            {
                throw new FormatException($"{nameof(CreatorApplication)} is not a 4 byte string");
            }

            if (CreatorVersion == null)
            {
                throw new FormatException($"{nameof(CreatorVersion)} must be defined");
            }
            if (!Tools.InRange(ushort.MinValue, CreatorVersion.Major, ushort.MaxValue))
            {
                throw new FormatException($"{nameof(CreatorVersion)}.{nameof(Version.Major)} must be in the range of {ushort.MaxValue}-{ushort.MinValue}");
            }
            if (!Tools.InRange(ushort.MinValue, CreatorVersion.Major, ushort.MaxValue))
            {
                throw new FormatException($"{nameof(CreatorVersion)}.{nameof(Version.Major)} must be in the range of {ushort.MaxValue}-{ushort.MinValue}");
            }

            if (CreatorHostOS == null || E.GetBytes(CreatorHostOS).Length != 4)
            {
                throw new FormatException($"{nameof(CreatorHostOS)} is not a 4 byte string");
            }

            if (OriginalSize == 0)
            {
                throw new FormatException($"{nameof(OriginalSize)} can't be zero");
            }
            if (CurrentSize == 0)
            {
                throw new FormatException($"{nameof(CurrentSize)} can't be zero");
            }
            if (CurrentSize % 512 != 0)
            {
                throw new FormatException($"{nameof(CurrentSize)} must be a multiple of 512");
            }

            if (DiskGeometry == null)
            {
                throw new FormatException($"{nameof(DiskGeometry)} is not defined");
            }
            if (DiskGeometry.Cylinders < 1 || DiskGeometry.Cylinders > CHS.MAX_CYLINDERS)
            {
                throw new FormatException($"{nameof(DiskGeometry)}.{nameof(CHS.Cylinders)} must be in the range of 1-{CHS.MAX_CYLINDERS}");
            }
            if (DiskGeometry.Heads < 1 || DiskGeometry.Heads > CHS.MAX_HEADS)
            {
                throw new FormatException($"{nameof(DiskGeometry)}.{nameof(CHS.Heads)} must be in the range of 1-{CHS.MAX_HEADS}");
            }
            if (DiskGeometry.SectorsPerTrack < 1 || DiskGeometry.SectorsPerTrack > CHS.MAX_SECTORS_PER_TRACK)
            {
                throw new FormatException($"{nameof(DiskGeometry)}.{nameof(CHS.SectorsPerTrack)} must be in the range of 1-{CHS.MAX_SECTORS_PER_TRACK}");
            }
            if (!DiskGeometry.Equals(new CHS(CurrentSize)))
            {
                throw new FormatException($"{nameof(DiskGeometry)} does not matches {nameof(CurrentSize)}");
            }


            if (!Enum.IsDefined(typeof(VhdType), DiskType))
            {
                throw new FormatException($"{nameof(DiskType)} is not one of the defined enum values");
            }

            if (Checksum != ComputeChecksum())
            {
                throw new FormatException($"{nameof(Checksum)} is invalid.");
            }

            if (DiskId == Guid.Empty)
            {
                throw new FormatException($"{nameof(DiskId)} is invalid");
            }
            if (Reserved == null || Reserved.Length != RESERVED_FIELD_SIZE)
            {
                throw new FormatException($"{nameof(Reserved)} must be {RESERVED_FIELD_SIZE} bytes long");
            }
            if (Reserved.Any(m => m != 0))
            {
                throw new FormatException($"{nameof(Reserved)} must be made up of nullbytes only");
            }
        }

        /// <summary>
        /// Exports the current header to a 512 byte array
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
                    BW.Write(Tools.ToNetwork((uint)Features));
                    BW.Write(Tools.ToNetwork((ushort)FileFormatVersion.Major));
                    BW.Write(Tools.ToNetwork((ushort)FileFormatVersion.Minor));
                    BW.Write(Tools.ToNetwork(DataOffset));
                    BW.Write(Tools.ToNetwork((int)ToTimestamp(TimeStamp)));

                    BW.Write(Encoding.Default.GetBytes(CreatorApplication));
                    BW.Write(Tools.ToNetwork((ushort)CreatorVersion.Major));
                    BW.Write(Tools.ToNetwork((ushort)CreatorVersion.Minor));
                    BW.Write(Encoding.Default.GetBytes(CreatorHostOS));

                    BW.Write(Tools.ToNetwork(OriginalSize));
                    BW.Write(Tools.ToNetwork(CurrentSize));

                    BW.Write(Tools.ToNetwork((ushort)DiskGeometry.Cylinders));
                    BW.Write((byte)DiskGeometry.Heads);
                    BW.Write((byte)DiskGeometry.SectorsPerTrack);

                    BW.Write(Tools.ToNetwork((uint)DiskType));

                    BW.Write(Tools.ToNetwork(Checksum));

                    BW.Write(DiskId.ToByteArray());
                    BW.Write((byte)(SavedState ? 1 : 0));
                    BW.Write(Reserved);

                    BW.Flush();

                    return MS.ToArray();
                }
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
            Data[64] = Data[65] = Data[66] = Data[67] = 0;
            for (var i = 0; i < Data.Length; i++)
            {
                Sum += Data[i];
            }
            return ~Sum;
        }

        /// <summary>
        /// Converts a DateTime object into a timestamp
        /// </summary>
        /// <param name="DT">DateTime object</param>
        /// <returns>Timestamp</returns>
        private double ToTimestamp(DateTime DT)
        {
            return DT
                .ToUniversalTime()
                .Subtract(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;
        }

        /// <summary>
        /// Converts a Timestamp into a DateTime object
        /// </summary>
        /// <param name="Seconds">Number of seconds since 2000-01-01 00:00:00 UTC</param>
        /// <returns>DateTime instance</returns>
        /// <remarks><paramref name="Seconds"/> can also be negative although the format didn't exist back then</remarks>
        private DateTime FromTimeStamp(double Seconds)
        {
            return (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .AddSeconds(Seconds)
                .ToLocalTime();
        }

        /// <summary>
        /// Checks if this header equals another header
        /// </summary>
        /// <param name="obj">Reference header</param>
        /// <returns>true, if identical headers</returns>
        public override bool Equals(object obj)
        {
            //Note: Do not use "a is b" expression for type checking because this matches for derived types too.
            if (obj == null || obj.GetType() != typeof(Footer))
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
            foreach (var Prop in typeof(Footer).GetProperties())
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
