# CotW spontaneous action-bar load investigation — 2026-10-17

## Owner-authorized alpha 2 publication

The owner subsequently requested a public alpha, superseding the development
checkpoint's no-publication boundary for this one artifact only.
[Controller-load alpha 2](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.157-controller-load-alpha.2)
published at 2026-10-17T22:34:31Z, prerelease / never Latest, exact unchanged
candidate ZIP below renamed `KingmakerGunslinger-0.0.157-controller-load-alpha.2.zip`.
Public download SHA-256 matches; tag peels to product `92b442c06dd8`, manifest
docs/tooling identity is `b66c1dd44c6caef653e4a33fdd0f96b0c43b161a`.
This is the KMG/Tabletop Added Rules update itself; no companion Buff Planner
or CotW update. UMM remains 0.0.157. No rebuild, full suite, native run, merge
or installation occurred. All native/affected-state gaps below remain NOT_RUN.
Stable v0.0.157 and the earlier alphas are unchanged. See
[test/install guidance](RELEASE-NOTES-CONTROLLER-LOAD-ALPHA-2.md).

## Frozen local candidate

Repair/product commit `92b442c06dd8d13f246ca006fdda6382b9eb6236`; source branch
`codex/cotw-controller-actionbar-load-20261017` is pushed and its original
worktree remains clean/frozen. Subsequent records live on
`codex/cotw-controller-actionbar-load-review-20261017` in the separate
`C:/Dev/KingmakerGunslingerLab/worktrees/cotw-controller-actionbar-load-review-20261017`
checkout; they do not change the candidate identity.

Local UMM ZIP (not a release):
`C:/Dev/KingmakerBuffPlannerLab/handoff/KingmakerGunslinger-0.0.157-development-cotw-controller-actionbar-load-92b442c06dd8.zip`.
The same package, development receipt, checksum and build logs are preserved in
the frozen checkout under
`artifacts/development-candidates/cotw-controller-actionbar-load/92b442c06dd8/`.

- ZIP SHA-256: `798ff15e5aca7b8860f1635c35e9f06260150dc2a91598081d673e1527b3f5f3`.
- DLL SHA-256: `e78d79cfb334f6d59c2948b6258128ce73334db59e8f7dba16a01860cc946801`.
- DLL MVID: `0fa8ccba-349e-47e1-a2ec-8b46d20ed69c`.
- UMM version remains 0.0.157; development purpose is
  `cotw-controller-actionbar-load`, releaseQualification=false.
- Candidate build: 69.245 s; existing development wrapper, 6/6 selected startup
  tests of 2,652 registered, exact-reference .NET 4.7 / C# 7.3 compilation,
  37 native-contract companion assertions and strict package validation PASS.
  Command: `Build-Local.ps1 -DevelopmentPurpose cotw-controller-actionbar-load
  -DevelopmentTestFilter brown-fur.startup -ReferenceBundleDir
  C:/Dev/KingmakerGunslingerLab/private/refresh-2026-10-15/extracted/KingmakerGunslinger-private-build-references`.
  Initial default bundle was incomplete (PhysicsModule missing); used the existing
  refreshed provenance-checked bundle, without changing references or validators.
- Exact packaged DLL then passed `Test-CotwSpontaneousSlotCompatibility.ps1`:
  **39/39**, 0.950 s; log `artifacts/actionbar-inspection/candidate-adapter.log`.
  Build log `artifacts/actionbar-inspection/candidate-refreshed-build.log`.
  Full domain suite and native save-load acceptance were NOT run by this build.

## Native boundary at this handoff

DATA availability is authorized, not a permission/hardware blocker. No game
process, staging transaction or native mode change was started. The source
inspection found an important qualification gap: the qualified working-save
loader uses the mouse main-menu button/SaveSlot receiver, while the existing
Teleportation controller fixture starts AFTER a successful mouse load, disposes
the old loading-screen context, loads the native controller loading UI, and
reloads the area. Reusing that casting fixture would test the wrong scenario.
Simply assigning Game.ControllerMode sets its field/cursor; inspected setter IL
does not perform those UI lifecycle operations. It is not by itself a verified
cold-controller startup workflow.

