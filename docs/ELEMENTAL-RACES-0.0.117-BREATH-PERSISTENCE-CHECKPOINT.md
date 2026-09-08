# Elemental Races 0.0.117 breath action and ten-trait save checkpoint

## Outcome and limits

PASS for this incremental checkpoint only. Repository validation, all 1,428
domain/reflection cases, clean exact-reference Release build and strict package
validation pass. Nine guarded Steam App ID 640820 processes pass 11,325
assertions. All three complete 968-entry mod-folder restorations independently
match the live original manifest and settings. No game process remains.

Release C is incomplete: Treacherous Earth, Breeze-Kissed and Nereid Fascination
are unimplemented. Breath-specific turn-based action proof, eleven other
traits' persistence, full Crystalline semantic catalog, complete lifecycle and
final release-wide compatibility/module gates remain. Visual Adjustments is
absent/NOT-RUN. This checkpoint does not retroactively expand any older PASS.

## Native action and persistence evidence

The dedicated breath scenario now drives ordinary UnitCommands through native
UnitActionController.TickCommand. Commands are not Cutscene and do not ignore
cooldowns. Queued cancellation spends no use/action; an existing Standard
cooldown blocks starting. Accepted casts incur the native Standard cooldown
exactly once, preserve Move/Swift cooldowns, spend one use and finish through
native AbilityExecutionProcess. Later controller ticks do not double-charge.
Only fixture animation/projectile timing is isolated. A missing main-menu
hand controller is supplied by a request-local native controller and restored
exactly; no game patch or effect/resource-spend fallback is used.

The append-only ten-trait matrix retains all older pure matrices unchanged.
It covers 24 race/sex/heritage fixtures, all now trait-bearing, and six legal
two-trait Ifrit combinations. Both Undine breaths each cover three fixtures,
both sexes and all three parent heritages. The six earlier blood/Insight
traits, Efreeti Magic and Crystalline Form remain in the matrix.

Twelve actual native breath commands spend the uses in prepare and module-OFF
recast phases. The target is one exact disposable Ifrit fixture, never a
campaign party member. Audited native walkable placement excludes unrelated
units; positions, wounds, saving-throw fixture state, random state, clock,
projectiles and unrelated buff references are restored exactly.

Before any fresh-load mutation, the saved command-created Sickened effect
retains exactly its target, blueprint, ability, caster, caster level, DC,
absolute end time and remaining duration. Both independent cross-process
comparisons pass all eight fields. Prepare-to-OFF is CL 1/DC 10; OFF-to-ON is
CL 2/DC 11. Both preserve endTimeTicks 462709910000 and 18 seconds remaining.
Its source is the fixed Ooze Breath ability, not a reconstructed test effect.

All six Undines preserve zero spent uses through native module-OFF level-up.
Ordinary rest restores exactly one, and a native recast spends it to zero.
Module-ON native respec returns to base traits while changing heritage:
General to Mistsoul, Mistsoul to General, and Rimesoul to Mistsoul, both sexes.
The 24-fixture matrix retains 168 exact trait and 42 exact Crystalline
observations, alongside spent blood capacity, armed consent and active size
and feat effects. Exact cleanup and a separate fresh process prove fixture
and breath-condition absence.

The pinned 0.0.114 producer, 117 consumer and separate absence process pass
all eight legacy race/sex fixtures: markerless General, stats, facts, stored
appearance and zero spent resource amounts remain exact before and after
reconciliation. Only KMG_AUTOMATION_WORKING is used by the guarded transaction;
KMG_AUTOMATION_BASELINE is excluded. Subjective image review is not claimed.

## Artifact and evidence ledger

Starting master: `6874dc15a27ded132456dbdd480f47c794543a05`.
Branch: `codex/elemental-races-expansion`.
Embedded parent: `cf2426ac092b6bed33ff721fca722be9486f5e89`.
Version: `0.0.117-elemental-traits`.
Source-state SHA-256:
`c0efe468af0cc2d3d5016aa355d4b870a26697b9e17c09b0c7e27ef0f45b9f2e`.

