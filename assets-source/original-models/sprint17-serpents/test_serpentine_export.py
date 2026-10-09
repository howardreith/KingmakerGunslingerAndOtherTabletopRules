#!/usr/bin/env python3
"""Byte-preserving original winding regressions; no game or native assets."""
import base64
import copy
from pathlib import Path
import struct
import sys
import unittest

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import serpentine_export as export


def fixture():
    raw = bytearray(bytes(range(128)) + struct.pack("<6i", 0, 1, 2, 0, 2, 3) + bytes(range(128)))
    return dict(schemaVersion=2, vertexCount=4, triangleCount=2,
                data=base64.b64encode(raw).decode("ascii"), bones=["Original"],
                albedo=dict(file="original.png", sha256="unchanged"))


class OriginalExportTests(unittest.TestCase):
    def test_changes_only_each_second_third_index_and_preserves_input(self):
        for creature in export.KEYS:
            source = fixture()
            before = copy.deepcopy(source)
            result = export.finish_original_winding(source, creature)
            self.assertEqual(before, source)
            raw, corrected = (base64.b64decode(p["data"]) for p in (source, result))
            self.assertEqual(raw[:128], corrected[:128], "vertices/normals/UV byte-identical")
            self.assertEqual(raw[152:], corrected[152:], "all bone weights byte-identical")
            self.assertEqual((0, 2, 1, 0, 3, 2), struct.unpack_from("<6i", corrected, 128))
            self.assertEqual(export.WINDING, result["triangleWinding"])
            for key in source.keys() - {"data"}:
                self.assertEqual(source[key], result[key], key)

    def test_rejects_other_families_and_double_correction(self):
        for creature in (None, "", "purple-worm", "crocodile", "Viper"):
            with self.assertRaises(ValueError):
                export.finish_original_winding(fixture(), creature)
        corrected = export.finish_original_winding(fixture(), "viper")
        with self.assertRaises(ValueError):
            export.finish_original_winding(corrected, "viper")

    def test_rejects_incomplete_schema_counts_and_bytes_without_mutation(self):
        for field, value in (("schemaVersion", 1), ("vertexCount", 2), ("vertexCount", True),
                             ("triangleCount", 0), ("triangleCount", 1.5), ("data", "bad!"),
                             ("data", base64.b64encode(b"short").decode("ascii"))):
            source = fixture()
            source[field] = value
            before = copy.deepcopy(source)
            with self.assertRaises(ValueError):
                export.finish_original_winding(source, "viper")
            self.assertEqual(before, source)

    def test_rejects_degenerate_and_out_of_range_indices_without_mutation(self):
        for triangle in ((0, 0, 1), (0, 1, 0), (0, 1, 1), (-1, 1, 2), (0, 1, 4)):
            source = fixture()
            raw = bytearray(base64.b64decode(source["data"]))
            struct.pack_into("<3i", raw, 128, *triangle)
            source["data"] = base64.b64encode(raw).decode("ascii")
            before = copy.deepcopy(source)
            with self.assertRaises(ValueError):
                export.finish_original_winding(source, "constrictor-snake")
            self.assertEqual(before, source)


if __name__ == "__main__":
    unittest.main()
