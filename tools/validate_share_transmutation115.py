#!/usr/bin/env python3
"""Focused source gate for the 0.0.115 Share Transmutation provider API."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

VERSION = "0.0.115"
INFORMATIONAL_VERSION = "0.0.115-share-transmutation-instant"
DETERMINISTIC_TEST_COUNT = 1393
STATIC_KEY = "shareTransmutationInstant115"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.115 release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.115 release token(s): {missing}")
    return text


def validate_provider_contract(root: Path) -> str:
    api = require_tokens(root / "src/KingmakerGunslinger/BrownFur/"
        "BrownFurDirectCastApi.cs", "public const int ContractVersion = 1",
        "Validate(\n            AbilityData ability, TargetWrapper target)",
        "Begin(\n            AbilityData ability, TargetWrapper target)",
        "CompleteRule(RuleCastSpell rule)", "BrownFurDirectCastStatus Cleanup()",
        "public void Dispose()", "TransactionIdentity", "ReservoirCost")
    runtime = require_tokens(root / "src/KingmakerGunslinger/BrownFur/"
        "BrownFurCastExecutionRuntime.cs", "Coordinator.TryGetByAbility(rule.Spell",
        "!ReferenceEquals(rule.SpellTarget.Unit,",
        "binding.Target.Unit)", "handle.Matches(rule)",
        "provider-direct-commit-rejected", "rule.ExecutionProcess",
        "direct-reservoir-rollback-failed")
    intent = require_tokens(root / "src/KingmakerGunslinger/BrownFur/"
        "BrownFurCastIntentRuntime.cs", "ValidateDirect(", "BeginDirect(",
        "BuildIntent(ability, ability, target",
        "RuntimeHelpers.GetHashCode(identityAnchor)",
        "direct-cast-reservation-rejected")
    lifecycle = require_tokens(root / "src/KingmakerGunslinger/BrownFur/"
        "BrownFurCastLifecycleTracker.cs", "BeginDirect(", "CompleteDirect(",
        "CancelDirect(", "FailDirect(", "DirectProcessAttached(")
    require_tokens(root / "src/KingmakerGunslinger/BrownFur/"
        "BrownFurCastCommitCoordinator.cs", "BeginDirect(", "CompleteDirect(",
        "CancelDirect(", "FailDirect(", "DirectProcessAttached(")
    require_tokens(root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
        "BrownFur\\BrownFurDirectCastApi.cs")
    direct_sources = api + runtime + intent + lifecycle
    for prohibited in ("KingmakerBuffPlanner", "Felix", "Resinous Skin",
            "41ceee31b77741e99d3b0990bbe40a2a", "new UnitUseAbility"):
        if prohibited in direct_sources:
            raise AssertionError(f"Direct provider path contains prohibited coupling: {prohibited}")

    program = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/Program.cs",
        'Case("brown-fur.cast-direct-delayed-process"',
        'Case("brown-fur.cast-direct-four-sequential"',
        'Case("brown-fur.cast-direct-revalidation-reuse"')
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/BrownFurCastTests.cs",
        "DirectCastCoordinatorRetainsDelayedProcess",
        "DirectCastCoordinatorSupportsFourSequentialCasts",
        "DirectCastRevalidatesAndReusesAbilitySafely")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/BrownFurContractTests.cs",
        "BrownFurDirectCastApi", "ContractVersion = 1")

    return program


def validate(root: Path) -> None:
    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    if info.get("Version") != VERSION or info.get("Id") != "KingmakerGunslinger":
        raise AssertionError("Info.json release identity mismatch")
    require_tokens(root / "Directory.Build.props",
        f"<KmgVersion>{VERSION}</KmgVersion>",
        f"<KmgInformationalVersion>{INFORMATIONAL_VERSION}</KmgInformationalVersion>",
        "<LangVersion>7.3</LangVersion>", "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>")
    require_tokens(root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs",
        f'AssemblyVersion("{VERSION}")', f'AssemblyFileVersion("{VERSION}")',
        f'AssemblyInformationalVersion("{INFORMATIONAL_VERSION}")')
    require_tokens(root / "scripts/Build-Local.ps1", "active version 0.0.115",
        "local-runtime\\0.0.115", "validate-repository.ps1", "test-domain.ps1")
    require_tokens(root / "scripts/package.ps1",
        "$($info.Id)-$($info.Version)-share-transmutation-instant.zip",
        "validate-build-output.ps1", "validate-package.ps1")
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "active version 0.0.115")

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))
    entries = manifest.get("entries", [])
    active = [entry for entry in entries if entry.get("status") == "active"]
    reserved = [entry for entry in entries if entry.get("status") == "reserved"]
    if (len(entries), len(active), len(reserved)) != (1706, 1704, 2):
        raise AssertionError("Authoritative blueprint manifest arithmetic drifted")

    program = validate_provider_contract(root)
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic test cases")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.115.md",
        "Kingmaker Gunslinger 0.0.115", "ContractVersion = 1",
        "KingmakerGunslinger-0.0.115-share-transmutation-instant.zip",
        "Save-backed gameplay: NOT RUN", "owner authorized")
    require_tokens(root / "CHANGELOG.md", INFORMATIONAL_VERSION,
        "direct-cast", "four sequential")
    require_tokens(root / "README.md", INFORMATIONAL_VERSION,
        "BrownFurDirectCastApi", "animated fallback")
    require_tokens(root / "planning/BROWN-FUR-COTW-CONTRACT.md",
        "0.0.115 direct-cast integration addendum", "ContractVersion = 1")

    profiles = json.loads((root / "compatibility/profiles.json").read_text(
        encoding="utf-8")).get("profiles", [])
    historical_package = "KingmakerGunslinger-0.0.114-local-runtime.zip"
    if not profiles or any(profile.get("requiredGunslingerPackage") !=
            historical_package for profile in profiles):
        raise AssertionError("Published 0.0.114 compatibility evidence was relabeled")
    schema = json.loads((root / "compatibility/profiles.schema.json").read_text(
        encoding="utf-8"))
    package_const = schema["properties"]["profiles"]["items"]["properties"][
        "requiredGunslingerPackage"]["const"]
    if package_const != historical_package:
        raise AssertionError("Published compatibility schema was relabeled")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("0.0.115 static release identity mismatch")
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "contractVersion": 1,
        "exactAbilityAndTargetBinding": True,
        "providerOwnsReservoirDebit": True,
        "delayedProcessRetained": True,
        "sequentialCastCount": 4,
        "plannerCompileDependency": False,
        "runtimeQualificationPending": True,
        "publicReleaseAuthorized": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.115 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Share Transmutation {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Share Transmutation {VERSION} focused source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
