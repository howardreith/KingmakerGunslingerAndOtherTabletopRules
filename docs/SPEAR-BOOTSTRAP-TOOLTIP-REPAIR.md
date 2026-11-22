# Spear startup, summoning and weapon-tooltip repair

Investigation and repair record for
`planning/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR-MISSION.md` (draft PR #195, branch
`codex/spear-bootstrap-tooltip-repair`, prepared from `master` `2e61e78097`,
v0.0.192). This is a focused development checkpoint under
`docs/VALIDATION-POLICY.md`. It does not cover release, packaging or broad
compatibility qualification.

## Status: manual development checkpoint (2026-11-22)

**Owner decision.** For the remainder of PR #195 the owner returned to the
earlier process: Claude implements and validates, and the owner and ChatGPT
review the resulting checkpoint. The agent-review enrollment
`kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704` was retired
through the controller's supported commands, with its pinned package
`34fb8529…d43c`:

- The enrollment was found `PAUSED`, with no developer running, no lock
  holder, no pending publications and no runtime lock present.
- `archive` committed and pushed the 40 posted records as `3d47a8460e`.
- `cancel` was run with the reason "the owner has selected manual development
  and review for the remainder of PR #195". The enrollment's state now reads
  `CANCELLED`, and its host registry entry and worktree claim were removed.

Cancellation is not an acceptance of any finding. The controller's records
under `docs/agent-reviews/pr-195/` are unchanged history. The paused report's
statement that all 11 round-11 findings were fixed was the developer's own
report on `a049ad3c87`, not an independent approval.

This checkpoint:

- **Simplifies the loader-compat runner** to the owner-approved fail-closed
  contract (§3.5): one new, exclusively owned session followed by its normal
  verified completion. Interrupted or ambiguous sessions need deliberate
  manual recovery (§3.5.3).
- **Leaves the product repair unchanged.** The `src/KingmakerGunslinger`
  tree is `c948a78d1c6b3d56bb43a92a248da72e199d068e` at `a049ad3c87`, at this
  checkpoint and at the tested native package revision `4e0ac475aabc`. The
  domain-test and native-harness trees are also unchanged from `a049ad3c87`.
- **Gives a disposition for every round-11 finding** (§3.6).
- **Records the new checks** (offline fixtures, focused gate and one native
  completion session) in §4.0. All earlier native evidence keeps its
  original package, harness, runner and environment.

Identities (current first):

- **Current loader-compat runner**: commit `1149394770` (manual checkpoint,
  §3.5). Every runner described in §3.4 and every session before §4.0 used
  an earlier runner, now removed.
- **Current product source**: commit `6d6a295b64` (round 7). It adds the
  bounded item-damage diagnostic to the round-6 product (R7-F2): donor item
  values and the `BlueprintItemWeapon.BaseDamage` patch owners.
  `ecf1d641bc` and later commits change only the loader-compat runner and
  harness, their sessions and this record. `ecf1d641bc` itself carries the
  shared runtime ownership (R7-F1).
- **Current native package**: the development candidate built from
  `4e0ac475aabc`, whose product source is identical to `6d6a295b64`:
  - `KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-4e0ac475aabc.zip`;
  - ZIP `253fbe43188d433f57a771261a215ba794160ff98b06142fafcedca69d643518`;
  - DLL `6b2696c70988db4ea1ed42ce015b99226a4de1fdd1727c569bf35eefd5e7049f`;
  - harness DLL `494dadf9…d471`.

  Build-Local passed strict standalone UMM package validation and 30 focused
  tests. Sessions I7, P7, X7 and R7 in §4.2 ran on this package.
- **Round-6 product and package**: product `dfc86851a8`, the round-5 product
  (`195d17ce19`) plus:
  - spear items use the family 1d8 damage (R6-F1);
  - the live `Enchantments` getter check (R6-F2);
  - the returned `GetSubCategories` list check (R6-F3), giving 18 profile
    checks.

  Its development candidate was built from `0a926ce1d8bd`, whose product
  source is identical to `dfc86851a8`:
  - `KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-0a926ce1d8bd.zip`;
  - ZIP `83c936d58bdda1e7e09b6988e612c4550287d8b0d2ba212a281001e1965fcb75`;
  - DLL `5e7d69d17e9dfd5509c4b32e0ef40cedfed4c12e8d13a1ea5df1ccd33ee3fe7d`;
  - harness DLL `8bbee78c…b8d4`.

  Build-Local passed strict standalone UMM package validation and 29 focused
  tests. A first build attempt had stopped with the repository's
  administrator-elevation refusal; it succeeded once the environment ran
  unelevated. Sessions X6, S6, C6 and R6 in §4.2 ran on this package.
- **Round-5 product and package**: product `195d17ce19`, candidate built
  from `18361607eda8`:
  - ZIP `c96e1b3c94cf13707cb8f73368681e61150e23f0526542d050979bdc3707f20e`;
  - DLL `f0fb25cb6027a83ed4265b880122dbd0cc573ca1f04f4eb4f74552c3bc4c8330`.

  Sessions S5, C and R5 in §4.2 ran on it.
- **Round-2 product and package**: product `cc8da0e71f` (the original repair
  `288ce4dd07` plus the round-1 `88f1e86b5e` and round-2 corrections),
  candidate built from `8ee7eabaeda0`:
  - `KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-8ee7eabaeda0.zip`;
  - ZIP `f8988fdee8411c59d7d2cd726b9e4adebf49d90801e8eaf883d2b52fd4bbbb00`;
  - DLL `f0f17f7c3d459845b6a2ce20aaeb7165c2966bcf79c43f7da06d8036016ab574`.

  Sessions S, P, L, O and R in §4.2 ran on it. The round-1 sessions used
  candidate `0f08f76f644d` (package `53f58154…`) with harness `d7dd0ff01d` /
  `b348c3f815`, and are superseded.
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
- **Round 6 (R6-F1 to R6-F3).** Three further gaps are closed:
  - **Item damage.** Foundation, named and progression spear items now clear
    the damage-dice override they would otherwise inherit from the cloned
    native Standard Longspear (`m_OverrideDamageDice` false, `m_DamageDice`
    1d8). Item validation requires the override off and the effective
    `BlueprintItemWeapon.BaseDamage` equal to 1d8.
  - **Live enchantments.** The `movement-enchantment` predicate reads both
    `m_Enchantments` and the live `BlueprintWeaponType.Enchantments` getter.
    It compares entries with the registered movement-accuracy enchantment by
    object identity.
  - **Returned sub-category list.** A new `subcategory-list` predicate
    compares the live `WeaponCategoryExtension.GetSubCategories` result with
    the intended five. Order does not matter; extras and repeats are
    visible. Donor context and that method's patch owners are reported.

  The profile now has 18 predicates, and the diagnostics read `N of 18`.
- **Validation (R1-F1, R2-F1 to R2-F3).** Every one of the predicates (17 before round 6) must
  hold, through the game-independent `ElvenBranchedSpearProfilePolicy`. Each
  predicate checks:
  - the **owned serialized field** against the intended value; and
  - the **effective value** players and the engine use against the 2.1.7b
    engine contract for that field. For most predicates that is the live
    getter. For the sub-category predicate it is the patched engine method
    `WeaponCategoryExtension.HasSubCategory` for the owned category, compared
    with the mod's own policy. The comparison (R5-F1) spans **every
    `WeaponSubCategory` value** plus the `None` wildcard, through
    `ElvenBranchedSpearProfilePolicy.SubCategoryText`. The text lists the
    intended five in policy order (Melee, Finessable, TwoHanded, Exotic,
    Metal), then any other granted value such as Simple or Martial, then
    `!None` if the wildcard is denied. An extra, missing or wildcard
    difference therefore fails validation. The effective `AttackRange` is compared at full
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
  `Elven Branched Spear type profile is invalid: N of 18 checks failed. <predicate> [owned-field|effective-getter]: expected=… actual=… donor=…; getter expected=… actual=… donor=…; getterPatches=…`.
  The profile had 17 checks before round 6, so runs before then quote
  `N of 17`. The item-damage predicate (round 7, R7-F2) uses the same
  bounded form: `Elven Branched Spear item damage is invalid: <item>:
  item-damage [effective-getter]: expected=override=False actual=override=… donor=override=…; getter expected=<1d8> actual=… donor=…; getterPatches=…`.
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
| `spearSubCategoryAddition` | a late (`Priority.Last`) `HasSubCategory` postfix grants extra sub-categories to the spear category only (R5-F1) |

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

