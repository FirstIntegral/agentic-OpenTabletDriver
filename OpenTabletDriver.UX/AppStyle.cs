using Eto.Drawing;

namespace OpenTabletDriver.UX
{
    public static class AppStyle
    {
        public const int Space = 8;
        public const int SpaceLarge = 16;
        public const int NavWidth = 196;
        public const int DefaultWindowWidth = 1100;
        public const int DefaultWindowHeight = 780;
        public const int HeaderLogoSize = 28;

        public static Padding PagePadding => new(16, 12);
        public static Padding HeaderPadding => new(12, 10);
    }
}
