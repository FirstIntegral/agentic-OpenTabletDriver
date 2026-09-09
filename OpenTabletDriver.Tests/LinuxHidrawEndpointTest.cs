using System;
using OpenTabletDriver.Devices.LinuxHidraw;
using OpenTabletDriver.Native.Linux.Hidraw;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class LinuxHidrawEndpointTest
    {
        // Digitizer report map from G930L HOGP (PID 0x8251), 101 bytes.
        private static readonly byte[] DigitizerDescriptor = Convert.FromHexString(
            "050d0902a10185020920a101094209440945093c09430944150025017501950681020932750195018102810305010930150026ff7f7510950181020931150026ff7f751095018102050d093026ff1f751095018102093d093e1581257f750895028102c0c0");

        private static readonly byte[] KeyboardDescriptor = Convert.FromHexString(
            "05010906a1018501050775089501810119e029e7150025017501950881020507190029ff26ff00750895068100c0");

        [Fact]
        public void DigitizerDescriptor_IsTenByteNumberedReport()
        {
            var endpoint = new LinuxHidrawEndpoint(
                "/sys/devices/virtual/misc/uhid/0005:256C:8251.000D/hidraw/hidraw12",
                "/dev/hidraw12",
                0x256C,
                0x8251,
                Hidraw.BUS_BLUETOOTH,
                "Inspiroy Giano-864",
                "20:23:08:01:90:00",
                DigitizerDescriptor,
                canOpen: true);

            Assert.Equal(10, endpoint.InputReportLength);
            Assert.True(endpoint.ReportsUseID);
            Assert.Equal(0, endpoint.OutputReportLength);
            Assert.False(endpoint.DeviceAttributes.ContainsKey("USB_INTERFACE_NUMBER"));
            Assert.True(endpoint.DeviceAttributes.ContainsKey("HID_REPORTS"));
        }

        [Fact]
        public void KeyboardDescriptor_IsNotTenBytes()
        {
            var endpoint = new LinuxHidrawEndpoint(
                "/sys/devices/virtual/misc/uhid/0005:256C:8251.000C/hidraw/hidraw11",
                "/dev/hidraw11",
                0x256C,
                0x8251,
                Hidraw.BUS_BLUETOOTH,
                "Inspiroy Giano-864",
                "20:23:08:01:90:00",
                KeyboardDescriptor,
                canOpen: true);

            Assert.NotEqual(10, endpoint.InputReportLength);
            Assert.Equal(9, endpoint.InputReportLength);
        }

        [Theory]
        [InlineData("hidraw0", true)]
        [InlineData("hidraw12", true)]
        [InlineData("hidraw", false)]
        [InlineData("../hidraw0", false)]
        [InlineData("hidraw0/../passwd", false)]
        public void IsHidrawName_RejectsTraversal(string name, bool expected)
        {
            Assert.Equal(expected, LinuxHidrawEndpoint.IsHidrawName(name));
        }

        [Fact]
        public void TryParseHidId_BluetoothG930L()
        {
            Assert.True(LinuxHidrawEndpoint.TryParseHidId("0005:0000256C:00008251", out var bus, out var vid, out var pid));
            Assert.Equal(Hidraw.BUS_BLUETOOTH, bus);
            Assert.Equal(0x256C, vid);
            Assert.Equal(0x8251, pid);
        }

        [Fact]
        public void TryParseHidId_UsbIsBus3()
        {
            Assert.True(LinuxHidrawEndpoint.TryParseHidId("0003:0000256C:00000061", out var bus, out _, out _));
            Assert.Equal(Hidraw.BUS_USB, bus);
        }
    }
}
