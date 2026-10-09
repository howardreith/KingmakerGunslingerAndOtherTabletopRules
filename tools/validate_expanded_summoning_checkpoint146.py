#!/usr/bin/env python3
"""Validate the reviewable union of released v145 and qualified Phase2B.

Runtime qualification is deliberately separate. Historical release records,
published master identities and both accepted production implementations must
survive integration; a new active record does not rewrite old qualification.
"""
from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_favored_class140 as inherited
import inspect_elemental_character_trait_icons as trait_icons

VERSION = "0.0.146"
INFORMATIONAL_VERSION = VERSION + "-expanded-summoning-phase2b-checkpoint"
MASTER = "bf8a1e41b308bfb148cb028d75a576555b7e8dd1"
SUMMONING = "c6effc3224754e1bdf250a703eb1440c1dbbfb78"
KEY = "expandedSummoningCheckpoint146"
MASTER_PROTECTED = (
    "src/KingmakerGunslinger/ElementalRaces/",
    "src/KingmakerGunslinger/FavoredClass/",
    "src/KingmakerGunslinger/Acquisition/",
    "src/KingmakerGunslinger/RuntimeTesting/GuardedDisposableSaveLease.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ElementalCharacterTrait",
    "docs/RELEASE-NOTES-0.0.142.md",
    "docs/RELEASE-NOTES-0.0.143.md",
    "docs/RELEASE-NOTES-0.0.144.md",
    "docs/RELEASE-NOTES-0.0.145.md",
    "src/KingmakerGunslinger/Blueprints/EasternWeaponCampaignBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/RareFirearmCampaignLootBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/HeirloomNodachiBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/MagicFirearmBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/EasternWeaponNamedBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearNamedBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearCampaignBlueprints.cs",
    "src/KingmakerGunslinger/CraftMagicItemsCompatibility/CraftMagicItemsRegistrationCatalog.cs",
    "validation/weapon-findability-",

)
SUMMONING_PROTECTED = (
    "src/KingmakerGunslinger/Summoning/",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoning",
    "assets-source/original-icons/expanded-summoning/",
    "assets/flying-animals/", "assets/sprint12-quadrupeds/",
    "assets/sprint13-creatures/", "assets/ungulates/",
    "assets/sprint14-insects/", "assets/sprint16-crocodilians/", "assets/sprint17-serpents/",
)

def blob(root, ref, path):
    return subprocess.check_output(["git", "-c", "core.longpaths=true", "show", ref + ":" + path], cwd=root)


def document(root, path):
    return json.loads((root / path).read_text(encoding="utf-8-sig"))


def preserved_files(root, ref, prefixes):
    paths = subprocess.check_output(["git", "-c", "core.longpaths=true", "ls-tree", "-r", "--name-only", ref],
                                    cwd=root, text=True).splitlines()
    selected = [p for p in paths if any(p.startswith(k) for k in prefixes)]
    if not selected:
        raise AssertionError("Empty protected production boundary")
    for path in selected:
        current = (root / path).read_bytes()
        # Git attributes may give PowerShell scripts CRLF, but these protected
        # surfaces are source/JSON/PNG. No semantic-only comparison admits art.
        accepted = blob(root, ref, path)
        if Path(path).suffix in {".cs", ".md", ".json"}:
            current, accepted = current.replace(b"\r\n", b"\n"), accepted.replace(b"\r\n", b"\n")
        if current != accepted:
            raise AssertionError("Accepted production changed: " + path)
    return len(selected)


def validate_identity_union(master, imported, current):
    expected = list(master["entries"])
    by_symbol = {e["symbol"]: e for e in expected}
    for entry in imported["entries"]:
        if entry["symbol"] in by_symbol:
            if by_symbol[entry["symbol"]] != entry:
                raise AssertionError("Shared identity/metadata differs: " + entry["symbol"])
        else:
            expected.append(entry)
    if current["entries"] != expected:
        raise AssertionError("Master prefix or imported append order/content drift")
    if len({e["guid"] for e in expected}) != len(expected) or len({e["symbol"] for e in expected}) != len(expected):
        raise AssertionError("Duplicate stable identity")
    if len(expected) != 2922 or sum(e["status"] == "active" for e in expected) != 2920:
        raise AssertionError("Integrated identity count drift")


