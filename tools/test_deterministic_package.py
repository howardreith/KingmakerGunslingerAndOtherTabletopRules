"""Focused serializer admission tests; strict product paths remain in validate-package."""
import contextlib
import io
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import create_deterministic_package as package


class PackageTests(unittest.TestCase):
    def test_declared_new_count_and_determinism(self):
        with tempfile.TemporaryDirectory(prefix="kmg-package-contract-") as folder:
            root = Path(folder)
            source = root / "KingmakerGunslinger"
            source.mkdir()
            for index in range(416):
                (source / str(index)).write_bytes(str(index).encode())
            def run(count, output):
                with patch("sys.argv", ["package", "--source", str(source),
                        "--output", str(root / output), "--expected-file-count", str(count)]), \
                        contextlib.redirect_stdout(io.StringIO()):
                    return package.main()
            self.assertEqual(0, run(416, "one.zip"))
            self.assertEqual(0, run(416, "two.zip"))
            self.assertEqual((root / "one.zip").read_bytes(), (root / "two.zip").read_bytes())
            for count in (415, 417):
                with self.assertRaisesRegex(RuntimeError, "Expected exactly"):
                    run(count, "refused.zip")
                self.assertFalse((root / "refused.zip").exists())
            for count in (0, -1):
                with contextlib.redirect_stderr(io.StringIO()), self.assertRaises(SystemExit):
                    run(count, "refused.zip")
                self.assertFalse((root / "refused.zip").exists())


if __name__ == "__main__":
    unittest.main()
