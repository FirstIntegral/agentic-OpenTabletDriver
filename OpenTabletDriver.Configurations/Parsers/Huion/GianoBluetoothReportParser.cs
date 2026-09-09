using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class GianoBluetoothReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            if (data.Length < 10 || data[0] != 0x02)
                return new OutOfRangeReport(data);

            // HID In Range is bit 6 of the button byte.
            if (!data[1].IsBitSet(6))
                return new OutOfRangeReport(data);

            return new GianoBluetoothReport(data);
        }
    }
}
