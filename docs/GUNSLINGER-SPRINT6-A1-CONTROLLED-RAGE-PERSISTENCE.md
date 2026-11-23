# Sprint 6 checkpoint A1: Controlled Rage persistence (RISK-03, first owner)

Status: **accepted by the second manual review (2026-11-22) for its stated Controlled Rage development scope**, at head
`f3f91386f1` (product `f9c2440651`, candidate DLL `e7afabe4…`, native session `20261122T143026Z`). Both first-review
findings are closed. The acceptance is a scoped development-review acceptance: nothing is merged or released, and
release qualification remains a separate boundary. RISK-03 stays **partial** until Brown-Fur, its second owner
(checkpoint A2, stacked PR), is reviewed. The first review found two defects; section 10 records the corrections and
their evidence. Records-only follow-ups after acceptance (this status, and the narrowed failure language in §4 row 8
and §10.2) did not change product bytes and did not require a rebuild.

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
- The rejected text is kept, unchanged, in the save. The log warns when a character's condition is first read after a
  load and again whenever the condition changes; unchanged repeated reads are silent (section 4, Diagnostic).
- A save that has kept rejected text but has no current allocation does not get a new default. The ordinary tier, and
  every tier the kept text mentions, stay blocked until the player chooses (section 10.1).

## 2. Engine contracts (Kingmaker 2.1.7b `Assembly-CSharp`, decompiled)

- `UnitPartsManager.PreSave` and `PostLoad` loop over the parts with no try/catch. A throwing part aborts the save, or
  stops that descriptor's load. The parts after it are then skipped, including their `EventBus.Subscribe`.
  `UnitDescriptor.PreSave` calls the parts first. `UnitDescriptor.PostLoad` calls them last, after the facts.
- `Fact.TurnOn` catches each component's `OnTurnOn` exception and logs it, so tier controllers failed per load, with an
  error each time.
- `DefaultJsonSettings`: `OptInContractResolver`, `TypeNameHandling.Auto`, `PreserveReferencesHandling.Objects`, and
  `MissingMemberHandling` at Newtonsoft's default (Ignore). Demonstrated natively: a JSON string value of arbitrary
  content in `_state` (malformed, duplicate, unknown-tier and JSON-looking text) reaches `PostLoad` unchanged, and so do
  the strings of `_rejectedStates`. Non-string JSON shapes in these members (an object, array or number) were not
  written or tested, so their deserialization behavior is not established. There is no typed-deserialization boundary
  for the string content itself. Older product versions ignore the new `_rejectedStates` member on read (section 7,
  Downgrade).

## 3. Released representation

`UnitPartControlledRageSelection._state` (string) uses the grammar `tier:STR,DEX,CON` joined by `;`, ordered by tier,
with tier 4 always present. Urban Barbarian first shipped in **v0.0.95**, together with this part (`96e7a4ca74`). The
format is unchanged up to this branch's base: `git diff v0.0.95 dfc182aee7` (the pre-A1 master revision, which
contains v0.0.193 `527f7e2fba`) is empty for `ControlledRageSelectionState.cs` and
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
| 7 | Repeated reads, callbacks, mutations and saves | Idempotent: the retained list is the same instance when nothing changes. The diagnostic is deduplicated against the last condition signature (section 4, Diagnostic) |
| 8 | Failed write | Both fields are computed before either is assigned. An explicit selection that is refused by validation, or whose marker addition returns null, restores the exact pre-call persisted values and leaves every marker as it was (staged synchronization, section 10.2). An exception thrown inside a marker addition or removal is not caught or rolled back by this owner |
| 9 | Null current state beside retained history | Not fresh absence: the ordinary tier and every tier a retained payload claims or taints stay blocked unless the current state holds them; retained text is never replayed (section 10.1) |

**Absent vs malformed.** Legitimate absence means a null `_state` with no retained payload. It keeps the released
ordinary STR +4 default. Every non-null string is classified. A null `_state` beside any retained payload is an
already-used part whose current allocation data is missing (section 10.1).

**Persisted form:**

- `_state` keeps its released name and type and holds the active tiers in the released grammar. It is null when no
  tier is active. That does not mean every tier is blocked: after the migration of `4:bad`, for example, only the
  ordinary tier is blocked and Greater and Mighty remain legitimately absent.
