# DATA mid-game firearms and protection copy

Status: implementation, focused runtime, and all 24 module boundary states PASS. Installable candidate retained; DATA installation restored.

## Baseline and safety

- Host: DATA. Start: clean master at dfd551080a1aad38cdd0b19714fbcb12c81ca4ca, version 0.0.115. A fetch confirmed origin/master at the same commit.
- Task branch: codex/midgame-firearms-and-protection-copy, created in the historical checkout. Candidate baseline is published 0.0.115; no unmerged elemental work imported.
- Other worktree and branches untouched. No Kingmaker process was running at intake.
- DATA setup: C:/Dev/KingmakerDevSetup/reports/SETUP-REPORT.md; installed UMM 0.33.0, Harmony12 1.2.0.1, isolated Python 3.12.10, .NET 4.7 reference surface and existing exact-reference build route.
- Sandbox process setup and patch helper failed before execution. Approved elevated commands provide local access; no sandbox configuration changed.
- The documented Saved Games root was absent at intake. Steam subsequently populated it with the named working and protected baseline saves. The canonical working-save smoke passed on the candidate; no alternative save/root or fallback loader was used.

## Progression audit before implementation

Costs below are blueprint base values, not displayed merchant buying prices.
Optional Craft Magic Items upgrades and diagnostic spawning are excluded from ordinary campaign progression.

| Display name | Family | Actual enhancement | Properties | Base gp | Ordinary acquisition / approximate availability |
|---|---|---:|---|---:|---|
| Pistol | Pistol | 0 | None | 1,000 | Capital blacksmith after founding the barony; Honest Guy in campaign/standalone Tenebrous Depths; ordinary crafting base |
| Musket | Musket | 0 | None | 1,500 | Same |
| Blunderbuss | Blunderbuss | 0 | Scatter family | 2,000 | Same |
| Pistol +1 | Pistol | +1 | None | 3,300 | Capital blacksmith and Honest Guy; early magical stock |
| Musket +1 | Musket | +1 | None | 3,800 | Same |
| Blunderbuss +1 | Blunderbuss | +1 | Scatter family | 4,300 | Same |
| Duelist's Rebuttal | Pistol | +2 | Reliable | 19,300 | VarnholdStockade: Forest_Container_7_good; Varnhold campaign arc |
| The River King's Measure | Musket | +4 | Reliable | 51,800 | IrovettiPalace: PoorHuman_IrovettiChambers_ChestHuge_Outline (3); Pitax arc |
| Irovetti's Ovation | Blunderbuss | +4 | Reliable | 52,300 | IrovettiPalace: RichHuman_ConservatoryLoot; Pitax arc |
| The Last Word | Pistol | +5 | Reliable, Seeking | 99,300 | FinalDungeon3: RichHuman_Loot_2_3lvl; finale |
| Watch at the World's End | Musket | +5 | Reliable, Fey Bane | 99,800 | HouseAtTheEdgeOfTime: FirstWorld_GoodLoot02; late campaign |

Hidden legacy identities: Advanced Rifle (5,000 gp) and Advanced Revolver (4,000 gp), both unenchanted, remain recognition-only and unavailable through ordinary campaign merchants/crafting bases. Test Musket is diagnostic equipment, not progression.

Implemented additions: Roadwarden, +3 Reliable musket (equivalent +4, 33,800 gp); Dead Reckoning, +3 Seeking pistol (equivalent +4, 33,300 gp). Pricing follows family base + 300 gp masterwork + 2,000 × equivalent bonus squared. Existing +2 Reliable pistol and +4 Reliable musket use the same model.

## Final equipment and acquisition

The eight magical firearms become ten. The five fixed-loot rewards retain their exact identities and locations. The three generic magical firearms, three mundane crafting bases, and unrelated vendor stock remain unchanged. All seven named magical firearms retain Craft Magic Items upgrade-only treatment.

| New item | Stable project GUID | Actual / price-equivalent bonus | Base value | Observed buy / sell gp |
|---|---|---|---:|---:|
| Roadwarden | 66d2f8c4d6aa43e0be72ac18ed9fcd81 | +3 / +4; Reliable musket | 33,800 | 33,800 / 8,450 |
| Dead Reckoning | b8db89aba5364c27b1626896664a1913 | +3 / +4; Seeking pistol | 33,300 | 33,300 / 8,325 |

