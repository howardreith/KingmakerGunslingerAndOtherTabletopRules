# Mission: repair spear startup, summoning and weapon tooltips

Mission contract for the `single` plan (`final` checkpoint) of the agent-review
workflow. Owner request of 2026-11-21. Branch
`codex/spear-bootstrap-tooltip-repair`, target `master` (prepared at
`2e61e78097`, v0.0.192 records merge).

Repair the startup failure and the gameplay/UI faults it sets off in
**Mr. Bard's Tabletop Rules Expansion**: the Elven Branched Spear profile check
aborts blueprint initialization, Expanded Summoning never registers, and
weapon tooltips throw because the battered-firearm sale-price hook demands a
blueprint that was never created.

This document is the contract. `AGENTS.md` and `docs/VALIDATION-POLICY.md`
govern how it is validated. Nothing here authorizes a merge, release, tag,
public version bump, or installing the repair into the owner's normal-play
environment.

## 1. Roles

- **Headless developer (Claude Opus, high).** Implement this mission in this
  worktree only, following the controller's injected instructions. Do not
  create another enrollment, launch another controller, invoke Codex, author
  the controller's transcript files under `docs/agent-reviews/`, or post to
  GitHub.
- **Reviewer (Codex `gpt-6.2-astra`, high).** Reviews each candidate commit
  independently from a detached snapshot, through the controller.
- **Controller.** Owns the draft PR, review requests, review records, archive
  commits and the final handoff.

The launching session prepared this worktree, committed this mission, pushed
the branch through the guarded wrapper and enrolled it. It is not a second
writer on this worktree.

## 2. Bug report

The reporter says:

> I am on UMM 0.31.1 and have a number of other mods. Enabled options:
> Acadamae Graduate, Expanded Summoning, Elven Branched Spears, Eastern
> Weapons, Brown-Fur Transmuter, Protection from Alignment, Elemental Races,
> Magic Circle Spells.
>
> Creating a new sorcerer character with Summon Monster I, the spell doesn't
> expand to show the possible creatures to be summoned—it shows an empty stub.
> Weapons in inventory don't show their tooltips.

### 2.1 Evidence from the reporter's log

The reporter's `output_log(1)(1).txt` was not attached to the request, and no
copy exists in the authorized intake on this machine. The local
`output_log.txt` / `output_log(2).txt` files are unrelated v0.0.190 sessions
and contain neither exception. The excerpts below are the owner's transcription
from the log. They are the only evidence of the reporter's environment. The
raw log is not committed and must not be committed if it arrives later.

The log identifies:

- Kingmaker **2.1.7b**.
- Unity Mod Manager **0.31.1.0**.
- Tabletop Rules Expansion **0.0.188**, build identifier
  `0.0.188-umm-0311-minimum` (release commit
  `097e522f9fa0a8fdc8a773fb51fc3cfbf91d115c`).
- **43 mods** loaded.
- Settings line includes `gunslinger=False`, even though the later tooltip
  failure is firearm-related.
- Call of the Wild **1.14.4c-2.1**, ArmsArmor **1.0.10**, RacesUnleashed
  **1.0.11**, KingmakerBugfixes **1.3.3**, CraftMagicItems **2.1.0**, and
  other mods.

The first failure:

```text
System.InvalidOperationException:
Elven Branched Spear type profile is invalid.

SpearWeaponTypeAccess.Validate
SpearWeaponTypeAccess.Configure
ElvenBranchedSpearBlueprints.Register
BlueprintBootstrap.InitializeCore
```

followed by:

```text
Rolled back 287 custom blueprint registration(s)
Rolled back 28 owned visual resource proxies
Blueprint lifecycle initialization failed and will not be retried in this process
```

Later summoning messages:

```text
Blocked:package-blueprint-lifecycle-failed
bootstrap-unavailable
```

The weapon-tooltip failure:

```text
InvalidOperationException:
The battered origin token blueprint is unavailable.

BatteredFirearmOriginRuntime.RequireBlueprint
BatteredFirearmOriginRuntime.Matches
BatteredFirearmOriginRuntime.TryGetOwner
BatteredFirearmOriginRuntime.IsBattered
BatteredFirearmSaleValueRuntime.TryGetFixedValue
BatteredFirearmSaleValuePatch.Postfix
VendorLogic.GetItemSellPrice
UIUtilityItem.FillTooltipData
TooltipConstructor.SetContent
TooltipBase.Show
```

