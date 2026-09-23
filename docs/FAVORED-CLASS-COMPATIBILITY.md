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
Gunslinger class is present at the host scan. Runtime confirmation is pending.

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
