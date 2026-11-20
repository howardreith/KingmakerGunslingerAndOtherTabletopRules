# True Grit's two different deeds through native level-up (RISK-04 / S4 T6)

Status: **observer and reframing on a review branch, awaiting review. Not merged, not released.** Branch
`codex/risk04-true-grit-distinct`, stacked on the preceding stacked checkpoint `codex/risk01-advanced-revolver` (draft
PR #182). The native result comes from the final Sprint 4 candidate and is recorded on the Sprint 4 closure branch.

## Contract

Epic S4 T6, inventory RISK-04: "The explicit distinct-pair check is test-only, but one-rank features and native filtering
may already prevent duplicate selection." Attempt the same deed twice in a real level-up transaction, including back
navigation, cancellation and respec. If native filtering enforces distinctness, record that and remove or reframe the
misleading unused policy. If duplicates commit, repair at the owned selection boundary, scoped to True Grit.

## What the source does

- `GunslingerClassBlueprints` adds the one `KMG.Classes.TrueGritSelection` to level 20 twice, so the visit has two
  states of the same selection.
- Each of the 24 `KMG.Classes.TrueGrit*` choices is a `BlueprintFeature` with `Ranks = 1` and one `PrerequisiteFeature`
  on the owned deed. The selection is obligatory and does not ignore prerequisites.
- Native 2.1.7b `BlueprintFeatureSelection.CanSelect` (IL inspected) requires the item to be in `AllFeatures`, requires
  `MeetsPrerequisites` unless the state ignores them, then refuses when
  `unit.Progression.Features.GetRank(feature) >= feature.Ranks`. The level-up controller evaluates it on the preview
  unit, which already carries the deed chosen in the other state. A one-rank deed held by one state is therefore
  expected to be neither offered nor accepted by the other.
- `TrueGritCatalog.IsValidPair` was called only by the `true-grit.pair-uniqueness` domain test. No production path
  consulted it.

## What changed

- **New guarded product scenario `disposable-true-grit-level-up-transaction`**
  (`RuntimeTesting/RuntimeTestRunner.TrueGritLevelUp.cs`; registered in the scenario catalog, dispatch,
  `RuntimeAutomation.Common.ps1` and the preflight list). A disposable Gunslinger is advanced to 19 natively, then:
  - **Cancel:** a level-20 visit holding two deeds is cancelled, and the character, facts and grit must be unchanged.
  - **Injected failure:** a visit throws after holding two deeds; the guarded visit cancels it, and the character
    must be unchanged.
  - **Duplicate attempts with back and replacement, then a gated commit.** The other open requirements are filled
    deterministically, then each step records what is offered, accepted and held, and checks after every step that no
    deed is held twice and no preview rank exceeds one:
    1. A goes into state 0.
    2. A into state 1 must be neither offered nor accepted.
    3. B goes into state 1.
    4. B over A in state 0 without back must be refused.
    5. Back clears state 0.
    6. B into state 0 must be refused.
    7. Replacement C goes into state 0.
    8. Back clears state 1.
    9. The freed A goes into state 1.

    The commit then needs `LevelUpState.IsComplete` before `ApplyLevelup`. The character must end at Gunslinger 20
    with exactly the two final deeds at rank 1, and each must cost 0 through `TrueGritRuntime`.
  - **Respec:** a cancelled native respec must leave both deeds. A committed one must restart at Gunslinger 1 with no
    True Grit fact (True Grit itself is reachable again only at level 20).
  - **Isolation:** party and global unit snapshots must be unchanged.
- `TrueGritCatalog.IsValidPair` is removed. The catalog now states that distinctness is enforced by the native
  selection and points to this scenario. The domain case `true-grit.pair-uniqueness` becomes
  `true-grit.catalog-deeds-distinct`, which checks the catalog's own invariant: each deed appears once, so each has
  exactly one choice feature.
- No blueprint, identity, text or save change.

## Tests

| Command | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'true-grit;sprint59;runtime-scenario;scenario-catalog;runtime-automation'` | production compiled; 11 of 11 | 27 s |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | 1205 preflight checks passed | 55 s |
| `scripts/Invoke-KmgGate.ps1 -Filter 'true-grit;sprint59'` (after the reframing) | production compiled; 10 of 10 | 26 s |

`tools/validate_sprint59.py` names `IsValidPair` and `TrueGritPairUniqueness` as source tokens. It is dispatched only
for version 0.0.59, and the current tree already lacks another of its tokens (`TrueGritDeed.Deadeye` in
`DeadeyeRuntime.cs`), so it is not part of current validation. It is left unchanged, as historical validators are.

## Native evidence

To be recorded from the final Sprint 4 candidate on the closure branch. If the scenario shows a duplicate committing,
this story is reopened for a repair at the True Grit selection boundary, as the contract requires.
