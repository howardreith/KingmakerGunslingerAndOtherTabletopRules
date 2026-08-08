# Autonomous Gunslinger blockers

## Rare Firearms Seeking stop — RESOLVED BY USER AUTHORITY (2026-08-08)

The continuation amendment expressly accepts the complete native investigation
preserved immediately below, forbids repeating it, supersedes the custom-Seeking
prohibition and native-only hard stop, and authorizes project-owned Seeking with
stable GUID `036fc59fd1e24753b98f9d92cdb1e93e`. This is not an active blocker.
The mission resumed from clean published checkpoint
`69fd9a0b9e5889082f15f0e536eb940e79be138b`.

## Rare Firearms mission — critical native Seeking absence (2026-08-08)

The Rare Firearms and Campaign Integration work order requires The Last Word to
use the installed native Seeking weapon enchantment and defines absence or
unsafe impossibility of a required accepted native property other than optional
Thundering as a critical hard stop.

Three materially distinct checks found no native Seeking authority:

1. Guarded blueprint-graph run
   `20260808T1734599486274Z-observe-vendor-table-contracts` enumerated native
   weapon enchantments by internal name and exact donors/components: no Seeking.
2. Guarded run `20260808T1739377557378Z-observe-vendor-table-contracts` expanded
   matching to localized display names: no Seeking.
3. Guarded run `20260808T1744000629586Z-observe-vendor-table-contracts`, runtime
   ID `20260808T1744000942305Z-9576e406512a450bbe5766283bc57d5b`, exact source
   `d0e039a705075f971673b660f742650b4d8f20b2`, independently selected every
   `BlueprintWeaponEnchantment` with a concealment-related component as well as
   name/display candidates. It returned 17 relevant enchantments and none was
   Seeking or carried a concealment component. A separate exact installed
   Assembly-CSharp type/string contract search found concealment/reroll mechanics
   but no native Seeking enchantment authority.
4. Continuation audit guarded run
   `20260808T1748440170104Z-observe-vendor-table-contracts`, runtime ID
   `20260808T1748440250732Z-7e65d222eca74ef59b9522c572e1ac8e`, exact source
   `ebef1c36d0cccf3e8db0bdb95051eda2a7bbf669`, inspected primitive, enum, and
   string values on every weapon-enchantment component. The same 17 relevant
   enchantments contained no Seeking, concealment, miss-chance, or blindness-
   suppression value.

Historical original stop instruction (superseded): do not custom-reimplement Seeking, remove it, rename another property, or begin
production registration. Required resolution is new user authority changing the
nonoptional native-property requirement or an authoritative installed-game asset
that supplies the missing native enchantment.

---

## 2026-08-07 Pistolero/Musket Master branch publication

- Revision 2 mission baseline, unchanged qualification, and durable mission
  documents are committed locally as `c962e33` on required branch
  `codex/pistolero-musket-master-archetypes`.
- The exact approved push helper refused the branch because its external
  allowlist contains only `codex/firearm-wwise-audio`,
  `codex/firearm-native-weapon-rigs`, and
  `codex/postbase-archetypes-compatibility`.
- Raw push, helper modification, policy workaround, and reuse of the obsolete
  compatibility branch are prohibited. The work order classifies this required
  policy conflict as a hard stop.
- Human action: add `codex/pistolero-musket-master-archetypes` to the approved
  helper allowlist. Then publish `c962e33`, verify the remote SHA, and resume at
  the mandatory pre-implementation inventory.

## 2026-08-07 Call of the Wild character-class catalog

- Human exact-build evidence reached new-game character creation. Call of the
  Wild classes were visible, but Gunslinger was absent.
- Disposition is `CONFLICT-CONFIRMED`: final character-creation catalog omits
  Gunslinger. Root-catalog versus selector-cache causality is not yet proven.
- This is independent of the standalone Dodge fixture blocker below.

## 2026-08-07 standalone comprehensive qualification

- Standalone and Arms & Armor plus Toggle Custom Soundpacks comprehensive runs
  reproduce the same failure. The detached Gunslinger's Dodge command finishes
  as `Interrupt` and applies no timed buff. Exact diagnostic run:
  `20260807T2057209416590Z-a48e33c01d6f48f2b407eb08fe361035`.
