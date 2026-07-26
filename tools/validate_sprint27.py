#!/usr/bin/env python3
"""Portable source and optional sealed-evidence validator for Sprint 27."""
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
VERSION = "0.0.27"
INFORMATIONAL_VERSION = "0.0.27-s27-item-lifecycle-recovery-contract"
DECLARED_TESTS = 543


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

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    cases = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', program)
    if len(cases) != DECLARED_TESTS or len(cases) != len(set(cases)):
        fail(f"Expected {DECLARED_TESTS} unique declared tests, observed {len(cases)}.")
    required_cases = {
        "state.overhaul.wrecked-to-broken",
        "state.overhaul.normal-rejected",
        "state.overhaul.broken-rejected",
    }
    if not required_cases.issubset({name for name, _ in cases}):
        fail("Sprint 27 overhaul tests are missing.")

    state_machine = read("src/KingmakerGunslinger/Firearms/FirearmStateMachine.cs")
    require(
        state_machine,
        (
            "internal static FirearmState OverhaulWrecked(FirearmState state)",
            "FirearmStateTransitionError.NotWrecked",
            "Only a wrecked firearm can use the same-item overhaul transition.",
            "FirearmCondition.Broken",
        ),
        "FirearmStateMachine",
    )
    transition_error = read("src/KingmakerGunslinger/Firearms/FirearmStateTransitionException.cs")
    require(transition_error, ("NotWrecked = 7",), "FirearmStateTransitionError")

    bridge = read("src/KingmakerGunslinger/Development/KingmakerDevelopmentBridge.cs")
    require(
        bridge,
        (
            "OverhaulFirstEquippedWreckedFirearmForDebug",
            "FirearmStateMachine.OverhaulWrecked",
            "before.Repository.RepositoryIdentity",
            "after.Repository.RepositoryIdentity",
            "before.Repository.RuntimeReferenceHash",
            "after.Repository.RuntimeReferenceHash",
            "after.Repository.Revision != before.Repository.Revision + 1",
            "The item was not removed, replaced, or silently repaired to Normal.",
        ),
        "KingmakerDevelopmentBridge",
    )

    ui = read("src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    require(
        ui,
        (
            "0.0.27 Sprint 27 item-lifecycle recovery-contract smoke test",
            "Arm removal of ALL unequipped Test Muskets (destructive)",
            "CONFIRM remove ALL unequipped Test Muskets",
            "Cancel Test Musket removal",
            "Overhaul first equipped Wrecked firearm to Broken (contract test)",
            "same-item lifecycle contract probe=ACTIVE",
        ),
        "DevelopmentUi",
    )
    if 'Button("Remove unequipped Test Muskets from shared inventory")' in ui:
        fail("Sprint 27 must not retain the old one-click destructive Test Musket cleanup button.")

    if read("SMOKE-TEST-GUIDE.md") != read("SMOKE-TEST-GUIDE-0.0.27.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.27 guide.")
    guide = read("SMOKE-TEST-GUIDE-0.0.27.md")
    require(
        guide,
        (
            INFORMATIONAL_VERSION,
            "Run the same-item overhaul probe",
            "repositoryIdentity=<same value as before>",
            "referenceHash=<same value as before>",
            "revision=<before>-><before+1>",
            "Verify cleanup confirmation safety",
            "Cancel Test Musket removal",
            "Sprint 28 remains blocked",
        ),
        "0.0.27 smoke-test guide",
    )

    contracts = read("evidence/sprint27-contracts/exact-item-lifecycle-contracts.json")
    require(
        contracts,
        (
            '"sha256": "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb"',
            '"ItemsCollection.Remove(ItemEntity)"',
            '"ItemsCollection.Extract(ItemEntity)"',
            '"ItemSlot.RemoveItem(bool,bool)"',
            '"ItemEntity.Dispose()"',
            '"ItemSwitch.RunAction()"',
            '"ItemRestoreValue.RunAction()"',
            '"sprint27Decision"',
        ),
        "exact item-lifecycle contract evidence",
    )
    validate_checksum_file("evidence/sprint27-contracts/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint26-runtime-acceptance/2026-07-16/SHA256SUMS.txt")

    required_docs = (
        "SPRINT-27-REPORT.md",
        "docs/FIREARM-ITEM-LIFECYCLE-AND-RECOVERY.md",
        "docs/decisions/ADR-0034-retain-wrecked-and-qualify-same-item-overhaul.md",
        "planning/SPRINT-28-ENTRY-CRITERIA.md",
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
        "readyForKingmakerSmokeTest",
        "exactRuntimeContractEvidencePassed",
        "sprint26RuntimeAcceptancePassed",
    ):
        if qualification.get(key) is not True:
            fail(f"BUILD-QUALIFICATION.json does not affirm {key}=true.")
    if qualification.get("privateReferencesRedistributed") is not False:
        fail("BUILD-QUALIFICATION.json violates the private-reference boundary.")

    hashes = {
        "DLL": qualification.get("modDllSha256"),
        "PDB": qualification.get("modPdbSha256"),
        "test output": qualification.get("testOutputSha256"),
        "package": qualification.get("installPackageSha256"),
    }
    for label, value in hashes.items():
        if re.fullmatch(r"[0-9a-f]{64}", value or "") is None:
            fail(f"BUILD-QUALIFICATION.json has an invalid {label} hash.")

    compile_evidence = json.loads(read("evidence/sprint27/compile/deterministic-compile-evidence.json"))
    if compile_evidence.get("dll", {}).get("secondSha256") != hashes["DLL"] or compile_evidence.get("dll", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative DLL.")
    if compile_evidence.get("pdb", {}).get("secondSha256") != hashes["PDB"] or compile_evidence.get("pdb", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative PDB.")

    test_evidence = json.loads(read("evidence/sprint27/tests/executed-test-evidence.json"))
    if test_evidence.get("declaredTests") != DECLARED_TESTS or test_evidence.get("runs") != 3:
        fail("Executed test evidence has the wrong count or run count.")
    if test_evidence.get("failures") != 0 or test_evidence.get("repeatedOutputIdentical") is not True:
        fail("Executed test evidence does not prove three clean identical runs.")
    if test_evidence.get("testOutputSha256") != hashes["test output"]:
        fail("Executed test evidence has the wrong output hash.")

    package_evidence = json.loads(read("evidence/sprint27/package/standalone-package-evidence.json"))
    if package_evidence.get("sha256") != hashes["package"] or package_evidence.get("modDllSha256") != hashes["DLL"]:
        fail("Standalone package evidence has the wrong package or DLL hash.")
    if package_evidence.get("entryCount") != 8 or package_evidence.get("binaryCount") != 1:
        fail("Standalone package evidence has the wrong entry or binary count.")
    if package_evidence.get("privateReferencesRedistributed") is not False:
        fail("Standalone package evidence violates the private-reference boundary.")

    report = read("SPRINT-27-REPORT.md")
    if "PENDING_FINAL_QUALIFICATION" in report:
        fail("Sprint 27 report still contains a qualification placeholder.")
    require(
        report,
        (
            f"{DECLARED_TESTS} tests × 3 runs, 0 failures",
            hashes["DLL"],
            hashes["PDB"],
            hashes["test output"],
            hashes["package"],
            "READY FOR KINGMAKER — Sprint 27 item-lifecycle recovery-contract smoke test",
        ),
        "Sprint 27 report",
    )

    validate_checksum_file("evidence/sprint27/compile/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint27/tests/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint27/package/SHA256SUMS.txt")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--final", action="store_true", help="also validate sealed qualification evidence")
    args = parser.parse_args()
    main_items, test_items = validate_structure()
    if args.final:
        validate_final()
    print("Sprint 27 source invariant validation passed" + (" with final evidence." if args.final else "."))
    print("Blueprints: 12 stable / 11 active / 1 reserved.")
    print(f"Compile items: main={len(main_items)}; tests={len(test_items)}.")
    print(f"Declared dependency-free tests: {DECLARED_TESTS} unique.")
    print("Lifecycle decision: retain exact empty/Wrecked item and qualify same-item empty/Broken overhaul.")
    print("Private runtime/compiler/framework binaries in source tree: none.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print(f"Sprint 27 validation failed: {exception}", file=sys.stderr)
        raise SystemExit(1)
