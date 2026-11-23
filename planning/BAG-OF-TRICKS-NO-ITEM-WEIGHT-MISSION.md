# Claude mission for Bag of Tricks zero weight compatibility

- Owner: Howie Reith
- Date: November 22, 2026
- Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules
- Target: current origin/master, preserving the released 0.0.193 repair
- Suggested branch: codex/bag-of-tricks-no-item-weight
- Mission path in the implementation worktree: planning/BAG-OF-TRICKS-NO-ITEM-WEIGHT-MISSION.md

## 1. Objective and completion boundary

Make Mr. Bard's Tabletop Rules Expansion (TRE) compatible with Bag of Tricks' No Item Weight setting. With that setting enabled, every TRE inventory item must have zero effective weight, including items already in saves and items created afterward. Disabling the setting must restore each item's intended weight. Enabling, disabling, loading, or inspecting this optional setting must never make TRE roll back its blueprint registration, enter a failed initialization state, disappear from the game, or lose unrelated features such as Expanded Summoning.

This is a complete compatibility repair. Fix every affected construction and validation path in one coherent change, rather than advancing startup from one weight assertion to the next.

The no-failure requirement applies to this weight setting and TRE's optional compatibility handling in every supported lifecycle state. It does not authorize reporting successful initialization after an unrelated genuine defect, such as a missing required game assembly, invalid blueprint identity, or incorrectly authored combat mechanics. Preserve truthful failures for those defects. A deliberate global runtime weight override is valid behavior and must not be classified as corrupted TRE data.

Deliver implemented code, focused tests, native evidence, and a reviewable draft PR through the existing Claude writer / Codex reviewer workflow. Final handoff remains for Howie's review. This mission does not authorize merging, tagging, publishing a release, changing the public version, or installing the candidate into Howie's normal-play environment.

## 2. Established evidence

The reporter's follow-up says:

> On 0.0.193, making another sorcerer character, weapon tooltips show but summon monster I still has no sub-abilities.

The supplied Log.txt identifies Kingmaker 2.1.7b, UMM 0.31.1.0, Bag of Tricks 1.16.4, Call of the Wild 1.14.4c-2.1, and TRE 0.0.193-spear-bootstrap-tooltip-repair. Its root-cause entry is:

```
Elven Branched Spear type profile is invalid: 1 of 18 checks failed.
weight [effective-getter]: expected=10 actual=10 donor=9;
getter expected=10 actual=0 donor=0; getterPatches=BagOfTricks
```

The stored spear weight is correct at 10. Both the spear's and the native Longspear donor's effective getters return zero. TRE rejects the getter difference, rolls back 296 custom blueprint registrations and 28 visual resource proxies, and reports that blueprint initialization will not be retried within that process. Summoning integrations then report `Blocked:package-blueprint-lifecycle-failed`.

The improved tooltip fallback works: the log records that weapon price and tooltip inspection keep native values when the battered-origin blueprint is unavailable. Preserve that repair.

The exact setting value is not dumped in the reporter's log. The named Bag of Tricks getter patch and zero results match its documented No Item Weight behavior. Verify the exact installed 1.16.4 implementation during intake; do not mislabel a synthetic patch or a newer fork as the reporter's original binary.

For intake correlation, the supplied log is 194,896 bytes with SHA-256:
`dbdeb82c912c43c0ca767ce9a4ca9c80aa024f42a6690964dd0086dbdca0f163`

The excerpts above are sufficient to begin. If the raw attachment is unavailable on the implementation machine, record that fact and continue from the source and bounded reproduction; do not substitute an unrelated local log. Keep raw logs out of Git.

### Reporter settings

Enabled: Acadamae Graduate, Expanded Summoning, Elven Branched Spears, Eastern Weapons, Brown-Fur Transmuter, Protection from Alignment control immunity, Elemental Races, and Magic Circle Spells.

Disabled: Gunslinger, Shield Other, Urban Barbarian, Bodyguard Feats, and Teleportation Spells. showOriginalSummonElemental=False.