- This prevents comprehensive and working-save qualification and is classified
  `GUNSLINGER-REPAIR-REQUIRED`, not an optional-mod interaction defect. The
  strict mechanic assertions were not weakened.
- Call of the Wild 1.14.4c-2.1 independently remains `CONFLICT-OBSERVED` after
  300- and 600-second guarded readiness timeouts.

## 2026-08-07 optional-mod compatibility publication policy

- Resolved 2026-08-07: the user updated the approved helper allowlist. Exact
  helper publication succeeded and local/origin both pointed to `9da61a4`.
- Validated checkpoint `274d4d7` on required branch
  `codex/postbase-archetypes-compatibility` invoked the exact approved push
  helper.
- The helper refused the branch because its external allowlist contains only
  `codex/firearm-wwise-audio` and `codex/firearm-native-weapon-rigs`.
- Raw push and policy workarounds are prohibited. Safe local work and durable
  commits may continue, but publication remains externally blocked until the
  user updates the approved helper allowlist.

## 2026-08-04 sixth-playtest external gates

- Functional 0.0.66 qualification is complete at `448e0d1`, including two
  comprehensive runs and two canonical working-save smokes. The remaining
  grip/pose/holster, bolt visibility, audible layering, and shop-UI claims
  require the consolidated supervised human checklist.
- The exact repository-restricted checkpoint publication script was invoked
  after the commit. Workstation security categorically rejected network access;
  raw `git push` and policy workarounds remain prohibited. This is an external
  publication boundary, not a mod functionality failure.

The Sprint 83 human-input gate is resolved by explicit authorization of option
1. Other bounded gates remain independently documented.

## 2026-08-02 save-backed feature-scenario permission boundary

- The external runtime reviewer rejected `production-firearm-catalog`, stating
  that only `mod-load-smoke` and `working-save-smoke` are explicitly permitted
  save-backed scenarios at that boundary.
- No retry, indirect execution, or workaround is permitted. This blocks the
  existing save-backed catalog/capacity/starting-item feature routes unless
  explicit authority changes.
- Save-free detached observers remain permitted and productive, so this is not
  yet an overall human-input hard stop.

## 2026-08-02 Sprint 64 save-backed runtime permission boundary

- Exact mod load passes on source `fdf54ec`.
- The external execution policy rejected the first guarded
  `production-firearm-catalog` request before launch because it names the
  disposable working save. No workaround is authorized and no save was
  accessed.
- This is a bounded runtime-evidence gate, not an overall mission hard stop.
  Continue save-free and source-qualified lifecycle work; retry only after the
  external boundary explicitly permits this exact guarded scenario.

## 2026-08-02 Sprint 61 vendor reference investigation (resolved)

- Direct-cast and speculative `Get()` resolver attempts failed closed because
  `LootItemsPackFixed.m_Item` is a `LootItem` wrapper.
- The mission-required mode change used non-invoking metadata observation to
  prove the wrapper's exact read-only `Item` property, then safely resolved all
  native entries. Production qualification passed twice on `c2fd27b`.
- This is resolved and is not a human-input blocker.

## 2026-08-01 disposable respec cleanup investigation (resolved)

- Exact metadata observation on `dd85431` passed twice and proved
  `UnitEntityData.PrepareRespec` delegates to `UnitDescriptor.PrepareRespec`,
  whose direct call graph only sets `Body`.
- Source-qualified scenario commit `25d2da1` passed 691/691 tests, clean Release
  build, and strict packaging. Exact mod-load PASS evidence is
  `20260801T1805094993616Z-mod-load-smoke`.
- First save-free attempt
  `20260801T1806257874513Z-disposable-gunslinger-respec-preview` failed closed
  with `NullReferenceException`; no save was loaded.
- Materially different stage-labeled commit `f2fbcc5` passed all source gates
  and mod-load evidence `20260801T1809469616185Z-mod-load-smoke`. Its run
  `20260801T1811040970058Z-disposable-gunslinger-respec-preview` also failed
  closed with `NullReferenceException`. Mandatory cleanup masked the labeled
  inner exception, consistent with disposal after native body replacement but
  not sufficient to claim the exact call site.
- Two materially different initiating attempts failed, so the mission requires
  a mode change. The next authorized action is a metadata-only observation of
  the exact body setter and descriptor/entity disposal call graphs. Do not
  launch a third initiating respec attempt until that narrower evidence supports
  a different cleanup architecture. Existing same-class and multiclass preview
  qualifications remain valid.
