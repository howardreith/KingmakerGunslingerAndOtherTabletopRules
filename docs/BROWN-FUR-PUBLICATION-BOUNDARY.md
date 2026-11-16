# The Brown-Fur publication check: a transaction judged against a later moment (INT-02)

Status: **development change on a review branch, awaiting review. Not merged, not released.** Branch
`codex/int02-brown-fur-publication-attribution`, stacked on draft PR #174 at its accepted head `e2a716c59e` (which is
stacked on #173, #172, #171, #170 and #169). The published v0.0.191 does not contain it. Nothing here is a release
qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: story S3 T2, inventory record INT-02.
Current status is kept in `planning/GUNSLINGER-REMEDIATION-STATUS.md`. It was traced on its own evidence; nothing
was carried over from INT-01 but the configuration that reproduces it.

## Disposition

**Observer defect, corrected. Publication is right and behaves as before.**

`feature-module-brown-fur-publication-gate` fails whenever the Favored Class mod is installed beside Call of the
Wild. One predicate of its ten fails. The check required the publication transaction's evidence to say that it had
found as many foreign archetypes as the Arcanist selector holds *now* (`before=14;after=15`). The transaction found
six and left seven, and its evidence says so (`before=6;after=7`). Eight more archetypes are added afterwards by
Favored Class. The transaction and the final selector are two different moments, and the check compared them as
one.

The archetype is published once, at the right place, and every archetype that was there is still there in order.

## The failing predicate

On the published v0.0.191 and on PR #174's candidate, beside the lab's twelve mods, the check's own observation is
the same character for character:

```
contract:True/identities:25/published:True/selectorRef:1/selectorGuid:1/foreign:14/unique:True/index:6/
firstCombined:7/knownCombined:5/transaction:surface=brown-fur-registered-identities;action=applied|
surface=cotw-arcanist-archetypes;action=published;before=6;after=7;boundary=False;index=6|
transaction;action=committed;surfaces=2
```

Each predicate of the old check against those values:

| Predicate | Required | Observed | Result |
|---|---|---|---|
| Provider contract compatible | true | `contract:True` | holds |
| Dependency available, status published | published | `published:True` | holds |
| Identity set built | 25 | `identities:25` | holds |
| Archetype in the selector by reference | 1 | `selectorRef:1` | holds |
| Archetype in the selector by identity | 1 | `selectorGuid:1` | holds |
| Foreign archetypes | at least 6 | `foreign:14` | holds |
| No empty or repeated entry | true | `unique:True` | holds |
| Ahead of the first known combined archetype | index below 7 | `index:6` | holds |
| Transaction committed | present | `transaction;action=committed` | holds |
| Transaction counts | `before=14;after=15` | `before=6;after=7` | **fails** |

The last row's requirement was built from the row above it marked `foreign:14`: "before" was set to the number of
foreign archetypes in the final selector and "after" to that plus one.

## The two moments

**When the transaction ran.** The module's log gives the checkpoint:
`[brown-fur][publication.complete] checkpoint=package-blueprint-library-ready;...before=6;after=7;boundary=False;index=6`.
That checkpoint is inside this package's own blueprint-library step. The selector held Call of the Wild's six
standalone Arcanist archetypes and no combined archetype, so Brown-Fur was appended at position 6.

**The selector after every mod has finished**, listed by the loader harness, which does not use the product:

