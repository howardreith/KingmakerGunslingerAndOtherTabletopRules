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

- Phase 1 (`68e6d9a8a`…`2cccec68b`): adapter, contained registry, grit
  leaves, transactional publication, coordinator; grit proven on the
  Pistolero progression (D1) plus a Mysterious Stranger pair.
- Phase 2 (`fba7516d3`, `cf8a66d21`, `732cddae3`): all first-party Gunslinger
  counters (per-type misfire, confirmation, Pistol-Whip, Halfling
  Nimble/Dodge, Drow Nimble, Ifrit Initiative, dirty trick/trip); the D3
  initiative-timing defect reproduced and fixed; H01 host-absent profile.
- Phase 3 simple counters (`9ee86bfbe`, `11b488be3`, `d9d09b8b8`): I01 I05
  I07 O04 O05 U02 U04 in each class's own host selection.
- Phase 6 settings (`f2db844c2`): optional restart-required
  `FavoredClassIntegration.json` (schema 1, five Boolean controls; absent or
  invalid gives the charter defaults, reported).
- Test fixes (`4c9409ba4`): bomb probe above the native damage floor; the
  menus scenario checks each counter's own class selection.
- Phase 5 (`cc0168d89`): four-race Mostly Human trait (Option A) and the
  scoped host ancestry bridge.
- Phase 3 advanced and Phase 4: O06-O08 (`94cb93919`), I08/S06
  (`07c21f29b`), I06/S04 (`9b3db78bf`, walk fix `76e0a1f11`), O01
  (`6db62e57d`); L04 host-state follows the profile (`fece33038`).
- Phase 7 (`ef250f877`): icon dispositions for every favored-class and
  Mostly Human identity, `docs/FAVORED-CLASS-TARGET-MANIFEST.md`, fail-safe
  guards on the two new postfixes.
- Final local candidate: `ef250f877` (DLL `fe451f35…`, MVID
  `2c9d8c5f-c424-4179-91cd-72ca85438c54`, package `5947ea17…`). Its full
  guarded regression, L01 persistence and working-save smoke are recorded in
  `FAVORED-CLASS-IMPLEMENTATION-REPORT.md` and `FAVORED-CLASS-COVERAGE.json`.
  Status: PARTIAL — NOT RELEASE QUALIFIED; owner review pending.

Read-only audit reports (private, not committed):
`C:\Dev\KingmakerGunslingerLab\private\favored-class-mission\audits\` A–G.

## Shared-environment change log (restore obligations)

Deployments use `scripts\Deploy-Local.ps1` (backs up the live mod tree,
preserves `FeatureModules.json` bytes). The owner's pre-mission install
(0.0.136, DLL `c6cccdac…`, Info `f66de05d…`, FeatureModules `6e24b278…`) is
held byte-identically in two verified backups:
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260923T2118026258530Z`
(pre-mission lab operation) and
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z`
(first mission deployment). Restore at the end with:

```powershell
.\scripts\Restore-Live-Mod.ps1 -BackupDirectory C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z -Confirm:$false
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
| 20260924T0128156041063Z | `fba7516d3` | `568df1a2…` | `20260924T0128101694163Z` |
| 20260924T0137206615261Z | `cf8a66d21` | `580a668c…` | `20260924T0137156318614Z` |
| 20260924T0154375760971Z | `732cddae3` | `563bbb35…` | `20260924T0154330289272Z` |
| 20260924T0208433668788Z | `9ee86bfbe` | `7fb6b259…` | `20260924T0208386740818Z` |
| 20260924T0336453212821Z | `11b488be3` | `77d5a6e8…` | `20260924T0336405726039Z` (holds `7fb6b259…`: nothing foreign changed the tree in between) |
| 20260924T0347379474215Z | `f2db844c2` | `5a278b75…` | `20260924T0347332630857Z` |
| 20260924T0417550988006Z | `cc0168d89` | `bf21df18…` | `20260924T0417505303610Z` |
| 20260924T0438148659964Z | `94cb93919` | `5dcac0cc…` | `20260924T0438102353175Z` |
| 20260924T0452445149702Z | `07c21f29b` | `93de2dee…` | `20260924T0452399299405Z` |
| 20260924T0534033280786Z | `9b3db78bf` | `d4c9535f…` | `20260924T0533587963124Z` |
| 20260924T0549439890701Z | `6db62e57d` | `245d3c65…` | `20260924T0549393364348Z` |
| 20260924T0601504282380Z | `ef250f877` | `fe451f35…` | `20260924T0601455989736Z` (final candidate) |

The isolated H01 profile run (`compat-20260924T014026Z-1da92b038b7e`) staged
and restored its own Mods tree transactionally (restoration verified by the
profile runner: FeatureModules and CotW settings bytes restored).

Temporary settings profiles wrote `Mods\KingmakerGunslinger\FavoredClassIntegration.json`
only for their own runs and removed it afterwards (verified absent after each).

Restoration of the owner's pre-mission install: VERIFIED 2026-09-24T06:18Z. `Mods\KingmakerGunslinger` was restored from `20260924T0027014050156Z` with `scripts\Restore-Live-Mod.ps1`: DLL `c6cccdac…`, Info.json `f66de05d…` and FeatureModules.json `6e24b278…` match, all 238 files are identical to the backup (path and SHA-256), `FavoredClassIntegration.json` is absent, and the Favored Class, Call of the Wild and Races Unleashed settings are unchanged.

## Next concrete actions

1. Owner review of the local candidate and the owner decisions in
   `FAVORED-CLASS-BLOCKERS.md` (OD-1 … OD-10, D1, D2).
2. If requested: the NOT RUN families (E11, E16, L02, L03, L06 native),
   fresh-process persistence for the non-Gunslinger counters, the CotW-only
   profiles (needs the missing fixture root, B2) and art for the 127
   placeholder choices.
3. Nothing is pushed, merged, tagged or published; those remain separate
   owner actions.
