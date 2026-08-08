#!/usr/bin/env python3
import json, hashlib, struct
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
    entries=[e for e in ledger["entries"] if e["symbol"].startswith("KMG.Archetypes.")
             and e["symbol"] not in {"KMG.Archetypes.PistolTraining",
                                      "KMG.Archetypes.MusketTraining",
                                      "KMG.Archetypes.PistoleroProficiencies",
                                      "KMG.Archetypes.MusketMasterProficiencies",
                                      "KMG.Archetypes.Pistolero",
                                      "KMG.Archetypes.UpCloseAndDeadly",
                                      "KMG.Archetypes.TwinShotKnockdown",
                                      "KMG.Archetypes.PistoleroDeedsLevel1",
                                      "KMG.Archetypes.PistoleroDeedsLevel7",
                                      "KMG.Archetypes.PistoleroDeedsLevel11",
                                      "KMG.Archetypes.MusketMaster",
                                      "KMG.Archetypes.SteadyAim",
                                      "KMG.Archetypes.SteadyAimAbility",
                                      "KMG.Archetypes.SteadyAimArmed",
                                      "KMG.Archetypes.FastMusket"}]
    if len(entries)!=20: raise AssertionError(f"Expected 20 archetype assets, found {len(entries)}")
    if len({e["guid"] for e in ledger["entries"]})!=len(ledger["entries"]):
        raise AssertionError("Blueprint GUIDs are not unique")
    if "FirearmDefinitionComponent>().Count() != 1" not in mechanics:
        raise AssertionError("Focused Aim does not use exact weapon-type firearm identity")
    for token in ["LocalizedDuration", '"Until the end of your turn"',
                  "LocalizedSavingThrow", '"None"', "not attack rolls"]:
        if token not in source: raise AssertionError(f"Focused Aim presentation lacks {token}")
    icon=root/"assets/game/icons/focused-aim.png"
    data=icon.read_bytes()
    if data[:8]!=b"\x89PNG\r\n\x1a\n" or struct.unpack(">II",data[16:24])!=(128,128):
        raise AssertionError("Focused Aim icon is not a valid 128x128 PNG")
    if hashlib.sha256(data).hexdigest()!="ba962ad9dbd58f52fad6097dd973508f98ac000db8629f698126c7c5026ec7a8":
        raise AssertionError("Focused Aim icon export changed")
    icon_code=(root/"src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs").read_text(encoding="utf-8")
    if icon_code.count('Require("focused-aim")') != 3:
        raise AssertionError("Focused Aim feature, ability, and buff icon mapping changed")

if __name__=="__main__":
    validate(Path(__file__).resolve().parents[1])
    print("Focused Mysterious Stranger validation passed.")
