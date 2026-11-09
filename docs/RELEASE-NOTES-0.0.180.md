# Tabletop Added Rules v0.0.180

This stable release collects the summoned-creature playtest repairs merged since
v0.0.179 and alphabetizes the summon menus. It adds 26 ability and AI identities;
every identity from v0.0.179 is unchanged, so existing saves keep their references.

The roster stays at 134 creatures. The Stirge is withheld at the owner's decision, so
1,208 public summon choices remain of the 1,217 in v0.0.179.

## Alphabetical summon menus

Summon Monster and Summon Nature's Ally creature menus are now alphabetical by the
family-first name shown in the menu, such as "Mephit, Steam". The same creature's
choices run one, 1d3, then 1d4+1. A single flat menu no longer lists every single summon
before every group.

This covers tiers I-IX, higher-tier group choices, retained native choices, Druid
spontaneous conversion, and the supported alternate sources: Monster Tactician and the
Call of the Wild summon implementations. Names, icons and what each choice summons are
unchanged. Other mods' summon choices sort with the rest, including RacesUnleashed's
spider summons.

## Withdrawn at the owner's decision

- **Stirge.** No longer offered as a summon choice.
- **Wall of Ice.** Bone Devils and Ice Devils no longer cast it.
- **Magma Form.** The Magma Mephit no longer has it. It keeps its breath and Pyrotechnics.
- **Bardic Suggestion.** The Lillend no longer has it.
- **Sunder.** The Fire Giant no longer uses it. The Storm Giant keeps it.

Identities are retained, and saved summons are updated when loaded.

## Combat and rules repairs

- **Fear aura.** The Bone Devil and Ice Devil aura no longer repeats its saves against a
  target that remains inside it.
- **Mephit breaths.** Air breath deals slashing damage, Earth bludgeoning and Water acid.
  Water's cone looks like acid. Fire and the other breaths have their own icons, and the
  owned breaths affect enemies only.
- **Pyrotechnics.** Each executed cast now shows one blinding burst.
- **Cloud Giant.** Its Mist is an at-will Obscuring Mist through Call of the Wild. It
  replaces the previous Fog Cloud.
- **Pixie.** Entangle, full Confusion, Dispel Magic and Shield are at will. Irresistible
  Dance comes through Call of the Wild and keeps one use per summoning. Spells that need
  Call of the Wild are omitted without it.
- **Griffon.** The duplicate Charge is gone.
- **Mastodon.** Trample is added, at 2d8+18 and Reflex DC 29.

## Presentation

- **Portraits.** Every summon shows its own portrait in the initiative tracker.
- **Icons and descriptions.** Lantern Archon rays, Unholy Nimbus and Overrun have
  appropriate icons. The Ghaele's unnamed blast is now "Light Ray" with a description.
  Sprint, Nimbus, Trumpet Blast and Overrun no longer show empty tooltip rows.
- **Motion and effects.** The Hound Archon's sword and Aid, and the Redcap's scythe swing,
  use proper animations. The Lantern Archon's stray mass is removed. Monster-family arrival
  rims end after 3 seconds.
- **Sound.** The Vrock screech and Trumpet Archon blast each play once per use and stop
  when the creature is removed.
- **Models.** The Glabrezu, Nalfeshnee and Ice Devil have revised bodies. The Glabrezu's small
  arms are fused to its chest, the Nalfeshnee is a boar-headed winged demon, and the
  Ice Devil is one continuous skin. Their roster icons are unchanged.

## Disclosed open items

- **Native evidence.** Results stay credited to the development candidates that produced
  them. None was re-executed on this version-stamped build.
- **Alphabetical menus.** The owner accepted this on existing evidence. Its final native
  run was recorded as a failure because of observer setup defects, after 93 published menu
  arrays and six opened menus had sorted correctly. These were not observed in game: a
  completed cast of a reordered choice, selection from an alternate-source menu, a spider
  summon visible to a Spider Summoner, scrolling (107 rows fit at 1920x1080), console
  navigation, and saved hotbars. The late sort covers the loading sequence shown by the
  installed RacesUnleashed, not every possible later change by other mods.
- **Owner viewing and listening.** Aesthetic approval of the revised models, motion, effects,
  icons and sounds is not claimed.
- **Known small items.** The Nalfeshnee has 48 zero-area triangles at its wing roots. The
  Trample window shows some placeholder wording. The Ankylosaurus sound was deferred.
- **Earlier audit findings.** Sprint 41 findings A-5, A-6, A-7, A-9, A-13 and A-16 disclosed
  with v0.0.179 were not addressed. Roster-scale persistence and performance were not run.
