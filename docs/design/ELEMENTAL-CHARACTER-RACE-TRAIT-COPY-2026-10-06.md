# Elemental character race trait copy contract — 2026-10-06

Status: dormant final copy; no localization is registered, no icon consumer is
created and no trait is published. This contract is paired with
[the stable publication plan](ELEMENTAL-CHARACTER-RACE-TRAIT-PUBLICATION-PLAN-2026-10-06.json)
and `ElementalCharacterTraitCatalog`. Descriptions below are the proposed visible
paragraphs; supplemental paragraphs follow in the same tooltip description
using the repository's ordinary blank-paragraph convention.

## Fiery Glare — Ifrit

While Fiery Glare is enabled, Intimidate checks use a result of 10 whenever that
would succeed; otherwise, they are rolled normally. This functions even during
combat.

This is the frozen success-only take-10 adaptation. It is optional, initially
off, free to activate and uses no resource. It does not grant tabletop choice
to take 10 when that would fail, and does not affect Bluff, Diplomacy or general
Persuasion. The trait and its toggle share this exact title/description contract.

## Stoic Dignity — Oread

While conscious, you gain a +1 trait bonus on saving throws against mind-affecting
effects. Other allies within 10 feet gain a +1 morale bonus on these saving
throws. A creature receives no bonus against the same effect it is already
suffering from; an unrelated effect does not prevent the bonus.

Supplemental paragraph:

The same effect is identified by its exact effect or source lineage. If that
relationship cannot be established, the bonus applies. Stoic Dignity does not
remove or suppress an existing effect.

The qualified resolver gives exact incoming/existing buff identity precedence,
then exact ability/source/parent lineage; caster alone, names and descriptors
are not effect identity. Consciousness is checked at save resolution. The holder
does not benefit from its own morale aura. Duplicate ally morale modifiers obey
native stacking. No immunity, cleanse, duration change or effect removal is implied.

## Aerial Observer — Sylph

While Wings of Air is active, you gain a +2 trait bonus on Perception.

Supplemental paragraph:

This benefit requires the active Wings of Air effect. Visual wings, hovering,
elevated ground and other flight effects do not grant it.

This names the single qualified KMG carrier, `KMG.ElementalRaces.Feats.WingsOfAir.Buff`
(`e116e1e0a17a4aceb001000000000019`). It is an adaptation, not the tabletop
30-feet-above-ground condition and not a generic flight predicate.

## Whiteout — Undine

While outdoors in rain or snow of at least light intensity, attacks against you
have an additional independent 10% miss chance. This chance is checked after
other concealment and miss chances, and attacks that ignore concealment ignore
it. Magical fog and waterfall spray do not grant this benefit.

Supplemental paragraph:

The two miss chances are checked separately: a 20% concealment chance and
Whiteout's 10% chance give a combined 28% chance to miss. Whiteout affects attacks
that make the game's concealment and miss-chance check; attacks that skip that
check and effects without an attack roll are unaffected.

This is the frozen outdoors-only adaptation. Exact actual Rain/Snow Light+
and a real outdoor map identity are mandatory. Native false results short-circuit;
IgnoreConcealment and exact authorized Seeking bypass this independent roll.
No vision, targeting, threat, concealment-tier or attack-range change is implied.

## Registration boundary

The catalog holds immutable strings and staged localization keys for four
features; Fiery Glare's toggle intentionally shares its feature keys. Hidden
providers, activation/recipient buffs and the area have no visible copy.
`CreateDetached` may assign unresolved key objects only after all four originals
qualify; it never registers localization, a blueprint or a selection entry.
The current frozen asset evidence catalog is empty, so even detached native
construction is blocked. Final text wrapping and owner UI approval remain
future player-facing acceptance work.
