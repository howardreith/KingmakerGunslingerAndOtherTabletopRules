# Gunslinger's Dodge native-buff forensics

## Status and evidence boundary

This is a diagnostic report, not a repair. No production behavior, blueprint
GUID, save identity, or release package was changed. The human-established
observation is accepted as authoritative: the exact installed candidate DLL
SHA-256 is `0AFE5B76952134AA07D61B5F1C101F3EF5A6F86CD3B5F4D866D172B367E9868B`;
on a new character AC changed `15 -> 17`, one Grit was spent, the intended buff
and countdown appeared, and the buff and AC bonus remained after the countdown.

The installed and qualified-reference `Assembly-CSharp.dll` files are byte
identical, SHA-256
`3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB`.
All contracts below are from that Kingmaker assembly, not Wrath.

Important limitation: this work cycle did **not** complete an authorized live
native-control run. Consequently the requested native blueprint dump and the
native-versus-Dodge runtime scheduler trace are not represented as completed
evidence, and no lifecycle divergence is claimed as proven. The source-level
boundary is proven; the live differential remains the acceptance blocker.

## Exact installed contracts

### Application

`Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff`

```text
void RunAction()
fields:
BlueprintBuff Buff
bool Permanent
bool UseDurationSeconds
ContextDurationValue DurationValue
float DurationSeconds
bool IsFromSpell
bool IsNotDispelable
bool ToCaster
bool AsChild
```

`RunAction` obtains `MechanicsContext`, calculates either
`DurationValue.Calculate(context).Seconds` or `DurationSeconds.Seconds()`, uses
`null` duration only when `Permanent`, resolves target/caster, and calls:

```text
UnitHelper.AddBuff(UnitDescriptor, BlueprintBuff, MechanicsContext, TimeSpan?)
```

After creation it assigns `SourceAreaEffectId`, `IsFromSpell`, and
`IsNotDispelable`; `AsChild` only stores the result in the parent buff.

`Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction`

```text
bool HasSavingThrow { get; }
void Apply(AbilityExecutionContext, TargetWrapper)
fields: SavingThrowType SavingThrowType; ActionList Actions
```

`Apply` optionally triggers `RuleSavingThrow`, enters the execution context's
target data scope, and runs `Actions.Run()`. It has no buff scheduler logic.

Kingmaker's apparent `UnitDescriptor.AddBuff` calls are extension methods on
`Kingmaker.UnitLogic.UnitHelper`:

```text
Buff AddBuff(UnitDescriptor, BlueprintBuff, UnitEntityData,
    TimeSpan?, AbilityParams)
Buff AddBuff(UnitDescriptor, BlueprintBuff, MechanicsContext, TimeSpan?)
```

Both are null-blueprint guards and forward to `descriptor.Buffs.AddBuff`.

### Collection creation and enrollment

```text
Buff BuffCollection.AddBuff(BlueprintBuff, UnitEntityData,
    TimeSpan?, AbilityParams)
Buff BuffCollection.AddBuff(BlueprintBuff, MechanicsContext, TimeSpan?)
private Buff BuffCollection.TriggerRuleApplyBuff(BlueprintBuff,
    MechanicsContext, TimeSpan?)
private Buff BuffCollection.AddBuffInternal(BlueprintBuff,
    MechanicsContext, TimeSpan?)
protected override Fact BuffCollection.CreateFact(BlueprintFact,
    MechanicsContext)
protected override void BuffCollection.OnFactCreated(Fact)
protected override void BuffCollection.OnFactAdded(Fact)
```

`TriggerRuleApplyBuff` constructs and triggers `RuleApplyBuff`; its delegate is
`AddBuffInternal`. `AddBuffInternal` chooses `GameTime` (or turn start in
turn-based combat), computes nullable absolute end time `now + duration`,
performs `StackingType` handling, then stores that absolute value in
`m_EndTimeOverride`, stores the blueprint in `m_AddBuffNow`, and invokes the
owned fact collection's `AddFact`.

There is no installed method named `PrepareFactForAttach`. The exact equivalent
boundary is `BuffCollection.OnFactCreated(Fact)`: it clears `m_AddBuffNow`,
copies `m_EndTimeOverride.Value` into the new buff's `EndTime`, then clears the
override. `OnFactAdded` subsequently calls `UpdateNextEvent`. Therefore a new
buff created through the current duration-aware overload receives its end time
before scheduler selection.

### Scheduler

