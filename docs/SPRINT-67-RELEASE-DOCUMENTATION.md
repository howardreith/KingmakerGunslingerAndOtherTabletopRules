# Sprint 67 release documentation and package contract

## Outcome

The standalone package now contains `INSTALLATION-COMPATIBILITY.md` with
bounded installation, update, removal, and compatibility guidance. The guide
requires save backups and complete mod-folder replacement, disclaims arbitrary
downgrades and uninstall-safe saves, identifies serialized custom-content risk,
and limits compatibility claims to the qualified Kingmaker/UMM baseline and
known integration surfaces.

Both the ordinary and deterministic package paths enforce the exact nine-file
allowlist, including the guide. Historical version-specific eight-file archive
validators and evidence remain historical and unchanged.

## Qualification

- Qualified source commit: `cb6ffd7`.
- Focused release-documentation checks: 6 passed.
- Runtime request source checks: 38 passed.
- Runtime preflight checks: 84 passed.
- Repository validation: passed through the active Sprint 60 validator.
- Complete domain/reflection suite: 831 passed, 0 failed before and after the
  source commit.
- Clean exact-reference Release build: passed.
- Build-output and strict ordinary/deterministic nine-file package validation:
  passed.
- Exact committed-tree deterministic package SHA-256:
  `1a350b6deab0855abeaaa967c4e3cd09dbac55d0ec7bb097413ca5253f86ed3a`.
- Exact committed-tree DLL SHA-256:
  `902fda0ecbb0853f3217f1bf8065d6304fc5e07fe4a4c1464ede49aebb85de09`.

No game launch was required: the checkpoint changes packaged documentation and
package allowlists only, not the assembly source, blueprints, or runtime input.
