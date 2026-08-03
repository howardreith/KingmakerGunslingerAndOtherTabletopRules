# Fourth playtest repair report

## Live baseline identity

- Starting branch/commit: `codex/third-playtest-feats-reload-grit-dodge-assets`
  at `6a0117d39695c5dd20b84f244f59d28a586b4111`; clean tree.
- Repair branch: `codex/fourth-playtest-runtime-ux-repair`.
- Baseline version: `0.0.63`.
- Baseline package SHA-256: `80010458f3ffe461f5487da93eef9ac67f799ccc39603c7b977bb360024621f0`.
- Baseline package/live/cache DLL SHA-256:
  `f4855c22d8ada1ec2a209b4f2e60edfb9f60477f2873e32adbc72e572d5ca0f9`.
- Firearm AssetBundle SHA-256:
  `5418a9bc008b80e92f10a52e653728612a892bb398aa0a9664d603b21c26d324`.
- Fresh guarded Steam mod-load PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2225523236661Z-mod-load-smoke`.
- Loaded identity: version `0.0.63`, commit `6a0117d...`, MVID
  `80b76899-d9b8-464b-b926-7719a3f9b50d`, and the exact cache hash above.

The fresh process proves that the fourth-playtest screenshots show integration
defects in the qualified build, not a stale canonical DLL or stale UMM cache.
Earlier bundle/audio assertions proved prefab instantiation and callback counts,
but did not prove the player-visible doll, projectile, or native UI surfaces.

## Native firearm feat menu checkpoint

Root cause: firearm `FeatureUIData` rows were appended after Kingmaker's cached
native alphabetic list, and Rifle was displayed as `Advanced Rifle`. The prior
runtime assertion checked only that five entries existed somewhere.

Repair:

- merge native and firearm rows by displayed name using the current-culture,
  case-insensitive order;
- expose the exact labels `Blunderbuss`, `Musket`, `Pistol`, `Revolver`, and
  `Rifle`;
- retain one native top-level feat, native icons, hidden legacy wrappers,
  parameter-specific effects, and native prerequisite shapes;
- strengthen the guarded runtime assertion to inspect the full order and exact
  firearm subsequence in Weapon Focus and all four dependent native families.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 878/878 PASS;
- clean Release compile and strict standalone package validation: PASS;
- checkpoint package SHA-256:
  `496ce8acc36e04fbce055411ca204f9f1c79d1ee7a5d82b086a93c8f4d32bb09`;
- checkpoint DLL SHA-256:
  `c5789a755f324780f806c431fab87fe2b7430c98328f868e401f0f2f556243cc`;
- guarded save-free runtime PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2230567143233Z-disposable-firearm-dependent-feats`.

This mechanically qualifies the live native menu data and ordering. Final Phase
12 still requires a player-visible UI observation after the full `0.0.64`
repair package is coherent.

## Native Grit action-bar checkpoint

Root cause: every paid deed carried an `AbilityResourceLogic` bound to Grit,
but `IsSpendResource` was false. Exact installed `Assembly-CSharp` inspection
showed that `AbilityData.GetAvailableForCastCount()` ignores such a component
and returns no finite counter. The earlier runtime check proved only component
presence and therefore missed the screenshot-visible defect.

Repair:

- mark the shared native resource component as spend-enabled with an amount of
  one, enabling Kingmaker's own availability and remaining-use calculation;
- use a narrow `GritAbilityResourceUiLogic` subclass whose `Spend` is a no-op,
  retaining each deed's authoritative post-gate transaction and preventing a
  second resource mutation;
- strengthen source and runtime checks to require the exact shared component,
  real granted ability fact, finite native count, zero-resource disablement,
  and no UI-side spend.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 878/878 PASS;
- clean Release compile and strict standalone package validation: PASS;
- checkpoint package SHA-256:
  `cefc9fa962ac33b05a914ed46d05a70c063175e74f7a91cd041412ae5f06b1a9`;
- checkpoint DLL SHA-256:
  `68ee5be23eee638872b52f6a2b4553afcc676053f24e3b5b4910d775143816f1`;
