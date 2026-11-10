# Spider Summoner integration: status

**Implemented with verification pending; review corrections applied.** The full gameplay scope is implemented.
The corrected candidate passed its focused native phase (153 PASS, 0 FAIL), but named acceptance observations
remain open (table below), and one design question needs owner review. This is not mission completion and not
release qualification. No merge, release or permanent installation has been performed.

Baseline and its original result, unchanged: [BASELINE.md](BASELINE.md), including the `FAIL, 70 PASS / 1 FAIL`
result on candidate `efc75b8cc0b0`.

## Current candidate and runs

| Item | Value |
| --- | --- |
| Source | `8c1734743cce959dac5f2fbf2075967769b2c01e`, v0.0.181 development, branch `codex/races-unleashed-summoning` |
| Package SHA-256 | `919d97530f8fade789fe25a253c8b5fe1ff46c660c89a2daf9cef772107ba250` |
| DLL SHA-256 | `aa2fd33616ffce06e96612c1798aabc7e48db5a27e60aa130bd4a9fb900a5f17` |
| MVID | `934bb8d1-e946-4609-ad4d-40ac584f8e9c` |
| Purpose | Development candidate `races-unleashed-summoning`; never a release |
| Run 1 | `20261110T0227019018671Z-bc2097ae0f9e4f97abd4b474eb845027`: **FAIL, 64 PASS / 4 FAIL** |
| Run 2 (matched rerun, unchanged inputs) | `20261110T0231072443394Z-5067b15e071b480b8fd2012b3bedad76`: **PASS, 153 PASS / 0 FAIL** |
| Profile | The current DATA combined installation. Races Unleashed 1.0.11, DLL `6d18168c...4cb0`; Call of the Wild loaded |
| Restoration | `verified` after each run; live tree `5C3E4AD4...BFBB`, `Params.xml` `E44F4495...30BB` |

**Run 1 failure, retained.** The phase stopped in the cleric cell `cleric-foreign-legal`, before any spider cell,
when the popup-teardown guard refused a tooltip context that had no ability and no provable owner. In the earlier
passing run the same teardown found a tooltip owned by the hovered row. The cell's code is unchanged, so this is
classified as an incidental hover race caught by the safety guard, not a product defect. One matched rerun was
made under the policy's single-reproduction rule and passed. Run 1 still established that the live Ogre and
Tarantula rows classify as `verified-repaired-spider-contract` under the stricter contracts.

Earlier candidates on this branch are retained, not replaced. Their evidence stays bound to their own bytes:

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

## Design question for owner review (RU-13)

Without Call of the Wild, the native CMB escape branch ignores every web DC. The +2 applies wherever the game uses a
web DC, which is exactly once. Making the +2 reach the CMB branch would need a new rule, such as +2 to the spider's
CMD for that escape maneuver only, scoped to feat spiders and without changing the shared buff.

That rule would be a tabletop adaptation, so it is not implemented. Recommendation: keep the current hook, which
applies the bonus wherever the game reads a web DC, unless you want the scoped CMD adaptation for profiles without
Call of the Wild.

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
| Per-creature benefit action and DC owner | `Summoning/SpiderSummonerBenefit.cs` |
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
  Ogre would then keep its v0.0.181 cleric classification but lose its v0.0.181 Herald projection.

## Acceptance status

Native entries name their candidate. Focused evidence means the named domain tests; those are not native observations.

