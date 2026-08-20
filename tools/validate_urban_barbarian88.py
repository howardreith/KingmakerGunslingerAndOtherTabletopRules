#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_urban_barbarian87 as baseline

VERSION = "0.0.88"
INFORMATIONAL_VERSION = "0.0.88-overnight-gunslinger-bugfixes"
PACKAGE = "KingmakerGunslinger-0.0.88-local-runtime.zip"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.validate(root)

    sources = {
        "cord": root / "src/KingmakerGunslinger/Blueprints/CordOfStubbornResolveBlueprints.cs",
        "eastern": root / "src/KingmakerGunslinger/Blueprints/EasternWeaponCampaignBlueprints.cs",
        "spears": root / "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearCampaignBlueprints.cs",
        "runtime": root / "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
        "development": root / "src/KingmakerGunslinger/Development/KingmakerDevelopmentBridge.RareFirearms.cs",
    }
    text = {name: path.read_text(encoding="utf-8")
        for name, path in sources.items()}
    required = {
        "cord": ("PublishCampaignLoot", "e2add2e7254305b40aa1b9ae60ed2be0", "Rollback()"),
        "eastern": ("LootRowCount != 18", "placed.Distinct().Count() != 18", "CleanupLoot"),
        "spears": ("_loot.Count < 6",
            "_loot.Select(value => value.Target).Distinct().Count()",
            "CleanupLoot"),
        "runtime": ("project-magic-item-distribution", "targets.Values.Distinct().Count() == 30", "vendorRows == 0"),
        "development": ("DescribeProjectMagicItemAcquisition", "placements=", "countOneMatches="),
    }
    for name, tokens in required.items():
        for token in tokens:
            if token not in text[name]:
                raise AssertionError(
                    f"Issue 12 {name} contract is missing: {token}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Overnight Gunslinger {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Overnight Gunslinger {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
