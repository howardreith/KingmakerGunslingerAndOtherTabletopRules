# Known issues and conservative adaptations

- Expanded Summoning uses only existing Kingmaker views. Proxy appearance is
  mechanically reconstructed but cannot perfectly match every tabletop body.
  The final manual checklist covers residual aesthetic judgment for scale,
  camera framing, projectile appearance, and animation style.
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
