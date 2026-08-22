# Runtime qualification policy

This document is the authoritative runtime-coverage policy for the combined
package.

Exhaustive `2^N` enumeration belongs in fast domain and source tests for
configuration, migration, publication-plan, gating, and rollback logic. Those
tests may evaluate every Boolean configuration without launching Kingmaker.

The standard cross-module runtime qualification is the boundary matrix:

- all modules ON;
- all modules OFF;
- each module ON alone; and
- each module OFF while every other module is ON.

For `N` active modules this boundary contains exactly `2N + 2` states. It is
authoritative because, for Boolean module settings, it covers every possible
one-, two-, and three-module value combination. Bodyguard and In Harms Way add
the ninth module, so the current runtime boundary contains 20 states.

Add focused higher-order combined profiles only when a concrete architectural
reason suggests an interaction among four or more modules. An exhaustive
`2^N` game-launch matrix is not required for feature implementation,
module addition or removal, settings-schema changes, publication-plan changes,
shared bootstrap changes, or final release sealing. It may be authorized only
when specific defect evidence genuinely depends on higher-order combinations;
even then, test the smallest relevant interaction family.

For documentation-only changes, run no game process.

For icon-only changes, run package/icon validation plus focused all-ON asset
loading.

For model, material, grip, donor, or bundle-only changes, run focused family
visual-contract scenarios, Eastern Weapons ON/OFF, all modules ON, and the
highest-risk combined compatibility profile.

For a single module's mechanics or selector publication, run focused mechanical
scenarios, relevant optional-mod profiles, persistence when affected, and the
authoritative boundary matrix.

`Invoke-FeatureModuleRuntimeMatrix.ps1` derives the boundary from the
authoritative active-module catalog by default. `-Boundary` remains an explicit
spelling. The historical `-Boundary14` spelling is a deprecated compatibility
alias: it selects the same complete generic boundary and warns that its numeric
name is obsolete. `-Combination` runs one explicitly named focused state. The
runtime launcher deliberately exposes no generic exhaustive mode.

Build, test, package, validate, back up, and deploy exactly once per immutable
source commit. Reuse the exact installed artifact across every matrix launch,
verifying its commit, version, package SHA-256, DLL SHA-256, DLL MVID, installed
DLL hash, deployment-manifest identity, and package-manifest identity.

Matrix evidence is resumable only when every immutable artifact, game, optional
dependency, and original-settings identity matches. Package and CotW settings
must be restored byte-for-byte after qualification, including failure and
interruption paths.

Relevant external-mod configurations such as CotW normal, CotW balance-fixes,
and CotW absent remain focused compatibility profiles, not Cartesian-product
dimensions. Persistence remains focused on modules whose changes affect
persisted identities or state.

Human acceptance applies only to the exact immutable artifact reviewed. A
source or packaged-artifact change invalidates that acceptance and requires a
new candidate. Final release sealing reuses the accepted focused and boundary
evidence; it does not add an exhaustive game-launch requirement.
