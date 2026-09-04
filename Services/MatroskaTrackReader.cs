using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

/// <summary>Reads embedded track metadata from Matroska (MKV/WebM) files via lightweight EBML parsing.</summary>
internal static class MatroskaTrackReader
{
    // EBML element IDs
    private const uint IdEbmlHeader  = 0x1A45DFA3;
    private const uint IdSegment     = 0x18538067;
    private const uint IdTracks      = 0x1654AE6B;
    private const uint IdTrackEntry  = 0xAE;
    private const uint IdTrackNumber = 0xD7;
    private const uint IdTrackType   = 0x83;
    private const uint IdTrackName   = 0x536E;
    private const uint IdLanguage    = 0x22B59C;
    private const uint IdLangBCP47   = 0x22B59D;
    private const uint IdCodecId     = 0x86;
    private const uint IdCodecName   = 0x258688;
    private const uint IdFlagDefault = 0x88;
    private const uint IdFlagForced  = 0x55AA;
    private const uint IdAudio       = 0xE1;
    private const uint IdVideo       = 0xE0;
    private const uint IdSampleFreq  = 0xB5;
    private const uint IdChannels    = 0x9F;
    private const uint IdBitDepth    = 0x6264;
    private const uint IdPixWidth    = 0xB0;
    private const uint IdPixHeight   = 0xBA;

    private const long Unknown = long.MaxValue;
    private const int  SearchBytes = 16 * 1024 * 1024; // scan first 16 MB for Tracks element

    public static List<MediaTrackInfo> ReadTracks(string filePath)
    {
        var tracks = new List<MediaTrackInfo>();
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);

            if (ReadId(br) != IdEbmlHeader) return tracks;
            SkipElement(br);

            if (ReadId(br) != IdSegment) return tracks;
            ReadSize(br); // may be unknown — ignore, we search by byte limit

