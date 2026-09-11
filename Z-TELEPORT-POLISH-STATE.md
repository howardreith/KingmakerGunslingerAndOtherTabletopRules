# Z teleport polish + specialist mission — durable state

Companion to `Z-TELEPORT-POLISH-AND-SPECIALIST-MISSION.md` (the contract,
delivered as an attachment; eight requirements). This file records progress,
evidence, and exact resumption instructions only. Prefix mission records live
in `Z-TELEPORTATION-MISSION.md` / `Z-TELEPORTATION-STATE.md`.

## Repository / branch / artifact

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`
- Branch: `codex/z-teleport-polish-specialist` (from master `258decb8`,
  which contains PR #12 and the merged 0.0.123 character-visibility repair)
- Active version: `0.0.124-teleport-polish-specialist`
- FINAL artifact (all runtime results below attribute to this build):
  package `artifacts/local-runtime/0.0.124/KingmakerGunslinger-0.0.124-local-runtime.zip`
  DLL sha256 `fd41a7973efd0e139f7e11f0ae4f9643fc7107008155e46cacf1b9c434ec4a35`
  deployment manifest
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260911T1214535881843Z/deployment.json`
  (installed == package == build exact-match guard PASS; owner
  FeatureModules.json preserved every deployment; rollback backups under
  `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/`).
- Pre-mission installed artifact (owner reproduction baseline): 0.0.123,
  DLL sha256 `99FC60BA...` — the owner reproduced the specialist defect on a
  build that ALREADY contained the PR #12 Conjuration-list publication, so the
  report is not an old-build artifact.

## Root cause (R7/R8) — proven at IL + repository-evidence level

