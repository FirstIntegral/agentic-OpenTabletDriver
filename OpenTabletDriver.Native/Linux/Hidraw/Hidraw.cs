using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Hidraw
{
    public static class Hidraw
    {
        private const string libc = "libc.so.6";

        public const int O_RDONLY = 0;
        public const int O_RDWR = 2;
        public const int O_CLOEXEC = 0x80000;
        public const int HID_MAX_DESCRIPTOR_SIZE = 4096;
        public const int HID_MAX_BUFFER_SIZE = 4096;
        public const uint BUS_USB = 0x03;
        public const uint BUS_BLUETOOTH = 0x05;

        // linux/hidraw.h
        public static readonly UIntPtr HIDIOCGRDESCSIZE = Ior(0x01, 4);
        public static readonly UIntPtr HIDIOCGRDESC = Ioc(3, 0x02, HID_MAX_DESCRIPTOR_SIZE);
        public static readonly UIntPtr HIDIOCGRAWINFO = Ior(0x03, 8);

        public static UIntPtr HIDIOCGFEATURE(int length) => Ioc(3, 0x07, ClampIocSize(length));
        public static UIntPtr HIDIOCSFEATURE(int length) => Ioc(3, 0x06, ClampIocSize(length));

        public static bool IsValidFeatureLength(int length) =>
            length >= 1 && length <= HID_MAX_BUFFER_SIZE;

        [DllImport(libc, SetLastError = true)]
        public static extern int open(string pathname, int flags);

        [DllImport(libc, SetLastError = true)]
        public static extern int close(int fd);

        [DllImport(libc, SetLastError = true)]
        public static extern IntPtr read(int fd, byte[] buf, UIntPtr count);

        [DllImport(libc, SetLastError = true)]
        public static extern IntPtr write(int fd, byte[] buf, UIntPtr count);

        [DllImport(libc, SetLastError = true)]
        public static extern int ioctl(int fd, UIntPtr request, ref int arg);

        [DllImport(libc, SetLastError = true)]
        public static extern int ioctl(int fd, UIntPtr request, ref hidraw_devinfo arg);

        [DllImport(libc, SetLastError = true)]
        public static extern int ioctl(int fd, UIntPtr request, ref hidraw_report_descriptor arg);

        [DllImport(libc, SetLastError = true)]
        public static extern int ioctl(int fd, UIntPtr request, IntPtr arg);

        private static int ClampIocSize(int size) => Math.Clamp(size, 1, 0x3FFF);

        private static UIntPtr Ior(int nr, int size) => Ioc(2, nr, size);

        private static UIntPtr Ioc(int dir, int nr, int size)
        {
            return (UIntPtr)((ulong)dir << 30 | (ulong)ClampIocSize(size) << 16 | (ulong)'H' << 8 | (uint)nr);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct hidraw_devinfo
    {
        public uint bustype;
        public short vendor;
        public short product;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct hidraw_report_descriptor
    {
        public uint size;
        public fixed byte value[Hidraw.HID_MAX_DESCRIPTOR_SIZE];
    }
}
