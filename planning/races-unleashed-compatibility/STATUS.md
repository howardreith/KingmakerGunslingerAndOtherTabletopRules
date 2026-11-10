# Spider Summoner integration: status

**Incomplete with named blockers.** The two review corrections are implemented and accepted by review, and the
scoped native CMB web-escape bonus is now implemented. That bonus and the remaining gameplay acceptance observations
are open natively. The latest candidate's native run stopped on a recurring popup-teardown guard refusal before any
spider cell ran (see below). This is not mission completion and not release qualification. No merge, release or
permanent installation has been performed.

Baseline and its original result, unchanged: [BASELINE.md](BASELINE.md), including the `FAIL, 70 PASS / 1 FAIL`
result on candidate `efc75b8cc0b0`.

## Current candidate and run

| Item | Value |
| --- | --- |
| Source | `e6723d8723206d1fc213e98cb1132e4833862bb0`, v0.0.181 development, branch `codex/races-unleashed-summoning` |
| Package SHA-256 | `b916bc08fdb8972fff5ba7b69ca9c21fe81d3323f7d59d462fdcef6ea08cb0c9` |
| DLL SHA-256 | `298b3fa2d61edcada3f89926733dc8bba4ec78951851dd5d5fc42177e760452c` |
| MVID | `d81c80dd-8fa9-48ee-b187-00f32a499505` |
| Purpose | Development candidate `races-unleashed-summoning`; never a release |
| Run | `20261110T0329183919821Z-8081e6929acb4a5b8424e39ad87970fd`: **FAIL, 67 PASS / 2 FAIL** |
| Profile | The current DATA combined installation. Races Unleashed 1.0.11, DLL `6d18168c...4cb0`; Call of the Wild loaded |
| Restoration | `verified`; live tree `5C3E4AD4...BFBB`, `Params.xml` `E44F4495...30BB` |

**Recurring guard refusal (blocker).** The run stopped at the teardown of the Summon Monster V popup after the cleric
cell `cleric-foreign-legal`, which casts the repaired Ogre Spider. The popup-teardown guard refused a live tooltip
context that had no ability and no owning trigger; every candidate trigger reported `nativeHoverActive=false`. The
guard's recorded engine contract says a pointer entering any tooltip trigger can leave such a context behind, because
hiding the tooltip does not clear it. Before this run the cleric cells and the live contract classification passed
(Ogre and Tarantula `verified-repaired-spider-contract`). **No spider cell ran, so the web-escape bonus, the new escape
observations, the fiendish Crab and Widow casts and the retained-toggle cast have no native result on this candidate.**

This is the same refusal, at the same popup and around the same cast, as run 1 of candidate `8c1734743cce` (below):
two of the last three runs, on two candidates. The earlier classification is therefore withdrawn. Record it as: **a
recurring tooltip-ownership refusal at the Summon Monster V teardown around the repaired Ogre Spider cast; cause not
established.** An incidental pointer hover remains a plausible cause but is unproven. The cell's code did not change
in either candidate. The guard is a runtime-safety check, so it is not weakened here.

The next step is to narrow this before another run. Two options:
- Record which trigger created the leftover context, as diagnostic observer data.
- Confirm whether the DATA machine's pointer was in use during the 02:27 and 03:29 UTC runs.

Earlier candidates on this branch are retained, not replaced. Their evidence stays bound to their own bytes:

- `8c1734743cce`, package `919d9753...a250`, DLL `aa2fd336...5f17`, MVID `934bb8d1-e946-4609-ad4d-40ac584f8e9c`:
  - run 1 `20261110T0227019018671Z-...`: **FAIL, 64 / 4**, the same guard refusal as above;
  - run 2 `20261110T0231072443394Z-...`, matched rerun: **PASS, 153 / 0**. The run 2 evidence for RU-01, 02, 03, 04,
    06, 07, 08, 12, 18 and 19 below is bound to this candidate.
- `ae714f04965b`, package `27854009...ec9b`, DLL `511c1d3a...4e4d`, MVID `dc2771c2-05ab-4afc-83ae-46fae8be9a4a`, run
  `20261109T2245072861516Z-...`: **PASS, 139 / 0.** Its "held selection" cell built a new `AbilityData` after the feat
  was removed, so it proves only that a fresh choice without the feat is refused (see RU-02).
