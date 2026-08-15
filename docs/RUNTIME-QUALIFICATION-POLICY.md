# Runtime qualification policy

This document is the authoritative runtime-coverage policy for the combined
package.

Do not run the exhaustive `2^N` feature-module matrix during ordinary iterative
repairs unless the change modifies:

- `FeatureModuleConfiguration`;
- `FeatureModuleSettingsStore` or schema migration;
- `FeatureModulePublicationPlan`;
- shared blueprint registration or bootstrap;
- shared module gating or rollback;
- addition or removal of a module; or
- another cross-module authority whose correctness genuinely depends on every
  complete configuration.

For documentation-only changes, run no game process.

For icon-only changes, run package/icon validation plus focused all-ON asset
loading.

For model, material, grip, donor, or bundle-only changes, run focused family
visual-contract scenarios, Eastern Weapons ON/OFF, all modules ON, and the
highest-risk combined compatibility profile.

For a single module's mechanics or selector publication, run focused mechanical
scenarios, relevant optional-mod profiles, persistence when affected, and the
boundary matrix containing:

- all ON;
- all OFF;
- each module ON alone; and
- each module OFF while every other module is ON.

For `N` active modules this boundary contains exactly `2 + 2N` states. Brown-Fur
adds the seventh module, so the current boundary is 16 states. The exhaustive
matrix contains `2^7 = 128` states.

`Invoke-FeatureModuleRuntimeMatrix.ps1 -Boundary` derives both matrices from the
authoritative active-module catalog. The historical `-Boundary14` spelling is a
deprecated compatibility alias: it selects the same complete generic boundary
and warns that its numeric name is obsolete.

Build, test, package, validate, back up, and deploy exactly once per immutable
source commit. Reuse the exact installed artifact across every matrix launch,
verifying its commit, version, package SHA-256, DLL SHA-256, DLL MVID, installed
DLL hash, deployment-manifest identity, and package-manifest identity.

Matrix evidence is resumable only when every immutable artifact, game, optional
dependency, and original-settings identity matches. Package and CotW settings
must be restored byte-for-byte after qualification, including failure and
interruption paths.

During human iteration, do not perform a final exhaustive release seal before
human review. Brown-Fur pre-human qualification uses focused mechanics and CotW
contract profiles plus the complete 16-state boundary. After the exact immutable
candidate receives explicit human acceptance, reuse that artifact for the final
128-state matrix once. Any source change invalidates the prior acceptance and
requires a new immutable candidate.
