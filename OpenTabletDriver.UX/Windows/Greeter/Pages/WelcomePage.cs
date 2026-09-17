using Eto.Drawing;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Intro")]
    public class WelcomePage : StylizedPage
    {
        public WelcomePage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(256, 256)),
                new StylizedText($"{App.ProductName} Guide", SystemFonts.Bold(12), new Padding(0, 0, 0, 10)),
                $"Welcome to {App.ProductName}!",
                $"{App.ProductName} is a personal fork of OpenTabletDriver, an open source, cross platform, user mode tablet driver.",
                new PaddingSpacerItem(),
            };
        }
    }
}
