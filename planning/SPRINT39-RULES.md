# Sprint 39 rules — Nalfeshnee

The design contract. Printed rule on the left, what the installed Kingmaker
2.1.7b engine actually provides in the middle, and the selected adaptation on
the right. Every engine claim here was read from the private reference assembly
by reflection and `ildasm`, or from the loaded blueprint library by the one
bounded observation, and each says which.

Four authorities are kept apart throughout: the **printed PF1 rule**, the
**measured installed behaviour**, the **proposed adaptation**, and **owner
decisions**. Nothing is promoted from one to another silently.

Sprint40 carry-forward,2026-11-05: Nalfeshnee-owned Slow now derives the
printed close range,25+5 feet per two caster levels:55 feet atCL12. The
released .177 implementation's220-foot medium range remains a historical
observation, without established owner acceptance. The global spell and all
other Slow users retain their own contracts. Actual native inside/outside
command proof is being added in Sprint40; no whole S39 replay is selected.

## What was settled offline, with no guarded transaction spent

All of this comes from
`private/extracted-references/KingmakerGunslinger-private-build-references/Managed/Assembly-CSharp.dll`
by ReflectionOnly load plus `ildasm`, on 2026-11-04.

### 1. The engine has a real delayed-delivery mechanism, and it cannot cross a save

`Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverDelay` extends
`AbilityDeliverEffect` and overrides `Deliver`. Its coroutine captures
`Game.Instance.TimeController.GameTime`, yields `null` while
`GameTime - start < DelaySeconds`, and then yields **exactly one**
`AbilityDeliveryTarget(context.MainTarget)`.

Two consequences, both load-bearing for Unholy Nimbus:

* **Game time is the correct clock for a one-round delay in both combat
  modes.** It advances in real-time play and by one round duration per
  turn-based round, which is how every buff duration in this game is already
  measured. No mode test is needed and none is written.
* **A windup in flight cannot be saved.**
  `Kingmaker.Controllers.AbilityExecutionProcess` is a bare `System.Object`
  with no interfaces and no serialization attributes, and its whole pending
  state is a compiler-generated
  `IEnumerator<AbilityDeliveryTarget> m_Process`.
  `AbilityExecutionController` holds those processes in a plain
  `List<AbilityExecutionProcess>`. So a native-delay windup is **lost** on a
  cold load — not duplicated, but lost.

### 2. Neither a feature nor a buff receives `ITickEachRound`

Sprint 38 found that a component carried by a feature never receives
`ITickEachRound`. The reason generalises further than Sprint 38 stated it.

Both round dispatchers call **only** the `Logic` collection:

* `Kingmaker.Controllers.Units.UnitTicksController.TickNextRound` →
  `unit.Logic.CallFactComponents<ITickEachRound>(…)`, additionally gated on
  `IsInTurnBasedCombat()` being false and on `unit.IsInCombat`, with
  `TimeToNextRoundTick` decremented by `GameDeltaTime`;
* `TurnBased.Controllers.TurnController.Prepare` →
  `unit.Logic.CallFactComponents<ITickEachRound>(…)`.

`Kingmaker.EntitySystem.Entities.UnitLogicCollection` and
`Kingmaker.UnitLogic.Buffs.BuffCollection` are **separate sibling
collections**, both deriving from `OwnedFactCollection<UnitDescriptor>`. So
hanging a per-round callback on a buff would have failed the same way hanging
one on a feature did. `AddFactContextActions` implements `ITickEachRound` and
exposes a `NewRound` action list, which makes this an easy trap: the component
has the hook, but a buff carrier never receives the call.

**Consequence: the Nimbus windup keys off buff *expiry*, not a per-round
tick.**

### 3. Buff duration state *is* serialized, with an absolute deadline

`Kingmaker.UnitLogic.Buffs.Buff.m_EndTime` carries `[JsonProperty]`, and so
does its `m_Context`. `BuffCollection` owns `m_NextEvent`, `UpdateNextEvent`,
`PostLoad`, `m_EndTimeOverride` and `m_IsOwnerAlive`, so expiry is
event-driven and re-armed after a load.

