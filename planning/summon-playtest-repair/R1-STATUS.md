# Summoned Creature Manual Playtest Repair — R1 status

**R1 DEVELOPMENT CHECKPOINT / STOP.** The five owner cuts (CUT-01 to CUT-05) are
implemented on `codex/summon-playtest-r1-cuts`, covered by focused tests, built
into one development candidate and observed natively on DATA. This is not a
release, and nothing was installed permanently. R2 (combat entry and fear
evaluations) is the next sprint and has **not** started.

**R1 review follow-up (same day).** Review of draft PR #111 (head
`6256f16b39`) found no production defect in the five cuts, but two observer
gaps: the executed observer never read ProperFlanking2's real attack-replacement
buff, and the retained-after-fixer check reused a Lillend reading taken before
the fixers. The observer was repaired (observer-only commit `d46726d46d`), built
into a second development candidate and run once on DATA: **PASS 20/20**,
including the real buff observed active before reconciliation and absent after
it. No production behavior changed. The first candidate's FAIL and the reader
re-evaluation are kept and qualified below; no PASS moves between binaries.

Mission records adopted from the owner's planning package:
[MISSION](MISSION.md), [ISSUE-REGISTER](ISSUE-REGISTER.md),
[PRESERVATION](PRESERVATION.md), [SOURCES](SOURCES.md),
[EVIDENCE-INDEX](EVIDENCE-INDEX.md). The thirteen owner screenshots stay private
and local.

## Intake (DATA, 2026-11-07)

- Base: master `7ed6e247471d346cfd4b190844e3891639e16f24`, rechecked against
  `origin/master` before branching (unchanged; includes the v0.0.179 records).
- Installed Kingmaker Gunslinger: **v0.0.140**, DLL SHA-256
  `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`.
- Feature modules (installed `FeatureModules.json`): all thirteen enabled,
  including `expanded-summoning`.
- Installed mod folders: BagOfTricks 1.16.4, BetterVendors 2.0.8,
  CallOfTheWild 1.14.4c-2.1, CheatMenu 1.2.3, CraftMagicItems 2.1.0,
  EddicKingmakerRespec 1.0, KingmakerBuffPlanner 0.1.1-rc3,
  KingmakerBugfixes 1.3.3, KingmakerDiceRoller 0.1.6, KingmakerGunslinger
  0.0.140, KingmakerLastAzlantiPreserver 0.1.1, ProperFlanking2 1.2.3-2.1,
  RacesUnleashed 1.0.11, SkipIntro 1.3.1, TweakOrTreat 1.1.0,
  ZFavoredClass 1.3.1. (Folder presence; UMM enablement is not separately
  recorded here.)
- The binary behind the owner's screenshots is still not identified.

## Commits

| Commit | Content |
| --- | --- |
| `9677e01031` | Product: the five cuts, load fixer, policy, tests, roster ledger, R1 observer phase |
| `541e0d44f4` | Development-candidate validator: exempt only the owner-withdrawn Stirge (release validators unchanged) |
| `ab24a92394` | Sprint40 publisher self-test evaluates the frozen v0.0.178 shape (publisher and its pins unchanged) |
| `6256f16b39` | Observer toggle-identity constant correction (not rebuilt) and the first records; reviewed PR head |
| `d46726d46d` | Review follow-up, observer only: real Sunder chain and buff readings, bounded waits, live Lillend recheck after reconciliation |
| later | These follow-up records |

## Inventory: what changed and what is retained

