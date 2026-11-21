# Spear startup, summoning and weapon-tooltip repair

Investigation and repair record for
`planning/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR-MISSION.md` (draft PR #195, branch
`codex/spear-bootstrap-tooltip-repair`, prepared from `master` `2e61e78097`,
v0.0.192). This is a focused development checkpoint under
`docs/VALIDATION-POLICY.md`. It does not cover release, packaging or broad
compatibility qualification.

Identities:

- **Product source**: commit `cc8da0e71f`. That is the original repair in
  `288ce4dd07` plus the review round 1 (`88f1e86b5e`) and round 2 product
  corrections. Later commits change only the test-only loader-compat
  harness, its sessions and this record.
- **Native development candidate**, built from `8ee7eabaeda0`, whose product
  source is identical to `cc8da0e71f`:
  `KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-8ee7eabaeda0.zip`.
  Package SHA-256
  `f8988fdee8411c59d7d2cd726b9e4adebf49d90801e8eaf883d2b52fd4bbbb00`; DLL
  SHA-256 `f0f17f7c3d459845b6a2ce20aaeb7165c2966bcf79c43f7da06d8036016ab574`.
- **Round 2 native sessions** ran with harness commit `8ee7eabaed`. Their
  results supersede round 1's for every session they repeat. Round 1 used
  candidate `0f08f76f644d` (package `53f58154…`) with harness `d7dd0ff01d` /
  `b348c3f815`.
- **Baseline**: the published v0.0.192 package
  `KingmakerGunslinger-0.0.192-gunslinger-remediation.zip`, package SHA-256
  `936f3571a5f948911b410e6e39134629b60d0fc469d0cd6347d88971af072d69`. Its spear
  and battered sources are byte-identical to v0.0.188.
- The controller's archive and handoff commits are separate from all of the
  above.

## 1. Reported failure

These details come from the owner's transcription of the reporter's log. The
raw log was never available here and is not committed. The reporter ran
Kingmaker 2.1.7b, UMM 0.31.1, Tabletop Rules Expansion 0.0.188 and 43 mods
(including Call of the Wild 1.14.4c-2.1, ArmsArmor 1.0.10, RacesUnleashed
1.0.11, KingmakerBugfixes 1.3.3 and CraftMagicItems 2.1.0), with
`gunslinger=False`.

1. `SpearWeaponTypeAccess.Validate` threw `Elven Branched Spear type profile is
   invalid.` inside `ElvenBranchedSpearBlueprints.Register`. Startup then
   rolled back 287 registrations and stopped initialization for the process.
2. Expanded Summoning never registered. Summoning hooks reported
   `package-blueprint-lifecycle-failed` and `bootstrap-unavailable`.
3. Every weapon tooltip threw `The battered origin token blueprint is
   unavailable.` from `BatteredFirearmSaleValuePatch` (a
   `VendorLogic.GetItemSellPrice` postfix called from
   `UIUtilityItem.FillTooltipData`).

The same log records a normal Summoned Dog from Summon Monster I.

## 2. Findings

### 2.1 Engine contracts (Kingmaker 2.1.7b)

These come from the decompiled `Assembly-CSharp` and were confirmed natively
in §4.2.

- `BlueprintWeaponType` getters return their private serialized fields.
  `AttackRange` is the exception: a serialized range of 10 ft or less returns
  `max(GameConsts.MinWeaponRange (2 ft), range − 4)`. So a 10 ft spear reads
  6 ft, and the old `AttackRange.Value == 6` check required a serialized 10 ft
  range.
- Outside trading, `VendorLogic.GetItemSellPrice` is
  `(long)(item.Cost × Vendors.SellModifier)`. `UIUtilityItem.FillTooltipData`
  calls it for every item tooltip.
- Kingmaker's own Summon Monster I (`SummonMonsterISingle`) has **no
  variants**. Only the product's Expanded Summoning publication gives it a
  creature list. Without that list the action-bar slot cannot expand, and a
  click starts targeting for the spell itself.
- Clicking a creature row (`MechanicActionBarSlotSpontaneusConvertedSpell`)
  puts that row's `AbilityData` into `Game.Instance.SelectedAbilityHandler`.
  The handler's ground click issues `UnitUseAbility.CreateCastCommand` for it.
- While the product configures its clone, the clone still carries the native
  Longspear's asset GUID (Unity `Instantiate` copies it). The registry assigns
  the released GUID afterwards.
