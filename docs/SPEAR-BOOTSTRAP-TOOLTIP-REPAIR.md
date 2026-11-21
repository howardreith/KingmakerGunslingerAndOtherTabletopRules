# Spear startup, summoning and weapon-tooltip repair

Investigation and repair record for
`planning/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR-MISSION.md` (draft PR #195, branch
`codex/spear-bootstrap-tooltip-repair`, prepared from `master` `2e61e78097`,
v0.0.192). This is a focused development checkpoint under
`docs/VALIDATION-POLICY.md`. It does not cover release, packaging or broad
compatibility qualification.

Identities:

- Product source: commit `288ce4dd07`. Later commits change only the test-only
  loader-compat harness, its sessions and session runner, and this record.
- Native development candidate, built from `56e00163e9d1` with the product
  source unchanged from `288ce4dd07`:
  `KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-56e00163e9d1.zip`.
  - Package SHA-256 `b7b4d619d5e020bf259d4576f52820ebea890a66727992a2a21c99ab25b7083e`.
  - DLL SHA-256 `031dc659498f0c6a591f45abc22bf13ed1837f5babfcb8b34148b9af8c9e130a`.
- Baseline: the published v0.0.192 package
  `KingmakerGunslinger-0.0.192-gunslinger-remediation.zip`. Package SHA-256
  `936f3571a5f948911b410e6e39134629b60d0fc469d0cd6347d88971af072d69`; in-process
  DLL `0eff2af3…`. Its spear and battered sources are byte-identical to
  v0.0.188.
- Native sessions ran with harness commit `4ba6cab678`.
- The controller's archive and handoff commits are separate from all of these.

## 1. Reported failure

These details come from the owner's transcription of the reporter's log. The
raw log was never available here and is not committed. The reporter ran
Kingmaker 2.1.7b, UMM 0.31.1, Tabletop Rules Expansion 0.0.188 and 43 mods,
including Call of the Wild 1.14.4c-2.1, ArmsArmor 1.0.10, RacesUnleashed
1.0.11, KingmakerBugfixes 1.3.3 and CraftMagicItems 2.1.0, with
`gunslinger=False`.

1. `SpearWeaponTypeAccess.Validate` threw `Elven Branched Spear type profile is
   invalid.` inside `ElvenBranchedSpearBlueprints.Register`. Startup rolled
   back 287 registrations and stopped initialization for the process.
2. Expanded Summoning never registered. Summoning hooks reported
   `package-blueprint-lifecycle-failed` and `bootstrap-unavailable`.
3. Every weapon tooltip threw `The battered origin token blueprint is
   unavailable.` from `BatteredFirearmSaleValuePatch` (a
   `VendorLogic.GetItemSellPrice` postfix called from
   `UIUtilityItem.FillTooltipData`).

The same log records a normal Summoned Dog from Summon Monster I.

## 2. Findings

### 2.1 Engine contracts (Kingmaker 2.1.7b)

These were read from the decompiled `Assembly-CSharp` (`BlueprintWeaponType`,
`Feet`, `GameConsts`, `DamageTypeDescription`, `DiceFormula`, the action-bar,
level-up and vendor types). They were confirmed natively in §4.2.

- `BlueprintWeaponType` getters return their private serialized fields
  directly: `AttackType`, `BaseDamage`, `DamageType`, `CriticalRollEdge`,
  `CriticalModifier`, `FighterGroup`, `IsTwoHanded`, `IsLight`, `IsMonk`,
  `IsNatural`, `IsUnarmed` and `Weight`. `Category` is a public field.
- `AttackRange` is computed. A serialized range of 10 ft or less returns
  `max(GameConsts.MinWeaponRange (2 ft), range − 4)`; a longer range is
  returned unchanged. The old `AttackRange.Value == 6` check therefore
  required a serialized 10 ft range.
- Outside trading, `VendorLogic.GetItemSellPrice` is
  `(long)(item.Cost × Vendors.SellModifier)`. `UIUtilityItem.FillTooltipData`
  calls it for every item tooltip.
- Kingmaker's own Summon Monster I (`SummonMonsterISingle`,
  `8fd74eddd9b6c224693d9ab241f25e84`) has **no variants**. Only the product's
  Expanded Summoning publication gives it a creature list. Without that list,
  `ActionBarGroupSlot` shows no expand control, and a click starts targeting
  for the spell itself.

