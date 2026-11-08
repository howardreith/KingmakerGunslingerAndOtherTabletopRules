# Summoned Creature Manual Playtest Repair — R5 status

**R5 review repairs (2026-11-08, commit `06d9bc4d0e`): three review findings
repaired; review checkpoint.** The three findings are below. The original R5
record follows unchanged. The 22/0 native result belongs only to tested source
`366fad0ac41b271564cf394e6de952e6a4c68591`. The repaired product binary is new
and has **no native observation**; the PASS does not transfer to it.

### Finding 1 — optional spell binding was not atomic (repaired)

- **Defect:** both Call of the Wild binders copied the donor's fields into the
  already-granted owned ability and set `Hidden = false` before component
  cloning and the provider guard were complete. Dance's one-use cost was
  attached at that same late step. A failure was logged as an omission, but
  the placeholder was not restored: the ability could stay visible with donor
  components and no guard, and Dance could lack its cost.
- **Repair** (`ExpandedSummoningSpecialBuilder.SpellsR5.cs`, `R5CotwSpells.cs`,
  `SummonPlaytestR5Policy.BindOptional`):
  - Everything is prepared before the owned ability changes: provider lookups,
    contract checks, cloned components, the guard, caster parameters and
    Dance's one-use cost. Dance also refuses a provider cast that carries its
    own resource cost.
  - The replacement is then published still hidden.
  - It is verified: hidden, exactly one guard for this unit and spell, and for
    Dance exactly its one-use cost.
  - Visibility is the last step.
  - Any failure after the owned ability was touched restores a snapshot of the
    placeholder. A `finally` forces the stable name, `Hidden = true` and the
    placeholder's own guarded component array.
  - The binder returns a disposition (`active`, `provider-contract-failed`,
    `placeholder-restore-failed`) and never throws once the owned identities
    resolve.
  - `MistBound` and `DanceBound` (the only state that opens the guard) are set
    only for `active`.
  - Mist and Dance stay independent. On the successful path the spell
    mechanics, caster parameters, native action costs, ownership, identities
    and Dance's once-per-summoning frequency are unchanged.
- **Focused tests:**
  - `summon-playtest-r5.binding-failure-restores-placeholder` drives
    `BindOptional` with a model that follows the binders' steps and injects
    failure at:
    - preparation;
    - the donor field copy;
    - publication;
    - a missing guard;
    - a missing Dance cost;
    - reveal;
    - a failed snapshot restore combined with a publication or reveal failure.

    Each case checks that the ability is hidden, guarded and unavailable, that
    no unrestricted Dance path exists, and that the placeholder's components
    and stable identity are restored. A failed restore retains both errors.
  - `summon-playtest-r5.bindings-independent`: a failure of either binding
    leaves the other bound. Source contracts pin the production path:
    - visible publication happens only in the two `reveal:` steps;
    - both binders use `BindOptional`;
    - Dance's one-use cost appears only in its placeholder and its prepared
      replacement;
    - the Dance binding is attempted whatever Mist's outcome;
    - a guard opens only for an `active` binding.
  - Mutation check: with the restore call removed, both tests failed. Restored
    and re-verified.
- **Limit:** the binding failure path is established by the model plus source
  contracts, not by an in-game provider failure. The repaired success path was
  compiled, not natively observed.

### Finding 2 — UMM recovery ignored transaction ownership (repaired)

- **Defect:** `Restore-UmmParams.ps1` checked only that Kingmaker was absent. A
  receipt is pending from before launch, so recovery could complete it while
  the original wrapper was still staging.
- **Repair** (`UmmParams.Common.ps1`, `Restore-UmmParams.ps1`, the wrapper):
  - **Owner identity in the receipt (schema v2):** the wrapper creates the
    receipt while holding its own runtime lease from the shared coordination
    lock (`Enter-KmgRuntimeLease`, purpose `umm-params protection`). The
    receipt records that lease (run ID, state path, coordination root) and the
    owner's exact process identity (PID and start time, the identity runtime
    leases use).
  - **Owner-only in-place completion:** only that live owner completes the
    receipt in place.
  - **Serialized recovery:** `Invoke-KmgUmmParamsRecovery` takes its own lease.
    Entering it fails while any transaction holds the lock, and no lock is
    stolen or cleared. While holding the lease, recovery refuses if Kingmaker
    is running or the original owner process is alive. It then restores,
    records `restoredBy = recovery` and its lease ID, and releases the lease.
  - **Legacy and failure handling:** a pending v1 receipt (no owner identity)
    is refused. Completed receipts verify idempotently. Failures keep the
    receipt pending with `lastError` and the attempt count.
