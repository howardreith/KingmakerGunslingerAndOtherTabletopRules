# Third-party notices

This source milestone contains original Kingmaker Gunslinger code and documentation under the repository license. The planned custom SoundBank is derived solely from the five approved SSE Library: GUNS CC0 recordings documented in `THIRD-PARTY-ASSETS.md`. The current source-only checkpoint contains no generated SoundBank. It does not redistribute Pathfinder: Kingmaker, Unity, Unity Mod Manager, Harmony, Newtonsoft.Json, Wwise authoring tools or SDK binaries, Call of the Wild, Cowboys and Demons, or BlueprintCore.

## Build-time references

A local build references assemblies already present in the developer's Kingmaker and Unity Mod Manager installations. Every external reference is configured with `Private=False`, and package validation rejects copied dependencies.

Expected local references include:

- Owlcat/Kingmaker managed assemblies.
- Unity managed assemblies shipped with Kingmaker.
- Unity Mod Manager.
- Harmony 1.2 compatibility assembly.
- Newtonsoft.Json shipped with the game.

No rights to those assemblies are granted by this repository.

## Reference implementations

The implementation was informed by publicly available source from:

- **Call of the Wild / KingmakerRebalance** — Kingmaker UMM/Harmony bootstrap, blueprint creation, registration, and class-mod architecture.
- **Cowboys and Demons** — firearm identity and a dedicated firearm-proficiency `EquipmentRestriction` precedent in Owlcat's later Wrath runtime.
- **Unity Mod Manager** — `ModEntry.OnGUI` development-panel contract.

No source file from either gameplay mod is copied into this package. The Kingmaker Gunslinger implementation is independently written for the selected Kingmaker baseline and adds stricter stable-ID, transaction, rollback, and per-item-state architecture.

## Game and tabletop intellectual property

Pathfinder names, rules concepts, and related marks belong to their respective owners. Pathfinder: Kingmaker and its assets belong to their respective owners. This milestone contains no extracted game assets and no copied tabletop rules text.

Review the current game EULA, platform terms, Paizo policies, and all asset licenses before public distribution.
