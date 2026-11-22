# Sprint 6 checkpoint A2: Brown-Fur Powerful Change persistence (RISK-03, second owner)

Status: **implemented and natively qualified for the stated scope; returned for manual review** (2026-11-22).

- Nothing is merged or released.
- A1 (Controlled Rage) was accepted by the second manual review for its development scope. Its record is
  `docs/GUNSLINGER-SPRINT6-A1-CONTROLLED-RAGE-PERSISTENCE.md`.
- RISK-03 stays **partial** until this checkpoint is reviewed.
- RISK-02 (restoration when the caster cannot be resolved) is checkpoint B and is not changed here. Every positive
  native control below uses a present caster that can be resolved.
- REP-05 and REP-06 stay BLOCKED on their missing inputs, and the search for them was not repeated.

Branch `codex/s6-risk03-brown-fur-persistence` is stacked on the accepted A1 branch
(`codex/s6-risk03-controlled-rage-persistence` at `cdb01ffe32`, records only after the accepted head `f3f91386f1`).

## 1. What changed for players

Only saves whose Brown-Fur Powerful Change records are malformed, duplicated or unsupported are affected. No released
build has been shown to write such records. A released record reads and restores exactly as before (sections 4 and 6).

**Before (characterized natively, section 3).** One bad record could break far more than its own modifier.

- Some forms make the engine's own serializer throw: an unknown ability-score name, text where a number belongs, an
  element that is not an object, or a member that is not an array. The engine settings have no error handler, so the
  exception leaves the whole file.
  - For an area file, `AreaDataStash.UnstashAreaState` lets it escape, and the area load fails.
  - For `party.json`, `ThreadedGameLoader` logs it and drops the party's cross-scene state.

  This is read from decompiled 2.1.7b. These whole-file outcomes were not executed with a broken save.
- Other forms deserialize, but the owner's whole-list validation then throws in its unguarded `PreSave` or `PostLoad`:
  schema 2, a missing member, a null element, exact or conflicting duplicates. A throw in `PostLoad` stops that unit's
  remaining parts, and a throw in `PreSave` aborts the save. It also throws in every read: the Harmony callers swallow
  that, so every valid sibling on the unit silently lost its restored increase.
- An unknown member was silently dropped on the next save. An undefined numeric ability score (99) passed owner
  validation.

**Now.**

- The save loads.
- Every valid record whose meaning is established keeps restoring its exact increase.
- A rejected or ambiguous entry restores nothing; the spell's own base modifier is untouched.
- The rejected entry stays in the save with its JSON content, through later casts, buff removal or expiry, and saves.
- A new Powerful Change cast of the same spell creates current state again.
- The log warns when a unit's condition is first read after a load and again when it changes.

## 2. Owner, provider and identities

- Owner: `UnitPartBrownFurModifierPersistence._records`, a `[JsonProperty] List<BrownFurPersistedModifierRecord>`, first
  released in v0.0.95 (`805481e583`). `git diff v0.0.95 dfc182aee7` (the pre-A1 master, which contains v0.0.193) is
  empty for the part, the record and `BrownFurAbilityScore`. **No legacy schema was released.**
- Consumers: `BrownFurModifierAdjustmentRuntime.TryAdjust`/`TryRestorePersisted` (through the `ModifiableValue.AddModifier`
  prefix), `PrepareOrdinaryRecast`/`RestoreOrdinaryRecast` (`BuffCollection.AddBuff` patch), and `Forget` (`Buff.Remove`
  prefix). All three patches already catch exceptions.
- Provider: Call of the Wild 1.14.4c-2.1 (DLL `4ebf8e1e…6915`), the only provider Brown-Fur needs. The sessions stage
  it alone beside the product, with UMM 0.33.0 (owner profile) on the game 2.1.7b copy. The fixture is
  `KMCS_AFTER_NORMAL_012_MODONLY.zks`, from `C:/Dev/KingmakerSkipCinematicLab/fixtures`: SHA-256
  `0be64b3f58a0e0bea1b4951f0f6d145e27fd72e6da566acbf5b0b4aff8eda249`, 769,309 bytes.