Registration is not equivalent to feature publication. In 0.0.193, spear, Eastern Weapon, firearm, and other save-compatible identities are registered even when their feature's publication is disabled. Turning off the spear or Gunslinger option is not an acceptable repair.

## 3. Required player behavior

| State | Required behavior |
| --- | --- |
| Bag of Tricks absent | TRE initializes normally. Every item uses its intended authored weight. |
| Bag of Tricks present, No Item Weight off | Same normal behavior; no special compatibility dependency is needed. |
| No Item Weight on before startup | TRE fully initializes. All TRE item weights are effectively zero; Expanded Summoning and other enabled features publish. |
| Setting turned on during play | Existing and newly obtained TRE items become effectively weightless through the normal inventory refresh path. No restart, re-registration, or item replacement is required. |
| Setting turned off during play | The same item instances regain their intended weights, including items constructed while the setting was on. Naturally weightless items stay weightless. |
| Save and fresh-process load | Existing identities, quantities, enhancements, ownership, and base weights survive. Effective weights follow the setting in the new process. |
| Optional integration information unavailable | TRE continues to initialize. Missing optional members, unreadable settings, or patch-owner diagnostics do not poison the blueprint transaction. |

Cover weapons, their ordinary/masterwork/enhanced/named/magic variants, ammunition, repair items and crafting supplies, equipment, scrolls, and every other TRE BlueprintItem family discovered in current source. Do not restrict the change to the first spear or to the reporter's enabled modules. The all-items claim must be supported by an inventory of the registered TRE item blueprints and the engine getter paths they use.

Zero weight must affect the value used by the game for carried weight and stacks, as well as the displayed item value. Verify a native comparison item so Bag of Tricks' own refresh timing is understood; do not invent a separate TRE-only UI update convention.

## 4. Source audit and repair

Read AGENTS.md, docs/VALIDATION-POLICY.md, docs/AGENT-REVIEW-WORKFLOW.md, and the relevant native-testing instructions first. Start from the current repository; the following findings are pinned to release commit 527f7e2fba36d92ecc80ebd98be3267c287e6d58 and must be reconciled with any newer changes.

### 4.1 Separate authored weight from runtime weight

Use two explicit concepts in the implementation:

- Authored weight: the intended value stored in TRE's blueprint data or deliberately inherited from an authoritative donor field. Validate this against the appropriate catalog or construction contract.
- Effective weight: the value returned by the game's runtime getters after modifiers such as Bag of Tricks. With No Item Weight on, zero is correct.

A successful authored-weight check must not become a fatal startup error merely because a patched effective getter returns zero. Avoid consulting optional, patched runtime weight getters as mandatory inputs to construction or authored-data validation. If they are read for diagnostics, keep those reads contained and nonfatal.

This requirement supersedes the earlier spear mission's blanket rule that every foreign effective-getter mismatch must abort initialization for the runtime-weight case. Keep independent checks of damage, reach, proficiency, identity, enchantments, handedness, and other mechanics intact. Wrong authored values must still be detected; do not delete weight validation or excuse arbitrary malformed data because a mod is installed.

### 4.2 Audit all validators and construction inputs

Use rg to trace `.Weight`, `m_Weight`, weight constants, `Configure`/`ConfigureWeapon` callers, snapshots, copy operations, and validators through the current production source. Classify each relevant read as authored data or runtime observation. The following are mandatory starting points, not a claim that this list exhausts current source:

| File under src/KingmakerGunslinger/ | Established issue or responsibility |
| --- | --- |
| Blueprints/ElvenBranchedSpearBlueprints.cs | SpearWeaponTypeAccess validates the patched type getter; SpearItemAccess.Validate and ValidateNamed also require effective item weight 10. |
| ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs | Currently makes any effective mismatch fatal. Change weight handling narrowly. |
| Blueprints/EasternWeaponBlueprints.cs | Type, ordinary-item, and named-item validators compare patched weight with catalog weight. |
| Blueprints/WeaponTypeMechanicalAccess.cs | Firearm type validation compares effective weight with the firearm specification. |
| Blueprints/ProductionFirearmBlueprints.cs | Firearm item validation makes the same assumption. |
| Blueprints/BasicAmmunitionBlueprints.cs | Nonzero ammunition component weights are checked through the public getter. Preserve items whose authored weight is already zero. |
| Blueprints/FirearmRepairKitBlueprints.cs | Legacy compatibility item expects an effective weight of 1.0. Its identity still matters. |
| Blueprints/MagicFirearmBlueprints.cs | Construction passes source.Weight into ConfigureWeapon; validation compares two effective getters. An all-zero comparison can conceal incorrectly authored data. |
| Blueprints/ProgressionWeaponBlueprints.cs | Validation compares the effective item weight with the canonical item's effective weight; 0 == 0 can conceal a wrong stored weight. |
| Blueprints/MagicCircleScrollBlueprints.cs | Construction passes donor.Weight into item configuration. |
| Blueprints/TeleportationScrollBlueprints.cs | Construction also passes donor.Weight into item configuration, including disabled-publication identities. |
| Blueprints/BlueprintItemAccess.cs and related shared accessors | Inspect the actual field ownership, inheritance, capture/configuration semantics, and other callers before choosing a shared repair. |
| Bootstrap/BlueprintBootstrap.cs | Confirm full initialization, publication order, rollback boundaries, and unconditional identity registration. |
| Gunsmithing/BatteredFirearmOriginRuntime.cs and pricing callers | Preserve the 0.0.193 unavailable-state tooltip/price behavior and normal battered-item handling. |

Construction is part of the bug. Copying source.Weight while the setting is on can write zero into a new item's base data. Replacing fatal assertions alone would allow startup but leave that item permanently weightless after the setting is turned off. Use the authoritative intended value for TRE-defined items, and the correct authored donor value for items intentionally inheriting their donor's weight. Verify those engine contracts against the actual 2.1.7b assemblies. Never modify the native donor to make validation pass.

Do not snapshot an already-modified effective getter and call it the original weight. Test the case where the very first construction happens with zero weight enabled.

### 4.3 Prefer existing Bag of Tricks behavior

First verify whether Bag of Tricks 1.16.4 already patches the shared game getters used by every TRE item. If it does, preserve those getters and fix TRE's construction/validation assumptions; no new weight override or hard dependency is needed.

If a demonstrated TRE item path escapes that behavior, add only the smallest optional integration needed to cover it. Verify how the actual Bag of Tricks setting and toggle representation work. Do not infer an enabled setting from patch-owner presence: patches can remain installed when a setting is off. Treat UMM enabled/disabled state according to the real mod's behavior rather than guessing that toggling UMM removes Harmony patches.

Do not add a compile-time dependency on the Bag of Tricks assembly, duplicate its global patch when unnecessary, change its settings, temporarily disable/unpatch it during registration, force all weights to zero in serialized data, or override the player's zero back to TRE's normal weight. Reuse existing repository accessors and compatibility conventions where suitable; do not introduce a general compatibility framework or a new library for this repair.

### 4.4 Contain optional compatibility failures

The weight feature, its discovery, and its diagnostics must never call MarkFailed, propagate an optional-reflection exception into the global registration transaction, skip required TRE identities, or leave modules half-published. A correct blueprint with a valid runtime zero-weight override must complete initialization normally.

If optional setting/assembly/member inspection cannot be performed, preserve the engine's current effective behavior, keep correct authored data, and continue. Record a bounded diagnostic if useful. A compatibility observation failure must not become a new mandatory startup dependency.

Do not set an initialized flag after failed registration, catch and ignore the entire bootstrap, or publish an empty substitute. Preserve rollback and truthful reporting for unrelated genuine initialization defects. Broad module-isolation redesign is outside this mission unless a concrete part of this repair cannot be completed without a narrowly justified change.

Avoid per-frame, per-item-hover, or per-price-query log spam. Useful diagnostics identify the relevant item/type, intended and stored weight, effective weight when safely available, and optional patch context.

## 5. Verification and acceptance

