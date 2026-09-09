using OpenTabletDriver.Configurations.Parsers.Huion;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class GianoBluetoothReportParserTest
    {
        private static readonly GianoBluetoothAuxReportParser Aux = new();

        [Theory]
        [InlineData(new byte[] { 0x01, 0xE0, 0x05, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[] { 0x01, 0xE0, 0x01, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00 }, 1)]
        [InlineData(new byte[] { 0x01, 0xE0, 0x01, 0x57, 0x00, 0x00, 0x00, 0x00, 0x00 }, 2)]
        [InlineData(new byte[] { 0x01, 0xE0, 0x00, 0x2F, 0x00, 0x00, 0x00, 0x00, 0x00 }, 3)]
        [InlineData(new byte[] { 0x01, 0xE0, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00 }, 4)]
        [InlineData(new byte[] { 0x01, 0xE0, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00 }, 5)]
        public void Aux_MapsCapturedChords_ToButtons(byte[] report, int expectedIndex)
        {
            var parsed = Aux.Parse(report);
            var aux = Assert.IsType<GianoBluetoothAuxReport>(parsed);
            Assert.Equal(6, aux.AuxButtons.Length);
            for (var i = 0; i < 6; i++)
                Assert.Equal(i == expectedIndex, aux.AuxButtons[i]);
        }

        [Fact]
        public void Aux_AllUp_ClearsButtons()
        {
            var aux = Assert.IsType<GianoBluetoothAuxReport>(
                Aux.Parse([0x01, 0xE0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]));
            Assert.All(aux.AuxButtons, b => Assert.False(b));
        }

        [Fact]
        public void Digitizer_InRangeOrigin()
        {
            var parser = new GianoBluetoothReportParser();
            var report = parser.Parse([0x02, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
            var pen = Assert.IsType<GianoBluetoothReport>(report);
            Assert.Equal(0, pen.Position.X);
            Assert.Equal(0, pen.Position.Y);
            Assert.Equal(0u, pen.Pressure);
        }

        [Fact]
        public void Digitizer_OutOfRange_WhenInRangeBitClear()
        {
            var parser = new GianoBluetoothReportParser();
            Assert.IsType<OutOfRangeReport>(parser.Parse([0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]));
        }
    }
}
