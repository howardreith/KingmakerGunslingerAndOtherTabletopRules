# Licensing and Provenance Record

**This document is an engineering provenance checklist, not legal advice.**

## Current package

Sprints 1 and 2 contain original research notes, architecture documents, build scripts, project source, schemas, and package metadata. It contains no copied source file from a reference mod, no game assembly, no Unity assembly, no Unity Mod Manager assembly, no Harmony assembly, and no extracted game icon, audio, model, texture, animation, localization database, or other game asset.

## Reference licenses

| Reference | Observed license | Engineering consequence |
|---|---|---|
| Call of the Wild / KingmakerRebalance | MIT | Design patterns may be studied. If source is copied or substantially adapted, preserve the MIT notice and record the file-level provenance. |
| Cowboys and Demons code repository | CC0-1.0 | Code may be used under CC0, but provenance should still be recorded for maintainability and credit. |
| Cowboys and Demons external model assets | Individual third-party terms, including CC BY examples | The code repository’s CC0 status does not license every separately distributed model. Each model requires its own permission and attribution review. |
| Unity Mod Manager | MIT | Reference the installed DLL; do not bundle it unless a deliberate distribution review says otherwise. |
| Pathfinder: Kingmaker / Owlcat / Unity assemblies and assets | Proprietary or separately licensed | Local build references only. Do not commit or package them. |

## New project license

Original Kingmaker Gunslinger source and documentation are licensed under the MIT License, attributed to Howie and Kingmaker Gunslinger contributors. This license does not relicense Kingmaker, Unity, UMM/Harmony binaries, reference-mod source, tabletop text, or future third-party assets. Those retain their own terms and provenance requirements.

## Source-adaptation policy

For every future copied or materially adapted block, add a provenance entry containing:

```text
Project file and line/section
Reference project
Reference file
Pinned commit
Original license
Nature of use: copied / adapted / idea only
Changes made
Required notice location
```

Prefer original implementation against Kingmaker APIs. Copy source only when it materially reduces risk and the license/attribution path is clear.

## Game data policy

Allowed engineering practice for this project:

- Refer to a vanilla blueprint by stable ID.
- Clone or inspect it at runtime through the installed game.
- Store original mod-created blueprint data and localization.
- Require the user to own and install the game.

Prohibited in source/release packages without a separate rights review:

- Redistributing `Assembly-CSharp.dll`, Unity DLLs, game data bundles, or UMM/Harmony binaries.
- Shipping extracted Owlcat icons, models, animations, textures, sounds, portraits, or voice audio.
- Repackaging reference-mod binary releases.
- Treating a public blueprint dump as a redistributable game-data bundle.

## Pathfinder rules text and trademarks

The project will use original, concise implementation descriptions rather than copying long rules passages. Names and compatibility statements must be reviewed against Paizo’s current license pages and policies before public release.

Paizo’s current site exposes both Fan Content and Community Use materials, and its 2024 policy history changed more than once. This package does not assume which policy governs a software mod. Before public release:

1. Re-read the current Paizo license index, Fan Content Policy/FAQ, Community Use Policy, and any relevant compatibility/open-content materials.
2. Determine which notice and logo/trademark restrictions apply to a free video-game mod.
3. Include the required notice verbatim only after confirming the applicable policy.
4. Avoid implying endorsement by Paizo or Owlcat.

## Owlcat and platform terms

Owlcat’s current Terms of Use state that game use is also subject to the applicable EULA and the distribution platform’s terms. The public Owlcat EULA page presently appears focused on Wrath of the Righteous, so it is not treated as a Kingmaker-specific license source here. The developer should review the Kingmaker EULA supplied with the installed Steam/GOG game and applicable platform terms before publication.

## Asset-package policy

Custom models and other substantial assets will be separately packaged and versioned. Each asset must have:

- Original author/source.
- License.
- Modification permission.
- Redistribution permission.
- Required attribution text.
- Source URL and retrieval date.
- Whether commercial use is relevant.
- Exported derivative files covered by the permission.

If any field is unclear, the asset is excluded. The core mechanical package must continue working without it.

## Attribution file planned for source release

`THIRD-PARTY-NOTICES.md` is present and must be updated before copied or materially adapted third-party code is introduced. At minimum it will list Call of the Wild, Cowboys and Demons, Unity Mod Manager, and any later asset/software dependency actually incorporated. Merely studying a repository will still be acknowledged in project documentation even when its license does not require attribution.
