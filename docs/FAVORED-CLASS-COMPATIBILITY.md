# Favored Class compatibility contract

This document records the exact host contract the optional Favored Class
integration targets. It is evidence about installed binaries and their
decompiled code, not a claim that the integration works in game. Runtime
confirmation is recorded separately in the mission evidence index.

## Qualified profile

Only this exact binary profile is supported. A matching version string alone
never qualifies another binary.

| Component | UMM id / version | SHA-256 | MVID |
| --- | --- | --- | --- |
| Favored Class | `ZFavoredClass` 1.3.1 | `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c` | `3efd38e7-8682-4b4d-8d53-e368a3664919` |
| Call of the Wild (host dependency) | `CallOfTheWild` 1.14.4c-2.1 | `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` | `8caab254-aacf-4811-8093-44b9184e6e53` |
| Game | Pathfinder: Kingmaker 2.1.7b `Assembly-CSharp.dll` | `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` | `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7` |

Races Unleashed 1.0.11 (`6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0`,
MVID `e9b9acb5-9b3f-41ad-bbd7-74494d5d7680`) is the optional race provider of the
installed profile. The public Favored Class source at
`56ec6c5fd34f0da037350f951383ca7f1a0c5e57` (README version 1.3.2b) is intent
evidence only; the installed 1.3.1 binary is the authority.

Eldritch Arcana's favored-class system is a different implementation that the
host README calls incompatible; it is never adapted through this contract.

## Host initialization (installed binary)

- `ZFavoredClass.Main` installs a Harmony postfix on
  `LibraryScriptableObject.LoadDictionary` marked
  `[HarmonyAfter("RacesUnleashed")]`. The postfix assigns `Main.library`
  first, then runs `Core.load()`, the optional deity pass,
  `AlternativeRacialTraits.load()`, `Traits.load(enable_traits)`,
  `StackingArchetypes.load()` and a `loaded_blueprints.txt` dump, all inside
  one `try`/`catch` that only logs. `Main.library != null` is therefore not
  evidence that registration finished.
- `Core.load()` scans `library.Root.Progression.CharacterClasses` once
  (excluding Eldritch Scion `f5b8c63b141b2f44cbb8c2d7579c34f5`). For every
  class it creates:
  - progression `FavoredClass<name>Progression`, GUID
    `MergeIds("602ea6032c324258a183588f84522ea1", classGuid)`, with twenty
    level entries (ten for prestige classes);
  - bonus selection `FavoredClass<name>FeatureSelecion` (host spelling),
    GUID `MergeIds("f431abc7ab7b4771a58fff7ee2af8a01", classGuid)`,
    `HideInUI` and `HideInCharacterSheetAndLevelUp`;
  - private static map entries in `class_guid_progression_map` and
    `class_guid_bonus_selection_map` keyed by class GUID.
- A class appended to `CharacterClasses` after the scan is never discovered.
  Calling `Core.load()` again would repeat global initialization and is never
  done.
- `Core.load()` ends with `loadCustomFavoredClassBonuses()` and then
  `createPrestigiousSpellcaster()`. Each custom JSON file is parsed **outside**
  the per-file `try`, so a malformed file aborts the remainder of
  `Core.load()`. `Core.prestigious_spellcaster` is assigned only after the
  custom pass, making it a completion marker for `Core.load()`.

The installed host dump of 2026-09-23 lists
`FavoredClassKMG_Gunslinger_ClassProgression`
(`cbe4e1941a5f0fa8229919016a6b283b`) and
`FavoredClassKMG_Gunslinger_ClassFeatureSelecion`
(`5ffbec509d160a812695bef00c968c9b`): under the current bootstrap the KMG
Gunslinger class is present at the host scan (confirmed natively; see below).

## Registration and fractional contract

`Core.addFavoredClassBonus(feature, partial, classes, divisor, races)` is
public on an internal class; it is an inspected implementation contract, not
an extension SDK. For `divisor > 1` it creates (or reuses) a partial feature,
adds Call of the Wild's `PrerequisiteFeatureFullRank` to both leaves, sets the
partial capacity to `feature.Ranks * (divisor - 1)`, adds one
`ZFavoredClass.NewMechanics.PrerequisiteRace` (`Group = Any`) per non-null race
to each leaf, and appends both leaves to every listed class's bonus selection.
It appends nothing when every listed race is null, and registers an
**unrestricted** reward when the race array is empty.

`PrerequisiteFeatureFullRank.Check` returns false when either leaf is already
selected in the current selection's children; otherwise it computes
`(partialRank + fullRank + 1) % divisor == 0` and inverts the result for the
partial leaf. After N investments the full rank is `floor(N/d)` and the
partial rank `N - floor(N/d)`: **N = fullRank + partialRank**. The host's
partial capacity stops an uncapped divisor-six reward at eighteen
investments; KMG sizes its own leaves from `T = min(20, d * cap)` instead
(`FavoredClassRankPolicy`).

