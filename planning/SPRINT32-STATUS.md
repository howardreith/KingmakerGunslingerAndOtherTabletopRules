# Sprint 32 - Satyr and Lillend Azata

Owner Howie Reith; mission2026-10-25. This is the sole active operational record.
Finish at an independently reviewed, natively validated PR. No merge, release,
Sprint33 or new subsystem without its decision.
[Validation policy](../docs/VALIDATION-POLICY.md) governs ordinary selections.

## Ownership, scope and decisions

Accepted intake master: `48e6fb4abfc154f111a4019a806cb79a4184bd1c`.
Branch: `codex/expanded-summoning-sprint32-satyr-lillend`.
Worktree: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/.worktrees/expanded-summoning-sprint32`.
[Draft PR74](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/74).
HOWARD-LAPTOP owns mechanics and shared gameplay integration. DATA's separate
released-creature art and all other worktrees remain untouched. Review relevant
accepted dependency/renderer changes once before freeze.

Charter v1.0 sections3/4/7 and S32: Satyr SNA IV-IX, Lillend SM VI-IX;
quantities1/1d3/1d4+1; dependencies5,29-31. Preserve native actions, costs,
ownership, cleanup, saved identities and accepted ordinary-map flight.
No general performance, flight/navigation, AI or summoning-feat expansion.

- Execute and inspect ordinary native/technical visual validation unattended;
  no owner playtest or screenshot-approval dependency. Aesthetic approval separate.
- Ask before a materially new subsystem. No safe-withdrawal subsystem authorized.
- Owner: omit Countersong and Darkness, disclose both. No lighting subsystem or
  substitute fog; Lillend's printed three-per-day Darkness remains absent.
- Suggestion is conditional on active CowWithHat's Custom Spells; omit when
  absent. Source/clone research authorized. Its fixed avoid-combat/fascinate-until-
  harmed behavior, minutes/level, Will and SR are the conditional adaptation;
  it does not implement unrestricted tabletop instructions.
- Exact branch addition in sanctioned push wrapper authorized and applied.
- Guarded Steam640820 transactions only. Immutable baseline save; exact fresh
  human-installation/save/settings/reference intake and restoration. No weaker
  source/package identity checks. Final frozen candidate and separate records
  worktree where required.

## Current product and evidence boundary

IN_PROGRESS. Ten new roots remain hidden; all1,150 accepted roots stay published.
The current source batch expands Satyr's Fey8/HP44/CL8 profile with dagger,
secondary horns, Mobility/Skill Focus, native Charm Person/Sleep, bounded native
immunity/target eligibility and native melee/ranged switching. A private
out-of-reach horns filter preserves native melee and unrelated actors.
Noncombat Dancing Lights/Ghost Sound are filtered under the charter.
Production compile PASS; selected Sprint32 routes2/2 and related summoning554/554
PASS after reconciling the single stale test-surface count. The original13
count/offset failures remain in the earlier log. All7,437 previous manifest
entries are unchanged;14 private identities append. Evidence:
`artifacts/sprint32-intake/satyr-adult-melee-fixture.log` and
`satyr-adult-related-domain-corrected.log`. Native proof of this batch is pending.
The selected d49e76dc candidate passed its complete offline wrapper, exact
compile,2 selected tests and strict package validation, then the added metadata
reader failed before gameplay: `RequireExact` was incorrectly given a base type.
This observer defect is corrected with the six observed concrete types. Research
failure will remain FAIL while allowing independent gameplay cases to execute.
Source `d49e76dcb0c34b00c4bd26830a763f6e6df152a9`; purpose`sprint32-satyr-control`;
ZIP SHA256`fd1cec4108af8a3dd6454a13b3a1f62f888b8c9504cb324c63149798a0d52124`;
DLL`168239ee43f6df82e9e1372afe67dd668bb0f65adad96bff8367fa0177434dc8`;
MVID`fd8a5c47-886a-4b79-918d-3ab17b360ad0`. Original run
`20261025T1817496513281Z-4a5770cedb3849a9b61b6d1e21b4f000` remains FAIL,
resultSHA`2510aba5a0615aa2d45e5b63496203ce6dd9d133d96c7c69dcbb79e7777c9932`.
Wrapper/failure log: `artifacts/sprint32-control-d49e76dc/`; preserved Unity log
SHA`61efdf851166d78af9ca2e0f5bdd9421338132794fffe39fc56816b12eb8ca45`.

Current validated Satyr candidate: source`6d28b102eb28ee68b7ef87db983b23de15b2e08d`,
same purpose/development version. ZIP SHA256
`dc8d40acedd710b77d03e77170ff70ca40de87c8e1fee0cd19eb0addf094ea68`;
DLL`5a22767ad46b98c096abe2983282fecd6f90686f14b948e083d089eecf60d748`;
MVID`58a2d64f-0572-4741-953b-78c86d2b4254`. Complete offline wrapper/package PASS.
Run`20261025T1831274351804Z-132fca0f84c54079a5e926b1edb6fa86` captured the
selected native graphs, then failed before gameplay: native JSON defaults encode
dictionaries as arrays, so `JObject.FromObject(controlScores)` threw. This is an
observer defect; affected gameplay remains NOT_RUN. ResultSHA
`811a229c18d9c86216694859a63b5969c9ed2a36605c166bb7f830daf234e974`;
preserved Unity logSHA`aaef004d1888c9cab80a762010fbb9940adc034d36cfae0ea6f214a407b20a64`.
Raw folder`20261025T1831273973740Z-disposable-expanded-summoning-sprint32-native-review`;
wrapper/intake`artifacts/sprint32-control-6d28b102/`.
Replan after the two added-observer failures: pause repeated dispatch; inspect
all new receipt construction and fixture assumptions together, replace the one
default dictionary serialization with explicit JSON properties, round-trip it
using installed Newtonsoft.Json, and obtain read-only fixture review before
another candidate. The explicit three-score object round-trip PASS is a reader
construction check, never gameplay proof. No product stats change in this fix.
Corrected observer production compilation PASS25.4seconds and selected domain
tests2/2 PASS0.9seconds: `artifacts/sprint32-intake/satyr-explicit-score-receipt.log`.

Lillend has outsider7/HP73, private native spontaneous bard7 slots5/4/2,
Cure Light/Serious,20 owned native performance rounds, Courage+2, normal melee
fallback and independent equipment. The retained human carrier is investigation
only, not an approved final proxy. Full profiles/spell families, conditional
Suggestion, original Lillend body, public paths and final qualification remain
incomplete. Both original128px icons have provenance/export checks; technical
native presentation is NOT_RUN.

Last validated Lillend candidate: source
`4827296ea941906bee3c1d715491c01c72747bc9`, purpose`sprint32-lillend-support`,
development0.0.167, never a release. ZIP
`KingmakerGunslinger-0.0.167-development-sprint32-lillend-support-4827296ea941.zip`;
SHA256`ddbf6b71b7d3fa312f164f4e0bfb57237a82bc14a9c5c67ec2d0b7075c4b1303`;
DLL`0114b3d2cbdf13b4aa327707c379fd7c740ed590cbe202564ca339c698bae88f`;
MVID`11946561-f273-412a-8128-a0db99df8fdd`.
Complete offline wrapper/exact compile/selected route/strict417-file package
PASS. Domain554/554 at7e16 is reviewed reuse, not fresh on482.
Native run`20261025T1739131530378Z-ccd48e42bfa1400cb6797bf881c8e092`
PASS7/7: original native AI Move command/one cost, first performance debit20to19,
owned ally+2/enemy exclusion, natural Cure Serious/one Standard/ownL3slot2to1,
healthy/running-performance refusal and ordinary attack; sibling pools intact.
No fixture-issued casts/attacks, forced rolls or supplied slot/resource debits.
ResultSHA`ffca6995724f9cb68349c1c39e27e878d58ad321d37a6a5c092bb75df7517708`.
Raw folder`20261025T1739131217766Z-disposable-expanded-summoning-sprint32-native-review`;
local wrapper/intake`artifacts/sprint32-lillend-4827296e/`.
This early PASS does not qualify sustained performance, other modes or full profile.

Satyr source12d0bcfc's early Fear/DC18/one resource+Standard/bow fallback PASS6/6
remains evidence for that slice. Its old fear-only immunity fixture is replaced
for the expanded capability; affected native results must rerun. Exact earlier
artifacts and original failures remain in [history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md).

## Finite acceptance table

A1 has focused source/compile checks; A3/A4 have early RTWP slices above.
All final rows remain incomplete. Native/visual/persistence/integration coverage
must be executed on its bound candidate; old execution is never fresh evidence.

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

Next ordinary selection: existing guarded save-free Sprint32 review,
`reviewPhase=satyr-control`, standalone RTWP,600-second game deadline.
Fresh Satyr per positive Fear, fully control-immune bow fallback, and close
control-immune native hand-switch/dagger case. Query native full-attack plans
to check distant horns exclusion and dagger+secondary-horns mapping; a queried
plan alone is not a resolved full attack. Capture only the existing selected
Charm/Sleep/Fascinate nested graphs more deeply in this same process.
Target10-15minutes including restoration, ceiling30minutes. Final sequence
duration will be declared before selection. Two repeated orchestration failures
or about one active hour without useful evidence triggers a narrower approach.

## Workflow, review, storage and restoration

Reusable tooling repairs are separately committed: explicit candidate/version
binding, removal of concluded S30-31 path/quota selection assumptions, one
affected scenario declaration source. Safety positive/negative preflight1112
PASS in isolation. One earlier artifact-change refusal during concurrent cleanup
remains an orchestration failure, not native gameplay.

Independent read-only reviews covered workflow and early Satyr/Lillend fixtures.
The Lillend same-command defect is repaired in482 and natively confirmed.
Current Satyr review found no static dispatch blocker; its concrete melee/full-
attack coverage limits caused the bounded added case/plan observations above.
Full coherent final product/rules/coverage/evidence review remains pending.

Latest runtime transaction`compat-20261025T182857Z-05cb51dd4c22` restored/verified
at18:33:24UTC. Protected Compare PASS: Mods967/saves118/references18/settings83/
tasks0/one unchangedSteam process. No owned live transaction/game remains.

Required free space: measured1.1GB combined build/extraction/staging/restoration
peak plus2GB working margin. Latest authorized duplicate cleanup verified13
closed S28 package extractions byte-for-byte against retained validated ZIP
`4d806ea4d235db95a945ee93b0af4a47c0b92f50ecd9ee44ddb98b6b021c2f12`,
recovered1,087,285,303bytes; free4,225,912,832 at18:03:13UTC.
PlanSHA`3d12f22c4c5a4f3dfdb65f2e39661b1a4d078853fbffcf9ec38f8d9fce674fbb`.
Protected Compare PASS; unique ZIPs/raw evidence/original failures/receipts/
backups and saves retained. Prior cleanup receipts remain in history and
`artifacts/sprint32-intake/`. Recheck before package/dispatch.

Build-Local attempts/validated development packages/native launches:17/10/10
before the next selected candidate. Native setup/execution/restoration times:
255.68+224.24+238.49+270.34+270.60+233.46+289.04+276.85+254.32+268.05seconds.
Lillend482 native109.797seconds; full wrapper276.85seconds. Complete historical
active-time category totals are unavailable. Current work retains timestamped
compile/test/build/wrapper logs; do not infer throughput from unlike sprints.
Repeated failures: obsolete admission/count assumptions, two native registration
type/flag defects, and the native activatable same-command defect; all original
results retained. The latter is closed by482's useful natural gameplay PASS.
The4246 candidate attempt stopped before compilation/native staging because
the icon catalog's current registry hash still described the previous ledger.
Rebind that current hash to the verified14-entry append; protected icon bytes
and assignments remain unchanged. Original log: `build-satyr-adult-4246c336.log`.

Next: finish the adult Satyr batch and its selected native proof; inspect deeper
control graphs, then complete Lillend's bounded remaining rules, optional
Suggestion and original body. No owner decision is pending. No Sprint33.
Prior v0.0.167 gaps remain **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
Their [original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
