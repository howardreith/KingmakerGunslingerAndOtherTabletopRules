# Elemental Races 0.0.117 public release verification

Status: **PUBLISHED AND INSTALLED**. [Release v0.0.117](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.117) was published
at `2026-09-08T04:03:01Z` and is the latest stable release.
The final UMM installation and public download both match the runtime-qualified
release artifact. The sections below preserve the preparation history and add
its observed final verification.

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

## Published release and final installation

The validated preparation commit is `8e70ec500337429ad9ecec8154fde27464985ecc`.
Master was first fast-forwarded to the latest remote tip, then the accepted
feature branch was merged with an explicit merge commit:
`f8a2fd996752afb0e361a53bec175328ace5435a`. The required non-force push helper published master.
Annotated tag `v0.0.117` resolves to that exact merge commit locally and remotely.
No other feature branch was merged. Later documentation does not move this tag
or change the immutable release assets.

The guarded publisher ran twice from clean, fully pushed master: first to make
the draft, then with `-Publish -ConfirmReleaseReady` after runtime qualification.
Each invocation ran two clean builds, with all 1,458 domain/reflection tests
passing in each build and identical ZIP/DLL hashes. The canonical runtime
preflight passed 259 checks. The exact final publication command was:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-Release.ps1 `
  -ReferenceBundleDir C:/Dev/KingmakerGunslingerLab/private/extracted-references/KingmakerGunslinger-private-build-references `
  -Publish -ConfirmReleaseReady