**Runner support (historical).** Rounds 7 to 11 extended the runner with
interrupted-session recovery, journal conversion, lock adoption and owner
resolution. The manual checkpoint removed all of that functionality (§3.5).
The entries below describe what earlier sessions ran under, as history. They
are not current behaviour, and removing that code does not mean its
behaviour was proven correct. Round 11 (`a049ad3c87`, the S11 session in
§4.0) is described only in the controller's records.

- **Round 10 (`731f7d883a`, journal schema 4).**
  - **Atomic claims (R9-F1, R8-F2).** The journal, `compatibility.lock`
    and `loader-compat.lock` are each written completely to a private file,
    then published with an atomic no-overwrite rename (`Publish-LcClaim`).
    A claim is either absent or complete.
  - **Recovery lease identity (R8-F2).** Legacy recovery journals its
    recovery lease id before taking the shared lock. A schema-2 lease left
    between creation and recording is adopted only when it names the
    session, belongs to the journal's owner process and holds the shared
    lock.
  - **Journal identity (R8-F1).** Recovery compares the journal identity it
    read first (the transaction id, or an older journal's digest) with the
    journal under its guard. A replacement session is left for its own
    recovery.
  - **Exact registry comparison (R10-F1).** Reconciliation compares
    canonical exports that keep each key path, value name, type and ordered
    multiline data together.
  - **Unsettled settings (R10-F2, R8-F4).** These keep the journal, both
    locks and a `RecoveryRequired` lease:
    - unreadable or unrecognised live settings;
    - any older journal, which has no backup digest and no restore phase.

    A missing key is told apart from an unreadable one: a missing key means
    the delete ran without the import, so importing once is safe. The owner
    resolves the rest explicitly with `-RecoverOnly -RegistryResolution
    Import` or `KeepCurrent`.
  - **Host check before import (R10-F3).** The host is re-checked
    immediately before every import, after the snapshot or reconciliation
    export. A withheld import is recorded as such.
  - **Fresh verification (R10-F4).** Recovery re-verifies owner files for
    any transaction not yet committed.
  - **Bound backup (R10-F5).** The registry backup is bound to its SHA-256
    at intake and verified before any comparison or import. A changed
    backup is never imported.
  - **Recovery verdict (R9-F3).** The `-RecoverOnly` dispatch returns the
    recorded transaction verdict, cleanup-only completions included. A
    round-7 lease's missing owner-file evidence is reported as
    `unavailable`.
- **Round 9 (`2c84b833a8`).** The transaction now lives in
  `LoaderCompat.Transaction.ps1`, with journal schema 3.
  - **Atomic journal (R9-F1).** The journal is claimed with `CreateNew`, so
    exactly one starter wins. Every update writes a complete temporary file
    and `File.Replace`s the journal, only while it still names this
    transaction.
  - **Bound identities (R8-F2).** The lease run id is chosen and journaled
    before the shared lock exists; the lock is created with that id and the
    runtime-lease state written afterwards. The private lock is journaled as
    intended before it is created. An interruption at either point leaves a
    lock that recovery identifies from the journal.
  - **Serialized recovery (R8-F1).** A `DeleteOnClose` guard serializes
    recovery callers. Under it the journal is re-read, so a session another
    caller already finished, or a newer one, is left alone. A Completed
    lease means cleanup only, and an unleased older journal is recovered
    under a shared lock taken for the recovery.
  - **Reconciled restore (R8-F4).** A registry snapshot is journaled before
    the import. An interrupted restore is then reconciled:
    - settings equal to the backup are already applied;
    - settings equal to the snapshot are imported once;
    - anything else is an explicit `unresolved` result that is never
      overwritten.

    A round-7 Completed lease is honoured.
  - **Host re-check (R9-F2).** After the owner-file comparison the host is
    checked again; if it became busy, ownership is kept with the restore
    recorded.
  - **Exit codes (R9-F3).** A session exits 1 unless every launch passed and
    the transaction completed cleanly. `-RecoverOnly` exits 1 for deferred,
    unresolved or changed outcomes.
- Shared runtime ownership as one journaled session transaction. It was
  introduced in round 7 (R7-F1, `ecf1d641bc`) and reworked in round 8
  (`8354822130`).
  - The candidate package and harness are checked before anything is
    acquired (R8-F2).
  - `Enter-LcSessionTransaction` writes the schema-2 journal, then takes the
    lab's shared `compatibility.lock` through `Enter-KmgRuntimeLease`, then
    `loader-compat.lock`. Each step is journaled, and a failure undoes
    exactly what that call acquired.
  - Every later fallible step, evidence-directory creation included, is
    inside the runner's try. Its finally completes the transaction or
    retains it.
  - The owner's saves and installation manifests are stored with their
    digests beside the lease, as the intake.
  - Before every fresh launch, `Assert-LcLaunchAvailable` refuses any
    Kingmaker process, any sibling-lab or foreign lock, and a lost lease.
  - `Complete-LcTransaction` is shared by the finalizer and `-RecoverOnly`.
    It acts only at a safe boundary: no Kingmaker process and no sibling or
    foreign lock (R7-F1). It then:
    - restores game settings exactly once;
    - compares the owner's files with the intake while still holding the
      lease, recording `verified`, `changed` or an explicit `unresolved`
      result in the journal and the lease (R8-F3);
    - completes the lease and releases the shared lock, the private lock
      and finally the journal.

    Each step is journaled before the next, so an interrupted teardown
    resumes without a second restore (R8-F4). Otherwise the journal, both
    locks and a `RecoveryRequired` lease are kept.
  - Before its first registry write, `-RecoverOnly` establishes exclusive
    ownership of the exact interrupted session (R8-F1):
    - the original owner has exited (pid and start time; older journals via
      the lease or the private lock);
    - the host is free;
    - the shared lock, re-taken if missing, is held by this session's
      lease;
    - this caller holds the lock's exclusive recovery handle.

    Refused callers change nothing, and only matching lock files are ever
    removed.
- Read-only `-Report <session> [-ReportLaunch <prefix>]`.
- `-SafetyTests` (offline guard fixtures).

- Step `expectFailure` (the step must fail with that text and isolation must
  be restored).
- Launch `expectHarnessFailure` / `expectStepsNotRun`.
- The exactly-once `expectLog` group.

**Sessions:** reporter-stack, simulated, baseline, persistence,
probe-lifecycle and observer-selection.

### 3.5 Loader-compat runner contract (manual checkpoint, `1149394770`)

Files: `scripts/compatibility/loader-compat/LoaderCompat.Transaction.ps1`
(rewritten), `LoaderCompat.Common.ps1`, `Invoke-LoaderCompatSession.ps1` and
`Test-LoaderCompatSafety.ps1`. The runner is test-only, and nothing in it is
packaged with the product.

#### 3.5.1 Supported contract

A session is **new and exclusively owned**, and ends in its **normal
verified completion**:

1. **Admission refuses pre-existing state before anything changes.**
   `Assert-LcHostFree`, called by the preflight and again by
   `Enter-LcSessionTransaction`, refuses all of these:
   - any Kingmaker process;
   - every configured runtime lock: the shared `compatibility.lock` and
     each sibling lab's lock;
   - the game-copy lock `game-copies\loader-compat.lock`;
   - any `loader-compat.journal.json`, whether current, older, legacy or
     unreadable.

   The refusal names each blocking item. None of them is taken over,
   cleaned up or modified.
2. **Acquisition.** The journal, the shared lock (the lease run id) and the
   game-copy lock are each written to a private file and published with an
   atomic no-overwrite rename. The lease record is written beside the
   lease. A failure while starting releases exactly the claims that call
   made. If that release cannot finish, the remaining claims are reported
   for manual recovery.
3. **Intake.** Owner save and installation manifests are written with their
   digests. The registry key is exported, and the backup is bound to its
   SHA-256 in the journal.
4. **Before every launch**, `Assert-LcLaunchAvailable` checks all of these
   and withholds the launch otherwise:
   - no Kingmaker process and no foreign lock;
   - the live lease handle is still held;
   - the journal, shared lock and game-copy lock are still this session's;
   - a backup bound to its intake digest is recorded, so nothing launches
     that completion could not restore.
5. **Completion** (`Complete-LcTransaction`), at a safe boundary only:
   1. `Open-LcVerifiedBackup` reads the backup once and requires that its
      bytes match the intake digest. No digestless or legacy exception
      remains. It writes a private verified copy, holds it open with a
      handle that denies writes and deletion, and re-checks the held bytes.
   2. Immediately before the registry delete-and-import, host conflicts
      and claim ownership are checked again.
   3. The held verified copy is imported.
   4. `Test-LcRegistryReadback` exports the live key and compares it
      ordinally, in canonical form, with the same verified bytes. The
      canonical form keeps each key path, value name, type and ordered
      multiline data together, with keys and values in ordinal order. A
      successful `reg.exe` exit is not treated as proof.
   5. The owner files are compared with the intake.
   6. Host and claims are checked once more. Only then is the lease
      written `Completed`, with the registry, readback and integrity
      verdicts. The shared lock, the game-copy lock and the journal are
      then released, each only if it still names this session.
6. **Failure retains the session.** Each of these returns failure and keeps
   the journal (phase `retained`, with its reason), both locks, the backup,
   the intake and a `RecoveryRequired` lease naming the transaction:
   - an unsafe boundary or a withheld restore;
   - a missing, unreadable, changed or digestless backup;
   - a failed or interrupted import;
   - an unreadable, absent or mismatching readback;
   - owner files that are changed or unverifiable;
   - claims that changed;
   - any unexpected error during completion.

   Every later session refuses that state. A process killed mid-session
   leaves the same retained state.
7. **Verdict and exit status.** `Get-LcSessionVerdict` reports the scenario
   result (`scenarioPassed`) separately from the restoration result
   (`transactionSucceeded`).
   - A scenario failure with clean restoration is `FAIL`.
   - A retained or unverified transaction is `integrity-failed`, even
     when every launch passed.
   - A host error is `host-error`.
   - A session with zero launches does not pass.

   The script exits 0 only when the result is `PASS`.
   `session-summary.json` records `transactionId`, `registryVerification`
   and, when retained, a `retained` block listing every retained item.

#### 3.5.2 Removed

Removed as unsupported functionality. These paths are gone; the removal does
not claim their behaviour was ever proven correct.

- `-RecoverOnly`, `-RegistryResolution`, `-ResolveTransaction` and the
  `Invoke-LcRecovery` / `Invoke-LcRecoverOnlyCommand` paths.
- Automatic interrupted-session recovery, journal replay and the
  interrupted-restore reconciliation (`Resolve-LcInterruptedRestore`).
- Journal schema-2/3/round-7/legacy conversion (`Convert-LcLegacyJournal`,
  `Find-LcMatchingSchema2Lease`).
- Orphan and foreign lock adoption, and recovery lease acquisition.
- Deferred recovery retries and the `loader-compat.recovery.lock` guard.
- The waiting-caller verdict path (`Get-LcObservedVerdict`).
- Owner resolution (`ConvertTo-LcResolution`, `Get-LcResolutionHint`).
- The fixture process-death boundaries (`Step-LcBoundary`).
- The help text and instructions that promised these capabilities.

Nothing replaces them. There is no recovery command, migration mechanism
or general transaction framework.

#### 3.5.3 Manual recovery

A retained, interrupted or ambiguous session is resolved by a deliberate
maintenance action by the owner, or by an operator the owner has authorized
for that session, outside this runner. Never run it while that session's
owner process may still be alive, and never run it for another lab's state.

1. **Identify the session and its backup.** Read
   `C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat.journal.json`
   and record:
   - `transactionId`, `sessionId`, `phase` and `reason`;
   - `leaseRunId` and `leaseStatePath`;
   - `registryBackup` and `registryBackupSha256`;
   - `registryRestore` and `registryVerification`;
   - `intake` and `integrity`.

   The session's evidence is
   `runtime-evidence\loader-compat\<sessionId>\session-summary.json`, with
   its `retained` block when the runner got that far. If the journal is
   missing or unreadable, stop and investigate by hand. Do not infer
   ownership.
2. **Verify ownership and provenance.**
   - `compatibility-state\compatibility.lock` contains exactly `leaseRunId`.
   - `game-copies\loader-compat.lock` names `session=<sessionId>`.
   - The lease record at `leaseStatePath` has that `runId`, the purpose
     `loader-compat <sessionId>` and that `transactionId`.
   - No process with the journal's `ownerProcessId` and
     `ownerProcessStartedUtc` is alive.
   - No Kingmaker process and no other lab's lock exists.

   Stop if anything differs.
3. **Verify the backup.** Its SHA-256 must equal `registryBackupSha256`.
   If it does not, do not import it: the owner decides from the evidence.
4. **Restore the affected state.** Export the current key to a separate
   file first (`reg export "HKCU\Software\Owlcat Games\Pathfinder Kingmaker"
   <file> /y`), so nothing is lost. If restoration is needed, run
   `reg delete "HKCU\Software\Owlcat Games\Pathfinder Kingmaker" /f`, then
   `reg import <registryBackup>`. Investigate a `changed` or `unresolved`
   owner-file result against the intake manifests (`intake.savesPath`,
   `intake.installPath`). The runner never modifies owner files, so these
   are investigated, not restored.
5. **Verify the result.** Export the key again and compare it with the
   backup. For example, dot-source `LoaderCompat.Common.ps1` and evaluate
   `Test-LcRegistryEqual (Get-LcRegistryCanonical <backup>)
   (Get-LcRegistryCanonical <export>)`, which only reads. Re-hash the owner
   manifests against the intake.
6. **Clear only that session's retained state.** Record the action and its
   outcome in the lease record: set its status to `Completed` and give a
   reason naming the manual recovery. Then remove:
   - `compatibility.lock`, only if it still contains `leaseRunId`;
   - `loader-compat.lock`, only if it still names the session;
   - the journal, only if its `transactionId` is still that transaction.

   Keep the evidence directory, the backup, the intake manifests and the
   readback exports.

#### 3.5.4 Offline fixtures

`Test-LoaderCompatSafety.ps1` (run through `-SafetyTests`) was rewritten for
this contract, and the recovery-only fixtures were deleted with the removed
functionality. It runs 98 checks and requires 41 named scenarios to execute,
covering every case in §3.5.1. The old fixed minimum of 129 checks was
dropped. It uses real temporary files, real file handles and the real
shared-lock acquirer as a competitor. Only the external boundaries are
substituted: the process query, the owner manifests and a `.reg` text
standing for the key.

### 3.6 Round-11 findings: dispositions at this checkpoint

Each finding below was checked against the resulting code, not its name.
"Removed" means the affected functionality no longer exists. It is not a
claim that the old behaviour was tested and proven correct.

| Finding | Disposition | Code at `1149394770` | Evidence |
|---|---|---|---|
| R8-F1: recovery checked one journal read and acted on another | **Removed**: no code reads a pending journal to act on it. Every pending journal is refused untouched. | `Get-LcPreexistingState`, `Assert-LcHostFree`, `Enter-LcSessionTransaction` (`LoaderCompat.Transaction.ps1`) | `preexisting-{pending,older,legacy,unreadable}-journal-refused-without-mutation`, `second-session-refused-while-first-owns`; mutation `ignore-pending-journal` caught (21 checks fail) |
| R8-F2: legacy recovery could run without the shared lease | **Removed**: no legacy conversion and no recovery lease. The only lease is a new session's atomic claim, and an existing `compatibility.lock` is refused. | `Publish-LcClaim`, `Acquire-LcSharedLock`, `Enter-LcSessionTransaction` | `preexisting-shared-lock-…`, `game-copy-lock-race-releases-only-own-claims`, `lease-write-failure-releases-own-claims`, `claim-refuses-existing-and-leaves-it-intact` |
| R9-F3: a waiting recovery caller could turn a failed verdict into success | **Removed**: there are no recovery callers. The only verdict path is `Get-LcSessionVerdict`, and a retained transaction exits 1. | `Get-LcSessionVerdict`, `Test-LcTransactionSucceeded`; `Invoke-LoaderCompatSession.ps1` `exit $verdict.ExitCode` | `verdict-*` (8 checks), `*-retained` cases with `-not Test-LcTransactionSucceeded` |
| R10-F2: cleanup discarded an unresolved predecessor result | **Removed**: no predecessor journal is converted or cleaned up; all are refused as they are. | `Get-LcPreexistingState` | `preexisting-older-journal-…`, `preexisting-legacy-journal-…`, `preexisting-unreadable-journal-…` |
| R11-F1: a KeepCurrent spelling could import | **Removed**: no parameter or code path accepts a registry decision. | `Invoke-LoaderCompatSession.ps1` param block; no `RegistryResolution` reference remains | source search; the manual procedure (§3.5.3) is an owner action, not a runner option |
| R11-F2: a retry could overwrite settings a peer changed during deferral | **Removed**: no deferral and no retry. A withheld restore retains the session, and nothing re-imports automatically. | `Invoke-LcRegistryRestore`, `Set-LcRetained` | `peer-immediately-before-restore-withholds-and-retains`, `sibling-lock-at-completion-retains-without-restore`, `retained-boundary-refused-by-later-session` |
| R11-F3: an owner resolution was not bound to its session | **Removed**: there is no owner-resolution command. Manual recovery identifies the transaction id, lease and locks explicitly before acting (§3.5.3). | none (removed) | source search; §3.5.3 steps 1–2 |
| R10-F5: reconciliation could use different backup bytes | **Retained and verified**: one read, a required intake digest, a held write-denying verified copy for the import, and readback compared with the same verified bytes. | `Open-LcVerifiedBackup`, `Invoke-LcRegistryRestore`, `Test-LcRegistryReadback` | `backup-replaced-during-verification-verified-bytes-used`, `verified-copy-held-against-writes-during-import`, `backup-{missing,changed,unreadable,digestless}-retained-no-import`; mutations `reread-backup-path-for-import` and `digestless-allowed` caught |
| R10-F3: the final backup read separated the host check from the write | **Retained and verified**: backup verification completes first, then host and claims are checked immediately before `Restore`. | `Invoke-LcRegistryRestore` | `peer-immediately-before-restore-withholds-and-retains` (peer introduced during verification: zero imports, ownership retained), `foreign-claim-before-restore-withholds-and-keeps-it`; mutation `no-host-check-before-restore` caught |
| R10-F1: linguistic equality could match distinct strings | **Retained and extended**: ordinal canonical comparison is now also applied to the normal post-import readback. | `Test-LcRegistryEqual`, `Get-LcRegistryCanonicalText`, `Test-LcRegistryReadback` | `unicode-spellings-unequal-ordinally`, `readback-unicode-spelling-retained`, `swapped-subkey-values-unequal`, `reordered-multiline-data-unequal`, `value-type-difference-unequal`, `utf16-export-equals-same-data`, `utf16-backup-restores-and-verifies`; mutation `linguistic-compare` caught; native readback in session M (§4.0) |
| R11-F4: the healthy-profile fixture modelled 17 checks | **Retained**: the `a049ad3c87` fixture correction (`subcategory-list` in the common fixture, 18 checks) is unchanged. | `tests/KingmakerGunslinger.DomainTests/SpearBootstrapTooltipRepairTests.cs` | focused gate (§4.0) |

## 4. Validation

### 4.0 Manual checkpoint checks (2026-11-22)

All checks ran in the task worktree on host DATA, with PowerShell 5.1 for
the runner. The runner commit is `1149394770`, and the product source is
unchanged (§Status).

| Check | Command | Result | Time |
|---|---|---|---|
| Offline runner fixtures | `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SafetyTests` at `1149394770` (clean tree) | **PASS=98 FAIL=0**; 41 of 41 required scenarios executed | 6 s |
| Fixture sensitivity | seven single-line mutations of a scratch copy of `LoaderCompat.Transaction.ps1`: skip the readback, drop the host check before restore, ignore a pending journal, use linguistic comparison, release on changed owner files, import from the backup path instead of the held copy, allow a digestless backup | every mutation makes the fixture fail with named checks (1 to 21 failing). The scratch copy was deleted and is not committed | about 1 min |
| Real export round trip (read-only) | canonical comparison of the intake exports of B9, S9, S10, B10 and S11, each taken after the previous session's real delete-and-import, plus one read-only export of the live key | all five exports are byte-identical (`4f2a1907…`) and canonically equal; the live key equals S11's backup | seconds |
| Native completion session **M** | see below | **PASS**, exit 0, transaction `completed` | 161 s |
| Focused product gate | see §4.3 | see §4.3 | see §4.3 |

**Session M** (`runtime-evidence/loader-compat/20261122T060334Z-umm-0.31.1-spear-bootstrap-simulated-leased`):

- Runner `1149394770` (the final simplified runner). Session file
  `spear-bootstrap-simulated-leased.json`, unchanged since S9.
- Reused package, verified by hash before and in the run:
  `KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-4e0ac475aabc.zip`
  (ZIP `253fbe43…3518`, staged and in-process DLL `6b2696c7…7049f`), with
  harness DLL `494dadf9…d471`.
- Disposable copy `game-copies\loader-compat` with UMM **0.31.1.0**, the
  candidate and harness only, and fixture save
  `KMCS_AFTER_NORMAL_012_MODONLY.zks`.
- Host window: no Kingmaker process and no lab lock at start. The only
  active peer session (Mounted Combat) granted a no-launch window, which
  was released afterwards.
- Lease `runtime-20261122T060334Z-5ff4f76beee94b32b5cfb338568b7eef`,
  transaction `f60023e142cb461bb1c562bccc8c7c3a`; summary SHA-256
  `eda83c40…0c92`.
- **M-1** (`simulated-donor-mutation`, 73 s): PASS. Startup completed under
  the 15 ft / Polearms donor mutation, with the expected log lines present
  and the failure lines absent.
- **M-2** (`invalid-owned-profile`, 73 s, `harness-result.json`
  `55faaa96…e2dd`): PASS.
  - Initialization failed with `1 of 18 checks failed`, with the
    fighter-group diagnostic and its patch owner, each exactly once, and the
    unavailable-inspection warning exactly once. The missing-token text
    appeared 0 times.
  - Longspear, Longsword, Greataxe and Heavy Crossbow each took 25 price
    queries at 1, 4, 5 and 13 gp, with 0 price and 0 tooltip throws.
  - The native Summon Monster I summon and the residue check passed.
- **Completion**, exercised for real:
  - staging, both launches and completion;
  - the registry backup export bound to its digest;
  - import of the held verified copy;
  - readback export `registry-readback-11d0e6e9….reg` (SHA-256
    `4f2a1907…794c`), compared ordinally and canonically with the verified
    backup `4f2a1907…794c`: `verified`;
  - owner files `verified` (95 saves, installation unchanged);
  - the lease `Completed` with those verdicts.

  Afterwards the shared lock read `<free>`, and no game-copy lock, journal
  or Kingmaker process remained (checked by `-Report` and directly).
  `transactionSucceeded` and `scenarioPassed` are both true.

No failure path was staged natively. Retention, conflicts, backup problems,
failed imports and readback mismatches are covered only by the offline
fixtures. The owner's registry was not damaged and no real session was
terminated to test them.

**S11, historical** (`20261122T012813Z-…-spear-bootstrap-simulated-leased`,
summary `f4d38a68…9ffa`). It ran on the round-11 runner `a049ad3c87`, which
is now removed, with the same package, harness and session file. It passed
(38 and 74 s) with lease
`runtime-20261122T012813Z-d79340e45ebf4bba940a4bd6fc06a5ed`, `Completed`
with `applied` and `verified` (95 saves). That runner had no post-import
readback. S11 is retained as history and says nothing about the current
runner.

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

Every session that launched the game verified in-process loader and product
identity. Every session restored the game's registry settings and confirmed
the owner's saves and installation unchanged (`registryRestored`,
`ownerSavesUnchanged`, `ownerInstallUnchanged` all true). F8 launched no game
process. It is host-side setup and restoration evidence only: it failed while
copying a missing fixture save, before any launch, and then completed its
transaction. No owner save or normal-play installation was touched.

