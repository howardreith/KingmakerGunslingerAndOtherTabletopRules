"""Sprint 28 public source boundary; native/release proof remains separate."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import ENTRY, parsed_creatures, suppressed_keys

BASELINE = "dc9b83c71ff61f14a43d9cdabff37822796f43f2"
TAIL_COUNT = 63
TAIL_SHA256 = "58109ec4f6364fca7f2536f01cac9f53759757b248f931c93f60ff1ac4b98c03"
EXPECTED_GIANTS = {("hill-giant", None, False, 6),
                   ("stone-giant", None, False, 6),
                   ("fire-giant", None, False, 7)}
ART = {
    "assets-source/original-icons/expanded-summoning/sources/hill-giant.png": "4ea37f9c964b93d2bbfb0970d807068814be05678b6f378ea9f2e78210733545",
    "assets-source/original-icons/expanded-summoning/sources/stone-giant.png": "813ef8cba7164209ed9be07a52fce7d1cde8edabffe2b74b36c32bcf273e735e",
    "assets-source/original-icons/expanded-summoning/sources/fire-giant.png": "73a6bbda46ea635adf1ab56ab50048ad833ad58e063ab9091a648db20601d3cc",
    "assets/game/icons/expanded-summoning/hill-giant.png": "fbb909c017ea447566cb167e5e6dd0d918491950c65cc9c187e4f57d59dd5478",
    "assets/game/icons/expanded-summoning/stone-giant.png": "fa89836b7e34621bfbc8bd09363acc8538b12078db6483152e863edc122da558",
    "assets/game/icons/expanded-summoning/fire-giant.png": "dd4866cec136b98caf0bea44f87787de85abc7c0d601ec4c065dceefb88b543e",
}
ALLOWED_PRODUCTION = set("""src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.cs
src/KingmakerGunslinger/Blueprints/ExpandedSummoningIconBuilder.cs
src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Giants.cs
src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs
src/KingmakerGunslinger/KingmakerGunslinger.csproj
src/KingmakerGunslinger/Properties/AssemblyInfo.cs
src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs
src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningRosterPersistencePolicy.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint22Review.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint25Review.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Ai.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Catch.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Cleave.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Commands.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Disarm.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Inputs.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Modes.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Ownership.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Review.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs
src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs
src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs
src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs
src/KingmakerGunslinger/Summoning/GiantHumanoidAttackComponents.cs
src/KingmakerGunslinger/Summoning/GiantHumanoidCombatComponents.cs
src/KingmakerGunslinger/Summoning/GiantHumanoidRulesPolicy.cs
src/KingmakerGunslinger/Summoning/SummonGiantCleaveDelivery.cs
src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs
src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs""".splitlines())


def rows(text):
    return [(key, int(monster) if monster != "null" else None,
             templated == "true", int(ally) if ally != "null" else None)
            for key, name, monster, templated, ally, visual in ENTRY.findall(text)]


def check_scope(released, current, hidden):
    if current[:len(released)] != released:
        raise AssertionError("Released creature rows/order changed")
    if set(current[len(released):]) != EXPECTED_GIANTS or len(current) != len(released) + 3:
        raise AssertionError("Only the three adult untemplated SNA giant rows are authorized")
    if hidden:
        raise AssertionError("The public candidate still withholds a creature")
    return "public"


def check_tail(released, current):
    identities.check_append_only(released, current)
    tail = current[len(released):]
    digest = hashlib.sha256(json.dumps(tail, sort_keys=True,
        separators=(",", ":")).encode()).hexdigest()
    if len(tail) != TAIL_COUNT or digest != TAIL_SHA256 or any(
            row["status"] != "active" for row in tail):
        raise AssertionError("Exact Sprint 28 append-only identities changed")


def validate(root: Path):
    read = lambda path: (root / path).read_text(encoding="utf-8-sig")
    blob = lambda path: common.blob(root, BASELINE, path)
    if json.loads(read("Info.json"))["Version"] != "0.0.162":
        raise AssertionError("Wrong Sprint 28 stable patch")
    for name, path, pattern in identities.VERSION_FIELDS:
        expected = ("0.0.162-expanded-summoning-sprint28-giants"
                    if "Informational" in name else "0.0.162")
        if re.findall(pattern, read(path)) != [expected]:
            raise AssertionError("Version differs: " + name)
    current = json.loads(read("blueprints/blueprints.json"))
    released = json.loads(blob("blueprints/blueprints.json"))
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current[key] != released[key]:
            raise AssertionError("Identity ledger header changed: " + key)
    check_tail(released["entries"], current["entries"])
    catalog = "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
    check_scope(rows(blob(catalog).decode("utf-8-sig")), rows(read(catalog)),
                suppressed_keys())
    creatures = parsed_creatures()
    if len(creatures) != 114 or sum(c["monster"] is not None for c in creatures) != 96 or \
            sum(10-c["monster"] for c in creatures if c["monster"] is not None) != 545 or \
            sum(c["ally"] is not None for c in creatures) != 101 or \
            sum(10-c["ally"] for c in creatures if c["ally"] is not None) != 574:
        raise AssertionError("Sprint 28 public census differs")
    changed = identities.git(root, "diff", "--name-only", BASELINE, "--",
                             "src/KingmakerGunslinger").stdout.splitlines()
    changed += identities.git(root, "ls-files", "--others", "--exclude-standard",
                              "--", "src/KingmakerGunslinger").stdout.splitlines()
    if set(changed) - ALLOWED_PRODUCTION:
        raise AssertionError("Unrelated production edit: " +
                             str(set(changed) - ALLOWED_PRODUCTION))
    for path in identities.git(root, "ls-tree", "-r", "--name-only", BASELINE,
                               "--", "assets", "assets-source").stdout.splitlines():
        if path.lower().endswith((".png", ".jpg", ".jpeg", ".mesh", "-mesh.json")) \
                and (root / path).read_bytes() != blob(path):
            raise AssertionError("Released art changed: " + path)
    for path, digest in ART.items():
        if hashlib.sha256((root / path).read_bytes()).hexdigest() != digest:
            raise AssertionError("Giant icon bytes changed: " + path)
    static = json.loads(read("validation/static-validation.json"))
    prior = json.loads(blob("validation/static-validation.json"))
    for key, value in prior.items():
        if key not in ("version", "milestone") and static.get(key) != value:
            raise AssertionError("Historical static record changed: " + key)
    record = static.get("expandedSummoningSprint28162", {})
    if static.get("version") != "0.0.162" or record.get("stage") != "public" or \
            record.get("candidateOnly") is not True or \
            record.get("ownerAestheticApproval") != "NOT_PERFORMED_NONBLOCKING":
        raise AssertionError("Truthful source-only Sprint 28 record absent")
    result = subprocess.run([sys.executable, "-B",
        str(root / "tools/expanded_summoning_manifest.py")], cwd=root)
    if result.returncode:
        raise AssertionError("Controlled manifest/catalog reconciliation failed")
    print("Sprint28 public source boundary PASS: three adult SNA giants, eleven roots, 63 append-only identities; native evidence is separate.")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
        return 0
    except (AssertionError, ValueError, OSError, KeyError) as error:
        print(str(error), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
