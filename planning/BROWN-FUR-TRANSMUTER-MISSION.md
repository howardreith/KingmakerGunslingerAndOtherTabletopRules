# Brown-Fur Transmuter mission

## Engineering base authorization

- Verified engineering base:
  `a8b19fe39285da44ac443b7bcbd217870ec6ffb6` from
  `codex/weapon-visual-variety-firearm-fit-cleanup`.
- Cleanup human acceptance: **PENDING / intentionally deferred**.
- Brown-Fur authorization: the user explicitly overrode the original
  human-accepted-cleanup-base prerequisite and authorized Brown-Fur development
  directly from this pre-human cleanup candidate.

This authorization is limited to selecting the Brown-Fur source baseline. It
does not accept or complete the cleanup sprint, does not change the cleanup
acceptance report, and does not resolve its outstanding human visual review.
That work remains deferred for later.

## Mission boundary

Implement Brown-Fur Transmuter as the independent, default-enabled
`brown-fur-transmuter` feature module in the combined package. Call of the Wild
is a hard runtime prerequisite for Brown-Fur only; every unrelated module must
remain loadable when that dependency is absent or incompatible.

The mission proceeds through investigation, implementation, focused source and
runtime qualification, the seven-module 16-state boundary matrix, packaging,
guarded deployment, and one immutable pre-human candidate. The Brown-Fur human
acceptance gate remains mandatory. The final 128-state exhaustive matrix must
not run until the exact installed Brown-Fur candidate receives explicit human
acceptance.

## Current status

- Branch: `codex/brown-fur-transmuter-cotw-extension`.
- Release base: `0.0.80`.
- Preferred candidate identity: `0.0.81-brown-fur-transmuter`.
- Base gate: satisfied by explicit user override on 2026-08-15.
- Cleanup acceptance: still pending and outside the current work cycle.
- Brown-Fur acceptance: not yet requested; implementation investigation is in
  progress.
