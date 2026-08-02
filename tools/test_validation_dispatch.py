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
    sprint35 = source / "tools" / "validate_sprint35.py"
    sprint36 = source / "tools" / "validate_sprint36.py"
    sprint37 = source / "tools" / "validate_sprint37.py"
    sprint38 = source / "tools" / "validate_sprint38.py"
    sprint39 = source / "tools" / "validate_sprint39.py"
    sprint40 = source / "tools" / "validate_sprint40.py"
    sprint41 = source / "tools" / "validate_sprint41.py"
    sprint42 = source / "tools" / "validate_sprint42.py"
    sprint43 = source / "tools" / "validate_sprint43.py"
    sprint44 = source / "tools" / "validate_sprint55.py"

    run(
        [python, "-B", str(dispatcher), "--root", str(source)],
        0,
        "dispatched version 0.0.57 to validate_sprint57.py",
    )
    run([python, "-B", str(sprint44)], 0,
        "Sprint 55 source invariant validation passed")
    run([python, "-B", str(sprint42)], 1,
        "Info.json does not declare version 0.0.42")
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
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 34 file is missing: planning/SPRINT-34-ENTRY-CRITERIA.md",
        )
        report.write_bytes(saved_report)

        report35 = fixture / "planning" / "SPRINT-35-ENTRY-CRITERIA.md"
        saved_report35 = report35.read_bytes()
        report35.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 35 file is missing: planning/SPRINT-35-ENTRY-CRITERIA.md",
        )
        report35.write_bytes(saved_report35)

        info_path = fixture / "Info.json"
        info = json.loads(info_path.read_text(encoding="utf-8"))
        report36 = fixture / "planning" / "SPRINT-36-ENTRY-CRITERIA.md"
        saved_report36 = report36.read_bytes()
        report36.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 36 file is missing: planning/SPRINT-36-ENTRY-CRITERIA.md",
        )
        report36.write_bytes(saved_report36)

        report37 = fixture / "planning" / "SPRINT-37-ENTRY-CRITERIA.md"
        saved_report37 = report37.read_bytes()
        report37.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 37 file is missing: planning/SPRINT-37-ENTRY-CRITERIA.md",
        )
        report37.write_bytes(saved_report37)

        report38 = fixture / "planning" / "SPRINT-38-ENTRY-CRITERIA.md"
        saved_report38 = report38.read_bytes()
        report38.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 38 file is missing: planning/SPRINT-38-ENTRY-CRITERIA.md",
        )
        report38.write_bytes(saved_report38)

        report39 = fixture / "planning" / "SPRINT-39-ENTRY-CRITERIA.md"
        saved_report39 = report39.read_bytes()
        report39.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 39 file is missing: planning/SPRINT-39-ENTRY-CRITERIA.md",
        )
        report39.write_bytes(saved_report39)

        report40 = fixture / "planning" / "SPRINT-40-ENTRY-CRITERIA.md"
        saved_report40 = report40.read_bytes()
        report40.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 40 file is missing: planning/SPRINT-40-ENTRY-CRITERIA.md",
        )
        report40.write_bytes(saved_report40)

        report41 = fixture / "planning" / "SPRINT-41-ENTRY-CRITERIA.md"
        saved_report41 = report41.read_bytes()
        report41.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 41 file is missing: planning/SPRINT-41-ENTRY-CRITERIA.md",
        )
        report41.write_bytes(saved_report41)

        report42 = fixture / "planning" / "SPRINT-42-ENTRY-CRITERIA.md"
        saved_report42 = report42.read_bytes()
        report42.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 42 file is missing: planning/SPRINT-42-ENTRY-CRITERIA.md",
        )
        report42.write_bytes(saved_report42)

        report43 = fixture / "planning" / "SPRINT-43-ENTRY-CRITERIA.md"
        saved_report43 = report43.read_bytes()
        report43.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 43 file is missing: planning/SPRINT-43-ENTRY-CRITERIA.md",
        )
        report43.write_bytes(saved_report43)

        report44 = fixture / "planning" / "SPRINT-44-ENTRY-CRITERIA.md"
        saved_report44 = report44.read_bytes()
        report44.unlink()
        run(
            [python, "-B", str(sprint44), "--root", str(fixture)],
            1,
            "Required Sprint 44 file is missing: planning/SPRINT-44-ENTRY-CRITERIA.md",
        )
        report44.write_bytes(saved_report44)

        info["Version"] = "0.0.48"
        info_path.write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8")
        run(
            [python, "-B", str(dispatcher), "--root", str(fixture)],
            1,
            "Unsupported repository version: '0.0.48'",
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

        run(
            [python, "-B", str(sprint35), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.35",
        )

        run(
            [python, "-B", str(sprint36), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.36",
        )

        run(
            [python, "-B", str(sprint37), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.37",
        )

        run(
            [python, "-B", str(sprint38), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.38",
        )

        run(
            [python, "-B", str(sprint39), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.39",
        )

        run(
            [python, "-B", str(sprint40), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.40",
        )

        run(
            [python, "-B", str(sprint41), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.41",
        )

        run(
            [python, "-B", str(sprint42), "--root", str(fixture)],
            1,
            "Info.json does not declare version 0.0.42",
        )

    print("Validation dispatch integration tests passed: 29 checks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