- The new optional `[JsonProperty] List<string> _rejectedStates` holds each rejected input verbatim.
  - An entry is appended once (ordinal comparison) and never edited or removed.
  - Blocking is re-derived from it on every read, minus the tiers that are active in `_state`.
  - It is null in every save that never held rejected input.
- Why a second field. `_state` must keep carrying legitimate later writes (selections, first unlocks) in the released
  grammar. Keeping rejected entries inside it would re-trigger rejection on every load, and would mix the player's new
  choice with conflicting raw entries.

**Explicit selection over a blocked tier.** Endorsed by the first review (2026-11-22); it is not replaced by any automatic default or arbitrary choice among conflicting entries. The selector of a blocked
current tier stays available, and choosing an allocation is a strictly validated new gameplay write. It replaces the
block, and the rejected input stays retained. The alternative is a tier blocked permanently, with no recovery path for
the player.

**Diagnostic.** `[urban-barbarian][controlled-rage.persistence.retained]` reports:

- the unit's name and unique id;
- active and blocked tiers;
- how many payloads are retained;
- for a newly rejected input: its length, a SHA-256 prefix and a 96-character printable preview.

Each part instance remembers only the last condition signature it reported (blocked tiers and retained-payload count).
Unchanged repeated reads are silent. A transition to a different signature warns again; for example a recovery, the
restore after a failed selection, or a new rejection. Each load creates new instances, which report on their first read.
It is therefore not one warning per character per load, nor once per distinct condition over an instance's life. The
native evidence shows the transitions (section 10.4).

**Fact synchronization.** `f4e5de5f92` added the selected fact before removing the tier's other facts, per tier. That
was incomplete across tiers (review finding 2). It is now staged over all tiers (section 10.2). It skips blocked tiers.

## 5. Commits on `codex/s6-risk03-controlled-rage-persistence` (base master `dfc182aee7`)

| Commit | Kind | Content |
|---|---|---|
| `bd9da01b9f` | product source and domain tests | total reader, retained field, explicit selection, snapshot rollback, diagnostic; 4 domain tests |
| `f4e5de5f92` | product source | add-before-remove in fact synchronization (**product-build commit**) |
| `11197b3d81` | harness, observer and session | `ControlledRageObserver`, `kmgControlledRage`, session `risk03-crage-persist` |
| `c7483ddca5` | session | conditions moved to launch level |
| `6eed8d7fed` | harness | post-save injection check fixed; product values restored once the save completes |
| `1a06ae206f` | records | this document, register rows, known issue (first review head) |
| `f9c2440651` | product source and domain tests | review repairs 1 and 2; new domain test; lifecycle test renamed (**corrected product-build commit**) |
| `f935b0446b` | harness and session | null-state recovery and failed-selection rollback cases |
| (this commit) | records | section 10 and corrections in this document, register row, known issue |

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
- `urban-barbarian.persistence-retained-lifecycle` (method renamed to `RetainedInputSurvivesOperationsAndResaves` in
  `f9c2440651`; its body never executed a failed write):
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
- Fixture: `KMCS_AFTER_NORMAL_012_MODONLY.zks`, disposable, copied into the session profile from
  `C:/Dev/KingmakerSkipCinematicLab/fixtures` (SHA-256 `0be64b3f58a0e0bea1b4951f0f6d145e27fd72e6da566acbf5b0b4aff8eda249`,
  769,309 bytes). The runner's receipts do not record it, so it was hashed afterwards. Its party is the main character
  only, in the prologue throne room. After each session the profile's copy reads `45ae50a4…` (original PASS) or
  `6ec251d1…` (corrected session), 769,308 bytes: the game rewrites it during the create launch. Resulting saves and
  receipts are listed in section 10.4.

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
   - **Narrowed after the first review, by reading the written saves (offline, no launch).** In the four saves of the
     two PASS sessions (`Manual_1`/`Manual_2` of `20261122T082442Z` and `20261122T143026Z`), no selection-marker GUID
     occurs, except twice in the corrected session's `Manual_2`: rollbackLater's Greater `4,2,0` marker. That unit's
     Greater controller fact (`GreaterDefault`) had been removed before the save. The tier selectors and the
     `GreaterDefault` feature are present in every save. So a marker whose tier controller is on the unit is not in the
     written file. After a reload, synchronization rebuilds markers for unblocked tiers only, so a blocked tier's marker
     is absent.
   - Product source has one marker remover outside synchronization: the tier controller's `OnTurnOff`, through
     `RemoveTier` and `ClearTierFacts`. The engine step behind the save-time absence is **not attributed**.
   - These facts are hidden markers with no components, so nothing is granted or lost mechanically. The retained raw
     input in the part is the recoverable record.
   - Consequence: the original forced failed write compared an empty fact set with an empty one, and it fails before
     a later tier is attempted. Its evidence is kept as it was. It is **not** coverage of the review's rollback cases;
     those are covered by the corrected session (section 10.4).
