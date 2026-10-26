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
- **Approved 2026-10-26:** retain native hearing behavior and disclose the deafness
  limitation, including verified optional CotW deafness facts. No hearing adapter
  or new hearing/deafness subsystem is selected. Owner answered "Accept native
  hearing limitation"; native deaf-performer failure and deaf-target exceptions
  are not claimed.
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

Latest validated development candidate: `71fc39041a5e0f46b9a2fc34cb77fc1744e65b00`.
ZIP `de7ad0e381cdea26cc5e426e6de07563b55613c662ce47b7e81f22c49e67ea97`;
DLL `415cca34f03d322bb1e60bffd35b17f8c35c826460c12e4d6115ac0f94733e33`;
MVID `f7dafcfd-04e8-4adc-8fe6-990ac88546c7`;
SourceState `ade71561b56d9e5ccbfb0123294ade7a1cda4eed107aefd1210e588145439de5`.
Canonical artifacts/development-candidates/sprint32-lillend-support/71fc39041a5e/.
Build67: complete development wrapper/558selected/exact compile/strict ZIP
PASS265.430s, with no deployment during build. This is not final qualification.
Build65/66 metadata admission failures remain in history. Current Animal fixture
correction is newer than this package and requires a new compiled observer.

Native74 Hold **9/9PASS**: natural CL7/DC16/42s casts and real L2 slots,
accepted automatic failed/successful repeat saves, native source expiry removing
paralysis before its deadline, and exact sibling-effect/budget preservation.
No successful automatic retry explains the source-expiry result. Hold TB remains
pending. Native72 bardic Suggestion remains source-bound **18/18PASS**, including
ordinary-book negative (real L2slot4->3/hostile memory/Fascinate interruption).

Native76 standalone controls **3PASS/1FAIL**, before any control cast: native
Fighter advancement refused the Animal target. Reviewed fixture correction uses
native Animal class/prerequisites, checks existing Animal HD and exact advancement
to five HD. This changes the fixture only; both natural controls remain unverified.

Native77 Working prepare **17PASS/3FAIL**: actual Competence and new bodies pass;
all288 listener candidates fail navigation130 or clamp/separation158. None reach
acquisition/range/sight checks. No save write/stash; cold/OFF NOT_RUN. Original75
had no rejection breakdown and cannot establish its cause. After two geometry
failures, the same finite survey now reuses the proven connected native walking query,
ground anchors and separate horizontal/height bounds. No extra rings or weakened
postspawn guards. Actual-diff review found no concrete dispatch blocker; this
compiled observer correction needs fresh execution. Exact restorations are retained.

Current product includes private PF1 Sound Burst1d8 (nativeD8/D12 input guard)
and stateless exact private Charm caster/apparent-ally threat handling. Grouped
controls cover natural nonhumanoid Charm Monster, real RTWP helpful healing and
caster-attack break, plus safe Sound Burst/Fort/damage/stun/costs in RTWP/TB.
The Animal/ground-query corrections are independently reviewed and compile;
18/2755 focused tests PASS. Native execution remains required.
Nonally/ally threat negatives, wards and additional lifecycle remain pending.

Owner accepted the native hearing limitation after additional research found
optional CotW facts and a possible bounded adapter. No hearing adapter code was
written. The limitation remains explicit; the earlier proposed new hearing state
is not required or authorized by this disposition.
Private saved-state selections/budgets and fresh ON/OFF cleanup remain unverified.
Fixture equipment disposal is not native equipment-expiry proof.

## Development evidence and reuse limits

All executions retain original source/observer/package identities in history.
A source or compiled observer correction invalidates its affected native case.
No earlier execution below is called fresh on the private-control correction.

| Evidence | Actual result and remaining limit |
|---|---|
| f5756a4a/native71–73 | Hold8PASS/1FAIL observer placement; bardic18/18PASS including ordinary-book negative; Working prepare15PASS/2FAIL before save. Details and exact restoration above/history. |
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
Suggestion exception has native72 proof; expiry/break/provider-cold rows remain pending.
Private Hold Person requires real initial Will/SR, automatic repeat-save/action
observation and source-loss/sibling lifecycle checks under the accepted adaptation.
No whole-roster replay selected.

## Restoration, headroom, work and next action

Latest installed-stack transaction `sprint32-stack-20261026T191213Z-40930335b1ca`
and outer Working envelope restored exactly by19:18:51.3958920UTC. Original
Working SHA `650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`,
8,550,225B and original timestamps/attributes restored. Protected Compare PASS:
967Mods/118saves/18references/83settings/tasks0/unchangedSteam. No owned live
transaction/game/task. Local original evidence: artifacts/sprint32-working-71fc3904/.

Counts:67Build-Local attempts /53validated development packages /77native launches.
Build67 measured265.430s. Native76 wrapper267.022s/native90.841s; native77
wrapper407.532s/native206.325s, then full outer restoration. The recent work is
approximately10min fixture/research/records,4.4min build,11.2min native wrappers
(overlapping research), with exact exclusive category totals unavailable. No
owner waiting blocks independent work. The unconstrained survey approach is stopped; the replacement walking query
and Animal correction each have a specific installed-contract basis.

Closed native74 extraction and superseded build62 duplicates reclaimed142,662,260B
against retained exact ZIPs. Protected Compare PASS; all unique packages, raw
failures, receipts, protected backups, saves and DATA retained. Prior measured free space after75:
3,457,798,144B at18:50:55UTC after verified closed75 extraction and seven older
byte-identical extraction copies were removed. Two untouched references and
lossless content archives are retained; these are not rebuilt historical candidate
packages. Protected ComparePASS. Full selections/hashes are in history.
Working measured trio902,923,316B before allowance; rounded1.1GB plus2GB reserve
requires3.1GB initial. Each stage requires550MB plus2GB reserve (2.55GB).
After77, sixteen verified closed duplicate package extractions reclaimed
1,100,108,464B; two untouched references and reread lossless content archives
remain. Protected ComparePASS. Free3,884,158,976B at19:31:15UTC, above the
measured3.1GB initial requirement. No build during a live trio.

**Next:** commit/push and package the reviewed/compiled Animal/native-ground observer batch, then
execute its affected controls and one corrected Working preparation. If that
placement still refuses, retain its precise blocker instead of enlarging surveys.
Do not dispatch the old controls-turn helper with the known Animal fixture defect.
Working cold/ON/OFF remains pending until actual preparation succeeds; do not
repeat its unchanged survey. HoldTB/FearRTWP/TB, grab, presentation, provider
persistence and final integration remain selected required coverage. Save-free
selections expect8-10min/runtime600s; Working ON/OFF retain60-80min combined
ceiling including restoration. No owner playtest, release or Sprint33.

Prior v0.0.167 gaps stay **NOT_RUN**: all30public-route/body/icon, standalone
final integration, highest-risk combined final integration. [Original dispositions](../docs/SPRINT30-31-COVERAGE.md)
are not a Sprint32 waiver and have not been closed by these focused cases.
