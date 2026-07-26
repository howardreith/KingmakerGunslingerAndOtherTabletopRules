#!/usr/bin/env python3
"""Portable Sprint 21 source invariant validator."""
from __future__ import annotations

import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def fail(message: str) -> None:
    raise RuntimeError(message)


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.is_file():
        fail(f"Required file is missing: {relative}")
    return path.read_text(encoding="utf-8")


def main() -> int:
    info = json.loads(read("Info.json"))
    if info.get("Version") != "0.0.21":
        fail("Info.json must declare version 0.0.21.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json must retain Unity Mod Manager 0.32.4.")

    props = read("Directory.Build.props")
    for required in (
        "<KmgVersion>0.0.21</KmgVersion>",
        "<KmgInformationalVersion>0.0.21-s21-reload-ability</KmgInformationalVersion>",
        "<LangVersion>7.3</LangVersion>",
        "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
    ):
        if required not in props:
            fail(f"Directory.Build.props is missing {required}.")

    assembly_info = read("src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
    if 'AssemblyVersion("0.0.21.0")' not in assembly_info:
        fail("Assembly version is not 0.0.21.0.")
    if 'AssemblyInformationalVersion("0.0.21-s21-reload-ability")' not in assembly_info:
        fail("Assembly informational version is not Sprint 21.")

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
    reload_entry = next((entry for entry in entries if entry.get("symbol") == "KMG.Test.ReloadAbility"), None)
    if reload_entry is None or reload_entry.get("status") != "active" or reload_entry.get("guid") != "19e24b74331f437282077ce58e739d0f":
        fail("The stable Reload Test Musket blueprint must be active with its reserved GUID.")

    ET.fromstring(read("src/KingmakerGunslinger/KingmakerGunslinger.csproj"))
    ET.fromstring(read("tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj"))
    main_project = read("src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    required_main = (
        "Blueprints\\ReloadTestMusketAbilityBlueprints.cs",
        "Reloading\\FirearmReloadTransactionService.cs",
        "Reloading\\ReloadTestMusketAbilityLogic.cs",
        "Reloading\\ReloadTestMusketRuntime.cs",
    )
    for required in required_main:
        if required not in main_project:
            fail(f"Main project omits {required}.")

    bootstrap = read("src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs")
    if "ExpectedRegisteredBlueprintCount = 11" not in bootstrap:
        fail("Blueprint bootstrap must require exactly eleven registrations.")
    if "ReloadTestMusketAbilityBlueprints.Register" not in bootstrap:
        fail("Blueprint bootstrap does not register the reload ability.")
    if "AttachReloadAbility" not in bootstrap:
        fail("Blueprint bootstrap does not attach reload to Firearm Proficiency.")

    ability = read("src/KingmakerGunslinger/Blueprints/ReloadTestMusketAbilityBlueprints.cs")
    for required in (
        "AbilityType.Extraordinary",
        "AbilityRange.Personal",
        "SetIsFullRoundAction(true)",
        "ActionBarAutoFillIgnored = false",
        "ReloadTestMusketAbilityLogic.Create",
    ):
        if required not in ability:
            fail(f"Reload ability blueprint is missing {required}.")

    logic = read("src/KingmakerGunslinger/Reloading/ReloadTestMusketAbilityLogic.cs")
    if "public override IEnumerator<AbilityDeliveryTarget> Deliver" not in logic:
        fail("Reload mutation must occur through ability delivery.")
    if "ReloadTestMusketRuntime.Execute" not in logic:
        fail("Reload delivery is not connected to the runtime transaction.")

    transaction = read("src/KingmakerGunslinger/Reloading/FirearmReloadTransactionService.cs")
    for required in (
        "TryConsumeOneLoad",
        "stateStore.Replace",
        "RestoreState",
        "RestoreExact",
        "FirearmReloadTransactionException",
    ):
        if required not in transaction:
            fail(f"Reload transaction is missing {required}.")

    program = read("tests/KingmakerGunslinger.DomainTests/Program.cs")
    declarations = re.findall(r'Case\("([^"]+)"\s*,', program)
    if len(declarations) != 419 or len(set(declarations)) != 419:
        fail(f"Expected 419 unique test declarations; observed {len(declarations)} total and {len(set(declarations))} unique.")
    reload_cases = [name for name in declarations if name.startswith("reload.")]
    if len(reload_cases) != 21:
        fail(f"Expected 21 reload-specific cases; observed {len(reload_cases)}.")

    for required in (
        "SPRINT-21-REPORT.md",
        "SMOKE-TEST-GUIDE-0.0.21.md",
        "docs/RELOAD-ABILITY.md",
        "docs/decisions/ADR-0026-full-round-reload-cross-resource-transaction.md",
        "planning/SPRINT-22-ENTRY-CRITERIA.md",
    ):
        read(required)

    forbidden_extensions = {".dll", ".exe", ".pdb", ".mdb"}
    for path in ROOT.rglob("*"):
        if path.is_file() and path.suffix.lower() in forbidden_extensions:
            if "evidence" not in path.parts:
                fail(f"Source tree contains a compiled binary: {path.relative_to(ROOT)}")

    print("Sprint 21 source invariant validation passed.")
    print("Blueprints: 12 stable, 11 active, 1 reserved.")
    print("Tests: 419 declared, including 21 reload cases.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print("Sprint 21 validation FAILED: " + str(exception), file=sys.stderr)
        raise SystemExit(1)