- guarded save-free runtime PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2240193812834Z-disposable-gunslinger-grit-resource`.

The live engine observed `counterAtOne=1`, `counterAtZero=0`, availability
`True->False`, and unchanged Grit after the UI component's `Spend`. The first
attempt at `20260803T2236515694613Z` failed safely because the observer used a
detached `AbilityData` with no fact; it was corrected to use the descriptor's
actual granted native ability, without changing the production repair.

## Native Reload auto-use RTwP continuation checkpoint

Root cause: the empty-firearm construction patch replaced the rejected attack
with a native reload command, but discarded the original target and had no
completion continuation. The prior observer also assigned detached
`AbilityData`, so it did not prove the right-click state of a granted ability.

Repair:

- retain exactly one player-facing dynamic `Reload Firearm` blueprint;
- bind a pending continuation to the exact native `UnitUseAbility`, executor,
  target, and equipped weapon object;
- on successful reload completion, defer to the next game action, re-resolve
  the exact equipped item and auto-use state, require a loaded non-Wrecked
  firearm, and queue a native attack against the original target;
- fail closed after interruption, failed reload, weapon/target/auto-use drift,
  ambiguity, empty/Wrecked state, or unavailable turn-based standard action;
- retain native action economy and avoid recursion because the resumed attack
  is constructed only after the exact item is loaded.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 878/878 PASS;
- clean Release compile and strict standalone package validation: PASS;
- checkpoint package/DLL SHA-256:
  `03ccbba8c05dcb81514ac951b9eae83c5c6e62a87b2d3fe7e924f154f8482fce` /
  `edc45bb7390483926520e416e01f9e054b3394d1195f9776f0336c6e1eb74a0a`;
- guarded save-free RTwP command-collection PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2250479651771Z-disposable-reload-autocast`.

The live run observed native selection, `rounds=1`, powder and ball `0->1`,
`resumedTarget=True`, no retry when full, two stable no-ammunition polls, and
complete cleanup. The earlier run `20260803T2248367218954Z` failed only because
the observer assumed the linked-list queue rather than Kingmaker's complete
command collection; it was corrected to use `UnitCommands.Contains`.

Follow-up guarded evidence
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2257100760678Z-disposable-reload-autocast`
also passed exact weapon-switch cancellation, interrupted-command cancellation,
Wrecked rejection without a reload replacement, and the fail-closed action-mode
matrix: RTwP resumes, while turn-based continuation requires both a current turn
and an unused native standard action. Its package/DLL SHA-256 values are
`af2b9b8871004611edc5d77a7ddcc959ff8b2e100c93019ab1c4e2c2dc0ed93d` /
`75792762c2628bce8ce0090dd95575d350ecf7cd8546c1be5fd675bda63dcc27`.

Every firearm kind and actual player-visible right-click behavior remain for the
integrated/final acceptance runs; the guarded continuation mechanics are now
qualified.

## Timed Overhaul maintenance checkpoint

Overhaul Firearm now requires one uninterrupted minute out of combat. Starting
the delivery performs no mutation; interruption, disposal, combat at completion,
or exact-item drift consumes no repair kit and changes no firearm state. A valid
completion reuses the existing atomic exact-item transaction: Wrecked becomes
Broken and exactly one repair kit is consumed.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 879/879 PASS;
- clean exact-reference Release build and strict package validation: PASS;
- checkpoint package/DLL SHA-256:
  `7d5b83ba05b27eb5aaf657f7016cd02395629147cd6bc60408f58f24629404e3` /
  `438ab26030c6bea5ecd15dbd70ccde2e3c890f557c038d421e95f4268f9543db`;
- guarded save-free runtime PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2312021633458Z-disposable-overhaul-maintenance`.

The first two observer runs (`20260803T2305568410359Z` and
`20260803T2309470365802Z`) failed only the synthetic combat-state assertion.
The second package revealed that the native `m_InCombat` fixture flag had been
placed in an adjacent observer; it was removed there and attached to the exact
Overhaul unit. The final run observed `duration=1 minute`, delayed and
interruption-atomic delivery, real combat blocking, exact atomic completion,
cleanup, and version `0.0.63`.

## Native firearm condition tooltip checkpoint

The installed `DescriptionTemplatesItem.ItemHeader` contract receives the exact
`ItemEntity` used by both item tooltips and item descriptions. A narrow Harmony
postfix now appends a native text brick only when that exact item resolves as a
firearm. Normal, Broken, and Wrecked each have non-null mechanical wording;
Broken states its +4 misfire increase and recovery actions, while Wrecked states
its fire/reload prohibition and timed out-of-combat Overhaul recovery.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 880/880 PASS;
- clean exact-reference Release build and strict package validation: PASS;
- checkpoint package/DLL SHA-256:
  `4a3f9d55eae4ec59210804286bb25dcc964239e64b9a4d2ef552d13758b011be` /
  `91828430d78309a5eeffd278be45f9ce048f255ca15d8760bb62a4819c0167b6`;
- guarded fresh mod-load PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2317264754582Z-mod-load-smoke`.

The first exact compile rejected the modern `HarmonyLib` namespace; changing it
to this repository's installed `Harmony12` contract resolved the annotations.
Visible placement, wrapping, and refresh after condition changes remain for the
final supervised UI acceptance; bootstrap and patch installation are qualified.

## Remaining mission scope

Rapid Reload and semantic icon art, final player-visible Grit observation, real
equipped firearm models/projectile/audio presentation, integrated reload auto-use,
condition transition combat-log and Quick Clear presentation, Winchester attribution,
comprehensive regression/runtime runs,
and final player-visible acceptance remain incomplete.
