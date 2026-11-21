# Spear startup, summoning and weapon-tooltip repair

Investigation and repair record for
`planning/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR-MISSION.md` (draft PR #195, branch
`codex/spear-bootstrap-tooltip-repair`, prepared from `master` `2e61e78097`,
v0.0.192). This is a focused development checkpoint under
`docs/VALIDATION-POLICY.md`. It does not cover release, packaging or broad
compatibility qualification.

Identities:

- **Product source**: commit `88f1e86b5e`. That is the original repair in
  `288ce4dd07` plus the review round 1 product corrections. Later commits
  change only the test-only loader-compat harness, its sessions, the session
  runner and this record.
- **Native development candidate**, built from `0f08f76f644d`, whose product
  source is identical to `88f1e86b5e`:
  `KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-0f08f76f644d.zip`.
  Package SHA-256
  `53f58154067c35c60b7f06aeaac4041df7bfc63aa37eecdcb038598a421e44ca`; DLL
  SHA-256 `ae1250ccaa3531210eed4a4b47a1d0e9bd7ecb93631a21e486b7eafd51ca8196`.
- **Baseline**: the published v0.0.192 package
  `KingmakerGunslinger-0.0.192-gunslinger-remediation.zip`, package SHA-256
  `936f3571a5f948911b410e6e39134629b60d0fc469d0cd6347d88971af072d69`. Its spear
  and battered sources are byte-identical to v0.0.188.
- **Round 1 native sessions** ran with harness commit `d7dd0ff01d`, or
  `b348c3f815` for the probe-lifecycle and reporter-stack sessions. The two
  commits differ only by a session-runner variable fix that affects
  `expectStepsNotRun`.
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
  construction and refuses if any changed.
- **Validation (R1-F1).** Every one of the 17 predicates must hold, through the
  game-independent `ElvenBranchedSpearProfilePolicy`. Each predicate checks:
  - the **owned serialized field** against the intended value; and
  - the **live getter** (the effective value players and the engine use)
    against the 2.1.7b engine contract for that field.

  A getter mismatch is fatal **whoever patches the getter**. Harmony patch
  owners are reported as context, never as an exemption. The round-0 tolerance
  for foreign-patched getters is removed.
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
  - the clone validation;
  - the final post-registration validation;
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
  - requires a successful command carrying the selected ability, and a summon
    that the ability's actions spawn;
  - treats `native-single` as an explicit expectation that rejects an
    expanded spell.
- `kmgSorcererResidue`.

**Runner support:**

- Step `expectFailure` (the step must fail with that text and isolation must
  be restored).
- Launch `expectHarnessFailure` / `expectStepsNotRun`.
- The exactly-once `expectLog` group.

**Sessions:** reporter-stack, simulated, baseline, persistence and
probe-lifecycle.

## 4. Validation

### 4.1 Build and focused domain checks (worktree with `GamePath.props`)

| Command | Result | Time |
|---|---|---|
| `.\scripts\Invoke-KmgGate.ps1 -Filter 'elven-branched-spear'` (working tree of `88f1e86b5e`) | production compile passed; **26 of 3159** tests passed. That is 11 bootstrap-repair policy cases, including a foreign-patched getter being fatal, a throwing owned getter being fatal and reported, an unavailable donor not being fatal, and the field-only predicates, plus 15 spear neighbours | compile 14.9 s, tests 2.3 s |
| same, `-Filter 'custom-weapon;selector;martial-performance;eastern-weapons;battered;summoning-publication-boundary'` | **56** passed | 1.3 s |
| `powershell.exe -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir … -LoaderReferenceDir …` at `0f08f76f644d` | development candidate built; strict standalone UMM package validation passed; 26 tests passed | about 6 min |

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
| **S** `20261121T162812Z-…-spear-bootstrap-simulated` | candidate `53f58154…` | PASS, 7 launches (74, 72, 53, 60, 55, 55, 55 s) |
| **B** `20261121T163535Z-…-spear-bootstrap-baseline` | v0.0.192 | PASS (73 s): the unrepaired bytes reproduce the cascade under the simulated condition |
| **P** `20261121T163705Z-…-spear-bootstrap-persistence` | candidate | PASS (75, 65, 70 s) |
| **L** `20261121T164419Z-…-spear-bootstrap-probe-lifecycle` | candidate | PASS (96, 79 s) |
| **R** `20261121T164728Z-…-spear-bootstrap-reporter-stack` | candidate | PASS (103, 113 s) |
| **R-base** `20261121T153842Z-…-spear-bootstrap-reporter-stack` (round 0, harness `4ba6cab678`) | v0.0.192 | PASS: the partial stack does not reproduce |

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
   - S-2, owned clone's fighter group corrupted, harness postfix on the
     getter: `Elven Branched Spear type profile is invalid: 1 of 17 checks
     failed.` with `fighter-group [owned-field]: expected=Spears
     actual=Polearms donor=Spears; getter expected=Polearms actual=Polearms
     donor=Spears; getterPatches=KmgLoaderCompatHarness`.
   - S-3, a harness postfix gives only the owned spear a 7 ft effective range
     while its field and the donor stay correct (R1-F1): startup **fails**,
     with `attack-range [effective-getter]: expected=10ft actual=10ft
     donor=10ft; getter expected=6ft actual=7ft donor=6ft;
     getterPatches=KmgLoaderCompatHarness`.
   - S-4, the donor's `CriticalRollEdge` throws during validation (R1-F3):
     the correct spear still initializes. The condition fired at least once.
   - S-5, the owned `IsLight` getter throws and the fighter group is also
     corrupted: one message, `2 of 17 checks failed`, lists both. The light
     predicate shows `actual=<throws InvalidOperationException: …>` with its
     patch owner.
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
   - S-6: the corruption is written only on the second, post-registration
     validation (2 owned validations, 1 corruption).
   - S-7: named-spear registration throws after `foundation.ready`.
   - S-2, S-3 and S-5.
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
   - Clicking row 0 put `KMG_Summoning_Ability_SM_Tier1_FireBeetle_One` into
     the targeting handler. Its ground click submitted that ability, the
     command result was `Success`, and the summon was
     `KMG_Summoning_Unit_FireBeetle`, the creature that ability's actions
     spawn.
   - Failure controls (B and S-2): `native-single`, 0 variants, the menu
     closed. The slot click put `SummonMonsterISingle` in the handler, the
     ground click cast it with `Success`, and the summon was `DogSummoned`.
   - Residue checks after each summon found nothing left.
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

Harness iterations kept as evidence:

- Round 0: `151241Z`, `151542Z`, `151929Z`, `153059Z`, `153347Z`.
- Round 1, `20261121T162032Z`: three harness defects (donor recognised by
  GUID, and null-text checks). In that run the product correctly failed
  startup when the condition actually made the owned getter throw.
- Round 1, `20261121T164052Z`: the launch label printed wrongly on the
  console.

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
   owned spear's effective getter values now fails startup visibly, with
   diagnostics naming the predicate and the patch owner, as it did before
   the repair. Tolerating such a mod would need an explicit owner design
   decision and a specific supported rule.
3. Eastern Weapons is registered before Expanded Summoning and still
   inherits `m_AttackType`, `m_AttackRange` and `m_IsNatural` from its donors.
   It checks `AttackRange.Value == 2` with a generic message. It was not
   reported as failing and was not changed. A reach or natural-weapon mutation
   of its donors would still stop startup there.
4. Native evidence comes from a disposable copy, with the product alone or
   with four of the reported mods. It is not the reporter's installation.
