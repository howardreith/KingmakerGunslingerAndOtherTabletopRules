#!/usr/bin/env python3
"""Generate the Expanded Summoning ideal-roster and charter-traceability docs.

The frozen C# manifest is the single source of truth. This tool reads it,
re-derives every headline figure the charter publishes, and regenerates the two
planning documents. It never edits the manifest, so the documents can always be
rebuilt from source and compared.

Run:  python tools/expanded_summoning_ideal_roster.py [--check]
"""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdealRosterCatalog.cs"
SHIPPED = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
ROSTER_DOC = ROOT / "planning/EXPANDED-SUMMONING-IDEAL-ROSTER.md"
TRACE_DOC = ROOT / "planning/EXPANDED-SUMMONING-CHARTER-TRACEABILITY.md"

ENTRY = re.compile(
    r'R\("([^"]+)","([^"]+)",(null|\d+),(null|\d+),(null|\d+),'
    r'IdealRosterPriority\.(\w+),IdealRosterEffort\.(\w+),'
    r'IdealRosterCoverage\.(\w+),(true|false),"([^"]*)"\)')
SHIPPED_ENTRY = re.compile(r'C\("([^"]+)",')

# Charter section 5.1. Phase, name and roster weight per sprint; the charter
# closes at Sprint 45 and forbids an automatic Sprint 46.
SPRINTS = [
    (0, 0, "Baseline Freeze and Program Harness", "N/A"),
    (1, 0, "Roster Manifest v2 and Menu Scalability Gate", "N/A"),
    (2, 0, "Skinned-Creature Asset Pipeline and Pteranodon Vertical Slice", "3"),
    (3, 1, "Native Publication Pack I - Early Creatures and Giant Reuse", "5"),
    (4, 1, "Native Publication Pack II - Plants and Colossal Reuse", "4"),
    (5, 1, "Mephit Family Expansion", "6"),
    (6, 1, "Existing Signature Mechanics Repair", "7"),
    (7, 1, "Big-Cat Combat System", "8"),
    (8, 1, "Big-Cat Roster Completion", "4"),
    (9, 2, "Flying Animal Rig - Eagle and Dire Bat", "5"),
    (10, 2, "Flying Vermin - Stirge and Giant Wasp", "6"),
    (11, 2, "Equines and Ungulates", "7"),
    (12, 2, "Canines and Small Quadrupeds I", "8"),
    (13, 2, "Canines and Small Quadrupeds II plus Tiny Frog", "6"),
    (14, 2, "Insect Rig I - Fire Beetle and Ground Ants", "8"),
    (15, 2, "Insect Rig II - Drone Ant and Giant Stag Beetle", "5"),
    (16, 2, "Crocodilian Rig", "5"),
    (17, 2, "Snake and Serpentine Rig", "9"),
    (18, 2, "Primate Rig", "5"),
    (19, 2, "Four-Arm Rig - Girallon and Xill", "8"),
    (20, 2, "Giant Scorpion", "4"),
    (21, 2, "Bebelith and Giant Crab", "6"),
    (22, 3, "Ankylosaurus Completion", "3"),
    (23, 3, "Armored Dinosaurs - Triceratops and Stegosaurus", "6"),
    (24, 3, "Theropod Dinosaurs - Deinonychus and Tyrannosaurus", "6"),
    (25, 3, "Giant Bodies - Brachiosaurus and Elephant", "5"),
    (26, 3, "Gorgon and Bulette", "6"),
    (27, 3, "Griffon", "4"),
    (28, 3, "Giant Humanoids I - Hill, Stone, and Fire Giants", "6"),
    (29, 3, "Giant Humanoids II - Cloud, Storm, and Ettin", "7"),
    (30, 4, "Low-Tier Outsiders - Lemure, Dretch, and Hound Archon", "7"),
    (31, 4, "Celestial Core - Lantern, Astral Deva, and Trumpet Archon", "7"),
    (32, 4, "Fey and Celestial Support - Satyr and Lillend Azata", "6"),
    (33, 4, "Infiltration and Shadow Outsiders", "7"),
    (34, 4, "Infernal Reach I - Bearded Devil and Kyton", "6"),
    (35, 4, "Infernal Reach II - Barbed Devil and Bone Devil", "7"),
    (36, 4, "Demonic Skirmishers and Brutes - Babau and Hezrou", "6"),
    (37, 4, "Vrock", "4"),
    (38, 4, "Glabrezu", "4"),
    (39, 4, "Nalfeshnee", "4"),
    (40, 4, "Ice Devil", "4"),
    (41, 4, "Core Ideal Roster Release Audit", "N/A"),
    (42, 5, "Variant Elementals I - Ice and Lightning", "4 families / 8 pts"),
    (43, 5, "Variant Elementals II - Magma and Mud", "2 families / 4 pts"),
    (44, 5, "Aether Elemental Family", "1 family / 3 pts"),
    (45, 5, "Final Program Audit and Charter Closure", "N/A"),
]

