# Sprint 32 — Satyr and Lillend Azata

Owner Howie Reith; mission2026-10-25. Sole active operational record.
Finish at an independently reviewed, natively validated PR. No merge, release,
Sprint33, unapproved subsystem or owner playtest dependency.
[Validation policy](../docs/VALIDATION-POLICY.md) ·
[Append-only evidence/history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md) ·
[Draft PR74](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/74).

## Scope, baseline and ownership

Accepted intake master `48e6fb4abfc154f111a4019a806cb79a4184bd1c`.
Branch `codex/expanded-summoning-sprint32-satyr-lillend`; worktree
`C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/.worktrees/expanded-summoning-sprint32`.
HOWARD-LAPTOP owns mechanics/shared gameplay integration. DATA's released-creature
art/worktrees remain untouched. Review relevant accepted dependency changes at
one coherent checkpoint before freezing final source.
Charter v1.0 sections3/4/7 and S32: Satyr SNA IV-IX; Lillend SM VI-IX;
quantities1/1d3/1d4+1; dependencies5,29-31. Preserve normal actions, independent
resources, ownership, duration, cleanup, stable GUIDs and save compatibility.

## Accepted decisions

- Execute native/technical visual acceptance unattended; owner aesthetic approval
  remains separate. Functional-first cosmetic debt cannot waive technical defects.
- Conditional Suggestion uses CowWithHat Custom Spells when active and is omitted
  when absent. Accepted adaptation: avoid combat/fascinated until harmed,
  minutes/level with Will/SR; no unrestricted instruction interface is claimed.
- Omit/disclose Countersong, Darkness, Light, Dancing Lights, Ghost Sound,
  Hallucinatory Terrain and Detect Magic. No replacement fog or generic subsystem.
- Approved Lullaby: normal Standard cast,10ft burst, Will/SR,−5Perception/−2Will
  versus sleep, immediate concentration release and CL7 seven-round42s tail.
- Approved Lillend-only noncombat dispatcher: preselected Fascinate≤3,
  Inspire Competence and dependent bardic Suggestion, normal actions, native
 20-round pool, exclusivity, exact-source refusal/lifecycle. Competence/Fascinate
  have early native proof; final coverage remains incomplete.
- **Approved 2026-10-26:** implement the Lillend-only memory exception for bardic
  Suggestion on this exact Lillend's fascinated target. Bounded exact-cast context
  transport into the hostile-memory handler must preserve normal casting,
  Will/SR, stealth, invisibility and all other offensive reactions. No general
  memory/AI overhaul or persistence manager is authorized. Estimated3–8active
  hours uncertain. Owner answered "Implement the Lillend-only memory exception";
  implementation is present; native qualification remains pending, with no further owner
  permission needed inside this exact boundary.
- **Approved 2026-10-26:** Lillend Hold Person keeps native automatic Will saves
  each round without an action debit. Owner answered "Accept native automatic
  repeat saves"; PF1 voluntary full-round retries are not implemented. No new
  incapacitated-action subsystem. Private source cleanup remains required.
- Exact branch addition to sanctioned push wrapper authorized/applied. Development
  version0.0.167 remains candidate-bound; no release number reserved.
- Guarded Steam640820, leases, current protected intake, immutable baseline,
  named disposable Working/isolated fixtures and exact restoration remain binding.

## Product and current candidate

Satyr: adult Fey8/HP44/CL8, Fear, Charm Person, Sleep, conditional Suggestion,
control eligibility and bow/dagger/secondary-horns fallback. Lillend: adult
Outsider7/HP73/AC20, Large +1 sword/secondary tail, shared grab/constrict opt-in,
bard7 book5/4/2, own20-round Move performances, healing/control, safe Sound Burst,
Lullaby and corrected Daze60s immunity. Ten new public development roots preserve
1150 prior roots. Source profiles: [Satyr](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Satyr),
[Lillend](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lillend).
Native Persuasion/Perception combine printed social/Sense Motive skills; own
Intelligence drives Knowledge(nature). No substitute Survival, Disguise, Perform
or Fly stat. Accepted ordinary-map flight has no separate Hover/aerial system.
[Original body provenance](../assets-source/original-models/sprint32-support/README.md).

