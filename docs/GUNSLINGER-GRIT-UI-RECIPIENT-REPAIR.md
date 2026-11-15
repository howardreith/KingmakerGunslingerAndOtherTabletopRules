# Missing Gunslinger class, second report (against v0.0.190): grit UI recipients

Status: **development repair, reviewable, not released.** This record covers one review branch and one development
candidate. Nothing here is a release qualification, and v0.0.190 stays published and unchanged. The first report and
its repair are in `docs/GUNSLINGER-MISSING-CLASS-REPAIR.md`.

## The report

The same player updated to v0.0.190 and still had no Gunslinger class. Their new log records Pathfinder: Kingmaker
2.1.7b, Unity Mod Manager 0.33.3.0, mod 0.0.190 and the published DLL
(`88ab89933746fb1072a0b1c68ddad8a105320d911cccc202204c97fe0bc3df9c`). The game assembly they loaded has SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`, which is the project's Kingmaker 2.1.7b reference.

The v0.0.189 failure is gone. Startup now reaches a later step and fails there:

```text
[blueprints][initialize.root-cause] System.NullReferenceException: Object reference not set to an instance of an object
Kingmaker.UI.Common.UIUtilityTexts.GetLongOrShortText (description, state) [0x00008]
Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility.get_Description () [0x00006]
KingmakerGunslinger.Grit.GritAbilityUiIntegration.DescribesGritSpend (ability) [0x00009]
KingmakerGunslinger.Grit.GritAbilityUiIntegration.Apply (library, grit, dodge) [0x000b1]
KingmakerGunslinger.Bootstrap.BlueprintBootstrap.InitializeCore (context, library) [0x0097d]
```

The Protection from Alignment rollback from the first repair then reports `rollback.complete`, 20 of 20 restored. The
main registry rolls back, initialization fails and the class is never published.

## What was wrong

`GritAbilityUiIntegration.Apply` decided which abilities show grit on the action bar. It walked every ability in the
library whose internal name starts with `KMG_` and took an ability as a recipient if it was the Dodge ability, or a
component field referenced the grit pool, or its displayed description contained the English words "grit" and
"spend" and not "without spending grit". The file is byte-for-byte the same in v0.0.189 and v0.0.190; the first
repair only let startup get far enough to reach it.

Two things follow from that.

**The scan reads text it has no business reading.** The lab build holds 2,765 `KMG_` abilities. 2,638 of them belong
to Expanded Summoning. On a stock game the pass read the displayed description of 2,113 of them to bind 22.

**Kingmaker's description getter throws on a null description.** Decompiled from the reference assembly:

```csharp
// BlueprintAbility
public override string Description => UIUtilityTexts.GetLongOrShortText(base.Description, state: true);
// BlueprintUnitFact
public virtual string Description => m_Description;   // LocalizedString converted to string
```

`GetLongOrShortText` copies its argument to a local and calls `Contains` on it. That local is the only object the
method dereferences, and its first use is at IL offset `0x0008`, the offset in the report. So on the reporting
installation one `KMG_` ability's description converted to a null string. The exception is raised inside the getter;
the `IsNullOrWhiteSpace` check on the next line of `DescribesGritSpend` never ran.

Stock Kingmaker does not produce a null there. Its conversion returns the text `<null>` for a description that was
never stored and an empty string or a key placeholder for a stored one. A null needs something on the installation
to change that conversion.

## Reproduction

The loader-compat harness already had a condition that makes a never-stored `LocalizedString` resolve to an empty
string. The description getter tolerates an empty string, and v0.0.190 passes under it. A new condition,
`unassignedTextResolvesNull`, makes the same conversion return null.

Under that condition the published v0.0.190 package fails in the lab with the reported stack, frame for frame and
with the same five IL offsets, followed by the same `rollback.complete` line (launch
`03-unassigned-text-null-class-screen` of the baseline session below, which has no other harness patch on those
methods). The class catalog then holds no Gunslinger and the class screen lists it 0 times.

The harness recorded where the pass stopped:

| | |
|---|---|
| Ability | `KMG_Summoning_Infernal_BarbedDevil_Pyrotechnics` (`d31c6a33c4b7450cb461491d7bd7a1d9`), hidden |
| Description stored | no (the field is null; no key) |
| Raw resolution under the condition | null |
| Native displayed description | throws `NullReferenceException` |
| Descriptions read before it | 2,028 |

Three abilities in the build have no stored description, all hidden Expanded Summoning helpers:
`KMG_Summoning_Infernal_BarbedDevil_OrdersWrath`, `KMG_Summoning_Infernal_BarbedDevil_Pyrotechnics` and
`KMG_Summoning_Infernal_BoneDevil_DimensionalAnchor`. None has anything to do with grit.

**What this does and does not establish.** The mechanism is established: a null description reached the getter from
the grit UI pass. The lab reproduction matches the report's stack exactly on the same DLL and game assembly. It is
also consistent with the first report, where the same installation treated a never-stored name as missing. Not
established: which ability failed on the reporting installation, and what makes text resolve to null there. The log
names neither. The condition is a controlled stand-in. The Italian translation, the `zhCN` locale setting, Unity Mod
Manager 0.33.3 and the other fifty mods were not reproduced and are not blamed.

## The repair

Membership no longer comes from text or from a scan. `GritAbilityUiIntegration.Apply` takes the constructed
`GunslingerClassBlueprintSet` and binds an explicit list of typed ability references (`Recipients`). No description
is read and the library is not walked.

### Recipients

These are the 24 abilities the released build binds on a stock English game (census of the published package,
below), checked one by one against each deed's own spending code.

Shared indicator (22). The button shows remaining grit and is disabled at zero. The indicator's `Spend` does nothing;
the deed charges grit itself.

| Deed | Ability | Why it belongs |
|---|---|---|
| Quick Clear (move action) | `KMG_QuickClear_Move` | spends 1 when used |
| Pistol Whip | `KMG_PistolWhip_Ability` | spends 1 when used |
| Dead Shot | `KMG_DeadShot_Ability` | spends 1 in its transaction; its ability logic has no resource field |
| Targeting (Arms, Head, Torso, Legs) | `KMG_Targeting*_Ability` | each spends 1 when used |
| Menacing Shot | `KMG_MenacingShot_Ability` | spends 1 when used |
| Bleeding Wound (Hit Points, Strength, Dexterity, Constitution) | `KMG_BleedingWound_*_Ability` | arms a marker; 1 or 2 is spent on an eligible hit |
| Expert Loading | `KMG_ExpertLoading_Ability` | arms a marker; 1 is spent when a misfire would explode the firearm |
| Slinger's Luck (saving throw, skill check) | `KMG_SlingersLuck_SavingThrow_Ability`, `KMG_SlingersLuck_SkillCheck_Ability` | arms a marker; 2 or 1 is spent at the reroll |
| Death's Shot | `KMG_DeathsShot_Ability` | arms a marker; 1 is spent on a confirmed critical |
| Stunning Shot | `KMG_StunningShot_Ability` | arms a marker; 2 is spent on an eligible hit |
| Focused Aim (Mysterious Stranger) | `KMG_FocusedAim_Ability` | spends 1 when used |
| Clipping Shot (Mysterious Stranger) | `KMG_ClippingShot_Ability` | arms a marker; 1 is spent on a miss |
| Up Close and Deadly (Pistolero) | `KMG_UpCloseAndDeadly_Ability` | arms a marker; 1 is spent after the attack |
| Twin Shot Knockdown (Pistolero) | `KMG_TwinShotKnockdown_Ability` | spends 1 when used |
| Steady Aim (Musket Master) | `KMG_SteadyAim_Ability` | spends nothing but requires grit; has always carried the indicator |

Own grit resource logic (2). Built with a resource component that shows and spends through Kingmaker's own path; they
get nothing added.

| Deed | Ability | Component |
|---|---|---|
| Deadeye | `KMG_Deadeye_Ability` | `DeadeyeGritResourceLogic` |
| Gunslinger's Dodge | `KMG_GunslingerDodge_ProneAbility` | `$KMG_GunslingerDodge_NativeGrit` with its cost calculator |

Deliberately not recipients, as in every released version: Quick Clear as a standard action, Utility Shot (Stop
Bleeding), Startling Shot and Lightning Reload. Each needs grit to remain, spends none and is gated by its own logic.
Passive deeds (Gunslinger Initiative, Evasive, Cheat Death, Fast Musket) are features with no button. Stranger's
Fortune uses its own pool. `TrueGritCatalog` is a different list (eligibility for True Grit) and is not used here.

No recipient is optional. The three archetype sets are registered unconditionally before the pass, so a missing one
is an error, not something to skip.

### Contract checks

`Apply` checks every recipient before it changes any, adds the indicator where it is absent, then verifies the whole
set and returns how many abilities carry the shared indicator. Consequences:

- A second run on a satisfied set succeeds and replaces no component array. "Nothing added" is not a failure.
- An empty list, a missing ability, an ability listed twice, a shared recipient that already carries other grit
  resource logic, and an own-logic recipient without exactly one such component are each refused. Nothing is changed
  when one is refused.
- The message names the deed, the blueprint's internal object name and its GUID, and what was violated. It does not
  call a displayed name or description.

### What did not change

Blueprint GUIDs, symbols and internal names; grants, action types, targeting and visibility; grit costs, reductions
and when grit is spent; marker consumption; the indicator component (`$KMG_SharedGritUi`, amount 1, spend flag on)
and its no-op `Spend`. The log line still reads `Bound the native shared-grit indicator to 22 deed abilities.`

Files: `Grit/GritAbilityUiIntegration.cs`; `Blueprints/MysteriousStrangerBlueprints.cs` (the set now exposes its
Focused Aim and Clipping Shot abilities); and the call site in `Bootstrap/BlueprintBootstrap.cs`.
`BlueprintBootstrap.cs` is pinned by the icon catalog. The call-site change is recorded there as one exact
authorized hunk, reversed first, with the baseline hash unchanged, under the owner's instruction for this task to use
the authorized process for a precise required delta. It touches no icon or assignment.

## Evidence

All game launches ran in `C:\Dev\KingmakerGunslingerLab\game-copies\loader-compat` through
`scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1`, loader profile `owner-0.33.0`, with saves and
the area cache redirected to a per-session profile. Every session reported the owner's saves and installation
unchanged and the registry settings restored. Evidence root:
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\`.

| | Baseline | Repaired development candidate |
|---|---|---|
| Source | tag `v0.0.190` (`27244257b1`) | commit `4acb7055c92c1633c59383c32118d2c6dc1ea90f` |
| Package SHA-256 | `1f1f31fa161ea251396733f6ee45d6f7df20930f89431a4c9d91771bd4446de6` | `9d59acd5267399829759cb9e676f82e2b8be80fef0eaa63d926be04cf883ba79` |
| DLL SHA-256 (as loaded in process) | `88ab89933746fb1072a0b1c68ddad8a105320d911cccc202204c97fe0bc3df9c` | `4611260e4e07db275d2685c5645570e244e5ca33e2d8650ab466f216dfc43d7d` |

Session `gunslinger-grit-ui-recipients.json`, the same seven launches on both:

| Launch | Baseline `20261115T170712Z` | Repaired `20261115T171454Z` |
|---|---|---|
| `stock-recipients` | initialization completes, the class is published once and the recipients are the expected 24. FAIL only on the new requirements: the pass read 2,113 ability descriptions, and the product has no recipient-level entry point to repeat | PASS: 22 shared and 2 own, no other ability carries either; the pass replaced exactly those 22 arrays and read 0 descriptions; a second run returned 22 and replaced none; five malformed recipient lists refused by name and GUID with nothing changed |
| `unassigned-text-null` | FAIL: the reported exception; class published 0 times | PASS: `initialize.complete`; class published once; 22 shared and 2 own; 0 descriptions read |
| `unassigned-text-null-class-screen` | FAIL: the reported stack exactly; the class screen lists the Gunslinger 0 times | observations correct, launch FAIL on a harness screenshot check; PASS on the rerun below |
| `unassigned-text-null-creation` (`disposable-gunslinger-creation-commit`) | FAIL | PASS: native creation selected and committed a Gunslinger |
| `unassigned-text-blank` | initialization completes and the class is published once (as v0.0.190 already did); FAIL only on the 2,113 descriptions read | PASS |
| `unrelated-description-throws` | FAIL: initialization fails on the injected exception from `KMG_Summoning_Infernal_BoneDevil_DimensionalAnchor` | PASS: neither ability's getter was called; both are unchanged and not recipients |
| `unrelated-description-says-spend-grit` | FAIL: Hydraulic Push and Quick Clear (standard action) each received the grit indicator | PASS: neither is a recipient; both arrays untouched |

The repaired class-screen launch failed its first run on a harness fault, not on the game: the probe checked for its
last screenshot a fixed 30 frames after asking for it, before Unity had written the file (the picture is in the
evidence folder). Its recorded observations were already complete and correct. The probe now waits up to ten seconds
for a requested picture. The launch was then run again alone, unchanged, on the same candidate
(`20261115T172339Z`, session name `gunslinger-grit-ui-recipients-class-screen`): PASS. With unassigned text
resolving to null, the New Character class screen lists 24 classes and the Gunslinger once, labelled `Gunslinger`;
its own toggle selects it; Next moves on to the next screen with it still selected at class level 1; and the product
logs `chargen-selector-result count=25,byReference=1,byGuid=1`. This is the screen observation. Selection and commit
through the native creation controller is the separate `unassigned-text-null-creation` launch above.

Grit spending, the product's own guarded scenarios on the repaired candidate (each a fresh process):

| Scenario | Session | Result |
|---|---|---|
| `disposable-gunslinger-grit-resource` | `20261115T172457Z` | PASS: every paid deed exposes exactly one no-double-spend grit component; the native count reads 1 then 0; zero disables the button; the indicator's `Spend` leaves grit unchanged |
| `disposable-gunslinger-deadeye` (own resource logic) | `20261115T173801Z` | PASS: grit 2 to 1 on use; a duplicate callback leaves it at 1; a request that is not authorized spends nothing |
| `disposable-gunslinger-dead-shot` (spends when used) | `20261115T172457Z` | PASS: observed grit 4, 3, 3 across its volleys; `dead-shot-cost` passes |
| `disposable-gunslinger-expert-loading` (marker, spends later) | `20261115T172457Z` | PASS: observed grit 3, 2, 0 across its suppression and fail-closed cases; marker consumed |
| `disposable-gunslinger-slingers-luck` (marker, both variants) | `20261115T172457Z` | PASS: saving throw 4 to 2, skill check 2 to 1, each marker consumed, the other character's grit unchanged |
| `disposable-pistolero-deeds` | `20261115T172457Z`, `20261115T173518Z` | grit assertions PASS in all three runs (one grit for Up Close and Deadly on a hit and on a miss, none against precision immunity, one for Twin Shot Knockdown). The damage assertion `up-close-critical` is unstable: on the same candidate it failed twice (observed damage `6,3`) and passed once (`3,3`), and it passed once on the published package (`3,3`, `20261115T173211Z`) |
| `disposable-gunslinger-dodge` | `20261115T172457Z` | ERROR before any assertion: "The native Dodge command completed without its timed Buff fact". The published package gives the same error (`20261115T173211Z`) |

Dodge reproduces on the published baseline. The Pistolero damage assertion is intermittent on the repaired
candidate; its cause remains unestablished. One passing run of the published package does not show whether it is
intermittent there too. All recorded Pistolero grit assertions passed. The production change touches no damage code,
which is a reason not to suspect it and not a demonstration. Neither was repaired here; both are recorded in
`KNOWN-ISSUES.md`. Gunslinger's Dodge has no passing spend check in this record. The census shows its component array untouched and its own grit component still the only one. The committed
`gunslinger-grit-spending.json` holds the five scenarios with a stable verdict; Dodge and the Pistolero deeds were in
the file for the run above and were taken out afterwards, and Deadeye was added and run on its own.

Offline:

| Check | Result |
|---|---|
| `Invoke-KmgGate.ps1 -Filter 'grit-ui-recipients;third-playtest.grit-shared-ui;mysterious-stranger;armed-presentation;protection-alignment'` | on the branch head: production compiled; 31 of 2,993 selected, 0 failures |
| `Build-Local.ps1 -DevelopmentPurpose grit-ui-recipient-contract -DevelopmentCompleteSuite` | at `4acb7055c9`: complete suite 2,993 of 2,993, 0 failures; development validation and strict package validation PASS |
| `tools/validate_icon_catalog.py` | PASS |

The tested candidate is commit `4acb7055c9`. The branch head adds, after it, only the screenshot wait in the harness
probe, the trimmed spending session and these records. No file under `src/`, `assets/`, `assets-source/` or
`blueprints/` changed after `4acb7055c9`, so the candidate's product bytes are the branch's product source. The
candidate was not rebuilt to stamp the later commit.

A first build attempt in the new worktree failed 23 tests that read
`artifacts/inspection/bodyguard-native/Assembly-CSharp.il`, a machine-local file the fresh worktree did not have. It
was copied from the main checkout and the build repeated. No source changed between the two.

The two new domain tests (`grit-ui-recipients.*`) are engine-free. One ties the session's expected GUIDs to an
audit written out by manifest symbol and to the released manifest. The other keeps description reads and library
scans out of the integration source. The integration itself needs Kingmaker's blueprint types and is exercised only
in the game.

The previous repair still holds on this candidate: the null and blank launches log no `rollback-failed` and no
`rollback.incomplete`, and the complete suite includes `armed-presentation.*` and `protection-alignment.*`. The
earlier sessions `gunslinger-missing-class*.json` were not run again; their product code is untouched.

## Review follow-ups

The review of PR #166 at `21387ac767` found no blocking defect and listed two corrections, both done on the branch:

- **Evidence wording.** The record and the spending session's description said the unstable Pistolero damage
  assertion was not caused by this change. The runs do not show that. The wording above now states what they do show.
- **Refusal checks.** The harness accepted any exception as a refusal, and the repeat probe recorded the returned
  count without checking it. A refusal must now be an `InvalidOperationException` whose message states the violated
  rule and names the deed, and the internal name and GUID where there is an ability. The repeat must return the
  session's own count of shared recipients, 22. The `stock-recipients` launch was run again alone, unchanged, on the
  same candidate (`20261115T183438Z`, session name `gunslinger-grit-ui-recipients-stock`): PASS, with all five
  refusals of the intended type and reason and the repeat returning 22.

The review also confirmed that the bootstrap call-site edit was already authorized by the task and that reversing the
catalog's authorized edits reproduces the pinned baseline hash.

## Limits and remaining checks

- The reporter's installation, translation and mod list were not reproduced. Null resolution of unassigned text is a
  controlled stand-in that matches their stack. The failing ability on their installation is not known.
- The lab loader is Unity Mod Manager 0.33.0; the report used 0.33.3.
- The three hidden summoning abilities still have no stored description. The repaired pass never reads them, and
  startup, the class screen and character creation complete under the null condition. Any other code that asks one
  of them for its displayed description on such an installation would throw. They are hidden and were left alone
  here; see `KNOWN-ISSUES.md`.
- The action bar itself was not looked at for each deed. The census reads the blueprint components the action bar
  consumes; `disposable-gunslinger-grit-resource` reads the native count and availability for one granted deed.
- Spending was checked for Deadeye, Dead Shot, Expert Loading, both Slinger's Luck variants and the two Pistolero
  deeds. The other recipients were not each spent in the game; their spending code is untouched.
- `disposable-gunslinger-dodge` errors in the disposable copy, on the published package too. One Pistolero damage
  assertion is intermittent on the repaired candidate, and whether this change has any part in that is not
  established. Neither was investigated further.
- Design question, not changed: Steady Aim needs grit and spends none, and carries the indicator. Quick Clear
  (standard action), Utility Shot, Startling Shot and Lightning Reload are in the same position and do not. The
  indicator also counts remaining grit in units of 1 whatever the deed costs. Both are released behaviour and were
  preserved.
- This is a development candidate built against the released version number. A release needs its own version,
  release validator pins and the normal release process on the release build.
