using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    /// <summary>
    /// HOGP HID-keyboard pad on the G930L (report ID 1, 9 bytes).
    /// Firmware sends keyboard chords; each physical key is one aux button.
    /// Order is top-to-bottom as captured on this tablet.
    /// </summary>
    public struct GianoBluetoothAuxReport : IAuxReport
    {
        // (required modifier bits, HID keyboard usage)
        private static readonly (byte Mods, byte Key)[] PadKeys =
        [
            (0x05, 0x1D), // LCtrl+LAlt+Z
            (0x01, 0x56), // LCtrl+KeypadMinus
            (0x01, 0x57), // LCtrl+KeypadPlus
            (0x00, 0x2F), // Backslash
            (0x00, 0x30), // Non-US hash
            (0x00, 0x05), // B
        ];

        internal GianoBluetoothAuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[PadKeys.Length];
            if (report.Length < 9 || report[0] != 0x01)
                return;

            var mods = report[2];
            for (var i = 0; i < PadKeys.Length; i++)
            {
                var (need, key) = PadKeys[i];
                if ((mods & need) != need)
                    continue;
                for (var k = 3; k < 9; k++)
                {
                    if (report[k] == key)
                    {
                        AuxButtons[i] = true;
                        break;
                    }
                }
            }
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
