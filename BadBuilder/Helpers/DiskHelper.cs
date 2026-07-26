using BadBuilder.Models;
using BadBuilder.Platforms;
#if WINDOWS
using BadBuilder.Formatter;
#endif
using System.Runtime.InteropServices;

namespace BadBuilder.Helpers
{
    internal static class DiskHelper
    {
        private static readonly IPlatformDiskService _diskService = CreateDiskService();

        internal static List<DiskInfo> GetDisks() => _diskService.GetDisks();

        internal static bool ValidateDiskFormat(string mountPoint) => _diskService.ValidateDiskFormat(mountPoint);

        internal static string FormatDisk(DiskInfo disk)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "\u001b[38;2;255;114;0m[-]\u001b[0m Formatting is currently only supported on Windows. Please format your drive manually and try again.";

#if WINDOWS
            return DiskFormatter.FormatVolume(disk.MountPoint[0], disk.TotalSize);
#else
            return ""; // never reached — already returned above
#endif
        }

        private static IPlatformDiskService CreateDiskService()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new MacDiskService();
            return new WindowsDiskService();
        }
    }
}