| Position | Identity | Internal name | Shown as | Stage |
|---|---|---|---|---|
| 0 | `6ed134267c2f45debc77050d52581d01` | `SchoolSavantArchetype` | School Savant | found by the transaction |
| 1 | `712cbe46ce0f49dfb80aa0bb55fe6dc8` | `BloodArcanistArchetype` | Blood Arcanist | found by the transaction |
| 2 | `6ba8dccb41c245b59283097297b53f6d` | `UnletteredArcanistArchetype` | Unlettered Arcanist | found by the transaction |
| 3 | `3dbd81a35cf74aefb4dba140a0a22079` | `OccultistArchetype` | Occultist | found by the transaction |
| 4 | `efb2b9178a884d55a1e3ce19dadcbfa0` | `CollegiateArcanistArchetype` | Magaambyan Initiate | found by the transaction |
| 5 | `dfc6a2228a3247829e9d6dcb4d4cdc9f` | `SpellSpecialistArcanistArchetype` | Spell Specialist | found by the transaction |
| 6 | `aafa6e62241bb14582de5f587c179329` | `KMG_BrownFurTransmuter_Archetype` | Brown-Fur Transmuter | Brown-Fur, inserted by the transaction |
| 7 | `0579e8ed3ded006b2ef40c7fc5ed226c` | `SchoolSavantArchetypeUnletteredArcanistArchetype` | School Savant / Unlettered Arcanist | added later; known combined |
| 8 | `1a84628d8fcd0c6a2a89a9c9c24b52a5` | `BloodArcanistArchetypeUnletteredArcanistArchetype` | Blood Arcanist / Unlettered Arcanist | added later; known combined |
| 9 | `56155d681d350f5a2658a83237171f14` | `UnletteredArcanistArchetypeOccultistArchetype` | Unlettered Arcanist / Occultist | added later; known combined |
| 10 | `841a65dccb4a08e03360c76b4d6980cd` | `UnletteredArcanistArchetypeCollegiateArcanistArchetype` | Unlettered Arcanist / Magaambyan Initiate | added later; known combined |
| 11 | `b46e7ee9cbf002370c1e64b9daf9e3f2` | `UnletteredArcanistArchetypeSpellSpecialistArcanistArchetype` | Unlettered Arcanist / Spell Specialist | added later; known combined |
| 12 | `1e94108befebb3728ec03be1a6ee70db` | `UnletteredArcanistArchetypeSpellSpecialistArcanistArchetypeKMG_BrownFurTransmuter_Archetype` | Unlettered Arcanist / Spell Specialist / Brown-Fur Transmuter | added later; built from Brown-Fur |
| 13 | `c152b2a965d9f4f0105d562aeba2ac44` | `UnletteredArcanistArchetypeKMG_BrownFurTransmuter_Archetype` | Unlettered Arcanist / Brown-Fur Transmuter | added later; built from Brown-Fur |
| 14 | `753ccc40ae29f6c71c433293315b4fb6` | `SpellSpecialistArcanistArchetypeKMG_BrownFurTransmuter_Archetype` | Spell Specialist / Brown-Fur Transmuter | added later; built from Brown-Fur |

Positions 7 to 14 are added after the transaction. They are the Favored Class mod's combined archetypes
(Favored Class 1.3.1, `ZFavoredClass`): the five at 7 to 11 are in that mod's own identity table
(`blueprints.txt`) and are the five this project already names as "known combined"; the three at 12 to 14 are in
the list that mod writes of what it created (`loaded_blueprints.txt` in the lab's snapshot of the owner's mod
folder, dated 2026-11-12). That attribution is from those files and from the mod's code naming a
`createCombinedArchetype` routine; the mod's behaviour was not otherwise examined.

So the foreign count the transaction saw (6) and the foreign count now (14) differ by exactly the eight later
additions, and the hypothesis is confirmed as stated: the expected transaction-time counts were derived from the
final selector.

## How the expectation became stale

| Date | Change | Effect |
|---|---|---|
| 2026-08-23 | `01d7932ab2`: the check's fixed `before=6;after=7` and `foreign == 6` became `before=<final foreign>;after=<final foreign + 1>` and `foreign >= 6`, "to allow later foreign archetype publication" | Correct while every foreign archetype is already there when the module publishes |
| 2026-08-28 | Recorded live matrix: all twenty Brown-Fur gates pass; Brown-Fur at 6, first combined at 7 | Consistent with the combined archetypes being present when the module published (a boundary found, eleven before, twelve after) |
| 2026-10-17 | `3aeb77f9fa`: registration and publication moved to the package's own blueprint lifecycle boundary, to repair a startup-order defect | The module now publishes before Favored Class adds its archetypes, so the final count is no longer what the transaction saw |
| 2026-11-13 | The gate is recorded failing on the owner's installation and on a matched v0.0.185 baseline, with `foreign:14` | The same values as here |

