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

Result on the final Sprint 4 candidate `16708bd5e237` (package `e017a5f3d8ec…`, DLL `cbca54912642…`, MVID `90ce7e7d-9c5a-46ec-9c95-c0734d2faaec`), session `20261118T014552Z-owner-0.33.0-true-grit-level-up`, PASS, 6 of 6:

- **Shape:** one selection granted twice at level 20; 24 one-rank choices; prerequisites not ignored. The level-20
  visit offered 20 owned deeds.
- **Duplicates:** at every step the deed held by the other state was neither offered (`CanSelect` false) nor
  accepted (`SelectFeature` false):
  - Deadeye into the second state while the first held it;
  - Gunslinger's Dodge over Deadeye without back;
  - Gunslinger's Dodge into the cleared first state.

  Replacement (Quick Clear) and the freed Deadeye were accepted. No step held a deed twice or a preview rank above one.
- **Commit:** after `IsComplete`, Gunslinger 20 with exactly Deadeye and Quick Clear, each at rank 1 and each costing
  0 through `TrueGritRuntime`.
- **Cancellation and failure:** a cancelled visit holding two deeds, and a visit that threw after holding two, left
  the level-19 character, its facts and its grit unchanged.
- **Respec:** a cancelled native respec left both deeds. A committed one restarted the character at Gunslinger 1 with
  no True Grit fact, and the respec visit offered no True Grit state.
- **Isolation:** party and global units unchanged.

The native boundary therefore enforces distinctness, and the invariant is demonstrated through the exercised paths.
No repair was needed. The removed `IsValidPair` helper was not production protection.

Preserved first run: on the first closure candidate `fd4087c312bd` (session `20261118T013304Z`), every distinctness,
cancellation, injected-failure and committed-respec check passed. The cancelled-respec comparison failed, because of a
fixture defect: the observer had added `chosenDeedCosts` to the after-commit snapshot it compared with. That was
repaired in `bb789b1891` (the costs are now recorded on the commit row), and the candidate was rebuilt.

Limits:

- The disposable character is advanced to 19 without earlier selections.
- The level-up screen and the UI's `Next` path were not observed.
- True Grit through respec is reachable again only by re-levelling to 20, which was not driven.
