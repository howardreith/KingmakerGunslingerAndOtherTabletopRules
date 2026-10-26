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

Latest validated development candidate: `214ccd3574bab6c54e747bad0b6e44535faf8905`.
ZIP `6e5d8eb454d2cf86334ef2abe8a708863c13a6d10aad026144a29bf61259da10`;
DLL `898c9de328604d95bfb54e2cc373d027440913e0313915edd54290f4c8fe3b9f`;
MVID `7d63f483-f0ee-4608-8bb7-779fa115d608`;
SourceState `ad817636e4f7dc2d913e62d3c545da53cf796843fb47c5df7ba3049fe5e07870`.
Canonical artifacts/development-candidates/sprint32-lillend-support/214ccd3574ba/.
Build71 wrapper/558of2755/exact compile/strictZIP PASS266.704s. Development
evidence, not final qualification. Current reviewed fixture source is newer.

Native87 Satyr RTWP13/13 and88turn14/14 PASS: natural Fear, immune bow/melee
fallback, normal actions and exact source/sibling cleanup. Native grounding
changed probeY by0.41936612m; retained surveyed graph coordinates resolve the
placement failure. Turn case's expiry helper is separately RTWP.88's original
outer readerFAIL remains; corrected immutable readerPASS, no fresh replay.

Native89 grab4PASS/1FAIL before Default at compound idle caster/anchor guard;
no grab gameplay. Independent installed-contract review found completed setup
commands remain in the caster queue. Correction retires only proven exact
finished commands, with empty pending queue, and records every guard predicate.
The original raw cannot identify the sole failed conjunct. Native90 Working
17PASS/2FAIL: fixed Neutrals is attacked by intake; no listener spawned/save
written. ColdON/OFF remain NOT_RUN. Bounded correction selects an existing
registered faction from actual full intake unit/group relations and persists
that exact identity. Cold checks never select anew. All hostility guards remain.

The new fixed technical-presentation case groups both creatures' loading,
selection surface, movement/gait, actual attacks/contact, hit/death/expiry and
owned/borrowed resources using existing native seams. Independent review found
and corrected exact command/animation attribution and pending-mode restoration.
It is compiled development work, not native presentation evidence.

Earlier source-bound evidence remains in history: player input Wizard16/16,
Sorcerer28/28,Druid31/31; Satyr at-will13/13 each mode; support13/13;
lifecycle15/15; cantrips7/7; Fascinate24/24; bardic Suggestion18/18 incl ordinary
negative; controls RTWP9/9 andTB7/7; Hold RTWP9/9 andTB6/6. These are reviewed
reuse candidates, never labeled fresh on changed bytes. Required final coverage
and technical presentation remain incomplete. All original failures are retained.

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

Latest Working envelope restored22:05:03.4181125UTC; protected intake exact
967Mods/118saves/18refs/83settings/tasks0/Steam1. Original WorkingSHA
650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850,
8550225B and intake timestamps/attributes restored. No owned live transaction.
OFF was not launched after failedON. Detailed transaction/reader identities in history.

Counts71Build-Local attempts/57validated packages/90native launches. Build71
266.704s; native87/88/89/90 113.919/128.534/91.072/214.476s. Recent active work
approximately15min fixture/contracts/review and5min tooling, with overlapping
native supervision; exclusive totals unavailable; owner waiting0. No unchanged
repeat of failed89/90. Post90 initial compile24.5s/focused tests15/2755 PASS;
reviewed batch compile25.5s/tests0.9s PASS15/2755.

Measured Working trio902923316B, rounded1.1GB plus2GB practical reserve requires
3.1GB initial; each stage550MB plus2GB requires2.55GB. After89 scoped verified
duplicates reclaimed299214552+43102446+464198406+397884348B; free3719770112B
21:56:53UTC, protected ComparePASS. After90 free3435790336B observed.
Retain unique packages/content archives/raws/failures/backups; no DATA/personal
cleanup. Recheck headroom before the next selected transaction.

**Next:** complete the bounded review corrections, focused compile and coherent
candidate. Run affected grab and technical-presentation selections (each10min,
runtime600s) and corrected WorkingON/OFF (predeclared60–80min combined including
restoration). Stop a failed sequence and classify it. Provider persistence and
final integration remain required. Before final qualification, review relevant
accepted descendants, freeze source/assets/observers/publication inputs, run
one explicit complete development gate and qualify its exact ZIP with a
separate records worktree. No owner playtest, merge/release or Sprint33.

Prior v0.0.167 gaps remain **NOT_RUN**: grouped all30public-route/body/icon,
standalone final integration, highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
