# Firearm native weapon rigs implementation report

## Outcome

Mission initialized. No firearm is yet enabled as a new custom candidate and no
human visual gate is claimed.

## Starting identity

- Source: `codex/firearm-wwise-audio` / `2d9d95c8b0f919fb5f129c783522608bc47e2029` / `0.0.70`.
- Feature: `codex/firearm-native-weapon-rigs` in isolated repository-local worktree.
- Baseline rig-manifest SHA-256: `326E3B59A0FF869D8BA570F2A01C5D6137F828CC3FAA652CC9191309779B219D`.

## Exact Kingmaker findings

Pending exact local assembly and donor inspection.

## Root causes

Starting evidence identifies non-semantic pivots/grips, inconsistent source
units, absent native support-hand IK, and coupled animation/projectile/holster
lifecycle as the likely causes. Exact findings remain pending.

## Implementation

The Unity builder now uses explicit per-prefab rig specifications and produces
identity grip roots with `Visual`, `Muzzle`, and long-gun `SupportHandTarget`.
Runtime loading validates each equipped model independently before publication.
It rejects nonidentity roots, absent/invalid hierarchy, nonfinite transforms,
bad +Z muzzle/support ordering, cameras/lights, and invalid materials. For every
validated long gun it attaches the exact installed `EquipmentOffsets` component
when absent and assigns `IkTargetLeftHand` to the authored support target.
Capabilities publish transactionally beside the prefab dictionaries.

Presentation readiness is explicit (`NativeFallback`, `AutonomousCandidate`,
`HumanAccepted`) and custom selection also requires a validated runtime
capability. All five weapons currently remain `NativeFallback`; no human status
is assigned.

The new allowlisted `observe-native-firearm-rig-contracts` scenario is save-free
and records exact native Light/Heavy Crossbow visual parameters, hierarchy,
renderers, attach slots, projectile count, belt/sheath identity, EquipmentOffsets,
left-hand IK target, all custom capability diagnostics, fallback readiness, and
transient cleanup.

## Rejected experiments

None. A sandbox Git ref-lock denial was an environment permission issue, not a
weapon-rig experiment.

## Automated evidence

Baseline ancestry and isolation pass. Source/build/runtime evidence for new rig
work is pending.

## Regression evidence

The starting commit carries the qualified Wwise/projectile/fallback evidence
documented in `docs/FIREARM-WWISE-AUDIO-IMPLEMENTATION-REPORT.md`. No new
regression claim has been made.

## Candidate identities

No candidate package exists for this mission yet.

## Human acceptance required

See `docs/FIREARM-NATIVE-WEAPON-RIGS-MANUAL-ACCEPTANCE.md`. All checks remain
pending.

## Known limitations

Exact native signatures, authored rigs, runtime structural qualification, and
human visual judgment are all pending.

## Next action

Inspect exact installed Kingmaker rig contracts and native crossbow donors, then
add the guarded non-mutating forensic scenario.
