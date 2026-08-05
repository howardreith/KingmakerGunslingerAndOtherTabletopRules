# KingmakerGunslinger Pro Stabilization Handoff

## Purpose

This handoff contains a source-only stabilization patch prepared from the
user-uploaded `0.0.67` repository snapshot. It is intended for review and
integration by a fresh Codex instance on a new `codex/*` branch.

It is **not** a compiled, packaged, or runtime-qualified release. The patch must
not be committed merely because it applies cleanly or passes static validation.

## Source baseline

- Local working branch: `pro/stabilization-repair`
- Uploaded-source baseline commit:
  `368164d12c9807b1d2fb6740cb600e9efe8ef9d2`
- Version left unchanged: `0.0.67`
- No existing blueprint GUID was intentionally changed.
- No save, runtime evidence, generated package, or proprietary Kingmaker/Unity
  assembly is included.

The baseline commit is a local reconstruction of the uploaded repository, not a
claim about the latest GitHub branch head. Codex must compare the patch with the
current canonical `codex/seventh-playtest-player-path-repair` head before
integration.

## Human-observed state used as authority

### Accepted and frozen

- Manual Reload Firearm works.
- Immediate basic-ammunition crafting works.
- Beneath the Stolen Lands vendor publication works.
- Firearm killing-blow Grit recovery now occurs during combat.

The patch does not redesign those systems. Reload transaction services,
crafting costs/output, vendor quantities, and Grit-resource identity must remain
unchanged during integration.

### Still failed or unqualified

- Gunslinger's Dodge does not visibly or mechanically increase AC immediately.
- Scatter Shot has dealt damage, but its live cone animation and targeting
  presentation remain unqualified.
- Ordinary Blunderbuss attacks were incorrectly converted into Scatter Shot.
- Musket and Blunderbuss custom models disappeared; every custom firearm wrapper
  has failed at least one human visibility, grip, orientation, clipping, idle,
  or combat-pose check.
- Iterative full attacks cannot continue after a one-shot firearm empties unless
  a genuinely Free reload occurs inside the native attack command.
- Firearm audio remains human-failed/unqualified.

## Implemented source changes

### 1. Gunslinger's Dodge

Preserved stable feature, ability, buff, marker, and Grit-resource identities.

The ability now:

- remains a Swift extraordinary personal action;
- uses `CastAnimationStyle.Immediate` and fast animation;
- retains native `AbilityResourceLogic` for Grit spending;
- retains the True-Grit-aware custom cost calculator;
- delivers the existing one-round buff through
  `GunslingerDodgeProneAbilityLogic`;
- rejects activation while the same Dodge buff is already active.

The buff replaces generic `AddStatBonus` with
`GunslingerDodgeArmorClassBonus`, an owned lifecycle component that explicitly
adds and removes an exact `+2` `ModifierDescriptor.Dodge` modifier using the
same `ModifiableValue` pattern as the already-qualified Nimble implementation.

**Established by source:** the intended command, resource, buff, and modifier
wiring.

**Not established:** live click timing, Grit timing, buff creation, raw AC,
`RuleCalculateAC`, inventory/combat UI refresh, or one-round expiration in a real
character view.

### 2. Blunderbuss dual-mode behavior

The Blunderbuss definition now has a normal 10-foot range increment while
retaining `IsScatter=true`.

Removed:

- pre-command conversion of every loaded ordinary Blunderbuss attack into
  Scatter Shot;
- late `RuleAttackRoll` forced-miss behavior that made ordinary attacks
  harmless.

Ordinary Attack is again a normal single-target firearm attack. Scatter Shot is
an optional separate ability.

### 3. Scatter Shot native cone presentation

Scatter Shot now resolves the installed native Burning Hands blueprint
`4783c3709a74a794dbe7c8e7e0b1b038` and clones its exact
`AbilityDeliverProjectile` cone-presentation contract into the project delivery
component.

The ability now:

