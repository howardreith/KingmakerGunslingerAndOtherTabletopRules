# Magic Circle PR 19 review hardening

R1 and R2 are implemented and qualified. C1 was reproduced and fixed through an
exact-identity adapter, then qualified through native selection, commit and fresh
persistence. This is an installable review candidate within the recorded scope.
It is not a public release or a new claim about every earlier feature scenario.

Review base: `11039183780a7815e97024ae99abc3062abd9ef9`.
Branch: `codex/magic-circle-alignment-spells`.
[Draft PR 19](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/19).
The source/publication correspondence is recorded in `PR19-COMMITTED-SOURCE.json`
after the coherent source commit. No merge or history rewrite is authorized.

## R1: exact preparation before mutation

Previously, the wrapper accepted preparation A, native cleanup could find a
structurally similar B by fixed names, remove it and write Working, and only then
fail the wrapper comparison. That late comparison was an audit masquerading as
authorization.

The request now contains immutable preparation-record, successful-result and
producer loaded-build-identity bytes with their hashes, plus the pinned consumer
artifact. It validates the exact Working descriptor and artifact identities.
The native phase synchronously compares the complete persisted fixture identity
before any deliberate fixture mutation, refresh, cleanup or save authorization.
Names are supplementary. Cleanup additionally requires the authorization flag;
all existing save-name, descriptor and write guards remain. The post-run
comparison remains an audit. Producer and consumer identities are separate,
so a corrected runner can process an exact older preparation without pretending
to be its producer. Both must have the expected semantic version.

Three production binding tests cover two valid distinct preparations, rejection
before mutation/write callbacks, the matching case, malformed/mismatched data,
artifact and save mismatches, and immutable ownership of the snapshot. Request
regressions cover missing bindings, hashes, duplicate keys and exact transport
through JSON. The reviewed constructor accepted a missing cleanup binding.
An intermediate serializer omission was reproduced for all three bound phases
and corrected. Native Newtonsoft null-string versus parsed-null tokens are
normalized through the complete JSON representation; no identity is omitted.

The decisive native regression used successful A and B preparations with disjoint
actor identities. Cleanup requested A while B was loaded. It failed specifically
with `loaded prepared fixture mismatch`, with `fixtureMutationStarted=false`.
The complete observed before/after snapshots both equal prepared B. There were
zero SaveGame/stashed-area writes, no unexpected writes, normal automatic exit,
exact settings restoration and a completed/released lease. The native result
remains **FAIL**; the expected-rejection safety audit is **PASS**. Matching A and
B authorizations, cleanup and the later B fresh load passed.

## R2: settings as an owned transaction

Previously, game preflight preceded unlocked settings capture/write; reusable
artifact checks did not establish ownership; finally restored original bytes
without checking the current file's owner.

The wrapper now holds the existing shared compatibility/runtime lock from
capture through atomic override, inherited launch/deployment, game exit and
restoration. Actual deployment, restore, profile and other settings-writing
entry points participate. Ownership, game and deployment checks repeat before
writes. Nested callers inherit the held lease. Exact backup bytes and a recovery
journal survive interruption. Restore accepts only original/owned bytes and
preserves foreign changes. Recovery cannot steal a living owner, run alongside
another recovery writer, or restore while Kingmaker is running. WhatIf is
mutation-free. The existing fixed-path recovery script provides the exact command.

Twelve disposable behavior cases pass against production orchestration and file
handling: busy owner, game-start race, foreign settings, exact restoration,
nested ownership/wrong coordination root, launch exception, running recovery,
recovery after exit including competing recoveries, WhatIf, native exit save,
owned native-predecessor recovery, and unproven foreign formatting. Only external
process/launch boundaries are stubbed. The actual compiled settings store is
invoked for native serialization tests. The reviewed file-handling statements,
extracted directly from git, reproduce the required busy/race/foreign/recovery
failures. A recovery interleaving also failed before exclusive recovery ownership
was added.

