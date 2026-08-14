#!/usr/bin/env python3
"""Portable source validator for Sprint 31 with inherited Sprint 30 checks."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_sprint30

VERSION = "0.0.31"
INFORMATIONAL_VERSION = "0.0.31-s31-early-firearm-catalog"
TEST_COUNT = 624
EXPECTED_ACTIVE_BLUEPRINTS = 20
EXPECTED_LEDGER_ENTRIES = 21


def fail(message: str) -> None:
    raise RuntimeError(message)


def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        fail(f"Required Sprint 31 file is missing: {relative}")
    return path.read_text(encoding="utf-8")


def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        fail(f"{label} is missing required token(s): {missing}")


def validate(
    root: Path,
    version: str = VERSION,
    informational_version: str = INFORMATIONAL_VERSION,
    test_count: int = TEST_COUNT,
    expected_active_blueprints: int = EXPECTED_ACTIVE_BLUEPRINTS,
    expected_ledger_entries: int = EXPECTED_LEDGER_ENTRIES,
) -> None:
    root = root.resolve()
    validate_sprint30.validate(
        root,
        version=version,
        informational_version=informational_version,
        test_count=test_count,
        expected_active_blueprints=expected_active_blueprints,
        expected_ledger_entries=expected_ledger_entries,
        expected_registered_blueprints=expected_active_blueprints,
    )
    require_tokens(
        read(root, "planning/SPRINT-31-ENTRY-CRITERIA.md"),
        [
            "Pistol",
            "Musket",
            "Blunderbuss",
            "special",
            "no numeric single-target range adaptation is authorized yet",
            "generic",
            "two fresh-process feature PASS runs",
        ],
        "Sprint 31 entry criteria",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Firearms/FirearmDefinitions.cs"),
        [
            "CreateEarlyPistol",
            "CreateEarlyBlunderbuss",
            "FirearmKind.Pistol",
            "FirearmKind.Blunderbuss",
            "ReloadActionType.Standard",
            "ReloadActionType.FullRound",
        ],
        "Sprint 31 canonical definitions",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Firearms/FirearmDefinition.cs"),
        ["HasFixedRangeIncrement", "FixedRangeIncrementFeet", '"special"'],
        "Sprint 31 range vocabulary",
    )
    require_tokens(
        read(root, "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        [
            'Case("factory.early-pistol-fresh-instances"',
            'Case("factory.early-pistol-canonical-equality"',
            'Case("factory.early-blunderbuss-ordinary-range"',
            'Case("factory.early-blunderbuss-fixed-range-accessible"',
            'Case("ac.blunderbuss-first-increment-touch"',
        ],
        "Sprint 31 definition tests",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Firearms/ProductionFirearmCatalog.cs"),
        [
            "CreatePistol",
            "CreateMusket",
            "CreateBlunderbuss",
            '"pistol"',
            '"musket"',
            '"blunderbuss"',
        ],
        "Sprint 31 production firearm catalog",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs"),
        [
            "EarlyPistolWeaponType",
            "EarlyMusketWeaponType",
            "EarlyBlunderbussWeaponType",
            "UnavailableProductionFirearmRestriction",
            "mechanicalAccess.Configure",
        ],
        "Sprint 31 production firearm blueprints",
    )
    registration_token = (("ExpectedRegisteredBlueprintCount = 298 +"
        if (root / "src/KingmakerGunslinger/EasternWeapons/EasternWeaponCatalog.cs").is_file()
        else "ExpectedRegisteredBlueprintCount = 283 +"
        if (root / "src/KingmakerGunslinger/ElvenBranchedSpear/ElvenBranchedSpearCatalog.cs").is_file()
        else "ExpectedRegisteredBlueprintCount = 254 +")
        if (root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs").is_file()
        else "ExpectedRegisteredBlueprintCount = " + str(expected_active_blueprints))
    require_tokens(
        read(root, "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs"),
        [registration_token,
         "ProductionFirearmBlueprints.Register"],
        "Sprint 31 blueprint bootstrap",
    )
    print("Sprint 31 source invariant validation passed with inherited Sprint 30 checks.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 31 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
