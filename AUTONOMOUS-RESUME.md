# Sprint 24 theropod dinosaurs — IN PROGRESS

The owner selected the complete Deinonychus and Tyrannosaurus engineering and
release campaign on HOWARD-LAPTOP. Work is on
`codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus`, based on current
released master `e825411a8f920fdb9447f7ad48d3f7d0267e6e32`. Stable v0.0.153
is the verified baseline; v0.0.154 was free at intake but must be rechecked at
publication. Sprint 25 has not been started.

The primary Pathfinder First Edition rules, original charter pages 21–22,
current repository policy, Sprint 22/23 reuse seams, and actual machine state
have been read. The compact source contract and requirement-to-proof map are in
[the Sprint 24 coverage record](docs/SPRINT24-COVERAGE.md); exact intake and
policy identities are in
[the machine-readable ledger](planning/SPRINT24-THEROPOD-DINOSAURS-LEDGER.json).
Both new catalog keys must remain unpublished until qualification.

Fresh intake evidence is local-only at
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint24-deinonychus-tyrannosaurus-intake-20261014T050745Z/intake.json`
(SHA-256 `c54cfea1d5ee6577760ec710d37167951d2367a3e12088b8f823244c5f54a450`).
It records 968 Mods entries, installed KMG 0.0.117 with 136 files, 83 settings,
118 saves, the protected baseline SHA-256
`cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`,
and authorized working-save SHA-256
`795d59ff448173c6631518676b655d3bd03b31dc31e418f578f9e9488cb10ada`.
There was no Kingmaker process, runtime transaction, lock, game staging, or
publisher checkout.

The external push wrapper changed only by adding the two owner-authorized
Sprint 24 branches. Its SHA-256 moved from
`688b12798eac1b46b63a28205acbb647825e371cd3e9ae5e4db0ee13610d2fb2`
to `9354658937f2e679d3b77493a8bc56eed2395a53b8da3614b4e2cd5a05246542`;
removing those exact two lines reproduces the original hash.

Next action: finish the exact builder/component/identity and asset seam audit,
append stable identities once, implement both hidden profiles and focused
tests, then make the first coherent pushed checkpoint and open one draft
product PR against master. Do not run the broad Sprint gate yet. Do not begin
Brachiosaurus, Elephant changes, companions, polymorph, mounts, feats, or any
Sprint 25 work.
