#!/usr/bin/env python3
"""Portable source invariant validator for 0.0.23 Sprint 23."""
from __future__ import annotations

import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
EXPECTED_ASSEMBLY_CSHARP = "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb"
EXPECTED_MOD_DLL = "1d04c99705279524d4877888d3803fded1ab04ca988669b501c53db451e47a63"
EXPECTED_MOD_PDB = "684d9b4eeaa85303062fcc75f46b30ed9f482e818f1de8200debbf3ea488ab1e"
EXPECTED_TEST_OUTPUT = "86c65e395cc29674a5776e6f6f2468aa31ff57808fc5836de8fe1f51574da5f6"
EXPECTED_STANDALONE_PACKAGE = "d09a3ced53c8f77c5b11dee9feb20b4e409a35d2dc67922ee81740d2d82c21b2"
EXPECTED_SCREENSHOTS = {
    "05-loaded-empty-save-restart-pass.png": "cc595531abbeb01eea08a9a88dbebd49a2e62ea8f8ced29a9311a72e8d08c970",
    "06-loaded-broken-before-fire.png": "0f0ec8c69bdd44caf5f31fef4a03c37b79057f14f1bf156bb1ac0e71f27b3f63",
    "07-broken-fired-remains-broken.png": "72800af3628cdebce6daa0f02bf94599a26c9a1c55b5cc931580af64eb10802c",
    "08-wrecked-before-rejection.png": "54ef070bd0a7f0437b06c8ab0c18d0648535feed83c367c14900c4bf66bda645",
    "09-wrecked-attack-rejected.png": "79f1eab9a94805fd3d40733843b80092f9f1483aec9de8ede68aec7511679f74",
    "10-native-heavy-crossbow-isolated.png": "f2cbfec03a653cd3e8eee64090d3f91b5909edaffc144b24a489a266c4969744",
}


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