Shared runtime leases:
- No lease was opened by the sessions before round 7 (R-base, S, B, P, L,
  O, S5, C, R5, X6, C6, R6 and S6), which used the earlier runner with
  only its private loader-compat lock.
- From round 7 every session holds the lab's shared runtime lease for its
  whole life. The round-7 runner was used for I7, P7, X7 and R7: leases
  `runtime-20261121T230213Z-ea20f41f…`, `runtime-20261121T230443Z-94c386c4…`,
  `runtime-20261121T230845Z-795b2bba…` and `runtime-20261121T231156Z-9c43c85a…`,
  each `Completed` with the shared lock free.
- The round-8 runner (journaled transaction) was used for the round-8
  sessions below: F8, I8 and R8.
- The round-9 runner (schema 3) was used for B9 and S9:
  - lease `runtime-20261122T000746Z-951a9f66…` for B9;
  - lease `runtime-20261122T000920Z-6d4227bf…` for S9.

  Both leases are `Completed`.

| Session (`runtime-evidence/loader-compat/`) | Package | Result |
|---|---|---|
| **S** `20261121T171410Z-…-spear-bootstrap-simulated` | candidate `f8988fde…` | PASS, 12 launches (53–73 s each) |
| **B** `20261121T174507Z-…-spear-bootstrap-baseline` | v0.0.192 | PASS (74 s): the unrepaired bytes reproduce the cascade under the simulated condition |
| **P** `20261121T175105Z-…-spear-bootstrap-persistence` | candidate | PASS (76, 69, 71 s) |
| **L** `20261121T181849Z-…-spear-bootstrap-probe-lifecycle` (round 3 harness, git `9153164dcf`) | candidate | PASS (98, 80, 71 s); supersedes `20261121T174636Z` |
| **O** `20261121T181329Z-…-spear-bootstrap-observer-selection` (round 3 harness, git `9153164dcf`) | candidate | PASS (70, 79, 80, 72 s) |
| **R** `20261121T175459Z-…-spear-bootstrap-reporter-stack` | candidate | PASS (105, 111 s) |
| **R-base** `20261121T153842Z-…-spear-bootstrap-reporter-stack` (round 0, harness `4ba6cab678`) | v0.0.192 | PASS: the partial stack does not reproduce |
| **S5** `20261121T190216Z-…-spear-bootstrap-simulated` (round 5, git `18361607ed`) | round-5 candidate | PASS, 12 launches (53–74 s each) |
| **C** `20261121T185458Z-…-spear-bootstrap-subcategory` (round 5) | round-5 candidate | PASS (71, 52, 53 s) |
| **R5** `20261121T185820Z-…-spear-bootstrap-reporter-stack` (round 5) | round-5 candidate | PASS (101, 109 s) |
| **X6** `20261121T220237Z-…-spear-bootstrap-item-enchantment-list` (round 6) | round-6 candidate | PASS (72, 53, 53 s) |
| **C6** `20261121T220623Z-…-spear-bootstrap-subcategory` (round 6) | round-6 candidate | PASS (70, 53, 53 s) |
| **R6** `20261121T220934Z-…-spear-bootstrap-reporter-stack` (round 6) | round-6 candidate | PASS (100, 108 s) |
| **S6** `20261121T221322Z-…-spear-bootstrap-simulated` (round 6) | round-6 candidate | PASS, 12 launches (53–75 s each) |
| **I7** `20261121T230213Z-…-spear-bootstrap-item-damage-diagnostic` (round 7) | round-7 candidate | PASS (53, 53 s) |
| **P7** `20261121T230443Z-…-spear-bootstrap-persistence` (round 7) | round-7 candidate | PASS (72, 62, 70 s) |
| **X7** `20261121T230845Z-…-spear-bootstrap-item-enchantment-list` (round 7) | round-7 candidate | PASS (70, 53, 54 s) |
| **R7** `20261121T231156Z-…-spear-bootstrap-reporter-stack` (round 7) | round-7 candidate | PASS (102, 110 s) |
| **F8** `20261121T233622Z-…-spear-bootstrap-round8-setup-failure` (round 8, disposable untracked session file) | round-7 candidate | host-error by design (setup failure after acquisition), transaction completed |
| **I8** `20261121T233715Z-…-spear-bootstrap-item-damage-diagnostic` (round 8) | round-7 candidate | PASS (60, 54 s) |
| **R8** `20261121T233924Z-…-spear-bootstrap-reporter-stack` (round 8) | round-7 candidate | PASS (102, 111 s) |
| **B9** `20261122T000745Z-…-spear-bootstrap-baseline` (round 9, leased) | v0.0.192 | PASS (73 s) |
| **S9** `20261122T000920Z-…-spear-bootstrap-simulated-leased` (round 9, leased) | round-7 candidate | PASS (72, 75 s) |
| **S10** `20261122T004013Z-…-spear-bootstrap-simulated-leased` (round 10, leased) | round-7 candidate | PASS (74, 69 s) |
| **B10** `20261122T004257Z-…-spear-bootstrap-baseline` (round 10, leased) | v0.0.192 | PASS (70 s) |
| **S11** `20261122T012813Z-…-spear-bootstrap-simulated-leased` (round-11 runner `a049ad3c87`, removed; leased) | round-7 candidate | PASS (38, 74 s) |
| **M** `20261122T060334Z-…-spear-bootstrap-simulated-leased` (final runner `1149394770`, leased; §4.0) | round-7 candidate | PASS (73, 73 s); readback `verified` |