3. **Other limits.**
   - The synthetic corruption is test-only and labelled. No ordinary shipped state has been shown to produce
     malformed Controlled Rage data.
   - Level-up through the real level-up UI was not used. Features were granted directly, and that grant runs the
     production tier controllers.
   - Mighty tier was not natively exercised. It is covered by the domain tests.
   - The explicit-selection-over-a-block behavior was endorsed by the first review (§4).
   - Downgrade, **inferred from the v0.0.193 source and not executed**. The old reader (`dfc182aee7`
     `UnitPartControlledRageSelection.Read`) treats a null or whitespace `_state` as fresh and creates the ordinary
     STR +4 default. It ignores `_rejectedStates`. A non-empty `_state` goes to the strict `Parse`, which throws when
     the ordinary tier is missing or an entry is malformed. A migrated save would therefore behave under the old
     version as follows:
     - a null `_state` gets a new STR +4 allocation;
     - a `_state` such as `6:2,2,2` (ordinary blocked beside an active Greater) throws in the unguarded callbacks;
     - a `_state` with the ordinary tier present loads normally.
     The old writer does not write the unknown `_rejectedStates` member, so the kept text would be lost on its next
     save (also inferred).
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

## 10. First review corrections (2026-11-22)

The review of head `1a06ae206f` endorsed the explicit-recovery interaction: a deliberate, strictly validated selection
through the current tier's selector clears that tier's block and keeps the rejected text. It found two defects, and
seven record statements to correct. Corrected product commit `f9c2440651`; harness `f935b0446b`.

### 10.1 Repair 1: null current state beside retained history

**Defect.** `Restore(null, retained)` treated the part as fresh whenever the retained payloads tainted no tier. The
review's inputs, all reproduced before the repair (10.3):

- A: `_rejectedStates = ["4:0,4,0;4:0,4,0"]` restored as a new `4:4,0,0`;
- B: `["4:0,4,0;10:4,4,2"]`, the same;
- C: `["4:0,4,0;6:bad"]` blocked Greater only, and the next ordinary `Unlock` created `4:4,0,0`.

**Rule now.** A retained payload means the part was already used.

- With any retained payload, these tiers are blocked unless the current state holds them:
  - the ordinary tier;
  - every known tier that a payload claims (any `k:` entry with k in {4, 6, 8});
  - every tier a payload taints.

  This holds whether `_state` is null or not.
- Retained text only blocks; it is never replayed as an allocation.
- With no retained payload, a null `_state` is fresh absence and keeps the first-unlock default.
- A tier that no payload mentions stays legitimately absent. For example, `4:bad` migrates to a null `_state` and
  `["4:bad"]`, and Greater and Mighty still get their defaults on unlock.

Released and migrated states read exactly as before, because in every flow the product itself produces, a claimed tier
is already active or tainted. The lifecycle and fuzz reload tests pass unchanged. Explicit recovery is unchanged.

**Player-visible consequence.** On such a save, the character's Controlled Rage grants nothing for the blocked tiers and
no STR +4 appears. Choosing an allocation with the tier's selector restores that tier, and later saves keep the choice.

### 10.2 Repair 2: complete rollback of a failed explicit selection

**Defect.** `TrySelect` restored the unit-part snapshot after synchronization failed, then synchronized again from the
restored part. That second pass skips blocked tiers. The review's scenario:

1. The ordinary tier is current and blocked.
2. A valid Greater selection is saved, but its marker is absent (after `RemoveTier`).
3. The ordinary addition succeeds, then the Greater addition fails.
4. The part rolls back and ordinary is blocked again, so the second pass skips it.

The new ordinary marker stayed in place and the previous one stayed removed. The markers are inert hidden facts, so
this was an incomplete failed-selection transaction. It was not a demonstrated wrong rage bonus or a lost rejected
payload.

**Repair.**

- `SynchronizeSelectionFacts` is staged.
  - It first adds every missing selected marker for the unblocked tiers.
  - Only when all additions succeed does it remove stale markers.
  - A failed addition removes exactly the instances this call added and returns false, so no marker of any tier
    changes. (`UnitHelper.RemoveFact(UnitDescriptor, Fact)` removes the given instance; decompiled 2.1.7b.)
- `TrySelect` captures the part snapshot before its `Unlock`. When the selection is refused (`TrySelectExact` returns
  false) or a marker addition returns null, it restores the snapshot and does not synchronize again. These are the two
  supported failure paths. An exception thrown by the engine inside `AddFact` or `RemoveFact` propagates; it is not
  caught, and the rollback does not cover it.
- `ResolveSelection` and `UnlockTier` get the same all-or-nothing marker behavior. Their success paths are unchanged.

### 10.3 Focused regressions

