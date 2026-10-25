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
published. Public native route qualification, pending performances and full
acceptance remain incomplete. The cc artifact below predates publication.

Current validated development source `cc74175e033a79d8f483764749e48ca2a3e975ca`:
ZIP `8455caf8e98121484e306db25716b2e47d8ee932eeedee1524d18e09eb2b0ecf`;
DLL `85be717720bf30b6ab7d40b576588f51aed01bb58450e87e033a0d5e0f2364e7`;
MVID `2715abbd-148a-4354-8892-b0742ae9717e`.
SourceState `fa0faf6acf7420a959ac0d345dcfdc8fdaf47f208017ad94c905baaa3f8b7cad`.
Canonical path `artifacts/development-candidates/sprint32-lillend-support/cc74175e033a/`.
Wrappers,22 selected/2753 registered checks, exact-reference compile and strict
419-file package PASS. This is a development candidate, not final qualification.

Fresh cc native executions, standalone:

| Case | Result | Observed behavior | Local evidence |
|---|---|---|---|
| Satyr turn | PASS11/11 | Nature+10/live Int/sibling isolation; native Fear and one resource; immune bow; Move hand switch then actual dagger/horns completed plan with normal full-round debit | `artifacts/sprint32-satyr-turn-cc74175e/`; raw folder `20261025T2243084094213Z-disposable-expanded-summoning-sprint32-native-review` |
| Lillend turn | PASS10/10 | Nature+9/live Int/sibling isolation; Move Courage, ally+2, exact native upkeep20-to19-to18 in owner rounds1/2; actual sword/sword/tail completed plan; separate Standard healing/own slot2-to1, sibling2 | `artifacts/sprint32-lillend-turn-cc74175e/`; raw folder `20261025T2248529144541Z-disposable-expanded-summoning-sprint32-native-review` |

Both attack traces prove final native Success at the actual plan's end before
terminal Interrupt. This proves mechanical completion, not animation completion
or the exact interruption caller. Private-faction ownership guards pass without
modifying intake actors. Original clock/terminal failures remain immutable.

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

Reviewed reuse on earlier bytes:2d optional-provider Suggestion PASS7/7 and
standalone Lillend support/body cleanup PASS11/11;0723 Satyr RTWP control/refusal/
fallback PASS9/9. The cc product change is only Nature; new attack tracing is
turn-only. These old executions are not fresh cc qualification. Suggestion proved
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

Latest transaction `compat-20261025T224634Z-b01d75f1c126` restored/verified
22:51:01.6995512UTC. Protected Compare PASS: Mods967/saves118/references18/settings83,
tasks0/one unchanged Steam. No owned live game/task/transaction remains.

Measured combined build/extract/stage/restore peak1.1GB plus2GB practical margin.
Latest approved cleanup22:33:47UTC removed16 verified reproducible closed S29 package
extractions, retaining exact ZIPs/evidence/receipts/backups/saves. Recovered
1,342,958,720bytes; free4,722,253,824bytes; protected Capture/Compare PASS. Plan SHA
`f744c0747df66a7131490f7c1d7ee6fd531f4fbf6edd5b4ddc3f0017c7094890`.
Local selection/script `artifacts/sprint32-intake/*s29-review-margin*`.
Recheck headroom before each build/staging selection.

Build-Local attempts/validated development packages/native launches:29/19/25.
Latest cc wrappers279.810s(Satyr)/269.268s(Lillend); native114.782s/109.281s.
After both transactions free4,158,525,440bytes; recheck before staging.
Current focused compile27s/tests1s; earlier measured subsets stay in history.
Approximate current work since21:18UTC is mainly observer repair/review and native
supervision; overlapping work prevents honest summed category totals. Complete
active implementation/observer/build/native/owner-wait totals remain unavailable.

Current source batch: publish ten candidate routes; native Druid20 Satyr and
Wizard20 Lillend prepared-route/body/icon checks, plus fixed four-summon lifecycle
selection over the existing fixture. Full summoning focused check PASS557/2753,
production compile24.4s/domain22.6s. Admission script PASS1148 in isolation; its
first execution correctly detected concurrent compilation and remains FAIL,
classified as orchestration interference, with no guard changes.

Next selection: commit/package this coherent development batch once, then run
standalone satyr-routes, lillend-routes and support-lifecycle as three fresh
guarded transactions. Each is planned at10minutes,600s runtime ceiling; the
three-stage sequence is predeclared at30minutes plus mandatory safe restoration.
Inspect public gameplay-camera captures before further presentation expansion.
Keep pending subsystem scope pending. Batch coherent changes before another
candidate; ordinary native selections retain10-15minute target/30minute ceiling,
fresh-process boundaries and guarded restoration. Final qualification remains
separate and uses one frozen full-gate candidate plus a records worktree.
Full final independent review and every final acceptance row remain incomplete.
Pending owner decisions remain pending; no implementation treats silence as assent.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
