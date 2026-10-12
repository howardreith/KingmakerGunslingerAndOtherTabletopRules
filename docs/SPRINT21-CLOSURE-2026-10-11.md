# Sprint 21 closure: qualified for stable promotion

Owner mission issued October 11; qualification completed October 12, 2026 UTC
on **HOWARD-LAPTOP**, in
`C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`, branch
`codex/expanded-summoning-sprint21-bebelith-giant-crab`, PR #36.
Status: **QUALIFIED**. No inherited baseline exceptions and no unexplained
mandatory not-run coverage remain. Merge/publication verification is recorded
separately after it occurs. No Sprint 22 is authorized.

Current [AGENTS.md](../AGENTS.md), [validation policy](VALIDATION-POLICY.md) and
[owner mission](SPRINT21-CLOSURE-OWNER-MISSION-2026-10-11.md) govern this campaign.
Master workflow PR #37 was already integrated: master `a756a498028313e3c95f93882580212080953b0d`,
workflow implementation `42b9644c0f6a71cec9f58dda30fd5a1981acbd3b`. It was not
imported again. The [ledger](../planning/SPRINT21-CLOSURE-LEDGER.json) retains
original source/artifact identities, failed trials, reader versions and proof hashes.

## Exact final candidate

| Item | Identity |
| --- | --- |
| Tested source | `b3d9436fc09736530b5b5962d40982165454ea0b` |
| ZIP SHA-256 | `7ea2630f25ce05d4eb03a1972a6ba56ce6d3bdf58e7cd9f31e3f4a77e5c328ec` |
| DLL SHA-256 | `3fb5741f8e2f16cbac7aec8e626a0289c378657a8b483516bfb57c789fa28161` |
| DLL MVID | `bac5fbd3-9a8b-42c2-876d-d2d54a2560c8` |
| Build-Local manifest SHA-256 | `032743710b711ea46075a59067de32e2ae1055b3a066d020c6ad85d0620e55f6` |
| Source fingerprint | `ee9cccf87af6826592a29b5e8cbe97a85a8e289741a54a883001451ee167d19e` |
| Fresh test executable SHA-256 | `cb79d10691cd75743dd3dd64d703d7677a6607776da7c384c84ea6c5d174fec4` |

