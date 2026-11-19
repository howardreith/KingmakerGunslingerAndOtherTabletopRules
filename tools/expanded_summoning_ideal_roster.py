#!/usr/bin/env python3
"""Generate the Expanded Summoning ideal-roster and charter-traceability docs.

The frozen C# manifest is the single source of truth for the ideal roster. This
tool reads it, re-derives every headline figure the charter publishes, and
regenerates the two planning documents. It never edits a source file, so the
documents can always be rebuilt from source and compared.

Coverage is reported per placement: one creature, one spell family, one parent
spell, one quantity. The ideal placements come from the ideal roster. The
declared choices are derived independently, from the project catalog under
GenerateVariants semantics, the retained native wrappers and the visibility
catalog, and each is checked against the identity manifest. The two are then
compared exactly; a creature's identity never stands in for a placement.

Generation and freshness are separate from coverage: `--check` passes when the
documents are current, however incomplete the coverage they report.

An ideal placement the owner has approved out of scope (APPROVED_OUT_OF_SCOPE)
is reported as out of scope, never as covered and never as missing. `--cells`
prints one decision row per ideal placement of the named creatures, with five
independent facts: ideal scope, the exact family-specific ledger identity,
the declared choice, its admission, and native evidence for that exact identity
from planning/expanded-summoning-native-placement-evidence.json.

Run:  python tools/expanded_summoning_ideal_roster.py [--check] [--cells key,key]
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter, defaultdict, namedtuple
from pathlib import Path

sys.dont_write_bytecode = True
from expanded_summoning_manifest import ENTRY as CREATURE  # noqa: E402
from expanded_summoning_manifest import OWNER_WITHDRAWAL_NOTES, SUPPRESSED, token  # noqa: E402

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdealRosterCatalog.cs"
SHIPPED = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
WRAPPERS = ROOT / "src/KingmakerGunslinger/Summoning/SummonNativeExpansionCatalog.cs"
VISIBILITY = ROOT / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"
IDENTITIES = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs"
LEDGER = ROOT / "blueprints/blueprints.json"
EVIDENCE = ROOT / "planning/expanded-summoning-native-placement-evidence.json"
ROSTER_DOC = ROOT / "planning/EXPANDED-SUMMONING-IDEAL-ROSTER.md"
TRACE_DOC = ROOT / "planning/EXPANDED-SUMMONING-CHARTER-TRACEABILITY.md"

ENTRY = re.compile(
    r'R\("([^"]+)","([^"]+)",(null|\d+),(null|\d+),(null|\d+),'
    r'IdealRosterPriority\.(\w+),IdealRosterEffort\.(\w+),'
    r'(true|false),"([^"]*)"\)')

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
                 "baseline", "package")

    def __init__(self, m):
        def num(v):
            return None if v == "null" else int(v)
        (self.key, self.name, sm, sna, sprint, self.priority, self.effort,
         baseline, self.package) = m.groups()
        self.sm, self.sna, self.sprint = num(sm), num(sna), num(sprint)
        self.baseline = baseline == "true"

    def tier(self, family):
        return self.sm if family == "sm" else self.sna


def load():
    return read_ideal(MANIFEST.read_text(encoding="utf-8"))


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


def roster_doc(entries, d, cov):
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
        f"| Ideal quantity placements | 1,232 | "
        f"{d['sm_total'] + d['sna_total']:,} |",
        "",
        "Derived independently from the manifest; the delta against every charter",
        "figure is zero. This table checks the ideal manifest against the charter",
        "only. What the source actually offers is reported under \"Coverage of",
        "ideal placements\" below.",
        "",
        "## Projected menu scale",
        "",
        "The 1,232 figure is an aggregate of ideal placements across eighteen parent",
        "spells, never one menu. The scalability gate is the largest single parent spell.",
        "",
    ]
    lines += family_table(d["sm_rows"], "Summon Monster")
    lines += family_table(d["sna_rows"], "Summon Nature's Ally")
    lines += coverage_section(entries, cov)
    lines += [
        "Frost Giant has one retained native unit identity and published "
        "Summon Monster and Nature's Ally wrappers in both families.",
        "",
        f"Identity reuse: {len(cov['project_keys']) + len(cov['wrapper_only'])} existing creature "
        f"identities are reused in place (charter decision D-01, one creature "
        f"one identity); {cov['unrepresented']} new identities remain to be allocated.",
        "",
        "## Creatures",
        "",
        "Placement columns count published ideal placements of that creature in that",
        "family: Complete; Partial (some published); Withheld (all registered, none",
        "admitted); Missing (none declared); Unpublished (none published, some withheld and",
        "some missing). The parenthesis gives the unpublished placements by state.",
        "",
        "| Creature | SM | SNA | Sprint | Priority | Effort | Unit identity | SM placements | SNA placements | Asset package |",
        "|---|---|---|---|---|---|---|---|---|---|",
    ]
    for e in sorted(entries, key=lambda x: x.key):
        lines.append(
            f"| {e.name} | {e.sm or '-'} | {e.sna or '-'} | "
            f"{('S%d' % e.sprint) if e.sprint is not None else 'complete'} | "
            f"{e.priority} | {e.effort if e.effort != 'None' else '-'} | "
            f"{cov['identity'][e.key]} | {cov['family_status'][e.key]['sm']} | "
            f"{cov['family_status'][e.key]['sna']} | {e.package} |")
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
            2: "Sprint 2 in progress",
            9: "Internal technical PASS: original models, Bat 40-foot imprecise blindsense and preserved publication, native combat/contact, obstructed doorway travel, lifecycle, persistence and controls; owner visual review pending",
            10: "Internal technical PASS: both species published with original icons; native combat/contact, signature mechanics, cleanup, player path and live menu qualified. Owner visual review pending; native Filth Fever cure timing unmeasured.",
            11: "Internally complete and published; Phase 2 evidence index owns exact qualification; owner visual review pending",
            12: "Internally complete and published; Phase 2 evidence index owns exact qualification; owner visual review pending",
            13: "Internally complete and published; Phase 2 evidence index owns exact qualification; owner visual review pending",
            14: "Internally complete and published under PASSIVE_CREATURE_SENSES_UNMODELED; owner visual review pending",
            15: "Internally complete and published; owner visual review pending",
            16: "Internally complete and published: exact e3aeae63 hidden and 155ada89 publication PASS, all twenty public roots; accepted engine limitations/adaptation retained; owner visual review pending",
            17: "In progress; exact f507 native-rig research PASS 8/8 with restoration, not gameplay/visual qualification; original snake and separate Salamander weapon prototypes next",
            27: "Released v0.0.161; functional native qualification and exact restoration PASS; owner visual review not performed (nonblocking)"}
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


class ReadError(Exception):
    """An input could not be read completely. Nothing is reported from it."""


Placement = namedtuple("Placement", "key family parent quantity")
Choice = namedtuple("Choice", "placement source symbol admitted")
# One ideal placement that is not published. `state` is what the source does
# with it (withheld or missing); `recorded` is whether a decision on record
# explains it. The two are independent: either state may be explained or not.
Unpublished = namedtuple("Unpublished", "placement state disposition symbols recorded")

QUANTITIES = ("One", "OneD3", "OneD4PlusOne")
FAMILIES = ("sm", "sna")
FAMILY_SYMBOL = {"sm": "SM", "sna": "SNA"}
FAMILY_NAME = {"sm": "Summon Monster", "sna": "Summon Nature's Ally"}
ROMAN = (None, "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX")
QUANTITY_LABEL = {"One": "single", "OneD3": "1d3", "OneD4PlusOne": "1d4+1"}

# A wrapper factory call starts an indented line; its family is its factory.
# N(tier, key, name, multiplicity, ...) is Summon Monster;
# A(tier, key, iconKey, name, multiplicity, ...) is Summon Nature's Ally.
WRAPPER_CALL = re.compile(r'^[ \t]*([NA])\(', re.M)
WRAPPER_N = re.compile(
    r'N\(\s*(\d+)\s*,\s*"([A-Za-z]+)"\s*,\s*"[^"]*"\s*,\s*SummonMultiplicity\.(\w+)\s*,')
WRAPPER_A = re.compile(
    r'A\(\s*(\d+)\s*,\s*"([A-Za-z]+)"\s*,\s*"[a-z0-9-]+"\s*,\s*"[^"]*"\s*,'
    r'\s*SummonMultiplicity\.(\w+)\s*,')

# Placements the ideal roster wants and the source does not offer, for which
# a decision is already recorded. Keyed by exact placement, so a further
# missing placement of the same creature is never covered by one of these.
POL04 = ("POL-04 / Sprint 5 T3: Brachiosaurus is registered only in Summon Nature's Ally; "
         "whether Summon Monster offers it is an open charter decision")
POL05 = ("POL-05 / Sprint 5 T4: the retained native wrapper ladder stops where the game's own "
         "options stop; whether to extend it is an open decision")
RECORDED_DISPOSITIONS = dict(
    [(Placement("brachiosaurus", "sm", 7, "One"), POL04),
     (Placement("brachiosaurus", "sm", 8, "OneD3"), POL04),
     (Placement("brachiosaurus", "sm", 9, "OneD4PlusOne"), POL04)] +
    [(Placement(key, family, parent, "OneD4PlusOne"), POL05) for key, family, parents in (
        ("redcap", "sm", (8, 9)), ("axiomite", "sm", (9,)), ("soul-eater", "sm", (9,)),
        ("manticore", "sna", (8, 9)), ("mite", "sna", (4, 5, 6, 7, 8, 9)))
     for parent in parents])

# S5 T3 / POL-04: the three Summon Monster cells await the owner's choice between
# keeping the shipped Nature's Ally-only scope (A) and publishing them (B).
SCOPE_PENDING = {Placement("brachiosaurus", "sm", 7, "One"), Placement("brachiosaurus", "sm", 8, "OneD3"),
                 Placement("brachiosaurus", "sm", 9, "OneD4PlusOne")}
# Ideal placements the owner has explicitly approved out of scope, with the
# decision on record. Empty until a decision is recorded; keyed by exact placement.
APPROVED_OUT_OF_SCOPE = {}


def canonical(wrapper_key):
    """PascalCase wrapper key to the canonical kebab creature key, as
    ExpandedSummoningCoveragePolicy.CanonicalKey does."""
    out = []
    for i, c in enumerate(wrapper_key):
        if i and c.isupper():
            out.append("-")
        out.append(c.lower())
    return "".join(out)


def quantity_at(source, parent):
    """The quantity GenerateVariants and the charter give a creature of tier
    `source` under parent spell `parent` (parent >= source)."""
    return "One" if parent == source else "OneD3" if parent == source + 1 else "OneD4PlusOne"


def ladder(key, family, tier):
    return [Placement(key, family, parent, quantity_at(tier, parent))
            for parent in range(tier, 10)]


def read_ideal(text):
    calls = len(re.findall(r'\bR\("', text))
    entries = [Entry(m) for m in ENTRY.finditer(text)]
    if not entries or len(entries) != calls:
        raise ReadError("ideal roster: %d of %d R(...) entries could be read" % (len(entries), calls))
    seen = Counter(e.key for e in entries)
    repeated = sorted(key for key, count in seen.items() if count > 1)
    if repeated:
        raise ReadError("ideal roster: creature keys appear more than once: " + ", ".join(repeated))
    for e in entries:
        for family in FAMILIES:
            tier = e.tier(family)
            if tier is not None and not 1 <= tier <= 9:
                raise ReadError("ideal roster: %s has %s tier %s" % (e.key, family, tier))
    return entries


def read_project(text):
    """Creature keys and family tiers from ExpandedSummoningCatalog."""
    calls = text.count('C("')
    rows = CREATURE.findall(text)
    if not rows or len(rows) != calls:
        raise ReadError("project catalog: %d of %d C(...) creatures could be read" % (len(rows), calls))
    creatures = []
    for key, name, monster, _templated, ally, _visual in rows:
        creature = {"key": key, "name": name,
                    "sm": None if monster == "null" else int(monster),
                    "sna": None if ally == "null" else int(ally)}
        for family in FAMILIES:
            if creature[family] is not None and not 1 <= creature[family] <= 9:
                raise ReadError("project catalog: %s has %s tier %s" % (key, family, creature[family]))
        creatures.append(creature)
    repeated = sorted(k for k, n in Counter(c["key"] for c in creatures).items() if n > 1)
    if repeated:
        raise ReadError("project catalog: creature keys appear more than once: " + ", ".join(repeated))
    return creatures


def read_wrappers(text):
    """Every retained native wrapper, with its family, tier, quantity and
    choice identity. A factory call that does not read completely is named."""
    wrappers = []
    problems = []
    for call in WRAPPER_CALL.finditer(text):
        line_number = text.count("\n", 0, call.start()) + 1
        line = text[call.start(1):text.find("\n", call.start())]
        if call.group(1) == "N":
            match = WRAPPER_N.match(line)
            family = "sm"
        else:
            match = WRAPPER_A.match(line)
            family = "sna"
        if match is None:
            problems.append("line %d: %s" % (line_number, line.strip()))
            continue
        tier, key, quantity = int(match.group(1)), match.group(2), match.group(3)
        if quantity not in QUANTITIES or not 1 <= tier <= 9:
            problems.append("line %d: tier %d, quantity %s" % (line_number, tier, quantity))
            continue
        wrappers.append({"family": family, "tier": tier, "key": canonical(key),
                         "quantity": quantity,
                         "symbol": "KMG.Summoning.NativeOption.%s.Tier%d.%s.%s" % (
                             FAMILY_SYMBOL[family], tier, key, quantity)})
    if problems or not wrappers:
        raise ReadError("native wrappers: %d factory calls could not be read: %s" % (
            len(problems), "; ".join(problems) or "no wrapper found"))
    return wrappers


def read_suppressed(text):
    match = SUPPRESSED.search(text)
    if match is None:
        raise ReadError("visibility catalog: SuppressedCreatureKeys could not be read")
    return set(re.findall(r'"([a-z0-9-]+)"', match.group(1)))


def read_declared(identity_text, visibility_text, project_text):
    """The totals the source states for itself, to cross-check against."""
    def constant(text, name, where):
        match = re.search(r"internal const int " + re.escape(name) + r" = (\d+);", text)
        if match is None:
            raise ReadError(where + ": constant " + name + " could not be read")
        return int(match.group(1))

    families = dict((name, (int(roster), int(placements))) for name, roster, placements in re.findall(
        r"ValidateFamily\(SummonFamily\.(Monster|NaturesAlly),\s*(\d+),\s*(\d+)\)", project_text))
    if set(families) != {"Monster", "NaturesAlly"}:
        raise ReadError("project catalog: ValidateFamily totals could not be read")
    return {
        "units": constant(identity_text, "UnitCount", "identity catalog"),
        "wrappers": constant(identity_text, "NativeExpandedOptionIdentityCount", "identity catalog"),
        "registered": constant(visibility_text, "RegisteredLogicalPlacementCount", "visibility catalog"),
        "suppressed": constant(visibility_text, "SuppressedLogicalPlacementCount", "visibility catalog"),
        "sm": families["Monster"][1],
        "sna": families["NaturesAlly"][1],
    }


def read_registered_symbols(text):
    try:
        entries = json.loads(text)["entries"]
        return {row["symbol"] for row in entries if row["status"] == "active"}
    except (ValueError, KeyError, TypeError) as error:
        raise ReadError("identity manifest could not be read: " + str(error)) from error


def ideal_placements(entries):
    return [placement for e in entries for family in FAMILIES if e.tier(family)
            for placement in ladder(e.key, family, e.tier(family))]


def declared_choices(creatures, wrappers, suppressed):
    """Every choice the source declares, derived from the catalogs alone and
    never from the ideal roster. Project choices follow GenerateVariants; the
    visibility catalog decides which are admitted. Wrappers are published
    unconditionally, as the publisher does."""
    choices = []
    for creature in creatures:
        for family in FAMILIES:
            if creature[family] is None:
                continue
            for placement in ladder(creature["key"], family, creature[family]):
                choices.append(Choice(
                    placement, "project",
                    "KMG.Summoning.Ability.%s.Tier%d.%s.%s" % (
                        FAMILY_SYMBOL[family], placement.parent, token(creature["key"]),
                        placement.quantity),
                    creature["key"] not in suppressed))
    for w in wrappers:
        choices.append(Choice(Placement(w["key"], w["family"], w["tier"], w["quantity"]),
                              "native-wrapper", w["symbol"], True))
    return choices


def withheld_disposition(key):
    """(text, recorded) for a withheld placement of this creature."""
    note = OWNER_WITHDRAWAL_NOTES.get(key)
    if note:
        return "Owner withdrawal: " + note, True
    return "Unexplained: withheld by the visibility catalog with no recorded reason", False


def missing_disposition(placement):
    """(text, recorded) for a placement with no declared choice. Only an exact
    placement on record is explained."""
    if placement in RECORDED_DISPOSITIONS:
        return RECORDED_DISPOSITIONS[placement], True
    return "Unexplained: no declared choice and no recorded decision", False


def compare(ideal, choices, registered_symbols, declared):
    """Exact placement comparison. Multiplicity is kept: a placement declared
    twice is reported, not merged."""
    ideal_count = Counter(ideal)
    by_placement = defaultdict(list)
    for choice in choices:
        by_placement[choice.placement].append(choice)

    unpublished = []
    states = {}
    for placement in sorted(ideal_count):
        offered = by_placement.get(placement, [])
        state = "covered" if any(c.admitted for c in offered) else \
            "withheld" if offered else \
            "out-of-scope" if placement in APPROVED_OUT_OF_SCOPE else "missing"
        states[placement] = state
        if state == "covered":
            continue
        if state == "out-of-scope":
            disposition, recorded = "Approved out of scope: " + APPROVED_OUT_OF_SCOPE[placement], True
        else:
            disposition, recorded = withheld_disposition(placement.key) if state == "withheld" \
                else missing_disposition(placement)
        unpublished.append(Unpublished(placement, state, disposition,
                                       [c.symbol for c in offered], recorded))

    project = [c for c in choices if c.source == "project"]
    wrappers = [c for c in choices if c.source == "native-wrapper"]
    totals = {
        "ideal": len(ideal),
        "project": len(project),
        "project_sm": sum(1 for c in project if c.placement.family == "sm"),
        "project_sna": sum(1 for c in project if c.placement.family == "sna"),
        "wrapper": len(wrappers),
        "declared": len(choices),
        "admitted": sum(1 for c in choices if c.admitted),
        "suppressed_choices": sum(1 for c in choices if not c.admitted),
        "covered": sum(ideal_count[p] for p, s in states.items() if s == "covered"),
        "withheld": sum(ideal_count[p] for p, s in states.items() if s == "withheld"),
        "missing": sum(ideal_count[p] for p, s in states.items() if s == "missing"),
        "out_of_scope": sum(ideal_count[p] for p, s in states.items() if s == "out-of-scope"),
    }
    extra = [c for c in choices if c.placement not in ideal_count]
    duplicates = sorted((p, [c.symbol for c in cs]) for p, cs in by_placement.items() if len(cs) > 1)
    ideal_duplicates = sorted(p for p, n in ideal_count.items() if n > 1)
    unregistered = [c for c in choices if c.symbol not in registered_symbols]
    unexplained = [u for u in unpublished if not u.recorded]
    stale = sorted(p for p in RECORDED_DISPOSITIONS if states.get(p) == "covered")
    # A placement approved out of scope that the source nevertheless declares contradicts the decision.
    scope_conflicts = sorted(p for p in APPROVED_OUT_OF_SCOPE if by_placement.get(p))
    totals.update({"extra": len(extra), "duplicates": len(duplicates),
                   "ideal_duplicates": len(ideal_duplicates),
                   "unregistered": len(unregistered), "unexplained": len(unexplained),
                   "unexplained_withheld": sum(1 for u in unexplained if u.state == "withheld"),
                   "unexplained_missing": sum(1 for u in unexplained if u.state == "missing")})
    cross = [
        ("Project creatures (`UnitCount`)", declared["units"], declared["project_creatures"]),
        ("Registered project placements (`RegisteredLogicalPlacementCount`)",
         declared["registered"], totals["project"]),
        ("Summon Monster project placements (`ValidateFamily`)", declared["sm"], totals["project_sm"]),
        ("Summon Nature's Ally project placements (`ValidateFamily`)", declared["sna"], totals["project_sna"]),
        ("Withheld project placements (`SuppressedLogicalPlacementCount`)",
         declared["suppressed"], totals["suppressed_choices"]),
        ("Native wrapper choices (`NativeExpandedOptionIdentityCount`)", declared["wrappers"], totals["wrapper"]),
    ]
    return {"totals": totals, "states": states, "unpublished": unpublished,
            "extra": extra, "duplicates": duplicates, "ideal_duplicates": ideal_duplicates,
            "unregistered": unregistered, "stale_dispositions": stale, "cross": cross,
            "scope_conflicts": scope_conflicts,
            "ideal_count": ideal_count}


def placement_counts(comparison, key, family, tier):
    """Covered, withheld and missing ideal placements of one creature in one
    family; None where the ideal roster does not place it there."""
    if tier is None:
        return None
    states = [comparison["states"][p] for p in ladder(key, family, tier)]
    counts = {"covered": states.count("covered"), "withheld": states.count("withheld"),
              "missing": states.count("missing"), "total": len(states)}
    # Present only when an approved-out-of-scope decision applies, so earlier readers keep their shape.
    if states.count("out-of-scope"):
        counts["out_of_scope"] = states.count("out-of-scope")
    return counts


def describe_counts(counts):
    """The label shown for one family, read from the counts only. Partial
    means at least one placement is published."""
    if counts is None:
        return "-"
    covered, withheld, missing, total = (counts["covered"], counts["withheld"],
                                         counts["missing"], counts["total"])
    if covered == total:
        return "Complete %d/%d" % (covered, total)
    if withheld == total:
        return "Withheld 0/%d" % total
    if missing == total:
        return "Missing 0/%d" % total
    out_of_scope = counts.get("out_of_scope", 0)
    if out_of_scope == total:
        return "Approved out of scope 0/%d" % total
    rest = ", ".join(part for part in ("%d withheld" % withheld if withheld else "",
                                       "%d missing" % missing if missing else "",
                                       "%d approved out of scope" % out_of_scope if out_of_scope else "") if part)
    return "%s %d/%d (%s)" % ("Partial" if covered else "Unpublished", covered, total, rest)


CREATURE_CATEGORIES = (
    ("complete", "Every ideal placement published"),
    ("partial", "Some ideal placements published, some not"),
    ("withheld", "No ideal placement published; every one registered and withheld"),
    ("mixed", "No ideal placement published; some registered and withheld, some with no declared choice"),
    ("missing", "No ideal placement published or registered"),
)


def creature_category(counts_by_family):
    """A creature's category across every family the ideal roster places it
    in, from the placement counts alone."""
    counts = [c for c in counts_by_family.values() if c is not None]
    covered = sum(c["covered"] for c in counts)
    withheld = sum(c["withheld"] for c in counts)
    missing = sum(c["missing"] for c in counts)
    total = sum(c["total"] for c in counts)
    out_of_scope = sum(c.get("out_of_scope", 0) for c in counts)
    if covered == total or (covered and covered + out_of_scope == total):
        return "complete"
    if covered:
        return "partial"
    if withheld == total:
        return "withheld"
    if missing == total:
        return "missing"
    return "mixed"


def coverage(entries, project_text, wrapper_text, visibility_text, identity_text, ledger_text):
    """Read every input, derive the declared choices independently of the
    ideal roster, and compare placement by placement."""
    creatures = read_project(project_text)
    wrappers = read_wrappers(wrapper_text)
    suppressed = read_suppressed(visibility_text)
    project_keys = {c["key"] for c in creatures}
    ideal_keys = {e.key for e in entries}
    if not suppressed <= project_keys:
        raise ReadError("visibility catalog withholds an unregistered creature: " +
                        ", ".join(sorted(suppressed - project_keys)))
    unknown = sorted({w["key"] for w in wrappers} - ideal_keys)
    if unknown:
        raise ReadError("native wrappers name creatures absent from the ideal roster: " + ", ".join(unknown))
    declared = read_declared(identity_text, visibility_text, project_text)
    declared["project_creatures"] = len(creatures)
    comparison = compare(ideal_placements(entries),
                         declared_choices(creatures, wrappers, suppressed),
                         read_registered_symbols(ledger_text), declared)
    wrapper_keys = {w["key"] for w in wrappers}
    comparison.update({
        "identity": {e.key: ("ProjectOwned" if e.key in project_keys else
                             "NativeWrapper" if e.key in wrapper_keys else "None")
                     for e in entries},
        "project_keys": sorted(project_keys),
        "wrapper_only": sorted(wrapper_keys - project_keys),
        "unrepresented": sum(1 for e in entries if e.key not in project_keys | wrapper_keys),
    })
    counts = {e.key: {f: placement_counts(comparison, e.key, f, e.tier(f)) for f in FAMILIES}
              for e in entries}
    comparison.update({
        "placement_counts": counts,
        "family_status": {key: {f: describe_counts(value[f]) for f in FAMILIES}
                          for key, value in counts.items()},
        "creature_category": {key: creature_category(value) for key, value in counts.items()},
        "available_somewhere": sum(1 for value in counts.values()
                                   if any(c and c["covered"] for c in value.values())),
    })
    return comparison


def coverage_summary(cov):
    t = cov["totals"]
    word = "complete" if t["covered"] + t["out_of_scope"] == t["ideal"] else "incomplete"
    scope = ("; %d approved out of scope" % t["out_of_scope"]) if t["out_of_scope"] else ""
    return ("Source-derived coverage is %s: %s of %s ideal placements admitted; %d withheld; "
            "%d missing%s; %d extra; %d duplicate; %d unexplained. Native menus and casts are not "
            "established by this report." % (word, format(t["covered"], ","), format(t["ideal"], ","),
                                             t["withheld"], t["missing"], scope, t["extra"],
                                             t["duplicates"], t["unexplained"]))


def read_evidence(text):
    """Native observations keyed by exact ability identity. A missing or empty ledger is no evidence."""
    if not text:
        return {}
    try:
        rows = json.loads(text)["entries"]
        evidence = defaultdict(list)
        for row in rows:
            if row["kind"] not in ("graph", "menu", "cast") or row["result"] not in ("PASS", "FAIL"):
                raise ReadError("evidence row has an unknown kind or result: " + row["symbol"])
            evidence[row["symbol"]].append(row)
        return dict(evidence)
    except (ValueError, KeyError, TypeError) as error:
        raise ReadError("native evidence ledger could not be read: " + str(error)) from error


Cell = namedtuple("Cell", "placement ideal scope symbol registered declared admitted graph menu cast")


def project_symbol(placement):
    return "KMG.Summoning.Ability.%s.Tier%d.%s.%s" % (
        FAMILY_SYMBOL[placement.family], placement.parent, token(placement.key), placement.quantity)


def cell_report(entries, cov, choices, registered_symbols, evidence, keys):
    """One row per ideal placement of each named creature, in both families.
    Every fact is read independently from its own input and only for the exact
    family-specific identity: another family's identity never fills a cell."""
    by_placement = defaultdict(list)
    for choice in choices:
        by_placement[choice.placement].append(choice)
    rows = []
    for key in keys:
        entry = next((e for e in entries if e.key == key), None)
        if entry is None:
            raise ReadError("creature absent from the ideal roster: " + key)
        for family in FAMILIES:
            tier = entry.tier(family)
            if tier is None:
                continue
            for placement in ladder(key, family, tier):
                offered = by_placement.get(placement, [])
                symbols = [c.symbol for c in offered] or [project_symbol(placement)]
                def native(kind):
                    rows_for = [row for symbol in symbols for row in evidence.get(symbol, []) if row["kind"] == kind]
                    if not rows_for:
                        return "none"
                    return ",".join(sorted({row["result"] + ":" + row["session"] for row in rows_for}))
                scope = ("approved out of scope" if placement in APPROVED_OUT_OF_SCOPE else
                         "pending owner decision" if placement in SCOPE_PENDING else "in scope")
                rows.append(Cell(placement, True, scope, symbols[0],
                                 any(symbol in registered_symbols for symbol in symbols),
                                 bool(offered), any(c.admitted for c in offered),
                                 native("graph"), native("menu"), native("cast")))
    return rows


