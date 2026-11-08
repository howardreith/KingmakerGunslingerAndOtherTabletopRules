# Summoned Creature Manual Playtest Repair — R3 status

**R3 COMPLETE FOR REVIEW (description checkpoint, 2026-11-08).** Every R3
item is now established on actual native framebuffer evidence at normal UI
scale, and every item's live data predicates pass. UI-01 (the initiative
tracker) keeps its inspected screenshot from candidate `57a8e659b1e6`, with
its limitation. UI-02..UI-06 are established by the eight native ability
description windows captured on candidate `31e61aa73a01`. Product code has
been unchanged since `221cadc7a67e`. Owner aesthetic approval is separate and
still pending. Not merged or released.

## Active summary (current)

| Item | Live data | Native view binding | Framebuffer evidence | Owner |
| --- | --- | --- | --- | --- |
| UI-01 portraits | PASS | PASS: each portrait zone is bound to its exact unit and shows that unit's own art | PASS on `57a8e659b1e6`: `native-ui-008.png` shows all seven representatives (inspected). Not rerun on `31e61aa73a01`. Limitation below. | pending |
| UI-02 Lantern icons | PASS | PASS: exact window, `Data`, live fact and owner | PASS on `31e61aa73a01`: Light Ray (Searing Light art) and Twin Light Rays (Scorching Ray art) are distinct; no Duration/Saving Throw rows | pending |
| UI-03 Ghaele ray | PASS | PASS | PASS: the owned `c3224dec…` fact on the live KMG Ghaele renders "Light Ray", a Special type, an icon, Target, Spell Resistance, Casting Time and its description; nothing blank or nameless | pending |
| UI-04 Nimbus icon | PASS | PASS | PASS: the icon is visible, with Duration "Bursts after 1 round; dazed 1d10 rounds" and Saving Throw "Will negates (non-demons)" | pending |
| UI-05 Overrun icon/rows | PASS | PASS | PASS: Overrun shows brachiosaurus creature art and Trample keeps `Longstrider`; Overrun has no Duration/Saving Throw rows; Trample's only row is "Reflex DC 32 half; none for a target that makes its attack of opportunity instead"; no inherited "Trample" or native maneuver text | pending |
| UI-06 tooltip rows | PASS | PASS | PASS: Cheetah Sprint "1 round"/"None", Trumpet Blast "1d4 rounds"/"Fortitude negates", and Nimbus as above; no `<null>` in any of the eight windows | pending |

UI-03 data, worded exactly: the live KMG Ghaele has one owned light-ray fact
and no native ray fact. Its live brain holds one AI action that references the
owned ray and none that references the native ray. No ray cast was executed in
any R3 run.

UI-01 limitation, retained: on `57a8e659b1e6`, the native one-time
"Auto End Turn" prompt appeared mid-screen on the first player turn. It did
not cover the tracker, but that candidate's ownership predicate did not
detect it. It was left unanswered, and the settings snapshot restored the
native settings key. The modal refusal added below was not in that
candidate, and the tracker was not replayed.

## Description checkpoint (allocation cap 1,500 s)

Ledgers, kept separate:
- Original R3: ≈ 1,256 s.
- Rendered-UI continuation: 505 s.
- UI-evidence allocation (900 s, later restated as a 1,500 s cap that
  includes it): 573 s at the previous checkpoint, plus 576 s in this
  continuation (gates 25.9 + build 409 + run 141). That is 1,149 s of the
  1,500 s cap.
- Cumulative R3: ≈ 2,910 s.

- **Intake recheck (unchanged).**
  - Repository: HEAD and PR #113 at `22f906caad`; this continuation had not
    been executed.
  - Installation: 0.0.140, DLL `00EF6757…`, live tree `5C3E4AD4…` (254
    files), 16 mod directories, 66 settings values.
  - Saves and references: 95 protected save files (baseline `CC7CBB0D…`),
    reference bundle `7F1EDE2B…`.
  - Runtime: no Kingmaker process; last lease `Completed`, no recovery
    required.
- **Native construction, verified in the installed 2.1.7b assembly before
  editing.**
  - `DescriptionBuilder.Build` switches on `TooltipData.Type`.
    `TooltipType.Ability` → `DescriptionBuilder.Ability`, which for a
    non-null `data.Ability` runs
    `AbilityHeader(isTooltip, HeaderBox, data.Ability, data.BlueprintAbility)`,
    then `DescriptionTemplatesAbility.AbilityDescription`.
  - `AbilityDescription` reads the fact's blueprint for Target, Duration,
    Saving Throw, Spell Resistance and Casting Time. It reads
    `data.Ability.Data` for metamagic and spell level. With
    `isTooltip:false` it shows the full description.
  - The native `TooltipDataHelper` builds exactly
    `Type = TooltipType.Ability, Ability = <fact>` for a live Ability.
    `BlueprintAbility` stays null, and `IsHUDPanelAbility` is set only for
    HUD triggers.
- **Observer corrections (observer only; product code frozen).**
  - The descriptions are built as `new TooltipData { Type = TooltipType.Ability, Ability = fact }`.
  - Each result records its construction: type, window type, fact GUID,
    owner unit and blueprint, `BlueprintAbility`/`AbilityData`/HUD flags,
    whether the window holds the request's `Data`, and header/content/footer
    brick counts.
  - New unrelated-modal refusal: `TeleportationConfirmationSurface.UnrelatedModalShown()`
    (desktop `DialogMessageBox.IsShown`), referenced only, never modified.
    It is checked before opening each description, while the window opens,
    on every layout frame, on every capture frame (through the ownership
    predicate) and after closing.
  - The guard goes through a pure predicate,
    `ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned`,
    shared by the description and the retained tracker capture helpers. A
    new focused test (`summon-playtest-r3.surface-modal-refusal`) covers
    the allowed case and the modal/default/narrative/surface refusals. A
    shown modal stops UI interaction without answering it.
  - Descriptions-only: no combat entry, tracker replay, turn advance or
    Auto End Turn trigger.
- **Focused gate** (same filter; 9 registered tests including the new one).
  - Production compiled, then one test-project compile error (a missing
    `using`), then 9/9 PASS.
- **Candidate** (one build, explicit `-ReferenceBundleDir`, 9 tests
  selected, 409 s).
  - Source `31e61aa73a0108ad7673f250210930254d952933`.
  - Package `KingmakerGunslinger-0.0.179-development-summon-playtest-r3-31e61aa73a01.zip`,
    SHA-256 `f8258ca4e45cfcc552d41a0fe3230d2296f3cb16919ca86b2920720062fa1d5f`.
  - DLL `af3ebc14b7fe384befdb22e5273d756055b15c6c040ecc7dfccf55ea71a6fe74`,
    loaded MVID `8ee07148-f739-4880-a7a9-0f3d3378be0c`.
  - Observer: the R3 phase as compiled into that DLL.
- **One guarded run** `20261108T0321010236250Z-1f81847c43fd42d8afb9badc53730de2`
  (141 s; evidence and 8 PNGs local and private): 23 PASS, 0 FAIL.
  - All data predicates passed: `portrait-roster`,
    `donor-portraits-untouched`, `initiative-portraits-live`,
    `lantern-rays`, `ghaele-ray-owned-grant`,
    `overrun-trample-presentation`, `metadata-rows`.
  - All eight `render-*` checks passed, as did admission, native Default
    enter/restore, `owned-ui-restored` and fixture restore.
  - Every construction record shows `type: Ability`, the exact fact and
    owner, `windowDataIsRequest: true` and `unrelatedModal: false`.
  - Brick counts: 1 header and 10–16 content bricks. No scroll was needed
    for any row.
- **Images inspected personally** (`native-ui-000`..`007`, 1920×1080,
  normal UI scale). Each shows the native ability window over the Oleg
  fixture, with no other dialog:
  - `000` Lantern Light Ray: Searing Light art; Special; Target, Spell
    Resistance, Casting Time "Standard action", description.
  - `001` Twin Light Rays: Scorching Ray art; Casting Time "Full round".
  - `002` Ghaele Light Ray: Searing Light art; "One enemy creature within
    long range"; description "Fires 2 ranged-touch rays… 2d12…".
  - `003` Unholy Nimbus: icon visible; Supernatural; Duration and Saving
    Throw rows; full description visible.
  - `004` Overrun: brachiosaurus creature medallion; Extraordinary; no
    Duration/Saving Throw rows; "ordinary overrun maneuver… not the
    creature's trample".
  - `005` Trample: `Longstrider` art; the Saving Throw row reads
    "Reflex DC 32 half; none for a target that makes its attack of
    opportunity instead"; no Duration row.
  - `006` Cheetah Sprint: cheetah medallion; "1 round"/"None"; Swift action.
  - `007` Trumpet Blast: trumpet icon; "1d4 rounds"/"Fortitude negates".
    The long description continues under the native window scrollbar; every
    row is fully visible.
- **Native-template observations (not product defects; owner awareness).**
  - Custom-range targets read "within 30 ft. feet." and, for
    Overrun/Trample, "within 0 ft. feet." The native
    `BlueprintAbility.GetTarget` formats the localized
    "within {0} feet" string with a `Feet` value that already prints "ft."
  - Every window's header shows "None" (the spell-school slot). The native
    `AbilityHeader` with `BlueprintAbility` null does this for any
    non-spell ability, exactly as on the HUD path.
  - The Nimbus icon is the native Unholy Aura ability's own sprite (asset
    name `ProtectionFromGood`).
  - The combat-log pane shows the fixture's pre-existing "-unit name not
    set- uses ability" lines.
