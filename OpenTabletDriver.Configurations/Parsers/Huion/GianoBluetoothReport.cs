using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    /// <summary>
    /// HID-over-GATT digitizer report used by the G930L over Bluetooth
    /// (PID 0x8251, report ID 2, 10 bytes).
    /// </summary>
    public struct GianoBluetoothReport : ITabletReport, ITiltReport
    {
        internal GianoBluetoothReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);
            Tilt = new Vector2
            {
                X = (sbyte)report[8],
                Y = (sbyte)report[9]
            };

            // byte1: tip, barrel, eraser, invert, tablet pick, barrel, in-range, pad
            PenButtons =
            [
                report[1].IsBitSet(1),
                report[1].IsBitSet(2)
            ];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
