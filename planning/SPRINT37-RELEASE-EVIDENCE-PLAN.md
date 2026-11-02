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
build. ON and OFF differ only by a wrapper-level switch.

**That last sentence was wrong when it was written, and is retracted.** I
claimed the shape needed orchestration this mission had not established. It
did not: `Invoke-Sprint22InstalledStack.ps1` deploys the package it is given
once per invocation and hands the harness that exact deployment through the
guarded reuse path, so any number of invocations can bind to one release
candidate, and the module-off switch is already one of its parameters. The
obstacle was mine, not the machinery's.

### The owner's decisions, 2026-11-02

Recorded here because this file posed the evidence question and must not be
left reading as though it were still open. The gameplay decisions and their
full reasoning live in `SPRINT37-RULES.md`.

**The strict contract stands.** Every mandatory row is supported by fresh
execution against the final frozen release artifact. No `REUSED_PASS`
substitution is authorized for this sprint, and the publisher check refuses
one. Three distinctions the contract depends on:

* Reusing the exact package bytes across many fresh executions is required
  efficiency, not reuse of a verdict. One build, many launches.
* One correctly observed execution may support several requirements when its
  captured facts establish each of them. Launching once per assertion to make
  a table look fresh is waste, not rigour.
* The development-candidate receipts remain historical and supporting
  evidence. They are not final rows and are not rewritten into new-source
  execution.

**Spores ships with one explicit counterplay omission** — the printed
holy-water application — with its other three printed counters required to
pass final-artifact evidence. **Automatic Screech stays conservative**, with
manual use preserved through the real action bar rather than fixture dispatch.

## Two observer corrections this file records

**The save-or-lose cells needed a bounded natural-twenty retry.** The combined
mechanics group on candidate `ff52c3ff` failed exactly one cell,
`screech-two-owner-isolation`, and its receipt showed every ownership fact
exact: the hour marker on the first vrock, none on the second, the first
ability unavailable, the second available, and no victim effect attributable
to the second. The sole shortfall was `victimEffectsFromFirst: 0` — the victim
had saved. A natural twenty always succeeds, so a cell that requires a failed
save fails a correct creature at a predictable rate; this was luck, not a
product finding.

`screech-identity-exemption` had the same construction and worse odds: it
needs the enemy, the summoner's own ally and the bone devil to all fail one
Fortitude save, so roughly one run in seven would have failed it, and it had
been passing on luck. Both cells now carry the bounded retry the released
Sprint 36 burst observer established — at most three attempts, retried only
when every deterministic fact was already exact and the sole shortfall was a
natural twenty, every attempt recorded with its d20 and DC, `forcedRolls: 0`,
and an explicit retry rule. No roll is seeded and no assertion is relaxed;
exhausting the attempts still reports the cell failed, with its whole history.

`screech-live-constitution-dc` and `screech-radius-boundary` read the
difficulty class and whether a save was rolled rather than its result, so
neither needed changing, and the spores, core, geometry and AI phases involve
no save at all.

**The pre-build sequencing error recurred.** The rule recorded above —
the release candidate must be built by the guarded harness inside the run —
was broken a second time, at commit `8cee019d`. `Assert-KmgReleaseScratch`
again refused correctly, with the live tree untouched and nothing deployed.
As with `0b7f159d`, that artifact's bytes are preserved rather than deleted or
moved; the output directory is keyed by commit, so the harness builds the
final candidate at the commit that carries this correction, and the earlier
two artifacts simply stand unused. The remedy is the one already written here,
applied again, not a new mechanism.

## The finish sequence

1. The complete source, build and package gate, once, by the harness inside
   the first run — freezing source state, package SHA-256, DLL SHA-256, MVID,
   manifest, reference identities and the separately recorded tooling identity.
   A release-candidate label alone is not a passing gate.
2. The combined mechanics group on that exact package: vrock controls, the
   three supported Spores counters, the self-buffs, the corrected Babau
   dispel follow-up, and the combat-mode, AI and action distinctions.
3. The ON persistence envelope: prepare, a fresh enabled load with verify and
   cleanup, and a fresh absence check, restoring the original Working bytes
   and metadata at the envelope boundary.
4. The OFF persistence envelope: prepare with the module on, load and cleanup
   with it off, absence with it on again, on the same package through the
   supported configuration switch.
5. Final integration and public coverage — all three public Summon Monster
   routes with their actual quantities, action, slot, duration and cleanup,
   and the packaged body and icon.
6. Exact reproducibility through the existing supported path, keeping the
   primary qualified package rather than duplicating an unchanged gate.
7. Publication without a release rebuild: publisher refusal and preflight,
   premerge authority, an ordinary merge, promotion of the exact tested ZIP,
   and verification of the public downloads, checksums, manifest, DLL, MVID,
   tag target and ancestry.
8. Restore and close: full intake, protected saves, the original Working and
   baseline fixtures, no owned game, lease, task or live staging, and the
   human installation preserved.

A deployment manifest is authority for its own active transaction. After a
restoration the next process gets a fresh transaction and deployment receipt
against the same package; a live-installation authority is never carried
across restored transactions.

Nothing is merged, published or tagged, and no guard has been weakened or
bypassed, until every row above genuinely passes.
