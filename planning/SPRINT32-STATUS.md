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

Latest validated development candidate: `b37da6dad568e309c33b54d8deb780fe30a71032`.
ZIP `b8547caf94d259d884915d55a9eb4dbbaacaf5d5faf0b29d167a3c6694183723`;
DLL `b8de78d95aa75b2f8edbcce0752347e241316833b84cc6438f09d75555e8d381`;
MVID `54c48154-c458-4d5d-b9a6-c7b8dd902139`;
SourceState `987d3146b8340327ae4af2339f4e2d2f52dd93b9b4672bca9adf01a2f1b6f0fe`.
Canonical artifacts/development-candidates/sprint32-lillend-support/b37da6dad568/.
Build68: complete development wrapper/558selected/exact compile/strict ZIP
PASS257.718s. This is not final qualification. The healing/group corrections
below are newer unqualified source; original failures remain immutable.

Native74 Hold **9/9PASS**: natural CL7/DC16/42s casts and real L2 slots,
accepted automatic failed/successful repeat saves, native source expiry removing
paralysis before its deadline, and exact sibling-effect/budget preservation.
No successful automatic retry explains the source-expiry result. Hold TB remains
pending. Native72 bardic Suggestion remains source-bound **18/18PASS**, including
ordinary-book negative (real L2slot4->3/hostile memory/Fascinate interruption).

Native78 RTWP controls **5PASS/2FAIL**: corrected Animal target and natural
Charm Monster pass, including CL7/DC17/seven days, Standard/L3 debit and active
target refusal. Actual Cure Light succeeds and heals12->2/L1slots5->4, but
incorrectly breaks Charm. Product defect: native Cure has Ally=None/Enemy=Harmful;
the coarse threat classifier mistakes positive healing for harm. Reviewed fix
requires both Cure and RestoreHP descriptors and checks the recipient's native
IsUndead (including installed negative-energy affinity). RestoreHP alone is not
exempt. Caster-threat and RTWP Sound Burst were NOT_REACHED; rerun required.

Native79 turn-based controls **7/7PASS**: natural nonhumanoid Charm Monster and
one safe Sound Burst, L2slots4->3, exact per-cast native Fort outcomes, both1d8
sonic damage, only failed save receives6s stun; sibling safe/independent.
Native turn frames101, fixture AI ticks0. This case does not cover helpful heal
or the RTWP caster-threat probe and does not close78.

Native80 Working prepare **18PASS/2FAIL**: corrected connected walking query
finds two valid positions after31candidates. Actual Competence passes. The next
combined ownership/nonparty/intake-neutrality guard refuses before time advance
or save (writes0/stashes0). Raw80 cannot distinguish its failed conjunct.
Read-only native IL identifies a real fixture flaw: DefaultPlayerCharacter can
cache its player group; SwitchFactions never reassigns it. Reviewed correction
sets only this new owned actor's GroupId to its UniqueId before changing faction,
preserving all safety predicates and exposing their individual values. No further
geometry change. Cold/ON/OFF remain NOT_RUN. Original75/77 remain failures.

Current product includes private PF1 Sound Burst1d8 and exact private Charm
threat handling. Nonally/ally threat negatives, wards and additional lifecycle
remain pending. Independent review identified a concrete cold-load risk in
native Charm faction bookkeeping. The same Working trio now adds three separate
effect-only Charm/Sleep/Hold recipients after the real performance ledger exists,
with production durations, exact cold/precleanup state, baseline faction return
and sibling isolation. Five points use the same finite survey; no new scenario.
The correction compiles and18/2755 focused tests pass; native proof is pending.

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

Latest installed-stack transaction `sprint32-stack-20261026T195102Z-4c393a9d4998`
restored19:57:31.4893446UTC; outer Working envelope restored19:57:45.0700574UTC.
Original Working SHA `650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`,
8,550,225B and original timestamps/attributes restored. Protected Compare PASS:
967Mods/118saves/18references/83settings/tasks0/unchangedSteam. No owned live
transaction/game/task. Original evidence: artifacts/sprint32-working-b37da6da/.

Counts:68Build-Local attempts /54validated development packages /80native launches.
Build68 measured257.718s. Native78/79/80 measured106.160/124.951/206.074s;
wrappers274.718/297.170/411.637s plus80 outer restoration. Recent work includes
approximately15min implementation/fixture review/records,4.3min build and16.6min
native wrappers, overlapping research; exact exclusive totals unavailable.
Owner waiting0 for this batch. The repeated geometry approach ended at77; the
demonstrated walking-query correction passed80. Do not repeat80 unchanged.

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

After80, five verified closed extraction duplicates reclaimed498,461,544B against
retained exact candidate67/68 ZIPs. Full protected ComparePASS; free3,583,602,688B
at20:01:36UTC. Selection/plan and original failures remain in history.

**Next:** build the reviewed/compiled healing/group/saved-control batch once and
rerun affected RTWP
controls and corrected Working preparation. HoldTB/FearRTWP/TB, grab, technical
presentation, provider persistence and final integration remain required. Save-free
selections expect8-10min/runtime600s; Working ON/OFF retain60-80min combined
ceiling including restoration. No owner playtest, release or Sprint33.

Prior v0.0.167 gaps stay **NOT_RUN**: all30public-route/body/icon, standalone
final integration, highest-risk combined final integration. [Original dispositions](../docs/SPRINT30-31-COVERAGE.md)
are not a Sprint32 waiver and have not been closed by these focused cases.
