using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd.VHD
{
    /// <summary>
    /// Represents a parent locator entry in the dynamic disk header
    /// </summary>
    public class LocatorEntry
    {
        /// <summary>
        /// The size in bytes of a locator entry
        /// </summary>
        public const int ENTRY_SIZE = 24;

        public string Code { get; set; }
        /// <summary>
        /// The number of 512 blocks needed to store the parent hard disk locator
        /// </summary>
        public uint DataSpace { get; set; }
        /// <summary>
        /// Stores the actual length of the hard disk locator
        /// </summary>
        public uint DataLength { get; set; }
        /// <summary>
        /// This field must be set to zero
        /// </summary>
        public uint Reserved { get; set; }
        /// <summary>
        /// Absolute offset of the platform data object
        /// </summary>
        public ulong DataOffset { get; set; }

        /// <summary>
        /// Initializes an empty locator entry
        /// </summary>
        public LocatorEntry()
        {
            Code = DataLocatorPlatformCode.NONE;
        }

        /// <summary>
        /// Initializes a locator entry from existing data
        /// </summary>
        /// <param name="Data">24 bytes</param>
        public LocatorEntry(byte[] Data)
        {
            if (Data == null)
            {
                throw new ArgumentNullException();
            }
            if (Data.Length != ENTRY_SIZE)
            {
                throw new ArgumentException($"Expected {ENTRY_SIZE} byte long argument, but is {Data.Length} bytes");
            }
            using (var MS = new MemoryStream(Data, false))
            {
                using (var BR = new BinaryReader(MS))
                {
                    Code = Encoding.Default.GetString(BR.ReadBytes(4));
                    DataSpace = Tools.ToNetwork(BR.ReadUInt32());
                    DataLength = Tools.ToNetwork(BR.ReadUInt32());
                    Reserved = Tools.ToNetwork(BR.ReadUInt32());
                    DataOffset = Tools.ToNetwork(BR.ReadUInt64());
                }
            }
        }

        /// <summary>
        /// Exports this instance to a byte array
        /// </summary>
        /// <returns>Byte array with <see cref="ENTRY_SIZE"/> elements</returns>
        public byte[] Export()
        {
            using (var MS = new MemoryStream())
            {
                using (var BW = new BinaryWriter(MS))
                {
                    BW.Write(Encoding.Default.GetBytes(Code));
                    BW.Write(Tools.ToNetwork(DataSpace));
                    BW.Write(Tools.ToNetwork(DataLength));
                    BW.Write(Tools.ToNetwork(Reserved));
                    BW.Write(Tools.ToNetwork(DataOffset));
                    BW.Flush();
                    return MS.ToArray();
                }
            }
        }

        /// <summary>
        /// Validates the current instance and throws a ValidationException if it's not valid
        /// </summary>
        public void Validate()
        {
            if (Code == null || Encoding.Default.GetByteCount(Code) != 4)
            {
                throw new ValidationException(nameof(Code), "Not a 4 byte string");
            }
            if (Reserved != 0)
            {
                throw new ValidationException(nameof(Reserved), "Must be zero");
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
            if (obj == null || obj.GetType() != typeof(LocatorEntry))
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
            foreach (var Prop in typeof(LocatorEntry).GetProperties())
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
