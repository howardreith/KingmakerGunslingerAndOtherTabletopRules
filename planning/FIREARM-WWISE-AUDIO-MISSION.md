# Firearm Wwise audio mission

## Durable objective

Replace the ineffective Unity `AudioSource` firearm fallback with one native
Kingmaker Wwise bank and route every committed physical firearm discharge to
exactly one supported event. Mechanical behavior must remain unchanged.

## Current checkpoint

- Branch: `codex/firearm-wwise-audio`
- Starting commit: `871609aef37bb714ad612878c1e64fc8ebe44c40`
- Mod version: `0.0.70`
- Qualified runtime ancestor `4f28dcfda655e35ed7be59babc9c0fe4ee4982ff`: present
- Worktree: isolated because the original `master` worktree contains unrelated
  untracked split/archive files.
- Compatible Wwise authoring: Wwise 2016.2.6.6153 is installed at
  `C:\Audiokinetic\Wwise_2016.2.6.6153`; both x64 authoring executables report
  file/product version 2016.2.6.6153.
- Authoring project: curated from the Owlcat.Templates 1.14.4
  `kmsoundvoicemod` seed. Supplied Work Units are preserved byte-for-byte; only
  the project name is curated to `KingmakerGunslingerFirearms`.

## Checklist

- [x] Preserve Git isolation and verify qualified ancestry.
- [x] Curate exact local Wwise/Kingmaker runtime contract.
- [x] Complete discharge-to-audio matrix.
- [x] Implement strict manifest and staging contract with tests.
- [x] Implement Wwise lifecycle and diagnostics.
- [x] Route ordinary, Scatter, Dead Shot, Startling Shot, Menacing Shot, and
  Stop Bleeding committed discharges.
- [x] Remove Unity firearm playback.
- [x] Create reproducible Wwise 2016.2.6 authoring project and generation validation.
- [x] Preserve the template's exact Kingmaker Master Mixer and `WEAPONS` bus.
- [ ] Generate and validate `KMG_Firearms.bnk` if compatible authoring exists.
- [x] Integrate strict build/package validation with explicit source-only gate.
- [x] Add guarded runtime scenario and development controls.
- [ ] Run all source/build/package/runtime gates.
- [ ] Complete human auditory acceptance.

## Next concrete action

Open the curated project in Wwise 2016.2.6.6153 and perform the minimal GUI
import/event/bank operations documented in the authoring README. Wwise must
generate the new object GUIDs; they will not be invented in source XML.
