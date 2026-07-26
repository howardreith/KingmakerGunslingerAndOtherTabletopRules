# Sprint 1 Report — Technical Baseline and Architecture Record

## Objective

Establish enough verified technical context and architectural direction to begin the Kingmaker project scaffold without embedding avoidable assumptions from a Wrath of the Righteous implementation.

## Work completed

### 1. Runtime and toolchain pinned

The initial validation target is Pathfinder: Kingmaker Enhanced Plus Edition **2.1.7b**, the final official Steam patch published on 2021-05-27. The development target follows the proven Call of the Wild project shape: .NET Framework 4.7, C# 7.3, AnyCPU, local references to the installed game, Unity Mod Manager, and the Harmony 1.2 compatibility assembly.

Unity Mod Manager **0.32.5** is the development-environment baseline as of 2026-07-12. It is not treated as an embedded runtime dependency: the build will reference the copy installed for Kingmaker and will not redistribute it.

### 2. Reference revisions pinned

Research was performed against fixed revisions rather than floating branches:

- Call of the Wild / KingmakerRebalance: `1332fb0db844b7863f484ca978bea2349fe49769`
- Cowboys and Demons: `68f2ee40ef6dc779df0e104392029b7764684fb5`
- Unity Mod Manager: `6c51f21caa273ffea2747f5bf23c817a0b24bafd` (`0.32.5`)

The exact source paths and URLs are recorded in `SOURCES.md` and `research/reference-revisions.csv`.

### 3. Call of the Wild startup audited

The adopted Kingmaker startup sequence is:

1. Unity Mod Manager invokes `Main.Load`.
2. The mod stores its logger and settings context.
3. A Harmony 1.2 instance is created from the mod identifier.
4. The executing assembly is patched.
5. A postfix on `LibraryScriptableObject.LoadDictionary` runs once.
6. The blueprint library reference is captured.
7. Stable IDs and localization are initialized.
8. Feature modules register their blueprints in a deterministic order.

Call of the Wild demonstrates this sequence and also demonstrates the importance of disallowing accidental GUID generation in release builds. The new project will retain the sequence but split initialization into smaller modules rather than reproducing a monolithic `Main` method.

### 4. Cowboys and Demons firearm design audited

The useful patterns are:

- Firearms remain real weapons.
- Firearm mechanics are attached through focused components and rule-event handlers.
- Reloading is represented by character actions/abilities.
- Crossbow animation and visual data provide a viable fallback.
- Touch-AC behavior, misfires, and damaged-firearm behavior can be implemented in the Owlcat rule pipeline.
- Code/content and custom 3D assets are separated.

The following implementation shortcuts are explicitly rejected:

- Loaded state or damaged state attached to the wielder as a buff.
- Firearm identity inferred solely from `WeaponCategory.HandCrossbow`.
- Touch AC applied without the appropriate range-increment test.
- Ammunition represented only visually, with no inventory transaction.
- One-round capacity embedded as a permanent architectural assumption.
- Direct use of Wrath-only BlueprintCore or Wrath Modification Template APIs in the Kingmaker project.

### 5. Blueprint candidates identified

Four clone candidates are confirmed by public Kingmaker blueprint data:

| Purpose | Blueprint | ID |
|---|---|---|
| Musket weapon-type visual/stat base | `HeavyCrossbow` | `36d0551b8a28587438a47fcbbf53c083` |
| Musket item base | `StandardHeavyCrossbow` | `19a5092244dcf99478dcd73c974828b1` |
| Compact firearm fallback | `LightCrossbow` | `d525e7a6d8d5aa648a976ac41194b8d0` |
| Compact firearm item fallback | `StandardLightCrossbow` | `511c97c1ea111444aa186b1a58496664` |

The `HandCrossbow` category and a projectile GUID used by Cowboys and Demons are retained only as investigation leads. They are not considered confirmed Kingmaker assets until inspected in the installed game.

### 6. Architecture records written

Eight ADRs record the initial decisions:

1. Target runtime and toolchain.
2. Initialization and blueprint registration.
3. Stable blueprint identifiers.
4. Firearm identity and weapon-category adapter.
5. Weapon-based attack pipeline.
6. Per-item firearm state repository.
7. Asset and animation strategy.
8. Dependency policy.

### 7. Licensing and provenance constraints recorded

The package documents the MIT status of Call of the Wild and Unity Mod Manager, the CC0 status of the Cowboys and Demons code repository, and the distinct attribution obligations attached to its external model assets. It also prohibits redistributing game assemblies or extracted game assets.

No license has been asserted for the new project in this sprint. MIT is a reasonable candidate, but that is a project-owner decision and must be recorded before public source release.

## Acceptance check

Sprint 1 required `ARCHITECTURE.md` to name:

- [x] Initialization hook.
- [x] Stable-ID strategy.
- [x] Firearm-category workaround.
- [x] Item-state strategy candidates.
- [x] Test blueprints.

Additional package checks:

- [x] Reference revisions pinned.
- [x] Confirmed blueprint candidates separated from unverified leads.
- [x] No third-party binary or extracted game asset included.
- [x] `source.zip` generated.
- [x] File hashes generated.
- [x] Archive integrity tested.

## Sprint disposition

**Complete.** Sprint 2 can begin from this architecture. The first action in Sprint 2 should be local-environment inspection: confirm installed assembly versions and type signatures before generating the solution files.
