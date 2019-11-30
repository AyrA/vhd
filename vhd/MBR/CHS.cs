using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd.MBR
{
    public class CHS
    {
        /// <summary>
        /// Size of a CHS entry
        /// </summary>
        public const int CHS_SIZE = 3;
        /// <summary>
        /// Maximum number of sectors
        /// </summary>
        public const int MAX_SECTORS = 0x3F;
        /// <summary>
        /// Maximum number of cylinders
        /// </summary>
        public const int MAX_CYLINDERS = (0xF0 << 2) | 0xFF;

        public byte Heads { get; set; }
        public ushort Cylinders { get; set; }
        public byte Sectors { get; set; }

        /// <summary>
        /// Initializes an empty CHS instance
        /// </summary>
        public CHS()
        {

        }

        /// <summary>
        /// Initializes a CHS instance with the given values
        /// </summary>
        /// <param name="Data">Values from an MBR</param>
        public CHS(byte[] Data)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }
            if (Data.Length != CHS_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(Data), $"{CHS_SIZE} bytes expected, {Data.Length} given");
            }
            //Heads is the entire first byte
            Heads = Data[0];
            //Sectors are the lower 6 bits of the second byte, giving a range of 0-63
            Sectors = (byte)(Data[1] & MAX_SECTORS);
            //Cylinders are the top two bits of the second bytes
            //We shift the two bits into the location they need to be and mask off the rest
            Cylinders = (ushort)((Data[1] << 2) & 0xF00);
            //We add the lower 8 bits to the Cylinders value.
            Cylinders += Data[2];
        }

        /// <summary>
        /// Validates this instance and throws an exception if it's invalid
        /// </summary>
        public void Validate()
        {
            //Don't validate an empty entry
            if (Heads == 0 && Sectors == 0 && Cylinders == 0)
            {
                return;
            }
            //Fail individual zero entries
            if (Heads == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Heads), "Head count is zero");
            }
            if (Sectors == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Sectors), "Sector count is zero");
            }
            if (Cylinders == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Cylinders), "Cylinder count is zero");
            }
            //Fail too large entries
            if (Sectors > MAX_SECTORS)
            {
                throw new ArgumentOutOfRangeException(nameof(Sectors), $"Sector count can't be more than {MAX_SECTORS}");
            }
            if (Cylinders > MAX_CYLINDERS)
            {
                throw new ArgumentOutOfRangeException(nameof(Cylinders), $"Sector count can't be more than {MAX_CYLINDERS}");
            }
        }

        /// <summary>
        /// Exports data into the bytes from an MBR entry
        /// </summary>
        /// <returns>MBE entry bytes</returns>
        /// <remarks>
        /// This will not validate.
        /// Use <see cref="Validate"/> unless the entry is allowed to be invalid
        /// </remarks>
        public byte[] Export()
        {
            var ret = new byte[] {
                //Heads has its own field
                Heads,
                //Sectors occupy the lower 6 bits of this
                (byte)(Sectors | MAX_SECTORS),
                //Lover 8 bits of the cylinder entry here
                (byte)(Cylinders & 0xFF)
            };
            //Fill in the upper two bits of the sector entry with the upper 2 bits of the cylinder value
            ret[1] += (byte)((Cylinders & 0xF00) >> 2);
            return ret;
        }

        /// <summary>
        /// Gets a maxed out instance
        /// </summary>
        /// <returns>Maxed out instance</returns>
        public static CHS Maxed()
        {
            return new CHS(new byte[] { 0xFF, 0xFF, 0xFF });
        }
    }
}