- Metadata-only run `20260801T1817013647054Z-observe-character-creation-contracts`
  on `07dd111` proved the body setter has no nested managed calls, while entity
  disposal delegates to descriptor disposal and descriptor disposal calls
  `UnitBody.Dispose`. Restoring the retained original disposable body before
  entity disposal is therefore the next evidence-supported architecture; this
  is not a blind third variation.
- Restored-body commit `4fdbfea` passed mod load at
  `20260801T1821061256490Z-mod-load-smoke`. Its reduced initiating run
  `20260801T1822203121648Z-disposable-gunslinger-respec-preview` preserved the
  real first failure as `start-respec-controller`; cleanup no longer masks it.
  Investigation has changed back to metadata-only inspection of controller
  start, construction, and preview call graphs before any further initiating
  attempt.
- Metadata PASS run `20260801T1826247826437Z-observe-character-creation-contracts`
  on `578d404` proved startup immediately constructs the controller; its
  constructor starts/requests the preview, and `RequestPreview` posts and turns
  on the preview entity. Destructive source preparation before construction is
  the invalid ordering. The next reduced scenario keeps the disposable source
  body intact and tests native `Respec` preview mode without `PrepareRespec` or
  `Commit`.
- The reduced single-unit run
  `20260801T1831121462577Z-disposable-gunslinger-respec-preview` completed safely:
  source/body isolation and cleanup passed, but preview retained Fighter 1 and
  added Gunslinger 1. Exact installed `Player.RespecCompanion` IL then proved
  native respec creates a fresh unit from the same blueprint, copies experience,
  and initiates respec on that replacement candidate. The next scenario uses a
  second detached `ChargenUnit` as the level-zero replacement and avoids the
  native global creation/replacement callbacks.
- Exact detached-replacement commit `3d4ba8f` passed mod load at
  `20260801T1836154433116Z-mod-load-smoke` and two independent respec preview
  runs `20260801T1837314150470Z-disposable-gunslinger-respec-preview` and
  `20260801T1838472989503Z-disposable-gunslinger-respec-preview`. The runtime
  investigation is resolved; broad replacement commit remains an engineering
  boundary, not a human-input hard stop.

## Active gates

- Sprint 83 is resolved: the user authorized persistent originating-owner
  binding, exact owner-bound effective condition, fixed 22 gp value, and a real
  automatically granted Gunsmithing feature gating Repair/Overhaul. Sprint 84
  source-qualifies the feature split; item-owner persistence remains active
  engineering work, not a human-input blocker.

- Sprint 80 comprehensive acceptance exposed one probabilistic observer
  assumption: a -100 Will modifier does not defeat Kingmaker's natural-20 save
  success. Sprint 81 corrected only the fixture with exact native d20 seeds.
  Exact `58baf84` mod load and two comprehensive fresh-process runs passed all
  30 slices; this issue is resolved and is not an active blocker.

- Sprint 62 scatter completion is bounded on a player-facing balance decision.
  Installed native cone geometry requires a numeric distance, but every
  authoritative local source describes Blunderbuss range only as `special`.
  Choosing a distance would alter balance without authority. Preserve the
  fail-closed unavailable restriction and continue independent production
  presentation/lifecycle work.

- Sprint 40 Utility Shot is disposition-complete and Stop Bleeding is
  runtime-qualified on `8270ade`. Bonus-feat selection is the next engineering
  gate; no human-input blocker was created.

- Sprint 41 Bonus Feats is runtime-qualified using the exact native Fighter
  combat-feat selection. Kingmaker's lack of a native grit-feat category is
  documented and does not block the base combat-feat progression. Sprint 42
  Gun Training is the next engineering gate.

- Sprint 42 Gun Training is runtime-qualified on `76ae9f9` at version `0.0.42`.
  The next incomplete base-class/deed row is an engineering gate, not a
  human-input blocker.

- Sprint 43 Dead Shot is runtime-qualified at version `0.0.43` on `fdd5d7c`.
  Exact mod load and two independent guarded mixed/all-misfire runs passed.
  Startling Shot is the next engineering gate, not a human-input blocker.

