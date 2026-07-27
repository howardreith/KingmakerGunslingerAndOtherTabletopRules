#!/usr/bin/env python3
"""Focused integration tests for version-aware repository validation."""
from __future__ import annotations

import json
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path


def run(command: list[str], expected: int, required: str) -> None:
    completed = subprocess.run(command, text=True, capture_output=True, check=False)
    output = completed.stdout + completed.stderr
    if completed.returncode != expected:
        raise RuntimeError(
            f"Expected exit {expected}, observed {completed.returncode}: {command}\n{output}"
        )
    if required not in output:
        raise RuntimeError(f"Missing expected output {required!r}: {command}\n{output}")


def main() -> int:
    source = Path(__file__).resolve().parents[1]
    python = sys.executable
    dispatcher = source / "tools" / "validate_repository.py"
    sprint29 = source / "tools" / "validate_sprint29.py"
    sprint30 = source / "tools" / "validate_sprint30.py"

    run(
        [python, "-B", str(dispatcher), "--root", str(source)],
        0,
        "dispatched version 0.0.30 to validate_sprint30.py",
    )
    run(
        [python, "-B", str(sprint29)],
        1,
        "Info.json does not declare version 0.0.29",
    )

    with tempfile.TemporaryDirectory(prefix="KmgValidatorTests-") as temporary:
        fixture = Path(temporary) / "repository"
        shutil.copytree(
            source,
            fixture,
            ignore=shutil.ignore_patterns(".git", "artifacts", "__pycache__"),
        )

        report = fixture / "SPRINT-30-REPORT.md"
        saved_report = report.read_bytes()
        report.unlink()
        run(
            [python, "-B", str(sprint30), "--root", str(fixture)],
            1,
            "Required Sprint 30 file is missing: SPRINT-30-REPORT.md",
        )
        report.write_bytes(saved_report)

        info_path = fixture / "Info.json"
        info = json.loads(info_path.read_text(encoding="utf-8"))
        info["Version"] = "0.0.31"
        info_path.write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8")
        run(
            [python, "-B", str(dispatcher), "--root", str(fixture)],
            1,
            "Unsupported repository version: '0.0.31'",
        )

        info["Version"] = "0.0.29"
        info_path.write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8")
        run(
            [python, "-B", str(sprint30), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.30",
        )

    print("Validation dispatch integration tests passed: 6 checks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