`PrerequisiteRace.Check` is exact equality: `race == unit.Progression.Race`.
Kingmaker's `BlueprintFeature.MeetsPrerequisites` ANDs every `All`
prerequisite and ORs every `Any` prerequisite, then requires both.

## Generic and human rewards

- Generic rewards for every scanned class: `FavoredClassBonusHitPointFeature`
  (`AddHitPointOnce`, divisor 1, 20 ranks) and
  `FavoredClassBonusSkillRankFeature` (`AddSkillRankOnce`, divisor 2: one
  skill rank per two selections, 10 ranks). The host also installs its own
  automatic-HP handling; KMG adds no second HP patch.
- The twenty class families with a built-in human route are Alchemist,
  Arcanist (normal and Unlettered), Bard, Bloodrager, Inquisitor (normal and
  Ravener Hunter), Investigator (normal, Questioner, Jinyiwei, Psychic
  Detective), Kineticist (wild talent and Metahealer), Magus (arcane and
  eldritch pools/arcana), Monk (ki), Occultist, Oracle, Psychic, Rogue (talent
  and Ninja trick), Shaman, Skald, Slayer, Sorcerer, Warpriest, Witch (normal
  and Winter Witch) and Wizard. Their human routes list human, half-elf,
  half-orc, Aasimar and Tiefling explicitly.
- The host ships nine custom JSON rewards in `ZFavoredClass/Custom/`. One
  (`bonus_panache.json`, class `b1c7989ee7f04af8b11309f02b34cb4a`) has a human
  route. Custom content is preserved and never written; it is not bridged to
  Mostly Human without a separate contract review.

## Suspected host defect (verification pending)

In the installed Dwarf Summoner reward, the pet-side
`EidolonACFavoredClassFeature` configures `MasterFeatureRank` against itself,
while every sibling companion reward (saves, DR/magic, DR/evil) configures it
against the master-owned wrapper returned by `createAddFeatToAnimalCompanion`.
If `MasterFeatureRank` reads the master's rank of the named feature, the
Dwarf reward contributes nothing. KMG's Oread Summoner reward (O08) does not
copy that component graph either way: it reads the master's own full-leaf
rank, and its owner/rank relationship is tested directly.

## Runtime confirmation (local candidate)

The guarded contract scenario confirms this profile natively on every
candidate. Checks: exact host SHA-256, MVID and six IL fingerprints; the
host's completion marker; one Gunslinger progression
(`cbe4e1941a5f0fa8229919016a6b283b`) and bonus selection
(`5ffbec509d160a812695bef00c968c9b`) with twenty levels; unchanged generic
HP/skill rewards; and KMG leaves appended as a suffix after the untouched
foreign entries of every class selection they use. The final-candidate run is
recorded in `FAVORED-CLASS-IMPLEMENTATION-REPORT.md`.

## Optional provider content read by KMG counters

KMG never calls host or Call of the Wild code and never writes their folders.
It reads their blueprints by exact identity only after they exist, at the
first Unity Mod Manager update after the host initialized:

| Counter | Provider blueprints | How they are read |
| --- | --- | --- |
| I06/S04 revelations | Call of the Wild Oracle `32c02466b2364c8a906e6e4761175099`, Demon Hunter archetype `3b3b5950e8264819b69d9aaeffe179da`, the 52 manifest revelations | Bounded walk of each revelation's live graph at publication; the Oracle engine's class-level rank configs, its ability-parameter calculator (by type name and Oracle class) and oracle-level resources |
| O08 eidolon armor | Call of the Wild eidolon class `e3b3ad6decb14cdba2e7e14982d90035` | Identity of the current pet's class |
| O01 performances | Song of Fiery Gaze, Satire, Glorious Epic, Scandal and Dance of the Dead (five published provider targets; Mockery is registered but not published) | Feature, toggle and area identities |
| Summoner and Oracle bonus selections | the host's scan of Call of the Wild classes | `host.BonusSelectionFor(class)`; a class the host did not scan publishes nothing |

The rank read point is a KMG postfix on `ContextRankConfig.GetBaseValue` that
runs after Call of the Wild's own postfix on the same method
(`[HarmonyAfter("CallOfTheWild")]`, `Priority.Last`). Call of the Wild's
postfix assigns the base value for `SummClassLevelWithArchetype` configs; a
KMG change made before it would be overwritten. KMG adds the steps only for
configs scoped to a revelation the caster invested in, keyed by the config
instance and the context's own blueprint. Rank bonus (`AddBonusCasterLevel`)
is deliberately not used, because Call of the Wild counts it again in every
Oracle-engine rank.