- Real committed cast path. A level-five CotW Arcanist with both resolved spellbooks, the Brown-Fur Powerful Change and
  Share Transmutation features and the reservoir, spawned from the registered default player blueprint so it
  deserializes, casts Bull's Strength (`4c3d0893…556d`, buff `b175001b…2116`) or Cat's Grace (`de7a025d…f69d`) through
  `UnitUseAbility` with the score toggle on. The product's own cast transaction commits. The recipe is adapted from
  the product's `BrownFurNativeCastScenario`, not from the historical working-save wrapper, which opens the adjustment
  scope directly.

## 3. Serializer boundary characterization (before any product change)

**Session `20261122T152012Z-owner-0.33.0-risk03-bf-characterize`: PASS 2 of 2** (108 s and 102 s).

- **Product and harness.** Candidate `f9c244065147` (package `6bc93163…118e`, DLL `e7afabe4…ad96`). Its Brown-Fur code
  is the released v0.0.95–v0.0.193 code. Harness DLL `498d7f6582b8afc77b5f94d731d7c270f5d6ebeda87eb903fb27624941e10351`,
  sources committed as `b136f4b532`.
- **Restoration.** Lease `runtime-20261122T152012Z-d9746be4…`, completed. Transaction `d561a70e…`. Registry readback
  verified (`25f28ffd…7514`). Owner saves (95) and the installation are unchanged; no host conflict.
- **Serializer settings, read in-process from `DefaultJsonSettings.DefaultSettings`.**
  - Newtonsoft 8.0.0.0 with `OptInContractResolver`.
  - `TypeNameHandling.Auto`, `PreserveReferencesHandling.Objects`, `MissingMemberHandling.Ignore`, no `Error` handler.
  - Converters: ten engine converters (Blueprint, Bounds, Color, Dictionary, Quaternion, Rect, UnitStrip, Vector2,
    Vector3, CollectionsConverter), plus `StringEnumConverter` and `VersionConverter`.
  - `AreaDataStash` and `ThreadedGameLoader` both build their serializer with `JsonSerializer.Create(DefaultSettings)`.
- **Real record.** The real cast produced a +6 Enhancement modifier: base +4, increase 2, carrier `AddStatBonus`. The
  Arcanist reservoir went from 4 to 3, and the transaction completed. After the native `PreSave`, the record read back
  from `Manual_1_bfchar1.zks` (`0a89ba82….json`) was:
  `{"$id":"1235","SchemaVersion":1,"BuffGuid":"b175001b42b1a02479881b72fe132116","SpellGuid":"4c3d08935262b6544ae97599b3a9556d","CasterId":"e9d20d9d-…","AbilityScore":"Strength","Increase":2,"OriginalValue":4,"OriginalDescriptor":"Enhancement","CarrierFamily":"AddStatBonus","EndTimeTicks":438309050000}`.
  It has the engine's `$id`, the enum written as its name, and the members in declaration order. It is the domain
  tests' fixture.
- **First consumer, fresh process.** The part's native `PostLoad` ran first (sequence 1). Then `ResolveIncrease`
  (sequence 2) returned 2 from the `AddModifier` restoration, and the modifier was +6 again. This was observed for a
  unit in the area state; a party member's load path was not exercised.

**Matrix: SYNTHETIC variants of that record, deserialized in-process into the released part type with
`JsonSerializer.Create(DefaultJsonSettings.DefaultSettings)`, then read through the owner's `Count` (its whole-list
validation).**

| Variant | Engine deserialization | Owner read (released code) | Written back by the same serializer |
|---|---|---|---|
| valid, as written (with `$id`) / without `$id` | ok | 1 | unchanged, with a new `$id` |
| unknown member `FutureField` | ok | 1 | **member dropped** |
| `SchemaVersion: 2` | ok | throws `NotSupportedException` | kept |
| `AbilityScore: "Strenght"` | **throws** `JsonSerializationException` (`Error converting value "Strenght"`) | not reached | — |
| `AbilityScore: "strength"` | ok (case-insensitive) | 1 | rewritten `"Strength"` |
| `AbilityScore: 99` | ok | **1 (passes)** | `99` |
| `AbilityScore: 1` | ok | 1 | rewritten `"Strength"` |
| `Increase: "four"` | **throws** `JsonReaderException` | not reached | — |
| `EndTimeTicks: 1.5` | ok, **rounded to 2** | 1 | `2` |
| `OriginalDescriptor` missing | ok (null) | throws `InvalidOperationException` | `null` |
| element `"garbage"` | **throws** `JsonSerializationException` | not reached | — |
| element `null` | ok (`[null]`) | throws `NotSupportedException` | `[null]` |
| `_records` an object | **throws** `JsonSerializationException` | not reached | — |
| exact duplicate pair / conflicting pair | ok | throws (duplicate logical modifiers) | kept |

