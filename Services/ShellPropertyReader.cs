using System.Runtime.InteropServices;

namespace FileInfoViewer.Services;

internal static class ShellPropertyReader
{
    private static Guid _iid = new("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");

    public static ShellVideoProps ReadVideo(string filePath)
    {
        var p = new ShellVideoProps();
        IPropertyStore? store = null;
        try
        {
            if (SHGetPropertyStoreFromParsingName(filePath, IntPtr.Zero, 0, ref _iid, out store) != 0 || store == null)
                return p;

            p.Title   = GetStr(store, PKEY_Title);
            p.Subject = GetStr(store, PKEY_Subject);
            p.Comment = GetStr(store, PKEY_Comment);
            p.Tags    = GetStrArray(store, PKEY_Keywords);
            p.Rating  = GetU32(store, PKEY_Rating);

            ulong dur = GetU64(store, PKEY_Media_Duration);
            if (dur > 0) p.Duration = TimeSpan.FromTicks((long)dur);

            p.FrameWidth  = (int)GetU32(store, PKEY_Video_FrameWidth);
            p.FrameHeight = (int)GetU32(store, PKEY_Video_FrameHeight);
            uint fr = GetU32(store, PKEY_Video_FrameRate);
            if (fr > 0) p.FrameRate = fr / 1000.0;
            p.TotalBitrate    = GetU32(store, PKEY_Video_TotalBitrate);
            p.DataRate        = GetU32(store, PKEY_Video_DataRate);
            p.AudioBitrate    = GetU32(store, PKEY_Audio_EncodingBitrate) / 1000u;
            p.AudioChannels   = (int)GetU32(store, PKEY_Audio_ChannelCount);
            p.AudioSampleRate = GetU32(store, PKEY_Audio_SampleRate);
        }
        catch { }
        finally { if (store != null) Marshal.ReleaseComObject(store); }
        return p;
    }

    private static string GetStr(IPropertyStore store, PROPKEY key)
    {
        PROPVARIANT pv = default;
        try
        {
            store.GetValue(ref key, out pv);
            if ((pv.vt == VT_LPWSTR || pv.vt == VT_BSTR) && pv.ptr != IntPtr.Zero)
                return Marshal.PtrToStringUni(pv.ptr) ?? "";
            return "";
        }
        finally { PropVariantClear(ref pv); }
    }

    private static string[] GetStrArray(IPropertyStore store, PROPKEY key)
    {
        PROPVARIANT pv = default;
        try
        {
            store.GetValue(ref key, out pv);
            if (pv.vt == (VT_VECTOR | VT_LPWSTR) && pv.cElems > 0)
            {
                var result = new string[pv.cElems];
                for (int i = 0; i < result.Length; i++)
                {
                    var ptr = Marshal.ReadIntPtr(pv.pElems, i * IntPtr.Size);
                    result[i] = ptr != IntPtr.Zero ? Marshal.PtrToStringUni(ptr) ?? "" : "";
                }
                return result;
            }
            if ((pv.vt == VT_LPWSTR || pv.vt == VT_BSTR) && pv.ptr != IntPtr.Zero)
                return [Marshal.PtrToStringUni(pv.ptr) ?? ""];
            return [];
        }
        finally { PropVariantClear(ref pv); }
    }

    private static uint GetU32(IPropertyStore store, PROPKEY key)
    {
        PROPVARIANT pv = default;
        try { store.GetValue(ref key, out pv); return pv.vt == VT_UI4 ? pv.ui4 : 0u; }
        finally { PropVariantClear(ref pv); }
    }

    private static ulong GetU64(IPropertyStore store, PROPKEY key)
    {
        PROPVARIANT pv = default;
        try { store.GetValue(ref key, out pv); return pv.vt == VT_UI8 ? pv.ui8 : 0ul; }
        finally { PropVariantClear(ref pv); }
    }

