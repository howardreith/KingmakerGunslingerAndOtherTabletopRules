# Character race traits feasibility: Fiery Glare, Stoic Dignity, Earthsense

**Mission:** Z weekend 2026-10-03, slices 3–5.
**Status of this document:** research + technical finding only. **No production
code, blueprint, registration, icon or patch for any of these three traits was
changed or added.** Each trait ends this mission blocked; the per-trait
conclusions and the exact unblock path are below.

All engine findings below come from read-only inspection of the legally
installed game assemblies through the repository's established practice
(members read from `Kingmaker.dll`-equivalent `Assembly-CSharp` metadata via the
lab's existing IL dump at
`C:/Dev/KingmakerGunslingerLab/private/charvis-native-il/Assembly-CSharp.il`;
no proprietary source is reproduced here, only member names, types and behavior
summaries).

## Source rule identity (public references)

All three are tabletop **character race traits** — chosen as one of a
character's starting traits, keyed to a race — **not** ARG alternate racial
traits and not racial feats:

- Fiery Glare (Ifrit) — *"You can always take 10 on Intimidate checks, even in
  combat."* Bastards of Golarion pg. 29 (public reference:
  d20pfsrd `traits/race-traits/fiery-glare-ifrit/`, now HTTP 410; Archives of
  Nethys `TraitDisplay.aspx?ItemName=Fiery Glare` confirms the text). No
  replacement slot.
- Stoic Dignity (Oread) — while conscious, the holder gains a **+1 trait
  bonus** on saves against mind-affecting effects not already suffered, and
  allies within 10 feet gain a **+1 morale bonus** under the same restriction.
  Bastards of Golarion pg. 29. No replacement slot.
- Earthsense (Oread) — swift action, tremorsense 60 ft. until the start of the
  user's next turn; uses per day 1 at level 1, +1 at 5th and every 5 levels
  after, maximum 5 at 20th. Blood of the Elements pg. 9. No replacement slot.

## Offering mechanism analysis (shared)

The mod has exactly three established trait-like offering systems:

1. **Elemental alternate-trait slot selections**
   (`ElementalAlternateTraitPolicy`: 21 traits, 10 slot selections, three base
   slots — Energy Resistance / Elemental Affinity / Racial SLA — per race).
   Every trait in this system replaces at least one base racial slot, matching
   its printed ARG replacement. All three mission traits are character race
   traits with **no printed replacement slot**; assigning one would be an
   invented balance decision (mission §4.1 forbids autonomous balance
   decisions). Not usable without a human design ruling.
2. **Elemental racial feats** (native BasicFeatSelection / Fighter combat feat
   slots). These are racial *feats*; using a feat slot for a starting race
   trait would also be a new adaptation decision, and no project precedent
   maps race traits into feat slots.
3. **Favored Class (ZFavoredClass 1.3.1) trait-category publication** — the
   project's established representation of the tabletop *character trait*
   system. The mod already publishes save-stable, race/feature-gated choices
   into the installed FC selections through the exact-commit
   `FavoredClassTraitResolver` contract and the atomic, idempotent,
   conflict-rejecting `HelpfulPublicationTransaction`
   (`Heirloom Weapon: Nodachi` into Equipment Traits since 0.0.93; combat
   `Helpful` into Combat Traits). FC exposes a `racial_traits`
   `BlueprintFeatureSelection` (already read and contract-checked by
   `FavoredClassTraitResolver`), and the FC global Traits phase demonstrably
   appears and functions for the mod's elemental races (elemental character
   creation stabilization acceptance checklist, step 6).

**Conclusion:** the faithful offering mechanism is (3): register KMG-owned
save-stable trait feature blueprints and late-publish them into FC's
`racial_traits` selection, race-gated by a native `PrerequisiteRace` bound to
KMG's `BlueprintRace` (`KMG.ElementalRaces.Ifrit.Race` /
`KMG.ElementalRaces.Oread.Race` are real registered `BlueprintRace` objects),
with `PrerequisiteNoFeature(self)` for duplicate prevention (Heirloom
precedent). Publication stays conditional on FC present + compatible +
`enable_traits` + the Elemental Races module, exactly like the Heirloom and
Helpful publications. This preserves the tabletop trait-category semantics,
the project's standalone behavior when FC is absent, and existing saves.

## Fiery Glare (Ifrit) — mechanics FEASIBLE; blocked on icon authoring

### Engine contracts (verified)

