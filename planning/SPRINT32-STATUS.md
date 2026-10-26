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

Satyr has the adult Fey8/HP44/CL8 profile, Fear/Charm Person/Sleep, conditional
Suggestion, eligibility/refusal policy and bow/dagger/secondary-horns fallback.
Lillend has adult Outsider7/HP73/AC20, Large +1 sword/secondary tail with accepted
shared grab/constrict, bard7 book5/4/2, independent20-round Move Courage,
healing/control and ally-safe Sound Burst eligibility. Ten legal roots are
published for development;1150 prior roots remain. Product scope and final
qualification are incomplete, including pending decisions above.

Current validated development candidate is `b8743418c1b3c6e6ca50ff97ca7664648e1311e4`:
ZIP `57b384dc47668934872d7c1146e4b0cce0b2e39b2e7214e6abb70e20cdf308e8`;
DLL `10f68f11852872d9f8c1713d22034195c5fedc0656f9bf5f5dde8b15f0b7b63a`;
MVID `4cd5f211-4200-42ec-bb35-1a5e0f0a5194`;
SourceState `bb46d9a2cf2b46fd308d1572bd91b7ed168a650234b7da412099c129156b41c6`.
Path `artifacts/development-candidates/sprint32-lillend-support/b8743418c1b3/`.
Wrappers,557 selected/2753 registered checks, exact-reference compile and strict
419-file package PASS. Development version0.0.167; no release qualification.
Current observer corrections are newer source, not execution on that package.

| Evidence | Result and dependency limit |
|---|---|
| Fresh b874 support lifecycle | PASS10/FAIL2 overall FAIL. Both expiries, player+2/enemy safety and isolated siblings pass. Both dead owners have no captured grants/areas, but each retains native self RecentlyDeadBuff with2.326game seconds remaining when the10wall-second observer stops. Later death/expiry assertions not reached. `artifacts/sprint32-support-lifecycle-b8743418/` |
| Fresh b874 Druid input | FAIL1/cleanup PASS2, before casting: live c0_before interchapter in the inherited mansion scene. Strict UI refusal is correct; fixture selection is wrong. Wizard/Sorcerer NOT_RUN after common failure. `artifacts/sprint32-druid-input-b8743418/` |
| Reviewed reuse:2d90 RTWP12/12 and turn10/10 | Natural Move Courage and actual ally+2/enemy exclusion; Standard self-healing18-to0/own level3 slots2-to1/sibling2; useful fallback; completed sword/sword/tail acts; live Nature and sibling isolation. Player-ally healing remains unverified. |
| Reviewed reuse:392 public prepared routes | Satyr six SNA tiers and Lillend four SM tiers; legal quantities, one slot per cast,120s duration, body/icon/selection. Helper pumps native commands; no natural-input timing claim. |
| Reviewed reuse:Satyr control and optional Suggestion | Fear, immunity refusal and bow/dagger/horns fallback in both modes. Provider Suggestion natural DC17/CL8 Satyr and DC16/CL7 Lillend, normal actions/duration/own slot and active-effect refusal. Positive Will/high SR, expiry/damage break and provider cold load NOT_RUN. |
| Body evidence | Independent Lillend authored bindings;62 owned objects released/158 borrowed retained; recognizable rest views. Full gameplay camera motion/contact/hit/death/despawn/performance acceptance remains pending. |

Exact tuples, raw failure paths, reader corrections, original logs and review
findings are retained in [append-only history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md).
Original Lillend [provenance](../assets-source/original-models/sprint32-support/README.md)
and functional-first cosmetic policy remain; no owner aesthetic approval claimed.

## Finite acceptance contract

All final rows remain incomplete. Early slices above do not replace final qualification.

