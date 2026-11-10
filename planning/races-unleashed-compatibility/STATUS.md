# Spider Summoner integration: status

**Incomplete with named blockers.** Every gameplay behavior in the contract is implemented, and the delivered
candidate `106cda92f3` (with six phases carried from `deebec4edb39`, below) passed all seven completion
transactions under controlled input. Four acceptance items still lack required observations: **RU-16** saved
references across save/reload, plus the reload parts of RU-02 and RU-14; **RU-15** rendered SNA and Druid popups;
**RU-10** metamagic, Crab I/Widow IV Herald admission and a cast by the naturally levelled Herald; and **RU-09** a cast
from the multiclass cleric's book. The continuation's 120-minute validation allowance cannot fit them (ledger below).
This is focused compatibility evidence, not release qualification, and owner review is pending. No merge, release or
permanent installation has been performed.

Baseline and its original result, unchanged: [BASELINE.md](BASELINE.md), including the `FAIL, 70 PASS / 1 FAIL`
result on candidate `efc75b8cc0b0`.

## Completion continuation (owner authorization of 2026-11-10)

The owner authorized this continuation with a 90-minute planning target and a 120-minute cumulative ceiling for
builds, checks, preflights and guarded native validation. That replaces the ordinary 30-minute ceiling for this named
continuation only; the general validation policy is unchanged.

### Delivered candidate

| Item | Value |
| --- | --- |
| Source | `106cda92f38ba8700737ea6e888847c843ffdf43`, v0.0.181 development, branch `codex/races-unleashed-summoning` |
| Package SHA-256 | `a41753728786df801acee41f7d7aea65b2b6f911cf17ec1b1703d4e9ebaf2249` |
| DLL SHA-256 | `2ecb8761fae4d43ace4dba153d0c2aee2a299df1d64cdd9fff2f9cab36916f52` |
| Purpose | Development candidate `races-unleashed-summoning`; never a release |
| Selected tests | `summon-cleric-alignment;summon-spider-summoner;expanded-summoning.display-ord;druid`, 33 selected, 0 failures |
| Observer | Same source: phases `summon-cleric-alignment`, `spider-web`, `spider-mechanics`, `spider-routes`, `spider-module-off` |

**Carried evidence.** `106cda92f3` differs from `deebec4edb394265c4f975b151517d128e08c090` only in two files.
- **What changed:** an optional spawn-count predicate on the shared Herald/Druid cast observer
  (`Cleanup2MenuAdapterDruidCast`), and the cleric phase call that passes it. Without the predicate, every existing
  caller keeps exactly its old behavior.
- **Rerun on the delivered bytes:** the cleric phase, the only one affected.
- **Carried from `deebec4edb39`** (package `911ed942...486c`, DLL `c6a18055...887a`): the six spider-web,
  spider-mechanics, spider-routes and spider-module-off transactions. Their source is identical in both candidates.

### Native transactions

| Phase | Profile | Candidate | Run ID | Result | Input |
| --- | --- | --- | --- | --- | --- |
| `summon-cleric-alignment` | combined DATA install | `106cda92f3` | `20261110T0656579610169Z-...` | **PASS 186 / 0** | pointer parked, 64/64 samples |
| `spider-routes` | combined | `deebec4edb39` | `20261110T0628118612342Z-c31d6bd0b2be4c0296d9ee978bf50034` | **PASS 22 / 0** | pointer moved (0,0 and 640,360 read) |
| `spider-mechanics` | combined | `deebec4edb39` | `20261110T0631458477894Z-340574a59df14795ad885716ded63751` | **PASS 13 / 0** | parked |
| `spider-web` | combined | `deebec4edb39` | `20261110T0639091492164Z-2b581e637044446d9e82479cdc081428` | **PASS** | pointer moved |
| `spider-module-off` | combined, Expanded Summoning OFF | `deebec4edb39` | `20261110T0641112018794Z-67e930f0203d481597f3f8f54ab9c861` | **PASS** | parked |
| `spider-web` | `gunslinger-races-unleashed` (no Call of the Wild) | `deebec4edb39` | `20261110T0643560297139Z-7908c6bce41d43a3ac92b3c352845924` | **PASS** | parked |
| `spider-web` as RU-absent control | `gunslinger-only` | `deebec4edb39` | `20261110T0646424296444Z-80d88f824f904ac28e0a6cc9a4685f43` | **PASS** | pointer moved |

