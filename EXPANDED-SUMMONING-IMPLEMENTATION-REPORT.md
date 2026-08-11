# Expanded Summoning implementation report

Status: active; implementation and qualification are not yet complete.

Selected baseline: `origin/master` at
`2894d9fcce250708e354894ffd8e1be9c7493b9b`, containing required
`e4d560f8dd2909518614e3a20e77ba4d70dadeb8`. Release baseline: 0.0.77.

No release or runtime qualification claim is made at this checkpoint.

The mandatory Shield Other prerequisite is source-qualified: established links
no longer depend on distance, while close initial targeting and all other
lifecycle rules remain intact. Repository validation, 981/981 domain tests,
clean Release build, and strict 0.0.77 package validation pass.

Current implementation checkpoint: the complete frozen 681-placement catalog,
67 summon-unit identities, 1,045 abilities, six HD-banded celestial or
fiendish template buffs, and two bounded smite markers are registered
deterministically. The exact template
SR threshold is implemented as low (0-4 HD, no SR), mid (5-10 HD, CR+5 SR),
and high (11+ HD, CR+5 SR), with resistance/DR values 5/5/10. The ledger is
1,375 stable IDs: 1,374 active and one reserved. Source qualification passes
1,004 tests, clean Release, and strict package validation.

The six-buff graph subsequently passed guarded fresh-process run
`20260811T1954362756414Z-observe-expanded-summoning-inventory` on committed
source `d384ba06cf76896543a6b23ed480d3f6715bbba2`, including exact low/mid/high
mechanics, all aligned execution counts, registry 1,372, and 681 live parent
placements. The run was save-free.

The exact optional smite inventory finished on
`20260811T2008011506353Z-observe-expanded-summoning-inventory`: the native-like
optional graph uses a fixed one-use resource but applies a permanent non-child
buff to its target. Reusing that graph would allow external target state to
outlive the summon. KMG instead implements smite as a unit-local marker: the
first successful attack against the opposed alignment receives nonnegative
Charisma to attack and HD to damage, consumes the marker, and creates no target
state. This intentionally omits manual swift target selection and persistent
bonuses against one selected target, a conservative non-overpowered lifecycle
adaptation. Its committed-source structural runtime qualification remains
pending. Runtime unit-alignment fidelity also remains open and is not claimed.
