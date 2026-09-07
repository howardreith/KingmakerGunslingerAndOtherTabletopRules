# Elemental Races 0.0.117 Breeze-Kissed native core checkpoint

## Outcome and limits

PASS for this incremental native core only. Complete repository validation,
all 1,432 domain/reflection tests, clean exact-reference Release build and
strict package validation pass. Two guarded Steam App ID 640820 processes pass
11,376 assertions, including 380 dedicated Breeze observations. Both complete
968-entry mod/settings restorations independently match. No game remains.

Nineteen required trait mechanics now have incremental native proof; ten
traits retain their earlier save-backed proof. Treacherous Earth and Nereid
Fascination are still unimplemented. This is not full Breeze qualification or
Release C PASS. Broader nonmagical attack-source classification, native
turn-based actions, eleven other traits' persistence, full lifecycle and
final six-profile/module gates remain. Visual Adjustments is absent/NOT-RUN.
Direct Buff Planner Instant consumer gameplay remains NOT-RUN.

## Rules and owned implementation

[Archives of Nethys: Sylph](https://aonprd.com/RacesDisplay.aspx?ItemName=Sylph)
supplies the printed affinity replacement, nonmagical ranged AC, swift wind
controls, daily gust and ordinary maneuver rule. No special caster-level or
mental-stat CMB formula is printed. The gust uses native Standard supernatural
action economy and ordinary BAB/current Strength/size/CMB resolution, not
Hydraulic Push's replacement formula. Ordinary rest is the mission-approved
daily-recovery convention; no independent real-world 24-hour timer is claimed.

The existing marker/provider consume ElementalAffinity only. A local AC
subscriber uses exact current attack/weapon/stat references and completed
native physical EnhancementTotal metadata. Ready winds add +2 native racial
AC; swift Calm applies one owned permanent buff, Renew removes it without
restoring a spent use. A daily native resource and the existing owner ledger
retain exhaustion through unchanged reconciliation and level-up. Standard
Bull Rush/Trip variants commit the same resource once, then execute the native
maneuver. No native/foreign blueprint, global rule or optional assembly changes.

Currently the production AC boundary rejects ability-sourced attacks and
unknown/nonphysical weapon metadata. Actual crossbow, pistol, melee,
masterwork and temporary enhancement controls pass; the broader extraordinary
weapon-ability and nonmagical alchemical attack boundaries are still open.
This fail-closed subset is not claimed as complete printed coverage.

## Executable and guarded proof

The pure policy enumerates 256 readiness/calm/source/range/physical/enhancement
states. Native tests cover all three Sylph heritages, exact affinity
replacement, live ranged/melee AC, native racial stacking, masterwork,
temporary +1 enhancement and its removal. The native default stacking set
preserves a separate +4 racial source alongside this trait's +2.

Actual UnitCommands and UnitActionController drive Calm, Renew and both gusts.
Only request-local animation timing and an absent hand controller are isolated.
No Cutscene or IgnoreCooldown flags bypass native costs. Queued cancellation
spends no action/use; accepted commands incur exact native Standard or Swift
cooldown, preserve the other action counters and complete their execution.
Calmed buff reference survives native whole-owner TurnOff/On; this is not a
fresh-save claim.

Fighter 5/Wizard 4 uses native CMB 11; temporary Strength raises it to 13,
whereas a Charisma change does not. Success, ordinary failure and immunity
each consume exactly one use and emit exactly one native maneuver. Trip
success sets native pending prone; failed/immune cases preserve target state.
Native immunity returns before CMB/CMD/dice calculation, so its default
Success getter is not used as proof of an effect. An independent native
immune-rule control and actual position/prone fields establish the outcome.

Zero use blocks gusts and wind renewal and removes the AC benefit. Native
rest restores one use; native level-up and provider removal/re-add do not
restore a spent use. Removing the trait removes only its controls/calm buff
and restores the correct heritage affinity. All fixture units, projectiles,
clock, random state and request-local controller references restore exactly.

The disposable pistol receives one legitimate loaded pre-state. Normal
discharge, misfire and AC remain active. Only the final combat-log UI sink is
spied on synchronously during that exact attack: the normal message validator,
publication service and counters still execute. Captured attempts and faults,
spent round and exact original sink restoration are checked. No game UI is
used as mechanical proof.

## Artifact and identities

Mission starting master: `6874dc15a27ded132456dbdd480f47c794543a05`.
Current authoritative master: `dfd551080a1aad38cdd0b19714fbcb12c81ca4ca`.
Branch: `codex/elemental-races-expansion`.
Embedded parent: `0d9cd38144132a94acac997b82409f84c54d2b94`.
Version: `0.0.117-elemental-traits`.
Source-state SHA-256: `955ab659e3cad5f4aa414614c6d65bab89e970b311f87eaedf2655d422f560ed`.

Ignored archive: `artifacts/qualification/0.0.117/breeze-kissed-08`.

- ZIP: 23,259,329 bytes, 135 entries; SHA-256
  `acc04aa24697170f00336198d67f0ef4aa98dedc5a7884acf263d0d713de3fe4`.
- DLL: 6,264,320 bytes; SHA-256
  `b3839a63fb83a5894169fa7ea2cbc1ef6e15f01229081b51d0f74b0d88984f05`;
  MVID `0a84af89-794b-46b5-b55e-13bf9959bcd6`.
- Deployment `20260907T0149447114593Z`; SHA-256
  `b019f345e29654a73931788bad2c57d66920208b6915a422788a273c83135c38`.

Seven fixed identities append under
`KMG.ElementalRaces.Traits.Sylph.BreezeKissed`:

| Suffix | Stable GUID |
| --- | --- |
| `.Resource` | `e117e1e0a17a4acec001000000000077` |
| `.Gust` | `e117e1e0a17a4acec001000000000078` |
| `.BullRush` | `e117e1e0a17a4acec001000000000079` |
| `.Trip` | `e117e1e0a17a4acec001000000000080` |
| `.CalmedBuff` | `e117e1e0a17a4acec001000000000081` |
| `.CalmWinds` | `e117e1e0a17a4acec001000000000082` |
| `.RenewWinds` | `e117e1e0a17a4acec001000000000083` |

Existing marker `e117e1e0a17a4acec001000000000033` and provider
`e117e1e0a17a4acec001000000000054` remain unchanged.
Manifest: 1,867 total / 1,865 active / two reserved; 229 active elemental
identities (201 blueprints, 28 visual proxies), including 83 Release C:
ten selections, 52 features, five buffs, five resources, ten abilities and
one activatable ability. Every new identity registers regardless of module
publication. No General race/SLA/resource GUID, visual or settings schema changes.

## Runtime and restoration ledger

| Profile | Run ID | Assertions | Result warnings |
| --- | --- | ---: | ---: |
| KMG only | `20260907T0150166123561Z-4c2126abd9594fd5818b03ca95f9189a` | 5688 | 0 |
| Highest-risk combined | `20260907T0152582879337Z-716236caed0c4b7d92682033ba4e440a` | 5688 | 0 |

Each profile passes all 190 Breeze observations and three fixture lifetimes,
with zero fixture errors/exceptions. Result/evidence and mechanic hashes:

| Profile | File | SHA-256 |
| --- | --- | --- |
| Only | runtime-result.json | `b26fe7c9e1ca4e7cb399b4b0551b0b0dde5958e17484379e515ad6197e194db1` |
| Only | runtime-evidence.json | `66e792a02460fb683087e31205191fdc5678ace9c3039586219b11a83fe037d7` |
| Only | elemental-breeze-kissed.json | `060eadfbdb1681737a7a194fb6b46f0a1770c4e82a2bf73b8ded3a6427b6ccbe` |
| Only | output_log-breeze-kissed-08-only.txt | `f1eae5fc3593fd3afb9dd4f07298a7910a07bd7e36664fdfa2b1785fb45a9edb` |
| Only | compatibility-attribution-log-breeze-kissed-08-only.json | `2fc4518943936a7ecea475db1e106fa8a929577b90d2082d678bbbe2ddeef4c1` |
| Combined | runtime-result.json | `a3bbe63cbdb35c0a3d925facb6269ee8fbbf62558dd11f261e042894f6bc9787` |
| Combined | runtime-evidence.json | `90d60607bb01a5c42dab93a9cf9813e1d70bd6b76e1b4208cab6e584ffa67e95` |
| Combined | elemental-breeze-kissed.json | `8f825bb93ba68bd14caece6b35bc958515a5fc806d773e61ee4828b98948fe3e` |
| Combined | output_log-breeze-kissed-08-combined.txt | `85df781dbff160febf662f80fab219032ba90c48cdf9848028a250bef4904593` |
| Combined | compatibility-attribution-log-breeze-kissed-08-combined.json | `55df64360c3a49b665ad26a175f43b8108c8b15032bf29312e4476ecb06148b0` |

Complete subsidiary evidence hashes are in
`releaseCBreezeKissedNativeQualification` in STATE. Restored manifest SHA-256:
`27c5bb398be48d92c3b98bf03d9747ca192b553e803a9e396805b4f697d455c7`.
Encoding: UTF-8 PowerShell `originalManifest | ConvertTo-Json -Depth 6 -Compress`.
Restored FeatureModules.json SHA-256:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
Transaction SHA-256 (only / combined):
`b0a9a0c944137f8876590919ad5e7a114b3702f84b56db141c30c794b4ad7852`,
`c838a699e58a8543f90f180d4c63b0eee3a1660b6821153cf0a2e8d5a9bcf60e`.

Zero scenario warnings does not mean a warning-free native log. Each retains
four shader-all-passes-removed, four fallback, four GPU, four missing-script
and one lightmap signature. Combined retains four pre-existing ZFavoredClass
KeyNotFound signatures. Elemental ERROR, post-load and firearm annotation/
condition-publication failures are zero. These save-free runs do not prove
that the isolated combined profile can load the working-save fixture.

## Failed candidates and continuation

Candidates 01-07 remain diagnostic failures/mixed results in STATE. They
exposed an empty disposable pistol, null enchantment context, a real missing
Calm duration initialization, time-zero get-up/AC fixture assumptions, native
racial stacking assumptions and too-narrow firearm UI isolation. The real
Calm defect was corrected before this checkpoint; native accepted-command,
buff-state and defense observations supply the regression proof.

Candidate 07 passed only and failed combined when a genuine natural 1
committed the firearm's Broken condition but reached an absent test UI.
The correction isolates the final sink, not misfire or damage logic.
The final source/package is unchanged across both candidate 08 processes.
Subsequent edits record documentation/evidence only; the ending checkpoint is
the child commit identified by this report's addition to git history.

No save was loaded or written here. Earlier 0.0.114 migration and ten-trait
persistence evidence is preserved, not expanded to Breeze. Continue broader
source controls, save/lifecycle/TB qualification and the two outstanding
implementations. No feature-to-master merge, tag, release, PR or committed
generated package occurs at this checkpoint; the prior owner-authorized
master-to-feature integration is retained.
