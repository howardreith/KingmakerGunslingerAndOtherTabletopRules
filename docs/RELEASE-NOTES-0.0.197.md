# Tabletop Added Rules v0.0.197

This stable release repairs the developer's save-persistence check for the elemental races. It changes no gameplay:
no ability score, racial label, heritage, trait, feat or save behavior differs from v0.0.196. Every identity from
v0.0.196 is unchanged and none is appended, so existing saves keep their references. Unity Mod Manager 0.31.1 or
newer is still the minimum.

## Elemental race persistence check

The mod ships a developer test that saves every elemental race, heritage and sex, reloads them with the Elemental
Races module off and on, respecs them and checks that nothing is left behind. Since v0.0.120 one of its checks
failed even though every character it observed was correct.

- **What was wrong.** The check required one test pass to exercise all 21 published alternate racial traits.
  No single pass does: the ordinary pass exercises 19, the Nereid Fascination pass exercises 18, and Treacherous
  Earth is checked by its own test.
- **Repair.** Each pass now declares exactly which traits it must cover. Every other published trait must be
  assigned to a named test that covers it, so a newly published trait with no test still fails. The check still
  requires every character, every step and every observation to be exact.
- **Result.** On a development build of this source the whole save, reload, respec and cleanup sequence passed,
  including the final check that a fresh load of the cleaned save contains nothing left over. That final check had
  not run on v0.0.196.

## Existing saves

Nothing in this release changes, adds or removes any identity, and nothing that runs during ordinary play changes.
No save migration is needed.

## Disclosed open items

- **Tested with the developer's own mod set.** The checks ran on the developer's installation with its usual mods.
- **Nereid Fascination pass.** Its new coverage rules were checked by offline tests only; that pass was not rerun in
  the game.
- **Tested on a development build.** The checks before release ran on a development build of the reviewed source.
  After review, a diagnostic message in the test was corrected and checked offline only. The release build is checked
  in the game after publication, and that result is recorded in the repository's release record.
- **Earlier items.** The disclosures in the v0.0.196 notes still stand, unchanged.
