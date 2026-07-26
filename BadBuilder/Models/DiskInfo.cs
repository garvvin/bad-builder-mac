namespace BadBuilder.Models
{
    internal class DiskInfo
    {
        internal string MountPoint { get; init; }
        internal string Type { get; init; }
        internal string SizeFormatted { get; init; }
        internal long TotalSize { get; init; }

        internal DiskInfo(string mountPoint, string type, long totalSize)
        {
            MountPoint = mountPoint;
            Type = type;
            TotalSize = totalSize;
            SizeFormatted = FormatSize(totalSize);
        }

        private static string FormatSize(long bytes)
        {
            const double KB = 1024.0;
            const double MB = KB * 1024;
            const double GB = MB * 1024;
            const double TB = GB * 1024;

            if (bytes >= TB) return $"{bytes / TB:F2} TB";
            if (bytes >= GB) return $"{bytes / GB:F2} GB";
            if (bytes >= MB) return $"{bytes / MB:F2} MB";
            if (bytes >= KB) return $"{bytes / KB:F2} KB";
            return $"{bytes} bytes";
        }
    }
}