The August run's transaction evidence was not re-read; that row is what its recorded positions and its pass imply.

## One thing found on the way, not classified (NEW-10)

Because the module now publishes before Favored Class combines archetypes, Favored Class builds three combined
archetypes *from Brown-Fur*: "Unlettered Arcanist / Spell Specialist / Brown-Fur Transmuter", "Unlettered Arcanist /
Brown-Fur Transmuter" and "Spell Specialist / Brown-Fur Transmuter". They are that mod's blueprints, with identities
this project does not declare. They were in the owner's installation on 2026-11-12 by that mod's own list. Whether
they work in play, and whether the project wants them offered, was not examined. Recorded as NEW-10; nothing was
changed for it. The check lists them as later additions and does not call them the project's.

## What changed

- **`BrownFur/BrownFurPublicationAudit.cs`** (new, no engine types) judges one reading as named results. The
  aggregate assertion keeps its name and passes only when every named result does.
- **Transaction-time evidence.** `BrownFurPublicationTransaction` now also keeps, for each list surface it applies
  to, the identities it found and the identities it left, with the position and whether a boundary was found. The
  coordinator keeps the selector, by identity, as it stood when the module first reached its decision to publish or
  not. The transaction's behaviour, its rollback and its evidence text are unchanged; these are records only.
- **The transaction is judged against its own record**: committed; one record of the archetype surface; the
  archetype not already there; the selector after equal to the selector before with the archetype at the recorded
  position; that position following the ordering rule for what the transaction found; the evidence text agreeing
  with the record; the reconciled selector equal to what the transaction found. Nothing in it comes from the final
  selector.
- **The final selector is judged for what must still hold**: the archetype once by reference and by identity; no
  empty, unreadable or repeated entry; the archetype ahead of every known combined archetype; and every archetype
  found at reconciliation still present, in the same order, by identity. The count floor of six is kept beside
  that.
- **Later additions** are listed by position, identity and name, and marked where they are known combined
  archetypes. They are not constrained and not attributed.
- **Module off and provider absent** are judged by the same code: with the module off, no transaction, no
  archetype in the selector, and preservation against the reconciled selector; with Call of the Wild absent, the
  existing absent-provider contract, now also as named results.
- **A ledger**, `brown-fur-publication-ledger.json`, holds every result, the final selector with each entry's
  stage, and the transaction's record.

Two constraints are new, and both passed everywhere they were run: the archetype may not be in any other class's
selector (before, that was required only when Call of the Wild was absent), and with the provider present the
25 identities are counted in the library as well as in the module's own set.

The named results, each an assertion `feature-module-brown-fur-publication-<name>`:

| Name | Judges | Moment |
|---|---|---|
| `provider` | Call of the Wild present and its contract compatible, or absent with nothing resolved | now |
| `module-state` | The module's published status against the requested state | now |
| `identities` | The 25 identities registered (none when the provider is absent) | now |
| `selector-occurrences` | The archetype once, or nowhere, by reference and identity, and in no other class | final selector |
| `selector-integrity` | No empty, unreadable or repeated entry | final selector |
| `ordering` | Ahead of every known combined archetype, or absent | final selector |
| `transaction` | The transaction's own record | when it ran |
| `foreign-preservation` | What was found at reconciliation is still there in order; later additions listed | both, by identity |

## Focused tests

Gate filter `brown-fur;summoning-publication-boundary;feature-modules.publication-plans`: production compiled,
100 of 3,065 selected, 0 failures. Eight tests are new. One existing source-pinned test,
`brown-fur.runtime-resolver-contract`, was updated to follow the observer: it named the removed count-derived
locals, and now names the audit call in their place.