### 2.2 What the old constructor relied on

`ElvenBranchedSpearBlueprints` cloned the live native Longspear type
(`fa2dd17cbde7d3f4aa918d467c30516e`). `SpearWeaponTypeAccess.Configure` wrote
only category, text, weight and enchantments. `Validate` then checked melee,
1d8 piercing, 20/×3, Spears, two-handed / not light / not natural, 6 ft
effective range and 10 lb through the getters. Everything except category and
weight was the **donor's** value at clone time. So any mutation of those donor
fields before this mod's bootstrap, or a patch on one of those getters, failed
the whole initialization transaction with a message that named no predicate
and no value.

Owned patches that touch the checked values:

- `ElvenBranchedSpearCategoryRuntime` patches the category's sub-category and
  display lookups. Validation uses the mod's own static policy, not the
  patched engine method.
- The mod's `get_AttackRange` patches target `ItemEntityWeapon`, not
  `BlueprintWeaponType`.

### 2.3 Third-party scan (offline) and native partial-stack attempt

- **Offline.** Byte searches of the local mod DLLs found no reference to the
  Longspear type GUID in Call of the Wild, KingmakerBugfixes, CraftMagicItems
  or RacesUnleashed.
  - Call of the Wild only reads `AttackRange`.
  - KingmakerBugfixes 1.3.3 has `SetFighterGroup` / `SetIsLight` helpers whose
    targets could not be determined offline. Its public source could not be
    fetched from this session.
  - ArmsArmor 1.0.10 is not available on this host.
- **Native.** Session `spear-bootstrap-reporter-stack` (§4.2, session R) ran
  the **unrepaired v0.0.192** under UMM 0.31.1 with:
  - the four reported mods this lab holds, at the reported versions: Call of
    the Wild 1.14.4c-2.1, CraftMagicItems 2.1.0, KingmakerBugfixes 1.3.3 and
    RacesUnleashed 1.0.11;
  - the reporter's module settings, applied through real panel clicks:
    Gunslinger, Shield Other, Urban Barbarian, Bodyguard feats and
    Teleportation off; the reported modules on.

  Initialization **completed**, and the native Longspear passed every
  old-release predicate. **That partial stack does not reproduce the
  report.**

### 2.4 Established mechanism and remaining uncertainty

Established natively (simulated condition, §4.2 session B). When the live
Longspear carries values the old validation rejects, the unrepaired package:

1. fails with the generic message;
2. rolls back every owned registration (no spear type remains);
3. throws `The battered origin token blueprint is unavailable.` on every
   ordinary weapon price and tooltip query;
4. leaves Summon Monster I as Kingmaker's own unexpanded spell, which still
   summons a Summoned Dog.

This matches every observable in the report. The donor in that simulation had
a 15 ft reach and the Polearms group; that is a stand-in, not the reporter's
values.

**Not established:** which predicate failed for the reporter, and which of
the missing ~39 mods (or other local change) caused it. The raw log, the full
stack and ArmsArmor are unavailable, and no mod is blamed.

The reporter's "empty stub" wording is consistent with an unexpanded native
Summon Monster I (no creature list to expand). No menu that opens empty was
observed in any run.

## 3. Changes

### 3.1 Spear construction (`Blueprints/ElvenBranchedSpearBlueprints.cs`)

- `SpearWeaponTypeAccess.Configure` writes the whole fixed mechanical profile
  onto the **owned clone**: melee; 10 ft serialized reach; catalog 1d8; a new
  physical/piercing `DamageTypeDescription`; 20/×3; Spears; two-handed; not
  light, monk, natural or unarmed; no attack-stat override; 10 lb; the
  movement-accuracy enchantment. Nothing mechanical is inherited from the
  donor.
- The native donor is never written. `Register` fingerprints every serialized
  mechanical field of the donor, plus the identity of its reference-typed
  values, before construction. It refuses if anything changed afterwards. The
  earlier category, weight and item-type checks remain.
