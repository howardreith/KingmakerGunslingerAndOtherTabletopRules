# Sprint 50 entry criteria — Bleeding Wound

## Authority and adaptation

At Gunslinger level 11, after a firearm hit against a living creature, the
gunslinger may spend one grit to add recurring hit-point bleed equal to her
Dexterity modifier, or two grit to add one point of Strength, Dexterity, or
Constitution bleed. Sneak-attack-immune creatures are immune.

Kingmaker cannot pause a resolved attack for this four-way post-hit choice.
Sprint 50 therefore exposes four personal free-action selections that arm the
next exact firearm attack. The marker is consumed by that attack whether it hits
or misses; grit is spent only after an eligible hit. Arming one choice replaces
any other Bleeding Wound choice. Non-firearm actions do not consume it.

## Native delivery

- A qualifying hit retains its ordinary firearm damage.
- Hit-point bleed deals the positive Dexterity modifier once per round. A zero
  or negative modifier produces zero additional damage and spends no grit.
- Ability-score choices deal exactly one native `RuleDealStatDamage` point per
  round to the selected attribute.
- Every applied fact carries native `SpellDescriptor.Bleed`, uses replacement
  stacking for its own kind, and remains removable by native healing/bleed
  removal, including Utility Shot — Stop Bleeding.
- Bleed facts do not carry an arbitrary duration; they persist until removed.

## Gates and atomicity

The rider fails closed for a miss, non-exact firearm, ineligible firearm attack,
nonliving target, sneak-attack immunity, insufficient grit, or nonpositive HP
bleed amount. Rejections do not spend grit or apply a fact. The armed marker is
unit-owned and unrelated attacks or units cannot consume it.

## Qualification

Focused tests must cover all four choices, costs, marker consumption, immunity,
living-target and modifier gates, and invalid inputs. After all source/build/
package/harness gates and an exact clean commit, two guarded fresh-process runs
must prove ordinary hit damage, recurring HP and ability damage, exact grit and
chamber accounting, miss/immunity rejection, bleed removal, and cleanup without
using a save.
