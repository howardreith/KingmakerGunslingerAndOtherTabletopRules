# Favored Class Integration — Resume

Read `FAVORED-CLASS-MISSION.md` first, then this file, then
`FAVORED-CLASS-STATE.json` and `FAVORED-CLASS-BLOCKERS.md`. Re-inspect the
actual Git and installation state before running anything; never replay a
stale install, fixture or cleanup command from this file.

## Location

| Item | Value |
| --- | --- |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\favored-class-integration` |
| Branch | `claude/favored-class-integration` (local only; never pushed) |
| Baseline | `996105ed9e72259a220be56e2a74e0cc18e5ef47` (master, published 0.0.138 identity record; code parent `a0cae195c`) |
| Main checkout | `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger` (master, untouched) |
| Private decompiled references (not committed) | `C:\Dev\KingmakerGunslingerLab\private\favored-class-mission\decompiled\` (`ZFavoredClass\`, `CallOfTheWild.decompiled.cs`, `AssemblyCSharp\`) |

## Pre-mission shared installation state (must be restored at the end)

The last lab operation before this mission was
`runtime-20260923T211917Z-c235fb2d55674a94bd5963a56e65f407`
"restore explicit live-mod backup" (Completed). The installed state is the
owner's normal-play profile:

| File | SHA-256 |
| --- | --- |
| `Mods\KingmakerGunslinger\KingmakerGunslinger.dll` (0.0.136, MVID `7dce994f-ae50-443b-b893-2b3afaf11574`) | `c6cccdac914ed59fa4d85d020108588d7d12cfb4ac38cf5a162772bacc9b465c` |
| `Mods\KingmakerGunslinger\Info.json` | `f66de05d5c6282eece8218b6c4f31d49dfc8ef9efa27dfeceeda712034717e17` |
| `Mods\KingmakerGunslinger\FeatureModules.json` (schema 12, all 13 modules ON) | `6e24b2788a0c8f063d6e561a27c93f9c5349f2fc21b5217689da8aefdbb385d0` |
| `Mods\ZFavoredClass\settings.json` (`deity_for_everyone=true`, `enable_traits=true`) | `bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b` |
| `Mods\CallOfTheWild\settings.json` | `24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8` |
| `Mods\RacesUnleashed\Settings.json` | `270899c3f6c3d29bfe777fc2b55a0bb0404ae50786dbac8f1dccf77e8b9cabbf` |

Restoration must use the repository's existing Backup/Restore scripts and be
confirmed by byte checks. No compatibility lock was held at mission start;
Kingmaker was not running.

## Installed identities (Phase 0, read-only)

| Component | Version | SHA-256 | MVID |
| --- | --- | --- | --- |
| Game `Assembly-CSharp.dll` (2.1.7b) | — | `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` | `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7` |
| Favored Class `ZFavoredClass.dll` | 1.3.1 | `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c` | `3efd38e7-8682-4b4d-8d53-e368a3664919` |
| Call of the Wild `CallOfTheWild.dll` | 1.14.4c-2.1 | `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` | `8caab254-aacf-4811-8093-44b9184e6e53` |
| Races Unleashed `RacesUnleashed.dll` | 1.0.11 | `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0` | `e9b9acb5-9b3f-41ad-bbd7-74494d5d7680` |
| UMM `UnityModManager.dll` | 0.33.0.0 | `63e5baf7b1738e4091b5fd17ccb738ecdb4d1dbf246061dfd55dc52835d52691` | `54059519-2754-445a-b5ee-fdb9326336e2` |
| Harmony `0Harmony12.dll` | 1.2.0.1 | `aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292` | `918c071f-383e-46dc-a374-6879300cbe15` |

Other installed mods (not certified by this mission): BagOfTricks,
BetterVendors, CheatMenu, CraftMagicItems, EddicKingmakerRespec,
KingmakerBuffPlanner, KingmakerBugfixes, KingmakerDiceRoller,
KingmakerLastAzlantiPreserver, ProperFlanking2, SkipIntro, TweakOrTreat.
Favored Class ships nine custom JSON rewards in `ZFavoredClass\Custom\`.

## Latest meaningful checkpoint

- `7cb2cab92` pure policy, catalog, eligibility, host contract, 0.0.139 plumbing.
- `68e6d9a8a` Phase 1 code: adapter, contained registry, grit leaves,
  publication, coordinator, two guarded scenarios.
- `5fd8ef951` explicit fingerprint serialization; playable-race scoring.
- `c6c748f6d` chargen alignment settled around feature choices; grit claims
  fail closed; idempotency observer substring fix.
- `2cccec68b` grit proven on the Pistolero progression (base class dead-ends
  at level 17, blocker D1) plus a Mysterious Stranger pair; menu icon evidence.
  Build-Local: 1772/1772 domain tests; package `5121e2d8…`, DLL `5e256ab2…`,
  MVID `c65c50f2-f0d3-401c-a591-c644adcad711`.
- Native PASS on `2cccec68b`:
  `runtime-evidence60924T0102047921088Z-observe-favored-class-contract`
  and `runtime-evidence60924T0101128312625Z-disposable-favored-class-grit`.

Read-only audit reports (private, not committed):
`C:\Dev\KingmakerGunslingerLab\privateavored-class-missionudits\` A–G.

## Shared-environment change log (restore obligations)

Deployments use `scripts\Deploy-Local.ps1` (backs up the live mod tree,
preserves `FeatureModules.json` bytes). The owner's pre-mission install
(0.0.136, DLL `c6cccdac…`, Info `f66de05d…`, FeatureModules `6e24b278…`) is
held byte-identically in two verified backups:
`C:\Dev\KingmakerGunslingerLabuntime-backups\live-mod60923T2118026258530Z`
(pre-mission lab operation) and
`C:\Dev\KingmakerGunslingerLabuntime-backups\live-mod60924T0027014050156Z`
(first mission deployment). Restore at the end with:

```powershell
.\scripts\Restore-Live-Mod.ps1 -BackupDirectory C:\Dev\KingmakerGunslingerLabuntime-backups\live-mod60924T0027014050156Z -Confirm:$false
```

then byte-check DLL/Info.json/FeatureModules.json against the table above.
Only `Mods\KingmakerGunslinger` is changed by this mission; ZFavoredClass,
CallOfTheWild and RacesUnleashed files and settings are never written.

| Deployment (UTC) | Commit | DLL SHA-256 | Backup of the previous tree |
| --- | --- | --- | --- |
| 20260924T0027061224719Z | `68e6d9a8a` | `4f1ad2d8…` | `20260924T0027014050156Z` (owner pre-mission) |
| 20260924T0043533213527Z | `5fd8ef951` | `c03a15a4…` | `20260924T0043487547075Z` |
| 20260924T0053135459911Z | `c6c748f6d` | `a53e53c9…` | `20260924T0053085191115Z` |
| 20260924T0101047085831Z | `2cccec68b` | `5e256ab2…` | `20260924T0101000748327Z` |

## Next concrete actions

1. Phase 2 code: per-type misfire (policy applied last, floor 1; scatter
   aggregate on the effective threshold), firearm confirmation (better of
   Critical Focus), Pistol-Whip attack, Halfling Nimble and Gunslinger's Dodge
   counters, Gunslinger Initiative (verify and fix the deed timing first, D3),
   Drow Nimble and dirty trick/trip (third-party profile OFF); manifest
   identities, icon dispositions, domain tests.
2. Native: menu/eligibility per ancestry, native combat events in the
   save-free PortalHarness + ElementalNativeTurnScope, initiative timing.
3. Phase 1 remainder: H01 host-absent clean launch, H02/H04, L01 save/load.
