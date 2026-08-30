# Gunslinger Outfit Kitbash Journal

## 2026-08-30 - Intake and isolation

- Read the repository agent instructions and the complete mission package
  documents before modifying tracked files.
- Verified repository root and fetched origin metadata.
- Verified local `master`, `origin/master`, and the pre-branch `HEAD` were
  identical at `5949165e2a6407ca480d46cd86d8944e4152e2fb`
  (`v0.0.110`, `Release protection from alignment control immunity`).
- Created and switched to exactly
  `codex/gunslinger-class-outfit-kitbash` from `origin/master`.
- Verified active source pins are `0.0.110` with informational identity
  `0.0.110-protection-from-alignment-control-immunity`. No version change has
  been made.
- The human-supplied external reference-package path was absent. A pre-existing
  untracked, non-symlink package under `docs/reference` contained the three
  named documents and exactly eight images. Its document/image manifest entries
  matched the computed image SHA-256 values. The package remains untouched and
  is explicitly ignored to prevent accidental publication. This location
  discrepancy remains an intake uncertainty; findings below come from the
  manifest-matching local package.
- Positively inspected all eight images. The rejected male/female Gunslinger
  presentations both use the generic blue Fighter tunic, red sash, belt, dark
  trousers, and boots. Barbarian and Paladin benchmarks demonstrate that native
  outfits can retain clear class identity at preview scale. The four
  inspirations consistently support fitted/long garments, dark leather,
  restrained burgundy, practical belts/pouches/straps, gloves/bracers/boots,
  and controlled asymmetry; hats and visible weapons are not essential to the
  shared vocabulary.
- Confirmed the current class registration still resolves Fighter and the
  current class creation path copies Fighter presentation fields. Production
  selection remains prohibited until installed API and rendered-asset evidence
  exist.
- Created the five required durable mission records. No gameplay source,
  production asset ID, native blueprint, installed game file, or version pin
  has been changed.

## 2026-08-30 - Installed API and native-resource investigation

- Verified the live game through Steam App ID 640820. Unity Mod Manager reports
  Kingmaker `2.1.7b`; the loaded `Assembly-CSharp.dll` SHA-256 is
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`
  and its MVID is `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.
- Installed reflection/IL inspection established that
  `BlueprintCharacterClass.GetClothesLinks(gender, race)` yields shared-wrapper
  links before gender-specific direct links. Shared
  `KingmakerEquipmentEntity` wrappers select race/gender links and provide the
  installed fallback behavior. `EquipmentEntityLink.AssetId` is the stable
  loading boundary.
- Verified the public avatar operations needed by the next audit stage:
  add/remove equipment entities, rebuild the outfit, apply primary/secondary
  ramps, and restore saved equipment. Verified native equipment metadata for
  layer, hidden body parts, lower-material behavior, color profiles/ramps, body
  parts, outfit parts, and special cloak/backpack parts.
- Verified armor/item presentation is separately exposed through
  `BlueprintItemEquipment.EquipmentEntity` and alternatives. The production
  class currently copies Fighter shared, male, female, and color fields; this
  remains unchanged.
- Inspected native Fighter, Barbarian, and Paladin definitions as structural
  benchmarks, then inventoried class, item-linked, and bounded raw native
  streams. Public Visual Adjustments research was used only as architectural
  precedent; the installed assembly and live resource library are authoritative.

## 2026-08-30 - Guarded catalog checkpoint

- Added the default-off, save-free `gunslinger-outfit-audit` request to the
  existing guarded runtime harness. It dynamically discovers supported player
  races and class/item/raw resource sources; loads and describes exact links;
  emits sorted, deduplicated ignored JSON; hashes the deterministic candidate
  set; and records that it did not mutate a save, inventory, progression, or
  avatar.
- The first guarded run (`20260830T1956227219163Z`) failed closed without an
  exception. It exposed three audit defects: Unity returned
  `Application.version=UNKNOWN`, race blueprints included duplicate enum IDs,
  and already-known resources were excluded from raw-source classification.
  The harness was narrowed to the exact loaded assembly hash/MVID, distinct race
  enum IDs, and independent source classification.
- The second guarded run (`20260830T2005018122430Z`) passed eight of nine
  assertions and again had zero exceptions. Its sole failure showed that
  Kingmaker maps equipment entities as bare names such as
  `EE_Armor_LeatherRanger_M`, not only path-qualified `/EE_...` names. A focused
  test and the smallest exact `EE_` classifier correction were added.
- The third guarded run (`20260830T2012181937219Z`) passed all nine assertions:
  49 class sources, 163 item-linked sources, 361 bounded raw sources, 1,206
  unique loaded entities, 3,816 matrix rows, 4,878 resolved links, zero
  unresolved links, zero inspection errors, and nine dynamically discovered
  player-race enum IDs across both genders.
- Passing candidate-set ID:
  `dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`.
  Catalog SHA-256:
  `73af097a4dd21fe905d2f9b4388f2ef6a68503f4b6723040e1dd00d3e3e2e294`.
  Raw catalog and runtime batches remain ignored local evidence.
- During the passing launch pipeline, repository validation passed, all
  `1362/1362` domain/reflection tests passed, source compilation passed, and the
  generated installable and local-runtime packages passed strict standalone UMM
  validation. These results qualify the audit checkpoint, not outfit aesthetics.
- The explicit clean checkpoint command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` then passed
  repository validation, all 1,362 tests, clean Release compilation, package
  creation, and strict package validation.
- The immediately following runtime-preflight invocation hit the repository's
  known post-build timestamp check
  `unsupported-does-not-build-or-stage-package`. The unchanged rerun after
  outputs became quiescent passed all 157 checks. Both outcomes are recorded;
  the passing quiescent result is the integration gate.

Exact next action: commit and publish the coherent guarded-catalog checkpoint,
then extend the audit with disposable-avatar apply, rebuild, valid-color
sampling, deterministic capture, and exact state restoration for the serious
native donor candidates.
