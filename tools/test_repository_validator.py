"""Regression fixtures for a validator that exits zero without any checks."""
import tempfile
import sys
import unittest
from pathlib import Path

sys.dont_write_bytecode = True
from validate_repository import require_validator_source


class ValidatorSourceTests(unittest.TestCase):
    def test_empty_or_whitespace_validator_fails_closed(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "validator.py"
            for source in ("", " \r\n\t"):
                path.write_text(source, encoding="utf-8")
                with self.assertRaisesRegex(RuntimeError, "validator is empty"):
                    require_validator_source(path)

    def test_missing_validator_fails_closed(self):
        with tempfile.TemporaryDirectory() as directory:
            with self.assertRaises(FileNotFoundError):
                require_validator_source(Path(directory) / "missing.py")

    def test_nonempty_validator_is_allowed_to_run(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "validator.py"
            path.write_text("raise SystemExit(1)\n", encoding="utf-8")
            require_validator_source(path)


if __name__ == "__main__":
    unittest.main()