- Validation is kept and is stricter. It covers 15 predicates through the new
  game-independent `ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs`; the
  enchantment and sub-category checks are unchanged. Each predicate compares
  the owned serialized field with the intended value, and the live getter
  with the engine contract for that field.
  - Any owned-field mismatch is fatal.
  - A getter mismatch is fatal unless the getter's Harmony patch info names an
    owner other than this mod.
  - A getter changed by another mod's patch, while the owned field is exact,
    is logged once as warning `elven-branched-spear` /
    `profile.external-getter-patch` and does not fail startup.
- Diagnostics: one message, logged once by the existing initialization
  failure records:
  `Elven Branched Spear type profile is invalid: N of 15 checks failed.`
  followed by, per failed predicate:
  - the predicate name and failure kind;
  - expected, actual and donor serialized values;
  - expected, actual and donor getter values;
  - the getter's Harmony patch owners (`none`, the owner list, or
    `unavailable:<reason>` — the installed Harmony12 facade throws for an
    unpatched method).

  Values are capped at 96 characters, owners at 8 and entries at 24. Patch
  info is read only for failing predicates.

### 3.2 Tooltip and pricing (`Gunsmithing/`)

- New `BatteredFirearmOriginRuntime.IsBatteredForInspection`, used only by the
  sale-price hook. It is decided by `BatteredFirearmOriginInspectionPolicy`:
  - **Unavailable**: initialization is pending or failed, and the token
    blueprint was never registered. No live item can carry an enchantment of a
    blueprint that does not exist in the process, so the weapon is not
    battered and keeps its native price. One warning (`gunsmithing` /
    `battered-origin.inspection-unavailable`) is logged per process.
  - **Inspect** in every other case, including successful initialization
    without the token. That is an invariant violation, and
    `RequireBlueprint` still throws. Multiple tokens or a token without a
    stable owner still throw.
- `Bind`, `TryGetOwner` and the creation, transfer and CMI paths still require
  the blueprint. `RequireBlueprint`'s message now says why it is missing.
- Not gated on the Gunslinger toggle. The token registers unconditionally in a
  successful initialization.

### 3.3 Failure containment, inspected and unchanged

- `ExpandedSummoningVariantMenuPatch` and
  `ExpandedSummoningPublisher.IsPublishedExpandedParent` act only on the
  product's published variants component. After a failed startup they leave
  the native spell alone, which §4.2 confirmed.
- The other tooltip hooks already return to native behaviour when
  initialization did not complete:
  - `FirearmConditionTooltipPatch` and `FirearmQualitiesTooltipPatch`
    (`FirearmRuntimeState.IsConfigured`);
  - `BasicAmmunitionSaleValuePatch` (null ammunition set);
  - the quality-filter transpilers.
- The summoning blocked-state messages are truthful and were not changed.
- `BlueprintBootstrap.cs` (hash-pinned), GUIDs, registration order, released
  identities, module publication and the summon roster are unchanged.

### 3.4 Test-only native harness (`tools/LoaderCompatHarness`, `scripts/compatibility/loader-compat`)

Nothing here is packaged with the product.

- `SpearRepairProbes.cs` adds conditions, all applied before the product
  initializes:
  - `mutateNativeLongspear` (simulated donor mutation);
  - `captureWeaponTypes` (pre-product weapon-type fingerprints);
  - `corruptOwnedSpearProfile` (writes onto the owned clone as it is
    validated);
  - `patchWeaponTypeGetters` (harness-owned identity postfixes);
  - `externalAttackRangePatch` (+1 ft on the shared `AttackRange` getter,
    limited to the Longspear family).
- It also adds three ops:
  - `kmgWeaponTypeProfile`: owned spear fields and getters, native donor
    fields, getters and patch owners, old-release clone predicate failures,
    and unchanged-since-capture checks.
  - `kmgItemPricing`: native `GetItemSellPrice` and `FillTooltipData` on
    ordinary weapons, repeated; a production pistol bound as battered to the
    main character.
  - `kmgSorcererSummon`: a fresh level-1 sorcerer created through
    `LevelUpController` in character-generation mode with the native class
    path, learning Summon Monster I through `SelectSpell`. It joins the party
    and is selected as a portrait click does. The probe opens the action
    bar's spell group with its own click handler, clicks the Summon Monster I
    slot, reads the creature menu the game filled (names, icons, visibility),
    casts the first entry with the `AbilityData` that slot holds, and waits
    for the summoned unit. Everything it creates is removed.
