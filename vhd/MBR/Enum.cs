using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd.MBR
{
    /// <summary>
    /// List of partition types
    /// </summary>
    /// <seealso cref="https://en.wikipedia.org/wiki/Partition_type"/>
    public enum PartitionType : byte
    {
        /// <summary>
        /// This partition is not in use
        /// </summary>
        Empty = 0x00,
        /// <summary>
        /// FAT12 limited to the first 32 MB of the disk
        /// </summary>
        FAT12Small = 0x01,
        /// <summary>
        /// XENIX root partition
        /// </summary>
        XENIXRoot = 0x02,
        /// <summary>
        /// XENIX user partition
        /// </summary>
        XENIXUser = 0x03,
        /// <summary>
        /// FAT16 limited to the first 32 MB of the disk
        /// </summary>
        FAT16Small = 0x04,
        /// <summary>
        /// Extended partition, limited to the first 8 GB of the disk
        /// </summary>
        ExtendedPartition = 0x05,
        /// <summary>
        /// FAT16 limited to the first 8 GB of the disk
        /// </summary>
        FAT16Limited = 0x06,
        /// <summary>
        /// Partition used by different file systems.
        /// Generally used for proprietary file systems that need special drivers
        /// </summary>
        IFS_HPFS_NTFS_exFAT_QNX = 0x07,
        /// <summary>
        /// Proprietary but mostly dead file systems
        /// </summary>
        Commodore_OS2_AIXBoot_QNX = 0x08,
        /// <summary>
        /// Proprietary but mostly dead file systems
        /// </summary>
        AIXData_QNX_Coherent_RBF = 0x09,
        /// <summary>
        /// Proprietary but mostly dead file systems
        /// </summary>
        OS2Boot_CoherentSwap = 0x0A,
        /// <summary>
        /// FAT32 file system, limited to CHS addressing
        /// </summary>
        FAT32Limited = 0x0B,
        /// <summary>
        /// FAT32 with LBA instead of CHS
        /// </summary>
        FAT32LBA = 0x0C,
        /// <summary>
        /// FAT16 with LBA instead of CHS
        /// </summary>
        FAT16LBA = 0x0E,
        /// <summary>
        /// Extended partition with LBA instead of CHS
        /// </summary>
        ExtendedPartitionLBA = 0x0F,
        /// <summary>
        /// Unisys, Missing documentation?
        /// </summary>
        Unisys = 0x10,
        /// <summary>
        /// Logical or hidden FAT12 by two different vendors
        /// </summary>
        LogicalFAT_HiddenFAT = 0x11,
        /// <summary>
        /// Various rescue filesystems.
        /// Generally a bootable FAT12,FAT16,FAT32 with a small recovery OS installed
        /// </summary>
        RescueFAT = 0x12,
        /// <summary>
        /// Proprietary but mostly dead file systems
        /// </summary>
        ASTLogicalFAT_HiddenFAT16_Omega = 0x14,
        /// <summary>
        /// Proprietary but mostly dead file systems.
        /// The hidden extended behaves like <seealso cref="ExtendedPartition"/>
        /// </summary>
        HiddenExtendedCHS_MaverickSwap = 0x15,
        /// <summary>
        /// Behaves like <seealso cref="FAT16Limited"/>
        /// </summary>
        OS2HiddenFAT16 = 0x16,
        /// <summary>
        /// Partition used by different file systems.
        /// Generally used for proprietary file systems that need special drivers.
        /// Hidden version of <seealso cref="IFS_HPFS_NTFS_exFAT_QNX"/>
        /// </summary>
        HiddenIFS_HPFS_NTFS_exFAT_QNX = 0x17,
        /// <summary>
        /// Partition for hibernation mode in AST Windows
        /// </summary>
        ASTZeroVoltSuspension = 0x18,
        /// <summary>
        /// Variant of <see cref="WindowsMobileXIP_OFS1"/>
        /// </summary>
        WillowtechPhoton = 0x19,
        /// <summary>
        /// Hidden version of <see cref="FAT32Limited"/>
        /// </summary>
        HiddenFAT32 = 0x1B,
        /// <summary>
        /// Hidden version of <see cref="FAT32LBA"/>
        /// </summary>
        HiddenFAT32LBA_AsusRecovery = 0x1C,
        /// <summary>
        /// Hidden version of <see cref="FAT16LBA"/>
        /// </summary>
        HiddenFAT16LBA = 0x1E,
        /// <summary>
        /// Hidden version of <see cref="ExtendedPartitionLBA"/>
        /// </summary>
        HiddenExtendedLBA = 0x1F,
        /// <summary>
        /// Execute in place partition for windows mobile and variant of <see cref="WillowtechPhoton"/>
        /// </summary>
        WindowsMobileXIP_OFS1 = 0x20,
        /// <summary>
        /// HP volume expansion and oxygen filesystem
        /// </summary>
        HPVolumeExpansion_FSo2 = 0x21,
        /// <summary>
        /// Ogygen extended partition
        /// </summary>
        OxygenExtendedPartition = 0x22,
        /// <summary>
        /// Execute In Place boot partition for Windows Mobile
        /// </summary>
        WindowsMobileBootXIP = 0x23,
        /// <summary>
        /// Logical addresses FAT12 or FAT16 for NEC
        /// </summary>
        LogicalNECFAT = 0x24,
        /// <summary>
        /// Windows Mobile file system
        /// </summary>
        WindowsMobileIMGFS = 0x25,
        /// <summary>
        /// Reserved by Microsoft
        /// </summary>
        MSReserved1 = 0x26,
        /// <summary>
        /// Version of <see cref="RescueFAT"/> but with proprietary file systems
        /// </summary>
        RescueNTFS = 0x27,
        /// <summary>
        /// AtheOS file system, also reserved for DR DOS
        /// </summary>
        AthFS_Reserved = 0x2A,
        /// <summary>
        /// SyllableSecure variant of AthFS
        /// </summary>
        SylStor = 0x2B,
        /// <summary>
        /// Reserved by Microsoft
        /// </summary>
        MSReserved2 = 0x31,
        /// <summary>
        /// Alien Internet Services
        /// </summary>
        NOS = 0x32,
        /// <summary>
        /// Reserved by Microsoft
        /// </summary>
        MSReserved3 = 0x33,
        /// <summary>
        /// Reserved by Microsoft
        /// </summary>
        MSReserved4 = 0x34,
        /// <summary>
        /// OS/2 implementation of AIX
        /// </summary>
        JFS = 0x35,
        /// <summary>
        /// Reserved by Microsoft
        /// </summary>
        MSReserved5 = 0x36,
        /// <summary>
        /// THEOS version 3.2 up to 2 GB
        /// </summary>
        THEOS32 = 0x38,
        /// <summary>
        /// THEOS version 4.0 spanned volume
        /// </summary>
        THEOS40Spanned = 0x39,
        /// <summary>
        /// THEOS version 4.0, up to 4 GB
        /// </summary>
        THEOS404GB = 0x3A,
        /// <summary>
        /// THEOS version 4.0, extended partition
        /// </summary>
        THEOS40Extended = 0x3B,
        /// <summary>
        /// PowerQuest repair progress partition
        /// </summary>
        PowerQuestRepair = 0x3C,
        /// <summary>
        /// Hidden NetWare partition
        /// </summary>
        PowerQuestHiddenNetware = 0x3D,
        /// <summary>
        /// PICK R83 or Venix 80286
        /// </summary>
        PICK_Venix = 0x40,
        /// <summary>
        /// Personal RISC boot partition, Old linux partition, PPC PReP boot
        /// </summary>
        PersonalRISC_OldLinux_PPC = 0x41,
        /// <summary>
        /// Secure File System, old linux swap or MS extended partition marker
        /// </summary>
        SFS_OldLinuxSwap_DynamicExtendedPartition = 0x42,
        /// <summary>
        /// Old linux native partition
        /// </summary>
        OldLinuxNative = 0x43,
        /// <summary>
        /// GoBack partition of various vendors
        /// </summary>
        GoBack = 0x44,
        /// <summary>
        /// Priam, Boot US Boot Manager, or EUMEL/ELAN
        /// </summary>
        Priam_BootUSBootManager_EUMEL = 0x45,
        /// <summary>
        /// EUMEL/ELAN
        /// </summary>
        EUMEL_0x46 = 0x46,
        /// <summary>
        /// EUMEL/ELAN
        /// </summary>
        EUMEL_0x47 = 0x47,
        /// <summary>
        /// EUMEL/ELAN
        /// </summary>
        EUMEL_0x48 = 0x48,
        /// <summary>
        /// Aquilla or ALFS/THIN advanced lightweight file system for DOS 
        /// </summary>
        Aquilla_ALFS = 0x4A,
        /// <summary>
        /// ETH Zürich Oberon OS file system
        /// </summary>
        AosFS = 0x4C,
        /// <summary>
        /// Primary QNX partition
        /// </summary>
        PrimaryQNX = 0x4D,
        /// <summary>
        /// Secondary QNX partition
        /// </summary>
        SecondaryQNX = 0x4E,
        /// <summary>
        /// Tertiary QNX partition, Oberon OS Boot/native file system
        /// </summary>
        TertiaryQNX_AosFSBoot = 0x4F,
        /// <summary>
        /// Alternative native file system, Disk Manager readonly,
        /// Lynx RTOS, Novell undocumented
        /// </summary>
        AosFS_DMRO_LynxOS_Novell = 0x50,
        /// <summary>
        /// Novell undocumented, Disk Manager readwrite
        /// </summary>
        Novell_DMRW = 0x51,
        /// <summary>
        /// CP/M-80
        /// </summary>
        CPM80_MicroportSystemVAT = 0x52,
        /// <summary>
        /// Disk Manager 6 auxiliary
        /// </summary>
        DiskManagerAuxiliary = 0x53,
        /// <summary>
        /// Disk Manager 6 Dynamic Drive Overlay
        /// </summary>
        DiskManagerDDO = 0x54,
        /// <summary>
        /// EZ-Drive, Maxtor, MaxBlast, or DriveGuide INT 13h redirector volume
        /// </summary>
        EZDrive = 0x55,
        /// <summary>
        /// AT&T Logical FAT12 or FAT16, EZ_BIOS, VFeature partitioned volume
        /// </summary>
        ATTDOS_EZBIOS_VFeature = 0x56,
        /// <summary>
        /// DrivePro or Novell VNDI
        /// </summary>
        DrivePro_VNDI = 0x57,
        /// <summary>
        /// Priam EDisk Partitioned Volume
        /// <see cref="Priam_BootUSBootManager_EUMEL"/>
        /// </summary>
        EDisk = 0x5C,
        /// <summary>
        /// Hidden FAT12
        /// </summary>
        HiddenFAT12 = 0x61,
        /// <summary>
        /// Readonly hidden FAT12 or generic unix
        /// </summary>
        HiddenReadonlyFAT12_GeneralUnix = 0x63,
        /// <summary>
        /// Hidden FAT16 , Netware FS 286/2, PC-ARMOUR
        /// </summary>
        HiddenFAT16_Netware286_PCARMOUR = 0x64,
        /// <summary>
        /// NetWare 386
        /// </summary>
        NetWare386 = 0x65,
        /// <summary>
        /// Hidden FAT16, Storage Management Service, Netware FS 386
        /// </summary>
        HiddenFAT16_SMS_NetWare386 = 0x66,
        /// <summary>
        /// NetWare Wolf Mountain
        /// </summary>
        NetWareWolf = 0x67,
        /// <summary>
        /// Undocumented NetWare
        /// </summary>
        NetWare = 0x68,
        /// <summary>
        /// Novell Storage Services
        /// </summary>
        NSS = 0x69,
        /// <summary>
        /// DragonFly BSD partition
        /// </summary>
        BSDSlice = 0x6C,
        /// <summary>
        /// DiskSecure Multiboot
        /// </summary>
        DiskSecureMultiboot = 0x70,
        /// <summary>
        /// Reserved by Microsoft
        /// </summary>
        MSReserved6 = 0x71,
        /// <summary>
        /// APTI alternative FAT12, Unix V7/x86
        /// </summary>
        APTI_V7 = 0x72,
        /// <summary>
        /// Reserved by Microsoft
        /// </summary>
        MSReserved7 = 0x73,
        /// <summary>
        /// SpeedStor hidden FAT 16B
        /// </summary>
        SpeedStorHiddenFAT16 = 0x74,
        /// <summary>
        /// IBM PC/IX
        /// </summary>
        IBMPCIX = 0x75,
        /// <summary>
        /// SpeedStor hidden FAT 16B readonly
        /// </summary>
        SpeedStorHiddenFAT16RO = 0x76,
        /// <summary>
        /// Novell VNDI, M2FS, M2CS
        /// </summary>
        VNDI_M2FS_M2CS = 0x77,
        /// <summary>
        /// XOSL bootloader file system
        /// </summary>
        XOSL = 0x78,
        /// <summary>
        /// APTI FAT16 CHS mode
        /// </summary>
        APTIFAT16CHS = 0x79,
        /// <summary>
        /// APTI FAT16 LBA mode
        /// </summary>
        APTIFAT16LBA = 0x7A,
        /// <summary>
        /// APTI FAT16B CHS mode
        /// </summary>
        APTIFAT16BCHS = 0x7B,
        /// <summary>
        /// APTI FAT32 LBA mode
        /// </summary>
        APTIFAT32LBA = 0x7C,
        /// <summary>
        /// APTI FAT32 CHS mode
        /// </summary>
        APTIFAT32CHS = 0x7D,
        /// <summary>
        /// PrimoCache Level 2
        /// </summary>
        PrimoCache2 = 0x7E,
        /// <summary>
        /// Reserved for individual or local use and temporary or experimental projects
        /// </summary>
        Experimental = 0x7F

        //TODO: Second half of the list
    }

    [Flags]
    public enum PartitionStatus : byte
    {
        /// <summary>
        /// Partition is not inactive
        /// </summary>
        Inactive = 0x00,
        Invalid_1 = 0x1, //1
        Invalid_2 = Invalid_1 << 1, //2
        Invalid_3 = Invalid_2 << 1, //4
        Invalid_4 = Invalid_3 << 1, //8
        Invalid_5 = Invalid_4 << 1, //16
        Invalid_6 = Invalid_5 << 1, //32
        Invalid_7 = Invalid_6 << 1, //64
        /// <summary>
        /// Partition is active
        /// </summary>
        /// <remarks>At most one partition on each disk is supposed to have this</remarks>
        Active = Invalid_6 << 0x01, //128
    }
}