Because `m_EndTime` is an **absolute** game time, the remaining windup is
preserved exactly across a cold load and the burst then fires once, at the
right moment. That is genuine exactly-once continuation, and it is strictly
better than the "pending work is cleaned up on load" behaviour the native
delay would have forced.

### 4. `AddFactContextActions.Deactivated` fires on *every* deactivation

Its fields are `Activated`, `Deactivated` and `NewRound`, with
`OnFactActivate`/`OnFactDeactivate` hooks. `OnFactDeactivate` runs for natural
expiry, dispel, manual removal **and** owner death alike —
`BuffCollection.OnFactRemoved` calls `Buff.OnRemove()` in all of those cases,
and `BuffCollection` tracks `m_IsOwnerAlive` and checks `UnitState.IsDead`.

**Consequence: the burst must be guarded.** Detonating on any deactivation
would make a dispelled windup explode and would let a destroyed summon's
pending work fire — both forbidden. The guard distinguishes genuine expiry
(game time has reached the recorded deadline) from cancellation, and
additionally requires a live, non-destroyed, exact-source owner.

### 5. `UnholyGrace` covers two of the aura's branches, not five

> **Corrected by the live observation. `UnholyGrace` is not the aura's
> carrier at all** - it is a standalone feature, and `UnholyAuraBuff` carries
> every printed branch. See *Three corrections to the offline analysis* below.
> The component reading here is accurate about `UnholyGrace` itself; the
> inference that it was therefore the aura's implementation was wrong, and it
> is retained to show how the name misled the offline pass.

`Kingmaker.UnitLogic.FactLogic.UnholyGrace` carries exactly two fields:
`m_AcModifier` and `m_ReflexModifier`. It does **not** supply the Fortitude
and Will resistance bonuses, the conditional spell resistance, the
alignment-related protection, or the Strength-damage response.

The other branches do have native support:

* `Kingmaker.UnitLogic.FactLogic.SpellResistanceAgainstAlignment` takes a
  `ContextValue` and an `AlignmentComponent` — exactly the aura's conditional
  resistance against good spells and good casters;
* `Kingmaker.Enums.ModifierDescriptor` has `Deflection = 8` and
  `Resistance = 22` (and `NaturalArmor = 16`), so the printed bonus **types**
  are expressible and will not stack with an equivalent source;
* `Kingmaker.UnitLogic.Mechanics.Components.AddTargetAttackRollTrigger` is the
  "when this unit is attacked" hook the Strength-damage response needs.

### 6. Feeblemind's prohibition has a native component; its score change does not

> **Refined by the live observation.** A native feeblemind graph does ship,
> and it already implements the printed arcane term as a conditional DC
> increase. What it omits is the casting prohibition. See below.

