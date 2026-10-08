"""Reject identity-order and binary drift at the two-release integration seam."""
import copy
import json
import tempfile
import unittest
import sys
from pathlib import Path
from unittest.mock import patch

sys.dont_write_bytecode = True

import validate_expanded_summoning_checkpoint143 as gate
from validate_repository import VALIDATORS

ROOT = Path(__file__).resolve().parents[1]


class Checkpoint143Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.master = json.loads(gate.blob(ROOT, gate.MASTER, "blueprints/blueprints.json"))
        cls.imported = json.loads(gate.blob(ROOT, gate.SUMMONING, "blueprints/blueprints.json"))
        cls.current = gate.document(ROOT, "blueprints/blueprints.json")

    def test_exact_union_and_version_dispatch(self):
        gate.validate_identity_union(self.master, self.imported, self.current)
        self.assertEqual("validate_expanded_summoning_checkpoint143.py", VALIDATORS["0.0.143"])

    def test_master_prefix_and_import_order_cannot_move(self):
        for index in (1, len(self.master["entries"]) + 1):
            altered = copy.deepcopy(self.current)
            altered["entries"][index], altered["entries"][index - 1] = altered["entries"][index - 1], altered["entries"][index]
            with self.assertRaisesRegex(AssertionError, "order/content drift"):
                gate.validate_identity_union(self.master, self.imported, altered)

    def test_shared_identity_metadata_cannot_change(self):
        altered = copy.deepcopy(self.imported)
        altered["entries"][0]["status"] = "reserved"
        with self.assertRaisesRegex(AssertionError, "Shared identity/metadata differs"):
            gate.validate_identity_union(self.master, altered, self.current)

    def test_binary_bytes_are_not_line_normalized(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "asset.png").write_bytes(b"binary\r\npayload")
            with patch.object(gate.subprocess, "check_output", return_value="asset.png\n"), patch.object(gate, "blob", return_value=b"binary\npayload"):
                with self.assertRaisesRegex(AssertionError, "Accepted production changed"):
                    gate.preserved_files(root, "accepted", ("asset",))

    def test_original_flight_correction_pins_both_exact_versions(self):
        self.assertEqual(2, len(gate.RESOURCE_CORRECTION))
        for path in gate.RESOURCE_CORRECTION:
            accepted = gate.blob(ROOT, gate.SUMMONING, path).replace(b"\r\n", b"\n")
            current = (ROOT / path).read_bytes().replace(b"\r\n", b"\n")
            self.assertTrue(gate.exact_resource_correction(gate.SUMMONING, path, accepted, current))
            self.assertFalse(gate.exact_resource_correction(gate.SUMMONING, path, accepted, current + b" drift"))
            self.assertFalse(gate.exact_resource_correction(gate.SUMMONING, path, accepted + b" drift", current))

    def test_resource_pin_cannot_relax_other_files_or_master(self):
        for path in gate.RESOURCE_CORRECTION:
            accepted = gate.blob(ROOT, gate.SUMMONING, path).replace(b"\r\n", b"\n")
            current = (ROOT / path).read_bytes().replace(b"\r\n", b"\n")
            self.assertFalse(gate.exact_resource_correction(gate.MASTER, path, accepted, current))
            self.assertFalse(gate.exact_resource_correction(gate.SUMMONING, "other.cs", accepted, current))


if __name__ == "__main__":
    unittest.main()