**Round 10: the schema-4 runner.** The product is unchanged since round 7.
S10 and B10 repeat S9 and B9 under the round-10 runner. Both exited 0 and
were surfaced through `-Report`, each with:
- `transactionSucceeded` true;
- `registryRestore` `applied`;
- `ownerIntegrity` `verified` (95 saves);
- a `Completed` lease recording the same results;
- the shared lock `<free>`.

The sessions:

- **S10** (lease `runtime-20261122T004013Z-e422f11f…`, summary
  `68e9dcee…0083`, loaded DLL `6b2696c7…`):
  - S10-1: the 15 ft / Polearms donor mutation; startup completed.
  - S10-2 (`0a882c2a…2fb3`): one `1 of 18` fighter-group failure.
    Longspear, Longsword, Greataxe and Heavy Crossbow each took 25 price
    queries and 25 tooltip fills with 0 throws, at 1, 4, 5 and 13 gp.
    `batteredInspectionUnavailableReported` true, the warning ×1, the
    missing-token text ×0; the native Summon Monster I summon passed.
- **B10** (published v0.0.192, loaded DLL `0eff2af3…`, lease
  `runtime-20261122T004258Z-fbc822e0…`, summary `51aa31ae…6f81`): the
  generic profile failure reproduced.

The round-10 offline fixtures (`-SafetyTests`) passed, PASS=99:

- **Initial claim (R9-F1)**: a start interrupted after its private claim is
  written leaves no journal, and the next start succeeds. The loser of
  interleaved starts is refused, and the winner's complete journal and locks
  are preserved.
- **Partial acquisition (R8-F2)**:
  - an interrupted `compatibility.lock` claim leaves no lock;
  - legacy recovery interrupted between taking and recording the shared
    lock leaves a lock its journal identifies, which the next recovery
    treats as its own;
  - a schema-2 journal beside its own partial lease is reconciled, while a
    foreign lease's lock is preserved;
  - the next session then acquires both locks.
- **Delayed recovery (R8-F1)**: a recovery paused after reading session A,
  resumed after A finished and B was left by an exited owner, refuses. B's
  journal, lock and settings are untouched, and B recovers on its own
  invocation.
- **Older journals (R8-F4)**: a pre-round-7 journal, with the backup still
  applied and again after a later settings change, gets zero imports and
  keeps the later settings until the owner chooses `KeepCurrent`.
- **Registry comparison (R10-F1)**: swapped subkey values and reordered
  multiline data compare unequal. A swapped live state is preserved and
  unresolved until the owner chooses `Import`.
- **Unreadable registry (R10-F2)**: an unreadable export once keeps
  ownership. The next recovery settles the same transaction without
  re-importing, and a missing key is imported once.
