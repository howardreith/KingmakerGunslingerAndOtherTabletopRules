# Elemental Races 0.0.117 feat-preview correction checkpoint

## Outcome

PASS for this incremental correction only. All 1,424 domain/reflection tests,
repository validation, clean exact-reference Release build and strict package
validation pass. Nine guarded Steam App ID 640820 processes pass 10,594
assertions. Release C remains incomplete; five traits are unimplemented.

The existing feat-transient restoration service attempted to restore buffs
inside a native level-up preview whose BuffCollection deliberately refuses
buff additions. Native `SetupPreview` sets the private boolean `m_Disabled`;
`AddBuffInternal` returns null there. No public equivalent reliably covered
companion previews. The correction reads that exact flag only after finding
the owner's existing project transient UnitPart, and returns without changing
any preview ledger, buff or item. An absent or changed field fails closed
through the existing diagnostic path. It never writes the native flag.

Normal owner reconciliation and save hydration are unchanged. There is no
new save schema, identity, global game patch or optional assembly dependency.
The service originated in Release B; this repair and expanded preview proof
are in 117. The retained 116 ZIP is not repaired or retroactively qualified by
these new tests. Earlier PASS records retain their actual scope and hashes.

## Actual behavior qualified

The dedicated feature scenario uses native commands to activate Scorching
Weapons on two exact native manufactured weapon items and Elemental Strike.
Actual native LevelUpController preview/cancel/commit proves:

- The preview alone has a disabled BuffCollection. Repeated project-service
  reconciliation succeeds without changing its copied absolute expiry ledger.
- Cancel preserves the original level; commit advances it. Both preserve the
  same original UnitPart, buff, exact item enchantments and absolute end times.
- Native buff ticking and native per-item enchantment ticking expire the exact
  effects. The test does not remove enchantments manually as proof of expiry.
- The request-local project-log observer receives its positive INFO witness,
  records zero restoration errors, and is released. Fixture membership,
  original clock and random state are restored.

Each native profile passes 21 preview observations. A scoped read-only spy on
the project's own logger observes this boundary; it does not suppress messages,
patch game mechanics or run outside the active scenario.

The four current-version save phases requalify the existing eight-trait
matrix: 24 race/sex/heritage fixtures, 168 exact trait observations and 42 exact
Crystalline observations. Module-OFF/ON loading, native level-up, ordinary rest,
spent daily uses, armed consent, partially consumed blood capacity, active
effects, base-trait respec, cleanup and fresh-process absence pass. This is not
the other thirteen traits' persistence or complete death/polymorph lifecycle.

The pinned 0.0.114 producer, 117 consumer and separate absence process pass
for all eight legacy race/sex fixtures. Stats, facts, spent uses and stored
appearance data remain exact. Only `KMG_AUTOMATION_WORKING` is used under the
guarded harness; `KMG_AUTOMATION_BASELINE` is excluded.

## Artifact and evidence

Starting master: `6874dc15a27ded132456dbdd480f47c794543a05`.
Branch: `codex/elemental-races-expansion`.
Embedded parent: `686cd42efeb10d3c979e6ca4d951449d157998ca`.
Version: `0.0.117-elemental-traits`.
Source-state SHA-256: `bf35c5f4aa34e50dd73af67f0e83c5b3e9300d0f1c19b08ec054fd2883cc0bce`.

Immutable ignored archive:
`artifacts/qualification/0.0.117/feat-preview-correction-02`.

- ZIP: 23,231,540 bytes / 135 entries;
  SHA-256 `bb3f292315b1e62f4f95f222cdc3fe7f0e07fab277ebc51a7451207384d7baa8`.
- DLL: 6,183,936 bytes;
  SHA-256 `2e917226048915b621e325be98dc3c4213c6101832edf5eec3b4c185072dc523`;
  MVID `350d40cc-1881-4b51-ac3f-5829150bc042`.
- Deployment `20260906T1826251162824Z`;
  SHA-256 `e5a58e2b5c5c735575f0f7a82dcf60e0d1cc8766729b6692f283d119a1a30035`.

Manifest unchanged: 1,856 total / 1,854 active / two reserved; 218 active
Elemental identities, comprising 190 blueprints and 28 visual proxies.
Release C still contributes 72 identities. This correction adds zero.

