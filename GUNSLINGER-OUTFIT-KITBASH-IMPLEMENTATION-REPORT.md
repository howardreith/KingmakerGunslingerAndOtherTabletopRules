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
Inquisitor default cap links structurally hide hair and/or ears. The guarded
renderer freezes 32 audited, gender-specific links for six serious
presentations while excluding Ranger/Rogue/Slayer caps and Ranger/Rogue capes.
These are investigation-only IDs; no production asset identifier is approved.

The renderer captures preview-like four-view contact sheets and elevated
isometric views for male and female Humans in native-default/no-weapon,
native-default/pistol, native-default/musket, and alternate-valid-ramp
no-weapon cases. It snapshots, restores, and verifies entity order, both ramp
arrays, and saved links around every candidate, then verifies request-local
cleanup. It cannot write a save.

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

The subsequent guarded-render checkpoint passes 160 runtime preflight checks,
repository validation, all 1,365 domain/reflection tests, exact-reference
Release compilation, deterministic package construction, and strict validation
of both standalone and local-runtime packages. Both packages have SHA-256
693c09684256fab77b4835b78eff12ab974c2bc460a63824f877768cd9c16ce8;
the DLL SHA-256 is
17bfe03b52e85cab627be425c680b1ccf6db88275ba4e253081065685304e377.
The first launch request was rejected before game start solely because the
harness requires a clean commit. Installed-game rendering remains pending.

## Uncertainty

The supplied external mission-package path was absent at intake. A
manifest-matching pre-existing untracked package was inspected provisionally
without modifying or publishing it. The path discrepancy remains explicit.
