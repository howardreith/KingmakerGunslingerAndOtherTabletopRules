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
- Owner explicitly authorized omitting Lillend's printed Darkness SLA after
  disclosure of the missing illumination/vision subsystem. No lighting subsystem
  or substitute fog effect is included; the three-per-day ability remains absent.
- Only guarded Steam640820 transactions; immutable baseline save, protected
  current human installation/saves/settings/references. Exact fresh intake and
  restoration. Frozen source/package for native work; records worktree at final
  qualification where needed. No weaker identity checks.

## Current product and candidate

IN_PROGRESS. Satyr's hidden Fey8/CL8 Fear control and native bow fallback now
have an early natural RTWP PASS, including frightened-immune refusal. Lillend's
hidden outsider7/native bard7 slice adds independent spontaneous healing slots,
20 native performance rounds, Inspire Courage+2 and normal melee fallback.
Ten new roots remain hidden; all1,150 accepted generated roots stay published.
Full adult attacks/feats/spell families, optional Suggestion, original Lillend body,
public routes and final qualification remain incomplete. Both original128px
icons are exported with provenance; agent-inspected Lillend icon is recognizable.
Native presentation remains NOT_RUN; the human Lillend carrier is investigation
only, not an owner-approved final proxy.

Satyr early PASS artifact: source `12d0bcfc8964dbc5aac0f13efb701c5631fd9787`.
Development purpose `sprint32-satyr-control`, retained version0.0.167; not release.
ZIP `KingmakerGunslinger-0.0.167-development-sprint32-satyr-control-12d0bcfc8964.zip`
SHA256 `093d29864e528e8ff05c46c3c42e885eff414c5079bc98a25db5f6fda0259d74`;
DLL `26ae962b40bc40120b470940ae0554d362f5afdd1527eec2f6129877df0f73b1`;
MVID `c993efa4-2209-4409-b983-04e6100b8ac8`.
Complete offline wrapper, exact compile, selected Satyr route test and strict416
file package PASS; unchanged-domain553/553 at986b8de9 is reviewed reuse, not fresh
execution on12d. Native run `20261025T1620422768377Z-c0ee2fe7d3e34cbd909d74ddecc12da3`
PASS6/6: real Fear CL8/DC18, failed native Will/Frightened, oneStandard action,
resource1?0; immune target scores0 and induces real bow fallback while retaining
Fear resource1. No fixture cast/attack, forced roll or direct resource debit.

Original c43 missing menu mapping and986 native armor type initialization FAILs
remain immutable. The exact BlueprintUnitFact lookup was corrected after the
second startup failure; focused independent review cross-checked native input
types and fixture assumptions before12d launch. The35f184d0 precompile refusal
(stale derived roster row) and zero-test filter refusal remain distinct from
native gameplay failures. Current Lillend compile PASS; related domain554 tests
initially exposed stale current roster/proxy declarations, now corrected. The
previous553 passing assertions are retained; the affected proxy test rerun PASS.
Those offline checks alone established no native or final qualification.

Current early support PASS candidate `4827296ea941906bee3c1d715491c01c72747bc9`,
purpose `sprint32-lillend-support`, retained0.0.167. Complete wrapper, selected
hidden route, exact compilation and strict417-file package PASS. Related554-test
execution on7e16 is reviewed reuse, not fresh execution of482.
ZIP SHA256 `ddbf6b71b7d3fa312f164f4e0bfb57237a82bc14a9c5c67ec2d0b7075c4b1303`;
DLL `0114b3d2cbdf13b4aa327707c379fd7c740ed590cbe202564ca339c698bae88f`;
MVID `11946561-f273-412a-8128-a0db99df8fdd`.
Native `20261025T1739131530378Z-ccd48e42bfa1400cb6797bf881c8e092` **PASS7/7**:
CL7/slots5/4/2/rounds20; one original AI Courage command/Move/round20->19;
owned ally+2 and enemy exclusion; natural Cure Serious heals18 damage to0 with
one Standard action/ownL3 slot2->1; healthy/running-performance refusal produces
a normal full attack, two weapon events and one native action cost. Sibling
slots2/rounds20 remain unchanged. No fixture casts/attacks/debits/forced rolls.
The exact private same-instance performance guard corrected the demonstrated
native command replacement. Independent read-only review found P1/P2 addressed
in the actual482 diff; native command-start evidence confirms one original AI
activation. Sustained-round consumption, turn-based, lifecycle and final body
remain separate acceptance work. Original7e/8af/2e FAIL results are retained.

## Finite acceptance table

Final rows remain incomplete. A3 has the early RTWP execution above; A4 has
the early natural support/fallback PASS above. Other final
gameplay/visual/persistence/integration coverage remains NOT_RUN.

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
Next early probe uses the same guarded scenario, `reviewPhase=lillend-support`,
standalone RTWP, one hostile and two fresh independent Lillends. Observe native
Move-action Courage/ally+2/round debit; meaningful self-heal/Standard/one ownL3
slot; healthy/running-performance refusal and natural melee fallback.10?15min
including restoration,600-second game deadline. No owner input or save mutation.
Final sequence duration will be declared before dispatch. Ordinary target 15 min,
ceiling 30 min; repeat failures or about 1 hour without product evidence trigger
replanning, not a quota or a new approval ledger.

## Evidence, restoration and next action