```

| Final release identity | Value |
| --- | --- |
| Tag / artifact source | `v0.0.117` / `f8a2fd996752afb0e361a53bec175328ace5435a` |
| Informational version | `0.0.117-elemental-char-gen-stabilization` |
| ZIP SHA-256 | `9368c1ff2c82b76574bab5ed75868d7eb633e759f925e82a1c0da7e861f62f6f` |
| DLL SHA-256 | `fd2fc61c250b13857d81acc197a896450f5b242ee392fa7192b01201e908f35f` |
| DLL MVID | `18f5eaaa-3021-4836-8763-9ba22965b958` |
| Source-state SHA-256 | `c6dd37868df849d4feb29098779db60d03dc83c8f8070a98ef5265d6f66b48f7` |
| ZIP bytes / package files / manifest identities | 23,351,596 / 135 / 1,869 |

Installable archive:
`artifacts/release/0.0.117/KingmakerGunslinger-0.0.117-elemental-char-gen-stabilization.zip`.
The local-runtime and public-download copies are byte-identical. Compared with
the accepted candidate, only README, CHANGELOG, INSTALLATION-COMPATIBILITY and
the DLL's embedded build provenance differ. All gameplay source, version
metadata, assets and blueprint identities remain unchanged.

## Final-artifact guarded runtime verification

All **9 fresh Steam processes and 107 assertions PASS** on the exact release
artifact, with no failed release-runtime attempt. Each guarded launch verified
the commit, DLL hash/MVID, ZIP hash, owner context and Steam App ID 640820.
The release matrix uses the established reversible profile driver, with the
optional dirty-Git allowance removed; all runs require clean committed source.
Only `KMG_AUTOMATION_WORKING` is used for saved-world fixtures.

| Release check | Guarded evidence run | Assertions | Result / restoration |
| --- | --- | ---: | --- |
| creator-f-ifrit | `20260908T0257453655130Z-working-save-elemental-character-creation-regression` | 12 | PASS / exact |
| creator-f-oread | `20260908T0304180863244Z-working-save-elemental-character-creation-regression` | 12 | PASS / exact |
| creator-f-sylph | `20260908T0311107375788Z-working-save-elemental-character-creation-regression` | 12 | PASS / exact |
| creator-f-undine | `20260908T0317272351388Z-working-save-elemental-character-creation-regression` | 12 | PASS / exact |
| respec-f-sylph | `20260908T0323145173079Z-working-save-elemental-native-respec` | 12 | PASS / exact |
| respec-f-oread | `20260908T0335187632087Z-working-save-elemental-native-respec` | 12 | PASS / exact |
| traits-bodyguard-off | `20260908T0347586921758Z-disposable-elemental-character-creation-baseline` | 11 | PASS / exact |
| elemental-off-f | `20260908T0352561319890Z-elemental-races-races-unleashed-compatibility` | 13 | PASS / exact |
| smoke-f | `20260908T0355456836449Z-working-save-smoke` | 11 | PASS / exact |

The four full-stack creator runs complete twelve real characters, one per race
and heritage, across 32 native final reviews and 184 exact racial graph
observations. Ifrit and Oread use point-buy; Sylph Gunslinger and Undine Fighter
use Dice Roller. Both global Trait selections are observed and completed,
back-navigation preserves allocation baselines, and commits leave no unresolved
selections. The Bodyguard-OFF control reviews four native first-level builds;
it intentionally does not commit saved-world characters and is not counted
among the twelve completed characters.

The two full-stack Player respec runs include two initial seed characters and
fourteen actual native respec callbacks: sixteen commits and 128 exact racial
graph observations. All callbacks return to the original identity/descriptor.
Sixteen committed daily-resource records include ten spent-use records; every
amount equals its expected amount and every provider count is exact. All 128
blood-capacity observations match the expected expenditure, including nonzero
spent counters. Preview mismatches are zero. Ordinary rest restores the intended
uses and blood capacity after respec. Party, remote companions, inventory,
money, cross-scene entities and pause state clean up exactly.

The foreign-selector audit independently checks seven creator/respec profiles
at construction, reconciliation, active first-level global Traits and three
repeated reconciliation callbacks. Combat Features remains the exact original
empty array. With Bodyguard ON, AllFeatures preserves fourteen exact ordered
foreign choices and contains Helpful once; with Bodyguard OFF it retains the
original fourteen-entry array and Helpful is absent. Both top-level Trait
selectors preserve their exact objects and arrays, with eight categories each.
No foreign choice is lost, duplicated or reordered.

Every profile restores the previous mod tree, file hashes/timestamps, settings
and UMM state exactly. Protected baseline data remains exact; the disposable
working save changes only its ordinary load counter in the seven load-backed
runs. Other save-file metadata remains exact. No new character or fixture is
saved to a campaign by these release-sealing scenarios.

All nine logs have zero KMG ERROR lines. Each retains the same one native
BugReportCanvas startup exception and four known ZFavoredClass custom-data
exceptions. Those diagnostics remain separate from KMG failures; none is
suppressed or repaired. The accepted candidate's broader migration,
OFF/ON/rest/level/cleanup and nineteen-trait lifecycle matrix retains its own
artifact attribution and is reused only for unchanged gameplay source.

## Public download and local UMM verification

GitHub reports `v0.0.117` as the latest stable, non-draft, non-prerelease at
`2026-09-08T04:03:01Z`. The end-user download includes exactly the UMM ZIP,
`SHA256SUMS.txt` and `release-manifest.json`. All three GitHub asset digests match
the downloaded files; the checksum, manifest, ZIP CRC, embedded metadata,
DLL identity and strict package validation pass. The downloaded ZIP is identical
to the artifact used by all nine release runs. Publication evidence is recorded
in `published-release.json`, `public-download-verification.json`,
`download-package-validation.log`, `draft-publisher.log` and `publish.log`.

Backup-first installation completed at `2026-09-08T04:05:26.0367443Z` using the
existing `Deploy-Local.ps1` installer. The installed directory is:
`C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger`.

An independent comparison verifies all 135 package files byte for byte; the
only additional installed file is the preserved FeatureModules.json. UMM,
Elemental Races and Bodyguard Feats remain enabled. Module settings bytes and
timestamp are unchanged (SHA-256 `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`).
UMM settings bytes and timestamp are unchanged (SHA-256 `058de3da0ac8a070e448ee92cd3ed5fcdc89137867ae27fa802ef21c4a6e6646`).
All unrelated mod files and timestamps remain exact. The named save payloads
and other saves' metadata remain unchanged, with no save load during installation.
No game or active compatibility transaction remains.

The complete previous installation, including its settings backup and runtime
cache, is recoverable with exact file hashes/timestamps at:
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260908T0405189875596Z`.

Deployment record: `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260908T0405222302243Z\deployment.json`.
Local installation evidence: `installed-release.json`,
`installation-save-audit-after.json` and `final-installation.log`.
All machine-local artifacts remain ignored and uncommitted.

Owner acceptance and release authorization are complete as explicitly supplied.
Individual full-screen checklist results were not supplied and are not invented.
Treacherous Earth, Nereid Fascination, favored-class bonuses and other content
expansion remain deferred; this release concludes the authorized stabilization.
