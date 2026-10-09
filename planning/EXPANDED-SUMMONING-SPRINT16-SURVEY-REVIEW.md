# Sprint 16 narrow survey review, 2026-10-05

Research only: **7/7 assertions PASS**, not full Sprint 16 qualification.
Source `8250ac8f8369699af7825459bc860682e3e5eae5`; guarded Steam run
`20261005T1254571775971Z-disposable-expanded-summoning-crocodilians`.
The complete wrapper, 2013 domain tests, exact-reference clean Release and
strict package checks passed before launch. The exact working save was loaded;
no save write was requested. Exact fixture cleanup passed. The process exited.

## Accepted engine limitation

`OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`

The owner's activation condition is satisfied. The live census enumerated
9,299 native assembly types and 109,284 loaded blueprints. Assembly-CSharp MVID:
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`. Eight type-name matches were inspected:
the swallow controller and its compiler helper; `ContextActionSwallowWhole`;
`ContextActionForEachSwallowedUnit`; `UnitPartSwallowWhole` and its compiler
helper; `UnitPartSwallowed`; and `Kingmaker.View.SwallowWholeSettings`.

The offline note incorrectly said `SwallowWholeSettings` did not exist. It
does exist, but has only `Head` (Transform) and `SpitOutAnimation`. It is a view
carrier, not an attackable interior. The actual swallowed part has only
`Swallower`, `m_Buff`, `ShouldBeFree`, `SecondsToBreakFreeAttempt`, and the
`Init`, `OnRemove`, `TryToBreakFree` methods. The swallower part tracks swallowed
unit references, animation and release/death/destruction callbacks. None of
these types or the live swallowed graphs provides an interior target, AC,
cumulative HP or cut-free-by-damage path. The additional iteration action
contains an action list, not an interior damage accumulator.

This applies to Dire Crocodile and the existing Purple Worm, without reopening
the Worm's accepted mechanics. AC 16 and 13 HP remain printed Dire contract
numbers, **not implemented mechanics**. No alternative save/check substitutes
for them and no custom interior subsystem is authorized. Do not mark this
accepted limitation BLOCKED or withhold Dire solely for it after other gates
pass. Size legality, successful initial grab/later maintain, initial bite,
later-round damage, native escape timing/checks, source ownership and all
cleanup boundaries remain mandatory and unwaived.

## Live swallowed graph audit

Dire's own buff is `327f8a0108ae4e7eb50ec7793e67f92c`. Its complete component
list is exactly:

- `AddStatBonus`: AdditionalCMB -2, UntypedStackable, no BAB scaling.
- `AddStatBonus`: Dexterity -4, UntypedStackable, no BAB scaling.
- `AddFactContextActions`: Activated empty; Deactivated empty; NewRound one
  direct `ContextActionDealDamage`, Physical/Bludgeoning, simple 3d6+13,
  IgnoreCritical true, no half/save, drain or AoE. No nested action, conditional
  or save lists and no second damage action exist.

The generic graph printer displays the shared empty Deactivated array as a
previously seen `GameAction[]`; the direct live assertion independently checks
its length is zero. The inactive `Energy=Fire` field is a default union member,
not energy damage: the active damage type is Physical. There is no inherited
Purple Worm name, acid or 4d8+12 damage action in Dire's state.

Native `PurpleWormSwallowed` and KMG Purple Worm retain one 4d8+12 bludgeoning
NewRound action and the same two native penalties. KMG Flytrap retains its
1d8+7 bludgeoning and 2d6 acid NewRound actions plus those penalties. All three
graphs have empty activation/deactivation. This audit proves graph separation,
**not** application-frame/later-round damage cadence or live escape behavior.
Those are still required in the hidden candidate.

## Exact land skills

| Creature / skill | Ranks | Class | Ability | Size | Skill Focus | Live total |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Crocodile Perception | 1 | 3 | Wis 12: +1 | 0 | 3 | +8 |
| Crocodile Stealth | 2 | 3 | Dex 12: +1 | -4 | 3 | +5 |
| Dire Perception | 6 | 3 | Wis 14: +2 | 0 | 3 | +14 |
| Dire Stealth | 6 | 3 | Dex 10: +0 | -12 | 3 | +0 |

Both have zero Mobility ranks. Crocodile's Mobility total +1 is its Dexterity,
not an invented rank; Dire's is zero. Swim and water-only Stealth are not applied
on land. Hold Breath remains omitted without a swimming/drowning consumer;
low-light vision stays under `PASSIVE_CREATURE_SENSES_UNMODELED`.

The Run/Sprint fact-name census returned 112 matches, mostly Rune/Overrun or
unrelated substring matches. The only movement-named native candidates were
empty Puja wolf Sprint buffs and a scripted citizen run-away state; the
third-party Rice Runner trait contains stat/class-skill components. No exact
meaningful tabletop Run fact was found. Dire's Run is an explicit engine
omission; no general running subsystem is introduced.

## Art and AI research boundaries

The Monitor Lizard capture has one skinned renderer and 41 complete bind frames,
root `cent_spine1_jnt`, Y-up / -Z-forward. Only private bind metadata was
captured; no native vertices, texture or animation data. Original geometry,
UVs and paintings remain to be authored and qualified.

No `IsEngagedConsideration` instance was found among library-indexed blueprints.
This does not prove none exists inline on a native AI action, nor qualify Sprint
AI. Audit those references or use an explicitly owned native consideration;
real movement/bite/tail and both combat-mode gates remain open.

## Exact evidence

All raw evidence remains machine-local under `runtime-evidence`.

- Package SHA-256: `9a762b2881880eb03ddbb02501f6a9a0aa5997bfe2957a3956781155e16cdef9`.
- DLL SHA-256: `fe45b0beb90ec3eb97f456138ef02b58ad9335809eb2a86911fd224a86ffbf0f`.
- Result SHA-256: `1A04BC4A554172ABB1FC86E34153D00C1BB1F756A4909FE8C6DDF80BD5E20917`.
- Census SHA-256: `9A20AC89EA8F2B330FD050525C84786B94CE07998888D2EB4FA63E06ED9A120C`.
- Private bind-capture SHA-256: `D7C91E3E45384E37D3F906BD48E2F318ACBFE0CCB30607C8970665D27A08C4EA`.
- Restoration `20261005T1258077246783Z-disposable-expanded-summoning-crocodilians.json`:
  SHA-256 `9FFEFA15CD347D2EB3B5754C2F12F2167EF9BB3D1B8A3EDD53A2CD3B5991D229`, verified.
- Exact before/after live tree: 136 files, Info 0.0.117,
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

`HumanReview: NOT_PERFORMED_NONBLOCKING`.
