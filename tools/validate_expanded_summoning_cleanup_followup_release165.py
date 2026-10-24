"""v0.0.165 summon cleanup follow-up release on released v0.0.164.

Relative to released v0.0.164 master (after its release record, PR #68) the
release changes no identity: the released ledger is unchanged. It changes only
the pinned follow-up production files and the version metadata; leaves the
project file exactly as released; modifies only the pinned assets and source
art, in place, to exactly their reviewed bytes; adds only the pinned source
art; and adds only its own record to the static evidence. The released public
roster and choice census still hold. The historical sprint and release
validators are left as they were.
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

# Released v0.0.164 master, after its release record (PR #68).
RELEASED = "114564ac71c5efbbf170963f7bc523ee0fcdd91d"
VERSION = "0.0.165"
INFORMATIONAL = "0.0.165-expanded-summoning-cleanup-followup"
RECORD_KEY = "expandedSummoningCleanupFollowupRelease165"

CLEANUP_PRODUCTION = {
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningPublication.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2MenuAdapters.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2Review.cs",
    "src/KingmakerGunslinger/Summoning/Cleanup2CreatureReviewRoster.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPresentationPatch.cs",
    "src/KingmakerGunslinger/Summoning/GiantSummonPresentationPolicy.cs",
}
VERSION_PRODUCTION = {"src/KingmakerGunslinger/Properties/AssemblyInfo.cs"}
PROJECT = "src/KingmakerGunslinger/KingmakerGunslinger.csproj"

# Released assets and source art this release modifies in place, with their
# reviewed bytes: the connected-arm Deinonychus body and paint, the painted
# Pony and Cyclops icons, the icon manifests and catalog, and the authoring
# sources and body verifier behind them.
ART_MODIFIED = {
    "assets-source/original-icons/expanded-summoning/icon-manifest.json":
        "9f4da34f398612e83223b7d9741194954189e6f06dd0585c465aa796cd5e34f6",
    "assets-source/original-icons/icon-catalog.json":
        "2ba164d9838171ac25602377737bccb22842a5eedbe24f1ea236bd9640675ff7",
    "assets-source/original-models/sprint21-arthropods/verify_cleanup2_bodies.py":
        "cc3989fd334dca4624f20f450c6b60a3b7fd4d8d3c9320adef2e924dcdd20294",
    "assets-source/original-models/sprint24-theropods/SOURCE.md":
        "34e6fce8dfa2303d23d3fd14353c2be0123c49cde46c13536c91de0d800a4209",
    "assets-source/original-models/sprint24-theropods/build_deinonychus.py":
        "086d6e633d5d5ae351ac6bdd1e2cb6c7e44a395736f087a98f90d2ccd310d3cf",
    "assets/game/icons/expanded-summoning/cyclops.png":
        "299ebec61241a634d2559e535b13625956ecaf3c661b0a33145ec546b87d2b4d",
    "assets/game/icons/expanded-summoning/icon-manifest.json":
        "26d796b1ae8c537d80bf7cd82966c4033c9a735574063b3d8e1ee8c37c95936f",
    "assets/game/icons/expanded-summoning/pony.png":
        "a6a310530057ea44e706774b635ce93b993050caf40d7046a867c0eb7563b8b9",
    "assets/sprint24-theropods/deinonychus-albedo.png":
        "a487d804338a5270715affba69d0e9012924b4454775ca1bfb4ae91d2d89aaac",
    "assets/sprint24-theropods/deinonychus-mesh.json":
        "012c1e6fc0a6a2029d6e14fc4bb5f4ed48f3bd8029c5f99a91f2337d1411c78c",
}
# Files this release adds: the original Pony and Cyclops paintings and
# their provenance notes.
ART_ADDED = {
    "assets-source/original-icons/expanded-summoning/edits/cyclops/EDIT.md":
        "9d79c8bed4d921c9876247fe0763a0f338b5a7b4fdf2ae1cf617abac1863a1d3",
    "assets-source/original-icons/expanded-summoning/edits/cyclops/cyclops.png":
        "2a878b2beab3c52c878993152a05bc2c50ee2b6e0fe7b4f941e39c0689b4d3b2",
    "assets-source/original-icons/expanded-summoning/edits/pony/EDIT.md":
        "19c0adcc9faf119a35e883d3c5ee6837a82ecc4816ae353b416bbf5af7b82296",
    "assets-source/original-icons/expanded-summoning/edits/pony/pony.png":
        "9513c71c5f769037624a18c8efa45f22e899c86a7fd7217f7ddb558129cf4545",
}
ART = dict(ART_MODIFIED, **ART_ADDED)

RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.164",
    "change": "Summon cleanup follow-up: the Deinonychus arms joined to its body, "
              "painted Pony and Cyclops icons, tier-I summon children linked to "
              "their parent so the Druid level-1 conversion lists creatures instead "
              "of the Summon Nature's Ally I row, and the Frost Giant prefab's frost "
              "effect dropped from the Fire and Storm Giants",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "modifiedAssets": len(ART_MODIFIED),
    "addedAssets": len(ART_ADDED),
    "productionFiles": len(CLEANUP_PRODUCTION),
    "addedProjectLines": 0,
    "visibleChoiceTotal": 1156,
    "guardedNativeReviewed": True,
    "ownerVisualReview": "release authorized by the owner on 2026-10-24",
}


def check_identities(released_entries, current_entries):
    """The released ledger, exactly: this release appends nothing."""
    appended = identities.check_append_only(released_entries, current_entries)
    if appended != 0:
        raise AssertionError("v0.0.165 appends no identity, observed " + str(appended))


def check_project(released_text, current_text):
    """This release compiles exactly the released project."""
    if current_text.splitlines() != released_text.splitlines():
        raise AssertionError("The project changed; v0.0.165 adds no compile item or reference")


def check_production_scope(changed):
    unexpected = sorted(set(changed) - CLEANUP_PRODUCTION - VERSION_PRODUCTION - {PROJECT})
    if unexpected:
        raise AssertionError("Production source outside the v0.0.165 release changed: " +
                             ", ".join(unexpected))


def check_art(changed, digest):
    """changed maps each asset/source-art path that differs from the release to
    its git status letter; digest returns a path's SHA-256."""
    unexpected = sorted(path for path in changed if path not in ART)
    if unexpected:
        raise AssertionError("Asset outside the reviewed follow-up changed: " +
                             ", ".join(unexpected[:5]))
    wrong = sorted(path for path, status in changed.items()
                   if status != ("M" if path in ART_MODIFIED else "A"))
    if wrong:
        raise AssertionError("Assets may only be modified in place or added as pinned: " +
                             ", ".join(wrong[:5]))
    missing = sorted(path for path in ART if path not in changed)
    if missing:
        raise AssertionError("Reviewed follow-up asset is absent or still the released file: " +
                             ", ".join(missing[:5]))
    for path, expected in sorted(ART.items()):
        if digest(path) != expected:
            raise AssertionError("Follow-up asset differs from its reviewed bytes: " + path)


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
        raise AssertionError("v0.0.165 release static record mismatch")


def check_version(info_version, props, assembly):
    if info_version != VERSION:
        raise AssertionError("Unsupported v0.0.165 release version: " + repr(info_version))
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL + '")' not in assembly:
        raise AssertionError("Active v0.0.165 release version metadata mismatch")


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
    print("Cleanup follow-up release PASS: identities=" + str(len(current)) +
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
        print("Cleanup follow-up release FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
