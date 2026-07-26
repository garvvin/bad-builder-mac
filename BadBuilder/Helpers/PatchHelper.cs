using Spectre.Console;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace BadBuilder.Helpers
{
    internal static class PatchHelper
    {
        private const uint XEX2Magic = 0x58455832;
        private const int SecurityInfoMinOffset = 0x18;

        internal static async Task PatchXexAsync(string xexPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(xexPath);

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

                if (secInfoOff < SecurityInfoMinOffset || secInfoOff > int.MaxValue || (long)secInfoOff + 0x184 > xexData.Length)
                {
                    AnsiConsole.MarkupLine("[#FF7200][!][/] File [italic]{0}[/] has invalid SecurityInfo offset.", fileName);
                    return;
                }

                int sec = (int)secInfoOff;

                BinaryPrimitives.WriteUInt32BigEndian(xexData.AsSpan(sec + 0x178, 4), 0xFFFFFFFF);
                BinaryPrimitives.WriteUInt32BigEndian(xexData.AsSpan(sec + 0x17C, 4), 0xFFFFFFFF);

                uint imageFlags = BinaryPrimitives.ReadUInt32BigEndian(xexData.AsSpan(sec + 0x10C, 4));
                BinaryPrimitives.WriteUInt32BigEndian(xexData.AsSpan(sec + 0x10C, 4), imageFlags & ~0x08u);

                Array.Clear(xexData, sec + 0x08, 256);

                if (!RepairHeaderHash(xexData, sec))
                {
                    AnsiConsole.MarkupLine("[#FF7200][!][/] Failed to repair header hash for [italic]{0}[/]. File not modified.", fileName);
                    return;
                }

                await File.WriteAllBytesAsync(xexPath, xexData);

                AnsiConsole.MarkupLine("\n[#76B900][+][/] Patched [italic]{0}[/] (all regions, all media, retail).", fileName);
            }
            catch (Exception ex)
            {
                string status = "[-]";
                AnsiConsole.MarkupLineInterpolated($"\n[#FF7200]{status}[/] The program {fileName} was unable to be patched: {ex.Message}");
            }
        }

        private static bool RepairHeaderHash(byte[] data, int sec)
        {
            uint dataOffset = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(0x08, 4));

            int part1Start = sec + 0x29C;
            int part1Len = (int)dataOffset - part1Start;

            if (part1Len < 0 || part1Start + part1Len > data.Length)
                return false;

            try
            {
                using SHA1 sha1 = SHA1.Create();
                sha1.TransformBlock(data, part1Start, part1Len, null, 0);
                sha1.TransformFinalBlock(data, 0, sec + 0x128);
                byte[] hash = sha1.Hash!;

                Array.Copy(hash, 0, data, sec + 0x164, 20);
                return true;
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
        }
    }
}