| Cut | Removed from fresh summons (public / automatic) | Retained for compatibility | Older saved summon |
| --- | --- | --- | --- |
| CUT-01 Stirge | All nine SNA I-IX placements (`One`, `1d3`, `1d4+1` rows) leave the canonical Summon Nature's Ally I-IX parents, every alternate-source projection and the Druid conversion rows, through `SummonVisibilityCatalog.IsPublished`. Public surface 1188 -> 1179 logical placements, 1217 -> 1208 visible choices. | Unit `KMG.Summoning.Unit.Stirge` (`c49f994e…`), nine roots `KMG.Summoning.Ability.SNA.Tier1-9.Stirge.*`, every alternate adapter identity, StirgeTouch, CombatTraits, Hold, Remove Stirge, icons, mesh and albedo. | A summoned Stirge in a save keeps working; nothing is reconciled. |
| CUT-02 Wall of Ice | Bone Devil: `WallOfIce` grant, its spell-like parameter slot, `WallAi` in the brain. Ice Devil: `WallOfIce` grant, parameter slot, `WallOfIceAi` in the brain. | Both `WallOfIce` abilities, `WallAi`/`WallOfIceAi` (configured, not in brains), `WallSectionUnit`, `WallSectionUnitType`, `WallSectionTraits`, `WallSectionBrain`, `WallResidualArea`, and each devil's `InfernalWallSourceLifetime35` (`$KMG_S35_BoneWallSourceLifetime`; Ice Devil's ordinal-named component) with unchanged `WallAbility`/`WallSection`/`ResidualArea`/`FogArea`. No trait component was added or removed, so no saved component name moves. | Fixer removes the exact `WallOfIce` fact and Wall AI action. Pre-existing sections/residuals keep retiring through the unchanged section and source-lifetime components. |
| CUT-03 Magma Form | Magma Mephit's `SpellLikeTwo` (Magma Form) grant, `SpellLikeTwoResource` from CombatTraits, `SpellLikeTwoAi` from the brain. | Profile slot `SpellLikeTwo = "MagmaForm"`, the configured ability, resource, AI action and `MagmaFormState` buff. Breath, `SpellLikeOne` (Pyrotechnics), their resource and AI are unchanged. | Fixer removes the exact ability fact, AI action and resource. An already active Magma Form state expires on its own five-round duration. |
| CUT-04 Lillend Bardic Suggestion | `BardicSuggestion` grant on the unit, the named `$KMG.Lillend.BardicSuggestion.Parameters` component, the noncombat dispatcher's route (`BardicSuggestion = null`). | The `BardicSuggestion` ability and its `BindSuggestionProvider32` / Fascinate configuration; `ConfigureSuggestion32` (Satyr and Lillend Suggestion spell consumers) unchanged; Lillend spellbook, Courage, Competence, Fascinate. Null-guarded consumers (`LillendBardicMemory32`, `LillendFascinated32`) fall through to native behavior. | Fixer removes the exact `BardicSuggestion` fact. |
| CUT-05 Fire Giant Sunder | Native Improved Sunder (`9719015e…`) is no longer in the Fire Giant's feat list. Its own AddFacts grant the native `SunderAction` (`fa9bfb9f…`) and, with ProperFlanking2 installed, its default-on `SunderActionToggleAbility` (`d9604eade0f54d88a899b5052f98d536`), which while running applies `SunderActionToggleBuff` (`f9f7edc1148c453181460dccbbefa3dc`, the only carrier of AttackReplacementWithCombatManeuver). All of them go with the feat. (`a0808193…` `SunderArmorToggleAbility` is PF2's never-granted save-cleanup dummy; see the first run's failure below.) | The native feat and all native/ProperFlanking2 code untouched; Storm Giant's separate grant unchanged; Fire Giant's Great Cleave, the other feats, weapon and attacks unchanged. ProperFlanking2 not altered. | Fixer removes the exact feat from the exact KMG Fire Giant; native AddFacts deactivation removes the action and toggle, and the toggle's deactivation stops it and removes its buff (observed natively, induced state). |

> **Later owner decision (SUNDER-01, 2026-11-10).** No supported summon keeps Sunder: Storm Giant and the Greater
> and Elder Earth Elementals are now withdrawn too, with new fixer names. See
> [../summon-menu-order-and-sunder/STATUS.md](../summon-menu-order-and-sunder/STATUS.md). The R1 verdicts below,
> including the Storm Giant control, stay with their R1 candidates.

The load fixer is `SummonWithdrawnCapabilityFixer`, a native `FixUnitOnPostLoad`
component on each affected unit blueprint. Native `UnitDescriptor.PostLoad` runs
each named fixer once per unit and records its name (`m_AppliedFixers`). Names:
`$KMG_R1_WithdrawnCapabilities_<Unit>`; a later withdrawal needs a new name.
Decisions and the exact-owner/exact-identity selection live in the pure
`SummonPlaytestWithdrawalPolicy`.

Why reconciliation is needed (verified against the installed 2.1.7b
Assembly-CSharp): `UnitBrain.Actions` and `AvailableActions` are
`[JsonProperty]`, so a saved summon keeps its brain action list; granted facts
persist; blueprint `AddFacts` are applied only at creation. Fact component state
is rebound by component name on load, removed components are skipped, new ones
activated.

