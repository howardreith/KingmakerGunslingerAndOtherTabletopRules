# Spear startup, summoning and weapon-tooltip repair

Investigation and repair record for
`planning/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR-MISSION.md` (draft PR #195, branch
`codex/spear-bootstrap-tooltip-repair`, prepared from `master` `2e61e78097`,
v0.0.192). This is a focused development checkpoint under
`docs/VALIDATION-POLICY.md`. It does not cover release, packaging or
compatibility qualification.

The product change is the commit that adds this record. The controller's later
archive and handoff commits are separate from it.

## 1. Reported failure

These details come from the owner's transcription of the reporter's log. The
raw log was never available here and is not committed. The reporter ran
Kingmaker 2.1.7b, UMM 0.31.1, Tabletop Rules Expansion 0.0.188 and 43 mods,
including Call of the Wild 1.14.4c-2.1, ArmsArmor 1.0.10, RacesUnleashed
1.0.11, KingmakerBugfixes 1.3.3 and CraftMagicItems 2.1.0, with
`gunslinger=False`.

1. `SpearWeaponTypeAccess.Validate` threw `Elven Branched Spear type profile is
   invalid.` inside `ElvenBranchedSpearBlueprints.Register`. Startup then
   rolled back 287 registrations and stopped initialization for the process.
2. Expanded Summoning never registered. Summoning hooks reported
   `package-blueprint-lifecycle-failed` and `bootstrap-unavailable`.
3. Every weapon tooltip threw `The battered origin token blueprint is
   unavailable.` The throw came from `BatteredFirearmSaleValuePatch` (a
   `VendorLogic.GetItemSellPrice` postfix called from
   `UIUtilityItem.FillTooltipData`).

The same log records a normal Summoned Dog from Summon Monster I. So the log
does not show that every form of the spell was unusable.

## 2. Findings

### 2.1 Engine contracts (Kingmaker 2.1.7b)

These were read from the decompiled `Assembly-CSharp`
(`BlueprintWeaponType`, `Feet`, `GameConsts`, `DamageTypeDescription`,
`DiceFormula`). Production code was compiled against the installed game's
`Kingmaker_Data/Managed`.

- `BlueprintWeaponType` getters return their private serialized fields
  directly: `AttackType`, `BaseDamage`, `DamageType`, `CriticalRollEdge`,
  `CriticalModifier`, `FighterGroup`, `IsTwoHanded`, `IsLight`, `IsMonk`,
  `IsNatural`, `IsUnarmed` and `Weight`. `Category` is a public field.
- `AttackRange` is computed. A serialized range of 10 ft or less returns
  `max(GameConsts.MinWeaponRange (2 ft), range − 4)`; a longer range is
  returned unchanged. The old `AttackRange.Value == 6` check therefore
  required a serialized 10 ft range.
- `AttackBonusStat` returns Strength for an unranged weapon unless
  `m_OverrideAttackBonusStat` is set.

### 2.2 What the old constructor relied on

`ElvenBranchedSpearBlueprints` clones the live native Longspear type
(`fa2dd17cbde7d3f4aa918d467c30516e`). `SpearWeaponTypeAccess.Configure` wrote
only the category, text, weight and enchantments. `Validate` then checked
these values through the getters:

- melee attack type;
- 1d8 piercing physical damage;
- 20/×3 critical;
- Spears fighter group;
- two-handed, not light, not natural;
- 6 ft effective range;
- 10 lb weight.

All of these except category and weight were **inherited from the donor at the
moment the clone was made**. Two things could therefore fail the whole
initialization transaction:

- any change to the donor's serialized fields made before this mod's bootstrap
  (by another mod or a blueprint edit);
- a patch on one of those getters.

The message named neither the failed predicate nor the actual value.

Owned patches that touch the checked values:

- `ElvenBranchedSpearCategoryRuntime` postfixes
  `WeaponCategoryExtension.HasSubCategory` / `GetSubCategories` and
  `StatsStrings.GetText`. The subcategory policy check calls the mod's own
  static `ElvenBranchedSpearCategoryRuntime.HasSubCategory`, not the patched
  engine method.