The new tests run the real transaction composed as the coordinator composes it, the selector reader and the audit,
on the identities the game held.

| Test (`brown-fur-publication-boundary.` +) | What it shows |
|---|---|
| `published-then-later-foreign-additions-pass-and-are-listed` | Publication into six, then eight later additions: passes; the transaction result reports six to seven; the eight are listed in order; the same with nothing added; the same with the combined archetypes there first (eleven to twelve, boundary found) |
| `transaction-is-judged-by-its-own-record` | No transaction evidence, no commit, evidence text disagreeing with the record, a record that lost an entry, a transaction that published nothing, a reconciled selector that differs or is missing, and a position the ordering rule does not give: each fails the transaction result and only it |
| `lost-or-reordered-foreign-archetypes-fail-by-identity` | One archetype replaced by another at the same count, two exchanged, and fewer than six: each fails preservation, with the lost identity named |
| `occurrences-duplicates-and-unreadable-entries-are-named` | The archetype twice; a second object with its identity; the archetype in another class; an empty and an unreadable position, named with their place while everything else is still judged; no selector at all |
| `ordering-against-known-combined-archetypes` | A known combined archetype put ahead of Brown-Fur later fails ordering alone, while the transaction still passes |
| `module-off-contract` | A clean module-off configuration passes with later additions listed; publication with the module requested off, the archetype present with no transaction, a requested publication that did not happen, and a lost foreign archetype each fail |
| `absent-provider-contract` | A clean absent-provider configuration passes; identities, a published status, a selector reference, a transaction, a blocked dependency or a resolved contract each fail their own result |
| `transaction-records-identities` | The transaction's own record for an append, an insertion ahead of a boundary, a repeated commit and an unchanged surface |

Eight deliberate faults put into the audit were each caught: preservation by count only, ignoring the
transaction's problems, occurrences by reference only, any final position accepted, a transaction allowed with the
module off, unreadable or repeated entries allowed, the module state not compared, and identities allowed without
the provider.

What the tests establish and what needs the game: the tests establish how each reading is judged, and that the
transaction records what it found. They do not establish what the game's selector holds, when the module publishes
relative to other mods, or that the coordinator's composition in the game matches the one in the tests. Those are
the native results below.

## Native results

Sessions in the disposable game copy (Unity Mod Manager 0.33.0). The baselines are packages that already existed;
nothing was rebuilt for them. The candidate is one fresh build of this branch.

| Product | Configuration | Session | Brown-Fur check | Summoning check | Observation |
|---|---|---|---|---|---|
| published v0.0.191 | twelve-mod stack, module on | `20261116T181751Z` | FAIL: transaction counts | FAIL (`unclassified=38`, INT-01) | 45 of 47 |
| PR #174 candidate `4856b7084b5c` | twelve-mod stack, module on | `20261116T184404Z` | FAIL: transaction counts | PASS | 51 of 52 |
| PR #174 candidate `4856b7084b5c` | twelve-mod stack, module on | `20261116T191607Z` | harness listing of the final selector only | not observed | read-only |
| candidate `7eba68b822a9` | twelve-mod stack, module on | `20261116T193512Z` | PASS, all eight named results | PASS | 61 of 61 |
| candidate `7eba68b822a9` | twelve-mod stack, Brown-Fur off | `20261116T193512Z` | PASS, all eight named results | PASS | 61 of 61 |
| candidate `7eba68b822a9` | no other mod (Call of the Wild absent) | `20261116T193913Z` | PASS, all five applicable results | PASS | 58 of 58 |

The first two rows were recorded for INT-01 and are reused as the baseline's failing condition: the same failing
value on both, quoted under "The failing predicate". The third is a new read-only listing on the same baseline
package.

**The same setup, baseline against candidate.** Twelve-mod stack, every module on. The baseline fails one
predicate; the candidate passes with the transaction's own values unchanged:

| Named result | Candidate's observation |
|---|---|
| `provider` | `provider=present;availability=Available;contractCompatible=True` |
| `module-state` | `requested=enabled;active=enabled;statusPublished=True` |
| `identities` | `registered=25` |
| `selector-occurrences` | `byReference=1;byIdentity=1;inAnyClass=1` |
| `selector-integrity` | `entries=15;unreadable=0;duplicates=0` |
| `ordering` | `brownFurIndex=6;firstKnownCombined=7;knownCombined=5` |
| `transaction` | `evidence=3;surfaceRecords=1;action=published;before=6;after=7;boundary=False;index=6` |
| `foreign-preservation` | `atReconciliation=6;stillPresent=6;missing=0;sameOrder=True;foreignNow=14;laterAdditions=8`, then the eight by position, identity and name |

The aggregate's own observation is unchanged from the baseline's, character for character in its Brown-Fur part
(`foreign:14`, `index:6`, `before=6;after=7`). What changed is how it is judged.

**Identity evidence.** The candidate's ledger gives the transaction's record as six identities before and seven
after, with Brown-Fur at position 6 and no boundary found; the selector recorded at reconciliation equals the six
the transaction found; and those six are the first six of the final selector. The final selector in the ledger
equals the harness listing position by position, and the harness listing on the candidate equals the harness
listing on the baseline package: fifteen entries, the same identities in the same order. So the change did not
alter what is published.

**Module off, same twelve mods.** Brown-Fur was switched off through the loader's own panel and read in a fresh
process:

| Named result | Candidate's observation |
|---|---|
| `module-state` | `requested=disabled;active=disabled;statusPublished=False` |
| `identities` | `registered=25` |
| `selector-occurrences` | `byReference=0;byIdentity=0;inAnyClass=0` |
| `ordering` | `brownFurIndex=-1;firstKnownCombined=6;knownCombined=5` |
| `transaction` | `evidence=0;surfaceRecords=0` |
| `foreign-preservation` | `atReconciliation=6;stillPresent=6;missing=0;sameOrder=True;foreignNow=11;laterAdditions=5` |

The selector then holds eleven entries: the six standalone archetypes and Favored Class's five. The three built
from Brown-Fur are not there, which is what shows they exist only because Brown-Fur was published first. The
ledger equals the harness listing here too.

**Call of the Wild absent.** `provider=absent;availability=Unavailable;contract=none`, `statusPublished=False`,
`registered=0`, `inAnyClass=0`, `evidence=0;surfaceRecords=0`: the absent-provider contract holds with the module
requested on.

**The summoning check in the same observations.** It passes in all three candidate observations with the INT-01
totals: `spiderRows=38` and `nativeVariants=8` on the twelve-mod stack, `spiderRows=0` with no other mod.

All sessions reported the owner's 95 save files and installation unchanged and the registry restored. No save was
loaded. No Kingmaker process was left running.

Validation took about 18 minutes: one read-only baseline listing (79 s), the focused gate four times while the
change settled (about 70 s in all, the first failing to compile and the second on a pinned test, as described
under "What was tried"), eight mutation runs (about 160 s), one candidate build (420 s) and two candidate sessions
(316 s). That is inside the 30-minute ceiling and a little over the 15-minute target.

### What was tried

Kept as it happened:

- The first gate run did not compile (a list type in the new runner file). Fixed before anything was built.
- The second gate run failed one existing test, `brown-fur.runtime-resolver-contract`, which requires the surface
  key `cotw-arcanist-archetypes` to be spelled out in the coordinator; a constant had been put in its place. The
  literal was restored and the audit keeps its own constant of the same value.
- No candidate was built from either state. One candidate was built, and its first sessions are the ones above.

## Identities

