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


if __name__ == "__main__":
    unittest.main()
