# Contextual world-map teleportation implementation report

Status: IN PROGRESS. This report does not qualify contextual casting or authorize
a release. Release metadata remains 0.0.116.

## Base and scope

Clean base: `58d9511082af30f1a4ec88c1238ae7ae2b3651c2`, verified against freshly
fetched `origin/master` before source edits. The expected
`6874dc15a27ded132456dbdd480f47c794543a05` is an ancestor; all eleven intervening
commits, including published 0.0.115 and 0.0.116 work, were inspected and retained.
See `docs/TELEPORTATION-NATIVE-FORENSICS.md` for the reconciliation. Branch:
`codex/contextual-world-map-teleportation`. No merge or history rewrite occurred.

Final qualified source SHA: pending completion of all required gates.

Pushed coherent checkpoints:

- `0cd3eca4684bfa181746fca23d849319b745bd15`: pure teleportation policies and guarded native inventory.
- `11540f8efe6301262d92330e9f3a93b077e3584d`: compatibility profile package identity and exact restoration regression.
- `d0b746d8542655665eb4842c90cb104644c16a36`: contextual actions, alternate outcomes, one-use transactions and mishaps.
- `b919c267bcadc4f005a5777ea969778d84e64c57`: guarded native world-map panel observation.
- `87782a8a8c742aa91f43b663e2d88ac138518ea0`: default-ON twelfth module and schema-11 contracts.

Every checkpoint was pushed using the owner's exact policy wrapper. The draft
pull request and final source commit list remain pending implementation.

## Implementation and evidence

Pure policies cover the exact d100 table, familiarity thresholds and strict
versioned ledger encoding, centralized point eligibility, exact recall IDs,
positive source availability, deterministic source ordering/deduplication,
alternate distance/bucket selection, idempotent single-use commitment, technical
compensation guards, and repeated mishaps with a defensive cap. Domain coverage
does not establish actual native spell expenditure or relocation.

The native desktop destination presenter is `GlobalMapMessageBox.OnLocationSelect`.
Its existing vertical destination layout and native Accept/Hide/circle buttons
were inspected through guarded structured evidence. Additional spell actions
will use this panel; no raw pointer interception or separate travel window is
needed. Actual augmentation, confirmation, and no-spell preservation proof are
still pending.

The three manifest identities are:

| Spell | Symbol | Stable ID |
|---|---|---|
| Teleport | `KMG.Spells.Teleport.Ability` | `82e3fb1dce1647b58d3b7169c8520af0` |
| Greater Teleport | `KMG.Spells.GreaterTeleport.Ability` | `73d19adfe18743e0a2a3a21abf4af5f3` |
| Word of Recall | `KMG.Spells.WordOfRecall.Ability` | `596d85a666204d6ea5c0188e53f4b4de` |

The publication slice adds Conjuration/standard presentation,
empty non-null material data, no local targets/effects, no metamagic, and no
action-bar autofill. It publishes Wizard 5/7, optional exact Travel domain 5/7,
Cleric 6, and Druid 8 through a separate reversible native-list transaction.
Repeated publication preserves already valid native list instances; rollback
restores exact prior list and filtered-cache references. It never grants spells
to characters. Late `SpellListComponent` metadata is allowed by exact native type;
other extra components are rejected.

Native `Spellbook.Spend` handles prepared groups and spontaneous slots. Native
`RestoreSpontaneousSlots` and captured prepared-slot availability restoration
were traced, but the production cast-source/resource adapter is not implemented.

The settlement-circle wrapper calls `TeleportParty`, which opens outgoing edges.
That wrapper would violate the no-reveal contract. The narrower native
`SetCurrentPosition(new MapPosition(point))` plus `UpdatePawnPosition()` path was
traced; safe real contextual relocation remains unqualified.

A game-save-owned `UnitPart` is planned for the already tested familiarity state.
Only the two ordinary-arrival calls inside `MapMovementController.MoveAlongEdge`
are candidates for recording visits; map activation/reconstruction must not count.
The serialized runtime part and live ordinary-arrival hook are not implemented.

The [point audit](docs/TELEPORTATION-MAP-POINT-AUDIT.md) and companion CSV retain
706 unique stable IDs and 611 observed main-map anchors. Point-specific explicit
exclusions and current campaign arrival gates remain qualification work.

## Validation checkpoints

- Current domain suite: 1,432 passing cases, including 4,096 module settings round
  trips and 4,096 publication-intent combinations. The guarded catalog generates
  26 boundary configurations for 12 modules.
- Clean Release build and strict installable-package validation passed for the
  publication candidate. Final artifact qualification remains in progress.
- Native standalone inventory: `20260907T2336260492141Z-78c2851fd60a41468e664a68683e50a8`,
  directory `20260907T2336260376893Z-observe-teleportation-native-contracts`, PASS;
  compatibility transaction `compat-20260907T233547Z-878395056426` restored exact bytes.
- Native panel: `20260908T0028468837443Z-700a1cbb6ead4159812161c32be81f08`, directory
  `20260908T0028468636122Z-observe-teleportation-world-map`, five assertions PASS.
- Twelve-module working-save regression: `20260908T0044326727368Z-252ae5a1786547be8ab5ee7ea16f3d2d`,
  directory `20260908T0044326571115Z-working-save-smoke`, eleven assertions PASS.
- Publication/list rollback with installed mods: `20260908T0121349574829Z-c5b927ca303441749237d48dd72e9151`,
  directory `20260908T0121349454809Z-observe-teleportation-native-contracts`, seven assertions PASS.

All listed directories are under the machine-local project runtime-evidence root.
Raw artifacts, saves, proprietary assemblies, and packages are not committed.
Earlier rejected probes and their diagnoses are retained in the native-forensics
notes; two publication probes correctly failed the initially overstrict component
contract before exact inert native list metadata was identified.

Additional publication qualification:

- Standalone ON: `20260908T0125261375669Z-778971707ad141d0a3b7afab95922da6`,
  directory `20260908T0125261285579Z-observe-teleportation-native-contracts`, seven
  assertions PASS; exact restoration `compat-20260908T012436Z-1388860e92dc`.
- Teleportation OFF / other eleven ON: `20260908T0129144377215Z-2b8748198eca415fbcca4e2006bd264f`,
  directory `20260908T0129144220691Z-observe-feature-module-settings`, 31 assertions
  PASS with exact settings restoration and no OFF-mode publication fixture.
- Publication working-save regression: `20260908T0133209151824Z-e0fd19bcdadc456dbe3a1b1ea77bc991`,
  directory `20260908T0133208995428Z-working-save-smoke`, eleven assertions PASS.

## Remaining qualification and constraints

Native cast-source enumeration/spend/compensation, UI augmentation and native
confirmation, saved familiarity, ordinary movement counting, canonical damage/RNG,
relocation invariants, all required contextual scenarios, persistence, complete
26-state runtime boundaries, and compatibility profiles remain incomplete.

The installed host is UMM 0.33.0.0; the requested 0.32.4 host has not been qualified.
Required Arms and Armor and Toggle Custom Soundpacks references were absent from
the configured reference folder, installed Mods, and inspected project backups.
Owner questions about those references, UMM qualification, and the next unused
release version remain pending. Existing 0.0.115/0.0.116 releases will be preserved.

No claim of completed contextual casting, final compatibility, or release
readiness follows from these preparatory checkpoints.
