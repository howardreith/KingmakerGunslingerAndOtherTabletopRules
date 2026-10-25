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
- Lullaby: native lookup absent; sustained concentration lacks a proven native
  contract. Owner decision requested in conversation: Lillend-only sustained
  concentration (estimated8-20 active hours), recommended legal cast-and-immediate-
  release policy with7-round tail (2-5hours), or explicit omission. No Lullaby
  implementation while pending. Independent work continues; no silence assumed.
- Exact branch addition in sanctioned push wrapper authorized and applied.
- Guarded Steam640820 transactions only. Immutable baseline save; exact fresh
  human-installation/save/settings/reference intake and restoration. No weaker
  source/package identity checks. Final frozen candidate and separate records
  worktree where required.

## Current product and evidence boundary

Latest development source`098b3b5c9804d6cca51807cbb678f7657aba3e1d` passed the
complete offline wrapper,14 selected tests, exact-reference compile and strict
417-file package validation. ZIP SHA256
`0a559c627c5857024624af5d989372c3f81c91f11fb9a37a39292e422639ad26`;
DLL`97ed9c05dacaf310707e6a1e8cee5231deafe3afa1e5ded46cd4ca4ab12dea74`;
MVID`ba454bcd-64cc-4824-93ab-d6213f341e05`. Its fresh standalone Lillend run
`20261025T1937208911636Z-f075c2b513e0457589b627bc8ff572be` **FAIL** during
blueprint initialization, before gameplay assertions: the new Charm Person SLA
resource reused the spell's localization prefix with a different description.
Product defect, corrected by using the resource's own existing identity prefix;
no rule, native observer or protection check changed. Original Unity log
`artifacts/sprint32-lillend-098b3b5c/output_log-original.txt`, SHA256
`8c2a19f341979f6c16a922bd393d67e47f94fc82e8dd840d7ba9f469b86586da`.
The affected standalone selection below must rerun on the corrected package.

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
`satyr-adult-related-domain-corrected.log`. The bounded fresh native proof is recorded below.
Current validated Satyr candidate: source`0723b5dc2a8d9568f687763c740336177b4c6666`,
purpose`sprint32-satyr-control`, development0.0.167, never a release.
ZIP SHA256`4d841d01e3186fc9e72f582f356fcf5a1580ea9687c510ca476cc101940666c8`;
DLL`75f037b0190d061dbd4b4cd328d00c8c0a4c9d7510dcab756ec710fd287204f3`;
MVID`01308e95-07b0-4a4c-8368-a029deb61642`. Complete offline wrapper, exact compile,
2 selected tests and strict package validation PASS. Related554/554 is reviewed
reuse from the unchanged product batch, not fresh testing of this observer DLL.
Run`20261025T1846192242029Z-9aa5cf603327442e9dd60e0a664f6451` PASS9/9:
natural Fear/DC18/one Standard/resource1to0; full mind-affecting immunity makes
Fear/Charm/Sleep scores0 and produces a real bow attack; close immunity produces
a native Move hand-set switch followed by a real dagger Standard attack.
Queried plans exclude distant horns and include melee dagger+secondary horns;
this is not a resolved two-hit full attack. No fixture casts/attacks/forced rolls
or supplied resource debits. Positive Fear also left a queued, unstarted Charm;
final active-effect/cancellation coverage must inspect its later disposition.
ResultSHA`5e4a95b2f616a582cdbc1f7dc683c71bb0c842d4402e2590fce3667cdd66b215`.
Raw folder`20261025T1846191828893Z-disposable-expanded-summoning-sprint32-native-review`;
wrapper/intake`artifacts/sprint32-control-0723b5dc/`. Native106.841seconds;
setup/execution/restoration270.96seconds. Final RTWP/TB/public/visual/persistence
qualification remains incomplete.
The d49 concrete-type and6d native-dictionary-serialization observer FAILs remain
immutable in raw evidence and [history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md).
After two failures, dispatch paused for a complete new-receipt/fixture check,
installed-JSON round-trip, compile and independent read-only review. Explicit
JSON properties fixed the demonstrated converter boundary; the reviewed retry
above establishes fresh gameplay proof. No creature rule changed for the fixture.

The next unqualified product batch adds Lillend's Large2d6 secondary tail,
same-size-or-smaller grab, live2d6+Strength constrict/maintain, native trip
immunity, Combat Casting and extraplanar consumer. Private type range9 maps
through installed native range getters to10 feet at Large and preserves live
size/reach changes; no range Harmony patch. Satyr receives printed racial
Perception/Stealth+4, six Perception ranks and check-specific Intimidate-6;
unprinted Mobility ranks and Lillend Stealth ranks are removed. Full remaining
skill reconciliation is still pending. Three private identities append; all
7,451 prior entries are unchanged. Current registry binding is updated without
changing the published binding or protected icon bytes. Compile PASS23.5seconds,
selected route tests2/2 PASS4.2seconds; related summoning554/554 PASS15.2seconds
(`artifacts/sprint32-intake/fey-celestial-adult-related.log`). Native/visual proof
of this batch is NOT_RUN.
Original missing-namespace compile failure is retained; corrected log:
`artifacts/sprint32-intake/fey-celestial-adult-tail-corrected.log`.

