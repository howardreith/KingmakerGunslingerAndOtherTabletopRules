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
- Compatible Wwise authoring: not found in standard Program Files,
  ProgramData, per-user Audiokinetic, environment, command, or registry
  locations. The user's stated "16.2" installation remains to be identified;
  Kingmaker requires Wwise 2016.2.x authoring generation, not merely a launcher
  or a current Wwise release.

## Checklist

- [x] Preserve Git isolation and verify qualified ancestry.
- [ ] Curate exact local Wwise/Kingmaker runtime contract.
- [ ] Complete discharge-to-audio matrix.
- [ ] Implement strict manifest and staging contract with tests.
- [ ] Implement Wwise lifecycle and diagnostics.
- [ ] Route ordinary, Scatter, Dead Shot, Startling Shot, Menacing Shot, and
  Stop Bleeding committed discharges.
- [ ] Remove Unity firearm playback.
- [ ] Create reproducible Wwise 2016.2 project and generation validation.
- [ ] Generate and validate `KMG_Firearms.bnk` if compatible authoring exists.
- [ ] Integrate strict build/package validation.
- [ ] Add guarded runtime proof and development controls.
- [ ] Run all source/build/package/runtime gates.
- [ ] Complete human auditory acceptance.

## Next concrete action

Finish the curated exact runtime contract and implement the dependency-free
manifest/catalog/staging slice.