```text
void BuffCollection.UpdateNextEvent()
void BuffCollection.Tick()
private Buff BuffCollection.m_NextEvent
```

`UpdateNextEvent` clears `m_NextEvent`, enumerates every raw buff fact, and
selects the first buff with the smallest `NextEventTime` (strict `<` comparison;
ties retain collection order).

`Tick` uses current `GameTime`, or turn start in turn-based combat. While the
selected `NextEventTime <= now`, it:

1. respects the turn-based owner/caster gate;
2. calls `TickMechanics` when `NextTickTime <= now`;
3. calls `RemoveFact(m_NextEvent)` when `EndTime <= now`;
4. calls `UpdateNextEvent` and repeats while another event is due.

The death branch first removes every non-`StayOnDeath` buff.

### Buff time fields

```text
private TimeSpan? Buff.m_EndTime
TimeSpan Buff.EndTime { get; set; }
internal TimeSpan Buff.TickTime { get; set; }
internal TimeSpan Buff.NextTickTime { get; set; }
TimeSpan Buff.TimeLeft { get; }
TimeSpan Buff.NextEventTime { get; }
bool Buff.IsPermanent { get; }
```

There is no `Buff.SetDuration` in this Kingmaker build. `EndTime` is its narrow
storage equivalent, but its setter only writes `m_EndTime`; it does **not** call
`UpdateNextEvent`. `RemoveAfterDelay(TimeSpan)` is the safe public rescheduling
operation: it writes `GameTime + delay` and explicitly calls
`Owner.Buffs.UpdateNextEvent`. `MakePermanent()` clears `m_EndTime` and also
updates the collection.

`EndTime` returns `m_EndTime` or `TimeSpan.MaxValue`. `IsPermanent` is true when
the nullable value is absent or at least `TimeSpan.MaxValue`. `TimeLeft` returns
`MaxValue` for permanent buffs, otherwise `max(EndTime - GameTime, zero)`.
`NextEventTime` is `min(EndTime, NextTickTime)`.

The runtime constructor copies `BlueprintBuff.TickTime`. That blueprint getter
returns `TimeSpan.MaxValue` when the blueprint has no `ITickEachRound`
component; otherwise it converts `BlueprintBuff.Frequency` to rounds. The
constructor initializes `NextTickTime` to `MaxValue`, or `now + TickTime` for a
periodic buff (subject to `SetBuffOnsetDelay`). Thus merely leaving `Frequency`
at its enum default does not create a zero-time tick for a non-periodic Dodge
buff.

### Removal boundary

```text
void FactCollection.RemoveFact(BlueprintFact)
void FactCollection.RemoveFact(Fact)
protected override void BuffCollection.OnFactRemoved(Fact)
void Buff.OnRemove()
void Buff.Remove()
```

`RemoveFact(Fact)` verifies membership, deactivates an active fact, removes it
from `m_Facts`, invokes `OnFactRemoved`, disposes it, then raises the collection
change boundary. BuffCollection's override raises `IUnitBuffHandler`, invokes
`Buff.OnRemove`, and refreshes `m_NextEvent`. `Buff.OnRemove` removes stored
modifiers/facts and invokes `BuffLogic.OnRemoved`. Component `OnTurnOff` occurs
during `Fact.Deactivate`, before collection removal.

## Current Dodge source compared with the native seam

`GunslingerDodgeProneAbilityLogic.Deliver` constructs a `MechanicsContext` and
calls the exact Kingmaker extension:

```text
caster.AddBuff(m_ArmorClassBuff, buffContext, TimeSpan.FromSeconds(6))
```

That reaches `m_EndTimeOverride -> OnFactCreated.EndTime -> OnFactAdded ->
UpdateNextEvent`. The earlier source comment claiming that this overload
establishes pre-attach enrollment is correct for the installed assembly.
Changing from this overload to `ContextActionApplyBuff` alone is therefore not
an assembly-supported explanation for the observed persistence: both converge
on the same collection path.

The custom buff has `IsClassFeature=true`, `Stacking=Replace`, and one
`GunslingerDodgeArmorClassBonus`; it does not set private flags, `Frequency`,
FX, or resource IDs. With no `ITickEachRound` component its tick deadline should
be `TimeSpan.MaxValue`, so its scheduled event should be its six-second
`EndTime`.

## Native blueprint precedents