- **Restoration (exact).**
  - The guarded wrapper restored the live tree to `5C3E4AD4…` (0.0.140);
    record `expanded-summoning-restoration/20261108T0322467042643Z-…`.
  - The runner record shows 0.0.140 before and after, 66 settings values
    restored, and 95 protected save files unchanged (baseline `CC7CBB0D…`).
  - `owned-ui-restored`: no errors, flags restored, tutorial untouched,
    selection restored, anchor unchanged and out of combat.
  - No process; last lease `Completed`, no recovery required.

### Owner retest checklist

Every item is visual approval only. All mechanics and presence are already
established.

1. Initiative: the summoned creatures show their own species art, and the
   cropped medallion look is acceptable.
2. Lantern Archon: the Light Ray and Twin Light Rays icons are acceptable.
3. Ghaele: "Light Ray" and its icon are acceptable, and no blank action
   remains.
4. Nalfeshnee: the Unholy Nimbus icon (the native Unholy Aura art) is
   acceptable.
5. Brachiosaurus: the Overrun creature medallion versus Trample's
   `Longstrider` icon is acceptable.
6. Optional polish to decide on, not R3 scope: the native "ft. feet." target
   wording on custom-range KMG abilities.

---

# UI-evidence checkpoint (historical, retained)

**R3 PARTIAL (UI-evidence checkpoint, 2026-11-07).** UI-01 is now established
on actual framebuffer evidence: the native initiative tracker shows every
representative's own portrait with native overlays intact. UI-02..UI-06 still
lack rendered evidence. All eight native description windows were captured
under exact ownership but rendered **empty**, because the observer built
`TooltipData` without `Type = TooltipType.Ability`. That is an observer
construction defect, not product content. Data predicates for UI-01..UI-06
pass natively. Owner visual approval is separate and pending.

## Active summary (superseded by the description checkpoint above)

| Item | Live data | Native view binding | Framebuffer evidence | Owner |
| --- | --- | --- | --- | --- |
| UI-01 portraits | PASS | PASS: portrait zones bound to each exact unit show its own art | PASS: `native-ui-008.png`, all seven representatives, inspected | pending |
| UI-02 Lantern icons | PASS | not observed | missing (empty description window) | pending |
| UI-03 Ghaele ray | PASS | not observed | missing | pending |
| UI-04 Nimbus icon | PASS | not observed | missing | pending |
| UI-05 Overrun icon/rows | PASS | not observed | missing | pending |
| UI-06 tooltip rows | PASS | not observed | missing | pending |

UI-03 data, worded exactly: the live KMG Ghaele has one owned light-ray fact,
no native ray fact, and its live brain holds **one AI action that references
the owned ray and none that references the native ray**. No ray cast was
executed in any R3 run.

## UI-evidence checkpoint (owner allocation 900 s)

Ledgers kept separate: original R3 ≈ 1,256 s; rendered-UI continuation 505 s
(cumulative ≈ 1,761 s before this allocation); this allocation 573 s
(gates 33.1 + build 402 + run 138). Cumulative R3 ≈ 2,334 s.

- Intake recheck (unchanged): master `fa3eeb9e1a`; PR #113 open, draft, base
  master, head `20a1447bf7`; installed 0.0.140, DLL `00EF6757…`, live tree
  `5C3E4AD4…` (254 files), 16 mod directories, 66 settings values, 95
  protected save files (baseline `CC7CBB0D…`), reference bundle `7F1EDE2B…`;
  no Kingmaker process, no held compatibility lock, last lease `Completed`
  with no recovery required.
- Engine contracts read before editing (2.1.7b): a non-controllable unit's
  idle Acting turn is ended natively (`TurnController.Tick` → AI force-tick
  → idle wait), which explains the `76a9cf269310` failure. A directly
  controllable unit's turn waits for input and stays `Preparing` until it
  acts. `DescriptionBricksBox.m_ShowList` holds rendered bricks in order, and
  `AddH2AndDescriptionBlock` appends a TitleH2 brick then its SimpleText value.
- Observer changes (product code frozen):
  - The eight descriptions run first, before combat, each independent.
    Ownership (native Default, no narrative, exact window, `Data`, live fact,
    no foreign hover, fully faded-in surface) gates the capture. Content is
    assessed afterwards from brick bindings. A content FAIL is kept and the
    next owned surface continues; an owned scroll view is used only if a row
    is clipped.
  - The tracker is captured during the verified owned anchor's
    input-waiting turn (only party member, quiet brain, directly
    controllable, natively enrolled), then graded on representative
    coverage (an animal, Lantern Archon, an outsider) and on any wrong
    visible picture. Units outside the visible list are outside coverage.
- Focused gate (same filter, 8 registered tests): one compile error
  (`ScrollToTop` is a Kingmaker extension; replaced with the `ScrollRect`
  API), then 8/8 PASS.
- Candidate: source `57a8e659b1e67efee2f7174603709257cb843588`, package
  `KingmakerGunslinger-0.0.179-development-summon-playtest-r3-57a8e659b1e6.zip`
  SHA-256 `36f7054abae1dd2021717b3df3fa46cfde30a273fc4500c0e92ff87b85e64f55`,
  DLL `fad7135459a11c03851aebe197fce48d6be4a97585ada3484c0e8470bdd53eca`
  (MVID `5aa8ae1b-ae1b-449b-b207-073e96d58253`). Observer: the R3 phase as
  compiled into that DLL.
- One guarded run `20261107T2331275709810Z-4252cb3b422f4dcf9a16291cb3a19e2a`
  (138 s; evidence and 9 PNGs local and private): 17 PASS, 8 FAIL; no execution failure.
  - PASS: every data predicate, `anchor-waiting-owner`,
    `tracker-representative-coverage` (covered all seven, `wrongVisible` 0,
    current turn the anchor in `Preparing`, no fixture turn advances needed),
    plus admission, Default and fixture restore, and `owned-ui-restored`.
  - FAIL `render-*` ×8: each window is captured under ownership and is empty.
    The reader found one empty header brick and one empty content brick.
    Inspected images `native-ui-000`..`007` show only the parchment frame and
    close button. Root cause: `DescriptionBuilder.Build` switches on
    `TooltipData.Type`; the observer left it at default, so
    `TooltipType.Ability` (→ `DescriptionTemplatesAbility`) never ran.
    Classification: observer defect. No product inference is drawn from these.
  - Tracker image `native-ui-008.png`, inspected: from left, the anchor
    (current, 13/13), Cheetah, Ghaele, Nalfeshnee, Trumpet Archon, Lantern
    Archon, Dire Rat and Brachiosaurus, each its own species art; then the
    caster and hostile with native NPC pictures. HP bars, buff/condition icons,
    frames, Delay and round counter are intact. The square medallion art is
    cropped to the native aspect and readable (aesthetics: owner).
  - Observer limitation: the first player turn raised the native
    "Auto End Turn" prompt in mid-screen. It does not cover the tracker, but
    the ownership predicate did not detect this unowned modal. It was left
    unanswered, and the native settings key was restored from snapshot.
- Restoration: live tree `5C3E4AD4…` (0.0.140) restored (record
  `expanded-summoning-restoration/20261107T2333129292555Z-…`); 66 settings
  restored; 95 protected save files unchanged; tutorial flag relocked;
  selection restored; anchor out of combat; no process; no lock.

### Remaining gap and precise remedy

UI-02..UI-06 rendered descriptions. Remedy, observer only: construct the
description through the native ability path, setting
`TooltipData.Type = TooltipType.Ability` with `Ability = <live fact>` (or use
the native action-bar slot trigger's own data). Also extend the tracker
ownership predicate to refuse unowned native modals. Alternatively, the owner
can check these by hand in the retest checklist below.

### Corrections to earlier active summaries

- The portrait-zone tracker reader **did execute** on `76a9cf269310` (it
  found all seven before the pause). "Compile-checked, not yet run" applies
  only to the historical completion section.
- Ghaele: the observation found one live AI action referencing the owned ray
  and none referencing the native ray; it did not execute a ray cast.
- On `76a9cf269310` the two FAIL labels were `tracker-rendered` and the
  resulting `execution` capture exception.
- All historical verdicts stay attached to their candidates (`42cd2e26978e`,
  `221cadc7a67e`, `76a9cf269310`).

---

# Rendered-UI continuation and earlier checkpoints (historical, retained)

**R3 PARTIAL (rendered-UI continuation, 2026-11-07).** Product repairs for
UI-01..UI-06 pass every native data predicate on candidates `221cadc7a67e` and
`76a9cf269310`. On `76a9cf269310`, the native initiative tracker was observed
rendering all seven representatives' own portraits (data from the live views;
no screenshot). The capture then failed on an observer assumption: pausing did
not freeze the in-flight end of a brainless AI turn. No screenshot or
description window was captured, so rendered-screen evidence and owner visual
approval are still missing.

## Rendered-UI continuation (owner allowance 600 s)

- Intake recheck, unchanged: installed 0.0.140, DLL `00EF6757…`, live tree
  `5C3E4AD4…` (254 files), 16 mod directories, 66 settings values, 95
  protected save files (baseline `CC7CBB0D…`), reference bundle
  `7F1EDE2B…`, no Kingmaker process. Branch head `76a9cf2693`, clean.
