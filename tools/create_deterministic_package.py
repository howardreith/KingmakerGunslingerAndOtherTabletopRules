#!/usr/bin/env python3
"""Create a byte-deterministic ZIP from one staged mod directory."""
from __future__ import annotations

import argparse
import zipfile
from pathlib import Path

FIXED_TIME = (2026, 1, 1, 0, 0, 0)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--expected-file-count", required=True, type=int,
                        choices=(41, 42, 43, 44, 45, 121, 123, 125, 130, 132))
    args = parser.parse_args()
    source = args.source.resolve()
    if not source.is_dir() or source.name != "KingmakerGunslinger":
        raise RuntimeError("Source must be the staged KingmakerGunslinger directory.")
    files = sorted((path for path in source.rglob("*") if path.is_file()), key=lambda p: p.as_posix())
    if len(files) != args.expected_file_count:
        raise RuntimeError(
            f"Expected exactly {args.expected_file_count} staged package files, observed {len(files)}."
        )
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(
        args.output,
        "w",
        compression=zipfile.ZIP_DEFLATED,
        compresslevel=9,
        strict_timestamps=True,
    ) as archive:
        for path in files:
            relative = path.relative_to(source.parent).as_posix()
            info = zipfile.ZipInfo(relative, FIXED_TIME)
            info.compress_type = zipfile.ZIP_DEFLATED
            info.create_system = 0
            info.external_attr = 0
            archive.writestr(info, path.read_bytes(), compress_type=zipfile.ZIP_DEFLATED, compresslevel=9)
    print(args.output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