## Candidates

Two development candidates, same purpose, never releases. Product source is
identical between them; the second differs only in the R1 observer.

**Candidate 1 (original R1 run)**

| | |
| --- | --- |
| Purpose | `summon-playtest-r1` (development candidate; never a release) |
| Source | `ab24a92394eafa9dfeb900080514246a191fdecc` |
| Source fingerprint | `c2f106f94d6fb1e4250e7fd6132366d2b49991be427c785d1977de203d0dd993` |
| Package SHA-256 | `2f14eebcfe7341af08a2657b2874e83a101ca9e36fb8963e0879992f3845ff8c` |
| DLL SHA-256 / MVID | `27f94a69b586dd16ff8fd341e20c38d81398a014ef1e8be410ff52f59483b875` / `3d4b7896-818f-4b2d-a274-23fa3195320e` |
| Version metadata | `0.0.179` (unchanged released metadata; baseline tag `v0.0.179`) |
| Selected tests | 196 of 2,848 registered, 0 failures |

**Candidate 2 (review follow-up, corrected observer)**

| | |
| --- | --- |
| Purpose | `summon-playtest-r1` |
| Source | `d46726d46ddc83a03c683c693975bac6e17dd7a5` |
| Source fingerprint | `f64a8e9bfcc409924a24ac6299fb33a8f4df7bd6ada75bc01a95035a9ed126c2` |
| Package SHA-256 | `492a8f1226f7afffa39fe436b0c79011796a412325de0610741a53a60ae1980d` |
| DLL SHA-256 / MVID | `4cf8131f084a4ec6e2654258c10af0f99c863069d54bed7de280bfeef59cfdec` / `98eb3b05-4433-4add-a62f-0ce8d7f73af7` |
| Version metadata | `0.0.179` (baseline tag `v0.0.179`) |
| Selected tests | 6 of 2,848 registered (`summon-playtest-r1;expanded-summoning.sprint32-native-admission`), 0 failures |
| References | `-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`, passed explicitly; its `Assembly-CSharp`, `UniRx` and `UnityEngine.CoreModule` match DATA's installed game byte for byte |

Between the two candidates only `RuntimeTestRunner.SummonPlaytestR1.cs`
changed (`git diff ab24a92394 d46726d46d -- src` touches that file alone).

## Focused tests

New, filter `summon-playtest-r1`:

- `summon-playtest-r1.stirge-canonical-routes` — exactly nine withheld
  placements, all Stirge SNA I-IX; every unrelated placement published; 1188
  registered / 1179 published; unrelated SNA I-IX controls intact.
- `summon-playtest-r1.stirge-alternate-routes` — every supported alternate
  caller keeps its registered Stirge identities unpublished and exposes none,
  while still exposing its other creatures.
- `summon-playtest-r1.withdrawn-identities-retained` — every withdrawn and
  Wall-cleanup identity remains registered and `active` in the manifest; the
  nine Stirge roots still resolve; fixer names are unique and fixed.
- `summon-playtest-r1.only-named-capabilities-withdrawn` — only Magma Form
  among all mephit slots; Fire Giant loses only Improved Sunder; Storm Giant
  and other giants unchanged; Lillend loses only Bardic Suggestion.
- `summon-playtest-r1.load-reconciliation-exact` — exact owner, exact identity,
  fresh summons unchanged, fail-closed on missing configuration.

Updated (historical "nothing withheld" pins now exempt only the owner-withdrawn
key, through `SummoningCurrentSurface.WithheldKeys`): `coverage-union`,
`sprint10-stirge-published-registration` (now asserts withheld at all nine
tiers), `sprint32-public-satyr-routes`, and eleven pins that already read
`IsWithheld`. Added admission assertions to `sprint32-native-admission`.
A pre-change diagnostic run of the full domain suite (2,843 tests) showed
exactly these 14 Stirge pins failing and nothing else.

Development validator tests: 31/31 (two new). Sprint40 publisher self-test: 23
refusal checks PASS.

Review follow-up: the change is observer-only (runtime code with no domain
seam), so no domain test was added; the focused gate
`-Filter 'summon-playtest-r1;expanded-summoning.sprint32-native-admission'`
compiled production and selected 6 of 2,848 tests, 0 failures (14.9 s).

