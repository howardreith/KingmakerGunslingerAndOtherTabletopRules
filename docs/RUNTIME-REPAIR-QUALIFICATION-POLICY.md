# Runtime repair qualification policy

Ordinary repair iterations use risk-shaped runtime coverage. They do not run
the exhaustive 64-state feature-module matrix unless shared module authority,
schema, bootstrap, registration, gating, rollback, or module membership changed.

- Documentation-only: no game process.
- Icon-only: package/icon validation and focused all-ON asset loading.
- Model/material/grip/donor/bundle-only: focused family visual contracts,
  Eastern Weapons ON/OFF, all modules ON, and the highest-risk compatibility
  profile.
- One module's mechanics or selector publication: focused mechanics, relevant
  optional profiles, affected persistence, and the 14-state boundary matrix:
  all ON, all OFF, each of six modules ON alone, and each OFF with the other five
  ON.

`Invoke-FeatureModuleRuntimeMatrix.ps1 -Boundary14` constructs exactly those 14
states. `Invoke-KingmakerRuntimeTest.ps1 -ReuseInstalledArtifact` skips build
and deployment only after `Assert-KmgReusableDeployment` verifies the clean Git
commit, version, package SHA-256, DLL SHA-256, DLL MVID, installed DLL hash,
firearm-bundle hash, exact live path, and current settings hash. The deployment
manifest schema records those immutable identities plus the settings identity
preserved during the one deployment.

Build, complete tests, deterministic packaging, strict validation, backup, and
deployment occur once for an immutable commit. Every later launch reuses and
re-verifies that installation. A caller must supply the exact package and
deployment manifest; missing, stale, dirty, mismatched, or old-schema evidence
fails closed. The existing non-reuse mode remains for a standalone canonical
run and retains its guarded single build/deploy boundary.

Matrix resumability is intentionally not inferred. A resumed controller must
separately prove the same immutable deployment and original settings identity;
otherwise it starts a new settings transaction. Human visual iteration stops
before any final exhaustive release seal. If such a seal is genuinely required,
it runs once after human acceptance.
