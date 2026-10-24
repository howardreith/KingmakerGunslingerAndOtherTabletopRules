# Cleanup 2 body animation observer correction

## Scope and evidence

This correction changes only the animation selection and bounded diagnostic
helpers in `RuntimeTestRunner.ExpandedSummoningCreatureReview.cs`. It does not
change body assets, action sets, capture timing, the required packet contract,
the positive attack/Hit handle guards, or mechanical attack commands.

The preserved first body observer result is under the machine-local directory
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261021T2344278601321Z-disposable-expanded-summoning-cleanup2-native-review`.
Its run ID is `20261021T2344278771330Z-14d505db922b48e9bade083969db346c`.

| Artifact | SHA-256 |
| --- | --- |
| `runtime-result.json` | `f4fa8fe641e20723a5c8ba39c687b5097a804d1972474c81c129370c59bba74a` |
| `summon-cleanup2-observations.json` | `4f13359e436239f93f5fbdb51b85f31500f48c3c0a467d275aaafd42788b5f33` |

All nine bodies produced 9 stills and 108 tracking frames, for 117 files each.
That file coverage does not prove an attack or hit animation played.

| Body | Native travel, metres | Attack handle | Hit handle |
| --- | ---: | --- | --- |
| Giant Crab | 5.900497 | false | false |
| Griffon | 6.03035069 | false | false |
| Giant Stag Beetle | 5.88382339 | false | false |
| Girallon | 5.94729328 | true | false |
| Ettin | 5.932849 | true | false |
| Elephant | 6.06145144 | false | false |
| Stegosaurus | 5.911079 | false | false |
| Brachiosaurus | 5.68587065 | false | false |
| Gorgon | 5.87053776 | false | false |

The old observer recorded no native action inventory, clip inventory, or selected
natural-weapon animation. Its seven absent attack handles therefore cannot be
attributed to missing animations on those assets. Its nine absent Hit handles
remain unresolved pending a bounded native action inventory.

## Verified installed engine contracts

The inspected installed `Assembly-CSharp.dll` has MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7` and SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
Reflection and matching private IL were read from the actual Kingmaker 2.1.7b
installation. Proprietary IL is not added to this repository.

* `UnitEntityView.AnimationManager` returns the character avatar's manager when
  present, otherwise its native animator manager.
* `UnitAnimationManager.GetAction(UnitAnimationType)` selects the first
  `UnitAnimationAction` whose `Type` matches from its actual `ActionSet`.
  `ActionSet` is the manager's `AnimationSet.Actions` filtered to that native
  action type.
* `CreateHandle(UnitAnimationType, bool)` calls that lookup and returns null
  when the action is absent. The action overload creates a typed native handle.
  Neither creation method tests movement, exclusivity, `CanPlay`, or a
  `CanCreateHandle` state. No `TryPlay` method exists on the inspected manager
  contract.
* `Execute(UnitAnimationType)` calls this same `CreateHandle(type, false)` and
  executes the result only when non-null. Queue, active-action and execution
  mode handling occur after a non-null handle reaches `Execute(handle)`.
  Consequently they cannot explain the recorded null handles.
* `UnitAnimationActionHit.Type` is `UnitAnimationType.Hit` (15), and its
  `IsAdditive` getter returns true. The native `UnitHitFxManager` melee-hit path
  requests `AnimationManager.Execute(UnitAnimationType.Hit)` after its hit,
  relative-facing and manager checks. A different enum would not establish a
  native hit reaction.
* `UnitAnimationActionClip.OnStart` starts its configured clip. A handle's
  `StartInternal` catches action start exceptions and then marks `IsStarted`.
  `IsStarted` alone therefore does not prove a clip was produced. The observer
  additionally records whether `ActiveAnimation` is present.

### Native attack selection

`AttackHandInfo.GetAnimationType(out UnitAnimationType,
out UnitAnimationSpecialAttackType, out int, IEnumerable<AttackHandInfo>)`
uses the actual weapon's visual parameters. Weapons with special animations
select `SpecialAttack` and their own special enum; ordinary primary and
secondary hands select `MainHandAttack` and `OffHandAttack` respectively.

`AttackHandInfo.CreateAnimationHandleForAttack(IEnumerable<AttackHandInfo>)`
uses that selected type. It creates the exact special-action handle or applies
the real weapon animation style to an ordinary handle, and sets the native
attack count and variant. Requesting `MainHandAttack` directly omits this
selection for animal natural weapons.

`UnitAttack.CreateSingleAttack()` chooses a valid native primary, secondary or
additional hand using the actor's attack plan. The correction uses a native
`UnitAttack` solely to initialize this plan and obtain one `AttackHandInfo`.
The command is never queued or executed. Only the selected native animation
handle is executed; no attack roll, attack command, weapon damage or target
asset modification is introduced by this selection helper.

## Implemented correction

`StartRefinementAnimation(actor, MainHandAttack)` now obtains that actor's
native single-attack plan, records the selected weapon GUID, hand type,
actual animation type and special enum, checks that the selected native action
exists, and delegates handle creation to `AttackHandInfo`.

The existing call signature is unchanged and can be reused by the cloth
observer with its own actor. Other requested animation types, including Hit,
still call the exact requested native `CreateHandle(type, false)`.

Each request reads a bounded inventory from the same manager: manager identity,
animation set, whether its view is the actor's view, stopping state, requested
action availability, up to 64 native actions and up to 8 non-null clips per
action (with names and lengths). Truncation and diagnostic errors are explicit.
It also records post-execution handle state and actual clip-playable presence.
The existing exact `animation:<requested>;handle=True/False` record is retained.

Missing Hit actions still produce a missing handle and a failed positive guard.
The observer does not substitute another action, alter a donor action set, or
turn an absent animation into a passing packet. If the new inventory establishes
that a particular native donor implements no Hit action, that is evidence for
an explicit unsupported disposition; it is not itself an accepted guard change.

## Validation and limits

This lane performed read-only scalar evidence inspection, exact installed
reflection/IL inspection and the narrow source correction. Production
compilation and a native execution of the changed observer are pending the root
integrator's batched candidate. No native animation PASS is claimed here.

The original failure record is retained. Existing still/movement evidence may
be reviewed independently with its stated framing limits. An attack or Hit
before/after comparison requires matching runs of the corrected observer.
The root's separate placement correction and cloth observer changes are
outside this helper's ownership.