def validate(root):
    master = json.loads(blob(root, MASTER, "blueprints/blueprints.json"))
    imported = json.loads(blob(root, SUMMONING, "blueprints/blueprints.json"))
    validate_identity_union(master, imported, document(root, "blueprints/blueprints.json"))
    protected_master = preserved_files(root, MASTER, MASTER_PROTECTED)
    protected_summons = preserved_files(root, SUMMONING, SUMMONING_PROTECTED)
    state = document(root, "validation/static-validation.json")
    old = json.loads(blob(root, MASTER, "validation/static-validation.json"))
    for key in ("expandedSummoningPhase2A141", "dataContentTraits142",
                "weaponFindability143", "weaponFindabilityRelease144", "heirloomNodachiIconRelease145"):
        if state[key] != old[key]:
            raise AssertionError("Historical release record changed: " + key)
    imported_state = json.loads(blob(root, SUMMONING, "validation/static-validation.json"))
    if state["expandedSummoningCheckpoint143"] != imported_state["expandedSummoningCheckpoint143"]:
        raise AssertionError("Historical unreleased checkpoint record changed")
    from weapon_findability_native_reference import production_targets
    targets = production_targets(root)
    master_targets = production_targets(root, lambda path: blob(root, MASTER, path).decode("utf-8-sig"))
    references = document(root, "validation/weapon-findability-native-reference.json")["weapons"]
    if targets != master_targets or [dict((k, row[k]) for k in targets[0]) for row in references] != targets:
        raise AssertionError("Released findability targets/native contracts changed")
    count = len(re.findall(r'\bCase\("', (root / "tests/KingmakerGunslinger.DomainTests/Program.cs").read_text(encoding="utf-8-sig")))
    record = state[KEY]
    expected = {"releaseVersion": VERSION, "releaseInformationalVersion": INFORMATIONAL_VERSION,
                "masterBase": MASTER, "qualifiedPhase2BImport": SUMMONING,
                "deterministicTestCount": count, "packageMemberCount": 333,
                "registeredManifestEntryCount": 2922, "publishedGeneratedPlacements": 1008,
                "retainedNativeWrappers": 29, "visibleChoiceTotal": 1037,
                "salamanderIncluded": True, "sprint18Started": False}
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Active checkpoint metadata differs: " + key)
    if record.get("runtimeQualified") and not record.get("runtimeEvidence"):
        raise AssertionError("Runtime qualification requires exact closure evidence")
    # The full inherited chain remains active. The v142-only forbidden
    # Summoning boundary is replaced, not bypassed, by the exact two-lineage
    # production and identity checks above. v142's own validator stays intact.
    inherited.VERSION = VERSION
    inherited.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    inherited.PACKAGE = "KingmakerGunslinger-" + VERSION + "-local-runtime.zip"
    inherited.PACKAGE_SUFFIX = "expanded-summoning-phase2b-checkpoint"
    inherited.DETERMINISTIC_TEST_COUNT = count
    inherited.validate(root)
    intake = trait_icons.inspect(root)
    if intake["AssetReadiness"] != "READY" or not intake["existingIconContractPass"]:
        raise AssertionError("All four original master trait assets must remain qualified")
    visibility = (root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs").read_text(encoding="utf-8")
    for token in ("RegisteredLogicalPlacementCount = 1008", "SuppressedLogicalPlacementCount = 0"):
        if token not in visibility:
            raise AssertionError("Qualified publication surface differs")
    notes = (root / "docs/RELEASE-NOTES-0.0.146.md").read_text(encoding="utf-8")
    for token in (INFORMATIONAL_VERSION, "Salamander", "PASSIVE_CREATURE_SENSES_UNMODELED",
                  "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD", "SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED",
                  "SWALLOW_ELIGIBLE_TARGET_ELSE_DEATH_ROLL", "uninstall", "Sprint 18"):
        if token not in notes:
            raise AssertionError("Release notes lack honest disposition: " + token)
    print(f"Checkpoint146 integration PASS: tests={count}; identities=2922; protected master={protected_master}; protected summons={protected_summons}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print("Checkpoint146 validation failed: " + str(exc), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())


