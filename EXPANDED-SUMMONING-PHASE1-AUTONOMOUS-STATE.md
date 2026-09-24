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
| 3 | IMPLEMENTED; RUNTIME QUALIFICATION IN PROGRESS | see "Sprint 3 record" |
| 4 | NOT STARTED | - |
| 5 | NOT STARTED | - |
| 6 | NOT STARTED | - |
| 7 | NOT STARTED | - |
| 8 | NOT STARTED | - |

## Sprint 3 record - Native Publication Pack I

Donor audit: `observe-expanded-summoning-native-donors`, evidence
`20260924T1724064386340Z-observe-expanded-summoning-native-donors` (PASS;
292 units, 76 classes, 603 facts, 255 abilities, 65 buffs, the native Grab
graph), restoration `20260924T1725555974441Z`. Its `native-donor-audit.json`
chose every donor and GUID below; nothing was guessed.

Decisions (recorded here rather than asked):

- Pony (SM I templated, SNA I) and Horse (SM II templated, SNA II) reuse
  Kingmaker's own dedicated summoned units `PonySummoned`
  `3f95557fc806db741b500a5735990841` and `HorseSummoned`
  `5bb9579fdb2b26b48bb10d61c81cfdfb` as donors; the profiles are the tabletop
  stat blocks (Appendix A) on the animal class, both hooves as primary limbs
  exactly as those native summons carry them. Endurance and Run are omitted
  (no proven final-live feature identity).
- Owlbear (SNA IV) is the first magical-beast chassis: `CR8_OwlbearStandard`
  `d6e0acbdbdb56114898922063ae2cba0` as a sanitized body donor, 5 HD on
  `MagicalBeastClass` `b9e97f47cb86f2d45a0784a096ff8037`, natural armor +5
  (`7661741dbb9604842a642457456fd0e4`), bite 1d6 and two 1d6 claws, Improved
  Initiative, Great Fortitude, Skill Focus (Perception), reduced reach. Claw
  grab is deferred to the shared grapple lifecycle Sprint 4 introduces and is
  recorded as a deviation until then.
- Cyclops (SNA V) is the first humanoid chassis: `CR5_CyclopStandard`
  `124f1c45ef24d654e9cd420fe84f7f36` as a sanitized body donor, 10 HD on
  `HumanoidClass` `6ab4526f94d2e3e439af0599a29b6675`, natural armor +7
  (`e73864391ccf0894997928443a29d755`), the native `StandardGreataxe`
  `6efea466862f014469cec6c3f2b85cb7` in hand (the game scales it to the
  Large 3d6), Ferocity, Power Attack and Cleave
  (`d809b6c4ff2aaff4fa70d712a70f7d7b`). Flash of Insight is bounded: once
  per summoning, as a swift action, the next attack roll in the round is an
  automatic hit and critical threat (`CyclopsFlashOfInsightComponent` on a
  one-round state that the native `RemoveBuffOnAttack` ends after one
  attack); the confirmation roll stays ordinary; a small brain spends it
  when the cyclops fights and the player may spend it from the action bar.
  Hide armor and the heavy crossbow are omitted (no equipment beyond the
  weapon); Alertness, Great Cleave and Improved Bull Rush are omitted (no
  proven identity).
- Frost Giant under Summon Nature's Ally VII, VIII and IX reuses the retained
  native unit `590cd3d5e76fdc649a5f97bc984cd3c4` through three creature-named
  wrappers carved from the native Mastodon options
  (`6d8d59aa38713be4fa3be76c19107cc0`, `256739c1e61e3f64eaf71734d271f4be`,
  `9bd8cb6180842f44e9302c58e47b91f0`, which the publisher already reconciles
  to the KMG Mastodon). The native option builder gained one bounded
  device for this: when a wrapper's spec says so, the cloned umbrella's single
  direct spawn is pointed at the retained unit, keeping the umbrella's count,
  duration, pool and post-spawn actions. No second Frost Giant identity
  exists; coverage now reports it Published in both families.
- Icons: this environment has no image-generation capability, so the four
  new concepts are project-owned procedural renders from Blender 4.5
  (`assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py`;
  metaballs and primitives, procedural materials, warm key/cool rim/low fill,
  radial backdrop, dark bronze ring, 1254 px sources exported to 128 px by
  the existing tool). They were reviewed at 128 px in this session: each
  reads as its creature; they are plainer than the 77 painterly concepts and
  are recorded as such. No game pixels, downloaded model or texture is an
  input.
- Pins moved with the roster: 71 creatures, SM 68/378, SNA 61/348, 726
  logical placements (14 suppressed, 712 published), 199 templated, 30
  natural profiles, 81 icons, 29 wrappers (24 distinct sources), foundation
  identities 1276; the ledger gained exactly 92 appended, active identities
  (4 units, 45 placements, 34 template executions, 3 wrappers, 6 Cyclops
  specials) allocated by `tools/expanded_summoning_manifest.py --allocate`
  and activated; visible choices 693 -> 741 (SM 388, SNA 353); package
  237 -> 241 files. The static validation chain
  (`tools/validate_expanded_summoning_phase1.py`, wired from the 0.0.138
  validator) pins the append and the Sprint 3 figures; the runtime runner
  derives every roster pin from the catalogs instead of literals.
- New scenario `working-save-expanded-summoning-creature-review`
  (`creatures=<keys>`): casts the named creatures one at a time through the
  real parent chain into the working save and renders each from the party
  camera idle, moving and attacking, using the Phase 0 motion review
  generalised away from the Pteranodon. The save is read, never written.
  It is the standing internal-review instrument for every visual of
  Sprints 3-8.
- Domain suite: 1779 cases (five Sprint 3 regressions in
  `ExpandedSummoningSprint3Tests`), all passing; repository validation PASS.

Runtime qualification (guarded, live installation restored after each
batch): recorded below as it completes.

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

Sprint 3 guarded runtime qualification: structural inventory, mechanical
casting, visual contracts, the creature review of the four new creatures,
the player-path matrix, the persistence trio and the two compatibility
transactions; internal review of the review renders; then record the
evidence, update the PR body and open Sprint 4.
