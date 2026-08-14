# Eastern Weapons journal

## 2026-08-14 - repository gate

- Read the governing repository, architecture, runtime, working-save, and Elven
  Branched Spear implementation/qualification documents before source changes.
- Confirmed repository root
  `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger` and upstream
  `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Ran `git fetch --all --prune`.
- Confirmed local `master` and `origin/master` are identical at
  `4ffd15b09992bd9cee9d330eee0a650ad2c94661`, with divergence `0 0` and a
  clean worktree.
- Confirmed active version `0.0.79` and the complete landed Elven Branched Spear
  implementation. Per explicit user correction, the authoritative static child
  title is **Weapon Proficiency (Elven Branched Spear)**; new Eastern exotic
  children will follow the same structure.
- Repository validation passed.
- Complete clean Release domain/reflection suite passed: `1033/1033`.
- Clean exact-reference Release build and build-output validation passed.
- Created `codex/eastern-weapons` directly from the verified `origin/master`.
- No runtime launch, save access, package deployment, optional-mod transaction,
  or source implementation occurred during the gate.

## Next

Inspect the landed spear/custom-category, bootstrap, module, commerce,
runtime-testing, compatibility, packaging, Blender, and Unity implementations.
Then add and run the save-free `observe-eastern-weapon-contracts` investigation
before selecting production donors, native enchantments, effect hooks, category
values, or campaign targets.

## 2026-08-14 - investigation observer source

- Added `observe-eastern-weapon-contracts` as an autonomous, save-free guarded
  runtime scenario.
- The observer inventories all required weapon donor families, approved native
  enchantment families, installed proficiency/grip/combat/size rule types,
  compatible selectors, vendor tables, campaign loot, and direct owners.
- Added focused source guards that prohibit save, inventory, selector, loot, and
  blueprint mutation from the observer.
- Production category values, GUIDs, donors, enchantment references, prices, and
  campaign placements remain intentionally unselected pending live evidence.
- Repository validation passed; the complete suite passed `1034/1034`; the
  clean Release build, build-output validation, deterministic package creation,
  and strict standalone validation passed for the observer checkpoint.

## 2026-08-14 - observer timeout refinement

- The first guarded save-free run
  `20260814T1104529826303Z-b2fcb4605fd84bbabd97ad2bf6af9aa2` did not produce a
  result before the 120-second orchestration timeout. No save interaction
  occurred. The request was accepted, but the first runner update arrived only
  after the locally active compatibility stack completed its long blueprint
  initialization window.
- Narrowed the observer's expensive discovery predicates to component type
  names, restricted direct-reference scans to plausible campaign owners, and
  added phase timing events. No production choice or mutation was introduced.
- The refinement passed repository validation, `1034/1034` tests, clean Release
  build, deterministic packaging, and strict package validation.
- The mandatory guarded push helper rejected the exact authorized branch
  `codex/eastern-weapons` because its external allowlist contains only
  `codex/eastern-weapon`. No direct push or branch rename was used.

## 2026-08-14 - installed contract observation PASS

- Guarded save-free run
  `20260814T1110588439047Z-7f131097a8ca48ac916f675e77b57c47`
  passed on exact source commit `41b7687079f380a044ffed3a0bf0d3dac771228e`.
- Loaded version, DLL hash, and MVID matched the deployed candidate. No save
  interaction occurred and the guarded process exited after completion.
- Recorded exact identities for eight native weapon donors and sixteen approved
  native enchantment contracts, including Brilliant Energy's native Undead and
  Construct exclusions and Speed's native Haste-marked extra attack.
- Mighty Cleaving, Impact/size, member-level coup-de-grace, category collision,
  and bastard-sword grip authority require one narrower follow-up inventory;
  no production identity or donor has been chosen yet.
- Extended the same save-free observer with numeric category occupancy, every
  installed weapon-type group contract, alternate mechanic blueprint names,
  and loaded CLR types selected by declared member names. The follow-up source
  passed validation, `1034/1034` tests, clean build, deterministic packaging,
  and strict package validation.

## 2026-08-14 - targeted mechanic observation PASS

- Guarded save-free run
  `20260814T1119161920060Z-d07fac81ae644db0ac092e1fa3cfa3fe`
  passed on source commit `34f3093118ef028242f39e3f63e497a9c16a7580`.
- Loaded version `0.0.79`, DLL SHA-256
  `57B42B4F18FC05614AC7078564CB2D0A83536480A1C97CF3BBA1DA771FD32A7E`,
  and MVID `5337c8ba-2d31-4c60-a39f-34017ce40339` were recorded. No save
  interaction occurred.
- Proved category values `0x004B4D48` through `0x004B4D4A` are unoccupied
  across the live 136-weapon-type graph.
- Selected `ItemEntityWeapon.HoldInTwoHands` as the shared exact authority for
  katana proficiency and Moonlit Crossing. Selected
  `RuleCalculateWeaponStats.IncreaseWeaponSize` for Unfixed Form and exact
  `RuleAttackRoll` hit/confirmed-critical state for weapon-hit effects.
- No native Mighty Cleaving or Impact/Lead Blades enchantment contract exists
  in the installed graph. The native Cleave feats are not substitutes.
- Deadly is deferred because the installed coup-de-grace action graph exposes
  no reliable virtual-damage-only Fortitude DC rule hook.
- The guarded push helper continues to reject the exact authorized branch
  because its external allowlist contains only the singular branch spelling.

## 2026-08-14 - reusable category and sixth-module foundation

- Added the independent default-on `eastern-weapons` module, settings schema 5
  migration, immutable active/pending state, publication gates, UI label, exact
  six-Boolean guarded request contract, and 64-state matrix transaction.
- Added a data-driven custom-category definition and collision registry plus the
  three locked Eastern category profiles and twelve generic catalog records.
- Stable category values are `0x004B4D48` Wakizashi, `0x004B4D49` Katana, and
  `0x004B4D4A` Nodachi. Generic prices use base cost, base plus 300 gp
  masterwork, doubled base cost for cold iron, and base plus 2,300 gp for +1.
- Repository validation, all `1037/1037` domain/reflection tests, clean Release
  build, build-output validation, deterministic package creation, and strict
  package validation passed.
- The first six-module live run exposed a stale inherited spear observer
  expectation: the accepted merged path has one Exotic `AllFeatures` reference
  and two Finesse Training references, not four static references. No spear
  source or saved identity was changed; the observer was corrected to three.
- Corrected all-ON run
  `20260814T1137520825950Z-1974f02c68834bcb8c08805a3724c2cd`
  passed. Transactional Eastern-OFF run
  `20260814T1140244851767Z-123a6d83d9634e0f9f0de39a479164ff`
  passed and restored settings bytes with SHA-256
  `2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.