| ID | Implementation | Focused evidence | Native evidence | Candidate | Remaining work |
| --- | --- | --- | --- | --- | --- |
| RU-01 | Done | Contract and matrix tests | All 8 rows repaired from exact inputs, including the donor application comparison: `repaired=8;generated=38;problems=` | `8c1734743cce` | None |
| RU-02 | Done | Input and shape negatives | An original row and a generated row, each obtained from the real popup with the feat and executed after its removal: refused, no spend, no spawn. A fresh no-feat choice is refused. The ordinary Giant Spider casts without the feat | `8c1734743cce` | Natural Drow feat acquisition (the fixture grants the exact feature) |
| RU-03 | Done | Matrix tests | Four Summon Monster originals cast as the named unit; repaired Ogre under evil and non-evil casters | `8c1734743cce` | **Four Summon Nature's Ally original casts** |
| RU-04 | Done | Every matrix cell; quantity expression | 46 of 46 placements in the census; 1d4+1 and 1d3 Summon Monster casts | `8c1734743cce` | Representative Summon Nature's Ally quantity casts |
| RU-05 | Done (donor rank preserved) | Quantity expression rejects a fixed bonus | None | none | **Superior and Augment Summoning native cast** |
| RU-06 | Done | Selection and band policy | Celestial on all four species; fiendish on the Ogre and Tarantula; correct Mid band, no leftovers | `8c1734743cce` | Native fiendish on Crab and Black Widow; LN and CN focused only |
| RU-07 | Done | None new | Neutral toggle read at execution, off and on; no Summon Monster template on Summon Nature's Ally rows | `8c1734743cce` | Toggle changed with a menu open; Summon Nature's Ally cast under both toggle positions |
| RU-08 | Done | Repaired Ogre judged as spider | LG/Erastil, LE/Asmodeus, TN/Pharasma casts; deity-derived control; repaired Ogre offered to LG in both routes | `8c1734743cce` | None for the restriction itself |
| RU-09 | Done | Membership tests | Non-cleric provider control | `8c1734743cce` | **Real multiclass book; ordinary spontaneous SM and SNA pool casts** |
| RU-10 | Done; admission now requires a repaired row | Family-route and agreement tests | Repaired Ogre and Tarantula in Herald tier V and VII menus | `8c1734743cce` | **Herald spider casts, including a quantity tier, debit and metamagic**; Crab I and Widow IV admission |
| RU-11 | Done; admission now requires a repaired row | Family-route and agreement tests | None beyond census | none | **Druid conversion and prepared SNA comparison and casts** |
| RU-12 | Done; consumers traced | Benefit scope | Poison DC base+2 in the resolving context, stable on recalculation (ordinary Giant Spider 12 to 14; 12 without the feat) | `8c1734743cce` | **Actual first and continuing saves from isolated exposures; inherited Hunting poison; Augment Constitution change; reload; ineligible-source controls** |
| RU-13 | Hook implemented; effect-level correctness open | Benefit scope | Loaded consumer in this profile is Call of the Wild's replacement; webbed context DC 12 to 14 with the marker (structural) | `8c1734743cce` | **Owner decision above; a native web hit and escape roll here and in a valid profile without Call of the Wild** |
| RU-14 | Done | Exact identity negatives | Ordinary Giant Spider with and without the feat | `8c1734743cce` | **Reload**; native ineligible-source controls |
| RU-15 | Done | Display order tests | Real popups exact, sorted, no duplicates, including spider rows on the cleric's menus | `8c1734743cce` | SNA and Druid rendered menus; caster without the feature in those routes |
| RU-16 | Done | None | None | none | **Fresh process, saved shortcut, reload** of an original and a generated row |
| RU-17 | Done | Module boundary | None | none | **RU-absent and module-off profiles**, after confirming each profile's dependencies |
| RU-18 | Reopened by review, corrected | Agreement tests: unfamiliar and misplaced variants components, empty after-spawn, changed application, reordered or extra actions, unreadable donor | Zero unresolved rows; all 8 live rows accepted by the stricter contracts | `8c1734743cce` | None known |
| RU-19 | Done | Observer compiles | Stale-selection cells corrected; old failure records retained | `8c1734743cce` | None |
| RU-20 | Done | n/a | Draft PR updated, restoration verified | `8c1734743cce` | **NPC-without-feat selection/AI control**; owner review |

## Next bounded acceptance plan

The design question above should be answered first, because one answer changes code. Then build one candidate and
run each open item as an explicit scenario, reusing existing fixtures where they exist:

| Open IDs | Scenario |
| --- | --- |
| RU-03, 04, 07, 11, 15 | A Drow Druid with a prepared book: the four SNA original casts, one 1d3 and one 1d4+1 cast, the Druid conversion menu and a cast with its debit, and SNA under both toggle positions |
| RU-09 | A multiclass caster and an ordinary spontaneous SM and SNA book: pool casts and debits |
| RU-10 | The Herald cleric: Crab I and Widow IV admission, one quantity-tier spider cast with debit, and one metamagic cast |
| RU-05, 12 | Superior and Augment Summoning on one quantity cast; isolated poison exposures for the first and a continuing save, including the inherited Hunting poison and Augment's Constitution change |
| RU-13 | A web hit and one escape roll here; then the same in a profile without Call of the Wild whose dependencies are confirmed valid |
| RU-02 | Natural Drow acquisition of the feat through level-up |
| RU-14, 16 | Save, fresh process, reload: an original and a generated row from saved shortcuts; marker and DC after reload; ineligible-source controls |
| RU-17 | RU-absent and module-off profiles, after their dependencies are confirmed |
| RU-20 | An NPC without the feat: selection and AI never pick a gated spider row |

## Validation ledger (this correction task: 15-minute target, 30-minute ceiling)

| Step | Seconds |
| --- | ---: |
| Focused gates: 31 selected, then 785 related | about 40 |
| Candidate build `8c1734743cce` | 422 |
| Guarded native run 1, FAIL (hover-guard refusal) | 173 |
| Guarded native run 2, matched rerun, PASS | 283 |
| **Total** | **about 920** |

The 900 s target was slightly exceeded; the 1,800 s ceiling was not. The previous task's ledger (about 1,600 s)
is unchanged and separate. The remaining observations do not fit, so they are reported rather than launched.

Focused results:
- **Candidate filter:** `summon-cleric-alignment;summon-spider-summoner;expanded-summoning.display-ord;druid`, 31
  of 2,918 selected, 0 failures. The candidate build recorded this.
- **Related neighbors:** `expanded-summoning;alternate-summoning;summon-;druid;manifest`, 785 tests, 0 failures, on
  the corrected source. This covers the shared signature model.

The complete domain suite and release qualification are deferred.
