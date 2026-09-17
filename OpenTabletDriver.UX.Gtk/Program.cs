using System;

namespace OpenTabletDriver.UX.Gtk
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App.PlatformSetup = GtkAppTheme.Apply;
            App.Run(Eto.Platforms.Gtk, args);
        }
    }
}
