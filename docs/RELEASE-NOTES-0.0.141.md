# Kingmaker Gunslinger 0.0.141 — Expanded Summoning Phase 2A

Version: `0.0.141`

Informational version: `0.0.141-expanded-summoning-phase2a`

Package: `KingmakerGunslinger-0.0.141-expanded-summoning-phase2a.zip`

This release publishes Expanded Summoning Sprints 9-11. It adds Eagle, Dire
Bat, Giant Wasp, Stirge, Aurochs, Bison, Rhinoceros and Woolly Rhinoceros
across their authorized Summon Monster and Summon Nature's Ally lists. The
published catalog contains 882 generated choices and 29 retained native
wrappers, for 911 visible choices in total.

Eagle and Dire Bat use their qualified original views, senses and natural
attacks. Giant Wasp has its printed sting and DC 18 Dexterity poison. Stirge
uses a session-scoped attachment owned by the Stirge: its prey remains free to
move, act and attack while the Stirge feeds, and can spend a standard action
using the better current CMB or Mobility modifier to remove it. Four actual
Constitution drains detach the Stirge. Save/load resets an active attachment
cleanly. Filth Fever is the disclosed Kingmaker disease adaptation, with the
primary Stirge rule's once-per-Stirge/victim exposure limit.

Aurochs, Bison and Woolly Rhinoceros use the owner-authorized automatic
Trample response: one legal melee attack of opportunity at -4, or a Reflex
save when that response cannot execute. Aurochs and Bison receive Stampede
only while three adjacent allies with Stampede are each actively executing
their own Trample in the same round. Rhinoceros and Woolly Rhinoceros retain
their printed Powerful Charge behavior. Corrected original hooves, legs,
icons and views passed internal motion, combat, quantity and cleanup review.

The release deliberately stops at the Sprint 11 boundary. Sprint 12 has
checked-in Dire Rat, Dog, Hyena and Goblin Dog rules and original visual
groundwork, but all 68 placements remain hidden. Sprint 12 is not published:
its full player-path, persistence, compatibility, movement/contact, natural
expiry, module-disabled, inventory and final internal visual matrix remains
for the next development cycle.

The owner visual review is `NOT_PERFORMED_NONBLOCKING`. The release records the
completed internal engineering and visual review; it does not claim the
owner's personal art approval.

## Verification

- Repository validation and all 1,958 deterministic domain tests pass.
- A clean Release build and strict standalone package validation pass.
- The previously qualified firearm SoundBank remains byte-for-byte unchanged
  at SHA-256
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Optional Craft Magic Items support remains runtime-discovered through
  `CraftMagicItems.dll`; the mod still has no hard dependency on it. The
  inherited summon-menu and fatigue qualification baseline contains 1,288
  focused tests, and the later fatigue-authority baseline contains 1,325;
  both are retained by the larger current suite.
- Guarded Steam-backed runtime run
  `20260930T1626175528422Z-disposable-expanded-summoning` passed 41/41
  assertions, including direct and quantity attachment of the three new
  original Sprint 12 views while their choices remained hidden. Its runtime
  result SHA-256 is
  `3351CBB24DF11FA01317D2FA50B640E96F8648928F23069AE09FB07BC01AD462`.
- The same run confirmed the published Sprints 9-11 mechanics and lifecycle
  matrix remained intact. Its 282-entry local-runtime package SHA-256 is
  `AAC80B5AD7680B519230599B8CED673F4B19A74527598DBE04C0AECD813AA8EB`
  and its loaded DLL SHA-256 is
  `82A14E21C90DA5DE5E3FDB5FBAD8B83B99B38788230DE9FEEE545B9198012A5A`.
  Restoration record
  `20260930T1630580562591Z-disposable-expanded-summoning.json`, SHA-256
  `CA468E81FBCF9052BD0A77C4BE026EEBE0550FCFCA06FBE173429C3716D96749`,
  restored the original 136-file installation exactly to tree SHA-256
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

The qualifying runtime artifact loaded the exact 0.0.141 assembly identity.
Its evidence manifest records the complete precommit working-tree inventory;
the guarded publisher rebuilds and compares the committed release artifacts
before creating the tag and GitHub release.

## Install, update and uninstall

Install or update through Unity Mod Manager by selecting the release ZIP. Keep
the ZIP intact; do not copy individual files into the game directory. Existing
summons and saved characters require no migration. To uninstall, disable or
remove Kingmaker Gunslinger through Unity Mod Manager. Active session-only
Stirge attachment state is not serialized and therefore leaves no saved link.