Pointer samples are supporting evidence only. "Moved" samples (including 0,0 and 640,360 while the game window
started) appeared in phases with no UI interaction. The UI-driven cleric phase ran with the pointer parked throughout.

**Restoration.** Every transaction restored exactly.
- **Scenario transactions:** live tree `5C3E4AD4...BFBB` restored, verified each time.
- **Profile transactions:** `restorationVerified=True`, with Call of the Wild settings and `FeatureModules.json`
  byte-exact. The wrapper does not protect UMM `Params.xml`, which UMM rewrites at launch, so the runner restored it
  each time.
- **`Params.xml`:** ended at its intake hash `E44F4495...30BB` after every run.

**Earlier candidates of this continuation (retained):**

| Candidate | Result | What it showed |
| --- | --- | --- |
| `d94acab01653` (package `3918fffd...c9ef9`) | web FAIL; mechanics FAIL 7/6; module-off FAIL; RU-absent **PASS** | Two summons' CMDs differed by 4 with no web reason, so CMD escapes are now decided within one spider. Poison target died. Module-off observer missed RU's tier I attachment route |
| `5107a283bd20` (package `2cb359c8...8942`) | web **PASS** (both profiles); module-off **PASS**; cleric **PASS 174/0**; mechanics FAIL; routes FAIL | The target was dead, which explained the missing poison states. Herald children are expanded by the popup. Hunter SNA |
| `df4b3e4bc6a9` (package `b592b381...2886`) | mechanics FAIL 1 (Tarantula natural-1 miss; NPC execution); routes FAIL (my lifetime check; Herald slot) | Fixed in `deebec4edb39` |
| `deebec4edb39` | six PASS; cleric FAIL 184/2 | The Herald quantity cast was correct, but the observer assumed one spawn; fixed in `106cda92f3` |

### What the native runs established

- **Real web hit → held state → loaded escape** (RU-13), in both profiles:
  - **The hit:** a ranged-touch attack hit, no initial save was rolled, uses went 2→1→0, and the held state's
    source, parent context (the Giant Spider web) and remaining 10 rounds were exact.
  - **Combined profile (loaded Call of the Wild escape):** skill and CMB branches were each 14 for the feat summon
    against 12 for the unmarked summon.
  - **Profile without Call of the Wild (loaded native escape):** the skill branch was 14 against 12; the CMB grapple
    maneuver against the spider's CMD was 14 against 12, both across summons and within one spider.
  - **Ordinary grapple:** unchanged.
  - **Measurement method:** two summons' CMDs can differ for reasons unrelated to the feat (a +4 gap appeared once
    with no web reason). CMD escapes and the ordinary-grapple control are therefore decided within one spider, with
    its marker present and removed, and the cross-summon pair is kept as a diagnostic.
- **Actual poison saves** (RU-12, RU-14), each from one forced-hit bite in an isolated exposure, with the target kept
  alive and unharmed:
  - **Ordinary Giant Spider:** first and continuing saves at 14 with the feat against 12 without.
  - **All four RU species:** every resolving poison, including the inherited Hunting poison, saved at 16 against an
    unbonused 14, first and continuing, with no repeated first save.
  - **Augment Summoning:** Constitution 12→16 and DC 16 against 14, still exactly +2.
  - **Ineligible sources** (Bebelith, Giant Scorpion and others summonable with the feat held): no benefit.
- **Superior Summoning** (RU-05): with the 1d3 die forced to 2, the generated row summoned 2 without Superior
  Summoning and 3 with it, each spider with its summon buff, one template and exactly one benefit.
- **Routes** (RU-02/03/04/07/09/11):
  - **Natural Drow:** a Drow levelled natively from character creation selected Spider Summoner from its level 1
    feat selection.
  - **Prepared SNA:** cast all four originals, and with forced dice 1d3 → 2 and 1d4+1 → 3, each with exactly its
    slot spent and a finite `SummonedUnitBuff` lifetime.
  - **SNA toggle:** under both positions, no Summon Monster template and caster-matched alignment.
  - **Druid conversion** at tiers V and VII: exactly the prepared parent's spider rows, no duplicates, source chain
    kept, exactly the source slot spent.
  - **Druid without the feat:** spider rows hidden and unavailable.
  - **Natural Herald:** a cleric/wizard Herald Caller from native progression holds the conversion feature and two
    books.
  - **Sorcerer (spontaneous SM) and Call of the Wild Hunter (spontaneous SNA):** each lost exactly one pool use, and
    the stale choice was refused with nothing spent. Each learned its summon spell as an ordinary known spell from its
    class list.
