#!/usr/bin/env python3
"""Validate the bounded 0.0.24.1 Sprint 24 Broken-reload repair."""
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
VERSION = "0.0.24.1"
INFORMATIONAL_VERSION = "0.0.24.1-s24-broken-reload-repair"
DECLARED_TESTS = 503


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
        (ROOT / "src/KingmakerGunslinger/Reloading" / name).resolve()
        for name in (
            "FirearmReloadResult.cs",
            "FirearmReloadTransactionService.cs",
            "ReloadTestMusketRuntime.cs",
        )
    }
    if not required_main.issubset(set(main_items)):
        fail("The main project does not compile every repaired reload source.")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    cases = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', program)
    if len(cases) != DECLARED_TESTS or len(cases) != len(set(cases)):
        fail(f"Expected {DECLARED_TESTS} unique declared tests, observed {len(cases)}.")
    required_cases = {
        "reload.transaction.success",
        "reload.transaction.broken-loads",
        "reload.transaction.loaded-broken-already-loaded",
        "reload.transaction.wrecked",
        "reload.result.success-validation",
        "reload.result.success-broken-validation",
        "misfire-condition.normal-to-broken",
        "misfire-condition.broken-to-wrecked",
    }
    if not required_cases.issubset({name for name, _ in cases}):
        fail("Required Broken-reload and condition-transition test declarations are missing.")

    runtime = read("src/KingmakerGunslinger/Reloading/ReloadTestMusketRuntime.cs")
    if 'state.Condition == FirearmCondition.Broken' in runtime and 'will remain Broken' not in runtime:
        fail("Reload availability still rejects Broken firearms.")
    if 'return Rejected("The equipped Test Musket is broken' in runtime:
        fail("Reload availability still contains the stale Broken rejection.")
    require(
        runtime,
        (
            "state.Condition == FirearmCondition.Wrecked",
            "the firearm will remain Broken",
            "Ready to load one Lead Ball with one Black Powder Charge.",
        ),
        "ReloadTestMusketRuntime",
    )

    transaction = read("src/KingmakerGunslinger/Reloading/FirearmReloadTransactionService.cs")
    if "return FirearmReloadStatus.Broken;" in transaction:
        fail("The atomic reload transaction still rejects Broken firearms.")
    require(
        transaction,
        (
            "state.Condition == FirearmCondition.Wrecked",
            "return FirearmReloadStatus.Wrecked;",
            "FirearmStateMachine.Load",
        ),
        "FirearmReloadTransactionService",
    )

    result = read("src/KingmakerGunslinger/Reloading/FirearmReloadResult.cs")
    require(
        result,
        (
            "BeforeState.Condition == FirearmCondition.Normal",
            "BeforeState.Condition == FirearmCondition.Broken",
            "AfterState.Condition != BeforeState.Condition",
            "preserve an empty Normal or Broken firearm's condition",
        ),
        "FirearmReloadResult",
    )

    state_machine = read("src/KingmakerGunslinger/Firearms/FirearmStateMachine.cs")
    require(
        state_machine,
        (
            "if (state.Condition == FirearmCondition.Wrecked)",
            "A wrecked firearm cannot be loaded.",
            "state.Condition);",
        ),
        "FirearmStateMachine",
    )

    ui = read("src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    require(
        ui,
        (
            "0.0.24.1 Sprint 24 broken-reload repair smoke test",
            "permits an empty Broken Test Musket to reload without repairing it",
            "Equip one empty Normal or Broken Test Musket",
            "Wrecked firearms must remain unavailable",
        ),
        "DevelopmentUi",
    )

    if read("SMOKE-TEST-GUIDE.md") != read("SMOKE-TEST-GUIDE-0.0.24.1.md"):
        fail("SMOKE-TEST-GUIDE.md must be byte-identical to the 0.0.24.1 guide.")
    guide = read("SMOKE-TEST-GUIDE-0.0.24.1.md")
    require(
        guide,
        (
            "## Test 1 — Broken reload readiness",
            "## Test 2 — Reload the exact Broken Test Musket",
            "## Test 3 — Broken misfire becomes Wrecked",
            "## Test 4 — Wrecked reload remains blocked",
            "## Evidence to capture",
        ),
        "0.0.24.1 smoke-test guide",
    )

    for required in (
        "SPRINT-24-REPAIR-REPORT-0.0.24.1.md",
        "docs/RELOAD-ABILITY.md",
        "docs/decisions/ADR-0031-condition-preserving-broken-reload.md",
        "planning/SPRINT-25-ENTRY-CRITERIA.md",
        "evidence/sprint24-repair/runtime-failure-2026-07-16/ASSESSMENT.md",
    ):
        read(required)
    validate_checksum_file("evidence/sprint24-repair/runtime-failure-2026-07-16/SHA256SUMS.txt")

    forbidden_names = {
        "Assembly-CSharp.dll",
        "Assembly-CSharp-firstpass.dll",
        "UnityModManager.dll",
        "0Harmony12.dll",
        "Newtonsoft.Json.dll",
    }
    loose_binaries = []
    for path in ROOT.rglob("*"):
        if not path.is_file():
            continue
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
        "brokenReloadRepairImplemented": True,
        "brokenReloadRuntimeAcceptancePassed": False,
        "sprint25EntryApproved": False,
        "privateReferencesRedistributed": False,
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

    compile_evidence = json.loads(read("evidence/sprint24-repair/compile/deterministic-compile-evidence.json"))
    if compile_evidence.get("dll", {}).get("secondSha256") != dll_hash or compile_evidence.get("dll", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative DLL.")
    if compile_evidence.get("pdb", {}).get("secondSha256") != pdb_hash or compile_evidence.get("pdb", {}).get("byteIdentical") is not True:
        fail("Deterministic compile evidence does not prove the authoritative PDB.")

    test_evidence = json.loads(read("evidence/sprint24-repair/tests/executed-test-evidence.json"))
    if test_evidence.get("declaredTests") != DECLARED_TESTS or test_evidence.get("runs") != 3:
        fail("Executed test evidence has the wrong count or run count.")
    if test_evidence.get("failures") != 0 or test_evidence.get("repeatedOutputIdentical") is not True:
        fail("Executed test evidence does not prove three clean identical runs.")
    if test_evidence.get("testOutputSha256") != test_hash:
        fail("Executed test evidence has the wrong output hash.")

    package_evidence = json.loads(read("evidence/sprint24-repair/package/standalone-package-evidence.json"))
    if package_evidence.get("sha256") != package_hash or package_evidence.get("modDllSha256") != dll_hash:
        fail("Standalone package evidence has the wrong package or DLL hash.")
    if package_evidence.get("entryCount") != 8 or package_evidence.get("binaryCount") != 1:
        fail("Standalone package evidence has the wrong entry or binary count.")
    if package_evidence.get("privateReferencesRedistributed") is not False:
        fail("Standalone package evidence violates the private-reference boundary.")

    report = read("SPRINT-24-REPAIR-REPORT-0.0.24.1.md")
    if "PENDING_FINAL_QUALIFICATION" in report:
        fail("The repair report still contains a qualification placeholder.")
    require(
        report,
        (
            f"{DECLARED_TESTS} tests × 3 runs, 0 failures",
            dll_hash,
            pdb_hash,
            test_hash,
            package_hash,
            "READY FOR KINGMAKER — Sprint 24 Broken-reload repair smoke test",
        ),
        "Sprint 24 repair report",
    )

    validate_checksum_file("evidence/sprint24-repair/compile/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint24-repair/tests/SHA256SUMS.txt")
    validate_checksum_file("evidence/sprint24-repair/package/SHA256SUMS.txt")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--final", action="store_true", help="also validate sealed qualification evidence")
    args = parser.parse_args()
    main_items, test_items = validate_structure()
    if args.final:
        validate_final()
    print("Sprint 24 Broken-reload repair source invariant validation passed" + (" with final evidence." if args.final else "."))
    print("Blueprints: 12 stable / 11 active / 1 reserved.")
    print(f"Compile items: main={len(main_items)}; tests={len(test_items)}.")
    print(f"Declared dependency-free tests: {DECLARED_TESTS} unique.")
    print("Reload states: empty Normal and empty Broken load; Wrecked rejects; condition preserved.")
    print("Private runtime/compiler binaries in source tree: none.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print(f"Sprint 24 repair validation failed: {exception}", file=sys.stderr)
        raise SystemExit(1)
