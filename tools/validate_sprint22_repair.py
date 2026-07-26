#!/usr/bin/env python3
"""Portable source invariant validator for the 0.0.22.1 Sprint 22 repair."""
from __future__ import annotations

import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}

EXPECTED_SCREENSHOTS = {
    "01-empty-ready-to-reload.png": "ebcb4ba75ec3c44ca53a86ecbb6fbec10464444f3af610b7415932b5376d7844",
    "02-loaded-after-quicksave.png": "ef23645bf52e3b272226c197a49a0f2f21acbe591ba5690e876b513b42c562d6",
    "03-native-heavy-crossbow-pipeline-attack.png": "8e63e998216606b8005ac7ba13032d4cc1a06adea01e69b4b899326613aea529",
    "04-post-attack-loaded-state-and-shield-faults.png": "b3e9fbd6816dfdebd6001fdf11ca6d7a814e2145fc266f472dc3f85773d8c828",
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
    result: list[Path] = []
    for node in tree.findall(".//m:Compile", MSBUILD_NS):
        include = node.attrib.get("Include")
        if not include:
            continue
        item = (project.parent / include.replace("\\", "/")).resolve()
        if not item.is_file():
            fail(f"Compile item does not exist: {include} -> {item}")
        result.append(item)
    if not result or len(result) != len(set(result)):
        fail(f"Compile items are empty or duplicated in {relative}.")
    return result


def main() -> int:
    info = json.loads(read("Info.json"))
    if info.get("Version") != "0.0.22.1":
        fail("Info.json must declare version 0.0.22.1.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json must retain Unity Mod Manager 0.32.4.")

    props = read("Directory.Build.props")
    for required in (
        "<KmgVersion>0.0.22.1</KmgVersion>",
        "<KmgInformationalVersion>0.0.22.1-s22-attack-hook-repair</KmgInformationalVersion>",
        "<LangVersion>7.3</LangVersion>",
        "<PlatformTarget>AnyCPU</PlatformTarget>",
        "<Deterministic>true</Deterministic>",
        "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
    ):
        if required not in props:
            fail(f"Directory.Build.props is missing {required}.")

    assembly_info = read("src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
    for required in (
        'AssemblyVersion("0.0.22.1")',
        'AssemblyFileVersion("0.0.22.1")',
        'AssemblyInformationalVersion("0.0.22.1-s22-attack-hook-repair")',
    ):
        if required not in assembly_info:
            fail(f"Assembly metadata is missing {required}.")

    manifest = json.loads(read("blueprints/blueprints.json"))
    entries = manifest.get("entries", [])
    if len(entries) != 12:
        fail(f"Expected 12 stable blueprint ledger entries; observed {len(entries)}.")
    active = [entry for entry in entries if entry.get("status") == "active"]
    reserved = [entry for entry in entries if entry.get("status") == "reserved"]
    if len(active) != 11 or len(reserved) != 1:
        fail(
            f"Expected 11 active and 1 reserved blueprint; observed "
            f"{len(active)} active and {len(reserved)} reserved."
        )
    symbols = [entry.get("symbol") for entry in entries]
    guids = [entry.get("guid") for entry in entries]
    if len(set(symbols)) != len(symbols) or len(set(guids)) != len(guids):
        fail("Blueprint symbols and GUIDs must be unique.")
    for guid in guids:
        if not isinstance(guid, str) or re.fullmatch(r"[0-9a-f]{32}", guid) is None or guid == "0" * 32:
            fail(f"Invalid stable blueprint GUID: {guid!r}")

    main_items = compile_items("src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    test_items = compile_items("tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj")
    main_project = read("src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    test_project = read("tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj")
    for required in (
        "Diagnostics\\RuleEventPatchContract.cs",
        "Diagnostics\\RuleEventPatchTarget.cs",
        "Firearms\\FirearmStateTokenReconciliationPatch.cs",
        "Firearms\\FirearmStateTokenReconciliationRuntime.cs",
        "Firing\\FirearmDischargeRuntime.cs",
    ):
        if required not in main_project:
            fail(f"Main project omits {required}.")
    if "Diagnostics\\RuleEventPatchContract.cs" not in test_project:
        fail("Domain-test project omits the exact rule-event patch contract helper.")

    contract = read("src/KingmakerGunslinger/Diagnostics/RuleEventPatchContract.cs")
    for required in (
        "method.ReturnType != typeof(void)",
        "parameters.Length == 1",
        "parameters[0].ParameterType == eventContextType",
    ):
        if required not in contract:
            fail(f"Rule-event contract helper is missing {required}.")

    target = read("src/KingmakerGunslinger/Diagnostics/RuleEventPatchTarget.cs")
    for required in (
        "using Kingmaker.RuleSystem;",
        "BindingFlags.DeclaredOnly",
        "RuleEventPatchContract.IsCompatibleOnTrigger",
        "typeof(RulebookEventContext)",
        "expected one void instance OnTrigger(RulebookEventContext) method",
    ):
        if required not in target:
            fail(f"Rule-event target resolver is missing {required!r}.")
    if "method.GetParameters().Length == 0" in target or "zero-argument instance OnTrigger" in target:
        fail("The rejected zero-argument rule-event target assumption was reintroduced.")

    reconcile_patch = read("src/KingmakerGunslinger/Firearms/FirearmStateTokenReconciliationPatch.cs")
    for required in (
        'method.Name,\n                        "ApplyEnchantments"',
        "method.GetParameters().Length == 0",
        "method.ReturnType == typeof(void)",
        "FirearmStateTokenReconciliationRuntime.Before",
        "FirearmStateTokenReconciliationRuntime.After",
    ):
        if required not in reconcile_patch:
            fail(f"Native token-reconciliation patch is missing {required!r}.")

    for required in (
        "using Kingmaker.Items;",
        "ItemEntityWeapon weapon = __instance as ItemEntityWeapon;",
        "weapon == null",
        "FirearmStateTokenReconciliationInvocation.Empty",
        "FirearmStateTokenReconciliationRuntime.Before(weapon)",
    ):
        if required not in reconcile_patch:
            fail(f"Weapon-only reconciliation prefix is missing {required!r}.")
    guard_index = reconcile_patch.find("ItemEntityWeapon weapon = __instance as ItemEntityWeapon;")
    before_index = reconcile_patch.find("FirearmStateTokenReconciliationRuntime.Before(weapon)")
    if guard_index < 0 or before_index < 0 or guard_index > before_index:
        fail("Non-weapon rejection must occur before token inspection.")

    reconcile_runtime = read("src/KingmakerGunslinger/Firearms/FirearmStateTokenReconciliationRuntime.cs")
    for required in (
        "RestoreMissingStateToken",
        "verified.Count != 1",
        "FirearmStateTokenReconciliationAction.Conflict",
    ):
        if required not in reconcile_runtime:
            fail(f"Token-reconciliation runtime is missing {required!r}.")

    trace_patches = read("src/KingmakerGunslinger/Diagnostics/CombatTracePatches.cs")
    discharge_index = trace_patches.find("FirearmDischargeRuntime.BeforeAttackRoll")
    ac_index = trace_patches.find("FirearmArmorClassRuntime.BeforeAttackRoll")
    if discharge_index < 0 or ac_index < 0 or discharge_index > ac_index:
        fail("Loaded-round enforcement must run before touch-AC attack-roll observation.")

    discharge_runtime = read("src/KingmakerGunslinger/Firing/FirearmDischargeRuntime.cs")
    for required in (
        "RuleAttackRoll attackRoll",
        "FirearmMarkerLookup.ReadFromRuleEvent",
        "FirearmRuntimeState.Service.Transition",
        "attackRoll.AutoHit = false",
        "attackRoll.AutoMiss = true",
        "ReferenceEventGate",
    ):
        if required not in discharge_runtime:
            fail(f"Discharge runtime is missing {required}.")
    if any(term in discharge_runtime for term in ("BasicAmmunition", "BlackPowder", "LeadBall")):
        fail("Attack-time discharge must not consume inventory ammunition.")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    declarations = re.findall(r'Case\("([^"]+)"\s*,', program)
    if len(declarations) != 455 or len(set(declarations)) != 455:
        fail(
            f"Expected 455 unique test declarations; observed "
            f"{len(declarations)} total and {len(set(declarations))} unique."
        )
    sprint22_cases = [
        name for name in declarations
        if name.startswith("discharge.") or name.startswith("event-gate.") or name.startswith("token-reconcile.")
    ]
    if len(sprint22_cases) != 27:
        fail(f"Expected 27 original Sprint 22 cases; observed {len(sprint22_cases)}.")
    repair_cases = [name for name in declarations if name.startswith("patch-target.")]
    if len(repair_cases) != 9:
        fail(f"Expected 9 patch-target repair cases; observed {len(repair_cases)}.")

    inspector = read("scripts/inspect-runtime-contracts.ps1")
    for required in (
        "Kingmaker.RuleSystem.RulebookEventContext",
        "$parameters.Count -eq 1",
        "$parameters[0].ParameterType.FullName -eq $rulebookEventContextType.FullName",
        "requiredOnTriggerSignature = 'System.Void OnTrigger(Kingmaker.RuleSystem.RulebookEventContext)'",
        "$itemApplyEnchantmentsMethods.Count -eq 1",
        "firearmStateTokenCarrier = [ordered]@{",
        "requiredForCurrentBuild = $false",
    ):
        if required not in inspector:
            fail(f"Runtime-contract inspector is missing {required!r}.")
    contract_gate_start = inspector.find("$contractPassed = (")
    contract_gate_end = inspector.find("\n    )", contract_gate_start)
    contract_gate = inspector[contract_gate_start:contract_gate_end]
    if contract_gate_start < 0 or contract_gate_end < 0:
        fail("Could not locate the runtime-contract gate.")
    for rejected_gate in ("$firearmItemIdentityContractPassed", "$unitPartVaultContractPassed"):
        if rejected_gate in contract_gate:
            fail(f"The rejected identity-vault contract remains a current build gate: {rejected_gate}")

    package_script = read("scripts/package.ps1")
    for required in (
        "CHANGELOG.md",
        "LICENSE",
        "README.md",
        "SMOKE-TEST-GUIDE.md",
        "strict eight-file standalone UMM archive",
        "PDB symbols must not be included",
    ):
        if required not in package_script and required not in read("scripts/README.md"):
            fail(f"Current package tooling is missing {required!r}.")

    package_validator = read("scripts/validate-package.ps1")
    for required in (
        "The standalone UMM package must contain exactly one binary",
        "Package entries do not match the strict eight-file allowlist",
        "SMOKE-TEST-GUIDE.md",
    ):
        if required not in package_validator:
            fail(f"PowerShell package validator is missing {required!r}.")

    qualifier = read("scripts/qualify-runtime-candidate.ps1")
    for forbidden in (
        "0.0.18-s18-runtime-smoke-candidate",
        "NoGoIncomplete",
        "blockedRowsBeforeRuntimeTest",
    ):
        if forbidden in qualifier:
            fail(f"Current qualification script retains obsolete persistence-era text: {forbidden}")
    for required in (
        "Completed 455 tests; failures=0.",
        "Same-output-path deterministic compilation failed",
        "sprint23Blocked = $true",
        "attack-hook-repair-smoke-test.zip",
    ):
        if required not in qualifier:
            fail(f"Current qualification script is missing {required!r}.")

    if read("SMOKE-TEST-GUIDE.md") != read("SMOKE-TEST-GUIDE-0.0.22.1.md"):
        fail("SMOKE-TEST-GUIDE.md must match the versioned 0.0.22.1 guide exactly.")

    for required in (
        "SPRINT-22-REPORT.md",
        "SPRINT-22-REPAIR-REPORT-0.0.22.1.md",
        "SMOKE-TEST-GUIDE-0.0.22.md",
        "SMOKE-TEST-GUIDE-0.0.22.1.md",
        "planning/SPRINT-23-ENTRY-CRITERIA.md",
        "docs/decisions/ADR-0028-exact-rule-event-contract-and-weapon-only-reconciliation.md",
        "evidence/sprint22-repair/runtime-result-assessment.md",
        "evidence/sprint22-repair/exact-runtime-contracts.json",
    ):
        read(required)

    contracts = json.loads(read("evidence/sprint22-repair/exact-runtime-contracts.json"))
    expected_assembly_hash = "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb"
    if contracts.get("assembly", {}).get("sha256") != expected_assembly_hash:
        fail("Exact-runtime contract evidence references the wrong Assembly-CSharp.dll hash.")
    expected_source_hashes = {
        "ruleEventPatchContractSha256": sha256(ROOT / "src/KingmakerGunslinger/Diagnostics/RuleEventPatchContract.cs"),
        "ruleEventPatchTargetSha256": sha256(ROOT / "src/KingmakerGunslinger/Diagnostics/RuleEventPatchTarget.cs"),
        "tokenReconciliationPatchSha256": sha256(ROOT / "src/KingmakerGunslinger/Firearms/FirearmStateTokenReconciliationPatch.cs"),
        "runtimeInspectionScriptSha256": sha256(ROOT / "scripts/inspect-runtime-contracts.ps1"),
    }
    if contracts.get("source") != expected_source_hashes:
        fail("Exact-runtime contract evidence source hashes do not match the repair tree.")
    for type_name in (
        "Kingmaker.RuleSystem.Rules.RuleAttackRoll",
        "Kingmaker.RuleSystem.Rules.RuleAttackWithWeapon",
        "Kingmaker.RuleSystem.Rules.RuleCalculateAC",
    ):
        methods = contracts.get("ruleEventContracts", {}).get(type_name, [])
        if len(methods) != 1 or not methods[0].get("repairedExactMatcherWouldAccept"):
            fail(f"Exact-runtime contract evidence is incomplete for {type_name}.")
        parameter_types = methods[0].get("signature", {}).get("parameterTypes", [])
        if parameter_types != ["Kingmaker.RuleSystem.RulebookEventContext"]:
            fail(f"Unexpected OnTrigger parameter contract for {type_name}: {parameter_types!r}")

    screenshot_dir = ROOT / "evidence" / "sprint22-repair" / "runtime-screenshots"
    for name, expected_hash in EXPECTED_SCREENSHOTS.items():
        path = screenshot_dir / name
        if not path.is_file():
            fail(f"Runtime screenshot is missing: {path.relative_to(ROOT)}")
        actual_hash = sha256(path)
        if actual_hash != expected_hash:
            fail(f"Runtime screenshot hash mismatch for {name}: {actual_hash}")

    forbidden_extensions = {".dll", ".exe", ".pdb", ".mdb"}
    for path in ROOT.rglob("*"):
        if path.is_file() and path.suffix.lower() in forbidden_extensions and "evidence" not in path.parts:
            fail(f"Source tree contains a compiled binary: {path.relative_to(ROOT)}")
    forbidden_private_names = {
        "Assembly-CSharp.dll",
        "Assembly-CSharp-firstpass.dll",
        "UnityModManager.dll",
        "0Harmony12.dll",
        "Newtonsoft.Json.dll",
    }
    for path in ROOT.rglob("*"):
        if path.is_file() and path.name in forbidden_private_names:
            fail(f"Source tree contains a private runtime reference: {path.relative_to(ROOT)}")

    print("Sprint 22 repair source invariant validation passed.")
    print("Blueprints: 12 stable, 11 active, 1 reserved.")
    print(f"Compile items: {len(main_items)} main, {len(test_items)} test.")
    print("Tests: 455 declared, including 27 Sprint 22 and 9 patch-target repair cases.")
    print("Repair invariants: exact RulebookEventContext hooks and weapon-only token inspection.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print("Sprint 22 repair validation FAILED: " + str(exception), file=sys.stderr)
        raise SystemExit(1)