- uses point targeting;
- has an exact 15-foot custom range;
- copies native cone projectile, line width, length, timing, animation, fast flag,
  and resource preload identities;
- keeps the clicked point as the authoritative direction;
- runs the existing firearm attack/damage transaction after native presentation;
- logs a presentation failure but does not erase a valid mechanical discharge;
- treats an empty cone as a valid shot that consumes the chamber;
- commits chamber consumption before irreversible target damage and does not
  manufacture a round back after partial delivery.

Direct `ScatterShotRuntime.Execute` tests remain mechanics-only. They are not
clickable-command, visual, or audible evidence.

### 4. Free reloads between iterative attacks

Added a narrow prefix at the installed `UnitAttack.OnAction` boundary. Before a
later iterative attack fires, it inspects the previous completed attack and the
exact planned attack.

The same exact firearm may reload inside the existing native full-attack command
only when:

- the command is a full attack;
- a previous attack and a planned next attack both exist;
- both attacks use the same exact firearm item;
- the target remains alive;
- the firearm is empty but not effectively Wrecked;
- its calculated reload action is exactly `Free`;
- the existing atomic reload availability and transaction both succeed.

A loaded multi-capacity firearm continues without reloading. Non-Free, Wrecked,
unavailable, or failed cases end the remaining full attack before an empty fake
shot.

The patch does **not** promote Move, Standard, or Full-round reloads to Free.
Under the current matrix this primarily enables matching Rapid Reload on
Advanced Rifle and Advanced Revolver. Lightning Reload remains a separate deed.

### 5. Native visual fallback and transactional assets

All five custom equipped/belt wrapper capabilities are disabled for this
stabilization candidate because none has a complete human PASS.

`FirearmWeaponPresentation` now preserves the cloned native
`WeaponVisualParameters` contract:

- prototype;
- equipped model;
- belt and sheath models;
- attach slots;
- animation style;
- sound fields.

Only the firearm projectile and an individually approved, non-null custom
capability may override native presentation.

`FirearmAssetRuntime` now loads into temporary dictionaries and publishes the
bundle/caches transactionally. Missing, duplicate, or non-renderable custom
assets are logged as optional capability failures rather than leaving partially
published global state.

This intentionally produces visible native Light/Heavy Crossbow fallback art
instead of invisible or body-clipping custom guns. It is a stabilization result,
not final firearm art.

### 6. Audio

The patch stops zeroing native weapon sound fields, so native presentation sound
remains available as a fallback. The existing raw Unity firearm-clip path is
unchanged and remains human-failed/unqualified. A proper Wwise sound-bank/event
integration is a separate task.

## Automated-test and validator changes

- Added six pure policy tests for iterative full-attack reload decisions.
- Updated Blunderbuss definition, catalogue, touch-AC, Deadeye, and Scatter
  expectations for the 10-foot ordinary mode.
- Updated Scatter tests to distinguish command construction from direct
  mechanics-only transaction evidence.
- Updated Dodge runtime observation to run the granted command and inspect actual
  Grit, buff, and raw AC state rather than manually invoking resource/effect
  components.
- Updated static validators to reject the removed Blunderbuss conversion,
  forced-miss path, detached native presentation, and sound-zeroing fields.
- Current source test catalogue count: `871`.

## Validation completed in this workspace

Passed:

```text
python3 tools/validate_playtest67.py --root .
python3 tools/validate_repository.py --root .
git diff --check
```

`git diff --check` emitted only the repository's existing PowerShell CRLF
normalization notices.

Additional source-only checks passed:

- all main-project and test-project `<Compile Include>` paths exist;
- all registered domain-test case methods resolve uniquely;
- changed C# files have balanced delimiters after comments and literals are
  excluded.

Unavailable here and therefore **not run**:

- complete domain-test executable;
- exact private-reference Release compile;
- strict package build/validation;
- guarded Steam runtime scenarios;
- working-save regression;
- live AC/UI observation;
- live Scatter cone/VFX observation;
- live full-attack action-state observation;
- visual firearm-model review;
- audible firearm review.

