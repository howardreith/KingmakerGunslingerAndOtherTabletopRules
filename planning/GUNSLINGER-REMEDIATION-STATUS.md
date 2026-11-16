# Gunslinger remediation: current status of the records worked so far

This is the current-status layer for the Gunslinger remediation epic of 2026-11-15
(`Gunslinger_Remediation_Epic_2026-11-15.docx`, kept in the lab directory, outside the repository). It covers only
the records that have been worked or touched. It is **not** the full register of all fifty inventory records that
story S0 T1 asks for; the other records keep their epic status until someone works them.

The frozen audit in `planning/release-audit-0.0.182/` and the older reports are history. They are not rewritten here.

States are the epic's: *investigating or blocked*, *implemented*, *native verified for stated scope*, *merged*,
*released*, *disproved or intentional*, *retained limitation*. Evidence and delivery are tracked separately.

## Baseline as of 2026-11-15, end of day

The epic was written when v0.0.190 was the latest release. Since then:

| Change | Merge | Release |
|---|---|---|
| PR #164: effect links on the eight named Elven Branched Spear buffs, Boughkeeper's icon | `30e9fdf68b` | v0.0.191 |
| PR #165: spear icon on the three visible spear penalty buffs | `ce3d0be334` | v0.0.191 |
| PR #166: grit UI recipient contract (reviewed head `e99888c4ad`) | `b8ba915d36` | v0.0.191 |
| PR #167: v0.0.191 release opening | `3e13965d9d` | v0.0.191 |
| PR #168: v0.0.191 publication record | `a7c34718f4` | records only |

v0.0.191 was published on 2026-11-15 from release commit `3e13965d9d` (package SHA-256
`c09ca6255feefe6c547d051ab3cf3ead8bc918402d7488f077efa0ac5d2e7fbb`, DLL
`7c3b0324f40e32b0a476c24e9e66a63a2ecefd8908dc3902bed8ec81a5d87287`). Its check on the published bytes in the game
completed: three sessions passed (`docs/BUILD-AND-RELEASE.md`). The epic's "no release from the spear merge" note is
superseded by the owner's later instruction to cut v0.0.191.

## Records

