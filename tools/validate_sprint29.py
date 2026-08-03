#!/usr/bin/env python3
"""Portable source and sealed-evidence validator for Sprint 29."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

VERSION = "0.0.29"
INFORMATIONAL_VERSION = "0.0.29-s29-complete-maintenance-loop"
TEST_COUNT = 599
EXPECTED_ACTIVE_BLUEPRINTS = 14
EXPECTED_LEDGER_ENTRIES = 15
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}


def fail(message: str) -> None:
    raise RuntimeError(message)


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


def validate(
    final: bool,
    root: Path | None = None,
    version: str = VERSION,
    informational_version: str = INFORMATIONAL_VERSION,
    test_count: int = TEST_COUNT,
    require_current_guide_match: bool = True,
    expected_active_blueprints: int = EXPECTED_ACTIVE_BLUEPRINTS,
    expected_ledger_entries: int = EXPECTED_LEDGER_ENTRIES,
    expected_registered_blueprints: int = EXPECTED_ACTIVE_BLUEPRINTS,
) -> None:
    root = (root or Path(__file__).resolve().parents[1]).resolve()

    info = json.loads(read(root, "Info.json"))
    if info.get("Version") != version:
        fail(f"Info.json does not declare version {version}.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json changed the UMM target unexpectedly.")

    props = read(root, "Directory.Build.props")
    require_tokens(
        props,
        [
            f"<KmgVersion>{version}</KmgVersion>",
            f"<KmgInformationalVersion>{informational_version}</KmgInformationalVersion>",
            "<LangVersion>7.3</LangVersion>",
            "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
            "<Deterministic>true</Deterministic>",
        ],
        "Directory.Build.props",
    )

    assembly = read(root, "src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
    require_tokens(
        assembly,
        [
            f'[assembly: AssemblyVersion("{version}")]',
            f'[assembly: AssemblyFileVersion("{version}")]',
            f'[assembly: AssemblyInformationalVersion("{informational_version}")]',
        ],
        "AssemblyInfo",
    )

    ledger = json.loads(read(root, "blueprints/blueprints.json"))
    entries = ledger.get("entries", [])
    if len(entries) != expected_ledger_entries:
        fail(f"Blueprint ledger expected {expected_ledger_entries} entries, observed {len(entries)}.")
    active = [entry for entry in entries if entry.get("status") == "active"]
    if len(active) != expected_active_blueprints:
        fail(f"Blueprint ledger expected {expected_active_blueprints} active entries, observed {len(active)}.")
    symbols = [entry.get("symbol") for entry in entries]
    guids = [entry.get("guid") for entry in entries]
    if len(symbols) != len(set(symbols)) or len(guids) != len(set(guids)):
        fail("Blueprint ledger contains duplicate symbols or GUIDs.")
    by_symbol = {entry.get("symbol"): entry for entry in entries}
    expected = {
        "KMG.Test.FirearmRepairKitItem": ("f2b564234b8a4b0d88a7a46128556bef", "BlueprintItem"),
        "KMG.Test.OverhaulAbility": ("8a0ba821382640b58ec9ff168ed778a5", "BlueprintAbility"),
        "KMG.Test.RepairAbility": ("c914b3c0786463b7a1e17e47447ee5b1", "BlueprintAbility"),
    }
    for symbol, (guid, planned_type) in expected.items():
        entry = by_symbol.get(symbol)
        if (
            entry is None
            or entry.get("guid") != guid
            or entry.get("plannedType") != planned_type
            or entry.get("status") != "active"
        ):
            fail(f"Blueprint ledger entry is incorrect: {symbol}")

    main_sources = compile_items(root, "src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    test_sources = compile_items(root, "tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj")
    required_main = {
        "RepairTestMusketAbilityBlueprints.cs",
        "FirearmRepairTransactionService.cs",
        "FirearmRepairRuntimeResult.cs",
        "RepairTestMusketAbilityLogic.cs",
        "RepairTestMusketRuntime.cs",
        "RepairRuntimeDiagnostics.cs",
        "MaintenanceQualificationBaseline.cs",
        "MaintenanceQualificationObservation.cs",
        "MaintenanceQualificationReport.cs",
        "MaintenanceQualificationService.cs",
        "MaintenanceQualificationSession.cs",
        "MaintenanceQualificationStage.cs",
        "KingmakerDevelopmentBridge.Sprint29.cs",
    }
    if not required_main.issubset({path.name for path in main_sources}):
        fail("Main project does not compile every Sprint 29 source file.")
    required_tests = {
        "FirearmRepairTransactionService.cs",
        "FirearmRepairRuntimeResult.cs",
        "MaintenanceQualificationBaseline.cs",
        "MaintenanceQualificationObservation.cs",
        "MaintenanceQualificationReport.cs",
        "MaintenanceQualificationService.cs",
        "MaintenanceQualificationSession.cs",
        "MaintenanceQualificationStage.cs",
        "Sprint29Tests.cs",
    }
    if not required_tests.issubset({path.name for path in test_sources}):
        fail("Domain test project does not link every Sprint 29 policy/test source.")

    bootstrap = read(root, "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs")
    require_tokens(
        bootstrap,
        [
            f"ExpectedRegisteredBlueprintCount = {expected_registered_blueprints}",
            "RepairTestMusketAbilityBlueprints.Register",
            "repairTestMusketAbility",
            "AttachReload",
            "GunsmithingBlueprints.Register",
            "_repairTestMusketAbility",
        ],
        "Blueprint bootstrap",
    )

    proficiency = read(root, "src/KingmakerGunslinger/Blueprints/GunsmithingBlueprints.cs")
    require_tokens(
        proficiency,
        [
            "BlueprintAbility repairAbility",
            "grants[0].Facts.Length != 2",
            "grants[0].Facts[0], overhaulAbility",
            "grants[0].Facts[1], repairAbility",
            "DoNotRestoreMissingFacts = false",
        ],
        "Gunsmithing maintenance grant",
    )

    transaction = read(root, "src/KingmakerGunslinger/Recovery/FirearmRepairTransactionService.cs")
    require_tokens(
        transaction,
        [
            "TryRepairBrokenToNormal",
            "FirearmStateMachine.Repair",
            "inventory.Remove(1)",
            "RestoreState",
            "RestoreInventory",
            "Rollback refused to overwrite an unexpected concurrent firearm state",
        ],
        "Ordinary repair transaction",
    )
    runtime = read(root, "src/KingmakerGunslinger/Recovery/RepairTestMusketRuntime.cs")
    require_tokens(
        runtime,
        [
            "TryResolveSingleEquippedTestMusket",
            "More than one distinct firearm is equipped",
            "FirearmCondition.Broken",
            "A Wrecked firearm must be Overhauled",
            "One Firearm Repair Kit is required",
            "FirearmRepairRuntimeResult",
        ],
        "Repair runtime",
    )
    ability = read(root, "src/KingmakerGunslinger/Blueprints/RepairTestMusketAbilityBlueprints.cs")
    require_tokens(
        ability,
        [
            'Symbol = "KMG.Test.RepairAbility"',
            "AbilityType.Extraordinary",
            "AbilityRange.Personal",
            "SetIsFullRoundAction(true)",
            "RepairTestMusketAbilityLogic.Create",
        ],
        "Repair ability blueprint",
    )
    logic = read(root, "src/KingmakerGunslinger/Recovery/RepairTestMusketAbilityLogic.cs")
    require_tokens(
        logic,
        [
            "Mutation occurs only in Deliver",
            "RepairTestMusketRuntime.Evaluate",
            "RepairTestMusketRuntime.Execute",
            "RepairRuntimeDiagnostics.Record",
        ],
        "Repair ability delivery",
    )

    qualification = read(root, "src/KingmakerGunslinger/Qualification/MaintenanceQualificationService.cs")
    require_tokens(
        qualification,
        [
            "FixtureReady",
            "OverhaulPassed",
            "RepairPassed",
            "MaintenanceLoopPassed",
            '"exactItem"',
            '"secondItem"',
            '"faults"',
            '"duplicates"',
            "baseline.Revision + 3",
            "baseline.RepairKits - 2",
            "baseline.BlackPowder - 1",
            "baseline.LeadBalls - 1",
        ],
        "Maintenance qualification evaluator",
    )
    bridge = read(root, "src/KingmakerGunslinger/Development/KingmakerDevelopmentBridge.Sprint29.cs")
    require_tokens(
        bridge,
        [
            "PrepareMaintenanceQualificationFixture",
            "RunMaintenanceQualificationImmediately",
            "DescribeMaintenanceQualification",
            "FirearmStateMachine.Wreck(FirearmState.CreateEmpty())",
            "FirearmState.CreateEmpty()",
            "repairKits.Add(2 - repairKitCount)",
            "MaintenanceQualificationSession.Begin",
            "This diagnostic bypasses action economy",
        ],
        "Sprint 29 development harness",
    )

    program = read(root, "tests/KingmakerGunslinger.DomainTests/Program.cs")
    case_count = len(re.findall(r'Case\("[^"]+",\s*[A-Za-z0-9_]+\)', program))
    if case_count != test_count:
        fail(f"Expected {test_count} declared tests, observed {case_count}.")
    require_tokens(
        program,
        [
            'Case("repair.transaction.success"',
            'Case("repair.transaction.post-state-mutation-failure-restores-both"',
            'Case("repair.transaction.inventory-rollback-failure-surfaced"',
            'Case("repair.runtime-result.success"',
            'Case("maintenance.fixture-pass"',
            'Case("maintenance.loop-pass"',
            'Case("maintenance.session-lifecycle"',
        ],
        "Sprint 29 tests",
    )

    ui = read(root, "src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    require_tokens(
        ui,
        [
            "0.0.29 Sprint 29 complete maintenance-loop smoke test",
            "Repair runtime:",
            "Print Repair Test Musket readiness",
            "Repair equipped Test Musket immediately",
            "Prepare Sprint 29 maintenance qualification fixture",
            "Run complete maintenance qualification immediately",
            "Print Sprint 29 maintenance PASS/FAIL matrix",
        ],
        "Development UI",
    )

    guide = read(root, "SMOKE-TEST-GUIDE-0.0.29.md")
    if require_current_guide_match and guide != read(root, "SMOKE-TEST-GUIDE.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.29 guide.")
    require_tokens(
        guide,
        [
            "Run the one-command transaction regression",
            "stage=MaintenanceLoopPassed",
            "Verify Overhaul interruption",
            "Verify Repair interruption",
            "Complete ordinary Repair",
            "repair kits: another -1",
            "condition=Normal",
            "blocks Sprint 30",
        ],
        "0.0.29 smoke-test guide",
    )

    for required in [
        "README.md",
        "KNOWN-ISSUES.md",
        "SPRINT-29-REPORT.md",
        "docs/FIREARM-COMPLETE-MAINTENANCE-LOOP.md",
        "docs/decisions/ADR-0036-complete-maintenance-loop-and-qualification-harness.md",
        "planning/SPRINT-30-ENTRY-CRITERIA.md",
        "planning/ACCELERATION-PLAN.md",
        "planning/ROADMAP-SPRINTS-29-38.md",
        "evidence/sprint28-runtime-acceptance/final-2026-07-17/ASSESSMENT.md",
        "validation/sprint28-runtime-acceptance.sha256",
    ]:
        read(root, required)

    evidence_root = root / "evidence/sprint28-runtime-acceptance/final-2026-07-17"
    checksum_lines = read(root, "validation/sprint28-runtime-acceptance.sha256").splitlines()
    if len(checksum_lines) != 6:
        fail("Sprint 28 acceptance checksum fixture must contain exactly six files.")
    for line in checksum_lines:
        expected_hash, name = line.split("  ", 1)
        evidence_path = evidence_root / name
        if not evidence_path.is_file():
            fail(f"Sprint 28 acceptance evidence is missing: {name}")
        actual_hash = hashlib.sha256(evidence_path.read_bytes()).hexdigest()
        if actual_hash != expected_hash:
            fail(f"Sprint 28 acceptance evidence hash mismatch: {name}")

    architecture = read(root, "docs/ARCHITECTURE.md")
    active_words = ({14: "fourteen", 59: "fifty-nine", 61: "sixty-one",
                     64: "sixty-four", 84: "eighty-four",
                     87: "eighty-seven", 90: "ninety",
                     94: "ninety-four", 96: "ninety-six",
                     101: "one-hundred-one", 102: "one-hundred-two",
                     106: "one-hundred-six", 125: "one-hundred-twenty-five",
                     126: "one-hundred-twenty-six",
                     127: "one-hundred-twenty-seven",
                     128: "one-hundred-twenty-eight",
                     131: "one-hundred-thirty-one",
                     136: "one-hundred-thirty-six",
                     140: "one-hundred-forty",
                     152: "one-hundred-fifty-two"}
                    .get(expected_active_blueprints, "twenty-four"))
    ledger_summary = (f"{expected_ledger_entries} stable IDs: "
                      f"{expected_active_blueprints} active and one reserved")
    require_tokens(
        architecture,
        [
            "## Sprint 29 current layer",
            active_words + " active blueprints",
            "Repair ability delivery",
            "## Sprint 29 authoritative maintenance layer",
            ledger_summary,
        ],
        "Architecture",
    )
    manifest_doc = read(root, "docs/BLUEPRINT-MANIFEST.md")
    require_tokens(
        manifest_doc,
        [
            (f"{expected_ledger_entries} stable identifiers: "
             f"{expected_active_blueprints} active and one reserved"),
            "complete " + active_words + "-blueprint transaction",
            "KMG.Test.RepairAbility",
            "c914b3c0786463b7a1e17e47447ee5b1",
        ],
        "Blueprint manifest documentation",
    )

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
        if path.suffix.lower() in {".dll", ".exe", ".pdb", ".mdb", ".pyc"}:
            fail(f"Compiled binary/cache is present in source tree: {path.relative_to(root)}")
        lower = path.name.lower()
        if "private-build-references" in lower and path.suffix.lower() in {".zip", ".7z", ".rar", ".tar", ".gz"}:
            fail(f"Private reference bundle is present in source tree: {path.relative_to(root)}")

    if final:
        compile_report = json.loads(read(root, "evidence/sprint29/compile/deterministic-compile-evidence.json"))
        test_report = json.loads(read(root, "evidence/sprint29/tests/executed-test-evidence.json"))
        package_report = json.loads(read(root, "evidence/sprint29/package/standalone-package-evidence.json"))
        build_qualification = json.loads(read(root, "BUILD-QUALIFICATION.json"))
        if not compile_report.get("dllByteIdentical") or not compile_report.get("pdbByteIdentical"):
            fail("Final compile evidence is not deterministic.")
        if compile_report.get("warningsAsErrors") is not True or compile_report.get("sameOutputPathCompileRuns") != 2:
            fail("Final compile evidence does not satisfy the qualification contract.")
        if (
            test_report.get("declaredTests") != test_count
            or test_report.get("runs") != 3
            or test_report.get("failures") != 0
            or test_report.get("repeatedOutputIdentical") is not True
        ):
            fail("Final executed-test evidence is incomplete.")
        if (
            package_report.get("entryCount") != 8
            or package_report.get("binaryCount") != 1
            or package_report.get("privateReferencesRedistributed") is not False
        ):
            fail("Final standalone package evidence is incomplete.")
        if build_qualification.get("modVersion") != version or build_qualification.get("informationalVersion") != informational_version:
            fail("BUILD-QUALIFICATION.json still describes another sprint.")
        if (
            build_qualification.get("declaredTests") != test_count
            or build_qualification.get("testRuns") != 3
            or build_qualification.get("testFailures") != 0
        ):
            fail("BUILD-QUALIFICATION.json has incorrect test evidence.")
        if build_qualification.get("sprint28RuntimeAcceptancePassed") is not True:
            fail("BUILD-QUALIFICATION.json does not record Sprint 28 runtime acceptance.")
        if (
            build_qualification.get("readyForKingmakerSmokeTest") is not True
            or "pending" not in build_qualification.get("runtimeAcceptance", "").lower()
        ):
            fail("BUILD-QUALIFICATION.json has an invalid runtime classification.")

    print("Sprint 29 source invariant validation passed" + (" with final evidence." if final else "."))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--final", action="store_true")
    args = parser.parse_args()
    try:
        validate(args.final)
    except Exception as exception:
        print(f"Sprint 29 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