## Native observation (DATA)

Two guarded, save-free runs of the same phase `summon-playtest-r1-cuts` on the
disposable Sprint32 static-interior fixture (scenario
`disposable-expanded-summoning-sprint32-native-review`), one per candidate.

### Run 2 — corrected observer (candidate 2): PASS 20/20

Run `20261107T1455011632213Z-3164b960aa29463fbb4a776dc6473e79`, 117 s
including restoration; loaded mod 0.0.179 at commit `d46726d46ddc`. Raw
evidence stays local:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261107T1455011472245Z-disposable-expanded-summoning-sprint32-native-review/`
(`summon-playtest-r1-cuts.json` SHA-256 `2355cfc9993e…`,
`sprint32-review-progress.json` `56e9cd741877…`).

ProperFlanking2 1.2.3-2.1 was loaded and active (UMM entry loaded, active, its
own assembly, no load error). Chain status **`provider-active-exact`**: native
Improved Sunder `9719015e…` grants `SunderAction` `fa9bfb9f…` and
`SunderActionToggleAbility` `d9604ead…` (default-on), whose buff is
`SunderActionToggleBuff` `f9f7edc1…` carrying
`AttackReplacementWithCombatManeuver`.

Real Sunder chain, each reading separate (present / on / running for the
toggle; present / active for the buff):

| Unit and moment | Improved Sunder | `SunderAction` | Toggle present / on / running | Buff present / active |
| --- | --- | --- | --- | --- |
| Fire Giant, fresh | no | no | no / no / no | 0 / 0 |
| Storm Giant, fresh (control) | yes | yes | yes / yes / yes | 1 / 1 |
| Fire Giant, induced feat, before fixer | yes | yes | yes / yes / yes | 1 / 1 |
| Fire Giant, same frame after fixer | no | no | no / no / no | 0 / 0 |
| Fire Giant, after 5 frames | no | no | no / no / no | 0 / 0 |
| Storm Giant, after all fixers | yes | yes | yes / yes / yes | 1 / 1 |

Every bounded wait (900 frames / 8 game seconds) was already satisfied when
first checked (0 extra frames): the toggle was running with its buff after the
fixed settle frames, and the buff was gone in the same frame the fixer ran.
Fire Giant kept Great Cleave, the Cleave action and
`KMG_Summoning_Giant_Greatsword3d6`; Storm Giant wields
`KMG_Summoning_Giant_Sprint29_StormGreatsword4d6`.

Lillend, read from the live unit both before and after reconciliation:
spellbook present, Courage / Competence / Fascinate present, no
`BardicSuggestion` fact, dispatcher present with no bardic route. The
CowWithHat Suggestion provider was **inactive: "provider inactive / not
observed"**, so the Suggestion spell itself was not observed.

| Check | Result |
| --- | --- |
| `r1-stirge-withheld-everywhere` | PASS: 1179 canonical references exact; 439 menus (75 with adapter children), none offers any of the 9 canonical or 24 adapter Stirge children; all resolve |
| `r1-sunder-chain-identities` (new) | PASS: `provider-active-exact` |
| `r1-*-withdrawn-absent` | PASS ×5 |
| `r1-devils-retain-other-spells-and-wall-cleanup` | PASS |
| `r1-magma-retains-breath-and-pyrotechnics` | PASS |
| `r1-lillend-retains-suggestion-spell-and-performances` | PASS (Suggestion provider inactive, not observed) |
| `r1-fire-giant-no-sunder-source-storm-keeps-it` | PASS: rows "fresh" above |
| `r1-fixer-exact-owner-only` | PASS |
| `r1-*-legacy-retired` | PASS ×5 |
| `r1-fire-giant-fixer-removes-feat-dependents` | PASS: rows "induced" above, Storm Giant rechecked |
| `r1-retained-after-fixer` | PASS: live re-read, including Lillend |
| `fixture-restored` | PASS |

### Run 1 — original observer (candidate 1): FAIL 16/18

Run `20261107T0733045341820Z-7f884c82d45a4ec081637e44394a8a6a`,
123 s including restoration. Raw evidence stays local:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261107T0733045151696Z-disposable-expanded-summoning-sprint32-native-review/`
(`summon-playtest-r1-cuts.json` SHA-256 `5d030f73f774…`,
`sprint32-review-progress.json` `b92540ca12fd…`).

