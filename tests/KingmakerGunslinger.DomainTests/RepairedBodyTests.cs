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
    /// unchanged; only triangle order, the normals of inward mirrored parts
    /// and the primate/crab winding marker moved.
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
                Sha256 = "a7276d63c5e14fb6f45adb055c437bf61980f43344ee329b4c6cd5d048cc3cd6" },
            new Body { Path = "assets/sprint19-fourarmed/girallon-mesh.json", ClosedParts = true,
                Sha256 = "003512a54f6daeabd483f7cb85b1e84bf05b57b0d45a9ab1374cc09ca857d0bb" },
            new Body { Path = "assets/sprint21-arthropods/giant-crab-mesh.json", ClosedParts = true,
                Sha256 = "dd7448eb9fc384350baf734fa8e7430994a5b4c08858f79b53eab1c30d2ed8eb" },
            new Body { Path = "assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json", ClosedParts = true,
                Sha256 = "6c5dcb055eafe011e76616c7a137a18d1c8ff1a63e603ddb4dbb8328b87c4117" },
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