Use the existing focused gate and native harness. Tests must exercise production construction, validation, and observable item behavior. A test of a duplicated policy, a source-string search, a fabricated helper that skips the real startup, or merely seeing UMM report Active is insufficient.

### 5.1 Required acceptance criteria

| ID | Acceptance condition |
| --- | --- |
| BW01 | A bounded baseline reproduction demonstrates the 0.0.193 weight failure with actual Bag of Tricks 1.16.4, or an explicitly labeled isolated equivalent if that binary is unavailable. Reproduce once; retain the evidence. |
| BW02 | Fresh startup with No Item Weight already on completes TRE registration and enabled publication without a weight-caused rollback, failed state, or missing identity. |
| BW03 | A census of all registered TRE inventory-item blueprints shows correct authored weights and zero effective weights with the setting on. Include disabled-publication and legacy identities. Record expected and observed item counts without hard-coding an obsolete total. |
| BW04 | Representative real inventory instances from every distinct item/getter/construction path have zero displayed and carried weight, including a stack, an existing item, and an item obtained after enabling the setting. |
| BW05 | Live off-to-on-to-off and on-to-off-to-on transitions work on the same items. Correct normal weights return without restart, replacement, double application, or mutation of base data. |
| BW06 | Items constructed while the setting was already on recover the intended weight when it is turned off. Explicitly include magic/named firearms, Magic Circle scrolls, and Teleportation scrolls. |
| BW07 | Save while on, then load that same disposable fixture in fresh processes with the setting on and with it off. Preserve item identities, quantities, enchantments, ownership, and intended base weights; effective values must follow the new process's setting. |
| BW08 | Bag of Tricks absent and present-with-setting-off both initialize and retain normal item behavior. Optional integration discovery/diagnostic failures, if the implementation introduces such paths, cannot fail TRE startup. |
| BW09 | The reporter's feature configuration, including gunslinger=False, initializes successfully. A new sorcerer's Summon Monster I has the expected available creature choices and successfully casts a selected summon. Check the final variant state and the actual cast. |
| BW10 | Ordinary weapon tooltips and prices still work; legitimate battered-firearm pricing/ownership remains correct. Preserve the existing failed-bootstrap tooltip safeguard. Run its focused regression or native check as appropriate to the changed paths. |
| BW11 | Incorrect authored weight and one relevant non-weight spear defect remain detectable. Negative tests cannot pass merely because both compared effective getters return zero. |
| BW12 | Native donors and unrelated items retain their authored data. Global zero weight follows Bag of Tricks normally. Stable GUIDs, released keys, module publication semantics, and save compatibility are unchanged. |
| BW13 | Initialization works whether the external weight modifier is already active when TRE constructs items or becomes active afterward, within supported lifecycle/patch ordering. Optional metadata availability is not a prerequisite. |
| BW14 | Production compilation, a nonempty focused regression selection, independent review, exact candidate identification, and runtime restoration evidence are complete. Missing native evidence remains an explicit gap. |

### 5.2 Keep the native work bounded

Combine checks into three configurations and reuse one disposable fixture rather than running the whole historical matrix. Use the additional restart needed to verify both saved-state transitions:

1. Setting on at startup: use the reporter's module settings, capture completed initialization and the complete item census, instantiate representative families, inspect real inventory weights, perform on-to-off-to-on transitions, test the new sorcerer's Summon Monster I choices/cast, and save the disposable fixture.
2. Setting on at reload: load the saved fixture in a new process with the setting still on. Verify existing item identities and zero effective weights; do not overwrite the fixture merely to manufacture evidence for the next case.
3. Setting off at startup: load that same fixture in another fresh process, verify restored weights and identities, perform the complementary toggle sequence, obtain an additional item, and verify native tooltip/pricing behavior.
4. Bag of Tricks absent: use an isolated configuration for normal startup and representative item behavior. Do not remove it from the human installation.

