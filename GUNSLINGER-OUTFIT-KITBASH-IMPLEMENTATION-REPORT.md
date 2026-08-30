# Gunslinger Outfit Kitbash Implementation Report

Status: active investigation; production implementation is intentionally
unchanged.

## Intake

- Baseline: `5949165e2a6407ca480d46cd86d8944e4152e2fb`
- Branch: `codex/gunslinger-class-outfit-kitbash`
- Intake version: `0.0.110`
- Installed-game target: Pathfinder: Kingmaker 2.1.7

## Verified current cause

The Gunslinger class registration still resolves the native Fighter blueprint,
and its creation path copies Fighter male, female, shared clothing, and default
color presentation fields. This explains the rejected generic Fighter
appearance. Exact assignment and array-aliasing evidence will be preserved in
the focused test baseline before replacement.

## Investigation

The installed 2.1.7b class-clothing path is now verified. Shared
`KingmakerEquipmentEntity` wrappers resolve gender/race-specific
`EquipmentEntityLink` values, followed by direct gender-specific class links.
Loaded `EquipmentEntity` records expose layer, body/outfit parts, special
cloak/backpack treatment, hiding flags, lower-material behavior, color
profiles, and primary/secondary ramps. Public avatar operations cover
add/remove, ramp application, rebuild, and saved-equipment restoration.

The guarded `gunslinger-outfit-audit` scenario now inventories the live class,
item-linked, and bounded raw resource streams without touching save-owned
state. Passing run `20260830T2012181937219Z` resolved 4,878/4,878 links across
3,816 class/item matrix rows and nine discovered player-race IDs for both
genders. It loaded 1,206 unique entities with zero inspection errors.

Deterministic evidence identities:

- candidate set:
  `dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`;
- ignored catalog:
  `73af097a4dd21fe905d2f9b4388f2ef6a68503f4b6723040e1dd00d3e3e2e294`.

Installed class-link evidence makes Bard, Alchemist, Magus, Ranger, Rogue,
Slayer, and Inquisitor concrete render donors. Ranger, Rogue, Slayer, and
Inquisitor default cap links structurally hide hair and/or ears, so serious
candidate sets will omit those cap links unless direct rendering disproves the
risk. No asset identifier has been approved or hardcoded.

## Selected appearance

Pending. This section will record exact shared/male/female asset IDs, resource
names, donor classification, default colors, ramp behavior, score, runners-up,
fallbacks, and rationale only after installed-game proof.

## Production changes

Pending evidence-based selection.

## Tests, build, package, and runtime

The audit checkpoint passes repository source validation, all 1,362
domain/reflection tests (including guarded boundary, deterministic/read-only
inventory, and evidence-manifest preservation), Release compilation, strict
standalone UMM package validation, and its Steam-backed guarded runtime
scenario. The first two runtime iterations failed closed and improved the
instrumentation; the third passed all nine assertions with no exceptions.

These checks prove the reusable audit and native catalog only. Candidate
rendering, aesthetic scoring, production binding, full compatibility
qualification, final clean build/package, and release runtime proof remain
pending. A build or domain-test pass is not visual proof.

## Uncertainty

The supplied external mission-package path was absent at intake. A
manifest-matching pre-existing untracked package was inspected provisionally
without modifying or publishing it. The path discrepancy remains explicit.
