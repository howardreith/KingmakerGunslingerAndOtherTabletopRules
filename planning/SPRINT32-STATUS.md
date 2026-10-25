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

Current validated development source `a8b151da0c754078e254c3d1df996807a3993759`:
ZIP `3282955822cb5a941733e788d8ef56577649d7bd24297890641bbab831a995df`;
DLL `13b585f1a25ed9dfe2e7d089cdf6d7149381f1cb7482cbb10b11706340fd4c2b`;
MVID `d5488ad1-c652-4e37-8edf-9c1d6e13036d`.
SourceState `a306c5fc2fc47870ad73cd134d72992afaf3259c226e677068dde53d7cadd700`.
Canonical path `artifacts/development-candidates/sprint32-lillend-support/a8b151da0c75/`.
Offline wrappers,22 selected checks, exact-reference compile, strict419-file ZIP PASS.
This is an investigation candidate, not final qualification.

Fresh standalone run `20261025T2116353873942Z-40483acd79b1482bb8c53c7c9b39a76b`
**FAIL10PASS/1FAIL**: original body binding, adult/book/reach, real Move Courage,
Standard Cure Serious/own slot and native sword/sword/tail fallback all PASS.
Fixture-only native maneuver immunity isolated ordinary attacks: native Success,
Courage retained19 rounds and ally effect, tail1/1/1 with finite poses. Actual grab
still needs its own observation; this does not retroactively attribute892's failure.
The post-disposal eight-frame wait ran after the core restored intake turn mode;
the exact-owner clock refused ambient combat participants. Body-cleanup assertion
remains NOT_RUN. World/installation restoration PASS. Original log/result retained
under `artifacts/sprint32-lillend-a8b151da/` and raw folder
`20261025T2116352978122Z-disposable-expanded-summoning-sprint32-native-review`.

Fresh Satyr-turn run `20261025T2122366772199Z-55fb5f416fb04588926126de4bc183f0`
**FAIL3PASS/1FAIL**, before ready/gameplay assertions: one uncovered live combat
participant triggered the same native-clock ownership guard. Its actor type was
not captured; do not infer a product failure. Raw folder
`20261025T2122366383276Z-disposable-expanded-summoning-sprint32-native-review`;
original log/intake under `artifacts/sprint32-satyr-turn-a8b151da/`.
Two clock-coverage failures triggered a replan, not unchanged retries. Provider
dispatch on this candidate was withheld to avoid the known cleanup error. Lillend
turn was never launched because parent review found a target-ownership overwrite.

Current unqualified fixture batch owns mode until all phase cleanup frames finish,
retains one hostile across Lillend turn cases, and moves the initial hostile40m
away before setup ticks. Source comparison with the proven Trumpet fixture exposed
the initial near-party setup assumption; Satyr's exact actor cause remains unknown.
The existing coverage description is now included if the preserved guard refuses.
One fixed optional-Suggestion phase observes both adults' natural casts, normal
Will/DC/action/slot costs, CL duration and active-effect refusal; declared resource
pre-spends and target immunities isolate choices. Actual expiry/damage break,
high SR and cold load remain pending. No new rules subsystem is added.
Focused production compile23.8s and22 tests4.3s PASS. Runtime preflight1130 checks PASS. Independent
read-only review of this batch found no additional dispatch blocker. The reviewer
confirmed the mode/ownership fixes and separate setup-versus-cast evidence; the
prior missed hostile overwrite is acknowledged. Native execution remains required.

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

Latest transaction `compat-20261025T212011Z-5b3399521219` restored/verified
21:24:30.8378934UTC. Protected Compare PASS: Mods967/saves118/references18/settings83,
tasks0/one unchanged Steam. No owned live game/task/transaction remains.

Measured combined build/extract/stage/restore peak1.1GB plus2GB practical margin.
Latest approved cleanup21:08:44UTC removed13 verified reproducible closed S29 package
extractions, retaining exact ZIPs/evidence/receipts/backups/saves. Recovered
1,091,765,064bytes; free4,501,954,560bytes; protected Capture/Compare PASS. Plan SHA
`093e456ec58d1c00ac8d92fcb5cb639941ed5282ac2495d77bf73e131dfc3fc9`.
Local selection/script `artifacts/sprint32-intake/*s29-control-margin*`.
After the two latest native runs free3,921,842,176bytes; recheck before staging.

Build-Local attempts/validated development packages/native launches:25/16/19.
Latest native wrappers278.27s(standalone),260.34s(Satyr-turn); native111.842s/95.012s.
Current focused compile23.8s/tests4.3s; earlier measured subsets stay in history.
Approximate current work since21:18UTC is mainly observer repair/review and native
supervision; overlapping work prevents honest summed category totals. Complete
active implementation/observer/build/native/owner-wait totals remain unavailable.

Next: build this reviewed batch once and reuse its exact ZIP for optional provider
Suggestion casts first, corrected standalone body cleanup and the corrected turn
paths. Provider support is expanded only if a concrete combined seam requires it. Each is a fresh save-free
600-second process,10-15minute target/30minute ceiling, guarded intake/restoration.
If turn ownership still fails after the changed setup, retain the precise actor
receipt and pause that approach while independent authorized work continues.
Full final independent review and every final acceptance row remain incomplete.
Pending owner decisions remain pending; no implementation treats silence as assent.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
