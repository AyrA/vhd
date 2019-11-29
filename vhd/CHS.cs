using System;

namespace vhd
{
    /// <summary>
    /// Represents the CHS value in a VHD <see cref="Header"/>
    /// </summary>
    public class CHS
    {
        /// <summary>
        /// Maximum allowed cylinder value.
        /// By default, this is the maximum possible value
        /// </summary>
        public const int MAX_CYLINDERS = 65535;
        /// <summary>
        /// Maximum allowed number of tracks per head
        /// </summary>
        public const int MAX_HEADS = 16;
        /// <summary>
        /// Maximum number of sector per track
        /// </summary>
        public const int MAX_SECTORS_PER_TRACK = 255;
        /// <summary>
        /// Maximum number of sectors total
        /// </summary>
        public const int MAX_SECTORS = MAX_CYLINDERS * MAX_HEADS * MAX_SECTORS_PER_TRACK;

        /// <summary>
        /// Sectors in each track
        /// </summary>
        public int SectorsPerTrack { get; set; }
        /// <summary>
        /// Number of Heads
        /// </summary>
        public int Heads { get; set; }
        /// <summary>
        /// Cylinders of the disk
        /// </summary>
        public int Cylinders { get; set; }

        public CHS()
        {
            SectorsPerTrack = MAX_SECTORS_PER_TRACK;
            Heads = MAX_HEADS;
            Cylinders = MAX_CYLINDERS;
        }

        /// <summary>
        /// Creates a CHS instance with the reference calculation from microsoft
        /// </summary>
        /// <param name="totalSectors"></param>
        public CHS(ulong totalSectors)
        {
            int cylinderTimesHeads = 0;

            //Cap the maximum sectors
            totalSectors = Math.Min(totalSectors, MAX_SECTORS);

            //For almost maxed out values
            if (totalSectors >= MAX_CYLINDERS * MAX_HEADS * 63)
            {
                SectorsPerTrack = MAX_SECTORS_PER_TRACK;
                Heads = MAX_HEADS;
                cylinderTimesHeads = (int)(totalSectors / (ulong)SectorsPerTrack);
            }
            else
            {
                //Begin with 17 sectors per track
                SectorsPerTrack = 17;
                cylinderTimesHeads = (int)(totalSectors / (ulong)SectorsPerTrack);
                Heads = Math.Max(4, (cylinderTimesHeads + 1023) / 1024);

                //If too many heads or too many cylinders, increase the sectors per track
                if (Heads > 16 || cylinderTimesHeads >= (Heads * 1024))
                {
                    SectorsPerTrack = 31;
                    Heads = 16;
                    cylinderTimesHeads = (int)(totalSectors / (ulong)SectorsPerTrack);
                }
                //If still too many cylinders, increase the sectors per track further
                if (cylinderTimesHeads >= (Heads * 1024))
                {
                    SectorsPerTrack = 63;
                    Heads = 16;
                    cylinderTimesHeads = (int)(totalSectors / (ulong)SectorsPerTrack);
                }
            }
            Cylinders = cylinderTimesHeads / Heads;
        }

        public CHS(int cylinders, int heads, int sectorsPerTrack)
        {
            SectorsPerTrack = sectorsPerTrack;
            Heads = heads;
            Cylinders = cylinders;
        }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) ||
                obj.GetType() == typeof(CHS) &&
                ((CHS)obj).Cylinders == Cylinders &&
                ((CHS)obj).Heads == Heads &&
                ((CHS)obj).SectorsPerTrack == SectorsPerTrack;
        }

        public override int GetHashCode()
        {
            return
                Cylinders ^ Heads ^ SectorsPerTrack;
        }
    }
}