- **Peer during export (R10-F3)**: a peer appearing during the snapshot
  export (completion and recovery) or the reconciliation export withholds
  the import, with zero imports and state retained. One import follows after
  the peer leaves.
- **Stale verification (R10-F4)**: a verified-but-uncommitted transaction
  whose manifest changed while it was down records `changed` on recovery,
  with no second import.
- **Changed backup (R10-F5)**: a backup changed after intake is refused
  before any registry change, in completion and in recovery even with
  `Import`. Once it is intact again, it is imported once.
- **Recovery exit codes (R9-F3)**: the `-RecoverOnly` dispatch, run in child
  processes, exits 0 for a verified cleanup-only completion. It exits 1, after
  cleanup, for changed and unreadable integrity, and 1 for a deferred
  recovery, which keeps its state. A round-7 Completed lease is cleanup only,
  with integrity `unavailable`.

**Round 9: the schema-3 runner.** The product is unchanged since round 7.
Both sessions below exited 0 and were surfaced through `-Report`. Each
records:
- `ownership` `completed`;
- `transactionSucceeded` true;
- `registryRestore` `applied`;
- `ownerIntegrity` `verified` (95 saves, install unchanged);
- a `Completed` shared lease carrying the same integrity and registry
  results;
- the shared lock `<free>`.

The sessions:

- **B9 (R9-F4)**: the baseline control on the published v0.0.192 package
  (ZIP `936f3571…2d69`, loaded DLL `0eff2af3…b96f`), lease
  `runtime-20261122T000746Z-951a9f66…`, summary `3c77dec3…b207`.
  - The simulated donor mutation was recorded before the product: the
    captured Longspear had `m_AttackRange` 15 and `m_FighterGroup`
    Polearms. The Heavy Crossbow control was captured unchanged.
  - Initialization failed with the generic `Elven Branched Spear type
    profile is invalid.`, and no owned spear is registered.
  - Standard Longspear and Longsword: 5 of 5 price queries and 5 of 5
    tooltip fills threw, every one with the missing-token cause.
  - Summon Monster I stayed Kingmaker's single spell and summoned its own
    creature.
- **S9 (R9-F4)**: the two decisive simulated launches on the round-7
  candidate (ZIP `253fbe43…`, loaded DLL `6b2696c7…`), lease
  `runtime-20261122T000920Z-6d4227bf…`, summary `734c4fee…0e6b`.
  - **S9-1** (`b151eb41…687b`): the same donor mutation was captured before
    the product (15 ft, Polearms). Initialization completed, and the owned
    spear has its intended effective profile (6 ft, Spears, 1xD8, X3, two-
    handed, Strength, 10 lb). The selectors were published. Longspear,
    Longsword and Heavy Crossbow each took 20 queries with 0 throws, at 1, 4
    and 13 gp with matching tooltips. The battered and unbound pistol
    checks passed.
  - **S9-2** (`510a779e…9105`): one failure, `Elven Branched Spear type
    profile is invalid: 1 of 18 checks failed. fighter-group [owned-field]:
    expected=Spears actual=Polearms donor=Spears; … getterPatches=KmgLoaderCompatHarness`.
    The selectors were restored. Standard Longspear, Longsword, Greataxe and
    Heavy Crossbow each took 25 price queries and 25 tooltip fills with 0
    throws, at 1, 4, 5 and 13 gp with matching tooltip text.
    `batteredInspectionUnavailableReported` is true and the warning was
    logged exactly once; the missing-token text appeared ×0. The new
    sorcerer's Summon Monster I stayed Kingmaker's single spell, and the
    summon succeeded.
- **Offline transaction fixtures (round 9)**: `-SafetyTests` PASS=87.
  - **R9-F1**: interleaved starts leave the loser refused with the winner's
    journal and locks intact. An update interrupted after its temporary
    file is written leaves the last complete journal, from which recovery
    finishes and removes the temporary file.
  - **R8-F2**: a lease-state write failure rolls back cleanly. Interruptions
    right after the shared lock and right after the private lock are both
    identified from the journal and recovered, and a following session
    acquires and completes.
  - **R8-F1**: a second recovery paused after its first read while the
    first finishes makes one restore in total. When a newer session starts
    meanwhile, the resumed caller refuses and the newer session is
    preserved. Concurrent recoveries are refused. A pre-round-7 journal is
    refused while its owner lives and while another caller holds the shared
    lock or the recovery guard, then recovered under its own shared lock.
  - **R8-F4**:
    - interrupted after the import applied but before it was recorded:
      reconciled without a second import;
    - interrupted before the import: imported exactly once;
    - settings changed by someone else meanwhile: `unresolved`, not
      overwritten, failing outcome, recorded in the lease;
    - a round-7 journal with a Completed lease: cleanup only.
  - **R9-F2**: a peer appearing during verification keeps ownership with
    the restore recorded. Recovery waits for the peer, then completes
    without another import.
  - **R9-F3**: child-process exit codes are 0 only for the clean
    completion, and 1 for the deferred, changed and unreadable results.
  - **Also kept**: the sibling lock, foreign shared lock, competing-handle,
    integrity and boundary-interruption checks from round 8.

**Round 8: the journaled runner on the round-7 candidate.** The product is
unchanged since round 7 (ZIP `253fbe43…`, DLL `6b2696c7…`); round 8 changed
only the runner (`8354822130`). Every session below was surfaced through
`-Report`.

- **Missing package (R8-F2)**: the runner, given a nonexistent
  `-CandidateZip`, stopped with `Candidate ZIP not found: … Nothing was
  acquired.` before taking any lock.
- **F8 (R8-F2)**: a disposable, untracked session file names a fixture save
  that does not exist. After both locks were taken, the intake recorded and
  the game settings backed up, `Copy-Item` failed. The summary
  (`669e5b17…21ed9`) records `status` `host-error` with that failure,
  `ownership` `restored`, `registryRestored` true and `ownerIntegrity`
  `verified` (95 saves, install unchanged). The lease
  (`runtime-20261121T233623Z-9f612ace…`) is `Completed`, with the same
  integrity record, and the shared lock is `<free>`.
- **I8** then acquired both locks without manual deletion and repeated
  I7's two item-damage failures. Summary `3308b3f8…ee37`, lease
  `runtime-20261121T233715Z-a2c499fb…` `Completed`, integrity `verified`,
  shared lock free.
- **R8** repeated the reporter-shaped stack. Summary `385b1601…21c0`; lease
  `runtime-20261121T233924Z-ee1b6b5d…` `Completed`, integrity `verified`,
  shared lock free; startup complete, with no profile-invalid, missing-token
  or unavailable-state line.
