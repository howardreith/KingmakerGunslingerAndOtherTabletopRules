# Races Unleashed summon compatibility

**Status: DEFERRED — NOT AUTHORIZED TO START.**

This is a planning record, not an active execution instruction. Do not begin this work, build,
launch the game or edit Races Unleashed because this file exists. It needs explicit owner
activation, with its own scope and budget.

Parent: the completed cleric mission, [STATUS](../summon-cleric-alignment/STATUS.md). Owner
acceptance of that mission, with these limitations, is recorded there.

## Tested environment

| Item | Value |
| --- | --- |
| Races Unleashed | Id `RacesUnleashed`, version 1.0.11, DLL SHA-256 `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0` |
| Profile | The DATA combined installation: 16 mods enabled in UMM `Params.xml`, including Call of the Wild |
| KMG candidate | `efc75b8cc0b08da3220d9d1151860899d94adb99`, v0.0.180 development, not released |
| Run | `20261109T2033140995495Z-99cc4bd7d946443e95754fd706afc0fa`, **FAIL, 70 PASS / 1 FAIL** |

No standalone Races Unleashed reproduction exists. Every finding below is from the combined
installation, plus a read-only decompile of the installed DLL (`RacesUnleashed.SpiderSummoner`).

Private evidence, not committed, under `C:\Dev\KingmakerGunslingerLab\runtime-evidence\`:

- `20261109T2033140795313Z-disposable-expanded-summoning-sprint32-native-review\`:
  `sprint32-review-progress.json` holds the contracts and cast records. `native-ui-*.png` are the 11 menu images.
- `20261021T2235009759632Z-observe-expanded-summoning-inventory\runtime-result.json`: native row and buff names.
- `expanded-summoning-restoration\20261109T2035521602556Z-disposable-expanded-summoning-sprint32-native-review.json`.

## Row identities

Feature gating all four rows, as both the required fact and the visibility fact: Spider Summoner
`bd03ca4637e74d4caefc66961093f0a0`. Quantity is one, and no row carries alignment descriptors.
Ability identities are the full string, longer than 32 characters.

| Row | Ability identity | Parent, tier | Copied native row | Unit reference |
| --- | --- | --- | --- | --- |
| Giant Crab Spider | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterIGiantCrabSpider` | `8fd74eddd9b6c224693d9ab241f25e84`, I | `8fd74eddd9b6c224693d9ab241f25e84` | `b7e1252281ee46ea8dcd045aa1e56ee9` |
| Giant Black Widow | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterGiantBlackWidow` | `7ed74a3ec8c458d4fb50b192fd7be6ef`, IV | `efa433a38e9c7c14bb4e780f8a3fe559` | `9b1fda47e300491ab44011be9f8ef755` |
| Ogre Spider | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterOgreSpider` | `630c8b85d9f07a64f917d79cb5905741`, V | `0964bf88b582bed41b74e79596c4f6d9` | See B |
| Giant Tarantula | `130211b7fcb443e9b67d5561d75cf6a0SummonMonsterGiantTarantula` | `ab167fd8203c1314bac6568932f1752f`, VII | `2920d48574933c24391fbb9e18f87bf5` | `2ea0fc9b62634a1b99073119c02e673f` |

## Findings and completed work

### A. Shared menus and loading order: completed

