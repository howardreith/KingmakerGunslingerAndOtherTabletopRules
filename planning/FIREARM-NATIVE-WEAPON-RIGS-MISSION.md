# Firearm native weapon rigs mission

## Durable objective and authority

Implement Owlcat-native firearm presentation while preserving the qualified
Kingmaker mechanical pipeline. Every firearm is a complete rig: an identity
dominant-hand grip root, source-specific `Visual`, barrel-opening `Muzzle`, a
long-gun `SupportHandTarget` wired through exact Kingmaker
`EquipmentOffsets.IkTargetLeftHand`, a separately gated belt/back policy, and a
validated animation/attach-slot policy. Models must flow through
`WeaponVisualParameters.Model` and native hands-equipment lifecycle; no second
weapon may be spawned on a guessed hand transform.

Starting identity: repository `howardreith/KingmakerGunslingerAndOtherTabletopRules`,
qualified source `codex/firearm-wwise-audio` at
`2d9d95c8b0f919fb5f129c783522608bc47e2029`, version `0.0.70`, Steam App ID
640820, Unity 2018.4.10f1 project
`C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1`.
Mission branch is `codex/firearm-native-weapon-rigs` in the repository-local
isolated worktree `.worktrees/firearm-native-weapon-rigs`.

Automated evidence may establish structure, lifecycle, mechanical safety, and
package identity. It cannot establish acceptable grip, clipping, apparent
scale, pose, animation quality, or timing. Those remain explicit human gates.

## Frozen contracts

- Preserve Light/Heavy Crossbow mechanical ancestry and native delivery.
- Preserve exactly one cloned firearm projectile and its callbacks; hide only
  its renderers through the exact firearm-projectile hook.
- Preserve touch-AC, reload/ammunition, capacity/misfire/condition/repair,
  Grit/deeds, Scatter mechanics, feat integration, persistence, and Wwise bank,
  events, lifecycle, and exactly-once committed discharge posting.
- Never introduce direct damage/raycast delivery, Unity `AudioSource`, global
  weapon-scale patches, native donor mutation, or whole-character renderer
  name scanning.
- Do not change damage, range, handedness, action economy, or balance.

## Phases and checkpoints

- [x] Verify exact qualified ancestry and establish isolated feature branch.
- [ ] Commit and publish durable mission/checkpoint documents.
- [ ] Inspect exact installed Kingmaker rig contracts and native Light/Heavy
  Crossbow donors; add guarded non-mutating donor observation.
- [ ] Replace positional bundle-builder arguments with deterministic per-prefab
  rig specifications and deliberate source-unit normalization.
- [ ] Build identity-root equipped rigs, Muzzle points, long-gun support targets,
  exact `EquipmentOffsets` integration, separate holster policy, rig manifest,
  and deterministic double-build validation.
- [ ] Introduce per-weapon `NativeFallback`, `AutonomousCandidate`, and
  `HumanAccepted` readiness with independently gated equipped, belt, animation,
  attach/quiver, and muzzle-effect capabilities.
- [ ] Add the development-only, session-local calibration lab with safe world
  and doll refresh, diagnostics, markers, allowlisted animation selection,
  deterministic export/import/promotion, and reset controls.
- [ ] Qualify Musket first, then Blunderbuss, Rifle, Pistol, and Revolver, using
  independent calibration and native fallback on individual failure.
- [ ] Replace/retire broad renderer scanning with exact firearm-slot
  sheath/quiver lifecycle handling and hidden holster when no candidate passes.
- [ ] Run focused tests, repository validation, complete clean Release domain
  suite, exact-reference build, deterministic Unity builds, output/package
  validators, frozen regressions, guarded structural scenarios, critical
  scenarios twice, and final working-save smoke when authorized.
- [ ] Produce exact hashes/package identity, manual checklist, implementation
  report, matrices/provenance/changelog updates, final coherent commits, and
  policy-script publication. Never merge.

## Weapon order and candidate policy

Musket proves the Heavy Crossbow/Crossbow two-hand contract. Blunderbuss and
Rifle reuse only the schema, never Musket transform values. Pistol and Revolver
must be unit-, pivot-, orientation-, scale-, and holster-correct before testing
`PiercingOneHanded`, `Fencing`, `Dagger`, then inherited Crossbow. Never allow
`ThrownStraight`. A structurally qualified model may be visible as an
`AutonomousCandidate`, but only a curated human record can set
`HumanAccepted`. Belt/back candidates are independent; hidden is preferable to
broken placement.

## Required evidence discipline

The append-only journal records UTC time, pre-experiment branch/commit,
question, exact assemblies/assets, changed files, commands, evidence paths and
hashes, result (`pass`, `fail`, `ambiguous`, or `reverted`), meaning, and next
action. `AUTONOMOUS-RESUME.md` must lead with current branch/commit/phase, last
passing checks, candidate package identity, and next command. Only genuine
unresolved blockers go in `AUTONOMOUS-BLOCKERS.md`. Packages, runtime evidence,
private references, Unity Library/Temp/Logs, saves, proprietary assemblies, and
machine-local configuration are never committed.

## Runtime safety

All launches use guarded `-kmgRuntimeTestRequest` tooling through Steam App ID
640820. Never launch `Kingmaker.exe` directly and never use Computer Use, OCR,
screenshots, or mouse coordinates as mechanical proof. Never overwrite
`KMG_AUTOMATION_BASELINE`; use only authorized disposable fixtures or
`KMG_AUTOMATION_WORKING`. Fail closed on wrong identity, ambiguous save/UI,
missing prerequisites, unsafe Steam dialogs, or ambiguous evidence.

## Hard stops

A hard stop exists only after safe alternatives are exhausted and one of these
is precisely evidenced: qualified ancestry cannot be isolated without
overwriting unrelated work; required exact private references fail provenance;
Unity 2018.4.10f1 is absent after all safe source-only work; Steam presents a
prohibited environment; the exact native contract is proven absent after all
safe native-compatible strategies; continuing requires destructive Git,
protected-save mutation, proprietary redistribution, unauthorized install or
network access, or unrelated-project modification; or the repository remains
uncompilable after multiple evidence-based strategies isolate a minimal cause.
At a hard stop, finish safe docs/tests, update journal/blockers/resume/report,
commit/push coherent state if gates permit, and name the exact failed command,
evidence, and smallest human action.

## Definition of done

Done requires current durable mission/forensics/journal/report/resume/blockers
and manual checklist; exact local signatures; deterministic structured rigs and
source-unit rationale; identity grip roots; Muzzle on every firearm; native IK
support targets on all long guns or a proven blocker; explicit per-weapon
readiness and safe non-null fallbacks; exact projectile/Wwise preservation;
exact firearm-scoped holster/quiver lifecycle; tested non-destructive calibration
lab; five independently calibrated candidates with allowlisted short-gun
animation qualification; deterministic Unity double-build; all source, domain,
Release, package, runtime, regression, and cleanup gates; exact final hashes;
truthful separation of automation from human judgment; pushed branch; no merge.

