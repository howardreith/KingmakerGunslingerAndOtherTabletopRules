#!/usr/bin/env python3
"""The exact-reference builder binds only to pinned loader references.

Offline fixtures: the builder's own pin check accepts the pinned bytes, records
their identity and refuses missing, substituted or incompletely pinned files.
"""
from __future__ import annotations

import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import build_mod_from_private_references as builder  # noqa: E402

ROOT = Path(__file__).resolve().parents[1]
PATHS = ["UnityModManager/UnityModManager.dll", "UnityModManager/0Harmony12.dll"]


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


class PinnedLoaderReferenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.loader = self.root / "loader"
        rows = []
        for relative in PATHS:
            path = self.loader / relative
            path.parent.mkdir(parents=True, exist_ok=True)
            data = ("pinned " + relative).encode()
            path.write_bytes(data)
            rows.append({"relativePath": relative, "assemblyVersion": "0.31.1.0",
                         "url": "https://fixture.invalid/" + relative, "sha256": digest(data)})
        self.pins = self.root / "pins.json"
        self.pins.write_text(json.dumps({"schemaVersion": 1, "minimumManagerVersion": "0.31.1",
                                         "references": rows}), encoding="utf-8")

    def tearDown(self):
        self.temp.cleanup()

    def test_pinned_files_are_the_compiler_references_and_are_recorded(self):
        paths, identity = builder.pinned_loader_references(self.loader, self.pins)
        self.assertEqual([p.relative_to(self.loader).as_posix() for p in paths], PATHS)
        self.assertEqual(identity["minimumManagerVersion"], "0.31.1")
        self.assertEqual(identity["pinsSha256"], digest(self.pins.read_bytes()))
        self.assertEqual(set(identity["files"]), set(PATHS))
        self.assertEqual(identity["files"][PATHS[0]]["sha256"],
                         digest((self.loader / PATHS[0]).read_bytes()))

    def test_substituted_newer_loader_is_refused(self):
        (self.loader / PATHS[0]).write_bytes(b"installed 0.33.0 UnityModManager")
        with self.assertRaisesRegex(ValueError, "differs from its pin"):
            builder.pinned_loader_references(self.loader, self.pins)

    def test_missing_loader_is_refused_without_fallback(self):
        (self.loader / PATHS[1]).unlink()
        with self.assertRaises(FileNotFoundError):
            builder.pinned_loader_references(self.loader, self.pins)

    def test_incomplete_pins_are_refused(self):
        document = json.loads(self.pins.read_text(encoding="utf-8"))
        document["references"] = document["references"][:1]
        self.pins.write_text(json.dumps(document), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "must name exactly"):
            builder.pinned_loader_references(self.loader, self.pins)

    def test_repository_pins_declare_the_info_json_floor(self):
        pins = json.loads((ROOT / "scripts" / "loader-reference-pins.json").read_text(encoding="utf-8-sig"))
        info = json.loads((ROOT / "Info.json").read_text(encoding="utf-8-sig"))
        self.assertEqual(pins["minimumManagerVersion"], "0.31.1")
        self.assertEqual(info["ManagerVersion"], pins["minimumManagerVersion"])
        self.assertEqual([row["relativePath"] for row in pins["references"]], PATHS)


if __name__ == "__main__":
    unittest.main()
