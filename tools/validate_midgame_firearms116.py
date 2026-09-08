#!/usr/bin/env python3
"""Content gate for 0.0.116; retains the published 0.0.115 API regression checks."""
from __future__ import annotations

import argparse
import hashlib
import json
import sys
from pathlib import Path

VERSION = "0.0.116"
INFORMATIONAL_VERSION = "0.0.116-midgame-firearms-and-protection"
# Current deterministic suite includes contextual teleportation; historical release evidence below remains 1398.
DETERMINISTIC_TEST_COUNT = 1482
STATIC_KEY = "midgameFirearms116"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.116 candidate file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks required release token(s): {missing}")
    return text


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
    require_tokens(root / "scripts/Build-Local.ps1", "active version 0.0.116",
        "local-runtime\\0.0.116", "validate-repository.ps1", "test-domain.ps1")
    require_tokens(root / "scripts/package.ps1",
        "$($info.Id)-$($info.Version)-midgame-firearms-and-protection.zip",
        "validate-build-output.ps1", "validate-package.ps1")
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "active version 0.0.116")

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))
    entries = manifest.get("entries", [])
    active = [entry for entry in entries if entry.get("status") == "active"]
    reserved = [entry for entry in entries if entry.get("status") == "reserved"]
    if (len(entries), len(active), len(reserved)) != (1711, 1709, 2):
        raise AssertionError("Authoritative blueprint manifest arithmetic drifted")

    baseline_hash = hashlib.sha256(json.dumps(entries[:1706], sort_keys=True,
        separators=(",", ":")).encode()).hexdigest()
    if baseline_hash != "ae89fe54c51ecd174867e7bc37389745420e6d706bfa8cc129235768b7093259":
        raise AssertionError("Published baseline identity ledger changed; published prefix is immutable")
    published116_hash = hashlib.sha256(json.dumps(entries[:1708], sort_keys=True,
        separators=(",", ":")).encode()).hexdigest()
    if published116_hash != "6c4bb61fb4ccfeb705bbeaf0902638b9a24aaffe45ca370e70962376affc448d":
        raise AssertionError("Published 0.0.116 identity ledger changed")
    strategic = entries[1708:]
    if {entry["symbol"] for entry in strategic} != {
        "KMG.Spells.Teleport.Ability", "KMG.Spells.GreaterTeleport.Ability",
        "KMG.Spells.WordOfRecall.Ability"}:
        raise AssertionError("Only the three assigned strategic spell identities may be appended")
    if len({entry["guid"] for entry in entries}) != len(entries) or \
            len({entry["symbol"] for entry in entries}) != len(entries):
        raise AssertionError("Duplicate blueprint GUID or symbol")
    require_tokens(root / "src/KingmakerGunslinger/Firearms/MidgameFirearmCatalog.cs",
        '"Roadwarden"', '"Dead Reckoning"', '33800, true, false',
        '33300, false, true', 'ActualEnhancement = 3', 'EquivalentBonus = 4',
        '80bb8a737579e35498177e1e3c75899b')
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/MagicFirearmBlueprints.cs",
        'plus3Components[0].EnhancementBonus != 3', 'plus3Components[0].Stack',
        'catalog.Entries.Length != 10', 'MidgameFirearmCatalog.Entries',
        'new[] { plus3, reliable }', 'new[] { plus3, seeking }')
    require_tokens(root / "src/KingmakerGunslinger/Development/KingmakerDevelopmentBridge.RareFirearms.cs",
        "value.Entries.Length != 10", "all ten rare-firearm test items")
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/RareFirearmCampaignLootBlueprints.cs",
        'BlueprintItem[] owned = Targets.Select(target =>',
        'Published five exact count-one named firearms')
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/SkeletalSalesmanBlueprints.cs",
        'RequireExact<BlueprintSharedVendorTable>', 'ReadVisibleSortKey',
        'item.ItemType', 'item.Name', 'CopiesPerStock', 'publication.Validate()')
    require_tokens(root / "src/KingmakerGunslinger/CraftMagicItemsCompatibility/CraftMagicItemsRegistrationCatalog.cs",
        'magic.NamedEntries', 'NamedUpgradeOnly')
    protection_hash = hashlib.sha256()
    for path in sorted((root / "src/KingmakerGunslinger/Spells/ProtectionFromAlignment").glob("*.cs")):
        if path.name != "ProtectionFromAlignmentDescriptions.cs":
            protection_hash.update(path.relative_to(root).as_posix().encode() + b"\0" +
                path.read_bytes().replace(b"\r\n", b"\n"))
    if protection_hash.hexdigest() != "4496f413b51cb9e9df59b0827702fe643107697baa6815bcbe4f6f6cd82f3e9e":
        raise AssertionError("Protection control/publication source changed in a wording-only patch")

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
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic test cases")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/BrownFurCastTests.cs",
        "DirectCastCoordinatorRetainsDelayedProcess",
        "DirectCastCoordinatorSupportsFourSequentialCasts",
        "DirectCastRevalidatesAndReusesAbilitySafely")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/BrownFurContractTests.cs",
        "BrownFurDirectCastApi", "ContractVersion = 1")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.116.md",
        "Kingmaker Gunslinger 0.0.116", "Roadwarden", "Dead Reckoning",
        "33,800 gp", "33,300 gp", "15 Protection", "24-state",
        "explicitly authorized")
    require_tokens(root / "scripts/Publish-Release.ps1",
        r"docs\RELEASE-NOTES-0.0.116.md", "ConfirmReleaseReady")
    require_tokens(root / "README.md",
        INFORMATIONAL_VERSION, "Roadwarden", "Dead Reckoning")
    require_tokens(root / "CHANGELOG.md", "## " + INFORMATIONAL_VERSION)

    require_tokens(root / "docs/RELEASE-NOTES-0.0.115.md",
        "Kingmaker Gunslinger 0.0.115", "ContractVersion = 1",
        "KingmakerGunslinger-0.0.115-share-transmutation-instant.zip",
        "Save-backed gameplay: NOT RUN", "owner authorized")
    require_tokens(root / "CHANGELOG.md", "0.0.115-share-transmutation-instant",
        "direct-cast", "four sequential")
    require_tokens(root / "README.md", "0.0.115-share-transmutation-instant",
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
        raise AssertionError("0.0.116 static release identity mismatch")
    state = static.get(STATIC_KEY, {})
    expected = {"deterministicTestCount": 1398, "newNamedFirearms": 2, "magicalFirearmCount": 10, "fixedLootFirearmCount": 5, "actualEnhancement": 3, "equivalentPricingBonus": 4, "merchantStockVariants": 4, "controlMechanicsUnchanged": True, "publicReleaseAuthorized": True}
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.116 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Mid-game firearms {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Mid-game firearms {VERSION} focused source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
