# Sprint 6 checkpoint A1: Controlled Rage persistence (RISK-03, first owner)

Status: **implemented and natively qualified for the stated scope; returned for manual review** (2026-11-22). Nothing
is merged or released. RISK-03 stays **partial**: Brown-Fur, its second owner, is checkpoint A2 and is not changed
here. The draft PR is not accepted until Howie/ChatGPT review it.

Also in this checkpoint: the bounded REP-05/REP-06 historical-input intake (section 8). Both are BLOCKED on missing
inputs.

## 1. What changed for players

Only saves whose Controlled Rage data is malformed are affected. No released or ordinary build has been shown to
write such data; every released state restores exactly as before (section 4).

- Before, malformed Controlled Rage data threw an exception in several places:
  - in the engine's unguarded unit-part save and load callbacks, which aborts the save or that unit's load;
  - in every tier controller's turn-on;
  - in the action-bar name and icon patches.
- Treating the data as empty would instead have handed out a fresh full-Strength default.
- Now the save loads, and every tier whose meaning is unambiguous keeps its exact allocation.
- A tier whose saved entry is invalid or conflicting is **blocked**. Raging grants no Controlled Rage bonus for it, and
  no default is made up. Choosing an allocation with that tier's selector replaces the block.
- The rejected text is kept, unchanged, in the save. The log gets one warning per affected character per load.

## 2. Engine contracts (Kingmaker 2.1.7b `Assembly-CSharp`, decompiled)

- `UnitPartsManager.PreSave` and `PostLoad` loop over the parts with no try/catch. A throwing part aborts the save, or
  stops that descriptor's load. The parts after it are then skipped, including their `EventBus.Subscribe`.
  `UnitDescriptor.PreSave` calls the parts first. `UnitDescriptor.PostLoad` calls them last, after the facts.
- `Fact.TurnOn` catches each component's `OnTurnOn` exception and logs it, so tier controllers failed per load, with an
  error each time.
- `DefaultJsonSettings`: `OptInContractResolver`, `TypeNameHandling.Auto`, `PreserveReferencesHandling.Objects`, and
  `MissingMemberHandling` at Newtonsoft's default (Ignore). A `string` member therefore deserializes any payload
  unchanged: there is **no typed-deserialization boundary for Controlled Rage**. Older product versions ignore the new
  `_rejectedStates` member.

## 3. Released representation

`UnitPartControlledRageSelection._state` (string) uses the grammar `tier:STR,DEX,CON` joined by `;`, ordered by tier,
with tier 4 always present. Urban Barbarian first shipped in **v0.0.95**, together with this part (`96e7a4ca74`). The
format is unchanged since then: `git diff v0.0.95 HEAD` is empty for `ControlledRageSelectionState.cs` and
`UnitPartControlledRageSelection.cs`. **No separate legacy `_state` schema was released**, so no migration format was
invented. The only other inputs the released strict reader accepted were unordered tiers and leading zeros. Both are
covered.

The pre-part path "selection facts but no part" in `ResolveSelection` and the legacy selector removal are unchanged.

## 4. Behavior by case (production `ControlledRageSelectionState.Restore`, total, never throws)

| # | Case | Behavior |
|---|---|---|
| 1 | Valid current state | Restores exactly as the strict `Parse`; rewritten canonically, as before |
| 2 | Legacy | No separate schema was released. Unordered tiers and leading zeros restore with the same meaning, not rejected |
| 3 | Malformed entry for a known tier (bad values, wrong total, extra field) | That tier is **blocked**: no selection, no default, no grant |
| 4 | Duplicates | Identical duplicates have one meaning and stay active. **Conflicting groups are blocked**; no first/last/larger record is chosen |
| 5 | Unsupported or future data | An entry with an integer key other than 4/6/8 is retained, grants nothing and leaves the known tiers alone. Content not attributable to any tier (no `k:` key, empty entry, empty or whitespace string) blocks **every** tier. A state without an ordinary claim blocks ordinary, because the released writer always records it |
| 6 | Valid siblings beside invalid entries | Kept, with exact meaning. A legitimately absent tier beside a blocked one still gets its normal first-unlock default |
| 7 | Repeated reads, callbacks, mutations and saves | Idempotent: the retained list is the same instance when nothing changes; one diagnostic per distinct condition per part instance |
| 8 | Failed write | Both fields are computed before either is assigned. `TrySelect` restores the exact prior snapshot when fact synchronization fails |

