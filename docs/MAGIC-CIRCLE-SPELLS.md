# Magic Circle spells

This content module adds four separate level-3 spells: Magic Circle against
Evil, Good, Law and Chaos. Each is a standard-action touch abjuration that makes
the touched creature the bearer of a moving ten-foot emanation for ten minutes
per caster level. The original caster supplies level, metamagic, duration and
dispel attribution; the bearer supplies position.

All creatures covered by the native area receive its benefits, including the
bearer, enemies and summons. Each gains a +2 deflection bonus to AC and a +2
resistance bonus to saves against matching-alignment attacks and effects.
Native typed stacking applies, so overlapping circles do not add their bonuses
together and stronger equipment bonuses remain effective.

The existing `protection-from-alignment-control-immunity` setting is the single
authority for the Protection family's added control immunity. When enabled, the
circles use the same component, catalog, source resolver and alignment policy as
individual and communal Protection. Only **new** qualifying control applications
from matching controllers are blocked. A summoned controller's own alignment is
used. Missing sources retain the shared fail-open policy; there is no new trusted
alignment metadata. When this setting is disabled, ordinary circle defenses
remain available and descriptions omit the added protection.

`magic-circle-spells` follows the existing startup/restart model. Content OFF
keeps the stable blueprint identities needed to hydrate known spells, existing
items and active circles, while disabling new casts and spell-list publication.
It does not silently erase active effects or reset unrelated settings. Existing
inert known spells or scrolls may remain visible, as with the established content
module convention. Canonical vendor stock definitions also stay registered OFF
to retain native purchase memory; even an unvisited supplier can display inert
scrolls. Casting, list access and saved-vendor migration remain gated. This is
not an uninstall-safety claim.

## Access and acquisition

| Spellbook/list | Level-3 entries |
| --- | --- |
| Cleric, Sorcerer/Wizard, Inquisitor | All four |
| Paladin | Against Evil and Chaos |
| Verified installed CotW Oracle, Warpriest, Summoner, Shaman, Spiritualist | All four |
| Verified CotW Occultist and Relic Hunter Abjuration implement choices | All four, through their actual class/list relationships |
| Verified CotW Antipaladin | Against Good and Law |

Arcanist obtains access through the actual inherited Wizard list. Witch and
Unlettered Arcanist do not receive a new entitlement. Native filtered specialist
lists retain school restrictions. The spells remain separate entries, so a
spontaneous caster learns one variant per normal known-spell selection.

Each spell has its own level-3, CL5, single-charge scroll at the native 375-gp
base price. The established arcane/priest supplier mechanism stocks finite
batches of five. Reopening a merchant does not refill a purchased batch. Native
scroll casting and supported scribing use the exact corresponding spell.
The spell consumes a slot, or its scroll consumes one charge, once per cast;
area refresh and new recipients consume nothing. No silver-dust inventory
requirement is added.

## Lifetime and ownership

The timed carrier owns expiration. Native `AddAreaEffect` attaches one area to
that carrier, and native membership gives each recipient a separate contribution
identified by its exact area instance. Entry, re-entry or reconstruction does not
restart the carrier. Removing one carrier cannot remove another cast's benefits,
standalone Protection or a Paladin aura.

Recipient buffs represent proximity and are not independently dispellable.
Successfully dispelling the carrier or its native area ends that cast. Native
scene unloading can reconstruct the area from the original carrier; it is not a
new cast. Original caster references are retained even when the caster no longer
resolves, without substituting the bearer or recipient.

The owner-approved Kingmaker adaptation ends the circle on **bearer death**.
Unconsciousness and original caster death do not end a circle around a living
bearer. Permanent removal of the bearer removes its area. Native area geometry
uses a ten-foot radius (3.048 metres), creature body radius and the engine's line
of sight obstruction test; there is no global distance scan. The native targeting
preview uses the same ten-foot radius.

## Rules and limits

The PF1 spell entries supply the school, casting time, range, area, duration,
opposing alignment descriptor and class entitlements:
[Evil](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Evil),
[Good](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Good),
[Law](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Law),
[Chaos](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Magic%20Circle%20against%20Chaos).
Against Evil uses Good, against Good uses Evil, against Law uses Chaotic and
against Chaos uses Lawful. The implemented defensive effect has no spell
resistance; the tabletop exception relates to the deferred creature barrier.

Kingmaker's native ally predicate supplies the willing-touch convention. A
hostile touched bearer receives the native Will-negates check; entering an
already active circle causes no save. These are implementation adaptations,
separate from the tabletop source text.

This first release does **not** add new saves or a morale bonus against existing
control; remove, suppress, pause or resume existing control; exclude summoned
creatures or implement contact/SR/breach barriers; implement inward circles,
planar binding or diagrams; or confer blanket immunity to mind-affecting,
charm, compulsion, fear or emotion effects. Existing domination stays active.
The individual and communal Protection spells retain their existing levels,
targeting and durations.

## Qualification and art

The final installable review candidate passed feature-specific native mechanics,
all four startup combinations, saved-world lifecycle and desktop UI checks.
[The acceptance record](../planning/MAGIC-CIRCLE-ACCEPTANCE.md) maps exact artifact
identities to evidence. [The handoff](MAGIC-CIRCLE-HANDOFF.md) records publication,
installation and limits. Saved-world qualification uses the original eleven-mod
stack; optional present/absent profiles have bounded native save-free coverage.

Four original paintings have exact owner-approved exports. Each alignment
intentionally shares its painting across the spell, held touch, carrier,
recipient and scroll. The final candidate verifies all twenty visible consumers and actual learning,
casting, buff and inventory screens with the control enhancement ON and OFF.
[Native visual inspection](../reports/magic-circle/FINAL-NATIVE-UI-REVIEW.json) is
separate from the recorded owner pixel approval. Native recipients show the
engine's untimed "Permanent" label; the "Within Circle" name and description
explain that protection lasts only while covered.
The original artwork family and protected unrelated assignments are preserved.
