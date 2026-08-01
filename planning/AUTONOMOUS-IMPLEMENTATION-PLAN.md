# Autonomous base Gunslinger implementation plan

## Governing contract

This plan implements `AUTONOMOUS-GUNSLINGER-MISSION.md` on
`codex/complete-gunslinger`. Sprint boundaries are checkpoints only. The next
slice is always selected from the mandatory coverage matrix after the previous
slice passes its applicable source, package, and runtime gates.

## Dependency order

1. Qualify the reconstructed Sprint 30 generic firearm action runtime in game.
2. Add the production early pistol, musket, and blunderbuss catalog.
3. Implement scatter and close-range firearm behavior.
4. Add multi-round capacity, partial reload, and the advanced catalog.
5. Add the level 1-20 Gunslinger class chassis and progression integration.
6. Add grit and the reusable deed framework.
7. Implement and qualify every deed tier, bonus feats, Nimble, Gun Training,
   True Grit, and the capstone behavior.
8. Integrate acquisition, presentation, migration, compatibility, and release
   documentation.
9. Run the final comprehensive scenario twice from fresh Steam-launched
   processes and seal the release evidence.

## Checkpoint acceptance template

Each checkpoint records the authoritative rule, any Kingmaker adaptation,
observable behavior, focused deterministic tests, required runtime evidence,
non-goals, and fail-closed/rollback behavior in its entry criteria or report.
It then runs repository validation, the complete dependency-free domain suite,
a clean Release build, strict package validation, deterministic qualification,
scenario WhatIf, repository audits, and the guarded runtime scenarios required
by the changed behavior. Only a passing checkpoint is committed.

## Current checkpoint

Sprint 35 grit resource and reusable deed framework. The first checkpoint adds
the bounded dependency-free resource rules, daily reset, maximum reconciliation,
atomic spend/restore, and operation deduplication. Next bind these rules to an
exact persistent Kingmaker per-unit ability resource, then add daily-rest and
firearm critical/killing-blow recovery contracts. Scatter content remains
fail-closed pending an authoritative numeric Blunderbuss cone distance.