- **Offline transaction fixtures**: `Invoke-LoaderCompatSession.ps1
  -SafetyTests` ran `Test-LoaderCompatSafety.ps1`, PASS=74. It uses a
  disposable state root, locks, journal and manifests, and a substituted
  peer process.
  - **Inputs (R8-F2)**: a missing ZIP and a missing harness are refused
    with nothing acquired.
  - **Acquisition**: a foreign shared lock is refused and kept. A stale
    private lock of another session is kept while this session's
    acquisition rolls back.
  - **Setup failure (R8-F2)**: a setup failure after acquisition completes
    with integrity `unresolved` and releases both locks. The next session
    acquires both.
  - **Sibling lock (R7-F1)**: with an empty game list, launch two is
    withheld. The finalizer defers with zero restores and keeps both locks,
    a `RecoveryRequired` lease and the journal.
  - **Refused recoveries (R8-F1)**: recovery is refused, with zero restores
    each time, while the original owner lives, while the sibling lock
    remains, and while a competing caller holds the recovery handle.
    Recovery is also refused beside a foreign shared-lock holder, whose lock
    is preserved. After release, exactly one restore follows, then full
    release.
  - **Integrity (R8-F3)**: a competing lease acquisition during
    verification is refused. A changed manifest is recorded as `changed`,
    and an unreadable one as `unresolved`, in both the journal and the
    lease.
  - **Interruptions (R8-F4)**: completion was interrupted after `restored`,
    `verified`, `lease-completed`, `shared-released` and `private-released`.
    Each resumed through recovery with no second restore and every owned
    marker cleared; a sibling lock at `shared-released` was preserved first.
  - **Older journals**: a round-7 journal gets the same owner check.

**Round 7 on the round-7 candidate** (ZIP `253fbe43…`, DLL `6b2696c7…`). These
runs used the round-7 runner. Each summary, surfaced through `-Report`,
records:
- `ownership: restored`;
- the session's shared runtime lease now `Completed` with
  `recoveryRequired` false;
- the shared `compatibility.lock` `<free>`;
- `registryRestored`, `ownerSavesUnchanged` and `ownerInstallUnchanged`
  true.

The sessions:

- **I7 (R7-F2)**: a harness patch on `BlueprintItemWeapon.BaseDamage`,
  keyed on the foundation spear item `6edc216d…`, changes only that item;
  its weapon type stays correct (`itemDamageOverrides` 1, no type-profile
  failure). Both launches failed startup once, and the selectors were
  restored.
  - With 2d6 (`03a6640e…3d0e`): `Elven Branched Spear item damage is
    invalid: Elven Branched Spear: item-damage [effective-getter]:
    expected=override=False actual=override=False donor=override=False;
    getter expected=1d8 actual=2d6 donor=1d8;
    getterPatches=KmgLoaderCompatHarness`.
  - With a throwing getter (`746fd1c5…4daf`): the same single diagnostic,
    with `actual=<throws InvalidOperationException: KmgLoaderCompatHarness
    injected getter failure (item 6edc216d...` bounded, `donor=1d8` and the
    harness patch owner.
- **P7 (R7-F3)**: the persistence fixture, with Gunslinger off in a fresh
  process, on the corrected package (`317344f5…7c0b` for the reload). The
  loaded save is the session's own `spearpersist` save
  (`expectLoadedFrom(spearpersist)` passed).
  - The loaded released foundation spear (`6edc216d…`,
    `KMG_ElvenBranchedSpear_Item`) reports item BaseDamage `1xD8` with
    `itemOverrideDamageDice` `False`, resolving to
    `KMG_ElvenBranchedSpear_WeaponType` (`77f72b0f…`) with its intended
    profile. Both item values are enforced by the reload probe.
  - The battered pistol kept its owner Hedwirg (`535ba77c…`) and sold for
    22 gp with tooltip "22". The unbound pistol sold for 250 gp and the
    longsword for 4 gp. Counts: 1 spear, 2 pistols (1 battered), 1
    longsword.
- **X7**: the round-6 controls on the round-7 product.
  - With the native item's 2d6 override, startup completes, with `item
    damage is invalid` ×0. The donor values are context only.
  - The empty live enchantments and Simple-in-list controls each still fail
    once.
- **R7**: the reporter-shaped stack still starts: no profile-invalid,
  missing-token or unavailable-state line, and the Fire Beetle summon
  passed.

Runner ownership (R7-F1) is also covered offline. `Invoke-LoaderCompatSession.ps1
-SafetyTests` ran `Test-LoaderCompatSafety.ps1`: PASS=35. It uses a
disposable state root, lock files, journal and a substituted peer process,
and shows:
- a foreign shared lock is refused and never overwritten;
- launch one is admitted under the session's own lock;
- launch two is withheld once a peer game or a sibling-lab lock appears;
- with the peer still running, the finalizer neither restores settings nor
  releases anything, and keeps the lease `RecoveryRequired`, the journal
  and both locks;
- `-RecoverOnly` refuses while the peer runs, then restores exactly once,
  completes the lease and releases the journal and both locks;
- normal completion restores and releases.

**Round 6 on the round-6 candidate** (ZIP `83c936d5…`, DLL `5e7d69d1…`). Every
session below was surfaced through `-Report`, with `registryRestored`,
`ownerSavesUnchanged` and `ownerInstallUnchanged` true.

- **X6-1 (R6-F1)**: the live native Standard Longspear item is given a 2d6
  damage-dice override before the product runs. Startup completed, with
  `item damage is invalid` ×0. `kmgItemDamage` (`harness-result.json`
  `30a5953c…fbc2`) recorded:
  - `StandardLongspear`: override `True`, BaseDamage `2xD6`, unchanged by
    registration;
  - foundation `KMG_ElvenBranchedSpear_Item` (`6edc216d…`): override
    `False`, `1xD8`;
  - named `KMG_ElvenBranchedSpear_Boughkeeper` (`4a084b02…`): override
    `False`, `1xD8`.

  The owned profile was intact and the selectors were published.
- **X6-2 (R6-F2)**: a postfix makes `Enchantments` return an empty
  collection for the owned spear only, while its backing field stays
  correct; `enchantmentOverrides` was 1. Startup failed once
  (`a79f5b22…29ed6`) with `1 of 18 checks failed. movement-enchantment
  [effective-getter]: expected=KMG_ElvenBranchedSpear_MovementOpportunityAccuracy
  actual=KMG_ElvenBranchedSpear_MovementOpportunityAccuracy donor=<none>;
  getter expected=KMG_ElvenBranchedSpear_MovementOpportunityAccuracy
  actual=<none> donor=<none>; getterPatches=KmgLoaderCompatHarness`. The
  owned spear was absent and the selectors were restored.
- **X6-3 (R6-F3)**: a late `GetSubCategories` postfix appends Simple to the
  spear category's list only; `subCategoryListAdditions` was 1. Startup
  failed once (`18be2a83…53d6`) with `1 of 18 checks failed.
  subcategory-list [effective-getter]: … getter
  expected=Melee,Finessable,TwoHanded,Exotic,Metal
  actual=Melee,Finessable,TwoHanded,Exotic,Metal,Simple
  donor=Melee,TwoHanded,Metal,Simple;
  getterPatches=KingmakerGunslinger,KmgLoaderCompatHarness`.
  `subcategory-policy [effective-getter]` appeared ×0, so membership still
  rejected Simple. The selectors were restored.
- **S6** repeats all twelve simulated results with `N of 18`. Summary
  `bde397e4…8e1b`; `productSha256` `5e7d69d1…` in every launch. S6-2
  (`520d5c57…ffa2`): four ordinary weapons at 25 queries each, 0 price or
  tooltip throws, prices 1, 4, 5 and 13, and
  `batteredInspectionUnavailableReported` true; the warning was logged once.
- **C6** repeats the healthy, Finessable-removed and Simple-added
  membership controls (`a181e9f2…83e6`).
- **R6**: the reporter-shaped stack still starts with the 18-check profile
  (`6588f4a9…cf89`): no profile-invalid, missing-token or unavailable-state
  line, and the Fire Beetle summon passed.

"Candidate" (rows S to R) is the development package built at
`8ee7eabaeda0`: ZIP SHA-256
`f8988fdee8411c59d7d2cd726b9e4adebf49d90801e8eaf883d2b52fd4bbbb00`, product
DLL SHA-256
`f0f17f7c3d459845b6a2ce20aaeb7165c2966bcf79c43f7da06d8036016ab574`.

Round 5 changed the product's sub-category check (R5-F1, `195d17ce19`). The
**round-5 candidate** is the development package built at `18361607eda8`:
ZIP `c96e1b3c94cf13707cb8f73368681e61150e23f0526542d050979bdc3707f20e`,
DLL `f0fb25cb6027a83ed4265b880122dbd0cc573ca1f04f4eb4f74552c3bc4c8330`,
harness DLL `cfad84a0…07a4`. Build-Local passed strict standalone UMM package
validation and 28 focused tests. At the round-5 checkpoint (`0b3a9533cd`),
`src/` and `tests/` were unchanged from `18361607eda8`. Rounds 6 and 7
changed the product again; see the opening identity list for the current
product and package.

On it, the simulated session S5, the sub-category controls C and the
reporter-shaped stack R5 all passed, with `registryRestored`,
`ownerSavesUnchanged` and `ownerInstallUnchanged` true; each was surfaced
through `-Report`.

- S5 repeats every S result below. Summary sha256 `5edbd352…ec53`.
  `productSha256` in all 12 launches is `f0fb25cb…8330`. S5-2's
  `harness-result.json` (`231dcb62…986f`) again shows four ordinary weapons
  at 25 queries each, 0 price or tooltip throws, prices 1, 4, 5 and 13, and
  `batteredInspectionUnavailableReported` true. The warning was logged once.
- C-1: healthy startup, intended profile, selectors published.
- C-2: Finessable withheld; startup failed once with the sub-category
  diagnostic; selectors restored.
- C-3: a late postfix grants only **Simple** to the spear category and keeps
  the five intended classifications. Startup failed once. The diagnostic
  (`harness-result.json` `8415b9ee…766d`) reads: `subcategory-policy
  [effective-getter]: expected=Melee,Finessable,TwoHanded,Exotic,Metal
  actual=Melee,Finessable,TwoHanded,Exotic,Metal donor=<not-applicable>;
  getter expected=Melee,Finessable,TwoHanded,Exotic,Metal
  actual=Melee,Finessable,TwoHanded,Exotic,Metal,Simple
  donor=Melee,TwoHanded,Metal,Simple;
  getterPatches=KingmakerGunslinger,KmgLoaderCompatHarness`.
  `subCategoryAdditions` was 1, and the native selectors were restored.
- R5: with Call of the Wild, CraftMagicItems, KingmakerBugfixes and
  RacesUnleashed, the stricter check still lets startup complete. There was
  no profile-invalid, missing-token or unavailable-state line, and the
  Fire Beetle summon passed.

**Machine-recorded summary of S (R3-F1).** These values are read from the
session runner's own `session-summary.json` and the per-launch
`harness-result.json`, without re-interpretation. The runner's read-only
report mode prints them as command output, with each file's SHA-256:

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -Report 20261121T171410Z-umm-0.31.1-spear-bootstrap-simulated [-ReportLaunch 02]
```