## Required Codex integration review

Codex must inspect rather than blindly accept these source assumptions:

1. `AbilityDeliverProjectile` subclass serialization and Burning Hands cone
   delivery on the exact installed Kingmaker build.
2. `CastAnimationStyle.Immediate` plus custom Dodge delivery through a real live
   `UnitUseAbility` command.
3. `UnitAttack.OnAction` prefix timing relative to `PlannedAttack`,
   `LastAttackRule`, animation release, target replacement, and command result.
4. Atomic reload state after each iterative firearm shot, including capacity
   weapons and weapon switching.
5. Native `WeaponVisualParameters` fallback identity at both weapon-type and
   item levels.
6. Any interaction between restored native weapon sound and the existing raw
   Unity shot invocation.

## Mandatory runtime and human acceptance matrix

### Frozen regressions

- Manual Reload Firearm: all five firearm families.
- Matching and mismatched Rapid Reload action economy.
- Immediate ammunition crafting, once per rest, exact cost/output.
- BTSL vendor inventory.
- Firearm killing-blow Grit recovery.
- Existing item state, condition, misfire, repair, save identity, and native feat
  integration.

### Gunslinger's Dodge

A real action-bar click must prove, in order:

1. command accepted;
2. Grit spent immediately;
3. Dodge buff added immediately;
4. raw AC increases by exactly 2;
5. `RuleCalculateAC` reflects the same +2;
6. inventory/combat UI reflects the change;
7. no firearm shot or enemy attack is required;
8. insufficient Grit changes nothing;
9. True Grit produces the authorized zero cost;
10. buff expires after one round and AC returns to baseline.

### Ordinary Blunderbuss

- A loaded ordinary attack at legal range performs one normal firearm attack.
- It consumes one chamber, deals normal firearm damage, and allows Reload
  afterward.
- It does not invoke Scatter Shot.
- Empty ordinary attack follows the qualified empty-firearm reload/rejection
  behavior.

### Scatter Shot

- Action bar displays a directional 15-foot cone rather than a large Close-range
  circle.
- Native cone presentation is visible and correctly oriented.
- One target and multiple targets each receive one independent `-2` firearm
  attack.
- Out-of-cone targets are untouched.
- A legal empty cone still consumes one chamber.
- Cancellation before delivery consumes nothing.
- Completed delivery consumes one chamber exactly once and enables Reload.
- No spell damage, save, descriptor, or caster-level scaling is introduced.

### Iterative full attacks

- Matching Rapid Reload Advanced Rifle can fire, reload for Free, and continue
  the same native full attack.
- Advanced Revolver continues through remaining loaded capacity and reloads only
  when empty and another iterative attack remains.
- Non-Free Pistol/Musket/Blunderbuss reloads do not occur inside the full attack;
  remaining empty attacks end without fake projectiles or ammunition mutation.
- Wrecked, missing-ammunition, switched-weapon, dead-target, interrupted, and
  failed reload cases stop safely.
- Actual turn-based move, standard, swift, and full-round availability is
  recorded before and after.

### Visual and audio

- Native fallback must be visible for every firearm in inventory, idle, draw,
  attack, switching, and holster states.
- Native fallback is not final firearm-art acceptance.
- Audible behavior requires a human verdict. Event counters or `PlayOneShot`
  invocation are not audible evidence.

## Commit gate

Do not commit until:

- patch review is complete;
- complete deterministic suite passes;
- clean Release build passes;
- strict package validation passes;
- guarded runtime evidence passes;
- frozen regressions remain green;
- human-observed Dodge, Scatter, ordinary Blunderbuss, full-attack reload,
  fallback visibility, and audio findings are accurately recorded.

Prefer one functional commit for the coherent stabilization patch and a separate
evidence-only commit only when the repository workflow calls for it. Do not
rewrite prior evidence or claim human-gated behavior from automation.