- `3e27c5809a9d`, package `461dc4f1...9953`, DLL `a9aac03e...2f1f`, run `20261109T2231457729890Z-...`: **FAIL, 132 / 3.**
  This mission's own quantity check, which assumed a fixed dice form, withheld all 38 generated rows.

Raw evidence stays private under `runtime-evidence\`.

## Review corrections (PR #125 review)

1. **One contract for repair, classification and conversion.** A row is now either the integration's exact
   repaired result, or exactly the known unrepaired input, or unresolved. The permissive fallback, which accepted
   any subset of a whitelist of after-spawn actions, is gone, so a row refused for repair can no longer be classified
   as verified. Conversion routes (Herald Caller, Druid) admit only a verified repaired row.
2. **Variants components.** Only `$KMG_ExpandedSummoning_Variants` on a tier I row is recognized. Races Unleashed
   inherits it by copying the already-published native tier I parent (read from its decompiled
   `CreateSpiderSummon`). The repair removes only that component. Any other variants component, or that one on
   another tier, makes the row unsupported, so it is left untouched and unclassified.
3. **Full buff applications.** The signature records each native buff application's recipient, permanence,
   duration, spell source, dispellability and parenting. The input contract requires them to equal the loaded
   native donor that Races Unleashed copied. The repaired contract requires the summon buff to be applied exactly as
   the donor applies it, with the lifecycle exactly `summon buff, family action, benefit`.
4. **Stale selection observer.** The observer now opens the real popup while the feat is held, keeps the exact
   `AbilityData` offered, removes the feat and executes that same object. It does this for an original row and a
   generated row. The fresh no-feat refusal is kept, under its own name.
5. The manifest now loads only on the integration's single attempt, which resolves the earlier review item.

Focused negatives for each case are in `summon-spider-summoner.contract-agreement`.

## Loaded DC consumers

**Poison.** The initial save (`ContextActionSavingThrow`) and the continuing saves (`BuffPoisonStatDamage`, DC
`Context.Params.DC` plus 2 per additional dose) read the poison feature's calculated parameters through the
context chain. Call of the Wild does not modify these poisons. No product change is indicated, but the actual saves
are unobserved (RU-12). Each comparison must use one isolated exposure, because extra doses legitimately raise the
DC; the Bebelith investigation shows how that can be mistaken for a calculation defect.

**Web.**
- **Inheritance:** the shared webbed buff `a719abac...` has no parameter calculator of its own, so its context
  inherits the web ability's parameters. The +2 therefore reaches `context.Params.DC`.
- **Without Call of the Wild:** the installed native break-free action is `ContextActionBreakFree` with **UseCMB on
  and UseCMD off**. This was read offline from the installed `sharedassets1.assets`, following the buff to its
  `AddFactContextActions.NewRound` element. Native `TryBreakFree` uses that DC for the skill check. When the
  target's CMB beats its best of Mobility and Athletics, it instead rolls a grapple maneuver against the spider's
  CMD and reads **no DC at all**, neither the base DC nor the +2.
- **With Call of the Wild (this profile):** its `fixGrappleSpells` replaces the action on this exact buff with
  `ContextActionBreakFreeFromSpellGrapple`, which reads the DC on both branches. Run 2 observed that loaded
  replacement, and a context DC of 12 without the marker and 14 with it.

## Native CMB web escape (RU-13), implemented in e6723d872320

The second review recommended completing the benefit on the native CMB branch, and that is implemented as
`SpiderSummonerWebEscapeDifficulty` on the feat marker. It adds exactly 2 to the one `RuleCalculateCMD` that sets
the escape maneuver's target number, and only when all of these hold:

- the calculation is a grapple CMD for this spider;
- it is directly enclosed by a grapple `RuleCombatManeuver` whose initiator is the same escapee;
- that maneuver's reason context is the webbed state `a719abac...`, whose parent context is the Giant Spider web
  `ea284589...` and whose caster is this spider;
- the spider holds the feat marker.

The component keeps no state between rules. The decision is the pure `SpiderWebEscapePolicy`, tested in
`summon-spider-summoner.web-escape-scope`. Its boundaries are exactly those the review set:
- no persistent or general CMD bonus;
- no change to ordinary grapples or other maneuvers;
- no change for other web sources such as the Bebelith, or for unmarked spiders;
- no second adjustment on paths that read the bonused DC, because the native skill check and Call of the Wild's
  replacement trigger no maneuver;
- no change to the web attack, duration, uses or the shared webbed blueprint.

The CMD baseline is kept; it is not replaced by the web DC.

**Observer added (not yet run).** For the summoned ordinary Giant Spider with the feat, each escape is attempted
with and without the marker on the same spider, and again on a spider summoned without the feat. Each attempt
records the actual escape rule and its final target number, and requires exactly one rule with a target number
exactly 2 higher. The escape paths are:
- the native skill branch and the native CMB branch: the installed native action's configuration, run in the
  webbed state's own context;
- the loaded action in this profile, which is Call of the Wild's replacement, on both of its branches.

A standalone CMD calculation checks that an ordinary CMD is unchanged. The escapee is the disposable caster, with its
BAB or skills set to select the branch and restored afterwards. The webbed state is applied in the spider's own web
context; the projectile hit itself remains covered only by the correction rules scenario, without the feat.

**Profile without Call of the Wild.** The committed profile `gunslinger-races-unleashed` is a valid dependency set.
Races Unleashed declares no requirements, and the three mods that require Call of the Wild (ProperFlanking2,
TweakOrTreat, ZFavoredClass) are not in it. The guarded launcher, however, admits a `profileId` only for
`observe-optional-mod-compatibility`, not for this summoning review phase. Running there needs a reviewed change to
the runtime wrapper. Within the combined profile, the native-path observation exercises the native escape code
directly.

## What the loaded game showed (unchanged findings)

- The Spider Summoner feature has only a Drow prerequisite. **Races Unleashed implements no DC benefit**, so this
  integration owns the +2.
- The four species copy the native Giant Spider unit (7 HD, Mid template band), keep its `HuntingSpiderPoison` and
  add their own copied poison. They have no web, so the ordinary Giant Spider's web is the only web in scope.
- Native quantity rows count as one die plus a rank bonus, the Superior Summoning hook, and from tier VII branch
  into two spawns. Generated rows clone that donor.

## Implementation map

| Concern | Where |
| --- | --- |
| Species, 46-placement matrix, exact DC blueprint set | `Summoning/SpiderSummonerCatalog.cs` |
| Exact input and repaired contracts, donors, conversion admission | `Summoning/SummonForeignRowContracts.cs` |
| Full buff applications in the signature | `Blueprints/SummonSignatureReader.cs`, `Summoning/SummonSignature.cs` |
| Live reading: variants components, donor applications | `Summoning/SummonClericAlignmentGate.cs` (`SummonForeignRowRuntime`) |
| In-place repair, generated rows, publication | `Blueprints/SpiderSummonerIntegration.cs` |
| Per-creature benefit action, DC owner, native CMB web-escape bonus | `Summoning/SpiderSummonerBenefit.cs` |
| Web-escape scope decision | `Summoning/SpiderSummonerCatalog.cs` (`SpiderWebEscapePolicy`) |
| Conversion admission by spell family | `Summoning/DruidSummonConversionVariantPatch.cs` |
| Late hook, before the existing reorder | `Summoning/AlternateSummoningCoordinator.cs` |

**Who owns the +2.** One hidden marker buff, granted once per eligible creature by the after-spawn action, adds 2
inside `RuleCalculateAbilityParams` for exact poison and web blueprints only. Nothing accumulates across saves,
recalculation or reload. Existing summons from before an upgrade receive no marker.

**Decisions recorded for review.**
- Original rows keep their Races Unleashed names, while generated rows use family-first labels such as "Spider, Ogre (1d3)".
- Summon Nature's Ally spiders follow the existing caster-alignment action and receive no Summon Monster template.
- The cleric rule stays Summon Monster only.
- Only repaired rows are projected into conversion routes. This affects only a failed integration: an unrepaired
  Ogre or Tarantula would then keep its v0.0.181 cleric classification but lose its v0.0.181 Herald projection.

## Acceptance status

Native entries name their candidate; `8c1734743cce` evidence is from its passing run 2. Focused evidence means
the named domain tests; those are not native observations.

| ID | Implementation | Focused evidence | Native evidence | Candidate | Remaining work |
| --- | --- | --- | --- | --- | --- |
| RU-01 | Done | Contract and matrix tests | All 8 rows repaired from exact inputs, including the donor application comparison: `repaired=8;generated=38;problems=` | `8c1734743cce` | None |
| RU-02 | Done | Input and shape negatives | An original row and a generated row, each obtained from the real popup with the feat and executed after its removal: refused, no spend, no spawn. A fresh no-feat choice is refused. The ordinary Giant Spider casts without the feat | `8c1734743cce` | Natural Drow feat acquisition (the fixture grants the exact feature) |
| RU-03 | Done | Matrix tests | Four Summon Monster originals cast as the named unit; repaired Ogre under evil and non-evil casters | `8c1734743cce` | **Four Summon Nature's Ally original casts** |
| RU-04 | Done | Every matrix cell; quantity expression | 46 of 46 placements in the census; 1d4+1 and 1d3 Summon Monster casts | `8c1734743cce` | Representative Summon Nature's Ally quantity casts |
| RU-05 | Done (donor rank preserved) | Quantity expression rejects a fixed bonus | None | none | **Superior and Augment Summoning native cast** |
| RU-06 | Done | Selection and band policy | Celestial on all four species; fiendish on the Ogre and Tarantula; correct Mid band, no leftovers | `8c1734743cce` | Fiendish Crab and Black Widow casts: cells added in `e6723d872320`, **not run**; LN and CN focused only |
| RU-07 | Done | None new | Neutral toggle read at execution, off and on; no Summon Monster template on Summon Nature's Ally rows | `8c1734743cce` | A Summon Monster choice kept from the real popup with the toggle off, then cast after it is turned on: cell added in `e6723d872320`, **not run**; Summon Nature's Ally cast under both toggle positions |
| RU-08 | Done | Repaired Ogre judged as spider | LG/Erastil, LE/Asmodeus, TN/Pharasma casts; deity-derived control; repaired Ogre offered to LG in both routes | `8c1734743cce` | None for the restriction itself |
| RU-09 | Done | Membership tests | Non-cleric provider control | `8c1734743cce` | **Real multiclass book; ordinary spontaneous SM and SNA pool casts** |
| RU-10 | Done; admission now requires a repaired row | Family-route and agreement tests | Repaired Ogre and Tarantula in Herald tier V and VII menus (Herald conversion feature granted to the fixture) | `8c1734743cce` | **Herald spider casts, including a quantity tier, debit and metamagic**; Crab I and Widow IV admission; **a legitimately acquired Herald Caller** |
| RU-11 | Done; admission now requires a repaired row | Family-route and agreement tests | None beyond census | none | **Druid conversion and prepared SNA comparison and casts** |
| RU-12 | Done; consumers traced | Benefit scope | Poison DC base+2 in the resolving context, stable on recalculation (ordinary Giant Spider 12 to 14; 12 without the feat) | `8c1734743cce` | **Actual first and continuing saves from isolated exposures; inherited Hunting poison; Augment Constitution change; reload; ineligible-source controls** |
| RU-13 | Done: DC hook plus native CMB-branch bonus (`e6723d872320`) | Benefit scope; web-escape scope | Loaded consumer in this profile is Call of the Wild's replacement; webbed context DC 12 to 14 with the marker (structural, `8c1734743cce`) | `8c1734743cce` | **Escape rules and final target numbers for native skill, native CMB and loaded paths, feat on, off and absent: observer added, not run**; a feat-marked projectile hit; the same in the `gunslinger-races-unleashed` profile after a reviewed wrapper change |
| RU-14 | Done | Exact identity negatives | Ordinary Giant Spider with and without the feat | `8c1734743cce` | **Reload**; native ineligible-source controls |
| RU-15 | Done | Display order tests | Real popups exact, sorted, no duplicates, including spider rows on the cleric's menus | `8c1734743cce` | SNA and Druid rendered menus with the feat, and **for a caster without the feat** |
| RU-16 | Done | None | None | none | **Fresh process, saved shortcut, reload** of an original and a generated row |
| RU-17 | Done | Module boundary | None | none | **RU-absent and module-off profiles**, after confirming each profile's dependencies |
| RU-18 | Reopened by review, corrected | Agreement tests: unfamiliar and misplaced variants components, empty after-spawn, changed application, reordered or extra actions, unreadable donor | Zero unresolved rows; all 8 live rows accepted by the stricter contracts | `8c1734743cce` | None known |
| RU-19 | Done | Observer compiles | Stale-selection cells corrected; old failure records retained | `8c1734743cce` | None |
| RU-20 | Done | n/a | Draft PR updated, restoration verified | `e6723d872320` | **Recurring popup-teardown guard refusal to narrow**; **NPC-without-feat selection/AI control**; owner review |

## Next bounded acceptance plan

First narrow the recurring guard refusal, so the next run can reach the spider cells. Then build one candidate and run
each open item as an explicit scenario, reusing existing fixtures where they exist:

| Open IDs | Scenario |
| --- | --- |
| Blocker | Diagnose the leftover tooltip context at the Summon Monster V teardown (owning trigger and pointer state), without weakening the guard |
| RU-06 | Fiendish Crab I and Black Widow IV casts under an evil caster (cells present) |
| RU-07 | Keep a Summon Monster choice from the real popup with the toggle off, turn it on, cast that choice (cell present); SNA under both toggle positions |
| RU-13 | Escape rules and final target numbers for the native skill branch, native CMB branch and loaded action, with the feat, with the marker removed and without the feat, plus the ordinary CMD control (cells present); a feat-marked projectile hit; then the `gunslinger-races-unleashed` profile after a reviewed wrapper change |
| RU-03, 04, 07, 11, 15 | A Drow Druid with a prepared book: the four SNA original casts, one 1d3 and one 1d4+1 cast, the Druid conversion menu and a cast with its debit, and SNA under both toggle positions; the same SNA and Druid menus rendered for a caster without the feat |
| RU-09 | A multiclass caster and an ordinary spontaneous SM and SNA book: pool casts and debits |
| RU-10 | A legitimately acquired Herald Caller rather than a granted conversion feature: Crab I and Widow IV admission, one quantity-tier spider cast with debit, and one metamagic cast |
| RU-05, 12 | Superior and Augment Summoning on one quantity cast; isolated poison exposures for the first and a continuing save, including the inherited Hunting poison and Augment's Constitution change |
| RU-02 | Natural Drow acquisition of the feat through level-up |
| RU-14, 16 | Save, fresh process, reload: an original and a generated row from saved shortcuts; marker, DC and escape bonus after reload; ineligible-source controls |
| RU-17 | RU-absent and module-off profiles, after their dependencies are confirmed |
| RU-20 | An NPC without the feat: selection and AI never pick a gated spider row |

## Validation ledger

Second review task (15-minute target, 30-minute ceiling):

| Step | Seconds |
| --- | ---: |
| Focused gates (one compile failure, then 32 selected) | about 25 |
| Candidate build `e6723d872320` | 592 |
| Guarded native run, FAIL (guard refusal) | 182 |
| **Total** | **about 800** |

No rerun was launched. The refusal recurred, so the policy calls for narrowing rather than relaunching unchanged.
A further build and run would also come close to the ceiling. Earlier tasks' ledgers (about 1,600 s and about
920 s) are unchanged and separate.

Focused results on `e6723d872320`: `summon-cleric-alignment;summon-spider-summoner;expanded-summoning.display-ord;druid`,
32 of 2,919 selected, 0 failures, recorded by the candidate build. The 785-test related selection was run on
`8c1734743cce`; this change touches only the marker, its new component and the observer, outside that shared
signature surface. The complete domain suite and release qualification are deferred.