def main() -> int:
    info = json.loads(read("Info.json"))
    if info.get("Version") != "0.0.23":
        fail("Info.json must declare version 0.0.23.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json must retain Unity Mod Manager 0.32.4.")
    if info.get("AssemblyName") != "KingmakerGunslinger.dll":
        fail("Info.json names the wrong assembly.")

    props = read("Directory.Build.props")
    require(
        props,
        (
            "<KmgVersion>0.0.23</KmgVersion>",
            "<KmgInformationalVersion>0.0.23-s23-natural-roll-misfire</KmgInformationalVersion>",
            "<LangVersion>7.3</LangVersion>",
            "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
        ),
        "Directory.Build.props",
    )
    assembly_info = read("src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
    require(
        assembly_info,
        (
            'AssemblyVersion("0.0.23")',
            'AssemblyFileVersion("0.0.23")',
            'AssemblyInformationalVersion("0.0.23-s23-natural-roll-misfire")',
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
    required_main = {
        (ROOT / "src/KingmakerGunslinger/Misfires" / name).resolve()
        for name in (
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
        fail("The main project does not compile all Sprint 23 misfire files.")
    required_test = {
        (ROOT / "src/KingmakerGunslinger/Misfires" / name).resolve()
        for name in (
            "FirearmMisfireDecision.cs",
            "FirearmMisfirePatchContract.cs",
            "FirearmMisfireService.cs",
            "ForcedNaturalRollQueue.cs",
        )
    }
    if not required_test.issubset(set(test_items)):
        fail("The test project does not compile the pure Sprint 23 sources.")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    cases = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', program)
    if len(cases) != 489 or len(cases) != len(set(cases)):
        fail(f"Expected 489 unique declared tests, observed {len(cases)}.")
    required_case_prefixes = {
        "misfire.natural-one-forces-miss",
        "misfire.natural-two-forces-miss",
        "misfire.above-threshold-preserves-hit",
        "forced-roll.set-consume",
        "forced-roll.replace",
        "misfire-patch.roll-setter-exact",
        "misfire-patch.success-exact",
    }
    if not required_case_prefixes.issubset({name for name, _ in cases}):
        fail("Required Sprint 23 test declarations are missing.")

    decision = read("src/KingmakerGunslinger/Misfires/FirearmMisfireDecision.cs")
    require(
        decision,
        (
            "IsMisfire = naturalRoll <= misfireValue;",
            "FinalSuccess = nativeSuccess && !IsMisfire;",
        ),
        "FirearmMisfireDecision",
    )
    runtime = read("src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs")
    require(
        runtime,
        (
            "ConditionalWeakTable<RuleAttackRoll, EligibleAttackContext>",
            "ForcedRolls.TryConsume",
            "naturalRoll != context.FinalNaturalRoll",
            "FinishAttack(RuleAttackRoll attackRoll)",
            "RecordCompletedWithoutNaturalRoll",
        ),
        "FirearmMisfireRuntime",
    )
    for forbidden in (
        "FirearmStateMachine.Misfire",
        "FirearmCondition.Broken",
        "FirearmCondition.Wrecked",
        "FirearmRuntimeState.Service.Transition",
    ):
        if forbidden in runtime:
            fail(f"Sprint 23 runtime contains a deferred condition mutation: {forbidden}")

    discharge = read("src/KingmakerGunslinger/Firing/FirearmDischargeRuntime.cs")
    fired_index = discharge.find("result.Status == FirearmDischargeStatus.Fired")
    register_index = discharge.find("FirearmMisfireRuntime.TryRegisterEligibleAttack")
    if fired_index < 0 or register_index < fired_index:
        fail("Misfire eligibility is not registered only after a Fired decision.")
    require(
        discharge,
        (
            "marker.IsExactFirearm",
            "FirearmRuntimeState.Service.Transition",
            "ForceMiss(attackRoll);",
        ),
        "FirearmDischargeRuntime",
    )

    patches = read("src/KingmakerGunslinger/Misfires/FirearmMisfirePatches.cs")
    contract = read("src/KingmakerGunslinger/Misfires/FirearmMisfirePatchContract.cs")
    require(
        patches,
        (
            "ref RulebookEvent.RollEntry value",
            "int d20",
            "ref bool __result",
        ),
        "FirearmMisfirePatches",
    )
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
    for natural in (1, 2, 3, 20):
        if f"ForceNextFirearmNaturalRoll{ {1:'One',2:'Two',3:'Three',20:'Twenty'}[natural] }" not in controls:
            fail(f"Force-next-roll control for {natural} is missing.")
        if f"natural d20 to {natural}" not in ui:
            fail(f"UMM force-next-roll button for {natural} is missing.")
    require(ui, ("Natural-roll misfires: ", "condition unchanged", "pending"), "DevelopmentUi")

    if read("SMOKE-TEST-GUIDE.md") != read("SMOKE-TEST-GUIDE-0.0.23.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.23 guide.")
    for required in (
        "SPRINT-23-REPORT.md",
        "docs/NATURAL-ROLL-MISFIRE-DETECTION.md",
        "docs/decisions/ADR-0029-exact-natural-roll-misfire-hooks.md",
        "planning/SPRINT-24-ENTRY-CRITERIA.md",
        "evidence/sprint22-runtime-acceptance/ASSESSMENT-2026-07-15.md",
        "evidence/sprint23/runtime-contracts/exact-rule-attack-roll-contracts.json",
    ):
        read(required)

    contracts = json.loads(read("evidence/sprint23/runtime-contracts/exact-rule-attack-roll-contracts.json"))
    if contracts.get("assemblyCSharpSha256") != EXPECTED_ASSEMBLY_CSHARP:
        fail("Sprint 23 contract evidence references the wrong Assembly-CSharp.dll.")
    c = contracts.get("contracts", {})
    if c.get("mainRollSetter", {}).get("signature") != (
        "private System.Void set_Roll(Kingmaker.RuleSystem.RulebookEvent+RollEntry value)"
    ):
        fail("Exact private Roll setter evidence is missing or changed.")
    if c.get("successEvaluator", {}).get("signature") != (
        "public System.Boolean IsSuccessRoll(System.Int32 d20)"
    ):
        fail("Exact IsSuccessRoll evidence is missing or changed.")
    fields = {field.get("name"): field.get("type") for field in c.get("rollEntry", {}).get("fields", [])}
    if fields != {
        "Value": "System.Int32",
        "RollHistory": "System.Collections.Generic.List<System.Int32>",
        "RerollSource": "System.String",
    }:
        fail(f"Unexpected RollEntry field evidence: {fields!r}")

    screenshot_dir = ROOT / "evidence/sprint22-runtime-acceptance/screenshots"
    for name, expected in EXPECTED_SCREENSHOTS.items():
        path = screenshot_dir / name
        if not path.is_file() or sha256(path) != expected:
            fail(f"Accepted Sprint 22 runtime screenshot is missing or changed: {name}")

    qualification = json.loads(read("BUILD-QUALIFICATION.json"))
    expected_qualification = {
        "informationalVersion": "0.0.23-s23-natural-roll-misfire",
        "declaredTests": 489,
        "testRuns": 3,
        "testFailures": 0,
        "testOutputSha256": EXPECTED_TEST_OUTPUT,
        "modDllSha256": EXPECTED_MOD_DLL,
        "modPdbSha256": EXPECTED_MOD_PDB,
        "installPackageSha256": EXPECTED_STANDALONE_PACKAGE,
        "installPackageEntryCount": 8,
        "installPackageBinaryCount": 1,
        "sprint22RuntimeAcceptancePassed": True,
        "sprint23EntryApproved": True,
        "sprint24EntryApproved": False,
        "runtimeAcceptancePassed": False,
        "misfireConditionTransitionsImplemented": False,
        "privateReferencesRedistributed": False,
    }
    for key, expected in expected_qualification.items():
        if qualification.get(key) != expected:
            fail(
                f"BUILD-QUALIFICATION.json has unexpected {key}: "
                f"expected {expected!r}, observed {qualification.get(key)!r}"
            )
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

    report = read("SPRINT-23-REPORT.md")
    if "PENDING_FINAL_QUALIFICATION" in report:
        fail("SPRINT-23-REPORT.md still contains the qualification placeholder.")
    require(
        report,
        (
            "489 tests × 3 runs, 0 failures",
            EXPECTED_MOD_DLL,
            EXPECTED_MOD_PDB,
            EXPECTED_TEST_OUTPUT,
            EXPECTED_STANDALONE_PACKAGE,
            "READY FOR KINGMAKER — Sprint 23 natural-roll misfire smoke test",
        ),
        "SPRINT-23-REPORT.md",
    )
    require(
        read("BUILD-INFO.txt"),
        (
            "Version: 0.0.23-s23-natural-roll-misfire",
            "Exact dependency-free tests: 489 x 3 runs; 0 failures",
            EXPECTED_MOD_DLL,
            EXPECTED_STANDALONE_PACKAGE,
            "Sprint 24 status: blocked",
        ),
        "BUILD-INFO.txt",
    )
    read("VALIDATION-RESULTS-S23.txt")

    compile_evidence = json.loads(
        read("evidence/sprint23/compile/deterministic-compile-evidence.json")
    )
    if compile_evidence.get("dll", {}).get("secondSha256") != EXPECTED_MOD_DLL:
        fail("Deterministic compile evidence has the wrong DLL hash.")
    if compile_evidence.get("pdb", {}).get("secondSha256") != EXPECTED_MOD_PDB:
        fail("Deterministic compile evidence has the wrong PDB hash.")
    if compile_evidence.get("dll", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not affirm DLL identity.")
    if compile_evidence.get("pdb", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not affirm PDB identity.")

    test_evidence = json.loads(read("evidence/sprint23/tests/executed-test-evidence.json"))
    if test_evidence.get("declaredTests") != 489 or test_evidence.get("runs") != 3:
        fail("Executed test evidence has the wrong test count or run count.")
    if test_evidence.get("testOutputSha256") != EXPECTED_TEST_OUTPUT:
        fail("Executed test evidence has the wrong output hash.")
    if test_evidence.get("failures") != 0 or test_evidence.get("repeatedOutputIdentical") is not True:
        fail("Executed test evidence does not prove three clean identical runs.")

    package_evidence = json.loads(
        read("evidence/sprint23/package/standalone-package-evidence.json")
    )
    if package_evidence.get("sha256") != EXPECTED_STANDALONE_PACKAGE:
        fail("Standalone package evidence has the wrong archive hash.")
    if package_evidence.get("modDllSha256") != EXPECTED_MOD_DLL:
        fail("Standalone package evidence has the wrong DLL hash.")
    if package_evidence.get("entryCount") != 8 or package_evidence.get("binaryCount") != 1:
        fail("Standalone package evidence has the wrong entry or binary count.")
    if package_evidence.get("privateReferencesRedistributed") is not False:
        fail("Standalone package evidence does not preserve the private-reference boundary.")

    forbidden_binary_suffixes = {".dll", ".exe", ".pdb", ".mdb"}
    loose_binaries = [
        path.relative_to(ROOT).as_posix()
        for path in ROOT.rglob("*")
        if path.is_file() and path.suffix.lower() in forbidden_binary_suffixes
    ]
    if loose_binaries:
        fail(f"Source tree contains loose binaries/private-reference risk: {loose_binaries[:5]}")

    print("Sprint 23 source invariant validation passed.")
    print(f"Blueprints: 12 stable / 11 active / 1 reserved.")
    print(f"Compile items: main={len(main_items)}; tests={len(test_items)}.")
    print("Declared dependency-free tests: 489 unique.")
    print("Condition transitions: absent from Sprint 23 misfire runtime.")
    print("Private runtime/compiler binaries in source tree: none.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print(f"Sprint 23 validation failed: {exception}", file=sys.stderr)
        raise SystemExit(1)