Symbols: KMG.Firearms.RoadwardenItem and KMG.Firearms.DeadReckoningItem. No native English display-name collision was found. Native Enhancement3 is BlueprintWeaponEnchantment 80bb8a737579e35498177e1e3c75899b, with one non-stacking WeaponEnhancementBonus of 3 and enchantment cost 3. The builder verifies this identity/type/name/component before use. The installed references and serialized blueprint agree. Both designs reuse the approved Service family presentation/fallback and canonical family types: musket 1d12, x4, two-handed, 40 ft; pistol 1d8, x4, one-handed, 20 ft. No family mechanics or shared donor arrays changed.

Exact campaign merchant: BlueprintUnit RE_Trader, b8b362de19b0a8340ad050586f1162d1, localized Skeletal Salesman; dialogue 0dd69ac03c55bc14292a9f9c887885ff. Each table has exactly one fixed copy of each new item:

| BlueprintSharedVendorTable | GUID | Native availability |
|---|---|---|
| C3_VendorTableLarge | b3bc1bb9f4a59f3438edc505e0f3b407 | Kingdom day 491–690 |
| C3_VendorTableSmall | 9126c670f0743b647b4e9ba850214d8d | Kingdom day 491–690 |
| C4_VendorTableLarge | fc01b45fee3606749a21d9612c5629a6 | Kingdom day 691 onward |
| C4_VendorTableSmall | 4b1bb03a5d19a534bad2aa5cd766af92 | Kingdom day 691 onward |

The native Hills, Plains, and Forest trader encounters each choose between their two stock variants. Reverse-reference inspection found only ItemAddVendorTable actions for this exact RE_Trader unit consuming these tables. Encounter identities: Hills a67c2a6ee216da54988c4f01b09442fe; Plains 67472e421e50dd443badcbca27c55247; Forest 54c762e193e456b409fed48d98aa7668. C2 stock is untouched. Availability follows native encounter generation; this patch does not force encounters or refresh stock when trade opens.

Prices use family base + 300 gp masterwork + 2,000 × 4². Native stock includes comparable equivalent +4 weapons at 32,000 gp; the firearm base premium accounts for the difference. The documented request-local RE_Trader fixtures use native PriceModifier=1 and sale multiplier=0.25. All four variants returned the same buying prices, and the normal VendorUI tooltip/deal debited exactly the displayed amount. No extra markup or economy override was added.

Fresh stock generates one copy of each. Already generated native own-inventory stock receives the additions on its next native UnitPartVendor.PostLoad, using persistent m_OwnKnownItems. Runtime fixtures serialized stock generated against an isolated pre-patch table, resolved its native table reference on load, and retained every prior item count plus buyback stock while adding exactly two items. Purchase, reopen, and another native serialization/PostLoad did not restore purchased copies. There is no custom merchant lifecycle hook or save migration. An already open, generated inventory must pass through normal load before reconciliation; very old stock with null KnownItems follows native behavior and waits for fresh stock generation rather than an invasive repair.

## Normal shop ordering

Publication uses CreateIntegrated with scoped native item type and localized display name. It preserves every unrelated source entry's relative order. The actual desktop VendorUI independently defaults to TypeUp (item type, then localized name). The following immediate neighbors were observed in the real Store.VirtualSlots after opening the shop; both all-items and weapons-filter views passed. The deliberately selected descending-price sort survived repeat publication.

| Stock | Dead Reckoning neighbors | Roadwarden neighbors |
|---|---|---|
| C3 Large | Corrosive Tongi +2 → Dead Reckoning → Flaming Earth Breaker +2 | Radiant Dueling Sword +2 → Roadwarden → Shock Sling Staff +2 |
| C3 Small | Corrosive Vermin Bane Dwarven Waraxe +2 → Dead Reckoning → Flaming Kama +2 | Lycanthrope Bane Heavy Crossbow +2 → Roadwarden → Shock Elven Curve Blade +2 |
| C4 Large | Corrosive Thundering Gnome Hooked Hammer +2 → Dead Reckoning → Frost Holy Two-bladed Sword +2 | Necrotic Vicious Scythe +2 → Roadwarden → Shock Necrotic Dwarven Urgrosh +2 |
| C4 Small | Corrosive Flaming Battleaxe +2 → Dead Reckoning → Mithral Speed Scimitar +2 | Plant Bane Flaming Sickle +2 → Roadwarden → Shock Thundering Trident +2 |

