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

        // E10: the permission-graph extension is a runtime-test seam only; it
        // is defined in FavoredClassRuntime and called from RuntimeTesting.
        internal static void PermissionGraphSeamIsTestOnly()
        {
            string root = Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger");
            foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                string relative = file.Substring(root.Length + 1).Replace('\\', '/');
                if (relative.StartsWith("RuntimeTesting/", StringComparison.Ordinal) ||
                    relative == "FavoredClass/FavoredClassRuntime.cs")
                    continue;
                Assertions.True(!File.ReadAllText(file).Contains("ExtendPermissionGraphForRuntimeTest"),
                    "A production path extends the permission graph: " + relative);
            }
            string runtime = File.ReadAllText(Path.Combine(root, "FavoredClass", "FavoredClassRuntime.cs"));
            Assertions.True(runtime.Contains("_graph = VerifiedGraph;") &&
                runtime.Contains("graph = _graph;") &&
                runtime.Contains("A permission graph extension is already active."),
                "The seam restores the verified graph and refuses nesting; lookups read the current graph.");
        }
    }
}
