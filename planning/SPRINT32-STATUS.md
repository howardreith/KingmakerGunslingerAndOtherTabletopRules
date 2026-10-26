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

Latest validated development candidate: `a1a8ac14e19e7213bd04537905cdcb8bdcf25246`.
ZIP `58ca3938c138757bcf10fcc8d5f7ff80f33fc6bbcc1fb24a3f01a2bc2967a044`;
DLL `aaa6730f3679ba312dd910e6061b42a884e0feffe19e61d3f070c79daa771791`;
MVID `b7372651-f332-4e33-8d3a-0ddba2ad7e1b`;
SourceState `7a38bc139f37bf14ca4c17eb7ad3c55658d2f1ac9f70d4af74ac225d5da1283a`.
Canonical artifacts/development-candidates/sprint32-lillend-support/a1a8ac14e19e/.
Build70: development wrapper/558 selected/exact compile/strict ZIP PASS308.438s.
Development evidence, not final qualification. Current reviewed fixture batch is
newer source, compiled with6/2755 focused tests PASS; fresh affected execution is
required. Initial admission test accidentally included the new phase among
negative cases; corrected. A compile caught two old string-faction references;
both corrected. Original logs retained.

Native85 Working prepare **18PASS/3FAIL**: two-point geometry succeeds in28
candidates. First listener has exact ownership, its unique nonparty group, but
the unchanged mutual-hostility guard finds1058 intake relations and refuses
before advancing time or saving. No cold ON/OFF coverage. Independent installed
contract/save review found stale Player membership in the new group plus14
genuine intake actors attacking Mobs (13 in game). Correction uses the existing
registered neutral faction, refuses intake attack relations before spawn,
refreshes both native caches only after proving a unique one-member owned group,
and keeps the full mutual-hostility guard. Applies to the two listeners and
three effect-only controls; cold checks use the same registered identity.

Native86 Satyr RTWP **7PASS/1FAIL**: natural Fear/CL8/DC18/Standard/one-use PASS;
same vacated-sibling guard refuses. TB/fallback not reached. Stop this repeated
approach. Installed Translocate/ForcePlaceAboveGround changes actual actor Y
through physics. The bounded replan retains the exact original surveyed graph
coordinate for Fear's effect-only probe instead of rediscovering it from the
later sibling pose, preserving every clamp/separation check and recording its
individual values. Actual failed subpredicate remains unknown until execution;
no claim that the original failure is closed.

New fixed `lillend-grab` selection groups Lillend RTWP/TB and one Salamander
RTWP baseline using existing native Default controllers. Actual AI attack,
tail hit, maneuver, initial/later riders, prospective live Strength and failed
maintain release are observed; real target escape remains enabled. Only proven
idle owned profile anchors can be passed; no tested-actor Prepare/EndTurn.
Independent review corrected mode cost, native conditions, anchor ownership,
lease/finalizers, exact establishing-command attribution, appearance readiness
and Salamander physical packets. Final bounded review found no dispatch blocker.
This source is compiled, not natively executed; no movement/presentation proof.

Native81 RTWP controls **9/9PASS** closes native78's healing defect: normal
CureLight heals12->6, L1slots5->4 and preserves exact Charm. A subsequent normal
caster attack starts and breaks Charm before acting; this proves threat, not
hit/damage. Natural CharmMonster CL7/DC17/seven days/Standard/L3slot and refusal
pass. Natural safe SoundBurst pays one L2slot, both targets take1d8 sonic and
only the failed Fort save receives6s stun; sibling safe/independent. Native79's
turn controls7/7 remain source-bound reuse, without the RTWP threat probe.

Native83 Hold TB **6/6PASS**: natural Hold/real Standard/L2slot4->3,
78 native turn frames and0 fixture AI ticks. The separate expiry/sibling probe
switches to RTWP as declared; no TB automatic-retry claim. Native74 RTWP9/9
remains source-bound proof of accepted automatic failed/successful retries and
exact source expiry/sibling isolation. Native72 bardic Suggestion18/18 remains
source-bound, including the ordinary-book hostile-memory negative.