Latest validated development candidate: `5a1199de6d581f840536336258c5b506a9ae8310`.
ZIP `6d5571e44ad7f6a28acb95a184d5e2d02517f0e26fed61ba35b05f687e8a0420`;
DLL `709b11028f4eef72323a4e1b7c180deb25838f28c0722c1025e2d673bdf0e127`;
MVID `5a569287-ec79-465d-bdf4-7679f31dddd9`;
SourceState `376d55e1d813bb855f243639afa4e313bf203fe45bd3b2156960179e36193094`.
Path `artifacts/development-candidates/sprint32-lillend-support/5a1199de6d58/`.
Build60: wrappers,557selected/2754registered, exact-reference compilation and
strict package PASS in253.779s. Native69 **15PASS/3FAIL**: all positive bardic
cases pass (saved, SR100 refusal, successful CL7/DC17 effect, same Fascinate,
normal Standard actions, native upkeep only, untouched slots/sibling, global/
aggro/stealth callbacks, exact boundary-injected Invisibility removed). Three
casts remain three through another native round/current-effect refusal.
Ordinary-book negative then enters native Pause at combat entry; the strict
Default lease and its finalizer correctly refuse, and no negative verdict is
available. Outer transaction restores the human intake exactly. The failed
fixture finalizer is not relabeled PASS. Correct the expected auto-pause setting
within the bounded fixture, keeping foreign-mode refusal, then rerun that phase.
Original65-69 failures remain immutable.

Previous source279de1e3/native63RTWP and64turn **13/13PASS each** on their exact ZIP.
Both modes show natural Satyr Sleep/Charm, actual normal Standard costs,
CL8/DC15,480s/28800s applications, four-HD Sleep budget/ally exclusion,
controlled-target refusal and independent resources. Both original effects
clear on native source expiry before their own deadlines; exact separate
sibling effects/deadlines survive. Charm's direct attack faction restores
immediately and its group cache/IsEnemy restores after one ordinary frame.
This confirms the62 observer-order defect;62/61/60/59 remain historical FAIL.
Turn mode used native turns and zero fixture AI ticks. The sibling application
is explicitly effect-only lifecycle isolation, not a second casting proof.

The approved Lillend-only memory exception is implemented. Exact native
controller/provider/ability/own live Fascinate guards and invocation-local cast
context preserve original global then actor-local events and aggro callbacks.
Nonmatching calls delegate unchanged. No ambient flag or persistence ledger.
Independent product review's subclass boundary and fixture memory/HashSet
findings were corrected before65. Foreign Harmony patch inventory remains a
required native guard. The original observer compile failure is retained.

Sourcefd54cde1 contains the shared-description correction, private Suggestion
source cleanup and private Hold/terminal with accepted automatic repeat saves.
Two stable identities append without changing7506 originals. Independent review
corrected an unsupported Hold branch-count assumption before66; the distinct
provider branch was then exposed by actual combined-profile initialization.

Sourced43a72d8 handles only the exact positive Infectious Charms conditional in
the private Hold clone, with an existing native AbilityCasterHasNoFacts guard.
Independent review verified identical native UnitHelper.HasFact membership and
no additional behavior/persistence subsystem. Native67 establishes registration
with the provider profile; the temporary sibling discovery availability probe
ran before the observer exception but has no recorded verdict and must rerun.

Sourcefa5614e0 fixes only the compiled patch-inventory observer. The
installed Harmony12 shim's global GetPatchedMethods list is checked before
GetPatchInfo, reusing the existing CotwSpontaneousSlotCompatibility pattern.
Missing methods and any registered patch still refuse, including patch kinds
not represented by Harmony12 owner lists. Exact installed IL retained. Focused
production23.2s/18tests0.9s PASS. Native68 confirms this correction; both inspected
methods have no foreign patches. The later invisibility case remains incomplete.
Native67 captured Fear's Shaken1round/FrightenedCL-round spell terminals, neither
with source cleanup; private mappings remain required. Sound Burst's one-round
Stunned is explicitly IsFromSpell=false and an instantaneous-spell consequence;
do not add caster-loss removal merely to clear a fixture. Hold's three native
applications include the main effect and two discovery helpers, now excluded
privately under the exact caster guard. Metadata is not gameplay evidence.

