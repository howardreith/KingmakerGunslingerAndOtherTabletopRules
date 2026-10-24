"""v0.0.166 Druid rows and giant sizes release on released v0.0.165.

Relative to released v0.0.165 master (after its release record, PR #70) the
release changes no identity: the released ledger is unchanged. It changes only
the pinned production files and the version metadata; leaves the project file
exactly as released; changes no asset or source art; and adds only its own
record to the static evidence. The released public roster and choice census
still hold. The historical sprint and release validators are left as they
were.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import NATIVE_EXPANDED_OPTIONS, parsed_creatures, suppressed_keys

# Released v0.0.165 master, after its release record (PR #70).
RELEASED = "00ef53dd821ad92f10365b6aaab2e9c5ee9d93a2"
VERSION = "0.0.166"
INFORMATIONAL = "0.0.166-expanded-summoning-druid-giant-sizes"
RECORD_KEY = "expandedSummoningDruidGiantSizesRelease166"

RELEASE_PRODUCTION = {
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2GiantsOff.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2MenuAdapters.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2Review.cs",
    "src/KingmakerGunslinger/Summoning/DruidSummonConversionPolicy.cs",
    "src/KingmakerGunslinger/Summoning/DruidSummonConversionVariantPatch.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPresentationPatch.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPresentationPolicy.cs",
}
VERSION_PRODUCTION = {"src/KingmakerGunslinger/Properties/AssemblyInfo.cs"}
PROJECT = "src/KingmakerGunslinger/KingmakerGunslinger.csproj"

# This release changes no asset or source art.
ART_MODIFIED = {}
ART_ADDED = {}
ART = dict(ART_MODIFIED, **ART_ADDED)

RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.165",
    "change": "A Druid's prepared Summon Nature's Ally lists each creature once "
              "instead of twice, and the summoned giants keep their family heights "
              "in ordinary play on the view's own scale, with a Huge space for the "
              "Cloud and Storm Giants",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "modifiedAssets": len(ART_MODIFIED),
    "addedAssets": len(ART_ADDED),
    "productionFiles": len(RELEASE_PRODUCTION),
    "addedProjectLines": 0,
    "visibleChoiceTotal": 1156,
    "guardedNativeReviewed": True,
    "ownerVisualReview": "release authorized by the owner on 2026-10-24",
}


def check_identities(released_entries, current_entries):
    """The released ledger, exactly: this release appends nothing."""
    appended = identities.check_append_only(released_entries, current_entries)
    if appended != 0:
        raise AssertionError("v0.0.166 appends no identity, observed " + str(appended))


def check_project(released_text, current_text):
    """This release compiles exactly the released project."""
    if current_text.splitlines() != released_text.splitlines():
        raise AssertionError("The project changed; v0.0.166 adds no compile item or reference")


def check_production_scope(changed):
    unexpected = sorted(set(changed) - RELEASE_PRODUCTION - VERSION_PRODUCTION - {PROJECT})
    if unexpected:
        raise AssertionError("Production source outside the v0.0.166 release changed: " +
                             ", ".join(unexpected))


def check_art(changed, digest):
    """changed maps each asset/source-art path that differs from the release to
    its git status letter; digest returns a path's SHA-256."""
    unexpected = sorted(path for path in changed if path not in ART)
    if unexpected:
        raise AssertionError("Asset changed although this release changes none: " +
                             ", ".join(unexpected[:5]))
    wrong = sorted(path for path, status in changed.items()
                   if status != ("M" if path in ART_MODIFIED else "A"))
    if wrong:
        raise AssertionError("Assets may only be modified in place or added as pinned: " +
                             ", ".join(wrong[:5]))
    missing = sorted(path for path in ART if path not in changed)
    if missing:
        raise AssertionError("Pinned asset is absent or still the released file: " +
                             ", ".join(missing[:5]))
    for path, expected in sorted(ART.items()):
        if digest(path) != expected:
            raise AssertionError("Pinned asset differs from its reviewed bytes: " + path)


def check_census(creatures, hidden):
    """The released public roster and choice census, unchanged."""
    if hidden:
        raise AssertionError("The release withholds a released creature")
    monster = sum(10 - c["monster"] for c in creatures if c["monster"] is not None)
    ally = sum(10 - c["ally"] for c in creatures if c["ally"] is not None)
    if len(creatures) != 117 or sum(c["monster"] is not None for c in creatures) != 96 or \
            monster != 545 or sum(c["ally"] is not None for c in creatures) != 104 or \
            ally != 582 or monster + ally + len(NATIVE_EXPANDED_OPTIONS) != 1156:
        raise AssertionError("Released public census differs")


def check_static(released, current):
    for key in set(released) | set(current):
        if key in ("version", "milestone", RECORD_KEY):
            continue
        if released.get(key) != current.get(key):
            raise AssertionError("Released static evidence changed: " + key)
    if current.get("version") != VERSION or current.get("milestone") != INFORMATIONAL:
        raise AssertionError("Static record version metadata mismatch")
    if current.get(RECORD_KEY) != RECORD:
        raise AssertionError("v0.0.166 release static record mismatch")


def check_version(info_version, props, assembly):
    if info_version != VERSION:
        raise AssertionError("Unsupported v0.0.166 release version: " + repr(info_version))
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL + '")' not in assembly:
        raise AssertionError("Active v0.0.166 release version metadata mismatch")


def changed_production(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", RELEASED, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    changed += subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--",
         "src/KingmakerGunslinger"], cwd=root, text=True).splitlines()
    return changed


def changed_assets(root: Path):
    changed = {}
    for line in subprocess.check_output(
            ["git", "diff", "--name-status", "--no-renames", RELEASED, "--",
             "assets", "assets-source"], cwd=root, text=True).splitlines():
        fields = line.split("\t")
        changed[fields[-1]] = fields[0][:1]
    for path in subprocess.check_output(
            ["git", "ls-files", "--others", "--exclude-standard", "--",
             "assets", "assets-source"], cwd=root, text=True).splitlines():
        changed[path] = "A"
    return changed


def validate(root: Path):
    released_ledger = json.loads(common.blob(root, RELEASED, "blueprints/blueprints.json"))
    current_ledger = common.document(root, "blueprints/blueprints.json")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current_ledger.get(key) != released_ledger.get(key):
            raise AssertionError("The identity ledger header changed: " + key)
    current = current_ledger["entries"]
    check_identities(released_ledger["entries"], current)
    check_census(parsed_creatures(), suppressed_keys())
    check_production_scope(changed_production(root))
    check_project(common.blob(root, RELEASED, PROJECT).decode("utf-8-sig"),
                  (root / PROJECT).read_text(encoding="utf-8-sig"))
    check_art(changed_assets(root),
              lambda path: hashlib.sha256((root / path).read_bytes()).hexdigest())
    check_static(json.loads(common.blob(root, RELEASED, "validation/static-validation.json")),
                 common.document(root, "validation/static-validation.json"))
    check_version(common.document(root, "Info.json").get("Version"),
                  (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
                  (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
                  .read_text(encoding="utf-8-sig"))

    manifest = subprocess.run(
        [sys.executable, "-B", str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    print("Druid and giant size release PASS: identities=" + str(len(current)) +
          "; modifiedAssets=" + str(len(ART_MODIFIED)) + "; addedAssets=" + str(len(ART_ADDED)) +
          "; version=" + VERSION + "; milestone=" + INFORMATIONAL + ".")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Druid and giant size release FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