- **Herald spider casts** (RU-10, cleric phase, `106cda92f3`): the repaired Ogre and the Crab 1d4+1 (2 to 5 creatures)
  through the real Herald Summon Monster V popup over an unrelated prepared spell. Both had the native command and
  rule, the retained quantity conversion in the chain, and exactly one original slot spent.
- **Module OFF** with RU installed (RU-17/16 lifecycle):
  - the eight original rows keep their exact Races Unleashed shape;
  - the tier I Crabs attach through Races Unleashed's own `CustomSpontaneousSpell`;
  - no KMG row is listed;
  - the original Widow casts with its native template and no benefit.
- **RU absent** (RU-17): the integration reports `absent:`, there are no spider rows or foreign identities, and the
  ordinary Giant Spider casts with no benefit.
- **NPC** (RU-20):
  - **Census:** no loaded AI cast action, of 917, names a spider row.
  - **Ordinary variant:** an NPC without the feat got a cast command from the native AI action and really summoned.
  - **Gated variant:** the gated spider variant yielded no cast command (the AI returns a move), so there is no
    selection or refusal loop.
  - **Limit:** the AI-built ability was executed by the native ability executor; the command-layer spend is not part
    of this control.

## History: third review task


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
| Completion phases: real web hit, poison and summoning feats, NPC, module-off, RU-absent | `RuntimeTesting/RuntimeTestRunner.SpiderCompletion.cs` |
| Routes phase: natural Drow and Herald, SNA, Druid conversion, spontaneous books | `RuntimeTesting/RuntimeTestRunner.SpiderRoutes.cs` |
| Escape observation verdicts (named failures) | `RuntimeTesting/SpiderEscapeObservationVerdict.cs` |
| Exact module-OFF admission pair | `scripts/ExpandedSummoningOrchestration.Common.ps1`, checked by `scripts/Test-SummonOverhaulOffAdmission.ps1` |
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

Disposition against every subrequirement of `ACCEPTANCE.md`. Native candidates are named per row.