Dead Reckoning naturally appears early alphabetically (weapon indices 1/1/5/2); Roadwarden indices are 9/4/14/5. No pinned rows, promotional grouping, prefixes, global resort, or player-sort override exists. Structured evidence includes five neighboring rows with identities and prices for each item and view, beyond source-array indices.

## Final player-facing weapon text

Roadwarden rules: Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses. Penetration: Touch AC within the first range increment (40 ft. base); Normal AC beyond.

Roadwarden flavor: The stock bears the mile marks of a road that no longer appears on any map. Its last keeper never missed a watch.

Dead Reckoning rules: Seeking ignores concealment miss chances. It does not reveal unseen creatures, allow targeting a creature you could not otherwise target, or bypass other defenses. Penetration: Touch AC within the first range increment (20 ft. base); Normal AC beyond.

Dead Reckoning flavor: Its maker promised that no debtor could lose themselves in the mist. The promise outlived them both.

## Protection wording and behavior

Only ProtectionFromAlignmentDescriptions.cs changed inside the Protection subsystem. Publication, immunity policy, registry, durations, targeting, identities, stacking, levels, and independent setting remain unchanged; repository validation hashes the other subsystem sources against the starting baseline. Existing immunity regression tests pass. No fear, confusion, sleep, possession, summoned-creature, or blanket mind-affecting immunity was added.

Enabled publication owns 15 exact description targets and five terminal-buff immunity components. Disabled publication retains native descriptions and adds no components. Twelve installed usable-item tooltip surfaces inherit the exact ability text through native UIUtilityItem.FillTooltipData; separate scroll or global localization changes are unnecessary.

These are the final outputs exported from the compiled description formatter. The paladin evil-protection effect uses the identical evil-buff text.

**Protection from Evil**

Protection from Evil wards the target against evil creatures. The target gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by evil creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place the target under the control of an evil creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Evil, Communal**

Protection from Evil, Communal wards allies against evil creatures. Each affected ally gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by evil creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place an affected ally under the control of an evil creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Evil buff**

This creature is warded against evil creatures. It gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by evil creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place this creature under the control of an evil creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Good**

Protection from Good wards the target against good creatures. The target gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by good creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place the target under the control of a good creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Good, Communal**

Protection from Good, Communal wards allies against good creatures. Each affected ally gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by good creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place an affected ally under the control of a good creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Good buff**

This creature is warded against good creatures. It gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by good creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place this creature under the control of a good creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Law**

Protection from Law wards the target against lawful creatures. The target gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by lawful creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place the target under the control of a lawful creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Law, Communal**

Protection from Law, Communal wards allies against lawful creatures. Each affected ally gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by lawful creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place an affected ally under the control of a lawful creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Law buff**

This creature is warded against lawful creatures. It gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by lawful creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place this creature under the control of a lawful creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Chaos**

Protection from Chaos wards the target against chaotic creatures. The target gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by chaotic creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place the target under the control of a chaotic creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Chaos, Communal**

Protection from Chaos, Communal wards allies against chaotic creatures. Each affected ally gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by chaotic creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place an affected ally under the control of a chaotic creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Chaos buff**

This creature is warded against chaotic creatures. It gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by chaotic creatures. While this protection lasts, it prevents new charm, domination, and similar effects that would place this creature under the control of a chaotic creature. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Alignment selector**

Protection from Alignment lets the caster choose evil, good, law, or chaos. The target gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by creatures of the selected alignment. While this protection lasts, it prevents new charm, domination, and similar effects that would place the target under the control of a creature of the selected alignment. It does not remove or suppress a control effect that was already active when the protection was applied.

**Protection from Alignment selector (communal)**