Original verdict: **FAIL, 16/18 + fixture restored** (kept). Both failures were
an observer defect: it identified ProperFlanking2's toggle by
`a0808193…` (`SunderArmorToggleAbility`, PF2's never-granted save-cleanup
dummy), and its buff readings used that dummy's buff. The toggle PF2 really
grants through Improved Sunder's AddFacts is `SunderActionToggleAbility`
`d9604eade0f54d88a899b5052f98d536` (buff `SunderActionToggleBuff`
`f9f7edc1148c453181460dccbbefa3dc`).

What the reader-only re-evaluation of the immutable raw evidence actually
establishes (`runtime-evidence/summon-playtest-r1/20261107-reader-reevaluation.json`,
qualified by `20261107-reader-reevaluation-qualification.json`; both local):
the run recorded the real toggle by **fact and presence only**: Improved
Sunder's two grant names, per-unit grant counts (Fire Giant 0/2 fresh, 2/2
induced before the fixer, 0/2 after; Storm Giant 2/2), and activatable lists
(Storm Giant `SunderActionToggleAbility:on`; no such toggle on the Fire Giant
fresh or after the fixer). It recorded **no buffs** at all, no toggle running
state, and no on-state for the induced Fire Giant before the fixer. Its
`fireToggleBuff`/`stormToggleBuff` fields read the dummy's buff. So the
re-evaluated "18/18" is a fact-level reading of the two Fire Giant predicates,
not an observation of the attack-replacement buff. The buff was first observed
by run 2, on candidate 2's bytes; that PASS belongs to candidate 2 only.

Run 1's `r1-retained-after-fixer` also reused the Lillend performances value
read before the fixers ran; run 2 re-reads it from the live unit.

| Check | Result |
| --- | --- |
| `r1-stirge-withheld-everywhere` | PASS: canonical inventory exact, 1179 references; 439 menus scanned (75 with adapter children), none offers any of the 9 canonical or 24 adapter Stirge children; all resolve; unit resolves |
| `r1-{bone-devil,ice-devil,magma-mephit,lillend-azata,fire-giant}-withdrawn-absent` | PASS ×5: fresh summons cast through ordinary own-tier children have no withdrawn grant, resource or brain action; fixer attached |
| `r1-devils-retain-other-spells-and-wall-cleanup` | PASS: Invisibility/Quickened Invisibility, Cone of Cold/Ice Storm and their AI; exactly one Wall source lifetime each, bound to the retained Wall ability |
| `r1-magma-retains-breath-and-pyrotechnics` | PASS |
| `r1-lillend-retains-suggestion-spell-and-performances` | PASS: spellbook, Courage/Competence/Fascinate, dispatcher with no bardic route. **The optional CowWithHat Suggestion provider was not active**, so the Suggestion spell itself was not observed |
| `r1-fire-giant-no-sunder-source-storm-keeps-it` | FAIL → fact-level PASS on reread: Fire Giant no Improved Sunder, 0/2 grants, no `SunderActionToggleAbility`, Great Cleave + greatsword kept; Storm Giant feat, 2/2 grants, `SunderActionToggleAbility:on`. Real buff not recorded |
| `r1-fixer-exact-owner-only` | PASS: the Bone Devil fixer left an Ice Devil's induced Wall untouched |
| `r1-*-legacy-retired` | PASS ×5: real fixer removed exactly the induced pre-R1 grants, AI actions and resource |
| `r1-fire-giant-fixer-removes-feat-dependents` | FAIL → fact-level PASS on reread: induced feat brought 2/2 grants; after the fixer 0/2 and no toggle. Toggle on/running and real buff not recorded |
| `r1-retained-after-fixer` | PASS as written, but its Lillend term was read before the fixers |
| `fixture-restored` | PASS |

In both runs the pre-R1 state was **induced** on fresh summons (facts, brain
actions and a resource added, then the real fixer's `OnPostLoad` called
directly). No older save was cold-loaded, so the native `m_AppliedFixers`
once-per-unit bookkeeping was not exercised.

### Restoration

