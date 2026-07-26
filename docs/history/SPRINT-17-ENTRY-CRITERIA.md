# Sprint 17 entry criteria and branch decision

## Branch A — persistence GO

Sprint 17 may add Black Powder Charges and Lead Balls only when a locally compiled Sprint 16 UMM candidate has produced an evidence session with:

- decision `Go`;
- all 30 Critical rows passing;
- two distinct passing run IDs for I03, I10, I11, I13, I15, I19, and I23;
- no unexplained identity transfer, state loss, duplication, or migration corruption;
- the exact candidate ZIP, hashes, runtime-contract report, environment fingerprint, logs, and exported evidence retained.

The ammunition sprint must not modify the accepted persistence carrier. Its bounded scope is stackable powder/projectile inventory blueprints, an inventory query service, and atomic non-combat consumption tests. Reload actions remain a later sprint.

## Branch B — qualified candidate fails or remains incomplete

When the evidence decision is `NoGoFailed` or `NoGoIncomplete`, Sprint 17 remains a persistence or runtime-qualification sprint. It must:

1. name the exact failed or blocked row;
2. preserve the build fingerprint and evidence artifacts;
3. reproduce the failure where the matrix requires two runs;
4. change only code justified by the observed Kingmaker behavior;
5. preserve all stable blueprint IDs and readable legacy carriers;
6. update migrations and the matrix before retesting.

It must not use buffs, wielders, names, blueprint keys, equipment slots, inventory indices, runtime hashes, generated IDs, or diagnostic sidecar files as authoritative firearm state.

## Branch C — no compiled candidate yet

If no locally qualified Sprint 16 UMM ZIP and no runtime evidence exist, Sprint 17 must not add ammunition. Work remains bounded to making the qualification path compile and run against the actual installed Kingmaker, UMM, Harmony, Unity, and Newtonsoft.Json assemblies.

## Build gate

A milestone is **READY FOR KINGMAKER** only when it includes a compiled `KingmakerGunslinger.dll` inside a validated UMM-installable ZIP produced against the installed runtime assemblies.
