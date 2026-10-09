# v0.0.146 normal-merge integration ledger

Status: source integration candidate; NOT runtime-qualified or released.

## Exact parents and preserved refs

- Expanded Summoning/module fix: c6effc3224754e1bdf250a703eb1440c1dbbfb78; prior reporting30be632cdf4e9cb85c70e95e85773b89d92c5311.
- Current released master/v0.0.145: bf8a1e41b308bfb148cb028d75a576555b7e8dd1.
- Common ancestor: 4ba8d4aca087391144abf401f526189f59b26535 (released v0.0.142).
- Normal merge, no force-push/rebase/reset. PR29 remains the integration PR against master.
- Permanent safety refs: codex/local-safety/release146-intake-pr29-30be632c; release146-intake-master145-bf8a1e41; release146-module-boundary-c6effc32; release146-premerge-pr29-c6effc32; release146-premerge-master145-bf8a1e41 (all share codex/local-safety/ prefix). All older refs preserved.
- Old unreleased v143 summoning records and every failed attempt remain intact. Master v143 findability notes win their add/add collision; the summoning notes advance to docs/RELEASE-NOTES-0.0.146.md, with their original bytes preserved at c6effc32.

## Conflict/hunk dispositions

The 35 conflicted files contained 56 marker hunks. Non-conflicting master hunks were retained, not replaced by entire older files. Hunk indices follow the original merge output.

| File | Hunks | Resolution |
| --- | --- | --- |
| CHANGELOG.md | 1 | New146 candidate entry;master145/144/143/history kept verbatim. |
| Directory.Build.props | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| INSTALLATION-COMPATIBILITY.md | 1 | 146 archive/install guidance;master145/144/141 history retained. |
| Info.json | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| README.md | 1 | 146 pending qualification summary plus master release history;no release claim. |
| compatibility/profiles.json | 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| compatibility/profiles.schema.json | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| docs/RELEASE-NOTES-0.0.143.md | 1 | Master143 findability notes kept byte-exact;unreleased summon notes moved forward to146;original in c6effc32 history. |
| scripts/Build-Local.ps1 | 1, 2, 3 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| scripts/Publish-Release.ps1 | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| scripts/RuntimeAutomation.Common.ps1 | 1 | Union targeted summon scope checks/counts with weaponSave/weaponRoute counts;automatic-exit and strict-key guards retained. |
| scripts/package.ps1 | 1, 2 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| src/KingmakerGunslinger/Development/DevelopmentUi.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| src/KingmakerGunslinger/Properties/AssemblyInfo.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs | 1 | Union strict C# parameter counts;keep expanded crowdReview and targetedSummonPersistence plus master weaponSave/weaponRoute. |
| tests/KingmakerGunslinger.DomainTests/EasternFavoredCompatibilityTests.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| tests/KingmakerGunslinger.DomainTests/ElvenBranchedSpearCatalogTests.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| tests/KingmakerGunslinger.DomainTests/ExpandedSummoningPresentationTests.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| tests/KingmakerGunslinger.DomainTests/ExpandedSummoningSprint11Tests.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| tests/KingmakerGunslinger.DomainTests/ExpandedSummoningSprint12Tests.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| tests/KingmakerGunslinger.DomainTests/PaperCartridgeFoundationTests.cs | 1 | Union historical package counts including master291 and qualified full-roster325. |
| tests/KingmakerGunslinger.DomainTests/ScrollItemIconTests.cs | 1 | Qualified additive assets/count325 retained;active identity146 (master-only nonconflict hunks preserved). |
| tools/create_deterministic_package.py | 1 | Union historical package counts including master291 and qualified full-roster325. |
| tools/validate_bodyguard90.py | 1, 2 | Union historical master version acceptance plus146, no historical record rewrite. 146 suffix added before unchanged master release suffix chain. |
| tools/validate_compatibility72.py | 1 | Union historical master version acceptance plus146, no historical record rewrite. |
| tools/validate_craft_magic_items99.py | 1 | Union historical master version acceptance plus146, no historical record rewrite. |
| tools/validate_eastern_favored93.py | 1 | Master historical keys preserved;146 active count record added. |
| tools/validate_elemental_heritages115.py | 1 | Union historical master version acceptance plus146, no historical record rewrite. |
| tools/validate_expanded_summoning_phase1.py | 1 | Full qualified Phase2B asset inventory70+master4 retained;146 aggregate package325. |
| tools/validate_fatigue_authority106.py | 1 | Union historical master version acceptance plus146, no historical record rewrite. |
| tools/validate_firearm_audio96.py | 1 | 146 suffix added before unchanged master release suffix chain. |
| tools/validate_protection_from_alignment110.py | 1 | Union historical master version acceptance plus146, no historical record rewrite. |
| tools/validate_repository.py | 1, 2 | Master143/144/145 dispatch unchanged;append146 integration validator. |
| tools/validate_spear79.py | 1 | 146 suffix added before unchanged master release suffix chain. |
| validation/static-validation.json | 1, 2, 3 | Active146 identity. Keep both historical summon and masterfindability/144/145 JSON records without changing their values;new146 record follows separately. |

## Subsequent explicit integration adjustments

- Active identity is 0.0.146 everywhere; master v144/v145 historical release metadata and notes are unchanged.
- Final strict package count is **333** (331 without the two optional soundbank files): qualified325 + seven existing master findability documents + new146 release notes. The preliminary conflict choices above retained325 before this explicit union inventory calculation. No executable asset is dropped.
- Master production is byte-pinned by the146 validator: Elemental Races, Favored Class, Acquisition/recovery, weapon placement/item/crafting contracts, Heirloom Nodachi icon, native trait-save protocol and historical notes/reference data.
- Qualified summoning production/assets are byte-pinned to c6effc32, which includes the narrow source-qualified module gate. Runtime qualification remains pending. The already-qualified flight teardown fixes survive unchanged.
- Stable blueprint registry is the exact master prefix plus qualified additive entries:2922 entries,2920active,2reserved; no reorder or regenerated identity. Icon registry hash/exports unchanged.
- Shared request validators combine the targeted summoning persistence/crowd guards with master weapon-save/weapon-route guards. No save permission is broadened.
- Request-local representative player-path scope uses the EXISTING scenario. Default remains exhaustive. Representative mode explicitly labels its six generated family/quantity routes and native family/quantity/alignment representatives; fixed real-parent/slot/template matrix retained. It never claims exhaustive PASS.
- Read-only weapon-findability fixture adds actual Heirloom Nodachi selection/three-choice icon identity checks; no production remap.
- New146 validator chains inherited full source checks while preserving master143/144/145 dispatch and immutable records. Historical v143 checkpoint validator remains evidence; the active integration-corruption suite targets146.

## Evidence reuse boundary

Master142..145 does not touch summoning publication, creature implementation or summon player-path production. The merge adds weapon recovery and request-local fixtures/shared request validation. The final summon implementation equals the source-qualified module boundary c6effc32. Therefore prior1008 generated roots/29wrappers, Crocodilians212, snakecommands71, Salamander, enabledcleanup119/absence6 and flight26 remain original-provenance evidence. Final146 still requires inventory/menu, representative real paths, all nine variants ON, the directly affected whole-roster OFF cleanup/absence, five-profile matrix and focused coexistence. No historical failed result is relabeled.

No merge to master/tag/release until every required gate passes. No Sprint18 or Phase2C.
