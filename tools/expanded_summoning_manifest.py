#!/usr/bin/env python3
"""Allocate once and validate the frozen Expanded Summoning foundation IDs."""

import argparse
import json
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
DONORS = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs"
MANIFEST = ROOT / "blueprints/blueprints.json"
ROSTER = ROOT / "planning/EXPANDED-SUMMONING-ROSTER.md"
ENTRY = re.compile(
    r'C\("([^"]+)","([^"]+)",(null|\d+),(true|false),(null|\d+)'
    r'(?:,"([^"]+)")?\)')
DONOR = re.compile(r'"([a-z0-9-]+)\|([0-9a-f]{32})\|([01])"')

DONOR_NAMES = {
    "028cc6f46e7998f46855a33ffde89567": "MastodonSummon",
    "03dd28e92faf2e44eb9564a6ba01fdd0": "DireWolfSummon",
    "04944455200bc224d955a8e9bbd64f3f": "SummonedAirElementalSmall",
    "0b214d8e81a563549ba0be37cd1c16d0": "CR4_BearStandard",
    "0cc7a2526e4557945b1d8eb277d1fb3a": "CR7_Nymph",
    "10a820de0a417f345866f794324205ad": "MephitFireSummoned",
    "1832be68f9814254dbbdab6df7fd5d0b": "SoulEaterSummoned",
    "1ed9a630f0d9d7f44855d3d1d1b2cdf2": "GiantFrogSummoned",
    "24719a49b84c5cd43b894268d22d9c89": "CR6_WillOWispStandart",
    "260da5b557e3fb04bb4960a36a5d1dc4": "CR7_BearDire",
    "2e24256e459468743b91fbb9aa85e1ab": "SummonedAirElementalHuge",
    "2f65fd8032e5182418ee83dd4f7858dd": "CR0.5_GiantCentipedeStandard",
    "30080a8d8ae40bb43aca496b11b74c6b": "CR4_GiantFrogPoisonous",
    "313a17cbd273d1f40bd1654ee2ae186e": "CR2_WorgStandart",
    "33bb90ffd13c87b4c8e45d920313752a": "SummonedAirElementalElder",
    "3764b43791a00e1468257adbca43ce9b": "SummonedAirElementalLarge",
    "394610e32cfbc4f43a0efaab16faae49": "CR1_Nixie",
    "3b86a449e7264174eaccef9b8f02fe20": "SummonedEarthElementalHuge",
    "3bd31a0b4d800f04a8c5b7b1a6d7061e": "SummonedWaterElementalElder",
    "406c1e1af5400ac4881e330502ccbd9e": "CR3_GiantEagleStandard",
    "4109b40f6bbb49640840644cc84ada67": "MonitorLizardSummoned",
    "4615328295cd7e84bb2ef09d3dba8403": "MephitWaterSummoned",
    "46779f56cab2cb0438161fec0129790d": "MephitEarthSummoned",
    "46cede83b1f34ad4fa46b8776e352b02": "SummonedFireElementalSmall",
    "50782bc4eb36aac4287023e20ee00808": "MephitAirSummoned",
    "51c66b0783a748c4b9538f0f0678c4d7": "CR7_GiantSpiderDoombringing",
    "56372b0a2749c224392a5ee74105c534": "SummonedWaterElementalSmall",
    "58574e8d1d4dc464c976f396d9115b1a": "AzataBralaniSummoned",
    "5f968d63d756f994ebff0d774e88e4ab": "CR2_BoarStandard",
    "62a3e860e6e72e6499c38bb8b2fe303e": "SummonedWaterElementalMedium",
    "640fb7efb7c916945837bbcab995267e": "SummonedFireElementalHuge",
    "651600a51edd20141adb67696986c582": "SummonedEarthElementalSmall",
    "676f8b7d0a170674cb6e504e0e30b4f0": "SummonedAirElementalMedium",
    "680b5b61c80af664daec46af7644486c": "SummonedWaterElementalLarge",
    "6b4cb9b6116f2194192e1e7e379c48d7": "SummonedEarthElementalElder",
    "6ea3a75279bab234aa723989e30cb15a": "CR8_ErinyesDevilStandard",
    "6ec9c63c41a1e754ea4dcd85557625b4": "DireBoarSummoned",
    "76597216769b0d540aafafa07edf0cec": "WolfSummon",
    "768275c9885dd954fb3c84ba69ac4281": "LeopardSummoned",
    "812c9a0348e004242ba4e46efa91e38e": "SummonedEarthElementalMedium",
    "877c154a296ee8e45be1a00668319923": "SummonedWaterElementalHuge",
    "9e120b5e0ad3c794491c049aa24b9fde": "GiantSpiderSummoned",
    "a0ab0c31b1a92554291a82e598f39ba4": "SummonedFireElementalMedium",
    "b0b4091bdaebb464e903857a95189dea": "SummonedFireElementalGreater",
    "ba5026596b06b204eb2efed2b411c5b9": "SummonedFireElementalLarge",
    "bc8ca1437c0f48948b317b7e64febf0d": "AzataGhaelSummoned",
    "beae4985629a6f64eb98081e3171e4c1": "SmilodonSummoned",
    "c3524f96954a1d94f8525b86e7626633": "CR6_HodagStandard",
    "cda7013db24f4c547b79bfc5c617066b": "SummonedEarthElementalGreater",
    "d3d9ab560534bd948b10ac00abbff083": "SummonedEarthElementalLarge",
    "e770cfbb96b528c4db258d7d03fe6533": "SummonedAirElementalGreater",
    "e8276e28b2234a745900fed80670bfdb": "CR1_LizardfolkStandard",
    "ea0f0bbc6e5e471428d535501b21eb26": "SummonedFireElementalElder",
    "ece348345859351439e1263115f5fdb9": "HellhoundSummoned",
    "fcc939e3acf355b458ddf9617d8c6c28": "SummonedWaterElementalGreater",
}

