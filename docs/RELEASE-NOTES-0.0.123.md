# Kingmaker Gunslinger 0.0.123

Informational version: **0.0.123-character-visibility-repair**.
The owner accepted the character-visibility repair mission report and
explicitly authorized merge and public release.

## Fixed: newly created characters had invisible bodies

New characters — during new-game character creation and mercenary recruitment
alike — could end up with invisible bodies while their clothes and weapons
remained visible, and stayed that way after creation.

### Root cause (reproduced and renderer-verified)

The native character creator's doll update (`CharGenDollRoom.
UpdateDollCoroutine`) plans removals for equipment entities dropped from the
current doll state. For each removed entity it calls
`EquipmentEntity.UnloadInnerAssetsExceptGiven` and then
`ResourcesLibrary.TryUnloadResource` — whose `LoadedResource.Unload()` runs
`AssetBundle.Unload(true)`, a forceful destroy of **every asset loaded from
that bundle**. The shared character materials and ramp textures still
referenced by every Kingmaker Gunslinger elemental visual proxy were
destroyed as collateral (confirmed live: the committed character's world view
contained all five correct entities but zero renderable renderers; all 28
proxies and 74/75 donors lost their inner assets in a single creator
session). Separately, every area transition's counter-based
`CleanupLoadedCache` evicted and destroyed untouched registered resources.
Because the proxies are cache-only clones, they could not reload, so bodies
stayed invisible while native clothes and weapons reloaded from their
bundles on demand.

### The repair

- The creator retention plan now also protects proxy and donor ramp textures
  (`PrimaryRamps`/`SecondaryRamps`), not only `GetInnerAssets()`.
- A `ResourcesLibrary.TryUnloadResource` guard keeps the exact 75 registered
  native donor identities cached and alive; no foreign resource is retained.
- A hidden `DontDestroyOnLoad` anchor plus request-counter arming at every
  native cache cleanup keep the registered identities alive across area
  transitions without pinning anything else.
- Damage-aware recovery reconstructs exactly the destroyed resources at the
  proven boundaries (creator update, creator close, cache cleanup): donors
  are reloaded from their bundles and validated by recorded provenance,
  proxies are re-cloned under their original stable GUIDs, and a foreign
  object under a registered GUID is never displaced.
- The creator retention hook can no longer abort the native doll update for
  any race: elemental failures are isolated with rate-limited diagnostics
  while ordinary character creation proceeds.

Blueprint, appearance, equipment, race-mechanics, settings, and save
identities are unchanged. No new dependencies.

### Qualification

- 1,567 deterministic domain/reflection tests, clean Release build, strict
  standalone UMM package validation.
- Guarded native runs (Steam App 640820, full owner stack): the
  character-creator visual lifecycle scenario **PASS** (two real mercenary
  creators through commitment and world appearance across a native area
  reload — run 13; zero resource damage at every checkpoint), the canonical
  `working-save-smoke` **PASS**, and the four-race main-menu creator
  baseline **PASS** with per-character acceptance and zero asset unloads
  (run 14).
- Unfixed-versus-fixed behavioral evidence and the disclosed NOT-RUN matrix
  cells (main-menu campaign-start commitment scenario, both-sexes matrix,
  browsing/cancellation repetition, save/full-exit/reload persistence,
  module-OFF/KMG-only comparisons on this artifact) are recorded in
  `CHARACTER-VISIBILITY-REPAIR-REPORT.md` on the release branch.

## Upgrade

Install **KingmakerGunslinger-0.0.123-character-visibility-repair.zip**
through Unity Mod Manager and restart the game. Existing module settings and
saves are preserved. Characters already affected before this repair resolve
their bodies again on their next load — no save migration is involved.

## Retained baselines

This Kingmaker Gunslinger 0.0.123 release carries the retained qualification
counts forward: the inherited Gunslinger-fixes baseline of 1,288 tests, the
fatigue-authority baseline of 1,325 tests, and the current deterministic suite
of 1,567 tests all pass.

The installable archive is
`KingmakerGunslinger-0.0.123-character-visibility-repair.zip`. The qualified
firearm SoundBank is retained unchanged: `KMG_Firearms.bnk` SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Foreign assemblies — `CraftMagicItems.dll` remains an externally installed
optional mod and is never bundled.

This release is superseded by the teleport polish and specialist cache repair:
install `KingmakerGunslinger-0.0.124-teleport-polish-specialist.zip`
(0.0.124-teleport-polish-specialist) for the genuine Conjuration
specialist-slot fix for existing specialists and the teleport UI polish.
