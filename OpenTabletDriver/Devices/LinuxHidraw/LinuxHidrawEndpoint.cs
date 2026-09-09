using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using HidSharp.Reports;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Hidraw;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.LinuxHidraw
{
    /// <summary>
    /// hidraw endpoint for non-USB HID (Bluetooth HOGP / uhid).
    /// HidSharpCore 1.3.0 only creates devices with a USB parent.
    /// </summary>
    public sealed class LinuxHidrawEndpoint : IDeviceEndpoint
    {
        internal LinuxHidrawEndpoint(
            string devicePath,
            string fileSystemName,
            int vendorId,
            int productId,
            uint bustype,
            string? productName,
            string? serialNumber,
            byte[] reportDescriptor,
            bool canOpen)
        {
            DevicePath = devicePath;
            FileSystemName = fileSystemName;
            VendorID = vendorId;
            ProductID = productId;
            Bustype = bustype;
            ProductName = productName ?? "Unknown Product Name";
            FriendlyName = ProductName;
            Manufacturer = "Unknown Manufacturer";
            SerialNumber = serialNumber ?? string.Empty;
            CanOpen = canOpen;

            var parser = new ReportDescriptor(reportDescriptor);
            InputReportLength = parser.MaxInputReportLength;
            OutputReportLength = parser.MaxOutputReportLength;
            FeatureReportLength = parser.MaxFeatureReportLength;
            ReportsUseID = parser.ReportsUseID;
            ReportDescriptor = reportDescriptor;

            var attributes = new Dictionary<string, string>();
            HidSharpBackend.Extensions.ExtractHidUsages(attributes, () => new ReportDescriptor(reportDescriptor));
            DeviceAttributes = attributes;
        }

        internal string FileSystemName { get; }
        internal uint Bustype { get; }
        internal bool ReportsUseID { get; }
        internal byte[] ReportDescriptor { get; }

        public int ProductID { get; }
        public int VendorID { get; }
        public int InputReportLength { get; }
        public int OutputReportLength { get; }
        public int FeatureReportLength { get; }
        public string Manufacturer { get; }
        public string ProductName { get; }
        public string FriendlyName { get; }
        public string SerialNumber { get; }
        public string DevicePath { get; }
        public bool CanOpen { get; }
        public IDictionary<string, string> DeviceAttributes { get; }

        public IDeviceEndpointStream Open()
        {
            if (!CanOpen)
                throw new UnauthorizedAccessException($"Not permitted to open HID class device at {FileSystemName}.");
            return new LinuxHidrawStream(this);
        }

        public string GetDeviceString(byte index) => string.Empty;

        internal static bool IsHidrawName(string name)
        {
            if (!name.StartsWith("hidraw", StringComparison.Ordinal) || name.Length < 7)
                return false;
            for (var i = 6; i < name.Length; i++)
            {
                if (name[i] < '0' || name[i] > '9')
                    return false;
            }
            return true;
        }

        internal static bool TryParseHidId(string? hidId, out uint bus, out int vendorId, out int productId)
        {
            bus = 0;
            vendorId = 0;
            productId = 0;
            if (string.IsNullOrEmpty(hidId))
                return false;
            var parts = hidId.Split(':');
            if (parts.Length < 3)
                return false;
            if (!uint.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bus))
                return false;
            if (!int.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out vendorId))
                return false;
            if (!int.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out productId))
                return false;
            vendorId &= 0xFFFF;
            productId &= 0xFFFF;
            return true;
        }

        internal static LinuxHidrawEndpoint? TryCreate(string hidrawName)
        {
            if (!IsHidrawName(hidrawName))
                return null;

            var sysClass = Path.Combine("/sys/class/hidraw", hidrawName);
            var fileSystemName = Path.Combine("/dev", hidrawName);
            if (!Directory.Exists(sysClass) || !File.Exists(fileSystemName))
                return null;

            // Filter USB via sysfs before any open. HidSharp already owns USB hidraw.
            var hidId = TryReadHidUevent(sysClass, "HID_ID");
            uint bus = 0;
            var vendorId = 0;
            var productId = 0;
            var parsedId = TryParseHidId(hidId, out bus, out vendorId, out productId);
            if (parsedId && bus == Hidraw.BUS_USB)
                return null;

            string devicePath;
            try
            {
                devicePath = Directory.ResolveLinkTarget(sysClass, true)?.FullName ?? sysClass;
            }
            catch
            {
                devicePath = sysClass;
            }

            var reportDescriptor = ReadSysfsReportDescriptor(sysClass, 0);
            if (reportDescriptor == null)
                return null;

            var canOpen = false;
            var fd = Hidraw.open(fileSystemName, Hidraw.O_RDONLY | Hidraw.O_CLOEXEC);
            if (fd >= 0)
            {
                try
                {
                    var info = new hidraw_devinfo();
                    if (Hidraw.ioctl(fd, Hidraw.HIDIOCGRAWINFO, ref info) == 0)
                    {
                        if (info.bustype == Hidraw.BUS_USB)
                            return null;
                        bus = info.bustype;
                        vendorId = info.vendor & 0xFFFF;
                        productId = info.product & 0xFFFF;
                        parsedId = true;
                    }
                    canOpen = true;
                }
                finally
                {
                    Hidraw.close(fd);
                }
            }
            else
            {
                var errno = Marshal.GetLastPInvokeError();
                if (errno != (int)ERRNO.EACCES && errno != (int)ERRNO.EPERM)
                    return null;
            }

            if (!parsedId || bus == Hidraw.BUS_USB)
                return null;

            var productName = TryReadHidUevent(sysClass, "HID_NAME");
            var serial = TryReadHidUevent(sysClass, "HID_UNIQ");

            Log.Debug("LinuxHidraw", $"{fileSystemName} bus={bus:X} {vendorId:X4}:{productId:X4} canOpen={canOpen} name='{productName}'");

            return new LinuxHidrawEndpoint(
                devicePath,
                fileSystemName,
                vendorId,
                productId,
                bus,
                productName,
                serial,
                reportDescriptor,
                canOpen);
        }

        private static byte[]? ReadSysfsReportDescriptor(string sysClass, int descSize)
        {
            try
            {
                var path = Path.Combine(sysClass, "device", "report_descriptor");
                using var fs = File.OpenRead(path);
                var buf = new byte[Hidraw.HID_MAX_DESCRIPTOR_SIZE];
                var n = fs.Read(buf, 0, buf.Length);
                if (n <= 0)
                    return null;
                if (descSize > 0 && descSize <= n)
                    n = descSize;
                var desc = new byte[n];
                Buffer.BlockCopy(buf, 0, desc, 0, n);
                return desc;
            }
            catch
            {
                return null;
            }
        }

        private static string? TryReadHidUevent(string hidrawSysClass, string key)
        {
            try
            {
                var uevent = Path.Combine(hidrawSysClass, "device", "uevent");
                if (!File.Exists(uevent))
                    return null;
                foreach (var line in File.ReadLines(uevent))
                {
                    if (line.StartsWith(key + "=", StringComparison.Ordinal))
                        return line[(key.Length + 1)..];
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }
    }
}