- Both runs loaded the exact local candidate with DLL SHA-256
  `5b74019e31f732eb08c56e5b615cbaf094c8fc120650b5eef78c32097cfe5048`
  and MVID `01a66571-5cbf-4c21-a386-74fda2042386`. No save was accessed.

## 2026-08-14 - stable generic weapon catalog

- Registered three production weapon types and all twelve generic items under
  every module state. Each family shares one stable category and weapon type
  across mundane, masterwork, cold-iron, and +1 forms.
- Wakizashi uses the exact locked light 1d6, 18-20/x2,
  piercing-or-slashing profile; Katana uses the locked versatile one-handed
  1d8, 18-20/x2 slashing profile; Nodachi uses the locked two-handed 1d10,
  18-20/x2 slashing-or-piercing profile. None has reach, thrown, or brace
  behavior.
- Masterwork and +1 reuse exact native enchantment blueprints. Cold iron uses
  the native item-level physical material field. Prices follow the documented
  base, 300 gp masterwork, doubled cold-iron, and +1 formula.
- Repository validation passed; the complete suite passed `1038/1038`; clean
  Release build, build-output validation, deterministic packaging, and strict
  standalone validation passed.
- All-ON run
  `20260814T1155410119533Z-cf305c900e5344d686ed42d62969399a`
  and Eastern-OFF run
  `20260814T1158109553961Z-ab0aed33e9304c618125dc1cfd1230cf`
  passed. The latter kept all 15 identities registered while suppressing
  Eastern presentation, then restored settings bytes exactly.