Protection from Alignment, Communal lets the caster choose evil, good, law, or chaos. Each affected ally gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by creatures of the selected alignment. While this protection lasts, it prevents new charm, domination, and similar effects that would place an affected ally under the control of a creature of the selected alignment. It does not remove or suppress a control effect that was already active when the protection was applied.

## Qualification, candidate, and limitations

The package was built and frozen before committing, following the commit-after-checks rule. All final runtime results below belong to the exact pre-commit artifact. Its Git metadata records the starting commit plus the attested task source state; it is not described as a build from the later checkpoint commit. This final report adds documentation after qualification. The tested ZIP is retained unchanged.

Implementation checkpoint: 1f6e00012d37aa754e009cea81c39fcd7f41eef7 (Add mid-game Salesman firearms and clarify Protection descriptions). The approved DATA checkpoint helper pushed and verified this commit on codex/midgame-firearms-and-protection-copy. This final documentation addendum records that completed commit; it changes no implementation or package input.

- Candidate: 0.0.116 / 0.0.116-midgame-firearms-and-protection.
- Installable ZIP: C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/artifacts/local-runtime/0.0.116/KingmakerGunslinger-0.0.116-local-runtime.zip.
- ZIP size: 23,063,010 bytes; validated standalone UMM layout, 135 files.
- ZIP SHA-256: aa1e5bd56e48b95124b74b937f9fca0851b69eba53588baba7659bc0d8351d48.
- DLL SHA-256: af5ceed3c2b8d04492bb796bacb50fdb5df95dbf0f0cc2a2b18f8c508a217549.
- DLL MVID: e79f9c12-4c8b-46a0-a06f-8508368b3067.
- Build Git commit: dfd551080a1aad38cdd0b19714fbcb12c81ca4ca; task branch codex/midgame-firearms-and-protection-copy.
- Frozen source-state SHA-256: 9e84458849d769e3dfadcc72bca4128b0e64bbd115037a3fd3efd91fe434ecb4.
- Non-report repository inputs: 2,641 files, Git-managed line endings normalized; aggregate SHA-256 57bcdb97f12745d0dd6a2e0d2be218cafe732010e81e52a64304998b0901bf4e. All match the qualified snapshot. Only this report changes after qualification.

Exact build command, from the repository root with the documented DATA Python/.NET paths on PATH:

    .\scripts\Build-Local.ps1 -ReferenceBundleDir C:/Dev/KingmakerGunslingerLab/private/extracted-references/KingmakerGunslinger-private-build-references

PASS: repository validation; complete clean Release domain suite (1,398 tests, zero failures, including all 2,048 module configurations); exact-reference clean Release compilation; supply icons; build-output checks; SoundBank verification; strict standalone package validation. Evidence: artifacts/midgame/working-build-2.log and the ZIP's .build-local.json manifest. No proprietary game/Unity assemblies are packaged.

Deployment used scripts/Deploy-Local.ps1 with this ZIP and -Confirm:$false. Deployment manifest: C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260907T2002305431617Z/deployment.json. Explicit backup: C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260907T2002292432945Z.

Every runtime launch used Windows PowerShell 5.1, the guarded request mechanism, and Steam App ID 640820. The exact shared parameters and scenario invocations were:

    $common = @{
        ExpectedVersion = '0.0.116'
        ExitAfterCompletion = $true
        AllowDirtyGit = $true
        ReuseInstalledArtifact = $true
        DeploymentManifestPath = 'C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260907T2002305431617Z/deployment.json'
        PackagePath = 'artifacts/local-runtime/0.0.116/KingmakerGunslinger-0.0.116-local-runtime.zip'
        Confirm = $false
    }
    .\scripts\Invoke-KingmakerRuntimeTest.ps1 @common -Scenario disposable-midgame-firearms -TimeoutSeconds 180

