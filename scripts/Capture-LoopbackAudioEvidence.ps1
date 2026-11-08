[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [Parameter(Mandatory = $true)][double]$Seconds,
    [string]$StopFilePath
)

# Single-purpose evidence helper: records what the default Windows output
# device is playing (WASAPI shared-mode loopback) to one 32-bit-float WAV and a
# sidecar JSON holding its UTC start, so native event times can be located in
# it. It records the whole device mix, not one process. It selects no device,
# changes no volume or device setting, sends no input and needs nothing beyond
# Windows and .NET Framework. Audibility is for a listener to judge; a level
# measured here only shows signal was present.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($Seconds -le 0 -or $Seconds -gt 900) { throw 'Seconds must be within (0, 900].' }

if (-not ('KmgLoopbackRecorder' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
public static class KmgLoopbackRecorder {
    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")] class Enumerator {}
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumerator { int EnumAudioEndpoints(int f, int s, out IntPtr c); [PreserveSig] int GetDefaultAudioEndpoint(int f, int r, out IDevice d); }
    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDevice { [PreserveSig] int Activate(ref Guid iid, int ctx, IntPtr p, [MarshalAs(UnmanagedType.IUnknown)] out object o); }
    [Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IClient {
        [PreserveSig] int Initialize(int mode, int flags, long buffer, long period, IntPtr format, IntPtr session);
        [PreserveSig] int GetBufferSize(out uint frames);
        [PreserveSig] int GetStreamLatency(out long latency);
        [PreserveSig] int GetCurrentPadding(out uint padding);
        [PreserveSig] int IsFormatSupported(int mode, IntPtr format, out IntPtr closest);
        [PreserveSig] int GetMixFormat(out IntPtr format);
        [PreserveSig] int GetDevicePeriod(out long def, out long min);
        [PreserveSig] int Start();
        [PreserveSig] int Stop();
        [PreserveSig] int Reset();
        [PreserveSig] int SetEventHandle(IntPtr h);
        [PreserveSig] int GetService(ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object o);
    }
    [Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ICapture {
        [PreserveSig] int GetBuffer(out IntPtr data, out uint frames, out uint flags, out ulong pos, out ulong qpc);
        [PreserveSig] int ReleaseBuffer(uint frames);
        [PreserveSig] int GetNextPacketSize(out uint frames);
    }
    static void Check(int hr, string what) { if (hr != 0) throw new InvalidOperationException(what + " failed: 0x" + hr.ToString("x8")); }

    // Silence delivers no packets in loopback, so elapsed wall time is filled
    // with zeros: positions in the file stay aligned with real time.
    public static string Record(string path, double seconds, string stopFile) {
        var en = (IEnumerator)new Enumerator();
        IDevice dev; Check(en.GetDefaultAudioEndpoint(0, 1, out dev), "default endpoint");
        Guid iidClient = typeof(IClient).GUID; object o; Check(dev.Activate(ref iidClient, 23, IntPtr.Zero, out o), "activate");
        var client = (IClient)o;
        IntPtr fmt; Check(client.GetMixFormat(out fmt), "mix format");
        short tag = Marshal.ReadInt16(fmt, 0); short channels = Marshal.ReadInt16(fmt, 2); int rate = Marshal.ReadInt32(fmt, 4);
        short align = Marshal.ReadInt16(fmt, 12); short bits = Marshal.ReadInt16(fmt, 14);
        if (bits != 32) throw new InvalidOperationException("Unexpected mix format bits " + bits);
        Check(client.Initialize(0, 0x00020000, 2000000, 0, fmt, IntPtr.Zero), "initialize loopback");
        Guid iidCapture = typeof(ICapture).GUID; Check(client.GetService(ref iidCapture, out o), "capture service");
        var capture = (ICapture)o;
        DateTime started = DateTime.UtcNow; long framesWritten = 0; long silentFrames = 0; double peak = 0;
        using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read)) {
            var header = new byte[44]; fs.Write(header, 0, 44);
            var clock = System.Diagnostics.Stopwatch.StartNew();
            Check(client.Start(), "start");
            byte[] buffer = new byte[align * rate]; byte[] zeros = new byte[align * 4096];
            while (clock.Elapsed.TotalSeconds < seconds && (stopFile == null || !File.Exists(stopFile))) {
                Thread.Sleep(10);
                uint packet; Check(capture.GetNextPacketSize(out packet), "packet size");
                while (packet > 0) {
                    IntPtr data; uint frames, flags; ulong pos, qpc;
                    Check(capture.GetBuffer(out data, out frames, out flags, out pos, out qpc), "get buffer");
                    int bytes = (int)frames * align;
                    if (bytes > buffer.Length) buffer = new byte[bytes];
                    if ((flags & 2) != 0) Array.Clear(buffer, 0, bytes); else Marshal.Copy(data, buffer, 0, bytes);
                    Check(capture.ReleaseBuffer(frames), "release");
                    if (tag == 3 || tag == -2)
                        for (int i = 0; i + 4 <= bytes; i += 4) { double v = Math.Abs(BitConverter.ToSingle(buffer, i)); if (v > peak) peak = v; }
                    fs.Write(buffer, 0, bytes); framesWritten += frames;
                    Check(capture.GetNextPacketSize(out packet), "packet size");
                }
                long expected = (long)(clock.Elapsed.TotalSeconds * rate);
                while (expected - framesWritten > rate / 5) {
                    int fill = (int)Math.Min(expected - framesWritten - rate / 10, 4096);
                    fs.Write(zeros, 0, fill * align); framesWritten += fill; silentFrames += fill;
                }
            }
            client.Stop();
            int dataBytes = checked((int)(framesWritten * align));
            fs.Position = 0;
            var w = new BinaryWriter(fs);
            w.Write(new[] { 'R','I','F','F' }); w.Write(36 + dataBytes); w.Write(new[] { 'W','A','V','E','f','m','t',' ' }); w.Write(16);
            w.Write((short)3); w.Write(channels); w.Write(rate); w.Write(rate * align); w.Write(align); w.Write((short)32);
            w.Write(new[] { 'd','a','t','a' }); w.Write(dataBytes); w.Flush();
        }
        Marshal.FreeCoTaskMem(fmt);
        return "{\"startedUtc\":\"" + started.ToString("o") + "\",\"sampleRate\":" + rate + ",\"channels\":" + channels +
            ",\"frames\":" + framesWritten + ",\"silenceFilledFrames\":" + silentFrames + ",\"peak\":" +
            peak.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture) + "}";
    }
}
'@
}

$destination = [IO.Path]::GetFullPath($OutputPath)
if (Test-Path -LiteralPath $destination) { throw "Refusing to overwrite: $destination" }
$summary = [KmgLoopbackRecorder]::Record($destination, $Seconds, $StopFilePath)
[IO.File]::WriteAllText("$destination.json", $summary, [Text.UTF8Encoding]::new($false))
Write-Host $summary