- The mod's `get_AttackRange` patches (Elephant, Griffon, Infernal, Triceratops
  and the summon reach patches) target `ItemEntityWeapon`, not
  `BlueprintWeaponType`. They do not affect `Validate`.

### 2.3 Third-party scan (offline, limited)

These checks were byte searches of the mod DLLs available on this host
(`game-copies/loader-compat/.lab-pristine/Mods`) and the existing Call of the
Wild decompile. They are not a full disassembly.

| Mod (local version) | Weapon-type references found | Longspear GUID |
|---|---|---|
| CallOfTheWild | reads `BlueprintWeaponType.AttackRange`; writes `m_IsTwoHanded` on other types (for example `d516765b…`) | type absent; item `f28f6031…` referenced as a list entry |
| KingmakerBugfixes 1.3.3 | `SetFighterGroup` / `SetIsLight` helpers that write `m_FighterGroup` / `m_IsLight` | no UTF-16 GUID literals at all (targets not determinable offline) |
| ProperFlanking2 | references `get_AttackRange` | absent |
| CraftMagicItems, RacesUnleashed | none | absent |
| ArmsArmor 1.0.10 | **not available on this host** | — |

KingmakerBugfixes 1.3.3 is also part of the owner's live stack, where spear
registration succeeds on v0.0.192. Its public source could not be fetched from
this session (web access denied). **No mod is blamed.**

### 2.4 Established mechanism and remaining uncertainty

The old construction was unsafe for any donor mutation or getter patch on the
inherited predicates, and it reported none of the values. **Which predicate
failed in the reporter's 43-mod environment is still unknown.** The raw log,
the full mod stack and ArmsArmor 1.0.10 are unavailable, and no native
reproduction could be run (§4.2).

## 3. Changes

### 3.1 Spear construction (`Blueprints/ElvenBranchedSpearBlueprints.cs`)

- `SpearWeaponTypeAccess.Configure` now writes the spear's whole fixed
  mechanical profile onto the **owned clone**: melee; 10 ft serialized reach;
  1d8 (catalog dice); a new physical/piercing `DamageTypeDescription`; 20/×3;
  Spears; two-handed; not light, monk, natural or unarmed; no attack-stat
  override; 10 lb; the movement-accuracy enchantment. Nothing mechanical is
  inherited from the donor any more.
- The native donor is never written. `Register` now captures a fingerprint of
  every serialized mechanical field of the donor, plus the identity of its
  reference-typed values, before construction. It refuses
  (`Native Longspear donor profile was mutated: before=… after=…`) if anything
  changed. The earlier category, weight and item-type checks remain.
- Validation is kept and is stricter. It checks 15 predicates through the new
  game-independent `ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs`. The
  existing enchantment and subcategory checks follow unchanged. For each
  predicate the policy compares:
  - the **owned serialized field** with the intended value;
  - the **live getter** with the engine contract for that field (identity, or
    the reach formula).

  Outcomes:
  - Any owned-field mismatch is fatal.
  - A getter mismatch is fatal unless the getter's Harmony patch info names an
    owner other than this mod. That case covers an engine contract change and
    a getter patched only by this mod.
  - A getter changed by another mod's patch on a shared getter (every weapon
    is affected, and the owned field is exact) is logged once, as warning
    `elven-branched-spear` / `profile.external-getter-patch`, and does not
    fail startup. A wrong owned field stays fatal even when the getter is
    externally patched.
- Diagnostics. A failure throws one message, logged once by the existing
  `blueprints` / `initialize.failed` record. It has the form `Elven Branched
  Spear type profile is invalid: N of 15 checks failed.`, followed by one entry
  per failed predicate with:
  - the predicate name and failure kind;
  - the expected, actual and donor serialized values;
  - the expected, actual and donor getter values;
  - the getter's Harmony patch owners (`none`, the owner list, or
    `unavailable:<reason>` — the installed Harmony12 facade throws for an
    unpatched method).

  Values are capped at 96 characters, owners at 8 and entries at 24. Patch
  info is read only for failing predicates, so a healthy startup makes no
  Harmony queries. Nothing runs per frame or per hover.

