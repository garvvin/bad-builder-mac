using BadBuilder.Models;
using BadBuilder.Platforms;
#if WINDOWS
using BadBuilder.Formatter;
#endif
using System.Diagnostics;
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return FormatDiskMacOS(disk);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return FormatDiskWindows(disk);

            return "Formatting is not supported on this platform. Please format your drive manually.";
        }

        private static string FormatDiskMacOS(DiskInfo disk)
        {
            if (string.IsNullOrEmpty(disk.DeviceIdentifier))
                return "\u001b[38;2;255;114;0m[-]\u001b[0m Could not determine the device identifier for formatting. Please format manually.";

            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/sbin/diskutil",
                        Arguments = $"eraseDisk FAT32 BADUPDATE MBRFormat {disk.DeviceIdentifier}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();

                if (!process.WaitForExit(120000))
                {
                    process.Kill(entireProcessTree: true);
                    return "\u001b[38;2;255;114;0m[-]\u001b[0m Format timed out. Please try again or format manually.";
                }

                if (process.ExitCode == 0)
                    return string.Empty;

                string error = stderr.Trim();
                if (string.IsNullOrEmpty(error))
                    error = stdout.Trim();

                return $"\u001b[38;2;255;114;0m[-]\u001b[0m Format failed: {error}";
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return "\u001b[38;2;255;114;0m[-]\u001b[0m diskutil is not available on this system. Please format manually.";
            }
            catch (Exception ex)
            {
                return $"\u001b[38;2;255;114;0m[-]\u001b[0m Format error: {ex.Message}";
            }
        }

        private static string FormatDiskWindows(DiskInfo disk)
        {
#if WINDOWS
            return DiskFormatter.FormatVolume(disk.MountPoint[0], disk.TotalSize);
#else
            return ""; // never reached
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
