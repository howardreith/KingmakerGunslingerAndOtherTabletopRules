# Pistolero and Musket Master Journal

## 2026-08-07 - Mission intake and unchanged baseline

- Authority: user-issued Revision 2 autonomous work order; durable operational
  copy: `planning/PISTOLERO-MUSKET-MASTER-MISSION.md`.
- Exact base branch/commit: `master` at
  `10b792735db5d685b46749dc08ea819f31fa8052`.
- Feature branch: `codex/pistolero-musket-master-archetypes`.
- Initial status: clean; `Info.json` version `0.0.72`.
- Required compatibility merge is the exact base commit. The repository contains
  the merged profile transaction framework and
  `EvasiveBlueprints.PreservesCurrentComponentContract` current-donor repair.
- Content search found no user-provided local Pistolero or Musket Master rules
  `.txt`/`.md` file. The work order explicitly authorizes its reproduced rules
  when those files are absent.
- Inherited deterministic baseline: 911 tests. First two sandboxed runs failed
  only `audio.staging-lifecycle` because its temp-directory `File.Replace` was
  denied; the identical unchanged suite passed 911/911 with authorized temp
  access. Repository validation passed.
- Unchanged exact-reference Release, build-output, SoundBank, and strict package
  gates passed. Fresh local-runtime package SHA-256:
  `C9EC17E87805D3E1C93DC1879FBAC300E3BE0493AB422CE93B2445556D0BC4FE`;
  DLL SHA-256:
  `895D0EA7F1D4CB7658CA9C81B3F478D75C29A5FCEA8839E44908BE6E13F525FF`.
- Unchanged guarded baseline PASS:
  - mod load `20260808T0332364552630Z-mod-load-smoke`;
  - class contracts `20260808T0334429458961Z-observe-class-blueprint-contracts`;
  - presentation `20260808T0336494740671Z-observe-gunslinger-presentation`.
- Inherited standalone status: profile `gunslinger-only` remains
  `GUNSLINGER-REPAIR-REQUIRED` because detached Gunslinger's Dodge finishes
  `Interrupt` without its timed buff; exact inherited diagnostic
  `20260807T2057209416590Z-a48e33c01d6f48f2b407eb08fe361035`.
- Call of the Wild facts remain distinct: public exact profile
  `gunslinger-call-of-the-wild` is `CONFLICT-CONFIRMED` from human chargen
  omission; later dependency-free current-donor repair passed load
  `20260807T2146571019519Z-mod-load-smoke` and observer
  `20260807T2149121927539Z-a37fb450a1164ec9b664812be3073704`, retaining all
  46 helper classes and observing Gunslinger once in root/chargen input. Human
  confirmation of the repaired candidate remains pending.

## Committed compatibility profiles inherited unchanged

| Profile ID | Exact local identity/disposition | Allowed scenario boundary |
|---|---|---|
| gunslinger-only | standalone; `GUNSLINGER-REPAIR-REQUIRED` | load, optional observer, class contracts, presentation; working save permitted by profile but blocked by Dodge baseline |
| gunslinger-call-of-the-wild | CotW 1.14.4c-2.1 DLL `4EBF8E1E...B26915`; `CONFLICT-CONFIRMED` | committed observer matrix; no working save |
| gunslinger-craft-magic-items | no compiled root; `STATIC-AUDITED-ONLY` | no runtime scenarios |
| gunslinger-arms-armor | Arms & Armor 1.0.10 DLL `CEC7C177...E33733`; `RUNTIME-QUALIFIED-EXACT` | load/observer/presentation/rig/switching; no working save |
| gunslinger-toggle-custom-soundpacks | Toggle Custom Soundpacks 1.0.1 DLL `A2582533...0C9434`; `RUNTIME-QUALIFIED-EXACT` | load/observer/presentation/Wwise; no working save |
| gunslinger-call-of-the-wild-craft-magic-items | CotW plus source-only CMI; `STATIC-AUDITED-ONLY` | no runtime scenarios |
| gunslinger-high-risk-combined | CotW+A&A+Toggle; `CONFLICT-OBSERVED` | committed high-risk matrix; working save permitted but not qualified |
| gunslinger-all-loadable-local | all runtime-capable local references; `CONFLICT-OBSERVED` | committed high-risk matrix; no working save |
| gunslinger-qualified-combined | A&A+Toggle; `GUNSLINGER-REPAIR-REQUIRED` | passing targeted matrix; comprehensive/working save blocked by inherited Dodge |

## Exact next action

Checkpoint `c962e33` committed the five durable mission documents. The exact
required push helper was invoked and refused the branch before network access:

`Refusing to push non-allowlisted branch
'codex/pistolero-musket-master-archetypes'. Allowed branches:
codex/firearm-wwise-audio, codex/firearm-native-weapon-rigs,
codex/postbase-archetypes-compatibility`.