Each subsequent invocation ran only after the preceding owned process exited. Working scenarios additionally supplied -SaveName KMG_AUTOMATION_WORKING:

    .\scripts\Invoke-KingmakerRuntimeTest.ps1 @common -Scenario working-save-midgame-prepare -SaveName KMG_AUTOMATION_WORKING -TimeoutSeconds 300
    .\scripts\Invoke-KingmakerRuntimeTest.ps1 @common -Scenario working-save-midgame-verify-cleanup -SaveName KMG_AUTOMATION_WORKING -TimeoutSeconds 300
    .\scripts\Invoke-KingmakerRuntimeTest.ps1 @common -Scenario working-save-midgame-verify-absent -SaveName KMG_AUTOMATION_WORKING -TimeoutSeconds 300
    .\scripts\Invoke-KingmakerRuntimeTest.ps1 @common -Scenario observe-rare-firearm-acquisition -TimeoutSeconds 300
    .\scripts\Invoke-KingmakerRuntimeTest.ps1 @common -Scenario observe-rare-firearm-blueprint-contracts -TimeoutSeconds 300
    .\scripts\Invoke-KingmakerRuntimeTest.ps1 @common -Scenario observe-craft-magic-items-compatibility -TimeoutSeconds 300
    .\scripts\Invoke-FeatureModuleRuntimeMatrix.ps1 @common -Boundary

The matrix default timeout is 300 seconds. All three working scenarios used fresh launches and the canonical correlated load path. Prepare and cleanup each armed exactly one native write to the working descriptor; absence observed no writes. No raw archive editing, fallback loader, baseline load, or human campaign load occurred.

| Focused scenario | Assertions | PASS directory under C:/Dev/KingmakerGunslingerLab/runtime-evidence |
|---|---:|---|
| disposable-midgame-firearms | 91 | 20260907T2003168370273Z-disposable-midgame-firearms |
| working-save-midgame-prepare | 28 | 20260907T2005067901229Z-working-save-midgame-prepare |
| working-save-midgame-verify-cleanup | 5 | 20260907T2006560340071Z-working-save-midgame-verify-cleanup |
| working-save-midgame-verify-absent | 3 | 20260907T2009174829632Z-working-save-midgame-verify-absent |
| observe-rare-firearm-acquisition | 25 | 20260907T2011041809204Z-observe-rare-firearm-acquisition |
| observe-rare-firearm-blueprint-contracts | 14 | 20260907T2012327669954Z-observe-rare-firearm-blueprint-contracts |
| observe-craft-magic-items-compatibility | 27 | 20260907T2013463954693Z-observe-craft-magic-items-compatibility |

Each directory contains runtime-result.json, loaded-build identity, request, summary, and evidence manifests. Weapon checks observed +3 attack and damage, canonical family types, equipping/firing/one-round discharge, ordinary powder-and-ball reload, exact Reliable/Seeking isolation, natural-one misses/misfires, Roadwarden's reduced misfire range, and Seeking bypass of a controlled concealment miss. No revelation or targeting permission is granted by the unchanged Seeking implementation.

Normal shop purchases reloaded with their exact blueprint identities, native salesman provenance, two static enchantments, and distinct ammunition/condition states. Exact task items were then removed and their absence verified on a further fresh launch. Fixture funding was restored before saving.

Installed Craft Magic Items 2.1.0 passed 27 native checks, including zero named identities in creation bases. Call of the Wild 1.14.4c-2.1 remained installed. No concrete change justified additional external-mod combinations. Twelve source-only request checks rejected missing, protected-baseline, and incorrectly cased names for all three new working scenarios; artifacts/midgame/working-request-checks.log records PASS with no launch, request-file write, or save access.

