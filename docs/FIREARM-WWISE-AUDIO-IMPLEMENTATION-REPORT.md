# Firearm Wwise audio implementation report

Status: investigation/source implementation in progress; not release-qualified.

No Wwise bank has been generated or claimed. No manifest with a placeholder
hash will be created. Automated `PostEvent` acceptance, when available, will
not be represented as proof of audible output.

Implemented source-complete architecture now includes strict validation and
staging, one-time readiness/load state, Wwise posting and diagnostics, global
and selected-unit preview controls, all six required discharge routes, Unity
playback removal, source-only/release package gates, deterministic WAV staging,
an authoring handoff, and a save-free guarded runtime scenario. Authentic bank
generation, resulting runtime PASS evidence, and listening remain pending.

Wwise 2016.2.6.6153 is verified at
`C:\Audiokinetic\Wwise_2016.2.6.6153`. The repository now contains the curated
Owlcat.Templates 1.14.4 Kingmaker authoring project, including the exact Master
Mixer and native `WEAPONS` bus identity. Its template Work Units are
byte-identical to the generated seed. Wwise GUI interaction is required to
import the prepared WAVs and create the five sounds/events and bank so Wwise,
rather than hand-edited XML, assigns their object GUIDs.

Previous source-complete commit: `3cbfe4a`; documentation checkpoint:
`e34c1a0`. Both are present on `origin/codex/firearm-wwise-audio`. Mod version
remains `0.0.70`; no release version bump was made because the authentic bank
and runtime qualification are absent.