- Kingmaker 2.1.7b items carry no unique id.

### 2.2 What the old constructor relied on

`ElvenBranchedSpearBlueprints` cloned the live native Longspear type
(`fa2dd17cbde7d3f4aa918d467c30516e`). It then wrote only category, text,
weight and enchantments, and checked the inherited values through the
getters:

- melee;
- 1d8 piercing;
- 20/×3 critical;
- Spears fighter group;
- two-handed, not light, not natural;
- 6 ft effective range;
- 10 lb.

Everything except category and weight was the **donor's** value at clone time.
Any earlier mutation of those donor fields, or a getter patch, therefore
failed the whole initialization transaction. The resulting message named no
predicate and no value.

### 2.3 Third-party scan and native partial-stack attempt

- **Offline byte scan.** Searches of the local Call of the Wild,
  KingmakerBugfixes, CraftMagicItems and RacesUnleashed DLLs found no
  reference to the Longspear type GUID. KingmakerBugfixes 1.3.3 has
  `SetFighterGroup` / `SetIsLight` helpers whose targets could not be
  determined offline. ArmsArmor 1.0.10 is not available on this host.
- **Native attempt.** Session R-base (§4.2) ran the **unrepaired v0.0.192**
  under UMM 0.31.1 with the four reported mods this lab holds, at the reported
  versions, and the reporter's module settings applied through real panel
  clicks. Initialization **completed**, and the native Longspear passed every
  old-release predicate. **That partial stack does not reproduce the report.**

### 2.4 Established mechanism and remaining uncertainty

Established natively with a simulated condition (§4.2, session B). With the
live Longspear carrying values the old validation rejects (15 ft reach,
Polearms), the unrepaired package:

1. fails initialization with the generic message;
2. keeps no spear type;
3. throws `The battered origin token blueprint is unavailable.` on **every**
   price query and **every** tooltip fill (5/5 each per item, every exception
   with that cause);
4. leaves Summon Monster I as Kingmaker's own unexpanded spell, which still
   summons a Summoned Dog.

**Not established:** which predicate failed for the reporter, and which of the
missing ~39 mods (or which local change) caused it. No mod is blamed. The
"empty stub" wording is consistent with the unexpanded native spell; a menu
that opens empty was never observed.

## 3. Changes

### 3.1 Spear construction and validation (`Blueprints/ElvenBranchedSpearBlueprints.cs`, `ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs`)

- **Explicit owned profile.** `SpearWeaponTypeAccess.Configure` writes the
  whole fixed mechanical profile onto the owned clone: melee; 10 ft serialized
  reach; catalog 1d8; a new physical/piercing `DamageTypeDescription`; 20/×3;
  Spears; two-handed; not light, monk, natural or unarmed; no attack-stat
  override; 10 lb; the movement-accuracy enchantment.
- **Donor untouched.** The native donor is never written. `Register`
  fingerprints all of the donor's serialized mechanical fields before
  construction, `m_Weight` included, and refuses if any changed. No donor
  getter is read for that comparison (R1-F3).
- **Validation (R1-F1, R2-F1 to R2-F3).** Every one of the 17 predicates must
  hold, through the game-independent `ElvenBranchedSpearProfilePolicy`. Each
  predicate checks:
  - the **owned serialized field** against the intended value; and
  - the **effective value** players and the engine use against the 2.1.7b
    engine contract for that field. For most predicates that is the live
    getter. For the sub-category predicate it is the patched engine method
    `WeaponCategoryExtension.HasSubCategory` for the owned category, compared
    with the mod's own policy. The effective `AttackRange` is compared at full
    `Feet` precision, so 6.25 ft does not pass as 6 ft.

  The effective profile is judged **only on the registered type**, after it
  has its released GUID and before selector publication. Inside the registry
  factory the clone still carries the donor's GUID, so `Configure` checks only
  the serialized profile there. Effective values it does not read are recorded
  as `<deferred>` and never judged. A failing owned field still reports its
  getter's patch owners from Harmony metadata.

  An effective mismatch is fatal **whoever patches the method**. Harmony patch
  owners are reported as context, never as an exemption.