O01 is read inside each performance area's own view and never in a shared
blueprint: an `AreaEffectView.InitAtRuntime` postfix widens that instance's
cylinder and scales that instance's ring, an `AreaEffectView.SpawnFxs` postfix
scales a ring the native attach spawns later, a `GameObjectsPool.Release`
prefix restores the exact ring scales before a pooled effect is reused, and
`Fact.SelectUIData` and `MechanicActionBarSlotActivableAbility.GetDescription`
postfixes show the owner's range in the owner's own descriptions, following
the recorded widening outcome of that owner's live areas: native while one
failed or waits for its ring, and after a failure until a later success
(`docs/FAVORED-CLASS-TARGET-MANIFEST.md`).

Two scopes are closed in finally blocks rather than by postfixes alone, since
Harmony 1.2 has no finalizers. A transpiler on
`LevelUpController.ApplyLevelup` replaces only the replay's
`ILevelUpAction.Check` and `ILevelUpAction.Apply` calls with helpers that make
the same calls inside a scope of that controller (the same-level owned-target
check). A transpiler on `ActionList.Run` replaces only its per-action
`GameAction.RunAction()` call with a helper that makes the same call and
restores the demoralize scope depth in a finally block; the native try/catch,
logging and order are unchanged. The `Demoralize.RunAction` prefix (first,
before Call of the Wild's replacing prefix) opens the I07 frame and its
postfix restores the depth it opened at, so a nested demoralize keeps the
outer frame and a demoralize that throws never reaches the next action. Each
transpiler changes nothing unless its call sites are found exactly once; I07
is withheld without the `ActionList.Run` envelope, and without the replay
scope a same-level target counts from the next level-up.

## Settings file

`Mods\KingmakerGunslinger\FavoredClassIntegration.json` is optional, read once
per process and never written by KMG. It uses schema 1 with exactly five
Boolean keys: `integration`, `firstParty`, `adaptations`, `thirdParty` and
`mostlyHuman`. Absent or invalid, it gives the charter defaults (everything
ON except `thirdParty`), and an invalid file is reported in the log. Changes
need a restart.

## Lifecycle, migration and recovery

- **Identities always resolve.** Every KMG favored-class leaf and helper
  registers on every load, whether or not the host is present and whatever
  the settings say. Saved investments therefore load even while the host is
  missing or the integration is disabled.
- **Integration OFF** (`integration: false`): no choices are offered and every
  owned numerical effect is suppressed. Nothing is refunded or removed.
  Turning it back ON restores the effects.
- **A profile OFF** (`thirdParty`, `adaptations`, `firstParty`): no new
  choices through those routes. Choices a character already earned keep
  working.
- **Mostly Human OFF**: the choice is no longer offered to new characters. A
  character who has the trait keeps it and its identity.
- **Respec (native `Player.RespecCompanion`)**: a committed respec removes
  every favored-class counter once; the rebuilt character earns new ones
  normally. The Mostly Human identity is a class feature granted only by the
  trait, so it never survives a respec to the standard ancestry on its own. A
  companion whose master is respecced is destroyed natively and keeps no
  projected armor; a new companion receives the projection once. A cancelled
  respec changes nothing.
- **Companion and eidolon armor (O07/O08)**: the projected armor follows one
  qualified pet, the master's current pet of the counter's pet class. A native
  unlink or dismissal removes it from that pet, a relinked, replacement or
  resummoned pet receives it exactly once, a pet that no longer qualifies loses
  it, and a pet handed to another master who holds the same counter keeps it
  under that master.
- **Host removed, disabled, unsupported or not published**: no choices are
  offered and every owned numerical effect is zero. Owned effects need both
  the enabled integration and a committed publication of the exact qualified
  host; the host activation starts off and is cleared for an absent, disabled,
  unsupported or incompletely initialized host, a failed publication and any
  rollback. Saved ranks still resolve and nothing is refunded or removed, so
  the effects return once the exact host is published again. The Mostly Human
  racial identity and its bridge do not depend on it. The host's own
  favored-class progression is host content: restore the host rather than
  editing a save.
- **Call of the Wild removed**: the Oracle and Summoner counters are not
  offered, and their saved leaves still resolve. Revelation and eidolon effects
  are inert because their provider blueprints are gone.
- **Leaving the favored-class integration entirely**: back up saves first.
  Removing KMG leaves every KMG identity unresolved; restore the mod rather
  than editing the save.
- **Unsupported and not claimed**: other favored-class systems (Eldritch
  Arcana), a changed host or provider binary, level caps above 20, gestalt,
  custom respec tools and other race providers' "counts as human" features.
