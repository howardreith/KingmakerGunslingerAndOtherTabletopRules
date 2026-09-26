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

## Fourth review follow-up checkpoint

Status: PARTIAL - NOT RELEASE QUALIFIED. A follow-up to the fourth review found that
`FavoredClassRangeGroup.Feet` kept widened text for a member unresolved only
by a failed liveness read. Fix: native text while any member is unresolved;
the liveness, randomized and guarded tests follow. Requalification: the full
domain suite, two identical clean Release builds, the Bard range, lifecycle,
visual-census, smoke and persistence stages on the exact new DLL, then the
complete final gate.

## Fourth review checkpoint

Status then: COMPLETE LOCALLY - AWAITING OWNER REVIEW (superseded by the follow-up checkpoint
above). The fourth PR #24 review found two defects in
`12651613c`, both fixed and requalified: Dead Shot's confirmation fails on a
natural 1 and succeeds on a natural 20 before the total is compared with the
critical AC (`7054b3917`), and the Bard range group keeps a member tracked
(blocking widening, with a diagnostic and a later retry) until its native
state or its actual ending is verified (`ff55924c2`).

Candidate `4748a47b2b8f90fb544fde2fbddbbfc7e225102a` (package `cbec54d159ce7aec524852d738bd55ae5080df91c0ee42cd0b025c4ba1306237`, DLL `a35f338ccbd6fd92f152af17d98a92b5b58b37ddbedf2e2d4f13f15b66f00981`, MVID `bf422dd9-16e7-4027-9086-b01e1a71b9a5`),
deployment `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260926T1114257470410Z\deployment.json`: the 1,854-case domain suite, two identical clean builds,
the focused lanes and the complete final gate (41 guarded runs PASS; report
sections 4 and 14).

## Completion round checkpoint

Candidate `12651613c6bbf8533884e7f73cf1b5dd0a5eaf39` (package `95d57d55c31c829ae4f7175b01df310a2081100af4cc207d49b2e9d9d91d8ec1`, DLL `ee0a67e40ebf5a926ef7df4d84485d3702f3c52e0677ec91b51a00e83bbf9f8f`, MVID `62badef3-342b-409a-9733-20dc6a664578`),
deployment `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260926T0424273426492Z\deployment.json`. The owner's directive was to finish every remaining
item; the owner decided D1 and D2 (tabletop rules) and authorized the host and
dependency stagings (H02 disabled host, simulated H04/H05, L06).

- Every charter scenario family is observed natively: E10, E15, M07, M10,
  M12, M13, M14, M24 and L07 closed; H02 native (disabled host) and simulated
  (unsupported, partial); H04 and H05 simulated on cloned live observations;
  L06 save tested (missing-dependency transaction).
- The own persistence cases of G05, G06 (Dodge), G11, G17, G21, I01, I05, I07,
  O04, O05, O08, U02, S04 and S06 are save tested.
- The fifteen deferred targets are published (32 appended identities).
- Found and fixed: D5 (stored level plans) and the M10 Pistol-Whip CMB leak
  under the owner's mod stack; recorded for the owner: D6.
- B3 re-measured: the committed memory comes from the lanes that drive the native
  character-build screens (the census peaks at 43 GB with no screenshot, the
  creator lanes at up to 72 GB), not from screenshots and not from the
  favored-class integration (an integration-off creator run peaked at 70.1 GB).
  It stays an open environment risk for qualification batches.
- Final qualification: 40 guarded runs PASS (report section 4).

Status then: COMPLETE LOCALLY - AWAITING OWNER REVIEW (superseded by the fourth review checkpoint above).

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

## PR #24 third review checkpoint

The third review of PR #24 (at `d13a1589e`) found three follow-on problems in
the Bard outcome state machine, all addressed:

1. a success can no longer coexist with older failed or deferred live areas
   of the same bard's performance: the live areas widen only as a whole
   (`4de114511`);
2. a sibling rollback restores the ring and the radius independently and
   verifies both; an area that cannot be verified native is ended, and the
   toggle whose own current buff runs it is turned off (`4de114511`; the
   toggle match, found in this pass's own review, follows the area context's
   ancestry: `a17c2efe7`; lane follow-ups `51f56820e` and `5636d940b`);