[Historical workflow/early evidence](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md)
retains all original results, exact artifact/cleanup identities and limitations.
Local logs: `artifacts/sprint32-intake/`; Satyr native wrapper/intake:
`artifacts/sprint32-control-12d0bcfc/`. Raw runtime-evidence directory:
`20261025T1620421820687Z-disposable-expanded-summoning-sprint32-native-review`.
Result SHA256 `a3b1a24765b4523597264ee8b2f61b43a01fa35cda48455182fd98fcab1ce3f4`.
Transaction `compat-20261025T161820Z-4742246b5c09` Restored/verified at16:22:49UTC;
exact module settings restored and protected Compare PASS Mods967/saves118/
references18/settings83/tasks0/unchangedSteam. No owned live transaction/game.
Original986 failure log remains in `artifacts/sprint32-control-986b8de9/` with
SHA256 `0e05a7365a12c41991f82841f94d0bb8f550ebf1555bfa5f90735a84ce63061f`.
Metadata-only native audit is reviewed input evidence, not mechanics.

Latest Lillend wrapper/intake `artifacts/sprint32-lillend-7e16bf24/`; evidence folder
`20261025T1653581258798Z-disposable-expanded-summoning-sprint32-native-review`.
Result SHA256 `f0eb2387002aeb7f5f2d498641097a0a787055558b3c18c0132f3e51e4407cb9`;
original Unity failure log `4ae34ecef9fa7e23a4b55664118be259555b453cb1a606cb58b4fa3a606bc52b`.
Transaction `compat-20261025T165106Z-edd4bc213ad9` Restored/verified16:55:35UTC;
full protected Compare PASS967mods/118saves/18refs/83settings/0tasks/unchangedSteam.
No owned live transaction/game remains. Native70.778s; full transaction270.60s.
Latest8af wrapper/intake `artifacts/sprint32-lillend-8af31322/`; evidence folder
`20261025T1706471321780Z-disposable-expanded-summoning-sprint32-native-review`.
Result SHA256 `e7aaa6537e47fe042bdaaa6ae2b44236a9677cb8fa029e3e63cb798999b76068`;
original Unity failure log `5c5b657b5aea1e74a515acd1854df4e742289c69f55303e76679d0cf84623ab4`.
Transaction `compat-20261025T170420Z-2871883ca6d8` Restored/verified; protected
Compare PASS967mods/118saves/18refs/83settings/0tasks/unchangedSteam. No owned live
transaction/game. Native67.856s; full transaction233.46s.

Independent read-only reviews completed for workflow, early Satyr fixture and
packaging corrections; concrete early fixture findings repaired. Final coherent
product/rules/coverage/evidence review remains pending. Provider 1.4.5 public ZIP
retained for research; no provider installation/integration yet.

Storage: verified closed duplicate cleanup recovered 3,511,693,023 plus
1,344,249,872 plus504,093,702bytes; unique packages/raw evidence/backups/saves retained.
Additional four closed Sprint32 extraction duplicates recovered393,168,768bytes;
all exact packages/failure logs/receipts retained. Cleanup plan SHA256
`0839095c060c231715170b7d0b41123f564b7ec0d97c3e0b92224fd64f1962ab`;
protected Compare PASS. Free after cleanup3,690,156,032bytes. Free space
before482 dispatch3,349,237,760bytes; after restoration3,139,276,800bytes; required allowance 1.1 GB peak plus 2 GB margin.
Recheck before next package/dispatch. A full extra checkout measures 1,019,350,022
bytes, so do not casually duplicate it; final records-only checkout can be bounded.

Build attempts/validated development packages/native launches:14/8/8.
Observed native setup/execution/restoration255.68 +224.24 +238.49 +270.34 +270.60 +233.46 +289.04 +276.85 seconds. Full active-time
category totals unavailable; retain timestamps, do not infer throughput.
Repeated blockers were old admission/count assumptions, now exercised through
actual offline/package checks. The corrected registration assumptions are closed by12d natural gameplay PASS.

Admission retry note: preflight first run refused its no-artifact-change assertion
because concurrent authorized cleanup wrote receipts. This is orchestration
interference, not gameplay; isolated rerun PASS1112 including exact Lillend
positive and malformed/no-exit negative cases. Original refusal is retained.

Focused independent Lillend review completed: actual native input types,
WithUnitCommand start/cancellation, Dialog initial round/area/buff controller
admission and fixture assumptions have no remaining static dispatch blocker.
Native execution must prove actual costs/effects; successive performance rounds
are explicitly outside this first-debit probe and remain final acceptance work.
Icon catalog30/30 and canonical137-source/export validation PASS; all136 prior
rows remain identical. Production compile and corrected hidden/proxy test PASS.

Latest raw evidence folder `20261025T1739131217766Z-disposable-expanded-summoning-sprint32-native-review`;
result SHA256 `ffca6995724f9cb68349c1c39e27e878d58ad321d37a6a5c092bb75df7517708`.
Wrapper/current-intake/profile receipts: `artifacts/sprint32-lillend-4827296e/`.
Transaction `compat-20261025T173647Z-2aef333cbcf8` Restored/verified17:41:23UTC;
protected Compare PASS967mods/118saves/18refs/83settings/0tasks/unchangedSteam.
No owned live transaction/game remains. Native109.797s, transaction276.85s.
Six further closed S29 duplicate extractions recovered503,971,440bytes;
plan SHA256 `d165d9a36f49373d2e0bd808e9d492f1617b07a5f50a58088cf8844eea4d6a48`.
Exact ZIP/receipts/failures/backups retained, protected Compare PASS; free3,498,569,728.

Next: complete bounded adult attacks/feats and supported spell families,
conditional Suggestion adapter, and original Lillend body; batch uncertain native
graph observations with affected product proof. No owner decision is pending.
The full independent final product/rules/coverage review remains pending.
Prior v0.0.167 gaps remain **NOT_RUN**: grouped all 30 public-route/body/icon,
standalone final integration, highest-risk combined final integration. Their
[original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