- **One complete report (R1-F3).** Owned and donor getter reads are captured
  per predicate:
  - A throwing owned getter is recorded as `<throws …>` and fails validation.
  - A throwing donor getter only makes that comparison `<unavailable …>`.
    Donor values never decide a verdict.

  The movement-enchantment and sub-category checks are predicates in the same
  verdict. So one failure message lists every discrepancy, in the form
  `Elven Branched Spear type profile is invalid: N of 17 checks failed. <predicate> [owned-field|effective-getter]: expected=… actual=… donor=…; getter expected=… actual=… donor=…; getterPatches=…`.
  Values are capped at 96 characters, owners at 8 and entries at 24. Patch
  info is read only for failing predicates.
- **Selector publication ordering (R1-F2).** All fallible foundation checks
  now run before any native Exotic Weapon Proficiency or Finesse Training
  selection, Elven Weapon Familiarity component or custom-weapon selector
  runtime state is touched:
  - the serialized clone validation;
  - the definitive post-registration validation;
  - the item checks;
  - the donor fingerprint.

  Publication is deferred to `ElvenBranchedSpearBlueprintSet.AttachNamed`, so
  it runs only after the named spears have also registered. `Publish` restores
  its own changes if it fails, and the bootstrap takes the handle on the next
  line. A failure anywhere in foundation or named-spear registration therefore
  leaves native state untouched. `BlueprintBootstrap.cs` (hash-pinned) is
  unchanged.

### 3.2 Tooltip and pricing (`Gunsmithing/`)

- `BatteredFirearmOriginRuntime.IsBatteredForInspection` is used only by the
  sale-price hook. `BatteredFirearmOriginInspectionPolicy` decides its
  outcome:
  - **Unavailable**: initialization is pending or failed and the token
    blueprint was never registered. No live item can carry an enchantment of
    a blueprint that does not exist in the process, so the weapon keeps its
    native price, with one warning per process.
  - **Inspect**: every other case, including successful initialization
    without the token, where the strict `RequireBlueprint` still throws.
- `Bind`, `TryGetOwner` and the creation, transfer and CMI paths are
  unchanged.
- The behaviour is not gated on the Gunslinger toggle.

### 3.3 Failure containment, inspected and unchanged

- The Expanded Summoning menu and publication hooks act only on the product's
  published variants component.
- The firearm tooltip hooks are gated on configured state.
- The summoning blocked-state messages are truthful.
- GUIDs, registration order, released identities, module publication and the
  summon roster are unchanged.

### 3.4 Test-only native harness (`tools/LoaderCompatHarness`, `scripts/compatibility/loader-compat`)

Nothing here is packaged with the product.

**Conditions** (`SpearRepairProbes.cs`). The donor is identified by
reference, because the clone carries the donor's GUID while it is configured.

| Condition | Effect |
|---|---|
| `mutateNativeLongspear` | simulated donor mutation |
| `captureWeaponTypes`, `captureSpearSelectors` | pre-product snapshots |
| `corruptOwnedSpearProfile` (+ `…OnCall`) | corrupts the owned clone on every validation, or only on the n-th |
| `patchWeaponTypeGetters` | harness-owned identity postfixes |
| `ownedSpearAttackRangePatch` | +1 ft on the shared getter, for the owned spear only |
| `throwingWeaponTypeGetters` | the donor or owned getter throws during spear validation |
| `failNamedSpearRegistration` | named-spear registration throws |
| `attackRangeByGuid` | `AttackRange` of the type with that asset GUID gains or returns a value, keyed on the GUID as a mod would key it |
| `throwingGettersByGuid` | a getter throws for that GUID on every read inside the product's spear registration |
| `spearSubCategoryRemoval` | a late (`Priority.Last`) `HasSubCategory` postfix withholds sub-categories from the spear category only |

**Ops:**

- `kmgWeaponTypeProfile`: serialized and effective profile, plus invariants on
  how often a condition fired.
- `kmgSpearSelectorState`: compares selections, familiarity and selector
  runtime with their pre-product state.
- `kmgItemPricing`: the throws mode now requires every price query and every
  tooltip fill to throw with the expected cause, with per-call cause counts.
- `kmgSpearPersistFixture` and `kmgSpearPersistedItems`.
- `kmgSorcererSummon`: an owned probe (`IOwnedProbe` in the `ProbeSlot`) that:
  - clicks the creature row through its own slot;
  - requires exactly that row's ability in the targeting handler;
  - casts through the handler's ground click;
  - requires a successful command, and judges the chosen row, the submitted
    command and the spawned unit against **independent session
    expectations** (R1-F5): the released ability and unit identities. The
    creatures named in the ability's own actions are only recorded;
  - treats `native-single` as an explicit expectation that rejects an
    expanded spell;
  - refuses a non-idle UI (active targeting or an open action-bar group)
    before changing anything. It releases only the targeting, group and menu
    it acquired, and restores the exact incoming selection and current
    character. It then checks that the UI is idle and the selection matches
    what it captured (R2-F4).