The same log also records a normal **Summoned Dog** under
`Context: Summon Monster I`. The report does not prove that every form of the
spell is unusable, and nothing written for this mission may say it does.

### 2.2 Source findings already established

Re-inspect current source before editing, but start from these findings
(checked again against `master` `2e61e78097` when this mission was prepared):

- `ElvenBranchedSpearBlueprints` clones the **live native Longspear type**
  (`NativeLongspearTypeGuid`) through `BlueprintCloneService.Clone`.
- `SpearWeaponTypeAccess.Configure` changes category, text, weight and
  enchantments, then `Validate` checks mechanical properties that the clone
  mostly inherited from the donor.
- `Validate` requires melee, 1d8 physical piercing, 20/×3 criticals, the
  Spears fighter group, two-handed / non-light / non-natural,
  `AttackRange.Value == 6` and weight 10. It throws one generic message
  (`ElvenBranchedSpearBlueprints.cs`, around line 372) that names neither the
  failed predicate nor the actual value.
- In `BlueprintBootstrap.InitializeCore`, spear registration (around line 801)
  runs **before** Expanded Summoning registration (around line 825) and
  battered-origin registration (around line 995). In the failed run those
  later identities are never created, and the transactional rollback removes
  everything registered before the throw.
- Spear registration is unconditional. The Elven Branched Spears setting gates
  publication, not this constructor, so unticking the feature does not bypass
  the failing code.
- The sale-price hook probes every `ItemEntityWeapon`.
  `BatteredFirearmOriginRuntime` demands the missing blueprint
  (`RequireBlueprint`) before it determines whether the weapon carries the
  battered marker at all.
- `ElvenBranchedSpearBlueprints.cs`, `BatteredFirearmOriginRuntime.cs` and
  `BatteredFirearmSaleValueRuntime.cs` are byte-identical between v0.0.188 and
  current `master` (v0.0.192). The defect is present in the current release.
- UMM 0.31.1 is explicitly supported by v0.0.188. The log does not establish a
  loader-version incompatibility.

**Still unknown:** which spear predicate fails in the reporter's environment,
and whether a particular third-party mutation or getter patch causes it. Do not
blame another mod without evidence.

Primary files:

```text
src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs
src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs
src/KingmakerGunslinger/Gunsmithing/BatteredFirearmOriginRuntime.cs
src/KingmakerGunslinger/Gunsmithing/BatteredFirearmSaleValueRuntime.cs
src/KingmakerGunslinger/ElvenBranchedSpear/ElvenBranchedSpearCategoryRuntime.cs
src/KingmakerGunslinger/Blueprints/ExpandedSummoningPublication.cs
src/KingmakerGunslinger/Summoning/ExpandedSummoningVariantMenuPatch.cs
```

## 3. Required repair

### A. Diagnose and repair the spear initialization failure

1. Determine which inherited fields and computed properties the constructor
   and `Validate` rely on, including any Harmony patches that affect them (for
   example `ElvenBranchedSpearCategoryRuntime` and any category, range,
   fighter-group or weight getter patches, owned or third-party). Verify engine
   contracts against the actual Kingmaker 2.1.7b references.
2. Improve diagnostics so a failed profile reports:
   - each failed predicate;
   - expected and actual values;
   - the corresponding native donor values;
   - relevant patch owners when a patched getter or category calculation is
     implicated (for example from the Harmony patch info of the getter).

   Keep diagnostics bounded and useful: emitted once per failed initialization,
   never per frame or per hover.
3. Attempt a focused reproduction in the available approved environment with
   the relevant mod configuration (§4.2). If the full reporter stack is
   unavailable, reproduce the specific unsafe assumption in isolation with
   real game objects or the established native harness. Label any simulated
   donor mutation or injected failure as simulated: it proves the failure
   mechanism, not the reporter's exact external interaction.
