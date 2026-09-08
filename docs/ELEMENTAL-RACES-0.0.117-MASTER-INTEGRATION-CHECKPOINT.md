# 0.0.117 authoritative-master integration checkpoint

Status: qualified integration of the owner's requested public 0.0.115 fix.
This is not a complete Release C qualification or a public release.

## Provenance and conflict resolution

- Original mission master: `6874dc15a27ded132456dbdd480f47c794543a05`.
- Feature parent: `14dea6215927517a9cb7c91e6553fa5592abe44a`.
- Authoritative master parent: `dfd551080a1aad38cdd0b19714fbcb12c81ca4ca`,
  fetched initially and reconfirmed before this checkpoint.
- Branch: `codex/elemental-races-expansion`. The commit containing this report
  records both parents; no history was rewritten.
- All 22 unqualified Breeze-Kissed files were preserved in named stash
  `b1c5e1a7d45205b968443bce711088a5e87aaafb`; the tree was clean before
  the no-commit integration merge. That work is resumed after this checkpoint.

The intervening commits are API implementation
`a788d2269fcc4aaa24f8c49f820257ceb9cf7403`, provider documentation
`e985aad7671992885210df448deca18d05081096`, release preparation
`636d70bfd64db922a7d42070144302723002553c`, authorized upstream merge
`473f83bd901602ebe610cfdf291f11ce4a3faa57`, and the master release record
`dfd551080a1aad38cdd0b19714fbcb12c81ca4ca`.

The five Brown-Fur production files and their upstream domain regressions
match authoritative master exactly. The versioned direct API, delayed process
retention, exact target binding, provider-owned reservoir debit, and cleanup
are retained. No Buff Planner or optional-mod compile dependency is added.

Shared version defaults remain `0.0.117-elemental-traits`. Both distinct
0.0.115 release-note records are preserved with explicit provenance: the
published Share Transmutation fix is not the unpublished heritage checkpoint.
The reference-bundle publisher option and manifest-checked firearm bundle
fallback are retained. The 117 validator runs the new provider contract checks
without relabelling the old release's version, manifest or evidence.

No elemental production file or manifest entry differs from the feature
parent. This integration adds zero blueprint identities: 1,860 total, 1,858
active, two reserved; 222 active elemental identities (194 blueprints and
28 visual proxies), including 76 Release C identities.

## Immutable candidate and mechanical gates

Archive: `artifacts/qualification/0.0.117/master-integration-01`.
The candidate embeds the pre-merge feature parent above plus the exact
uncommitted merged source fingerprint; it does not pretend the final merge
commit existed when the binary was compiled.

| Artifact | Exact identity |
|---|---|
| Source-state SHA-256 | `afa4544373627ef783606760555ba7e9bf190e92be2728ee7cbdec88746a449f` |
| ZIP SHA-256 | `55b7c03c66b63207d4bcdd069e03d89b98e9e6b323dae1988bc72a26ce73c50b` |
| ZIP size / entries | 23,251,812 bytes / 135 |
| DLL SHA-256 | `bf671d1ae024fa6a7b1c387f7988ee0a8c03ffef6cd5ba2fec4a5e855bbef0c2` |
| DLL size / MVID | 6,238,720 bytes / `22d0f7ce-2243-49ec-9898-0c6a7805d075` |
| Deployment | `20260906T2348075275844Z` |
| Deployment SHA-256 | `e9715f98d22abba26da0abc1690c737b108c05af98be6eec68a99f4c57b5e99a` |

Repository validation, all 1,431 domain/reflection cases, clean exact-reference
Release compilation, and both strict standalone packages pass. Thirty
additional read-only checks pass against the exact compiled public API:
version constant, native parameter/return types, read-only status properties,
IDisposable and absence of optional assembly references. These are provider
signature checks, not the upstream consumer's historical 87 checks.

## Guarded runtime qualification

Six qualifying fresh Steam App ID 640820 processes pass 11,033 assertions:

| Scenario / profile | Run ID | Assertions |
|---|---|---|
| Elemental traits / KMG only | `20260906T2348380469624Z-6e86c034b32c4b8693627e50c26b4abb` | 5,498 |
| Elemental traits / highest-risk combined | `20260906T2351188334353Z-c11bad1904254a7b96a21c4c45bbc0c1` | 5,498 |
| Brown-Fur cast transaction / combined | `20260906T2355019827849Z-1d8c11bf53044631a7dbc725efdcef8b` | 9 |
| Brown-Fur absent / KMG only | `20260906T2357425204034Z-9e07bb1ca618488b8e013f8bdc63b423` | 7 |
| Working-save smoke / installed copy | `20260907T0012379786712Z-effcb93c78224a0abc289eeea5fcf125` | 11 |
| Brown-Fur native casts / installed copy | `20260907T0015038544685Z-27ec29f0975f4c7a948bba6728b635f8` | 10 |