- `kmgWeaponTypeProfile` reads effective `Feet` values at full precision
  (the private float, not the truncating `Feet.Value`). Its
  `longspearGetter:<Name>` and `ownedGetter:<Name>` invariants assert the
  recorded text (R2-F3).
- `kmgSorcererResidue`, `kmgSetTargeting`, `kmgTargetingState`,
  `kmgSetSelection` (`empty` or `main`, through the native selection manager)
  and `kmgSelectionState`.

**Runner support:**

- Step `expectFailure` (the step must fail with that text and isolation must
  be restored).
- Launch `expectHarnessFailure` / `expectStepsNotRun`.
- The exactly-once `expectLog` group.

**Sessions:** reporter-stack, simulated, baseline, persistence,
probe-lifecycle and observer-selection.

## 4. Validation

### 4.1 Build and focused domain checks (worktree with `GamePath.props`)

| Command | Result | Time |
|---|---|---|
| `.\scripts\Invoke-KmgGate.ps1 -Filter 'elven-branched-spear'` (working tree of `cc8da0e71f`) | production compile passed; **27 of 3160** tests passed. That is 12 bootstrap-repair policy cases, including a foreign-patched getter being fatal, a throwing owned getter being fatal and reported, an unavailable donor not being fatal, a deferred effective value not being judged, and the field-only predicates, plus 15 spear neighbours | compile 14.6 s, tests 2.7 s |
| same, `-Filter 'custom-weapon;selector;martial-performance;eastern-weapons;battered;summoning-publication-boundary'` (round 1, `88f1e86b5e`) | **56** passed | 1.3 s |
| `powershell.exe -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir … -LoaderReferenceDir …` at `8ee7eabaeda0` | development candidate built; strict standalone UMM package validation passed; 27 tests passed | about 6 min |

The domain cases exercise the production policies directly and are
supporting evidence only.

### 4.2 Native evidence (disposable loader-compat game copy)

Environment for all sessions:

- Kingmaker 2.1.7b, in the disposable copy
  `C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat`.
- UMM **0.31.1.0**, Harmony 2.3.1.1 and the 0.31.1 Harmony12 facade.
- Fixture save `KMCS_AFTER_NORMAL_012_MODONLY.zks` in a per-session profile;
  `locale=enGB`.

Every session verified in-process loader and product identity, restored the
game's registry settings, and confirmed the owner's saves and installation
unchanged (`registryRestored`, `ownerSavesUnchanged`, `ownerInstallUnchanged`
all true). No owner save or normal-play installation was touched. No runtime
lease was opened.

| Session (`runtime-evidence/loader-compat/`) | Package | Result |
|---|---|---|
| **S** `20261121T171410Z-…-spear-bootstrap-simulated` | candidate `f8988fde…` | PASS, 12 launches (53–73 s each) |
| **B** `20261121T174507Z-…-spear-bootstrap-baseline` | v0.0.192 | PASS (74 s): the unrepaired bytes reproduce the cascade under the simulated condition |
| **P** `20261121T175105Z-…-spear-bootstrap-persistence` | candidate | PASS (76, 69, 71 s) |
| **L** `20261121T181849Z-…-spear-bootstrap-probe-lifecycle` (round 3 harness, git `9153164dcf`) | candidate | PASS (98, 80, 71 s); supersedes `20261121T174636Z` |
| **O** `20261121T181329Z-…-spear-bootstrap-observer-selection` (round 3 harness, git `9153164dcf`) | candidate | PASS (70, 79, 80, 72 s) |
| **R** `20261121T175459Z-…-spear-bootstrap-reporter-stack` | candidate | PASS (105, 111 s) |
| **R-base** `20261121T153842Z-…-spear-bootstrap-reporter-stack` (round 0, harness `4ba6cab678`) | v0.0.192 | PASS: the partial stack does not reproduce |

"Candidate" is the development package built at `8ee7eabaeda0`: ZIP
SHA-256 `f8988fdee8411c59d7d2cd726b9e4adebf49d90801e8eaf883d2b52fd4bbbb00`,
product DLL SHA-256
`f0f17f7c3d459845b6a2ce20aaeb7165c2966bcf79c43f7da06d8036016ab574`. Product
sources (`src/`, `tests/`) are unchanged from `8ee7eabaeda0` to the final
candidate (`git diff --stat 8ee7eabaeda0 -- src tests` is empty). Round 3
changed only the test-only harness, a session file and this record.

