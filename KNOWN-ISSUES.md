# Known issues and conservative adaptations

- Brown-Fur `0.0.81` failed human review and is superseded. The repaired
  `0.0.82` artifact passed focused runtime, persistence, optional-mod profiles,
  the authoritative 16-state boundary, and human presentation/play acceptance.
- Brown-Fur fails closed on an unknown or ambiguous future CotW Arcanist
  structure. This preserves all unrelated package modules but may require a new
  compatibility fingerprint and adapter before the archetype can be published.
- Removing Call of the Wild from a save containing a CotW Arcanist or Brown-Fur
  character is unsupported because CotW owns the parent class. Switching only
  the Brown-Fur module OFF while compatible CotW remains installed is supported
  and preserves existing owners.
- Eastern Weapons' subjective equipped-model appearance remains pending human
  review across the rigs, armor, animations, grips, and size-changing cases in
  `docs/EASTERN-WEAPONS-MANUAL-ACCEPTANCE.md`. Automated checks prove bundle,
  prefab, renderer, material, anchor, bounds, fallback, and cleanup structure;
  they are not a claim of aesthetic acceptance.
- `DEFERRED  ENGINE HAS NO RELIABLE COUP-DE-GRACE DC HOOK`. Tabletop Deadly is
  therefore omitted from Wakizashi and Katana. It is not approximated with
  ordinary damage and is not claimed in player-facing item text.
- Nodachi intentionally has no Brace behavior because Kingmaker lacks the
  relevant readied-action system. Its Polearms fighter-group membership does
  not add reach or change its two-handed sword animation.
- The exact qualified Arms and Armor build supplies no overlapping Katana,
  Wakizashi, or Nodachi content, so Eastern Weapons has no known optional-mod
  duplicate-name presentation limitation in the tested profiles.

- Expanded Summoning uses only existing Kingmaker views. Proxy appearance is
  mechanically reconstructed but cannot perfectly match every tabletop body.
  The final manual checklist covers residual aesthetic judgment for scale,
  camera framing, projectile appearance, and animation style.
- The outer Summon Monster and Summon Nature's Ally spellbook parents retain
  their native spell icons. Every visible creature-choice child instead uses
  a project-owned original icon; no Owlcat or optional-mod pixels are used in
  the 77-icon Expanded Summoning set.
- Dire Bat is not published in the player-facing roster. The installed game
  exposes no proven bat-compatible summon rig, and the Roc proxy failed human
  visual acceptance; all frozen identities remain registered for save safety.
- Elephant and Mastodon share Owlcat's Mastodon material. Their view scales
  are differentiated, but Elephant is not recolored gray because a safe
  per-instance material clone was not proven and shared-material mutation
  would alter the native Mastodon.
- Kingmaker stores spell descriptors on shared ability blueprints rather than
  per caster invocation. KMG direct templated roots retain `Summoning`; their
  spawned alignment, Celestial/Fiendish template and smite are caster-correct.
  KMG does not mutate shared descriptors dynamically. Sacred Summons therefore
  remains fail-closed when no exact optional surface is installed.
- Standalone Kingmaker 2.1.7b has no proven Aura of Menace carrier. Lantern
  Archon reuses the exact carrier when Call of the Wild supplies it and
  conservatively omits the aura standalone.
- Shadow Demon possession is omitted because a duration-bounded, save/load-safe
  implementation was not proven. Teleportation, planar travel, and creature
  summoning powers are removed from every adapted summon.
- Succubus has bounded charm and temporary energy-drain combat mechanics. It
  does not grant a permanent profane gift or any effect intended to outlive the
  summon.
- Bebelith's permanent armor destruction is represented as a DC 25 Reflex-gated
  one-round -2 AC effect after two same-target claw hits; equipped items are
  never mutated. Rot is omitted.
- Pixie sleep arrows are non-transferable, zero-weapon-damage attacks with a
  bounded resource. Irresistible Dance uses a project-owned touch-range state
  because the installed native-equivalent carrier belongs to an optional mod.
- Several movement modes and natural-creature secondary abilities lack safe
  native representations. Each omission is listed per creature in
  `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md`; no omitted mechanic is
  described as implemented.
- Removing the entire mod is not save-safe for campaigns that have used its
  content. Disabling Expanded Summoning is supported: identities remain
  registered, active summons load and expire safely, and no new KMG variants
  are published after restart.
- The save-free comprehensive Gunslinger runtime fixture has a historical
  limitation: its detached unit has no Swift-action controller, so the
  Gunslinger's Dodge command interrupts before `Start`. This is a test-fixture
  limitation, not a change introduced by Expanded Summoning; focused domain
  coverage and prior live gameplay behavior remain unchanged.
- The same broad aggregate also has order-sensitive forced-roll and detached
  target fixtures for Targeting/Bleeding Wound. Focused domain and production
  regression scenarios remain authoritative; Expanded Summoning does not
  patch firearm or deed execution paths.
