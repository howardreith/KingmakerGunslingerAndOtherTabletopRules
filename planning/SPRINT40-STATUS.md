# Sprint 40 — Ice Devil (Gelugon)

IN PROGRESS. No Ice Devil registration, native qualification or release yet.
Owner mission selects completion through exact-artifact stable publication,
public downloads, restoration and merged records. Stop before Sprint 41.

## Current continuation state

- Product owner: this native Goal; branch `codex/expanded-summoning-sprint40-ice-devil`,
  root `C:/Dev/KingmakerGunslingerLab/wt/s40`; reviewed parent `29b9f539`.
- CLI reports `0.166.0`. No surfaced capacity/quota/tool refusal was present;
  the previous capacity-stop cause is unknown. Native Goal is now active.
- Completed: prior intake/range correction/branch additions preserved. Added
  one bounded Sprint40 metadata observer using the existing research scenario,
  graph/view readers, admission and restoration paths. No Ice Devil registered.
- Checks: production compiled; 9/9 selected S39 domain tests; runtime preflight
  1193 checks passed. This is observer admission proof, not creature gameplay.
- Next: freeze this observer commit, build one version-gated development
  package, run its single guarded native-input query, inspect the actual
  cold/Slow/Daze graphs and body/Persistent Image results, then implement.
- Pending owner decision: Persistent Image disposition only if no faithful
  bounded graph is established; the earlier proposal was not approved.
- Deployment/save state: no game or active transaction; original captured
  intake and Working remain the authority. No new intake campaign is selected.

## Intake

- HOWARD-LAPTOP, unelevated HOWARD-LAPTOP\Howard; no Kingmaker process at intake.
- Fetched origin once: master `ffb1f8bf2c0014ee6bb5465103252195f75e449b`.
- Product branch `codex/expanded-summoning-sprint40-ice-devil`, promotion root
  `C:/Dev/KingmakerGunslingerLab/wt/s40`. Original dirty master worktree preserved.
- v0.0.177 remains released; its immutable evidence is
  [SPRINT39-RELEASE-EVIDENCE.json](SPRINT39-RELEASE-EVIDENCE.json).
- v0.0.178 is expected, not reserved or verified free at publication.
- No game/save/deployment mutation has occurred. A current protected intake
  snapshot is required before the first runtime transaction.

## Compact reuse / gap / patch / proof map

| Requirement | Existing consumer | Real gap | Smallest change | Required proof |
|---|---|---|---|---|
| Profile, spear iteratives and secondary naturals | S34 infernal weapons, S29 giant equipment, native natural roles | Spear +1 frost; printed bite +6 versus tail +3 Strength | Exact-owner spear/bite/tail profile and narrow role handling | Native standard/full/charge/AoO/Cleave, disarm/fallback, damage derivations |
| Hit Slow | S35 event identity guards; S38–39 victim contexts; native Slow carrier | Fort/live Con, on-hit spear/tail only; independent of final damage | Small owned production attack component, no poison/enchantment | Real saves and action/movement, miss/bite/zero-damage/unrelated-owner negatives, expiry/immunities/haste |
| Cold offense | Retained Cone of Cold and Ice Storm graphs; released spell-like parameters | Confirm complete delivery/cadence and CL13 consumers | Reuse graphs with own parameters and conservative shape-aware AI | Real cone, physical/cold storm packets, SR/save distinctions, at-will RTWP/TB actions |
| Wall | Released S35 `InfernalWallRules35` / `InfernalWallNavmesh35` | CL12 constants embedded in section creation/duration | Parameterize actual context-dependent values; keep Bone unchanged | CL13 39 HP, 130 rounds, residual 1d6+13; obstruction/breach/expiry, same/cross-species ownership, reload/OFF cleanup |
| Fear/regeneration/flight | S35 Bone fear; S34 native regeneration; Airborne representation | Ice fear radius/DC; good-only regeneration; 60-foot flight | Own profile parameters and exact native consumers | Good suppression/recovery, live fear consequences, one movement grant |
| Persistent Image | Existing illusion evidence and installed library | No faithful scripted figment support established yet | Bounded evidence inspection, then concrete owner disposition if absent | A name miss alone establishes no impossibility; no invented decoy |
| Body/icon | Original infernal body exporters, view ownership, medallion renderer/exporter | No genuine suitable native Ice Devil body established | Distinct original insectile geometry if native evidence is unsuitable; complete icon before bootstrap | Inspect pixels and actual attached body; movement/casts/attacks/selection/death/cleanup |
| One route and identities | Existing allocation/catalog/build/bootstrap/adapters | One adult untemplated SM IX root | Dependency-complete hidden append, then publish only after qualification | Source-derived totals 134 creatures / 1217 choices if no unrelated additions; no Nature/demon-catalog changes |
| Persistence and promotion | S39 mixed scope, group setter/lender, installed-stack/publisher | New scope/assertions/resources and six fresh saved-state boundaries | Extend authoritative admissions and existing orchestration | Nonempty offline dispatch; final-artifact ON/OFF six processes; exact package reproduction/download/restoration |
| S39 carry-forwards | Released Nimbus receipt/condition; Nalfeshnee custom range | Nimbus public wording; Slow medium rather than close | Verify actual condition before metadata correction; own Slow 55 feet at CL12 | Preserve .177 evidence; failing focused reproduction plus actual native range boundary/cast |

