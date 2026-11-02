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