**Absent vs malformed.** Legitimate absence means a null `_state` with nothing retained. It keeps the released
ordinary STR +4 default. Every non-null string is classified.

**Persisted form:**

- `_state` keeps its released name and type and holds the active tiers in the released grammar. It is null only when
  every tier is blocked.
- The new optional `[JsonProperty] List<string> _rejectedStates` holds each rejected input verbatim.
  - An entry is appended once (ordinal comparison) and never edited or removed.
  - Blocking is re-derived from it on every read, minus the tiers that are active in `_state`.
  - It is null in every save that never held rejected input.
- Why a second field. `_state` must keep carrying legitimate later writes (selections, first unlocks) in the released
  grammar. Keeping rejected entries inside it would re-trigger rejection on every load, and would mix the player's new
  choice with conflicting raw entries.

**Explicit selection over a blocked tier.** This is a reviewable choice, not owner policy. The selector of a blocked
current tier stays available, and choosing an allocation is a strictly validated new gameplay write. It replaces the
block, and the rejected input stays retained. The alternative is a tier blocked permanently, with no recovery path for
the player.

**Diagnostic.** `[urban-barbarian][controlled-rage.persistence.retained]` reports:

- the unit's name and unique id;
- active and blocked tiers;
- how many payloads are retained;
- for a newly rejected input: its length, a SHA-256 prefix and a 96-character printable preview.

**Fact synchronization** now adds the selected fact before it removes the tier's other facts. A failed add then leaves
that tier's facts as they were (`f4e5de5f92`). It skips blocked tiers.

## 5. Commits on `codex/s6-risk03-controlled-rage-persistence` (base master `dfc182aee7`)

| Commit | Kind | Content |
|---|---|---|
| `bd9da01b9f` | product source and domain tests | total reader, retained field, explicit selection, snapshot rollback, diagnostic; 4 domain tests |
| `f4e5de5f92` | product source | add-before-remove in fact synchronization (**product-build commit**) |
| `11197b3d81` | harness, observer and session | `ControlledRageObserver`, `kmgControlledRage`, session `risk03-crage-persist` |
| `c7483ddca5` | session | conditions moved to launch level |
| `6eed8d7fed` | harness | post-save injection check fixed; product values restored once the save completes |
| (this commit) | records | this document, register rows, known issue |

The `bd9da01b9f` commit message says a blocked tier's selection facts "are left untouched". That holds for
`SynchronizeSelectionFacts` only; see §7.2.

## 6. Validation

### 6.1 Focused gate (production compile and filter `urban-barbarian`)