The private stateless control destruction adapter has actual Satyr removal and
sibling-isolation proof; retained native area-load handling still needs cold proof.
Affected Lillend book/SLA/cantrip, death and fresh-load cases remain required.
No owner decision is pending.

## Development evidence and reuse limits

All executions retain original source/observer/package identities in history.
A source or compiled observer correction invalidates its affected native case.
No earlier execution below is called fresh on the private-control correction.

| Evidence | Actual result and remaining limit |
|---|---|
| 5a1199de/native69 | 15PASS/3FAIL: all positive bardic saved/resisted/successful/current-effect-refusal cases PASS, same Fascinate/actions/budgets and native offensive reactions. Ordinary negative enters Pause; strict lease/finalizer refuse, outer transaction restores exactly. Negative remains unverified. |
| fa5614e0/native68 | 15PASS/1FAIL: Hold discovery guard, natural saved/high-SR bardic attempts, exact Fascinate/actions/slots/upkeep and global/aggro callbacks PASS. Retention fails after the subsequent invisibility setup; successful effect and remaining checks unverified. |
| d43a72d8/native67 | 12PASS/1FAIL: provider initialization, exact metadata and natural Fascinate PASS; patch-inventory observer null conversion throws before bardic readiness/casting. Fix reuses proven global-patch enumeration; rerun required. |
| fd54cde1/native66 | Bootstrap FAIL before gameplay: CotW Infectious Charms conditional unsupported in private Hold mapping. Exact bounded mapping correction unbuilt; no assertions executed. |
| 096b045c/native65 | 5PASS/2FAIL: provider binding localization collision before natural bardic cast. Original result retained; shared-description correction unbuilt. |
| 279de1e3/native63,64 | RTWP13/13 and turn13/13PASS: natural Sleep/Charm, costs/durations/refusal, expiry and exact sibling-effect isolation. Charm group cache restores one ordinary frame after direct faction restoration. |
| 61ba8681/native62 | 12PASS/1FAIL: both original private effects removed and exact sibling effects preserved. Sleep restored; Charm hostility sampled before verified cache propagation. Observer-only bounded-wait correction pending native proof. |
| 158147c0/native61 | Natural casts/durations PASS with private effects; both source-expiry rows FAIL. Native component exists on real buffs but misses destruction ordering. Stateless exact-event correction and sibling-effect probe are unbuilt. |
| ef3307a2/native59 | Natural Satyr Sleep/Charm, costs/eligibility PASS; both source-expiry rows FAIL. Sleep480s, combat Charm60s. Exact effects remained active after caster disappearance while targets/sibling survived. Private duration/lifecycle correction invalidates these affected cases. |
| 49786548/native57 | Fascinate24/24PASS: natural Move,20-to19-to18, WillDC17/noSR, cap3/frozen targets,24h source marker deadline, ShakeFree, restart refusal, actual0.9409m approach/one save each, visible CureLight threat/slot5-to4, sibling isolation, exact marker/helper/area/command cleanup. No24h expiry/cold/bardic proof. |
| a333ab3f/native55,56 | Fascinate13/13 and Competence12/12PASS; natural brain refusal/activation, +3recipient, audible maintenance and terminal recipient-loss cleanup. Native54 original11PASS/2FAIL remains; corrected reader waited native carrier retirement. Terminal7.699m is later movement, not continuous out-of-range residence. |
| 7aed020a/native53 | Cantrips7/7PASS: natural Lullaby modifiers/refusals/original42s deadline; Daze twice with actual60s marker expiry, cross-source refusal and native AI cooldown1-to0. RTWP only. Earlier48/51 expiry flag was insufficient; original failures retained. |
| bcd41d04;51cf0e58 | Lifecycle15/15 and support13/13PASS: native expiry/death, external Courage/sibling cleanup; natural self/party-ally healing18-to0, ownL3slots2-to1-to0, untouched sibling, Standard and exact free first touch. |
| 4b509d67 native input | Wizard16/16, Sorcerer28/28, Druid31/31PASS: actual visible-menu/target prepared/spontaneous/conversion paths, legal1/d3/d4+1, real slots/actions,120s, body/icon and exact reset. No supplied casts or manual command ticks. |
| 2d90 RTWP12/12;turn10/10 | Natural Courage, real ally+2/enemy exclusion, healing, useful fallback, completed sword/sword/tail mechanics, live Nature and sibling isolation. No full animation/contact claim. |
| Earlier Satyr/optional provider | Fear, immunity refusal and bow/dagger/horns fallback in both modes. Natural provider Suggestion DC17/CL8 Satyr and DC16/CL7 Lillend, normal actions/duration/own book slot/current-effect refusal. Positive Will/highSR, break/expiry/provider cold remain NOT_RUN. |
| Bodies | Independent Lillend bindings,62owned objects released/158borrowed retained, recognizable rest views. Full gameplay-camera movement/contact/hit/death/despawn/performance still pending. |

