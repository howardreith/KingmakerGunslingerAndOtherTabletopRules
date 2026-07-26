#!/usr/bin/env python3
"""Validate the strict standalone 0.0.29 Sprint 29 UMM ZIP."""
from __future__ import annotations

import argparse
import hashlib
import json
import sys
import zipfile
from pathlib import Path, PurePosixPath

ROOT = "KingmakerGunslinger"
EXPECTED = {
    f"{ROOT}/KingmakerGunslinger.dll",
    f"{ROOT}/Info.json",
    f"{ROOT}/CHANGELOG.md",
    f"{ROOT}/LICENSE",
    f"{ROOT}/README.md",
    f"{ROOT}/SMOKE-TEST-GUIDE.md",
    f"{ROOT}/blueprints/blueprints.json",
    f"{ROOT}/blueprints/blueprints.schema.json",
}


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("package", type=Path)
    parser.add_argument("--expected-dll-sha256")
    parser.add_argument("--expected-package-sha256")
    args = parser.parse_args()
    try:
        if not args.package.is_file():
            raise FileNotFoundError(args.package)
        package_bytes = args.package.read_bytes()
        package_sha = digest(package_bytes)
        if args.expected_package_sha256 and package_sha != args.expected_package_sha256.lower():
            raise RuntimeError("Package SHA-256 does not match the qualified value.")

        with zipfile.ZipFile(args.package) as archive:
            bad = archive.testzip()
            if bad:
                raise RuntimeError(f"ZIP CRC validation failed at {bad}.")
            names = archive.namelist()
            if len(names) != len(set(names)):
                raise RuntimeError("ZIP contains duplicate entries.")
            files = {name for name in names if not name.endswith("/")}
            if files != EXPECTED:
                raise RuntimeError(f"Unexpected standalone package layout: {sorted(files)}")
            for name in names:
                path = PurePosixPath(name)
                if path.is_absolute() or ".." in path.parts or "\\" in name:
                    raise RuntimeError(f"Unsafe ZIP path: {name}")

            binaries = [
                name
                for name in files
                if PurePosixPath(name).suffix.lower() in {".dll", ".exe", ".pdb", ".mdb"}
            ]
            if binaries != [f"{ROOT}/KingmakerGunslinger.dll"]:
                raise RuntimeError(f"Unexpected packaged binaries: {binaries}")
            dll = archive.read(f"{ROOT}/KingmakerGunslinger.dll")
            dll_sha = digest(dll)
            if args.expected_dll_sha256 and dll_sha != args.expected_dll_sha256.lower():
                raise RuntimeError("DLL SHA-256 does not match the qualified value.")

            info = json.loads(archive.read(f"{ROOT}/Info.json"))
            if info.get("Version") != "0.0.29":
                raise RuntimeError("Info.json does not declare version 0.0.29.")
            ledger = json.loads(archive.read(f"{ROOT}/blueprints/blueprints.json"))
            entries = {entry.get("symbol"): entry for entry in ledger.get("entries", [])}
            if len(entries) != 15:
                raise RuntimeError("Packaged blueprint ledger does not contain 15 unique entries.")
            expected = {
                "KMG.Test.FirearmRepairKitItem": "f2b564234b8a4b0d88a7a46128556bef",
                "KMG.Test.OverhaulAbility": "8a0ba821382640b58ec9ff168ed778a5",
                "KMG.Test.RepairAbility": "c914b3c0786463b7a1e17e47447ee5b1",
            }
            for symbol, guid in expected.items():
                entry = entries.get(symbol)
                if entry is None or entry.get("guid") != guid or entry.get("status") != "active":
                    raise RuntimeError(f"Packaged blueprint entry is incorrect: {symbol}")

            guide = archive.read(f"{ROOT}/SMOKE-TEST-GUIDE.md").decode("utf-8")
            for token in [
                "0.0.29-s29-complete-maintenance-loop",
                "Run the one-command transaction regression",
                "Verify Repair interruption",
                "stage=MaintenanceLoopPassed",
                "blocks Sprint 30",
            ]:
                if token not in guide:
                    raise RuntimeError(f"Packaged smoke guide is missing: {token}")
            readme = archive.read(f"{ROOT}/README.md").decode("utf-8")
            for token in [
                "Repair Test Musket",
                "Complete maintenance loop",
                "Accelerated qualification harness",
            ]:
                if token not in readme:
                    raise RuntimeError(f"Packaged README is missing: {token}")
            changelog = archive.read(f"{ROOT}/CHANGELOG.md").decode("utf-8")
            if "## 0.0.29" not in changelog or "599" not in changelog:
                raise RuntimeError("Packaged changelog does not describe Sprint 29.")

        print("Sprint 29 standalone UMM package validation passed.")
        print(
            json.dumps(
                {
                    "packageSha256": package_sha,
                    "dllSha256": dll_sha,
                    "entryCount": 8,
                    "binaryCount": 1,
                },
                sort_keys=True,
            )
        )
        return 0
    except Exception as exception:
        print(f"Sprint 29 package validation failed: {exception}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