| Command | Head | Selected | Result | Duration |
|---|---|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'urban-barbarian'` | worktree of `bd9da01b9f` | 14 of 3,167 | PASS, 0 failures | 19 s |
| same | worktree of `f4e5de5f92` | 14 of 3,167 | PASS, 0 failures | about 20 s |

The four new cases are listed below. Each drives the production `Restore`, `Serialize` and `MergeRetained` cycle that
the part's `PostLoad` and `PreSave` perform:

- `urban-barbarian.persistence-released-states`: all 1,056 released states, plus the strictly accepted variants,
  restore exactly as `Parse` and are never treated as rejected.
- `urban-barbarian.persistence-total-reader`: 8,000 deterministic inputs (seed 20261122), random and mutated. Restore
  never throws and never activates an illegal allocation or a tier that is both active and blocked. It agrees with
  `Parse` on every accepted input. Every rejected input is kept verbatim and reloads identically.
- `urban-barbarian.persistence-malformed-siblings`: a table of 16 cases (malformed, conflicting and identical
  duplicates, unknown tier, unattributable, empty). Blocked tiers refuse unlock; an absent sibling still unlocks.
- `urban-barbarian.persistence-retained-lifecycle`:
  - repeated callbacks;
  - first unlock and a valid selection;
  - refusals while raging and for the wrong tier;
  - explicit selection over a block;
  - a second, different rejection appended;
  - null beside retained input is not legitimate absence.

Limits: the domain tests cannot load `Assembly-CSharp`. The part itself, the real serializer and the engine callbacks
are covered only natively.

### 6.2 Development candidate (built once, at the product commit)

- Command: `scripts/Build-Local.ps1 -DevelopmentPurpose risk03-controlled-rage-persistence -DevelopmentTestFilter
  urban-barbarian -ReferenceBundleDir <summon-overhaul-baseline bundle> -LoaderReferenceDir <repo>/artifacts/references/loader`.
- Built at detached `f4e5de5f92`; 491 s; strict standalone UMM package validation passed; 14 of 3,167 selected tests
  passed.
- Package `KingmakerGunslinger-0.0.193-development-risk03-controlled-rage-persistence-f4e5de5f92dd.zip`, SHA-256
  `0a1f947fd1fa9979a10b50317f5091f06a19d95267934bc18f46243f740e086c`.
- DLL SHA-256 `f373e027d0ef031db36887684da4506422d5c33d3f74a66d610c8eec91490ef3`.
- The DLL's informational version reads `0.0.193-spear-bootstrap-tooltip-repair`: the version label inherited on
  master, not a release.
- No deployment to the human installation was performed.

### 6.3 Native session (disposable loader-compat game copy)

- Runner: `Invoke-LoaderCompatSession.ps1 -SessionFile scripts/compatibility/loader-compat/sessions/risk03-crage-persist.json
  -LoaderProfile owner-0.33.0 -CandidateZip <package above> -HarnessDir artifacts/loader-compat-harness-risk03`.
- Environment: game 2.1.7b copy; UMM 0.33.0 (owner profile); the product alone beside the test-only harness; every
  module on.
- Fixture: `KMCS_AFTER_NORMAL_012_MODONLY.zks`, disposable, copied into the session profile. Its party is the main
  character only, in the prologue throne room.

| Session | Harness DLL | Launches | Verdict | Restoration | Duration |
|---|---|---|---|---|---|
| `20261122T081651Z-owner-0.33.0-risk03-crage-persist` | `23a280d0…18cfb` (`11197b3d81`) | create FAIL | **FAIL** (observer authoring error) | transaction completed, settings applied, owner files verified | 92 s |
| `20261122T081904Z-owner-0.33.0-risk03-crage-persist` | `23a280d0…18cfb` (`c7483ddca5` session) | create FAIL, reload-mutate PASS, final PASS | **FAIL** (probe bug) | as above | 247 s |
| `20261122T082442Z-owner-0.33.0-risk03-crage-persist` | `d8fbb4e23eae9c3382f9a644765fd4a86d29faa39833ea5fdbbaba41c02a72fb` (`6eed8d7fed`) | create PASS, reload-mutate PASS, final PASS | **PASS** | lease `runtime-20261122T082442Z-46eac45e…` Completed; transaction `7ca5c4c3ae4c4a828e6207d788bb22b2`; registry readback verified (`4f2a1907…794c` = backup); owner saves and install unchanged; no host conflict | 250 s |

**First FAIL.** The session put `conditions` inside the `harness` block. The runner reads them at launch level, so the
observer was never requested.

**Second FAIL.** The probe compared a JSON object with a string after the save. Its reload-mutate and final launches
passed, with the same results as the accepted session. The first launch's log also showed 4 product warnings: the game
writes the save over several frames after `PreSave`, so the injected values were live in memory and a product read
contained them. The probe now restores the product's values once the save completes. The create launch no longer
asserts the absence of that warning.

Both failed sessions are retained as failures. Every launch loaded the candidate DLL `f373e027…`.

**Accepted session, observed (all native):**

- **Valid state through real paths.** The main character and four disposable units, spawned from the registered
  default player blueprint, received the Controlled Rage grant, then the Greater grant. Each grant gave the production
  first-unlock default, `4:4,0,0` and then a Greater default. Allocations were then chosen through the real allocation
  abilities with `UnitUseAbility`: all 8 commands ended `Success`, and every selection was confirmed through the part.
- **Rage before the save.** The native Rage buff was substituted with Urban Rage and granted exactly the current-tier
  allocation as Morale modifiers.
- **Save (native `PreSave`).** One completed `PreSave` per part. The main character saved `4:0,0,4;6:2,4,0`. After the
  product's own `PreSave`, the observer wrote **SYNTHETIC** raw `_state` values on the four disposable units:
  - `4:2,2,0;6:6,2,0`, a malformed Greater entry beside a valid ordinary;
  - `4:0,4,0;4:4,0,0`, a conflicting ordinary duplicate;
  - `{"schema":2,"tiers":{"4":[0,0,4]}}`, unattributable future-format content;
  - `4:0,2,2;4:0,2,2;6:2,2,2;10:4,4,2`, an identical duplicate and an unknown tier beside valid tiers.
- **Fresh process, native `PostLoad`.** 5 of 5 completed. Each read the raw value from the file and wrote exactly:

  | Unit | `_state` after | `_rejectedStates` after | Blocked |
  |---|---|---|---|
  | valid | `4:0,0,4;6:2,4,0` | null | none |
  | malformed sibling | `4:2,2,0` | `["4:2,2,0;6:6,2,0"]` | 6 |
  | conflicting duplicate | null | `["4:0,4,0;4:4,0,0"]` | 4 |
  | unattributable | null | `["{\"schema\":2,…}"]` | 4, 6, 8 |
  | siblings, unknown, identical | `4:0,2,2;6:2,2,2` | `["4:0,2,2;4:0,2,2;6:2,2,2;10:4,4,2"]` | none |

- **Rage after load.**
  - valid: STR +2, DEX +4.
  - malformed sibling: nothing. Its current tier is Greater, which is blocked.
  - conflicting duplicate: nothing.
  - unattributable: nothing.
  - siblings: +2 each.
  - One Morale modifier per stat at most, and everything removed with the buff.
- **Repeated reads.** `ResolveSelection` three times, and `UnlockTier` for each held tier, changed nothing.
- **Diagnostic.** Exactly once per newly rejected input (the `once` needles, x1 each).
- **Ordinary mutations** (allocation abilities, all `Success`):
  - valid: Greater `0,2,4`;
  - malformed sibling: explicit Greater `0,6,0` over its block;
  - siblings: Greater `0,4,2`;
  - unattributable: granted Greater Rage. The Greater unlock was refused, so no default appeared and all three tiers
    stayed blocked.
- **Forced failed write.** `UnitHelper.AddFact` was made to return null for the STR +2 / DEX +2 ordinary fact, and
  `ControlledRageRuntime.TrySelect` was called on the conflicting-duplicate unit. It returned false. `_state` stayed
  null, `_rejectedStates` stayed byte-identical, and the snapshot (selections, blocked tiers, facts, selectors) was
  unchanged. Then saved (native `PreSave`).
- **Fresh process on that save.** 5 of 5 `PostLoad` completed, with no new rejection: `newlyRejected=length=` x0, and
  retained payloads x4 with `newlyRejected=none`. The mutations survived exactly; the retained payloads are
  byte-identical; the rage grants were as mutated (valid DEX +2/CON +4, malformed sibling DEX +6, siblings DEX +4/CON +2,
  the blocked units nothing).
- **Exceptions.** No engine-logged exception named Urban Barbarian code in any launch. The only exception in the player
  logs is the native `Kingmaker.UI.BugReportCanvas.OnEnable` NullReferenceException at startup, present in every
  launch.

## 7. Limits and open items for review

1. **No released-product baseline was run.** Under v0.0.193 the same synthetic save would hit the throwing `Parse` in
   `PostLoad`. That is inferred from source and §2, not executed in this checkpoint.
2. **Selection facts of blocked tiers do not survive the reload.** Observed: the conflicting duplicate's `4:0,4,0` fact
   and the malformed sibling's `6:4,2,0` fact were present at creation and absent after the reload. Every unblocked
   tier's fact was present.
   - Product source has one remover: the tier controller's `OnTurnOff`, through `RemoveTier` and `ClearTierFacts`.
     Resynchronization then skips blocked tiers.
   - Which engine step turns the controllers off and on again between the save and the reload snapshot is **not
     attributed**.
   - These facts are hidden markers with no components, so nothing is granted or lost mechanically. The retained raw
     input in the part is the recoverable record.
   - Consequence: the forced failed write compared an empty fact set with an empty one. The add-before-remove change
     is **source-reviewed only**; it was not demonstrated natively against a present fact.
3. **Other limits.**
   - The synthetic corruption is test-only and labelled. No ordinary shipped state has been shown to produce
     malformed Controlled Rage data.
   - Level-up through the real level-up UI was not used. Features were granted directly, and that grant runs the
     production tier controllers.
   - Mighty tier was not natively exercised. It is covered by the domain tests.
   - The explicit-selection-over-a-block behavior is a design choice awaiting review (§4).
   - Downgrade: an older product version ignores `_rejectedStates`. On a migrated save whose ordinary tier is blocked,
     the old strict reader would throw as before. Not executed.
4. **Deferred by policy:** complete suite, repository wrapper, release packaging, mod-stack profiles and the human
   installation.

## 8. REP-05 and REP-06 historical-input intake (bounded, read-only)

One bounded pass on host DATA covered:

- the live save folder (95 files);
- `runtime-evidence`, `runtime-backups`, `game-copies`, `private`, `compatibility-state` and 40 worktrees;
- the repository's `artifacts/` and `evidence/` folders;
- the LocalLow and lab-root logs.

The searches were by name, by exact size and by hash. Nothing was modified, copied or launched.

- **REP-05: BLOCKED.**
  - No file matches `Quick_6.zks` (489,715 bytes, `B4D6D093…032655D`) or `Quick_5.zks` (644,034 bytes,
    `980821EB…C921`).
  - The live save folder does hold different saves with those slot names (23,743,296 and 24,295,173 bytes, mtime
    2026-09-07 15:28, the bulk migration onto DATA).
  - The 2026-08-20 run directories and `artifacts/save-forensics` are absent.
  - The forensics record names no log path, mod stack, DLC or loader for the stall.
  - DATA's lab history begins on 2026-09-07; the incident runs predate it.
- **REP-06: BLOCKED.**
  - No log contains both `SpawnParticleEffect` and `OnAreaLoaded`, and no 2,680,892-byte `output_log.txt` exists.
  - The only recorded form is the four-frame stack in `planning/release-audit-0.0.182/SUMMARY.md:151`.
  - Missing:
    - the save;
    - the buff GUID;
    - the DLL or package hash (only the version string `0.0.182-summon-menu-order-spider-summoner` is recorded);
    - the incident machine's identity.
  - Null-FX causation stays withdrawn, and the null-blueprint idea stays a hypothesis (GS-AUD-0024).
- **Exact inputs needed.** These must come from HOWARD-LAPTOP or the owner's original uploads:
  - for REP-05: the 2026-08 `Quick_6.zks` and `Quick_5.zks`, or their `artifacts/save-forensics` copies and the
    20260820 run directories;
  - for REP-06: the 2,680,892-byte `output_log.txt`, plus `output_log_prev.txt` or a crash folder, and the save.

  The search will not be repeated unchanged.

## 9. A2 preparation: Brown-Fur serializer boundary (source read only; no change)

`UnitPartBrownFurModifierPersistence` stores a `[JsonProperty]` list of typed `BrownFurPersistedModifierRecord`
objects. Fields: `SchemaVersion`, the buff, spell and caster ids, `AbilityScore` (an internal enum), `Increase`, the
original value and descriptor, the carrier family and `EndTimeTicks`. Whole-list validation runs in `PreSave`,
`PostLoad`, `Count`, `Remember`, `ResolveIncrease`, `ResolveOrdinaryRecast` and `Forget`.

With the game's settings (§2):

- an unknown member is dropped during deserialization, before `PostLoad` can see it;
- an unrecognized `AbilityScore` value goes through the registered `StringEnumConverter`, so it is expected to fail
  that object's deserialization rather than reach any per-record check.

A2 must establish both boundaries natively before choosing between a per-record raw envelope and a narrow compatible
reader.