Next bounded work: extend/prove a no-cast, request-bound controller load entry
and loading-UI lifecycle, with positive/negative protocol tests and unchanged
working-save identity/read leases, current-stack transaction, settings rollback
and exit guards. Run a fresh matched mouse/controller pair, observe actual mode,
all 25 Brown-Fur types, real action-bar VMs and party/page refresh, then restore
everything exactly. Only after that PASS resume Buff Planner no-cast entry/exit;
the installed lab KBP is the older 0.1.1-rc3, not the affected computer's
0.5.4-controller.1. Any newer KBP candidate requires its separate guarded
artifact transaction. Neither direct helper calls nor this source-tested ZIP
qualifies that controller path. This checkpoint deliberately preserves the useful
repair instead of selecting an unreviewed loading/scene workaround.

Scope: isolated `codex/cotw-controller-actionbar-load-20261017`, based on
`86de8f281c63458b296ca7d406e6f575ef8193f2`. The published Brown-Fur alpha and
all its lifecycle changes remain intact. No release, merge or permanent install
is authorized for this investigation. Buff Planner source is unchanged.

## Attribution and demonstrated defect

The new supplied tail fails in CotW's conversion helper while the native console
ActionBarSlotVM is constructed. This is a different stack from the earlier
Brown-Fur missing identity / Player.PostLoad failure. The complete new log and
affected installation's KMG DLL/hash were requested; the owner no longer has
them. The handoff's existing output_log.txt is the previous failure, not
evidence of this alpha's startup. Do not infer installed alpha identity or
Brown-Fur registration completion from the version label or this tail.

Exact DATA installation (read-only inspection, all paths below relative to
`C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker`):

| DLL | SHA-256 | MVID |
| --- | --- | --- |
| Mods/CallOfTheWild/CallOfTheWild.dll | `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` | `8caab254-aacf-4811-8093-44b9184e6e53` |
| Kingmaker_Data/Managed/Assembly-CSharp.dll | `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` | `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7` |
| Mods/KingmakerGunslinger/KingmakerGunslinger.dll | `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd` | `842d73db-a49f-459f-9b47-ca80cf0f273a` |

DATA's actual human KMG installation is therefore NOT the tested Brown-Fur
alpha DLL. The preserved alpha product is `913dfd6e89386ff233849f06f902ca8ea2de239e`,
DLL `f878c8c3e36264b9f34d0441443a26c50da5ea939490d03e264f30164731afbb`,
MVID `88974741-ddbe-4fcb-8f1c-145305534eaa`. Its earlier protected native run
registered all 25 Brown-Fur identities at the library-ready boundary; that
evidence remains bound to its original binary, not this new repair or the
affected computer. See [Brown-Fur record](BROWN-FUR-STARTUP-ORDER-REPAIR.md).

Installed IL, not just upstream source, confirms both native slot classes
directly extend MechanicActionBarSlot; neither extends the other. Spontaneous
slots hold an AbilityData field. Memorized slots hold a SpellSlot and their
Spell getter reads SpellSlot.Spell. CotW's spontaneous prefix accepts the
memorized class and invokes that getter three times. An executable headless
probe of that exact getter with the spontaneous receiver returns an actual
BlueprintAbility, NOT the original AbilityData. This establishes incorrect
slot access and the source of bad input without changing any saved state.

The helper's IL starts the reported chain at 000e: load spell, load Caster,
load Progression, load Features, call Features.Enumerable. The stack's 000e
offset covers that sequence; it does NOT distinguish its first null by itself.
The exact first null on the affected character remains unobserved. Do not
claim it was necessarily Caster or that no second readiness issue can exist.
The upstream commit supplied by the owner is a lead, not a substitute for the
matching installed IL (private extracts under artifacts/actionbar-inspection).

