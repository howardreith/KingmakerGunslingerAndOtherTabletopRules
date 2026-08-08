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

Commit and publish the five durable mission documents, verify the remote SHA,
then inspect and document every mandatory pre-implementation source and exact
installed Kingmaker contract before writing archetype features.