def cell_table(rows):
    lines = ["| Creature | Family | Parent | Quantity | Ideal | Scope | Exact identity | Registered | "
             "Declared | Admitted | Native graph | Native menu | Native cast |",
             "|---|---|---|---|---|---|---|---|---|---|---|---|---|"]
    yes = lambda value: "yes" if value else "no"
    for r in rows:
        p = r.placement
        lines.append("| %s | %s | %s | %s | %s | %s | `%s` | %s | %s | %s | %s | %s | %s |" % (
            p.key, FAMILY_NAME[p.family], ROMAN[p.parent], QUANTITY_LABEL[p.quantity], yes(r.ideal), r.scope,
            r.symbol, yes(r.registered), yes(r.declared), yes(r.admitted), r.graph, r.menu, r.cast))
    return "\n".join(lines)


def coverage_section(entries, cov):
    t = cov["totals"]
    complete = t["covered"] == t["ideal"]
    names = {e.key: e.name for e in entries}
    lines = [
        "## Coverage of ideal placements",
        "",
        ("Coverage is **complete**: " if complete else "Coverage is **incomplete**: ") +
        "%s of %s ideal placements have a choice admitted by the current publication policy. "
        "%d are registered and withheld, and %d have no declared choice." % (
            format(t["covered"], ","), format(t["ideal"], ","), t["withheld"], t["missing"]),
        "",
        "A placement is one creature in one spell family, under one parent spell, at one",
        "quantity. Every ideal placement is matched exactly against the choices the source",
        "declares. A creature's registered identity, or a choice in another family, tier or",
        "quantity, never stands in for one. The declared choices are derived from the",
        "catalogs alone, never from the ideal roster's promises.",
        "",
        "| Evidence state | Placements | Established by |",
        "|---|---|---|",
        "| Ideal placement | %s | `ExpandedSummoningIdealRosterCatalog` |" % format(t["ideal"], ","),
        "| Registered project choice | %s | `ExpandedSummoningCatalog` tiers under `GenerateVariants` "
        "semantics, each an active identity in `blueprints/blueprints.json` |" % format(t["project"], ","),
        "| Registered native wrapper choice | %d | `SummonNativeExpansionCatalog`, each an active "
        "identity in `blueprints/blueprints.json` |" % t["wrapper"],
        "| Admitted by the publication policy | %s | `SummonVisibilityCatalog`, which withholds %d "
        "project choices |" % (format(t["admitted"], ","), t["suppressed_choices"]),
        "| Observed in a native menu | not established | this report is source-derived |",
        "| Exercised by a native cast | not established | this report is source-derived |",
        "",
        "Assumptions: the Expanded Summoning module is enabled; the publication policy is the",
        "source's own; native wrappers are published unconditionally, as the publisher does.",
        "Optional providers are not counted: the Spider Summoner rows that Races Unleashed makes",
        "possible, and any other mod's choices, are outside the core charter and outside the",
        "catalogs read here.",
        "",
        "| Comparison | Placements |",
        "|---|---|",
        "| Ideal placements covered by an admitted choice | %s |" % format(t["covered"], ","),
        "| Ideal placements withheld by the publication policy | %d |" % t["withheld"],
        "| Ideal placements with no declared choice | %d |" % t["missing"],
        "| Declared choices not in the ideal roster (extra) | %d |" % t["extra"],
        "| Placements declared more than once (duplicate) | %d |" % t["duplicates"],
        "| Declared choices without an active registered identity | %d |" % t["unregistered"],
        "| Unpublished placements without a recorded disposition | %d (%d withheld, %d missing) |" % (
            t["unexplained"], t["unexplained_withheld"], t["unexplained_missing"]),
        "",
        "Cross-checked against the totals the source states for itself:",
        "",
        "| Figure | Stated in source | Derived here |",
        "|---|---|---|",
    ]
    lines += ["| %s | %s | %s |" % (label, format(a, ","), format(b, ",")) for label, a, b in cov["cross"]]
    lines += ["", "### Ideal placements not published", ""]
    if cov["unpublished"]:
        lines += ["| Creature | Family | Parent | Quantity | State | Decision on record | Disposition |",
                  "|---|---|---|---|---|---|---|"]
        for u in cov["unpublished"]:
            lines.append("| %s | %s | %s %s | %s | %s | %s | %s |" % (
                names[u.placement.key], FAMILY_NAME[u.placement.family], FAMILY_SYMBOL[u.placement.family],
                ROMAN[u.placement.parent], QUANTITY_LABEL[u.placement.quantity],
                "Registered, withheld" if u.state == "withheld" else "No declared choice",
                "yes" if u.recorded else "no", u.disposition))
    else:
        lines.append("None.")
    for title, rows in (
            ("Declared choices not in the ideal roster", ["`%s`" % c.symbol for c in cov["extra"]]),
            ("Placements declared more than once", ["%s %s %s %s: %s" % (
                p.key, FAMILY_SYMBOL[p.family], ROMAN[p.parent], p.quantity,
                ", ".join("`%s`" % s for s in symbols)) for p, symbols in cov["duplicates"]]),
            ("Declared choices without an active registered identity",
             ["`%s`" % c.symbol for c in cov["unregistered"]]),
            ("Recorded dispositions whose placement is now published",
             ["%s %s %s %s" % (p.key, FAMILY_SYMBOL[p.family], ROMAN[p.parent], p.quantity)
              for p in cov["stale_dispositions"]])):
        lines += ["", "### " + title, ""]
        lines += ["- " + row for row in rows] if rows else ["None."]

    groups = defaultdict(list)
    for e in sorted(entries, key=lambda x: x.key):
        groups[cov["creature_category"][e.key]].append(e.name)
    lines += ["", "### Coverage by creature", "",
              "| Creature coverage | Creatures | Which |", "|---|---|---|"]
    for key, label in CREATURE_CATEGORIES:
        lines.append("| %s | %d | %s |" % (
            label, len(groups[key]), "" if key == "complete" else ", ".join(groups[key])))
    lines += [
        "",
        "Available somewhere: %d creatures have at least one published ideal placement (the first"
        % cov["available_somewhere"],
        "two rows). That is not complete coverage; the partial row is the difference.",
        "",
        "### Registered unit identity",
        "",
        "Identity is a separate and weaker fact than a published placement.",
        "",
        "| Unit identity | Creatures | Meaning |",
        "|---|---|---|",
        "| Project-owned | %d | A summon-safe unit in ExpandedSummoningCatalog |" % len(cov["project_keys"]),
        "| Retained native wrapper | %d | A native unit exposed through a preserved wrapper |"
        % len(cov["wrapper_only"]),
        "| None yet | %d | In the ideal roster; no unit identity exists |" % cov["unrepresented"],
        "",
    ]
    return lines


