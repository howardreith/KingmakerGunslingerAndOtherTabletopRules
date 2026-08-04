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

## Quick Clear player-facing checkpoint

The existing exact-item mechanics were correct, but the icon loader did not
traverse progression level entries and Quick Clear was not an explicit root.
Its feature and both granted actions could therefore retain null icons. Quick
Clear is now an explicit project-icon root, recursively assigning the dedicated
`quick-clear` sprite to its standard and move actions. Its unavailable reason
now explains the one Broken firearm/one Grit requirements and directs Wrecked
firearms to Overhaul.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 880/880 PASS;
- clean exact-reference Release and strict package validation: PASS;
- checkpoint package/DLL SHA-256:
  `dbfecb4500a9a8cf65f1456d4678e0a4b0d5e0c03bbf6911916248685749f8dd` /
  `bf35cd354c344c4780dc4f9f4259b2a7ba6260e9ec4e895be6b3ffdf67452f12`;
- guarded save-free runtime PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2323347859647Z-disposable-gunslinger-quick-clear`.

The preceding run `20260803T2321232806825Z` passed every mechanical and
availability assertion but failed native presentation, exposing the null-icon
root omission. The final run proves real level-granted facts, semantic icons,
meaningful names/reason, Broken availability, Wrecked and zero-Grit rejection,
standard `2->2` and move `2->1` Grit behavior, exact Broken-to-Normal repair,
atomic rejection, diagnostics, and cleanup.

## Winchester provenance and five-prefab bundle checkpoint

The corrected handoff identifies the exact advanced-rifle payload as
*Winchester lever action rifle* by Killian Delias (`Killian_Delias`), Sketchfab
UID `678f6e091d7149da8fce413b6fd31288`, CC-BY-4.0. All six preserved payload
hashes match the corrected provenance record; embedded Winchester/lever-action
names agree, former Martini-Henry records remain in `provenance-history`, and
clearance is limited to Advanced Rifle. The handoff ZIP was correctly classified
as playtest evidence rather than an original Sketchfab distribution.

Unity `2018.4.10f1` produced two byte-identical five-prefab bundles:
SHA-256 `D902F279D8E745BC7852ABDEF6F7C03B97128C92F38641101D5DFC140E39FBFD`,
16,680,466 bytes. Repository validation, 880/880 tests, clean exact-reference
Release, and strict packaging pass. Package/DLL SHA-256 are
`7128c91a75c43ad246ac58cdb6d84cbd5cd44ad43a4121912e4636e8edd36db1` /
`19b9ea5cc49fbd899ddf3c9af3bcc20d6fd36c6077cc933fbc1da98abd038955`.
Guarded run
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2330553131263Z-observe-production-firearm-fallbacks`
passed all five prefab/material mappings and transient cleanup.

This checkpoint proves provenance, deterministic bundle identity, loadability,
and prefab/material resolution. It does not yet prove the live doll socket,
complete crossbow/bolt/quiver suppression, projectile appearance, or absence of
layered crossbow audio; those remain production-context/final visual acceptance.

## Native equipment-model and crossbow-audio suppression checkpoint

Installed IL establishes that `WeaponVisualParameters.Model` is the native
equipment model and that `HasQuiver` is unconditionally true for the retained
Crossbow animation style. Each production firearm now owns a distinct visual
parameter instance whose model is the exact approved firearm prefab. Native
belt/sheath models and prototype fallback are removed; inherited crossbow
combat sound, miss sound, and whoosh values are cleared. Projectile and attack
animation contracts remain unchanged. The equipment fact no longer spawns a
second model on a guessed hand socket; it only disables enabled renderers whose
names identify crossbow, bolt, quiver, or arrow presentation and restores them
when the firearm is unequipped.

Evidence:

- repository validation: PASS;
- dependency-free domain/reflection suite: 880/880 PASS;
- clean exact-reference Release and strict package validation: PASS;
- checkpoint package/DLL SHA-256:
  `af3c5aebedf627f021819ab6382dd90eb5177a4b51eaa9da5fe3f59e46a90a23` /
  `e2c13102059b6ce0fa614cf79abad11cf3f1b9509b0a3c1c46d99df7b7dfe006`;