- The candidate DLL SHA-256 was
  `3946018FA2E1FCD1F19B13595D309391973D9404AB34F6E9DE09C47E9490760F`,
  MVID `6917d74c-2525-4677-b485-e5c36addf5e7`; package SHA-256 was
  `DD4C3C72641EEE50F42CA21B7C4D225D026BD666309DE5D3B2F2F498FDD6160F`.
  No save was accessed.

## 2026-08-14 - eighteen-item native named catalog

- Added all six Wakizashi, six Katana, and six Nodachi named item identities.
  Every item reuses its family's one stable weapon type and exact installed
  native enhancement blueprints. Paper Lantern, Wayfarer's Oath, and Border
  Sentinel use the native item-level cold-iron representation and surcharge.
- Native property arrays cover Flaming, Frost, Agile, Keen, Ghost Touch, Shock,
  Thundering, Holy, Brilliant Energy, and Speed by exact observed GUID. Heaven's
  Measure uses the preferred native Brilliant Energy contract pending its later
  realistic target-exclusion combat qualification.
- Added explicit price decomposition for all 30 weapons. Bespoke effects retain
  zero automatic enchantment cost and fixed premiums; every family is price
  monotonic, early guaranteed upgrades remain attainable, and each capstone is
  exactly effective +10.
- This slice does not claim the five bespoke effects or Mighty Cleaving; those
  remain the next dedicated rule-event implementation and qualification slice.
- Repository validation, all `1040/1040` tests, clean Release build,
  build-output validation, deterministic packaging, and strict package
  validation passed.
- Final priced-candidate all-ON run
  `20260814T1251391041848Z-c77e2fb20680498eb435791ccc2a7eb4` and Eastern-OFF
  run `20260814T1254138877422Z-a2e85e6529d84fbdbfc11ef3a1a9141e`
  passed. All 30 items remained registered in both states; publication and
  presentation were suppressed only while disabled. Settings restored exactly.
- DLL SHA-256 is
  `CD3F2346F896B8D53A1205CCF8ABFADE5434BD815FB14A7C9A77BECF84A2EF23`,
  MVID `e73f5e38-9d3d-4cb7-8195-b154c75fadf8`; package SHA-256 is
  `2227E202A0E942403E65DC30D69CD0F7007F9A24C7C7D85DA66345C9C16FE69B`.
  No save was accessed.

## 2026-08-14 - proficiency, selectors, and fighter-group publication

- Added the exact static children `Weapon Proficiency (Katana)` and
  `Weapon Proficiency (Wakizashi)`, preserving the accepted
  `Weapon Proficiency (Elven Branched Spear)` naming structure and merged
  placement immediately after Elven Curve Blade.
- Added one shared multi-category parameterized-selector runtime for the spear,
  Wakizashi, Katana, and Nodachi. The seven approved generic selectors receive
  singular WK, KA, NO entries through one merge and deterministic sort.
- Katana proficiency uses `ItemEntityWeapon.HoldInTwoHands`: broad martial
  proficiency is valid only in two-handed grip; the exact Katana fact is valid
  in either grip. Wakizashi remains exotic and Nodachi is integrated into the
  installed broad martial category grants without broadening partial grants.
- Added Light Blades for Wakizashi, Heavy Blades for Katana, and maximum-rank
  Heavy Blades-or-Polearms handling for Nodachi without duplicated training or
  reach changes. Wakizashi alone receives `Finesse Training (Wakizashi)`.
- Two guarded attempts failed closed and rolled back cleanly: the first exposed
  mistyped native proficiency donor constants; the second proved that native
  Martial Weapon Proficiency contains multiple `AddProficiencies` components.
  The final implementation reuses the exact accepted spear constants and
  selects the unique largest broad martial weapon grant.
