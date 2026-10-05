# Character race traits: corrected implementation contracts

Reconciled for the 2026-10-04 all-night mission from weekend research commit
`bc83e0044748b87a7b4e11dd3a2bc85703403b27`, against the exact Phase 2A base
`5482db429bd3c4009a031aa733a3091edfa5fe5e`. This is research and contract
correction only. None of these three traits is implemented or published here.
The weekend's blanket exactness and icon-only blocker conclusions are superseded.

## Offering mechanism

Fiery Glare, Stoic Dignity and Earthsense are character race traits, not alternate
racial replacements or racial feats. The eventual faithful offering mechanism
is conditional late publication into Favored Class's `racial_traits` selection.
Use `FavoredClassTraitResolver` and the atomic, idempotent, conflict-rejecting
`HelpfulPublicationTransaction` precedent. Require the compatible installed FC
host, its `enable_traits` option, the Elemental Races module, native
`PrerequisiteRace` referencing the exact KMG race, and duplicate prevention via
`PrerequisiteNoFeature(self)`. Missing FC leaves no acquisition path.

Do not assign an alternate-trait replacement slot, spend a feat slot, or grant
these automatically to a race. No production publication is authorized by this
mission. Existing alternate traits use three explicit replacement slots; these
character traits have no printed replacement slot.

## Fiery Glare: ADAPTED — native success-only take-10 behavior

The source rule allows choosing to take 10 on Intimidate even in combat
(Bastards of Golarion, p. 29). The weekend inspected the installed engine's
`Kingmaker.Designers.Mechanics.Facts.Take10ForSuccess`, its `Skill` field, and
`RuleSkillCheck.OnTrigger`. The native behavior is narrower than literal player
choice: when `10 + StatValue + Bonus >= DC`, use 10; otherwise roll normally.
A tabletop player can choose 10 even when that fails. Thus this implementation
must never be classified EXACT.

The eventual implementation contract is:

- An optional toggle, off by default, affects only `StatType.CheckIntimidate`
  (`0x67`). Bluff, Diplomacy, general Persuasion and other checks are untouched.
- While enabled, use 10 when it succeeds; roll normally when 10 would fail.
- This works during combat and is player-favorable relative to literal tabletop
  behavior. Acceptance depends on a description that states that real behavior.
- Use the existing no-action activatable/buff pattern, with no global skill patch.
  The qualified Favored Class Intimidate/Demoralize code supplies the stat precedent.

Required eventual description:

> While Fiery Glare is enabled, Intimidate checks use a result of 10 whenever that would succeed; otherwise, they are rolled normally. This functions even during combat.

Before publication, qualify the optional toggle, both branches of the DC test,
combat and dialogue Intimidate, unaffected checks, and the final resolved text.
Original icon authoring and visible publication review remain future work.

## Stoic Dignity: same-effect correlation is mandatory

The source rule grants a conscious holder +1 trait to saves against
mind-affecting effects not already suffered; allies within 10 feet receive +1
morale under the same restriction (Bastards of Golarion, p. 29).

Installed/project precedents retained from the weekend:
`AddAreaEffect.OnFactActivate/OnFactDeactivate`, a 10-foot cylinder with an
ally predicate as used by Magic Circle, `UnitState.IsConscious`, the
`SpellDescriptor.MindAffecting` context/parent-context/source-ability walk,
and native `ModifierDescriptor.Trait` / `Morale` stacking. A rule-local
once-only guard must prevent repeated rule delivery from double-granting.
The aura must exclude its own holder and recheck the holder's consciousness
at save time, independently of the area's next update.

The weekend predicate "holder has any active mind-affecting buff" is rejected.
Being charmed must not suppress protection against a new fear effect.
Implementation must identify the incoming effect by the most exact stable
source available: ability, buff, fact, parent context, or exact source lineage.
Suppress the bonus only when that same incoming effect or exact lineage is
already affecting the target. An unrelated mind-affecting condition must not
suppress it. If exact correlation cannot prove the incoming effect is already
present, grant the bonus. Do not cleanse, remove, shorten or immunize against
any existing effect. Proving this correlation is a mandatory implementation
prerequisite, not an optional refinement or a claim already established by the
weekend descriptor scan.

| Existing condition | Incoming effect | Expected bonus |
|---|---|---|
| none | Charm A | yes |
| Charm A | another save against Charm A | no |
| Charm A | Fear B | yes |
| Fear B | Charm A | yes |
| any | holder unconscious | no |
| two nearby Stoic Dignity allies | new effect | one +1 morale bonus |
| holder saving | new effect | +1 trait only; no own morale aura |

Future tests must cover exact source correlation, uncorrelated fallback,
consciousness, radius/ally boundaries, self-exclusion, descriptor stacking,
repeated events and cleanup. Original art alone does not resolve the mechanics
prerequisite. No implementation is authorized in this mission.

## Earthsense: OMITTED-NO-FAITHFUL-ENGINE-CARRIER

The source rule is a swift action for tremorsense 60 feet until the next turn,
with daily uses scaling at levels 1, 5, 10, 15 and 20 (Blood of the Elements,
p. 9). The weekend's installed metadata scan found no `Tremor*` carrier.
That supports a current-engine omission, not proof of future impossibility.

The statement "there is no distance-limited sense" is incorrect. Native and
project Blindsense and Blindsight precedents exist. Phase 2A's
`KMG.Summoning.Natural.DireBat.Blindsense` uses the native imprecise 40-foot
component; `ExpandedSummoningNaturalProfiles` also records Blindsight for other
creatures. These distinct mechanics establish neither grounded-only detection
nor detection through earth or obstacles, tremorsense's line-of-sight behavior,
nor exclusion of flying/non-grounded creatures. Substitution would materially
change the rule and is forbidden by this mission.

Action/resource/duration patterns are available, but they do not supply the
missing sense. Do not implement an approximation. A future narrowly designed
subsystem would require its own exact engine evidence and owner scope.

## Art and evidence boundaries

The icon guide and reference catalog remain authoritative. Fiery Glare and
Stoic Dignity need later original paintings in their approved magical/racial
families (ember-lit eye; dignified stone face with protective radiance). No
procedural substitute, monogram or unrelated donor art is authorized. The
weekend session's tool-availability statement is historical, not a permanent
engine limitation. This mission authorizes neither their implementation nor
icon authoring. No new icon consumer, blueprint, selection reference, setting,
localization entry or mechanic for these three traits is created by this research.
