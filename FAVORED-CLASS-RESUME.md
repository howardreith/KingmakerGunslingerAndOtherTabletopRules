# Favored Class Integration — Resume

Read `FAVORED-CLASS-MISSION.md` first, then this file, then
`FAVORED-CLASS-STATE.json` and `FAVORED-CLASS-BLOCKERS.md`. Re-inspect the
actual Git and installation state before running anything; never replay a
stale install, fixture or cleanup command from this file.

## Location

| Item | Value |
| --- | --- |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\favored-class-integration` |
| Branch | `claude/favored-class-integration` (pushed to PR #24 on the owner's instruction; never merged) |
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

## Continuation checkpoint (repair, qualification and evidence cleanup)

The owner's continuation brief (2026-09-24) replaced the owner-decision list
with charter rules and required tests. This pass, on the same branch and
worktree, completed:

- Mostly Human as a genuine dual identity (human humanoid for race-related
  rules and the host's human prerequisites; native outsider identity, race,
  RaceId, scores and geniekin routes kept; fail closed for other providers).
  The identity is a class feature granted only by the trait, so a native
  respec never carries it over on its own.
- O01 redesign: owner-local actual range, ring and description; Storm Call
  and Mockery excluded; the manifest classified by mechanics with exact read
  points (`docs/FAVORED-CLASS-TARGET-MANIFEST.md`).
- Donor icons for every published choice; domain census; native visual
  census of every reward selector (the host routes it to the Determinator
  phase) and of the four Mostly Human selectors in the actual creator.
- E11 multiclass, E16/L03 respec (Gunslinger, Mostly Human, Sorcerer selected
  power, Ranger with a companion), L02 lifecycle (two paladins, two bards,
  death, polymorph, party area reload) and L01 fresh-process reload of one
  subject per state/mechanic family, including a selected firearm target.
- The older elemental creator (4 races) and native respec (Sylph, Oread)
  lanes re-run with Mostly Human published.
- B2: the CotW-only and CotW + Favored Class profiles staged from the
  byte-verified 2026-09-08 capture
  (`C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger\artifacts\teleportation\compatibility-references`).

Shared-installation protocol with the KingmakerBuffPlanner lab: check its
`runtime-state\deployment.lock` and `kbp-gate.active` markers, hold this lab's
`compatibility.lock` for a whole batch, message that session at batch start
and end, and never touch its files, saves or processes.

Scratchpad drivers used by this pass (machine-local, not committed):
`leased-batch.ps1` (`name[@Save][#k=v;...]`), `batch-and-persist.ps1`,
`final-stage.ps1`, `leased-profiles.ps1` (temporary settings profiles),
`compat-batch.ps1`, `restore-owner.ps1` and `post-stage.ps1`.

Final local candidate of the continuation: `15c37695c` (package `4a7b5e2d...`, DLL `0261ebd3...`, MVID `613caf93-62bb-41e8-87fa-4a154cb4cfd6`). Its
final cycle, settings profiles, persistence transaction, working-save smoke and
compatibility profiles are recorded in `FAVORED-CLASS-IMPLEMENTATION-REPORT.md`.
Status: PARTIAL - NOT RELEASE QUALIFIED; owner review pending.

## PR #24 review checkpoint

The owner's review of PR #24 listed six findings; each was addressed in
order on this branch (section 10 of `FAVORED-CLASS-IMPLEMENTATION-REPORT.md`
maps every finding to its commits, domain tests and native runs):

1. owned effects apply only while the integration is enabled and the exact
   host publication is committed (host activation);
2. the chosen revelation's own level gates and Blast's extra uses follow the
   effective level (both the native and the Call of the Wild layout); Spirit of
   the Warrior is excluded (BAB);
3. the companion projection follows one qualified desired pet (unlink,
   relink, qualification loss, dismissal, resummoning);
4. a performance's cylinder is widened only together with its ring;
5. the level-up replay and Demoralize scopes close in finally blocks;
6. the bootstrap aggregate counts every registry, Mostly Human's included.

Final candidate: `aa298650e` (package `9abc9454...`, DLL `5edb5e42...`, MVID `ecc3e660-1295-4132-8019-c01b703561d2`),
33 guarded runs PASS, owner install restored and verified. Status:
PARTIAL - NOT RELEASE QUALIFIED; owner review pending.

## First-pass checkpoint (superseded by the checkpoints above)

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

Continuation deployments (guarded `scripts\Deploy-Local.ps1`; each backed up the previous tree
under `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\`; rows marked profile were
deployed inside an isolated compatibility profile and restored by its transaction):

| Deployment (UTC) | Commit | DLL SHA-256 | MVID | Backup of the previous tree |
| --- | --- | --- | --- | --- |
| 20260924T1231430167010Z | `0138d70b6` | `bab6aaf9...` | `30900fe0-e6d9-4582-87b7-bc857cddf8db` | `20260924T1231384662202Z` |
| 20260924T1301193301166Z | `e97271392` | `adc29f04...` | `64f83a8c-f63e-4192-b553-d1a984501265` | `20260924T1301147516433Z` |
| 20260924T1318030056407Z | `08ab4d17c` | `fbe7cdfa...` | `accc0ea9-28df-46ee-8b37-c8adaa582c2b` | `20260924T1317583962386Z` |
| 20260924T1331590044054Z | `b2cf0cf81` | `1dfd0b02...` | `5cd47b62-78a0-4f9a-b204-ae8da64cb039` | `20260924T1331542631295Z` |
| 20260924T1602289102359Z | `ff434c829` | `b4fdc828...` | `6ce95367-69d6-4c62-a0d8-f9bfa4be693a` | `20260924T1602239758431Z` |
| 20260924T1625326651297Z | `1b2d4174d` | `e82dee2a...` | `dbb1a2e4-e490-4492-a1c7-2aad6e8a5064` | `20260924T1625279463297Z` |
| 20260924T1710329170172Z | `1ebd83fa9` | `c44b3ada...` | `4e74b259-2e00-47b5-9c99-d19af21479cd` | `20260924T1710282766956Z` |
| 20260924T1732404304962Z | `e3617e224` | `0b39f19d...` | `04098026-bf0c-44bb-8b12-b92e5851b1ea` | `20260924T1732355393019Z` |
| 20260924T1820508011281Z | `e3617e224` | `0b39f19d...` | `04098026-bf0c-44bb-8b12-b92e5851b1ea` | `20260924T1820462973327Z` (profile) |
| 20260924T1824157380471Z | `e3617e224` | `0b39f19d...` | `04098026-bf0c-44bb-8b12-b92e5851b1ea` | `20260924T1824112883998Z` (profile) |
| 20260924T1827526231444Z | `e3617e224` | `0b39f19d...` | `04098026-bf0c-44bb-8b12-b92e5851b1ea` | `20260924T1827479363666Z` (profile) |
| 20260924T1855251804593Z | `bd8f7c60c` | `6bda3905...` | `5267d7af-574d-4b8d-b683-db6b05b31ea2` | `20260924T1855205210682Z` |
| 20260924T1910002412197Z | `77afdde62` | `03a28f95...` | `be1cfcaa-486d-47a0-a4ec-05e6bbe397ca` | `20260924T1909556042780Z` |
| 20260924T1925499018311Z | `675b117b0` | `2ac5970f...` | `76077322-1702-46fe-bd83-5d13a13b8cd6` | `20260924T1925452180506Z` |
| 20260924T2056272827946Z | `cacf3a002` | `e85660cc...` | `47c993a5-2e5e-48dd-83f0-a5402f55b6ba` | `20260924T2056225420585Z` |
| 20260924T2155495526048Z | `cacf3a002` | `e85660cc...` | `47c993a5-2e5e-48dd-83f0-a5402f55b6ba` | `20260924T2155450157633Z` (profile) |
| 20260924T2159057722985Z | `cacf3a002` | `e85660cc...` | `47c993a5-2e5e-48dd-83f0-a5402f55b6ba` | `20260924T2159010883330Z` (profile) |
| 20260924T2202398694367Z | `cacf3a002` | `e85660cc...` | `47c993a5-2e5e-48dd-83f0-a5402f55b6ba` | `20260924T2202352148081Z` (profile) |
| 20260924T2220560490337Z | `15c37695c` | `0261ebd3...` | `613caf93-62bb-41e8-87fa-4a154cb4cfd6` | `20260924T2220514938917Z` |
| 20260924T2319493525825Z | `15c37695c` | `0261ebd3...` | `613caf93-62bb-41e8-87fa-4a154cb4cfd6` | `20260924T2319446874767Z` (profile) |
| 20260924T2323061224769Z | `15c37695c` | `0261ebd3...` | `613caf93-62bb-41e8-87fa-4a154cb4cfd6` | `20260924T2323016112056Z` (profile) |
| 20260924T2326399101015Z | `15c37695c` | `0261ebd3...` | `613caf93-62bb-41e8-87fa-4a154cb4cfd6` | `20260924T2326353817722Z` (profile) |
| 20260925T0411348835015Z | `719085d51` | `36849dc7...` | `69e74a51-319c-4c91-bf2b-67d7f094ba5d` | `20260925T0411302528284Z` |
| 20260925T0426279539132Z | `27176f424` | `3c07668a...` | `54f66021-2e12-4cd4-b719-0697c1be6147` | `20260925T0426232586030Z` |
| 20260925T0441598369963Z | `aa298650e` | `5edb5e42...` | `ecc3e660-1295-4132-8019-c01b703561d2` | `20260925T0441553147956Z` |
| 20260925T0542174416518Z | `aa298650e` | `5edb5e42...` | `ecc3e660-1295-4132-8019-c01b703561d2` | `20260925T0542126789494Z` (profile) |
| 20260925T0545354126150Z | `aa298650e` | `5edb5e42...` | `ecc3e660-1295-4132-8019-c01b703561d2` | `20260925T0545309351465Z` (profile) |
| 20260925T0549103789544Z | `aa298650e` | `5edb5e42...` | `ecc3e660-1295-4132-8019-c01b703561d2` | `20260925T0549056382959Z` (profile) |

Final restoration of the owner's install after the PR #24 review round:

The owner's pre-mission KMG install (0.0.136, backup
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z`)
was restored through `scripts/Restore-Live-Mod.ps1` (under this lab's lease)
after the last run, and verified byte for byte:

- `Info.json` SHA-256 `f66de05d5c6282eece8218b6c4f31d49dfc8ef9efa27dfeceeda712034717e17` (match True)
- `FeatureModules.json` SHA-256 `6e24b2788a0c8f063d6e561a27c93f9c5349f2fc21b5217689da8aefdbb385d0` (match True)
- `KingmakerGunslinger.dll` SHA-256 `c6cccdac914ed59fa4d85d020108588d7d12cfb4ac38cf5a162772bacc9b465c` (match True)
- whole tree: 238 backup files, 238 live files, 0 only in the backup, 0 only live
- no `FavoredClassIntegration.json` remains (the default settings apply)
- other mod settings unchanged: `CallOfTheWild\settings.json` SHA-256 `24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8` (match True)
- other mod settings unchanged: `RacesUnleashed\Settings.json` SHA-256 `270899c3f6c3d29bfe777fc2b55a0bb0404ae50786dbac8f1dccf77e8b9cabbf` (match True)
- other mod settings unchanged: `ZFavoredClass\settings.json` SHA-256 `bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b` (match True)
- each temporary settings profile removed `FavoredClassIntegration.json` after
  its own runs (verified absent after each profile and at the end)
- the persistence transaction restored the settings and the complete Mods tree
  (`20260925T0531109526835Z_0f112e09e2c341e4b594527cf2b02a11`); each compatibility profile restored the exact original Mods tree
  and FeatureModules bytes before releasing the lock.

Restoration status: VERIFIED.

## Next concrete actions

1. Owner review of PR #24 at `aa298650e` and of the two pre-existing KMG defects
   D1 and D2 (`FAVORED-CLASS-BLOCKERS.md`).
2. To reach COMPLETE, observe natively the partial families E10, E15, M07, M10, M12, M13, M14, M24, L07:
   turn-based variants of the lanes (L07, M12), True Grit at level 20 (M13),
   bomb splash and critical (M14), the misfire ammunition and condition
   matrix (M07), the deed interruption path (M10), native auto-level (E15),
   per-revelation values for all published revelations (M24) and an injected
   permission cycle or duplicate fact (E10). H02, H04, H05 and L06 stay
   domain tested unless the owner authorizes staging a host or dependency state.
3. The fifteen deferred threshold-only targets (fourteen revelations and
   Elemental Resistance) need new owned identities before they can be
   published.
4. Nothing is merged, tagged or published; those remain separate owner
   actions.