- Repository validation, all `1039/1039` tests, clean Release build,
  build-output validation, deterministic packaging, and strict standalone
  package validation passed.
- All-ON run
  `20260814T1230559883866Z-3026975996b14809aa03ee8bfe11558a` and Eastern-OFF
  run `20260814T1233282614632Z-a9739de4c73c449e89a7a193f2f9b2f0`
  passed. Settings restored exactly to SHA-256
  `2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.
- Candidate DLL SHA-256 is
  `4F8C951143D6466DBDF2561CB38F815DD57AA4DCD2F57E2336125CE9B83D390B`,
  MVID `198d704e-5c73-487b-ae7c-5c5110f58951`; package SHA-256 is
  `D052FF1125EB45E72ADF9144D26A0A4662F7BC93EA1FAF1CC319C36E2F7ED534`.
  No save was accessed.

## 2026-08-14 - five bespoke named-weapon mechanics

- Added exact active-equipment Initiative handling for Wayfarer's Oath,
  confirmed-critical refresh-only Dodge AC for Falling Petal, and mutually
  exclusive `ItemEntityWeapon.HoldInTwoHands` armor/damage modes for Moonlit
  Crossing.
- Mountain-Sunder now consumes the exact installed Power Attack toggle's live
  `ActivatableAbility.IsRunning` state for one noncritical, nonrecursive 1d6
  force packet per round. Its Mighty Cleaving wrapper preserves native Cleave
  and Greater Cleave while allowing exactly one additional successful Cleave
  target only when Mountain-Sunder is the threat weapon.
- Unfixed Form adds exactly one native `RuleCalculateWeaponStats` size step
  when either current size differs from original size or polymorph is active.
  It does not alter unit size, reach, animation, model, or weapon identity.
- Seven append-only persistent identities were added: four buffs/equipped
  facts and three zero-cost custom enchantments. The exact runtime registry
  cardinality advanced from 1,504 to 1,511.
- The first guarded attempt failed closed because Harmony 1.2 rejects a direct
  Boolean-return patch of the compiler-generated Cleave iterator `MoveNext`;
  the implementation was narrowed to the public `AbilityCustomCleave.Deliver`
  enumerator boundary. The second attempt failed closed after proving the
  known Power Attack GUID identifies its feat rather than its toggle. Existing
  observer evidence then resolved and the production code validates the exact
  feat -> `AddFacts` -> toggle and `PowerAttackWatcher` -> same-toggle graph.
  The third attempt exposed the stale 1,504 cardinality guard. All three
  failures occurred before runtime request execution, rolled back owned
  identities, and accessed no save.
- Final all-ON save-free module run
  `20260814T1330417078100Z-42e5aa3303704f81ad756819bddfd880`
  passed from evidence directory
  `20260814T1330416854732Z-observe-feature-module-settings`: 1,511 identities,
  3 categories, 30 items, exact proficiency names, 21 parameterized entries,
  and Eastern presentation all matched. Settings restored exactly to SHA-256
  `2E53FA0A09C56662434F6EA548FF5EBCF91F5AAF293D668248221239A1308655`.
- Repository validation, all `1041/1041` tests, clean Release build,
  build-output validation, deterministic release packaging, and strict
  standalone package validation passed. Clean DLL SHA-256 is
  `0F09D2DDF86EE9177F36D5D6CC7F7FB6FC8F3051E058D1DACC6E1A6FDB2B59DE`,
  MVID `b74a7744-8581-4510-a38e-9e525461cd92`; release package SHA-256 is
  `25768C292EC69556856F9007C3E7473D1A6AEE56F161E7373CEFA8258A6D6643`.
  Comprehensive live positive/negative combat assertions remain a later
  qualification slice; this run proves registration and publication only.

## 2026-08-14 - campaign commerce and fixed-loot publication

- Added one data-driven, transactional campaign publisher for four required
  base-game merchants, the four optional Beneath the Stolen Lands weapon
  tables, and four main-path fixed-loot containers. Every feature-owned row is
  count one, append-only, idempotent, module-gated, and exactly rollback-owned.
- The enabled catalog contains 49 base merchant rows (42 generic and seven
  dependable named upgrades), 48 generic BTSL rows when all four installed
  tables resolve, and 11 named fixed-loot rows. All 18 named weapons have one
  exact campaign acquisition; ordinary BTSL tables contain no named item.
- Required GUID/name/area contracts fail closed. Optional DLC tables skip only
  when absent; an installed malformed table is an error. Rollback refuses a
  foreign post-publication mutation rather than removing another mod's work.
- Repository validation, all `1042/1042` tests, clean Release build,
  build-output validation, deterministic packaging, and strict standalone
  package validation passed. Clean DLL SHA-256 is
  `B1B3E41C38E84938BCD10C10A3D0AD7071428E682C12492787FC50B0DE9DC8F0`,
  MVID `b53a13e6-92fd-4a3b-8ea4-9d65ddd77280`; package SHA-256 is
  `B55452D9317E3C76918D99AFBCBD9B11F315C8B3B6DC60DB0ACEAC6D5BB2C2BB`.
- Enabled save-free run
  `20260814T1343180894067Z-9c6e5326e6fa4ee8a6f0761a7cd2af78`
  passed with 97 Eastern merchant rows across the base and installed BTSL
  tables, seven named merchant rows, and 11 fixed-loot rows.
- A request that changed only expected parameters correctly failed because the
  immutable installed settings snapshot remained ON; it was not accepted as
  an OFF result. The repository's settings transaction then staged a real
  schema-5 OFF snapshot. Fresh-process run
  `20260814T1349013224092Z-26cb873bd080433ebe1bd5f3658f3061`
  passed with zero Eastern commerce/loot rows and restored original settings
  byte-for-byte to SHA-256
  `2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.
  No save was accessed or written.