This is a workstation-policy hard stop under section 17 of the work order. Raw
push, helper modification, policy workaround, reuse of the obsolete compatibility
branch, or further unpublishable source commits are not authorized.

Exact next action: a human must add
`codex/pistolero-musket-master-archetypes` to the approved push helper allowlist.
Then rerun the exact helper, verify origin points to `c962e33`, and resume with
the mandatory pre-implementation source and installed-contract inventory.

## 2026-08-07 - Publication restored and inventory completed

- Human updated the external helper policy and published exact clean checkpoint
  `8ade461eab25f8fc2b068d8a739aa8ee1044f850`.
- Verified branch, local HEAD, and origin branch all equal that SHA; worktree was
  clean; the approved helper returned `Everything up-to-date` and explicitly
  confirmed publication without merge, force-push, reset, clean, rebase, or
  history rewrite.
- Completed the mandatory source/test/runtime-harness inventory in
  `planning/PISTOLERO-MUSKET-MASTER-INVENTORY.md`.
- Read-only exact installed IL proves public archetype
  `ReplaceStartingEquipment`/`StartingItems` fields and native
  `LevelUpHelper.AddStartingItems(UnitDescriptor)` selection/grant behavior.
  The native contract directly supports the exact Musket Master array and
  explains the existing detached no-inventory-delta path.

## Exact next action

Commit and publish the pre-implementation inventory, then implement the
canonical handedness policy and scoped proficiency foundations with focused
tests before generalizing the starting-firearm observer.

## 2026-08-08 - Canonical firearm handedness

- Added one project-owned `FirearmHandedness` enum and fail-closed
  `FirearmHandednessPolicy` mapping all five current production kinds exactly
  once: Pistol/Revolver one-handed; Musket/Blunderbuss/Rifle two-handed.
- `ProductionFirearmWeaponSpec` now validates its `IsTwoHanded` contract through
  the canonical policy instead of a second embedded kind list.
- Added three focused cases covering the complete catalog, both family scopes,
  cross-family rejection, unknown scope, unknown kind, and undefined kind.
- Repository validation, complete 914/914 deterministic suite, clean exact-
  reference Release, build-output, SoundBank, package creation, and strict
  package validation pass.
- Candidate package SHA-256:
  `E7D01B712448B85CC8693135CA362C608547BB853E26B40AC040615F9A3CA7FF`;
  DLL SHA-256:
  `E1600DC86BAEAE09CE7352A30D7B2309851DB430DAD7A93AD23110CDA10EACC0`.

## Exact next action

Commit/publish canonical handedness, then add stable one-handed/two-handed
proficiency blueprints and a pure full-or-matching-scope policy. Transactionally
rewire production firearm restrictions and focused tests before adding EWP.

## 2026-08-08 - Scoped firearm proficiency foundation

- Added stable manifest-backed one-handed and two-handed proficiency facts; the
  existing full `KMG.Firearms.FirearmProficiency` GUID and behavior are unchanged.
- Added one pure `FirearmProficiencyPolicy`: exactly one known marker is required;
  full proficiency permits all five kinds; one-handed permits Pistol/Revolver;
  two-handed permits Musket/Blunderbuss/Rifle; missing facts, unknown kinds, and
  ambiguous marker counts fail closed.
- Each production firearm restriction now stores its exact project kind plus
  exact full/one-/two-handed facts. The development Test Musket retains its
  historical full-only overload and cannot leak into archetype starter logic.
- Scoped action grants are exact: one-handed grants the existing Reload action;
  two-handed grants the same Reload plus existing Scatter Shot. No duplicate
  abilities were created.
- Appended two new GUIDs without changing existing entries. Bootstrap count is
  208 active; manifest ledger is 209 stable IDs including one reserved.
- Repository validation, complete 919/919 suite, clean exact-reference Release,
  build-output, SoundBank, package creation, and strict validation pass.
- Candidate package SHA-256:
  `D3627BBCFDC818D2D25E0CB5795B21A87B91C5B2EB73F04AA8A6CA072648E17F`;
  DLL SHA-256:
  `2A8A2846464A339342F6E8A18E8CB7415D66581F4FB9077FA9FF4379D58A05AB`.
- Guarded mod load requires a clean Git state, so the first dirty-tree launch
  was correctly rejected before deployment. Commit/publish this source-qualified
  phase, then run fresh mod load against the exact commit.

## Exact next action

Commit/publish scoped proficiency, run guarded `mod-load-smoke`, record the run,
then inspect exact BAB/duplicate prerequisite APIs and implement the single EWP
(Firearms) publication plus scoped firearm-feat prerequisites.