Native82 Working prepare **17PASS/3FAIL**, before spawning listeners or saving.
The five-wide footprint found no geometry in288 candidates:224 horizontal clamp,
9 acquisition exclusion,55 physical sight. No save writes/stashes. Earlier
native80 proved a two-point footprint; the candidate70 correction retained that pair
and chooses three separate effect-only controls from the same finite candidates.
All five must pass navigation/height/acquisition/separation before spawn; only
the real performance pair requires LOS/range. No wider survey. Native85 proved the unique GroupId; real
saved-control contexts and cold ON/OFF remain NOT_RUN.

Native84 Satyr RTWP **7PASS/1FAIL**: natural Fear CL8/DC18/Standard/one use PASS;
then combined vacated-sibling placement guard refuses before effect cleanup.
The raw result does not distinguish its failed subpredicate. Source inspection
shows this caller never surveyed the sibling's spawn point. Correction selects
one of only two connected, walkable points behind the established Fear lane
before casting. Exact target/protected-party/expiry guards remain. No TB launch
or fallback rows after this failure. Independent reviewer found no blocker in
both placement corrections; compilation24.1s and18/2755 tests PASS1.2s.

Private Charm/Sleep/Hold state coverage remains grouped into the existing Working
trio. Real effect-only contexts/flags/deadlines and actual precleanup conditions
must survive cold ON/OFF. Primary source removal must restore Charm hostility
and clear Hold while preserving sibling Sleep/performance. No speculative native
Charm faction repair is implemented before observing the concrete cold-load risk.

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

Latest native transaction `compat-20261026T210944Z-47e9b23c93d5` restored
21:14:18.2160718UTC. Full protected ComparePASS967Mods/118saves/18references/
83settings/tasks0/unchangedSteam. No owned live game/task/transaction. Native85
original Working SHA650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850,
8,550,225B and original timestamps/attributes restored21:08:42.1828065UTC.

Counts70 Build-Local attempts /56 validated development packages /86 native
launches. Build70 308.438s; native85/86 207.936/100.866s; wrappers413.583/275.249s,
plus85 outer restoration. Recent work approximately12min fixture/contracts/review/
records,1min focused compilation/tests,5.14min package,11.48min native wrappers;
intervals overlap, exclusive totals unavailable. Storage verification is recorded
separately in history; owner waiting0. No unchanged replay of85/86.

Working measured trio902,923,316B before allowance:1.1GB rounded peak plus2GB
reserve requires3.1GB initial; each stage550MB plus2GB requires2.55GB. After84,
four verified closed extraction duplicates reclaimed398,838,048B; protected
ComparePASS, free3,037,814,784B. Three superseded build staging/DLL copies then
reclaimed129,128,809B, free3,165,351,936B20:51:48UTC; protected ComparePASS.
Exact ZIPs, receipts, original failures, unique artifacts, backups/saves and DATA
remain. Four further byte-identical closed S25-stack extraction copies reclaimed328,308,916B
against a reread55,258,222B content archive and untouched reference. Protected
ComparePASS; free3,438,936,064B20:55:09UTC exceeds the initial3.1GB margin.
The archive is lossless retained content, not a rebuilt historical candidate.
No live cleanup or machine-wide deletion.

After86, two exact closed package extractions and build69 duplicate staging/DLL
reclaimed199,424,144B +43,097,326B. Four further byte-identical S26 closed
extractions reclaimed330,868,748B against a reread55,583,936B archive and untouched
reference. Free3,368,800,256B at21:30:03UTC; protected Compare967/118/18/83/tasks0/
Steam1 PASS. Unique packages/raw evidence/backups remain. The cleanup caller's
stale exit-code read and an incomplete retention manifest refused safely;
corrected metadata/caller completed without weakening protection.

**Next:** package the reviewed fixture batch once. Run grouped grab, corrected
Working ON/OFF and affected Satyr RTWP/TB against that exact package. Save-free
selections expect8-10min/runtime600s; Working ON/OFF retain60-80min combined
ceiling including restoration. Stop a failed sequence and classify it.
Technical presentation, provider persistence and final integration remain
required. No owner playtest, release or Sprint33.

Prior v0.0.167 gaps stay **NOT_RUN**: all30public-route/body/icon, standalone
final integration, highest-risk combined final integration. [Original dispositions](../docs/SPRINT30-31-COVERAGE.md)
are not a Sprint32 waiver and have not been closed by these focused cases.