| ID | Observable result | Route and evidence | Mode/profile | Invalidated by |
|---|---|---|---|---|
| A1 | Adult identity, defenses, attacks, legal facts, spell lists; exact legal tiers and quantities; existing routes retained | Domain catalog/route tests; native composed-profile and inventory observations | Offline; standalone native | Catalog, builders, registration, adapters |
| A2 | Both public summons pay real cost/slot, produce legal count and duration with usable body/icon; distinct prepared/spontaneous/adapter paths covered where materially different | Existing player-route helper with bounded Sprint 32 cases; structured slot/spawn/expiry receipts | Standalone; affected casting-source integrations | Publication, costs, quantities, duration, adapter generation |
| A3 | Satyr naturally uses useful control without pipes; rejects immune/ineligible targets and avoids repeats; attacks when control is unavailable | Existing disposable first-area fixture and exact-owner AI/command observers; control/refusal/fallback records | RTWP and turn-based | Satyr rules, targeting, shared AI/action handling |
| A4 | Lillend naturally supports with safe buffs/performance and healing; resources/slots and actions debit correctly; ordinary offensive fallback remains useful | Same bounded native fixture; native action/effect/resource observations | RTWP and turn-based | Spellbook, performance mapping, support AI, actions |
| A5 | Expiry, death and dismissal/cancellation leave no owned items/effects/commands; owner/sibling state is isolated | Existing lifecycle/quantity fixture; exact ownership and terminal observations | Standalone; affected modes | Lifecycle, equipment, resources, effects, commands |
| A6 | Fresh-process save/load preserves required serialized state, safely resets session state, cleans up; module OFF deserializes safely; baseline remains compatible | Working prepare â†’ cold verify/cleanup â†’ fresh absence, ON/OFF, with original bytes/metadata restored | Installed stack required by Working | Serialized state, registration, lifecycle dependencies |
| A7 | Loading, selection, scale, movement, attack contact, hit/death/despawn and performance are technically usable at gameplay camera | Existing native creature-review/presentation route, agent-inspected captures plus structured facts | Standalone and relevant combined seam | Assets, renderer, attachment, loading, lifecycle |
| A8 | Standalone and directly affected high-risk combined integration work; changed shared consumers retain behavior | Selected existing profile transaction and focused baseline/control cases | Standalone; high-risk combined; additional profile only for concrete dependency | Shared AI/registration/serialization or changed integrations |

## Current work, restoration and next action

The separately identifiable reusable workflow repair retains explicit candidate/
version binding, shared scenario declarations, guarded staging/restoration and
positive/negative admission checks. Concluded-campaign selectors/quotas no longer
block Sprint32. Prior independent tooling findings were corrected. Final coherent
product/rules/coverage review is still pending.

After two input setup failures, the bounded replan reuses the proven non-DLC Oleg
scene only for the three fixed input phases. No dialog is dismissed or ignored.
The lifecycle observer now waits up to30wall seconds for normal game-time expiry,
with strict zero source grants unchanged and both clocks captured. Lillend's
existing support case adds exact player-faction ally healing, real action/slot
cost and sibling isolation; wounded caster position/damage restore in finally.
Independent read-only review found no concrete dispatch blocker. Focused production
compile22.8s and5/2753 tests4.3s PASS; original wrong focused-reference invocation
refusal retained. A subsequent expected-text clarification needs no broader tests;
selected candidate build compiles it. These corrections need fresh native runs.

Latest transaction `compat-20261026T013016Z-54db8e796163` restored/verified
01:34:39.4716972UTC; lifecycle transaction restored01:29:13.3055930UTC.
Both protected Compare PASS: Mods967/saves118/references18/settings83/tasks0/
one unchanged Steam. No owned live transaction/game/task remains. Original Unity
logs are retained before another launch; protected saves and current intake exact.

Build-Local attempts/validated development packages/native launches:35/25/36.
Latest build253.122s; latest two wrappers279.332s/264.476s, native113.613s/89.694s.
Complete summed active work categories remain unavailable; observer research/repair
and supervision overlapped. No speedup inference. Owner subsystem waiting is not
approval. Working disk minimum remains measured1.1GB peak plus2GB margin; check
and recover only verified closed extraction duplicates before the next batch.
Verified two-transaction cleanup at01:41:28.6623828UTC recovered198,615,570B;
free3,795,656,704B. Original ZIPs,
raw evidence, receipts, unique artifacts and protected backups remain; Compare
PASS967/118/18/83/tasks0/Steam1. Exact plan receipt is in history.

Next selection: one coherent development package, then `support-lifecycle`,
`lillend-support`, `druid-input`, and, only after common setup succeeds,
`wizard-input`/`sorcerer-input`, each a fresh guarded process. Expected aggregate
25-35minutes plus build; each ordinary selection retains10-15minute target/
30minute ceiling and complete restoration. No unchanged input loop.
Continue remaining control/support risks, technical presentation, cold ON/OFF
persistence and affected integration; pending subsystem rows stay pending.
Final qualification requires independent coherent review and one frozen complete-
gate candidate, with separate records where identity guards require them.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
