using Spectre.Console;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace BadBuilder.Helpers
{
    internal static class PatchHelper
    {
        private const uint XEX2Magic = 0x58455832;

        internal static async Task PatchXexAsync(string xexPath, string xexToolPath)
        {
            string fileName = Path.GetFileName(xexPath);

            try
            {
                byte[] xexData = await File.ReadAllBytesAsync(xexPath);

                if (xexData.Length < 24)
                {
                    AnsiConsole.MarkupLine("[#FF7200][!][/] File [italic]{0}[/] is too small to be a valid XEX.", fileName);
                    return;
                }

                uint magic = BinaryPrimitives.ReadUInt32BigEndian(xexData.AsSpan(0, 4));
                if (magic != XEX2Magic)
                {
                    AnsiConsole.MarkupLine("[#FF7200][!][/] File [italic]{0}[/] is not a valid XEX file.", fileName);
                    return;
                }

                uint secInfoOff = BinaryPrimitives.ReadUInt32BigEndian(xexData.AsSpan(0x10, 4));
                int sec = (int)secInfoOff;

                if (sec + 0x184 > xexData.Length)
                {
                    AnsiConsole.MarkupLine("[#FF7200][!][/] File [italic]{0}[/] has invalid SecurityInfo offset.", fileName);
                    return;
                }

                BinaryPrimitives.WriteUInt32BigEndian(xexData.AsSpan(sec + 0x178, 4), 0xFFFFFFFF);
                BinaryPrimitives.WriteUInt32BigEndian(xexData.AsSpan(sec + 0x17C, 4), 0xFFFFFFFF);

                uint imageFlags = BinaryPrimitives.ReadUInt32BigEndian(xexData.AsSpan(sec + 0x10C, 4));
                BinaryPrimitives.WriteUInt32BigEndian(xexData.AsSpan(sec + 0x10C, 4), imageFlags & ~0x08u);

                for (int i = 0; i < 256; i++)
                    xexData[sec + 0x08 + i] = 0;

                RepairHeaderHash(xexData, sec);

                await File.WriteAllBytesAsync(xexPath, xexData);

                string status = "[+]";
                AnsiConsole.MarkupLine("\n[#76B900]{0}[/] Patched [italic]{1}[/] (all regions, all media, retail).", status, fileName);
            }
            catch (Exception ex)
            {
                string status = "[-]";
                AnsiConsole.MarkupLineInterpolated($"\n[#FF7200]{status}[/] The program {Path.GetFileNameWithoutExtension(xexPath)} was unable to be patched: {ex.Message}");
            }
        }

        private static void RepairHeaderHash(byte[] data, int sec)
        {
            uint dataOffset = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(0x08, 4));

            int part1Start = sec + 0x29C;
            int part1Len = (int)dataOffset - part1Start;

            if (part1Len < 0 || part1Start + part1Len > data.Length)
                return;

            using SHA1 sha1 = SHA1.Create();
            sha1.TransformBlock(data, part1Start, part1Len, null, 0);
            sha1.TransformFinalBlock(data, 0, sec + 0x128);
            byte[] hash = sha1.Hash!;

            Array.Copy(hash, 0, data, sec + 0x164, 20);
        }
    }
}
