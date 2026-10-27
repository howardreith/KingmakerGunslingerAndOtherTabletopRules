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

Latest validated development candidate75: `f5a40e2030ef9ff5279eb12b0e15cc35bbaa6055`.
ZIP `05cd5eac5a67607bb07db821ccfdfb3759913d416cdda2063063cc1e38fdeda9`;
DLL `bcbc5e0ffc9b9e0d1cbbf0e1510170ce298a918c215c6753f49fdaa72f4de23e`;
MVID `f1ef748b-74ac-4c48-baf9-c2d177f99d91`;
SourceState `627cda9cfba5b16f96da73a2a379ef3c7f4e1fb2f852fb8f45509a36f84e8a9e`.
Canonical artifacts/development-candidates/sprint32-lillend-support/f5a40e2030ef/.
Build75 wrapper/558of2755/exact compile/strictZIP PASS272.131s; development,
not the complete final gate. New reviewed observer edits compile; the focused
`sprint32;fey-celestial` gate passed8/2755 (production28.6s/domain1s).

Native102 grab12PASS/2FAIL. Natural acquisition and live-Strength maintenance
pass. The release observer incorrectly expected no damage after its CMB change:
actual native CMB23/CMD-1 allowed three legitimate successes before a natural1.
Immutable rows show exact tail/constrict pairs for those three rounds, none for
the failed round, unchanged3weapon attacks and all10 cleanup predicates true
one frame/.021s after link loss. Independent read-only review supports narrow
**reviewed-reuse RTWP release PASS**, retaining original aggregateFAIL.
The compiled correction restores the fixture attack boost before lowCMB, checks
each intervening successful rider pair and mandatory positive tail damage.
TB/Salamander remain NOT_RUN.
Native103 presentation16PASS/1FAIL. Both Satyr moves and natural command/path
retirement pass; agent inspected all3 rest/movement frames, recognizable/intact
without severe sampled deformation. A later attack/cost/contact compound throws
before its values were serialized: exact culprit UNKNOWN. Installed native IL
supports reusing the existing final-act/complete-plan observer in RTWP rather
than requiring terminalSuccess. New per-attempt evidence retains costs, contacts
and errors; strict animation checks remain. Bow evidence is launch, not impact.
Native104 Working preparation37/37PASS. Native105 cold11PASS/3FAIL passes the
corrected exact listener boundary and saved Charm/Sleep/Hold controls. The first
Lillend's saved Fascinate response1 becomes3, running toggle becomes off, and
27 assistance grants plus its fascinated effect disappear. Budgets/books/items
remain unchanged. Cold lifecycle change is unresolved; no weakened comparison
or PASS. Native unloading invokes area-exit, and load-time area callbacks can
execute at zero game delta. Independent review cleared the bounded read-only
64-row cold trace. The next changed observation retains the prepared Working
ZIP and exact lifecycle callbacks; it does not alter product state/comparison.
Absence/provider-transition stages remain NOT_RUN. All original failures remain
immutable; both save-free transactions and the entire Working envelope restored.

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
break/expiry/source-loss/provider persistence (shared Will/highSR reviewed reuse); new serialized performance/book/
cantrip state ON/OFF; technical presentation and final integration. Bardic
Suggestion exception has native72 proof; expiry/break/provider-cold rows remain pending.
Private Hold Person requires real initial Will/SR, automatic repeat-save/action
observation and source-loss/sibling lifecycle checks under the accepted adaptation.
No whole-roster replay selected.

## Restoration, headroom, work and next action

Latest Working envelope restored2026-10-27T01:15:55.9386711Z; exact intake
967Mods/118saves/18refs/83settings/tasks0/Steam1. Original WorkingSHA
650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850,
8550225B and current intake metadata restored. No owned live transaction.
Absence was not launched after failed cold; detailed identities in history.

Counts75Build-Local attempts/61validated packages/105native launches.
Build75 272.131s; native102/103/104/105 128.342/108.533/243.987/191.751s.
Independent review covered immutable102 release, actual native completion IL,
observer diffs and mandatory tail damage; concrete findings addressed in the
unpackaged correction. Review also cleared exact-initiator/weapon damage binding
for the added unrelated-wolf Charm check. Approximate overlapping
work since00:58UTC: observer/review/research~20min, native supervision/restoration
~17min; no additional build; owner waiting0. Exclusive cumulative totals unavailable.

Measured Working trio902923316B, rounded1.1GB plus2GB reserve requires3.1GB
initial; each stage550MB plus2GB requires2.55GB. After97 verified duplicate
cleanup retained all unique content. After101 verified cleanup reclaimed
991895080B from twelve exact closed duplicate package extractions, retaining
reference copies/content archives/validated ZIPs. Free3538485248B and full
protected ComparePASS; exact receipts are in history.
Unique packages/raws/failures/receipts/protected backups/DATA/personal data remain.

After103 cleanup reclaimed199601296B from the two closed exact package
extractions, retaining ZIP/raws/receipts/backups; free3219337216B and protected
ComparePASS. After105 cleanup reclaimed1039415973B from15 closed duplicate
package extractions, retaining canonical ZIPs/reference copies/content archives,
raws/receipts/backups. Free3661586432B; protected967Mods/118saves/18refs/
83settings/tasks0/Steam1 ComparePASS. No protected backup, unique package,
save or personal data was removed. Exact receipts are in history.

**Next:** package the reviewed/compiled grab, presentation, controls and cold-trace
batch once, then classify cold Fascinate with the changed observation and retained
prepared ZIP. Finish the remaining finite performance/Suggestion cases alongside
that native work; their ignored drafts are not implemented/qualified. No unchanged
cold retry. No product change is justified before the first callback is identified.
Grab/presentation each10min/runtime600s. BaseWorkingON and provider retained/
removed/module-off trios remain pending; provider sequence90?120min including
restoration is predeclared, with module-off also covering ordinary OFF assertions.
Retain the exact prepared disposable ZIP as local evidence before future cold
stages; it was not retained by104, so original intake cannot substitute for it.
The finite reviewed additions are actual Good/Chaos Charm ward refusal,
unrelated-attacker retention/apparent-ally break, Competence RTWP and FascinateTB
combat transition, and ordinary Suggestion real damage-break/unshortened expiry.
Source-loss/shared Will/SR cases may use dependency-reviewed existing evidence.
Final complete gate, dependency refresh, independent final review, integration
and exact-candidate qualification remain required. No owner playtest, merge,
release or Sprint33.

Prior v0.0.167 gaps remain **NOT_RUN**: grouped all30public-route/body/icon,
standalone final integration, highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
