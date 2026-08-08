#!/usr/bin/env python3
from __future__ import annotations
import importlib.util
import json
import shutil
import sys
from pathlib import Path

sys.dont_write_bytecode = True

MODULE = Path(__file__).with_name("scan_optional_mod_sources.py")
spec = importlib.util.spec_from_file_location("compat_scan", MODULE)
scanner = importlib.util.module_from_spec(spec)
assert spec.loader
spec.loader.exec_module(scanner)

fixture = Path(__file__).resolve().parents[2] / "artifacts" / "compatibility" / "tests" / "static-scanner"
if fixture.exists():
    shutil.rmtree(fixture)
try:
    root = fixture / "repo"
    refs = fixture / "refs"
    (root / "src").mkdir(parents=True)
    (root / "compatibility").mkdir()
    (refs / "Third").mkdir(parents=True)
    guid = "1234567890abcdef1234567890abcdef"
    (root / "src" / "Main.cs").write_text(f'[HarmonyPatch(typeof(UnitViewHandsEquipment), "UpdateWeaponScale")]\n[HarmonyPostfix]\nvoid P() {{ AddAsset(x, "{guid}"); }}', encoding="utf-8")
    (refs / "Third" / "Main.cs").write_text(f'[HarmonyPatch(typeof(UnitViewHandsEquipment), "UpdateWeaponScale")]\n[HarmonyPrefix]\nvoid P() {{ AddBlueprint(x, "{guid}"); }}', encoding="utf-8")
    (refs / "Third" / "Info.json").write_text('{"Id":"Duplicate","Version":"1"}', encoding="utf-8")
    catalog = {"schemaVersion": 1, "references": [{"key": "third", "folderAliases": ["Third"], "role": "source-reference", "runtimeStagingAllowed": False, "availabilityDisposition": "NOT-TESTED"}]}
    (root / "compatibility" / "reference-catalog.json").write_text(json.dumps(catalog), encoding="utf-8")
    report = scanner.build_report(root, refs)
    assert len(report["hardGuidCollisions"]) == 1, json.dumps(report, indent=2)
    assert len(report["harmonyTargetOverlaps"]) == 1
    assert report["harmonyTargetOverlaps"][0]["patches"][0]["confidence"] == "high"
finally:
    if fixture.exists():
        shutil.rmtree(fixture)
print("Optional-mod static scanner fixtures passed.")