## 2026-08-14 - original models, icons, and fail-safe presentation

- Added a procedural Blender 4.5 generator and reproducible source set for
  original 0.76 m Wakizashi, 1.05 m Katana, and 1.58 m Nodachi meshes. The
  source has 39 mesh objects and 3,522 triangles, uses metric +Z-tip/grip-origin
  coordinates, and exports exactly three FBXs without cameras or lights.
- Added three family icons and distinct Night Without Moon, Heaven's Measure,
  and World-Tree Severer icons. All six production assets are exact 128x128
  transparent RGBA PNGs with distinct hashes.
- The exact Unity 2018.4.10f1 builder creates exactly three prefabs with
  `Visual`, `Grip`, `SupportHandTarget`, `Tip`, and `Butt`, opaque Standard
  materials, finite family-specific bounds, and one dedicated bundle. Two
  consecutive builds were byte-identical at SHA-256
  `39884FF681EE553DE957E36E01B350AB926A452F994C4E8D33015D57D4EAD1EC`.
- Added a transactional runtime loader and per-family model assignment. Any
  missing, corrupt, wrong-cardinality, incomplete, nonrenderable, or implausible
  candidate keeps the exact Kukri, Bastard Sword, or Falchion donor model while
  preserving all mechanics and identities. Module OFF also retains donors.
- Enabled save-free run
  `20260814T1420325300375Z-43bcb3114abe402b81663d0dfde65c13`
  observed three validated prefabs, six exact item icons, three successful live
  instantiations, and complete immediate cleanup. Disabled run
  `20260814T1423059037866Z-ff46339be9fd435893d8a4dd8c0b7694`
  observed no custom prefab/icon publication and restored settings exactly.