3. no outcome is remembered after its area ends (`4de114511`).

The first requalification of `4de114511` (before a machine reboot) passed
every lane except where the machine's exhausted commit charge interrupted it:
the Ifrit creator run ended with the game process exiting three times, and
one Sylph creator run reported Out of memory (its rerun passed). About 66 GB
was committed with no game running, and the game's private bytes reached
60 GB (B3). The reboot cleared it (15 GB committed with nothing running).

Final candidate: `5636d940b` (package `e7828df6...`, DLL `197ba4e5...`, MVID
`76aef19e-962c-427d-b10e-864274b981b0`), 33 guarded runs PASS, owner
install restored and verified. Status: PARTIAL - NOT RELEASE QUALIFIED; owner
review pending.

## PR #24 second review checkpoint

The second review of PR #24 found two remaining paths, both addressed:

1. a Bard's feature, toggle and action-bar descriptions follow the actual
   widening outcome of that bard's live areas (`7f9c781d8`);
2. an excluded, partial, unadmitted or withheld revelation target's counter
   keeps its saved ranks and is mechanically inert; the bloodline-power and
   performance counters share the withholding guard (`f7b4ae4f5`).

Final candidate: `f7b4ae4f5` (package `1c4127fc...`, DLL `faa7ecad...`, MVID `60acfa62-36ed-4722-9bb3-af928f5d8758`),
33 guarded runs PASS, owner install restored and verified. Status:
PARTIAL - NOT RELEASE QUALIFIED; owner review pending.

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
| 20260925T1323002022844Z | `f7b4ae4f5` | `faa7ecad...` | `60acfa62-36ed-4722-9bb3-af928f5d8758` | `20260925T1322555882117Z` |
| 20260925T1426308025611Z | `f7b4ae4f5` | `faa7ecad...` | `60acfa62-36ed-4722-9bb3-af928f5d8758` | `20260925T1426260728248Z` (profile) |
| 20260925T1429494091294Z | `f7b4ae4f5` | `faa7ecad...` | `60acfa62-36ed-4722-9bb3-af928f5d8758` | `20260925T1429447280940Z` (profile) |
| 20260925T1433314390845Z | `f7b4ae4f5` | `faa7ecad...` | `60acfa62-36ed-4722-9bb3-af928f5d8758` | `20260925T1433266105214Z` (profile) |
| 20260925T1532361168725Z | `4de114511` | `4effec58...` | `02a4d150-8419-4645-92f1-3c407f7585ee` | `20260925T1532312414478Z` |
| 20260925T2029203755841Z | `51f56820e` | `2eb28832...` | `229dff67-8bdf-4b41-9c44-e76bbd251da5` | `20260925T2029158628588Z` |
| 20260925T2042107499390Z | `5636d940b` | `197ba4e5...` | `76aef19e-962c-427d-b10e-864274b981b0` | `20260925T2042062113831Z` |
| 20260925T2141418189823Z | `5636d940b` | `197ba4e5...` | `76aef19e-962c-427d-b10e-864274b981b0` | `20260925T2141373030765Z` (profile) |
| 20260925T2144577711475Z | `5636d940b` | `197ba4e5...` | `76aef19e-962c-427d-b10e-864274b981b0` | `20260925T2144531218406Z` (profile) |
| 20260925T2148287351239Z | `5636d940b` | `197ba4e5...` | `76aef19e-962c-427d-b10e-864274b981b0` | `20260925T2148241245341Z` (profile) |

The `4de114511` deployment's backup holds the owner's restored 0.0.136 tree
(DLL `c6cccdac...`); that candidate stayed installed over the machine reboot,
and the `51f56820e` deployment backed it up (DLL `4effec58...`) before
replacing it.

Final restoration of the owner's install after the third PR #24 review round:

The owner's pre-mission KMG install (0.0.136, backup
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z`)
was restored through `scripts/Restore-Live-Mod.ps1` (under this lab's lease)
after the last run, and verified byte for byte on 2026-09-25 at 21:51 UTC:

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
  (`20260925T2130213710790Z_ca397ca4f0124a0c9ddf9fb9a1497c94`); each
  compatibility profile (`compat-20260925T213940Z-2ad3bc526451`,
  `compat-20260925T214311Z-3b10efecc949`, `compat-20260925T214643Z-ffa851fb8dd2`)
  restored the exact original Mods tree and FeatureModules bytes before
  releasing the lock.

Restoration status: VERIFIED.

Completion round deployments (guarded `scripts\Deploy-Local.ps1`; each backed up the previous tree
under `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\`; rows marked profile were
deployed inside an isolated compatibility profile and restored by its transaction):

| Deployment (UTC) | Commit | DLL SHA-256 | MVID | Backup of the previous tree |
| --- | --- | --- | --- | --- |
| 20260926T0036028458970Z | `dbe42baa0` | `2451cd04...` | `656c0a1e-167f-4302-bb9c-d05a757fc8b3` | `20260926T0035582634617Z` |
| 20260926T0046425713667Z | `dbe42baa0` | `ed111fc7...` | `17286783-35c9-4f0d-acb7-9a03d8215aa8` | `20260926T0046380556982Z` |
| 20260926T0101291732894Z | `54d545e4b` | `2cb031e8...` | `aaf1ffc3-a939-49af-9e76-f4467cfd1ee5` | `20260926T0101235193416Z` |
| 20260926T0112224611439Z | `e191d78e1` | `27edb5f3...` | `8f6d9a13-dd4b-4c04-a51b-6ce7ac76fe84` | `20260926T0112179795272Z` |
| 20260926T0140076147763Z | `9e927fb0a` | `be7b1fe7...` | `8c7ebf7b-1806-4695-8082-d0c8deb8b898` | `20260926T0140031717966Z` |
| 20260926T0205115644947Z | `60c93b319` | `2494f234...` | `405b7662-335c-4720-b081-b18d961ebfb9` | `20260926T0205069682243Z` |
| 20260926T0223198182138Z | `178225027` | `76119fc4...` | `77c895b7-30c8-453b-9d54-5b6f9850aff9` | `20260926T0223152817628Z` |
| 20260926T0242075075466Z | `313f5b275` | `23273e68...` | `a90b3199-cd2b-4075-b3c9-8e5ce80c8496` | `20260926T0242030546400Z` |
| 20260926T0249064487791Z | `21c92d939` | `5c9cd164...` | `f2643fec-7e5a-4744-8cf0-87e5db885051` | `20260926T0249019997030Z` |
| 20260926T0300241210830Z | `d8fd0ea4a` | `512c2082...` | `d9572312-257e-4fbb-8bb5-7a1054fe5de4` | `20260926T0300196163674Z` |
| 20260926T0309202181736Z | `5ae4eafc7` | `a07c0715...` | `0a3b187d-2b0d-4b62-8257-fc1b55769eff` | `20260926T0309155539136Z` |
| 20260926T0412232808729Z | `5ae4eafc7` | `a07c0715...` | `0a3b187d-2b0d-4b62-8257-fc1b55769eff` | `20260926T0412186496114Z` (profile) |
| 20260926T0415392246601Z | `5ae4eafc7` | `a07c0715...` | `0a3b187d-2b0d-4b62-8257-fc1b55769eff` | `20260926T0415345779462Z` (profile) |
| 20260926T0419126615836Z | `5ae4eafc7` | `a07c0715...` | `0a3b187d-2b0d-4b62-8257-fc1b55769eff` | `20260926T0419080465981Z` (profile) |
| 20260926T0424273426492Z | `12651613c` | `ee0a67e4...` | `62badef3-342b-409a-9733-20dc6a664578` | `20260926T0424227072867Z` |
| 20260926T0528021983878Z | `12651613c` | `ee0a67e4...` | `62badef3-342b-409a-9733-20dc6a664578` | `20260926T0527576132057Z` (profile) |
| 20260926T0531183393450Z | `12651613c` | `ee0a67e4...` | `62badef3-342b-409a-9733-20dc6a664578` | `20260926T0531137268509Z` (profile) |
| 20260926T0534504686448Z | `12651613c` | `ee0a67e4...` | `62badef3-342b-409a-9733-20dc6a664578` | `20260926T0534458681249Z` (profile) |

The first `dbe42baa0` deployment was built in the integration worktree; every later one in the
runner worktree (`claude/favored-class-runner`, fast-forwarded to each candidate), so the same
commit and source state appear with two DLLs (two clean builds in one worktree are identical).

Final restoration of the owner's install after the completion round:

The owner's pre-mission KMG install (0.0.136, backup
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z`)
was restored through `scripts/Restore-Live-Mod.ps1` (under this lab's lease)
after the last run of the completion round, and verified byte for byte on
2026-09-26 at 05:44 UTC:

- `Info.json` SHA-256 `f66de05d5c6282eece8218b6c4f31d49dfc8ef9efa27dfeceeda712034717e17` (match True)
- `FeatureModules.json` SHA-256 `6e24b2788a0c8f063d6e561a27c93f9c5349f2fc21b5217689da8aefdbb385d0` (match True)
- `KingmakerGunslinger.dll` SHA-256 `c6cccdac914ed59fa4d85d020108588d7d12cfb4ac38cf5a162772bacc9b465c` (match True)
- whole tree: 238 backup files, 238 live files, 0 only in the backup, 0 only live
- no `FavoredClassIntegration.json` remains (the default settings apply)
- other mod files unchanged: `CallOfTheWild\settings.json` `24cc3f80...`, `RacesUnleashed\Settings.json` `270899c3...`,
  `ZFavoredClass\settings.json` `bdceed77...` and Tweak or Treat's regenerated `loaded_blueprints.txt` `96e13782...` (match True)
- Unity Mod Manager's `Params.xml`: every disabled-host stage of the round (`20260926T022705Z`, `20260926T030221Z`, `20260926T040626Z`, `20260926T052206Z`) and each L06
  transaction backed it up, disabled only ZFavoredClass and restored the exact pre-stage bytes after
  the game exited (SHA-256 match; backups in `C:\Dev\KingmakerGunslingerLab\runtime-backups\umm-params\`
  and the transaction folders); the regenerated `loaded_blueprints.txt` files were restored byte for byte
  after each disabled-host run; ZFavoredClass is enabled now
- each temporary settings profile removed `FavoredClassIntegration.json` after
  its own runs (verified absent after each profile and at the end)
- the B3 memory diagnostic wrote the integration-off `FavoredClassIntegration.json` only for its one
  run and removed it (verified absent)
- the persistence transaction `20260926T0515255118814Z_3bbf199cd52b4a5eb9d4d430a20064b1` restored the settings and the complete Mods tree;
  its owned save was deleted from the save folder with hash proof (94 preexisting saves preserved;
  the prepared save's evidence copy stays in the machine-local transaction folder)
- the L06 transaction `20260926T0523169075109Z_328953d2b5034ecb8d9ad783de2b1b80` restored `Params.xml` (match True) and deleted its fixture from the save
  folder with hash proof (its evidence copy stays in the machine-local transaction folder); the lab's
  only saves are `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING`
- each compatibility profile (`compat-20260926T052602Z-a75f8c60d59d`, `compat-20260926T052931Z-3aacec891548`, `compat-20260926T053303Z-0acfe3e6e607`) restored the exact original Mods tree and
  FeatureModules bytes before releasing the lock (restoration verified)

Restoration status: VERIFIED.

Fourth review deployments (guarded `scripts\Deploy-Local.ps1`; each backed up the previous tree
under `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\`; rows marked profile were
deployed inside an isolated compatibility profile and restored by its transaction):

| Deployment (UTC) | Commit | DLL SHA-256 | MVID | Backup of the previous tree |
| --- | --- | --- | --- | --- |
| 20260926T1036471431158Z | `ff55924c2` | `a75b9b47...` | `cec9fb76-85d0-4406-991d-2dff5c5041c6` | `20260926T1036425119410Z` |
| 20260926T1048226979589Z | `d94f94216` | `0d4472d5...` | `c1e95bf7-e22e-4949-aef9-9f89931e80a6` | `20260926T1048181157956Z` |
| 20260926T1103286070910Z | `4748a47b2` | `a35f338c...` | `bf422dd9-16e7-4027-9086-b01e1a71b9a5` | `20260926T1103239552852Z` |
| 20260926T1114257470410Z | `4748a47b2` | `a35f338c...` | `bf422dd9-16e7-4027-9086-b01e1a71b9a5` | `20260926T1114211436511Z` |
| 20260926T1218350022281Z | `4748a47b2` | `a35f338c...` | `bf422dd9-16e7-4027-9086-b01e1a71b9a5` | `20260926T1218304756703Z` (profile) |
| 20260926T1221511146180Z | `4748a47b2` | `a35f338c...` | `bf422dd9-16e7-4027-9086-b01e1a71b9a5` | `20260926T1221464986774Z` (profile) |
| 20260926T1225242845901Z | `4748a47b2` | `a35f338c...` | `bf422dd9-16e7-4027-9086-b01e1a71b9a5` | `20260926T1225196704469Z` (profile) |

Final restoration of the owner's install after the fourth review's requalification:

The owner's pre-mission KMG install (0.0.136, backup
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z`)
was restored through `scripts/Restore-Live-Mod.ps1` (under this lab's lease)
after the last run of the fourth review's requalification, and verified byte for byte on
2026-09-26 at 12:28 UTC:

- `Info.json` SHA-256 `f66de05d5c6282eece8218b6c4f31d49dfc8ef9efa27dfeceeda712034717e17` (match True)
- `FeatureModules.json` SHA-256 `6e24b2788a0c8f063d6e561a27c93f9c5349f2fc21b5217689da8aefdbb385d0` (match True)
- `KingmakerGunslinger.dll` SHA-256 `c6cccdac914ed59fa4d85d020108588d7d12cfb4ac38cf5a162772bacc9b465c` (match True)
- whole tree: 238 backup files, 238 live files, 0 only in the backup, 0 only live
- no `FavoredClassIntegration.json` remains (the default settings apply)
- other mod files unchanged: `CallOfTheWild\settings.json` `24cc3f80...`, `RacesUnleashed\Settings.json` `270899c3...`,
  `ZFavoredClass\settings.json` `bdceed77...` and Tweak or Treat's regenerated `loaded_blueprints.txt` `96e13782...` (match True)
- Unity Mod Manager's `Params.xml`: every disabled-host stage of the round (`20260926T121237Z`) and each L06
  transaction backed it up, disabled only ZFavoredClass and restored the exact pre-stage bytes after
  the game exited (SHA-256 match; backups in `C:\Dev\KingmakerGunslingerLab\runtime-backups\umm-params\`
  and the transaction folders); the regenerated `loaded_blueprints.txt` files were restored byte for byte
  after each disabled-host run; ZFavoredClass is enabled now
- each temporary settings profile removed `FavoredClassIntegration.json` after
  its own runs (verified absent after each profile and at the end)
- the persistence transaction `20260926T1205578501702Z_2d76d8dea8fa4d318f97fd297772ec8f` restored the settings and the complete Mods tree;
  its owned save was deleted from the save folder with hash proof (94 preexisting saves preserved;
  the prepared save's evidence copy stays in the machine-local transaction folder)
- the L06 transaction `20260926T1213475709412Z_99e2a6b32050436c94ec26ff6275e748` restored `Params.xml` (match True) and deleted its fixture from the save
  folder with hash proof (its evidence copy stays in the machine-local transaction folder); the lab's
  only saves are `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING`
- each compatibility profile (`compat-20260926T121633Z-5083c0ce5fb9`, `compat-20260926T122004Z-b08da2da404f`, `compat-20260926T122336Z-c6fb2faf6f8b`) restored the exact original Mods tree and
  FeatureModules bytes before releasing the lock (restoration verified)

Restoration status: VERIFIED.

## Next concrete actions

1. Fix the fourth review's follow-up finding (report section 14) with its domain
   and guarded tests, then requalify as the follow-up checkpoint states.
2. Owner review of PR #24 at the requalified candidate, including the
   simulated disposition of H04/H05 and the open owner decision D6
   (`FAVORED-CLASS-BLOCKERS.md`).
3. Nothing is merged, tagged or published; those remain separate owner
   actions.
4. B3 follow-up (harness, outside the favored-class rows): isolate the allocation
   the native character-build screens retain; until then start qualification
   batches only with a low idle commit charge.