* `Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting` exists and carries a
  `ForbidMagicItems` flag — which covers both of the printed spell's
  prohibitions, and does so for *all* spellcasting rather than guessing at
  arcane-versus-divine. (The printed **save penalty** is the only arcane-only
  term, and it is a penalty on the target's save, not a bonus to the DC.)
* There is **no** `ContextActionDealStatDamage`; only
  `Kingmaker.RuleSystem.Rules.RuleDealStatDamage`. Healing stat damage *does*
  have a native action, `ContextActionHealStatDamage` with a
  `StatDamageHealType`.
* There is no "set an ability score to a fixed value" component at all.

### 7. Greater Dispel Magic: the native action offers one, or all, and nothing between

`Kingmaker.UnitLogic.Mechanics.Actions.ContextActionDispelMagic.RunAction`,
disassembled:

* with a **point** target it iterates area effects whose shape contains the
  point, calling `TryDispelAreaEffect`;
* with a **unit** target it takes `Target.Unit.Buffs.Enumerable.ToList()`,
  **sorts** it, then walks it by index calling `TryDispelBuff`;
* after each successful removal it checks `m_StopAfterFirstRemoved` and
  returns if set, otherwise continues to `Count`.

`TryDispelBuff`'s eligibility chain, which any adaptation must preserve:
`Target.Unit` non-null → a caster is required when `m_MaxSpellLevel` is set →
`!buff.IsNotDispelable` → the `m_BuffType` filter (`All`, `FromSpells`,
`NotFromSpells`) against `buff.IsFromSpell` → `buff.Context.SpellLevel` within
`m_MaxSpellLevel` → `CheckSchoolsAndDescriptor(buff.Context)` → then a real
`RuleDispelMagic` with `m_CheckType` and `m_Skill`.

The component's complete field set is `m_StopAfterFirstRemoved`,
`m_BuffType`, `m_MaxSpellLevel`, `m_UseMaxCasterLevel`, `m_MaxCasterLevel`,
`m_CheckType`, `m_Skill`, `Schools`, `Descriptor`, `OnSuccess`, `OnFail`.
**There is no success-count field of any kind.**

So the native action expresses exactly two behaviours: remove **one** eligible
effect, or remove **every** eligible effect. The printed targeted greater form
removes **one for every four caster levels**, which is three at caster level
12. Neither native setting is that number. This is an enumerated native
limitation, recorded before implementation rather than discovered during
qualification.

## What the one bounded observation returned

Run `20261104T0415356825476Z-observe-expanded-summoning-sprint39-native-inputs`,
status **PASS**, against development candidate `e531ea954b9d` (package
`b1b94bca39ff388ccb86d7fb2b58d17b46e6ffaf385b7e75f3c751afa09242df`, DLL
`d0711d8c3a4d9d08bd9cf6084742bf258a8fb1a4edb729fb4064a7d55fb74019`, MVID
`9460a8c3-b1e6-48a6-9f3d-644ec83f25ed`). Research only: no actor created, no
save loaded. The live mod tree was
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` before the
run and restored to exactly that afterwards.

A name search establishes its own result and not a universal absence. These are
the identities it named.

| Input | Result |
|---|---|
| native Nalfeshnee unit | **none** - 0 units, and the name query returned 0 matches of any type |
| `Feeblemind` | `444eed6e26f773a40ab6e4d160c67faa` (ability) |
| `FeeblemindBuff` | `8b3b4c225fe0fb046bfa8881c3ddad0d` |
| `UnholyAura` | `47f9cb1c367a5e4489cfa32fce290f86` (ability) |
| `UnholyAuraBuff` | `9eda82a1f78558747a03c17e0e9a1a68` |
| `DispelMagicGreater` | `f0f761b808dc4b149b08eaf44b99f633` (variant parent) |
| `DispelMagicGreaterTarget` | `6d490c80598f1d34bb277735b52d52c1` |
| `DispelMagicGreaterArea` | `b9be852b03568064b8d2275a6cf9e2de` |
| `CallLightning` | `2a9ef0e0b5822a24d88b16673a267456` |
| `Slow` | `f492622e473d34747806bdb39356eb89` |
| `Heal` | `ff8f1534f66559c478448723e16b6624` |
| `Restoration` | `f2115ac1148256b4ba20788f7e966830` |
| `BreakEnchantment` | `7792da00c85b9e042a0fdfc2b66ec9a8` |
| `ImprovedCritical` | `f4201c85a991369408740c6888362e20`, a `BlueprintParametrizedFeature` |
| `UnholyGrace` carriers | exactly **one**, the standalone feature `2334cca051360914d970839e81b1f7d9` |
| `SpellResistanceAgainstAlignment` carriers | 6, including `UnholyAuraBuff`, `HolyAuraBuff`, `CloakOfChaosBuff`, `ShieldOfLawBuff` |
| `ForbidSpellCasting` carriers | 10, all rage/transformation/skald buffs - **`FeeblemindBuff` is not among them** |

### Three corrections to the offline analysis

**`UnholyGrace` is not the aura's carrier.** The offline reading inferred from
the component's name that it implemented the aura and therefore covered only
two branches. The live library disagrees: `UnholyGrace` is a standalone
`BlueprintFeature` carrying only that component, and the actual aura consumer
is `UnholyAuraBuff`, which carries **all** of the printed branches:

* `AddStatBonus` Descriptor `Deflection`, Stat `AC`, Value 4;
* `BuffAllSavesBonus` Descriptor `Resistance`, Value 4 - all three saves, not
  Reflex alone;
* `BuffDescriptorImmunity` descriptor 16 and `SpellImmunityToSpellDescriptor`
  descriptor 16, which is the printed protection from mental control;
* `AddTargetAttackRollTrigger` with `OnlyHit` and `OnlyMelee`, whose actions on
  the attacker are conditional on `ContextConditionAlignment Good` (or the
  evil-domain buff) and then take a **Fortitude** saving throw - the printed
  Strength-damage response to a good melee attacker;
* `SpellResistanceAgainstAlignment` Value 25, Alignment `Good` - the printed
  conditional resistance.

So the selected mechanism is **clone `UnholyAuraBuff`**, not assemble the
branches from `UnholyGrace` plus separate stat bonuses. Simpler, exactly the
native consumer, and the amounts match the derivation: racial AC 25 + 4 = the
printed 29, and racial Fort/Ref/Will 18/5/17 + 4 = the printed 22/9/21.

**Greater dispel's cap is confirmed absent, from the shipped configuration
rather than from the component's field list.** The two targeted graphs differ in
exactly one value:

| Blueprint | `m_StopAfterFirstRemoved` | Effect |
|---|---|---|
| `DispelMagicTarget` | `True` | removes exactly one effect |
| `DispelMagicGreaterTarget` | `False` | removes **every** eligible effect |

Both use `m_BuffType = FromSpells`, `m_CheckType = CasterLevel`, no school or
descriptor filter and no maximum spell level. So the installed greater form is
**unbounded**, and the printed one-per-four-caster-levels cap is genuinely
unexpressible natively. The adaptation stands and is now evidence-backed, and
it makes the summon *weaker* than the native spell - the correct direction for
fidelity. Both the targeted and area variants ship; **no counterspell variant
exists**, which is the exact disposition rather than a blanket claim.

**Native feeblemind is faithful about the save and silent about the
prohibition.** The ability carries `ContextActionSavingThrow` Type `Will` with a
`ConditionalDCIncrease` of 4 gated on `ContextConditionTargetIsArcaneCaster` -
so the printed arcane penalty is already implemented, as a DC increase rather
than a save penalty, and the engine owns the authoritative answer to "is this
target an arcane caster" so nothing has to be inferred from Intelligence, a
class label or the presence of spell-like abilities.

But `FeeblemindBuff` carries **only** `AddStatBonusAbilityValue` Intelligence
-99 and Charisma -99, plus the descriptor. It does **not** carry
`ForbidSpellCasting`, and it is absent from that component's ten shipped
carriers. So the native effect reduces two ability scores and relies on that
alone to stop casting - which is exactly the failure mode the mission names: an
ability-score change that silently fails to restrict the relevant casting
behaviour. A Wisdom-based divine caster loses nothing it needs to cast.

The bounded correction is therefore one native component on the project's own
clone: keep the faithful Will save, the arcane DC increase, the descriptor and
the Int/Cha reduction, and **add `ForbidSpellCasting`** so the printed
prohibition actually holds. The native buff is not modified.

### One scoping decision: the constant aura's recipients

The native `UnholyAura` **ability** is the spell's normal cast -
`AbilityTargetsAround` with a 20-foot radius, `m_TargetType = Ally`, and a
caster-level duration in rounds. A printed **constant** creature aura is not
that cast, and the mission is explicit that it must not be converted into
unlimited permanent party-wide protection.

So the constant feature grants the cloned `UnholyAuraBuff` **to the Nalfeshnee
itself**, permanently, through an exact-source grant that reconstructs once on
load. The 20-foot ally extension is deliberately **not** implemented: the
printed stat block establishes only that this creature benefits from its own
aura - its AC, saves and CMD all include it - and extending +4 AC, +4 on all
saves and SR 25 against good to every party member standing near a summoned
creature would be a material power addition the entry does not grant. This is
recorded as an accurately scoped decision with disclosure, on the same footing
as Sprint 38's self-only Veil scoping, and **not** as an omission of the
feature.

### The bite's threat range

A native `ImprovedCritical` exists, but as a `BlueprintParametrizedFeature` -
the player feat, parametrised by weapon category. The printed 19-20 is
therefore delivered by the bite weapon type's own critical roll edge of 19, as
Sprint 38 delivered the pincer's, and the parametrised feat is **not** also
attached. Exactly one mechanism, never both.

## The three risks, and the selected designs

### Risk 1 — Unholy Nimbus

Printed: three times per day, free action, bursts **one round later** in a
60-foot radius, any **non-demon** creature caught in the area makes a DC 22
Will save or is **dazed for 1d10 rounds**, save DC Charisma-based.

It is not the Vrock's instant Stunning Screech, it is not a nausea aura, and
it is not the constant unholy aura.

**Selected carrier: a project-owned pending-nimbus buff on the Nalfeshnee
itself, with the burst fired from that buff's expiry.** Rationale in findings
1–4 above: the native delay has the right clock but cannot persist, no carrier
receives a per-round tick, and buff end times are serialized as absolute game
times.

| Element | Selected mechanism |
|---|---|
| activation | free-action ability, three daily uses on its own resource |
| use debit | at activation; spent uses serialize with the resource |
| pending state + deadline | the buff's own serialized `m_EndTime` |
| burst trigger | the buff's deactivation, guarded for genuine expiry |
| exactly-once | one removal per buff, plus the expiry guard |
| condition | the native daze condition, 1d10 rounds, rolled natively |
| eligibility | `SummonDemonIdentityCatalog` non-demon membership |
| DC | derived: 10 + ½ HD + Cha, never pinned |

Decided deliberately and recorded rather than left implicit:

* **Target eligibility is decided at the real burst boundary**, so a creature
  that enters or leaves during the windup is resolved against the world as it
  is when the light bursts — never against an activation snapshot.
* **Burst centre** is the Nalfeshnee's own position at burst time, since the
  printed light plays around its body and then bursts. The actual observed
  behaviour will be recorded, not asserted.
* **No refill** on cancellation, dispel, owner death or reload.
* **Sprint 32's daze immunity marker is deliberately not reused.** Sprint 32
  attaches a one-minute immunity to its Lillend daze because the *Daze
  cantrip*'s own printed rule says so. Unholy Nimbus has no such clause, so
  copying that marker would invent a restriction the creature does not have.
  The native daze **condition** buff is reused; the cantrip's immunity is not.
* **Collateral is retained** for the manual effect against eligible
  non-demons. The AI is conservative and ally-safe with a useful fallback, not
  faction-based immunity, and it does not reposition party actors. Because the
  burst is delayed, allies cannot be guaranteed to stay clear after
  activation; that difference is documented rather than presented as perfect
  safety.
* Manual activation is available from the real supported interface.

**The one question frozen before any scheduler is written:** what a second
activation does while a windup is already pending. The printed text does not
say. No unlimited stacked explosions, no invented concentration requirement
and no invented cooldown; the selected answer is recorded here before the code
exists.

### Risk 2 — Feeblemind

Printed: target creature, Will save, spell resistance applies,
**instantaneous** with lasting consequences. Intelligence and Charisma each
drop to 1; the subject cannot use Intelligence- or Charisma-based skills, cast
spells, understand language or communicate coherently, and cannot use magic
items. It remains so until *heal*, *limited wish*, *miracle* or *wish* cancels
it. A creature that can cast **arcane** spells takes a −4 penalty on its save.

Selected design, pending the bounded observation's answer on whether a native
feeblemind graph ships:

| Element | Selected mechanism |
|---|---|
| prohibition | native `ForbidSpellCasting` with `ForbidMagicItems` |
| score change | a project-owned permanent buff holding the Int/Cha reduction |
| dispel immunity | `IsNotDispelable`, since the printed spell is not dispellable |
| save penalty | −4 applied only for an actual arcane spellbook |
| attribution | the released stable victim-effect context, not a live caster reference |
| cure | native *heal*, which is one of the printed cures and does ship |

Three points of care, each one a way to get this wrong:

* **Not ability damage.** Modelling the reduction as stat damage would make
  the native per-day healing rate cure feeblemind on its own, turning a
  "until *heal*" effect into a self-expiring one. The reduction is therefore
  carried by a permanent buff, not `RuleDealStatDamage`.
* **Arcane casting is established, not inferred.** Not from a high
  Intelligence, not from a class label, and not from the presence of
  spell-like abilities — only from an actual arcane spellbook. This creature
  itself illustrates the trap: Intelligence 23 and a long spell-like list, and
  no arcane spellbook at all.
* **The lasting result survives the summon.** The printed spell is
  instantaneous, so its result is not erased merely because the Nalfeshnee
  disappears, and `RemoveBuffIfCasterIsMissing` is therefore **not** attached.
  Equally, a permanent reference to a destroyed caster is not required: the
  released stable attribution is used instead. If the project's established
  summon-lifetime contract conflicts with this, that exact issue is settled
  before testing rather than silently nerfed or silently made permanent.

Of the four printed cures, Kingmaker ships *heal*; *limited wish*, *miracle*
and *wish* are not in this game. So the cure list is the printed list
intersected with what exists, which is a disclosed narrowing rather than an
invented addition — notably, *restoration* and *break enchantment* are **not**
added, because the printed spell does not list them.

Testing uses only explicitly disposable, owned victims, with the real cure
exercised through its real consumer, and an arcane/non-arcane control plus an
eligible/immune control. No irreversible test on a protected party member or
campaign actor.

### Risk 3 — constant Unholy Aura

Printed: a constant spell-like aura, DC 23. *Unholy aura* supplies +4
deflection AC, +4 resistance on saves, spell resistance 25 against good spells
and spells cast by good creatures, alignment-related protection, and a
Strength-damage response to a good creature that strikes it in melee.

**The printed defences already include it**, so the racial values are derived
separately — see `planning/SPRINT39-STATUS.md` for the full derivation table
and `NalfeshneeRules39Policy.Validate()` for the executable version. The
blueprint carries natural armour 16 and racial saves Fort +18 / Ref +5 /
Will +17; the buff supplies the +4s that reach the printed 29 / +22 / +9 /
+21 and CMD 42.

| Branch | Selected mechanism |
|---|---|
| +4 deflection AC | native `UnholyGrace.m_AcModifier` |
| +4 resistance Reflex | native `UnholyGrace.m_ReflexModifier` |
| +4 resistance Fort/Will | `ModifierDescriptor.Resistance` stat bonuses |
| conditional SR 25 vs good | native `SpellResistanceAgainstAlignment` |
| Strength damage to good melee attackers | `AddTargetAttackRollTrigger` + a real save |
| general SR 25 | the creature's own, separate from the above |

Care points:

* **General SR 25 and the aura's conditional SR 25 are different facts** that
  print the same number for this creature, because CR 14 + 11 happens to equal
  the spell's fixed 25. They are separate named quantities with a test that
  records the coincidence as the trap it is.
* **A constant creature feature is not the spell's normal multi-target cast.**
  The intended recipient and radius behaviour is verified rather than
  converted into unlimited permanent party-wide protection.
* **Bonus types are preserved** so the aura does not stack with an equivalent
  source, and it is not reduced to an AC marker or a duplicated Magic Circle.
* **Exact-source grants**: removing one source must not remove another
  source's protection, and a constant self feature reconstructs once on load
  without accumulating bonuses.
* **The ordinary spell's focus or material entries are not a reason to drop a
  printed constant SLA.** The summon policy is applied deliberately, as
  Sprint 38 did, rather than a price being read and the feature quietly
  omitted.

### Risk D — Greater Dispel Magic

Printed targeted greater form at caster level 12: a dispel check against
active effects, removing **up to three** (one per four caster levels).

Given finding 7, the native action cannot express the cap. Selected
adaptation: a small project-owned action that preserves the native
eligibility chain and ordering exactly and delegates each individual attempt
to the engine's own `RuleDispelMagic`, counting **successful removals** and
stopping at `GreaterDispelMaxRemovals(casterLevel)`.

Rejected alternative, and why: invoking the native action three times in
sequence would also yield up to three removals and would reuse every native
decision, but each invocation re-reads and re-sorts the buff list, so an
effect whose dispel check *failed* would be retried on the next invocation.
That grants extra chances the printed rule does not, so it is not used.

The released useful-enemy-buff heuristic is reused without pretending it is a
perfect global buff taxonomy; it distinguishes at least useful-beneficial,
harmful-only and no-useful-effect targets. Normal action costs and an
attack-or-other-spell fallback apply. No existing dispeller is rewritten. If
an area variant is selected it keeps its real collateral, and safe AI does not
make it enemy-only. Whether the installed game exposes targeted, area and
counterspell forms at all is answered by the bounded observation and recorded
exactly; no new counterspell subsystem is built if none suitable exists, and
the precise disposition is surfaced instead of a blanket claim of
completeness.

## Reused at their released identities

Named here so nothing is re-derived or re-guessed, with the sprint that proved
each one present in the installed library.

| Input | Identity | From |
|---|---|---|
| native daze condition buff | `9934fedff1b14994ea90205d189c8759` | S32 |
| native Slow buff | `0bc608c3f2b548b44b7146b7530613ac` | S36 |
| native True Seeing buff | `09b4b69169304474296484c74aa12027` | S38 |
| native Dispel Magic (targeted) | `143775c49ae6b7446b805d3b2e702298` | S38 |
| native Dispel Magic | `92681f181b507b34ea87018e8f7a528a` | S31 |
| native Improved Bull Rush | `b3614622866fe7046b787a548bbd7f59` | S29 |
| native Cleave feature / action / penalty | `d809b6c4ff2aaff4fa70d712a70f7d7b` / `6447d104a2222c14d9c9b8a36e4eb242` / `914b04c279e3d3447848fecf5796cdcd` | S29 giants |
| native Iron Will | `175d1577bb6c9a04baf88eec99c66334` | released |
| native Power Attack | `9972f33f977fc724c838e59641b2fca5` | released |
| native Improved Initiative | `797f25d709f559546b29e7bcb181cc74` | released |
| native Improved Critical (claw) | `76a335b7d69691c4e8376f9379338778` | S35 |
| Awesome Blow | project-authored, Sprint 29 | S29 |
| Call Lightning + native bolt action | project-authored, Sprint 29 | S29 |

The native Improved Critical (claw) identity matters for a reason beyond
reuse: per-category Improved Critical features demonstrably ship, so a **bite**
variant plausibly does too. Whichever answer the bounded observation returns,
the printed 19–20 bite threat range is delivered **exactly once** — either by
the weapon type's own critical edge or by a native feat, never by both.

## Excluded, and by whose authority

| Ability | Authority |
|---|---|
| greater teleport (self + 50 lbs) | Sprint 39 charter row: planar travel excluded |
| summon (1/day, nalfeshnee/hezrous/vrocks) | Sprint 39 charter row: summonable demon reinforcements excluded |

Both are charter exclusions for this sprint, routed through the established
summon sanitization policy that already excludes travel and nested summoning
for every summon. Neither is a new omission, and Sprint 38's Reverse
Gravity/Veil omissions and Sprint 37's Holy Water omission authorise no
further omission here.

## Art

The body and icon are authored, not borrowed. `render_creature_icon.py` is a
procedural project-owned Blender renderer — metaballs, mesh primitives and
procedural materials, with the explicit contract that no game pixels,
downloaded model, texture or generative-model output is an input — and
Blender 4.5 is installed on this machine. A `nalfeshnee` entry is added
alongside the existing entries through `glabrezu`.

The creature is a **corpulent, boar-headed, winged demon**. A tinted Glabrezu
or Hezrou, or a generic humanoid, is not this creature and will not be passed
off as it. Functional shape, selection, movement, attacks and casts, death and
resource cleanup are required; high-resolution anatomy, facial acting, perfect
wing articulation and cosmetic seams stay in the backlog. Assets wire through
build, package, strict inventory, view attachment, donor policy and
private-resource receipt paths, reusing the **unified** Sprint 38 view-policy
predicate rather than copying a donor list into new locations. A repository
JSON test passing, or a successful donor fallback, is not proof that the
authored body was installed.
