# Sprint 32 â€” Satyr and Lillend Azata

Owner Howie Reith; mission2026-10-25. Sole active operational record.
Finish at an independently reviewed, natively validated PR. No merge, release,
Sprint33, unapproved subsystem or owner playtest dependency.
[Validation policy](../docs/VALIDATION-POLICY.md) Â·
[Append-only evidence/history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md) Â·
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
- Approved Lullaby: normal Standard cast,10ft burst, Will/SR,âˆ’5Perception/âˆ’2Will
  versus sleep, immediate concentration release and CL7 seven-round42s tail.
- Approved Lillend-only noncombat dispatcher: preselected Fascinateâ‰¤3,
  Inspire Competence and dependent bardic Suggestion, normal actions, native
 20-round pool, exclusivity, exact-source refusal/lifecycle. Competence/Fascinate
  have early native proof; final coverage remains incomplete.
- **Approved 2026-10-26:** implement the Lillend-only memory exception for bardic
  Suggestion on this exact Lillend's fascinated target. Bounded exact-cast context
  transport into the hostile-memory handler must preserve normal casting,
  Will/SR, stealth, invisibility and all other offensive reactions. No general
  memory/AI overhaul or persistence manager is authorized. Estimated3â€“8active
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

Latest validated development candidate76: `03f48bdcab58bb09df307c82dc9a4754f74d63e3`.
ZIP `66b808b25e4ad41be14a1e2345f5a765de66751e5455aea09f1aa98e4a0e816d`;
DLL `c043c96a5aece035a5144a34d9c1664bbe9d3895daea2a436974bfe9854681d0`;
MVID `4e78071f-990e-4ce5-b2ed-3fcb78072a2c`;
SourceState `4244a3e7787cd6a7309737bc823c63540fe5068ac013216edc331ebf7e4ea858`.
Canonical artifacts/development-candidates/sprint32-lillend-support/03f48bdcab58/.
Build76 wrapper/558of2755/exact compile/strictZIP PASS268.581s. This is filtered
development evidence, not the complete final gate.

Native106 preparation37/37PASS; native107 cold11PASS/3FAIL. Retaining the
exact prepared ZIP proved Fascinate was already ended before cold load. Its
response1 became3 and toggle stopped during save suspension. Installed native IL
shows SaveManager turns off the area/player before PreSave; UnitDescriptor marks
IsTurnedOn=false before fact callbacks. Temporary Fact.TurnOff leaves Active
unchanged and does not invoke native WillStop. The private dispatcher/carrier
mistook that suspension for actual deactivation. The applied correction preserves
performances only while the owner is suspended, clears transient commands, and
retains unconditional actual OnFactDeactivate/native WillStop cleanup. No saved
state is rewritten. Fresh cold/absence/provider/moduleOFF validation is pending.

Native108 presentation16PASS/2FAIL: Satyr rest/moves/native path retirement pass;
three agent-inspected frames show recognizable intact body/equipment without
severe sampled deformation. Bow command, full attack completion and actual
Standard cost pass. The end-of-frame animation compound failed; its individual
predicate is unknown. The correction captures synchronously at the exact native
weapon event using the proven capture seam, retains strict frame/command/clip
predicates, and separately correlates actual projectile damage. Contacts, wounds,
death/expiry and Lillend presentation remain unqualified.

Native109 grab15PASS/2FAIL: full Lillend RTWP acquisition, live-Strength
maintenance, exact tail/constrict, release and finalizers PASS. TB refuses before
gameplay because the owned idle anchor has a command; exact producer is unknown.
Salamander is NOT_RUN. The grouped phase now owns that proven idle anchor's brain
across cell boundaries, retaining all command/refusal checks and exact restoration.

Native110 controls8PASS/1FAIL: actual Good/Chaos ward refusals, natural Charm
Monster and harmless healing PASS. Unrelated-wolf readiness refused before attack;
the compound predicate is unknown. Native supplied attacks do not require the
visibility memory cache. The corrected fixture retains physical LOS/reach and all
ownership/liveness checks, clears the existing sight cache after owned relocation,
and records each predicate. Exact positive weapon-damage binding remains required.
Apparent-ally threat and safe Sound Burst were not reached.

One coherent batch also adds the finite Competence RTWP/Fascinate TB combat-entry
and provider Suggestion real positive-damage break/unshortened expiry cases.
These use existing native commands, clocks, owned actors and scenario declaration;
no new generic observer or product subsystem. Suggestion uses a disclosed native
600s fixture summon to outlive the original CL7 420s effect; select900s runtime.
All applied changes compile; focused `sprint32;fey-celestial`8/2755 PASS
(production29.1s/domain4.9s). Initial compile failure was missing two project
entries and a namespace import, corrected. Five shell admission cases pass,
including case/whitespace/extra-save/foreign-scenario refusals. These are not native
qualification. Original failed results remain immutable.

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

Latest Working envelope restored2026-10-27T01:59:44.8678022Z; native110
restored02:18:27.1642590Z. Full protected ComparePASS after cleanup:
967Mods/118saves/18refs/83settings/tasks0/Steam1. Original WorkingSHA
650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850,
8550225B; original times/attributes restored. No owned live transaction.

Counts76Build-Local attempts/62validated packages/110actual native launches.
Build76 268.581s; native106/107/108/109/110
246.425/192.489/108.008/124.380/116.438s. Independent review read native save
contracts and the bounded correction drafts; actual applied diff reviewed. One P2 found: Suggestion terminal fixture must not
restore anchor AI while Default ownership remains pending. Corrected with the
same exact-identity/pending guard; candidate compilation follows.
Approx01:30-02:28UTC overlapping work: build/package4.48min; native setup,
execution/restoration~29min; implementation/observer/research~30-40min; storage
~5min; owner waiting0. Exclusive cumulative totals unavailable.

Measured Working peak902923316B rounded1.1GB, build154MB, plus2GB reserve
requires3.254GB before this next build. After110 verified closed duplicate cleanup
reclaimed1144920553B from15 extractions, retaining canonical packages, reference
copies and four verified content archives190453466B. Free3439124480B after full
protected ComparePASS. Unique packages/raws/failures/receipts/protected backups,
DATA and personal files remain. Exact selection/results/retention identities are
in history. Check actual headroom before each admitted transaction.

**Next:** commit/package the coherent compiled batch once; run the changed base
Working ON prepare/cold/absence, presentation, grab and controls scenarios, then
the finite performance/Suggestion cases. No unchanged failed retry. Each ordinary
selection targets15min and stays below30min including safe restoration. Provider
retained/removed/moduleOFF trios remain predeclared90-120min combined, with fresh
processes and original Working restoration. Preserve prepared ZIPs as evidence.
Integrate relevant accepted dependencies at one coherent checkpoint, obtain the
independent final review, freeze inputs, run one complete cheap final gate and
qualify its exact ZIP. Final integration, presentation and persistence remain
required. No owner playtest, merge, release or Sprint33.

Prior v0.0.167 gaps remain **NOT_RUN**: grouped all30public-route/body/icon,
standalone final integration, highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