**Machine-recorded summary of S (R3-F1).** These values are read from the
session runner's own `session-summary.json` and the per-launch
`harness-result.json`, without re-interpretation:

- Session `20261121T171410Z-umm-0.31.1-spear-bootstrap-simulated`.
- Git `8ee7eabaeda05f443804ce9e989b6c6011029cfe`, loader `umm-0.31.1`.
- `candidateZipSha256` `f8988fde…bbbb00`. `productSha256` in every launch
  identity is `f0f17f7c…ab574`, and the active mods are
  `KingmakerGunslinger 0.0.192` and `KmgLoaderCompatHarness 0.1.0`. The
  harness DLL is `be0d9e79…2b8e`.
- `status` `PASS`; `registryRestored` true; `ownerSavesUnchanged` true (95
  owner save files); `ownerInstallUnchanged` true.

| Launch | Verdict | Harness | Key log assertions (count as recorded) |
|---|---|---|---|
| 01 simulated-donor-mutation | PASS | passed | `initialize.complete` ×1, `foundation.ready` ×1; profile-invalid ×0 |
| 02 invalid-owned-profile | PASS | passed | `initialize.failed` ×1; `1 of 17 checks failed.` ×1 (once); `fighter-group [owned-field]: …` ×1; `battered-origin.inspection-unavailable` ×1 (once); missing-token exception ×0 |
| 03 owned-spear-getter-patch | PASS | passed | `attack-range [effective-getter]: … getter expected=6ft actual=7ft donor=6ft` ×1 (once) |
| 04 donor-getter-throws (`donor:CriticalRollEdge`) | PASS | passed | `initialize.complete` ×1; profile-invalid ×0 |
| 05 owned-getter-throws-with-invalid-field | PASS | passed | `2 of 17 checks failed.` ×1 (once); `light [effective-getter]: … actual=<throws InvalidOperationException: …` ×1 |
| 06 late-validation-failure | PASS | passed | `1 of 17` ×1 (once); `foundation.ready` ×0 |
| 07 named-registration-failure | PASS | passed | injected named-spear failure ×1; `initialize.failed` ×1 |
| 08 longspear-guid-getter-patch (`fa2dd17c…=+1`) | PASS | passed | `initialize.complete` ×1; profile-invalid ×0 |
| 09 released-spear-guid-getter-patch (`77f72b0f…=+1`) | PASS | passed | `actual=7ft` diagnostic ×1 (once) |
| 10 donor-weight-throws-by-guid | PASS | passed | `initialize.complete` ×1; profile-invalid ×0 |
| 11 fractional-effective-reach (`77f72b0f…=6.25`) | PASS | passed | `getter expected=6ft actual=6.25ft donor=6ft` ×1 (once) |
| 12 spear-finessable-removed | PASS | passed | `subcategory-policy [effective-getter]` with `actual=Melee,TwoHanded,Exotic,Metal` ×1; `1 of 17` ×1 (once) |

S-2's `kmgItemPricing_failed-startup`: `repeat` 25, `sellModifier` 0.25,
sell-price patch owner `KingmakerGunslinger`. Standard Longspear, Longsword,
Greataxe and Heavy Crossbow each have `queries` 25, `priceThrows` 0,
`tooltipThrows` 0, sell prices 1, 4, 5 and 13 with matching tooltip text.
`batteredInspectionUnavailableReported` is true, and bootstrap is
`initialized` false, `failed` true. Every step of S-2 records
`isolationRestored` true.

The O and round-3 L sessions record the same candidate ZIP and product DLL
hashes, git `9153164dcf0f3a10ecabc55821a26b9a420b391e`, harness DLL
`22ab82fa…3e11`, and `registryRestored`, `ownerSavesUnchanged` and
`ownerInstallUnchanged` all true.

Mission coverage:

1. **Spear construction (4.1.1).**
   - S-1, simulated donor mutation (native Longspear at 15 ft and Polearms,
     which fails the old `fighter-group` and `attack-range` predicates):
     initialization completed. The owned spear has the intended serialized
     profile (category `0x004b4d47`, Melee, 10 ft, 1×D8, Physical/Piercing,
     20, X3, Spears, two-handed, not light, monk, natural or unarmed, no
     override, 10 lb) and the intended effective profile (6 ft, Strength,
     same values).
   - The mutated donor and the unrelated Heavy Crossbow type show no changed
     field since capture.
   - The spear selector options were published.
   - R (reporter-shaped stack) shows the same profile.
2. **Failure diagnostics (4.1.2).** Each case below logged the stated line
   once:
   - S-2, owned clone's fighter group corrupted at the serialized
     pre-registration check, harness postfix on the getter: `Elven Branched
     Spear type profile is invalid: 1 of 17 checks failed.` with
     `fighter-group [owned-field]: expected=Spears actual=Polearms
     donor=Spears; getter expected=Polearms actual=<deferred>
     donor=<deferred>; getterPatches=KmgLoaderCompatHarness`.
   - S-3, a harness postfix gives only the owned spear a 7 ft effective range
     while its field and the donor stay correct (R1-F1): startup **fails**,
     with `attack-range [effective-getter]: expected=10ft actual=10ft
     donor=10ft; getter expected=6ft actual=7ft donor=6ft;
     getterPatches=KmgLoaderCompatHarness`.
   - S-4, the donor's `CriticalRollEdge` throws during validation (R1-F3):
     the correct spear still initializes. The condition fired at least once.
   - S-5, at the post-registration check the owned `IsLight` getter throws
     and the fighter group is also corrupted: one message, `2 of 17 checks
     failed`, lists both. The light predicate shows `actual=<throws
     InvalidOperationException: …>` with its patch owner.
   - S-8 (R2-F1): a postfix keyed on the **native Longspear GUID** adds 1 ft
     to its `AttackRange`. Startup completed. The donor kept the change (its
     old-release `attack-range` predicate fails), and the registered spear
     reads exactly 6 ft. This is a different experiment from the round-1
     failure in run `162032Z`: there, the session at `0f08f76f64` configured
     `throwingWeaponTypeGetters: donor:CriticalRollEdge` (S-4), and the
     harness of that time recognised "the donor" by asset GUID. While
     `Configure` ran, the clone still carried the donor's GUID, so the
     clone's own `CriticalRollEdge` threw and startup stopped. `d7dd0ff01d`
     then identified the donor by reference. The two experiments differ in
     getter and failure mode (a throwing `CriticalRollEdge` in the harness's
     donor test; an additive `AttackRange` change in a mod-style GUID key).
     Both exercise the same hazard: before the registry assigns the released
     GUID, anything keyed on the donor GUID also hits the clone. The product
     now runs its definitive effective-getter check only after registration
     (R2-F1), and S-8 shows that a donor-GUID-keyed patch no longer reaches
     the owned spear.
   - S-9, the failing control: the same patch keyed on the **released spear
     GUID** `77f72b0febaf212a5650e7193c00361f` fails startup once, with
     `attack-range [effective-getter]: … getter expected=6ft actual=7ft
     donor=6ft; getterPatches=KmgLoaderCompatHarness`.
   - S-10 (R1-F3): `Weight` throws for the native Longspear GUID on every read
     inside spear registration, `Register` included. Startup completed with
     the intended spear; the condition fired at least once.
   - S-11 (R2-F3): the released spear returns 6.25 ft. Startup failed once,
     with `getter expected=6ft actual=6.25ft donor=6ft;
     getterPatches=KmgLoaderCompatHarness`.
   - O-1 (R2-F3, round 3, the independent observer): the live native
     Longspear's `AttackRange` getter returns 6.25 ft (keyed on its GUID).
     Startup completed, because the product validates the owned spear rather
     than the donor's effective reach. `kmgWeaponTypeProfile` recorded the
     Longspear getter as `"6.25"` and the owned spear's ordinary effective
     reach as `"6"`, both enforced as invariants; the override fired 3 times.
     Before this round the observer formatted `Feet.Value` and would have
     recorded `"6"`.
   - S-12 (R2-F2): a late `HasSubCategory` postfix withholds Finessable from
     the spear category only. Startup failed once, with `subcategory-policy
     [effective-getter]: expected=Melee,Finessable,TwoHanded,Exotic,Metal …
     getter expected=Melee,Finessable,TwoHanded,Exotic,Metal
     actual=Melee,TwoHanded,Exotic,Metal`, naming the harness patch owner.
     The unmodified category passes in every other launch.