Races Unleashed appends feature-gated spider rows to shared summon menus, after KMG publication.
The alphabetization mission (PR #119, `325e10c4a1`) handles that late load and keeps rollback
ownership-safe. See `planning/summon-alphabetical-menus/STATUS.md`. Do not reopen it.

### B. Ogre Spider: enforcement completed, upstream mismatch remains

Loaded graph: a native condition on the caster,
`ContextConditionAlignment{Alignment=Evil;CheckCaster=True;Not=False}`.

| Caster | Selected creature | Unit |
| --- | --- | --- |
| Evil | Redcap | `af8bcff074b93c84b97938e486184222` |
| NonEvil | Bralani | `58574e8d1d4dc464c976f396d9115b1a` |

Those are two possible creature references in the graph. The one observed permitted cast produced
exactly one Bralani.

**Label and effect mismatch, confirmed in the tested combined installation:** the row is named
"Summon Monster V (Ogre Spider)", but the row does not reference the ogre spider unit
`4a205de6d159483d99eba5aa05dd8f48`. Races Unleashed replaces only top-level spawns, and this row's
spawns are nested in its branch.

KMG protection, completed:

- Classification by the branch that actually executes: Good and Chaos for the Bralani, none for the Redcap.
- One decision shared by the popup and execution.
- Forbidden LG/Erastil attempt: command failed, no spawn, no debit.
- Permitted CG/Desna cast: one Bralani, exactly one tier V slot spent.
- The existing Spider Summoner prerequisite is preserved.
- The foreign payload, name and blueprint are unchanged.

The Evil/Redcap branch was inspected structurally and tested in policy, but was not cast natively.

### C. Giant Tarantula: contract verified, no cast performed

Loaded contract: one direct spawn of the unit above, with the post-spawn action
`apply-buff:0dff842f06edace43baf8a2f44207045` only. Census outcome: verified inherently legal.

Menu evidence: present in the Herald tier VII conversion from an ordinary prepared spell for
LG/Erastil, at index 78 of 86, on its original prepared source. **No cast of this row was performed.**

### D. Giant Crab Spider and Giant Black Widow: unresolved

Exact observed post-spawn actions, after the summon buff (`6fcdf014694b2b542a867763b4369cb3` for
the Crab Spider, `50d51854cf6a3434d96a87d050e1d09a` for the Black Widow):

| Row | Caster Evil | Otherwise |
| --- | --- | --- |
| Crab Spider | Fiendish, `SummonMonsterFiendishBuffI` `c707fdbe58d0c614a89a872b76777f4b` | Celestial, `SummonMonsterCelestialBuffI` `83a8a909e7ad19b4fa57e306884cc8bd` |
| Black Widow | Fiendish, `SummonMonsterFiendishBuffI` `c707fdbe58d0c614a89a872b76777f4b` | Celestial, `SummonMonsterCelestialBuffII` `ec906031bb8766f42a37a52cf1d89836` |

Those buffs are in the repository's `NativeMonsterTemplateBuffs`. Their apparent relationship to the
existing caster-matched rule is a supported investigation lead, not a completed classification.

Current behavior:

- Where not governed, both rows retain their native behavior.
- The compatibility census leaves both unresolved. That is why the latest native result is FAIL.
- The Black Widow is missing from Herald tier IV conversions. This follows from the admission
  rule; no tier IV Herald popup was opened.
- By the same rule, the Crab Spider is not admitted to a flattened Herald tier I conversion. Not observed.
- Neither row's popup or cast was observed in the latest run.

Unresolved means unclassified. It is not evidence of an illegal cast, and the earlier "verified
equivalent contract" reading of these rows was withdrawn with the old census shortcut.

### E. Supporting changes already finished

- A common popup and execution policy (`SummonClericAlignmentRuntime.DecideGovernedRow`).
- Herald conversion admits verified foreign rows at their own tier and quantity, under their native prerequisite.
- Module-off protection: with Expanded Summoning inactive, the hooks leave the native result. This has
  focused-test evidence only, not a fresh native profile run.

## Future mission boundary, if activated

Remaining questions and proposed acceptance outcomes:

- Verify and classify both exact template contracts.
- Restore eligible feature-gated choices in the correct Herald tier and quantity, without duplicates or context loss.
- Preserve alphabetical order and native prerequisites.
- Decide explicitly whether to repair the upstream Ogre naming and spawn mismatch or continue restricting its existing effect.
- Validate only the affected menus, casting paths and module boundary.

Not authorized by this record: an audit of every race or feature, edits to Races Unleashed itself, a
new budget, installation changes, or another native campaign.