## Finite acceptance contract

Every final row remains incomplete. Group related observations; add a case only
for a concrete changed risk. Final qualification uses frozen source/assets/
observers/publication inputs, one complete cheap gate and its exact ZIP, plus
separate records worktree where identity guards require. Do not rebuild for prose.

| ID | Expected observable result | Existing route/evidence | Mode/profile | Invalidated by |
|---|---|---|---|---|
| A1 | Correct adult identity/defenses/attacks/facts/spells; legal tiers/quantities, prior routes preserved | Domain catalog/routes; native composed profiles/inventory | Offline; standalone | Catalog/builders/registration/adapters |
| A2 | Player summon pays actual action/slot, legal count/duration, usable body/icon | Native input/route fixture; slot/spawn/expiry receipts | Standalone; materially distinct prepared/spontaneous/conversion | Publication/cost/quantity/duration/adapters |
| A3 | Useful Satyr control without pipes, immune/ineligible/current-effect refusal and ordinary fallback | Exact-owner AI/commands/saves/effects | RTWP/TB | Satyr rules/targeting/shared AI/actions |
| A4 | Useful safe Lillend buffs/performance/healing/control and ordinary fallback; normal independent actions/slots/pools | Same bounded support/performance/cantrip fixtures | RTWP/TB | Book/performance/dispatcher/AI/action handling |
| A5 | Expiry/death/dismissal/cancellation remove owned effects/items/commands, preserve siblings | Existing lifecycle/native scheduler; exact captured instances | Standalone; affected modes | Lifecycle/equipment/resources/effects/commands |
| A6 | Fresh-process state/budgets/ownership survive, moduleOFF safely deserializes, original save restored | Existing Working prepare/cold verify-cleanup/fresh absence ON/OFF | Required installed stack/provider seam | Serialized state/registration/lifecycle |
| A7 | Technical gameplay-camera loading/selection/scale/movement/contact/hit/death/despawn/performance | Existing presentation captures + structured native facts, agent inspection | Standalone/direct combined seam | Assets/rendering/attachment/loading/lifecycle |
| A8 | Standalone and directly affected high-risk combined/shared consumers work | Existing profile transaction/focused baseline and integration cases | Standalone/high-risk combined; additional only for concrete risk | Shared AI/registration/serialization/integrations |

Concrete remaining cases: Lillend Charm Monster
(nonhumanoid/protection/duration/break/lifecycle), safe Sound Burst, real tail
hit/grab/immediate and maintained2d6+liveStr constrict/release with one affected
Salamander baseline; performance combat-entry/TB boundaries; Suggestion positive
Will/highSR/break/expiry/provider persistence; new serialized performance/book/
cantrip state ON/OFF; technical presentation and final integration. Bardic
Suggestion rows remain pending native validation of the approved exception.
Private Hold Person requires real initial Will/SR, automatic repeat-save/action
observation and source-loss/sibling lifecycle checks under the accepted adaptation.
No whole-roster replay selected.

## Restoration, headroom, work and next action

Latest transaction `compat-20261026T151119Z-587d56ba9095` restored/verified
15:17:21.8037457UTC. Protected Compare PASS967Mods/118saves/18references/
83settings/tasks0/one unchanged Steam. No owned live transaction/game/task.
Native69 raw directory and exact identities are in append-only history;
local originals/intake: `artifacts/sprint32-bardic-5a1199de/`.
Original failures remain immutable; no product-stat changes supplied a verdict.

