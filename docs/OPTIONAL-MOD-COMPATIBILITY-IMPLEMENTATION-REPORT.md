# Optional-mod compatibility implementation report

## Status

Framework implementation is in progress on
`codex/postbase-archetypes-compatibility`. The unchanged 0.0.71 baseline passes
911 deterministic tests and all exact-reference Release/package gates. This is
not yet compatibility qualification.

## Architecture

Implemented foundations include a canonical read-only reference inventory, a
logical catalog/schema, a deterministic standard-library GUID/Harmony/bootstrap
scanner, and committed exact-profile definitions/schema with truthful
static-only and unavailable dispositions.

The profile resolver binds only committed logical keys to canonical local
roots, reports complete exact identities and manifests, and refuses ambiguous
or missing runtime roots. Public transaction entry accepts a committed profile
ID rather than caller-provided source paths.

Transactional staging records the exact original Mods tree and managed
`KMG_Firearms.bnk` state, atomically renames the original Mods directory,
creates a sentinel-owned isolated directory, copies the validated Gunslinger
package and allowlisted references, and restores from `finally`/explicit
recovery. Restoration quarantines staged data, returns the original directory,
verifies exact metadata and hashes, restores the bounded SoundBank side effect,
and deletes only the sentinel-owned quarantine after verification.

Pending: guarded runtime observer, exact UMM/Harmony diagnostics, execution
matrix, and final reports.

## Safety and dependency result

The design admits no third-party compile or runtime dependency and no
third-party payload in the Gunslinger package. Runtime staging will operate on
copied profile roots and restore the exact original Mods-directory state.

## Verification

Inventory and scanner behavior fixtures pass. Repository validation, the
unchanged 911-test suite, exact-reference Release, SoundBank validation, and
strict 0.0.72 packaging passed at the first checkpoint. Runtime qualification
has not begun.
