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

That structural qualification subsequently passed on committed source
`a0d8e7752281d4ae1e51ce094c1226f6a30faf16` as guarded run
`20260811T2021406071785Z-observe-expanded-summoning-inventory`: exact registry
1,374, 67 units, 1,045 abilities, 681 placements, 182 executions per template,
six template buffs, two smite markers, and all sanitizer checks passed. The
preceding fresh launch exited during platform initialization after request
acceptance and before blueprint inspection; it was retained as failed evidence
and the evidence-supported retry passed. Neither run accessed a save. Runtime
unit-alignment fidelity remains open and is not claimed.

Spawn-local alignment is now source-qualified. A custom native post-spawn
action sets the new unit descriptor rather than changing a shared blueprint.
Celestial and fiendish summons preserve the unit's law/chaos axis while
replacing its moral axis; all Nature's Ally placements copy the actual caster's
exact alignment from the ability context. Missing or invalid context fails
closed without mutation. Repository validation, 1,005 tests, clean Release,
and strict package validation pass; committed-source structural observation
and later actual-cast/save-load proof remain pending.

The first alignment-aware structural run failed and was retained as
`20260811T2031332882968Z-observe-expanded-summoning-inventory`. It proved that
the generic graph clone had preserved references within native action lists,
allowing post-spawn actions to accumulate across abilities using the same
native quantity template. The initial repair explicitly cloned each
`ActionList`; the following run narrowed the deeper alias to its GameAction
elements. The observer now also rejects any KMG
post-spawn action or buff in a non-KMG ability. The repair is source-qualified;
its committed-source rerun remains pending.

That rerun (`20260811T2037058339804Z-observe-expanded-summoning-inventory`)
proved the ActionList-only repair incomplete: 286 non-KMG abilities still
contained KMG actions. Exact assembly inspection established that every
`GameAction` is itself a `SerializedScriptableObject` and was being returned by
the clone's Unity-object preservation guard. The follow-up now creates and
recursively copies each GameAction while still preserving immutable referenced
Unity assets. This deeper repair is source-qualified; its committed-source
rerun remains pending.

The deeper repair was proved by
`20260811T2043115519772Z-observe-expanded-summoning-inventory`: contamination
of non-KMG abilities fell from 286 to zero. Remaining assertion failures were
limited to the observer's incorrect one-action-per-ability assumption; native
quantity templates may contain multiple spawn nodes. The observer now requires
one matching alignment/template/smite action per actual spawn branch. This
cardinality correction is source-qualified and awaits its committed rerun.

That rerun passed as
`20260811T2048003275107Z-observe-expanded-summoning-inventory` on committed
source `b88e99cffb7464d7354416fba82d1da313e17ae2`. All celestial, fiendish, and
Nature's Ally actions matched exact native spawn-branch cardinality, and
non-KMG action contamination was zero. All registry, placement, template,
smite, and sanitizer assertions remained green. No save was accessed.