- **Tests** (`Test-UmmParamsProtection.ps1`, real temporary leases and a real
  child owner process): **52/52 PASS**, about 4 s. Cases:
  - live original owner with no game: refused by function and by script,
    bytes and receipt unchanged, lock released;
  - foreign active lease: refused, the foreign lock is not stolen, bytes and
    receipt unchanged;
  - a running process name: refused;
  - non-owner in-place completion: refused, and the receipt is not marked
    restored early;
  - interrupted owner: restored, recovery recorded, lock released,
    idempotent repeat;
  - recovery through the shipped script;
  - all earlier cases: timestamp, profile, unchanged, scenario failure,
    missing or corrupt backup, locked destination and retry, absent intake,
    unrecordable backup, and a released lease refusing backup.
- Now wired into `validate-repository.ps1`, which runs at sprint boundaries.

### Finding 3 — cleanup independence in both directions (repaired)

- **Defect:** the wrapper's `finally` began with an unguarded
  `Get-KmgTreeFingerprint`. An exception there skipped the later UMM step.
- **Repair:** the complete installation inspection and restoration (the first
  fingerprint included) is now the installation step of
  `Invoke-KmgIndependentInstallationAndUmmRestoration`. The UMM step runs
  whatever that step did, and each failure is retained. The wrapper now
  refuses PASS (it throws) when the live installation is not verified or not
  needed, as well as when UMM restoration fails.
- **Tests** (same file):
  - an installation hash failure while `Params.xml` differs from intake: the
    UMM step still restores the intake hash, and the installation failure is
    reported;
  - a UMM failure in a real child owner (destination locked): the
    installation step still ran with its result kept, the receipt stays
    pending with its error and an intact backup, and recovery restores the
    bytes after the owner exits;
  - both steps failing: both errors kept;
  - a running game defers only the UMM step;
  - a source contract that the wrapper's `finally` reaches the independent
    step before any fingerprint, backs up under its lease, and refuses PASS
    on unresolved restoration.
- Orchestration tests: 297 assertions pass. All changed scripts parse cleanly.

### Verification and identities (review repairs)

- **Sources:** review head `42ec35d1a6` → repairs `06d9bc4d0e`.
- **Focused gate:** production compilation passed; R5 filter **149/149**
  (2,871 registered; nonzero; R1–R5 groups plus the identity and ledger
  suites).
- **Not done:** no package rebuild, no Kingmaker launch, no native phase.
  Residual risk: the repaired binder's success path (same final state, new
  ordering, one added Dance contract check) is not natively observed on a new
  binary. The minimum observation, if wanted, would be the existing
  `summon-playtest-r5-spells` loaded-configuration cell on a new candidate. It
  was not selected.
- **Spend:**
  - UMM tests about 7 s (including one harness fix);
  - orchestration about 1 s;
  - three focused gates 24 + 16 (mutation) + 22 s;
  - **about 72 s in total.**
  - R5 cumulative is about **1,354 of 1,800 s**.

---

# R5 status as first reviewed (2026-11-08)

**R5 COMPLETE FOR REVIEW (2026-11-08).** SPELL-01..03 and MECH-01..02 are
technically established on DATA within the limits listed below. Owner
aesthetic approval is a separate, pending judgment. Nothing was merged,
released or installed.

- Branch `codex/summon-playtest-r5-spells`, stacked on draft PR #114
  (`codex/summon-playtest-r4-mephit`, head `294e7deed5`, open). The R5 PR
  targets that branch and depends on #114.
