#!/usr/bin/env python3
"""Portable Sprint 20 source validator.

This validator checks the bounded basic-ammunition milestone without requiring
Kingmaker or the private build-reference bundle. It deliberately verifies that
Sprint 19's accepted item-token carrier files are byte-for-byte unchanged.
"""
from __future__ import annotations

import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
ROOT = Path(__file__).resolve().parents[1]

EXPECTED_CARRIER_HASHES = {
    "src/KingmakerGunslinger/Firearms/FirearmStateTokenCatalog.cs":
        "f2367738ba113b00dd28d1db9936cacaa8cb2106acd90349433e7fdfc032b7a5",
    "src/KingmakerGunslinger/Firearms/TokenBackedFirearmStateRepository.cs":
        "a9820796cf2c8e12f6026d999647d0148855eb3da3cac9c43957918a735a085e",
    "src/KingmakerGunslinger/Firearms/KingmakerFirearmStateTokenStore.cs":
        "efe1f89adf3649ef77c997d834ea3c445ee26471dcda82639dfae825e9d5c645",
    "src/KingmakerGunslinger/Blueprints/FirearmStateTokenBlueprints.cs":
        "eaa38b34698ad824eb052b553eed824ed563c058edd51d903ca086a40692a36a",
}

EXPECTED_AMMUNITION_GUIDS = {
    "KMG.Test.BlackPowderItem": "ea966bf998a647cf97b0ed92f71c4b7d",
    "KMG.Test.LeadBulletItem": "55c29771445947d685dba9e1ead46a42",
}

REQUIRED_FILES = [
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionComponent.cs",
    "src/KingmakerGunslinger/Ammunition/IBasicAmmunitionInventory.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionInventorySnapshot.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionTransactionResult.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionTransactionException.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionTransactionService.cs",
    "src/KingmakerGunslinger/Ammunition/KingmakerBasicAmmunitionInventory.cs",
    "src/KingmakerGunslinger/Blueprints/BasicAmmunitionBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/BlueprintItemAccess.cs",
    "src/KingmakerGunslinger/Blueprints/LocalizationService.cs",
    "docs/BASIC-AMMUNITION.md",
    "planning/SPRINT-21-ENTRY-CRITERIA.md",
    "SPRINT-20-REPORT.md",
    "SMOKE-TEST-GUIDE-0.0.20.md",
]

PURE_FILES = [
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionComponent.cs",
    "src/KingmakerGunslinger/Ammunition/IBasicAmmunitionInventory.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionInventorySnapshot.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionTransactionResult.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionTransactionException.cs",
    "src/KingmakerGunslinger/Ammunition/BasicAmmunitionTransactionService.cs",
]

FORBIDDEN_BINARY_SUFFIXES = {
    ".dll", ".exe", ".pdb", ".mdb", ".so", ".dylib", ".class", ".pyc"
}


def fail(message: str) -> None:
    raise RuntimeError(message)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def compile_items(relative_project: str) -> list[str]:
    project = ROOT / relative_project
    tree = ET.parse(project)
    result = [
        node.attrib["Include"]
        for node in tree.findall(".//m:Compile", MSBUILD_NS)
        if node.attrib.get("Include")
    ]
    if not result or len(result) != len(set(result)):
        fail(f"Compile items are empty or duplicated in {relative_project}.")
    for include in result:
        source = (project.parent / include.replace("\\", "/")).resolve()
        if not source.is_file():
            fail(f"Compile item does not exist: {relative_project}: {include}")
    return result


def validate_versions() -> None:
    info = json.loads((ROOT / "Info.json").read_text(encoding="utf-8"))
    if info.get("Version") != "0.0.20":
        fail("Info.json must declare version 0.0.20.")
    if info.get("ManagerVersion") != "0.32.4":
        fail("Info.json must retain the runtime-proven UMM version 0.32.4.")
    props = (ROOT / "Directory.Build.props").read_text(encoding="utf-8")
    for expected in (
        "<KmgVersion>0.0.20</KmgVersion>",
        "<KmgInformationalVersion>0.0.20-s20-basic-ammunition</KmgInformationalVersion>",
        "<LangVersion>7.3</LangVersion>",
        "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
    ):
        if expected not in props:
            fail(f"Directory.Build.props is missing {expected}.")


