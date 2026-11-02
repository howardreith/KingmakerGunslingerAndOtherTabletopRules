# Sprint 37 release evidence plan

Why this file exists: the publisher contract for this sprint admits no reused
coverage row, so every mandatory proof must be fresh against the final release
candidate. Development-candidate receipts cannot reach the closure check at
all — `Assert-KmgClosureProofFiles` refuses any proof whose text carries
`"candidateKind": "development"` unless a named sprint allows a reused row, and
Sprint 37 allows none. That is a deliberate consequence of registering a new
creature.

## What the development runs did and did not establish

Twenty-one guarded transactions ran against development candidates. Each took
its own snapshot of the live mod tree and restored it to the exact pre-run
hash `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`; no
lease was left open and no save was touched.

They were development qualification, and they earned it:

* they found the unpackaged body — `assets/sprint37-vrock/` existed in the
  repository and in no installed mod, which no source test could have caught;
* they found the Disease descriptor on the wrong blueprint, which would have
  left native Remove Disease unable to strip the vines;
* they found five defects in the observer itself, one wrong assertion about AI
  opening moves, and one unsatisfiable cell ordering;
* and they established every mechanics behaviour the two reviews asked for.

None of their receipts can qualify the release. That is the charter working as
designed: a development candidate is never a release.

## Sequencing, and the error this file records

The release candidate must be built **by the guarded harness inside the run**,
not beforehand. `Invoke-KingmakerRuntimeTest` deploys and builds when it is
given no artifact-reuse authority, and `-ReuseInstalledArtifact` exists to
reuse a deployment that harness itself produced — it requires both the exact
deployment manifest and the exact package.

Building the release candidate separately first was a sequencing error. Its
immutable-output guard then correctly refused to rebuild over
`artifacts/local-runtime/0.0.175/0b7f159dfcb4`, with the live tree untouched
and nothing deployed. That artifact's bytes are preserved rather than deleted
or moved: the output directory is keyed by commit, so the harness builds the
final candidate at this commit instead, and the earlier artifact simply stands
unused.

## Mandatory rows, and the phase each one takes its proof from

| coverage row | phase |
|---|---|
| `core-profile-and-self-buffs` | `sprint37-vrock-core` |
| `spore-protection-and-geometry` | `sprint37-vrock-spores` + `sprint37-vrock-geometry` |
| `screech-identity-and-recharge` | `sprint37-vrock-screech` |
| `both-modes-screech` | `sprint37-vrock-turns` |
| `observed-ai-and-babau-dispel` | `sprint37-vrock-ai` |
| `public-routes-presentation` | `sprint37-vrock-public` |
| `mixed-on-persistence` | three fresh processes: prepare, cleanup, absent |
| `mixed-off-persistence` | three further fresh processes, module disabled |
| `highest-risk-integration` | the affected released surface |
| `complete-source-build-package-gate` | the full gate at the final candidate |
| `exact-reproducibility` | rebuild to identical package and DLL hashes |

Six fresh processes across the two persistence envelopes, with exact
Working-fixture restoration, and nothing merged or published until every row
above genuinely passes.

## Where this stands, and the one decision left

### Observed on the release candidate cea8d8a5 — 31 of 32 cells

| phase | cells |
|---|---|
| core | 6 / 6 |
| spores | 9 / 9 |
| geometry | 1 / 1 |
| screech | 7 / 7 |
| AI incl. Babau follow-up | 8 / 9 |

The single failure was the observer's own liveness proxy, not the product: the
receipt recorded the enemy-cast control correct, the selector reporting the
effect not useful, and the dispel not chosen — and failed only because the
Babau started no other command inside that window. That proxy is replaced by
asking the production dispel action whether it considers itself available for
the decision context, which is direct evidence rather than an inference from
silence.

### Two structural facts discovered late

**Development candidates are no longer available.** They may only extend a
released version, and the version is now 0.0.175, which is unreleased. So
every further native iteration is a release candidate, and release outputs are
built once per commit by design.

**Sprint 36 bound seven rows to one candidate and three to others.** Its
`packageSha256` values show the fresh rows — public routes, both persistence
envelopes, integration, gate, reproducibility, profiles-registration — sharing
the final candidate, while `babau-combat-acid`, `hezrou-combat-control` and
`both-modes-lifetime` were `REUSED_PASS` against their own earlier artifacts
with exact attribution.

Sprint 37's contract as written forbids any reused row. That is stricter than
the precedent, and it forces every mechanics row onto the single final
candidate — which in turn requires the mechanics phase, the whole-roster
integration run, and both three-process ON and OFF envelopes to share one
build. ON and OFF differ by a wrapper-level switch and cannot share a batch,
so that shape needs orchestration this mission has not established.

### The decision

Either:

1. **Keep the strict contract.** Every mandatory row fresh against one final
   candidate. This needs the multi-batch-one-candidate orchestration to be
   established first, and costs more guarded transactions.

2. **Adopt the Sprint 36 shape.** Permit `REUSED_PASS` for the pure mechanics
   rows only, under the same attribution the Sprint 36 check enforces: a
   recorded prior source, production dependency paths unchanged by `git diff`
   since that proof, observer paths refused as dependencies, and each proof
   bound to its own exact package, DLL and MVID. Public routes, both
   persistence envelopes, integration, the gate and reproducibility stay
   fresh against the final candidate.

This is a question about what standard of evidence the release carries, so it
is the owner's to settle rather than the agent's. Nothing is merged, published
or tagged, and no guard has been weakened or bypassed to reach this point.
