using BadBuilder.Models;
using System.Runtime.InteropServices;

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

                    disks.Add(new DiskInfo(path, type, drive.TotalSize));
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
    }
}
