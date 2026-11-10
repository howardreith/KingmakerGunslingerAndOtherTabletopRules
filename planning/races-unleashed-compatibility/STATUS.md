# Spider Summoner integration: status

**Incomplete with named blockers.** The review corrections and the scoped native CMB web-escape bonus are
implemented. Under controlled input, candidate `e6723d872320` passed its focused native phase (174 PASS, 0 FAIL),
including the new web-escape, fiendish and retained-toggle observations. The remaining gameplay acceptance
observations are open (table below). The corrected observer is committed but not yet built. This is not mission
completion and not release qualification. No merge, release or permanent installation has been performed.

Baseline and its original result, unchanged: [BASELINE.md](BASELINE.md), including the `FAIL, 70 PASS / 1 FAIL`
result on candidate `efc75b8cc0b0`.

## Candidates and runs (third review task)

| Candidate | Package / DLL | Run | Result |
| --- | --- | --- | --- |
| `e6723d8723206d1fc213e98cb1132e4833862bb0` (product: CMB web-escape bonus) | `b916bc08...0cb9` / `298b3fa2...452c` | `20261110T0329183919821Z-8081e6929acb4a5b8424e39ad87970fd` | **FAIL, 67 / 2**: guard refusal, before any spider cell |
| same | same | `20261110T0414271665285Z-af430dceb7de452a899db21058d3ef86`, **controlled input** | **PASS, 174 / 0** |
| `655ab8a8ae3e` (observer: rule and branch assertions, real grapple control, in-phase real web hit) | `0483c0ea...46dd` / `79f5e7d0...8c5c` | `20261110T0427495327289Z-81a9b3302a4b4e3d8842109f39574375`, controlled input | **FAIL, 155 / 9**: observer defects, below |
| `182717dd10` (observer fix) | not built | none | none |

**Controlled input.** No human used DATA during the two controlled runs. The pointer was parked at (300, 200), over
fog with no interactive UI, before launch, and sampled every 5 seconds. All 62 samples on the first run and all 56
on the second read (300, 200). Both runs were launched from the clean candidate commit in the main repository; the
development receipt binds each package to that path. The profile is the current DATA combined installation, with
Races Unleashed 1.0.11 and Call of the Wild loaded. Restoration was `verified` after every run: live tree
`5C3E4AD4...BFBB`, `Params.xml` `E44F4495...30BB`.

**Popup-teardown guard refusal.** Recorded as possible input interference, not a confirmed root cause. It stopped run
1 of `8c1734743cce` and the first run of `e6723d872320`, both at the Summon Monster V popup around the repaired Ogre
Spider cast. The two receipts differ:
- In run 1 the first close inside the cast failed, with the popup still open.
- In the `e6723d872320` run, the second, outer close after the cast failed, with the popup already cleared.

Both refused a tooltip context that had no ability and no live trigger. The owner reports probably using the mouse
during those runs. Under controlled input the same unchanged package passed, and the next candidate's run passed
that step too. That supports the interference hypothesis without proving what caused the earlier refusals; both
failures are retained. The guard was not changed.

**`655ab8a8ae3e` run: observer defects, not gameplay.**
- **Assertion bug:** the new rule-problem check compared a Json.NET token type. A null string stored in a `JObject`
  is a `String` token holding null, so the check failed although every escape triggered exactly its expected rule
  and branch. Fixed in `182717dd10`.
- **In-phase real web hit:** the cell created a hostile and fired the web through the correction scenario's
  detached-ability helper. The command was interrupted (`result=Interrupt`), and a second, `Pause`, mode was left on
  the native mode stack. Every later cell then failed its ownership checks, by design. Those later failures are
  consequences, not findings. The cell is withdrawn in `182717dd10`. A feat spider's real web hit needs a fixture
  that owns its own clock and hostile, such as the correction rules scenario.

The recorded values in that run still answer the review's two observer findings:
- **Loaded branches:** every escape attempt triggered exactly the rule and branch its implementation requires,
  identified as `native` or `cotw`. Native skill used `SkillAthletics`, native CMB a `Grapple` maneuver, loaded skill
  `SkillAthletics` and loaded CMB `AdditionalCMB`. Each was 14 with the marker and 12 without.
- **Ordinary grapple:** a real `RuleCombatManeuver` against the spider with no web reason had a target number of 12
  both with and without the marker.

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

**Native result (`e6723d872320`, controlled input, PASS).** The summoned ordinary Giant Spider with the feat attempted
each escape with and without the marker. The webbed state was applied in the spider's own web execution context, and
the escapee's BAB or skills selected the branch.