Every result has zero failed assertions, zero result warnings and no exception.
The exact loaded version, source fingerprint, package/DLL hashes and MVID
were verified for each reuse. Every process exited through its guarded request.

The native Brown-Fur regression proves ordinary native commands, combined
Share/Powerful cost two, one spell-slot spend, real ally buffs, score modifiers,
Share-off self casting, cancellation without spend, and exact terminal cleanup.
It does not exercise a Buff Planner Instant client or the public direct API's
effect-producing path; that upstream live-consumer acceptance remains NOT-RUN.

All six profile transactions, including the earlier diagnostic transaction,
were independently compared with the final live 968-entry original mod tree.
Every comparison passed. Restored manifest SHA-256:
`9501ebfb600fc98f89e2c6212d0bbcc2026353689033110be384c282ec05bafe`.
FeatureModules.json SHA-256:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
No original mod folder or setting was lost.

## Save safety: native header bookkeeping versus gameplay

The first working-save pair returned PASS for 21 in-game assertions, but the
outer whole-ZIP equality check failed. It is retained as a diagnostic, not
counted among the six qualifying processes. Its working ZIP changed from
`95094cae88f8f4fbb624d2ae05dfc7f3352545595ad15dbeb1413b0055aab3f3`
to `6303c014f1d2823fc50f2abefaa40ac180294992a6fccdcf4d8a46f554aac11e`.
The protected baseline did not change.

Local engine evidence identifies ordinary native bookkeeping:
`SaveManager+<LoadRoutine>d__50.MoveNext`, token `0x0600BF00`,
increments `LoadedTimes` at IL_0198..01A3, then serializes the header and
calls `ISaver.SaveJson/Save` at IL_01CC..01EC. Audited engine DLL SHA-256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
No engine, provider or save-writing guard was patched.

A fresh two-process repeat captured every non-header entry's exact name,
length and decompressed SHA-256, plus all header bytes with only the one
`LoadedTimes` integer normalized. Each load must increment that integer
exactly once. Both phases passed: all 11 gameplay/image/history entries stayed
exact, all other header bytes stayed exact, and the counter changed 3 -> 4 -> 5.
The protected baseline stayed byte-identical before, between and after loads:
`cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`.

The final working ZIP is
`2d0450d52b32ebe09276689cada68870b4537b4a439b2b71c6454f16e8bdd94f`.
The unchanged header-except-counter SHA-256 is
`a655ee6fbe4c99d7859c0135a96b28ac037ea70de7a4647878640e9fae6b02fb`.
Full generated comparison evidence SHA-256:
`ba877821a24440e56bf6be7509f92658ccff78951035e4ab0e269d8e8429f4b4`.
No gameplay SaveRoutine was invoked. No baseline or personal save was loaded
or overwritten; only the named disposable working save was loaded.

## Retained limitations and evidence location

All runtime result, evidence-manifest, loaded-identity, mechanical JSON, raw-log
and attribution hashes, transaction hashes, and per-entry save hashes are in
the mission STATE's masterIntegrationQualification,
masterIntegrationRuntimeEvidence and masterIntegrationWorkingPayload records.
Raw artifacts and saves remain untracked.

Native shader/missing-script/lightmap diagnostics remain present. The combined
and installed-copy profiles retain four KeyNotFound signatures per process;
exact elemental, Brown-Fur and Fact.PostLoad error signatures are zero. These
are not warning-free native logs. Initial read-only collector/PowerShell
serialization mistakes and the failed whole-ZIP assumption are retained.

This merge does not rerun every A/B/C gate. Pinned 0.0.114 migration, module-OFF
trait-bearing saves, full compatibility enumeration and unfinished Release C
mechanics remain separately scoped to their existing evidence or pending
gates. Visual Adjustments is absent/NOT-RUN. No favored-class content is added.

Only the owner's requested master-into-feature integration merge is performed.
Nothing is merged back into master, tagged, force-pushed or publicly released;
no generated ZIP, raw runtime artifact, save or proprietary assembly is committed.
