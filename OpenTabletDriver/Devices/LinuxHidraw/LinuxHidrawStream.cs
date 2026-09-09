using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Linux;
using OpenTabletDriver.Native.Linux.Hidraw;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.LinuxHidraw
{
    public sealed class LinuxHidrawStream : IDeviceEndpointStream
    {
        private readonly LinuxHidrawEndpoint _endpoint;
        private int _fd = -1;

        private readonly object _fdLock = new();

        internal LinuxHidrawStream(LinuxHidrawEndpoint endpoint)
        {
            _endpoint = endpoint;
            _fd = Hidraw.open(endpoint.FileSystemName, Hidraw.O_RDWR | Hidraw.O_CLOEXEC);
            if (_fd < 0)
            {
                var errno = Marshal.GetLastPInvokeError();
                if (errno == (int)ERRNO.EACCES)
                    throw new UnauthorizedAccessException($"Not permitted to open HID class device at {endpoint.FileSystemName}.");
                throw new IOException($"Unable to open HID class device at {endpoint.FileSystemName} ({(ERRNO)errno}).");
            }
        }

        public byte[] Read()
        {
            var length = Math.Max(_endpoint.InputReportLength, 1);
            var useId = _endpoint.ReportsUseID;
            var readLen = useId ? length : Math.Max(length - 1, 1);
            var payload = new byte[readLen];
            while (true)
            {
                long n;
                lock (_fdLock)
                {
                    EnsureOpen();
                    n = Hidraw.read(_fd, payload, (UIntPtr)payload.Length).ToInt64();
                }
                if (n > 0)
                {
                    if (useId)
                    {
                        if (n != payload.Length)
                            Array.Resize(ref payload, (int)n);
                        return payload;
                    }

                    var buffer = new byte[(int)n + 1];
                    buffer[0] = 0;
                    Buffer.BlockCopy(payload, 0, buffer, 1, (int)n);
                    return buffer;
                }

                var errno = Marshal.GetLastPInvokeError();
                if (errno == (int)ERRNO.EINTR)
                    continue;
                throw new IOException("I/O disconnected.");
            }
        }

        public void Write(byte[] buffer)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var slice = buffer;
                if (offset != 0)
                {
                    slice = new byte[buffer.Length - offset];
                    Buffer.BlockCopy(buffer, offset, slice, 0, slice.Length);
                }

                long n;
                lock (_fdLock)
                {
                    EnsureOpen();
                    n = Hidraw.write(_fd, slice, (UIntPtr)slice.Length).ToInt64();
                }
                if (n < 0)
                {
                    var errno = Marshal.GetLastPInvokeError();
                    if (errno == (int)ERRNO.EINTR)
                        continue;
                    throw new IOException($"Write failed ({(ERRNO)errno}).");
                }
                offset += (int)n;
            }
        }

        public unsafe void GetFeature(byte[] buffer)
        {
            if (!Hidraw.IsValidFeatureLength(buffer.Length) || buffer.Length < 2)
                throw new ArgumentException("Feature buffer length out of range.", nameof(buffer));

            var reportId = buffer[0];
            buffer[1] = reportId;
            lock (_fdLock)
            {
                EnsureOpen();
                fixed (byte* ptr = buffer)
                {
                    var bytes = Hidraw.ioctl(_fd, Hidraw.HIDIOCGFEATURE(buffer.Length - 1), (IntPtr)(ptr + 1));
                    if (bytes < 0)
                        throw new IOException("GetFeature failed.");
                    var clearFrom = 1 + bytes;
                    if (clearFrom < buffer.Length)
                        Array.Clear(buffer, clearFrom, buffer.Length - clearFrom);
                }
            }
        }

        public unsafe void SetFeature(byte[] buffer)
        {
            if (!Hidraw.IsValidFeatureLength(buffer.Length))
                throw new ArgumentException("Feature buffer length out of range.", nameof(buffer));

            lock (_fdLock)
            {
                EnsureOpen();
                fixed (byte* ptr = buffer)
                {
                    if (Hidraw.ioctl(_fd, Hidraw.HIDIOCSFEATURE(buffer.Length), (IntPtr)ptr) < 0)
                        throw new IOException("SetFeature failed.");
                }
            }
        }

        public void Dispose()
        {
            lock (_fdLock)
            {
                var fd = _fd;
                _fd = -1;
                if (fd >= 0)
                    Hidraw.close(fd);
            }
        }

        private void EnsureOpen()
        {
            if (_fd < 0)
                throw new ObjectDisposedException(nameof(LinuxHidrawStream));
        }
    }
}
