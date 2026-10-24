# Cleanup 2 Crab native attack binding

## Scope and engine evidence

The candidate 5 native packet retained a missing animation for weapon
`118fdd03e569a66459ab01a20af6811a`: ordinary native `AttackHandInfo`
requests `SpecialAttack.Claw`, but its `SpiderAnimset` contains only
`SpiderAnimset_Bite`, with clip `Spider_anim_Bite attack` (about 1.167 s).
The image sequence changing pose did not prove a native ordinary claw attack.

Installed Kingmaker 2.1.7b Assembly-CSharp SHA256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
Read-only reflection/IL inspection established:

- `AttackHandInfo.CreateAnimationHandleForAttack` 0600265A calls
  `CreateSpecialAttackHandle(requestedSpecial)`.
- `CreateSpecialAttackHandle` 0600160D calls the special `GetAction`
  overload 0600160A, then constructs a normal handle around its returned action.
- `UnitAnimationActionSpecialAttack.OnStart` 060016A1 and `GetVariant`
  060016A5 use that action's unchanged Entry/Variants/Rend/ranges and clips.
- Assigning `AnimationSet` 06001519 reinitializes the manager (06001524),
  finishing/clearing active and queued handles. This implementation never
  assigns or clones a set/action/clip.
- `UnitAttack.TryStartNextAttack` 06002682 uses `ScheduleAct(1)` when its
  handle is null. A normal handle uses native animation events instead;
  `GetAnimationDuration` 0600268E consumes clip length minus TransitionOut.
  The fix is **not timing-neutral**.

## Implementation

`CrabSummonAnimationBindingPatch.cs` supplies only a null Claw lookup on the
exact registered Giant Crab (`5965d242ffe1409d98e38f2793543aad`) and measured
spider prefab (`54e0335882f2dea4188214ea3c8c93de`). The released routine is
the primary claw plus one additional primary limb, not an equipment off-hand.
Both exact weapon identities, one live native summon marker/caster, and one
of the 25 exact registered canonical/adapter sources are required.

The candidate must have the exact live original-body receipt. A narrow shared
Pteranodon attachment addition retains two observational fields for Crab:
`OriginalVisualControllerReceipt` and `OriginalVisualDrivenReceipt`, captured
immediately after the existing material adoption. `HasLiveOriginalVisual`
requires the same owned mesh, exact renderer material references, exact
controller and its membership. It never adopts a foreign refresh by name.
Repeated attach retains the old receipt and only remains valid while those
references remain exact. No other species receives this new receipt.

The lookup borrows the exact native shared action from the fixed native spider
prefab, loaded read-only with `Prefab.Load(false)`. It never instantiates a
spider actor. Native action-list, Entry object/array, clip/range-array and base
action references/values are retained and checked on subsequent use. Changed
or unsupported input refuses. Any nonnull foreign result passes through
unchanged. Weak per-manager receipts create no Unity asset to destroy and
cannot enable a removed summon/body or a different manager/set.

`CrabSummonAnimationBindingPolicyTests.cs` supplies exact-source positives and
independent module-off, nonnull result, wrong requested type, native spider,
foreign unit/name/prefab/source, missing provenance/body/weapon, and changed
native receipt negatives. These test policy refusal, not native execution.

## Native observer integration

New partial `RuntimeTestRunner.SummonCleanup2CrabAttack.cs` exposes:

```csharp
IEnumerable<int> ReviewSummonCleanup2CrabAttack(
    ExpandedSummoningCorrectionFixture fixture, Spellbook book)
```

The integrator registers exact phase `crab-attack`, compile entries and the two
domain test methods:
`AllPublishedSourcesRequireExactScope`,
`EveryOwnershipGateRejectsIndependently`.

The observer uses the actual canonical SNA III parent/selected child/prepared
slot cast and exact positive shared actor cleanup. Its target is the existing
qualified request-owned hostile fixture with declared 100000 HP. It never
changes Crab stats, weapons, rolls, rules, attack handles, acts or animation
time. Single and full ordinary `UnitAttack` commands run through the existing
frame-synchronous fixture clock, `FirearmReviewCosts`, `Sprint26CommandTrace`
and read-only `Sprint24AttackObserver`. The older helper that sets
`Animation.IsActed` is deliberately not used.