Counts:60Build-Local attempts /49validated development packages /69native launches.
Build60 measured4.23min; native69 wrapper6.07min/native186.083s. Whole-mission
exclusive work-category totals unavailable. Build/native timestamps retained;
independent control-fixture planning overlapped unattended work. No owner waiting
or speedup claim. Recent distinct causes: null patch-info conversion67, multi-
frame invisibility setup68, combat-entry Pause69; no unchanged repeated launch.
Positive native product evidence advanced in68 and69.

Disk budget:1.1GBcombined peak +2GBreserve before building. Prior cleanup
receipts remain in history. At12:42UTC four completely byte-verified closed
package extractions reclaimed398,214,056B; free3,453,100,032B, protectedComparePASS.
At13:32UTC eleven old closed-transaction compile extractions, each fully byte-
verified against its retained compile ZIP, reclaimed277,865,125B; free3,374,972,928B.
Protected Compare PASS; all archives/bin/PDBs/receipts remain. Plan SHA
`06c21db0c455ea9e978f7992a95e13abd9163dd04855a1795845407e015ad37a`.
Unique packages/raw evidence/original failures/receipts/PDBs/saves/backups retained.
At14:03UTC eleven verified old duplicate DLLs plus the two restored65/66 package
extractions reclaimed390,974,359B; free3,393,720,320B, protectedComparePASS.
Unique archives/PDBs/raw evidence/receipts/saves/backups retained.
At14:32UTC four verified old build copies and closed67 package extraction
reclaimed185,276,688B; free3,222,036,480B, protectedComparePASS.
At15:04UTC seven already closed v160 build payload/DLL copies and the restored68
package extraction reclaimed348,596,555B; free3,207,180,288B, protectedComparePASS.
All retained ZIPs/PDBs/raw failures/receipts/saves/backups remain.
At15:46UTC verified closed build/extraction copies reclaimed360,725,277B;
free3,208,466,432B, protectedComparePASS. Exact plans are in history.
No machine-wide cleanup or DATA changes.

The bounded observer correction is independently reviewed: native Invisibility
is applied only at the natural exact bardic effect boundary, with pre/post own-
Fascinate guards, original handler retained even on caught setup errors, exact
buff removal and separate native stealth callback receipts. It does not claim
sustained invisible performance or an entered-stealth exit. Product unchanged.
Focused production24.5s/18tests0.9s PASS after correcting a missing namespace;
original compile failure retained. Review requested two diagnostic fields, added.

**Next:** commit/package the reviewed coherent private Fear, Hold fixture and
negative-case auto-pause batch. Production24.3s and570selected tests16.1s PASS;
independent review findings resolved, including non-vacuous source attribution
and cleanup after failure. All7508 prior identities unchanged; two private Fear
terminals appended. Selected native cases: satyr-control and satyr-turn (Fear
expiry plus existing neighbors), lillend-hold (actual cast/retries/expiry),
lillend-hold-turn (native turn cast/expiry), and provider bardic negative rerun.
Each uses its fixed save-free phase/runtime600s, planned8-10min per selection;
run against one coherent package, restoration between processes. The uncertain bardic
natural execution seam now has useful positive/refusal proof. Native negative
completion still requires rerunning its affected provider phase, expected10min/
runtime600s; do not treat that pending row as closed. Preserve the strict Default
lease and restore the captured native setting. No spotting/memory/action/roll
may be supplied to rescue a verdict.

Then continue affected Lillend controls/lifecycle, grab/constrict, cold
ON/OFF, technical presentation and final integration. Reuse the executed
S30-31 installed-stack original-Working envelopes for persistence.
Hold Person cleanup mapping needs native proof; Fear/SoundBurst terminal
components and deeper Hold branches are captured in67; Fear cleanup remains
required in the next control batch after the uncertain bardic execution seam. Invisibility is self-only.
Final frozen candidate qualification remains separate required work.

Prior v0.0.167 gaps stay **NOT_RUN**: all30public-route/body/icon, standalone
final integration, highest-risk combined final integration. [Original dispositions](../docs/SPRINT30-31-COVERAGE.md)
are not a Sprint32 waiver and have not been closed by these focused cases.
