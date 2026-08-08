# Pistolero and Musket Master Implementation Report

Status: mission-planning checkpoint only; implementation has not begun.

The feature branch starts from exact merged compatibility baseline
`10b792735db5d685b46749dc08ea819f31fa8052`, version 0.0.72. The intended
architecture, adaptations, replacement rows, compatibility boundaries, and
verification gates are fixed in the durable mission and replacement matrix.

This report will be updated at every coherent implementation checkpoint with:

- exact shared refactors and preserved behavior;
- new blueprint symbols and stable identities;
- installed Kingmaker API/IL contracts used;
- starting-equipment and ownership transaction evidence;
- deterministic, Release, package, runtime, and compatibility results;
- explicit base-starting-firearm implement/defer decision;
- remaining uncertainty and human-only observations.

No completion claim is made at this checkpoint.

## Publication hard stop

Local durable-document commit `c962e33` is clean and ready to publish. The
required approved helper refused the feature branch because its external
allowlist does not contain `codex/pistolero-musket-master-archetypes`. No raw
push or policy workaround was attempted. Implementation has not begun, so no
partially registered blueprints or runtime behavior exist.

Required human action: update the repository push-helper allowlist for the
exact feature branch, then rerun the approved helper and verify the remote SHA.