It starts nothing and changes nothing. In round 4 it reported
`session-summary.json` sha256 `78d5bb91…ec235`. The S-2 `harness-result.json`
was `6f179f0f…0e0a`. Launches 04, 05, 08, 09, 10, 11 and 12 were reported
as well. Their condition counters were:

- 04 `getterThrows` 1;
- 05 `getterThrows` 1, `ownedValidations` 3, `corruptionsApplied` 1;
- 08 `rangeOverrides` 3, owned getter `AttackRange` `"6"`;
- 09 `rangeOverrides` 1;
- 10 `guidGetterThrows` 1;
- 12 `subCategoryRemovals` 1.

Launch 11's failure chain ends `getter expected=6ft actual=6.25ft
donor=6ft`. Values:

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
     actual=Melee,TwoHanded,Exotic,Metal`, naming the patch owners
     `KingmakerGunslinger,KmgLoaderCompatHarness` (the product's own
     `HasSubCategory` patch and the harness postfix).
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

After the enrollment was cancelled, the focused gate runs through the
standalone repository gate, not the controller's `verify` helper:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Filter elven-branched-spear
```

It ran on the committed candidate that adds this record (the commit after ). Its result is recorded in the record-only commit that follows; product, test and runner source are identical between them.

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
6. Round 5's product change touches only the sub-category predicate. The
   persistence (P), probe-lifecycle (L) and observer-selection (O) sessions
   ran on the `8ee7eabaeda0` candidate and were not repeated on the round-5
   candidate. Startup, failure diagnostics and failed-startup pricing (S5),
   the sub-category controls (C) and the reporter-shaped stack with the
   summon (R5) were repeated.
   The read-only report keeps empty and single-element arrays as arrays
   (R5-F2). Every report first prints a shape check on a synthetic record.
7. Round 6's product changes touch item damage, the enchantment predicate
   and a new list predicate. On the round-6 candidate, the round-6 controls
   (X6), the simulated session (S6), the membership controls (C6) and the
   reporter-shaped stack with its summon (R6) were run.
8. Round 7's product change is confined to the item-damage check and its
   diagnostic. These ran on the round-7 candidate:
   - the item-damage controls (I7);
   - the persistence reload (P7);
   - the round-6 controls (X7);
   - the reporter-shaped stack (R7).

   These were not repeated on it:
   - the simulated (last S6) and membership (last C6) sessions;
   - the probe-lifecycle (L) and observer-selection (O) sessions, last run
     in round 3 on the round-2 candidate.

   The runner's contention paths are proven with offline fixtures (74
   checks in round 8), not with a real second Kingmaker process or a real
   sibling lab. These include a peer game or sibling lock after launch one,
   deferred restoration, refused, competing and interrupted recoveries, and
   integrity during verification. Real runs exercised the uncontended path
   and one real post-acquisition setup failure (F8).
9. Rounds 8 and 9 changed only the runner. The product and its round-7
   candidate are unchanged, so the round-7 product evidence stands.
   - I8 and R8 repeat two of those sessions under the round-8 runner.
   - B9 and S9 repeat the baseline control and the two decisive simulated
     launches under the round-9 runner's shared lease (R9-F4).
   - The remaining simulated launches (S6), the membership controls (C6)
     and the round-7 sessions ran under the earlier runners described above.

   The round-9 contention, interruption and exit-code paths are proven by
   87 offline fixture checks, not by a real second game, a real sibling lab
   or a real process kill.
10. **Round 10** again changed only the runner. S10 and B10 repeat the
   leased baseline and decisive simulated launches. The claim, recovery,
   registry-reconciliation, owner-resolution and exit-code paths are proven
   by 99 offline fixture checks with a .reg text standing in for the
   registry key. No real interrupted restore, real registry export failure,
   second game or sibling lab was staged.

   (Historical: the round-10 recovery and `-RegistryResolution` paths were
   removed at the manual checkpoint, §3.5.2.)
11. **Manual checkpoint (runner `1149394770`).**
   - The runner no longer recovers anything. Any interrupted, failed or
     ambiguous session is retained and blocks every later loader-compat
     session until someone performs the manual procedure in §3.5.3. That
     includes a peer game or lab lock appearing at completion, and a
     killed runner. This is the selected fail-closed trade-off.
   - Changed or unverifiable owner files now also retain the session.
     Earlier runners released and reported them.
   - Retention, conflicts, backup problems, failed imports and readback
     mismatches are proven only by offline fixtures, with a `.reg` text
     standing for the key. Session M exercised the real uncontended
     completion, including a real readback.
   - Journals left by earlier runners are refused, not migrated. None
     existed on this host when the checkpoint was made.
12. **Reporter limitation (unchanged).** The exact failing predicate and the
   interfering modification in the reporter's original 43-mod installation
   remain unidentified. The available subset did not reproduce the original
   failure without the controlled donor mutation. The repair is supported
   against the established failure mechanism (§2.4). The reporter's exact
   installation remains unverified.