| Boundary state | Assertions | PASS directory under the same evidence root |
|---|---:|---|
| All ON | 26 | 20260907T2015527263270Z-observe-feature-module-settings |
| All except elementalRaces | 26 | 20260907T2017066363079Z-observe-feature-module-settings |
| All except protectionFromAlignmentControlImmunity | 26 | 20260907T2018200382241Z-observe-feature-module-settings |
| All except bodyguardFeats | 26 | 20260907T2019335193374Z-observe-feature-module-settings |
| All except urbanBarbarian | 26 | 20260907T2020480271111Z-observe-feature-module-settings |
| All except brownFurTransmuter | 26 | 20260907T2022020132669Z-observe-feature-module-settings |
| All except easternWeapons | 26 | 20260907T2023158593229Z-observe-feature-module-settings |
| All except elvenBranchedSpears | 26 | 20260907T2024299011666Z-observe-feature-module-settings |
| All except expandedSummoning | 26 | 20260907T2025441650623Z-observe-feature-module-settings |
| All except shieldOther | 26 | 20260907T2026581767997Z-observe-feature-module-settings |
| All except acadamaeGraduate | 26 | 20260907T2028126810430Z-observe-feature-module-settings |
| Only gunslinger | 26 | 20260907T2029270062860Z-observe-feature-module-settings |
| All except gunslinger | 25 | 20260907T2030411685453Z-observe-feature-module-settings |
| Only acadamaeGraduate | 25 | 20260907T2031554147516Z-observe-feature-module-settings |
| Only shieldOther | 25 | 20260907T2033090050034Z-observe-feature-module-settings |
| Only expandedSummoning | 25 | 20260907T2034227368701Z-observe-feature-module-settings |
| Only elvenBranchedSpears | 25 | 20260907T2035361042287Z-observe-feature-module-settings |
| Only easternWeapons | 25 | 20260907T2036498333638Z-observe-feature-module-settings |
| Only brownFurTransmuter | 25 | 20260907T2038033563737Z-observe-feature-module-settings |
| Only urbanBarbarian | 25 | 20260907T2039165811198Z-observe-feature-module-settings |
| Only bodyguardFeats | 25 | 20260907T2040297727815Z-observe-feature-module-settings |
| Only protectionFromAlignmentControlImmunity | 25 | 20260907T2041429308503Z-observe-feature-module-settings |
| Only elementalRaces | 25 | 20260907T2042568440447Z-observe-feature-module-settings |
| All OFF | 25 | 20260907T2044107837156Z-observe-feature-module-settings |


All 24 distinct required states passed with the exact candidate DLL/hash/MVID. Transcript: artifacts/midgame/runtime-boundary.log. The matrix restored the original feature-settings bytes (SHA-256 a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17).

Development retries are retained separately and are not PASS evidence. The PowerShell 7.6.5 launch wrapper produced a List-wrapper mismatch (20260907T1926473633086Z); Windows PowerShell 5.1 resolved it without changing guards. The first request-local fixture omitted native CreateInventory (20260907T1928178751220Z); the next duplicated JSON converters through JsonConvert defaults (20260907T1939188807261Z). The corrected fixture calls CreateInventory and creates the native configured serializer directly. A sequential regression wrapper initially reached the no-running-game guard before the prior owned process finished exiting; the retry waited for each normal exit. No production mechanic or required check was disabled.

The canonical working-save-smoke passed earlier at 20260907T1941499572410Z-working-save-smoke on the preceding development artifact recorded there. It is a loader checkpoint, not a substitute for the final candidate's three fresh save-backed runs.

Final restore command:

    .\scripts\Restore-Live-Mod.ps1 -BackupDirectory C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260907T2002292432945Z -Confirm:$false

PASS: exact original live file set and all 135 hashes restored, including initial absence of FeatureModules.json; no game remains active. Evidence: artifacts/midgame/restore-3.log, restoration-verification.txt, live-baseline-files.json, and qualified-input-verification.txt. Protected KMG_AUTOMATION_BASELINE SHA-256 remains CC7CBB0D08581873ED0AD2A6AC8EBD16A95333B5665CD74DCD0C538E16119C07. KMG_AUTOMATION_WORKING was intentionally saved for purchase verification and cleanup; it is not claimed byte-identical.

No unresolved acceptance blocker remains. Native reconciliation timing and null historical KnownItems behavior are the availability limitations described above; immediate reconciliation in an already-open shop and repair of legacy null tracking are not claimed. No branch merge, public release, other-machine deployment, or elemental-work import occurred.


Integration hotspots are the append-only blueprint ledger, bootstrap publication/rollback and expected count, magic catalog counts, shared registration/project includes, presentation mapping counts, current-version validators, and runtime scenario dispatch/allowlists. The only edits in ElementalRaceProductionTests and BodyguardBlueprintContractTests update the shared aggregate ledger count for the two additions. No elemental-race implementation, assets, worktree, or task report changed. Version 0.0.116 is this task's candidate identity; public integration must coordinate any concurrent version allocation without importing unmerged work.
