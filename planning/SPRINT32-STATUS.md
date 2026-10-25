# Sprint 32 - Satyr and Lillend Azata

Owner Howie Reith; mission2026-10-25. Sole active operational authority.
Finish at an independently reviewed, natively validated PR; no merge, release,
Sprint33 or unapproved subsystem. [Validation policy](../docs/VALIDATION-POLICY.md).

## Scope, ownership and decisions

Accepted intake master `48e6fb4abfc154f111a4019a806cb79a4184bd1c`.
Branch `codex/expanded-summoning-sprint32-satyr-lillend`;
worktree `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/.worktrees/expanded-summoning-sprint32`.
[Draft PR74](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/74).
HOWARD-LAPTOP owns mechanics/shared gameplay integration. DATA's released-creature
art and other worktrees remain untouched. Review relevant accepted dependency
changes at the coherent pre-freeze checkpoint.

Charter v1.0 sections3/4/7 and S32: Satyr SNA IV-IX; Lillend SM VI-IX;
quantities1/1d3/1d4+1; dependencies5,29-31. Preserve native action economy,
resources, ownership, cleanup, saved identities and accepted ordinary-map flight.

- Execute and inspect required native/technical visual checks unattended.
  Owner aesthetic approval is separate; no manual owner playtest dependency.
- Owner explicitly omitted Countersong and Darkness; disclose both. No substitute fog.
- Suggestion conditional on active CowWithHat's Custom Spells; omit when absent.
  Its fixed avoid-combat/fascinate-until-harmed behavior, minutes/level, Will/SR
  is the conditional adaptation; unrestricted tabletop instructions are not claimed.
- **Pending Lullaby decision:** recommended legal cast/immediate concentration
  release with7-round CL7 tail(2-5active hours), Lillend-only sustained concentration
  (8-20hours), or explicit omission. No implementation while pending.
- **Pending noncombat performance decision:** native summon AI selects in combat;
  Fascinate requires calm conditions. Lillend-only autonomous noncombat dispatcher
  (6-14active hours with uncertainty), or explicit omission of Fascinate, Inspire
  Competence and dependent bardic Suggestion. No provisional dispatcher/performance
  implementation. Conditional spell/SLA Suggestion is independent.
- Exact branch addition to sanctioned push wrapper authorized/applied. No release
  number reserved. Development version0.0.167 remains explicitly candidate-bound.
- Guarded Steam640820 transactions, current protected intake, immutable baseline,
  leases, exact source/ZIP checks and restoration remain mandatory. Final qualification
  uses frozen source plus separate records worktree where identity guards require it.

## Current implementation and evidence

Satyr: adult Fey8/HP44/CL8; native Fear/Charm Person/Sleep and conditional
Suggestion; immune/ineligible/active-effect refusal; bow and dagger/secondary
horns fallback; printed native feats, racial Perception/Stealth and check-specific
Intimidate. Lillend: adult Outsider7/HP73/AC20; Large +1 longsword/secondary tail,
10-foot reach, tail-only grab/live constrict on the accepted lifecycle; native
bard7 book5/4/2, independent20-round Courage pool/Move activation, safe healing,
control and ally-safe Sound Burst eligibility; separate one-use Charm Person SLA.
The working candidate exposes all ten legal new roots;1150 prior roots remain
published. The ten prepared public-route checks now pass. Distinct natural
input/spontaneous/conversion paths, pending performances and full acceptance
remain incomplete.

Current validated development source `392728874ba2ef5519078dedaaacfc247d0f90d2`:
ZIP `9ca155b12438c3cc2a91e121ee51545062fd3f447cba745caab1d1eda43b3eb5`;
DLL `0322f4eebb62119b38c6b91f7482a8b7e1913585f2d769165264ed35d549b29f`;
MVID `a1c361a2-b3a4-4fe6-929a-89c46e41b064`.
SourceState `16c944d5335cee94bf315b6fbea6f4177347a79578abb555f6c143abae60bc6d`.
Canonical path `artifacts/development-candidates/sprint32-lillend-support/392728874ba2/`.
Wrappers,557 selected/2753 registered checks, exact-reference compile and strict
419-file package PASS. Development candidate only; final gate/qualification pending.

Fresh392 standalone executions:

| Case | Native result | Observed scope | Local evidence |
|---|---|---|---|
| Satyr public prepared routes | PASS12/12 | Druid20 SNA IV-IX; counts1/1/5/2/3/2; one slot each;120s each; native public inventory/body/icon/selection/reset | `artifacts/sprint32-satyr-routes-39272887/`; raw `20261025T2324526063522Z-disposable-expanded-summoning-sprint32-native-review` |
| Lillend public prepared routes | PASS10/10 | Wizard20 SM VI-IX; counts1/3/2/2; one slot each;120s each; native public inventory/original body/icon/selection/owned-resource reset | `artifacts/sprint32-lillend-routes-39272887/`; raw `20261025T2331344042836Z-disposable-expanded-summoning-sprint32-native-review` |
| Support lifecycle | FAIL1; cleanup PASS2 | Quiet toggle setup did not establish exact external CourageRecipient; no lifecycle behavior passed | `artifacts/sprint32-support-lifecycle-39272887/`; raw `20261025T2336582876467Z-disposable-expanded-summoning-sprint32-native-review` |

