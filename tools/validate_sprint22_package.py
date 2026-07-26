#!/usr/bin/env python3
"""Validate the standalone Sprint 22 Unity Mod Manager ZIP."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import stat
import zipfile
from pathlib import Path, PurePosixPath

EXPECTED = {
    "KingmakerGunslinger/CHANGELOG.md",
    "KingmakerGunslinger/Info.json",
    "KingmakerGunslinger/KingmakerGunslinger.dll",
    "KingmakerGunslinger/LICENSE",
    "KingmakerGunslinger/README.md",
    "KingmakerGunslinger/SMOKE-TEST-GUIDE.md",
    "KingmakerGunslinger/blueprints/blueprints.json",
    "KingmakerGunslinger/blueprints/blueprints.schema.json",
}


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("archive", type=Path)
    parser.add_argument("--expected-dll-sha256", required=True)
    args = parser.parse_args()
    if not args.archive.is_file():
        raise FileNotFoundError(args.archive)

    with zipfile.ZipFile(args.archive) as archive:
        infos = [info for info in archive.infolist() if not info.is_dir()]
        names = [info.filename for info in infos]
        if len(names) != len(set(names)):
            raise RuntimeError("Archive contains duplicate file entries.")
        if set(names) != EXPECTED:
            raise RuntimeError(f"Unexpected archive entries: {sorted(set(names) ^ EXPECTED)}")
        for info in infos:
            path = PurePosixPath(info.filename)
            if path.is_absolute() or ".." in path.parts or path.parts[0] != "KingmakerGunslinger":
                raise RuntimeError(f"Unsafe archive path: {info.filename}")
            mode = (info.external_attr >> 16) & 0xFFFF
            if stat.S_ISLNK(mode):
                raise RuntimeError(f"Archive contains a symlink: {info.filename}")

        info_json = json.loads(archive.read("KingmakerGunslinger/Info.json"))
        if info_json.get("Version") != "0.0.22":
            raise RuntimeError("Info.json does not declare version 0.0.22.")
        if info_json.get("AssemblyName") != "KingmakerGunslinger.dll":
            raise RuntimeError("Unexpected mod assembly name.")

        blueprint_manifest = json.loads(archive.read("KingmakerGunslinger/blueprints/blueprints.json"))
        entries = blueprint_manifest.get("entries", [])
        if len(entries) != 12:
            raise RuntimeError("Blueprint manifest does not contain twelve stable IDs.")
        if sum(entry.get("status") == "active" for entry in entries) != 11:
            raise RuntimeError("Blueprint manifest does not contain eleven active IDs.")
        if sum(entry.get("status") == "reserved" for entry in entries) != 1:
            raise RuntimeError("Blueprint manifest does not contain one reserved ID.")
        for entry in entries:
            if re.fullmatch(r"[0-9a-f]{32}", entry.get("guid", "")) is None:
                raise RuntimeError("Blueprint manifest contains an invalid GUID.")

        dll = archive.read("KingmakerGunslinger/KingmakerGunslinger.dll")
        actual_dll_hash = sha256_bytes(dll)
        if actual_dll_hash != args.expected_dll_sha256:
            raise RuntimeError(
                f"DLL hash mismatch: expected {args.expected_dll_sha256}, observed {actual_dll_hash}"
            )
        binaries = [name for name in names if Path(name).suffix.lower() in {".dll", ".exe", ".pdb", ".mdb"}]
        if binaries != ["KingmakerGunslinger/KingmakerGunslinger.dll"]:
            raise RuntimeError(f"Unexpected binary entries: {binaries}")
        if archive.testzip() is not None:
            raise RuntimeError("Archive CRC validation failed.")

    print("Sprint 22 UMM package validation passed.")
    print(f"Archive SHA-256: {sha256(args.archive)}")
    print(f"DLL SHA-256: {args.expected_dll_sha256}")
    print("Entries: 8; binaries: project-owned KingmakerGunslinger.dll only.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
