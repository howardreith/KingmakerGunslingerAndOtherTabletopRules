# Sprint 38 status — Glabrezu

**RELEASED at v0.0.176.** Sprint 38 is closed. The tested source is
`e4c4d77a88ca19c3d22c0a516a645669d43eb64c`, the product merge and tag are
`6370addc14a438fc80adf54e9b0062bd5777ef99` (PR #92), the published package is
`7c32c32cded0d4cffaeceebe0d6765bbf35caac3a5ca8da45dd053ec86aa5d82`, and the
records merge is `c69212d6c7871c8f3ebf043bfffa53ee3851455c` (PR #93). The
authority for the final state is the **Release qualification, 2026-11-04**
section below and `SPRINT38-RELEASE-EVIDENCE.json`. Sprint 37 is closed at
v0.0.175 and is not reopened.

The banner above this line previously read "IN PROGRESS. Not released, not
qualified, nothing claimed as proven on a frozen artifact." That was true when
written and became stale when the sprint released; it was corrected as a
documentation-only carry-forward during Sprint 39 intake, with no rebuild and
no change to any released byte. Every dated section below is preserved exactly
as recorded, including its failures.

## Working identities

| Identity | Value |
|---|---|
| product branch | `codex/expanded-summoning-sprint38-glabrezu` |
| records branch | `codex/expanded-summoning-sprint38-release-records` (not yet created) |
| product/build/native-test/promotion root | `C:/Dev/KingmakerGunslingerLab/wt/s38` |
| base commit | `2af6f60858b23fef58e981da78725c5e6d8cde73` (= `origin/master`, verified) |
| expected version | `v0.0.176`, only if actually available at publication time |

The promotion root is fixed **now**, before any native qualification, because
this repository's exact-reference build is byte-deterministic within a
repository root but not across roots, and the publisher can only promote from
the root where the artifact was built.

## Intake completed

* `AGENTS.md` and `docs/VALIDATION-POLICY.md` read; this is an explicitly
  selected sprint/release mission, so the ordinary 30-minute validation ceiling
  does not gate each authorized stage.
* `planning/SPRINT37-STATUS.md` and `planning/SPRINT37-RULES.md` read as the
  structural template. Sprint 37's recorded defects in its own evidence
  machinery are treated as recently repaired code with provenance, not as a
  mature safeguard by assertion.
* Charter row cross-checked against `EXPANDED-SUMMONING-IDEAL-ROSTER.md` and
  `EXPANDED-SUMMONING-CHARTER-TRACEABILITY.md`. **The charter document itself
  is not on this machine**; the mission's quoted Sprint 38 requirements are
  used, and the two ledgers agree with them.
* Printed profile transcribed from the live AoN entry rather than from recall
  or from a sibling demon's constants. It differs materially from the older
  3.5-derived Glabrezu that a reader might expect: CR 13 with 12d10+120,
  Dex 11 with no Dex contribution to AC, bite 1d8 rather than 2d6, and
  Vital Strike among the feats.
* `scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.sprint37'` run in
  this worktree: production compiled, 2,796 registered tests, 10 selected,
  0 failures. The promotion root's toolchain is confirmed.

## Push policy

The two authorized exact allowlist additions were made to
`C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1`:

| Item | Value |
|---|---|
| backup | `Push-KingmakerGunslinger.ps1.sprint38-backup-20261103T050547Z` |
| hash before | `0bd2b5120c5079d62edad0f78b26999dd6b59d0a9079a328b08deb2c3cd4e5e5` |
| hash after | `a5d018819216483c00c10354bcba6902e0d2b1c908983f79756069495e792358` |
| added | `codex/expanded-summoning-sprint38-glabrezu`, `codex/expanded-summoning-sprint38-release-records` |

Exactly two lines were inserted, 9,688 bytes to 9,793. The file's mixed line
endings were preserved byte-for-byte outside the addition (CRLF count
unchanged at 4; LF-only count up by exactly 2). No wildcard and no broadened
root. `scripts/Test-PushPolicyWrapper.ps1` passes: 38 isolated cases, fake Git
only, refusal-before-mutation asserted, and its negative control still
correctly rejects a push-before-refusal wrapper. HEAD descends from the
wrapper's pinned qualified baseline `2d9d95c8`.

## Early bounded probe

> **Historical, 2026-11-03.** This section and the two that follow it
> (**Review corrections** and **Progress**) record intake and in-flight state
> as it stood while Sprint 38 was open. Their "still UNKNOWN", "outstanding"
> and "pending" statements describe that moment, not the closed sprint. All of
> them were resolved before release; the final state is the **Guarded mechanics
> qualification** and **Release qualification** sections below. Retained
> verbatim, failures included, as the record of how the sprint reached its
> decisions.

The mission's three intake questions, and where each stands.

### 1. A suitable existing four-arm body/rig — **resolved, reuse available**

Sprint 19 is the charter's "Four-Arm Rig" sprint and its released assets are
`assets/sprint19-fourarmed/{girallon,xill}-{albedo.png,mesh.json}`. The body
pipeline is **project-owned, procedural and code-authorable**: each creature
has a Blender 4.5 generator under
`assets-source/original-models/<sprint>/`, and the Sprint 37 script
`generate_sprint37_vrock.py` composes shared modules from the serpent and
theropod pipelines over a measured rig capture.

Verified present on this machine:

| Input | State |
|---|---|
| `C:\Program Files\Blender Foundation\Blender 4.5\blender.exe` | present |
| measured rig capture `sprint24-theropod-census.json` (E: archive) | present, SHA-256 `727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`, **matches** the value Sprint 37 recorded |
| procedural icon renderer `assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py` | present, 2,162 lines, Cycles, outputs the 1254x1254 RGBA source that `tools/New-ExpandedSummoningIcons.ps1` reduces to the 128x128 production icon |

This supersedes the earlier understanding that creature icons and albedos
required an image-generation session. They are rendered by this project's own
tools from procedural geometry and procedural materials, so the art is in
scope for a code-only session. The icon count pin is currently 145 and moves
to 146 only with the real rendered source and its 128x128 export, both hashed
in `icon-manifest.json`.

The charter's rejection of unrelated humanoid/spider-rig shortcuts is binding:
the Glabrezu gets its own geometry — two large pincer limbs, two smaller claw
arms, a readable head and trunk, and a coherent Huge footprint — and is not
assigned to an unsuitable rig.

### 2. Native Power Word Stun and support-spell graphs — **still UNKNOWN, by design**

The three installed mods' `loaded_blueprints.txt` dumps looked like a
no-launch blueprint census. They are not: they contain only each mod's own
additions, proved by the absence of all three known-native GUIDs the Sprint 37
audit recorded. The four Power Word Stun rows in them belong to
CallOfTheWild. See `SPRINT38-RULES.md`, "What the retained dumps do and do not
prove".

So every blueprint identity this sprint needs is routed to one bounded
installed-input observation, on the Sprint 37 pattern: Power Word Stun,
Reverse Gravity, Create Pit and its pit visual settings, True Seeing, Veil,
Confusion, Chaos Hammer, Unholy Blight, Dispel Magic, plain Vital Strike, and
any native Glabrezu unit. No GUID is guessed and no absence is asserted from
a mod's dump.

### 3. Bounded Reverse Gravity feasibility — **resolved technically; one owner decision outstanding**

Settled offline from the retained `Assembly-CSharp.il`, with no guarded
transaction spent. Two findings, both in `SPRINT38-RULES.md` in full:

* A substring search for `gravit` and `levitat` over 103 MB of
  `Assembly-CSharp.il` returned **zero occurrences**, against working positive
  controls. That is a result about those two names in that assembly at that
  moment: it establishes that no gravity or levitation concept is reachable
  under either name, and it is **not** a proof that the engine expresses such a
  concept nowhere by any other means, nor that a faithful implementation is
  universally impossible. *(Narrowed during Sprint 39 intake. The original
  wording concluded "The engine has no gravity and no levitation concept at
  all — A faithful implementation is impossible." The search result itself is
  unchanged and is retained above; only the inference drawn from it is
  corrected. Reverse Gravity was omitted by the recorded owner decision of
  2026-11-03, a balance and fidelity choice, and that decision does not rest on
  this inference.)*
* The engine **does** ship a complete, persisted, bounded vertical-capture
  framework: `AreaEffectPit` with `UnitPartInPit` and `UnitInPitController`
  already provide a point-anchored finite area, a **Reflex save against the
  ability's own DC** to avoid capture, a +2 edge bonus, size-cap and
  immunity-fact exemptions, auto-failure when helpless or force-moved,
  transit damage, a per-round hook, a separate end-of-effect action for units
  still held, and JSON-persisted capture state. Its vertical term is
  `FlyHeight = -yCurve(t) * DepthMeters * torsoScale`, so a **negative**
  `DepthMeters` on a project-owned visual-settings blueprint lifts the
  captured unit instead of sinking it, and release already restores
  `FlyHeight = 0` and the original position.

So a bounded Reverse Gravity is reachable by reusing an existing native
framework rather than writing any motion system. What it cannot match is
recorded honestly: `AreaEffectShape` offers only `Cylinder`, `Wall` and
`AllArea`, so the printed shapeable 10-ft-cube-per-level region is not
expressible; there is no overhead-surface detection, so transit damage must
come from a chosen lift height; and the native framework always offers a
climb-out the printed spell does not.

That last set is a balance and fidelity choice, not an engineering one, so it
was put to the owner as a single concrete decision with the exact feasible
effect, the severity options and every difference from print stated up front.

**Owner decision, 2026-11-03: omit Reverse Gravity and disclose it.** It is
not represented, nothing stands in for it, and the `AreaEffectPit` framework
is not used by this sprint. This is recorded as a **charter change rather than
a deferral under the charter** — the Sprint 38 row selects a bounded
adaptation rather than permitting deferral, and the option the owner chose
said exactly that. The feasibility evidence is retained, because it is what
made the decision informed and it establishes the omission as a balance choice
rather than an engineering dead end.

A second question confirmed the project's engine-limit disclosure standard was
acceptable. With omission selected there is no bounded effect for those
specific disclosures to attach to, so that answer is **not** recorded as
authorizing a bounded Reverse Gravity.

Consequence for the rest of the sprint: Power Word Stun is now the sprint's
single required signature ability, and the pincer rend and five-limb routine
carry the melee leg. No other requirement changes.

## Review corrections, 2026-11-03

A review of the pushed head identified four concrete gaps. All four are
corrected offline, before any further guarded candidate.

### The failed bootstrap was not an input probe

The guarded run `20261103T0651149368893Z` deployed, launched, started the game
and restored the live tree to its exact pre-run hash - and then failed at mod
initialization, so the scenario never executed. It is recorded as
**NOT_COMPLETED**, not as native input discovery and not as any kind of
qualification.

The cause is a hard registration dependency rather than anything about the
observation: `InfernalProfile36` calls
`ExpandedSummoningProjectIcons.Require(creatureKey)` while building the unit
type, and `ExpandedSummoningProjectIcons.Load` calls
`SummonIconCatalog.Validate()`, which derives its expected key set from the
roster. So the painted icon is required the moment the creature is
**registered**, and no guarded scenario of any kind can run until it exists.
That is the same dependency the two failing offline tests name; the launch
added nothing the offline failures had not already said, and it is not
repeated while the input is still missing.

### Reflex +4 is correct; the explanation was not

The earlier record claimed PF1 outsiders have three good saves and therefore
filed the printed Reflex +4 as a published stat-block anomaly. The premise was
wrong: the type grants **two** good saves, per-creature, and this entry's are
Fortitude and Will with a poor Reflex. All three printed totals derive exactly
on the standard progressions at 12 hit dice, so the shipped +18/+4/+11 were
always right and nothing about them is anomalous. The false explanation and the
assertion premised on it are gone; the values are unchanged.

### Rend now enforces the round, not the command

The claim that command resets prevented two rends in one round was false, and
the released source says so: `ClawRendTracker.Follow()` calls `Reset()` on a
sequence change, clearing `_emitted` along with the claw slots, so the released
gate's limit is per *command* and `OnNewRound` only clears what a command change
would have cleared anyway.

`GlabrezuRendGate38` is a Glabrezu-local replacement that holds the round
allowance as its own state with **no attack-command operand at all**, and
scopes the victim bookkeeping to the round as the printed universal rule does.
That makes the limit real in one direction and removes an under-trigger in the
other: two pincer hits on one victim in different commands inside one round
correctly rend. The round boundary is the installed `ITickEachRound.OnNewRound`,
raised by the base engine's `UnitTicksController` outside turn-based combat and
by the turn controller inside it, so it covers both modes without a fixture
flag. Only an armed attack that actually hit spends the allowance.

The released Dire Ape and Girallon keep `DireApeRendGate` and their behaviour
unchanged. A domain test asserts the released tracker's command-change
reopening directly, so the reason for the split is evidence rather than prose.

### Power Word Stun's AI decides the actual candidate

The eligibility consideration was attached only to `ActorConsiderations`, whose
engine tooltip is "Considerations that only depend on deciding unit". That left
a real selection gap: admitted because enemy A is eligible, selector picks
enemy B, single daily use spent for no effect. Suitability now also sits on
`TargetConsiderations`, which `AiAction.ScoreTarget` evaluates per candidate and
multiplies into that candidate's own score, so an unusable candidate is removed
rather than discouraged. Both paths call one shared predicate. Target protection
uses the released `SummonDescriptorImmunity` pair, because
`UnitPartSpellResistance.IsImmune` answers only for
`SpellImmunityToSpellDescriptor`; numerical spell resistance is deliberately not
consulted during scoring.

### The two caster levels, followed through

The pre-cast estimate really does report the creature's 12 hit dice:
`RuleCalculateAbilityParams..ctor` reads
`GetComponent<ContextSetAbilityParams>()`, the derived type, so it never sees
this project's `ContextCalculateAbilityParams`. But
`MechanicsContext.RecalculateAbilityParams` reads the
`ContextAbilityParamsCalculator` **base** type and does honour it, and
`AbilityExecutionProcess.PrepareCast` calls `context.Recalculate()` before any
effect is applied. So the execution context carries 14, and
`RuleSpellResistanceCheck.SpellPenetration` - which is that context's caster
level - is the printed 14 rather than 12. Range is a separate fact entirely:
the installed `GetRange` maps `AbilityRange.Close` to a flat 30 feet and reads
no caster level at all, so the printed 60 feet at CL 14 does not exist in this
engine and is recorded as a range compression. Both caster levels still get
recorded at their consumers in the native run.

Offline state after the batch: the complete domain suite is 2,800 registered,
2,800 executed, **two** failures, both the same icon dependency above.

## Progress, 2026-11-03

Registration, the demon-catalog extension, the chassis, the five printed
attacks, the rend gate, Power Word Stun at its selected 60-foot CL14 custom
range, the offensive and at-will spell-likes with their AI considerations, the
icon and the original four-armed body are all in source and pass the focused
suite and the complete unfiltered domain suite (2,800 registered, 2,800
executed, 0 failures).

### The repository wrapper is version-gated, and that is expected here

`tools/validate_repository.py` selects a release validator by the current
`Info.json` version. That is still `0.0.175`, whose validator pins the exact
reviewed Sprint 37 product and therefore refuses any in-progress Sprint 38
source:

`Sprint37 release175 FAIL: Product/project/assets/identities differ from reviewed source`

That is the version boundary doing its job, not a Sprint 38 regression. It was
confirmed by reproducing the closed-Sprint-37 check in its own isolated clone
at the reviewed merge `2af6f608`, where the product diff and the untracked list
are both empty and the reviewed refusal suite passes. The wrapper, and so the
`-Level Sprint` gate, goes green only once the version is bumped to `0.0.176`
and `tools/validate_expanded_summoning_sprint38_release176.py` exists with the
final product head pinned — which is the candidate-freeze step, exactly as
Sprint 37 did it.

Until that freeze, the real mid-sprint proof is `-Level Focused` plus the
complete unfiltered domain suite run as
`scripts/test-domain.ps1 -SkipRepositoryValidation`. No qualification level
reaches the complete suite without the wrapper, so this substitution is only
valid before freeze and is never used as release evidence.

## Guarded mechanics qualification, 2026-11-03

Six phases, each a separate guarded transaction against a development
candidate, each restoring the live mod tree to its exact pre-run hash
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

| Phase | Rows | Result |
|---|---|---|
| `sprint38-glabrezu-core` | bootstrap, native appearance, five primary attacks, defences, feats and constant sense, attack fallback | PASS |
| `sprint38-glabrezu-rend` | limbs, round clock live, live attack command, pair rends once, other limbs never, per-victim history, round-not-command | PASS |
| `sprint38-glabrezu-stun` | printed range, every bucket, immune recipient refused, penetration at the printed caster level, Vrock screech verification | PASS |
| `sprint38-glabrezu-spells` | at-will carries no resource, at-will repeats, bursts at printed difficulty | PASS |
| `sprint38-glabrezu-ai` | second enemy is real, candidates differ, the word targets the affectable candidate | PASS |
| `sprint38-glabrezu-public` | route scope, own 128px medallion with no fallback, real player route | PASS |

### What the runs found, and what changed because of them

1. **The authored body was not reaching the view.** The mesh loaded cleanly -
   `visual:published`, 3,130 vertices, the 512px albedo - and the live view read
   `donor-visual:donor-renderer-count=2`: the Sprint 38 policy was missing from
   the view patch's measured-theropod-donor branch and fell through to the
   single-renderer default. A published visual status is not proof the body
   shipped; the installed view is.
2. **Reach is read where it is printed.** The live *equipped* attack range is
   22 on this Huge body against a printed 15. The printed value is the weapon
   type's configured range, and the released Sprint 29 Huge giants already
   record that distinction, so the cell asserts the configured range on every
   limb plus one shared live reach.
3. **Perception is 28, not the printed 26.** The installed native Persuasive
   feat adds +2 Perception in this game as well as its Persuasion bonus, and
   this creature's printed feat list includes Persuasive. Disclosed in
   `planning/SPRINT38-RULES.md` and pinned exactly, with no unattributed
   modifier permitted.
4. **The round's rend allowance was never coming back.** The engine dispatches
   `ITickEachRound` through `unit.Logic` - both the base `UnitTicksController`
   and the turn-based `TurnController` call
   `unit.Logic.CallFactComponents` - so a component carried by a FEATURE never
   receives it, and the owner sat in real combat for 975 unpaused frames with
   its rend spent. The gate now stamps the round from game time, which advances
   in both combat modes; `OnNewRound` is kept and still resets. Scoped to
   Sprint 38: the released rend and stun consumers with the same shape are not
   touched.
5. **One stun reported twice is not two stuns.** The engine raised
   `RuleApplyBuff` twice for one application, both with the same eleven-round
   duration, so the cell counts carrier instances on the recipient instead.

### The narrow authorized Vrock check: nothing changed

Measured against two real registered high-resistance non-demon recipients, the
Trumpet Archon and the Astral Deva, both at spell resistance 25:

- the released Vrock screech declares **no** spell resistance, so
  `AbilityExecutionProcess.DoApplyEffect` never raises a resistance check for
  it. The reported concern does not reproduce;
- Power Word Stun, measured the same way as the resistance-applies neighbour,
  declares spell resistance and penetrates at the printed caster level 14 - not
  the 12 hit dice a pre-cast preview reports.

No global resistance bypass, no Sprint 37 replay, and no change to any released
byte. Sprint 37 remains closed.

## Release qualification, 2026-11-04

### The candidate, and why it moved four times

A release candidate is frozen bytes, and the publisher requires the artifact
head and a clean working tree to be exactly that commit. Four source
corrections arrived after the first freeze, so the candidate was rebuilt and
every mandatory native row re-run against the new bytes each time rather than
carried over. That is the contract working: no Sprint 38 row may be reused, and
a row bound to other bytes is not evidence.

| Candidate | Why it was superseded |
|---|---|
| `cb7d8e9d` | version bump only; the validator pins it as the reviewed product head |
| `5419dc95` | the compiled request validator refused the new persistence scope |
| `88b20ec8` | the view patch's donor-policy lists were missing Sprint 38 |
| `f091879a` | the prepared stun left an owned unit unable to act at the save boundary |
| `dd9218ef` | the publisher named the persistence assertions without the runner's prefix |
| `e4c4d77a` | **the released candidate** |

### What the release runs found

Every one of these was found by a guarded run against real bytes, and every run
restored the live mod tree to its exact pre-run hash
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

1. **The game refused the new persistence scope.** The compiled request
   validator kept its own chain of eighteen scope inequalities, and Sprint 38
   was absent: declared in the policy, dispatched by the runner, admitted by
   both PowerShell gates, and refused in-game after a real launch with
   `summoning-persistence-scope-invalid`. No hook was installed and no save was
   touched. The validator now asks the one authoritative list.
2. **One donor-policy list existed six times.** The authored body had loaded
   cleanly and the live view still read `donor-visual:donor-renderer-count=2`;
   later, the owned-resource receipt for a correctly attached body came back
   empty. Five of the six copies now ask one predicate. The sixth is left alone
   deliberately, because it is a different set and asking the predicate there
   would widen it.
3. **The prepared stun collided with the shared readiness contract.** Every
   owned unit must be able to act and move at the save boundary, and this
   fixture may write only to units it owns, so the only Power Word Stun
   recipient available is a second owned glabrezu - which a real stun
   legitimately stops from acting. The stun is now proven while it stands and
   withdrawn before the save; what crosses the save is the spent single use,
   the recipient's hit points and the constant sense.
4. **The publisher named assertions the runner does not write.** The
   roster-persistence scenarios prefix every assertion with
   `whole-roster-persistence-`, so requiring the bare cell name refused a
   complete envelope with "does not name its own saved state".

### One interrupted transaction, and how it was closed

The first attempt at the ON envelope was interrupted: the orchestrator process
ended while the game it had launched was still running, leaving the release
candidate deployed and the compatibility transaction open. It was closed
explicitly rather than left or worked around - the game was closed, the
transaction restored through the dedicated compatibility restore entry point
and verified, and the installed mod confirmed back at 0.0.117 with all eleven
mods present. The authorized Working fixture was untouched by it: the in-game
request had been refused before any hook was installed or any save written, and
its bytes still matched the captured original exactly.

A second aborted attempt did write a prepared save, and the Working fixture was
restored from its capture with a boundary proof naming the restart before the
envelope was retried.

### Working fixture boundaries

The authorized Working fixture was captured before any envelope at
`650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`, 8,550,225
bytes, and restored with bytes, last-write and creation time verified at three
boundaries: the aborted ON restart, ON to OFF, and after OFF. Nothing else in
the save directory was written: 118 entries before and after, with the baseline
save unchanged at `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`.

### Release and public verification

Stable **v0.0.176** is published from the exact tested ZIP, promoted without
rebuilding:

| Fact | Value |
|---|---|
| tag | `v0.0.176`, not a draft, not a prerelease, marked latest |
| tested source | `e4c4d77a88ca19c3d22c0a516a645669d43eb64c` |
| product merge | `6370addc14a438fc80adf54e9b0062bd5777ef99` (PR #92), and the tag is that merge |
| package | `7c32c32cded0d4cffaeceebe0d6765bbf35caac3a5ca8da45dd053ec86aa5d82` |
| assembly | `5b81f5f5cc314252b7cc7647cc63f1c76223679a73678f27d7b8e4ff626911a1` |
| module version id | `dd4a21a8-c4f4-44ff-b0d9-16fb14d53a62` |
| full suite | 2,801 registered, 2,801 executed, 0 failures, unfiltered |

The published asset was downloaded again from the public release and checked
rather than assumed: its bytes, the checksum file served beside it, the release
manifest, the assembly inside the archive, that assembly's module version id,
the `Info.json` version, the tag being the product merge, the tested source
being an ancestor of the tag, and that the archive carries no `scripts/` entry.
All nine agree.

### Closing machine state

Read back after publication and the public-download check: the live mod tree at
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`, the
owner's own 0.0.117 build installed with assembly
`fd2fc61c250b13857d81acc197a896450f5b242ee392fa7192b01201e908f35f`, 968 manifest
entries across eleven mods, 118 saves with the Working fixture at its captured
original bytes and the baseline untouched, no Kingmaker process, no leftover
staging directory and no compatibility lock. This is the state Sprint 37 left,
unchanged.

No owner aesthetic approval is claimed. **Sprint 38 is RELEASED. Sprint 39 and
the Nalfeshnee are not started.**

## Not started

Nothing in Sprint 38 remains. Sprint 39 and the Nalfeshnee are NOT_STARTED and
out of scope.