- Repository validation, all `1043/1043` tests, clean Release build,
  build-output validation, deterministic packaging, and strict package
  validation passed. DLL SHA-256 is
  `6C28CEF509A2D5C091886CB5D1EB0A7CAAA29338234E56D0BDB942E7ED847940`,
  MVID `354112cd-abc7-4d28-bc8d-8d71a710d2c1`; package SHA-256 is
  `72F26E72968A95279ED43AE0AC67E8B046010A6A8410AF7A8B4E35C15BED9F08`.
  The guarded exact-reference/runtime candidate loaded SHA-256
  `2E4D988AD7BDCB027E2720CE8EA71A0D0218AE40C568A4C8F2C3FB46AB12C5A5`,
  MVID `e91edbb1-2b73-4826-9fa2-63202cf44735` in both final asset runs.
  No save was accessed. Subjective rig/animation/clipping review is accurately
  left pending for a human.

## 2026-08-14 - focused save-free combat qualification

- Added the guarded save-free `disposable-eastern-weapons-combat` scenario.
  It creates request-local live units and real item entities, equips exact
  catalog weapons, uses native attack, critical-confirmation, weapon-stat,
  equipment-fact, activatable, damage, and size-rule events, and restores the
  global-unit snapshot during cleanup.
- Live runtime identified that `AddFactToEquipmentWielder` requires a
  `BlueprintFeature`, not a `BlueprintBuff`, for the persistent Wayfarer's Oath
  and Moonlit Crossing equipment facts. Two append-only feature identities were
  added while the original support identities remain registered and stable.
- The passing standalone run is
  `20260814T1513175535242Z-6d95393a15c44b96a168cb21132fee19`
  in evidence directory
  `20260814T1513175368815Z-disposable-eastern-weapons-combat`. It proves the
  corrected `Weapon Proficiency (...)` labels; Wakizashi, grip-dependent
  Katana, and Nodachi penalties; Wakizashi finesse; all five bespoke effects;
  capstone effective-bonus/Speed identities; exact cleanup; and loaded version
  `0.0.80`.
- The accepted Elven Branched Spear combat regression also passed as run
  `20260814T1515370965718Z-017e199cb77b4af891868cec2d3a840b`.
  Both compatibility transactions restored the Mods tree exactly. Neither
  scenario accessed a save.
- Repository validation, all `1044/1044` tests, the clean exact-reference
  Release build, build-output validation, and strict package validation passed.
  The built, installed, and runtime-loaded DLL is SHA-256
  `1EBA61259CB47F5D17F1AEB01026D26C1AD494A19D7ED05679478242C0A8EC00`,
  MVID `e80d25d8-4215-4fd9-b4fb-db7fd2136637`; package SHA-256 is
  `E74679549583863BBFC4C33439A51C9637F0BD8BF64DD8EC4CC16B4A99FBA79F`.
  Expanded fighter-group, selector, named-property, capstone attack-count, and
  transformation controls remain explicit pending work rather than claimed.

## 2026-08-14 - exact vendor and fixed-loot observer

- Extended the existing read-only `observe-vendor-table-contracts` scenario to
  validate every Eastern vendor specification by exact table GUID and name,
  every desired item reference and count, the absence of any extra KMG Eastern
  row, every fixed-loot GUID/name/area, and the singular eighteen-item named
  placement set.
- Enabled run `20260814T1531432806171Z-d6638ced8af7472fabeb9b65f2c233c7`
  passed with eight installed Eastern vendor tables, 97 exact rows total: 49
  base-campaign merchant rows and 48 generic BTSL rows. All four BTSL tables
  retained 48 firearm rows and 24 spear rows, contained zero named Eastern
  rows, and each Eastern row had count one. Four exact fixed-loot targets held
  eleven named rows; the seven merchant rows plus eleven loot rows covered all
  eighteen named weapons exactly once.
- A transaction with only Eastern Weapons disabled passed as run
  `20260814T1534024888613Z-29a2ce31f1db4aa7bedec9c2c14e6047`.
  It observed zero Eastern vendor, BTSL, and fixed-loot rows while the firearm
  and spear rows remained exact. The transaction restored the original feature
  settings and complete Mods tree exactly.