| ID | Implementation | Focused evidence | Native evidence | Candidate | Remaining work |
| --- | --- | --- | --- | --- | --- |
| RU-01 | Done | Contract, matrix and agreement tests | All 8 rows repaired from exact inputs: `repaired=8;generated=38;problems=` | `106cda92f3` | None |
| RU-02 | Done | Input and shape negatives | Natural Drow acquisition at level 1; retained and fresh no-feat choices refused, nothing spent; ordinary Giant Spider without the feat | `106cda92f3`, `deebec4edb39` | **Saved-choice attempt after reload** (RU-16) |
| RU-03 | Done | Matrix tests | Four SM and four SNA originals cast as the named spider with a finite summon lifetime; repaired Ogre under evil and non-evil casters | `106cda92f3`, `deebec4edb39` | None |
| RU-04 | Done | Every matrix cell; quantity expression | 46/46 census; SM 1d3 and 1d4+1; SNA 1d3 → 2 and 1d4+1 → 3 with forced dice; Herald 1d4+1 | `106cda92f3`, `deebec4edb39` | None |
| RU-05 | Done | Quantity expression | Superior: forced 1d3 → 2 without, 3 with, every spider with one benefit; Augment Con 12→16 and DC interaction | `deebec4edb39` | None |
| RU-06 | Done | Selection and band policy | Celestial and fiendish on all four species, Mid band, no leftover template | `106cda92f3` | LN and CN focused only, as the contract permits |
| RU-07 | Done | None new | Choice kept, toggle changed, cast (fiendish); SNA under both toggle positions with no SM template | `106cda92f3`, `deebec4edb39` | None |
| RU-08 | Done | Repaired Ogre judged as spider | LG/Erastil, LE/Asmodeus, TN/Pharasma; deity-derived control | `106cda92f3` | None |
| RU-09 | Done | Membership tests | Sorcerer feat holder; ordinary spontaneous SM (Sorcerer) and SNA (Call of the Wild Hunter) pool casts, exactly one use, stale refused; natural cleric/wizard with two books | `deebec4edb39` | **A spider cast from the multiclass cleric's book, with the other book unchanged** |
| RU-10 | Done | Family-route and agreement tests | Natural Herald Caller progression; Herald spider casts (repaired Ogre and Crab 1d4+1) through the real popup with exact debit and chain | `106cda92f3`, `deebec4edb39` | **Representative metamagic; Crab I and Widow IV Herald admission; a cast by the naturally levelled Herald** (the casts used the cleric phase's granted-feature Herald) |
| RU-11 | Done | Family-route tests | Druid conversion at V and VII compared with prepared SNA and cast, exact source debit, no duplicates | `deebec4edb39` | None |
| RU-12 | Done | Benefit scope | Actual first and continuing saves: ordinary 14/12; four species and Hunting poison 16 vs 14; Augment 16/14; no cumulative bonus | `deebec4edb39` | None |
| RU-13 | Done, with native CMB-branch bonus | Benefit and web-escape scope; escape verdicts | Real hit → held state → loaded escape, both branches, both profiles, exactly +2; no initial save; uses and duration unchanged; ordinary grapple unchanged | `deebec4edb39` | None |
| RU-14 | Done | Exact-identity negatives | Feat on/off matched casts; ineligible sources with the feat held get nothing | `deebec4edb39` | **No duplication after reload** (RU-16) |
| RU-15 | Done | Display order tests | Real SM popups including Herald conversion, sorted, no duplicates; SNA and Druid conversion rows at data level; no-feat Druid hidden | `106cda92f3`, `deebec4edb39` | **Rendered SNA and Druid conversion popups, with and without the feat** |
| RU-16 | Done (no implementation gap known) | None | Module-OFF lifecycle without a save: dormant rows, originals intact | `deebec4edb39` | **Fresh-process save/reload of an original and a generated shortcut and an active summon; disabled-module reload** |
| RU-17 | Done | Module boundary | RU-absent profile; Expanded-Summoning-inactive/RU-present control | `deebec4edb39` | None |
| RU-18 | Done | Agreement tests | Zero unresolved rows | `106cda92f3` | None |
| RU-19 | Done | Observer compiles; escape verdict tests | Per-row semantics; failures retained | `106cda92f3` | None |
| RU-20 | Done | n/a | Draft PR; exact restoration; NPC selection control | `106cda92f3`, `deebec4edb39` | Owner review |

## Smallest remaining actions

1. **RU-16 (with reload for RU-02, RU-14).** A `spider-summoner` persistence scope in the existing working-save
   workflow. It needs a scope constant and selection keys, request admission, the `Invoke-Sprint22InstalledStack.ps1`
   ValidateSet and development branch, and the routing tests. The prepare process summons through the spider rows and
   pins an original and a generated row to the action bar. Verify-cleanup then reloads, casts from the pins, checks the
   active summon's marker and DC, attempts a pinned row after feat removal, and retires the fixture. A module-OFF
   verify comes last. This means three or four fresh processes; Kingmaker saves only to `KMG_AUTOMATION_WORKING`.
2. **RU-15.** Open the SNA and Druid conversion popups through the cleric phase's existing `ClericOpenMenu`, for the
   natural Druid and the no-feat Druid.
3. **RU-10.** Prepare the unrelated spell with Extend Spell for one Herald spider cast; open the tier I and IV Herald
   popups; repeat one Herald cast with the naturally levelled Herald.
4. **RU-09.** One spider cast from the natural cleric/wizard's cleric book, comparing both books.

## Validation ledger (completion continuation: 90-minute target, 120-minute ceiling)

| Category | Seconds |
| --- | ---: |
| Focused gates and the OFF admission test | about 194 |
| Candidate builds (`d94acab016`, `5107a283bd`, `df4b3e4bc6`, `deebec4edb`, `106cda92f3`) | 2,215 |
| Guarded native transactions (23 runs, failures included) | 3,468 |
| **Total** | **about 5,877 (98 minutes)** |

- **Target and ceiling:** the 90-minute target was exceeded; the 120-minute ceiling was not.
- **Why native work stopped:** the remaining about 22 minutes cannot fit the RU-16 scope's implementation, build and
  three or more fresh processes.
- **Outside the ledger:** implementation and writing time, and restoration.
- **Separate earlier ledgers:** about 1,600, 920, 800 and 1,110 seconds.

Focused results on the delivered source: the candidate filter above (33 tests, 0 failures). The OFF admission test
passed: two exact pairs admitted, and 15 foreign and 8 cross-paired requests rejected. The complete domain suite and
release qualification are deferred.

## History: earlier ledgers


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