No exact installed-library dump was completed in this work cycle. In
particular, GUIDs, serialized action fields, private buff flags, and complete
component lists for Total Defense, True Strike, Arcane Accuracy, and Haste were
not guessed from Wrath or community GUID lists. They remain required runtime
evidence. Total Defense remains the preferred control if present; otherwise use
True Strike or Haste and record why.

## Live differential observation

The human Dodge observation proves application, resource consumption,
presentation, bounded `EndTime` display, and continued collection/effect after
the boundary. It does not expose `m_NextEvent`, `NextTickTime`, calls to `Tick`,
`RemoveFact`, or `OnTurnOff`.

No view-backed native-control trace was completed here. The first proven
runtime divergence therefore remains **undetermined**. The next probe must be
request-scoped, use `KMG_AUTOMATION_WORKING` through Steam App ID 640820, and
instrument only the selected native runtime buff and the exact Dodge runtime
buff. It must sample creation and post-boundary state and count the exact
collection/removal callbacks requested in the assignment. A detached chargen
fixture is not acceptable timing evidence.

### Guarded live attempts on 2026-08-05

An exact-blueprint, request-scoped lifecycle probe was added without changing
the Dodge application code. It patches `AddBuffInternal`, `UpdateNextEvent`,
`Tick`, `FactCollection.RemoveFact`, and `Buff.Dispose`, but every hook is a
no-op unless the probe is active and the buff name is exactly
`KMG_GunslingerDodge_ArmorClass_Buff`. The Dodge AC component records
`OnTurnOn` and `OnTurnOff`. Each event includes runtime reference identity,
game time, `EndTime`, `TimeLeft`, internal `NextTickTime`, `NextEventTime`,
permanence, active/disposed state, rank, and exact-blueprint collection count.

Three guarded Steam App ID 640820 launches used only
`KMG_AUTOMATION_WORKING`. The first failed closed at request validation before
loading because the new scenario was missing from the request parser's
working-save timeout group. After correcting the harness, two launches proved
the exact save load, stable fingerprint, no save-writing API, and game-thread
execution. Both then failed before lifecycle creation: native
`UnitDescriptor.AddBuff(BlueprintBuff, MechanicsContext, TimeSpan?)` returned
null for the main character, and then for every live controllable party unit.
The second attempt used the production-associated Dodge ability in the
`MechanicsContext`. No `AddBuffInternal` event occurred.

Evidence directories:

- `20260805T1653262569106Z-dodge-buff-instance-forensics`
- `20260805T1655492072095Z-dodge-buff-instance-forensics`
- `20260805T1659160763958Z-dodge-buff-instance-forensics`

This is a fixture/application rejection, not expiration evidence. It proves
neither case A nor case B. Specifically, there was no original runtime Buff
instance in these runs whose removal or replacement could be observed. Under
the explicit stop condition, no repair may be implemented or selected until a
live scenario that actually creates the Dodge buff proves one of:

- A: the same runtime reference remains after zero; or
- B: the original reference is removed/disposed and another same-blueprint
  reference remains or is created.

The decisive cases are:

| First differing observation | Meaning |
| --- | --- |
| Dodge `EndTime`/`IsPermanent` differs at creation | application/attach state divergence |
| Dodge `NextEventTime` differs with equal `EndTime` | tick/component initialization divergence |
| `UpdateNextEvent` does not select the earliest Dodge event | collection/order corruption or another overdue event |
| `Tick` is not called on the live owner's collection | owner/controller enrollment divergence |
| `Tick` reaches Dodge but does not call `RemoveFact` after `EndTime` | turn-based gate or time-base divergence |
| `RemoveFact` occurs but `OnTurnOff` does not | fact deactivation/component lifecycle divergence |

## Public Kingmaker precedents

Call of the Wild was inspected as MIT-licensed source precedent only; it was
not added as a binary, build, runtime, or `Info.json` dependency. Exact source
pin: Holic75/KingmakerRebalance commit
`1332fb0db844b7863f484ca978bea2349fe49769` (HEAD inspected 2026-08-05),
`CallOfTheWild/Common.cs`, helper
`createContextActionApplyBuff(BlueprintBuff, ContextDurationValue, bool,
bool, bool, bool, int)`. The helper directly creates Kingmaker's
`ContextActionApplyBuff` and assigns:

```text
IsFromSpell = is_from_spell
Buff = buff
Permanent = is_permanent
DurationValue = duration
IsNotDispelable = !dispellable
UseDurationSeconds = duration_seconds > 0
DurationSeconds = duration_seconds
AsChild = is_child
ToCaster = false
```