Agent inspected both gameplay-camera captures: recognizable Satyr and visible
original winged-serpent Lillend. Crowded rest views do not prove motion/contact,
full deformation/lifecycle/performance or personal aesthetic approval. The
existing prepared helper synchronously pumps native commands/animations; it
proves the declared cast/slot/quantity/duration scope, not natural input timing.
Its positive-route `withinCastRange` diagnostic is an unassigned default, not an
observed failed range check; actual range remains for natural-input observation.

Satyr's outer reader originally FAILed on an invented `sprint32-phase` assertion.
Original exit/error/reader remain immutable. Corrected reader verifies immutable
result hash, actual accepted request phase/RunId, all eleven core requirements
and every native verdict; reviewed reuse PASS, no new launch. Other two readers
used the actual request contract before dispatch. Native result itself was PASS12.

Lifecycle root cause remains unobserved; classify as fixture prerequisite not
established, without assigning a product defect. Next bounded change reuses the
proven owned combat activation path and records command/resource/recipient state
before removal; do not repeat unchanged quiet-toggle setup.

Knowledge(nature) uses native Lore Nature identification with each owner's live
Intelligence. Satyr has six ranks plus Fey class skill; Lillend has seven native
ranks/no class-skill bonus. Live Int+2 gives+1; Wis+2 gives0 to Nature and+1 to
ordinary Perception; sibling values remain unchanged. Source-backed private
creation mapping uses the exact native serialized readonly ability reference and
native dependency links, with no load-time refresh. Cold-load proof remains A6.
Sources: [Satyr](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Satyr),
[Fey](https://www.aonprd.com/MonsterTypes.aspx?ItemName=Fey),
[Lillend](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lillend).
Unmapped skills remain disclosed, not silently waived: Satyr Disguise/Perform/
Survival; Lillend Fly/Perform/Survival. Sense Motive shares Lillend's+13 Perception
mapping, not an independent native check. Survival is not merged into Nature.
Reconcile these against applicable accepted scope before final completion.

Reviewed reuse on earlier bytes:cc Satyr turn PASS11/11 and Lillend turn PASS10/10
prove live Nature/sibling isolation, normal control/healing actions, exact completed
attack plans and native Courage upkeep20-to19-to18. Final Success acts precede
terminal Interrupt; this proves mechanics, not animation completion/caller.
2d optional-provider Suggestion PASS7/7 and
standalone Lillend support/body cleanup PASS11/11;0723 Satyr RTWP control/refusal/
fallback PASS9/9. Since cc, production changed only publication; the new fixture changes do not
change these combat mechanics. These are reviewed reuse, not fresh392 executions. Suggestion proved
natural Satyr DC17/CL8 and Lillend DC16/CL7 casts, standard costs, native duration,
active-effect refusal and own level2 slot debit. Positive Will/high SR, elapsed
expiry, damage break and provider cold load remain NOT_RUN. Lillend body cleanup
proved62 owned resources gone and158 distinct borrowed resources retained.

Original Lillend geometry/paint use the accepted human/owned-tail seam and
unchanged Salamander gait/playback/settlement. [Provenance](../assets-source/original-models/sprint32-support/README.md).
Byte-identical export reproduction and three export checks PASS; full gameplay
camera/contact/movement/lifecycle technical acceptance remains incomplete.

Exact source/artifact/raw-result/log/restoration tuples, original failures and
review corrections are retained in [history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md).

## Finite acceptance contract

All final rows remain incomplete. Early slices above do not replace final qualification.

| ID | Observable result | Route and evidence | Mode/profile | Invalidated by |
|---|---|---|---|---|
| A1 | Adult identity, defenses, attacks, legal facts, spell lists; exact legal tiers and quantities; existing routes retained | Domain catalog/route tests; native composed-profile and inventory observations | Offline; standalone native | Catalog, builders, registration, adapters |
| A2 | Both public summons pay real cost/slot, produce legal count and duration with usable body/icon; distinct prepared/spontaneous/adapter paths covered where materially different | Existing player-route helper with bounded Sprint 32 cases; structured slot/spawn/expiry receipts | Standalone; affected casting-source integrations | Publication, costs, quantities, duration, adapter generation |
| A3 | Satyr naturally uses useful control without pipes; rejects immune/ineligible targets and avoids repeats; attacks when control is unavailable | Existing disposable first-area fixture and exact-owner AI/command observers; control/refusal/fallback records | RTWP and turn-based | Satyr rules, targeting, shared AI/action handling |
| A4 | Lillend naturally supports with safe buffs/performance and healing; resources/slots and actions debit correctly; ordinary offensive fallback remains useful | Same bounded native fixture; native action/effect/resource observations | RTWP and turn-based | Spellbook, performance mapping, support AI, actions |
| A5 | Expiry, death and dismissal/cancellation leave no owned items/effects/commands; owner/sibling state is isolated | Existing lifecycle/quantity fixture; exact ownership and terminal observations | Standalone; affected modes | Lifecycle, equipment, resources, effects, commands |
| A6 | Fresh-process save/load preserves required serialized state, safely resets session state, cleans up; module OFF deserializes safely; baseline remains compatible | Working prepare → cold verify/cleanup → fresh absence, ON/OFF, with original bytes/metadata restored | Installed stack required by Working | Serialized state, registration, lifecycle dependencies |
| A7 | Loading, selection, scale, movement, attack contact, hit/death/despawn and performance are technically usable at gameplay camera | Existing native creature-review/presentation route, agent-inspected captures plus structured facts | Standalone and relevant combined seam | Assets, renderer, attachment, loading, lifecycle |
| A8 | Standalone and directly affected high-risk combined integration work; changed shared consumers retain behavior | Selected existing profile transaction and focused baseline/control cases | Standalone; high-risk combined; additional profile only for concrete dependency | Shared AI/registration/serialization or changed integrations |

## Workflow, review, restoration and next selection

Reusable tooling repair is separately committed: explicit candidate/version binding,
removed concluded-campaign selectors/quotas, shared scenario declarations, protected
admission tests. Optional provider profile has14 positive/negative checks including
nested-junction refusal; default resolution retains its former selection. Two review
P2 findings (nested links/default inclusion) are closed. Full final independent review
remains pending. Body draft read-only review found no binding/lifecycle blocker;
corrected exact mesh pin/provenance and deterministic package's two-file count.

Latest transaction `compat-20261025T233432Z-3cd9044ba52e` restored/verified
23:39:01.1994536UTC. Every new transaction protected Compare PASS:
Mods967/saves118/references18/settings83/tasks0/one unchanged Steam. No owned live
transaction/game/task remains. Original Unity logs copied before later launches.

Working disk floor is measured1.1GB peak plus2GB margin. Verified closed S29
duplicate cleanup23:55UTC recovered1,341,815,888bytes; free4,561,539,072bytes.
All files in16 explicit extraction directories matched retained validated ZIPs.
PlanSHA `1e23bf148b3129a28f9c7bce99925132cfe65e400841080ddec92c6ed4fcc10d`.
Protected Compare PASS Mods967/saves118/references18/settings83/tasks0/Steam1.
Unique ZIPs/raw failures/receipts/backups/saves remain. No active transaction.

Build-Local attempts/validated development packages/native launches:31/21/28.
Latest full build log elapsed254.175s; preceding unused c6 package259.899s.
Fresh wrappers265.507s/259.803s/270.071s (total795.381s); native portions
96.057s/93.497s/103.873s. Focused checks47.569s then correction compile23.5s/tests1s.
Implementation/observer repair overlapped supervised waiting; complete summed
active category totals remain unavailable. No speedup inference. Owner decisions
remain pending; elapsed waiting is not approval.

Independent bounded batch review found two P2 fixture issues. Appearance pulse
readiness is corrected and passes native public loading. External recipient
predicate is corrected to refuse self/carrier-only proof; its new prerequisite
failed, so removal coverage remains pending. No other concrete batch-review
blocker; full final product/rules/coverage review remains required.

Current bounded correction reuses the proven private hostile/wake/combat/native
activation path for lifecycle setup and records state before failure. Quiet-toggle
392 failure remains immutable. Focused compilation25.1s and5 selected checks1.1s
PASS; initial wrong resource namespace compile failure is retained separately.
Independent read-only fixture review found no concrete blocker: private ownership, native activation, exact surviving recipient, retained references and outer teardown are sound. Native retry remains required; no gameplay rule changed.

Next selection: one standalone `support-lifecycle` process against the corrected
candidate, after review/build and verified duplicate cleanup. Target10-15minutes,
ceiling30minutes including orchestration but never interrupting restoration.
Required observations: exact external Courage recipient before each source removal,
native expiry/death, sibling isolation, owned resources/items/effects absent, exact
fixture reset. If setup fails again, use its captured state to narrow the fault;
do not repeat unchanged. Remaining natural casting/presentation/persistence work
continues afterward; no provisional pending subsystem work. Each ordinary selection retains10-15minute target/30minute ceiling and fresh
process/restoration boundaries. Final qualification uses one frozen complete-gate
candidate and a separate records worktree; every final acceptance row is incomplete.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
