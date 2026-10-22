using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The visual refinement's repaired first exports: original bodies outside
    /// the owner's review list that shipped inside out from the legacy shared
    /// writer, repaired in place by continuous-bodies/repair_outward_winding.py
    /// on the owner's word (2026-10-17). Geometry, UVs and weights are
    /// unchanged for Eagle, Stirge and Fire Beetle. Cleanup iteration 2
    /// deliberately reauthors Stag Beetle, Girallon, Crab and Stegosaurus;
    /// current export identities below replace only those four current assets.
    /// </summary>
    internal static class RepairedBodyTests
    {
        private sealed class Body
        {
            internal string Path;
            internal string Sha256;
            internal bool ClosedParts;
        }

        private static readonly Body[] Bodies =
        {
            new Body { Path = "assets/flying-animals/eagle-mesh.json", ClosedParts = false,
                Sha256 = "4b254a4b17bd1cab51b4856203dc07ff5ad70091d037e9e26eaa50aecaae93c4" },
            new Body { Path = "assets/flying-animals/stirge-mesh.json", ClosedParts = true,
                Sha256 = "f9ea0f0e639faf6f642b41f59a6e3216e2838f5c3dbb608a26c0c7eb073b3b82" },
            new Body { Path = "assets/sprint14-insects/fire-beetle-mesh.json", ClosedParts = true,
                Sha256 = "71acda0c408a0f925f50591744f9355ecc2f8cc8c126771e33ca4cd74eb8a661" },
            new Body { Path = "assets/sprint14-insects/giant-stag-beetle-mesh.json", ClosedParts = true,
                Sha256 = "2ecf222413eb857051b70df8b5fd61592a939108c7661ccdfb96f20fbcea794d" },
            new Body { Path = "assets/sprint19-fourarmed/girallon-mesh.json", ClosedParts = true,
                Sha256 = "aa81564f34a453f7a77902c45515eb5ac222fc731edd9a73aef797eb87a9f8f5" },
            new Body { Path = "assets/sprint21-arthropods/giant-crab-mesh.json", ClosedParts = true,
                Sha256 = "34833285f367258ed5d00127c72d86bb96897df94459740d3c9fd800e720893e" },
            new Body { Path = "assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json", ClosedParts = true,
                Sha256 = "27af896d68f8e6b0e201e0052a3ee5e5867c559669cd7de60ec01c3b3996e976" },
        };

        internal static void FirstExportsShipRepairedOutward()
        {
            foreach (Body body in Bodies)
            {
                string path = Path.Combine(Environment.CurrentDirectory,
                    body.Path.Replace('/', Path.DirectorySeparatorChar));
                using (SHA256 sha = SHA256.Create())
                    Assertions.Equal(body.Sha256, string.Concat(sha.ComputeHash(
                            File.ReadAllBytes(path)).Select(value => value.ToString("x2"))),
                        body.Path + " is the repaired export.");
                string key = Path.GetFileName(path).Replace("-mesh.json", "");
                OriginalMeshOrientation.AssertRepairedBody(path, key, body.ClosedParts);
            }
            // The Giant Crab's repair is also closed and consistently wound.
            OriginalMeshOrientation.AssertShippedBody(Path.Combine(Environment.CurrentDirectory,
                "assets", "sprint21-arthropods", "giant-crab-mesh.json"), "giant-crab");
            foreach (string path in new[] { "assets/sprint19-fourarmed/girallon-mesh.json",
                "assets/sprint21-arthropods/giant-crab-mesh.json" })
                Assertions.Equal("outward-visual-refinement", (string)JObject.Parse(File.ReadAllText(
                        Path.Combine(Environment.CurrentDirectory, path.Replace('/', Path.DirectorySeparatorChar))))
                        ["triangleWinding"],
                    path + " declares the outward winding.");
        }
    }
}