PHASES = {
    0: "Foundation and Feasibility Gates",
    1: "Current Roster Completion and Low-Risk Expansion",
    2: "Natural Creature Rig Library",
    3: "Dinosaurs, Magical Beasts, and Giants",
    4: "Outsider Expansion and Core Release",
    5: "Variant Elementals and Program Closure",
}

# Sprints 0, 1, 41 and 45 are program infrastructure; 42-44 carry the stretch
# elemental families, which are counted as families rather than creature rows.
AUDIT_SPRINTS = {0, 1, 41, 45}
ELEMENTAL_SPRINTS = {42, 43, 44}


class Entry:
    __slots__ = ("key", "name", "sm", "sna", "sprint", "priority", "effort",
                 "coverage", "baseline", "package")

    def __init__(self, m):
        def num(v):
            return None if v == "null" else int(v)
        (self.key, self.name, sm, sna, sprint, self.priority, self.effort,
         self.coverage, baseline, self.package) = m.groups()
        self.sm, self.sna, self.sprint = num(sm), num(sna), num(sprint)
        self.baseline = baseline == "true"

    def tier(self, family):
        return self.sm if family == "sm" else self.sna


def load():
    entries = [Entry(m) for m in ENTRY.finditer(MANIFEST.read_text(encoding="utf-8"))]
    if not entries:
        raise SystemExit("No ideal roster entries parsed; the manifest shape changed.")
    return entries


def placements(entries, family):
    """Per-parent single / 1d3 / 1d4+1 projection using charter semantics."""
    tiers = [e.tier(family) for e in entries if e.tier(family)]
    rows = []
    for parent in range(1, 10):
        one = sum(1 for t in tiers if t == parent)
        d3 = sum(1 for t in tiers if t == parent - 1)
        d41 = sum(1 for t in tiers if t < parent - 1)
        rows.append((parent, one, d3, d41, one + d3 + d41))
    return rows


def derive(entries):
    sm_rows, sna_rows = placements(entries, "sm"), placements(entries, "sna")
    return {
        "unique": len(entries),
        "sm_entries": sum(1 for e in entries if e.sm),
        "sna_entries": sum(1 for e in entries if e.sna),
        "sm_rows": sm_rows,
        "sna_rows": sna_rows,
        "sm_total": sum(r[4] for r in sm_rows),
        "sna_total": sum(r[4] for r in sna_rows),
        "baseline": sum(1 for e in entries if e.baseline),
        "outstanding": sum(1 for e in entries if not e.baseline),
    }


EXPECTED = {"unique": 145, "sm_entries": 120, "sna_entries": 110,
            "baseline": 50, "outstanding": 95}


def check(d):
    problems = [f"{k}: charter {v}, derived {d[k]}"
                for k, v in EXPECTED.items() if d[k] != v]
    if d["sm_total"] + d["sna_total"] != 1232:
        problems.append(
            f"total placements: charter 1232, derived {d['sm_total'] + d['sna_total']}")
    return problems


def family_table(rows, label):
    out = [f"### {label}", "",
           "| Parent | Single | 1d3 | 1d4+1 | Choices under this spell |",
           "|---|---|---|---|---|"]
    for parent, one, d3, d41, total in rows:
        out.append(f"| {label.split()[0][0]}{'M' if 'Monster' in label else 'NA'} "
                   f"{parent} | {one} | {d3} | {d41} | **{total}** |")
    out.append(f"| **Total** | | | | **{sum(r[4] for r in rows)}** |")
    out.append("")
    return out