- R4 is retained as reviewed: original candidate `a1e6c264193f` (26/0);
  finishing candidate `15f9f68a976a…` (17/0, package `a5c1d4b7…d555`, DLL
  `2928e004…dc25`, MVID `175dc344-f882-4f83-a2ed-a35047de98f7`). R4 ledger
  stays at 1,135 s.
- **R4 restoration qualification (kept visible):** after R4 the KMG install,
  settings and saves were restored and the enabled profile was unchanged,
  but UMM `Params.xml` did not return to its R4 intake hash; an older backup
  differed only in `LastUpdateCheck`, so exact restoration of that file was
  not established. No backup matching the full R4 intake hash exists, so no
  historical bytes were restored and no timestamp was invented. R5 uses the
  current file (`e44f4495…62630bb`, 3,077 bytes) as its intake.

## (1) UMM Params.xml protection (before any native launch)

- `scripts/UmmParams.Common.ps1` (shared, dot-sourced by the actual guarded
  DATA wrapper `Invoke-ExpandedSummoningRuntimeScenario.ps1`; reuses
  `Write-KmgRuntimeAtomicBytes`):
  - resolves `<game>/Kingmaker_Data/Managed/UnityModManager/Params.xml`;
  - before staging/launch records existence and full SHA-256, writes an
    exact byte copy plus a durable `KmgUmmParamsReceipt` (state `pending`)
    under `runtime-backups/umm-params/<UTC>-<guid>/`, verifies the copy, and
    **refuses launch** if any step fails or a pending receipt already exists;
  - after Kingmaker exits (success, failure, or wrapper `finally`) restores
    bytes atomically only if the hash differs, verifies against intake, and
    marks the receipt `restored`; on failure keeps the receipt pending,
    records `lastError`/attempts, and makes the wrapper throw so restoration
    is never reported as PASS;
  - byte preservation only (no XML reserialization); restoration is an
    independent cleanup step (it never blocks the live-mod restore and vice
    versa).
- `scripts/Restore-UmmParams.ps1`: interruption recovery from retained
  receipts (refuses while Kingmaker runs).
- `scripts/Test-UmmParamsProtection.ps1` (temp files, real shared
  functions): **25/25 PASS** — path resolution; changed timestamp; changed
  profile content; unchanged file; scenario failure restored in `finally`;
  missing backup (fails, writes nothing, stays pending, launch refused);
  corrupt backup; locked destination then retry (attempts=2); independent
  cleanup; absent intake; unrecordable backup root refuses launch; recovery
  script. Sprint40 laptop-restricted protection was not reused or renamed.
- Committed `5c20f8e978` before any R5 launch.
- Native receipts this sprint (both `restored`, `rewritten=false`,
  attempts 1, restored hash = intake `E44F4495…62630BB`):
  `20261108T0648412731796Z-f5963ade…` (first run) and
  `20261108T0659464055548Z-d4c45e20…` (second run). UMM did not rewrite
  `LastUpdateCheck` during these launches.

## Dispositions

### SPELL-01 Cloud Giant: one real Obscuring Mist SLA — changed

