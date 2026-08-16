#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_eastern80

VERSION = "0.0.82"
INFORMATIONAL_VERSION = "0.0.82-brown-fur-human-review-repair"
PACKAGE = "KingmakerGunslinger-0.0.82-local-runtime.zip"
MILESTONE_LABEL = "BROWN-FUR-HUMAN-REVIEW-REPAIR"
PACKAGE_SUFFIX = "brown-fur-human-review-repair"


def validate(root: Path) -> None:
    validate_eastern80.VERSION = VERSION
    validate_eastern80.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_eastern80.PACKAGE = PACKAGE
    validate_eastern80.MILESTONE_LABEL = MILESTONE_LABEL
    validate_eastern80.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    validate_eastern80.validate(root)

    required = (
        "planning/BROWN-FUR-TRANSMUTER-MISSION.md",
        "planning/BROWN-FUR-COTW-CONTRACT.md",
        "docs/RUNTIME-QUALIFICATION-POLICY.md",
        "src/KingmakerGunslinger/BrownFur/CotwProgressionPolicy.cs",
        "src/KingmakerGunslinger/BrownFur/CotwArcanistResolver.cs",
        "src/KingmakerGunslinger/BrownFur/CotwSharedSpellsBridge.cs",
        "src/KingmakerGunslinger/BrownFur/BrownFurOptionalExtensionCoordinator.cs",
        "src/KingmakerGunslinger/BrownFur/BrownFurPublicationTransaction.cs",
        "src/KingmakerGunslinger/BrownFur/BrownFurArchetypePlan.cs",
        "src/KingmakerGunslinger/BrownFur/BrownFurActivatableGroupRuntime.cs",
        "src/KingmakerGunslinger/Blueprints/BrownFurBlueprints.cs",
        "src/KingmakerGunslinger/RuntimeTesting/BrownFurCotwContractObserver.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Brown-Fur repair file missing: {relative}")

    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    if info.get("Requirements") != []:
        raise AssertionError("CotW must not be a package-wide UMM requirement")
    project = (root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj") \
        .read_text(encoding="utf-8")
    if "CallOfTheWild.dll" in project or 'Reference Include="CallOfTheWild' in project:
        raise AssertionError("Brown-Fur acquired a compile-time CotW reference")
    blueprints = (root / "src/KingmakerGunslinger/Blueprints/BrownFurBlueprints.cs") \
        .read_text(encoding="utf-8")
    targeting = (root / "src/KingmakerGunslinger/BrownFur/BrownFurShareTargetingRuntime.cs") \
        .read_text(encoding="utf-8")
    for token in ("ScoreActivatables", "ResourceSpendType.Never",
            "PowerfulChangeGroup", "TryResolvePendingShareTargeting"):
        if token not in blueprints + targeting:
            raise AssertionError(f"Brown-Fur repair contract missing: {token}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Brown-Fur {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Brown-Fur {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