## Narrow adapter

CotwSpontaneousSlotCompatibility applies only to the exact observed CotW/game
MVIDs, prefix signature/size and three verified getter calls. It transpiles
those calls to a type-checked spontaneous-slot read. It does NOT unpatch,
reorder or replace the native CotW prefix or rewrite its conversion algorithm.
The existing helper, metamagic setting, variants, storage/restoration options
and execution/resource logic remain in CotW. Unsupported contracts refuse
this optional adapter with a bounded diagnostic.

A small guard presents no conversions only for empty/unready contexts or a
mismatched owner; it does not synthesize another caster, book or AbilityData.
Ready bindings delegate to the full original helper. It keeps no conversion
cache: native page/unit rebinds read current state again. Installed native
ActionBarVM.OnUnitChanged calls OnCurrentPageChanges, which calls
ActionBarSlotVM.SetMechanicSlot and recomputes conversions. No global input
interception, periodic discovery, save editing or placeholder data is added.
An automatic same-page refresh after a genuinely deferred context, if needed,
is NOT established; observe whether that state actually occurs before adding
another lifecycle hook. Source support is not full native UI acceptance.

Startup diagnostics record native and CotW-prefix patch owners, priorities,
before/after constraints, indices, and assembly paths/hash/MVID before patching.
The first 12 ready / 8 pending calls log correct slot/spell/book/cached-book,
caster/owner/progression identities, input mode and entry stage. Diagnostics
tolerate missing objects; they never perform the unsafe old getter in-game.
No helper or loading exceptions are swallowed.

## Focused evidence and limits

- `Invoke-KmgGate.ps1 -Filter brown-fur.startup`: production compilation PASS,
  6/6 preserved startup tests (8.0 s compile + 2.1 s tests). Initial attempt's
  domain build lacked local GamePath.props; corrected the ignored worktree
  configuration, not validators/source assertions. Failure log retained.
- `Test-CotwSpontaneousSlotCompatibility.ps1`: executable installed-game/CotW
  methods plus compiled production transpiler, not source-string assertions.
  Latest working-tree pass 39/39 (0.96 s process); immutable candidate-bound
  result will be recorded separately. Final focused gate: 7.5 s compile,
  0.4 s tests; logs final-focused.log and final-adapter.log.
  Native wrapper is patched before instrumenting its prefix, matching production.
  Covers valid spontaneous and memorized slots, enabled/disabled metamagic
  helper path, retained conversion/variant source/book relationships, empty
  and unbound input, later rebind, wrong owner, multiple books and repeated
  calls without spending spontaneous slots. Headless Unity objects and native
  allocation/liveness are fixtures; settings are read from installed JSON and
  changed only in the isolated test process. Fixture setup failures (readonly
  fields, references, settings path, owner dictionary, component liveness)
  remain in the private logs; no failed behavior assertion was weakened.
- Nonempty metamagic-feat combinations and representative storage/restoration
  abilities still need targeted fixture/native observations; their unchanged
  source path alone is not a PASS. Full suite/release qualification deferred.
- Native campaign loading, party/page switching, same-page deferred refresh and
  Buff Planner no-cast entry/exit: NOT_RUN. The owner confirmed fresh exclusive
  DATA authorization after the source checks; no process has been launched.
  The current qualified loader is a mouse receiver-bound route. Inspect/prove
  its controller-mode boundary before claiming that comparison. Compare identical
  protected working fixture/stack in fresh mouse and controller processes,
  record exact Continue/Load Game action and actual mode, and verify all 25
  Brown-Fur identities before interpreting the action-bar result. Only then
  resume the separate Buff Planner no-cast entry/exit gate. Actual casting is
  later. Read all selected guarded entry points/nested calls before live use.
- Installation, saves, settings, Steam bindings and loader remain unchanged;
  no runtime transaction was started. No public release or default-branch merge.