4. Repair the constructor or compatibility handling based on that evidence.
   Where the owned spear has a fixed intended profile, set the required
   properties explicitly on the owned clone instead of relying on a mutable
   native donor to keep vanilla values. Do not modify the native donor or
   shared objects owned by the game or other mods.
5. Do not remove the validation, swallow the exception, or force a successful
   startup while producing an incorrectly configured spear. Validation stays
   and still fails for a genuinely wrong owned profile.

### B. Prevent a failed startup from breaking ordinary weapon tooltips

1. Make the pricing inspection path handle unavailable blueprint
   initialization safely. When the required KMG state is unavailable,
   inspecting an ordinary weapon must keep the native sale price and let its
   tooltip render. The price hook must not require successful firearm
   initialization merely to inspect an unrelated weapon.
2. Preserve:
   - correct pricing for legitimate battered firearms after successful
     initialization;
   - the ownership and integrity checks used when creating or modifying
     battered firearms;
   - existing-save behavior when Gunslinger publication is disabled.
3. Do not solve this only by checking the Gunslinger feature toggle. Existing
   saved items and disabled publication must stay compatible.
4. Use a narrow, explicit unavailable-state path. Do not broadly suppress
   unrelated corruption or programming errors across the origin runtime.

### C. Restore Expanded Summoning and contain the failure

1. After the spear fix, verify that Expanded Summoning registers and publishes
   successfully and that a new sorcerer's Summon Monster I presents usable
   creature choices.
2. The reported empty-menu appearance was not independently reproduced during
   the initial investigation. Observe the real path before claiming that
   visual symptom is fixed.
3. Inspect the directly relevant runtime hooks for the same
   failed-initialization assumption, including
   `ExpandedSummoningVariantMenuPatch` and `ExpandedSummoningPublication`. Fix
   confirmed instances needed to stop this incident's cascade. Keep this a
   bounded repair, not a redesign of every module.
4. If initialization still fails for a legitimate reason, the failure must stay
   visible and truthful, while unrelated native tooltip and pricing behavior
   stays usable.
5. Preserve stable GUIDs, released identities, save compatibility, module
   publication semantics and the existing summon roster. Do not bypass spear
   registration by dropping identities that existing saves may need.

## 4. Verification and acceptance

Follow `docs/VALIDATION-POLICY.md` (focused development checkpoint). Reuse the
existing harnesses and guarded runtime procedures.

### 4.1 Required coverage

Each item needs evidence, or an explicit gap that says what was not tested
and why.

1. **Spear construction.** The repaired construction path produces the
   intended spear profile under the reproduced condition. The native Longspear
   donor and unrelated weapons stay unchanged.
2. **Failure diagnostics.** An intentionally invalid profile reports its
   actual failed checks (predicate, expected, actual, donor value, patch
   owners where relevant), not the old generic message.
3. **Failed startup.** After an isolated, deliberately induced initialization
   failure, ordinary weapon price queries and tooltip inspection neither throw
   the missing battered-origin exception nor flood the log.
4. **Normal pricing.** Ordinary weapons keep their expected prices, and
   legitimate battered firearms keep their intended special pricing.
5. **Disabled publication.** The reporter's `gunslinger=False` configuration
   works. Existing battered-item handling and released identities stay
   consistent with the project's save-compatibility contract.
6. **Summoning.** With the reporter's enabled feature settings (§2) in an
   approved isolated setup, a successful startup is followed by a visible,
   usable Summon Monster I creature menu for a new sorcerer and a successful
   selected summon.
7. **Regression scope.** Exercise the affected spear and module publication,
   and the relevant summoning and pricing neighbors. Add a focused
   fresh-process save/load check if the repair touches persistence,
   registration or saved-item interpretation.

Prefer behavioral tests through the real production paths and dependencies.
Tests that search source strings, mirror the implementation, or mock away the
failure do not count toward this coverage.

### 4.2 Environment, safety and limits

- Use UMM 0.31.1 in an approved isolated configuration where available, such
  as the disposable loader-compat game copy under
  `C:/Dev/KingmakerGunslingerLab/game-copies/` and the repository's
  compatibility-profile scripts. Do not replace the owner's normal-play loader
  or installation to manufacture a reproduction.
