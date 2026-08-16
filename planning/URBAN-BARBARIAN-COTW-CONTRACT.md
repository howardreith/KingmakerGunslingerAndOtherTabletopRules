# Urban Barbarian Call of the Wild contract

Status: **INVESTIGATION OPEN — no adapter authorized**.

## Independence rule

Urban Barbarian is a native Kingmaker Barbarian archetype. It has no compile-
time Call of the Wild reference and no new external library. Urban blueprint
registration and native Barbarian publication must succeed when CotW is absent.
An absent, changed, unknown, or ambiguous CotW compatibility surface may disable
or mark only optional interoperability; it must never disable Urban core or any
other package module.

CotW's Urban Bloodrager is not an implementation donor. Its whole-stat-only
choice set is incomplete for Urban Barbarian; its chained class-skill mutation
is unsafe; and its magic/proficiency replacements belong to Bloodrager.

## Evidence required before an adapter decision

The guarded architecture inventory must record both final graphs:

- loaded CotW mod-entry ID/version and assembly identity;
- CotW DLL SHA-256 and MVID plus exact settings hash;
- native Rage feature, activatable, resource, buff, Greater/Tireless/Mighty
  facts, and representative native rage powers;
- CotW Rage marker identity, component type/assembly, and graph attachment;
- representative CotW passive and activated rage powers and how each recognizes
  or modifies Rage;
- any component ordering with semantic significance; and
- whether the selected Urban architecture already inherits/produces the exact
  marker and action behavior without reflection.

No adapter will be added merely because CotW is installed. If the finalized
Urban Rage already satisfies the marker and action contract, this document will
conclude that no runtime adapter is required.

## Conditional structural adapter rules

Only demonstrated missing interoperability may authorize an adapter. It must:

- use a structural contract and fingerprint, not only a private name;
- require exact mod entry, assembly, lifecycle, graph, and settings evidence;
- reconcile at a deterministic point after CotW Rage construction;
- preserve component ordering where semantics require it;
- append only missing exact behavior and reject duplicates;
- never change CotW Urban Bloodrager, native Rage owners, or unrelated owners;
- expose a precise failed-check diagnostic; and
- leave Urban publication and unrelated modules active when it cannot qualify.

## Required state model

| CotW state | Urban core | Optional interoperability | Diagnostic |
| --- | --- | --- | --- |
| Absent | Available | Not applicable | CotW not loaded |
| Supported normal | Available | Qualified if exact marker/action tests pass | Exact fingerprint |
| Supported balance fixes | Available | Qualified if exact marker/action tests pass | Exact fingerprint/settings |
| Unknown | Available | Disabled or unqualified | Exact failed structural check |
| Ambiguous | Available | Disabled or unqualified | Exact conflicting candidates/check |

## Current local evidence

The scoped Kingmaker installation currently contains CotW mod ID
`CallOfTheWild`, version `1.14.4c-2.1`, with `balance_fixes=true`. This is only
environment discovery, not a compatibility qualification. Exact binary,
settings, marker, component, action, and final-graph evidence remains pending
the guarded observer. The no-CotW profile will use the repository's reversible
compatibility transaction and must restore the original Mods tree byte-for-byte.