| Record | Subject | Fix | Native evidence | Merged | Released | State and limits |
|---|---|---|---|---|---|---|
| DEF-01 (S1 T1) | Description getter aborts initialization | PR #166: explicit recipient list, no description read, no library scan | Development candidate `4acb7055c9` and the published v0.0.191 bytes: `gunslinger-grit-ui-recipients` (`20261115T191036Z`) and `gunslinger-grit-spending` (`20261115T191716Z`), with v0.0.190 failing the same launches (`docs/GUNSLINGER-GRIT-UI-RECIPIENT-REPAIR.md`) | `b8ba915d36` | v0.0.191 | **Released; native verified for stated scope.** The reporting installation was not reproduced; null text is a controlled stand-in; lab loader is 0.33.0, the report's 0.33.3. Still disclosed: three hidden summoning abilities without a stored description; Dodge had no passing spend check in that record (see DEF-02 below for new evidence); `up-close-critical` damage intermittent, unattributed; grit counter on non-spending deeds is a design question |
| DEF-03 (S2 T2) | Boughkeeper buff drew a white square | PR #164: spear sprite | `reports/boughkeeper-buff-icon/NATIVE-UI-CHECK.json`; published bytes: `named-spear-buffs` (`20261115T192229Z`) | `30e9fdf68b` | v0.0.191 | **Released; native verified for stated scope.** Legibility at the portrait slot is the owner's visual decision |
| DEF-02, BUFF-15 to BUFF-22 | Eight named spear buffs without effect links | PR #164: links in the shared spear factory | as DEF-03: real Boughkeeper trigger and expiry, the spear combat scenario, direct application and expiry of all eight | `30e9fdf68b` | v0.0.191 | **Released; native verified for stated scope.** Not run: the seven enemy effects timed after their real triggers, next-round marker reuse in live time, turn-based mode, controller |
| NEW-01 (S2 T4) | Three visible spear penalties without icons | PR #165: spear sprite, owner-directed | `reports/boughkeeper-buff-icon/PENALTY-BUFF-ICONS-NATIVE-CHECK.json`; published bytes as above | `ce3d0be334` | v0.0.191 | **Released; technically verified.** Visual approval of the sprite at slot size is separate and the owner's |
| DEF-02, BUFF-01 to BUFF-14 (S2 T1) | Fourteen deed and Eastern buffs without effect links | Branch `codex/def02-buff-effect-links`, draft PR #169: `UseEmptyBuffEffects` at eight construction sites (`a479f87f59`) | Candidate `96dbf78fac0c` against the published v0.0.191: `docs/GUNSLINGER-EFFECT-FREE-BUFF-LINKS.md` | no | no | **Implemented: all fourteen have the links, read in the game. Native verified for stated scope, which differs per identity.** Complete through the real trigger in real-time play: Dodge, the four bleeds (including Stop Bleeding), Startled, Targeted Head, the Mountain-Sunder marker. Partial: Up Close and Deadly (damage not judged), Falling Petal and Deadeye Armed (icon check fails), Steady Aim Armed (consumption check fails, NEW-03). The two shells: direct application only. Open for all: baseline real triggers for four identities; turn-based, controller, save and load, other stacks; the shells in an old save |
| RISK-07 (S2 T3) | Suggestion buff's final effect links | none | Provider absent: both links present (empty), read in the game. Native donor has a start effect | no | no | **Blocked.** CowWithHat Custom Spells is not on the lab machine; the provider-present final blueprint was not read. Neither confirmed nor disproved |
| INT-03 (S3 T3) | Dead pre-AC Dodge diagnostic; the product's Dodge scenario gave no verdict | Branch `codex/int03-dodge-scenario` (`c014e5c999`), stacked on PR #170: `disposable-gunslinger-dodge` is a working-save scenario that uses the registered ability through the game's ability command; the unused delivery class, attack-time runtime and their diagnostics are removed; `DodgeGritCostCalculator` kept | Candidate `df7f11156a01` in the disposable game copy, run `20261116T012451Z`: the scenario PASSES, 11 of 11. After the review corrections (`15767a89ad`: recast timing in the verdict, abort evidence kept, non-exiting requests refused at admission), candidate `15767a89ad2b`, run `20261116T024032Z`: PASSES, 12 of 12, with the recast times recorded; a non-exiting request is refused by the product, run `20261116T024238Z` (`docs/GUNSLINGER-DODGE-SCENARIO.md`) | no | no | **Implemented; native verified for stated scope. Development item closed: the review of PR #171 at `7c4cb8ad44` accepted the three corrections (recast timing, outer-abort evidence, exit requirement at admission).** Observed through the real ability in the loaded working save: grit 5 to 4, armor class 10 to 12, incoming attack 10, 12, 10, attempted command refused while active and at 0 grit, unaided end at 6.02 s, recast isolation with all prerequisites. Gameplay unchanged; registered blueprints, the legacy armed marker and the expiry guard kept. Not run: turn-based, controller, save and load, True Grit in the game, the live guarded launcher on the owner's installation, the comprehensive aggregate after it lost its Dodge slice. The legacy-marker cleanup was already unreachable (since `93193c5170`) and is gone with the runtime; the tabletop reaction policy (`GunslingerDodgeService`) remains, uncalled |
| INT-04 (S3 T4), GS-AUD-0045 | The firearm visual-rig check was pinned to 14 items, so with 16 no binding was inspected | Branch `codex/int04-int05-presentation-diagnostics` (`baad52d579`), stacked on PR #171: the expected sixteen come from a declared roster; the census and every item's binding are separate assertions; unreadable and unmapped inputs are reported | Candidate `35049ef6fde7`, run `20261116T031851Z`, against PR #171's candidate (`20261116T030922Z`): roster 16 of 16 and all 16 bindings pass, the first run in which the bindings were inspected (`docs/GUNSLINGER-PRESENTATION-DIAGNOSTICS.md`) | no | no | **Implemented; native verified for stated scope.** The scenario still fails on `pistol-authored-marker-contract` alone (REP-03). No failing census or binding was produced in the game (domain tests). Not run in the owner's mod stack. The 27 progression firearms' visuals are read by no scenario |
| INT-05 (S3 T5), GS-AUD-0046 | The item presentation check was pinned to 55 weapons and 56 items, so with 57 and 58 the policy-enchantment check never ran | Same branch and commit: expected identities from the declared rosters and the manifest; weapon census, item census, enchantment census, policy enchantment and text are separate assertions, then the aggregate | Same runs: `observe-gunslinger-presentation` PASSES 14 of 14 (57 of 57, 58 of 58, 12 of 12; the policy enchantment is among the weapons' enchantments with blank text; no text issue); it failed 1 of 9 on the baseline | no | no | **Implemented; native verified for stated scope.** The policy condition, unverified until now, holds. The 12-enchantment expectation is unchanged. Not run in the owner's mod stack |
| REP-03 (S3 T8), GS-AUD-0047 | `pistol-authored-marker-contract` reports `exact=False` | none; the assertion now reports each sub-condition, verdict unchanged (`baad52d579`) | Run `20261116T031851Z`: on both the Duelist and Last Word prefabs the four markers are present once each and there is no support-hand target; the `Muzzle` is at local (-0.1483, -0.2357, 0.0521), expected (0, 0, 0.264). The assertion fails on the baseline and on the candidate alike | no | no | **Investigating.** The failing condition is identified: the muzzle position, the same on both prefabs. Whether the prefabs or the expected position is wrong is not established. Not caused by either census |
| REP-02 (S4 T2) | `up-close-critical` damage assertion intermittent | none | New, grit only: armed through its real ability on a Pistolero, nothing spent at arming, exactly one grit after a hit, unused state ends after a round with no charge | no | no | **Investigating.** Damage not judged; the assertion was not run or attributed |

## Findings added by this work

| Finding | What | State |
|---|---|---|
| NEW-02 | Deadeye spends grit and arms nothing (published v0.0.191; the apply-buff action's duration had no dice count) | **Implemented on the branch (`596285cb0d`); native verified for stated scope**: arms, is consumed by an eligible shot, ends unused after a round with no refund. Not merged, not released. Touch-AC effect through the real ability not judged |
| NEW-03 | Steady Aim's armed state is never consumed by a shot and never grants its 10 feet (handler read the attack roll before it existed) | **Implemented on branch `codex/new03-steady-aim-attack-roll` (`28836aaaea`), stacked on PR #169; native verified for stated scope.** Candidate `2ee86e0f6a6e` against PR #169's candidate, same harness: through the real ability and the game's attack command, a musket shot at 43.2 ft takes ordinary armor class unarmed and touch armor class armed, the state is removed once on a hit and on a miss, grit is unchanged, the next shot is ordinary, a shot after grit is lost uses the state with no bonus, the unused state ends after a round (`docs/GUNSLINGER-STEADY-AIM-RANGE.md`). Not merged, not released. Not run: turn-based, controller, save and load, other stacks, Blunderbuss and Rifle in the game, the True Grit exemption in the game, the published package |
| NEW-07 | The game's attack command measures the musket's 40 ft range edge to edge; the firearm armor-class rule measures centre to centre. An unarmed musket shot ordered from the edge of its range against a Medium target is resolved at about 43 ft, past the first increment | **Open; design question.** Observed on both builds. Not changed |
| NEW-08 | With Steady Aim and Deadeye both armed inside the lengthened first increment, the shot uses up Deadeye's state and its grit while Deadeye authorizes nothing | **Design question** (Deadeye pricing). Observed on the NEW-03 branch. Not changed |
| NEW-04 | Six visible buffs have no icon: Deadeye Armed, four Bleeding Wound bleeds, Falling Petal | **Open.** Needs an art decision and an authorized catalog change |
| NEW-05 | Deadeye reports itself available while armed (observed, availability check only). From source a second use would spend another grit and replace the state; that was not run | **Design question.** Policy unchanged |
| NEW-06 | The product's deed scenarios add armed buffs directly and call runtime hooks on units with no view, so they cannot see activation or real-ability defects | **Open coverage gap, closed for Gunslinger's Dodge only** (INT-03: its scenario now uses the registered ability in a loaded save). The other deed scenarios are unchanged; the loader-compat `kmgDeedLifecycle` probe covers the real triggers used so far |

## Next bounded items

This cut's two remaining checks were finished in the review follow-up (Stop Bleeding on all four bleeds; the Deadeye
step with the corrected expectation).

NEW-03 was then worked as its own cut on a branch stacked on PR #169 (`docs/GUNSLINGER-STEADY-AIM-RANGE.md`).

INT-03 was then worked as its own cut on a branch stacked on PR #170 (`docs/GUNSLINGER-DODGE-SCENARIO.md`), and
closed as a development item when the review of PR #171 accepted its corrections.

INT-04 and INT-05 were then worked together on a branch stacked on PR #171
(`docs/GUNSLINGER-PRESENTATION-DIAGNOSTICS.md`).

Sprint 3 stories not yet worked: S3 T1 (summoning publication entries), S3 T2 (Brown Fur publication), S3 T6 (the
Gunsmith Kit manifest note), S3 T7 (spell family and tier coverage) and S3 T8 (the named pistol asset condition).

1. S3 T8 / REP-03: the failing sub-condition is now known (the muzzle position on the Duelist and Last Word
   prefabs); decide whether the prefabs or the expected position is wrong.
2. NEW-04, icons for the six visible buffs, once the art choice is made.
3. RISK-07 with its provider; the broader compatibility and save-and-load work.
4. NEW-05 and NEW-08 (Deadeye's re-arm rule and pricing) and NEW-07 (range measurement), each a decision before any
   change. Up Close and Deadly's damage is still not judged.
5. NEW-06 for the remaining deed scenarios, which the Dodge scenario's working-save pattern now shows how to do.