- Native checks use the repository's approved runtime requests, leases, shared
  locks, disposable fixtures and exact restoration
  (`docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md`). `KMG_AUTOMATION_BASELINE` is
  immutable. Never mutate the owner's campaign saves.
- This host (`DATA`) is shared with other sessions that run Kingmaker copies.
  Before any native run, confirm that no other Kingmaker process and no
  configured runtime transaction lock is active. If one is, wait or report it.
  Do not take over or kill another session's game.
- Record exactly which game version, loader, mod configuration, candidate
  commit and artifacts were tested.
- The exact 43-mod reporter environment and its raw log are unavailable, and
  that limitation must be stated. A synthetic reproduction proves only the
  tested failure mechanism. Missing required verification stays a gap. Neither
  the developer nor the reviewer may silently turn it into PASS.
- Keep validation proportional. Broad release qualification, Sprint/Tranche
  levels and unrelated historical campaigns are out of scope unless a concrete
  dependency makes a particular extra check necessary.
- Any failed or ambiguous runtime transaction is restored before handoff.

### 4.3 Required gate

Compile the affected production code. On the final committed candidate, run
the `focused` gate through the enrollment's injected `verify` helper:

```text
python <pinned-controller-path-for-this-enrollment> verify --enrollment <id> --gate focused -- -Filter <actual-relevant-filter>
```

Find a real matching filter, or register the new regression cases through the
existing test mechanism. Do not invent a filter that selects no tests. Direct
test runs are supporting evidence, but they do not replace the controller's
receipt for the candidate under review.

## 5. Developer workflow obligations

- Commit coherent work. Push only through
  `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1 -RepositoryRoot <this worktree>`.
- Do not commit generated packages, raw machine-local evidence (including the
  reporter's log), saves, credentials, local configuration, or proprietary
  assemblies and assets. Put the sanitized investigation record, with
  commands, durations, results, tested versions and limits, in a committed
  document (for example `docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md`).
- Obtain the required gate receipt for the actual candidate commit, then
  return the controller's structured `agent-review.developer.v1` report. Use
  `checkpoint_ready` only for a pushed, clean candidate that has the required
  evidence.
- Address every returned finding with a supported disposition: fixed,
  evidence-based rebuttal, genuinely optional deferral, or a precise
  unresolved blocker.
- Continue through ordinary review corrections within the installed budgets.
  Do not weaken this mission, change permissions or model policy, raise
  budgets, or reinstall or activate the controller to get a blocked run moving.
- If a real access, environment, ownership, publication or design blocker
  occurs, keep the work and report its exact cause through the workflow.
- Do not merge, release, tag, bump the public version, request human
  reviewers, or install the repair into the owner's normal-play environment.

## 6. Final handoff

Completion is the controller's verified final handoff: the enrollment at
`AWAITING_OWNER_REVIEW`, no unresolved blocking findings, and a current remote
status check. The final developer report and committed investigation record
state:

- the reviewed product commit, as distinct from the controller's archive and
  handoff head;
- the spear failure mechanism actually established, and any remaining
  uncertainty about the reporter's exact stack;
- what changed in spear construction, tooltip/pricing handling and failure
  containment;
- focused test results and actual native observations, with tested versions
  and limits;
- confirmation that runtime restoration completed;
- remaining limitations that matter to the owner's review.

## 7. Launch-time setup notes

Recorded by the launching session on 2026-11-21:

- Installed controller `C:/Dev/agent-review` (package
  `65d19b4ba9d9…6c67e`). `preflight --project kingmaker-gunslinger` passed
  for both developer and reviewer.
- `check-project --project kingmaker-gunslinger` reports one problem:
  `instruction file docs/AGENT-REVIEW-WORKFLOW.md is missing`. The project
  configuration lists that file as an instruction for both agents, but it
  exists only on the closed, unmerged PR #194 branch
  (`codex/agent-review-workflow`) and not on `master`. This mission does not
  add or copy that file. Its repository-specific content (branch prefix, push
  wrapper, `focused` gate, developer extras and denials, runtime locks,
  private state root) is also in the installed project configuration and in
  sections 4 and 5 above.
- The reporter's raw log is unavailable (§2.1).