- Intimidate is a distinct engine stat from Persuasion:
  `Kingmaker.EntitySystem.Stats.StatType.CheckIntimidate = 0x67`
  (`CheckBluff = 0x65`, `CheckDiplomacy = 0x66`). Dialogue Intimidate checks
  and the combat Demoralize action both resolve as `RuleSkillCheck` with
  `StatType.CheckIntimidate`; the mod's own qualified I07 adaptation
  (`FavoredClass/Mechanics/FavoredClassIntimidateBonus.cs`,
  `FavoredClassDemoralizeScope.cs`) already distinguishes them exactly.
- The engine has a **native take-10 fact component**:
  `Kingmaker.Designers.Mechanics.Facts.Take10ForSuccess`
  (fields `Skill : StatType`, `MagicDeviceType`), an
  `IInitiatorRulebookHandler<RuleSkillCheck>` that sets
  `RuleSkillCheck.Take10ForSuccess = true` when `evt.StatType == Skill` (with a
  special case restricting Use Magic Device to the matching device type).
- `RuleSkillCheck.OnTrigger` consumes the flag precisely: when no
  `EnsureSuccess` override exists and `Take10ForSuccess` is set **and**
  `10 + StatValue + Bonus >= DC`, the d20 result is set to exactly 10;
  otherwise the check rolls normally. This is the engine's own take-10
  semantic — it substitutes 10 exactly when taking 10 succeeds, and never
  converts a take-able success into a failure.

### Faithfulness assessment

"Can take 10 on Intimidate, even in combat" is preserved:

- Only `StatType.CheckIntimidate` events qualify (the component filters by
  stat); Bluff/Diplomacy/Persuasion and all other checks are untouched.
- Optionality can be preserved with the repository's established no-action
  toggle pattern (`BlueprintActivatableAbility` off-by-default, `Immediately`,
  free, `DeactivateImmediately = true` — `BodyguardModeBlueprints.cs`), whose
  activation buff carries the `Take10ForSuccess` component. No action,
  resource or daily use is consumed, matching the mission's preferred
  interaction. (Under the native semantic the toggle is even strictly
  player-favorable: with it on, a check takes 10 only when 10 succeeds and
  rolls otherwise, which is the optimal per-check choice in every case.)
- The trait feature (FC `racial_traits` publication, `PrerequisiteRace`
  Ifrit) grants the activatable through an `AddFacts` component
  (established pattern, e.g. `DeadShotBlueprints`).
- No global patch, no new framework, no effect on units without the trait.

### Blocker

The mission (§8 slice 3, §9) permits a feature commit only when the feature
**and its original icon** meet qualification. This session has no authorized
image-generation tool: the existing trait icons' production briefs record
`"tool": "built-in image_gen"` (an agent-session built-in not available here);
no local generator exists on DATA; `docs/ICON-ART-GUIDE.md` forbids
procedural paintings as substitutes, and the mission forbids donor art,
monograms and unrelated fallbacks. Per §9 the icon blocker is recorded and the
slice ends without a feature commit rather than shipping a player-visible
trait with a missing/unrelated icon.

**Unblock path (no design questions remain):** author the approved
`fiery-glare` concept (ember-lit eye/flame iris, painted-magical family,
`ifrit-traits` review group, `project-painted-128` profile) with the built-in
image tool; then implement exactly: register the trait feature + toggle
activatable + `Take10ForSuccess(Skill=CheckIntimidate)` buff under
`KMG.ElementalRaces.Ifrit.*`-style stable symbols; publish into FC
`racial_traits` behind `PrerequisiteRace(KMG Ifrit)` + `PrerequisiteNoFeature`
via the existing publication transaction; add catalog/OwnedIconAssignments
entries and the domain tests listed in the mission.

## Stoic Dignity (Oread) — mechanics FEASIBLE; blocked on icon authoring

### Engine contracts (verified)

- `Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect` implements
  `OnFactActivate`/`OnFactDeactivate` (fact lifecycle, not buff-only), so a
  permanent racial trait feature can own a persistent moving area effect
  directly.
- `BlueprintAbilityAreaEffect` with `Shape = AreaEffectShape.Cylinder`,
  `Size = 10.Feet()` (the project's canonical 10-ft emanation conversion,
  used verbatim by the qualified Magic Circle carrier) and
  `AffectEnemies = false` plus an `AbilityAreaEffectBuff` delivery with the
  native ally predicate (`ContextConditionIsAlly`, the project's established
  willing-ally convention) applies a recipient buff to covered allies only.
- Mind-affecting identification follows the established descriptor walk
  (`SpellDescriptor.MindAffecting = 0x10`; context + parent-context + source
  ability chain, exactly as the qualified
  `ElementalAlternateTraitSaveBonus` already does for
  Enchantment/Divination).
