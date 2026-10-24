# Cleanup 2 menu observer: optional native input controllers

## Preserved failure identity

The candidate 2 menu run failed before its first popup capture. The original
failure is preserved at the machine-local evidence directory
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261021T2358597967263Z-disposable-expanded-summoning-cleanup2-native-review`.
Run ID: `20261021T2358598187293Z-154b1b409378423f87fdac07851cee9d`.

| Input / evidence | Identity |
| --- | --- |
| Candidate DLL MVID | `cfb63206-f16a-49fa-83d2-e52f527322b8` |
| Candidate DLL SHA-256 | `45df5f642234a0b554af4216a9ce5c82e7ea91afdeda62666a8e6c6547bb309c` |
| `runtime-result.json` SHA-256 | `aa4db9ede5691ca6174a806a60bd2168b10023e926c20a58d50110e1900af3ff` |
| `expanded-summoning-native-profile.json` SHA-256 | `ac474498b9dc45fe0f61bb865c0c71bb641fd80a2c01cc61a2cb30a086e0207a` |

The DLL was read from frozen candidate 2's
`artifacts/bin/Release/KingmakerGunslinger/KingmakerGunslinger.dll` in the
`summon-cleanup2-review2` worktree. It was not modified or rebuilt by this lane.

## Exact instruction and native contract

The managed exception names `RuntimeTestRunner.<Cleanup2Menus>d__1132.MoveNext`
at offset `0x001c3`. Reflection-only decoding of that exact module gives:

```text
IL_01B9 call     Kingmaker.Game::get_Instance()
IL_01BE callvirt Kingmaker.Game::get_SelectedAbilityHandler()
IL_01C3 callvirt ClickWithSelectedAbilityHandler::get_Ability()
```

The null receiver is `Game.SelectedAbilityHandler`. The real action group,
group elements and toggle array had already been read successfully. This is
not evidence that `ActionBarManager.Group` was uninitialized.

The actual installed Kingmaker 2.1.7b IL establishes:

* `Game.ClearPublicControllers()` explicitly clears both
  `ClickEventsController` and `SelectedAbilityHandler`.
* `Game.SetupPublicControllers()` exposes the pointer and selected-ability
  handler in `Default` and `Pause` modes. Its `Dialog` case exposes neither.
* The preserved profile reports native mode **Dialog**, a loaded native area
  and one party character. A null casting handler therefore matches this
  engine mode's contract; it is not proof of an incomplete scene startup.
* `ActionBarGroupSlot.OnToggleGroupClick()` binds the row's native selected
  actor, conversions and spell to `ActionBarSpellsGroup.Toggle`. That path
  calls native `Fill`, `FillSlots`, widget initialization and native `Set`.
  The inspected group/level/variant-opening path does not call the selected
  casting handler. `ActionBarSubGroupLevels.OnClick(int)` changes the real
  level index and invokes native slot selection.
* `PointerController.ClearPointerMode()` sets the native pointer mode to
  `Default`; `ClickWithSelectedAbilityHandler.DropAbility()` returns when its
  ability is already null. These operations still require a present receiver.

No casting ability is selected by this observer's group-opening operations.
The phase continues to qualify the actual popup, row identities, scroll
coverage and framebuffer; absent input controllers do not waive those checks.

## Narrow source correction

Only `Cleanup2Menus` in `RuntimeTestRunner.SummonCleanup2Review.cs` changes:

1. Record the actual intake input-controller references. Reject open groups,
   any exposed selected ability, or any exposed pointer outside `Default`.
2. Admit an absent selected-ability handler as absent, using a null-safe read.
   Record native mode, both presence flags, pointer mode, selected ability and
   closed-group count in a new `ui-input-intake` observation.
3. During cleanup, clear/drop only an intake controller that was present and
   is still the exact same native object. Do not create or claim absent input
   controllers, or touch a replacement controller exposed by another mode.
4. Include exact controller-reference preservation in the existing restoration
   predicate, alongside original party and memorized-slot object checks.

The phase does not force Default mode, initialize public controllers, fabricate
widgets, change original spellbooks/hotbars, or change popup acceptance rules.

## Validation limits

This lane decoded the exact failed binary and inspected the relevant installed
engine IL, then reviewed the narrow source correction. The integrator owns the
next candidate compilation and affected native menu rerun. No new menu capture
or native PASS is claimed. Candidate 2's failed record remains unchanged.
