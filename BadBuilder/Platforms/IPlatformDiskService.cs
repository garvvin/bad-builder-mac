using BadBuilder.Models;

namespace BadBuilder.Platforms
{
    internal interface IPlatformDiskService
    {
        List<DiskInfo> GetDisks();
        bool ValidateDiskFormat(string mountPoint);
    }
}