Native testing exposed the mod's exit-time settings reserialization. Overrides
now use that exact flat-schema format. Recovery of an earlier override requires
its exact hash-matched `.previous` bytes and only their deterministic native
serialization, archives the proof, and still rejects foreign values/formatting.
The live formatting interruption recovered exactly. The earlier rejected request
also retained its live-game lock/backup correctly; after the user's normal exit,
production recovery restored the original bytes. All eighteen review settings
transactions ultimately completed, with backups retained. Final normal-play
restoration is independently verified below.

## C1: reproduced and fixed

Verified installed feature: `bab7a67de47e4b6690c03fd5b744c482`,
`FavoredOracleOracleSpellList3ParametrizedFeature`. The adapter checks the exact
Oracle class, book, list, LearnSpellParametrized and level-four prerequisite
relationships. The legitimate Human Oracle route earns the partial bonus at
level 7 and the full spell selection at level 8.

Before repair, all four Circles appeared once in live spell-list extraction,
zero times in native Items, and failed native CanSelect. That preparation failed
with zero writes. The first, separate probe failure selected ordinary HP and is
not counted as bug evidence.

The repair adds only the four exact parameters to this feature through the
existing publication transaction and invalidates only its native cache. It does
not patch CanSelect or change prerequisites or known-spell limits. Native tests
cover warmed-cache invalidation, duplicate-safe publication, rollback and later
foreign additions/replacements.

Final-candidate preparation B observed each variant once in Items/extraction and
native CanSelect=true. Native SelectFeature and public LevelUpController.Commit
added exactly one against-Evil spell: known third-level spells 2 -> 3, one rank-1
parameter, normal allowance still 2, and zero ordinary third-level choices at
that level. Fresh B verification preserved that exact spell and parameter.
No direct AddKnown was used as evidence for this route; the separate existing
Sorcerer casting fixtures retain their seeded books.

## Candidate and validation

Package: `artifacts/local-runtime/0.0.132/KingmakerGunslinger-0.0.132-local-runtime.zip`
(28,170,697 bytes).

| Identity | Value |
| --- | --- |
| Version / assembly | `0.0.132` / `0.0.132.0` |
| Informational version | `0.0.132-icon-art-overhaul` |
| Package SHA-256 | `5c21b73a7e715717dee7bf39c60c845735e96b44a2ae5dfe6518435b02bf9e5e` |
| DLL SHA-256 | `01971d6aab3bfcce14d4858ad9921e56a2a0266d0aa49ff01d50489e4d604c3e` |
| MVID | `17245bc9-32b9-4330-83fd-352aaa010e0b` |
| Embedded commit | `11039183780a7815e97024ae99abc3062abd9ef9` (validated dirty tree) |
| Final Build-Local source-state SHA | `8ab2141a7c119f16e0b161b3413bfa26eaf0533cc7cb7248c11cc32f3c8aece9` |
| Frozen product/orchestration/test inputs | 1,883; canonical SHA `ca92b73dc8009e37c954c6bc33cfc8f3e680dfb96978e9d4183ab5af1ad75011` |

| Required gate | Result |
| --- | --- |
| Repository validation | PASS |
| Complete clean Release domain/reflection suite | 1,660 PASS, zero failures |
| Clean Release compilation | PASS |
| Strict standalone package validation | PASS |
| Focused request / production settings regressions | PASS / 12 PASS |
| Final affected native sequence | 10 PASS runs, 145 passing assertions; one intentional native FAIL with PASS safety audit |

Two reviewed-head auxiliary failures remain separate: Test-RuntimeDeployment's
legacy-114 inline-pin text assertion, and Test-RuntimeScenarioPreflight's stale
`documented-scenarios-retained` allowlist expectation. Neither is reported as PASS
or repaired here. Existing unrelated Teleportation observer limits also remain.

## Native evidence and scope

