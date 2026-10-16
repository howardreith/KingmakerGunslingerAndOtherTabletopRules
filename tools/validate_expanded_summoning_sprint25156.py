"""Sprint 25 giant-body source and append-only identity boundary.

Three reviewable states, exactly as the Sprint 24 boundary had: the released
Sprint 24 prefix plus the bounded Sprint 25 census, a registered hidden
candidate, and the final public candidate. Registration allocates every ID at
once; publication may remove only the one suppression key.

Two Sprint 25 facts this validator exists to enforce:

* Brachiosaurus is Summon Nature's Ally VII only. It has no Monster row and no
  celestial or fiendish execution children, so a registered candidate that
  grew Monster placements or template children is refused here rather than
  discovered in a native run. The owner settled that placement on 2026-10-15
  against a recorded source discrepancy.
* Elephant is an already-released creature. It is completed, never
  re-registered and never suppressed, so its appearance in the withheld set is
  refused outright.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
import validate_expanded_summoning_sprint24154 as prior

# The released Sprint 24 master: v0.0.155 published and verified, with its
# records and release tooling merged. The product source here is identical to
# the release merge 31672515; this commit simply also carries the records, so
# they are baseline rather than "unrelated".
BASELINE = "97c881ea847a5d3703982ed6b0aebc0e0537c7c5"
SPRINT25_KEY = "expandedSummoningSprint25156"
WITHHELD = {"brachiosaurus"}
RELEASED_NEVER_SUPPRESSED = {"elephant", "mastodon"}

GUIDS = {
    "KMG.Summoning.Unit.Brachiosaurus":
        "9093aef39ecc416092ef68655a7e56fb",
    "KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One":
        "5ff406f72e834d868276c43786a05133",
    "KMG.Summoning.Ability.SNA.Tier8.Brachiosaurus.OneD3":
        "7ab83d22b4284fbe8ab038776d443507",
    "KMG.Summoning.Ability.SNA.Tier9.Brachiosaurus.OneD4PlusOne":
        "10a645ecd39f41d88fc5ea0018e5210e",
    "KMG.Summoning.Natural.Brachiosaurus.Tail4d6":
        "13b51070d6f84a15bfed98a04c98fa30",
    "KMG.Summoning.Natural.Brachiosaurus.TailType":
        "5aaf157b2f5b489390f241bd9ec2decf",
    "KMG.Summoning.Natural.Brachiosaurus.UnitType":
        "1decf85e99fd4b6d9a7f68d2b5c9241e",
    "KMG.Summoning.Natural.Brachiosaurus.Chassis":
        "63280e8f08c94c3a9b67787ec2d7ab71",
    "KMG.Summoning.Special.Brachiosaurus.Trample":
        "309ec3dedcf04c15b3011933f5eafef4",
}


def planned_type(symbol: str) -> str:
    if symbol.startswith("KMG.Summoning.Unit."):
        return "BlueprintUnit"
    if symbol.startswith("KMG.Summoning.Ability."):
        return "BlueprintAbility"
    if symbol.endswith("Trample"):
        return "BlueprintAbility"
    if symbol.endswith("Tail4d6"):
        return "BlueprintItemWeapon"
    if symbol.endswith("TailType"):
        return "BlueprintWeaponType"
    if symbol.endswith("UnitType"):
        return "BlueprintUnitType"
    return "BlueprintFeature"


ADDED = {symbol: (guid, planned_type(symbol)) for symbol, guid in GUIDS.items()}

# A source-ownership boundary, not a claim that the listed files already
# qualify. Additions must be exact Sprint 25 seams and stay reviewable here.
ALLOWED_PRODUCTION = {
    "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
    # Every sprint stamps its own version here.
    "src/KingmakerGunslinger/Properties/AssemblyInfo.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/GiantBodyRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
}

# Original project-authored bodies and icons, pinned once authored. A public
# candidate must declare and carry them; a registered candidate is allowed to
# say truthfully that they do not exist yet.
ART: dict[str, str] = {}


def validate_append(accepted, current):
    if current[:len(accepted)] != accepted:
        raise AssertionError("A released identity or manifest row changed")
    tail = current[len(accepted):]
    if not tail:
        return "census"
    observed = {row["symbol"]: (row["guid"], row["plannedType"])
                for row in tail}
    if observed != ADDED or len(tail) != len(ADDED):
        raise AssertionError(
            "Sprint 25 must append exactly its %d pinned identities" % len(ADDED))
    if any(row.get("status") != "active" for row in tail):
        raise AssertionError("Every Sprint 25 identity must be active")
    if len({row["guid"] for row in current}) != len(current):
        raise AssertionError("Duplicate blueprint GUID")
    # No Monster row means no template execution children anywhere in the tail.
    if any(symbol.endswith(".Celestial") or symbol.endswith(".Fiendish")
           for symbol in observed):
        raise AssertionError(
            "Brachiosaurus is untemplated and cannot own execution children")
    if any(".Ability.SM." in symbol for symbol in observed):
        raise AssertionError(
            "Brachiosaurus has no Summon Monster row")
    return "registered"


def suppressed_keys(source: str):
    match = re.search(
        r"SuppressedCreatureKeys\s*=\s*\n?\s*new HashSet<string>\((.*?)\);",
        source, re.S)
    if match is None:
        raise AssertionError("Cannot read summon suppression source")
    return set(re.findall(r'"([a-z0-9-]+)"', match.group(1)))


def validate_visibility(source: str, identity_stage: str):
    keys = suppressed_keys(source)
    if keys & RELEASED_NEVER_SUPPRESSED:
        raise AssertionError(
            "A released Elephant or Mastodon route cannot be suppressed")
    if identity_stage == "census":
        if keys & WITHHELD:
            raise AssertionError("Unregistered Sprint 25 creature is suppressed")
        return "census"
    if keys == WITHHELD:
        return "hidden"
    if not keys:
        return "public"
    raise AssertionError(
        "Publication may remove only the one Sprint 25 suppression key")


def validate_production_scope(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", BASELINE, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    changed += subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--",
         "src/KingmakerGunslinger"], cwd=root, text=True).splitlines()
    unexpected = sorted(set(changed) - ALLOWED_PRODUCTION)
    if unexpected:
        raise AssertionError("Unrelated production source changed: " +
                             ", ".join(unexpected))


def validate_static(root: Path, stage: str):
    static = common.document(root, "validation/static-validation.json")
    released = json.loads(common.blob(
        root, BASELINE, "validation/static-validation.json"))
    for key, value in released.items():
        if key not in ("version", "milestone") and static.get(key) != value:
            raise AssertionError("Historical release evidence changed: " + key)
    record = static.get(SPRINT25_KEY)
    if stage == "census":
        if record is not None:
            raise AssertionError(
                "Census state cannot claim a registered Sprint 25 candidate")
        return
    if not isinstance(record, dict):
        raise AssertionError(
            "Registered Sprint 25 candidate lacks static metadata")
    expected = {
        "releaseVersion": "0.0.156",
        "releaseInformationalVersion":
            "0.0.156-expanded-summoning-sprint25-giant-bodies",
        "masterBase": BASELINE,
        "newCreatures": 1,
        "newLogicalRoots": 3,
        "newMonsterPlacements": 0,
        "newTemplateExecutionChildren": 0,
        "newPrivateIdentities": 5,
        "completedExistingCreatures": 1,
        "completedExistingRoots": 8,
        "registeredLogicalPlacements": 1096,
        "suppressedLogicalPlacements": 3 if stage == "hidden" else 0,
        "publishedLogicalPlacements": 1093 if stage == "hidden" else 1096,
        "retainedNativeWrappers": 29,
        "visibleChoiceTotal": 1122 if stage == "hidden" else 1125,
        "candidateOnly": True,
        "ownerAestheticApproval": "NOT_PERFORMED_NONBLOCKING",
        "sprint26Started": False,
    }
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Sprint 25 static metadata mismatch: " + key)
    authored = record.get("originalBodyAssetsAuthored")
    if not isinstance(authored, bool):
        raise AssertionError(
            "Sprint 25 must state whether its original bodies are authored")
    files = record.get("originalBodyFiles")
    if not isinstance(files, int) or files < 0:
        raise AssertionError("Sprint 25 original body file count is invalid")
    if authored != (files > 0):
        raise AssertionError(
            "Sprint 25 original body claim disagrees with its own file count")
    if authored != bool(ART):
        raise AssertionError(
            "Sprint 25 original body claim disagrees with its pinned art set")
    if stage == "public" and not authored:
        raise AssertionError(
            "Publication requires the original project-authored bodies")
    return authored


def validate_art(root: Path, stage: str, authored: bool):
    if stage == "census" or not authored:
        return
    for relative, expected in ART.items():
        path = root / relative
        if not path.is_file():
            raise AssertionError("Missing Sprint 25 original asset: " + relative)
        observed = hashlib.sha256(path.read_bytes()).hexdigest()
        if observed != expected:
            raise AssertionError(
                "Sprint 25 original asset hash changed: " + relative)
    for token in ("sprint25-giant-bodies", "brachiosaurus"):
        for script in ("scripts/Build-Local.ps1", "scripts/package.ps1",
                       "scripts/validate-build-output.ps1",
                       "scripts/validate-package.ps1"):
            if token not in (root / script).read_text(encoding="utf-8-sig"):
                raise AssertionError(script +
                                     " lacks package owner token: " + token)


def validate_version(root: Path):
    info = common.document(root, "Info.json")
    version = info.get("Version")
    if version != "0.0.156":
        raise AssertionError(
            "Unsupported Sprint 25 lineage version: " + repr(version))
    informational = "0.0.156-expanded-summoning-sprint25-giant-bodies"
    props = (root / "Directory.Build.props").read_text(encoding="utf-8-sig")
    assembly = (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs").read_text(
        encoding="utf-8-sig")
    if "<KmgVersion>" + version + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + informational + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + version + '")' not in assembly or \
            'AssemblyFileVersion("' + version + '")' not in assembly or \
            'AssemblyInformationalVersion("' + informational + '")' not in assembly:
        raise AssertionError("Active Sprint 25 version metadata mismatch")
    notes = root / ("docs/RELEASE-NOTES-" + version + ".md")
    if not notes.is_file():
        raise AssertionError("Sprint 25 release notes are missing")
    return version, informational


def validate(root: Path):
    accepted = json.loads(common.blob(
        root, BASELINE, "blueprints/blueprints.json"))["entries"]
    current = common.document(root, "blueprints/blueprints.json")["entries"]
    identity_stage = validate_append(accepted, current)
    visibility_source = (root / "src/KingmakerGunslinger/Summoning/"
                         "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    stage = validate_visibility(visibility_source, identity_stage)
    validate_production_scope(root)
    authored = validate_static(root, stage)
    validate_art(root, stage, bool(authored))
    version, informational = validate_version(root)

    if stage == "census":
        # Retain every released Sprint 24 check while the manifest is still
        # exactly that release. Its allowlist admits only the bounded census.
        prior.validate(root)

    manifest = subprocess.run(
        [sys.executable, str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    print("Sprint25 source PASS: stage=" + stage + "; identities=" +
          str(len(current)) + "; version=" + version + "; milestone=" +
          informational + "; bodiesAuthored=" + str(bool(authored)) + ".")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except (AssertionError, ValueError, KeyError,
            subprocess.CalledProcessError) as error:
        print("Sprint25 source FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