Favorite-slot eligibility is the per-book special-spell cache `m_SpecialSpells`,
serialized in saves and derived ONLY at (a) school-feature activation
(`AddSpecialList`, from then-known spells; for non-AllSpellsKnown books) and
(b) learn time (`AddKnown`, when the spell is in an attached special list at
that moment). `Spellbook.PostLoad` (called solely from
`UnitDescriptor.PostLoad`) rebuilds only `m_KnownSpellLevels`; fact
collections run `Fact.PostLoad` and never re-run component
`OnFactActivate` (repo-verified in `ElementalHeritageRuntime.cs`; the prior
mission's own fixture note also observed AddFact not running the component).
AllSpellsKnown books self-heal via `TryRestoreKnownSpells` inside PostLoad.

Therefore a Conjuration specialist whose Teleport/Greater-Teleport knowledge
predates the mod's publication into `WizardConjurationSpellList`
(`69a6eba1...`) keeps a stale cache forever: the favorite slot rejects the
spells (`GetMemorizeSlots` requires `GetSpecialSpells` membership) and the
spellbook shows no specialist styling, exactly as the owner reported — while
newly constructed books work (the prior mission's fixture attached
`AddSpecialList` AFTER knowledge + publication, masking the defect).

Call of the Wild compatibility was investigated: CotW's
`ClassToProgression+AddSpecialSpellList__OnFactActivate__Patch` only skips the
original when the book is missing (verified by IL dump); the native list
identity remains correct with CotW active.

## Repair

`TeleportSpecialistSpellCachePatches` (module-gated, fail-soft Harmony postfix
on `Kingmaker.UnitLogic.Spellbook.PostLoad`) restores, for exactly the
published Teleport/Greater Teleport spells, the native invariant
(known ∧ in an attached special list ⇒ special) via the private native
`AddSpecial(int, BlueprintAbility)` seam. Additive, idempotent, inert when
the module is off (list membership absent), never auto-learns, never grants
slots, never touches other schools/books. Pure decision logic lives in
`TeleportSpecialistSpellCachePolicy` (domain-tested).

## Requirement status (all eight implemented and runtime-verified)

| Req | Change | Runtime proof |
|---|---|---|
| 1 rows inside parchment | rows sized to the settled region of the dialog's ACTIVE native action buttons (Button or ConsoleButton), inset 8 units, flexibleWidth=0, post-settle containment verify; donor fallback when nothing active | coexistence compact-rows-fit (rendered extents within the native region); casting native-layout; gamepad/coexistence-gamepad |
| 2 direct GT cast | `Begin` dispatcher; `OpenDirect` synchronous settle through the same transaction/execution machinery; shared `Settle`; `CanBegin` gate (GT-only rows never need the confirmation surface); one Pending guard; `LastDirectCast` diagnostics | casting greater-exact (+ no-dialog duplicate assertion); interaction cast-after-frames; arrows audit; destinations special-point casts; gamepad greater-direct-settlement |
| 3 confirmation sections | `ConfirmationSections` + presenter-owned hairline rules measured from TMP textInfo; destination row-group separators (desktop+console) | interaction/coexistence scenario regressions (content unchanged); console kept plain |
| 4 GT success suppression | `SuppressSuccessAnnouncement` in `PublishResult`; diagnostics unchanged | domain policy test + casting/interaction commit paths (failures still announce; see limitations) |
| 5 somewhere-else copy | `ArrivalMessage` outcome templates, new localization keys; `Result.UnnamedPoint`/`Result.Arrived` usage removed | domain tests (exact six sentences) |
| 6 target-location copy | same | domain tests |
| 7 Teleport specialist | PostLoad reconciliation (root cause above) | **specialist-cache scenario 8/8 PASS ×3** (incl. twice consecutively on the qualifying build and once on the final build): stale-state reproduction (favorite slots REFUSE both spells while the book knows them and the list contains them) → production PostLoad seam restores membership → native UI prepare/mixed counting/rest → negative controls (evoker untouched, ConeOfCold refused, blank book auto-learns nothing) → idempotence → exact cleanup |
| 8 Greater Teleport specialist | same repair, exercised independently at L7 | same scenario: level-7 assertions independent; `rest-readies-both-favorite-levels` |

## Validation completed

- Repository validation PASS (dispatches 0.0.124 to `validate_teleport_polish124.py`).
- Full domain suite: 1,572 tests PASS, 0 failures (five new).
- Clean exact-reference Release build + strict package validation PASS
  (Build-Local) at every commit; runtime scenario preflight 471 PASS.

## Runtime evidence (guarded, Steam App 640820, KMG_AUTOMATION_WORKING)

FINAL-BUILD SWEEP — every teleportation scenario PASS on the exact final
artifact (DLL fd41a797…, evidence dirs beneath
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/`, newest per scenario):
specialist-cache PASS (twice: 20260911T1209… and 20260911T1218…), specialist
PASS, casting PASS, interaction PASS, coexistence PASS, arrows PASS, scrolls
PASS, travelers PASS, destinations PASS, coexistence-gamepad PASS, gamepad
PASS (20260911T1215…, after the direct-settlement fixture). Zero save writes
in every run (write sentinels asserted). Development fixes made during qualification:
scenario allowlisting (request validator + RuntimeAutomation metadata +
version gate), inactive-donor extent regression (rows measured from ACTIVE
native buttons — the donor Accept control is inactive at world-map points),
single-button vs full-region width (the two-line rows need the Travel+Cancel
span), console extent from ConsoleButton controls, and destinations/gamepad/
arrows fixtures routed through the direct settlement.

## Evidence log

- 2026-09-11 (session 1): mission surveyed; installed artifact identified
  (0.0.123 contains PR #12 publication — owner reproduction is current); root
  cause proven (IL dumps + native Spellbook.cs/PostLoad chain + CotW patch
  disassembly); branch created; eight requirements implemented; 5 new domain
  tests; version 0.0.124 bump across identity files, validator lattice,
  compatibility profiles, preflight/qualification literals; 1,572 domain tests
  PASS; Build-Local PASS; deployed with backup; checkpoint pushed.
- 2026-09-11 (session 2): guarded runtime qualification as above; all
  teleportation scenarios PASS on the final artifact; state/acceptance
  documentation updated; final checkpoint pushed.

## NOT RUN / limitations

- The owner's exact character/campaign save was not exercised (the bundle
  contains no save). The constructed specialist-cache fixture reproduces the
  reported stale-cache state through native seams (list rollback →
  AddSpecialList → AddKnown → publication restored), which is the same state
  an existing specialist save presents at load; the mission's no-save
  constraint is disclosed rather than masked.
- Player-facing banner VISUALS (R3 rules, R4/R5/R6 messages) are proven by
  code path + domain tests and the scenarios' transactional assertions; no
  screenshots were captured autonomously (AGENTS: screenshots optional
  supporting evidence only). Before/after owner screenshots 01-09 document
  the prior state.
- Aspect ratios other than the fixture geometry (owner 1920×1200 not
  re-exercised; scenario geometry is the working save's native mode).
- `disposable-teleportation-disabled` and the four-phase persistence suite
  were NOT rerun this mission (their dedicated orchestrators exist:
  Invoke-TeleportationHardeningQualification /
  Invoke-TeleportationPersistenceQualification); the disabled-scenario gate
  was updated to include the new patch's Installed flag.

## Acceptance report (eight items)

1. Rows inside parchment — DONE (settled active-native-region sizing +
   rendered containment verification; scenarios PASS as above).
2. Direct Greater Teleport — DONE (no second confirmation, one in-flight
   guard, transaction/execution reuse, desktop + gamepad, scrolls and
   prepared/spontaneous sources through the same Begin dispatcher).
3. Confirmation sections — DONE (hairline rules measured from rendered
   text; console keeps plain text; group separators in destination rows).
4. GT success suppression — DONE (policy + presenter; diagnostics intact).
5. Somewhere-else copy — DONE (complete-sentence templates).
6. Target-location copy — DONE (legacy fallback removed).
7. Teleport specialist — DONE (root cause + load-seam repair + runtime
   proof).
8. Greater Teleport specialist — DONE (independently exercised at L7).

No merge, no public release; owner review required.