The coherent unqualified batch also fills native Daze, Charm Person/Sleep,
Hold Person/Invisibility/Sound Burst, Cure Light/Serious and a private Charm
Monster using native Charm control with broadened eligible creature types and
CL days. A separate CL7/DC15 innate Charm Person owns one use independently of
bard slots. AI checks native immunity, eligible HD/conditions, occupied Sound
Burst ally footprint, healthy targets and existing invisibility. Satyr/Lillend
Charm choices consult the existing exact alignment-control protection policy.

Conditional Suggestion now has guarded private ability/buff identities and a
first-UMM-update adapter for the pinned CowWithHat provider. The private spell
and Satyr SLA use its native avoid-combat effect only when active; absence
leaves them unavailable. Saved known-spell/effect reconciliation distinguishes
pending from settled absence, never Resting slots. The provider graph is checked
before guarded clones are staged. Provider-present execution and cold-load
hydration ordering remain NOT_RUN. Bardic Suggestion performance, Fascinate,
Competence, Lullaby disposition, remaining skills and the original Lillend body
remain incomplete. The human carrier remains investigation-only.

This full batch appends31 private entries; all7,451 prior entries are unchanged.
Icon provenance/current registry binding PASS; no approved icon pixels or
assignments changed. Compile PASS22.7seconds and14 selected routes/protection
checks PASS0.9seconds (`suggestion-adapter-fixture-focused.log`). Related566
selection passed565 and found one historical-type-filter defect: the shared
Suggestion buff fell outside the old creature-specific exclusion. The exact
historical baseline filter now excludes the Sprint32 namespace; its affected
case freshly PASS1/1 (`suggestion-adapter-identity-corrected.log`). The original
FAIL remains in `suggestion-adapter-related.log`; no product rule was changed.
Earlier missing-namespace/API compile failures also remain in their original
logs. Final graph-type guard is included in the next candidate compile.

Independent read-only review found three adapter defects (unguarded partial
binding, insufficient nested graph validation, premature absence cleanup).
Staging complete guarded clones, bounded native graph checks and a settled
state fixed them. The corrected standalone/provider-absent support fixture
has no review dispatch blocker. Provider-present/cold-load proof remains pending.

Next ordinary selection: frozen development purpose`sprint32-lillend-support`,
standalone RTWP, existing save-free600-second phase. Observe actual composed
adult HP/AC/abilities, sword/tail reach, known-spell tiers and provider-absent
state, then natural Courage/heal/immune melee fallback with real costs and
independent siblings. Target mind immunity and ally proximity make all expanded
control choices ineligible; no product rule is altered to force fallback.
Expected10-15minutes including packaging/setup/restoration, ordinary ceiling30.
This is not final spell, tail-grapple, provider-present, TB, visual or save proof.
Both original128px icons retain provenance; native presentation remains NOT_RUN.

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

Completed early selection: existing guarded save-free Sprint32 review,
`reviewPhase=satyr-control`, standalone RTWP,600-second game deadline.
Fresh Satyr per positive Fear, fully control-immune bow fallback, and close
control-immune native hand-switch/dagger case. Query native full-attack plans
to check distant horns exclusion and dagger+secondary-horns mapping; a queried
plan alone is not a resolved full attack. Capture only the existing selected
Charm/Sleep/Fascinate nested graphs more deeply in this same process.
Completed within its10-15minute target including restoration, ceiling30minutes. Final sequence
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

Latest runtime transaction`compat-20261025T193501Z-8e384f0cffa0` restored/verified
at19:38:46.7387637UTC. Protected Compare PASS: Mods967/saves118/references18/settings83/
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

Follow-up authorized cleanup18:52:07UTC: seven closed Sprint32 extraction
directories were byte-identical to their retained validated ZIPs; recovered
688,413,824bytes, free3,863,474,176bytes. PlanSHA
`69756aed35e6d1eb5de5bff80f7165a0a316835c2fdc4a98e5d851f052341bc4`.
Selection/execute logs: `artifacts/sprint32-intake/cleanup-s32-followup-*`.
Fresh protected Capture/Compare PASS; all unique packages/raw evidence/original
failures/receipts/backups/saves retained. No owned runtime or task remains.

Build-Local attempts/validated development packages/native launches:19/12/12
before the next selected candidate. Native setup/execution/restoration times:
255.68+224.24+238.49+270.34+270.60+233.46+289.04+276.85+254.32+268.05+270.96+227.35seconds.
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

Next: complete remaining adult profiles, Lillend's bounded performances/spells,
conditional Suggestion and original body; group the next coherent product checks. Lullaby decision pending; no Sprint33.
Prior v0.0.167 gaps remain **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
Their [original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
