#!/usr/bin/env python3
"""Portable source validator for Sprint 60 player-facing presentation."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint59

VERSION = "0.0.60"
INFORMATIONAL_VERSION = "0.0.60-s60-player-presentation"
TEST_COUNT = 832

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 60 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint59.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 128, 129)
    require_tokens(read(root, "planning/SPRINT-60-ENTRY-CRITERIA.md"),
        ["non-hidden feature or ability", "approved fallback icon",
         "Progression UI groups", "presentation metadata only"],
        "Sprint 60 criteria")
    presentation = read(root,
        "src/KingmakerGunslinger/Blueprints/PlayerFacingPresentation.cs")
    require_tokens(presentation,
        ["SetIconIfMissing", "feature.HideInUI", "ability.Hidden",
         "StartsWith(\"KMG_\"", "selection.AllFeatures", "OfType<AddFacts>",
         "progression.UIGroups", "features.ToList()"],
        "Sprint 60 presentation graph")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["PlayerFacingPresentation.Apply(progression, characterClass.Icon)",
         "fighter.Progression.Icon",
         "presentationIcon = startingPistol.Icon",
         "approved native class and crossbow-compatible firearm sources",
         "result.m_Icon = presentationIcon"],
        "Sprint 60 bootstrap integration")
    bootstrap = read(root,
        "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs")
    require_tokens(bootstrap,
        ['"initialize.root-cause"', "initializationException",
         "CapitalVendorBlueprints.Publish(",
         "capitalVendorPublication.Rollback()",
         "registry.RollbackAll()"],
        "Sprint 60 pre-rollback root-cause diagnostic")
    acquisition = read(root,
        "src/KingmakerGunslinger/Blueprints/CapitalVendorBlueprints.cs")
    require_tokens(acquisition,
        ["afa2c7f292b8e1c4d9c835f0e8047dd3", "WeaponCount = 1",
         "ConsumableCount = 99", "AdvancedRifle.Item",
         "AdvancedRevolver.Item", "Blunderbuss included",
         "VendorCatalogPublication<BlueprintComponent>.Create",
         "capital vendor contains a duplicate or partial",
         "rollback refused because the table changed"],
        "Sprint 61 capital vendor publication")
    if bootstrap.index('"initialize.root-cause"') > bootstrap.index(
            "registry.RollbackAll()"):
        raise RuntimeError(
            "Sprint 60 root-cause diagnostic must precede rollback")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs") + read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs"),
        ["observe-gunslinger-presentation",
         "observe-vendor-table-contracts",
         "observe-production-firearm-fallbacks",
         "RunGunslingerPresentationObservation",
         "RunVendorTableContractObservation",
         "RunProductionFirearmFallbackObservation",
         "ObserveProductionFirearmFallbacks,",
         'Assertion("one-handed-firearm-fallbacks"',
         'Assertion("two-handed-firearm-fallbacks"',
         '";catalog="',
         '";owners="',
         '";capitalEntries="',
         '";capitalReferenceContracts="',
         '";fixedEntryPatterns="',
         'Assertion("vendor-fixed-entry-quantity-precedent"',
         'Assertion("gunslinger-capital-vendor-publication"',
         '";blunderbussEntries="',
         'Assertion("capital-vendor-fixed-entry-contract"',
         'Assertion("vendor-component-owners"',
         "unit.AddFacts ?? Array.Empty<BlueprintUnitFact>()",
         ".GroupBy(value => value.GetType().FullName)",
         "gunslinger-visible-fact-presentation",
         "gunslinger-hidden-fact-exclusion",
         "gunslinger-progression-ui-groups"],
        "Sprint 60 guarded presentation observation")
    print("Sprint 60 source validation passed with inherited Sprint 59 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 60 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
