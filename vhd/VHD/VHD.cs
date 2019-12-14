using System;
using System.IO;
using System.Linq;

namespace vhd.VHD
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
        /// The contents of the skipped range are undefined and depend on the file system the disk is on.
        /// On FAT, the parameter does nothing and the system writes bytes for us.
        /// On NTFS, the bytes are guaranteed to appear as zero
        /// </param>
        public static void CreateVHD(ulong Size, VhdType DiskType, Stream Output, bool Quick)
        {
            var H = new Footer();
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
                    Output.Write(H.Export(), 0, Footer.HEADER_LENGTH);
                    break;
                case VhdType.DynamicDisk:
                case VhdType.DifferencingDisk:
                default:
                    throw new NotImplementedException(nameof(VHD) + "." + nameof(CreateVHD) + $": Disk type {DiskType} is not implemented yet");
            }
        }

        /// <summary>
        /// Seeks a stream a given amount or writes nullbytes
        /// </summary>
        /// <param name="Size">Number of bytes to seek or write</param>
        /// <param name="VhdStream">Stream to operate on</param>
        /// <param name="Quick">
        /// Seek, don't write.
        /// Requires <see cref="Stream.CanSeek"/> to be set
        /// and <see cref="Stream.SetLength(long)"/> to work
        /// and <see cref="Stream.Position"/> to work</param>
        private static void SeekOrWrite(ulong Size, Stream VhdStream, bool Quick)
        {
            if (Quick)
            {
                var LengthDiff = (ulong)(VhdStream.Length - VhdStream.Position);
                //SetLength only works up to long.MaxValue
                if (Size < long.MaxValue && Size + LengthDiff < long.MaxValue)
                {
                    VhdStream.SetLength((long)(Size + LengthDiff));
                }
                else
                {
                    //Use traditional method
                    SeekOrWrite(Size, VhdStream, false);
                    /*
                    do
                    {
                        var Offset = (long)Math.Min(long.MaxValue, Size);
                        Output.Seek(Offset, SeekOrigin.Current);
                        Size -= (ulong)Offset;
                    } while (Size > 0);
                    //*/
                }
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
                    VhdStream.Write(Empty, 0, Amount);
                    Size -= (ulong)Amount;
                }
            }
        }

        /// <summary>
        /// Resizes an existing VHD in-place
        /// </summary>
        /// <param name="NewSize">New size of the disk (as seen from the VM)</param>
        /// <param name="VhdStream">VHD file stream</param>
        /// <param name="Quick">For expansion, use Quick mode. For reduction, has no effect</param>
        public static void ResizeVHD(ulong NewSize, Stream VhdStream, bool Quick)
        {
            if (VhdStream == null)
            {
                throw new ArgumentNullException(nameof(VhdStream));
            }
            if (NewSize == 0 || NewSize % 512 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(NewSize), "The new size must be 512 or a multiple of it");
            }

            if (!VhdStream.CanRead)
            {
                throw new IOException($"{nameof(VhdStream)} is not readable");
            }
            if (!VhdStream.CanWrite)
            {
                throw new IOException($"{nameof(VhdStream)} is not writable");
            }
            if (!VhdStream.CanSeek)
            {
                throw new IOException($"{nameof(VhdStream)} is not seekable");
            }

            VhdStream.Seek(-512, SeekOrigin.End);
            var H = new Footer(VhdStream);

            switch (H.DiskType)
            {
                case VhdType.FixedDisk:
                    if (H.CurrentSize < NewSize)
                    {
                        VhdStream.Seek(-512, SeekOrigin.End);
                        SeekOrWrite(NewSize - H.CurrentSize, VhdStream, Quick);
                        H.CurrentSize = NewSize;
                        H.DiskGeometry = new CHS(NewSize);
                        VhdStream.Write(H.Export(), 0, Footer.HEADER_LENGTH);
                    }
                    break;
                default:
                    throw new NotImplementedException("Resizing of for this disk type is not yet available");
            }
        }

        /// <summary>
        /// Gets the minimum possible disk size for the given disk
        /// </summary>
        /// <param name="VhdStream">Stream placed at the start of the MBR</param>
        /// <returns>Minimum disk size in bytes</returns>
        public static ulong GetMinDiskSize(Stream VhdStream)
        {
            var M = new MBR.MasterBootRecord(VhdStream);
            M.Validate();
            return GetMinDiskSize(M);
        }

        /// <summary>
        /// Gets the minimum possible disk size for the given disk
        /// </summary>
        /// <param name="MBRData">MBR data Size: <see cref="MBR.MasterBootRecord.MBR_SIZE"/></param>
        /// <returns>Minimum disk size in bytes</returns>
        public static ulong GetMinDiskSize(byte[] MBRData)
        {
            var M = new MBR.MasterBootRecord(MBRData);
            M.Validate();
            return GetMinDiskSize(M);
        }

        /// <summary>
        /// Gets the minimum possible disk size for the given disk
        /// </summary>
        /// <param name="DiskMBR">An MBR</param>
        /// <returns>Minimum disk size in bytes</returns>
        /// <remarks>This will not validate the partition table</remarks>
        public static ulong GetMinDiskSize(MBR.MasterBootRecord DiskMBR)
        {
            return DiskMBR.Partitions
                .Where(m => m.LBAFirstSector > 0 && m.LBASectorCount > 0)
                .Max(m => m.LBAFirstSector + (ulong)m.LBASectorCount - 1UL) * Footer.SECTOR_SIZE;
        }
    }
}
