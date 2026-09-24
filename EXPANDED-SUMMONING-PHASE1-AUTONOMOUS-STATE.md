# Expanded Summoning Phase 1 (charter Sprints 3-8) - autonomous state

Durable state for the Sprints 3-8 mission under the owner's order of
2026-09-24 ("Finalize Expanded Summoning Phase 0, Then Complete Sprints 3-8").
Read this first; it is the record the journal and the report summarise.

## Authority

- Order activated 2026-09-24 after the owner's manual review of the Phase 0
  Pteranodon candidate passed (revision unspecified). OwnerDelegationGranted
  for intermediate technical, menu/usability and visual acceptance; internal
  engineering, evidence and visual review between sprints; no routine
  confirmations, keyboard assistance, art choices or permission requests.
- Labels: OwnerDelegationGranted; InternalAcceptance per item; HumanReview:
  NOT_PERFORMED_NONBLOCKING for every piece of new work here.
- Hard limits: never write valued campaigns or protected test baselines
  (`KMG_AUTOMATION_BASELINE` is never selected or modified); no force-push,
  reset/clean of foreign work, credential or account changes, OS/game/Unity
  upgrades, purchases, or new paid fallbacks; proprietary captures stay in
  permitted local evidence locations; no release, deployment or Sprint 9.
  This PR stays a draft and is never merged under this order.

## Branch and baseline

- Phase 0 merged into `master` as `b0641a58` (PR #21 at reviewed head
  `ba5e20a7`, 2026-09-24T16:55:50Z). This branch,
  `codex/expanded-summoning-phase1-sprints3-8`, was created from that commit in
  its own worktree (`.worktrees/expanded-summoning-phase1`); the only draft PR
  for Sprints 3-8 is opened from it.
- Live installation must end at 0.0.117 / 136 files /
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after
  every guarded run; the restoration records verify it.
- Integrated master verification on `b0641a58`, from this fresh worktree:
  `Build-Local.ps1` PASS (1774 domain tests, 0 failures; package
  `KingmakerGunslinger-0.0.138-local-runtime.zip` SHA-256
  `056d19ea274f478d07d3c152e017db5054432c80fd60b210ebbfb54606ad7e9e`, DLL
  `05ab10e50806f518fd2ad9e6535c211ae485530b4b09e61865ea9367c0ecd1e4`);
  guarded smoke `20260924T1710306893099Z-observe-expanded-summoning-inventory`
  PASS 38/38 on `b0641a58`, live tree restored and verified
  (`20260924T1714020506347Z`). Two local, git-ignored prerequisites had to be
  carried into the fresh worktree before the tree built: `GamePath.props`
  (the private extracted-reference paths) and `artifacts/inspection/`
  (the IL inspection the bodyguard domain tests read). Neither is part of
  the tree; the first two verification attempts failed on their absence, not
  on the merge.

## Scope (25 creature work items)

| Sprint | Creatures (SM / SNA) | Signature |
|---|---|---|
| 3 | Pony I/I, Horse II/II, Owlbear -/IV, Cyclops -/V, Frost Giant VIII(retained wrapper)/VII (reuse unit) | native publication; Flash of Insight bounded |
| 4 | Shambling Mound -/VI, Giant Flytrap -/VII, Purple Worm -/VIII | grab/constrict/swallow on shared grapple infrastructure |
| 5 | Dust, Ice, Magma, Ooze, Salt, Steam Mephits IV/IV | distinct visuals, icons, breath, SLAs; no Lightning Mephit |
| 6 | Monitor Lizard, Grizzly Bear, Dire Bear (grab); Giant Spider (ranged Web, tremorsense, bounded climb only if feasible); Pixie (sleep arrow, dance) | signature mechanics repair |
| 7 | Leopard, Lion (+visual), Dire Lion, Dire Tiger/Smilodon | pounce / grab / charge-only rake |
| 8 | Tiger -/IV new; Cheetah authentic visual + bounded per-summon sprint | cat family completion |

Placements propagate to the 1d3 / 1d4+1 tiers. No feats, aquatic, mounts,
familiars, variant elementals or later-sprint creatures; Dire Bat stays
hidden; Eagle and Roc are not redesigned. Identities are reused, the ledger is
append-only, stable identities register even when publication is disabled,
and no future roster data is published.

## Sprint ledger

| Sprint | State | Evidence |
|---|---|---|
| 3 | NOT STARTED | - |
| 4 | NOT STARTED | - |
| 5 | NOT STARTED | - |
| 6 | NOT STARTED | - |
| 7 | NOT STARTED | - |
| 8 | NOT STARTED | - |

## Verified facts carried from Phase 0 (do not re-derive)

- Catalog pins at the start of Phase 1: 67 creatures, SM 66 / 361, SNA 57 /
  320, 681 logical placements (14 suppressed, 667 published), 182 templated
  placements, 26 natural profiles, 77 project icons, 8 view-scale entries,
  26 native wrappers, foundation identities 1184. Every one moves with the
  roster and is pinned in the domain tests, the manifest tool, the runner and
  the static validation.
- The native grapple lives in `ContextActionGrapple` (caster and target
  buffs), `UnitPartGrappleInitiator` / `UnitPartGrappleTarget` and
  `UnitGrappleController`; swallow whole in `ContextActionSwallowWhole`,
  `UnitPartSwallowWhole` / `UnitPartSwallowed` and `SwallowWholeSettings` on
  the view. The native Grab feature `efc1e80fb41e06544be46604983806d6`
  carries Shambling Mound constrict and target-state behaviour and is not
  reused as-is.
- Attack rules expose `RuleAttackWithWeapon.IsCharge`, `IsFullAttack`,
  `IsAttackOfOpportunity`, `AttackNumber`; `RuleAttackRoll.AutoHit`,
  `AutoCriticalThreat`, `AutoCriticalConfirmation` are settable; `UnitAttack`
  builds `AttackHandInfo` lists in `InitAttacks` / `CreateFullAttack`.
- A swapped material must be reset intact and adopted by the view's
  `StandardMaterialController` (`ReinitMaterials`) or the game's fades never
  reach it (Phase 0 closeout finding).
- The Frost Giant's native summoned unit is `590cd3d5e76fdc649a5f97bc984cd3c4`.
- The game's blueprints are not readable offline; native donors are found by
  the mod-load audit scenario `observe-expanded-summoning-native-donors`,
  which writes metadata only.

## Next executable action

Record the integrated master verification, open the draft PR, then Sprint 3:
run the native-donor audit, choose donors, add the five creatures to the
catalog, profiles, donors, ledger, icons and tests, and qualify them.
