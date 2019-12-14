using System;

namespace vhd.VHD
{
    /// <summary>
    /// Possible VHD features
    /// </summary>
    [Flags]
    public enum VhdFeatures : uint
    {
        /// <summary>
        /// No special features are enabled
        /// </summary>
        None = 0,
        /// <summary>
        /// This disk is temporary and a candidate for deletion on shutdown
        /// </summary>
        Temporary = 1,
        /// <summary>
        /// Must always be set to 1.
        /// </summary>
        Reserved = 2
    }

    /// <summary>
    /// Possible types of a VHD
    /// </summary>
    /// <remarks>
    /// This value indicates the type of the file on the host,
    /// not the type of disk emulated in the virtual environment
    /// </remarks>
    public enum VhdType : uint
    {
        /// <summary>
        /// No type
        /// </summary>
        None = 0,
        /// <summary>
        /// Deprecated value
        /// </summary>
        Reserved_1 = 1,
        /// <summary>
        /// Fixed size virtual disk
        /// </summary>
        FixedDisk = 2,
        /// <summary>
        /// Dynamically expanding virtual disk
        /// </summary>
        DynamicDisk = 3,
        /// <summary>
        /// Differenting virtual disk
        /// </summary>
        /// <remarks>This means the current disk is the difference</remarks>
        DifferencingDisk = 4,
        /// <summary>
        /// Deprecated value
        /// </summary>
        Reserved_2 = 5,
        /// <summary>
        /// Deprecated value
        /// </summary>
        Reserved_3 = 6
    }

    public struct DataLocatorPlatformCode
    {
        /// <summary>
        /// No data locator platform
        /// </summary>
        public const string NONE = "\0\0\0\0";
        /// <summary>
        /// Windows relative file name (ANSI)
        /// </summary>
        /// <remarks>Deprecated, use <see cref="W2K_UNI_REL"/> instead</remarks>
        public const string W2K_REL = "Wi2r";
        /// <summary>
        /// Windows absolute file name (ANSI)
        /// </summary>
        /// <remarks>Deprecated, use <see cref="W2K_UNI_ABS"/> instead</remarks>
        public const string W2K_ABS = "Wi2k";
        /// <summary>
        /// Windows relative file name (UTF-16)
        /// </summary>
        public const string W2K_UNI_REL = "W2ru";
        /// <summary>
        /// Windows absolute file name (UTF-16)
        /// </summary>
        public const string W2K_UNI_ABS = "W2ku";
        /// <summary>
        /// Mac OS alias stored as a blob
        /// </summary>
        public const string MAC = "Mac ";
        /// <summary>
        /// A file URL with UTF-8 encoding conforming to RFC 2396
        /// </summary>
        public const string MAC_OSX = "MacX";
    }

    /// <summary>
    /// Contains known app strings
    /// </summary>
    public struct VhdCreatorApp
    {
        /// <summary>
        /// Microsoft virtual PC
        /// </summary>
        public const string MICROSOFT_VIRTUAL_PC = "vpc ";
        /// <summary>
        /// Microsoft Virtual Server
        /// </summary>
        public const string MICROSOFT_VIRTUAL_SERVER = "vs  ";
    }

    /// <summary>
    /// Contains known app versions
    /// </summary>
    public struct VhdCreatorVersion
    {
        /// <summary>
        /// Microsoft Virtual PC
        /// </summary>
        public static Version VirtualPc
        {
            get
            {
                return new Version(5, 0);
            }
        }

        /// <summary>
        /// Microsoft Virtual Server
        /// </summary>
        public static Version VirtualServer
        {
            get
            {
                return new Version(1, 0);
            }
        }
    }

    /// <summary>
    /// Contains known Host OS strings
    /// </summary>
    public struct VhdCreatorHost
    {
        /// <summary>
        /// Windows
        /// </summary>
        public const string WINDOWS = "Wi2k";
        /// <summary>
        /// Mac
        /// </summary>
        public const string MAC = "Mac ";
    }
}
