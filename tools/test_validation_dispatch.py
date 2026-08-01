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
    sprint31 = source / "tools" / "validate_sprint31.py"
    sprint32 = source / "tools" / "validate_sprint32.py"
    sprint33 = source / "tools" / "validate_sprint33.py"
    sprint34 = source / "tools" / "validate_sprint34.py"

    run(
        [python, "-B", str(dispatcher), "--root", str(source)],
        0,
        "dispatched version 0.0.34 to validate_sprint34.py",
    )
    run(
        [python, "-B", str(sprint29)],
        1,
        "Info.json does not declare version 0.0.29",
    )

    fixture_parent = source / "artifacts" / "tmp"
    fixture_parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="KmgValidatorTests-", dir=fixture_parent) as temporary:
        fixture = Path(temporary) / "repository"
        shutil.copytree(
            source,
            fixture,
            ignore=shutil.ignore_patterns(".git", "artifacts", "__pycache__"),
        )

        report = fixture / "planning" / "SPRINT-34-ENTRY-CRITERIA.md"
        saved_report = report.read_bytes()
        report.unlink()
        run(
            [python, "-B", str(sprint34), "--root", str(fixture)],
            1,
            "Required Sprint 34 file is missing: planning/SPRINT-34-ENTRY-CRITERIA.md",
        )
        report.write_bytes(saved_report)

        info_path = fixture / "Info.json"
        info = json.loads(info_path.read_text(encoding="utf-8"))
        info["Version"] = "0.0.35"
        info_path.write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8")
        run(
            [python, "-B", str(dispatcher), "--root", str(fixture)],
            1,
            "Unsupported repository version: '0.0.35'",
        )

        info["Version"] = "0.0.29"
        info_path.write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8")
        run(
            [python, "-B", str(sprint30), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.30",
        )

        run(
            [python, "-B", str(sprint31), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.31",
        )

        run(
            [python, "-B", str(sprint32), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.32",
        )

        run(
            [python, "-B", str(sprint33), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.33",
        )

        run(
            [python, "-B", str(sprint34), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.34",
        )

    print("Validation dispatch integration tests passed: 10 checks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
