#!/usr/bin/env python3
import json
from pathlib import Path

def validate(root: Path) -> None:
    source=(root/"src/KingmakerGunslinger/Blueprints/MysteriousStrangerBlueprints.cs").read_text(encoding="utf-8")
    mechanics=(root/"src/KingmakerGunslinger/Archetypes/MysteriousStrangerMechanics.cs").read_text(encoding="utf-8")
    required=[
        "Entry(1,baseGrit,quickClear)", "Entry(1,grit,focused)",
        "Entry(5,training)", "Entry(5,fortune)",
        "Entry(11,bleeding)", "Entry(11,clipping)",
        "StatType.Charisma", "ModifierDescriptor.Luck",
        "cls.Archetypes=cls.Archetypes.Concat(new[]{archetype}).ToArray()"]
    missing=[token for token in required if token not in source]
    if missing: raise AssertionError(f"Mysterious Stranger contract lacks {missing}")
    for token in ["FocusedAimDamage", "ClippingShotAttackHandler",
                  "PhysicalDamage", "Modifier = 0.5f", "DeadShotRuntime"]:
        if token not in mechanics: raise AssertionError(f"Mechanics lack {token}")
    ledger=json.loads((root/"blueprints/blueprints.json").read_text(encoding="utf-8"))
    entries=[e for e in ledger["entries"] if e["symbol"].startswith("KMG.Archetypes.")]
    if len(entries)!=17: raise AssertionError(f"Expected 17 archetype assets, found {len(entries)}")
    if len({e["guid"] for e in ledger["entries"]})!=len(ledger["entries"]):
        raise AssertionError("Blueprint GUIDs are not unique")

if __name__=="__main__":
    validate(Path(__file__).resolve().parents[1])
    print("Focused Mysterious Stranger validation passed.")