3. **Failed startup (4.1.3).**
   - After S-2's failed initialization, Standard Longspear, Longsword,
     Greataxe and Heavy Crossbow each took 25 `GetItemSellPrice` and 25
     `FillTooltipData` calls with no throw, at native prices (1, 4, 5, 13 gp)
     and matching tooltip `SellPrice`.
   - `battered-origin.inspection-unavailable` was logged exactly once; the
     missing-token exception, zero times.
   - The native selector state was unchanged.
   - Baseline B (R1-F7): 5 of 5 price queries and 5 of 5 tooltip fills threw
     per item, each with the missing-token cause.
4. **Selector rollback (R1-F2).** In every case, the Exotic Weapon Proficiency
   and Finesse Training `Features`/`AllFeatures` arrays and the Elven Weapon
   Familiarity components were the same objects with the same contents as
   before the product ran, and the custom-weapon selector runtime held no
   spear source:
   - S-6: the corruption is written only on the post-registration
     validation (3 owned validation calls, 1 corruption).
   - S-7: named-spear registration throws after `foundation.ready`.
   - S-2, S-3, S-5, S-9, S-11 and S-12. S-1 and S-8 confirm publication on
     success.
5. **Normal pricing (4.1.4).** In S-1 and R, ordinary weapons sold at native
   prices. A production pistol bound as battered sold for the fixed 22 gp,
   and the unbound pistol at its native price.
6. **Disabled publication and persistence (4.1.5, 4.1.7, R1-F4).**
   - P-1 (Gunslinger on) added to the fixture inventory and saved:
     - a released foundation spear `6edc216d68810960f85417237748b042`;
     - a production pistol bound as battered to the main character Hedwirg;
     - an unbound pistol;
     - a Standard Longsword.
   - P-2 turned Gunslinger off through the panel.
   - P-3, a fresh process with `Gunslinger=False` active, loaded that save:
     - The inventory held 1 spear, 2 pistols (1 battered) and the longsword,
       matching the saved counts.
     - The spear resolved to `KMG_ElvenBranchedSpear_WeaponType`
       (`77f72b0febaf212a5650e7193c00361f`) with the intended serialized and
       effective profile.
     - The battered pistol kept `KMG_BatteredFirearm_Origin`. The product's
       strict owner lookup returned Hedwirg (the same unique id), and it sold
       for 22 gp with tooltip "22".
     - The unbound pistol sold for 250 gp and the longsword at its native
       price.
     - No throw, no missing-token message and no unavailable-state warning.
7. **Summoning (4.1.6, R1-F5).** R: a fresh level-1 sorcerer learned Summon
   Monster I through the native level-up selection.
   - The real spell group opened. The Summon Monster I slot opened a creature
     menu with 7 visible entries, each with a name and an icon: Fire Beetle,
     Dog, Eagle, Poisonous Frog, Pony, Dire Rat and Viper.
   - The session named the expected identities independently, from the
     released manifest: ability `c7ef177436854efca42ac4ae0a068610`
     (`KMG.Summoning.Ability.SM.Tier1.FireBeetle.One`) and unit
     `714b04102eae4cd2be200a8f09f04bc3` (`KMG.Summoning.Unit.FireBeetle`).
   - The "Beetle, Fire" row, its ability in the targeting handler, and the
     command the ground click submitted were all
     `KMG_Summoning_Ability_SM_Tier1_FireBeetle_One|c7ef1774…`. The command
     result was `Success`. The spawned unit was
     `KMG_Summoning_Unit_FireBeetle|714b0410…`, matching the session's
     expectation. The ability's own actions name the same unit, but that is
     recorded only.
   - Failure controls (B and S-2): `native-single`, 0 variants, the menu
     closed. The slot click put `SummonMonsterISingle` (`8fd74edd…`, the
     session's expected ability) in the handler, the ground click cast it
     with `Success`, and the summon was `DogSummoned`, the session's
     expected unit, identified by blueprint name because no native GUID is
     held locally.
   - Residue checks after each summon found nothing left. The UI was idle
     before and after the probe.
8. **Probe isolation (R1-F6).**
   - L-1: the probe was held after inserting the sorcerer into the party.
     The 25 s step timeout released through the probe slot, with release
     reason `step timeout after 25s` and party back to 1. That timeout was
     the step's expected outcome, with isolation restored, and the residue
     step found nothing left.
   - L-2: the probe completed. Its test-only release action threw after
     every real release, so the step failed, isolation was reported
     unrestored, and the launch stopped before its next step. The harness
     verdict was `failed`: `cleanup failed: injected-release … || … could not
     all be released`.
   - L-3 (R2-F4): an ability (`SummonMonsterISingle`) was put into the
     targeting handler before the probe. The probe refused with `the UI is
     not idle before the probe (targeting SummonMonsterISingle, open groups
     none); nothing was changed`, which was the step's expected outcome with
     isolation restored. Afterwards the handler still held
     `SummonMonsterISingle`, and no sorcerer remained in the area or party.
   - Selection ownership (R2-F4, round 3). The probe now captures the
     incoming `SelectionManager.SelectedUnits` (any size, empty included) and
     the group's current character before it changes anything. It restores
     that exact set before the caster is destroyed, with no main-character
     fallback, and verifies selection and current character on every release
     path; a mismatch fails the release. In every L and O launch the recorded
     `outgoingSelection` equals the `incomingSelection`:
     - O-2: an empty selection set through Kingmaker's own selection manager
       (`count` 0, current `StartGamePregenFighterUnit#535ba77c…`). After a
       full Fire Beetle summon (`Success`, `KMG_Summoning_Unit_FireBeetle|714b0410…`)
       and cleanup, the probe recorded `count` 0 with the same current
       character, and an independent `kmgSelectionState` step confirmed the
       empty selection.
     - O-3: the main character alone, before and after a full summon.
     - O-4: an empty selection plus pre-existing targeting. The probe refused
       before changing anything; the selection was still empty and the
       handler still held `SummonMonsterISingle`.
     - L-1, L-2 and L-3: the fixture's own incoming selection (the main
       character alone) was the same after the timeout release, after the
       injected release failure (whose real releases all ran), and after the
       refusal.