- "Not already suffering from" is checkable at save time by walking the
  unit's active buffs' source contexts/abilities for the MindAffecting
  descriptor (same helper as above).
- Consciousness: `Kingmaker.UnitLogic.UnitState.IsConscious` (also
  `IsDead`) exists and is the natural holder-active gate; checking it inside
  the recipient-buff save component (holder =
  `buff.Context` caster) makes the aura stop granting the benefit the moment
  the holder drops, even before area tick boundaries.
- Bonus typing: holder self-bonus as `ModifierDescriptor.Trait`, ally bonus as
  `ModifierDescriptor.Morale` — the engine's descriptor stacking then
  enforces "duplicate Stoic Dignity morale bonuses do not stack" and normal
  coexistence rules natively (the existing save-bonus component's temporary
  modifier + `ConditionalWeakTable` once-per-rule pattern prevents replay
  double-dipping).
- Holder self-exclusion: the recipient component skips the check when the
  save's initiator is the aura's own holder, so the holder never receives
  their own morale aura on top of their trait bonus.

### Blocker

Identical icon-authoring blocker as Fiery Glare (same §8/§9 rule). No
mechanics question remains; no production code was changed.

**Unblock path:** author the approved `stoic-dignity` concept (calm dignified
stone face/mask with subtle protective radiance, painted-magical family,
`oread-traits` review group); then implement: trait feature (hidden marker +
provider, following the 21-trait factory conventions but published through FC
`racial_traits` rather than a slot selection) carrying (a) the self save
component (trait descriptor) and (b) `AddAreaEffect` → cylinder 10 ft ally-only
area → recipient buff carrying the ally save component (morale descriptor,
holder-conscious + holder≠self + not-already-suffering gates). Mission test
list maps 1:1 to domain tests plus the existing runtime scenario harness.

## Earthsense (Oread) — BLOCKED on engine capability (no tremorsense exists)

### Evidence

- The complete `Assembly-CSharp` metadata contains **zero** matches for
  `Tremor*` (case-insensitive) — no tremorsense feature, component, fact,
  condition, descriptor or sense type exists in Kingmaker.
- The engine's sense/visibility model is: `UnitCondition` flags
  (`Invisible = 0x07`, `SeeInvisibility = 0x11`, `TrueSeeing = 0x12`,
  `Blindness = 0x01`, ...) plus `Kingmaker.UnitLogic.UnitStealth` with
  perception-based detection. There is no distance-limited sense, no
  ground-contact detection, and no partial-detection tier between
  `SeeInvisibility` and full `TrueSeeing`.
- No mod or project decision establishes a tremorsense adaptation
  (`planning/` and `docs/` contain no tremorsense reference).

### Gate application

The mission's faithful-implementation gate explicitly forbids substituting
blindsight, blindsense, see-invisibility, perception or all-target detection.
A faithful tremorsense (reveal hidden/stealthed/invisible grounded creatures
within 60 ft, no line-of-sight requirement) would require a broad Harmony
patch across the native stealth/detection and target-eligibility systems —
exactly the "invasive global patch" the gate bars. Swift-action activation,
daily uses by total character level (1/2/3/4/5 at 1–4/5–9/10–14/15–19/20+),
and one-turn expiration are all expressible with established resource/ability
patterns (`ElementalTraitDailyResourcePolicy`, `ElementalRaceAbilityFactory`)
— the blocker is solely the tremorsense effect itself.

**Conclusion: NOT-CURRENTLY-SAFE.** Leave production behavior unchanged
(done). If the owner wants an adaptation decision (e.g. "Earthsense grants
`SeeInvisibility`-equivalent detection of adjacent/grounded creatures"), that
is a player-facing design decision requiring explicit owner approval, and the
gate says not to make it autonomously.

## Icon authoring constraint (all three slices)

`docs/ICON-ART-GUIDE.md` requires original paintings produced with "an
available built-in image tool or authorized artist", with production briefs
recording the exact tool and prompt; the guide explicitly rejects procedural
substitutes and unrelated donor art. The 122-concept canonical catalog's
trait art was authored with a session built-in `"image_gen"` tool. That tool
is not present in this session, no local generator is installed on DATA, and
installing one is outside the mission's machine-safety contract. Therefore
the two mechanically-feasible traits cannot clear the mission's
feature-and-icon commit gate in this session. The icon designs above are
briefed and ready; only the painting step is missing.

## Explicit no-change confirmation

No blueprint, registration, localization, patch, catalog entry or asset for
Fiery Glare, Stoic Dignity or Earthsense was created, modified or published by
this mission. `blueprints/blueprints.json`, the icon catalog, and all trait
frameworks are untouched by these three slices.