def read_sources():
    sources = {name: path.read_bytes().decode("utf-8-sig") for name, path in (
        ("ideal", MANIFEST), ("project", SHIPPED), ("wrappers", WRAPPERS),
        ("visibility", VISIBILITY), ("identities", IDENTITIES), ("ledger", LEDGER))}
    sources["evidence"] = EVIDENCE.read_bytes().decode("utf-8-sig") if EVIDENCE.exists() else ""
    return sources


def cells(keys, sources=None):
    """The decision rows for the named creatures from the given (or real) sources."""
    sources = sources or read_sources()
    entries = read_ideal(sources["ideal"])
    cov = coverage(entries, sources["project"], sources["wrappers"], sources["visibility"],
                   sources["identities"], sources["ledger"])
    choices = declared_choices(read_project(sources["project"]), read_wrappers(sources["wrappers"]),
                               read_suppressed(sources["visibility"]))
    return cell_report(entries, cov, choices, read_registered_symbols(sources["ledger"]),
                       read_evidence(sources.get("evidence", "")), keys)


def build(sources):
    """Every document and summary from the given source texts. Raises
    ReadError for input that cannot be read completely."""
    entries = read_ideal(sources["ideal"])
    d = derive(entries)
    problems = check(d)
    if problems:
        raise ReadError("ideal roster does not reconcile with the charter: " + "; ".join(problems))
    missing = {c["key"] for c in read_project(sources["project"])} - {e.key for e in entries}
    if missing:
        raise ReadError("shipped creatures absent from the ideal roster: " + ", ".join(sorted(missing)))
    cov = coverage(entries, sources["project"], sources["wrappers"], sources["visibility"],
                   sources["identities"], sources["ledger"])
    mismatched = [label for label, stated, derived in cov["cross"] if stated != derived]
    if mismatched:
        raise ReadError("derived totals differ from the source's own: " + "; ".join(mismatched))
    if cov["ideal_duplicates"]:
        raise ReadError("ideal placements repeat: %d" % len(cov["ideal_duplicates"]))
    return entries, d, cov, roster_doc(entries, d, cov), trace_doc(entries)


