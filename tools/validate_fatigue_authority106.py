#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_player_presentation105 as baseline

VERSION = "0.0.106"
INFORMATIONAL_VERSION = "0.0.106-fatigue-authority-repair"
PACKAGE = "KingmakerGunslinger-0.0.106-local-runtime.zip"
PACKAGE_SUFFIX = "fatigue-authority-repair"
DETERMINISTIC_TEST_COUNT = 1325
FOCUSED_FATIGUE_CASE_COUNT = 21
STATIC_KEY = "fatigueAuthority106"
RELEASE_NOTES_VERSION = VERSION


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.106 fatigue authority file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks fatigue authority token(s): {missing}")
    return text


def reject_tokens(path: Path, *tokens: str) -> None:
    text = require_tokens(path)
    present = [token for token in tokens if token in text]
    if present:
        raise AssertionError(
            f"{path.name} retains forbidden fatigue token(s): {present}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.validate(root)

    policy = require_tokens(root / (
        "src/KingmakerGunslinger/Fatigue/"
        "CanonicalFatigueStatePolicy.cs"),
        "CanonicalFatigueApplicationIntent",
        "NativePassthrough",
        "EscalateIfAlreadyFatigued",
        "if (intent == CanonicalFatigueApplicationIntent.NativePassthrough)",
        "CanonicalConditionKind.Fatigued",
        "before == CanonicalFatigueState.Fatigued")
    intent = require_tokens(root / (
        "src/KingmakerGunslinger/Fatigue/"
        "CanonicalFatigueApplicationIntentScope.cs"),
        "[ThreadStatic] private static Request _active",
        "EnterAcadamaeEscalation(",
        "ReferenceEquals(request.BuffCollection, buffCollection)",
        "ReferenceEquals(request.ExpectedBlueprint, blueprint)",
        "request.Claimed = true",
        "public void Dispose()",
        "_active = Parent")
    if "StackTrace" in intent or "DateTime" in intent:
        raise AssertionError("Request intent uses an inferred call or timing window")

    runtime = require_tokens(root / (
        "src/KingmakerGunslinger/Fatigue/"
        "CanonicalFatigueApplicationRuntime.cs"),
        "ApplyPermanentAcadamaeFatigue(",
        "EnterAcadamaeEscalation(buffs, fatigued)",
        "CanonicalFatigueApplicationIntentScope.Claim",
        "ResolveNativePassthrough",
        "blocked-by-native-rule",
        "blocked-by-native-condition-immunity",
        "CordConditionRuntime.ResolveCanonical",
        "native-fatigue-passthrough",
        "acadamae-fatigue-escalated-to-exhausted")
    for forbidden in ("UnitPartWeariness", "GlobalMap", "StackTrace",
            "ApplyPermanentFatigue("):
        if forbidden in runtime:
            raise AssertionError(
                f"Canonical coordinator retains forbidden token: {forbidden}")

    acadamae = require_tokens(root / (
        "src/KingmakerGunslinger/Acadamae/AcadamaeCastingPatches.cs"),
        "if (!saving.IsPassed)",
        ".ApplyPermanentAcadamaeFatigue(",
        "fatigue-application-suppressed",
        "cord-substituted-exhaustion")
    if "ApplyPermanentFatigue(" in acadamae:
        raise AssertionError("Acadamae retains the ambiguous old adapter")

    program = require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        "fatigue.native-fresh",
        "fatigue.native-repeat",
        "fatigue.native-same-sequence",
        "fatigue.native-no-downgrade",
        "fatigue.acadamae-repeat",
        "fatigue.intent-exact-key",
        "fatigue.intent-one-shot",
        "fatigue.intent-nested",
        "fatigue.intent-exception",
        "fatigue.intent-thread")
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic tests")
    if program.count('Case("fatigue.') != FOCUSED_FATIGUE_CASE_COUNT:
        raise AssertionError(
            f"Expected {FOCUSED_FATIGUE_CASE_COUNT} focused fatigue cases")

    require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs"),
        '"disposable-native-fatigue-refresh"',
        '"disposable-acadamae-fatigue-escalation"',
        '"disposable-cord-of-stubborn-resolve"',
        '"working-save-fatigue-prepare"')
    require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        "RunDisposableNativeFatigueRefresh()",
        "RunDisposableAcadamaeFatigueEscalation()",
        "fatigued.Stacking == StackingType.Prolong",
        "ordinary repeated Fatigued is presented as Fatigue",
        "explicit Acadamae escalation is presented as Exhaustion",
        "native immunity blocks explicit escalation",
        "freshly deserialized native BuffCollection")
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "'disposable-native-fatigue-refresh'",
        "'disposable-acadamae-fatigue-escalation'",
        f"active version {VERSION}")
    require_tokens(root / "scripts/Invoke-FatigueWorkingSavePersistence.ps1",
        "[ValidateSet('KMG_AUTOMATION_WORKING')]",
        "working-save-fatigue-prepare",
        "working-save-fatigue-verify-cleanup",
        "working-save-fatigue-verify-absent",
        "-ReuseInstalledArtifact")

    require_tokens(root / "docs/FATIGUE-AUTHORITY-REPAIR-QUALIFICATION.md",
        "3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB",
        "0x060029F6", "0x060029F7", "0x06002186",
        "0x06002188", "0x060091B4", "StackingType.Prolong",
        "one in-game hour", "does not issue a duplicate Fatigued request",
        "supervised world-map acceptance")
    require_tokens(root / f"docs/RELEASE-NOTES-{RELEASE_NOTES_VERSION}.md",
        f"Kingmaker Gunslinger {VERSION}",
        f"KingmakerGunslinger-{VERSION}-{PACKAGE_SUFFIX}.zip",
        f"{DETERMINISTIC_TEST_COUNT:,}")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "focusedFatigueCaseCount": FOCUSED_FATIGUE_CASE_COUNT,
        "nativeDefaultIntent": "NativePassthrough",
        "acadamaeEscalationRequestLocal": True,
        "intentThreadLocal": True,
        "intentExactBuffCollection": True,
        "intentExactBlueprint": True,
        "nativeRuleFirstRequired": True,
        "nativeFatigueStacking": "Prolong",
        "nativeRefreshPeriodHours": 1,
        "travelSpecificPatchAllowed": False,
        "workingSaveBaselineMutationAllowed": False,
        "supervisedWorldMapAcceptancePending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.106 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Fatigue Authority {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Fatigue Authority {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
