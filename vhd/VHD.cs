using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd
{
    public static class VHD
    {
        /// <summary>
        /// Creates a new VHD image
        /// </summary>
        /// <param name="Size">Size of the disk (as seen from the VM)</param>
        /// <param name="DiskType">Type of the VHD</param>
        /// <param name="Output">Stream to write the image to</param>
        /// <param name="Quick">
        /// Quick create. Will not write <paramref name="Size"/> bytes but instead just skip.
        /// The contents of the skipped range are undefined
        /// </param>
        public static void CreateVHD(ulong Size, VhdType DiskType, Stream Output, bool Quick)
        {
            var H = new Header();
            H.CurrentSize = H.OriginalSize = Size;
            H.DiskGeometry = new CHS(Size);
            if (Quick && !Output.CanSeek)
            {
                throw new ArgumentException($"{nameof(Output)} is not seekable, but {nameof(Quick)} is enabled");
            }
            switch (DiskType)
            {
                case VhdType.None:
                    throw new ArgumentException("'None' is not a valid VHD type");
                case VhdType.FixedDisk:
                    H.Checksum = H.ComputeChecksum();
                    H.Validate();
                    SeekOrWrite(Size, Output, Quick);
                    Output.Write(H.Export(), 0, Header.HEADER_LENGTH);
                    break;
                case VhdType.DynamicDisk:
                case VhdType.DifferencingDisk:
                default:
                    throw new NotImplementedException(nameof(VHD) + "." + nameof(CreateVHD) + $": Disk type {DiskType} is not implemented yet");
            }
        }

        private static void SeekOrWrite(ulong Size, Stream Output, bool Quick)
        {
            if (Quick)
            {
                do
                {
                    var Offset = (long)Math.Min(long.MaxValue, Size);
                    Output.Seek(Offset, SeekOrigin.Current);
                    Size -= (ulong)Offset;
                } while (Size > 0);
            }
            else
            {
                //Write in 1M chunks
                byte[] Empty = new byte[1024 * 1024];
                while (Size > 0)
                {
#if DEBUG
                    Console.Error.Write('.');
#endif
                    var Amount = (int)Math.Min((ulong)Empty.Length, Size);
                    Output.Write(Empty, 0, Amount);
                    Size -= (ulong)Amount;
                }
            }
        }
    }
}
