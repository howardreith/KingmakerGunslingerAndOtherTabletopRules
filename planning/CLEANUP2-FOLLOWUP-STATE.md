# Summon cleanup follow-up after v0.0.164

**Released as stable v0.0.165 on 2026-10-24** from PR #69 (merge
`53446777812f18c707ef74cdd787ab4c2fd664df`). The public ZIP is
`60f8391d016a03a553e31334f83ddff1777402955c65c1cdeb083adc8b8a6184`. It is
downloaded and strictly validated, with the same code as the gated candidate
`f216160d8e`; see [the resume record](../AUTONOMOUS-RESUME.md). The lane is
closed.

Development checkpoint, 2026-10-24, on `codex/expanded-summoning-cleanup3-followup`
from master `114564ac71` (released v0.0.164 plus its record). The owner then
authorized the v0.0.165 release ("Go ahead and merge and release please.");
see [the release candidate](#v0165-release-candidate).

The owner's correction list (2026-10-24):

> 1. Deinocychus' arms are detached. Could they be made contiguous? 2. The pony
> icon and the cyclops icon feature artwork inconsistent with the rest of the
> icons' style. Could they be updated please? 3. The Summon Nature's Ally 1 paw
> icon still appears for the list of Summon Nature's Ally 1. Please remove it.
> 4. The storm giant and fire giant still have an animation to them where they
> are dropping chunks of ice, I assume since they're built off Frost Giant. Any
> way that can be removed?

## Changes

| Item | Cause | Change | Commit |
| --- | --- | --- | --- |
| 1 Deinonychus arms | Each arm and finger was its own tube pushed into the chest, partly weighted to donor arm bones whose pivots sit 0.65-0.98 m away; claws rode 100 % on finger bones | Arms, palms and palm-rooted fingers swept into the one body volume; restrained arm chain weights fade in past the shoulder; claws carry their fingertip's exact weights | `f544b6c796` |
| 2 Pony and Cyclops icons | Both were still Phase 1 procedural Blender busts | Two original paintings from the owner's Codex CLI image tool (owner-authorized), style references only; originals kept under `edits/` with exact prompts | `81dd600d19` |
| 3 SNA I paw row | SM I and SNA I are native direct spells, so their children had no `Parent`; the Druid conversion projection requires it, so a level-1 slot kept the native paw row | Publication links owned children that lack a parent; rollback unlinks them; module OFF stays native | `7a8c753efd` |
| 4 Fire/Storm ice | Both reuse the native Frost Giant prefab, whose root `SpawnFxOnStart` spawns `Fx_FrostGiant00` | Prefixes on `SpawnFx` and `HandleUnitDeath` skip exactly these two owned bodies while the module is active | `2c6bdd8f7e` |

Supporting commits: `2f5eaf9d92` admits version 0.0.164 and the existing
`menus` phase in the protected native-review wrapper and the Deinonychus in the
current-stack creature review; `05ba0dda49` adds the Deinonychus to the matching
native observer scope.

The `Parent` link (item 3) matches native semantics: every native variant child
carries it, and native readers are metamagic rods, Spell Specialization,
spell-resistance and damage triggers and the action bar's is-casting highlight.
SM I children are linked too, so those readers now treat both tier-I families
like tiers II-IX. The alternate summoner adapters set their own `Parent`
explicitly and are unaffected.

The installed Frost prefab's `FxOnDeath` is unset, so the death-path prefix
(item 4) currently changes nothing. It is kept so neither donor effect can be
spawned on these two bodies.

## Offline checks

- Focused gate (production compilation plus related domain tests) after each
  item, last `expanded-summoning.cleanup2;cleanup3;sprint24;presentation`:
  54/54. New domain case `expanded-summoning.cleanup3-giant-frost-effects`.
- Icon catalog validator PASS and `tools/test_icon_catalog.py` 30/30.
  `tools/New-ExpandedSummoningIcons.ps1 -OnlyKeys @('pony','cyclops')` exported
  129 distinct sources/outputs.
- Deinonychus: two clean Blender 4.5.10 builds with `PYTHONHASHSEED=0` produced
  identical bytes. `verify_cleanup2_bodies.py` (now covering the Deinonychus)
  PASS for all five bodies; the previous export fails its new rule. Before, all
  895 forelimb-weighted vertices sat in 14 parts outside the 4,199-vertex body;
  after, the 5,149-vertex body holds every arm, palm and finger vertex and only
  the six rooted claws are separate.
- `scripts/Test-SummonCleanup2ConsolePatchTargets.ps1` lists both new targets
  with their inspected installed IL lengths. On the candidate DLL below it
  prepared 39/39 targets against the installed 2.1.7b assemblies, including
  `SpawnFxOnStart::SpawnFx` (99 bytes) and `HandleUnitDeath` (79 bytes).

## Development candidate

`Build-Local.ps1 -DevelopmentPurpose summon-cleanup-iteration2` on `05ba0dda49`
(171 s): package
`30067d063d5d3f0da1a500af9313576d47f94211661c51989d37fb4a693b11a8`, DLL
`3c0babc7c3ee25aa71ebdfe3e6fe8ff003ed154c23910902e12eb86b79a38fa5`, version
0.0.164, never a release.

## Native reviews on that package

| Review | Result | Time | Raw id |
| --- | --- | --- | --- |
| giant-lineup | **PASS 84/0**. Fire and Storm carry the Frost prefab component but spawn nothing; both Frost routes spawn `Fx_FrostGiant00(Clone)` (particle systems SnowBody, SnowSplash, Snowflakes, DropsWithTrail, SmokeBody, Smoke, Smoke_Overdistance, FireTiny); Hill, Stone and Cloud have no such component | 267.3 s | `20261024T0557060594298Z` |
| menu-adapters with Call of the Wild | **FAIL 141/3**, only the known environmental pointer stop at Feral Hunter (native Demoralize tooltip `7d2233c3…`), as on review49. All eight Druid checks pass, including the new level-1 conversion: ten SNA I creatures with their own icons, no parent paw row, all ten children linked, and a Dog cast that spends only its preparation | 258.4 s | `20261024T0602279134237Z` |
| menus | **PASS 43/0**. The canonical SNA I popup holds exactly its ten creature rows; the painted Cyclops renders in the SNA V-IX popups | 198.8 s | `20261024T0608111704227Z` |
| Deinonychus creature review (current stack) | **FAIL 6/1**. Refinement sequences pass (9 stills, 108 frames, 5.7 m native travel, attack and hit). The summary check measures the reversibly suppressed `_ammunition04` accessory renderer and reports it disabled; the v0.0.157 review of 2026-10-17 (`20261017T1444078101921Z`) failed identically, so this is a pre-existing observer limitation | 188.2 s | `20261024T0612477308531Z` |

All four transactions restored exactly: live mod 254 files at the owner's
0.0.140 install, settings and all 95 saves exact, `KMG_AUTOMATION_WORKING`
unchanged (1,424,708 bytes, 2026-10-05 20:09:11 UTC), no lock or game process.

A local review page with before/after renders, icons and native crops is at
`artifacts/cleanup3-review/index.html` in this worktree (not uploaded).

## v0.0.165 release candidate

Release preparation `f216160d8e` bumps to
`0.0.165-expanded-summoning-cleanup-followup` with the narrow
`tools/validate_expanded_summoning_cleanup_followup_release165.py` (see
`docs/BUILD-AND-RELEASE.md`). Gates ran in the frozen detached worktree at
`f216160d8e` with the cleanup2 private reference bundle; native reviews deploy
the gate's exact package through `Invoke-ExpandedSummoningRuntimeScenario.ps1`,
which rebuilds and verifies the same bytes first.

| Gate | Time | Result | Raw id |
| --- | --- | --- | --- |
| Sprint gate (`Invoke-KmgGate.ps1 -Level Sprint -ReferenceBundleDir <bundle>`) | 280 s | **PASS**: 2740/2740, the v0.0.165 validator, exact build, strict package; package `20ab0d0dd0367a7053db0030a47af82f9da374659fc4f07ad1d53dac80990542`, DLL `ae880adc6dc70eef8d252a5e3bf2b286d4e48be20bcef6b7aae6664d1fde99ef` | - |
| visual contracts | 366.7 s | **PASS 15/15**: 117/117 renderable, bounded, attacking, hit and death; MVID `6fe0cf00-75ab-4a39-9e33-acb25f9ba395` | `20261024T0738205244384Z` |
| creature review: Crab, Girallon, Stag Beetle, Griffon, Elephant, Stegosaurus, Brachiosaurus, Gorgon, Fire Giant, Storm Giant | 689.4 s (397.8 s in game) | **PASS 36/36** | `20261024T0745057530976Z` |

Both reviews restored the live mod exactly (254 files, `5C3E4AD4…BFBB`); all 95
saves and `KMG_AUTOMATION_WORKING` are unchanged. The temporary reference-bundle
junction the harness build needs was removed afterwards. The specific Druid,
menu, giant-effect and Deinonychus observations above remain bound to
development candidate `05ba0dda49`, whose product source matches this
candidate apart from the version metadata.

## Limits

- Owner visual acceptance is pending for the Deinonychus body and both icons.
- The Deinonychus native frames are small at the party camera; contiguity is
  proved by the authored mesh, the verifier and the offline posed renders.
- Death of a Fire or Storm Giant was not exercised natively; the effect is unset
  on the installed prefab.
- The Feral Hunter and Preservationist PC hovers keep the owner's earlier
  environmental disposition.
- Other Phase 1 procedural icons (for example the six mephits, Owlbear, Tiger,
  Shambling Mound) are unchanged; the owner named only the Pony and Cyclops.