- Repository validation, all `1044/1044` tests, clean exact-reference Release
  build, build-output validation, and strict package validation passed. The
  built/installed/runtime DLL was SHA-256
  `EC4AF090D0D262A8B7405B382A7F6508B24EAABBEF131CF6057DB2D5ADA19A02`,
  MVID `ecf0128e-eab6-4fc5-bd65-650d786edf76`; package SHA-256 was
  `E2F8576222D4351D0D37C79A5287FABFC035365A343E726D124BD352909DE097`.
  Neither observer accessed a save.

## 2026-08-14 - development controls and working-save persistence

- Added development-only controls to audit the complete catalog, add all 30
  exact items, add one complete six-item family path, or add one exact catalog
  item by index. Controls report exact inventory deltas, warn about disposable
  saves, grant no facts or campaign state, and never call a save API.
- Added guarded prepare, module-disabled verify/cleanup, and final absence
  scenarios plus a transactional orchestration script restricted to
  `KMG_AUTOMATION_WORKING`. The script stages and restores feature settings
  exactly and waits for every Steam-launched process to exit before advancing.
- Prepare run `20260814T1546412271086Z-ec58de23ddad45c4b9c57c2594065f28`
  started from zero fixtures, added exactly 30 item instances and one exact
  Wakizashi/Katana proficiency fact on deterministic owners, proved their
  blueprint/type/category identity, and made exactly one correlated save write.
- Eastern-disabled cleanup run
  `20260814T1551228284204Z-e4d26f6cabb740c2a9dbbacb4785055e`
  deserialized all 30 items and both facts exactly once, observed zero Eastern
  public publication, removed only the exact fixtures, and made exactly one
  correlated save write.
- Absence run `20260814T1554167770731Z-9695a053916645e3b3cc4a44ceae8c29`
  observed zero exact items and facts and made no save write. The protected
  baseline stayed distinct and untouched. Settings restored byte-for-byte.
- No stable ordinary selected-weapon fact was added: these installed selectors
  generate parameterized feature instances dynamically, so persistence would
  require an unsafe invented identity. This is the mission's expressly allowed
  safe-construction exception.
- Repository validation, all `1046/1046` tests, clean exact-reference Release
  build, build-output validation, and strict package validation passed. DLL
  SHA-256 is
  `183DC903A64959C76A1A5063665E496A2E3FF2DF2BB575C99DA90092A3DEF8B4`,
  MVID `34061378-e413-4359-a96d-7952301e0efd`; package SHA-256 is
  `7D89041D27EBCE75BD397769D2681D2F9C9513D3BC409213B4DC3BC648FA5D70`.

## 2026-08-14 - Call of the Wild and Arms and Armor compatibility

- Call of the Wild transaction `compat-20260814T160106Z-f8e933764054`
  passed the exact optional-mod observer, all-on six-module publication, and
  live Eastern combat as runs
  `20260814T1602067440876Z-9806d27e2797451a865b3228ed0cb128`,
  `20260814T1604326146179Z-767318879aa145ea8a207afc56813dc5`, and
  `20260814T1606556714227Z-d43a373f30294a268e8cb1d396942061`.
  The Mods tree and settings were restored exactly.
- The first exact Arms and Armor combat run failed specifically because that
  mod's hard-coded versatile classifier forced KMG Katana one-handed. A first
  classification-only repair exposed its secondary hand-slot policy; neither
  failed request touched a save and both transactions restored exactly.
- Added a reflection-only, fail-closed bridge over the exact Arms and Armor
  classification and grip signatures. It recognizes only the exact registered
  KMG Katana type and derives grip from the active primary/offhand slots; it has
  no optional-mod compile dependency and mutates no foreign blueprint.
- Repair run `20260814T1626264154920Z-659ee31c63844b15a53f60366ffd55d6`
  passed correct two-/one-handed state, martial/exotic proficiency, Moonlit
  Crossing exclusivity, all other Eastern mechanics, and cleanup. Transaction
  `compat-20260814T162536Z-edb4ba5e8032` restored the exact Mods tree.
