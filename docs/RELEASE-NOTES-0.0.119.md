# Kingmaker Gunslinger 0.0.119

Version: `0.0.119-contextual-world-map-teleportation`.
Install `KingmakerGunslinger-0.0.119-contextual-world-map-teleportation.zip` from
the release assets. Preserve FeatureModules.json and all existing settings.

Teleport and Greater Teleport now use the same persisted visit evidence. A point
revealed, explored or unlocked after the initial migration stays unavailable to
both spells until an ordinary world-map arrival records it. Exact teleportation
no longer bypasses this rule. The real spellbook resources, native destination
controls, confirmation, spell levels, odds and outcomes are unchanged.

Existing positive familiarity counts, the migration-complete flag and the saved
exploration boundary retain their exact format and values. Pre-teleportation
saves receive one idempotent legacy migration. Native history cannot reliably
distinguish every old physical arrival from a scripted unlock; accepted old
flags are inferred once as one visit. They are never a continuing live bypass.
A point first visited with the module OFF can require one ordinary revisit ON.
Word of Recall keeps its separate exact Oleg/capital sanctuary rule, with no
fallback when capital state is ambiguous.

Four fresh Steam processes qualify real native campaign disk save/load,
disabled-module save retention and re-enabled restoration. Desktop/gamepad
coexistence regressions preserve a request-local foreign control and callback,
native Travel, navigation/input layers and dismissal/modal ownership. No
production UI defect was reproduced, so production UI behavior was retained.
The earlier duplicate-arrival concern was not reproduced; the movement observer
and transpiler are unchanged.

The owner authorized this normal public patch after the mandatory installed-stack
gates: 1,550 domain tests, clean deterministic Release/package validation,
guarded preflight, settings/profile transaction checks, all existing native
teleportation scenarios, working-save smoke, persistence/coexistence and all
26 current module boundaries. Exact final source/package identities and run IDs
are recorded in the hardening report and public verification evidence. All
pre-existing saves, including KMG_AUTOMATION_BASELINE and KMG_AUTOMATION_WORKING,
remain unchanged. Only transaction-owned disposable saves were written/deleted.

All published 0.0.118 content and its accepted 0.0.117 base are retained. No new
spell grants, scrolls, local-map targets, serialization format or blacklist
entries are introduced. Preserve FeatureModules.json when installing.

The installed stack uses Kingmaker 2.1.7b and UMM 0.33.0.0. Other UMM versions,
complete isolated optional-mod combinations, unavailable Arms and Armor binaries,
unusual late-campaign map states and the owner's separate high-level campaign
playtest remain unqualified. The automation save depends on Craft Magic Items;
installed-stack results do not establish standalone or every optional-mod
save-backed compatibility profile.

The firearm SoundBank retains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items integration remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test
and 1,325-test checkpoints remain historical evidence; the current suite
contains 1,550 cases.
