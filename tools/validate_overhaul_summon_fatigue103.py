#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_gunslinger_fixes102 as baseline

VERSION = "0.0.103"
INFORMATIONAL_VERSION = (
    "0.0.103-overhaul-summon-menu-fatigue-escalation")
PACKAGE = "KingmakerGunslinger-0.0.103-local-runtime.zip"
PACKAGE_SUFFIX = "overhaul-summon-menu-fatigue-escalation"
DETERMINISTIC_TEST_COUNT = 1278
STATIC_KEY = "overhaulSummonFatigue103"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.103 mission file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.103 mission token(s): {missing}")
    return text


def reject_tokens(path: Path, *tokens: str) -> None:
    text = require_tokens(path)
    present = [token for token in tokens if token in text]
    if present:
        raise AssertionError(
            f"{path.name} retains rejected 0.0.103 token(s): {present}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.validate(root)

    overhaul = root / (
        "src/KingmakerGunslinger/Recovery/"
        "OverhaulTestMusketAbilityLogic.cs")
    require_tokens(overhaul, "DeliverPromptly", "TryPrepare(context",
        "Complete(context, start)",
        "yield return new AbilityDeliveryTarget(target)",
        "ReferenceEquals(completed.Weapon, start.Weapon)",
        "no active combat")
    reject_tokens(overhaul, "TimeSpan.FromSeconds(60", "GameTime +",
        "yield return null", "completionTime")
    require_tokens(root / (
        "src/KingmakerGunslinger/Blueprints/"
        "OverhaulTestMusketAbilityBlueprints.cs"),
        '"Instantaneous"', "one full-round action",
        "Outside active combat")

    require_tokens(root / (
        "src/KingmakerGunslinger/Summoning/"
        "SummonVariantMenuLayoutPolicy.cs"),
        "SummonVariantMenuLayoutRequest", "ChooseDirection(",
        "CanvasSafeRect", "RequiresVerticalScrolling",
        "RequiresHorizontalScrolling", "safe.Contains(finalRect")
    require_tokens(root / (
        "src/KingmakerGunslinger/Summoning/"
        "ExpandedSummoningVariantMenuPatch.cs"),
        "typeof(ActionBarSpellsGroup), \"Toggle\"",
        "IsPublishedExpandedParent", "PrepareForNativeFill",
        "ExpandedSummoningVariantMenuRuntime.Apply")
    adapter = require_tokens(root / (
        "src/KingmakerGunslinger/Summoning/"
        "ExpandedSummoningVariantMenuRuntime.cs"),
        "Screen.safeArea", "canvas.pixelRect", "Canvas.ForceUpdateCanvases",
        "RectTransformUtility.ScreenPointToLocalPointInRectangle",
        "ScrollRect", "RectMask2D", "ConditionalWeakTable",
        "RestoreNative", "TryValidateNavigation")
    if "1920" in adapter or "1080" in adapter:
        raise AssertionError("Variant-menu adapter uses a fixed resolution")

    fatigue = require_tokens(root / (
        "src/KingmakerGunslinger/Fatigue/"
        "CanonicalFatigueApplicationRuntime.cs"),
        'FatiguedGuid =', 'ExhaustedGuid =',
        '[HarmonyPatch(typeof(BuffCollection), "TriggerRuleApplyBuff"',
        '[HarmonyAfter("CallOfTheWild")]', "if (result == null)",
        "NativeConditionPresent(scope)", "ApplyRelated(scope.Buffs, exhausted",
        "PreserveLongestDuration", "CordConditionRuntime.ResolveCanonical",
        "[ThreadStatic] private static ApplicationScope _activeScope",
        "private static Exception Finalizer")
    if ".name.Contains" in fatigue or "Description.Contains" in fatigue:
        raise AssertionError(
            "Canonical fatigue matching regressed to text classification")
    require_tokens(root / (
        "src/KingmakerGunslinger/Acadamae/AcadamaeCastingPatches.cs"),
        "ApplyPermanentFatigue", "CanonicalFatigueState.Exhausted")
    cord = require_tokens(root / (
        "src/KingmakerGunslinger/Cord/CordConditionPatches.cs"),
        "IsCanonicalApplication", "ResolveCanonical",
        "HasExactEquippedCord", "HPLeft - 1")
    if "TriggerRuleApplyBuff" in cord:
        raise AssertionError(
            "Cord retained a competing broad buff-application patch")

    runner = require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        "expanded-summoning.menu-layout-top",
        "expanded-summoning.menu-layout-navigation",
        "fatigue.repeated-fatigue", "fatigue.same-sequence",
        "fatigue.cord-order")
    if runner.count('Case("expanded-summoning.menu-layout-') != 14:
        raise AssertionError("Expected 14 focused summon-menu layout tests")
    if runner.count('Case("fatigue.') != 13:
        raise AssertionError("Expected 13 focused canonical fatigue tests")
    require_tokens(root / (
        "src/KingmakerGunslinger/KingmakerGunslinger.csproj"),
        '<Content Include="..\\..\\assets\\bundles\\kingmakergunslinger.firearms">',
        '<Link>assets\\bundles\\kingmakergunslinger.firearms</Link>',
        "UnityEngine.UIModule.dll", "UnityEngine.TextRenderingModule.dll")
    require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs"),
        '"disposable-overhaul-maintenance"',
        '"disposable-fatigue-escalation"',
        '"disposable-acadamae-graduate"',
        '"disposable-cord-of-stubborn-resolve"',
        '"observe-expanded-summoning-variant-menu"')
    require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "ExpandedSummoningVariantMenuObservation.cs"),
        "TryValidateNavigation", "NearTopLeft", "StableSamplesRequired",
        "opened published menu is not the largest runtime list")

    require_tokens(root / f"docs/RELEASE-NOTES-{VERSION}.md",
        f"Kingmaker Gunslinger {VERSION}", f"{PACKAGE_SUFFIX}.zip",
        "1,278")
    require_tokens(root / (
        "OVERHAUL-SUMMON-MENU-FATIGUE-ESCALATION-"
        "IMPLEMENTATION-REPORT.md"),
        "Root causes", "Overhaul Firearm", "variant menu",
        "canonical Fatigued")
    require_tokens(root / (
        "docs/OVERHAUL-SUMMON-MENU-FATIGUE-ESCALATION-QUALIFICATION.md"),
        "1278", "Guarded runtime", "Human acceptance")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "overhaulPromptDeliveryRequired": True,
        "overhaulGameTimeMutationAllowed": False,
        "summonMenuCanvasSafeBoundsRequired": True,
        "summonMenuOversizeScrollingRequired": True,
        "summonMenuHumanPresentationPending": True,
        "canonicalFatigueEscalationRequired": True,
        "cordSingleSubstitutionRequired": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.103 static mismatch: {key}")
    for key in ("overhaulRuntimeQualificationPending",
            "summonMenuObservationPending",
            "fatigueRuntimeQualificationPending"):
        if not isinstance(state.get(key), bool):
            raise AssertionError(f"0.0.103 qualification state missing: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Overhaul/Summon/Fatigue {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Overhaul/Summon/Fatigue {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
