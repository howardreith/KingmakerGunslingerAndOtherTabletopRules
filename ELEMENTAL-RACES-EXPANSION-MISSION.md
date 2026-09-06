# Elemental Races expansion mission

Implement and fully qualify three gated releases on
`codex/elemental-races-expansion` without stopping between milestones:

- 0.0.115 / `0.0.115-elemental-heritages`: alternate elemental heritages;
- 0.0.116 / `0.0.116-elemental-feats`: elemental racial feats; and
- 0.0.117 / `0.0.117-elemental-traits`: alternate racial traits.

Before Release A, harden the 0.0.114 affinity, racial SLA, Oread movement,
Hydraulic Push, shared-catalog ownership, visual ownership, and runtime-test
contracts. Favored-class bonuses are explicitly outside this mission.

## Durable safety contract

- Preserve every 0.0.114 race GUID and save-bearing identity.
- Register every Elemental Races identity whether the module is ON or OFF;
  gate only additive publication for new character creation and respec.
- Keep heritages, feats, and traits in the existing `elemental-races` module.
- Never generate a save-bearing GUID dynamically.
- Alternate heritages use the existing parent race blueprint and never become
  top-level races or new native `Race` enum values.
- Preserve `RaceId.Aasimar`, exact `BlueprintRace` identities, the accepted
  person-spell behavior, Keen Senses adaptation, and absence of `OutsiderType`.
- Do not mutate native or third-party donors, arrays, assets, or catalogs in
  place outside an existing exact reversible transaction.
- Shared publication remains additive, deterministic, idempotent,
  exact-GUID-aware, order preserving, conflict refusing, and reversible.
- Add no optional-mod compile-time dependency and no new runtime library.
- Keep mechanics in feature-specific services/components and runtime evidence
  in dedicated scenarios; central bootstrap and runner code only coordinate.
- Treat build, domain, package, guarded runtime, compatibility, and persistence
  as separate gates. An ambiguous runtime result is a failure.

## Release gates

Each release must independently pass repository validation, the complete
domain/reflection suite, a clean Release build, strict package validation,
its focused guarded Steam App ID 640820 runtime scenarios, compatibility
profiles, and required save-backed persistence. Release A additionally proves
0.0.114 migration. Release C additionally proves the complete replacement-slot
state matrix and every implemented trait mechanic.

Only named disposable fixtures may be used. Never overwrite
`KMG_AUTOMATION_BASELINE`; use `KMG_AUTOMATION_WORKING` only through the
documented guarded transaction. No screenshot, OCR, coordinate automation, or
direct `Kingmaker.exe` launch is mechanical evidence.

## Git and publication contract

Create coherent foundation, implementation, and qualification commits. After
every coherent commit, and before any pause or handoff, run exactly:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

Do not merge, tag, force-push, rewrite history, publish a GitHub release, or
commit generated packages, saves, proprietary assemblies, machine-local
configuration, or raw runtime artifacts.

## Hard-stop contract

Fail closed only after native mechanics, established project patterns,
authoritative rules, and safe reversible alternatives have been exhausted.
Record a feature-specific blocker and continue independent work. Do not invent
a compensating benefit or claim a release PASS while any required gate is
blocked, failed, ambiguous, or not run.

## Current engineering focus

Release C has fifteen prior native-proven mechanics and incremental
[Crystalline core proof](docs/ELEMENTAL-RACES-0.0.117-CRYSTALLINE-CORE-CHECKPOINT.md).
Complete Crystalline catalog/multi-ray/full-lifecycle boundaries and
implement Treacherous Earth, Breeze-Kissed, Acid Breath, Nereid Fascination and
Ooze Breath, then every final release gate. Continue independent work; no
hard stop is established. The earlier native audit is retained historically.

2026-09-06 continuation: the final Crystalline core candidate passes 1,423 tests,
clean Release/package and two native profiles with 10,168 assertions. Three
new resource/consent identities are fixed. Earlier failed fixtures remain FAIL
in STATE; no build, core run or earlier seven-trait save cycle implies full
Crystalline or Release C acceptance.

The subsequent eight-trait checkpoint passes native consent/spent-use
save/OFF/ON/rest/level/respec and renewed pinned 0.0.114 migration. Nine
processes pass 10,490 assertions; exact evidence and retained failures are in
[the persistence report](docs/ELEMENTAL-RACES-0.0.117-CRYSTALLINE-PERSISTENCE-CHECKPOINT.md).
The separate pre-existing feat-transient preview/hydration diagnostic and
every remaining release gate still require investigation/qualification.

The next test-only checkpoint qualifies native multi-ray/duplicate-impact,
non-damage-ray and two-handed/disabled-hand controls in both profiles (10,242
assertions). See the [controls report](docs/ELEMENTAL-RACES-0.0.117-CRYSTALLINE-CONTROLS-CHECKPOINT.md).
No new production identity or behavior is implied by its remaining-trait audit.
