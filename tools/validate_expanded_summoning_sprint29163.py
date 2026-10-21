"""Sprint 29 public source boundary; native and release proof are separate."""
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
from expanded_summoning_manifest import ENTRY, NATIVE_EXPANDED_OPTIONS, parsed_creatures, suppressed_keys

BASELINE = "f3e68aab30701df268ae559c2850159b7b3a52f7"
TAIL_COUNT = 84
TAIL_SHA256 = "ed47dc298f280b8d5b290eeae220e5a406be0e6ab40f24f05108049c1a03e60e"
EXPECTED_GIANTS = {
    ("ettin", None, False, 5),
    ("cloud-giant", None, False, 8),
    ("storm-giant", None, False, 9),
}
ART = {
    "assets-source/original-icons/expanded-summoning/sources/ettin.png": "10ea3f8184227c9dc3f4fa5454514a91f2a19fc693e419de74b0fa0b9e249b98",
    "assets-source/original-icons/expanded-summoning/sources/cloud-giant.png": "ff80e2f8068f2ef6537363499660f0723126c00cf20485bbf48b183205ffae9f",
    "assets-source/original-icons/expanded-summoning/sources/storm-giant.png": "b6d619772c2eb44b097975f1048beb7694e3dde7ee7ab52f80c5e0f0feeca5cc",
    "assets/game/icons/expanded-summoning/ettin.png": "230b7b479f28f94fae8f32e48f3d65ab358f8bda29d42b25a9bbccd40b92c900",
    "assets/game/icons/expanded-summoning/cloud-giant.png": "95df006858e71636d19e3a0db3dffc45696baaa4d44e231fb046cb4fbfbe4fba",
    "assets/game/icons/expanded-summoning/storm-giant.png": "94d21b78bb96a48a5816d55c1a2f4738830150b3214d47905b21c8544b5272e4",
}
ALLOWED_PRODUCTION = set("""src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.cs
src/KingmakerGunslinger/Blueprints/ExpandedSummoningIconBuilder.cs
src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Giants29.cs
src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs
src/KingmakerGunslinger/KingmakerGunslinger.csproj
src/KingmakerGunslinger/Properties/AssemblyInfo.cs
src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs
src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningRosterPersistencePolicy.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningResourceReceipts.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Review.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint25Review.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Catch.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint28Cleave.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Ai.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29AiNegative.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29AiTurns.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Awesome.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Commands.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Disarm.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Inputs.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Items.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Magic.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Review.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint29Vital.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs
src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs
src/KingmakerGunslinger/Summoning/EttinSecondHeadVisual.cs
src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs
src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs
src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs
src/KingmakerGunslinger/Summoning/GiantHumanoidAttackComponents.cs
src/KingmakerGunslinger/Summoning/GiantHumanoidCombatComponents.cs
src/KingmakerGunslinger/Summoning/GiantHumanoidRulesPolicy.cs
src/KingmakerGunslinger/Summoning/Sprint29AwesomeBlow.cs
src/KingmakerGunslinger/Summoning/Sprint29FogAiSafety.cs
src/KingmakerGunslinger/Summoning/Sprint29GiantSpellLikeParameters.cs
src/KingmakerGunslinger/Summoning/Sprint29LightningAiSafety.cs
src/KingmakerGunslinger/Summoning/Sprint29StormLightningBrainRefresh.cs
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
        raise AssertionError("Only the three adult untemplated Sprint 29 SNA giants are authorized")
    if hidden:
        raise AssertionError("The public candidate still withholds a creature")


def check_tail(released, current):
    identities.check_append_only(released, current)
    tail = current[len(released):]
    digest = hashlib.sha256(json.dumps(tail, sort_keys=True,
        separators=(",", ":")).encode()).hexdigest()
    if len(tail) != TAIL_COUNT or digest != TAIL_SHA256 or any(
            row["status"] != "active" for row in tail):
        raise AssertionError("Exact Sprint 29 append-only identities changed")


def validate(root: Path):
    read = lambda path: (root / path).read_text(encoding="utf-8-sig")
    blob = lambda path: common.blob(root, BASELINE, path)
    if json.loads(read("Info.json"))["Version"] != "0.0.163":
        raise AssertionError("Wrong Sprint 29 stable patch")
    for name, path, pattern in identities.VERSION_FIELDS:
        expected = ("0.0.163-expanded-summoning-sprint29-giants"
                    if "Informational" in name else "0.0.163")
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
    monster = sum(10-c["monster"] for c in creatures if c["monster"] is not None)
    ally = sum(10-c["ally"] for c in creatures if c["ally"] is not None)
    if len(creatures) != 117 or sum(c["monster"] is not None for c in creatures) != 96 or \
            monster != 545 or sum(c["ally"] is not None for c in creatures) != 104 or \
            ally != 582 or monster + ally + len(NATIVE_EXPANDED_OPTIONS) != 1156:
        raise AssertionError("Sprint 29 public source census differs")
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
            raise AssertionError("Sprint 29 icon bytes changed: " + path)
    static = json.loads(read("validation/static-validation.json"))
    prior = json.loads(blob("validation/static-validation.json"))
    for key, value in prior.items():
        if key not in ("version", "milestone") and static.get(key) != value:
            raise AssertionError("Historical static record changed: " + key)
    record = static.get("expandedSummoningSprint29163", {})
    if static.get("version") != "0.0.163" or record.get("stage") != "public" or \
            record.get("candidateOnly") is not True or \
            record.get("ownerAestheticApproval") != "NOT_PERFORMED_NONBLOCKING":
        raise AssertionError("Truthful source-only Sprint 29 record absent")
    result = subprocess.run([sys.executable, "-B",
        str(root / "tools/expanded_summoning_manifest.py")], cwd=root)
    if result.returncode:
        raise AssertionError("Controlled manifest/catalog reconciliation failed")
    print("Sprint29 public source boundary PASS: three adult SNA giants, eight public roots, 84 append-only identities; native evidence is separate.")


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
