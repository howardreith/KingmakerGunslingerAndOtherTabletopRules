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

Satyr: adult Fey8/HP44/CL8; native Fear/Charm Person/Sleep, conditional Suggestion;
immune/ineligible/active-effect refusal; bow and dagger/secondary-horns fallback;
printed native feats and racial Perception/Stealth, check-specific Intimidate.
Lillend: adult Outsider7/HP73/AC20; Large +1 longsword and secondary tail, native
10-foot reach; tail-only grab/live constrict on the accepted lifecycle; native
bard7 book5/4/2, independent20-round Courage pool, Move activation, safe healing,
control and ally-safe Sound Burst eligibility; separate one-use Charm Person SLA.
Ten new roots remain hidden;1150 accepted roots remain published. Remaining
skills, pending performances, public routes and full acceptance are incomplete.

Current validated development source `2d8f6f6e284a5e9524982834da8defb0a48a769e`:
ZIP `8403517080d9466103171fdbb8cd6f1c8c1f1b1260522b01d25b893f079447fe`;
DLL `cb8e017ff0bbb37908f7f1ef9773c305e023934d5613fc1d0bb1a9c9dd3b81d1`;
MVID `1f575a5a-4d1e-4355-b07c-3d8088e84ed7`.
SourceState `818b9ce3d1c32e58e17048e438410f411bb0424cdee9c5c340a9a38015d58d0f`.
Canonical path `artifacts/development-candidates/sprint32-lillend-support/2d8f6f6e284a/`.
Offline wrappers,22 selected checks, exact-reference compile, strict419-file ZIP PASS.
This is an investigation candidate, not final qualification.

Fresh optional-provider run `20261025T2137497774325Z-2c9ef7c3098a40d3ae86872f4f0c0a71`
**PASS7/7**: each adult naturally cast native Suggestion once with Standard cost,
failed unforced Will save, CL duration and active-effect refusal. Satyr DC17/CL8,
478.804s; Lillend DC16/CL7,419.509s and own level2 slots4-to3/sibling4. Declared
Fear/performance/other-tier pre-spends isolate the choice and are not cast proof.
Actual expiry, damage break, positive Will/high SR and provider cold load remain
NOT_RUN. Provider1.4.5 existed only in the guarded optional profile.

Fresh standalone run `20261025T2144411971768Z-f526b8825a714c668a9ffc73bba5a4af`
**PASS11/11**: original body binding, adult/book/reach, real Move Courage,
Standard Cure Serious/own slot and native sword/sword/tail fallback. Post-disposal
cleanup now proves62 owned resources gone and158 distinct borrowed resources
retained. Actual grab, sustained performance and full gameplay-camera presentation
remain separate. Earlier cleanup failure is retained unchanged in history.

Fresh Satyr-turn run `20261025T2150018376046Z-b05730f7968c4819aa171c7927f28418`
**FAIL3PASS/1FAIL**, before ready/gameplay: the unchanged ownership guard refused
Kanerah_Capital_prologue, native Neutrals faction, intake/unowned and in combat.
Both turn phases are held. Distance alone cannot prevent native group-memory combat
enrollment. The narrower pending correction uses the proven elemental turn fixture's
private target faction, hostile only to the player faction, with native nonplayer
intake-neutrality checks before advancing. No intake actor or guard is changed.
Independent read-only review found no concrete dispatch blocker in this bounded
correction. Focused compile22.4s/22 tests0.9s PASS; prevention of ambient enrollment
still requires the selected native execution.

Exact raw/result/log/restoration identities are in [history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md).
Local records: `artifacts/sprint32-{suggestion,lillend,satyr-turn}-2d8f6f6e/`.
2d focused compile23.8s/22 tests4.3s and runtime preflight1130 checks PASS.
Independent read-only review found no additional pre-dispatch blocker in that
batch; final independent review and native qualification remain incomplete.

The a8 private Suggestion adapter recognizes only the observed positive Infectious
Charms discovery gate, excludes its foreign actions from private creature copies,
and refuses a caster that owns the discovery using native UnitHelper.HasFact.
Its explicit native caster-level rank repairs the provider's missing rank for its
advertised minutes/level. No provider blueprint changes. Native binding remains
unproved. The prior892 provider failure and live graph explain this bounded adapter;
original FAILs and exact identities remain in history.

Original Lillend body source`cd2c34ec50a04474a239a69a612a5bcf9c38620a` uses the
accepted human/owned-tail seam and unchanged Salamander gait/playback/settlement.
[Authoring/provenance](../assets-source/original-models/sprint32-support/README.md).
Final-source mesh/paint reproduction is byte-identical; three export checks PASS.
Its original Build-Local failure was the child's two-file inventory omission,
corrected in892 with positive/undeclared-file-negative checks. No original FAIL
was rewritten. Earlier bb standalone supportPASS9/9 is reviewed reuse on older bytes;
its providerFAIL4/2 remains preserved with the now-explained branch refusal.

Reviewed earlier evidence: Satyr0723 natural Fear/DC18/one Standard/resource,
full mind-immune bow, close immune native switch/dagger PASS9/9. Queried horns
full-attack plan is not resolved horns proof. Lillend482 first support slicePASS7/7.
Exact tuples and all original failures remain in [history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md).

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

Latest transaction `compat-20261025T214740Z-d96cdeadfd40` restored/verified
21:51:54.5847888UTC. Protected Compare PASS: Mods967/saves118/references18/settings83,
tasks0/one unchanged Steam. No owned live game/task/transaction remains.

Measured combined build/extract/stage/restore peak1.1GB plus2GB practical margin.
Latest approved cleanup21:55:03UTC removed6 verified reproducible closed package
extractions, retaining exact ZIPs/evidence/receipts/backups/saves. Recovered
595,397,174bytes; free3,729,502,208bytes; protected Capture/Compare PASS. Plan SHA
`15b5c1befe185f1e2cf34bba1ad4d53db70b46cf6b2a881d5faabded5521d1a4`.
Local selection/script `artifacts/sprint32-intake/*s32-native-margin*`.
Recheck headroom before each build/staging selection.

Build-Local attempts/validated development packages/native launches:26/17/22.
Latest wrappers340.147s(provider),276.877s(standalone),255.267s(Satyr-turn);
native157.658s/110.499s/93.620s respectively.
Current focused compile23.8s/tests4.3s; earlier measured subsets stay in history.
Approximate current work since21:18UTC is mainly observer repair/review and native
supervision; overlapping work prevents honest summed category totals. Complete
active implementation/observer/build/native/owner-wait totals remain unavailable.

Next: review/compile the bounded private-faction correction, build once and run
Satyr turn; only proceed to Lillend turn if actor ownership is established. Each
is a fresh save-free600-second process,10-15minute target/30minute ceiling with
guarded intake/restoration. If the bounded changed approach still cannot establish
ownership, retain the blocker and continue other authorized acceptance work.
Full final independent review and every final acceptance row remain incomplete.
Pending owner decisions remain pending; no implementation treats silence as assent.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
