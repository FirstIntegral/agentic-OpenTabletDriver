namespace OpenTabletDriver.Desktop
{
    /// <summary>
    /// Display name for this fork. Runtime identity (namespaces, assemblies,
    /// RPC pipe, XDG config dir, systemd unit, udev, evdev names, otd* bins)
    /// stays OpenTabletDriver — see docs/DECISIONS.md.
    /// </summary>
    public static class ProductInfo
    {
        public const string Name = "agentic-OpenTabletDriver";
        public const string DaemonPipeName = "OpenTabletDriver.Daemon";
        public const string UxAppName = "OpenTabletDriver.UX";
    }
}