SPECIAL_NOTES = {
    "lantern-archon": "Dual bounded light rays and archon defenses; optional exact Aura of Menace carrier; planar travel omitted.",
    "salamander": "Spear/tail, heat, and bounded grab/constrict; cold vulnerability omitted.",
    "invisible-stalker": "Attack-safe natural invisibility and twin slams; tracking/scent omitted.",
    "shadow-demon": "Incorporeal/shadow offense; possession and teleportation omitted.",
    "succubus": "Bounded domination and one-round temporary energy drain; profane gift omitted.",
    "bebelith": "Demon hunting and DC 25 bounded one-round armor dismantle; rot and permanent item damage omitted.",
    "pixie": "Sixteen no-damage sleep arrows and one bounded irresistible dance; no ammunition or loot.",
}


def token(key):
    return "".join(part[:1].upper() + part[1:] for part in re.split(r"[^A-Za-z0-9]+", key) if part)


def parsed_creatures():
    values = []
    for key, name, monster, templated, ally, visual in ENTRY.findall(
            CATALOG.read_text(encoding="utf-8")):
        values.append({
            "key": key,
            "name": name,
            "monster": None if monster == "null" else int(monster),
            "templated": templated == "true",
            "ally": None if ally == "null" else int(ally),
            "visual": visual or name,
        })
    if len(values) != 67:
        raise SystemExit(f"Expected 67 parsed creatures; observed {len(values)}")
    return values


