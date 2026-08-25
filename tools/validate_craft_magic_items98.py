#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_compatibility_attribution97 as baseline

VERSION = "0.0.98"
INFORMATIONAL_VERSION = "0.0.98-craft-magic-items-compatibility"
PACKAGE = "KingmakerGunslinger-0.0.98-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1238
STATIC_KEY = "craftMagicItems98"
FOCUSED_TEST_COUNT = 10
PACKAGE_SUFFIX = "craft-magic-items-compatibility"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"Craft Magic Items compatibility file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks Craft Magic Items token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.validate(root)

    required = (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsCompatibilityPolicy.cs",
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsCompatibilityStatus.cs",
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsContractProbe.cs",
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsRegistrationCatalog.cs",
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsReflectionBridge.cs",
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsOptionalExtensionCoordinator.cs",
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsRuntimeQualification.cs",
        "src/KingmakerGunslinger/RuntimeTesting/"
        "CraftMagicItemsCompatibilityObserver.cs",
        "tests/KingmakerGunslinger.DomainTests/"
        "CraftMagicItemsCompatibilityTests.cs",
        "docs/CRAFT-MAGIC-ITEMS-COMPATIBILITY-REPORT.md",
        f"docs/RELEASE-NOTES-{VERSION}.md",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(
                f"Craft Magic Items compatibility file missing: {relative}")

    probe = require_tokens(root / required[2],
        "CraftMagicItems.Main", "AddAllCraftingFeats",
        "HarmonyLib.Harmony", "main-static-fields")
    bridge = require_tokens(root / required[4],
        "KMGMagicFirearms", "CraftMundaneKMGFirearms",
        "KMGFirearmAmmunition", "KMGReliable",
        "AmmunitionBatchCount", "RollbackCompatibilityGraph",
        "TryRestoreNewItemBaseState", "BuildCustomRecipeGuid",
        "FirearmRuntimeState.ReadStateTokenIds")
    coordinator = require_tokens(root / required[5],
        "CraftMagicItems", "AfterDataRead", "AddItemIdForEnchantment",
        "AddAllCraftingFeats", "patches=11",
        "__result = __result &&", "object[] __args", "UnpatchAll",
        "harmony.patch-install-rollback")
    require_tokens(root / required[7],
        "RunGuardedQualification", "exact-live-cmi-entry",
        "save-free-disposable-boundary")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs",
        "observe-craft-magic-items-compatibility")
    require_tokens(root / required[8],
        "ContractProbeAcceptsExactShape", "CatalogConstructionIsExact",
        "ReliableApplicabilityIsMarkerExact",
        "AmmunitionBatchEconomicsAreExact",
        "CustomBlueprintIntegrityBoundaryIsExact",
        "AssertNoStaticDependency")
    require_tokens(root / required[9],
        "CraftMagicItems", "2.1.0", "Reliable",
        "Black Powder Charge", "KMG_AUTOMATION_WORKING",
        "Remaining uncertainty")
    require_tokens(root / required[10],
        f"Kingmaker Gunslinger {VERSION}",
        f"{PACKAGE_SUFFIX}.zip",
        "CraftMagicItems.dll")

    project = (root / "src/KingmakerGunslinger/"
        "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    if re.search(r'<Reference\s+Include="CraftMagicItems', project,
            re.IGNORECASE) or "CraftMagicItems.dll" in project:
        raise AssertionError("Production has a static CraftMagicItems dependency")
    for source in required[:8]:
        name = Path(source).name
        if name not in project:
            raise AssertionError(f"Main project compile list lacks {name}")
    if "using CraftMagicItems" in probe or "using CraftMagicItems" in bridge \
            or "using CraftMagicItems" in coordinator:
        raise AssertionError("Production reflection boundary imports CMI types")

    test_project = (root / "tests/KingmakerGunslinger.DomainTests/"
        "KingmakerGunslinger.DomainTests.csproj").read_text(encoding="utf-8")
    runner = (root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs").read_text(encoding="utf-8")
    if "CraftMagicItemsCompatibilityTests.cs" not in test_project or \
            runner.count("craft-magic-items.") != FOCUSED_TEST_COUNT:
        raise AssertionError(
            f"{FOCUSED_TEST_COUNT} focused CMI compatibility tests are not registered")

    package = (root / "scripts/package.ps1").read_text(encoding="utf-8")
    if f"{PACKAGE_SUFFIX}.zip" not in package:
        raise AssertionError(f"{VERSION} package identity is missing")
    forbidden = ("CraftMagicItems.dll", "OwlcatKingmakerModCraftMagicItems",
        "external-cmi")
    if any(value in package for value in forbidden):
        raise AssertionError("Package script attempts to ship external CMI content")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    contract = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "optionalReflectionBoundary": True,
        "realCmiRuntimeQualificationRequired": True,
        "realCmiRuntimeQualified": True,
        "cmiRuntimeAssertionCount": 23,
        "workingSaveSmokeQualified": True,
        "runtimeQualificationPending": False,
        "namedUniqueCreationExcluded": True,
        "reliableMarkerRestricted": True,
        "ammunitionBatchCount": 20,
        "transactionalRollbackRequired": True,
    }
    for key, value in expected.items():
        if contract.get(key) != value:
            raise AssertionError(f"CMI static validation mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Craft Magic Items Compatibility {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Craft Magic Items Compatibility {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