Use focused production-path tests for construction permutations, negative cases, and any optional-probe failures. A synthetic Harmony postfix returning zero can establish the mechanism, but it does not establish compatibility with the exact installed Bag of Tricks binary. Native acceptance requires the real mod; if it cannot be obtained in the approved environment, leave those criteria unverified and retain the reviewable implementation.

The prior full 43-mod installation is not required to begin or to isolate this concrete interaction. Record the tested mod subset, UMM version, Bag of Tricks binary/version, game build, TRE commit, and artifact hashes. UMM 0.31.1 is the reported supported loader; use it in the isolated lab when available and state any substitution.

Follow the repository's focused validation budget and stop optional expansion once the identified risks are resolved. Do not run Sprint/Tranche gates, requalify every summoning creature, or expand into unrelated old failures. If required native evidence cannot fit the authorized environment or budget, report the exact remaining acceptance gap instead of resetting limits or declaring success.

Use repository-owned runtime requests, leases, shared locks, and transactional staging/restoration. Read the current guarded procedures, preserve KMG_AUTOMATION_BASELINE, and modify only named disposable fixtures. Never take over another session's Kingmaker process. Restore the exact initial lab configuration, including Bag of Tricks settings, before handoff.

## 6. Claude and Codex workflow

This is a new follow-up mission. Do not reopen or resume the cancelled automated enrollment for PR #195, copy its stale launch state, or rewrite its records.

Use Claude Opus with high effort as developer and Codex gpt-6.2-astra with max reasoning effort as independent reviewer. The maintained kingmaker-gunslinger project configuration now specifies that pairing; the repository guide may retain the earlier high-effort reviewer wording. Verify the installed controller's supported configuration and actual model provenance before enrollment. Do not silently downgrade or replace models, reinstall the controller, or modify its permissions/budgets as feature work.

### Launcher entry

If a matching active enrollment already exists for this new mission, use its documented continuation path. Otherwise:

1. Inspect current repository state and remote master. Create a dedicated codex/* branch/worktree, or reuse the explicitly assigned clean worktree for this mission. Preserve all unrelated work.
2. Put this mission in the stated planning path, verify the required repository instructions, and commit the mission before enrollment. Run the installed controller's project check and supported preflight. Resolve routine local preparation within existing permissions.
3. Push through the guarded repository wrapper, enroll the prepared mission with project kingmaker-gunslinger, plan single, and headless mode, then run the returned enrollment ID. Follow the current installed operating guide for exact syntax.

The documented command shape is:

```powershell
$weightReviewController = 'C:/Dev/agent-review/agent_review.py'
$weightWorktree = '<actual absolute worktree path>'
$weightMission = Join-Path $weightWorktree 'planning/BAG-OF-TRICKS-NO-ITEM-WEIGHT-MISSION.md'

python $weightReviewController check-project --project kingmaker-gunslinger --repo $weightWorktree
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1 -RepositoryRoot $weightWorktree
python $weightReviewController enroll --project kingmaker-gunslinger --worktree $weightWorktree --plan single --mission $weightMission --mode headless
python $weightReviewController run --enrollment <returned-enrollment-id>
```

Execute dependent commands only after the preceding check succeeds. The placeholders are inputs to resolve, not literal arguments. The mission must already be committed before the push/enrollment. A genuine controller/configuration block is reported precisely; it does not authorize rebuilding the review system or weakening this mission.

### Enrolled developer entry

If the controller launched you, skip launcher setup. Implement only in the assigned worktree, commit coherent changes, push through the required wrapper, and run the enrollment's required focused gate on the committed candidate with a real matching nonempty filter. Use the injected verification command rather than inventing a controller path or receipt.

The controller owns PR workflow posts and docs/agent-reviews/ archives. Do not start another controller, invoke a competing reviewer, edit its transcripts, or continue writing after ownership has passed. Address each returned finding with a fix or evidence-based disposition and the requested continuation. Preserve the installed limits; do not cancel/re-enroll or extend budgets to erase a stalled review history.

Keep the PR draft with no human reviewers requested. Follow the current controller's supported owner-guidance and continuation mechanism; never assume a status reason alone reaches either agent. The reviewed mission and any authorized amendments must remain visible to both writer and reviewer.

### Review focus

The reviewer must check for the next validator failure, zero copied into authored data, stale setting snapshots, wrong inventory/stack behavior, incomplete item-family coverage, disguised partial startup, optional-dependency exceptions, and missing native proof. Verify that required behavior is supported by the actual candidate and production paths. A successful build or a generic policy test alone cannot establish these results.

## 7. Deliverables and final handoff

Commit the repair and a concise investigation/evidence record, for example docs/BAG-OF-TRICKS-NO-ITEM-WEIGHT-COMPATIBILITY.md, containing:

- The audited construction/validation sites and the chosen authored/effective-weight design.
- Whether Bag of Tricks' existing getters were sufficient, and the evidence for any additional integration.
- A BW01–BW14 acceptance ledger with concrete results, candidate identity, and explicit unverified items.
- Native census and representative-item observations, including construction while on, restoration while off, save/load, and the actual summon cast.
- Commands, durations, versions, artifact identities, and restoration confirmation.

Do not commit raw machine-local logs, save files, packaged binaries, proprietary game/mod assemblies, credentials, or local configuration. Reuse existing test infrastructure and preserve project style; new dependencies require a concrete justification.

Successful automated handoff is a current draft PR at AWAITING_OWNER_REVIEW, with no unresolved blocking findings and a verified current remote status. Distinguish the reviewed product commit from controller archive commits. If required compatibility evidence is missing, report the implementation and precise blocker without describing the mission as fully complete.

The final explanation to Howie should state what players now experience with the setting on and off, which item families were checked, how the initialization failure was eliminated, whether the sorcerer cast was observed, and any material limitation. Do not merge or release.

## 8. Source references

- Reporter Log.txt, SHA-256 recorded above: root cause at line 828, rollback at line 830, nonretrying initialization at line 832, summoning blockage beginning at line 906, tooltip fallback at line 1033.
- TRE 0.0.193 release, release commit 527f7e2fba36d92ecc80ebd98be3267c287e6d58.
- Released blueprint source directory and spear validation policy.
- Bag of Tricks author's 1.16.4 guide, documenting Items & Equipment → setting item weight to zero.
- Author's 1.16.4 source archive listing. Archive retrieval was unavailable during planning; verify the actual approved installation directly.
- Corroborating weight patches in the DUMM derivative: toggleItemsWeighZero causes relevant shared weight getters to return zero. This is a derivative identifying as 1.16.4.39, not byte-identical proof of the reporter's original 1.16.4 build.
- Repository agent-review guide, maintained controller guide, and maintained project configuration. Reconcile installed instructions at launch; do not change controller versions during a run.

## Launcher intake notes (2026-11-22)

These are factual observations recorded by the launching session. They are not amendments to the mission above.

- Worktree: `C:/Dev/KingmakerGunslingerLab/worktrees/bag-of-tricks-no-item-weight`, branch `codex/bag-of-tricks-no-item-weight`, created from origin/master `dfc182aee7` (v0.0.193 publication record merge; contains release commit 527f7e2fba).
- No existing enrollment matched this mission; this is a new enrollment, not a continuation of the cancelled PR #195 enrollment.
- The reporter's raw Log.txt (194,896 bytes, SHA-256 above) was not found on this machine. Work from the excerpts in section 2.
- Bag of Tricks `Info.json` Version 1.16.4, `BagOfTricks.dll` SHA-256 `d03626594ece0f339aeef03ec7259684af7ddeb8b2edf41936f144848059a0e6`, is present in the loader-compat pristine lab (`C:/Dev/KingmakerGunslingerLab/game-copies/loader-compat/.lab-pristine/Mods/BagOfTricks`). The human installation has the same hash. The human installation is protected; use the isolated loader-compat lab. Whether this binary is byte-identical to the reporter's copy is not established.
- The launcher copied the git-ignored local inputs `GamePath.props` and `artifacts/inspection/bodyguard-native/` into the worktree from the main checkout, so the focused gate and complete-suite tests can run.