def validate_manifest() -> None:
    manifest = json.loads((ROOT / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    entries = manifest.get("entries")
    if not isinstance(entries, list) or len(entries) != 12:
        fail("Blueprint manifest must contain exactly 12 stable entries.")
    symbols = [entry.get("symbol") for entry in entries]
    guids = [entry.get("guid") for entry in entries]
    if len(symbols) != len(set(symbols)) or len(guids) != len(set(guids)):
        fail("Blueprint symbols and GUIDs must remain unique.")
    if any(not re.fullmatch(r"[0-9a-f]{32}", value or "") for value in guids):
        fail("Every blueprint GUID must be lowercase 32-character hexadecimal.")
    by_symbol = {entry["symbol"]: entry for entry in entries}
    for symbol, guid in EXPECTED_AMMUNITION_GUIDS.items():
        entry = by_symbol.get(symbol)
        if entry is None:
            fail(f"Missing ammunition blueprint manifest entry: {symbol}")
        if entry.get("guid") != guid or entry.get("status") != "active":
            fail(f"Ammunition blueprint entry is not active with its reserved GUID: {symbol}")
        if entry.get("plannedType") != "BlueprintItem":
            fail(f"Ammunition blueprint must remain a BlueprintItem: {symbol}")
    if sum(entry.get("status") == "active" for entry in entries) != 10:
        fail("Sprint 20 must have exactly 10 active blueprint entries.")
    reload_entry = by_symbol.get("KMG.Test.ReloadAbility")
    if reload_entry is None or reload_entry.get("status") != "reserved":
        fail("The reload ability must remain reserved in Sprint 20.")


def validate_source_structure() -> None:
    for relative in REQUIRED_FILES:
        if not (ROOT / relative).is_file():
            fail(f"Required Sprint 20 file is missing: {relative}")
    main_items = compile_items("src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    test_items = compile_items("tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj")
    if len(main_items) != 114:
        fail(f"Expected 114 main compile items; observed {len(main_items)}.")
    if len(test_items) != 70:
        fail(f"Expected 70 test compile items; observed {len(test_items)}.")
    program = (ROOT / "tests/KingmakerGunslinger.DomainTests/Program.cs").read_text(encoding="utf-8")
    cases = re.findall(r'Case\("([^"]+)",\s*([A-Za-z0-9_]+)\)', program)
    if len(cases) != 398 or len({name for name, _ in cases}) != 398:
        fail(f"Expected 398 unique test declarations; observed {len(cases)}.")
    ammunition_cases = [name for name, _ in cases if name.startswith("ammo.")]
    if len(ammunition_cases) != 25:
        fail(f"Expected 25 basic-ammunition tests; observed {len(ammunition_cases)}.")
    bootstrap = (ROOT / "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs").read_text(encoding="utf-8")
    if "ExpectedRegisteredBlueprintCount = 10" not in bootstrap:
        fail("Blueprint bootstrap must require exactly 10 registrations.")
    if "BasicAmmunitionBlueprints.Register" not in bootstrap:
        fail("Blueprint bootstrap does not register basic ammunition.")


def validate_layering_and_scope() -> None:
    forbidden_tokens = (
        "using Kingmaker", "Kingmaker.", "using Unity", "UnityEngine",
        "Harmony", "UnityModManager"
    )
    for relative in PURE_FILES:
        text = (ROOT / relative).read_text(encoding="utf-8")
        for token in forbidden_tokens:
            if token in text:
                fail(f"Pure ammunition-domain file depends on an engine layer: {relative}: {token}")
    ammo_tree = "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted((ROOT / "src/KingmakerGunslinger/Ammunition").glob("*.cs"))
    )
    if "FirearmStateMachine.Load" in ammo_tree or "ReloadAbility" in ammo_tree:
        fail("Sprint 20 ammunition code must not load a firearm or implement a reload ability.")
    ui = (ROOT / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    for label in (
        "Add 20 Black Powder Charges and 20 Lead Balls",
        "Consume one powder + ball pair atomically",
        "Print basic-ammunition counts",
    ):
        if label not in ui:
            fail(f"Development UI is missing required control: {label}")


def validate_carrier_unchanged() -> None:
    for relative, expected in EXPECTED_CARRIER_HASHES.items():
        path = ROOT / relative
        if not path.is_file():
            fail(f"Accepted Sprint 19 carrier file is missing: {relative}")
        actual = sha256(path)
        if actual != expected:
            fail(
                "Sprint 19's runtime-proven item-token carrier changed during the "
                f"ammunition sprint: {relative}: expected={expected}; actual={actual}"
            )


def validate_source_cleanliness() -> None:
    for path in ROOT.rglob("*"):
        if path.is_symlink():
            fail(f"Source tree contains a symlink: {path.relative_to(ROOT)}")
        if path.is_file() and path.suffix.lower() in FORBIDDEN_BINARY_SUFFIXES:
            fail(f"Source tree contains a compiled/binary artifact: {path.relative_to(ROOT)}")
    for forbidden_name in ("GamePath.props", "runtime-contracts.json"):
        if (ROOT / forbidden_name).exists():
            fail(f"Local environment file must not be packaged: {forbidden_name}")


def main() -> int:
    validate_versions()
    validate_manifest()
    validate_source_structure()
    validate_layering_and_scope()
    validate_carrier_unchanged()
    validate_source_cleanliness()
    print("Sprint 20 portable validation passed.")
    print("Main compile items: 114; test compile items: 70; declared tests: 398; ammo tests: 25.")
    print("Blueprints: 12 stable entries; 10 active; reload ability remains reserved.")
    print("Sprint 19 item-token carrier hashes are unchanged.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exception:
        print("Sprint 20 validation FAILED: " + str(exception), file=sys.stderr)
        raise SystemExit(1)
