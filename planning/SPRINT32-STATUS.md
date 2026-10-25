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

Current validated development source `892eafcca4d64eef59c696b3cb624d25e826a133`:
ZIP `1917c869f217c392c2870af91f09d6cdcef9c352cbc4b0edd822a3be1d1f4547`;
DLL `d2322cdf36b3f7c33e8387af2ea53b9d3840109b2525f0e7d6c888b184496611`;
MVID `a7bf554c-c8bd-40ab-8690-d129228e6d9a`.
Canonical path `artifacts/development-candidates/sprint32-lillend-support/892eafcca4d6/`.
Offline wrapper,21 selected checks, exact-reference compile, strict419-file ZIP PASS.
This is an investigation candidate, not final qualification.

Fresh standalone run `20261025T2043252830276Z-496e5b3f8bc54475bbf31d03f626402a`
**FAIL9PASS/2FAIL**: both independent original bodies loaded with finite poses and
borrowed actions preserved; adult/book/reach, Move Courage and Standard healing
passed. Native sword/sword/tail events occurred, but the attack ended Interrupt,
Courage stopped at19 rounds, and the success-only fallback assertion failed.
Tail playback itself completed1/1/1 without a reported failure. Native grab can
produce CantAct, attack interruption and performance stop; this receipt lacks
CMB/owned-hold evidence, so the actual cause remains unproved. Original result is
retained. Body cleanup assertion after the exception was NOT_RUN; world/installation
restoration passed. Agent inspected rest/tail-event art-camera frames: recognizable
body, no severe deformation seen; not gameplay-camera/contact qualification.
Raw folder `20261025T2043251748303Z-disposable-expanded-summoning-sprint32-native-review`;
local log/intake/inspection `artifacts/sprint32-lillend-892eafcc/`.

Fresh provider run `20261025T2053487013587Z-620dfd0b3542414cba8e140a497c0cf0`
**FAIL5PASS/2FAIL**: healthy Cow1.4.5/CotW1.14.4c-2.1, independent bodies and
books loaded, but Suggestion binding remained refused. The deeper live graph
identified CotW's positive Infectious Charms wizard-discovery branch appended
to failed-save actions. Base Suggestion is still the investigated native ApplyBuff.
Raw folder `20261025T2053486670343Z-disposable-expanded-summoning-sprint32-native-review`;
local original log/intake/derived graph `artifacts/sprint32-provider-892eafcc/`.
Original results and exact result/log hashes are appended in history.

Current unqualified batch recognizes that exact positive discovery gate, excludes
its foreign branch from the two private creature copies, and refuses a caster
unexpectedly granted the excluded wizard discovery. It tightens duration/lifecycle
checks and supplies an explicit native live-caster-level rank for the provider's
advertised minutes/level (Cow1.4.5 supplies no rank component; installed native IL
has no default CL rank). No provider blueprint is changed; no new subsystem.
The ordinary Lillend fallback fixture now uses explicitly owned native maneuver
immunity; real grab remains a separate required case. The established bounded
iterator wrapper observes body cleanup even after retaining an earlier failure.
New Satyr/Lillend turn phases reuse native enrollment, automatic AI and exact
turn action costs; no supplied casts/attacks/rolls/debits. These cases are NOT_RUN.
Production compilation41.9s and22 selected tests4.4s PASS; runtime preflight
1124 checks PASS. The review found that discovery membership must include Logic
facts, so the guard now calls the exact native UnitHelper.HasFact surface; its
final compilation is part of the next candidate gate. The first focused command
was refused precompile for its incompatible ReferenceBundleDir argument; corrected
ordinary compile used the existing GamePath.props. Both logs remain preserved.

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

Latest transaction `compat-20261025T205121Z-f5b26e0e9599` restored/verified
20:56:46.0116520UTC. Protected Compare PASS: Mods967/saves118/references18/settings83,
tasks0/one unchanged Steam. No owned live game/task/transaction remains.

Measured combined build/extract/stage/restore peak1.1GB plus2GB practical margin.
Authorized cleanup20:50:17UTC removed seven complete byte-identical closed S29
package extractions, retaining ZIPs/evidence/receipts/backups/saves. Recovered
588,064,472bytes; free3,620,425,728bytes, protected Capture/Compare PASS. Plan SHA
`f88148495eb89ca3922999d5606f467bcbeaa5e46dfc71718652ca318dd136ca`.
Selection/script/plan in `artifacts/sprint32-intake/*s29-controls*` and
`verified-cleanup-20261025T205016Z/`. Post-provider free3,408,973,824bytes.
Recheck before the next build/native selection; retain the practical margin.

Build-Local attempts/validated development packages/native launches:24/15/17.
Latest native setup/execution/restoration wrappers313.88s(standalone),325.75s(provider);
inside-game137.743s/151.587s. Focused compile41.9s/tests4.4s. Complete active-category
totals unavailable; timestamped subsets and earlier counts remain in history.
Two provider failures were not unchanged retries: the second captured the live graph
and explained the branch; adapter correction is now based on that observation.

Independent read-only review of the coherent adapter/fixture/turn diff and actual
live graph is complete: discovery-membership finding corrected; no remaining
concrete dispatch blocker. This is not final sprint review/native qualification.
Next: build once, then reuse its exact ZIP for corrected
standalone body/support, provider binding/support, Satyr turn and Lillend turn checks.
Each is a fresh save-free600-second process,10-15minute target/30minute ceiling,
guarded intake/restoration. No full presentation, real grab, Suggestion cast/save or
persistence claim follows merely from binding. Pending owner decisions remain pending.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