| Thing | Identity |
|---|---|
| Branch and base | `codex/int02-brown-fur-publication-attribution` on `e2a716c59e8c11fc40ace8e79183f6a95953e5f0` (PR #174's accepted head) |
| Candidate commit | `7eba68b822a9ab06b1b990effdcb520d5346011a` |
| Candidate package | `KingmakerGunslinger-0.0.191-development-int02-brown-fur-publication-7eba68b822a9.zip`, SHA-256 `e54bceb28a5e5286891bfac447ec5ce04ad98be4fb8e3fc3997a55851a55e1de` |
| Candidate DLL | SHA-256 `c165bb92f9996f42fc023b4bffcaf1e23ead59b1a5540df674504257a7d4166c` |
| Baseline, PR #174 candidate | commit `4856b7084b5c08d804f507a00fb7b981c69248e4`, package SHA-256 `97ac7308e4cd98c59ca587acb96d3524c7889b6b0dcfb5dc960e630914422fa0`, DLL `d5c0d98cf647715c5dc2ed3fbcaeee4f724d5828fad95f67f604f45b9a0faf88` |
| Baseline, published v0.0.191 | package SHA-256 `c09ca6255feefe6c547d051ab3cf3ead8bc918402d7488f077efa0ac5d2e7fbb`, DLL `7c3b0324f40e32b0a476c24e9e66a63a2ecefd8908dc3902bed8ec81a5d87287` |
| Harness, sessions `20261116T191607Z`, `20261116T193512Z`, `20261116T193913Z` | `KmgLoaderCompatHarness.dll`, SHA-256 `41e4300c58a60d0e6d93a9b81a710daba8492c11b47470260c76abd382321425`, built from `tools/LoaderCompatHarness` at `9ff2220464` (adds `kmgArchetypeCensus`) |
| Harness, the two INT-01 sessions reused | SHA-256 `56e8dd08844460582e9d2dde8646e582a437efd7703db887497aa349b593c5c0` |
| Twelve-mod stack | BagOfTricks 1.16.4, BetterVendors 2.0.8, CallOfTheWild 1.14.4c-2.1, CheatMenu 1.2.3, CraftMagicItems 2.1.0, EddicKingmakerRespec 1.0, KingmakerBugfixes 1.3.3, ProperFlanking2 1.2.3-2.1, RacesUnleashed 1.0.11, SkipIntro 1.3.1, TweakOrTreat 1.1.0, ZFavoredClass 1.3.1 |
| Call of the Wild | DLL SHA-256 `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` |
| Favored Class (`ZFavoredClass`) | DLL SHA-256 `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c` |
| Module settings | every module on; for the disabled run Brown-Fur Transmuter off and the rest on |
| Session `brown-fur-publication-boundary.json` | SHA-256 `ccc5325277e4af9512f933cfec23ad7d4fdaccc2b45e400cd9ba495311966e5a` |
| Session `brown-fur-publication-boundary-absent-provider.json` | SHA-256 `0ff8dbf7d0c00da376d0ff9f8a8ebf8fc11239da751589f4bf8f6d0d315f5a12` |
| Game | Pathfinder: Kingmaker 2.1.7b, disposable copy `game-copies/loader-compat`, loader profile `owner-0.33.0` |

## Limits and what is not established

- The owner's installation was not used, and the original report's eleven-mod installation was not reproduced. The
  twelve-mod stack is the lab's snapshot. The original report's Brown-Fur value is the same as the one reproduced.
- v0.0.185, the original baseline, was not rebuilt. The baselines here are the published v0.0.191 and PR #174's
  candidate.
- Which mod adds the eight later archetypes is attributed from Favored Class's own files and code strings, not by
  running with and without it.
- Only the order observed was run: this module publishes, then Favored Class adds. The other order (combined
  archetypes present first) is covered by tests and by the August record, not by a run here.
- No failing Brown-Fur boundary was produced in the game; those reports are covered by the tests.
- Brown-Fur was not selected or played in any run. This check is about the archetype in the selector.
- NEW-10, the three archetypes Favored Class builds from Brown-Fur, is observed and not classified.
- `observe-brown-fur-cotw-contract`, which has its own two publication assertions, was not changed and not run.
- The complete domain suite, the compatibility matrix and the other module states were not run.
