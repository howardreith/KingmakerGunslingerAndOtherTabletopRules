#!/usr/bin/env python3
"""Dispatch repository validation from authoritative Info.json version metadata."""
from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path

VALIDATORS = {
    "0.0.29": "validate_sprint29.py",
    "0.0.30": "validate_sprint30.py",
    "0.0.31": "validate_sprint31.py",
    "0.0.32": "validate_sprint32.py",
    "0.0.33": "validate_sprint33.py",
    "0.0.34": "validate_sprint34.py",
    "0.0.35": "validate_sprint35.py",
}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    root = args.root.resolve()
    try:
        info_path = root / "Info.json"
        info = json.loads(info_path.read_text(encoding="utf-8"))
        version = info.get("Version")
        validator_name = VALIDATORS.get(version)
        if validator_name is None:
            raise RuntimeError(f"Unsupported repository version: {version!r}")
        validator = Path(__file__).resolve().parent / validator_name
        command = [sys.executable, str(validator)]
        if version in {"0.0.30", "0.0.31", "0.0.32", "0.0.33", "0.0.34", "0.0.35"}:
            command.extend(["--root", str(root)])
        elif root != Path(__file__).resolve().parents[1]:
            raise RuntimeError("Sprint 29 fixture-root dispatch is not supported by its historical CLI.")
        completed = subprocess.run(command, check=False)
        if completed.returncode:
            return completed.returncode
        print(f"Repository validation dispatched version {version} to {validator_name}.")
        return 0
    except Exception as exception:
        print(f"Repository validation failed: {exception}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
