using BadBuilder.Models;
#if WINDOWS
using BadBuilder.Formatter;
#endif
using System.Runtime.InteropServices;
using Spectre.Console;

namespace BadBuilder.Helpers
{
    internal static class DiskHelper
    {
        internal static List<DiskInfo> GetDisks()
        {
            var disks = new List<DiskInfo>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (drive.Name == "/" || drive.Name.StartsWith("/System/")))
                        continue;
                    string driveLetter = drive.Name;
                    string volumeLabel = drive.VolumeLabel;

                    string type;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        type = driveLetter == "/" ? "System"
                             : driveLetter.StartsWith("/Volumes/") ? "Removable"
                             : drive.DriveType.ToString();
                    else
                        type = drive.DriveType.ToString();
                    long totalSize = drive.TotalSize;
                    long availableFreeSpace = drive.AvailableFreeSpace;
                    int diskNumber = 2;

                    disks.Add(new DiskInfo(driveLetter, type, totalSize, volumeLabel, availableFreeSpace, diskNumber));
                }
            }

            return disks;
        }

        internal static string FormatDisk(DiskInfo disk)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "\u001b[38;2;255;114;0m[-]\u001b[0m Formatting is currently only supported on Windows. Please format your drive manually and try again.";

#if WINDOWS
            return DiskFormatter.FormatVolume(disk.DriveLetter[0], disk.TotalSize);
#else
            return ""; // never reached — already returned above
#endif
        }
    }
}