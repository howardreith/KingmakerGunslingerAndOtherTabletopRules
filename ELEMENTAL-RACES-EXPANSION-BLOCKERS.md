# Elemental Races expansion blockers

## Active hard blockers

None established.

## Active publication-process blocker

- Local checkpoints through heritage reconciliation qualification commit
  `aca9aece0933d4713d5eae5cd98e1097fca52325` cannot currently be pushed.
  The mandatory external `Push-KingmakerGunslinger.ps1` wrapper refuses the
  exact required `codex/elemental-races-expansion` branch because it is absent
  from the wrapper's branch allowlist. The historical `codex/elemental-races`
  branch is allowlisted, but changing this mission's branch would violate the
  assignment. No bypass is attempted. Independent implementation continues.

## Open engineering questions requiring evidence

- The installed native/Wrath heritage SLA donors are resolved. Release B/C
  audits remain open for the flight abstraction, concealment and breathing
  catalogs, ray deflection, summon typing, and difficult terrain.
- Dirty Trick is implemented only if a genuine native player-facing combat
  maneuver path can be identified and qualified.
- Visual Adjustments remains NOT-RUN unless it is actually installed when the
  relevant compatibility checkpoint is executed.

These are investigation items, not hard stops. Features fail closed only under
the mission's hard-stop contract; independent work continues.

## Resolved foundation limitations

- Kingmaker 2.1.7b exposes only `RuleCalculateAbilityParams.AddBonusDC(int)`;
  it cannot attach `ModifierDescriptor.Racial` to that event. Exact
  nonduplication is enforced by the one-result affinity policy across the
  effective parent/variant chain.
- Raw `UnitUseAbility.CanStart` does not include resource availability.
  Player-path availability is the native combined
  `AbilityData.IsAvailable && CanStart` boundary and is qualified as such.
- Viewless chargen fixtures cannot safely evaluate `CurrentSpeedMps`.
  Movement qualification uses real native buffs/conditions plus the installed
  `CalculateSpeedModifier` contract; final in-area heritage qualification
  remains required in Release A.

## Resolved Release A implementation findings

- Native Aasimar has no runtime race icon. Hydraulic Push now uses the exact
  native Feather Step icon only as a presentation fallback; all other racial
  SLAs retain their exact donor icon. Live blueprint evidence proves every
  selection, marker, and SLA icon is non-null.
- The first heritage probe was missing central runtime-catalog membership; the
  request was rejected before hooks or state access. A focused regression now
  covers the allowlist entry.
- The first accepted probe exposed the null-icon bootstrap failure and exact
  owned-registration rollback. The repair is narrow and the subsequent live
  bootstrap passes. Neither finding is an active blocker.
- The first live heritage-mechanics run exposed a real marker-first hydration
  defect: inherited General providers could activate after an alternate marker
  had reconciled, leaving both provider sets active. One owned controller on
  the existing trailing heritage-selection fact now performs a post-race
  reconciliation. The corrected guarded run passes this order for all four
  races while preserving spent-resource bookkeeping.
- Call of the Wild installs a broad sticky-touch prefix that removes native
  `UnitPartTouch` state when its own multi-charge part is absent. Chill Touch's
  exact project charge state therefore declares `HarmonyBefore("CallOfTheWild")`
  and returns before that foreign prefix. Guarded evidence proves 20 -> 19
  charges and exact retained touch state for living and undead targets. The
  live audit checks Harmony's declared `before` metadata; its raw registration
  collection is insertion ordered and is not an execution-order report.
- Native Blur's party-member target checker uses `IsPlayerFaction`. The
  save-free Mistsoul fixture now uses the existing player faction without
  entering the party or touching a save, matching that native predicate.
