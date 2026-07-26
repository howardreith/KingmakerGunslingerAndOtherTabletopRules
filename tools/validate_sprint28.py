#!/usr/bin/env python3
"""Portable source and sealed-evidence validator for Sprint 28."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

VERSION = "0.0.28"
INFORMATIONAL_VERSION = "0.0.28-s28-player-facing-overhaul"
TEST_COUNT = 569
EXPECTED_ACTIVE_BLUEPRINTS = 13
EXPECTED_LEDGER_ENTRIES = 14
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}


def fail(message: str) -> None:
    raise RuntimeError(message)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        fail(f"Required file is missing: {relative}")
    return path.read_text(encoding="utf-8")


def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        fail(f"{label} is missing required token(s): {missing}")


def compile_items(root: Path, relative: str) -> list[Path]:
    project = root / relative
    tree = ET.parse(project)
    result: list[Path] = []
    for node in tree.findall(".//m:Compile", MSBUILD_NS):
        include = node.attrib.get("Include")
        if not include:
            continue
        path = (project.parent / include.replace("\\", "/")).resolve()
        if not path.is_file():
            fail(f"Compile item is missing: {include}")
        result.append(path)
    if not result:
        fail(f"Compile list is empty: {relative}")
    if len(result) != len(set(result)):
        fail(f"Compile list contains duplicate paths: {relative}")
    return result


def validate(final: bool) -> None:
    root = Path(__file__).resolve().parents[1]

    info = json.loads(read(root, "Info.json"))
    if info.get("Version") != VERSION:
        fail("Info.json does not declare version 0.0.28.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json changed the UMM target unexpectedly.")

    assembly = read(root, "src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
    require_tokens(
        assembly,
        [
            '[assembly: AssemblyVersion("0.0.28")]',
            '[assembly: AssemblyFileVersion("0.0.28")]',
            f'[assembly: AssemblyInformationalVersion("{INFORMATIONAL_VERSION}")]',
        ],
        "AssemblyInfo",
    )

    props = read(root, "Directory.Build.props")
    require_tokens(props, ["<LangVersion>7.3</LangVersion>", "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>"], "Directory.Build.props")

    ledger = json.loads(read(root, "blueprints/blueprints.json"))
    entries = ledger.get("entries", [])
    if len(entries) != EXPECTED_LEDGER_ENTRIES:
        fail(f"Blueprint ledger expected {EXPECTED_LEDGER_ENTRIES} entries, observed {len(entries)}.")
    active = [entry for entry in entries if entry.get("status") == "active"]
    if len(active) != EXPECTED_ACTIVE_BLUEPRINTS:
        fail(f"Blueprint ledger expected {EXPECTED_ACTIVE_BLUEPRINTS} active entries, observed {len(active)}.")
    by_symbol = {entry.get("symbol"): entry for entry in entries}
    expected = {
        "KMG.Test.FirearmRepairKitItem": ("f2b564234b8a4b0d88a7a46128556bef", "BlueprintItem"),
        "KMG.Test.OverhaulAbility": ("8a0ba821382640b58ec9ff168ed778a5", "BlueprintAbility"),
    }
    for symbol, (guid, planned_type) in expected.items():
        entry = by_symbol.get(symbol)
        if entry is None or entry.get("guid") != guid or entry.get("plannedType") != planned_type or entry.get("status") != "active":
            fail(f"Blueprint ledger entry is incorrect: {symbol}")

    main_sources = compile_items(root, "src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    test_sources = compile_items(root, "tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj")
    required_main = {
        "FirearmRepairKitBlueprints.cs",
        "OverhaulTestMusketAbilityBlueprints.cs",
        "FirearmOverhaulTransactionService.cs",
        "FirearmOverhaulRuntimeResult.cs",
        "OverhaulTestMusketAbilityLogic.cs",
        "OverhaulTestMusketRuntime.cs",
    }
    if not required_main.issubset({path.name for path in main_sources}):
        fail("Main project does not compile every Sprint 28 source file.")
    required_tests = {
        "FirearmOverhaulTransactionService.cs",
        "FirearmOverhaulRuntimeResult.cs",
        "RepairKitInventorySnapshot.cs",
    }
    if not required_tests.issubset({path.name for path in test_sources}):
        fail("Domain test project does not link every Sprint 28 policy source.")

    bootstrap = read(root, "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs")
    require_tokens(
        bootstrap,
        [
            "ExpectedRegisteredBlueprintCount = 13",
            "FirearmRepairKitBlueprints.Register",
            "OverhaulTestMusketAbilityBlueprints.Register",
            "AttachAbilities",
            "_firearmRepairKit",
            "_overhaulTestMusketAbility",
        ],
        "Blueprint bootstrap",
    )

    proficiency = read(root, "src/KingmakerGunslinger/Blueprints/FirearmProficiencyBlueprints.cs")
    require_tokens(
        proficiency,
        [
            "AttachAbilities",
            "new BlueprintUnitFact[]",
            "reloadAbility,",
            "overhaulAbility",
            "Facts.Length != 2",
            "DoNotRestoreMissingFacts = false",
        ],
        "Firearm Proficiency ability grant",
    )

    transaction = read(root, "src/KingmakerGunslinger/Recovery/FirearmOverhaulTransactionService.cs")
    require_tokens(
        transaction,
        [
            "OverhaulWrecked",
            "inventory.Remove(1)",
            "RestoreState",
            "RestoreInventory",
            "Rollback refused to overwrite an unexpected concurrent firearm state",
        ],
        "Overhaul transaction",
    )
    ability = read(root, "src/KingmakerGunslinger/Blueprints/OverhaulTestMusketAbilityBlueprints.cs")
    require_tokens(
        ability,
        [
            "AbilityType.Extraordinary",
            "AbilityRange.Personal",
            "SetIsFullRoundAction(true)",
            "OverhaulTestMusketAbilityLogic.Create",
        ],
        "Overhaul ability blueprint",
    )
    runtime = read(root, "src/KingmakerGunslinger/Recovery/OverhaulTestMusketRuntime.cs")
    require_tokens(
        runtime,
        [
            "TryResolveSingleEquippedTestMusket",
            "More than one distinct Test Musket is equipped",
            "FirearmCondition.Wrecked",
            "One Firearm Repair Kit is required",
            "FirearmOverhaulRuntimeResult",
        ],
        "Overhaul runtime",
    )

    program = read(root, "tests/KingmakerGunslinger.DomainTests/Program.cs")
    case_count = len(re.findall(r'Case\("[^"]+",\s*[A-Za-z0-9_]+\)', program))
    if case_count != TEST_COUNT:
        fail(f"Expected {TEST_COUNT} declared tests, observed {case_count}.")
    require_tokens(
        program,
        [
            'Case("overhaul.transaction.success"',
            'Case("overhaul.transaction.post-state-mutation-failure-restores-both"',
            'Case("overhaul.transaction.inventory-rollback-failure-surfaced"',
            'Case("overhaul.runtime-result.success"',
            "FakeFirearmOverhaulStateStore",
            "FakeRepairKitInventory",
        ],
        "Sprint 28 tests",
    )

    ui = read(root, "src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    require_tokens(
        ui,
        [
            "0.0.28 Sprint 28 player-facing overhaul smoke test",
            "Overhaul runtime:",
            "Print Overhaul Test Musket readiness",
            "Add one Firearm Repair Kit",
            "Remove all Firearm Repair Kits from shared inventory",
        ],
        "Development UI",
    )

    guide = read(root, "SMOKE-TEST-GUIDE-0.0.28.md")
    if guide != read(root, "SMOKE-TEST-GUIDE.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.28 guide.")
    require_tokens(
        guide,
        [
            "Verify interruption before delivery",
            "Complete the player-facing overhaul",
            "beforeInventory=[repairKits=1]",
            "exactItemPreserved=True",
            "Sprint 29 remains blocked",
        ],
        "0.0.28 smoke-test guide",
    )

    for required in [
        "SPRINT-28-REPORT.md",
        "docs/FIREARM-PLAYER-FACING-OVERHAUL.md",
        "docs/decisions/ADR-0035-player-facing-same-item-overhaul.md",
        "planning/SPRINT-29-ENTRY-CRITERIA.md",
        "planning/ACCELERATION-PLAN.md",
        "planning/ROADMAP-SPRINTS-29-38.md",
    ]:
        read(root, required)

    forbidden_names = {
        "Assembly-CSharp.dll",
        "Assembly-CSharp-firstpass.dll",
        "UnityModManager.dll",
        "0Harmony12.dll",
        "Newtonsoft.Json.dll",
    }
    for path in root.rglob("*"):
        if not path.is_file():
            continue
        if "artifacts" in path.parts or "evidence" in path.parts:
            continue
        if path.name in forbidden_names:
            fail(f"Private/reference assembly is present in source tree: {path.relative_to(root)}")
        lower = path.name.lower()
        if "private-build-references" in lower and path.suffix.lower() in {".zip", ".7z", ".rar", ".tar", ".gz"}:
            fail(f"Private reference bundle is present in source tree: {path.relative_to(root)}")

    if final:
        compile_report = json.loads(read(root, "evidence/sprint28/compile/deterministic-compile-evidence.json"))
        test_report = json.loads(read(root, "evidence/sprint28/tests/executed-test-evidence.json"))
        package_report = json.loads(read(root, "evidence/sprint28/package/standalone-package-evidence.json"))
        qualification = json.loads(read(root, "BUILD-QUALIFICATION.json"))
        if not compile_report.get("dllByteIdentical") or not compile_report.get("pdbByteIdentical"):
            fail("Final compile evidence is not deterministic.")
        if compile_report.get("warningsAsErrors") is not True or compile_report.get("sameOutputPathCompileRuns") != 2:
            fail("Final compile evidence does not satisfy the qualification contract.")
        if test_report.get("declaredTests") != TEST_COUNT or test_report.get("runs") != 3 or test_report.get("failures") != 0 or test_report.get("repeatedOutputIdentical") is not True:
            fail("Final executed-test evidence is incomplete.")
        if package_report.get("entryCount") != 8 or package_report.get("binaryCount") != 1 or package_report.get("privateReferencesRedistributed") is not False:
            fail("Final standalone package evidence is incomplete.")
        if qualification.get("modVersion") != VERSION or qualification.get("informationalVersion") != INFORMATIONAL_VERSION:
            fail("BUILD-QUALIFICATION.json still describes another sprint.")
        if qualification.get("declaredTests") != TEST_COUNT or qualification.get("testRuns") != 3 or qualification.get("testFailures") != 0:
            fail("BUILD-QUALIFICATION.json has incorrect test evidence.")
        if qualification.get("readyForKingmakerSmokeTest") is not True or qualification.get("runtimeAcceptance", "").lower().find("pending") < 0:
            fail("BUILD-QUALIFICATION.json has an invalid runtime classification.")

    print("Sprint 28 source invariant validation passed" + (" with final evidence." if final else "."))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--final", action="store_true")
    args = parser.parse_args()
    try:
        validate(args.final)
    except Exception as exception:
        print(f"Sprint 28 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
