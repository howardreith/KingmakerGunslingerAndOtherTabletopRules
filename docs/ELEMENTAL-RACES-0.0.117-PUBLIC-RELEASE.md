# Elemental Races 0.0.117 public release verification

## Owner acceptance and authorization

The owner accepted the installed `0.0.117-elemental-char-gen-stabilization`
candidate on 2026-09-07 and instructed:

> This is acceptable. Please finalize this, make sure you're at the latest tip of master, merge to master, push to remote, and cut a new release. Excellent work, thank you Codex. All of this is fully authorized.

This authorizes publication and supersedes the stabilization mission's earlier
candidate-only boundary. Owner acceptance is recorded as supplied; no
individual full-screen checklist results were supplied or inferred. The historical
[acceptance handoff](ELEMENTAL-RACES-CHARACTER-CREATION-STABILIZATION-HANDOFF.md)
and [original release notes](ELEMENTAL-RACES-0.0.117-HISTORICAL-RELEASE-NOTES.md)
retain their original statuses and artifact attribution.

## Starting state and release scope

Release preparation started from clean, pushed feature branch
`codex/elemental-races-expansion` at
`a5fe788fa95b1cc870ecab99993b15f503371a92`. Fetch confirmed latest
`origin/master` was `58d9511082af30f1a4ec88c1238ae7ae2b3651c2`, already an
ancestor of this feature branch. The latest public version was 0.0.116; version
0.0.117 had no existing tag or public release. GitHub's default branch is master.

The accepted artifact was built from
`132f0650e997579c589d19874a022aa5ee2213f2`:

- ZIP SHA-256: `7b930f3f084796f81e9d7cb0a6babea248404c87eb72381509019c8843c9ec14`.
- DLL SHA-256: `fcfe67df81077f9add11ad3770168161c65f3906805fe1c483ae096130892fef`.
- DLL MVID: `35dd73ec-4168-457b-8c03-5135e0e9e1ee`.
- Informational version: `0.0.117-elemental-char-gen-stabilization`.
- Manifest entries: 1,869; UMM package files: 135.

The immutable accepted ZIP and sidecar remain in the ignored stabilization
qualification directory. The live UMM installation was independently verified
against all 135 package files before the owner's acceptance. UMM and Elemental
Races were enabled. The owner's subsequent output log was preserved before any
new guarded launch; its SHA-256 is
`04f2430acfbdbca96932bdd1845868d5d51e1640f12f08b0a4a28656dbea1b80`.

Release preparation changes current documentation and its release-validation
contract only. Production source, version metadata, blueprints, assets and game
mechanics remain those of the accepted candidate. Rebuilding embeds the final
merge commit in the DLL; this provenance change receives separate artifact
verification. Roadwarden, Dead Reckoning, Skeletal Salesman stock and the
released Protection descriptions remain included. Treacherous Earth, Nereid
Fascination, favored-class bonuses and further content remain deferred.

## Qualification policy

The accepted candidate passed repository validation, 1,458 domain/reflection
tests, 259 runtime preflight checks, clean Release compilation and strict
deterministic packaging. Its final matrix passed 13,847 assertions in 28 fresh
guarded Steam processes. That evidence includes all four races and twelve
heritages, complete native character creation, ordinary global Traits,
back-navigation, real Player respec, all nineteen visible traits' persistence
and physical lifecycles, compatibility profiles and pinned 0.0.114 migration.
Exact identities, limitations and known third-party diagnostics remain in the
handoff and are not relabelled as new release-artifact runs.

Release sealing reruns the complete build/package gates and compares all
payload entries against the accepted candidate. Two clean publisher builds
must produce identical DLL and ZIP hashes. Focused guarded Steam App 640820
runs must verify the new artifact in character creation, native respec and
working-save loading using only KMG_AUTOMATION_WORKING. Each profile restores
the prior mod tree and settings exactly. Publication requires those checks to
pass, followed by download and hash verification of the public assets.

Exact merge, tag, final artifact, runtime and publication results are appended
after they are observed. The historical candidate records remain unchanged.

## Release preparation verification

`powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1`
passed repository validation, all 1,458 domain/reflection tests, clean
exact-reference Release compilation, supply icons, SoundBank validation and
strict deterministic UMM packaging. The original notes were archived byte for
byte. The first documentation validation identified omitted inherited artifact
guarantees; these were retained and the full validation/build pass succeeded.

Comparison with the accepted artifact confirms unchanged production source,
assets, blueprints and version metadata. All 135 package names are identical;
only README, CHANGELOG, INSTALLATION-COMPATIBILITY and the DLL's build provenance
differ. This preparation build is not the final merged release artifact.
Evidence remains under `artifacts/qualification/0.0.117/public-release`, including
`release-preparation-build.log` and `preparation-verification.json`.