def roster_doc(entries, d, shipped):
    lines = [
        "# Expanded Summoning ideal roster manifest",
        "",
        "Generated by `tools/expanded_summoning_ideal_roster.py` from the frozen",
        "`ExpandedSummoningIdealRosterCatalog`; do not edit by hand.",
        "",
        "This is a **planning manifest, not a publication**. No row here registers,",
        "reserves, or publishes a blueprint, and no publication path may reference",
        "it — `expanded-summoning.ideal-roster-isolation` enforces that in the",
        "domain suite. Live creatures remain owned by `ExpandedSummoningCatalog`.",
        "",
        "## Charter reconciliation",
        "",
        "| Figure | Charter | Derived here |",
        "|---|---|---|",
        f"| Unique creatures | 145 | {d['unique']} |",
        f"| Already complete (Appendix B) | 50 | {d['baseline']} |",
        f"| Requiring work (Appendix A) | 95 | {d['outstanding']} |",
        f"| Summon Monster base entries | 120 | {d['sm_entries']} |",
        f"| Summon Nature's Ally base entries | 110 | {d['sna_entries']} |",
        f"| Core visible quantity placements | 1,232 | "
        f"{d['sm_total'] + d['sna_total']:,} |",
        "",
        "Derived independently from the manifest; the delta against every charter",
        "figure is zero.",
        "",
        "## Projected menu scale",
        "",
        "The 1,232 figure is an aggregate across eighteen parent spells, never one",
        "menu. The scalability gate is the largest single parent spell.",
        "",
    ]
    lines += family_table(d["sm_rows"], "Summon Monster")
    lines += family_table(d["sna_rows"], "Summon Nature's Ally")

    lines += [
        "## Coverage ledger",
        "",
        "| State | Creatures | Meaning |",
        "|---|---|---|",
    ]
    order = ["Planned", "IdentityReserved", "Registered", "Published",
             "TechnicallyVerified", "OwnerAccepted"]
    meaning = {
        "Planned": "In the roster; no identity exists yet",
        "IdentityReserved": "GUID reserved append-only; nothing registered",
        "Registered": "Registered for save safety; withheld from menus",
        "Published": "Visible in at least one legal placement",
        "TechnicallyVerified": "Meets the charter Definition of Done under test",
        "OwnerAccepted": "Accepted by the owner in play; never self-declared",
    }
    for state in order:
        n = sum(1 for e in entries if e.coverage == state)
        lines.append(f"| {state} | {n} | {meaning[state]} |")
    lines += [
        "",
        f"Identity reuse: {len(shipped)} shipped creatures are reused in place "
        f"(charter decision D-01, one creature one identity); "
        f"{d['unique'] - len(shipped)} new identities remain to be allocated.",
        "",
        "## Creatures",
        "",
        "| Creature | SM | SNA | Sprint | Priority | Effort | Coverage | Asset package |",
        "|---|---|---|---|---|---|---|---|",
    ]
    for e in sorted(entries, key=lambda x: x.key):
        lines.append(
            f"| {e.name} | {e.sm or '-'} | {e.sna or '-'} | "
            f"{('S%d' % e.sprint) if e.sprint is not None else 'complete'} | "
            f"{e.priority} | {e.effort if e.effort != 'None' else '-'} | "
            f"{e.coverage} | {e.package} |")
    lines.append("")
    return "\n".join(lines)


def trace_doc(entries):
    by_sprint = {}
    for e in entries:
        if e.sprint is not None:
            by_sprint.setdefault(e.sprint, []).append(e)

    lines = [
        "# Expanded Summoning charter traceability ledger",
        "",
        "Generated by `tools/expanded_summoning_ideal_roster.py`; do not edit by hand.",
        "",
        "Every sprint in the charter appears exactly once, with the creatures it",
        "owns. Sprint status is recorded separately from owner acceptance: a sprint",
        "is not accepted because its tests pass. The charter closes at Sprint 45 and",
        "forbids an automatic Sprint 46.",
        "",
        "| Sprint | Phase | Name | Roster weight | Creatures | Status |",
        "|---|---|---|---|---|---|",
    ]
    done = {0: "Sprint 0 complete; owner acceptance pending",
            1: "Sprint 1 complete; owner acceptance pending",
            2: "Sprint 2 in progress"}
    for number, phase, name, weight in SPRINTS:
        owned = by_sprint.get(number, [])
        if number in AUDIT_SPRINTS:
            creatures = "_program infrastructure / audit_"
        elif number in ELEMENTAL_SPRINTS:
            creatures = ", ".join(e.name for e in sorted(owned, key=lambda x: x.key)) \
                or "_variant elemental families_"
        else:
            creatures = ", ".join(e.name for e in sorted(owned, key=lambda x: x.key))
        status = done.get(number, "Not started")
        lines.append(f"| S{number} | {phase} | {name} | {weight} | "
                     f"{creatures} | {status} |")

    lines += ["", "## Phases", "",
              "| Phase | Name |", "|---|---|"]
    for number, name in PHASES.items():
        lines.append(f"| Phase {number} | {name} |")
    lines += [
        "",
        "Sprint 41 is the release-quality completion boundary for the 145-creature",
        "core roster. Sprints 42-45 are a deliberate stretch extension and may be",
        "postponed without invalidating the core release.",
        "",
    ]
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true",
                        help="verify the documents are current without writing")
    args = parser.parse_args()

    entries = load()
    d = derive(entries)
    problems = check(d)
    if problems:
        print("Ideal roster reconciliation FAILED:", *problems, sep="\n  ")
        return 1

    shipped = set(SHIPPED_ENTRY.findall(SHIPPED.read_text(encoding="utf-8")))
    missing = shipped - {e.key for e in entries}
    if missing:
        print("Shipped creatures absent from the ideal roster:", sorted(missing))
        return 1

    roster, trace = roster_doc(entries, d, shipped), trace_doc(entries)
    if args.check:
        stale = [p.name for p, text in ((ROSTER_DOC, roster), (TRACE_DOC, trace))
                 if not p.exists() or p.read_text(encoding="utf-8") != text]
        if stale:
            print("Generated documents are stale:", ", ".join(stale))
            return 1
        print("Ideal roster documents are current.")
        return 0

    ROSTER_DOC.write_text(roster, encoding="utf-8")
    TRACE_DOC.write_text(trace, encoding="utf-8")
    print(f"Ideal roster PASS: {d['unique']} creatures; "
          f"SM {d['sm_entries']}/{d['sm_total']}; "
          f"SNA {d['sna_entries']}/{d['sna_total']}; "
          f"total {d['sm_total'] + d['sna_total']} placements; "
          f"{len(shipped)} identities reused.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