- Focused gate `summon-playtest-r3;expanded-summoning.sprint32-native-admission;elemental-traits.exact-ray-eligibility;expanded-summoning.sprint18-append-only-identities;expanded-summoning.identities-exact`:
  8/8 PASS, 1.0 s.
- Candidate build (explicit `-ReferenceBundleDir`, same filter, 8 selected):
  384 s. Source `76a9cf2693108e349b9e2860857db00ed1feb7e2`, package
  `KingmakerGunslinger-0.0.179-development-summon-playtest-r3-76a9cf269310.zip`
  SHA-256 `d95bf1fe5c8ee51978c920a1ef1d2ac145745f57c9c6118dfd7b67bb15a55bd4`,
  DLL `a67107c5b3c3823e99b736a525af4bceea1a7b54d2d53bfc91ee9edcc1e65d51`. The
  observer is the one in those bytes (portrait-zone tracker reader).
- One guarded run `20261107T2254033252514Z-fe084a4017b34020bf0cec8df9a2aeee`
  (evidence local and private): 120 s; 15 PASS, 2 FAIL.
  - PASS: all data predicates (`portrait-roster`, `donor-portraits-untouched`,
    `initiative-portraits-live`, `lantern-rays`, `ghaele-ray-owned-grant`,
    `overrun-trample-presentation`, `metadata-rows`), plus admission, quiet
    anchor, native Default enter/restore, `owned-ui-restored` and fixture
    restore.
  - Tracker observation before the pause (`passed: true`, round 1): Dire Rat,
    Lantern Archon, Ghaele, Nalfeshnee, Brachiosaurus and Trumpet Archon each
    had a visible native order view whose `InitiativeTrackerPortraitZone`
    picture was their own `KMG_SummonPortrait_*`. The Cheetah was the current
    unit, rendered in the visible current-unit view. This is native evidence
    for the UI-01 rendered binding, not a screenshot.
  - FAIL `r3-ui-tracker-rendered`: 30 frames after `IsPaused = true`, the
    current turn had moved from the Cheetah to the Trumpet Archon, and the
    Cheetah had no visible order view (it had passed beyond the visible
    list). The capture's ownership predicate then refused to start
    (`execution` FAIL: "Native capture lost its exact request-owned UI
    state"). No screenshot, and no description windows (the run stops there).
  - Cause, classified as an **observer defect**: the observer assumed `IsPaused`
    freezes the tracker, but a brainless AI unit's turn that is already ending
    completes anyway. No product defect is indicated. Per the continuation
    brief, no second run was made.
- Restoration: guarded wrapper restored the live tree to
  `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`
  (record `expanded-summoning-restoration/20261107T2255308115213Z-…`); runner
  record shows 0.0.140 before and after, 66 settings restored, 95 protected
  save files unchanged; tutorial flag and owned UI restored; no game process.
- Spend: 505 s of the 600 s owner allowance (gate 1.0 + build 384 + run 120).
  It is recorded separately from the original R3 ledger (≈1,256 of 1,800 s).

| Item | Data | Rendered UI | Owner |
| --- | --- | --- | --- |
| UI-01 portraits | PASS | live tracker views show all seven own portraits (observation; no screenshot) | pending |
| UI-02..UI-06 | PASS | not captured (run stopped at the tracker) | pending |

---

# R3 completion continuation and first checkpoint (retained)

**R3 PARTIAL (completion continuation, 2026-11-07). Product repairs for UI-01..UI-06
are implemented and pass their data predicates natively on candidate
`221cadc7a67e`. Rendered-UI evidence is still missing:** the completion run's
tracker predicate failed because of an observer defect (it read the wrong
tracker image), before any screenshot or description window was taken. The
corrected observer is committed (compile-checked only). One build plus one
guarded run (~550 s) does not fit the remaining R3 allowance (~544 s), so it
was not launched. Owner visual approval is separate and pending throughout.

## Completion continuation (2026-11-07)

Base: PR #112 merged as `fa3eeb9e1aeea1f8d57e1e46eb784d42a2881658`; PR #113
retargeted to `master` (its diff is exactly the R3 work). Intake recheck on
DATA, unchanged from the first R3 run: installed 0.0.140, DLL
`00EF67570D30E4FF9C54A2AD2027393660436D5724BCEE2BB99E4C5718E966FD`, live tree
254 files `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`,
16 mod directories, 66 native settings values, 95 protected save files
(KMG_AUTOMATION_BASELINE `CC7CBB0D08581873…`), reference bundle manifest
`7F1EDE2BB3A01E1F94898DD7753DCAA79169354A48BBEE93EC72E85ED1281C04`,
Assembly-CSharp `3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB`.

### UI-03: the proven Ghaele grant path and ownership boundary

- Preserved evidence (inventory graph, run
  `20261021T2235009759632Z-observe-expanded-summoning-inventory`, read-only):
  native `AzataGhaelSummoned` and the KMG Ghaele each carry a unit-level
  `AddAbilityToCharacterComponent` with `Abilities=[33e89979…]`, and the two
  are distinct native objects. Engine (2.1.7b `UnitDescriptor`): at unit
  creation every ability in those components is added unconditionally. There
  is no level, form or selection condition. It is not `AddFacts`, which is why
  the first R3 swap found nothing (`factSwaps=0`).
- Ownership: `CloneUnitShell` deep-clones every direct unit component and its
  arrays (`DeepCloneUnitComponent`); only the blueprint references inside are
  shared. **Correction:** the first R3 record's claim that the KMG unit shares
  its components with the donor was wrong. `CopyBlueprintFields` alone clones
  arrays shallowly, but `ComponentsArray` is then replaced with deep clones.
- Repair: the KMG unit's own component entry now names the owned clone
  `c3224deccbd349fe833283e7cd3ed712`. Ownership is asserted at load (the
  component and its array must not be the donor's), and any other grant shape
  fails closed. The shared native brain `AzataGhaelBrain` is copied into the
  owned brain `271b9ed69a024e9dbc3bf951ad3ce792` (5 actions). Only its one
  native ray action is replaced by the owned AI `e11f40cad76e4fceb48849d5770320be`
  (`Ability` = clone); other actions and scorers are unchanged. The wrong-path
  `AddFacts` swaps were removed.
- Compatibility: the clone deep-copies the native ray's components, so action
  cost, targeting, range, delivery, 2×2d12 damage, spell resistance and
  conditions are unchanged. The description is read from that data ("Fires 2
  ranged-touch rays of light at one enemy within long range; each ray deals
  2d12 damage. Spell resistance does not apply."). Duration and Saving Throw are
  omitted (empty). The clone has no `Parent`, so the crystalline-form
  classification would not have reached the native ray through the parent
  chain. The exact clone identity was therefore added to
  `ElementalCrystallineFormPolicy`, with a focused parity assertion; ray
  recognition was not broadened.
- Existing saves: a Ghaele summoned before this change keeps its native
  (blank but functional) ray until it expires. No load fixer was added,
  because that behavior is preserved, not broken.
- Native data (candidate `221cadc7a67e`): `r3-ui-ghaele-ray-owned-grant` PASS.
  The live Ghaele has 1 clone fact and 0 native facts, its blueprint grant
  lists only the clone, its live brain casts the clone once and the native ray
  never, and the donor's component, native ray, native AI action and native
  brain are unchanged.

### UI-05/06: Overrun and Trample

- Cause: `CopyFields(native overrun → owned ability)` copied the native
  ability's Duration ("Trample") and Saving Throw (the maneuver description)
  into every owned trample and the Brachiosaurus Overrun.
- Repair (owned builders only; `ConfigureUngulateTrample` is shared by every
  owned trample): Duration is omitted (empty) for both. Overrun's Saving Throw
  is omitted, because its combat maneuver check is not a save. Trample states
  "Reflex DC N half; none for a target that makes its attack of opportunity
  instead" (plus the Stampede DC where it applies), via
  `SummonActionTooltipPolicy`.
- Icon: each Trample keeps the protected native overrun art (`Longstrider`).
  Overrun uses the installed Greater Overrun feat's art only when that sprite
  differs from Trample's, and otherwise the approved Brachiosaurus creature
  art. Native result: Overrun `KMG_SummonIcon_brachiosaurus` (texture
  `KMG_SummonIcon_Texture_brachiosaurus`) versus Trample `Longstrider`. The run
  does not show whether the installed feat was absent or shared Trample's art.
- `r3-ui-overrun-trample-presentation` PASS. The original first-run
  `r3-ui-brachiosaurus-overrun` PASS stays as the result of its original,
  more limited predicate (icon non-null only) on `42cd2e26978e`.

### Native UI observation design

- Phase `summon-playtest-r3-ui` now uses the established non-DLC Oleg outdoor
  fixture under an owned `Cleanup2NativeDefaultScope` (admission requires the
  exact Sprint32 scenario and phase, plus the grouped quiet anchor). The mansion
  storybook is never dismissed. Focused admission tests cover the allowed case
  and refused cases: another scenario, a near-miss phase name, R1/R2 phases
  unchanged, no Wall preparation, not Sprint40.
- Turn-based entry reuses the performance-combat seam: acknowledge only the
  TBMStarted tutorial flag and relock it, PauseOnEngagement off, the native
  join controller owns the combat edge, then exact restoration.
- Captures are held by real predicates: owned native Default, no narrative,
  paused, and each representative's own portrait visible in the tracker; or,
  for descriptions, the native description window bound to the exact live
  `Ability` fact with its name, description, visible icon and exactly the
  intended Duration/Saving Throw rows (empty means the row is absent), and no
  `<null>` or inherited text. Tooltip plan: `DescriptionController.HandleOpenDescription`
  with the live fact renders through the same `DescriptionTemplatesAbility`
  rows as the hover. No owned-hover admission was changed.
- **Observer defect (completion run):** the tracker predicate read
  `InitiativeTrackerUnitOrderHorizontalPCView.m_Portrait`, an unused pooled
  image (it held a stale native sprite, `Valerie_Portrait`, and was not
  visible). Engine (2.1.7b): `InitiativeTrackerUnitBaseView.SetupPortrait`
  calls `InitiativeTrackerPortraitZone.SetUnit(unit)`, which assigns
  `m_Picture.sprite = unit.Portrait.SmallPortrait`. That confirms the product
  binding. The observer now reads the zone's `m_Picture`. That fix is
  compile-checked only and has not run natively.

### Completion validation (continues the original R3 ledger)

| Step | Seconds | Result |
| --- | ---: | --- |
| Focused gate, compile error (local name clash) | 13.0 | fixed |
| Focused gate `summon-playtest-r3;expanded-summoning.native-profile;elemental-traits.exact-ray-eligibility;expanded-summoning.sprint32-native-admission` | 17.6 | 15/15 PASS |
| Candidate build (explicit `-ReferenceBundleDir`), filter `summon-playtest-r3;expanded-summoning.sprint32-native-admission;elemental-traits.exact-ray-eligibility;expanded-summoning.sprint18-append-only-identities;expanded-summoning.identities-exact` | 394 | PASS, 8 selected |
| Guarded native phase, including deploy/restore | 152 | 13 PASS, 2 FAIL (`tracker-rendered`, `execution`) |
| Observer fix: focused gate `summon-playtest-r3;expanded-summoning.sprint32-native-admission` | 15.4 | 5/5 PASS (compile only) |
| **Continuation total** | **≈592 (+≈3 wrapper)** | |
| **Cumulative R3** | **≈1,256 of 1,800** | ≈544 s left |

Candidate: source `221cadc7a67ee39be8527c34dd9d7bfd79f890b2`, package
`KingmakerGunslinger-0.0.179-development-summon-playtest-r3-221cadc7a67e.zip`
SHA-256 `6c338b28c11900cccc206b13855582ddebec35c0d7fca68ad17b78efc9a8502e`, DLL
`2129b7044c0e659ba7892f392e90d7a6efcd79e0e1bcbaa7b910e13c29468eb4` (MVID
`f3b47ae0-7666-46af-a244-540da4834f07`). Native run
`20261107T2239493258556Z-d5a2547302ea48ab911c5a83b420675f` (evidence local and
private). Observer identity: the R3 observer as compiled into that DLL. The
corrected tracker observer is a later source change and is **not** in these
bytes.

Native results on `221cadc7a67e`, all data predicates: PASS for
`portrait-roster` (134 units, 0 wrong), `donor-portraits-untouched`,
`initiative-portraits-live`, `lantern-rays` (distinct sprite and texture),
`ghaele-ray-owned-grant`, `overrun-trample-presentation` and `metadata-rows`,
plus every admission, anchor and restore predicate. FAIL: `tracker-rendered`
(observer defect above) and `execution`. No screenshot was captured.

Restoration (completion run): guarded wrapper restored the live tree to
`5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB` (0.0.140),
record `expanded-summoning-restoration/20261107T2241453559137Z-…`; 66 settings
restored; 95 protected save files byte-identical; `r3-ui-owned-ui-restored`
PASS (tutorial flag relocked, flags exact); no Kingmaker process remains.

### Status by item (data / rendered UI / owner visual approval)

| Item | Data | Rendered UI | Owner |
| --- | --- | --- | --- |
| UI-01 portraits | PASS (`221cadc7a67e`) | not established (observer defect; no screen) | pending |
| UI-02 Lantern icons | PASS | not captured | pending |
| UI-03 Ghaele ray | PASS (owned grant path, brain, donor unchanged) | not captured | pending |
| UI-04 Nimbus icon | PASS | not captured | pending |
| UI-05 Overrun icon | PASS (distinct from Trample) | not captured | pending |
| UI-06 tooltip rows | PASS (Sprint, Nimbus, Trumpet, Overrun, Trample) | not captured | pending |

### Corrections to the first R3 record

- The shared-component claim (finding 2 below) was wrong; see the ownership
  boundary above.
- Attribution: the first record's "96/96" came from the focused gate filter
  `summon-playtest-r3;summon-playtest-r2;expanded-summoning.sprint32-native-admission;expanded-summoning.sprint18-append-only-identities;expanded-summoning.identities-exact;brown-fur`
  (96 selected). The filter printed with the candidate (13 selected) is the
  build filter. They are distinct invocations.
- The first record's UI-03 finding ("grant path unidentified") is resolved
  above. Its FAIL and the unusable mansion screenshots on `42cd2e26978e` stay
  as recorded.

---

# First R3 checkpoint (retained)

**R3 CHECKPOINT — PARTIAL; NOT COMPLETE.** One development candidate, one
guarded save-free native run on DATA. 9 of 10 R3 native predicates PASS;
`r3-ui-ghaele-ray-named` FAIL. Visual (screenshot) evidence for the initiative
tracker and action bar is unavailable: every capture was covered by the
new-game prologue storybook. UI-05 has a newly observed tooltip-text defect.
A narrow R3 repair is required before R4.

Records: [MISSION](MISSION.md), [ISSUE-REGISTER](ISSUE-REGISTER.md)
(UI-01..UI-06), [R2-STATUS](R2-STATUS.md).

## Intake (DATA, 2026-11-07)

- Base: `codex/summon-playtest-r2-lifecycle` head
  `ba254d734a8b515bfe6d4bda9e96538266882d5a` (draft PR #112, R2 closed by
  owner acceptance, not merged). Branch `codex/summon-playtest-r3-ui`.
  The R3 PR depends on #112.
- Installed live mod before: version 0.0.140, live tree 254 files,
  SHA-256 `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`.
- Native settings snapshot: 66 values. Protected saves: 95 files hashed
  (KMG_AUTOMATION_BASELINE immutable; this phase is save-free and loads no save).

## Scope

Presentation and metadata only (UI-01..UI-06). No mechanics changed; nothing
from R4+; R1/R2 not reopened. Native donor blueprints are read, never written.

## Changes

- **UI-01 portraits.** Consumer traced: the turn-based initiative tracker
  (`InitiativeTrackerUnitVM` / `UnitOrderHorizontalPCView`) reads
  `unit.Portrait.SmallPortrait`; `UnitUISettings.Portrait` resolves
  `m_CustomPortrait`, then `m_Portrait`, then `Blueprint.PortraitSafe`. The
  unit-level fields are serialized into saves, so they are left empty. KMG
  summon units were cloned from native donors by `CopyBlueprintFields` and so
  carried the donor's `BlueprintPortrait` (Dire Rat → wolf etc.). New
  `ExpandedSummoningPortraitBuilder` gives each registered KMG summon unit its
  own runtime `BlueprintPortrait` built from its approved 128 px creature icon
  (`ExpandedSummoningProjectIcons.Require(key)`), center-cropped to 98×128 to
  match the native 185×242 small-portrait aspect; only the KMG unit's private
  `m_Portrait` is set. Donor portrait objects/sprites are untouched; no global
  getter patch; native overlays (HP bar, status, selection frame) are untouched
  because only the sprite source changes. Policy: `SummonPortraitPolicy`.
- **UI-02 Lantern Archon.** Light Ray → native Searing Light icon
  (`bf0accce250381a44b857d4af6c8e10d`, sprite `SearingLight`); Twin Light Rays
  → icon of native `1fca0ba2fdfe2994a8c8bc1f0f2fc5b1` (sprite `ScorchingRay`).
  Distinct icons for the two actions.
- **UI-03 unnamed blast.** Owner identified: native `AzataGhaelLightRay`
  `33e8997912cf76b4c99dca0445082804` (no name, no description, no icon) on the
  KMG Ghaele `KMG.Summoning.Unit.GhaeleAzata`
  (`e7eccabdc8cd474d86475586792c03c7`), cloned from native
  `AzataGhaelSummoned` `bc8ca1437c0f48948b317b7e64febf0d`. Not the Trumpet
  Blast (already named/iconed; Personal range). Repair attempt: an owned clone
  `KMG.Summoning.Special.GhaeleAzata.LightRay`
  (`c3224deccbd349fe833283e7cd3ed712`, "Light Ray", description generated from
  the ray's own data, Searing Light icon), AI action
  `…LightRayAi` (`e11f40cad76e4fceb48849d5770320be`), brain `…Brain`
  (`271b9ed69a024e9dbc3bf951ad3ce792`), swapped into the KMG unit's own
  references. **Not effective natively** (see Results).
- **UI-04 Unholy Nimbus.** Native Unholy Aura ability icon
  (`47f9cb1c367a5e4489cfa32fce290f86`, sprite `ProtectionFromGood`).
- **UI-05 Brachiosaurus Overrun.** The builder no longer clears the copied
  icon: native overrun ability icon, falling back to the installed Greater
  Overrun feat icon, then creature art. Native result: sprite `Longstrider`.
- **UI-06 null rows.** Cheetah Sprint: Duration "1 round", Saving Throw
  "None". Unholy Nimbus: "Bursts after 1 round; dazed 1d10 rounds" /
  "Will negates (non-demons)". Sibling Trumpet Blast (same fresh-shell
  pattern): "1d4 rounds" / "Fortitude negates".
- Identities: 3 new private identities (above) appended; SpecialIdentityCount
  789 → 792; icon-catalog registry SHA updated; identity-count pins updated.
- Native observer: save-free Sprint32 phase `summon-playtest-r3-ui`
  (`RuntimeTestRunner.SummonPlaytestR3Ui.cs`).

## Validation (R3 ledger, separate from R2)

| Step | Seconds | Result |
| --- | ---: | --- |
| Focused gate compile attempts (3 failing: usings/API) | 18.0 | fixed |
| Focused gate (compile + tests) | 17.9 | PASS |
| Full domain run (pin discovery) | 79.9 | 2858; 2 identity-count pins FAIL → updated |
| Focused gate reruns (identity pins) | 6.2 | 96/96 PASS |
| `validate_icon_catalog.py` + `test_icon_catalog.py` | 12.0 | PASS (30 tests) |
| Candidate build (`Build-Local -DevelopmentPurpose summon-playtest-r3`, explicit `-ReferenceBundleDir`) | 391 | PASS, 13 selected tests |
| Guarded native phase incl. deploy/restore | 136 | 9/10 PASS, 1 FAIL |
| **Total** | **≈661 (≈11.0 min)** | target 15 min, ceiling 30 min |

Candidate: source `42cd2e26978e8747ab6a7f4059ae9f92b52ac163`,
`KingmakerGunslinger-0.0.179-development-summon-playtest-r3-42cd2e26978e.zip`,
package SHA-256 `8dc227c35d5a5cfc43bf8104bcfb1b0ae8cf4038fe3d8e161aa2cca38371b5d2`,
DLL SHA-256 `36e952bf24aeb63dddbc43a0f2c997577259131fa77cbd1627762b3d7251de96`.
Focused filter `summon-playtest-r3;summon-playtest-r2;expanded-summoning.sprint32-native-admission;expanded-summoning.sprint18-append-only-identities;expanded-summoning.identities-exact`.

Native run `20261107T2141577269449Z-f62d35d82770459b87ca3113fae6c7e9`
(evidence local/private), loaded mod 0.0.179, 77 s in game:

| Predicate | Result | Observation |
| --- | --- | --- |
| r3-ui-portrait-roster | PASS | 134 registered units (133 published + withheld Stirge), 0 wrong |
| r3-ui-donor-portraits-untouched | PASS | 72 distinct donors, 0 touched |
| r3-ui-initiative-portraits-live | PASS | 7 live summons: `unit.Portrait.SmallPortrait` = own art; no unit-level portrait |
| r3-ui-lantern-rays | PASS | `SearingLight` / `ScorchingRay` |
| r3-ui-ghaele-ray-named | **FAIL** | `factSwaps=0;brainSwap=False;nativeBrain=AzataGhaelBrain`; native blank ray still on the KMG Ghaele; clone not granted |
| r3-ui-nalfeshnee-nimbus | PASS | icon `ProtectionFromGood`; real rows |
| r3-ui-brachiosaurus-overrun | PASS (predicate) | icon `Longstrider` — see defect below |
| r3-ui-cheetah-sprint | PASS | "1 round" / "None" |
| r3-ui-trumpet-blast-sibling | PASS | "1d4 rounds" / "Fortitude negates" |
| sprint32-fixture-restored / profile-restored | PASS | |

Turn-based captures: all 7 representatives reached their own `Acting` turn in
round 1 with the tracker's portrait source reporting their own art, and 7 PNGs
were captured. **Personally inspected: every image shows the new-game prologue
storybook ("The Road to Glory") covering the screen; no initiative tracker or
action bar is visible.** These screenshots are not visual evidence for any UI
item. Hover tooltips were not captured (hover capture is admitted only for
Cleanup 2 phases), so tooltip rows are verified from live blueprint strings.

## Findings and limitations

1. **UI-03 FAIL.** The native ray does not reach the KMG Ghaele through the
   unit's `AddFacts` array or a unit-level `AddFacts` component, so no swap
   occurred. The actual grant path (for example a nested feature,
   `AddClassLevels` selection, or another component) is unidentified. The
   owned clone/AI/brain identities exist but are unused.
2. **Latent donor-mutation hazard in the UI-03 code.** `CopyBlueprintFields`
   clones `ComponentsArray` as an array but shares the component objects with
   the donor, so the component-level swap would write into the native donor's
   `AddFacts` component if it ever matched. It matched nothing on this run
   (no donor was changed), but it must be removed or made clone-first.
3. **UI-05 tooltip text defect (new).** Overrun now carries the native overrun
   icon `Longstrider`, identical to Trample's. Both Overrun and Trample show
   Duration "Trample" and the trample maneuver description in the Saving Throw
   row (inherited native field text). Not a literal `<null>`, but wrong.
4. **Visual gap.** No native screen of the initiative tracker or action bar;
   the save-free fixture starts under the prologue storybook overlay.
5. Sibling inspection: the only rows without Duration/Saving Throw objects on
   the 7 representatives are native activatable toggles (Power Attack, Fight
   Defensively, Combat Expertise, Bull Rush, Mobility, Ghaele Gaze, Reject
   Rage), which do not carry those fields. Native `RejectRageActivatableAbility`
   has an empty icon; native, out of scope.
6. Unholy Nimbus reuses Unholy Aura's art (sprite name `ProtectionFromGood`);
   owner visual approval is separate.

## Restoration

Guarded wrapper restored the live mod tree to
`5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB` (0.0.140,
verified). Settings restored (66 values). 95 protected save files
byte-identical before/after. No Kingmaker process remains. No lease left in
recovery.

## Portrait audit (UI-01)

Consumer binding: initiative tracker → `unit.Portrait.SmallPortrait` →
`UnitUISettings.Portrait` → (`m_CustomPortrait` ∅) → (`m_Portrait` ∅) →
`BlueprintUnit.PortraitSafe` → KMG `m_Portrait` (R3). "Before" is the donor
unit/portrait the KMG unit inherited; "after" is the KMG portrait sprite read
natively from the live library.

| Key | Published | KMG unit | Unit GUID | Before (donor unit / portrait) | After (small sprite) | Native |
| --- | --- | --- | --- | --- | --- | --- |
| `dire-rat` | yes | `KMG_Summoning_Unit_DireRat` | `e7e43c80489a48e1b562fe6ac11c33ee` | `DogSummoned` / `Dog` | `KMG_SummonPortrait_dire-rat` | PASS |
| `dog` | yes | `KMG_Summoning_Unit_Dog` | `e8c90cb29374455cb6301e4fa7d1f837` | `DogSummoned` / `Dog` | `KMG_SummonPortrait_dog` | PASS |
| `eagle` | yes | `KMG_Summoning_Unit_Eagle` | `7383db28c1d74dce98533ddc257a2e3c` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_eagle` | PASS |
| `poisonous-frog` | yes | `KMG_Summoning_Unit_PoisonousFrog` | `e1a8e5e206154dd48b6aca1d4262e8e7` | `CR4_GiantFrogPoisonous` / `GiantFrog` | `KMG_SummonPortrait_poisonous-frog` | PASS |
| `pony` | yes | `KMG_Summoning_Unit_Pony` | `67a661d7b39e44bc811ad9ff59120110` | `PonySummoned` / `Empty_Portrait` | `KMG_SummonPortrait_pony` | PASS |
| `fire-beetle` | yes | `KMG_Summoning_Unit_FireBeetle` | `714b04102eae4cd2be200a8f09f04bc3` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_fire-beetle` | PASS |
| `giant-centipede` | yes | `KMG_Summoning_Unit_GiantCentipede` | `baf9e8f829e9410db8f3d200bb62a2c6` | `CR0.5_GiantCentipedeStandard` / `Centipede` | `KMG_SummonPortrait_giant-centipede` | PASS |
| `wolf` | yes | `KMG_Summoning_Unit_Wolf` | `48d7740de99b4040bad6b6493a91f42b` | `WolfSummon` / `WolfBlack` | `KMG_SummonPortrait_wolf` | PASS |
| `giant-frog` | yes | `KMG_Summoning_Unit_GiantFrog` | `ca53b4bba8f04497acf3d36440a6e942` | `GiantFrogSummoned` / `GiantFrog` | `KMG_SummonPortrait_giant-frog` | PASS |
| `giant-spider` | yes | `KMG_Summoning_Unit_GiantSpider` | `4a3cd49e751448c8b8836485b262fdf1` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-spider` | PASS |
| `small-air-elemental` | yes | `KMG_Summoning_Unit_SmallAirElemental` | `5184af966fa84c6ebf42f6f941d5d533` | `SummonedAirElementalSmall` / `ElementalAir` | `KMG_SummonPortrait_small-air-elemental` | PASS |
| `small-earth-elemental` | yes | `KMG_Summoning_Unit_SmallEarthElemental` | `1830865b016a486db1de7f99e8b9f997` | `SummonedEarthElementalSmall` / `ElementalEarth` | `KMG_SummonPortrait_small-earth-elemental` | PASS |
| `small-fire-elemental` | yes | `KMG_Summoning_Unit_SmallFireElemental` | `6a95de017d0949708fd32548207f8195` | `SummonedFireElementalSmall` / `ElementalFire` | `KMG_SummonPortrait_small-fire-elemental` | PASS |
| `small-water-elemental` | yes | `KMG_Summoning_Unit_SmallWaterElemental` | `daa6a4f74317475da5a42c83082b7cdd` | `SummonedWaterElementalSmall` / `ElementalWater` | `KMG_SummonPortrait_small-water-elemental` | PASS |
| `goblin-dog` | yes | `KMG_Summoning_Unit_GoblinDog` | `f1584066792a436fa3a5ba0b3731b481` | `CR2_WorgStandart` / `Worg` | `KMG_SummonPortrait_goblin-dog` | PASS |
| `hyena` | yes | `KMG_Summoning_Unit_Hyena` | `ec12fab8be5c412d8ee824d15a6621d0` | `WolfSummon` / `WolfBlack` | `KMG_SummonPortrait_hyena` | PASS |
| `horse` | yes | `KMG_Summoning_Unit_Horse` | `629fa53bf40d4b4b9828f57cf81194e9` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_horse` | PASS |
| `giant-ant-worker` | yes | `KMG_Summoning_Unit_GiantAntWorker` | `9db94712f7b540679b8ff650dbaf746a` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-ant-worker` | PASS |
| `boar` | yes | `KMG_Summoning_Unit_Boar` | `d98699ea265441a09c5b9b51769ef7ce` | `CR2_BoarStandard` / `Boar` | `KMG_SummonPortrait_boar` | PASS |
| `leopard` | yes | `KMG_Summoning_Unit_Leopard` | `f36aad1d463444c69950f219457b894f` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_leopard` | PASS |
| `monitor-lizard` | yes | `KMG_Summoning_Unit_MonitorLizard` | `10b148d216364b509cd2c665a47b2950` | `MonitorLizardSummoned` / `MonitorLizard` | `KMG_SummonPortrait_monitor-lizard` | PASS |
| `cheetah` | yes | `KMG_Summoning_Unit_Cheetah` | `6eb29e76792c41c5ab9e3812fe4b66e0` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_cheetah` | PASS |
| `crocodile` | yes | `KMG_Summoning_Unit_Crocodile` | `ed3fa562802b418ab062a7da622874da` | `MonitorLizardSummoned` / `MonitorLizard` | `KMG_SummonPortrait_crocodile` | PASS |
| `dire-bat` | yes | `KMG_Summoning_Unit_DireBat` | `d867cb795b5640219a8362661f447697` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_dire-bat` | PASS |
| `wolverine` | yes | `KMG_Summoning_Unit_Wolverine` | `f640d3e77d7d4a0d8de3351129cd7148` | `CR2_WorgStandart` / `Worg` | `KMG_SummonPortrait_wolverine` | PASS |
| `giant-ant-soldier` | yes | `KMG_Summoning_Unit_GiantAntSoldier` | `ef2cd2abd3254cb2a90952253add918a` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-ant-soldier` | PASS |
| `lantern-archon` | yes | `KMG_Summoning_Unit_LanternArchon` | `02f8e9c6c91549deaded9ef667399449` | `CR6_WillOWispStandart` / `Empty_Portrait` | `KMG_SummonPortrait_lantern-archon` | PASS |
| `giant-crab` | yes | `KMG_Summoning_Unit_GiantCrab` | `5965d242ffe1409d98e38f2793543aad` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-crab` | PASS |
| `dire-boar` | yes | `KMG_Summoning_Unit_DireBoar` | `1710475cee544d9d858b05b24fb3ad4c` | `DireBoarSummoned` / `Boar` | `KMG_SummonPortrait_dire-boar` | PASS |
| `dire-wolf` | yes | `KMG_Summoning_Unit_DireWolf` | `d2e7b46ea8994f7085063abac3775142` | `DireWolfSummon` / `WolfBlack` | `KMG_SummonPortrait_dire-wolf` | PASS |
| `grizzly-bear` | yes | `KMG_Summoning_Unit_GrizzlyBear` | `f7370368039e41ba8f88e8218cfb39d0` | `CR4_BearStandard` / `Bear` | `KMG_SummonPortrait_grizzly-bear` | PASS |
| `medium-air-elemental` | yes | `KMG_Summoning_Unit_MediumAirElemental` | `d8e45b6e036b4723a0fc2c1c074fcaea` | `SummonedAirElementalMedium` / `ElementalAir` | `KMG_SummonPortrait_medium-air-elemental` | PASS |
| `medium-earth-elemental` | yes | `KMG_Summoning_Unit_MediumEarthElemental` | `fc989a8d68e24250bb613b8c7ac20238` | `SummonedEarthElementalMedium` / `ElementalEarth` | `KMG_SummonPortrait_medium-earth-elemental` | PASS |
| `medium-fire-elemental` | yes | `KMG_Summoning_Unit_MediumFireElemental` | `c75e8614029540dda8a985c7cba4ac00` | `SummonedFireElementalMedium` / `ElementalFire` | `KMG_SummonPortrait_medium-fire-elemental` | PASS |
| `medium-water-elemental` | yes | `KMG_Summoning_Unit_MediumWaterElemental` | `d1889a6ad861465cacd64508669ef7da` | `SummonedWaterElementalMedium` / `ElementalWater` | `KMG_SummonPortrait_medium-water-elemental` | PASS |
| `air-mephit` | yes | `KMG_Summoning_Unit_AirMephit` | `9de31a332f814795b7258bd1d3813678` | `MephitAirSummoned` / `Mephit` | `KMG_SummonPortrait_air-mephit` | PASS |
| `earth-mephit` | yes | `KMG_Summoning_Unit_EarthMephit` | `2ddb201a43134893bb626aba291a83eb` | `MephitEarthSummoned` / `Mephit_03` | `KMG_SummonPortrait_earth-mephit` | PASS |
| `fire-mephit` | yes | `KMG_Summoning_Unit_FireMephit` | `a841652d2c7242d6849a9b74982710f8` | `MephitFireSummoned` / `Mephit_04` | `KMG_SummonPortrait_fire-mephit` | PASS |
| `water-mephit` | yes | `KMG_Summoning_Unit_WaterMephit` | `556264c328834ffcbd0dddd86a93ac2b` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_water-mephit` | PASS |
| `dust-mephit` | yes | `KMG_Summoning_Unit_DustMephit` | `30f6615badf14d21b41d6468d5c93ac3` | `MephitAirSummoned` / `Mephit` | `KMG_SummonPortrait_dust-mephit` | PASS |
| `ice-mephit` | yes | `KMG_Summoning_Unit_IceMephit` | `b37cdd78c8cc43be8799e9ffd6953ba2` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_ice-mephit` | PASS |
| `magma-mephit` | yes | `KMG_Summoning_Unit_MagmaMephit` | `ce9551f1413d4fdcb59941734d4546b1` | `MephitFireSummoned` / `Mephit_04` | `KMG_SummonPortrait_magma-mephit` | PASS |
| `ooze-mephit` | yes | `KMG_Summoning_Unit_OozeMephit` | `7cbf19a6c83f4b8e849d17b8a54e6d51` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_ooze-mephit` | PASS |
| `salt-mephit` | yes | `KMG_Summoning_Unit_SaltMephit` | `8c8fe6eb536b450a9a2c10b8cbef9fbe` | `MephitAirSummoned` / `Mephit` | `KMG_SummonPortrait_salt-mephit` | PASS |
| `steam-mephit` | yes | `KMG_Summoning_Unit_SteamMephit` | `047f68ee53554d8ab1702de189ecd85d` | `MephitWaterSummoned` / `Mephit_02` | `KMG_SummonPortrait_steam-mephit` | PASS |
| `lion` | yes | `KMG_Summoning_Unit_Lion` | `14c7cbb0f32f4e30bbefaeaccba10269` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_lion` | PASS |
| `tiger` | yes | `KMG_Summoning_Unit_Tiger` | `5182125341e34ae98491eaf991bc0ef3` | `LeopardSummoned` / `Leopard` | `KMG_SummonPortrait_tiger` | PASS |
| `pteranodon` | yes | `KMG_Summoning_Unit_Pteranodon` | `c9a94142c9164ab793f7a06ae3fdcf56` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_pteranodon` | PASS |
| `hell-hound` | yes | `KMG_Summoning_Unit_HellHound` | `f8c0664ec7204643be3ab7a1ac16a391` | `HellhoundSummoned` / `HellHound` | `KMG_SummonPortrait_hell-hound` | PASS |
| `owlbear` | yes | `KMG_Summoning_Unit_Owlbear` | `7bde6f94222c47f687ed68c752cb24cb` | `CR8_OwlbearStandard` / `OwlBearBrown` | `KMG_SummonPortrait_owlbear` | PASS |
| `giant-ant-drone` | yes | `KMG_Summoning_Unit_GiantAntDrone` | `9387ef0ca1cd403a985893a85a6c7806` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-ant-drone` | PASS |
| `giant-stag-beetle` | yes | `KMG_Summoning_Unit_GiantStagBeetle` | `fc294cc691284525814324ac5bc5bf75` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-stag-beetle` | PASS |
| `large-air-elemental` | yes | `KMG_Summoning_Unit_LargeAirElemental` | `6fce34709ee54f5a85fd75ea5c6d30be` | `SummonedAirElementalLarge` / `ElementalAir` | `KMG_SummonPortrait_large-air-elemental` | PASS |
| `large-earth-elemental` | yes | `KMG_Summoning_Unit_LargeEarthElemental` | `4fee8d80c1944b1f96d905aaf15c61b3` | `SummonedEarthElementalLarge` / `ElementalEarth` | `KMG_SummonPortrait_large-earth-elemental` | PASS |
| `large-fire-elemental` | yes | `KMG_Summoning_Unit_LargeFireElemental` | `e9371f3a9bdd4f8780db58cdefb95292` | `SummonedFireElementalLarge` / `ElementalFire` | `KMG_SummonPortrait_large-fire-elemental` | PASS |
| `large-water-elemental` | yes | `KMG_Summoning_Unit_LargeWaterElemental` | `2c6483c5025d4dcba441cc4c805fa784` | `SummonedWaterElementalLarge` / `ElementalWater` | `KMG_SummonPortrait_large-water-elemental` | PASS |
| `dire-lion` | yes | `KMG_Summoning_Unit_DireLion` | `56c64aa6765a4c37a1b30c0c5b31427b` | `SmilodonSummoned` / `Smilodon` | `KMG_SummonPortrait_dire-lion` | PASS |
| `ankylosaurus` | yes | `KMG_Summoning_Unit_Ankylosaurus` | `10c80d5cfa594332bd3e5127799e426f` | `CR6_HodagStandard` / `Hodag` | `KMG_SummonPortrait_ankylosaurus` | PASS |
| `bralani-azata` | yes | `KMG_Summoning_Unit_BralaniAzata` | `fb640167f05541f2963de3d31e217d11` | `AzataBralaniSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_bralani-azata` | PASS |
| `salamander` | yes | `KMG_Summoning_Unit_Salamander` | `f8fb103168d74b4c93182437e5d2b4e4` | `CR1_LizardfolkStandard` / `Lizardfolk` | `KMG_SummonPortrait_salamander` | PASS |
| `cyclops` | yes | `KMG_Summoning_Unit_Cyclops` | `a85dfadbca3340f4aad99c1cc36e40b3` | `CR5_CyclopStandard` / `Cyclop2H` | `KMG_SummonPortrait_cyclops` | PASS |
| `dire-bear` | yes | `KMG_Summoning_Unit_DireBear` | `11b15a81cea1498babfdb57af1b53c41` | `CR7_BearDire` / `Bear` | `KMG_SummonPortrait_dire-bear` | PASS |
| `dire-tiger` | yes | `KMG_Summoning_Unit_DireTiger` | `d15ee151c2274f9f86ab523f111bc3af` | `SmilodonSummoned` / `Smilodon` | `KMG_SummonPortrait_dire-tiger` | PASS |
| `huge-air-elemental` | yes | `KMG_Summoning_Unit_HugeAirElemental` | `f6b8223178ca4ec7b9b8aac61f1fd0f8` | `SummonedAirElementalHuge` / `ElementalAir` | `KMG_SummonPortrait_huge-air-elemental` | PASS |
| `huge-earth-elemental` | yes | `KMG_Summoning_Unit_HugeEarthElemental` | `f8ba7baab64e42b2951d52e227a20b4f` | `SummonedEarthElementalHuge` / `ElementalEarth` | `KMG_SummonPortrait_huge-earth-elemental` | PASS |
| `huge-fire-elemental` | yes | `KMG_Summoning_Unit_HugeFireElemental` | `fe3b06c44a2d4d36be8f549b14042f73` | `SummonedFireElementalHuge` / `ElementalFire` | `KMG_SummonPortrait_huge-fire-elemental` | PASS |
| `huge-water-elemental` | yes | `KMG_Summoning_Unit_HugeWaterElemental` | `974d58335c9149b687140d09ef8e47a2` | `SummonedWaterElementalHuge` / `ElementalWater` | `KMG_SummonPortrait_huge-water-elemental` | PASS |
| `elephant` | yes | `KMG_Summoning_Unit_Elephant` | `9dd8544097234f05bcd35d400d91b510` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_elephant` | PASS |
| `erinyes-devil` | yes | `KMG_Summoning_Unit_ErinyesDevil` | `bf08d0f66ffc4ed78dd40e5f7e68ae9d` | `CR8_ErinyesDevilStandard` / `Empty_Portrait` | `KMG_SummonPortrait_erinyes-devil` | PASS |
| `invisible-stalker` | yes | `KMG_Summoning_Unit_InvisibleStalker` | `3cd7b4f2c65b4d35929dd4d969dfaa41` | `SummonedAirElementalMedium` / `ElementalAir` | `KMG_SummonPortrait_invisible-stalker` | PASS |
| `shadow-demon` | yes | `KMG_Summoning_Unit_ShadowDemon` | `627c1841a5eb4e32b1a94c0f43ec8a60` | `SoulEaterSummoned` / `SoulEater` | `KMG_SummonPortrait_shadow-demon` | PASS |
| `shadow-mastiff` | yes | `KMG_Summoning_Unit_ShadowMastiff` | `c0c955211eda4de18df0dcb4870eede2` | `CR2_WorgStandart` / `Worg` | `KMG_SummonPortrait_shadow-mastiff` | PASS |
| `succubus` | yes | `KMG_Summoning_Unit_Succubus` | `0c908145873f4b67a188397ca5f46da1` | `CR7_Nymph` / `Nymph` | `KMG_SummonPortrait_succubus` | PASS |
| `shambling-mound` | yes | `KMG_Summoning_Unit_ShamblingMound` | `c0680a67c42f4d17b3b55c011a8f9c56` | `CR6_ShamblingMound` / `ShamblingMound` | `KMG_SummonPortrait_shambling-mound` | PASS |
| `greater-air-elemental` | yes | `KMG_Summoning_Unit_GreaterAirElemental` | `c35e1370ca28417098d6a3b9835b0ba5` | `SummonedAirElementalGreater` / `ElementalAir` | `KMG_SummonPortrait_greater-air-elemental` | PASS |
| `greater-earth-elemental` | yes | `KMG_Summoning_Unit_GreaterEarthElemental` | `17c4fcc106df43af980fcea7a0877e04` | `SummonedEarthElementalGreater` / `ElementalEarth` | `KMG_SummonPortrait_greater-earth-elemental` | PASS |
| `greater-fire-elemental` | yes | `KMG_Summoning_Unit_GreaterFireElemental` | `7c7c5326601c4b2e8237c33969c47914` | `SummonedFireElementalGreater` / `ElementalFire` | `KMG_SummonPortrait_greater-fire-elemental` | PASS |
| `greater-water-elemental` | yes | `KMG_Summoning_Unit_GreaterWaterElemental` | `72d45b40b6764ab090a3dff639aa678a` | `SummonedWaterElementalGreater` / `ElementalWater` | `KMG_SummonPortrait_greater-water-elemental` | PASS |
| `mastodon` | yes | `KMG_Summoning_Unit_Mastodon` | `e129ffc2768d47c5bee61bb99b0c8703` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_mastodon` | PASS |
| `roc` | yes | `KMG_Summoning_Unit_Roc` | `439e955cb0fd41daafd0478d3641615a` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_roc` | PASS |
| `dire-crocodile` | yes | `KMG_Summoning_Unit_DireCrocodile` | `95bd522bb3a444d49986ba12b7822f10` | `MonitorLizardSummoned` / `MonitorLizard` | `KMG_SummonPortrait_dire-crocodile` | PASS |
| `bebelith` | yes | `KMG_Summoning_Unit_Bebelith` | `9d7d36e71ea141258509b8a32557577e` | `CR7_GiantSpiderDoombringing` / `GiantSpider` | `KMG_SummonPortrait_bebelith` | PASS |
| `giant-flytrap` | yes | `KMG_Summoning_Unit_GiantFlytrap` | `287490e71bed46bd92adef60489ecb4c` | `CR10_GiantFlytrapStandard` / `GiantFlytrap` | `KMG_SummonPortrait_giant-flytrap` | PASS |
| `elder-air-elemental` | yes | `KMG_Summoning_Unit_ElderAirElemental` | `1243adb4aecc4eb5ae54b2b54cfe1dec` | `SummonedAirElementalElder` / `ElementalAir` | `KMG_SummonPortrait_elder-air-elemental` | PASS |
| `elder-earth-elemental` | yes | `KMG_Summoning_Unit_ElderEarthElemental` | `2bdfdec402384e689351bad9a0d2d8af` | `SummonedEarthElementalElder` / `ElementalEarth` | `KMG_SummonPortrait_elder-earth-elemental` | PASS |
| `elder-fire-elemental` | yes | `KMG_Summoning_Unit_ElderFireElemental` | `20116595e2554c758a29b3b32553f0e5` | `SummonedFireElementalElder` / `ElementalFire` | `KMG_SummonPortrait_elder-fire-elemental` | PASS |
| `elder-water-elemental` | yes | `KMG_Summoning_Unit_ElderWaterElemental` | `54acc1934a55479ca43e826ed6e7d14c` | `SummonedWaterElementalElder` / `ElementalWater` | `KMG_SummonPortrait_elder-water-elemental` | PASS |
| `purple-worm` | yes | `KMG_Summoning_Unit_PurpleWorm` | `8739fe771e5b4f3fa3acbb10dd969892` | `PurpleWormSummoned` / `PurpleWorm` | `KMG_SummonPortrait_purple-worm` | PASS |
| `ghaele-azata` | yes | `KMG_Summoning_Unit_GhaeleAzata` | `e7eccabdc8cd474d86475586792c03c7` | `AzataGhaelSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_ghaele-azata` | PASS |
| `pixie` | yes | `KMG_Summoning_Unit_Pixie` | `396881ade24e4ddba188dc2e7ff481f9` | `CR1_Nixie` / `Nixie` | `KMG_SummonPortrait_pixie` | PASS |
| `giant-wasp` | yes | `KMG_Summoning_Unit_GiantWasp` | `156a063aaec7449992d2021d53341085` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_giant-wasp` | PASS |
| `stirge` | no (withheld) | `KMG_Summoning_Unit_Stirge` | `c49f994ea2a840d98108b2848781774c` | `CR3_GiantEagleStandard` / `GiantEagle` | `KMG_SummonPortrait_stirge` | PASS |
| `aurochs` | yes | `KMG_Summoning_Unit_Aurochs` | `40bf3f1e2b094e83ba2ab498c3d5d603` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_aurochs` | PASS |
| `bison` | yes | `KMG_Summoning_Unit_Bison` | `d1f0439382454d908f2086dda33eafcc` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_bison` | PASS |
| `rhinoceros` | yes | `KMG_Summoning_Unit_Rhinoceros` | `e0d2c0f0589e467792aff319fdb2b6f9` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_rhinoceros` | PASS |
| `woolly-rhinoceros` | yes | `KMG_Summoning_Unit_WoollyRhinoceros` | `8c4a8e045ca844a8bd42f614707b8e74` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_woolly-rhinoceros` | PASS |
| `viper` | yes | `KMG_Summoning_Unit_Viper` | `d8be82543ab64dc988c33e9f13608bad` | `PurpleWormSummoned` / `PurpleWorm` | `KMG_SummonPortrait_viper` | PASS |
| `constrictor-snake` | yes | `KMG_Summoning_Unit_ConstrictorSnake` | `f1a2eadf588e4c3b9fb670724d706364` | `PurpleWormSummoned` / `PurpleWorm` | `KMG_SummonPortrait_constrictor-snake` | PASS |
| `ape` | yes | `KMG_Summoning_Unit_Ape` | `c53c3e23097e4f25a411c50ce2868c60` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_ape` | PASS |
| `dire-ape` | yes | `KMG_Summoning_Unit_DireApe` | `5482b49785a3492aa7d29a4ee575cd66` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_dire-ape` | PASS |
| `giant-scorpion` | yes | `KMG_Summoning_Unit_GiantScorpion` | `4b5ec54e8e7a4e57b838541006affd3d` | `GiantSpiderSummoned` / `GiantSpider` | `KMG_SummonPortrait_giant-scorpion` | PASS |
| `girallon` | yes | `KMG_Summoning_Unit_Girallon` | `b9187f2a07a74c84b07d5a34280753a6` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_girallon` | PASS |
| `xill` | yes | `KMG_Summoning_Unit_Xill` | `440bd760511f4bdd9515ff59a0c5eb33` | `CR10_FerociousTrollGuard` / `Troll` | `KMG_SummonPortrait_xill` | PASS |
| `triceratops` | yes | `KMG_Summoning_Unit_Triceratops` | `ce540cd4b3b74861a4a91458d742a692` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_triceratops` | PASS |
| `stegosaurus` | yes | `KMG_Summoning_Unit_Stegosaurus` | `39cab5be4dc148339f4ce8b0157bd716` | `CR6_HodagStandard` / `Hodag` | `KMG_SummonPortrait_stegosaurus` | PASS |
| `deinonychus` | yes | `KMG_Summoning_Unit_Deinonychus` | `10a13f87e3664e30b642a3c59cc8c700` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_deinonychus` | PASS |
| `tyrannosaurus` | yes | `KMG_Summoning_Unit_Tyrannosaurus` | `04b1a75a76544a9581ee832045f8227a` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_tyrannosaurus` | PASS |
| `brachiosaurus` | yes | `KMG_Summoning_Unit_Brachiosaurus` | `9093aef39ecc416092ef68655a7e56fb` | `MastodonSummon` / `Mastodon` | `KMG_SummonPortrait_brachiosaurus` | PASS |
| `gorgon` | yes | `KMG_Summoning_Unit_Gorgon` | `ce2fc58942b54e558208ea67d595cb7b` | `HorseSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_gorgon` | PASS |
| `bulette` | yes | `KMG_Summoning_Unit_Bulette` | `f84a0babcc864630bae225b2019d6bba` | `CR7_BuletteStandart` / `Bulette` | `KMG_SummonPortrait_bulette` | PASS |
| `griffon` | yes | `KMG_Summoning_Unit_Griffon` | `4b55d1f8da5e4a3ea23f9a95191f8737` | `CR5_ManticoreStandard` / `Manticore` | `KMG_SummonPortrait_griffon` | PASS |
| `hill-giant` | yes | `KMG_Summoning_Unit_HillGiant` | `58770207fd8c4084a0d9addbac591616` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_hill-giant` | PASS |
| `stone-giant` | yes | `KMG_Summoning_Unit_StoneGiant` | `956501547dd54479b329a315aab07a05` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_stone-giant` | PASS |
| `fire-giant` | yes | `KMG_Summoning_Unit_FireGiant` | `65481592185641c99c502e21ba9a5fa2` | `FrostGiantSummoned` / `FrostGiant` | `KMG_SummonPortrait_fire-giant` | PASS |
| `ettin` | yes | `KMG_Summoning_Unit_Ettin` | `2481bc73abd64ef0ae581b48438b5404` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_ettin` | PASS |
| `cloud-giant` | yes | `KMG_Summoning_Unit_CloudGiant` | `ff4696c6336141a3b2236cb0a22c86f3` | `CR7_HillGiantStandart` / `HillGiant01` | `KMG_SummonPortrait_cloud-giant` | PASS |
| `storm-giant` | yes | `KMG_Summoning_Unit_StormGiant` | `4b1143140c614a0aaaa1118c8a44c9ae` | `FrostGiantSummoned` / `FrostGiant` | `KMG_SummonPortrait_storm-giant` | PASS |
| `lemure` | yes | `KMG_Summoning_Unit_Lemure` | `80fd27e965b943b5b307a8c0a45cfb96` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_lemure` | PASS |
| `dretch` | yes | `KMG_Summoning_Unit_Dretch` | `717d3fdec4284e42b21efe83bc03eb25` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_dretch` | PASS |
| `hound-archon` | yes | `KMG_Summoning_Unit_HoundArchon` | `4685038956fc47c7b18d5acf1d7d8d7b` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_hound-archon` | PASS |
| `trumpet-archon` | yes | `KMG_Summoning_Unit_TrumpetArchon` | `b6359294f13a4adab626d14c5ededd50` | `MovanicDevaSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_trumpet-archon` | PASS |
| `astral-deva` | yes | `KMG_Summoning_Unit_AstralDeva` | `2814e9aa55e34dda8f3322d84a5fe5e2` | `MovanicDevaSummoned` / `Empty_Portrait` | `KMG_SummonPortrait_astral-deva` | PASS |
| `satyr` | yes | `KMG_Summoning_Unit_Satyr` | `b0dab767cb2948ca9f918bf7d7c3dbb4` | `CR4_SatyrShortbowStandard` / `Empty_Portrait` | `KMG_SummonPortrait_satyr` | PASS |
| `lillend-azata` | yes | `KMG_Summoning_Unit_LillendAzata` | `71a8adf916b4409680023e853d28583b` | `OwlbearAttack_CapitalGuardCaptain1` / `Empty_Portrait` | `KMG_SummonPortrait_lillend-azata` | PASS |
| `bearded-devil` | yes | `KMG_Summoning_Unit_BeardedDevil` | `182032f380314a7581d0016c443945a2` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_bearded-devil` | PASS |
| `kyton-chain-devil` | yes | `KMG_Summoning_Unit_KytonChainDevil` | `96045f5c18ef4f4396a6c466a2be1a92` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_kyton-chain-devil` | PASS |
| `barbed-devil` | yes | `KMG_Summoning_Unit_BarbedDevil` | `a8ed4e16ec4b48a5bdfdc47f7f1b9cd0` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_barbed-devil` | PASS |
| `bone-devil` | yes | `KMG_Summoning_Unit_BoneDevil` | `74652de839a94feba3c2fb67b536437c` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_bone-devil` | PASS |
| `babau` | yes | `KMG_Summoning_Unit_Babau` | `1512476cf6674704b6b0bcc3b67b48d9` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_babau` | PASS |
| `hezrou` | yes | `KMG_Summoning_Unit_Hezrou` | `0c97e35a3e8e4d2db937a23775ea13ac` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_hezrou` | PASS |
| `vrock` | yes | `KMG_Summoning_Unit_Vrock` | `5f4aa097fc3e4fa4a627ce5d5be6509e` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_vrock` | PASS |
| `glabrezu` | yes | `KMG_Summoning_Unit_Glabrezu` | `3824e7589ae548f79a3eb8369f50359f` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_glabrezu` | PASS |
| `nalfeshnee` | yes | `KMG_Summoning_Unit_Nalfeshnee` | `9930f78084f0fe78d1de849aed58b753` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_nalfeshnee` | PASS |
| `ice-devil` | yes | `KMG_Summoning_Unit_IceDevil` | `6b5b789a08584fab84a00e7da42e1d2c` | `CityGatesHub_LizardfolkRattler` / `Lizardfolk` | `KMG_SummonPortrait_ice-devil` | PASS |
