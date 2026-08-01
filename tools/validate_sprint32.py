#!/usr/bin/env python3
"""Portable source validator for Sprint 32 with inherited Sprint 31 checks."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_sprint31

VERSION = "0.0.32"
INFORMATIONAL_VERSION = "0.0.32-s32-scatter-attacks"
TEST_COUNT = 634


def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 32 file is missing: {relative}")
    return path.read_text(encoding="utf-8")


def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")


def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint31.validate(
        root,
        version=VERSION,
        informational_version=INFORMATIONAL_VERSION,
        test_count=TEST_COUNT,
    )
    require_tokens(
        read(root, "planning/SPRINT-32-ENTRY-CRITERIA.md"),
        ["separate attack roll", "-2", "No cone length may be invented",
         "exact unit reference", "Two consecutive PASS runs"],
        "Sprint 32 entry criteria",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Scatter/ScatterTargetPlanService.cs"),
        ["ReferenceIdentityComparer.Instance", "ScatterGeometryDisposition.Unknown",
         "ReferenceEquals(candidate.Unit, exactWielder)", "accepted.Sort"],
        "Sprint 32 scatter target planner",
    )
    require_tokens(
        read(root, "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        ['Case("scatter.plan-empty"', 'Case("scatter.plan-dedupe-reference"',
         'Case("scatter.plan-unknown-fails-closed"',
         'Case("scatter.candidate-invalid-distance"'],
        "Sprint 32 scatter target tests",
    )
    print("Sprint 32 source invariant validation passed with inherited Sprint 31 checks.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 32 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
