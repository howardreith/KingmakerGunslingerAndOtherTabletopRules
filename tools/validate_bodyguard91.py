#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_bodyguard90 as baseline

VERSION = "0.0.91"
INFORMATIONAL_VERSION = "0.0.91-bodyguard-ac-breakdown"
PACKAGE = "KingmakerGunslinger-0.0.91-local-runtime.zip"
STATIC_KEY = "bodyguard91"
DETERMINISTIC_TEST_COUNT = 1192


def require_tokens(path: Path, *tokens: str) -> None:
    if not path.is_file():
        raise AssertionError(f"Bodyguard AC-breakdown gate file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks AC-breakdown contract token(s): {missing}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.STATIC_KEY = STATIC_KEY
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    policy = root / (
        "src/KingmakerGunslinger/BodyguardFeats/"
        "BodyguardArmorClassAttributionPolicy.cs")
    runtime = root / (
        "src/KingmakerGunslinger/BodyguardFeats/BodyguardRuntime.cs")
    scenario = root / (
        "src/KingmakerGunslinger/RuntimeTesting/BodyguardCombatScenario.cs")
    observer = root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "BodyguardNativeContractObserver.cs")

    require_tokens(policy, "BodyguardArmorClassAttributionPlan",
        "BodyguardAidPolicy.StackArmorClassBonus",
        "ActualArmorClassContribution", "FinalArmorClass")
    require_tokens(runtime, "new BonusSource(contribution.Bonus, source)",
        "attempt.Protector.Descriptor.GetFact(",
        "armorClass.BonusSources.Add(source)", "plan.FinalArmorClass",
        "armorClass.BonusSources.RemoveRange(", "bodyguardSourceCount=")
    require_tokens(scenario, "bodyguard-ac-breakdown-one",
        "bodyguard-ac-breakdown-failure", "bodyguard-ac-breakdown-two",
        "NativeAcBeforeBodyguard", "BodyguardSources")
    require_tokens(observer, "bodyguard-native-ac-breakdown",
        "AppendArmorClassBreakdown", "AddBonusSources",
        "typeof(List<BonusSource>)")

    main_project = (root / "src/KingmakerGunslinger/"
        "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    test_project = (root / "tests/KingmakerGunslinger.DomainTests/"
        "KingmakerGunslinger.DomainTests.csproj").read_text(encoding="utf-8")
    program = (root / "tests/KingmakerGunslinger.DomainTests/Program.cs") \
        .read_text(encoding="utf-8")
    if "BodyguardArmorClassAttributionPolicy.cs" not in main_project or \
            "BodyguardArmorClassAttributionPolicy.cs" not in test_project:
        raise AssertionError("AC-attribution policy is not explicitly compiled")
    for token in ("bodyguard-policy.ac-attribution",
                  "bodyguard-runtime.ac-breakdown"):
        if token not in program:
            raise AssertionError(f"AC-attribution test is not registered: {token}")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    if static.get("bodyguard91", {}).get(
            "nativeAcBreakdownAttributionRequired") is not True:
        raise AssertionError("Native AC-breakdown attribution is not gated")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Bodyguard AC Breakdown {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Bodyguard AC Breakdown {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
