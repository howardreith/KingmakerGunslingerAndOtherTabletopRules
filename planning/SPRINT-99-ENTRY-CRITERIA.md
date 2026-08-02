# Sprint 99 save-free Scatter Shot runtime acceptance

Add a guarded, non-save scenario that constructs an isolated Blunderbuss
wielder and exactly two native cone targets far from live units. It must prove
the registered transaction with forced mixed rolls (one misfire, condition
unchanged) and all-misfire rolls (Normal to Broken), one discharged chamber,
exact cleanup, loaded version, and no save interaction.

Installed iterator IL proves `GameHelper.GetTargetsAround` enumerates the exact
`Game.Instance.State.Units` pool. Detached targets are inserted request-locally
into its public `All` set and removed under boolean ownership in `finally`
before disposal; no loaded area is required. Both targets retain immortality during the transaction. The
scenario requires exact `State.Units.Count` delta `+2`, exact pool restoration,
and reference-identical external `AllUnits` restoration to the pre-request snapshot; it grants no faction, quest, loot, dialogue,
kingdom, or experience state.

Qualification requires an exact-commit mod-load smoke and two consecutive
fresh-process scenario PASS runs before the production item restriction or
vendor exclusion may be removed.