- `Invoke-LoaderCompatSession.ps1` gains an exactly-once `expectLog` group.
- Three sessions: `spear-bootstrap-reporter-stack.json`,
  `spear-bootstrap-simulated.json` and `spear-bootstrap-baseline.json`.

## 4. Validation

### 4.1 Build and focused domain checks

`GamePath.props` was absent in the first invocation; the operator has since
supplied it in the worktree.

| Command | Result | Time |
|---|---|---|
| `.\scripts\Invoke-KmgGate.ps1 -Filter 'elven-branched-spear' -KingmakerInstallDir …` | production compile passed; **25 of 3158** tests passed (10 new `elven-branched-spear.bootstrap-repair.*` policy cases and 15 existing spear cases) | compile 0.4 s incremental, tests 2.4 s |
| same, `-Filter 'battered;craft-magic-items;sale;vendor;player-facing'` | 98 passed | 0.8 s |
| same, `-Filter 'summoning-publication-boundary;summon.publication;alternate-summoning;summon-variant-menu'` | 43 passed | 0.9 s |
| `powershell.exe -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir … -LoaderReferenceDir …` at `56e00163e9d1` | development candidate built; strict standalone UMM package validation passed; the same 25 tests passed | about 6 min |
| `.\scripts\compatibility\loader-compat\Build-LoaderCompatHarness.ps1 …` (pinned UMM 0.31.1 references) | compiled | seconds |

The domain cases test the production policies directly and are supporting
evidence only.

### 4.2 Native evidence (disposable loader-compat game copy)

Environment for all sessions:

- Kingmaker 2.1.7b, in the disposable copy
  `C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat`.
- UMM **0.31.1.0**, in-process SHA-256 `2e038ff2…`; Harmony 2.3.1.1 and the
  0.31.1 Harmony12 facade.
- Harness commit `4ba6cab678`; fixture save `KMCS_AFTER_NORMAL_012_MODONLY.zks`
  in a per-session profile; `locale=enGB`.

Each session verified in-process loader and product identity. Each restored
the game's registry settings and confirmed the owner's saves and installation
unchanged (`registryRestored`, `ownerSavesUnchanged` and
`ownerInstallUnchanged` all true). No owner save or normal-play installation
was touched, and no runtime lease was opened.

| Session (evidence under `runtime-evidence/loader-compat/`) | Package | Result |
|---|---|---|
| **R-cand** `20261121T154235Z-…-spear-bootstrap-reporter-stack` | candidate | PASS (107 s + 115 s) |
| **R-base** `20261121T153842Z-…-spear-bootstrap-reporter-stack` | v0.0.192 | PASS (101 s + 109 s): the partial stack does not reproduce |
| **B** `20261121T155017Z-…-spear-bootstrap-baseline` | v0.0.192 | PASS (72 s): the unrepaired bytes reproduce the cascade under the simulated condition |
| **S** `20261121T154637Z-…-spear-bootstrap-simulated` | candidate | PASS (70 s, 73 s, 61 s) |

Mission coverage:

1. **Spear construction (4.1.1).**
   - S-1: simulated donor mutation, native Longspear at 15 ft and Polearms;
     old-release predicates `fighter-group` and `attack-range` fail on it.
     Initialization completed, and the owned spear has the intended profile:
     category `0x004b4d47`, Melee, 10 ft, 1×D8, Physical/Piercing, 20, X3,
     Spears, two-handed, not light, monk, natural or unarmed, no override,
     10 lb.
   - The native Longspear (still mutated, exactly as captured before the
     product ran) and the unrelated Heavy Crossbow type show no changed field.
   - R-cand shows the same owned profile on the reporter-shaped stack.
2. **Failure diagnostics (4.1.2).**
   - S-2 corrupts the owned clone's fighter group, with a harness postfix on
     the getter. The loader log records once:
     `Elven Branched Spear type profile is invalid: 1 of 15 checks failed.
     fighter-group [owned-field]: expected=Spears actual=Polearms donor=Spears;
     getter expected=Polearms actual=Polearms donor=Spears;
     getterPatches=KmgLoaderCompatHarness`.
   - S-3 shows the tolerated case, logged exactly once:
     `attack-range [external-getter-patch]: expected=10ft actual=10ft
     donor=10ft; getter expected=6ft actual=7ft donor=7ft;
     getterPatches=KmgLoaderCompatHarness`. Initialization completed. The
     v0.0.192 rule would have failed this donor (`attack-range`).