def run(check_only, sources=None, roster_path=ROSTER_DOC, trace_path=TRACE_DOC, out=print):
    try:
        entries, d, cov, roster, trace = build(sources or read_sources())
    except ReadError as error:
        out("Ideal roster input could not be read completely: " + str(error))
        return 1
    if check_only:
        stale = [path.name for path, text in ((roster_path, roster), (trace_path, trace))
                 if not path.exists() or path.read_bytes().decode("utf-8") != text]
        if stale:
            out("Generated documents are stale: " + ", ".join(stale))
            out(coverage_summary(cov))
            return 1
        out("Ideal roster documents are current (freshness only; coverage below).")
        out(coverage_summary(cov))
        return 0
    roster_path.write_bytes(roster.encode("utf-8"))
    trace_path.write_bytes(trace.encode("utf-8"))
    out(f"Ideal roster documents written. The ideal manifest reconciles with the charter: "
        f"{d['unique']} creatures; SM {d['sm_entries']}/{d['sm_total']}; "
        f"SNA {d['sna_entries']}/{d['sna_total']}; {d['sm_total'] + d['sna_total']} ideal placements.")
    out(coverage_summary(cov))
    return 0


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true",
                        help="verify the documents are current without writing")
    parser.add_argument("--cells", help="print decision rows for these comma-separated creature keys")
    args = parser.parse_args()
    if args.cells:
        try:
            print(cell_table(cells([key.strip() for key in args.cells.split(",") if key.strip()])))
        except ReadError as error:
            print("Decision rows could not be read completely: " + str(error))
            return 1
        return 0
    return run(args.check)


if __name__ == "__main__":
    sys.exit(main())