## Authorities and unresolved inputs

Owner mission explicitly adopts the S35 bounded movement-wall adaptation here.
It does not authorize new illusion effects or copied poison. External rules:
[Ice Devil](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Ice%20Devil%20(Gelugon)),
[Slow](https://legacy.aonprd.com/coreRulebook/spells/slow.html),
[Persistent Image](https://legacy.aonprd.com/coreRulebook/spells/persistentImage.html).
These describe requirements, not installed-engine capabilities.

The primary creature entry prints bite 2d6+6 and tail 3d6+3 alongside the
manufactured spear routine; this needs explicit native role verification.
The rider is a hit-triggered supernatural effect, not an injury poison.
Persistent Image disposition is pending inspection and owner decision if needed.

## Qualification limits

No PASS is claimed from this intake or map. No frozen artifact exists.
Unchanged consumers may retain explicitly attributed prior proof; every
mandatory new Ice Devil row requires fresh final-artifact execution.

## Development evidence, 2026-11-05 UTC

- Nalfeshnee-only Slow now uses `SlowRangeFeet(CL) = 25 + 5 * (CL / 2)`
  through the existing custom-range mechanism. CL12/13 give 55 feet; CL14
  gives 60. Other spells and other creatures are unchanged. The existing
  four-spell native declaration observer now expects close range for Slow.
- Failing reproduction: `KMG_TEST_FILTER=expanded-summoning.sprint39-ranges-scale-with-caster-level`
  with `scripts/test-domain.ps1 -SkipRepositoryValidation`: 1 selected of
  2,810, failed `Expected=55; Actual=220` (5.84 seconds including compilation).
  This exercises the range function bound to the production builder, not
  native targeting. Local log: `artifacts/s40-slow-range-before.log`.
- `scripts/Invoke-KmgGate.ps1 -Filter expanded-summoning.sprint39`:
  production compilation 25.9 seconds; focused domain build/run 4.1 seconds;
  9/9 PASS, 2,810 registered. Actual native footprint/range/command proof is
  still REQUIRED; no release correction is claimed complete from constants.
- Push-policy backup:
  `C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1.sprint40-backup-20261105T002130Z`.
  Before SHA-256 `dd65013eb97edeea8719689ce5a3c68231d6921a618a28660717ac57ed2e70d8`;
  after `28cf3aa465c184534d5809ac387e59a78443f8a9f23c1dbc7ed55d69f3a80579`.
  Byte comparison verified only the two exact branch additions. Existing
  isolated wrapper tests passed 38/38; six exact Sprint40 positive/case/suffix
  cases were then added to the same harness. The extended harness passed
  44/44, including both exact Sprint40 names, their case/suffix refusals and
  the push-before-refusal negative control. Local output:
  `artifacts/s40-push-policy-tests.log`.
- Fresh protected intake:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint40-intake-20261105T002300Z`.
  Contains current Mods/reference manifests, registry settings, all 118 save
  hashes/metadata, and `working-original.zks`. Its read-only catalog check
  passed. No prior version/hash was substituted. The intake helper initially
  supplied the wrong named cleanup argument; its original process exited,
  no game/deployment/settings mutation occurred, and the exact read-only
  lease was completed using `Exit-KmgRuntimeLease -Scope` after owner-exit,
  identity and no-restoration-debt checks. Journal:
  `runtime-20261105T002259Z-d2d78ab484b741808806086a7194d1bf`, Completed.
- Closing read-only comparison passed: installed Mods, private references,
  all save bytes/creation/write metadata/attributes and game settings exactly
  match this fresh intake. Local receipt: intake directory's
  `closing/comparison.json`. Kingmaker absent; no shared lock remains. No
  installation or save restoration write was needed because none was changed.

## Remaining work and boundaries

No Ice Devil body/icon or mechanics are registered. Continue with the compact
map above, complete dependencies before hidden bootstrap, and inspect the
retained cold spell/Slow graphs plus the bounded unresolved installed inputs.
Persistent Image omission was proposed conditionally to the owner; no approval
has been received and no omission is selected. Nimbus metadata remains
unchanged pending direct condition-consumer evidence (the existing S39
observer's `dazed` boolean tests buff presence, not the native condition).

The Ice Devil implementation, affected Nalfeshnee native range observation,
final-artifact qualification, all six persistence processes, product merge,
publication/download verification and records merge remain outstanding.
There is no qualified package, MVID, tag or public Ice Devil route yet.