3. **Failed startup (4.1.3).**
   - After S-2's failed initialization, 25 repeated `GetItemSellPrice` and
     `FillTooltipData` queries each on the Standard Longspear, Longsword,
     Greataxe and Heavy Crossbow threw nothing. Prices were native (1, 4, 5,
     13 gp), and the tooltip `SellPrice` text matched.
   - `battered-origin.inspection-unavailable` appeared **exactly once**; the
     missing-token exception, zero times.
   - The same queries on the unrepaired package (B) threw `The battered origin
     token blueprint is unavailable.` on every call (5/5 per item, price and
     tooltip).
4. **Normal pricing (4.1.4).** In S-1 and R-cand, ordinary weapons sold at the
   native price. A production pistol (`a303d71d…`) bound as battered to the
   main character sold for the fixed 22 gp; the unbound pistol sold at its
   native price. On R-cand the only `GetItemSellPrice` patch owner was this
   mod.
5. **Disabled publication (4.1.5).** R-cand ran with `gunslinger=False`
   (`Gunslinger=False` active in the process). Initialization completed,
   battered pricing kept its fixed value, and ordinary pricing stayed native.
   No identity or registration changed.
6. **Summoning (4.1.6).** R-cand: a fresh level-1 sorcerer learned Summon
   Monster I through the native level-up selection.
   - The real spell group opened and its Summon Monster I slot was clicked.
     The creature menu opened with **7 visible entries**, each with a name and
     an icon: Fire Beetle, Dog, Eagle, Poisonous Frog, Pony, Dire Rat and
     Viper. RacesUnleashed's crab spider is the parent's eighth variant and is
     gated without its summoner feature, as before.
   - The probe cast Fire Beetle from that slot: command `Success`, and a Fire
     Beetle was summoned. The screenshot is in R-cand's harness folder.
   - After S-2's failed startup, Summon Monster I stayed Kingmaker's own
     single spell. No menu opened, and casting it summoned a Summoned Dog.
7. **Regression scope (4.1.7).** Covered by the spear, publication, summoning
   and pricing neighbours above. No fresh-process save/load check is needed:
   persistence, registration order, identities and saved-item interpretation
   are unchanged, and the inspection path only reads.

Harness iterations before the final runs, kept as evidence:

- `151241Z`: JSON conversion defect.
- `151542Z`: the level-up path skipped `ApplySpellbook`.
- `151929Z`: wrong scrap-value namespace.
- `153059Z`: launch 1 exited during Steam start-up before the main menu
  (environmental), so launch 2 ran with default modules.
- `153347Z`: v0.0.192 read the menu after the action bar had fallen back to
  the main character. The probe now confirms the bar's selected unit before
  each click and records retries. None were needed in the final runs.

### 4.3 Required gate

The `focused` gate runs on the final committed candidate through the
enrollment's `verify` helper with `-Filter elven-branched-spear`. Its receipt
is in the developer report.

## 5. Remaining limitations and risks for the owner

1. The reporter's exact failing predicate and its cause remain unknown
   (§2.4). The four available reported mods do not reproduce it, even on the
   unrepaired package. ArmsArmor 1.0.10, about 38 other mods and the raw log
   are unavailable.
2. **Design point for owner review.** An externally patched shared getter is
   tolerated (logged once, not fatal) when the owned field is exact. The spear
   then follows the other mod's rule through the shared getter. The warning
   text says the rule "applies to every weapon", which assumes a global
   patch; S-3's stand-in was limited to the Longspear family.
3. Eastern Weapons, registered before Expanded Summoning, still inherits
   `m_AttackType`, `m_AttackRange` and `m_IsNatural` from its donors and checks
   `AttackRange.Value == 2` with a generic message. It was not reported as
   failing and was not changed. A reach or natural-weapon mutation of its
   donors would still stop startup there.
4. Native evidence comes from a disposable copy with the product alone or
   with four of the reported mods. It is not the reporter's installation.