| Path | Rule that ran | Without marker | With marker | Spider without the feat |
| --- | --- | ---: | ---: | ---: |
| Native skill | `RuleSkillCheck`, `SkillAthletics` | 12 | 14 | 12 |
| Native CMB | `RuleCombatManeuver`, `Grapple`, against the spider's CMD | 12 | **14** | 12 |
| Loaded (Call of the Wild) skill | `RuleSkillCheck`, `SkillAthletics` | 12 | 14 | 12 |
| Loaded (Call of the Wild) CMB | `RuleSkillCheck`, `AdditionalCMB` | 12 | 14 | 12 |

The new component therefore raises the native CMB branch's actual target number by exactly 2 through the engine's own
rule chain, and no path is adjusted twice.

Two limits apply to this candidate's verdict. Its pass condition did not itself assert the loaded branches, and its
control was a standalone CMD calculation. The next candidate's observer asserts both, using a real grapple
maneuver, and its recorded values already match (see above). The projectile hit itself is not part of this
observation.

**Profile without Call of the Wild.** The committed profile `gunslinger-races-unleashed` is a valid dependency set.
Races Unleashed declares no requirements, and the three mods that require Call of the Wild (ProperFlanking2,
TweakOrTreat, ZFavoredClass) are not in it.

The earlier statement that a launcher change is needed was wrong. The existing
`scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1` accepts that profile, the
`disposable-expanded-summoning-sprint32-native-review` scenario, its `-Parameters`, and a development candidate
through `-DevelopmentPurpose` with `-ReuseInstalledArtifact` and the exact `-PackagePath`. The next step is a
preflight on that route with the exact candidate. It was not run in this task, because the wrapper stages mod states
and the validation budget would not cover a profile run after the build.

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

Native entries name their candidate; `8c1734743cce` evidence is from its passing run 2, `e6723d872320` evidence from its
controlled-input run. Focused evidence means
the named domain tests; those are not native observations.

| ID | Implementation | Focused evidence | Native evidence | Candidate | Remaining work |
| --- | --- | --- | --- | --- | --- |
| RU-01 | Done | Contract and matrix tests | All 8 rows repaired from exact inputs, including the donor application comparison: `repaired=8;generated=38;problems=` | `8c1734743cce` | None |
| RU-02 | Done | Input and shape negatives | An original row and a generated row, each obtained from the real popup with the feat and executed after its removal: refused, no spend, no spawn. A fresh no-feat choice is refused. The ordinary Giant Spider casts without the feat | `8c1734743cce` | Natural Drow feat acquisition (the fixture grants the exact feature) |
| RU-03 | Done | Matrix tests | Four Summon Monster originals cast as the named unit; repaired Ogre under evil and non-evil casters | `8c1734743cce` | **Four Summon Nature's Ally original casts** |
| RU-04 | Done | Every matrix cell; quantity expression | 46 of 46 placements in the census; 1d4+1 and 1d3 Summon Monster casts | `8c1734743cce` | Representative Summon Nature's Ally quantity casts |
| RU-05 | Done (donor rank preserved) | Quantity expression rejects a fixed bonus | None | none | **Superior and Augment Summoning native cast** |
| RU-06 | Done | Selection and band policy | Celestial on all four species; fiendish on all four, including Crab and Black Widow under an LE caster; correct Mid band, no native or opposite template left | `e6723d872320` | LN and CN focused only |
| RU-07 | Done | None new | Neutral toggle read at execution, off and on. A Summon Monster choice kept from the real popup with the toggle off and cast after it was turned on summoned a fiendish Tarantula, spending exactly its slot. No Summon Monster template on Summon Nature's Ally rows | `e6723d872320` | Summon Nature's Ally cast under both toggle positions |
| RU-08 | Done | Repaired Ogre judged as spider | LG/Erastil, LE/Asmodeus, TN/Pharasma casts; deity-derived control; repaired Ogre offered to LG in both routes | `8c1734743cce` | None for the restriction itself |
| RU-09 | Done | Membership tests | Non-cleric provider control | `8c1734743cce` | **Real multiclass book; ordinary spontaneous SM and SNA pool casts** |
| RU-10 | Done; admission now requires a repaired row | Family-route and agreement tests | Repaired Ogre and Tarantula in Herald tier V and VII menus (Herald conversion feature granted to the fixture) | `8c1734743cce` | **Herald spider casts, including a quantity tier, debit and metamagic**; Crab I and Widow IV admission; **a legitimately acquired Herald Caller** |
| RU-11 | Done; admission now requires a repaired row | Family-route and agreement tests | None beyond census | none | **Druid conversion and prepared SNA comparison and casts** |
| RU-12 | Done; consumers traced | Benefit scope | Poison DC base+2 in the resolving context, stable on recalculation (ordinary Giant Spider 12 to 14; 12 without the feat) | `8c1734743cce` | **Actual first and continuing saves from isolated exposures; inherited Hunting poison; Augment Constitution change; reload; ineligible-source controls** |
| RU-13 | Done: DC hook plus native CMB-branch bonus | Benefit scope; web-escape scope | Every escape path's rule and final target number: native skill, native CMB, and loaded skill and CMB each 12 to 14 with the marker and 12 without the feat (`e6723d872320`); the same rules and branches asserted, with a real ordinary grapple unchanged at 12 (values from `655ab8a8ae3e`) | `e6723d872320` | Corrected observer's verdict on a built candidate; **a feat spider's real web hit followed by escape**; the `gunslinger-races-unleashed` profile through the existing compatibility wrapper |
| RU-14 | Done | Exact identity negatives | Ordinary Giant Spider with and without the feat | `8c1734743cce` | **Reload**; native ineligible-source controls |
| RU-15 | Done | Display order tests | Real popups exact, sorted, no duplicates, including spider rows on the cleric's menus | `8c1734743cce` | SNA and Druid rendered menus with the feat, and **for a caster without the feat** |
| RU-16 | Done | None | None | none | **Fresh process, saved shortcut, reload** of an original and a generated row |
| RU-17 | Done | Module boundary | None | none | **RU-absent and module-off profiles**, after confirming each profile's dependencies |
| RU-18 | Reopened by review, corrected | Agreement tests: unfamiliar and misplaced variants components, empty after-spawn, changed application, reordered or extra actions, unreadable donor | Zero unresolved rows; all 8 live rows accepted by the stricter contracts | `8c1734743cce` | None known |
| RU-19 | Done | Observer compiles | Stale-selection cells corrected; old failure records retained | `8c1734743cce` | None |
| RU-20 | Done | n/a | Draft PR updated, restoration verified | `e6723d872320` | **NPC-without-feat selection/AI control**; owner review |

