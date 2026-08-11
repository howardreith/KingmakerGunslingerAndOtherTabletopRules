#!/usr/bin/env python3
"""Allocate once and validate the frozen Expanded Summoning foundation IDs."""

import argparse
import json
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
MANIFEST = ROOT / "blueprints/blueprints.json"
ENTRY = re.compile(r'C\("([^"]+)","([^"]+)",(null|\d+),(true|false),(null|\d+)')


def token(key):
    return "".join(part[:1].upper() + part[1:] for part in re.split(r"[^A-Za-z0-9]+", key) if part)


def planned():
    rows = []
    creatures = []
    for key, _name, monster, templated, ally in ENTRY.findall(CATALOG.read_text(encoding="utf-8")):
        creatures.append((key, None if monster == "null" else int(monster),
                          templated == "true", None if ally == "null" else int(ally)))
        rows.append((f"KMG.Summoning.Unit.{token(key)}", "BlueprintUnit"))
    if len(creatures) != 67:
        raise SystemExit(f"Expected 67 parsed creatures; observed {len(creatures)}")
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
    if len(rows) != 1120 or len({symbol for symbol, _ in rows}) != 1120:
        raise SystemExit(f"Foundation plan invariant failed: {len(rows)} rows")
    return rows


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


if __name__ == "__main__":
    main()
