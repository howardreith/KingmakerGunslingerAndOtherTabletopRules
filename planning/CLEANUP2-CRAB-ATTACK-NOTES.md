# Cleanup 2 Crab ordinary attack observer correction

## Retained native failure

Source7 run `20261022T0230163237356Z-6f7d860e3931405ea86f0d3ecd66b5e2`, evidence directory `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261022T0230162536737Z-disposable-expanded-summoning-cleanup2-native-review`, remains **FAIL**. Its actual single command succeeded, emitted one real primary-claw weapon rule and Standard action cost, and acted at native command time **0.6670539 seconds**. The full command was skipped after the failure.

The observer's first recorded matching handle had zero motion samples/images; its second had 42 samples, left/right view-space driver displacement 1.186/1.227 and rotation 66.5/77.3 degrees, with three captured poses. The old code registered matching handles before checking whether they had started, finished or possessed an active animation, then required the dictionary count to equal the natural attack count. It could therefore count an unused preparatory handle as an extra attack. The old evidence did not record attack-info/weapon/handle association, so it is not retrospectively relabeled PASS.

## Actual installed lifecycle

Read-only reflection and IL inspection against installed Kingmaker 2.1.7b `Assembly-CSharp.dll`, SHA256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`, MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`:

- `UnitAttack.Init`, token `0x06002679`, calls `InitAttacks`.
- `UnitAttack.OnStart`, `0x0600267E`, calls `InitAttacks` again, then `TryStartNextAttack(true)`.
- `InitAttacks`, `0x0600267C`, replaces `m_AllAttacks` using fresh native single/full attack info objects and calls `ConfigureAnimations`.
- `ConfigureAnimations`, `0x0600268D`, invokes `AttackHandInfo.CreateAnimationHandleForAttack` (`0x0600265A`) for the fresh list. Its handle setter is `0x06002656`.
- `UnitCommand.Start`, `0x060027A5`, marks the command started before calling native `OnStart` and raising its start event. Preparatory observations before that native rebuild are distinct from the actual live chain.
- `UnitCommand.StartAnimation(handle)`, `0x060027A1`, sets the command's own animation reference and executes that handle through the native manager.
- `UnitAttack.OnAction`, `0x06002681`, raises the weapon rule for the current `PlannedAttack` before incrementing the attack index. This is the exact point where the observer can associate the real weapon event with the info/handle/weapon objects. A completed previous handle can remain in the native list while finishing its ordinary blend; only a previously accepted, exact rule-linked prior chain is recognized as such.
- `UnitAttack.TriggerAttackRule`, `0x06002686`, calls `Rulebook.Trigger` at `IL_007F` before returning to the caller's index increment. `Rulebook.TriggerEventInternal`, `0x060070D2`, calls native `RulebookEventBus.OnEventDidTrigger` at `IL_0042` after the rule's `OnTrigger` and before return; the bus method `0x0600754A` dispatches global subscribers first. The existing `Sprint24AttackObserver.OnEventDidTrigger(RuleAttackWithWeapon)` invokes `AfterWeapon` directly for the exact owner/target pair. The new callback records actual current index/info/command-handle references and rejects a mismatch; it never adjusts an index based on expected callback timing.

Scratch IL record: `artifacts/cleanup2-source-audit/crab-handle-lifecycle-il.txt`. This inspection does not prove behavior of the changed observer on new native inputs.

## Changed observer

Only `RuntimeTestRunner.SummonCleanup2CrabAttack.cs` and this new note were changed in this correction. The integrator separately owns the binding policy/patch/tests and compilation.

The observer now records bounded handle-state transitions and replacements, including planned index, exact attack-info/weapon/handle identities and whether the command/handle had genuinely started. A motion series is registered only for a started, nonfinished, active native handle that is the actual `command.Animation` and has a valid current planned or exact just-executed native association. Each planned index accepts one exact live chain. Prestart handles remain diagnostics; duplicate/replaced accepted chains, unknown active handles, wrong weapon entities, wrong native index/info or an unlisted current handle cause failure. An accepted prior native blend is recognized only when its exact weapon event was already observed once.

The native `AfterWeapon` observation captures the actual command's planned index/info/handle and exact weapon entity before native index advancement. Final proof requires one/two exact native events, one/two corresponding accepted series, each linked by in-memory reference equality to the final native attack list and expected primary/additional weapon entities. Printed identity hashes are labels, not proof. Existing real rolls, costs, both-chela motion, three live pose images, full routine and no-forced-act checks remain required.

Bounds: at most 8 native attack-list rows, 32 observed handles, 128 transition/replacement records, 8 observed native weapon associations and 32 distinct failure labels per command. Diagnostic overflow fails rather than dropping arbitrary extra handles. Existing 3,000-frame/35-second command and 150-second phase bounds remain.

## Request-only binding and timing limit

The integrator removed default Harmony registration from `CrabSummonAnimationBindingPatch`. The observer opens only `BeginReview(request, actor, caster)` after the canonical SNA III cast settles. It first requires the actual same live manager's production Claw lookup to be null. The exact per-request/per-actor/per-manager lease alone permits the experimental borrowed Bite lookup. Disposal occurs before final command cleanup, including the failed-command path, and the observer requires the same live manager's Claw lookup to be null again before actor teardown. Lease cleanup/restoration failures are strict failures.

The retained actual event at t=0.6670539 proves the Bite binding changes timing compared with the installed null-handle `ScheduleAct(1)` fallback. This is explicitly a **timing-changing request-only diagnostic experiment**, not an authorized production art-only correction. No timing neutrality is claimed from a successful attack, motion predicate or screenshot. A future timing-neutral render-only solution is a separate implementation/native qualification task. No matched baseline commands were added to this narrow correction.

Missing native Hit remains unsupported. Brachiosaurus receives no generic Tail alias. Its original authored retarget proposal remains synthetic scratch only, with no production adoption.

## Verification boundary

Read the current validation policy; inspected the immutable source7 raw events/points and actual installed lifecycle contracts; reviewed the corrected source and restoration flow. No build, test runner, game/native launch, package staging or Git operation was performed by this subtask. The integrator must compile the coherent checkpoint and run the narrowed affected native reproduction. Source7's old failure remains bound to its original observer/binary.