### 3.2 Tooltip and pricing (`Gunsmithing/`)

- New `BatteredFirearmOriginRuntime.IsBatteredForInspection`, used only by the
  sale-price hook (`BatteredFirearmSaleValueRuntime`). The decision is made by
  `BatteredFirearmOriginInspectionPolicy`:
  - **Unavailable**: initialization is pending or failed, and the token
    blueprint was never registered. No live item can carry an enchantment
    whose blueprint does not exist in the process, so the weapon is reported
    as not battered and keeps its native sale price. A single warning
    (`gunsmithing` / `battered-origin.inspection-unavailable`) is logged per
    process.
  - **Inspect** in every other case, including successful initialization
    without the token. That case is an invariant violation, and the strict
    `RequireBlueprint` exception still fires. Multiple tokens or a token
    without a stable owner still throw.
- `Bind`, `TryGetOwner` and the creation, transfer and CMI paths are
  unchanged. They still require the blueprint.
  `RequireBlueprint`'s message now says why the token is missing (failed,
  pending or invariant).
- Not gated on the Gunslinger toggle. The token is registered unconditionally
  during a successful initialization, so `gunslinger=False` and existing saves
  behave exactly as before.

### 3.3 Failure containment, inspected and unchanged

- `ExpandedSummoningVariantMenuPatch` returns to native behavior unless the
  source spell carries this mod's published variants component, which exists
  only after a successful publication. After a failed startup, Summon Monster
  I keeps its native menu.
- `ExpandedSummoningPublication.IsPublishedExpandedParent` uses the same
  marker test.
- Other tooltip-path hooks already return to native behavior when
  initialization did not complete:
  - `FirearmConditionTooltipPatch` and `FirearmQualitiesTooltipPatch`
    (`FirearmRuntimeState.IsConfigured`);
  - `BasicAmmunitionSaleValuePatch` (null ammunition set);
  - the quality filter transpilers (component tests on the enchantment
    itself).
- The `package-blueprint-lifecycle-failed` / `bootstrap-unavailable` summoning
  messages are truthful blocked states and were left as they are.
- `BlueprintBootstrap.cs` (hash-pinned by the icon catalog) is not edited.
  Registration order, GUIDs, released identities, module publication and the
  summon roster are unchanged.

## 4. Validation

### 4.1 Focused domain and compile checks (this host, `DATA`)

`GamePath.props` is absent from this worktree, and the headless developer is
not permitted to create it. Local runs therefore passed
`-KingmakerInstallDir 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker'`.
The first domain-test build in this worktree failed for the same reason (the
test project resolves `Newtonsoft.Json` through `$(KingmakerManagedDir)`). It
built after one `dotnet build` of the test project with
`-p:KingmakerManagedDir=…\Kingmaker_Data\Managed`, which placed the reference
in the output directory. These runs used the working tree before commit. The
controller's `focused` receipt for the exact candidate is separate (§4.3).

| Command | Result | Time |
|---|---|---|
| `.\scripts\Invoke-KmgGate.ps1 -Filter 'elven-branched-spear' -KingmakerInstallDir …` | production Release compile passed; **25 of 3158** selected tests passed: the 10 new `elven-branched-spear.bootstrap-repair.*` cases and 15 existing spear catalog, source-contract, publication and presentation cases | compile 0.4 s (incremental, after a full compile in the first run), tests 2.4 s |
| same, `-Filter 'battered;craft-magic-items;sale;vendor;player-facing'` | **98** passed, 0 failed (battered rules and ownership, CMI, vendor stock, player-facing presentation) | 0.8 s |
| same, `-Filter 'summoning-publication-boundary;summon.publication;alternate-summoning;summon-variant-menu'` | **43** passed, 0 failed | 0.9 s |

The new cases test the production policies directly. They are supporting
evidence and do not replace native observation:

- the engine reach formula (10 → 6, 5 → 2, 9 → 5, 11, 15);
- a healthy profile;
- owned-field failure diagnostics (predicate, expected, actual, donor, getter
  and patch owners) from a **simulated** donor mutation (15 ft reach, Polearms
  group);