- Provider: Call of the Wild (UMM id `CallOfTheWild`, loaded 1.14.4c-2.1,
  MVID `8caab254…6e53`), `ObscuringMistAbility` `4acf55ad7fec47478ded06d9e929fa06`
  (created in `NewSpells.createObscuringMist`; a copy of native Stinking
  Cloud): Medium range, target point, 20-ft spawn of `ObscuringMistFogArea`
  `0aee4360e6a34950b7baff11658aeb14`, 1 minute/level, Standard action, no SR,
  icon CloudForm. (Vanilla Kingmaker's own Obscuring Mist `15ba44b3…` is a
  different, short-range spell; the owner's E06 report matches the CotW one.)
- The existing owned identity `KMG.Summoning.Giant.Sprint29.CloudObscuringMist`
  (`9ea1c943…`) is reused as the single owned at-will SLA wrapper: bound late
  (first UMM update after bootstrap) by copying the provider's fields and
  deep-cloning its components (minus the spell-list entry), plus the
  provider guard and spell-like parameters (caster level 16, spell level 1,
  Charisma). The provider blueprint is not mutated.
- Removed: the custom Fog Cloud grant, its resource and its AI route; the
  old Mist route now references the provider-backed SLA once (owned AI
  action `CloudMistAi`, a fog-safety consideration retargeted to the CotW
  area). Saved giants: `SummonWithdrawnCapabilityFixer` spec `cloud-giant`
  retires exactly the Fog Cloud fact, AI action and resource; the Mist
  identity is kept.
- Provider absent or not exact/active: the wrapper stays a hidden
  placeholder whose caster checker refuses use; no substitute.

### SPELL-02 Pixie: four at-will spell-likes — changed

- Owner adaptation (documented, not a balance decision of ours): the
  printed Pixie has these as daily-limited and uses Lesser Confusion; the
  owner asked for **at will** and **full Confusion**.
- Owned SLA wrappers cloned from the actual native spells (caster level 8,
  Charisma, the spell's own level):
  - Entangle `0fd00984…` → `KMG.Summoning.Special.Pixie.Entangle`
    (`5a324eff…`); owned AI (3-round rest between casts).
  - Confusion `cf6c901f…` → `…Pixie.Confusion` (`5d7e893c…`); AI copied from
    the native Nixie Confusion action `c0341a72…`.
  - Dispel Magic `92681f18…` with both native variants (`143775c4…` target,
    `9f6daa93…` area) → `…Pixie.DispelMagic` (`c55c4961…`) with owned
    variants `…DispelMagicTarget` (`e9d58265…`) and `…DispelMagicArea`
    (`e8e7d42f…`); AI copied from native `13639c82…` with the variant
    remapped (fails closed otherwise).
  - Shield `ef768022…` → `…Pixie.Shield` (`a1692bed…`); AI copied from
    native `MageShieldAiAction` `8df9d860…`.
- No resource, native action type (all Standard), native targeting, saves
  and effects.

### SPELL-03 Pixie Dance: real Irresistible Dance — changed

- Provider: Call of the Wild `IrresistibleDanceAbilityCast`
  `7733b3550b4f03002c59bc590e07d9cc`, a sticky-touch cast whose
  `TouchDeliveryAbility` is the CotW touch spell `fad6a06a…` (Will partial,
  1d4+1 rounds, buff `4d283e0b70fb489ba79e69387818c3f3`).
- The existing identity `KMG.Summoning.Special.Pixie.IrresistibleDance`
  (`5cacf6a7…`) replaces the custom substitute: it clones the cast
  components and keeps the existing **one use per summoning** cost (not at
  will), CL 8, spell level 6. The sticky touch runs the touch spell with
  the Pixie's own ability data.
- Provider absent: hidden placeholder, no substitute. Shared CotW setup is
  untouched.

### MECH-01 Griffon "two charges" — duplicate removed

- Cause: the engine's `UnitDescriptor` already adds the native
  `ChargeAbility` (`c78506dd…`) to every unit; the KMG Griffon also granted
  that same blueprint explicitly, so two identical Charge actions appeared.
- The explicit grant is removed (profile, builder and fact map) and a build
  guard refuses it. Pounce, attacks, reach and resources are unchanged.
- Saved Griffons may still show the second instance until they expire (the
  withdrawal fixer would remove both copies, so it is not used).

### MECH-02 Mastodon trample — added

- Shared qualified ungulate carrier with the Mastodon's own row (14 HD,
  Strength 34): 2d8+18 bludgeoning, Reflex DC 29 half, full-round, once per
  target per round, with the interrupt and duplicate-response contract
  (`ContextActionUngulateTrample.ExactTrampleAbilityNames`).
- New identity `KMG.Summoning.Special.Mastodon.Trample`
  (`a1f52aace5124004b76e2351c7023566`). Elephant (2d8+15, DC 25) and Aurochs
  rows are unchanged. No AI action (commanded only, as with the others).

## Identities

11 append-only identities (catalog 804 → 815; manifest allocated and
activated; icon-catalog registry hash updated): Mastodon trample; Pixie
Entangle, Confusion, Dispel Magic (+2 variants), Shield; four Pixie AI cast
actions. Legacy Cloud Fog Cloud fact/AI/resource and the Pixie Dance state
identities are retained (no GUID reuse or deletion).

## Validation

### Planned observations (before the run)

Loaded configuration; live Griffon Charge count; commanded Mastodon
trample; Cloud Giant routes on a live unit; two commanded Mist casts at
target points; each Pixie spell-like twice; one Dance with its resource;
seven native description windows; owned UI and fixture restoration.

### Focused tests

- Filter (selected 147 of 2,869 registered; nonzero; nested wrappers
  inspected): R1–R5 playtest groups, Sprint 5/6-Pixie/11/23/25/26/27/29
  summoning, inventory, identity/manifest ledgers, native admission, and
  the identity-tail suites of other features. **147/147 PASS**, production
  compiled (Focused gate).
- New registered cases `summon-playtest-r5.*` (5): Pixie native spells,
  provider gate dispositions, Mastodon values (+ Elephant/Aurochs
  preserved), Griffon single Charge, Cloud Fog route retired. Native
  admission test extended for `summon-playtest-r5-spells` (fresh, save-free,
  outdoor Default, not Wall, not Sprint40); PowerShell admission accepts it
  and rejects a bogus phase.
- Candidate builder selection: 29 of 2,869 (filter in the receipt).

### Candidates and native runs (one guarded DATA phase each)

1. `73bfc8ca97a9` (package `96a70df9…bd5b8`, DLL `5af7c6eb…907b`, MVID
   `087def59-…5273`): **FAIL (observer defect)**. Loaded configuration
   PASS; the observer then requested native Pounce as `BlueprintUnitFact`
   through an exact-type lookup (it is a `BlueprintFeature`). Evidence kept.
   Restoration exact. Fixed in the observer only (plus R3's owned-window
   scroll fallback and ending owned areas between cells); product unchanged.
2. `366fad0ac41b` (commit `366fad0ac4`; package
   `8862c4859a880adca80e82154952a010e62fa64b9718d508e2feb45de51b3af7`, DLL
   `d5adb10c279877658cc2a86d4880845361acf0497f0c878270a329bbedeee936`, MVID
   `2e3f7440-2da3-4e68-b618-4e55c81fbe59`): **22 PASS / 0 FAIL**.

### Native results (candidate 2)

Evidence class per cell: **loaded config** = live blueprints after
bootstrap; **commanded** = `UnitUseAbility` issued by the observer with
brains inactive; AI-action references are configuration only (no
autonomous AI cast observed); saves on the disposable hostile are
**controlled failures** (seeded d20); the Dance touch is a **controlled
natural 20**. No induced legacy state and no cold load were exercised.

- `loaded-configuration`: provider active; Mist and Dance bound; Cloud
  Giant grants Mist once and Fog Cloud zero times, brain references Mist
  once and Fog zero times; Mist visible SpellLike, Medium, spawns the CotW
  area; Dance visible, one resource cost, touch = CotW touch spell; Pixie
  grants and brain-references Dance + four spell-likes; spell-likes have no
  resource cost; Mastodon grants trample; Griffon has no explicit Charge.
- `griffon-one-charge`: live Griffon has exactly 1 Charge; Pounce present.
- `mastodon-native-trample-command-mastodon-rtwp`: queued full-round
  trample moved 5.09 m, one contact, Reflex DC 29 (failed, controlled),
  2d8+18 bludgeoning (28 dealt), no attacks of opportunity, native economy.
- `cloud-custom-routes-removed`: live giant has 1 Mist fact; brain
  `CloudMistAi`, `AttackAiAction` only.
- `cloud-mist-target-point-at-will`: two commanded Standard-action casts;
  each produced exactly one CotW mist area at the chosen point (0.0 m from
  the point, away from the caster); 1 then 2 live areas; available after
  each.
- `pixie-four-at-will`: Entangle (point) ×2, Confusion (hostile) ×2,
  Dispel Magic targeted variant ×2, Shield (self) ×2; all Standard, native
  action type matched, resource-free, available after each; Confusion's
  buff on the hostile and Shield's on the Pixie observed. Save DCs seen:
  Entangle 14, Confusion 17 (all controlled failures). The area variant of
  Dispel was configured but not executed.
- `pixie-dance-provider`: cast + automatic sticky-touch delivery (controlled
  natural-20 touch), CotW dance buff on the hostile cast by the Pixie, Will
  DC 19 (controlled failure); resource 1 → 0; then unavailable.
- Seven `render-*` windows (each inspected personally) — name, icon,
  "Spell-like Ability"/"Extraordinary" type, own Duration/Saving Throw rows,
  no `<null>`: Obscuring Mist (CloudForm art; 20-ft point, medium range;
  1 minute/level), Irresistible Dance (CotW dancing-figure art; 1d4+1
  rounds; Will partial), Entangle, Confusion (full Confusion text), Dispel
  Magic (parent window), Shield (Personal), Mastodon Trample (Longstrider
  art; Reflex DC 29 half; text 2d8+18).
- Cast frame `r5-cloud-mist-point`: shows the Cloud Giant casting (floating
  "Obscuring Mist", casting ring). The mist cloud itself is at most a faint
  haze in that frame; area placement rests on the structured position, not
  on the image.
- `owned-ui-restored`, `fixture-restored`, native-profile restored: PASS.

### Limits and gaps (named)

- **Provider-absent omission not observed natively.** The installed DATA
  profile has Call of the Wild active; observing absence would need the
  compatibility-profile runner, which lacks Params.xml protection and can
  change the enabled profile. Covered only by domain tests of the gate and
  the hidden, caster-checked placeholder configuration.
- Saved legacy state (Cloud Giant fixer; old Dance substitute; Griffon's
  second Charge) was not induced or cold-loaded.
- No autonomous AI cast was observed for any new route.
- The Dispel Magic area variant was not executed.
- Shared Trample template text (pre-existing, also Elephant/Aurochs): the
  target row reads "Target point within 0 ft. feet." and the school row
  "None". Not changed in R5; recorded for the owner.
- Combat log names the fixture caster "-unit name not set-" (pre-existing
  fixture identity).

### Restoration (both runs)

KMG live tree restored to `5C3E4AD4…FBBB` (installed 0.0.140 before and
after); 66 native settings restored; 95/95 protected saves byte- and
metadata-unchanged; UMM `Params.xml` restored-verified at intake
`E44F4495…62630BB` (receipts above). KMG_AUTOMATION_BASELINE untouched; no
campaign save written. Raw evidence and screenshots stay local under
`runtime-evidence/` (not committed).

## R5 ledger (target 900 s, ceiling 1,800 s)

| Step | Seconds |
| --- | ---: |
| UMM protection tests + orchestration test | 4 |
| Earlier focused gates (identities, R5 tests) | 51 |
| Focused gates with the observer (incl. two compile refusals) | 81 |
| Candidate build 1 | 433 |
| Guarded native run 1 (observer FAIL) | 132 |
| Observer-fix focused gate | 15 |
| Candidate build 2 | 380 |
| Guarded native run 2 (PASS) | 186 |
| **R5 total** | **≈1,282** |

Over the 900 s target (the observer defect cost one rebuild and one run);
under the 1,800 s ceiling. R4 remains 1,135 s; R4 + R5 ≈ 2,417 s.

## Owner retest checklist

1. Cloud Giant: one "Obscuring Mist" button; target a point at medium
   range; a stationary mist appears there; cast again at once (at will).
   No Fog Cloud button.
2. Pixie: Entangle, Confusion (full), Dispel Magic (target/area choice) and
   Shield, each usable repeatedly at normal Standard-action cost.
3. Pixie: Irresistible Dance once per summoning (touch); target dances;
   button then unavailable.
4. Mastodon: Trample as a full-round action through smaller enemies,
   2d8+18, Reflex DC 29 half; Elephant/Aurochs unchanged.
5. Griffon: one Charge button; Pounce still works.
6. Optional: with Call of the Wild disabled, Mist and Dance should not
   appear and nothing substitutes for them.
7. Visual approval of the Mist and Dance art and of the shared Trample
   window wording.