    // PROPVARIANT — 24 bytes on 64-bit Windows.
    // Header: vt at offset 0.  Union: offset 8.
    // CALPWSTR layout: cElems (uint) at offset 8, pointer-aligned pElems at offset 16.
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    private struct PROPVARIANT
    {
        [FieldOffset(0)]  public ushort vt;
        [FieldOffset(8)]  public IntPtr ptr;    // VT_LPWSTR / VT_BSTR
        [FieldOffset(8)]  public uint   ui4;    // VT_UI4
        [FieldOffset(8)]  public ulong  ui8;    // VT_UI8
        [FieldOffset(8)]  public uint   cElems; // CALPWSTR.cElems
        [FieldOffset(16)] public IntPtr pElems; // CALPWSTR.pElems
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROPKEY { public Guid fmtid; public uint pid; }

    private const ushort VT_BSTR   = 8;
    private const ushort VT_UI4    = 19;
    private const ushort VT_UI8    = 21;
    private const ushort VT_LPWSTR = 31;
    private const ushort VT_VECTOR = 0x1000;

    [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint cProps);
        [PreserveSig] int GetAt(uint iProp, out PROPKEY pkey);
        [PreserveSig] int GetValue(ref PROPKEY key, out PROPVARIANT pv);
        [PreserveSig] int SetValue(ref PROPKEY key, ref PROPVARIANT pv);
        [PreserveSig] int Commit();
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHGetPropertyStoreFromParsingName(string path, IntPtr pbc, int flags, ref Guid riid, out IPropertyStore ppv);

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PROPVARIANT pv);

    private static PROPKEY Pk(string guid, uint pid) => new() { fmtid = new Guid(guid), pid = pid };

    private static readonly PROPKEY PKEY_Title   = Pk("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 2);
    private static readonly PROPKEY PKEY_Subject = Pk("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 3);
    private static readonly PROPKEY PKEY_Keywords = Pk("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 5);
    private static readonly PROPKEY PKEY_Comment  = Pk("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 6);
    private static readonly PROPKEY PKEY_Rating   = Pk("64440492-4C8B-11D1-8B70-080036B11A03", 9);
    private static readonly PROPKEY PKEY_Media_Duration       = Pk("64440490-4C8B-11D1-8B70-080036B11A03", 3);
    private static readonly PROPKEY PKEY_Video_FrameWidth     = Pk("64440491-4C8B-11D1-8B70-080036B11A03", 42);
    private static readonly PROPKEY PKEY_Video_FrameHeight    = Pk("64440491-4C8B-11D1-8B70-080036B11A03", 4);
    private static readonly PROPKEY PKEY_Video_FrameRate      = Pk("64440491-4C8B-11D1-8B70-080036B11A03", 6);
    private static readonly PROPKEY PKEY_Video_TotalBitrate   = Pk("64440491-4C8B-11D1-8B70-080036B11A03", 43);
    private static readonly PROPKEY PKEY_Video_DataRate       = Pk("64440491-4C8B-11D1-8B70-080036B11A03", 8);
    private static readonly PROPKEY PKEY_Audio_EncodingBitrate = Pk("64440490-4C8B-11D1-8B70-080036B11A03", 4);
    private static readonly PROPKEY PKEY_Audio_ChannelCount    = Pk("64440490-4C8B-11D1-8B70-080036B11A03", 7);
    private static readonly PROPKEY PKEY_Audio_SampleRate      = Pk("64440490-4C8B-11D1-8B70-080036B11A03", 5);
}

internal class ShellVideoProps
{
    public string   Title         { get; set; } = "";
    public string   Subject       { get; set; } = "";
    public string   Comment       { get; set; } = "";
    public string[] Tags          { get; set; } = [];
    public uint     Rating        { get; set; }
    public TimeSpan Duration      { get; set; }
    public int      FrameWidth    { get; set; }
    public int      FrameHeight   { get; set; }
    public double   FrameRate     { get; set; }
    public uint     TotalBitrate  { get; set; }  // kbps
    public uint     DataRate      { get; set; }  // kbps (video stream only)
    public uint     AudioBitrate  { get; set; }  // kbps
    public int      AudioChannels { get; set; }
    public uint     AudioSampleRate { get; set; } // Hz
}
