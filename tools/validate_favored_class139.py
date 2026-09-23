#!/usr/bin/env python3
"""Validate the 0.0.139 Favored Class integration candidate.

Retains every inherited gate. Mechanical acceptance is the domain suite and
guarded runtime evidence, not these documentation/metadata checks. This is an
unpublished local candidate: the metadata must say so, must keep the exact
qualified host profile, and must never claim native qualification that was not
observed.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_better_vendors138 as baseline

VERSION = "0.0.139"
INFORMATIONAL_VERSION = "0.0.139-favored-class-integration"
PACKAGE = "KingmakerGunslinger-0.0.139-local-runtime.zip"
PACKAGE_SUFFIX = "favored-class-integration"
DETERMINISTIC_TEST_COUNT = 1766
STATIC_KEY = "favoredClassIntegration139"

# Exact ordered (symbol, guid) pairs this candidate appends after the
# Better Vendors block.
APPENDED = ()

HOST_SHA256 = "dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c"
HOST_MVID = "3efd38e7-8682-4b4d-8d53-e368a3664919"
COTW_SHA256 = "4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915"
COTW_MVID = "8caab254-aacf-4811-8093-44b9184e6e53"


def validate(root: Path) -> None:
    baseline.AUTHORIZED_APPENDED_AFTER = APPENDED
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    # The host stays optional: no UMM requirement or compile-time reference.
    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    if info.get("Requirements") != [] or info.get("Version") != VERSION:
        raise AssertionError("Info.json must not require Favored Class or Call of the Wild")
    project = root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
    baseline.forbid_tokens(project, "ZFavoredClass.dll", "CallOfTheWild.dll",
        "<Reference Include=\"ZFavoredClass", "<Reference Include=\"CallOfTheWild")
    # The adapter reads the host by reflection and never executes host code:
    # no reflective invocation, no second Core.load(), no runtime identities.
    for source in (root / "src/KingmakerGunslinger/FavoredClass").rglob("*.cs"):
        baseline.forbid_tokens(source, "using ZFavoredClass", "using CallOfTheWild",
            "Core.load();", ".Invoke(", "Guid.NewGuid")
    baseline.require_tokens(
        root / "src/KingmakerGunslinger/FavoredClass/FavoredClassHostContract.cs",
        HOST_SHA256, HOST_MVID, COTW_SHA256, COTW_MVID,
        '"binary-sha256"', '"binary-mvid"', '"dependency-sha256"', '"dependency-mvid"',
        '"core-load-incomplete"', '"gunslinger-not-scanned"')
    baseline.require_tokens(
        root / "docs/FAVORED-CLASS-COMPATIBILITY.md",
        HOST_SHA256, HOST_MVID, COTW_SHA256, COTW_MVID,
        "N = fullRank + partialRank", "unrestricted", "outside")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if static.get("version") != VERSION or static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the 0.0.139 candidate")
    state = static[STATIC_KEY]
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": False,
        "candidateOnly": True,
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        "hostVerifiedVersion": "1.3.1",
        "hostVerifiedFileSha256": HOST_SHA256,
        "hostVerifiedMvid": HOST_MVID,
        "callOfTheWildVerifiedFileSha256": COTW_SHA256,
        "callOfTheWildVerifiedMvid": COTW_MVID,
        "hostRequired": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Favored Class candidate metadata mismatch: {key}")
    if not isinstance(state.get("compatibilityRuntimeQualificationPending"), bool):
        raise AssertionError("The compatibility qualification status must be recorded")
    if not isinstance(state.get("nativeRuntimeQualified"), bool):
        raise AssertionError("Native runtime qualification must be recorded explicitly")
    if state.get("nativeRuntimeQualified") and not state.get("nativeRuntimeEvidence"):
        raise AssertionError("A native qualification claim needs recorded evidence")

    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.139.md",
        INFORMATIONAL_VERSION, "Favored Class", "optional", "candidate",
        "not published", "uninstall")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Favored Class integration {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Favored Class integration {VERSION} validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