def planned():
    rows = []
    creatures = []
    for value in parsed_creatures():
        creatures.append((value["key"], value["monster"], value["templated"],
                          value["ally"]))
        rows.append((f"KMG.Summoning.Unit.{token(value['key'])}", "BlueprintUnit"))
    rows.extend((
        ("KMG.Summoning.Native.SM.Tier1", "BlueprintAbility"),
        ("KMG.Summoning.Native.SNA.Tier1", "BlueprintAbility"),
    ))
    for family, index in (("SM", 1), ("SNA", 3)):
        for parent in range(1, 10):
            for creature in creatures:
                source = creature[index]
                if source is None or source > parent:
                    continue
                count = "One" if source == parent else "OneD3" if source == parent - 1 else "OneD4PlusOne"
                symbol = f"KMG.Summoning.Ability.{family}.Tier{parent}.{token(creature[0])}.{count}"
                rows.append((symbol, "BlueprintAbility"))
                if family == "SM" and creature[2]:
                    rows.append((symbol + ".Celestial", "BlueprintAbility"))
                    rows.append((symbol + ".Fiendish", "BlueprintAbility"))
    for alignment in ("Celestial", "Fiendish"):
        for band in ("Low", "Mid", "High"):
            rows.append((f"KMG.Summoning.Template.{alignment}.{band}", "BlueprintBuff"))
    for alignment in ("Celestial", "Fiendish"):
        rows.append((f"KMG.Summoning.Smite.{alignment}.Available", "BlueprintBuff"))
    rows.extend((
        ("KMG.Summoning.Special.LanternArchon.LightRay", "BlueprintAbility"),
        ("KMG.Summoning.Special.LanternArchon.LightRayAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.LanternArchon.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.LanternArchon.Defenses", "BlueprintBuff"),
        ("KMG.Summoning.Special.ShadowDemon.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Salamander.Tail", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Salamander.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Succubus.Dominate", "BlueprintAbility"),
        ("KMG.Summoning.Special.Succubus.Domination", "BlueprintBuff"),
        ("KMG.Summoning.Special.Succubus.DominateAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Succubus.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Succubus.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Bebelith.Claw", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Bebelith.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Bebelith.DismantledArmor", "BlueprintBuff"),
        ("KMG.Summoning.Special.Pixie.SleepBow", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDance", "BlueprintAbility"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDanceState", "BlueprintBuff"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDanceResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.Pixie.SleepArrowResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.Pixie.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDanceAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Pixie.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Natural.Bite1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bite1d3", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tail1d12", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tail3d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bite2d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Talon2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Subtype.Extraplanar", "BlueprintFeature"),
    ))
    if len(rows) != 1152 or len({symbol for symbol, _ in rows}) != 1152:
        raise SystemExit(f"Foundation plan invariant failed: {len(rows)} rows")
    return rows


def generated_roster(manifest):
    by_symbol = {entry["symbol"]: entry for entry in manifest["entries"]}
    donors = {
        key: {"guid": guid, "dedicated": dedicated == "1"}
        for key, guid, dedicated in DONOR.findall(DONORS.read_text(encoding="utf-8"))
    }
    creatures = parsed_creatures()
    if len(donors) != 67 or set(donors) != {value["key"] for value in creatures}:
        raise SystemExit("Roster generation requires one exact donor per creature")
    lines = [
        "# Expanded Summoning roster and identity ledger",
        "",
        "Generated deterministically by `tools/expanded_summoning_manifest.py`; do not edit by hand.",
        "",
        "Frozen totals: 66 Summon Monster entries / 361 placements; 57 Summon Nature's Ally entries / 320 placements; 67 unique units; 681 logical placements.",
        "",
        "Final qualification source: `47c6a4ab04540276f97830a6f106b419cbcf1eff`. Structural run `20260812T0747050609622Z`; native cast run `20260812T0755367433231Z`; visual run `20260812T0855013819445Z`; enabled/disabled persistence runs `20260812T0900084936466Z` through `20260812T0913417070226Z`; eight required compatibility transactions PASS.",
        "",
        "Every placement has a distinct frozen ability identity so parent spell level, caster context, metamagic, duration, UI, and save identity remain local to that parent. Templated Summon Monster placements additionally own celestial and fiendish execution identities. No runtime GUID generation or shared cross-parent ability is used.",
        "",
    ]
    for family, tier_field, short in (("Summon Monster", "monster", "SM"),
                                      ("Summon Nature's Ally", "ally", "SNA")):
        lines.extend((f"## {family}", "",
            "| Tier / creature | Template or alignment | Donor / view | KMG unit | Frozen placement abilities | Adaptation and sanitization | Qualification |",
            "|---|---|---|---|---|---|---|"))
        for creature in sorted((value for value in creatures if value[tier_field] is not None),
                               key=lambda value: (value[tier_field], value["name"])):
            source = creature[tier_field]
            donor = donors[creature["key"]]
            donor_name = DONOR_NAMES.get(donor["guid"])
            if donor_name is None:
                raise SystemExit(f"Missing runtime-proven donor name: {donor['guid']}")
            unit_symbol = f"KMG.Summoning.Unit.{token(creature['key'])}"
            unit_guid = by_symbol[unit_symbol]["guid"]
            if short == "SM" and creature["templated"]:
                policy = "Celestial/fiendish; good/evil restricted, neutral chooses"
            elif short == "SNA":
                policy = "Caster alignment; never celestial/fiendish"
            else:
                policy = "Native alignment/subtypes; no template"
            abilities = []
            for parent in range(source, 10):
                multiplicity = "One" if parent == source else (
                    "OneD3" if parent == source + 1 else "OneD4PlusOne")
                symbol = (f"KMG.Summoning.Ability.{short}.Tier{parent}."
                          f"{token(creature['key'])}.{multiplicity}")
                entry = by_symbol[symbol]
                text = f"T{parent} {multiplicity} `{entry['guid']}`"
                if short == "SM" and creature["templated"]:
                    celestial = by_symbol[symbol + ".Celestial"]["guid"]
                    fiendish = by_symbol[symbol + ".Fiendish"]["guid"]
                    text += f" (C `{celestial}`; F `{fiendish}`)"
                abilities.append(text)
            donor_kind = "dedicated summon" if donor["dedicated"] else "visual/body donor"
            donor_text = (f"`{donor['guid']}` {donor_name}; {donor_kind}; "
                          f"view policy: {creature['visual']}")
            removed = ("XP, loot, inventory, campaign, persistence, teleport, planar travel, "
                       "and summon/conjure surfaces removed; ")
            adaptation = SPECIAL_NOTES.get(creature["key"],
                "Dedicated mechanics reused only where exact; otherwise donor is view/rig only and the checked-in tabletop profile owns stats, attacks, facts, and deviations.")
            lines.append(
                f"| {source} / {creature['name']} | {policy} | {donor_text} | "
                f"`{unit_guid}` | {';<br>'.join(abilities)} | {removed}{adaptation} "
                f"See `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md`. | "
                "Structural PASS; native cast PASS; visual contract PASS; required profiles PASS |")
        lines.append("")
    lines.extend((
        "## Explicit exclusions",
        "",
        "No aquatic-only entries, horses or ponies, unapproved ants, apes, rhinoceroses, giants, extra dinosaurs, campaign spawns, companions, pets, vendors, loot, or external assets are added. Existing vanilla and third-party entries are preserved by reference and order.",
        "",
    ))
    return "\n".join(lines)


def validate(manifest, plan):
    entries = manifest["entries"]
    by_symbol = {entry["symbol"]: entry for entry in entries}
    if len(by_symbol) != len(entries):
        raise SystemExit("Manifest contains duplicate symbols")
    guids = [entry["guid"] for entry in entries]
    if len(set(guids)) != len(guids) or any(not re.fullmatch(r"[0-9a-f]{32}", value) for value in guids):
        raise SystemExit("Manifest GUID format/collision check failed")
    for symbol, planned_type in plan:
        entry = by_symbol.get(symbol)
        if entry is None:
            raise SystemExit(f"Missing planned identity: {symbol}")
        if entry["plannedType"] != planned_type or entry["status"] not in ("reserved", "active"):
            raise SystemExit(f"Wrong type/status for {symbol}")
    active = sum(entry["status"] == "active" for entry in entries)
    reserved = sum(entry["status"] == "reserved" for entry in entries)
    print(f"Expanded Summoning manifest PASS: foundation={len(plan)} active={active} reserved={reserved} total={len(entries)}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--allocate", action="store_true")
    parser.add_argument("--activate", action="store_true")
    parser.add_argument("--emit-roster", action="store_true")
    args = parser.parse_args()
    plan = planned()
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    existing = {entry["symbol"]: entry for entry in manifest["entries"]}
    if args.allocate:
        used = {entry["guid"] for entry in manifest["entries"]}
        for symbol, planned_type in plan:
            if symbol in existing:
                continue
            guid = uuid.uuid4().hex
            while guid in used:
                guid = uuid.uuid4().hex
            used.add(guid)
            manifest["entries"].append({
                "symbol": symbol,
                "guid": guid,
                "plannedType": planned_type,
                "status": "reserved",
                "milestone": "Expanded Summoning",
                "notes": "Frozen foundation identity; activate only with exact deterministic runtime registration."
            })
        MANIFEST.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    if args.activate:
        planned_symbols = {symbol for symbol, _ in plan}
        for entry in manifest["entries"]:
            if entry["symbol"] in planned_symbols:
                entry["status"] = "active"
                entry["notes"] = "Registered in every feature-module state; live parent publication remains independently gated."
        MANIFEST.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    validate(manifest, plan)
    roster = generated_roster(manifest)
    if args.emit_roster:
        ROSTER.write_text(roster, encoding="utf-8")
    elif not ROSTER.is_file() or ROSTER.read_text(encoding="utf-8") != roster:
        raise SystemExit(
            "Expanded Summoning roster ledger is stale; run with --emit-roster")


if __name__ == "__main__":
    main()