- unpatched-getter and own-patch-only mismatches are fatal;
- an external getter patch is reported and not fatal;
- an owned mismatch still wins over an external patch;
- bounded output;
- observation-contract refusal;
- the battered inspection truth table.

### 4.2 Native evidence: not obtained (blocked)

Native work was not run. Each required item below remains an **explicit gap**,
not a pass.

| Mission item | Status | Why |
|---|---|---|
| 4.1.1 Spear construction under the reproduced condition; donor unchanged | GAP | No native run. The constructor change and the donor fingerprint guard are compiled only. |
| 4.1.2 Failure diagnostics from an intentionally invalid profile | Domain only (simulated observations) | Native injection not run. |
| 4.1.3 Failed startup: ordinary weapon price and tooltip, no flood | GAP | The loader-compat harness's injected-initialization-failure condition was not run. |
| 4.1.4 Normal and battered pricing | Domain neighbors only | No native price query. |
| 4.1.5 `gunslinger=False` | By construction and inspection (unconditional token registration, toggle not consulted) | No native run. |
| 4.1.6 New sorcerer Summon Monster I menu and summon, reporter settings, UMM 0.31.1 | GAP | The visual "empty stub" symptom has **not** been observed, before or after the repair, and is not claimed fixed. |
| 4.1.7 Fresh-process save/load | Not required by the change: persistence, registration order, identities and saved-item interpretation are unchanged. The inspection path reads only. | — |

Blocking causes, attempted once each in their simple form:

- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear`
  was **denied** by the session permission policy. It is needed to produce the
  development candidate ZIP that the loader-compat session stages.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Build-LoaderCompatHarness.ps1`
  was **denied**. It is needed for the harness that injects the failure and
  drives character creation, tooltips and the summon menu in the disposable
  UMM 0.31.1 copy.
- `scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1` is not
  on the allowlist and needs both outputs above.
- Writing `GamePath.props` is explicitly denied. The allowlisted guarded live
  route (`.\scripts\Invoke-KingmakerRuntimeTest.ps1`) was **not** launched
  for three reasons:
  - it builds through Build-Local and deploys into the owner's installation
    (UMM 0.33.0 and the owner's mod stack, not the reporter's 0.31.1
    settings);
  - this session cannot snapshot `Params.xml` (the UMM `LastUpdateCheck`
    restoration gap) or check for a peer Kingmaker process;
  - no existing scenario injects an initialization failure or opens a new
    sorcerer's menu.

**No runtime transaction, lease or deployment was started; nothing needed
restoration.**

### 4.3 Required gate

The `focused` gate was run on the committed candidate through the enrollment's
`verify` helper with `-Filter elven-branched-spear`. Its receipt and outcome
are reported in the developer report. The gate passes no install directory, so
without `GamePath.props` in the worktree its compile step is expected to stop
with `No Kingmaker path is configured`.

## 5. Remaining limitations and risks for the owner

1. The reporter's failing predicate is still unknown (§2.4). If it was an
   owned-profile field inherited from a mutated donor, the repair removes it.
   If another mod patches a shared getter, startup now continues, with one
   warning naming the patch owners. If a getter differs with no external
   patch, startup still fails, but the message now names the predicate,
   values and owners.
2. **Design point for owner review:** an externally patched shared getter is
   tolerated (logged, not fatal) when the owned field is exact. The spear
   then follows the other mod's global rule, as every native weapon does.
   Making that case fatal again is a one-line policy change.
3. Eastern Weapons (registered right after the spear and before Expanded
   Summoning) still inherits `m_AttackType`, `m_AttackRange` and `m_IsNatural`
   from its native donors and checks `AttackRange.Value == 2` with a generic
   message. It was not reported as failing and was not changed (bounded
   repair). It is the next most likely cascade point if the reporter's cause
   is a range or natural-weapon mutation.
4. The full 43-mod environment, ArmsArmor 1.0.10 and the raw log are
   unavailable. Every native item in §4.2 remains open until a native run is
   authorized and executed.