Both runs, separately snapshotted at their own intake (DATA was still on
v0.0.140 at the follow-up intake): live mod tree restored to its exact pre-run
hash `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB` (254
files; backups `runtime-backups/live-mod/20261107T0732338559668Z` and
`…/20261107T1454304761589Z`), installed version v0.0.140 before and after,
native settings restored, and all 95 protected save files (including
`KMG_AUTOMATION_BASELINE`) byte- and metadata-identical. No save was loaded or
written. Run 2 restoration record:
`runtime-evidence/expanded-summoning-restoration/20261107T1456256502651Z-disposable-expanded-summoning-sprint32-native-review.json`
(failures 0, restoration verified).

## Limitations and named gaps

- **Lillend Suggestion spell:** the optional CowWithHat provider is inactive in
  DATA's installed stack, so the spell could not be observed natively (both
  runs; run 2 records it explicitly as "provider inactive / not observed"
  before and after reconciliation). Its
  configuration path (`ConfigureSuggestion32`, `BindSuggestionProvider32`) is
  unchanged.
- **Fire Giant attacks:** no attack was executed, so no "ordinary attack not
  replaced" event was recorded. Run 2 observed state, not combat: the Fire
  Giant has neither `SunderActionToggleAbility` nor `SunderActionToggleBuff`
  (the only carrier of ProperFlanking2's replacement component), while the
  Storm Giant's toggle runs with that buff active.
- **Legacy reconciliation:** induced state, not a cold load of an older save.
  It shows the real fixer removes the feat, the Sunder action, the running
  toggle and its active buff from a live unit; it does not show native
  once-per-unit fixer bookkeeping or the load order of a real older save.
- **Pre-existing Wall retirement:** no native wall was formed in R1. No cleanup
  dependency changed: section and source-lifetime components, their fields and
  the Wall ability identity are untouched, and both lifetimes were observed
  bound to the retained Wall ability. Prior native retirement evidence stays
  bound to its original artifacts.
- **Druid conversion rows:** covered by the shared `IsPublished` predicate and
  domain tests; the Druid conversion UI was not driven natively.
- No release qualification, compatibility matrix, whole-roster persistence or
  owner visual review is claimed.

## Validation spend (15-minute target, 30-minute ceiling)

| Invocation | Seconds |
| --- | --- |
| Production compile (first) | 16.6 |
| Full domain suite, one pre-change diagnostic (test execution 65.4 s + build) | ~85 |
| Focused test runs (one compile error, then pass) | ~12 |
| Production compiles while fixing the observer (3 errors + pass) | ~43 |
| Focused gate (196/196) | 4.5 |
| Validator unit tests and Sprint40 self-test | ~10 |
| Build attempts refused before compilation (bundle, validator, self-test) | 2 + 9 + 9 |
| Candidate build `ab24a92394ea` | 382 |
| Native run incl. restoration | 123 |
| Focused gate after observer correction (196/196) | 16.6 |
| **R1 total** | **≈ 713 s (≈ 11.9 min)** |
| Review follow-up: focused gate with production compile (6/6) | 14.9 |
| Review follow-up: candidate build `d46726d46ddc` | 378 |
| Review follow-up: native run 2 incl. restoration | 117 |
| **Follow-up increment** | **≈ 510 s (≈ 8.5 min)** |
| **Cumulative R1** | **≈ 1,223 s (≈ 20.4 min)** |

The cumulative spend is past the 15-minute target and inside the original
30-minute ceiling, which was not reset or extended.

## Owner retest checklist

1. Summon Nature's Ally I-IX (and Druid spontaneous / alternate sources): no
   Stirge entry; Dire Rat, Viper, Eagle and others still present.
2. Bone Devil and Ice Devil: no Wall of Ice button and never cast one; Ice Devil
   still casts Cone of Cold / Ice Storm; Bone Devil still turns invisible.
3. Magma Mephit: no Magma Form; breath and Pyrotechnics still used.
4. Lillend: no Bardic Suggestion; performances work; with CowWithHat Custom
   Spells enabled, the Suggestion spell is still in its spellbook.
5. Fire Giant: never sunders, and ordinary greatsword attacks are not replaced;
   Storm Giant still can.
6. Optional: load a save made with a summoned Bone Devil, Ice Devil, Magma
   Mephit, Lillend or Fire Giant active; the withdrawn ability should be gone
   after load (and any existing wall should still dissolve normally).