Ignored archive: `artifacts/qualification/0.0.117/undine-breath-persistence-05`.

- ZIP: 23,247,343 bytes, 135 entries; SHA-256
  `b944ae3bc363ff5147ca05e6944c21b1fa73614df5b73de56874a8981a3a67c0`.
- DLL: 6,225,920 bytes; SHA-256
  `3d3bd6c419ca53979ef56b7383cb0d62c811043a4d422ca2cf63e164a44eaf5a`;
  MVID `a4da4c66-b0e1-4525-9703-103305fe00ab`.
- Deployment `20260906T2152578027384Z`; SHA-256
  `503dcb7035f05f076da281a9916a0def7e32bfa4d61742278922afbaa361141b`.

No production rule, identity or schema changes in this slice.
Manifest stays 1,860 total / 1,858 active / two reserved; 222 active elemental
identities (194 blueprints, 28 visual proxies), including 76 from Release C.

| Phase/profile | Run ID | Assertions | Result warnings |
| --- | --- | ---: | ---: |
| 117 prepare | `20260906T2153506788947Z-87a781f44ebd42f2b8c3820a505482bf` | 65 | 14 |
| 117 module-OFF | `20260906T2158382832444Z-31994c5d32b344a385c505e4db0bc5fd` | 154 | 16 |
| 117 module-ON respec/cleanup | `20260906T2202268457673Z-fe4d5f0e4b93433dbd34204f6147ae26` | 73 | 15 |
| 117 fresh absence | `20260906T2206305611860Z-391da80452cf426aa3c35bf3af6d2ae1` | 8 | 2 |
| 114 producer | `20260906T2209266695318Z-2186f4430a8546a5a4431317aee2d02f` | 11 | 5 |
| 117 legacy consumer | `20260906T2212340424678Z-22b3296bf662411eae00d4dbb8db80d0` | 10 | 6 |
| 117 legacy fresh absence | `20260906T2215171908617Z-acba70f2b83049a395c9442ccfddc03b` | 8 | 2 |
| KMG only native | `20260906T2218343655434Z-2f195d2583ff47199fc9b32cf3aaf8d0` | 5498 | 0 |
| Highest-risk combined native | `20260906T2220485105945Z-862b7d0cd7f74d6ca787d9ae5b575f84` | 5498 | 0 |

Native profiles each pass 356 breath observations, including 30 RTWP controller
observations. Complete result/evidence/index/mechanic/log/summary and transaction
hashes are in `releaseCUndineBreathPersistenceQualification` in STATE.

Restored manifest SHA-256:
`333e1f11ce22e26f1a5c6c825d8e520053672ac597c40ca05f094d7fbf303768`.
Encoding: UTF-8 PowerShell `originalManifest | ConvertTo-Json -Depth 6 -Compress`.
Restored FeatureModules.json SHA-256:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.

## Diagnostics retained

Failed save candidates 01-04 remain FAIL, with hashes in STATE. None called the
save API; fixture cleanup and mod restoration completed. Native evidence
identified deferred projectile removal, Sickened's Prolong context retention,
and the native SickenedSystemCondition companion. Each independent test cast
removes only earlier command-created conditions after fresh-load observation.
Native UnitState owns the exact root-referenced companion; the test neither
creates nor removes that companion directly and checks its exact state.

The final runs retain 60 framing/visual/DollData warnings, native shader,
missing-script/lightmap/zero-surface diagnostics and four inherited combined
KeyNotFoundException occurrences. Exact elemental ERROR, feat-transient
restoration and Fact.PostLoad signatures are zero. No blanket clean-log claim.
Read-only collector quoting/schema/array-shape errors changed no evidence and
did not cause game reruns.

No ZIP, raw runtime artifact, save or proprietary assembly is committed.
Nothing was merged, tagged, publicly released or made into a PR.
