#!/usr/bin/env python3
"""Portable source and optional sealed-evidence validator for Sprint 26."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
VERSION = "0.0.26"
INFORMATIONAL_VERSION = "0.0.26-s26-misfire-burst"
DECLARED_TESTS = 540


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


def require(text: str, values: tuple[str, ...], label: str) -> None:
    for value in values:
        if value not in text:
            fail(f"{label} is missing {value!r}.")


def compile_items(relative: str) -> list[Path]:
    project = ROOT / relative
    tree = ET.parse(project)
    result: list[Path] = []
    for node in tree.findall(".//m:Compile", MSBUILD_NS):
        include = node.attrib.get("Include")
        if not include:
            continue
        path = (project.parent / include.replace("\\", "/")).resolve()
        if not path.is_file():
            fail(f"Compile item does not exist: {include} -> {path}")
        result.append(path)
    if not result or len(result) != len(set(result)):
        fail(f"Compile items are empty or duplicated in {relative}.")
    return result


def validate_checksum_file(relative: str) -> None:
    checksum_path = ROOT / relative
    if not checksum_path.is_file():
        fail(f"Checksum file is missing: {relative}")
    for line in checksum_path.read_text(encoding="ascii").splitlines():
        if not line.strip():
            continue
        match = re.fullmatch(r"([0-9a-f]{64})  (.+)", line)
        if match is None:
            fail(f"Malformed checksum line in {relative}: {line!r}")
        expected, name = match.groups()
        target = checksum_path.parent / name
        if not target.is_file() or sha256(target) != expected:
            fail(f"Checksum mismatch in {relative}: {name}")


def validate_structure() -> tuple[list[Path], list[Path]]:
    info = json.loads(read("Info.json"))
    if info.get("Version") != VERSION:
        fail(f"Info.json must declare version {VERSION}.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json must retain Unity Mod Manager 0.32.4.")
    if info.get("AssemblyName") != "KingmakerGunslinger.dll":
        fail("Info.json names the wrong assembly.")

    require(
        read("Directory.Build.props"),
        (
            f"<KmgVersion>{VERSION}</KmgVersion>",
            f"<KmgInformationalVersion>{INFORMATIONAL_VERSION}</KmgInformationalVersion>",
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
            f'AssemblyVersion("{VERSION}")',
            f'AssemblyFileVersion("{VERSION}")',
            f'AssemblyInformationalVersion("{INFORMATIONAL_VERSION}")',
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
        (ROOT / "src/KingmakerGunslinger/Explosions" / name).resolve()
        for name in (
            "FirearmExplosionDecision.cs",
            "FirearmExplosionDisposition.cs",
            "FirearmExplosionRuntime.cs",
            "FirearmExplosionRuntimeDiagnostics.cs",
            "FirearmExplosionService.cs",
            "FirearmExplosionTargetCandidate.cs",
            "FirearmExplosionTargetPlan.cs",
            "FirearmExplosionTargetPlanService.cs",
            "FirearmExplosionTargetResult.cs",
        )
    }
    if not required_main.issubset(set(main_items)):
        fail("The main project does not compile every Sprint 26 explosion source.")
    required_tests = {
        (ROOT / "src/KingmakerGunslinger/Explosions" / name).resolve()
        for name in (
            "FirearmExplosionDecision.cs",
            "FirearmExplosionDisposition.cs",
            "FirearmExplosionService.cs",
            "FirearmExplosionTargetCandidate.cs",
            "FirearmExplosionTargetPlan.cs",
            "FirearmExplosionTargetPlanService.cs",
            "FirearmExplosionTargetResult.cs",
        )
    }
    if not required_tests.issubset(set(test_items)):
        fail("The dependency-free test project does not compile every pure Sprint 26 explosion source.")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    cases = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', program)
    if len(cases) != DECLARED_TESTS or len(cases) != len(set(cases)):
        fail(f"Expected {DECLARED_TESTS} unique declared tests, observed {len(cases)}.")
    required_cases = {
        "equality.different-misfire-burst",
        "invalid.misfire-burst-too-small",
        "invalid.misfire-burst-not-five-foot-step",
        "invalid.misfire-burst-too-large",
        "explosion.broken-to-wrecked-damages-burst",
        "explosion-target.plan-dedupes-exact-wielder",
        "explosion-target.plan-dedupes-nearby-reference",
        "explosion-target.plan-orders-and-wielder-last",
        "explosion-target.result-format",
        "explosion-target.result-negative-hp",
    }
    if not required_cases.issubset({name for name, _ in cases}):
        fail("Required Sprint 26 test declarations are missing.")

    definition = read("src/KingmakerGunslinger/Firearms/FirearmDefinition.cs")
    require(
        definition,
        (
            "MinimumMisfireBurstRadiusFeet = 5",
            "MaximumMisfireBurstRadiusFeet = 100",
            "MisfireBurstRadiusFeet",
            "misfireBurstRadiusFeet % 5 != 0",
            "misfireBurst={5}ft",
        ),
        "FirearmDefinition",
    )
    component = read("src/KingmakerGunslinger/Firearms/FirearmDefinitionComponent.cs")
    require(component, ("m_MisfireBurstRadiusFeet", "definition.MisfireBurstRadiusFeet"), "FirearmDefinitionComponent")
    require(read("src/KingmakerGunslinger/Firearms/FirearmDefinitions.cs"), ("40,", "2,", "5,"), "Early Musket definition")

    service = read("src/KingmakerGunslinger/Explosions/FirearmExplosionService.cs")
    require(
        service,
        (
            "internal const int ReflexSaveDifficultyClass = 12;",
            "FirearmMisfireConditionTransition.BrokenToWrecked",
            "FirearmExplosionDisposition.DamageBurst",
            "FirearmExplosionDisposition.None",
        ),
        "FirearmExplosionService",
    )
    if "DamageWielder" in service or "RequiresWielderDamage" in read("src/KingmakerGunslinger/Explosions/FirearmExplosionDecision.cs"):
        fail("The Sprint 25 exact-wielder-only disposition remains active.")

    planner = read("src/KingmakerGunslinger/Explosions/FirearmExplosionTargetPlanService.cs")
    require(
        planner,
        (
            "ReferenceIdentityComparer.Instance",
            "nearby.Sort(CompareNearby)",
            "left.DistanceMeters.CompareTo",
            "StringComparison.Ordinal",
            "nearby.Add(exactWielder)",
        ),
        "FirearmExplosionTargetPlanService",
    )
    plan = read("src/KingmakerGunslinger/Explosions/FirearmExplosionTargetPlan.cs")
    require(plan, ("exact wielder exactly once and last", "ObservedCandidates", "DuplicateCandidates"), "FirearmExplosionTargetPlan")

    runtime = read("src/KingmakerGunslinger/Explosions/FirearmExplosionRuntime.cs")
    require(
        runtime,
        (
            "GameHelper.GetTargetsAround(",
            "new Feet((float)burstRadiusFeet)",
            "true,",
            "false);",
            "wielder.Position",
            "unit.DistanceTo(wielder.Position)",
            "TargetPlanService.Build(",
            "ReferenceEventGate",
            "HashSet<object>",
            "ReferenceIdentityComparer.Instance",
            "new RuleSavingThrow(",
            "SavingThrowType.Reflex",
            "new RuleDealDamage(",
            "HalfBecauseSavingThrow = halfBecauseSavingThrow",
            "DisablePrecisionDamage = true",
            "AttackRoll = attackRoll",
            "CreateBaseWeaponDamageBundle(",
            "weapon.Blueprint.DamageType.CreateDamage",
            "VerifyCommittedState(",
            "ResolveStableIdentity",
            "exactWielderLast=True",
            "No broad fallback or retry",
        ),
        "FirearmExplosionRuntime",
    )
    if "RuleAttackWithWeapon.CreateDamage(false)" in runtime:
        fail("Sprint 26 runtime must not reuse the original target's calculated damage-description bundle.")
    if re.search(r"HPLeft\s*[+\-]?=", runtime):
        fail("Sprint 26 runtime must not mutate hit points directly.")

    diagnostics = read("src/KingmakerGunslinger/Explosions/FirearmExplosionRuntimeDiagnostics.cs")
    require(
        diagnostics,
        (
            "queries={7}; queryCandidates={8}; plannedTargets={9}",
            "targetAttempts={10}; targetApplied={11}; targetRejected={12}; targetDuplicates={13}; targetFaults={14}",
            "QUERY:",
            "TARGET ATTEMPT:",
            "TARGET APPLIED:",
            "TARGET DUPLICATE:",
            "TARGET REJECTED:",
            "TARGET FAULT:",
            "targets={11}; finalState=empty/Wrecked",
        ),
        "FirearmExplosionRuntimeDiagnostics",
    )

    misfire_runtime = read("src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs")
    require(
        misfire_runtime,
        (
            "postDischarge.Definition.MisfireBurstRadiusFeet",
            "context.MisfireBurstRadiusFeet",
            "explosion.RequiresBurstDamage",
            "FirearmExplosionRuntime.Apply(",
        ),
        "FirearmMisfireRuntime integration",
    )

    ui = read("src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    require(
        ui,
        (
            "0.0.26 Sprint 26 native misfire-burst smoke test",
            "Second-misfire explosion:",
            "5-foot burst",
            "Every unique qualified unit",
            "exact wielder resolves last",
        ),
        "DevelopmentUi",
    )

    if read("SMOKE-TEST-GUIDE.md") != read("SMOKE-TEST-GUIDE-0.0.26.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.26 guide.")
    guide = read("SMOKE-TEST-GUIDE-0.0.26.md")
    require(
        guide,
        (
            INFORMATIONAL_VERSION,
            "Arrange a controlled spatial fixture",
            "First misfire: no query and no burst",
            "Second misfire: native multi-target burst",
            "plannedTargets +N",
            "targetApplied +N",
            "exact wielder result must appear once and last",
            "Sprint 27 remains blocked",
        ),
        "0.0.26 smoke-test guide",
    )

    require(
        read("evidence/sprint26-contracts/exact-spatial-contracts.json"),
        (
            "GameHelper",
            "GetTargetsAround",
            "DistanceTo",
            "0.3048",
            '"checkLOS": true',
            '"includeDead": false',
        ),
        "exact spatial contract evidence",
    )
    validate_checksum_file("evidence/sprint26-contracts/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint25-runtime-acceptance/2026-07-16/SHA256SUMS.txt")

    required_docs = (
        "SPRINT-26-REPORT.md",
        "docs/FIREARM-EXPLOSION-DAMAGE.md",
        "docs/FIREARM-DEFINITION.md",
        "docs/decisions/ADR-0033-native-spatial-second-misfire-burst.md",
        "planning/SPRINT-27-ENTRY-CRITERIA.md",
    )
    for relative in required_docs:
        read(relative)

    forbidden_names = {
        "Assembly-CSharp.dll",
        "Assembly-CSharp-firstpass.dll",
        "UnityEngine.dll",
        "UnityModManager.dll",
        "0Harmony12.dll",
        "Newtonsoft.Json.dll",
        "csc.dll",
        "csc.exe",
        "mscorlib.dll",
    }
    for path in ROOT.rglob("*"):
        if path.is_file() and path.name in forbidden_names:
            fail(f"Private/compiler/framework binary is present in the source tree: {path.relative_to(ROOT)}")
        if path.is_file() and path.suffix.lower() in {".pdb", ".mdb"}:
            fail(f"Loose build symbol is present in the source tree: {path.relative_to(ROOT)}")

    return main_items, test_items


def validate_final() -> None:
    qualification = json.loads(read("BUILD-QUALIFICATION.json"))
    expected = {
        "version": VERSION,
        "informationalVersion": INFORMATIONAL_VERSION,
        "configuration": "Release",
        "targetFramework": ".NET Framework 4.7",
        "languageVersion": "7.3",
        "platformTarget": "AnyCPU",
        "declaredTests": DECLARED_TESTS,
        "testRuns": 3,
        "testFailures": 0,
        "installPackageEntryCount": 8,
        "installPackageBinaryCount": 1,
    }
    for key, value in expected.items():
        if qualification.get(key) != value:
            fail(f"BUILD-QUALIFICATION.json: expected {key}={value!r}, observed {qualification.get(key)!r}")
    for key in (
        "warningsAsErrors",
        "sourceInvariantValidationPassed",
        "sameOutputPathDllByteIdentical",
        "sameOutputPathPdbByteIdentical",
        "testOutputRepeatedByteIdentical",
        "installPackageValidationPassed",
        "privateReferencesRedistributed",
        "readyForKingmakerSmokeTest",
        "exactRuntimeContractEvidencePassed",
        "sprint25RuntimeAcceptancePassed",
    ):
        expected_value = False if key == "privateReferencesRedistributed" else True
        if qualification.get(key) is not expected_value:
            fail(f"BUILD-QUALIFICATION.json does not affirm {key}={expected_value}.")

    hashes = {
        "DLL": qualification.get("modDllSha256"),
        "PDB": qualification.get("modPdbSha256"),
        "test output": qualification.get("testOutputSha256"),
        "package": qualification.get("installPackageSha256"),
    }
    for label, value in hashes.items():
        if re.fullmatch(r"[0-9a-f]{64}", value or "") is None:
            fail(f"BUILD-QUALIFICATION.json has an invalid {label} hash.")

    compile_evidence = json.loads(read("evidence/sprint26/compile/deterministic-compile-evidence.json"))
    if compile_evidence.get("dll", {}).get("secondSha256") != hashes["DLL"] or compile_evidence.get("dll", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative DLL.")
    if compile_evidence.get("pdb", {}).get("secondSha256") != hashes["PDB"] or compile_evidence.get("pdb", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative PDB.")

    test_evidence = json.loads(read("evidence/sprint26/tests/executed-test-evidence.json"))
    if test_evidence.get("declaredTests") != DECLARED_TESTS or test_evidence.get("runs") != 3:
        fail("Executed test evidence has the wrong count or run count.")
    if test_evidence.get("failures") != 0 or test_evidence.get("repeatedOutputIdentical") is not True:
        fail("Executed test evidence does not prove three clean identical runs.")
    if test_evidence.get("testOutputSha256") != hashes["test output"]:
        fail("Executed test evidence has the wrong output hash.")

    package_evidence = json.loads(read("evidence/sprint26/package/standalone-package-evidence.json"))
    if package_evidence.get("sha256") != hashes["package"] or package_evidence.get("modDllSha256") != hashes["DLL"]:
        fail("Standalone package evidence has the wrong package or DLL hash.")
    if package_evidence.get("entryCount") != 8 or package_evidence.get("binaryCount") != 1:
        fail("Standalone package evidence has the wrong entry or binary count.")
    if package_evidence.get("privateReferencesRedistributed") is not False:
        fail("Standalone package evidence violates the private-reference boundary.")

    report = read("SPRINT-26-REPORT.md")
    if "PENDING_FINAL_QUALIFICATION" in report:
        fail("Sprint 26 report still contains a qualification placeholder.")
    require(
        report,
        (
            f"{DECLARED_TESTS} tests × 3 runs, 0 failures",
            hashes["DLL"],
            hashes["PDB"],
            hashes["test output"],
            hashes["package"],
            "READY FOR KINGMAKER — Sprint 26 native misfire-burst smoke test",
        ),
        "Sprint 26 report",
    )

    validate_checksum_file("evidence/sprint26/compile/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint26/tests/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint26/package/SHA256SUMS.txt")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--final", action="store_true", help="also validate sealed qualification evidence")
    args = parser.parse_args()
    main_items, test_items = validate_structure()
    if args.final:
        validate_final()
    print("Sprint 26 source invariant validation passed" + (" with final evidence." if args.final else "."))
    print("Blueprints: 12 stable / 11 active / 1 reserved.")
    print(f"Compile items: main={len(main_items)}; tests={len(test_items)}.")
    print(f"Declared dependency-free tests: {DECLARED_TESTS} unique.")
    print("Second misfire: native 5-foot LOS burst, one fresh Reflex/damage pair per unique qualified unit, exact wielder once and last.")
    print("Private runtime/compiler/framework binaries in source tree: none.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print(f"Sprint 26 validation failed: {exception}", file=sys.stderr)
        raise SystemExit(1)