[Curated qualification](PR19-REVIEW-QUALIFICATION.json) contains exact run/artifact
identities, assertion names, receipt hashes, settings journals, negative audit,
restoration and check-log hashes. Raw evidence stays machine-local under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/`.

| Evidence directory | Final result / purpose |
| --- | --- |
| `20260920T1621554581645Z-working-save-magic-circle-verify` | PASS; A content ON/control OFF |
| `20260920T1624352530404Z-working-save-magic-circle-verify` | PASS; A OFF/ON |
| `20260920T1627116793342Z-working-save-magic-circle-verify` | PASS; A OFF/OFF |
| `20260920T1629471474158Z-working-save-magic-circle-scene` | PASS; requested ON/ON, unload/travel/return |
| `20260920T1633520358204Z-working-save-magic-circle-scene` | PASS; requested OFF/OFF, unload/travel/return |
| `20260920T1638228023618Z-working-save-magic-circle-cleanup` | PASS; exact A authorization, one write |
| `20260920T1641099333999Z-working-save-magic-circle-prepare` | PASS; B, real Favored Class commit, one write |
| `20260920T1644443777342Z-working-save-magic-circle-cleanup` | Intentional FAIL; A against B, zero mutation/write; safety audit PASS |
| `20260920T1648395307832Z-working-save-magic-circle-verify` | PASS; matching B ON/ON, Favored Class fresh persistence |
| `20260920T1651154691147Z-working-save-magic-circle-cleanup` | PASS; exact B cleanup, one write |
| `20260920T1654025656433Z-working-save-magic-circle-absent` | PASS; fresh absence and exact inventory/gold/vendor restoration |

Original preparation A is `20260920T1300124946650Z-working-save-magic-circle-prepare`:
producer DLL `365e78b29244038dcde38fb5d17f788504c55202130a637abe3308d82064dc2e`,
MVID `eb343b33-1272-4ad7-a3ea-31068a789845`. Its one write plus the three final-
candidate writes make four authorized Working writes for this review. Failed
probes wrote zero times. No baseline or campaign write was authorized.

All launches used Steam App ID 640820. Feature evidence reports game `2.1.7b`;
the generic result's gameVersion remains UNKNOWN. Saved-world checks used the
original eleven-mod profile: BagOfTricks 1.16.4, CallOfTheWild 1.14.4c-2.1,
CheatMenu 1.2.3, CraftMagicItems 2.1.0, KingmakerBuffPlanner 0.0.16,
KingmakerDiceRoller 0.1.2, KingmakerGunslinger 0.0.132,
KingmakerLastAzlantiPreserver 0.1.0, RacesUnleashed 1.0.11, TweakOrTreat 1.1.0,
and ZFavoredClass 1.3.1. Earlier broad mechanics/UI/optional-profile results remain
historical evidence for their own binaries, not new qualification of this DLL.

## Preserved behavior, art and restoration

The existing content setting, shared Protection enhancement authority, class
lists, scroll acquisition, targeting, typed defenses and aura lifecycle remain.
Content OFF hydrates known spells/active circles; all four startup combinations
passed. Bearer death ends the circle as approved; the aura design is unchanged.
Deferred mechanics remain deferred: new saves/morale against existing control;
removing/suppressing/resuming control; summoned-creature barriers and breach/SR;
inward circles, planar binding and diagrams; blanket descriptor immunity.

The four owner-approved exports and protected assignments are unchanged. Their
approval and prior desktop UI review remain recorded separately. This pass makes
no new native UI, gamepad, reduced-profile save-repair or uninstall-safety claim.

The guarded restore used exactly
`C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260920T1225444445857Z`.
All 136 normal-play files match, version 0.0.117; settings SHA is
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
The global bank is unchanged. Kingmaker is closed; no shared lease remains;
the candidate is not left installed.

For owner-authorized installation, close Kingmaker, preserve the current KMG
folder and FeatureModules.json, verify the ZIP hash, and install through the
established Unity Mod Manager workflow. Review settings after a full restart and
use disposable saves first. Rollback restores that explicit folder/settings
backup while the game is closed; use a pre-candidate save with an older build.
Removing blueprint definitions from saves is not qualified.

No R1/R2/C1 blocker remains within this profile and scope. The next safe action
is owner review of the published draft and this exact candidate; no merge, tag
or public release is implied.
