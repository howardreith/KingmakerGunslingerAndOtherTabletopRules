#!/usr/bin/env python3
"""Portable Sprint 22 source invariant validator."""
from __future__ import annotations

import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}


def fail(message: str) -> None:
    raise RuntimeError(message)


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.is_file():
        fail(f"Required file is missing: {relative}")
    return path.read_text(encoding="utf-8")


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
    if info.get("Version") != "0.0.22":
        fail("Info.json must declare version 0.0.22.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json must retain Unity Mod Manager 0.32.4.")

    props = read("Directory.Build.props")
    for required in (
        "<KmgVersion>0.0.22</KmgVersion>",
        "<KmgInformationalVersion>0.0.22-s22-loaded-round-enforcement</KmgInformationalVersion>",
        "<LangVersion>7.3</LangVersion>",
        "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
    ):
        if required not in props:
            fail(f"Directory.Build.props is missing {required}.")

    assembly_info = read("src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
    if 'AssemblyVersion("0.0.22.0")' not in assembly_info:
        fail("Assembly version is not 0.0.22.0.")
    if 'AssemblyInformationalVersion("0.0.22-s22-loaded-round-enforcement")' not in assembly_info:
        fail("Assembly informational version is not Sprint 22.")

    manifest = json.loads(read("blueprints/blueprints.json"))
    entries = manifest.get("entries", [])
    if len(entries) != 12:
        fail(f"Expected 12 stable blueprint ledger entries; observed {len(entries)}.")
    active = [entry for entry in entries if entry.get("status") == "active"]
    reserved = [entry for entry in entries if entry.get("status") == "reserved"]
    if len(active) != 11 or len(reserved) != 1:
        fail(f"Expected 11 active and 1 reserved blueprint; observed {len(active)} active and {len(reserved)} reserved.")
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
    for required in (
        "Firing\\FirearmDischargeResult.cs",
        "Firing\\FirearmDischargeRuntime.cs",
        "Firing\\FirearmDischargeRuntimeDiagnostics.cs",
        "Firing\\FirearmDischargeService.cs",
        "Firing\\FirearmDischargeStatus.cs",
        "Firing\\ReferenceEventGate.cs",
        "Firearms\\FirearmStateTokenReconciliationPatch.cs",
        "Firearms\\FirearmStateTokenReconciliationRuntime.cs",
        "Firearms\\FirearmStateTokenReconciliationService.cs",
    ):
        if required not in main_project:
            fail(f"Main project omits {required}.")

    token_store = read("src/KingmakerGunslinger/Firearms/KingmakerFirearmStateTokenStore.cs")
    for required in (
        "MechanicsContext parentContext = CreateParentContext",
        "item.AddEnchantment(",
        "item.Wielder ?? item.Owner",
        "new MechanicsContext(",
    ):
        if required not in token_store:
            fail(f"State-token store is missing durability behavior: {required}")

    reconcile_patch = read("src/KingmakerGunslinger/Firearms/FirearmStateTokenReconciliationPatch.cs")
    for required in (
        'method.Name,\n                        "ApplyEnchantments"',
        "method.GetParameters().Length == 0",
        "FirearmStateTokenReconciliationRuntime.Before",
        "FirearmStateTokenReconciliationRuntime.After",
    ):
        if required not in reconcile_patch:
            fail(f"Native token-reconciliation patch is missing {required!r}.")

    reconcile_runtime = read("src/KingmakerGunslinger/Firearms/FirearmStateTokenReconciliationRuntime.cs")
    for required in (
        "RestoreMissingStateToken",
        "verified.Count != 1",
        "FirearmStateTokenReconciliationAction.Conflict",
    ):
        if required not in reconcile_runtime:
            fail(f"Token-reconciliation runtime is missing {required}.")

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
    if "BasicAmmunition" in discharge_runtime or "BlackPowder" in discharge_runtime or "LeadBall" in discharge_runtime:
        fail("Attack-time discharge must not consume inventory ammunition.")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    declarations = re.findall(r'Case\("([^"]+)"\s*,', program)
    if len(declarations) != 446 or len(set(declarations)) != 446:
        fail(f"Expected 446 unique test declarations; observed {len(declarations)} total and {len(set(declarations))} unique.")
    sprint22_cases = [
        name for name in declarations
        if name.startswith("discharge.") or name.startswith("event-gate.") or name.startswith("token-reconcile.")
    ]
    if len(sprint22_cases) != 27:
        fail(f"Expected 27 Sprint 22 cases; observed {len(sprint22_cases)}.")

    for required in (
        "SPRINT-22-REPORT.md",
        "SMOKE-TEST-GUIDE-0.0.22.md",
        "docs/LOADED-ROUND-ATTACK-AND-TOKEN-DURABILITY.md",
        "docs/decisions/ADR-0027-loaded-round-enforcement-and-native-token-reconciliation.md",
        "planning/SPRINT-23-ENTRY-CRITERIA.md",
    ):
        read(required)

    forbidden_extensions = {".dll", ".exe", ".pdb", ".mdb"}
    for path in ROOT.rglob("*"):
        if path.is_file() and path.suffix.lower() in forbidden_extensions:
            if "evidence" not in path.parts:
                fail(f"Source tree contains a compiled binary: {path.relative_to(ROOT)}")

    print("Sprint 22 source invariant validation passed.")
    print("Blueprints: 12 stable, 11 active, 1 reserved.")
    print(f"Compile items: {len(main_items)} main, {len(test_items)} test.")
    print("Tests: 446 declared, including 27 Sprint 22 cases.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print("Sprint 22 validation FAILED: " + str(exception), file=sys.stderr)
        raise SystemExit(1)
