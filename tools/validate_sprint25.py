#!/usr/bin/env python3
"""Portable source and optional sealed-evidence validator for Sprint 25."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
VERSION = "0.0.25"
INFORMATIONAL_VERSION = "0.0.25-s25-second-misfire-explosion"
DECLARED_TESTS = 513


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
        )
    }
    if not required_main.issubset(set(main_items)):
        fail("The main project does not compile every Sprint 25 explosion source.")
    stray = list((ROOT / "src/KingmakerGunslinger/Misfires").glob("FirearmExplosion*.cs"))
    if stray:
        fail(f"Uncompiled competing explosion implementation remains: {[p.name for p in stray]}")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    cases = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', program)
    if len(cases) != DECLARED_TESTS or len(cases) != len(set(cases)):
        fail(f"Expected {DECLARED_TESTS} unique declared tests, observed {len(cases)}.")
    required_cases = {
        "explosion.ordinary-normal-none",
        "explosion.ordinary-broken-none",
        "explosion.normal-to-broken-none",
        "explosion.broken-to-wrecked-damages-wielder",
        "explosion.reflex-dc-twelve",
        "explosion.null-condition",
        "explosion.format",
        "explosion.decision.broken-to-wrecked-none-rejected",
        "explosion.decision.normal-to-broken-damage-rejected",
        "explosion.decision.unknown-disposition",
    }
    if not required_cases.issubset({name for name, _ in cases}):
        fail("Required Sprint 25 explosion test declarations are missing.")

    service = read("src/KingmakerGunslinger/Explosions/FirearmExplosionService.cs")
    require(
        service,
        (
            "internal const int ReflexSaveDifficultyClass = 12;",
            "FirearmMisfireConditionTransition.BrokenToWrecked",
            "FirearmExplosionDisposition.DamageWielder",
            "FirearmExplosionDisposition.None",
        ),
        "FirearmExplosionService",
    )
    decision = read("src/KingmakerGunslinger/Explosions/FirearmExplosionDecision.cs")
    require(
        decision,
        (
            "Only a proven BrokenToWrecked misfire may damage the wielder",
            "requiresWielderDamage={1}",
            "reflexDC={2}",
        ),
        "FirearmExplosionDecision",
    )

    runtime = read("src/KingmakerGunslinger/Explosions/FirearmExplosionRuntime.cs")
    require(
        runtime,
        (
            "ReferenceEventGate",
            "ReferenceEquals(attackRoll.Initiator, wielder)",
            "ReferenceEquals(attackRoll.Weapon, weapon)",
            "ReferenceEquals(weaponAttack.Initiator, wielder)",
            "ReferenceEquals(weaponAttack.Weapon, weapon)",
            "ReferenceEquals(weapon.Wielder.Unit, wielder)",
            "current.Repository.RepositoryIdentity",
            "FirearmCondition.Wrecked",
            "weapon.Blueprint.DamageType.CreateDamage",
            "weapon.Damage",
            "new DamageBundle(",
            "weapon.Size",
            "new RuleSavingThrow(",
            "SavingThrowType.Reflex",
            "new RuleDealDamage(",
            "HalfBecauseSavingThrow = halfBecauseSavingThrow",
            "DisablePrecisionDamage = true",
            "AttackRoll = attackRoll",
            "Rulebook.Trigger(savingThrow)",
            "Rulebook.Trigger(dealDamage)",
            "VerifyCommittedState(",
            "no broad fallback",
        ),
        "FirearmExplosionRuntime",
    )
    if "RuleAttackWithWeapon.CreateDamage(false)" in runtime:
        fail("Sprint 25 runtime must not reuse the original target's calculated damage-description bundle.")
    if re.search(r"HPLeft\s*[+\-]?=", runtime):
        fail("Sprint 25 runtime must not mutate hit points directly.")

    diagnostics = read("src/KingmakerGunslinger/Explosions/FirearmExplosionRuntimeDiagnostics.cs")
    require(
        diagnostics,
        (
            "scheduled={0}; attempts={1}; applied={2}; notRequired={3}; rejected={4}; duplicates={5}; faults={6}",
            "SCHEDULED:",
            "NOT REQUIRED:",
            "ATTEMPT:",
            "APPLIED:",
            "attackRoll={2}",
            "repositoryIdentity={3}",
            "halfBecauseSavingThrow={9}",
            "finalState=empty/Wrecked",
        ),
        "FirearmExplosionRuntimeDiagnostics",
    )

    misfire_runtime = read("src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs")
    require(
        misfire_runtime,
        (
            "ExplosionService.Evaluate(condition)",
            "FirearmExplosionRuntimeDiagnostics.RecordDecision(",
            "context.TryScheduleExplosion()",
            "context.ExplosionRequired",
            "context.TryBeginExplosion()",
            "FirearmExplosionRuntime.Apply(",
        ),
        "FirearmMisfireRuntime integration",
    )

    ui = read("src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    require(
        ui,
        (
            "0.0.25 Sprint 25 second-misfire explosion smoke test",
            "Second-misfire explosion:",
            "Reflex DC 12",
            "base weapon-damage event",
            "Nearby-creature burst targeting remains deferred",
        ),
        "DevelopmentUi",
    )

    if read("SMOKE-TEST-GUIDE.md") != read("SMOKE-TEST-GUIDE-0.0.25.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.25 guide.")
    guide = read("SMOKE-TEST-GUIDE-0.0.25.md")
    require(
        guide,
        (
            "## 3. First misfire: Normal becomes Broken and does not explode",
            "notRequired=1",
            "## 4. Reload the exact Broken firearm without repairing it",
            "## 5. Second misfire: Broken becomes Wrecked and damages the exact wielder once",
            "scheduled=1",
            "attempts=1",
            "applied=1",
            "reflexDC=12",
            "halfBecauseSavingThrow=True or False",
            "## 10. Verify native Heavy Crossbow isolation",
            "## 11. Persistence regression",
            "## 12. Final evidence to capture",
        ),
        "0.0.25 smoke-test guide",
    )

    for required in (
        "SPRINT-25-REPORT.md",
        "docs/FIREARM-EXPLOSION-DAMAGE.md",
        "docs/decisions/ADR-0032-exact-wielder-second-misfire-damage.md",
        "planning/SPRINT-26-ENTRY-CRITERIA.md",
        "evidence/sprint24-repair/runtime-acceptance-2026-07-16/ASSESSMENT.md",
        "evidence/sprint25-contracts/exact-damage-contracts.json",
        "evidence/sprint25-contracts/PATHFINDER-RULE-SOURCE.md",
    ):
        read(required)
    validate_checksum_file("evidence/sprint24-repair/runtime-acceptance-2026-07-16/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint25-contracts/SHA256SUMS.txt")

    contract = json.loads(read("evidence/sprint25-contracts/exact-damage-contracts.json"))
    if contract.get("assembly", {}).get("sha256") != "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb":
        fail("Exact damage-contract evidence references the wrong Assembly-CSharp.dll hash.")
    boundary = contract.get("implementationBoundary", {})
    if boundary.get("globalPatchesAdded") is not False or boundary.get("nearbyUnitEnumerationAdded") is not False:
        fail("Sprint 25 contract evidence violates the bounded implementation boundary.")
    if "DamageType.CreateDamage" not in boundary.get("damage", "") or "new DamageBundle" not in boundary.get("damage", ""):
        fail("Exact damage-contract evidence does not describe the base weapon-damage path.")

    forbidden_names = {
        "Assembly-CSharp.dll",
        "Assembly-CSharp-firstpass.dll",
        "UnityModManager.dll",
        "0Harmony12.dll",
        "Newtonsoft.Json.dll",
    }
    loose_binaries: list[str] = []
    for directory, directory_names, file_names in os.walk(ROOT, followlinks=False):
        directory_names.sort()
        file_names.sort()
        base = Path(directory)
        for file_name in file_names:
            path = base / file_name
            if path.suffix.lower() in {".dll", ".exe", ".pdb", ".mdb"} or path.name in forbidden_names:
                loose_binaries.append(path.relative_to(ROOT).as_posix())
    if loose_binaries:
        fail(f"Source tree contains loose binaries/private-reference risk: {loose_binaries[:5]}")

    return main_items, test_items


def validate_final() -> None:
    qualification = json.loads(read("BUILD-QUALIFICATION.json"))
    expected = {
        "modVersion": VERSION,
        "informationalVersion": INFORMATIONAL_VERSION,
        "declaredTests": DECLARED_TESTS,
        "testRuns": 3,
        "testFailures": 0,
        "installPackageEntryCount": 8,
        "installPackageBinaryCount": 1,
        "secondMisfireExplosionImplemented": True,
        "exactWielderOnly": True,
        "reflexSaveDifficultyClass": 12,
        "nearbyUnitEnumerationImplemented": False,
        "privateReferencesRedistributed": False,
        "runtimeAcceptancePassed": False,
        "sprint26EntryApproved": False,
    }
    for key, value in expected.items():
        if qualification.get(key) != value:
            fail(f"BUILD-QUALIFICATION.json has unexpected {key}: expected {value!r}, observed {qualification.get(key)!r}")
    for key in (
        "sourceInvariantValidationPassed",
        "sameOutputPathDllByteIdentical",
        "sameOutputPathPdbByteIdentical",
        "testOutputRepeatedByteIdentical",
        "installPackageValidationPassed",
        "readyForKingmakerSmokeTest",
        "exactRuntimeContractEvidencePassed",
        "sprint24RuntimeAcceptancePassed",
    ):
        if qualification.get(key) is not True:
            fail(f"BUILD-QUALIFICATION.json does not affirm {key}.")

    hashes = {
        "DLL": qualification.get("modDllSha256"),
        "PDB": qualification.get("modPdbSha256"),
        "test output": qualification.get("testOutputSha256"),
        "package": qualification.get("installPackageSha256"),
    }
    for label, value in hashes.items():
        if re.fullmatch(r"[0-9a-f]{64}", value or "") is None:
            fail(f"BUILD-QUALIFICATION.json has an invalid {label} hash.")

    compile_evidence = json.loads(read("evidence/sprint25/compile/deterministic-compile-evidence.json"))
    if compile_evidence.get("dll", {}).get("secondSha256") != hashes["DLL"] or compile_evidence.get("dll", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative DLL.")
    if compile_evidence.get("pdb", {}).get("secondSha256") != hashes["PDB"] or compile_evidence.get("pdb", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative PDB.")

    test_evidence = json.loads(read("evidence/sprint25/tests/executed-test-evidence.json"))
    if test_evidence.get("declaredTests") != DECLARED_TESTS or test_evidence.get("runs") != 3:
        fail("Executed test evidence has the wrong count or run count.")
    if test_evidence.get("failures") != 0 or test_evidence.get("repeatedOutputIdentical") is not True:
        fail("Executed test evidence does not prove three clean identical runs.")
    if test_evidence.get("testOutputSha256") != hashes["test output"]:
        fail("Executed test evidence has the wrong output hash.")

    package_evidence = json.loads(read("evidence/sprint25/package/standalone-package-evidence.json"))
    if package_evidence.get("sha256") != hashes["package"] or package_evidence.get("modDllSha256") != hashes["DLL"]:
        fail("Standalone package evidence has the wrong package or DLL hash.")
    if package_evidence.get("entryCount") != 8 or package_evidence.get("binaryCount") != 1:
        fail("Standalone package evidence has the wrong entry or binary count.")
    if package_evidence.get("privateReferencesRedistributed") is not False:
        fail("Standalone package evidence violates the private-reference boundary.")

    report = read("SPRINT-25-REPORT.md")
    if "PENDING_FINAL_QUALIFICATION" in report:
        fail("Sprint 25 report still contains a qualification placeholder.")
    require(
        report,
        (
            f"{DECLARED_TESTS} tests × 3 runs, 0 failures",
            hashes["DLL"],
            hashes["PDB"],
            hashes["test output"],
            hashes["package"],
            "READY FOR KINGMAKER — Sprint 25 second-misfire explosion smoke test",
        ),
        "Sprint 25 report",
    )

    validate_checksum_file("evidence/sprint25/compile/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint25/tests/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint25/package/SHA256SUMS.txt")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--final", action="store_true", help="also validate sealed qualification evidence")
    args = parser.parse_args()
    main_items, test_items = validate_structure()
    if args.final:
        validate_final()
    print("Sprint 25 source invariant validation passed" + (" with final evidence." if args.final else "."))
    print("Blueprints: 12 stable / 11 active / 1 reserved.")
    print(f"Compile items: main={len(main_items)}; tests={len(test_items)}.")
    print(f"Declared dependency-free tests: {DECLARED_TESTS} unique.")
    print("Second misfire: exact item empty/Wrecked, Reflex DC 12, one native exact-wielder base weapon-damage event.")
    print("Private runtime/compiler binaries in source tree: none.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print(f"Sprint 25 validation failed: {exception}", file=sys.stderr)
        raise SystemExit(1)
