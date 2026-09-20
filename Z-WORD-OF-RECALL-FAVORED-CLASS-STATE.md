# Word of Recall Oracle Favored-Class Selection — Mission State

Branch: `codex/z-word-of-recall-favored-class`
Base: master `525625df` (release 0.0.132)

## Owner request

An Aasimar Oracle can earn additional spells known through the Favored
Class mod, but Word of Recall is missing from the sixth-level favored-class
bonus spell choices. Make the genuine Favored Class bonus-spell mechanism
offer and teach canonical Word of Recall (Aasimar and shared Human routes).

## Baseline recorded (2026-09-19)

- Source: master `525625df`, version 0.0.132, clean tree.
- Installed live mod: KingmakerGunslinger 0.0.130 (deployed candidate will
  replace it through the guarded backup-first deployment).
- Call of the Wild 1.14.4c-2.1, Favored Class (ZFavoredClass) 1.3.1,
  Races Unleashed 1.0.11, Tweak or Treat 1.1.0, Proper Flanking 2, Craft
  Magic Items 2.1.0, Better Vendors 2.0.8, Bag of Tricks 1.16.4 installed.
- Owner FeatureModules.json hash (from 0.0.126 record):
  `a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`.

## Static analysis (installed-assembly IL, private dumps)

- ZFavoredClass 1.3.1 `Core.addExtraKnownSpellsFavoredClassBonus` ordinary
  Oracle route: `CreateExtraSpellSelection(oracle_class.Spellbook,
  oracle_class, 8, null, ...)`; the per-level clones of the vanilla
  `MysticTheurgeInquisitorLevelParametrized1`
  (`bcd757ac2aeef3c49b77e5af4e510956`) store
  `SpellList = custom_spell_list ?? spellbook.SpellList` on BOTH the
  BlueprintParametrizedFeature and its replaced LearnSpellParametrized.
- Verified installed identities (ZFavoredClass blueprints.txt):
  - `FavoredOracleOracleSpellListBonusSpellFeatureSelection`
    `9ba3858327354e2093613efb9de198d7` (8 per-level children)
  - `FavoredOracleOracleSpellList6ParametrizedFeature`
    `7249760f01784ea997afaa9c433c2e68` (level 6)
  - `PartialFavoredOracleOracleSpellListBonusSpellFeatureSelection`
    `b19759026d6508b9022f1edb4ec4b31f`
  - `FavoredClassOracleClassFeatureSelecion`
    `c6f18fa1194d0bfb35e1913983b8da98`
  - Ganzi variant uses a separate deliberately filtered list
    (`GanziOracleFCBSpellList` `8a68f3a502b14d9dad81090d8db64560`) and must
    remain untouched.
- Per-level feature contract: `PrerequisiteClassSpellLevel(oracle, level+1)`
  (level-6 choice needs Oracle class spell level 7), `SpecificSpellLevel`,
  `SpellLevelPenalty=0`, Ranks=1 on the clone, selection Ranks=10.
- Native extraction (desktop CharBSelectorLayer and console phase VMs both
  call `IFeatureSelection.ExtractSelectionItems(before, preview)`):
  ParameterType LearnSpell(4) → `ExtractItemsFromSpellList(preview)` reads
  `feature.SpellList.SpellsByLevel[level].SpellsFiltered`, skipping
  already-known spells. CotW's Prefix intercepts only types 1 and 20-23,
  so the genuine native list route runs for this feature.
- `SpellLevelList.get_SpellsFiltered` caches into `m_SpellsFiltered`
  (rebuild when null/empty; filter is `SpellIsNotLocked`, DLC-only).
- `BlueprintParametrizedFeature.GetFullSelectionItems` caches
  `m_CachedItems`, but the level-up pickers use the live extraction path.
- Grant path: `LearnSpellParametrized.OnFactActivate` →
  `DemandSpellbook(SpellcasterClass).AddKnown(SpellLevel=6, spell)` when
  `SpecificSpellLevel` (component SpellList unused in that branch).
- Conclusion so far: statically the shared references should surface the
  reconciled Word of Recall; the failure must be a runtime fact. BEFORE
  evidence run planned through `observe-word-of-recall-favored-class`.

## Diagnostic scenario (added on this branch)

`observe-word-of-recall-favored-class` — save-free, read-only, mod-load
readiness; resolves the genuine installed blueprints by exact identity and
records class list membership, feature/component list identity, structure,
prerequisite, native filtered/cached state, and the DLC filter outcome.

## Status

- [x] Baseline + static chain analysis
- [x] Diagnostic scenario implemented (source)
- [ ] BEFORE runtime evidence
- [ ] Root cause + repair
- [ ] Regression tests
- [ ] Acceptance runtime scenario (Aasimar/Human + controls)
- [ ] Validation/build/package
- [ ] Qualification runs + handoff
