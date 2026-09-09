using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.LinuxHidraw
{
    /// <summary>
    /// Enumerates non-USB hidraw devices (Bluetooth HOGP via uhid).
    /// Complements <see cref="HidSharpBackend.HidSharpDeviceRootHub"/>, which skips hidraw
    /// nodes that have no USB parent.
    /// </summary>
    [DeviceHub, SupportedPlatform(PluginPlatform.Linux)]
    public sealed class LinuxHidrawRootHub : IDeviceHub, IDisposable
    {
        private const int DebounceMs = 100;
        private readonly object _sync = new();
        private readonly FileSystemWatcher? _watcher;
        private readonly Timer _debounce;
        private List<IDeviceEndpoint> _devices;

        public LinuxHidrawRootHub()
        {
            _devices = Enumerate();
            _debounce = new Timer(_ => Refresh(), null, Timeout.Infinite, Timeout.Infinite);
            try
            {
                _watcher = new FileSystemWatcher("/dev", "hidraw*")
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.Attributes,
                    EnableRaisingEvents = true
                };
                _watcher.Created += OnHidrawChanged;
                _watcher.Deleted += OnHidrawChanged;
                _watcher.Changed += OnHidrawChanged;
            }
            catch
            {
                _watcher = null;
            }
        }

        public event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            lock (_sync)
                return _devices;
        }

        private void OnHidrawChanged(object sender, FileSystemEventArgs e)
        {
            _debounce.Change(DebounceMs, Timeout.Infinite);
        }

        private void Refresh()
        {
            List<IDeviceEndpoint> previous;
            List<IDeviceEndpoint> current;
            lock (_sync)
            {
                previous = _devices;
                current = Enumerate();
                var changes = new DevicesChangedEventArgs(previous, current);
                if (!changes.Changes.Any())
                    return;
                _devices = current;
            }
            DevicesChanged?.Invoke(this, new DevicesChangedEventArgs(previous, current));
        }

        private static List<IDeviceEndpoint> Enumerate()
        {
            var list = new List<IDeviceEndpoint>();
            string[] nodes;
            try
            {
                nodes = Directory.GetFileSystemEntries("/dev", "hidraw*");
            }
            catch
            {
                return list;
            }

            foreach (var node in nodes)
            {
                var name = Path.GetFileName(node);
                if (string.IsNullOrEmpty(name))
                    continue;
                try
                {
                    var endpoint = LinuxHidrawEndpoint.TryCreate(name);
                    if (endpoint != null)
                    {
                        Log.Debug("LinuxHidraw", $"hub add {endpoint.DevicePath} {endpoint.VendorID:X4}:{endpoint.ProductID:X4} in={endpoint.InputReportLength} canOpen={endpoint.CanOpen}");
                        list.Add(endpoint);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug("LinuxHidraw", $"skip {name}: {ex.Message}");
                }
            }

            return list;
        }

        public void Dispose()
        {
            _debounce.Dispose();
            _watcher?.Dispose();
        }
    }
}