| Phase / profile | Run ID | Assertions | Result warnings |
| --- | --- | ---: | ---: |
| KMG-only native | `20260906T1826494969410Z-a16a3f32a0784609a037bdaa1a2fd451` | 5142 | 0 |
| Highest-risk combined native | `20260906T1828494406536Z-e65396ec343c4e35aa2f21359d310563` | 5142 | 0 |
| 117 prepare | `20260906T1833377257050Z-b8d959551acb4c65b1ec9b143e3a095d` | 58 | 14 |
| 117 module-OFF / level / rest | `20260906T1837308428916Z-a6e98afca45e4ade8ac6c6c7f272ce5a` | 146 | 16 |
| 117 module-ON / respec / cleanup | `20260906T1840506782600Z-797976425b5b4a2b954fcd98e7dc8abd` | 71 | 15 |
| 117 fresh absence | `20260906T1844505821115Z-3a1a0edbbc624f9da8400267dad6f3c3` | 7 | 2 |
| 114 legacy producer | `20260906T1847232968694Z-319f07bf964c44dc988d997f6980a21f` | 11 | 5 |
| 114 to 117 migration | `20260906T1850547624450Z-c90c23417ab5454c832bb722a4ec6e2b` | 10 | 6 |
| Legacy fresh absence | `20260906T1854090786764Z-4d08b51217164b28bdb70bf555709695` | 7 | 2 |

Every result is PASS with zero failed assertions. Exact per-run result,
`runtime-evidence.json`, persistence index, native log and attribution-summary
hashes, plus preview-observation hashes, are in
`releaseCFeatPreviewQualification` in the mission STATE.

Legacy ZIP SHA-256:
`b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694`.
Legacy DLL SHA-256:
`09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262`;
MVID `dcd73856-39d4-40ce-9b05-77bf249103d7`.
Legacy transaction `20260906T1847163030381Z-elemental-race-legacy-migration-transaction`;
SHA-256 `a8656a76b70e519772cc88e857d4913d5f5af30edc50dff279bf083ef107877a`.

## Restoration and diagnostics

All three outer profiles restore exactly and independently match the full
968-entry live Mods manifest, including contents and timestamps:

- `gunslinger-only`;
- `gunslinger-high-risk-combined-favored-class`;
- `gunslinger-exact-installed-copy` for the save and legacy transactions.

Manifest SHA-256:
`12fac23d6a08015611f155d181304e27f5adbb8e9a55dfb08fdc4943078ce18e`.
Encoding: UTF-8 PowerShell
`originalManifest | ConvertTo-Json -Depth 6 -Compress`.
FeatureModules SHA-256 before and after:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
Per-transaction hashes are in STATE. No Kingmaker process remains.

The renewed module-OFF log has zero of the four previously observed
`feat-transient.reconcile.failed` entries. All nine logs have zero of that
signature and zero Fact.PostLoad signatures. Both native profiles have zero
runtime-result warnings; all 60 save/legacy framing, visual-review and DollData
warnings remain explicit. Native shader, serialized-script and lightmap
diagnostics remain; combined native and module-OFF logs each retain four
KeyNotFoundException occurrences. No blanket error-free-log claim or subjective
image-appearance PASS follows from these mechanical results.

## Failed attempt and remaining gates

Candidate `feat-preview-regression-01` remains FAIL: five failed assertions
and 16 actual project restoration errors. Four failures reproduce the disabled
preview problem. The fifth required the native per-item expiry scheduler.
The initial Unity log callback did not observe UMM output, so its zero-error
assertion is explicitly NOT QUALIFIED; the corrected observer has an independent
positive witness. KMG-only restoration passed, and combined was not attempted
after that failure. Exact failed artifact and evidence hashes remain in STATE.

A read-only evidence-collection array-enumeration error was corrected without
changing raw evidence or launching another game process.

Still open: Treacherous Earth, Breeze-Kissed, Acid Breath, Nereid Fascination,
Ooze Breath, complete semantic ray/story-effect classification, all-trait
persistence/lifecycle and final six-profile release qualification. Visual
Adjustments is absent and NOT-RUN. No new favored-class behavior was added.
No package, save, raw artifact or proprietary assembly is committed.
Nothing was merged, tagged, publicly released or made into a PR.
