using System.Text;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

/// <summary>
/// Reads the header of a SID file (Commodore 64 music, PSID/RSID format).
/// Header layout (big-endian words): 0x00 magic, 0x04 version, 0x06 data offset, 0x08 load address,
/// 0x0A init, 0x0C play, 0x0E songs, 0x10 start song, 0x12 speed (32 bits), 0x16 title, 0x36 author,
/// 0x56 released, then (version 2+) 0x76 flags, 0x78 start page, 0x79 page length,
/// 0x7A second SID address, 0x7B third SID address (version 3/4).
/// </summary>
internal static class SidInfoReader
{
    public static bool IsSidFile(string filePath)
    {
        try
        {
            var b = new byte[4];
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fs.Read(b, 0, 4) < 4) return false;
            return Encoding.ASCII.GetString(b) is "PSID" or "RSID";
        }
        catch { return false; }
    }

    public static SidInfoModel Read(string filePath)
    {
        var raw = File.ReadAllBytes(filePath);
        if (raw.Length < 0x76) throw new InvalidDataException("The file is too short for a SID header.");

        int U16(int o) => (raw[o] << 8) | raw[o + 1];
        string Str(int o)
        {
            int len = 0;
            while (len < 32 && raw[o + len] != 0) len++;
            return Encoding.Latin1.GetString(raw, o, len).Trim();
        }
        static string Model(int bits) => bits switch { 1 => "MOS 6581", 2 => "MOS 8580", 3 => "6581 and 8580", _ => "Unknown" };

        var m = new SidInfoModel
        {
            Format      = Encoding.ASCII.GetString(raw, 0, 4),
            Version     = U16(0x04),
            DataOffset  = U16(0x06),
            LoadAddress = U16(0x08),
            InitAddress = U16(0x0A),
            PlayAddress = U16(0x0C),
            Songs       = U16(0x0E),
            StartSong   = U16(0x10),
            Title       = Str(0x16),
            Author      = Str(0x36),
            Released    = Str(0x56),
        };

        // One speed bit per song (bit 0 = song 1); 1 = CIA timer, 0 = vertical blank
        uint speed = ((uint)U16(0x12) << 16) | (uint)U16(0x14);
        int startIdx = Math.Clamp(m.StartSong - 1, 0, 31);
        m.StartSongUsesCia = ((speed >> startIdx) & 1) == 1;

        // Load address 0 means the real one is the first two bytes of the data (little-endian)
        int dataLen = Math.Max(0, raw.Length - m.DataOffset);
        if (m.LoadAddress == 0 && dataLen >= 2)
        {
            m.LoadAddress = raw[m.DataOffset] | (raw[m.DataOffset + 1] << 8);
            dataLen -= 2;
        }
        m.DataLength = dataLen;

        if (m.Version >= 2 && raw.Length >= 0x7C)
        {
            int flags = U16(0x76);
            m.Clock    = ((flags >> 2) & 3) switch { 1 => "PAL", 2 => "NTSC", 3 => "PAL and NTSC", _ => "Unknown" };
            m.SidModel = Model((flags >> 4) & 3);

            int second = raw[0x7A], third = raw[0x7B];
            m.SidCount = 1;
            if (m.Version >= 3 && second != 0)
            {
                m.SidCount++;
                m.SecondSidModel   = Model((flags >> 6) & 3);
                m.SecondSidAddress = 0xD000 + second * 16;
            }
            if (m.Version >= 4 && third != 0)
            {
                m.SidCount++;
                m.ThirdSidModel   = Model((flags >> 8) & 3);
                m.ThirdSidAddress = 0xD000 + third * 16;
            }
        }
        return m;
    }
}
