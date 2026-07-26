# Sprint 18 entry criteria and branch decision

## Goal

Produce the first genuine Kingmaker compile candidate and begin runtime evidence collection without adding new firearm gameplay content.

## Branch A — private runtime references supplied

When a valid Sprint 17 private reference bundle is available, Sprint 18 will:

1. Verify every file against `reference-manifest.json`.
2. Compile all main-project C# sources against .NET Framework 4.7 and the exact supplied Kingmaker/UMM assemblies.
3. Treat all warnings as errors.
4. Resolve every API or type mismatch rather than suppressing it.
5. Produce and validate a UMM-shaped ZIP containing only project-owned files.
6. Label the resulting artifact **READY FOR KINGMAKER SMOKE TEST**.
7. Run or have the owner run I01/I02 and preserve the generated logs/evidence.

## Branch B — references unavailable

If the private bundle is still unavailable, Sprint 18 must remain on evidence preparation only. It may improve validation and documentation, but it must not:

- Claim a main-mod compile.
- Produce a fake or stub-linked UMM package.
- Add ammunition or reload mechanics.
- Advance the persistence gate.

## Branch C — main compile fails

If exact-reference compilation fails, Sprint 18 is bounded to the observed errors. It will patch only the failing runtime contracts and repeat compile/test until either:

- A clean compile candidate is produced; or
- A specific incompatible Kingmaker/UMM contract is documented as a blocker.

## Ammunition gate

Black Powder Charges and Lead Balls remain blocked until a real Kingmaker evidence session evaluates to `Go` across all required Critical rows. A successful compile or I01/I02 pass alone is insufficient.
