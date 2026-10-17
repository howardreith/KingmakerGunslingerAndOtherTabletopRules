# Controller save-load repair — alpha 2

Owner-requested test prerelease `v0.0.157-controller-load-alpha.2`. This updates
**Kingmaker Gunslinger / Tabletop Added Rules**, not Buff Planner. It includes
the Brown-Fur lifecycle repair from alpha 1 plus the CotW spontaneous action-bar
adapter correction. Stable v0.0.157 remains Latest; no default-branch merge.

## What to install

Download **KingmakerGunslinger-0.0.157-controller-load-alpha.2.zip** from Assets
and install it through the normal Unity Mod Manager ZIP workflow on your test
computer, replacing the earlier KMG Brown-Fur alpha. Do not use the Source code
archives, install a second mod copy or change the UMM ID/folder.

**This ZIP is the accompanying Gunslinger update itself.** No new Buff Planner
build, CotW DLL replacement, loader change, mod removal or metamagic-setting
change is required by this repair. Keep your existing Buff Planner controller
alpha (the reported installation used 0.5.4-controller.1) and CotW installation.
The adapter is verified only against CotW MVID
`8caab254-aacf-4811-8093-44b9184e6e53` and Kingmaker 2.1.7b game MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`; another contract is refused safely.

**UMM still displays 0.0.157.** These are the unchanged tested candidate bytes,
product `92b442c06dd8d13f246ca006fdda6382b9eb6236`, not a newly rebuilt stable
version. Use the alpha ZIP name and logged DLL hash/MVID to identify it.

## What changed

The installed CotW spontaneous-slot prefix used the unrelated memorized-slot
Spell getter. An exact-assembly reproduction returned BlueprintAbility instead
of the slot's AbilityData. The narrow KMG adapter corrects those three reads,
retaining CotW's own conversions, setting, variants and helper implementation.
It does not globally remove/reorder CotW patches or replace spell execution.
Empty/unready conversion presentation is guarded without inventing another
caster/book. Bounded diagnostics record assembly identities and slot context.
Brown-Fur's stable GUIDs and committed-library retry remain intact.

## Evidence and limits

- Exact packaged DLL: 39/39 assembly-backed adapter assertions and 6/6 selected
  Brown-Fur startup regressions; exact-reference build and strict package PASS.
- Controller campaign save loading, the affected character's first null,
  deferred same-page refresh and physical controller input remain **NOT_RUN**.
  This is a confirmed defect repair, not proof the affected save now loads.
- Nonempty metamagic combinations, storage/restoration fixtures and Buff Planner
  no-cast entry/exit still require observation. No full release-suite rerun or
  native PASS from alpha 1 is claimed for this DLL.

Back up your installation and saves. First test **loading without saving over
the affected save**, then party selection and action-bar pages. Keep the
metamagic conversion setting unchanged. If loading succeeds, try Buff Planner
entry, navigation and Back **without casting first**. Actual routine execution
is a later gate.

Please retain a fresh output_log, say whether you used **Continue or Load Game**
and **controller or mouse/keyboard mode**, and include the new
`cotw-actionbar/assemblies` and `slot.observed` diagnostics if it fails. Do not
clear action bars, respec, edit saves or disable CotW to count this as a pass.

ZIP SHA-256: `798ff15e5aca7b8860f1635c35e9f06260150dc2a91598081d673e1527b3f5f3`.
DLL SHA-256: `e78d79cfb334f6d59c2948b6258128ce73334db59e8f7dba16a01860cc946801`.
DLL MVID: `0fa8ccba-349e-47e1-a2ec-8b46d20ed69c`.
