#!/usr/bin/env python3
"""Portable source and optional final-evidence validator for 0.0.24 Sprint 24."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
EXPECTED_ASSEMBLY_CSHARP = "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb"


def fail(message: str) -> None:
    raise RuntimeError(message)


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.is_file():
        fail(f"Required file is missing: {relative}")
    return path.read_text(encoding="utf-8")


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def compile_items(relative: str) -> list[Path]:
    project = ROOT / relative
    tree = ET.parse(project)
    items: list[Path] = []
    for node in tree.findall(".//m:Compile", MSBUILD_NS):
        include = node.attrib.get("Include")
        if not include:
            continue
        path = (project.parent / include.replace("\\", "/")).resolve()
        if not path.is_file():
            fail(f"Compile item does not exist: {include} -> {path}")
        items.append(path)
    if not items or len(items) != len(set(items)):
        fail(f"Compile items are empty or duplicated in {relative}.")
    return items


def require(text: str, values: tuple[str, ...], label: str) -> None:
    for value in values:
        if value not in text:
            fail(f"{label} is missing {value!r}.")


def validate_checksum_file(relative: str) -> None:
    path = ROOT / relative
    if not path.is_file():
        fail(f"Checksum file is missing: {relative}")
    for line in path.read_text(encoding="ascii").splitlines():
        if not line.strip():
            continue
        match = re.fullmatch(r"([0-9a-f]{64})  (.+)", line)
        if match is None:
            fail(f"Malformed checksum line in {relative}: {line!r}")
        expected, name = match.groups()
        target = path.parent / name
        if not target.is_file() or sha256(target) != expected:
            fail(f"Checksum mismatch in {relative}: {name}")


def validate_structure() -> tuple[list[Path], list[Path]]:
    info = json.loads(read("Info.json"))
    if info.get("Version") != "0.0.24":
        fail("Info.json must declare version 0.0.24.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json must retain Unity Mod Manager 0.32.4.")
    if info.get("AssemblyName") != "KingmakerGunslinger.dll":
        fail("Info.json names the wrong assembly.")

    require(
        read("Directory.Build.props"),
        (
            "<KmgVersion>0.0.24</KmgVersion>",
            "<KmgInformationalVersion>0.0.24-s24-misfire-condition-transitions</KmgInformationalVersion>",
            "<LangVersion>7.3</LangVersion>",
            "<PlatformTarget>AnyCPU</PlatformTarget>",
            "<Deterministic>true</Deterministic>",
            "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
        ),
        "Directory.Build.props",
    )
    require(
        read("src/KingmakerGunslinger/Properties/AssemblyInfo.cs"),
        (
            'AssemblyVersion("0.0.24")',
            'AssemblyFileVersion("0.0.24")',
            'AssemblyInformationalVersion("0.0.24-s24-misfire-condition-transitions")',
        ),
        "AssemblyInfo.cs",
    )

    manifest = json.loads(read("blueprints/blueprints.json"))
    entries = manifest.get("entries", [])
    if len(entries) != 12:
        fail("Blueprint ledger must retain twelve stable IDs.")
    if sum(entry.get("status") == "active" for entry in entries) != 11:
        fail("Blueprint ledger must retain eleven active IDs.")
    if sum(entry.get("status") == "reserved" for entry in entries) != 1:
        fail("Blueprint ledger must retain one reserved ID.")
    if any(re.fullmatch(r"[0-9a-f]{32}", entry.get("guid", "")) is None for entry in entries):
        fail("Blueprint ledger contains an invalid GUID.")

    main_items = compile_items("src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    test_items = compile_items("tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj")
    misfire_dir = ROOT / "src/KingmakerGunslinger/Misfires"
    required_main = {
        (misfire_dir / name).resolve()
        for name in (
            "FirearmMisfireConditionDecision.cs",
            "FirearmMisfireConditionService.cs",
            "FirearmMisfireConditionTransition.cs",
            "FirearmMisfireDecision.cs",
            "FirearmMisfirePatchContract.cs",
            "FirearmMisfirePatchTarget.cs",
            "FirearmMisfirePatches.cs",
            "FirearmMisfireRuntime.cs",
            "FirearmMisfireRuntimeDiagnostics.cs",
            "FirearmMisfireService.cs",
            "ForcedNaturalRollQueue.cs",
        )
    }
    if not required_main.issubset(set(main_items)):
        fail("The main project does not compile all Sprint 24 misfire files.")
    required_test = {
        (misfire_dir / name).resolve()
        for name in (
            "FirearmMisfireConditionDecision.cs",
            "FirearmMisfireConditionService.cs",
            "FirearmMisfireConditionTransition.cs",
            "FirearmMisfireDecision.cs",
            "FirearmMisfirePatchContract.cs",
            "FirearmMisfireService.cs",
            "ForcedNaturalRollQueue.cs",
        )
    }
    if not required_test.issubset(set(test_items)):
        fail("The test project does not compile the pure Sprint 24 sources.")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    cases = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', program)
    if len(cases) != 501 or len(cases) != len(set(cases)):
        fail(f"Expected 501 unique declared tests, observed {len(cases)}.")
    required_cases = {
        "misfire.natural-one-forces-miss",
        "misfire.natural-two-forces-miss",
        "misfire.above-threshold-preserves-hit",
        "misfire-condition.ordinary-normal",
        "misfire-condition.ordinary-broken",
        "misfire-condition.normal-to-broken",
        "misfire-condition.broken-to-wrecked",
        "misfire-condition.loaded-rejected",
        "misfire-condition.wrecked-rejected",
        "misfire-condition.misfire-none-rejected",
        "misfire-condition.ordinary-transition-rejected",
        "misfire-condition.unknown-transition-rejected",
        "forced-roll.set-consume",
        "misfire-patch.roll-setter-exact",
        "misfire-patch.success-exact",
    }
    if not required_cases.issubset({name for name, _ in cases}):
        fail("Required Sprint 24 test declarations are missing.")

    service = read("src/KingmakerGunslinger/Misfires/FirearmMisfireConditionService.cs")
    require(
        service,
        (
            "if (!postDischargeState.IsEmpty)",
            "postDischargeState.Condition == FirearmCondition.Wrecked",
            "FirearmStateMachine.ApplyMisfireDamage",
            "FirearmMisfireConditionTransition.NormalToBroken",
            "FirearmMisfireConditionTransition.BrokenToWrecked",
        ),
        "FirearmMisfireConditionService",
    )
    decision = read("src/KingmakerGunslinger/Misfires/FirearmMisfireConditionDecision.cs")
    require(
        decision,
        (
            "if (!before.IsEmpty || !after.IsEmpty)",
            "A detected misfire requires one of the bounded Sprint 24 condition transitions.",
            "NormalToBroken requires an empty Normal state followed by an empty Broken state.",
            "BrokenToWrecked requires an empty Broken state followed by an empty Wrecked state.",
        ),
        "FirearmMisfireConditionDecision",
    )

    runtime = read("src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs")
    require(
        runtime,
        (
            "ConditionalWeakTable<RuleAttackRoll, EligibleAttackContext>",
            "object firearmItem",
            "FirearmItemStateSnapshot postDischarge",
            "ForcedRolls.TryConsume",
            "bool firstEvaluation = context.TryBeginEvaluation();",
            "ConditionService.Evaluate",
            "CommitConditionTransition(context, condition);",
            "FirearmRuntimeState.Service.Transition",
            "current != condition.Before",
            "committed.Repository.State != condition.After",
            "context.RepositoryIdentity",
            "RecordCompletedWithoutNaturalRoll",
        ),
        "FirearmMisfireRuntime",
    )
    if runtime.find("bool firstEvaluation = context.TryBeginEvaluation();") > runtime.find("CommitConditionTransition(context, condition);"):
        fail("The per-attack evaluation gate does not precede condition mutation.")
    if "UniqueId" in runtime:
        fail("Sprint 24 runtime revived the rejected item UniqueId vault.")

    discharge = read("src/KingmakerGunslinger/Firing/FirearmDischargeRuntime.cs")
    fired_index = discharge.find("result.Status == FirearmDischargeStatus.Fired")
    transition_index = discharge.find("FirearmRuntimeState.Service.Transition")
    register_index = discharge.find("FirearmMisfireRuntime.TryRegisterEligibleAttack")
    if min(fired_index, transition_index, register_index) < 0 or not (fired_index < transition_index < register_index):
        fail("Misfire eligibility is not registered after the exact verified Fired transition.")
    require(discharge, ("attackRoll,", "weapon,", "after)"), "FirearmDischargeRuntime")

    diagnostics = read("src/KingmakerGunslinger/Misfires/FirearmMisfireRuntimeDiagnostics.cs")
    require(
        diagnostics,
        (
            "normalToBroken={4}",
            "brokenToWrecked={5}",
            "FirearmMisfireConditionTransition.NormalToBroken",
            "FirearmMisfireConditionTransition.BrokenToWrecked",
            "no condition damage occurred",
        ),
        "FirearmMisfireRuntimeDiagnostics",
    )

    patches = read("src/KingmakerGunslinger/Misfires/FirearmMisfirePatches.cs")
    contract = read("src/KingmakerGunslinger/Misfires/FirearmMisfirePatchContract.cs")
    require(patches, ("ref RulebookEvent.RollEntry value", "int d20", "ref bool __result"), "FirearmMisfirePatches")
    require(
        contract,
        (
            'method.Name, "set_Roll"',
            "method.IsPrivate",
            "method.IsSpecialName",
            'method.Name, "IsSuccessRoll"',
            "method.IsPublic",
            "parameters[0].ParameterType == typeof(int)",
        ),
        "FirearmMisfirePatchContract",
    )

    controls = read("src/KingmakerGunslinger/Development/DevelopmentControls.cs")
    ui = read("src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    for natural, word in ((1, "One"), (2, "Two"), (3, "Three"), (20, "Twenty")):
        if f"ForceNextFirearmNaturalRoll{word}" not in controls:
            fail(f"Force-next-roll control for {natural} is missing.")
        if f"natural d20 to {natural}" not in ui:
            fail(f"UMM force-next-roll button for {natural} is missing.")
    require(
        ui,
        (
            "0.0.24 Sprint 24 misfire-condition smoke test",
            "Normal to Broken, then Broken to Wrecked",
            "Normal misfire becomes Broken and a Broken misfire becomes Wrecked",
            "Natural-roll misfires: ",
        ),
        "DevelopmentUi",
    )

    if read("SMOKE-TEST-GUIDE.md") != read("SMOKE-TEST-GUIDE-0.0.24.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.24 guide.")
    for required in (
        "SPRINT-24-REPORT.md",
        "docs/FIREARM-MISFIRE-CONDITION-TRANSITIONS.md",
        "docs/decisions/ADR-0030-exact-item-misfire-condition-transitions.md",
        "planning/SPRINT-25-ENTRY-CRITERIA.md",
        "evidence/sprint23-runtime-acceptance/user-approved-carry-forward-2026-07-16/ASSESSMENT.md",
        "evidence/sprint23/runtime-contracts/exact-rule-attack-roll-contracts.json",
    ):
        read(required)
    validate_checksum_file(
        "evidence/sprint23-runtime-acceptance/user-approved-carry-forward-2026-07-16/SHA256SUMS.txt"
    )
    assessment = read(
        "evidence/sprint23-runtime-acceptance/user-approved-carry-forward-2026-07-16/ASSESSMENT.md"
    )
    require(
        assessment,
        (
            "Sprint 24 approved by the user with explicit carry-forward",
            "Controls not claimed as separately observed",
            "they are carried into `SMOKE-TEST-GUIDE-0.0.24.md`",
        ),
        "Sprint 23 carry-forward assessment",
    )

    contracts = json.loads(read("evidence/sprint23/runtime-contracts/exact-rule-attack-roll-contracts.json"))
    if contracts.get("assemblyCSharpSha256") != EXPECTED_ASSEMBLY_CSHARP:
        fail("Retained natural-roll contract evidence references the wrong Assembly-CSharp.dll.")
    c = contracts.get("contracts", {})
    if c.get("mainRollSetter", {}).get("signature") != "private System.Void set_Roll(Kingmaker.RuleSystem.RulebookEvent+RollEntry value)":
        fail("Exact private Roll setter evidence is missing or changed.")
    if c.get("successEvaluator", {}).get("signature") != "public System.Boolean IsSuccessRoll(System.Int32 d20)":
        fail("Exact IsSuccessRoll evidence is missing or changed.")

    forbidden_binary_suffixes = {".dll", ".exe", ".pdb", ".mdb"}
    loose_binaries = [
        path.relative_to(ROOT).as_posix()
        for path in ROOT.rglob("*")
        if path.is_file() and path.suffix.lower() in forbidden_binary_suffixes
    ]
    if loose_binaries:
        fail(f"Source tree contains loose binaries/private-reference risk: {loose_binaries[:5]}")

    return main_items, test_items


def validate_final() -> None:
    qualification = json.loads(read("BUILD-QUALIFICATION.json"))
    expected = {
        "informationalVersion": "0.0.24-s24-misfire-condition-transitions",
        "declaredTests": 501,
        "testRuns": 3,
        "testFailures": 0,
        "installPackageEntryCount": 8,
        "installPackageBinaryCount": 1,
        "sprint22RuntimeAcceptancePassed": True,
        "sprint23CoreRuntimeBehaviorObserved": True,
        "sprint24EntryApprovedByUserCarryForward": True,
        "sprint24RuntimeAcceptancePassed": False,
        "sprint25EntryApproved": False,
        "misfireConditionTransitionsImplemented": True,
        "privateReferencesRedistributed": False,
    }
    for key, value in expected.items():
        if qualification.get(key) != value:
            fail(f"BUILD-QUALIFICATION.json has unexpected {key}: expected {value!r}, observed {qualification.get(key)!r}")
    for key in (
        "sourceInvariantValidationPassed",
        "exactRuntimeContractEvidencePassed",
        "sameOutputPathDllByteIdentical",
        "sameOutputPathPdbByteIdentical",
        "testOutputRepeatedByteIdentical",
        "installPackageValidationPassed",
        "readyForKingmakerSmokeTest",
    ):
        if qualification.get(key) is not True:
            fail(f"BUILD-QUALIFICATION.json does not affirm {key}.")

    dll_hash = qualification.get("modDllSha256")
    pdb_hash = qualification.get("modPdbSha256")
    test_hash = qualification.get("testOutputSha256")
    package_hash = qualification.get("installPackageSha256")
    for label, value in (("DLL", dll_hash), ("PDB", pdb_hash), ("test output", test_hash), ("package", package_hash)):
        if re.fullmatch(r"[0-9a-f]{64}", value or "") is None:
            fail(f"BUILD-QUALIFICATION.json has an invalid {label} hash.")

    compile_evidence = json.loads(read("evidence/sprint24/compile/deterministic-compile-evidence.json"))
    if compile_evidence.get("dll", {}).get("secondSha256") != dll_hash or compile_evidence.get("dll", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative DLL.")
    if compile_evidence.get("pdb", {}).get("secondSha256") != pdb_hash or compile_evidence.get("pdb", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative PDB.")

    test_evidence = json.loads(read("evidence/sprint24/tests/executed-test-evidence.json"))
    if test_evidence.get("declaredTests") != 501 or test_evidence.get("runs") != 3:
        fail("Executed test evidence has the wrong count or run count.")
    if test_evidence.get("failures") != 0 or test_evidence.get("repeatedOutputIdentical") is not True:
        fail("Executed test evidence does not prove three clean identical runs.")
    if test_evidence.get("testOutputSha256") != test_hash:
        fail("Executed test evidence has the wrong output hash.")

    package_evidence = json.loads(read("evidence/sprint24/package/standalone-package-evidence.json"))
    if package_evidence.get("sha256") != package_hash or package_evidence.get("modDllSha256") != dll_hash:
        fail("Standalone package evidence has the wrong package or DLL hash.")
    if package_evidence.get("entryCount") != 8 or package_evidence.get("binaryCount") != 1:
        fail("Standalone package evidence has the wrong entry or binary count.")
    if package_evidence.get("privateReferencesRedistributed") is not False:
        fail("Standalone package evidence violates the private-reference boundary.")

    report = read("SPRINT-24-REPORT.md")
    if "PENDING_FINAL_QUALIFICATION" in report:
        fail("SPRINT-24-REPORT.md still contains a qualification placeholder.")
    require(
        report,
        (
            "501 tests × 3 runs, 0 failures",
            dll_hash,
            pdb_hash,
            test_hash,
            package_hash,
            "READY FOR KINGMAKER — Sprint 24 misfire-condition smoke test",
            "user-approved carry-forward",
        ),
        "SPRINT-24-REPORT.md",
    )
    require(
        read("BUILD-INFO.txt"),
        (
            "Version: 0.0.24-s24-misfire-condition-transitions",
            "Exact dependency-free tests: 501 x 3 runs; 0 failures",
            dll_hash,
            package_hash,
            "Sprint 25 status: blocked",
        ),
        "BUILD-INFO.txt",
    )
    read("VALIDATION-RESULTS-S24.txt")
    validate_checksum_file("evidence/sprint24/compile/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint24/tests/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint24/package/SHA256SUMS.txt")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--final", action="store_true", help="also validate sealed qualification evidence")
    args = parser.parse_args()
    main_items, test_items = validate_structure()
    if args.final:
        validate_final()
    print("Sprint 24 source invariant validation passed" + (" with final evidence." if args.final else "."))
    print("Blueprints: 12 stable / 11 active / 1 reserved.")
    print(f"Compile items: main={len(main_items)}; tests={len(test_items)}.")
    print("Declared dependency-free tests: 501 unique.")
    print("Condition transitions: exact-item Normal->Broken and Broken->Wrecked only.")
    print("Private runtime/compiler binaries in source tree: none.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print(f"Sprint 24 validation failed: {exception}", file=sys.stderr)
        raise SystemExit(1)
