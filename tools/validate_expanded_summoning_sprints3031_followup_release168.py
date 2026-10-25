"""v0.0.168 Sprints 30-31 follow-up release on released v0.0.167.

Relative to released v0.0.167 master (after its release record, PR #73) the
release changes no identity: the released ledger is unchanged. It changes only
the pinned production files and the version metadata; leaves the project file
exactly as released; modifies or adds only the pinned Lemure, Dretch and Hound
Archon exports and their source art, with their reviewed bytes; and adds only
its own record to the static evidence. The released public roster and
1,179-choice census still hold. The historical sprint and release validators
are left as they were.
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

# Released v0.0.167 master, after its release record (PR #73).
RELEASED = "48e6fb4abfc154f111a4019a806cb79a4184bd1c"
VERSION = "0.0.168"
INFORMATIONAL = "0.0.168-expanded-summoning-sprints30-31-followup"
RECORD_KEY = "expandedSummoningSprints3031FollowupRelease168"

RELEASE_PRODUCTION = {
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Astral31.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Hound30.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Trumpet31.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningAstral31Core.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningHound30Combat.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprints3031Circles.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprints3031Defenses.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprints30And31Inputs.cs",
    "src/KingmakerGunslinger/RuntimeTesting/VisualRefinementReviewPolicy.cs",
    "src/KingmakerGunslinger/Summoning/AstralDevaComponents.cs",
    "src/KingmakerGunslinger/Summoning/DretchVisualPolicy.cs",
    "src/KingmakerGunslinger/Summoning/HoundArchonSwordView.cs",
    "src/KingmakerGunslinger/Summoning/HoundArchonVisualPolicy.cs",
    "src/KingmakerGunslinger/Summoning/LemureVisualPolicy.cs",
}
VERSION_PRODUCTION = {"src/KingmakerGunslinger/Properties/AssemblyInfo.cs"}
PROJECT = "src/KingmakerGunslinger/KingmakerGunslinger.csproj"

# The rebuilt Lemure, Dretch and Hound Archon exports and source art, pinned
# to their reviewed bytes.
ART_MODIFIED = {
    "assets-source/original-models/outsider-celestial/SOURCE.md":
        "b4c8979464dd6022c48e35134e00699928d33a2c1feeb53c1c7674052a7d1722",
    "assets-source/original-models/outsider-celestial/build_dretch.py":
        "3bca5dbca69e83c6f9b69d6640daf1209f91f0135b643f21978eb29748bb45e0",
    "assets-source/original-models/outsider-celestial/build_hound_archon.py":
        "d7f6ac95d6ba231ea36d5db5805cd4dea70fbe423d07e6571f671d6f3d7ea727",
    "assets-source/original-models/outsider-celestial/build_lemure.py":
        "3703c467d6ed81e52a0e0427b416896303af2cb455711adc7943a47299b7d29b",
    "assets-source/original-models/outsider-celestial/dretch-source.json":
        "109c53f9c6316de3f6861309d77aaa8164779149638a998efdc0c045133cc832",
    "assets-source/original-models/outsider-celestial/hound-archon-source.json":
        "c8fe42df894429227f3894b6b8760ccb66a61e9011ed9c30449af8cfa438ba2e",
    "assets-source/original-models/outsider-celestial/lemure-source.json":
        "1734b47d00910ded5022595a8ae1627a50a9231f0fa6c39c6f4c7156b4580208",
    "assets/outsider-celestial/dretch-albedo.png":
        "d9d12d2f7077d66b9ecad9de793d7dfed57f30dcf2167bb66c39c847072c6b42",
    "assets/outsider-celestial/dretch-mesh.json":
        "016f3ce58f008f93dae835c8de146427f91c3cee526ecc6cdd2ec988fc3eb52d",
    "assets/outsider-celestial/hound-archon-albedo.png":
        "5bb8806bbffa8ada24f3107b413863b845124006c14b8c9181f1374005ed8433",
    "assets/outsider-celestial/hound-archon-mesh.json":
        "f7cb0529686aee779dd76cd00e7514598d733e01db58f7b57c442c5bbfef482e",
    "assets/outsider-celestial/lemure-albedo.png":
        "86d4718ab825096391f8561d7836a8332ffbbaaf4b565a9cbfcae57b52f0568a",
    "assets/outsider-celestial/lemure-mesh.json":
        "cf727788440c8796f99eeb04d73d29339dbaa80f2914dc585a8bed40212380e4",
}
ART_ADDED = {
    "assets-source/original-models/outsider-celestial/sdf_sculpt.py":
        "2dd16a84e5248361eb3ee700127ab711cd8f01c590815bbeaae1e42f4b85d345",
}
ART = dict(ART_MODIFIED, **ART_ADDED)

RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.167",
    "change": "The Astral Deva protective aura works as a lesser globe of "
              "invulnerability and extends its circle protection to 20 feet, the "
              "Trumpet Archon carries the ordinary Airborne fact, the Lemure, "
              "Dretch and Hound Archon bodies are rebuilt and the Hound holds its "
              "sword in its fist",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "modifiedAssets": len(ART_MODIFIED),
    "addedAssets": len(ART_ADDED),
    "productionFiles": len(RELEASE_PRODUCTION),
    "addedProjectLines": 0,
    "visibleChoiceTotal": 1179,
    "guardedNativeReviewed": True,
    "ownerVisualReview": "release authorized by the owner on 2026-10-25",
}


def check_identities(released_entries, current_entries):
    """The released ledger, exactly: this release appends nothing."""
    appended = identities.check_append_only(released_entries, current_entries)
    if appended != 0:
        raise AssertionError("v0.0.168 appends no identity, observed " + str(appended))


def check_project(released_text, current_text):
    """This release compiles exactly the released project."""
    if current_text.splitlines() != released_text.splitlines():
        raise AssertionError("The project changed; v0.0.168 adds no compile item or reference")


def check_production_scope(changed):
    unexpected = sorted(set(changed) - RELEASE_PRODUCTION - VERSION_PRODUCTION - {PROJECT})
    if unexpected:
        raise AssertionError("Production source outside the v0.0.168 release changed: " +
                             ", ".join(unexpected))


def check_art(changed, digest):
    """changed maps each asset/source-art path that differs from the release to
    its git status letter; digest returns a path's SHA-256."""
    unexpected = sorted(path for path in changed if path not in ART)
    if unexpected:
        raise AssertionError("Asset changed outside the v0.0.168 pins: " +
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
    if len(creatures) != 122 or sum(c["monster"] is not None for c in creatures) != 101 or \
            monster != 568 or sum(c["ally"] is not None for c in creatures) != 104 or \
            ally != 582 or monster + ally + len(NATIVE_EXPANDED_OPTIONS) != 1179:
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
        raise AssertionError("v0.0.168 release static record mismatch")


def check_version(info_version, props, assembly):
    if info_version != VERSION:
        raise AssertionError("Unsupported v0.0.168 release version: " + repr(info_version))
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL + '")' not in assembly:
        raise AssertionError("Active v0.0.168 release version metadata mismatch")


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
    print("Sprints 30-31 follow-up release PASS: identities=" + str(len(current)) +
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
        print("Sprints 30-31 follow-up release FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