- The authorized Arms and Armor source/runtime contains Temple Sword and Orc
  Hornbow, not Katana/Wakizashi/Nodachi, so there is no overlapping eastern
  identity or safe proficiency bridge to add.
- Repository validation, all `1047/1047` tests, clean Release build,
  build-output/package validation passed. DLL SHA-256 is
  `35182EE446A9CA148DBDABEF6015FD602AAD4BC0787A8F6D0408BB80A4771423`,
  MVID `8b26ae08-23c7-4a56-a65b-44c3ee5f37ef`; package SHA-256 is
  `DCA9BE195D54318F19DBDDA195F9E5E435526E5F2AFA7AEBC57CBC9A07661EF4`.

## 2026-08-14 - negative-control and maximum combined compatibility

- Toggle Custom Soundpacks transaction
  `compat-20260814T163039Z-4e78b724108d` passed exact optional-mod identity and
  live Eastern combat as runs
  `20260814T1631281716106Z-f2c9d4b1204e4cfda6676e1195b682b9` and
  `20260814T1633141740745Z-18723a67c397410783a5ffc276b2b280`.
- The maximum currently qualified combined profile staged Arms and Armor plus
  Toggle Custom Soundpacks. Transaction
  `compat-20260814T163437Z-2f088fd5a184` passed exact identity and all-on
  six-module publication as runs
  `20260814T1635283183758Z-21349748fc914622a1aa4b6d6d2075c2` and
  `20260814T1636409575327Z-104bf7b4dfcd4bc38f1f6a924103fe6f`.
- Its first combat launch accepted the guarded request but ended before the
  runner entered `OnUpdate` after Unity reported a transient failure to load
  the present Steam API DLL. It produced no runtime result and touched no save.
  The isolated retry passed as
  `20260814T1641090730695Z-2f410c5cf34e4afda160f266fd9024a4` under transaction
  `compat-20260814T164018Z-24ec1f9abdbb`.
- Every transaction restored the complete Mods tree exactly. Candidate commit
  `3902fbd32030950c718c32174820bb0bcaba1112` loaded with DLL SHA-256
  `14AF254B51979245B8046C57F770700E2ED7BED4DDBADFDB869D518CCDE7A59F`
  and MVID `60409a50-c431-4150-ba55-da63727381bc`; its deterministic package
  SHA-256 is
  `F469C4D1C7289F2A86C2F1D70AEB6BD39CA99005098DA8FD8D1C52DB944A6445`.

## 2026-08-14 - expanded live combat qualification

- Expanded the save-free combat fixture to enumerate every approved generic
  selector for all three categories, merged proficiency ordering and excluded
  Grace selectors, all eighteen exact named enchantment arrays, native fighter
  training through each approved group, all ten Wakizashi finesse variants,
  complete Speed attack planning, and native Brilliant Energy exclusions.
- Added negative controls for a Falling Petal miss and threatened but
  unconfirmed critical; Mountain-Sunder miss, inactive toggle, repeat,
  same-round weapon switching, and critical nonmultiplication; and Unfixed Form
  ordinary, changed-size, polymorph-only, and simultaneous engine states.
- Repository validation, all `1047/1047` tests, clean exact-reference Release
  build, build-output validation, and strict package validation passed.
- Guarded standalone run
  `20260814T1715447972862Z-9c6f32cdb16c41df9de32c4108c9f79c`
  passed in evidence directory
  `20260814T1715447806219Z-disposable-eastern-weapons-combat`. The loaded DLL
  SHA-256 was
  `6EBC8BF7B8339A8A65A23354D2EBA467E804210F64464502E579C6769492BBE7`,
  MVID `e6465005-9428-48ab-b646-ceffb7ed8a2b`; package SHA-256 was
  `98EE14CC220722AC3DED6DF1BCF5E6DF1A60F072F064E4F5E504EE6DCE6BA78A`.
  Transaction `compat-20260814T171454Z-0dc865f34ad3` restored the Mods tree
  exactly. The scenario was save-free.