Harness iterations kept as evidence:

- Round 0: `151241Z`, `151542Z`, `151929Z`, `153059Z`, `153347Z`.
- Round 1, `20261121T162032Z`: three harness defects (donor recognised by
  GUID, and null-text checks). In that run the product correctly failed
  startup when the condition actually made the owned getter throw.
- Round 1, `20261121T164052Z`: the launch label printed wrongly on the
  console.
- Round 1 passing sessions (`162812Z`, `163535Z`, `163705Z`, `164419Z`,
  `164728Z`, candidate `53f58154…`) are superseded by the round-2 runs above.
- Round 2 probe-lifecycle `20261121T174636Z` (PASS) is superseded by the
  round-3 L run.

### 4.3 Required gate

The `focused` gate runs on the final committed candidate through the
enrollment's `verify` helper with `-Filter elven-branched-spear`. Its receipt
is in the developer report.

## 5. Remaining limitations and risks for the owner

1. The reporter's exact failing predicate and its cause remain unknown
   (§2.4). The four available reported mods do not reproduce it, even on the
   unrepaired package. ArmsArmor 1.0.10, about 38 other mods and the raw log
   are unavailable.
2. **Behaviour change for the owner to note.** A foreign mod that changes the
   registered spear's effective values fails startup visibly, with
   diagnostics naming the predicate and the patch owner, as it did before the
   repair. The effective values are its getters, its engine sub-category
   result, and its full-precision range. Tolerating such a mod would need an
   explicit owner design decision and a specific supported rule. Patches
   keyed on the native Longspear no longer affect the spear.
3. Eastern Weapons is registered before Expanded Summoning and still
   inherits `m_AttackType`, `m_AttackRange` and `m_IsNatural` from its donors.
   It checks `AttackRange.Value == 2` with a generic message. It was not
   reported as failing and was not changed. A reach or natural-weapon mutation
   of its donors would still stop startup there.
4. Native evidence comes from a disposable copy, with the product alone or
   with four of the reported mods. It is not the reporter's installation.
5. The sorcerer probe's selection restoration was exercised natively with an
   empty selection and a single-unit selection. The fixture party has one
   member, so a multi-unit incoming selection was not exercised natively.
   The same code path restores it (native `MultiSelect` of the captured
   views), and the release-time set comparison would fail the release if it
   did not match. The S, B, P and R sessions ran with the round-2 harness,
   and their sorcerer steps used the earlier selection handling. Their
   product verdicts are unaffected, because selection is restored only after
   all observations. Those sessions also used the truncating observer, but
   every getter value they compared was a whole number of feet. In those
   runs the production validator, which always read full precision, passed
   the same values.