So **unknown members disappear during typed deserialization, before any owner code runs**. An unknown enum name
throws at enum conversion; an undefined *numeric* enum value does not throw, and it even passes owner validation. The
released reader also silently accepted forms the released writer never writes (lower-case names, integer scores,
fractional ticks rounded). Expected failures are recorded as observations: the session's PASS means the probe
recorded every row, not that the rows are acceptable.

## 4. Behavior (production `BrownFurPersistedModifierListConverter` + `BrownFurPersistedModifierStore`)

| # | Case | Behavior |
|---|---|---|
| 1 | Valid current record (the released writer's form) | Active, with exact meaning. Restores only on an exact match: buff, spell, caster, score, original value, descriptor, carrier and lifetime (`EndTimeTicks`), as before. Written back with its original content minus the engine's `$id` |
| 2 | Legacy | None was released. Forms the released *reader* accepted but no released writer produced are **not** treated as valid: lower-case names, integer scores, numeric strings, fractional numbers. They are retained (reviewable choice, section 7) |
| 3 | Absence: member missing, `null` or `[]` | Not rejection. No active record, no retained list |
| 4 | Malformed record or field: wrong token type, missing member, failed structural policy, non-object or `null` element, non-array member | Retained with its JSON content. Restores nothing |
| 5 | Exact duplicates (same identity, every field equal) | One meaning: the first copy stays active and the extra copies are retained. The increase is never multiplied, and the group is not treated as ambiguous |
| 6 | Conflicting duplicates (same identity, any other field different) | Every member is retained; none is chosen |
| 7 | Unsupported or unknown content: schema ≠ 1, undefined or unknown score, unknown member | Retained with every member, value and token type. Restores nothing |
| 8 | Valid siblings beside rejected entries | A valid record stays active unless a rejected entry in the same list could refer to it: each identity field the rejected entry states in a usable form (buff, spell, caster, score, carrier) equals the record's. An entry that states no usable identity (for example `"garbage"`) withholds every valid record in that list. Unrelated siblings stay active |
| 9 | Repeated reads, callbacks and saves | Idempotent. Normalization returns the same list instances when nothing changes, and retained entries are never re-examined or reactivated |
| 10 | New committed cast; ordinary recast; removal or expiry | `Remember` replaces only the active record of the same logical modifier. `Forget` removes only active records. Retained entries are untouched, and history never withholds a later cast |
| 11 | Refused or failed write | `Remember` validates strictly before anything changes, and the store computes both lists before the part assigns either. A refused write leaves the active and retained state unchanged. New writes also refuse undefined scores |

**Persisted form.**

- `_records` keeps its released name, type and element shape. It is now read and written by a member-level converter.
  The engine's resolver honors that converter ahead of its settings converters, and that was observed natively
  (section 6).
- Active records are written as the released ten members, without `$id`. The released typed reader accepts this; that
  is covered by a domain test, not by running an old build.
- New optional `_rejectedRecords` (same converter) holds retained entries. It is null when there are none, and older
  versions ignore it.
- **Round-trip guarantee:** lossless JSON content, not byte identity. The converter reads with dates kept as strings and
  numbers kept as decimals, and writes the same tokens back. Only the engine's top-level `$id` is dropped, because
  nothing refers to a record and the engine renumbers every save.

**Diagnostic.** `[brown-fur][powerful-change.persistence.retained]` gives the unit, the active, retained and newly
rejected counts, and up to four newly rejected entries (reason, length, SHA-256 prefix, 96-character printable
preview). Each part instance remembers only its last signature (active, retained and newly rejected counts).
Unchanged repeated reads are silent, a change warns again, and each load creates a new instance.

## 5. Commits on `codex/s6-risk03-brown-fur-persistence` (stacked on A1 `cdb01ffe32`)

| Commit | Kind | Content |
|---|---|---|
| `bda6aa363c` | product source and domain tests | converter, store, recovery field, stricter `Validate`, diagnostic; 5 domain tests; source-text contract tokens (**product-build commit**) |
| `b136f4b532` | harness and session | observer, real-cast fixture, characterization session (the harness that ran characterization) |
| `a0c8ddeb98` | harness and session | qualification modes, session `risk03-bf-persist` |
| `6bf012a1af` | harness | rest and teach both Arcanist books before each cast; diagnostics when a cast does not start |
| `104a99718e` | harness | place the caster within touch reach before each cast |
| (this commit) | records | this document, register row, known issue |

## 6. Validation

### 6.1 Focused gate

`scripts/Invoke-KmgGate.ps1 -Filter 'brown-fur'`, with production compilation, on the worktree that was committed as
`bda6aa363c` (the gate records head `cdb01ffe32`): **96 of 3,173 selected, PASS**. Compilation took 15 s and the tests
about 3 s. The first run had one failure, in the test itself: its inspection parser
turned date-like strings into dates. That was fixed in the test, and the product was unchanged. `Build-Local` reran the
96 at `bda6aa363c`, and they passed.

The new cases run the production converter and store through the game's Newtonsoft 8. They use the engine's settings
choices (StringEnumConverter, objects preserved by reference, automatic type names). The fixture is the natively
written record.

- `brown-fur.persistence-released-record`:
  - the released record restores with exact meaning;
  - each of eight single-field changes to the probe (buff, spell, caster, score, original value, descriptor, carrier,
    lifetime) restores nothing;
  - the active record is written in the released form;
  - the released typed reader reads that output;
  - a newly created record writes the same member names, order and value forms as the native file.
- `brown-fur.persistence-retained-content`, twelve unconvertible or unsupported entries:
  - each is retained with its reason, restores nothing, and survives three save/reload cycles with equal JSON content;
  - unknown content keeps its token types (the decimal 1.5 and a date-like string);
  - a non-array member is retained, and the member is written as an array again;
  - absence (`{}`, `null`, `[]`) is not rejection.
- `brown-fur.persistence-duplicates-siblings`:
  - an exact duplicate leaves one active copy with an undoubled increase;
  - a conflicting pair is fully retained;
  - an unrelated sibling stays active;
  - a misspelled-score entry withholds the record it could refer to;
  - an entry with no identity withholds every record.
- `brown-fur.persistence-operations`:
  - five repeated cycles keep the same list instances;
  - a new cast becomes active beside retained history and survives a save;
  - ordinary recast still finds the active record;
  - `Forget` removes only active records, and the retained content is unchanged afterwards.
- `brown-fur.persistence-refused-write`: four invalid new records are refused (undefined score, `None`, increase 3, no
  caster). The written text and the list instances are unchanged.

The existing `brown-fur.modifier-persistence-exact`, `-fail-closed` and `-ordinary-recast` cases still pass. Repair-free
domain tests cannot load `Assembly-CSharp`, so the engine resolver, the real files and the callbacks are covered only
natively (6.3).

### 6.2 Development candidate (built once)

- Command: `scripts/Build-Local.ps1 -DevelopmentPurpose risk03-brown-fur-persistence -DevelopmentTestFilter brown-fur
  -ReferenceBundleDir <summon-overhaul-baseline bundle> -LoaderReferenceDir <repo>/artifacts/references/loader`, at
  detached `bda6aa363c`.
- 435 s. Strict standalone UMM package validation passed. 96 of 3,173 selected tests passed.
- Package `KingmakerGunslinger-0.0.193-development-risk03-brown-fur-persistence-bda6aa363c4e.zip`, SHA-256
  `b76ae9b8a59920c79d5eb3b8c9d08e45bf2e1c9dfef1f78e166eea2f261439cc`.
- DLL SHA-256 `2e4933f4a04f2b083eec4132c5960761f63f10c99525fc8637bfd4c88f7a4e99`.
- No deployment to the human installation.

### 6.3 Native qualification

All three sessions used the runner `Invoke-LoaderCompatSession.ps1 -SessionFile …/risk03-bf-persist.json
-LoaderProfile owner-0.33.0 -CandidateZip <package above> -HarnessDir <harness>`, with the corrected DLL `2e4933f4…`.
In every session:

- the transaction completed and the registry readback was verified (`25f28ffd…7514`);
- the owner saves (95) and the installation were unchanged;
- there was no host conflict.

| Session | Harness DLL (commit) | Launches | Verdict | Duration |
|---|---|---|---|---|
| `20261122T153741Z-…-risk03-bf-persist` | `2bf13aa0…2a6c` (`a0c8ddeb98`) | create FAIL | **FAIL** (fixture: the 3rd–6th identical casts did not start) | 115 s |
| `20261122T154047Z-…-risk03-bf-persist` | `af4b103d…6f37e` (`6bf012a1af`) | create FAIL | **FAIL** (fixture: those commands were available and startable but ended `Interrupt`; the targets were 2.1 m or more away, beyond touch reach) | 116 s |
| `20261122T154318Z-owner-0.33.0-risk03-bf-persist` | `93516cf65b09cec34938c37740f3baa6eff8ab3966cb87d120b1d4a0c803b988` (`104a99718e`) | create PASS, reload-mutate PASS, final PASS | **PASS** | 326 s (103, 100, 108) |

Both FAIL sessions are kept. Neither reached a save, and neither failure involved product behavior.

**Accepted session.** Lease `runtime-20261122T154318Z-8ac041725951410cb99a3f335e9a300d`, completed; transaction
`8e874d695f6f4670bb4cbb1c1fb21d62`.

- `Manual_1_bfq1.zks`: `af79b36aca00830e4d0e438c5aa293581f7d9716aa8575d05ff1b7f9b1e894b9`.
- `Manual_2_bfq2.zks`: `a148fe38c244a7822ea0a6967282c3dbc6ced19fac0d1155870e9ff6ce8c2eab`.
- `session-summary.json`: `c7d9c917c191c87cb3572c5bacba67130c5351d44b01ebe3ce750d1e62256868`.
- Evidence root: `runtime-evidence/loader-compat/20261122T154318Z-owner-0.33.0-risk03-bf-persist`. Raw saves and logs
  are not committed.

Observed (all native):

1. **Real valid state.** A disposable Arcanist at caster level 5 made seven real committed Powerful Change casts: Bull's
   Strength on six targets and Cat's Grace on one.
   - Each produced exactly one +6 Enhancement modifier (base +4, increase 2, carrier `AddStatBonus`).
   - Each spent one reservoir point, and each transaction completed.
   - Each target's part held the product's record for its cast.
2. **Serialization and native callbacks, with the pre-`PostLoad` boundary.**
   - After the product's `PreSave`, the observer wrote **SYNTHETIC** entries beside the real records on five targets.
     All six `PreSave` calls completed.
   - Read back from the written file, each target's `_records` equals the injected list exactly; the valid target holds
     its single record.
   - The engine scanned the new public converter into `DefaultJsonSettings.Converters`. Its `CanConvert` admits only
     `List<BrownFurPersistedModifierRecord>`.
   - The serializer matrix (section 3) reran under the corrected product: all 16 variants deserialize without
     throwing. The owner count is 1 for the valid and exact-duplicate inputs and 0 for every other; retained content
     is written back.
3. **Fresh-process load.** On all six parts the first consumer was the native `PostLoad`, which completed, before any
   `ResolveIncrease`. Each part's first observed read held the entries from the file.

   | Target | File entries | Active / retained after load | Restored modifiers |
   |---|---|---|---|
   | valid | 1 real | 1 / 0 | STR +6 |
   | siblings | real DEX, real STR, SYNTHETIC `"Strenght"` copy of STR | 1 (DEX) / 2 | STR **+4** (base kept; ambiguous increase withheld), DEX +6 |
   | conflict | real STR, SYNTHETIC copy with the other increase | 0 / 2 | STR +4 |
   | exact | real STR, SYNTHETIC identical copy | 1 / 1 | STR +6, **not** +8 |
   | unsupported | real STR, SYNTHETIC schema-2, score `99` and unknown-member entries for another buff | 1 / 3 | STR +6 |
   | unattributable | real STR, SYNTHETIC `"garbage"` | 0 / 2 | STR +4 |

4. **Repeated reads.** Three rounds of `Count` and `RetainedCount` on every part changed nothing.
5. **Recast, removal and expiry with retained entries present.**
   - **Recast.** On `siblings`, a new real Bull's Strength cast (`Success`) produced STR +6. The part went to 2 active
     (DEX and the new STR) and the same 2 retained entries, unchanged.
   - **Removal.** On `unsupported`, removing the Bull's Strength buff forgot its active record (0 active) and removed
     the modifier. The 3 retained entries were unchanged.
6. **Refused write.** On `exact`, which holds 1 active and 1 retained entry, a real Cat's Grace cast ran with
   `BrownFurPersistedModifierPolicy.Validate` patched to throw only for a record not already in the part.
   - It fired exactly once, on the new Dexterity record.
   - The cast succeeded with a base DEX +4 (no increase).
   - The part's active and retained state was byte-for-byte unchanged, and STR stayed +6.
7. **Second save, then a fresh-process reload.**
   - All six `PostLoad` calls completed.
   - The parts held exactly the mutated split: valid 1/0, siblings 2/2, conflict 0/2, exact 1/1, unsupported 0/3,
     unattributable 0/2.
   - Modifiers were as mutated, with no duplicate effect.
   - The unknown entry's content, `{"nested":[1,"two",true,null,1.5,"2026-11-22T10:00:00Z"]}`, equals what was
     injected.
8. **Diagnostic.** In the reload log, `newlyRejected=1;`, `=2;` and `=3;` each appeared once per affected part, and
   `newlyRejected=0;` appeared on later reads. In the final log, `newlyRejected=1;`, `=2;` and `=3;` appeared zero
   times.
9. **Exceptions.** No engine-logged exception named Brown-Fur code. Each launch has only the native
   `BugReportCanvas.OnEnable` NullReferenceException.

## 7. Decisions for review, remaining limits

- **Only the released writer's form is valid (reviewable).**
  - The released reader also accepted lower-case names, integer scores and numeric strings, and it rounded fractional
    ticks.
  - No released writer produced those forms, and some of the reader's conversions change meaning (1.5 became 2).
  - A2 therefore retains them instead of restoring from them. Accepting the unambiguous ones would be a narrow change
    in `Structure`.
- **Exact duplicates collapse to one active copy (reviewable).**
  - This keeps both of the owner's existing invariants: at most one record per logical modifier, and an increase only
    on exactly one match.
  - It never multiplies the effect, and the surplus copies are retained.
  - The alternative is to treat the group as ambiguous and restore nothing.
- **Withholding beside a rejected entry (reviewable).**
  - A valid record is withheld when a rejected entry in the same list could be the same modifier.
  - An entry with no usable identity withholds every record in that list.
  - Retained history never withholds a later cast.
- **Global converter registration.** The engine's `CommonConverters` scan added the public converter to its global
  list. That is harmless for other types, but it is engine behavior we depend on not to change.
- **Not established.**
  - A party member's load path. Characterization and qualification used units in the area state; the
    `party.json`/`ThreadedGameLoader` path was not executed.
  - The whole-file consequences of the old code are read from decompiled code, not executed with a broken save.
  - Downgrade is inferred, not executed. The released reader accepts the active records A2 writes (domain test), and
    it ignores `_rejectedRecords` and drops it on its next save.
  - Wisdom, Intelligence, Charisma, Constitution, other carrier families, and the CotW shared-spell routes were not
    exercised natively. The real casts were Bull's Strength and Cat's Grace with `AddStatBonus`.
  - Every malformed input is synthetic. No ordinary build has been shown to write one.
- **Unchanged and separate.**
  - RISK-02: when the caster cannot be resolved, `TryRestorePersisted` returns without an increase. That is
    checkpoint B.
  - Complete suite, release packaging, whole-roster persistence, the 12-mod and other mod stacks, and the human
    installation.

## 8. Proposed disposition

- **Brown-Fur's part of RISK-03:** implemented and natively qualified for the stated scope, and ready for review. If the
  review accepts it, RISK-03 can be proposed for closure for both owners at development scope, with the limits in
  section 7.
- **A1:** accepted. **RISK-02:** open, checkpoint B. **REP-05/REP-06:** BLOCKED on inputs.