Its `createContextActionApplyBuffToCaster` variant assigns the same fields and
sets `ToCaster=true`. Representative finite-duration sites at that same commit:

- `CallOfTheWild/Classes/Hunter.cs`: Distracting Attack applies its custom
  buff with `duration_seconds: 6`; Tangling Attack applies native
  `f7f6330726121cf4b90a6086b05d2e38` with `duration_seconds: 6`; Aiding Attack
  uses `duration_seconds: 9`.
- `CallOfTheWild/BalanceFixes.cs`: a caster cooldown uses
  `dispellable:false, duration_seconds:6`.

These are Kingmaker precedents for constructing the native action with all
duration flags coherently. They do not prove that changing action graphs fixes
this issue because the installed assembly shows both routes converge on the
same duration-aware `BuffCollection` path.

Kingmaker Turn-Based Mod is also specifically Kingmaker code. Its documented
model treats six seconds as one round and includes fixes involving summon and
effect duration, reinforcing the need to record real-time versus turn-start
time in the live probe. Neither public project is authority for the installed
2.1.7b contracts.

Source links: [MIT license](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/LICENSE),
[helper](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/Common.cs),
[Hunter finite call sites](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/Classes/Hunter.cs), and
[BalanceFixes finite call site](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/BalanceFixes.cs).

## Native design decision withheld

The narrowest repair, fields to copy, fields not to copy, retention/removal of
`GunslingerDodgeProneAbilityLogic`, and cloning versus direct construction are
all deliberately **undecided**. Selecting any of them before proving A versus
B would violate the diagnostic stop condition. If a later trace selects a
native action graph, it must directly construct Kingmaker's own
`ContextActionApplyBuff` (or use an equivalent minimal internal helper); it
must not reference `CallOfTheWild.dll` or add Call of the Wild to `Info.json`.

## Acceptance criteria remaining

Source evidence is complete only after the exact native blueprint/component
dumps and pinned public repository commit/file references are appended.
Runtime evidence is complete only after a live native control expires and the
Dodge trace identifies the first differing field/callback with exact timestamps
and identities. Human acceptance then repeats the new-character scenario and
confirms Grit `-1`, AC `+2` during the effect, icon/countdown presentation, buff
removal, and AC restoration after one round, with built/package/live DLL hashes
matching. An ambiguous result is failure.

## Opt-in manual lifecycle trace

The diagnostic package now includes a passive manual observer gated by the
`dodge-forensics.enabled` marker in the installed mod directory. With no marker,
it creates no JSONL file, attaches no sampler, and every lifecycle callback
returns without recording. With the marker present, it observes the player's
actual activation of `KMG_GunslingerDodge_ProneAbility` and the exact Dodge Buff
(`bbd7d42117cc4c23b3e22af3a71621d9`,
`KMG_GunslingerDodge_ArmorClass_Buff`). It never applies or removes a fact and
does not alter scheduler, AC, Grit, command, or save state.

No native control is required for the next trace. Lifecycle records and the
read-only quarter-second sampler are written as immediately flushed JSON Lines beneath
`Application.persistentDataPath\KingmakerGunslinger\Diagnostics`. The trace is
diagnostic evidence only; its build and source tests do not establish which
expiration case occurs.

### JSONL evidence-boundary repair

The first manual trace from diagnostic commit `fa5c7a5` was invalid: its 6,816
physical lines contained one complete enable record followed by 6,815
`{"$id":"1"}` reference stubs. The DTO was newly constructed for every event;
record reuse was not the cause. The writer used the two-argument
`JsonConvert.SerializeObject` overload, which inherited Kingmaker's process-wide
Newtonsoft reference-preservation settings and persistent resolver state.

The writer now supplies explicit per-record settings:
`PreserveReferencesHandling.None`, `ReferenceLoopHandling.Error`, and
`NullValueHandling.Include`. Each snapshot is flattened to scalars before it is
serialized under the sequence/write lock. A write failure logs once, disables
forensics for that launch, and does not escape into gameplay or bootstrap.

Total Defense is no longer a required native control. Kingmaker does not expose
a usable Total Defense action through normal gameplay, and Fighting Defensively
is not an equivalent finite-duration effect. The collector validates a
Dodge-only trace and refuses reference metadata, malformed or incomplete
records, invalid sequences, missing actual Dodge delivery/application, or a
missing post-EndTime sample before creating an evidence ZIP.