- guarded Steam-backed PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2343075121479Z-observe-production-firearm-fallbacks`.

The observer passed all five exact custom-model mappings, distinct visual
instances, native sound suppression, one preserved projectile per firearm,
animation preservation, transient prefab resolution, cleanup, and version
`0.0.63`. This is structural runtime evidence, not live-doll or audible-output
acceptance. Actual socket/scale, enabled renderer state, projectile appearance,
and exactly-once audible playback remain for loaded-unit and final supervised
qualification.

## Rapid Reload and semantic progression icon checkpoint

The former Rapid Reload asset was a flat salmon disk. Its replacement is a
hand-painted, framed oxblood-and-antique-gold icon with one large ramrod and
circular reload arrow, legible in inspected 64- and 32-pixel exports. The
built-in image tool produced the high-resolution chroma source;
`tools/New-RapidReloadIcon.ps1` deterministically removes chroma, despills,
crops, and resamples the game asset. Source/export SHA-256 are
`CAB121AF2BE6943A4E9B29ADC9544F2F3A38B9CA35719C36231A7C2A12E5F319` /
`552455F3CE043B8D93E3DCE91B73AB78EE1852FDF51BDA99611CDD298D12560E`.

The strengthened first run
`20260803T2350599488388Z-observe-gunslinger-presentation` failed because six
core progression facts still shared the class icon. The loader traversed
selections and `AddFacts`, but not progression level entries. Production now
walks exact `BlueprintProgression.LevelEntries` while still skipping native
facts.

Final evidence:

- repository validation and 880/880 domain/reflection tests: PASS;
- clean exact-reference Release and strict package validation: PASS;
- package/DLL SHA-256:
  `efae74366d83ed74dbab0686d0f28748f707d1d8c8abbb1060d4b8972fd8c2dd` /
  `8bc39668f6e8b36666126fa714bcfd154e93c796f6bb275a7b6bba207704f1a2`;
- guarded Steam-backed PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260803T2353092670187Z-observe-gunslinger-presentation`.

The final run observes eleven distinct semantic icon references for
Proficiencies, Gunsmithing, Grit, Deeds, Nimble, Gun Training, True Grit, Quick
Clear, Reload, Repair, and Overhaul. It also proves exactly one Rapid Reload
selection in each native feat catalog, five choices, the exact new icon, and
choice matching only its Pistol/Musket/Blunderbuss/Rifle/Revolver kind. Native
Bonus Combat Feat remains unmodified and retains its native icon.

## Remaining mission scope

Final player-visible Grit observation,
loaded-doll firearm socket/projectile/audio presentation, integrated reload auto-use,
condition transition combat-log and Quick Clear presentation, Winchester attribution,
comprehensive regression/runtime runs,
and final player-visible acceptance remain incomplete.

## Firearm description and qualities checkpoint

Production firearm descriptions no longer describe approved placeholder or
crossbow-compatible visuals. The native item-qualities template now intercepts
only resolved firearms and presents firearm era, handedness, capacity, range or
scatter mode, effective misfire value, and current condition instead of the
observed `<null>, <null>` qualities.

Two broader patch attempts were rejected. Run
`20260803T2359504427346Z-observe-production-firearm-fallbacks` timed out during
bootstrap with PID 15468 after patching the global tooltip text overload; run
`20260804T0004138320330Z-mod-load-smoke` similarly timed out with PID 19340
after using an exact global target method. Both requested automatic exit and
both processes were explicitly terminated. The replacement is a narrow
`DescriptionTemplatesItem.ItemQualities` prefix; it passed fresh guarded
bootstrap at `20260804T0008070526912Z-mod-load-smoke`.

Final evidence:

- repository validation and 880/880 domain/reflection tests: PASS;
- clean exact-reference Release and strict package validation: PASS;
- package/DLL SHA-256:
  `c6a6b206f1eab54af986118ff2746ee1a0b01829bca89f07ae9e1b7a4fbbb2b4` /
  `b43890a9e9bcbdcdc16b42fc0ff6bf9c96c001f612135782bcfae76dc536395f`;
- guarded Steam-backed PASS:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260804T0009378520857Z-observe-production-firearm-fallbacks`.

The final observer resolved a detached production pistol in Broken state and
reported `Firearm, Early, One-Handed, Capacity 1, 20 ft. Range, Misfire 5,
Condition: Broken`, with no placeholder, crossbow, or `<null>` wording. It also
passed all five asset mappings and cleanup. Actual rendered tooltip wrapping,
placement, and live condition refresh remain final supervised UI acceptance.