- Sprint 44 Startling Shot remains source-qualified. Current exact mod load
  `20260802T1031471777161Z-mod-load-smoke` passed. Fresh runs
  `20260802T1033081927669Z` on `4cb5251` and `20260802T1038097680536Z` on
  `b7eeaa9` both failed closed because native `AddBuff` returned null. Retaining
  detached chargen immortality did not change that result. Independent
  Targeting Head evidence then proved Kingmaker can return null while still
  installing the exact timed condition fact. The two-attempt limit prohibits
  another speculative Startling run; exact applied-fact reconciliation is now
  implemented on `3a26059` with full source/build/package qualification. A
  newly authorized runtime attempt is still required; this bounded evidence
  limit is not an overall human-input hard stop.

- Sprint 45 Targeting Head remains source-qualified with strong partial runtime
  evidence. Run `20260802T1039503873162Z` proved hit, grit `3->2`, chamber
  `1->0`, and active Confusion while native `AddBuff` returned null. Repair
  `1928bba` reconciles the exact applied fact and explicitly dispatches detached
  hit damage. Run `20260802T1042345480789Z` then proved damage `0->5`, exact
  six-second nonpermanent Confusion, grit/chamber behavior, and cleanup. Its
  sole FAIL was stale observer bookkeeping (`Attack.MeleeDamage` remains null
  because the explicitly triggered `RuleDealDamage` is a separate rule) despite
  the authoritative damage delta. `4485dba` now uses that exact target delta.
  The two-attempt limit prohibits a third assertion-only rerun; this is not an
  overall human-input hard stop.

- Most base-class and production-content rows are not started; they are planned
  engineering work, not blockers.
- Sprint 56 Cheat Death is resolved. Exact completed `RuleDealDamage` handling
  on `10a4274` passed mod load and two fresh feature launches, leaving eligible
  units at 1 HP after spending all grit while the zero-grit control remained
  lethally damaged. The next incomplete coverage item is an engineering gate.
- Several deed adaptations require exact Kingmaker contract investigation.
  Existing project authority and reversible evidence gathering remain available.
- Sprint 57 Death's Shot is temporarily blocked after two materially different
  guarded observers. Destruction (`3b646e1d...`) is Death-descriptor divine
  damage, not a kill action. The complete Death-descriptor catalog contains
  three Fortitude/kill authorities: Scaled Fist Quivering Palm (`749e77f7...`),
  Monk Quivering Palm (`4de518e6...`), and conditional Death Clutch
  (`c3d2294a...`). Selecting one after the two-attempt limit requires human
  authority; direct HP/state death remains prohibited. Stunning Shot is
  independently actionable.
- Sprint 58 Stunning Shot is resolved and runtime-qualified on `f5dc6bb`.
  Two fresh-process PASS runs prove both native Fortitude branches, native
  critical immunity, exact grit/chamber behavior, one-round Stunned, damage,
  isolation, and cleanup. It is no longer an active engineering blocker.
- Sprint 59 True Grit is resolved and runtime-qualified on `1d7c5b6`. Two
  fresh-process PASS runs prove the production selection shape, selected native
  deed cost reduction, zero-grit edge rules, variable costs, fixed Slinger's
  Luck exclusion, isolation, and cleanup. It is not an active blocker.
- Sprint 60 presentation is resolved and runtime-qualified on `adcb030`.
  Installed Fighter class/progression icons were both null; ADR-0007's
  crossbow-compatible production Early Pistol icon is the qualified fallback.
  Two fresh-process observations proved 75 visible facts, one excluded hidden
  fact, zero incomplete facts, and six native UI groups. This is no longer an
  active blocker; later equipment acquisition is the next engineering gate.
- Sprint 78 detached corruption handling has strong single-run evidence on
  `731ff07`. The first run exposed only the exact exception type; the repaired
  second/final run preserved two ambiguous native tokens while rejecting state
  reconstruction. Do not run a third attempt. Live sale/restart qualification
  remains behind the separate save-backed permission boundary, but independent
  work remains available.
- The authoritative firearm table labels blunderbuss range `special`; the
  immutable definition and marker vocabulary now represent that fact without a
  numeric value and ordinary-AC selection fails closed. Concrete scatter range
  execution remains assigned to Sprint 32 and is engineering work, not a
  human-input hard stop.