The frozen worktree is external evidence root
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint21-closure-20261011T235000Z/candidate-native-qualified`.
Its package is `artifacts/local-runtime/0.0.150/KingmakerGunslinger-0.0.150-local-runtime.zip`.
Its test executable is
`artifacts/tests/Release/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.exe`,
modified `2026-10-12T03:51:47.1531275Z`. Freshness is established by the clean
test-target build, captured successful build/run exit, unfiltered registration
and executable hash, with mtime as supporting evidence.

One `Invoke-KmgGate.ps1 -Level Tranche` pipeline delegated repository validation,
the fresh unfiltered **2571/2571 PASS** suite, exact-reference build and strict
package validation to Build-Local: **273.9 seconds**. Its test build command was
`test-domain.ps1 -Configuration Release -Clean -SkipRepositoryValidation`;
repository validation had already run in that pipeline. No domain tests were skipped.
No extra Sprint/Tranche pipeline or per-profile rebuild followed.

Reporting documents and release tooling postdate the tested source. They do not
change compiled code, assets, blueprint data or version identity. Stable promotion
copies the exact ZIP; its embedded reporting text stays frozen at the tested source.
This closure record and the public release manifest carry current qualification.

## Fresh native evidence and justified reuse

The supervised remaining batch ran **45 fresh native cases**, each with the exact
source, loaded DLL/MVID, package and result hash verified. Every scheduled scenario
executed and every emitted raw assertion passed. Profiles took **81.6 minutes**;
the fourteen missing module configurations took **45.2 minutes**. The earlier
affected mechanics reproduction is separately identified, rather than represented
as part of the same transaction.

| Required coverage | Result and provenance |
| --- | --- |
| Gunslinger-only | Two complete executions: load 3/3, optional compatibility 16/16, inventory 50/50, mechanics 62/62. First mechanics and first base-three observations used separate matching transactions. Final candidate. |
| Call of the Wild | Two executions: load 3/3, optional compatibility 23/23, inventory 50/50, module boundary 47/47. First also ran dedicated Brown-Fur contract 14/14. Final candidate. |
| Arms and Armor | Load 3/3, optional compatibility 16/16, inventory 50/50. Final candidate. |
| Toggle Custom Soundpacks | Load 3/3, optional compatibility 16/16, inventory 50/50. Final candidate. |
| High-risk combined | Two executions: load 3/3, optional compatibility 23/23, inventory 50/50, mechanics 62/62. Final candidate. |
| All 16 original module combinations | Fourteen fresh configurations on final candidate plus exact ON/OFF 47/47 results from `3ee4f9a4040f9295d125369ee99a879d80d51a57`. Boundary observer/coordinator/product dependencies unchanged. |
| Sprint 18/19/20/21 native reviews | Reused 29/39/22/44 PASS from `7b37df94453eaff934d6ec443e04c9c973dc031f`; actual review partials and gameplay/assets unchanged. |
| Exhaustive player paths | Reused 1,063 generated roots and 29 wrappers, with cleanup, from `7b37df94`. Player-path implementation and its production dependencies unchanged. |
| Whole-roster enabled persistence and cleanup save | Reused 125/125 from `cf4bbc4aef48e8bfc65498a24dd55126679f96d2`: 114 units, 113 private resources released, borrowed/cache counts stable, hooks cleared and unrelated state preserved. |
| Module-disabled deserialization and cleanup | Reused 11/11 from `6b1ef853949c521c64ac52882b898bed80665666`. Save/roster backend and resource lifetime dependencies unchanged. |
| Fresh clean-save absence | 6/6 on `3ee4f9a4` under the exact eleven-mod human intake, with no write. Subsequent request-local fixture work did not alter that save/backend. |

Fresh module cases emitted **47 assertions when Gunslinger was on**, and **46
when off**. The only conditional omission is `salesman-repeat-publication`:
the existing observer at `RuntimeTestRunner.MidgameFirearms.cs:135` invokes
publication only when enabled. Calling it in an OFF state would publish forbidden
stock. Disabled-stock absence checks did execute. This is accounted as inactive
behavior, not a hidden skipped mandatory assertion.

The reduced stacks could not hydrate the existing eleven-mod working fixture;
their native `Player.PostLoad` failure occurred before mechanics. The exact human
stack then proved that fixture valid and clean. Reduced-profile mechanics instead
used a guarded, request-local, unsaved native first-level world with the original
mechanical case. All four mechanical runs proved zero save/load attempts, removed
guards/scene observers and restoration of the empty world. No protected save was
accessed and this campaign wrote no gameplay save.

The changed observers were rerun: Bebelith's two primary bites are an armor negative
control, because dismantling requires both claws; the native grapple AutoFailure
precondition is recorded alongside the misleading untouched Success getter;
and visual accounting covers every family registered with the existing patch,
including Crab/Bebelith, with exact events/eligible/attached counts and no ring
saturation. Positive claw behavior remains in the unchanged 44/44 Sprint 21 review.
The inventory observes the actual primary **2d6 bite**, two additional **2d4 claws**,
and zero secondary limbs. The alpha never qualified these changed observers.

## Seven boundary findings

Environment **E0**: startup-only `gunslinger-only`, no optional mods; final
boundary observer source `3ee4f9a4` compared with released-v0.0.149 diagnostic
source `99d9c4644307341c2cd1b006fe67d58f8f3b2d38`. The diagnostic retained
released product `d0ceafae75c1d74a1e0d4fa318193ef002240da1` and added the same
read-only observations; it is **not the original release artifact** and contains
no Sprint 21 gameplay. References, module settings and startup phase matched.
All modules were on except Brown-Fur; Expanded Summoning was tested both ON/OFF.
Steam DLC 900449/900447/900448 were enabled and all four recorded BTSL tables loaded.

Environment **E1**: final candidate with Call of the Wild `1.14.4c-2.1`, all
thirteen modules on including Brown-Fur. Provider-present contract and publication
were verified twice. Other profile references were Arms and Armor `1.0.10` and
Toggle Custom Soundpacks `1.0.1`; combined includes all three.

| Assertion | Expected / observed | Profile | Matched baseline evidence | Changed dependency | Classification / action |
| --- | --- | --- | --- | --- | --- |
| Brown-Fur publication gate | Old fixture required available25 when absent. Absent: unavailable, zero identities/selector/publication. Present: supported25, one unique selector. | E0 absent; E1 present | Same unavailable absent contract on matched149 | Provider-aware fixture; coordinator unchanged | Fixture defect; repaired and native verified |
| Teleport scroll `2c283c993b233df53b487fc7eeac2ba3` | Old donor-icon expectedTrue / actualFalse. All13 required clauses pass, assigned `scroll-of-teleport`. | E0 ON/OFF | Same failed donor-icon clause and passing required clauses | Owned-icon expectation/reporting; spell unchanged | Fixture defect; repaired and verified |
| Greater Teleport scroll `2a2b0185be1d2dba5aa1b5a24774e17d` | Old donor-icon expectedTrue / actualFalse. All13 required clauses pass, assigned `scroll-of-greater-teleport`. | E0 ON/OFF | Same exact subclauses | Owned-icon expectation/reporting; spell unchanged | Fixture defect; repaired and verified |
| Word of Recall scroll `42d3daeb7d503687df8953b47727372b` | Old donor-icon expectedTrue / actualFalse. All13 required clauses pass, assigned `scroll-of-word-of-recall`. | E0 ON/OFF | Same exact subclauses | Owned-icon expectation/reporting; spell unchanged | Fixture defect; repaired and verified |
| Gunslinger capital stock | Historical10 / actual7; contract7, no missing required/unexpected rows | E0 ON/OFF | Same three intentionally retired capital identities | VendorSpec-based fixture; publisher unchanged | Fixture defect; repaired and verified |
| Eastern stock | Historical66 / actual54; contract54, no missing required/unexpected rows | E0 ON/OFF | Same twelve retired capital identities | VendorSpec-based fixture; publisher unchanged | Fixture defect; repaired and verified |
| Spear stock | Historical34 / actual28; contract28, no missing required/unexpected rows | E0 ON/OFF | Same six retired capital identities | VendorSpec-based fixture; publisher unchanged | Fixture defect; repaired and verified |

The scroll reports retain each owning blueprint, expected/actual economics,
teaching, isolation, consumption and art clause. Historical donor-icon False
values and original failed runs remain preserved. Vendor expectations come from
the actual publication specs, not copied observed totals or a DLC hypothesis.
Capital table is `7de959347266092448d8a72089ef9778`. Every retired row has quantity1:

- Gunslinger: `236f3e167f5542bcac22bca72046fb1f`, `6c9cdfa2d47e4894847fa85d5319fbd2`, `a303d71d244640959827e9464df5a867`.
- Eastern: `0db026048052031be931b9701b3859ef`, `1aa01a0528eb595b5cbf19ac7c71a64e`, `35b7082d98ff45ba51dce536a1bc68a1`, `37e8c76b2fc196e9f82e1196e918263c`, `38e31ba5cdbdc668f8dcd8985070c0b7`, `58fd0f272f4523458016dc3656b778c3`, `599f8f45f325911ffcbdbd6544ba114f`, `83a507873a518b54793d0da632def246`, `87b3d851726a4a9abd0baec6beca957c`, `aba40a9e8302b31e4daa2acf6ab48a46`, `b61ee7e62bc9288004eb0121c8f5d37e`, `df5a3b333eab59c04028d88084d7ada9`.
- Spear: `25d8f6c6f4767b3168f4700a2890954f`, `66111becd22690a2a19444a5c6bd0c7b`, `6edc216d68810960f85417237748b042`, `8c0de00a236fe0f532d31711dcaa00a2`, `9c9edabf91f2117fd1b642c4d39b9574`, `b16c34215cae9d60345042157149a4c0`.

Loaded BTSL tables: `a6bae621a7bd96b4fb3c1511cd2f9fac`,
`08e090bb2038e3d47be56d8752d5dcaf`, `45f027c06962df249b8c014a4b4e95e3`,
`420f1da6c2523f64eba810b9b484f60f`. Exact matched multisets, required rows,
subclauses and environment evidence are retained in `matched-boundary-comparison.json`.
The baseline exception policy was not needed; no production defect was waived.

## Restoration and publication boundary

All **36 exact named campaign transactions** are Restored, with verified closure
and no original-backup or staged-quarantine directory remaining. Final transaction:
`compat-20261012T060432Z-5065241e8c21`. The repository's exact manifest matcher
confirmed all **968 actual intake entries/settings/Favored Class bytes and timestamps**.
The managed bank's hash, length and timestamp match intake. No game process or live
compatibility lock remains. The human installation is its actual intake, including
KMG0.0.117; that historical version was not substituted as a guessed target.
The working fixture is clean; no campaign save write occurred.

External evidence includes `remaining-native-qualification-summary.json`,
`remaining-qualification-restoration.json`, `remaining-qualification-all-named-transactions.json`,
and the source/dependency reuse audits. Original reader versions and qualified
f2fe16ca inventory artifact remain intact. Earlier2554/2559, failed2570/2571,
59/62 mechanical observation and setup trials remain FAIL; counts are not combined.

The initial four-hour allocation was revised with measured timings after fixture
diagnosis; the native batch completed safely within the revised finite plan.
No process was stopped to meet a budget. Owner aesthetic review remains
NOT_PERFORMED_NONBLOCKING; mechanical/rig evidence is not visual approval.

Next is the explicitly authorized normal PR #36 merge and exact stable promotion,
then downloaded asset/tag/Latest/alpha and final restoration verification. Preserve
`v0.0.150-alpha.1`, source `adddef438c9bc5798d462d95f6667671ca852344`, ZIP
SHA-256 `e18244e100b7598ccf8c348b7c413da727f63073fad11834fafbd6053feebc06`.
After verified publication and final pushed records, stop. No permanent deployment,
Sprint 22, DATA work or other-repository work follows.
