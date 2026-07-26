# Persistence evidence matrix (current through Sprint 17)

## Sprint 17 execution status

The 35-row matrix and gate rules are unchanged. Sprint 17 retains the trusted I01/I02 observations and deterministic A-D fixture introduced in Sprint 16, adds executed pure-code evidence, and prepares the exact runtime-reference handoff. Automatic recording remains unavailable to later rows, and the gate stays `NoGoIncomplete` until all Critical rows pass in Kingmaker.

> The evidence recorder codifies these 35 rows. PASS/FAIL/BLOCKED observations are written as external diagnostic evidence and never used as firearm save data. All Critical rows must pass; I03, I10, I11, I13, I15, I19, and I23 require passes from two distinct run IDs.

## Decision rule

The carrier remains **NO-GO** until every Critical row, including process restart, passes on the same compiled package and target Kingmaker installation. One state transfer, identity collision, unexplained state loss, or save corruption rejects the carrier.

Record for every row:

```text
Package SHA-256
Kingmaker/UMM/Harmony assembly fingerprints
Enabled mods
Save before/after hashes
Item UniqueId values before and after
Firearm state before and after
Identity-record count
Legacy-reference count
Identity-migration snapshot
Token-migration snapshot
Relevant [KMG] log lines
```

## Fixture

Create at least four Test Muskets with visibly different states:

| Label | State |
|---|---|
| A | Loaded/Normal |
| B | Empty/Broken |
| C | Broken/Loaded |
| D | Empty/Normal, no record |

Print and retain each exact `UniqueId` before every lifecycle operation.

## Matrix

| ID | Severity | Operation | Required result |
|---|---|---|---|
| I01 | Critical | Clean launch and blueprint bootstrap | One bootstrap, eight custom blueprints, no collisions or type errors |
| I02 | Critical | Runtime-contract inspection | Exactly one readable inherited `UniqueId`; value type Guid or string |
| I03 | Critical | Create A–D | Four distinct nonempty item IDs; A–C records match; D has no record |
| I04 | Critical | Equip/unequip | IDs and states unchanged |
| I05 | Critical | Switch weapon sets | IDs and states unchanged |
| I06 | Critical | Transfer between companions | IDs and states follow the physical guns, not wielders |
| I07 | Critical | Move to/from shared stash | IDs and states unchanged |
| I08 | Critical | Area transition | IDs and states unchanged |
| I09 | Critical | Rest | IDs and states unchanged |
| I10 | Critical | Save/load in same process | IDs and states reconstructed exactly |
| I11 | Critical | Exit to desktop, restart, reload | Same item IDs and states restored; new runtime objects may differ |
| I12 | Critical | Repeat three restart cycles | No drift, duplication, or record growth |
| I13 | Critical | Sell A, save, restart, repurchase A | Same physical gun retains identity and state |
| I14 | Critical | Sell A to one vendor and buy later | No state transfer to another item |
| I15 | Critical | Duplicate/copy a Test Musket through any available engine path | Duplicate receives a different ID and begins empty/Normal unless copying semantics are explicitly adopted later |
| I16 | Critical | Delete/destroy B, save, restart | No surviving gun receives B's identity or state |
| I17 | Critical | Respec wielder | Gun IDs/states unchanged |
| I18 | Critical | Load save made before Sprint 14 with ordinary Test Muskets | Items without records begin empty/Normal; no invented state |
| I19 | Critical | Sprint 13 fixture: direct-reference Broken/Empty | Next observation writes matching identity record and removes legacy record |
| I20 | Critical | Sprint 13 equivalent duplicate | Legacy record removed; identity record retained |
| I21 | Critical | Sprint 13 conflict | Both carriers preserved; access fails closed; no partial migration |
| I22 | Critical | Sprint 13 null/unresolved reference | Evidence preserved; no unrelated state loss |
| I23 | Critical | Sprint 12 token fixture | Token migrates into identity record, verifies, then clears |
| I24 | Critical | Sprint 12 token conflict | Both carriers preserved; no overwrite |
| I25 | Critical | Native Heavy Crossbow negative control | No identity-vault record and ordinary AC |
| I26 | Critical | Two blueprint-identical Test Muskets | Distinct IDs and independent state across restart |
| I27 | Critical | Missing/malformed ID diagnostic build or fixture | State access fails; no implicit empty record or generated ID |
| I28 | Critical | Duplicate identity-record corruption fixture | Load/read fails closed; records preserved |
| I29 | Critical | Unsupported record schema fixture | Load/read fails closed; records preserved |
| I30 | Critical | Remove mod after save backup | Documented dependency warning; never claim safe uninstall |
| I31 | High | Item tooltip and value | Identity records add no visible enchantment, price, or combat modifier |
| I32 | High | Save-size measurement over 100 writes/resets | Record count returns to expected value; no unbounded growth |
| I33 | High | Call of the Wild compatibility pass | No duplicate lifecycle initialization or UnitPart conflict |
| I34 | High | Craft Magic Items compatibility pass | No identity/state loss when enchanting a firearm copy, if supported |
| I35 | High | Proper Flanking/combat-rule compatibility | Touch-AC behavior remains scoped to exact firearms |

## GO criteria

A GO decision requires:

1. All Critical rows pass.
2. The exact compiled package and logs are retained.
3. A second clean run reproduces I03, I10, I11, I13, I15, I19, and I23.
4. No state is keyed by blueprint, owner, slot, display name, runtime hash, or generated ID.
5. Save backups open normally after every tested transition.

Any Critical failure keeps the project on the persistence branch.