- Sprint 92/93 bound-value permission and engineering blockers are resolved.
  The user authorized the exact named scenario; item-owned repair `6b1e413`
  passed mod load and two fresh guarded runs
  `20260802T1441506873456Z` / `20260802T1445126858809Z`. Native grant, exact
  origin, 22/1000 gp isolation, rollback, and no-save behavior all passed.
- Post-Sprint-93 re-audit found renewed runtime permission is the narrowest
  remaining gate. The repaired save-free Startling Shot and Targeting Head
  scenarios reached their prior attempt limits before their current
  evidence-supported fixes. Exact commands and the single authorization
  question are recorded in `HUMAN-INPUT-REQUIRED.md`. No attempt is permitted
  until the user renews authority.
- Resolution: the user authorized both save-free scenarios. Exact `8609ebd`
  passed two fresh Startling Shot runs and two fresh Targeting Head runs. Both
  deed runtime gates are closed; `HUMAN-INPUT-REQUIRED.md` is removed.
- Current hard stop: production Blunderbuss scatter requires a numeric cone
  distance, but every authorized source supplies only `special`. Native cone
  geometry and volley semantics are already qualified. The player-facing
  balance choice and conservative 15-foot recommendation are recorded in
  `HUMAN-INPUT-REQUIRED.md`.
- Resolution: the user directed the project to follow PnP, authorizing the
  15-foot pellet cone (and distinct 10-foot bullet range). Sprint 94 encodes
  that authority. Scatter is now an active engineering gate, not a blocker.
- Current hard stop: the guarded Scatter Shot runtime gate exhausted its two
  materially different attempts. Exact `07407d4` failed before mutation because
  the installed native cone predicate is an instance method. Repaired exact
  `ac5c520` passed mod load `20260802T1602457684816Z` but scenario
  `20260802T1604090773661Z` failed before mutation because detached
  `ChargenUnit` fixtures are absent from `GameHelper.GetTargetsAround`.
  Production remains restricted and vendor-excluded. A third runtime attempt,
  using a reversible live-area registration fixture or a human-positioned
  supervised fixture, requires renewed human authority.
- Resolution and policy supersession: standing authorization now explicitly
  covers repeated materially distinct runtime work and reversible request-local
  live-area registration. All historical retry-count ceilings in this ledger,
  the journal, resume notes, and qualification records are evidence-history
  only and impose no current stop. Scatter is an active engineering gate.
- Current external-input boundary: exact `f3f3ab0` has completed every
  deterministic, package, guarded mechanical, comprehensive, and working-save
  gate, including native combat-log notification coverage for every condition
  transition. The remaining claims are perceptual: rendered UI readability,
  live doll socket/scale/orientation and mesh suppression, projectile appearance,
  and audible quality/layering. Repository policy forbids automated visual/OCR/UI
  evidence as proof. One human must complete
  `FOURTH-PLAYTEST-VISUAL-ACCEPTANCE-CHECKLIST.md`; no autonomous engineering
  alternative remains unless that session reports a concrete failure.
- The Pistolero/Musket Master mission has no newly established hard stop. Its
  combined Musket Master starter/mechanics, expanded Pistolero deed, generic
  presentation, and class observers are runtime-qualified. Persistence,
  compatibility, and final integration gates remain autonomous.
- Archetype reconciliation is now independently runtime-qualified across five
  native respec transitions. No new blocker was established; the bounded
  ordinary-Gunslinger starting-firearm choice remains a non-blocking secondary
  investigation.
- The 0.0.73 qualified-combined comprehensive run reproduced the inherited
  detached Dodge missing-buff failure and additionally exposed an unchanged-
  source Targeting Torso cache defect: forced natural 19 and live edge 19 are
  observed, but Kingmaker's cached `IsCriticalRoll` remains false. Two bounded
  event-order adapters failed to alter that native cache and the ineffective
  adapter was removed. This does not block independent archetype qualification,
  but it blocks a full-mod aggregate claim alongside Dodge.
- These two aggregate defects do not block the archetypes' independent
  definition of done. Exact final 0.0.73 Pistolero and Musket Master runtime
  pairs and the canonical working-save pair all pass. No additional autonomous
  archetype repair is justified; retain both defects as full-Gunslinger
  blockers without attributing them to this mission.