Bounds are 3000 frames / 35 s per command and 150 s for the phase. Expected
native events are one primary claw for single, and the two exact existing
claw entities once each for full. Native natural rolls, real costs, command
states, frame/game/wall timing and current action/clip time are recorded.
Fourteen actual pedipalp transform points are measured in current view space;
both sides require positive motion during active native borrowed handles.
Three actual PNG poses per handle (up to nine total) use existing native
camera capture with exact camera/private framing restoration. No native mesh
vertices or animation curves are exported. The observer never manually
executes a pose handle, supplies a hit, retries a roll or forces an act.

Native negative controls verify the unchanged spider prefab still lacks Claw
and cannot claim our body receipt, and a nonnull result passes through without
binding. Actual module switching or foreign renderer mutation is not used as
a test; pure negative policy cases cover those refusal gates. Source data,
cost hooks, capture state, owned commands/holds, pause and waiting fields are
checked/restored; shared positive fixture cleanup retains final actor ownership.

## Limits and deferred checks

No build, game launch, Git operation or runtime claim was made by this agent.
The integrator owns compilation, focused registration/tests, guarded native
execution and actual frame review. Native hit reaction remains explicitly
unsupported; no donor Hit action exists. The borrowed pose does not establish
independent pincer-finger closure. Both pincer fingers currently bind to the
same terminal pedipalp driver.

The ordinary native command test establishes this fixture's event/cost/motion
behavior, not natural Default-mode or turn-based timing equivalence. Existing
null-handle one-second timing changes to native clip timing and must remain
visible in review.

Brachiosaurus Tail binding is blocked pending a proper tail pose or original
authoring. Mastodon Slam/Gore lookup compatibility does not demonstrate motion
of the authored Tail0_M..Tail4_M chain. This change contains no generic mapping
and cannot alias Tail.

## Review 6 fixture failure and narrow correction

Immutable native run `20261022T0139588966975Z-42bd8aa414854a4cb813318b485f5184`
in evidence directory
`20261022T0139588796997Z-disposable-expanded-summoning-cleanup2-native-review`
passed the exact native Bite/Claw binding, unchanged native spider, nonnull
foreign-result and restoration gates. It failed before an attack command:
the selected 2.39999962 m gap exceeded the real native `ApproachRadius` 2.2096.
Enemy and LOS were true. The failed record is retained; it supplies no ordinary
attack, motion, action-cost or timing acceptance.

Only the main observer and this note change for the correction. The production
binding, policy, shared lane helper and frozen source 6 are untouched.
`Cleanup2CrabAttackLane` reads an actual ordinary native probe's approach radius
and tests gaps radius minus 0.15/0.30/0.50 m, excluding any gap below the summed
native corpulence plus 5 cm. It searches at most 121 origin offsets within
15 m along each horizontal axis, the existing compass directions, 4096 target
candidates and 5 s. It uses a request-owned native RTWP graph-mask 1 constraint,
walkability and the anchor's connected area. The installed PathTo/ConstructTags
contracts establishing that query were already read from exact 2.1.7b IL.

The qualified lane requirements remain: finite native graph positions,
<=60 cm clamping, same graph/area, native geometry sight, >=3.5 m protected-party
clearance, nonoverlapping native footprints and actual mutual native LOS.
After placement the **real native** `IsUnitEnoughClose`, enemy and live hostile
state decide acceptance. A fresh final native probe repeats the pre-command
admission. There is no fixed 2.4 m floor, guessed weapon reach, range override,
stat/weapon/roll change, forced act or attack issued by the survey. The evidence
records range, footprint, gaps, counters, refusals and bounded placed examples.

This correction has been source reviewed only. The agent did not compile,
launch the game or run Git operations. The integrator must compile and obtain
new guarded native command/cost/motion evidence; earlier binding PASS does not
prove that this placement correction or subsequent attacks work.
