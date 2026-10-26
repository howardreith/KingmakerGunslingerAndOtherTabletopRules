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

Latest validated development candidate: `f5756a4ac9c1e3d19543da0953c434a48ad981b2`.
ZIP `bf93d301ea89fc244735768c5588871a63b49ad91943905780d9c09aeec314c7`;
DLL `080e8b9a4378a9124364121cf4523496c81958631e24fd0f168a8253541e9918`;
MVID `af560466-8d61-417c-881e-bf0bf5828b31`;
SourceState `a91887ef1ebf4bafc06fcb1126b755aa7c91a75c4d237b0a274a21e60b565051`.
Canonical artifacts/development-candidates/sprint32-lillend-support/f5756a4ac9c1/.
Build62 wrappers/558selected/exactcompile/strictZIP PASS282.576s; compiled
parser21 admission/refusal cases PASS. This is a development candidate.

Native72 bardic Suggestion **18/18PASS**: natural saved/high-SR/successful
attempts, same own Fascinate, Standard actions/native upkeep/independent slots,
other offensive reactions, current-effect refusal; ordinary book negative spends
L2slot4->3 and causes normal hostile memory/Fascinate interruption. Invisibility
was a disclosed synchronous reaction fixture, not sustained invisible performance
or entered-stealth proof. This closes native69's affected negative-probe failure.

Native71 Hold **8PASS/1FAIL** repeated the separate effect-probe placement
failure after useful natural cast/repeat-save evidence. Repeated approach paused.
Independent installed-engine review rejected speculative collider/settle claims:
point LOS uses cached geometry. The bounded correction reuses the vacated surveyed
sibling point and native creation Tick, retains walkability/identity/faction/
CanTarget/availability, and omits LOS/memory only for the labeled effect-only
lifecycle probe. No second casting claim is made. Source expiry remains unverified.

Native73 Working prepare **15PASS/2FAIL**: native load, four exact new creatures,
ownership/duration and appearance passed; new helper rejected its scene boundary
before any save write. Cold cleanup/absence and OFF were NOT_RUN. The existing
caller deliberately pauses after appearance; native IsPaused uses Pause mode.
The correction records every predicate, admits only native Pause over Default,
and waits bounded native resume before enabling the dispatcher. Strict calm,
party/owner, provider absence and no-save-after-failure guards remain.

Both fixture corrections passed independent read-only resolution review;
production compilation23.9s and focused persistence-scope1/2755 PASS0.8s.
No gameplay rule changes. Private deterministic component names, exact fixed
Working two-plus-two admission and saved-state fixture are present in candidate62.
Actual saved selections/cold resources/cleanup remain unverified. First persistence
selection requires provider absence; provider transitions/other controls are separate.
Fixture equipment disposal does not prove native equipment expiry. Older complete
tuples, original failures, admission checks and review findings stay in history.

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

Latest installed-stack transaction `sprint32-stack-20261026T173037Z-f654a9e91726`
restored installation/settings17:37:03.6919179UTC; original Working bytes/metadata
and full protected intake restored17:37:16.2932541UTC. Working SHA
`650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`.
Compare PASS967Mods/118saves/18references/83settings/tasks0/unchangedSteam.
No owned live transaction/game/task. Raw evidence identities are in history;
local original logs/intake: artifacts/sprint32-working-f5756a4a/.

Counts:62Build-Local attempts /51validated development packages /73native launches.
Build62 measured4.71min; native72 wrapper6.14min/native185.572s;
native73 prepare wrapper6.82min/native206.182s, outer restore followed.
Whole-mission exclusive category totals unavailable. Recent work since17:17UTC:
approximately12min storage/tooling verification and8min fixture investigation/
repair; native wall time overlapped investigation. No new build or owner wait.
Two consecutive Hold probe failures70/71 triggered the finite replan above;
Working73 is a distinct first scene-precondition failure, not a product verdict.

Closed extraction duplicates71/72 and superseded build61 reclaimed242,240,859B.
Nine older closed extraction trees matched a retained original and lossless
content archive byte-for-byte;752,717,547B reclaimed,56,019,488B archive retained.
A nonidentical earlier tree was refused and retained. Protected Compare PASS;
no unique package, raw failure, receipt, protected backup, save or DATA file removed.
Free space17:29UTC3,503,726,592B. Exact selection/archive/plan hashes are in history.

Working budget, independently reviewed: measured trio902,906,932B before allowance,
rounded1.1GB plus2GB reserve =>3.1GB initial admission. Each stage requires550MB
live peak plus2GB reserve =>2.55GB before task registration. Protected backups
remain retained. This replaces the inherited1.8GB envelope floor. No build during
an admitted trio. Measure again before the next candidate/native selection.

**Next:** finish resolution review, compile/test and commit this coherent fixture
batch; build one candidate. Rerun Hold expiry first, then affected Fear RTWP/TB,
Hold TB and Working prepare/cold cleanup/fresh absence ON/OFF as admitted by each
predecessor. Each save-free selection expects8-10min/runtime600s. Two Working
trios retain the predeclared60-80min ceiling including safe restoration; no
unchanged retry and no cold stage after failed preparation. First profile remains
the exact human installed stack with provider absent. Continue remaining
controls/grab/presentation and explicit final integration. No final qualification
or owner playtest is implied by focused checks.

Prior v0.0.167 gaps stay **NOT_RUN**: all30public-route/body/icon, standalone
final integration, highest-risk combined final integration. [Original dispositions](../docs/SPRINT30-31-COVERAGE.md)
are not a Sprint32 waiver and have not been closed by these focused cases.
