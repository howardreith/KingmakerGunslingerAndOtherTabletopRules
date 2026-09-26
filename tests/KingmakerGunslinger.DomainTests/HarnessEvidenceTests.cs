using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>Runtime-harness evidence rules that are not favored-class rows (B3).</summary>
    internal static class HarnessEvidenceTests
    {
        // B3: the native screenshot poll reads only the header and the IEND
        // trailer of a file still being written, and the image once.
        internal static void CompletedPngIsReadOnce()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "NativeIconScreenEvidence.cs");
            string source = File.ReadAllText(path).Replace("\r\n", "\n");
            int method = source.IndexOf("private static byte[] ReadCompletedPng(", StringComparison.Ordinal);
            Assertions.True(method >= 0, "ReadCompletedPng is missing.");
            int trailer = source.IndexOf("stream.Seek(length - PngEnd.Length, SeekOrigin.Begin);", method,
                StringComparison.Ordinal);
            int full = source.IndexOf("var bytes = new byte[(int)length];", method, StringComparison.Ordinal);
            Assertions.True(trailer > method && full > trailer,
                "The trailer must be checked before the whole image is allocated.");
            Assertions.True(source.IndexOf("new byte[(int)stream.Length]", StringComparison.Ordinal) < 0,
                "No full read before the trailer check may remain.");
            Assertions.True(source.Contains("0xAE, 0x42, 0x60, 0x82"),
                "The trailer is the complete empty IEND chunk with its fixed CRC.");
        }
    }
}
