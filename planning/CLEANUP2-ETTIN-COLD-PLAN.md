# Cleanup 2 Ettin cold hydration: prepared, live execution deferred

## Exact intended scope

The mission authorizes a focused Working-save reload after Ettin ownership and
material changes. The proposed development observation is restricted to:

| Field | Exact value |
| --- | --- |
| Development purpose | `summon-cleanup-iteration2` |
| Public version retained | `0.0.163` |
| Persistence scope | `giant-humanoids-ii` |
| Mutable save | `KMG_AUTOMATION_WORKING` |
| Mutable filename | `Manual_299_KMG_AUTOMATION_WORKING.zks` |
| Creature keys | `ettin`, `cloud-giant`, `storm-giant` |
| Native sequence | `working-save-expanded-summoning-prepare`, `working-save-expanded-summoning-verify-cleanup`, `working-save-expanded-summoning-verify-absent` |

Each phase needs a distinct fresh guarded Steam App ID 640820 process and the
same source-bound candidate. Module ON is required for the proposed cold
attachment check; this plan does not add an OFF run, another scope, a broader
roster, another save or a release qualification claim. The protected baseline
and every other save must match a fresh intake catalog. No fixed historical
save count substitutes for that catalog.

## Concrete missing transaction boundary

`Invoke-Sprint22InstalledStack.ps1` currently owns one phase. It validates the
candidate/package/source binding, requires exactly eleven identifiable installed
mods, stages that captured stack, launches the qualified native scenario, waits for exit, and restores
the actual intake installation and game settings. It does **not** back up or
restore original Working save bytes. WIN10-AUTONOMOUS-RUNTIME-TESTING explicitly
leaves that original-Working restoration to an outer operator. The actual fresh
human intake in this mission has sixteen mods; this historical eleven-mod
prerequisite is another refusal, not authority to substitute an old intake or
remove five current mods. A new narrowly scoped transaction must inventory and
restore the actual current intake; this checkpoint does not change that guard.

The three wrappers cannot safely be surrounded by an existing runtime lease:
`Enter-KmgCompatibilityTransaction` acquires its own lock and has no inherited
parent-lease contract. Releasing the lock between phases would leave changed
Working state outside a retained owner. No generic reentrancy change or
invented parent lease is included in this checkpoint.

The script-local `Restore-WorkingSave` in the older compatibility-mechanical
driver is insufficient: it blindly copies over Working after a bounded process
wait without an ensuing no-running-process assertion, validates only the hash,
and does not validate restored length/path/attributes. Its separate
baseline-derived compatibility fixture is not this cold-trio route. Do not call
that driver as a workaround. `Open-KmgProtectedSaveCatalog` in
TeleportationPersistence.Common holds read leases on **all** files, including
Working; it cannot admit this mutable trio unchanged.

Before any live use, implement and qualify one exact transaction spanning:

1. Fresh package/receipt/reference/selected-test validation and current clean
   source/commit/version binding through the existing readers. Capture actual
   current installation, settings, the complete save catalog and exact Working
   descriptor/header. Refuse ambiguity, reparse traversal or a running game.
2. A verified original-Working backup under the transaction's private recovery
   state, not Git or runtime screenshot/evidence exports. Retain canonical path,
   full SHA256, length, creation/write/access UTC ticks and attributes. Preserve
   any additional metadata the chosen replacement method could change. Hold
   write/delete/rename-denying leases on every other pre-existing save.
3. One retained owner for all three unchanged native scenarios, each with a
   fresh request/deployment receipt and fully exited process before continuing.
   A possible narrow seam is a new exact-trio branch inside the installed-stack
   transaction, rather than three reentrant installed-stack wrappers.
4. Original-Working restoration on success **and every failure**, only after
   exact process exit and ownership/path/backup checks. Verify restored bytes,
   length, exact path and all captured metadata; verify the baseline and complete
   protected catalog, with no unowned file creation. This is not a blind external
   copy-over. Preserve backup and recovery debt if any check fails.
5. Independent exact installation/settings restoration. A failed Working
   restoration must not be masked by successful installation restoration or
   allow the compatibility lock/recovery obligation to be cleared. The existing
   restoration helper alone does not yet compose this additional obligation.

This is a missing implementation/qualification prerequisite, not an owner
permission request. The authorized Working-only task does not waive it.

## Existing observer coverage limit

The selected roster policy already names exactly Ettin, Cloud and Storm.
`ObserveSprint29PersistentGiant` checks distinct owned nonremovable/nonlootable
equipment, party inventory, and Cloud/Storm spent resources/catch ledgers.
Native load readiness, saved profile/receipt identity, cleanup save and fresh
absence also exist.

For module-ON Ettin, the present `profile-view` visual predicate requires a
nonnull native view. It records body outcome, variant ownership and renderer
material names, but does not require the exact Ettin attachment or protected
material initialization state. A future trio PASS would therefore still need
a bounded exact-owner cold observation of the actual changed attachment and
material surfaces. No observer was modified in this checkpoint.

## Prepared source and tests

`ExpandedSummoningOrchestration.Common.ps1` now provides planning-only helpers:

* `Assert-KmgCleanup2EttinColdPlanScope`: exact purpose/scope/save/version only.
* `New-KmgCleanup2EttinColdPlan`: reuses the actual development source-binding
  predicate, freezes supplied scalar Working metadata, lists the exact trio and
  unresolved prerequisites, and always returns `liveUsePermitted=false` and
  `runtimeLaunchPermitted=false`. It does not pretend to validate package bytes.
* `Compare-KmgCleanup2WorkingSaveRestoration`: compares supplied canonical path,
  whole-file hash, length and timestamp/attribute records. It reads/writes no
  bytes and cannot prove that a native run or restoration happened.

The real installed-stack entry point explicitly refuses the new exact purpose
before process inspection, package access or any mutation. Historical Sprint
29 development authority and every existing scope/purpose refusal remain intact.

Executed checks from the main integration worktree:

| Command | Result | Tool-reported duration |
| --- | --- | --- |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Sprint22Cleanup2ColdPlan.ps1` | PASS, 45 predicates | 0.535 s |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Sprint29DevelopmentPersistence.ps1` | PASS, 8 predicates | 0.227 s |

The new tests use synthetic scalar records, execute exact positive planning
and negative scope/version/source cases, detect each restoration-field
mismatch, and invoke the actual pre-runtime refusal. No save, package, game,
installation or settings were accessed. No build, native execution, Git action,
frozen candidate modification or active-state update was performed. Cold
hydration remains deferred behind the retained-owner/restoration and exact
Ettin observation prerequisites above.
