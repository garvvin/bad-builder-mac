using BadBuilder.Models;

namespace BadBuilder.Platforms
{
    internal sealed class WindowsDiskService : IPlatformDiskService
    {
        public List<DiskInfo> GetDisks()
        {
            var disks = new List<DiskInfo>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady)
                    continue;

                disks.Add(new DiskInfo(drive.Name, drive.DriveType.ToString(), drive.TotalSize));
            }

            return disks;
        }

        public bool ValidateDiskFormat(string mountPoint)
        {
            try
            {
                var driveInfo = new DriveInfo(mountPoint);
                return driveInfo.DriveFormat.Equals("FAT32", StringComparison.OrdinalIgnoreCase)
                    && driveInfo.VolumeLabel.Equals("BADUPDATE", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