            long limit = Math.Min(fs.Length, fs.Position + SearchBytes);
            while (fs.Position < limit - 4)
            {
                uint  id      = ReadId(br);
                long  size    = ReadSize(br);
                long  dataPos = fs.Position;
                if (size == Unknown || size < 0) break;

                if (id == IdTracks)
                {
                    ParseTracks(br, dataPos + size, tracks);
                    break;
                }
                fs.Seek(dataPos + size, SeekOrigin.Begin);
            }
        }
        catch { }
        return tracks;
    }

    private static void ParseTracks(BinaryReader br, long end, List<MediaTrackInfo> tracks)
    {
        var fs = br.BaseStream;
        while (fs.Position < end - 2)
        {
            uint id   = ReadId(br);
            long size = ReadSize(br);
            long dp   = fs.Position;
            if (size <= 0 || size == Unknown) break;

            if (id == IdTrackEntry)
                tracks.Add(ParseEntry(br, dp + size));

            fs.Seek(dp + size, SeekOrigin.Begin);
        }
    }

    private static MediaTrackInfo ParseEntry(BinaryReader br, long end)
    {
        var t  = new MediaTrackInfo();
        var fs = br.BaseStream;
        while (fs.Position < end - 2)
        {
            uint id   = ReadId(br);
            long size = ReadSize(br);
            long dp   = fs.Position;
            if (size < 0 || size == Unknown) break;

            switch (id)
            {
                case IdTrackNumber: t.Number    = (int)ReadUInt(br, size); break;
                case IdTrackType:   t.Type      = TypeName(ReadUInt(br, size)); break;
                case IdTrackName:   t.Name      = ReadUtf8(br, size); break;
                case IdLanguage:    if (string.IsNullOrEmpty(t.Language)) t.Language = ReadAscii(br, size); break;
                case IdLangBCP47:   t.Language  = ReadAscii(br, size); break;
                case IdCodecId:     t.CodecId   = ReadAscii(br, size); break;
                case IdCodecName:   t.CodecName = ReadUtf8(br, size); break;
                case IdFlagDefault: t.IsDefault = ReadUInt(br, size) != 0; break;
                case IdFlagForced:  t.IsForced  = ReadUInt(br, size) != 0; break;
                case IdAudio:       ParseAudio(br, dp + size, t); break;
                case IdVideo:       ParseVideo(br, dp + size, t); break;
            }
            fs.Seek(dp + size, SeekOrigin.Begin);
        }
        return t;
    }

    private static void ParseAudio(BinaryReader br, long end, MediaTrackInfo t)
    {
        var fs = br.BaseStream;
        while (fs.Position < end - 2)
        {
            uint id   = ReadId(br);
            long size = ReadSize(br);
            long dp   = fs.Position;
            if (size < 0 || size == Unknown) break;
            switch (id)
            {
                case IdSampleFreq: t.SampleRate = ReadFloat(br, size); break;
                case IdChannels:   t.Channels   = (int)ReadUInt(br, size); break;
                case IdBitDepth:   t.BitDepth   = (int)ReadUInt(br, size); break;
            }
            fs.Seek(dp + size, SeekOrigin.Begin);
        }
    }

    private static void ParseVideo(BinaryReader br, long end, MediaTrackInfo t)
    {
        var fs = br.BaseStream;
        while (fs.Position < end - 2)
        {
            uint id   = ReadId(br);
            long size = ReadSize(br);
            long dp   = fs.Position;
            if (size < 0 || size == Unknown) break;
            switch (id)
            {
                case IdPixWidth:  t.TrackWidth  = (int)ReadUInt(br, size); break;
                case IdPixHeight: t.TrackHeight = (int)ReadUInt(br, size); break;
            }
            fs.Seek(dp + size, SeekOrigin.Begin);
        }
    }

    private static void SkipElement(BinaryReader br)
    {
        long size = ReadSize(br);
        if (size > 0 && size != Unknown)
            br.BaseStream.Seek(size, SeekOrigin.Current);
    }

    // EBML VINT element ID — leading-one bits are part of the ID (mask kept)
    private static uint ReadId(BinaryReader br)
    {
        byte b = br.ReadByte();
        if ((b & 0x80) != 0) return b;
        if ((b & 0x40) != 0) return ((uint)b << 8)  | br.ReadByte();
        if ((b & 0x20) != 0) return ((uint)b << 16) | ((uint)br.ReadByte() << 8) | br.ReadByte();
        if ((b & 0x10) != 0) return ((uint)b << 24) | ((uint)br.ReadByte() << 16) | ((uint)br.ReadByte() << 8) | br.ReadByte();
        return 0;
    }

    // EBML VINT data size — leading-one bit removed; returns Unknown for the all-ones sentinel
    private static long ReadSize(BinaryReader br)
    {
        byte b = br.ReadByte();
        if ((b & 0x80) != 0) { long v = b & 0x7FL;                                                                                                     return v == 0x7FL               ? Unknown : v; }
        if ((b & 0x40) != 0) { long v = ((long)(b & 0x3F) << 8)  | br.ReadByte();                                                                     return v == 0x3FFFL             ? Unknown : v; }
        if ((b & 0x20) != 0) { var x=br.ReadBytes(2); long v=((long)(b&0x1F)<<16)|((long)x[0]<<8)|x[1];                                              return v == 0x1FFFFFL           ? Unknown : v; }
        if ((b & 0x10) != 0) { var x=br.ReadBytes(3); long v=((long)(b&0x0F)<<24)|((long)x[0]<<16)|((long)x[1]<<8)|x[2];                             return v == 0x0FFFFFFFL         ? Unknown : v; }
        if ((b & 0x08) != 0) { var x=br.ReadBytes(4); long v=((long)(b&0x07)<<32)|((long)x[0]<<24)|((long)x[1]<<16)|((long)x[2]<<8)|x[3];           return v == 0x07FFFFFFFFL       ? Unknown : v; }
        if ((b & 0x04) != 0) { var x=br.ReadBytes(5); long v=((long)(b&0x03)<<40)|((long)x[0]<<32)|((long)x[1]<<24)|((long)x[2]<<16)|((long)x[3]<<8)|x[4]; return v==0x03FFFFFFFFFFL ? Unknown : v; }
        if ((b & 0x02) != 0) { var x=br.ReadBytes(6); long v=((long)(b&0x01)<<48)|((long)x[0]<<40)|((long)x[1]<<32)|((long)x[2]<<24)|((long)x[3]<<16)|((long)x[4]<<8)|x[5]; return v==0x01FFFFFFFFFFFFL ? Unknown : v; }
        if ((b & 0x01) != 0) { var x=br.ReadBytes(7); long v=0; foreach(var c in x) v=(v<<8)|c; return v==0x00FFFFFFFFFFFFFFL ? Unknown : v; }
        return Unknown;
    }

    private static ulong ReadUInt(BinaryReader br, long size)
    {
        ulong v = 0;
        for (int i = 0; i < Math.Min((int)size, 8); i++) v = (v << 8) | br.ReadByte();
        return v;
    }

    private static double ReadFloat(BinaryReader br, long size)
    {
        if (size == 4) { var b = br.ReadBytes(4); if (BitConverter.IsLittleEndian) Array.Reverse(b); return BitConverter.ToSingle(b, 0); }
        if (size == 8) { var b = br.ReadBytes(8); if (BitConverter.IsLittleEndian) Array.Reverse(b); return BitConverter.ToDouble(b, 0); }
        return 0;
    }

    private static string ReadUtf8(BinaryReader br, long size)
        => System.Text.Encoding.UTF8.GetString(br.ReadBytes((int)size)).TrimEnd('\0');

    private static string ReadAscii(BinaryReader br, long size)
        => System.Text.Encoding.ASCII.GetString(br.ReadBytes((int)size)).TrimEnd('\0');

    private static string TypeName(ulong type) => type switch
    {
        1  => "Video",
        2  => "Audio",
        17 => "Subtitle",
        3  => "Complex",
        16 => "Logo",
        18 => "Buttons",
        32 => "Control",
        33 => "Metadata",
        _  => $"Type {type}"
    };

    // Common ISO 639-2 and BCP-47 language codes → display name
    internal static string LanguageDisplay(string code) => code.ToLowerInvariant() switch
    {
        "und" or ""             => "Undetermined",
        "eng" or "en"           => "English",
        "swe" or "sv"           => "Swedish",
        "nor" or "nb" or "nn"   => "Norwegian",
        "dan" or "da"           => "Danish",
        "fin" or "fi"           => "Finnish",
        "fre" or "fra" or "fr"  => "French",
        "ger" or "deu" or "de"  => "German",
        "spa" or "es"           => "Spanish",
        "ita" or "it"           => "Italian",
        "por" or "pt"           => "Portuguese",
        "rus" or "ru"           => "Russian",
        "chi" or "zho" or "zh"  => "Chinese",
        "jpn" or "ja"           => "Japanese",
        "kor" or "ko"           => "Korean",
        "ara" or "ar"           => "Arabic",
        "dut" or "nld" or "nl"  => "Dutch",
        "pol" or "pl"           => "Polish",
        "tur" or "tr"           => "Turkish",
        "heb" or "he"           => "Hebrew",
        "hin" or "hi"           => "Hindi",
        "tha" or "th"           => "Thai",
        "vie" or "vi"           => "Vietnamese",
        "ind" or "id"           => "Indonesian",
        "ces" or "cze" or "cs"  => "Czech",
        "slk" or "slo" or "sk"  => "Slovak",
        "hun" or "hu"           => "Hungarian",
        "ron" or "rum" or "ro"  => "Romanian",
        "ukr" or "uk"           => "Ukrainian",
        "hrv" or "hr"           => "Croatian",
        "srp" or "sr"           => "Serbian",
        "bul" or "bg"           => "Bulgarian",
        "ell" or "gre" or "el"  => "Greek",
        "cat" or "ca"           => "Catalan",
        _                       => code
    };

    // Friendly codec name from Matroska codec ID
    internal static string CodecDisplay(string id) => id.ToUpperInvariant() switch
    {
        "V_MPEG4/ISO/AVC"    => "H.264",
        "V_MPEGH/ISO/HEVC"   => "H.265 (HEVC)",
        "V_AV1"              => "AV1",
        "V_VP8"              => "VP8",
        "V_VP9"              => "VP9",
        "V_MS/VFW/FOURCC"    => "VFW/DirectShow",
        "V_MPEG1"            => "MPEG-1",
        "V_MPEG2"            => "MPEG-2",
        "A_AAC"              => "AAC",
        "A_AAC/MPEG4/LC"     => "AAC LC",
        "A_AC3"              => "AC-3 (Dolby Digital)",
        "A_EAC3"             => "E-AC-3 (Dolby Digital Plus)",
        "A_DTS"              => "DTS",
        "A_DTS/EXPRESS"      => "DTS Express",
        "A_DTS/LOSSLESS"     => "DTS-HD Master Audio",
        "A_TRUEHD"           => "Dolby TrueHD",
        "A_FLAC"             => "FLAC",
        "A_OPUS"             => "Opus",
        "A_VORBIS"           => "Vorbis",
        "A_MP3"              => "MP3",
        "A_PCM/INT/LIT"      => "PCM (little-endian)",
        "A_PCM/INT/BIG"      => "PCM (big-endian)",
        "A_PCM/FLOAT/IEEE"   => "PCM (float)",
        "S_TEXT/UTF8"        => "SubRip (SRT)",
        "S_TEXT/ASS"         => "ASS/SSA",
        "S_TEXT/SSA"         => "SSA",
        "S_TEXT/WEBVTT"      => "WebVTT",
        "S_HDMV/PGS"         => "PGS (Blu-ray)",
        "S_DVBSUB"           => "DVB Subtitle",
        "S_VOBSUB"           => "VobSub",
        _                    => id
    };
}
