using BadBuilder.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace BadBuilder.Platforms
{
    internal sealed class MacDiskService : IPlatformDiskService
    {
        private const uint MNT_REMOVABLE = 0x00000200;

        [StructLayout(LayoutKind.Sequential)]
        private struct StatFS
        {
            public uint f_bsize;
            public int f_iosize;
            public ulong f_blocks;
            public ulong f_bfree;
            public ulong f_bavail;
            public ulong f_files;
            public ulong f_ffree;
            public int f_fsid0;
            public int f_fsid1;
            public uint f_owner;
            public ushort f_type;
            public uint f_flags;
        }

        private static readonly int F_FLAGS_OFFSET = (int)Marshal.OffsetOf<StatFS>("f_flags");

        [DllImport("libSystem.dylib")]
        private static extern int statfs([MarshalAs(UnmanagedType.LPStr)] string path, IntPtr buf);

        public List<DiskInfo> GetDisks()
        {
            var disks = new List<DiskInfo>();
            var deviceMap = GetDeviceMap();
            IntPtr buf = Marshal.AllocHGlobal(4096);

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (!drive.IsReady)
                        continue;

                    string path = drive.Name;

                    if (path == "/" || path.StartsWith("/System/"))
                        continue;

                    string type = GetDiskType(path, buf);

                    string? deviceId = null;
                    deviceMap.TryGetValue(path, out deviceId);

                    disks.Add(new DiskInfo(path, type, drive.TotalSize) { DeviceIdentifier = deviceId });
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }

            return disks;
        }

        public bool ValidateDiskFormat(string mountPoint)
        {
            try
            {
                var driveInfo = new DriveInfo(mountPoint);
                return driveInfo.DriveFormat.Equals("msdos", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string GetDiskType(string path, IntPtr buf)
        {
            if (statfs(path, buf) == 0)
            {
                uint flags = (uint)Marshal.ReadInt32(buf, F_FLAGS_OFFSET);
                if ((flags & MNT_REMOVABLE) != 0)
                    return "Removable";
            }
            return "Fixed";
        }

        private static Dictionary<string, string> GetDeviceMap()
        {
            var map = new Dictionary<string, string>();

            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/sbin/diskutil",
                        Arguments = "list -plist external",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string xml = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);

                if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(xml))
                    return map;

                var doc = XDocument.Parse(xml);
                var rootDict = doc.Root?.Element("dict");
                if (rootDict == null) return map;

                XElement? allDisksEl = null;
                var elements = rootDict.Elements().ToList();
                for (int i = 0; i < elements.Count - 1; i++)
                {
                    if (elements[i].Name == "key" && elements[i].Value == "AllDisksAndPartitions")
                    {
                        allDisksEl = elements[i + 1] as XElement;
                        break;
                    }
                }

                if (allDisksEl == null || allDisksEl.Name != "array")
                    return map;

                foreach (var diskDict in allDisksEl.Elements("dict"))
                {
                    string? wholeDiskId = null;
                    var diskEls = diskDict.Elements().ToList();

                    for (int i = 0; i < diskEls.Count - 1; i++)
                    {
                        if (diskEls[i].Name == "key" && diskEls[i].Value == "DeviceIdentifier")
                        {
                            wholeDiskId = diskEls[i + 1].Value;
                        }

                        if (diskEls[i].Name == "key" && diskEls[i].Value == "Partitions")
                        {
                            var partitionsEl = diskEls[i + 1];
                            if (partitionsEl.Name != "array") continue;

                            foreach (var partDict in partitionsEl.Elements("dict"))
                            {
                                var partEls = partDict.Elements().ToList();
                                string? mountPoint = null;

                                for (int j = 0; j < partEls.Count - 1; j++)
                                {
                                    if (partEls[j].Name == "key" && partEls[j].Value == "MountPoint")
                                    {
                                        mountPoint = partEls[j + 1].Value;
                                        break;
                                    }
                                }

                                if (mountPoint != null && wholeDiskId != null)
                                    map[mountPoint] = wholeDiskId;
                            }
                        }
                    }
                }
            }
            catch
            {
                // diskutil unavailable — map stays empty, formatting won't be available
            }

            return map;
        }
    }
}
