# Sprint 16 entry criteria

## Branch A — persistence GO

Sprint 16 may add Black Powder Charges and Lead Balls only when a compiled Sprint 15 UMM package has produced an evidence session with:

- decision `Go`;
- all 30 Critical rows passing;
- two distinct passing run IDs for I03, I10, I11, I13, I15, I19, and I23;
- no unexplained identity transfer, state loss, duplication, or migration corruption;
- the exported JSON and Markdown evidence retained with the build fingerprint.

The ammunition sprint must not modify the accepted persistence carrier.

## Branch B — persistence NO-GO

When the report is `NoGoFailed` or `NoGoIncomplete`, Sprint 16 remains a persistence sprint.

It must:

1. name the failed or blocked row;
2. preserve the evidence artifacts;
3. reproduce the failure where required;
4. change only code justified by the observed runtime behavior;
5. preserve all stable blueprint IDs and readable legacy carriers;
6. update the matrix and migration plan before retesting.

It must not fall back to buffs, wielder state, blueprint keys, names, slots, inventory indices, runtime hashes, guessed IDs, or diagnostic sidecar files as authoritative firearm state.

## Build gate

No milestone is READY FOR KINGMAKER until it includes a compiled DLL in a UMM-installable ZIP produced against the installed Kingmaker, UMM, and Harmony assemblies.