| Command | Head | Selected | Result | Duration |
|---|---|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'urban-barbarian.persistence'`, new test, product unrepaired | worktree of `1a06ae206f` | 5 of 3,168 | **FAIL**, 1 failure (intended reproduction) | about 4 s |
| `scripts/Invoke-KmgGate.ps1 -Filter 'urban-barbarian'`, both repairs | worktree before `f9c2440651` (product identical except one comment) | 15 of 3,168 | PASS, 0 failures | about 23 s |
| `Build-Local.ps1` selected tests | `f9c2440651` | 15 of 3,168 | PASS | part of the build |

The reproduction named each failing input:

- A, B and the fully claimed history `4:0,4,0;6:2,2,2;8:2,2,4` each restored as a new allocation `4:4,0,0`;
- for C, the ordinary unlock created `4:4,0,0`.

The new test is `urban-barbarian.persistence-null-state-beside-history`. It drives the production `Restore`,
`Serialize` and `MergeRetained` cycle:

- **Four read/unlock/write cycles per case.** Each cycle checks that:
  - no tier is active and the ordinary unlock is refused;
  - the blocked tiers are exactly as expected, and each refuses a default;
  - `_state` stays null;
  - the retained list stays the same instance, with unchanged content.
- **Explicit recovery, tier by tier.** Each recovery is checked over three more cycles with ordinary unlocks, and the
  retained text stays unchanged.
- **Controls.**
  - Fresh null, with no list and with an empty list, gets the default `4:4,0,0`.
  - The `4:bad` migration blocks only ordinary; Greater and Mighty keep their defaults over three cycles.
- **Reporting.** Every case runs before the test reports, so one failure message names each failing input.

Repair 2 has no domain test, because the domain tests cannot load `Assembly-CSharp`. Its evidence is native (10.4).

### 10.4 Corrected candidate and native session

**Candidate.** Built once at detached `f9c2440651`, with the section 6.2 command.

- **First attempt: BLOCKED, not FAIL.** It stopped after 181 s. `Test-Sprint32DevelopmentPersistence.ps1` refuses to run
  while any Kingmaker process exists, and a peer session's agreed live launch started during the build. Its residue is
  kept as `f9c244065147.failed-20261122T1422596321330Z-0a8364a1`.
- **Rebuild: PASS in 426 s**, in a no-Kingmaker window.
  - Strict standalone UMM package validation passed; 15 of 3,168 selected tests passed.
  - Package `KingmakerGunslinger-0.0.193-development-risk03-controlled-rage-persistence-f9c244065147.zip`, SHA-256
    `6bc93163924c57fdfeccd9a880ce7ff8b90e7724b0ae7a4b3486271fbaa8118e`.
  - DLL SHA-256 `e7afabe491ad2b5ac58c81a658bc73f20773256247321be3cf5405c29ce3ad96`.
- **Harness DLL** `cfab28c86cccba7f1046f7795e04ff396b5ade3ab79b8e7eae80d3fb58324610`, from the `f935b0446b` sources,
  built with `Build-LoaderCompatHarness.ps1` into `artifacts/loader-compat-harness-risk03-r1`.
- No deployment to the human installation.

**Session `20261122T143026Z-owner-0.33.0-risk03-crage-persist`: PASS.** Create 78 s, reload-mutate 76 s, final 73 s;
242 s total.

- Same runner, profile and fixture as section 6.3; the session git is `f935b0446b`. It ran in an agreed no-Kingmaker
  window.
- Lease `runtime-20261122T143027Z-1922c8da14d048f4837a1ffd09140cbf`, completed. Transaction
  `63f45d0660944a28bf2fe898135226e2`.
- Registry readback verified (`25f28ffd…7514`, equal to the backup).
- Owner saves (95) and the install are unchanged. No host conflict.

**Identities.** Raw saves and logs stay machine-local and are not committed.

| Item | Session `20261122T082442Z` (original PASS) | Session `20261122T143026Z` (corrected) |
|---|---|---|
| Source fixture | `0be64b3f…a249` (769,309 bytes) | same |
| `Manual_1_risk03cr1.zks` | `2c2fa8b820de4518c9f043082227f60fc69377e2256a0153c8c2c01250248e6f` (790,619 bytes) | `5d6ae663daf351663d6633820894fd0d834c2406223843bee357221e8db6263a` (488,389 bytes) |
| `Manual_2_risk03cr2.zks` | `a4a56b3fd1a06d282067d442b81a6c2e733cc75077085211cff6996859197560` (799,869 bytes) | `b1347487f95c645bec0ec6b11656068b9853a9b44f3c78ec5b3e72f56c56d025` (522,472 bytes) |
| `session-summary.json` | `47227f2c29c9d89cf1e3ddb99dba6af82a051b3f87ce5011baf65a8b7db05761` | `1761e768590b5950b192694b87ad595ae243dea16027f500340e715e89226008` |
| Evidence root | `runtime-evidence/loader-compat/20261122T082442Z-owner-0.33.0-risk03-crage-persist` | `runtime-evidence/loader-compat/20261122T143026Z-owner-0.33.0-risk03-crage-persist` |

**Preserved checks.** Every original case ran again on the corrected bytes, with the same expectations, and passed:
valid state, the four synthetic save injections, migration, rage, repeated reads, mutations, the original forced
failed write and the final reload. The original forced failed write now also records its `AddFact` attempts: exactly
one, `4:2,2,0 forced`.

**Repair 1, natively.** Three disposable units were built through the real grant and allocation ability (ordinary
`0,4,0`). After the product's `PreSave`, the observer wrote **SYNTHETIC** `_state = null` with
`_rejectedStates = [A | B | C]`.

- **Fresh process, native `PostLoad`:** 3 of 3 completed. Each read null and its payload, and wrote null and the
  identical payload.
- **After load:**
  - blocked tiers were A `4`, B `4`, C `4,6`;
  - no allocation and no marker, and raging granted nothing;
  - three rounds of `ResolveSelection` and `UnlockTier` changed nothing;
  - the log held `activeTiers=none;blockedTiers=4;…;newlyRejected=none` and `…blockedTiers=4,6;…`.
- **Explicit recovery** through the ordinary allocation abilities (`Success`): A `4:2,2,0`, B `4:0,2,2`, C `4:0,0,4`.
  Each kept its retained payload verbatim.
- **C's later Greater grant.** C then received Greater Rage, and Greater stayed blocked:
  - `_state` stayed `4:0,0,4`, with no Greater marker and one selector;
  - raging at Greater granted nothing;
  - the Greater ability then recovered `4:0,0,4;6:2,2,2`.
- **Final fresh process.** `PostLoad` read and wrote exactly these values, and the payloads were unchanged. Markers
  were rebuilt for the active tiers. Rage:
  - A: STR +2, DEX +2;
  - B: DEX +2, CON +2;
  - C: +2 to each, at Greater.

**Repair 2, natively.**

- **Setup.** In the reload process, a **SYNTHETIC** in-memory `_state` blocked the ordinary tier while the
  gameplay-created ordinary marker `4:0,4,0` was present. The setup is in-process because a blocked tier's marker
  cannot survive a reload (section 7.2).
- **Forced failure.** `UnitHelper.AddFact` was patched to return null for one named marker, and every `AddFact` call
  while armed was recorded. `ControlledRageRuntime.TrySelect` was then called.

| Case | Setup | Recorded `AddFact` attempts | Returned | Persisted values, before = after | Selections, blocked, markers, selectors, before = after | Rage, before / after |
|---|---|---|---|---|---|---|
| present marker (`rollbackPresent`) | `_state` `4:0,4,0;4:4,0,0;4:0,4,0`; ordinary blocked; marker `4:0,4,0` present; select `2,2,0` | `4:2,2,0 forced` | false | null + `["4:0,4,0;4:4,0,0;4:0,4,0"]` | blocked `4`; markers `[4:0,4,0]`; 1 selector | nothing / nothing |
| later tier (`rollbackLater`) | Greater Rage removed (`RemoveTier` cleared the Greater marker and kept `6:4,2,0`); `_state` `4:0,4,0;4:4,0,0;6:4,2,0`; ordinary current and blocked; marker `4:0,4,0` present; select `2,2,0` | `4:2,2,0 added`, `6:4,2,0 forced` | false | `6:4,2,0` + `["4:0,4,0;4:4,0,0;6:4,2,0"]` | Greater `4,2,0`; blocked `4`; markers `[4:0,4,0]`; 1 selector | nothing / nothing |

- **The injection fired as intended.** In both cases the forced failure fired once, at the intended marker. In the
  later-tier case the ordinary addition succeeded first, so the staged rollback did real work. Raging afterwards
  changed nothing.
- **Then a plain explicit selection** succeeded through the allocation ability. Each kept the synthetic text as its
  retained payload:
  - present marker: `4:2,2,0`, markers `[4:2,2,0]`;
  - later tier: `4:2,2,0;6:4,2,0`, markers `[4:2,2,0, 6:4,2,0]`.
- **Final fresh process.** `PostLoad` read and wrote those values; rage was STR +2 / DEX +2.
- **Pre-repair behavior is inferred only.** From the source, the unrepaired product would have left `[4:2,2,0]` in the
  later-tier case. No build of the unrepaired source ran these new cases.

**Diagnostics (reload log).** Deduplication behaves as section 4 describes:

- `…blockedTiers=4;…;newlyRejected=none` appeared four times: A and B at load, and the restored state after two failed
  selections;
- `…newlyRejected=length=23,` appeared once for each rollback unit;
- the four original `once` needles appeared once each;
- in the final launch, `newlyRejected=length=` appeared zero times and `newlyRejected=none` nine times.

**Exceptions.** No engine-logged exception named Urban Barbarian code.

- Each launch has the native `BugReportCanvas.OnEnable` NullReferenceException.
- The final launch also logged a native `GraphicsSettingsController.SetResolution` ArgumentOutOfRangeException while
  the main menu's settings initialized, before any save loaded. It has no product frame and did not occur in the
  earlier sessions. It is recorded, not attributed.

### 10.5 What the corrected checks establish, and limits

- **Established** under the real serializer, native callbacks and fresh processes:
  - null current state beside retained history (A, B, C) does not become an allocation through `PostLoad`, reads,
    unlocks, raging, a later Greater grant, or resaves;
  - explicit recovery works per tier and persists;
  - a failed explicit selection leaves the persisted values, blocked tiers, selections, markers, selectors and rage
    unchanged, both with a present prior marker and after a later tier's failure;
  - every original case still passes on the corrected bytes.
- **Not established.**
  - Mighty was not exercised natively.
  - The corruption and the in-memory blocking are synthetic.
  - No pre-repair candidate ran the new native cases.
  - Downgrade stays inferred (section 7).
  - The save-time marker absence is unattributed (section 7.2).
- **Retained, unchanged.** The two earlier FAIL sessions and the original PASS keep their identities. The original PASS
  ran `f373e027…`, not the corrected DLL.
- **Deferred by policy.** Complete suite, release packaging, mod-stack profiles, whole-roster persistence and the human
  installation.
