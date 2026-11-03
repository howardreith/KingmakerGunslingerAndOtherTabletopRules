# Sprint 38 status — Glabrezu

**IN PROGRESS.** Not released, not qualified, nothing claimed as proven on a
frozen artifact. Sprint 37 is closed at v0.0.175 and is not reopened.

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

* The engine has **no gravity and no levitation concept at all** — zero
  occurrences of `gravit` or `levitat` in 103 MB of IL, against working
  positive controls. A faithful implementation is impossible.
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

## Not started

Registration and the demon-catalog extension, the chassis and rend, Power Word
Stun, the support and offensive spell-likes and their AI, the body and icon,
the Sprint 38 scope and observers, all native qualification, the gate, the
release and the records. Sprint 39 and the Nalfeshnee remain NOT_STARTED and
are out of scope.