## Next bounded acceptance plan

Build the committed observer fix once, and run under controlled input: no human input to DATA, pointer parked.
Then cover each open item as an explicit scenario, reusing existing fixtures where they exist:

| Open IDs | Scenario |
| --- | --- |
| RU-13 | Corrected observer verdict on a built candidate; a feat spider's real web hit followed by escape, in a fixture that owns its clock and hostile (the correction rules scenario with the feat granted); the `gunslinger-races-unleashed` profile through `Invoke-KingmakerCompatibilityProfile.ps1` after its preflight |
| RU-03, 04, 07, 11, 15 | A Drow Druid with a prepared book: the four SNA original casts, one 1d3 and one 1d4+1 cast, the Druid conversion menu and a cast with its debit, and SNA under both toggle positions; the same SNA and Druid menus rendered for a caster without the feat |
| RU-09 | A multiclass caster and an ordinary spontaneous SM and SNA book: pool casts and debits |
| RU-10 | A legitimately acquired Herald Caller rather than a granted conversion feature: Crab I and Widow IV admission, one quantity-tier spider cast with debit, and one metamagic cast |
| RU-05, 12 | Superior and Augment Summoning on one quantity cast; isolated poison exposures for the first and a continuing save, including the inherited Hunting poison and Augment's Constitution change |
| RU-02 | Natural Drow acquisition of the feat through level-up |
| RU-14, 16 | Save, fresh process, reload: an original and a generated row from saved shortcuts; marker, DC and escape bonus after reload; ineligible-source controls |
| RU-17 | RU-absent and module-off profiles, after their dependencies are confirmed |
| RU-20 | An NPC without the feat: selection and AI never pick a gated spider row |

## Validation ledger

Third review task (15-minute target, 30-minute ceiling):

| Step | Seconds |
| --- | ---: |
| Refused launches (records commit checked out; development package path) | about 50 |
| Controlled-input run, `e6723d872320`, PASS | 311 |
| Focused gates, 32 selected, three times | about 60 |
| Candidate build `655ab8a8ae3e` | 410 |
| Controlled-input run, `655ab8a8ae3e`, FAIL (observer defects) | 280 |
| **Total** | **about 1,110** |

The observer fix was not built: another build and run (700 to 900 s) would reach the ceiling. Earlier tasks'
ledgers (about 1,600 s, 920 s and 800 s) are unchanged and separate.

Focused results: `summon-cleric-alignment;summon-spider-summoner;expanded-summoning.display-ord;druid`, 32 of 2,919
selected, 0 failures, on each of `e6723d872320`, `655ab8a8ae3e` and `182717dd10`. The 785-test related selection was
run on `8c1734743cce`. The complete domain suite and release qualification are deferred.
