# Sprint 32 - Satyr and Lillend Azata

Owner Howie Reith; 2026-10-25 unattended workflow/Sprint32 mission. This is the
sole active operational record. Finish at an independently reviewed, natively
validated PR. No merge, release, Sprint33 or new subsystem without its decision.
[Validation policy](../docs/VALIDATION-POLICY.md) governs ordinary selections.

## Ownership and scope

Accepted intake master `48e6fb4abfc154f111a4019a806cb79a4184bd1c`.
Branch `codex/expanded-summoning-sprint32-satyr-lillend`; HOWARD-LAPTOP owns
`C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/.worktrees/expanded-summoning-sprint32`.
[Draft PR74](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/74).
Other worktrees/untracked work preserved. DATA's separate released-creature art
is outside this worktree; no unaccepted DATA assets touched. Shared gameplay
integration is owned here; reconcile any later renderer/loading overlap before
freeze. Review later relevant accepted dependencies once at that checkpoint.

Charter v1.0 sections 3/4/7 and S32 read: Satyr SNA IV-IX; Lillend SM VI-IX,
quantity propagation1/1d3/1d4+1; dependencies5,29-31. Preserve native costs,
actions, ownership, cleanup, saved identities and accepted ordinary-map flight.
No general bard/performance, flight/navigation, AI or summoning-feat expansion.

## Accepted decisions

- Unattended tests, native observations and agent technical visual inspection;
  no owner playtest or screenshot-approval dependency. Aesthetic approval separate.
- Ask before a materially new subsystem. No safe-withdrawal subsystem authorized.
- Owner: omit Countersong. Suggestion is conditional on active **CowWithHat's
  Custom Spells** and omitted when absent; source inspection/cloning authorized.
  Its actual fixed avoid-combat/fascinate-until-harmed behavior is used, with
  minutes/level, Will and SR. It is not unrestricted tabletop instructions.
- Only guarded Steam640820 transactions; immutable baseline save, protected
  current human installation/saves/settings/references. Exact fresh intake and
  restoration. Frozen source/package for native work; records worktree at final
  qualification where needed. No weaker identity checks.

## Current product and candidate

IN_PROGRESS. Hidden Satyr adult/Fey8 shell, private one-use CL8/DC18 Fear,
conservative eligibility/ally safety and native bow fallback implemented.
Six roots hidden; all 1,150 accepted generated roots remain published. Full Satyr
melee/feats/SLAs, optional Suggestion, Lillend implementation/body, public routes
and final qualification remain incomplete. Original icons have provenance;
only Satyr has a runtime export. Native presentation remains NOT_RUN.

Latest built source `986b8de9ef96a87d832ad9ef86827e65e1a9dd4d`.
Development purpose `sprint32-satyr-control`, retained version0.0.167; not release.
ZIP `KingmakerGunslinger-0.0.167-development-sprint32-satyr-control-986b8de9ef96.zip`
SHA256 `6042437853500e3d539ad2fb03134742b71860917911577d3bebf8643af78681`;
DLL `f89d87cd97f04ff2d36d10e8fea9e9a7176ab51c3751b4a484a5f987e3976dcf`;
MVID `f4a058c6-9655-4b40-a0da-9e020d6d4949`.
Complete offline wrapper, exact compile,553/553 summoning-domain checks and
strict416-file package PASS. Native run
`20261025T1608320243890Z-6bfd690ced414184bfc6dd38f879f0e9` FAILED at
initialization: captured natural armor5 is BlueprintUnitFact, but the builder
requested BlueprintFeature. Zero creature assertions; no AI result.

Prior c43 native initialization failure (missing Satyr menu mapping) remains
FAIL; repaired mapping and affected historical test scopes passed the553 checks.
The35f184d0 precompile refusal was the stale derived Satyr roster row, regenerated
before986b8de9. Original logs/receipts remain immutable.

Replan after two distinct registration failures: correct the exact armor fact
lookup, inspect every remaining Satyr native lookup against the captured inputs,
and obtain a focused independent contract/fixture review before another launch.
The exact lookup is corrected. Focused production compilation and the actual
`sprint32-hidden-satyr-routes` test PASS. An initial misspelled filter selected
zero tests and correctly refused; its log is retained and is not a pass.
Independent focused reviewer completed the exact input/type and fixture
cross-check: no additional concrete blocker; donor/Fey8, facts, Fear, bow and
private registration types agree with the immutable capture. Gameplay remains
unproved. Do not infer native PASS from compilation or review.

## Finite acceptance table

All final rows remain NOT_RUN. Early registration failure above is retained;
no creature gameplay/visual/persistence/integration PASS is implied.

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


Early probe: existing save-free `disposable-expanded-summoning-sprint32-native-review`,
`reviewPhase=satyr-control`, standalone RTWP, one resettable hostile and fresh
Satyr per positive/immune-fallback case. 10-15 minutes including restoration;
600-second game deadline. Observe real Fear command/DC18/resource/action and
immune refusal with actual bow event. No issued cast/attack or forced roll.
Final sequence duration will be declared before dispatch. Ordinary target 15 min,
ceiling 30 min; repeat failures or about 1 hour without product evidence trigger
replanning, not a quota or a new approval ledger.

## Evidence, restoration and next action

[Historical workflow/early evidence](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md)
retains all original results, exact artifact/cleanup identities and limitations.
Local logs: `artifacts/sprint32-intake/`; latest native wrapper/intake/log:
`artifacts/sprint32-control-986b8de9/`. Raw result directory under runtime-evidence
starts `20261025T1608319212635Z-disposable-expanded-summoning-sprint32-native-review`.
Original output log SHA256 `0e05a7365a12c41991f82841f94d0bb8f550ebf1555bfa5f90735a84ce63061f`.
Transaction `compat-20261025T160603Z-137ce53251c1` Restored/verified at16:10:00UTC;
full protected Compare PASS Mods967/saves118/references18/settings83/tasks0 and
unchangedSteam. No owned live transaction or game process remains. Original
metadata-only native audit is reviewed input evidence, not mechanics.

Independent read-only reviews completed for workflow, early Satyr fixture and
packaging corrections; concrete early fixture findings repaired. Final coherent
product/rules/coverage/evidence review remains pending. Provider 1.4.5 public ZIP
retained for research; no provider installation/integration yet.

Storage: verified closed duplicate cleanup recovered 3,511,693,023 plus
1,344,249,872bytes; unique packages/raw evidence/backups/saves retained. Free space
before986 dispatch3,642,130,432bytes; required allowance 1.1 GB peak plus 2 GB margin.
Recheck before next package/dispatch. A full extra checkout measures 1,019,350,022
bytes, so do not casually duplicate it; final records-only checkout can be bounded.

Build attempts/validated development packages/native launches:9/3/3.
Observed native setup/execution/restoration255.68 +224.24 +238.49 seconds. Full active-time
category totals unavailable; retain timestamps, do not infer throughput.
Repeated blockers were old admission/count assumptions, now exercised through
actual offline/package checks. Current failures are diagnosed registration assumptions, not gameplay.

Next: finish the focused native-contract review, commit the armor lookup
correction, build one corrected candidate and rerun the bounded native probe.
Then implement/prove Lillend support. No owner decision is pending.
Prior v0.0.167 gaps remain **NOT_RUN**: grouped all 30 public-route/body/icon,
standalone final integration, highest-risk combined final integration. Their
[original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
