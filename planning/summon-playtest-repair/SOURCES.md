# Sources and Implementation Map

Prepared November 7, 2026. This is a planning inspection, not a source-code patch or new game run.

## Evidence hierarchy

1. The owner's latest manual report controls product scope and supersedes earlier preferences where explicitly changed.
2. The thirteen supplied images establish visible baseline conditions; see EVIDENCE-INDEX.md.
3. Current source identifies implementation seams. Recheck current master before editing.
4. PF1 primary rules establish a baseline where the owner requested an audit. Explicit owner adaptations remain authoritative.
5. Installed donor spell/mod behavior must be identified on DATA. Screenshots and spell names alone do not establish provider ownership.

## Inspected repository baseline

Repository: [KingmakerGunslingerAndOtherTabletopRules](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules).

Exact source: [7ed6e247471d346cfd4b190844e3891639e16f24](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/tree/7ed6e247471d346cfd4b190844e3891639e16f24). Master at planning time includes v0.0.179 publication records. This does not identify the owner's installed or screenshot build.

- [AGENTS.md](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/AGENTS.md)
- [docs/VALIDATION-POLICY.md](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/docs/VALIDATION-POLICY.md)

Read current repository instructions before execution. The ordinary validation target/ceiling is 15/30 minutes, not an unlimited continuation of the historical Wall campaign. Required safe restoration still completes. Follow the repository's current guarded deployment and push workflows.

## R1 source entry points

All code paths below are relative to src/KingmakerGunslinger/.

| Area | Source | Planning finding |
| --- | --- | --- |
| Stirge withholding | [Summoning/SummonVisibilityCatalog.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs) | Existing suppression design preserves registered definitions. SuppressedCreatureKeys is empty and validation currently pins no suppression. Update only the intended publication policy. |
| Stirge higher-tier routes | [Summoning/ExpandedSummoningCatalog.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs) | Stirge starts at SNA I; GenerateVariants yields choices through SNA IX. There are nine canonical affected logical placements at this baseline. Derive actual counts after intake. |
| Public filtering | [Blueprints/ExpandedSummoningPublication.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningPublication.cs) | Public composition uses IsPublished. Preserve registration while withholding choices. |
| Alternate routes | [Summoning/AlternateSummoningProjection.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/AlternateSummoningProjection.cs) | Default alternate projection shares the visibility predicate. Verify supported consumers, not only the level-I menu. |
| Bone Devil Wall | [Blueprints/ExpandedSummoningSpecialBuilder.Infernal35.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Infernal35.cs) | ConfigureBone35 separates Wall configuration, source cleanup, ability grant, and WallAi. Remove exposure, retain required legacy configuration. |
| Ice Devil Wall | [Blueprints/ExpandedSummoningSpecialBuilder.IceDevil40.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.IceDevil40.cs) | ConfigureIceDevil40 similarly has separate configuration, lifetime cleanup, grant, and AI. Source work does not require bypassing a restricted native phase. |
| Legacy Wall retirement | [Summoning/InfernalWallRules35.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/InfernalWallRules35.cs) | InfernalWallSection35 and InfernalWallSourceLifetime35 release owned wall/residual/navigation state on appropriate lifecycle events. Retain those dependencies for old objects. |
| Magma profile | [Summoning/ExpandedSummoningSpecialProfiles.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs) | Magma variant's SpellLikeTwo is MagmaForm. Four base types and six variants must all be considered in the later mephit audit. |
| Magma grants and state | [Blueprints/ExpandedSummoningSpecialBuilder.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs) | ConfigureMephitVariants grants ability/resource/AI from profile slots. ConfigureMagmaForm owns retained implementation. Nulling a slot without checking serialized state can leave incomplete legacy configuration. |
| Stable identities | [Summoning/ExpandedSummoningIdentityCatalog.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs) | Keep Magma and other withdrawn feature identities registered. |
| Lillend grant | [Blueprints/ExpandedSummoningSpecialBuilder.Lillend32.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Lillend32.cs) | ConfigureLillend32 owns BardicSuggestion grant, parameters, and dispatcher reference. Preserve the separate spell. |
| Lillend automatic use | [Summoning/LillendNoncombat32.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/LillendNoncombat32.cs) | Automatic BardicSuggestion route is separate; an existing null guard can support detachment. |
| Shared Suggestion setup | [Blueprints/ExpandedSummoningSpecialBuilder.Suggestion32.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Suggestion32.cs) | ConfigureSuggestion32 handles Satyr, Lillend spell, and Lillend Bardic consumers together. Preserve spell-list/provider behavior and valid retained configuration. |
| Fire Giant Sunder | [Blueprints/ExpandedSummoningSpecialBuilder.Giants.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Giants.cs) | Fire Giant feat list includes NativeImprovedSunder. Removing only an action button would leave that capability source. |
| Storm preservation / Cloud later | [Blueprints/ExpandedSummoningSpecialBuilder.Giants29.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Giants29.cs) | Storm has a separate Improved Sunder grant. Cloud has both custom mist/fog grants and AI routes. Preserve Storm; replace Cloud only in R5 after resolving its actual donor. |

The previously accepted Sprint 4 report established that the installed ProperFlanking2 can automatically replace an Improved Sunder holder's weapon attack with a sunder maneuver. Treat the currently installed mod/settings as the live contract and verify Fire Giant's scoped removal against it; do not infer success from a missing hotbar button.

## Later source leads

- [SummonSameTurnActivationPolicy.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/SummonSameTurnActivationPolicy.cs) and [SummonSameTurnActivationRuntime.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/SummonSameTurnActivationRuntime.cs): existing cast tracking, deferred spawn/enrollment, and native action gating. These are investigation entry points, not a diagnosed regression.
- [ExpandedSummoningProjectIcons.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Blueprints/ExpandedSummoningProjectIcons.cs): existing project icon manifest/cache. Audit actual initiative portrait consumers before adapting these images.
- [LanternArchonOrbView.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/7ed6e247471d346cfd4b190844e3891639e16f24/src/KingmakerGunslinger/Summoning/LanternArchonOrbView.cs): owns a golden orb on a donor and retains native renderers. A donor protrusion is plausible but requires observation.
- ConfigureSprint29Cloud / Sprint29CloneFogSpell in Giants29.cs: current custom mist/fog replacement seam. A blueprint's internal “native” label does not prove it is the working spell in the owner's screenshot.
- ConfigurePixieDanceState / ConfigurePixieDance / ConfigurePixieAi / ConfigurePixie in ExpandedSummoningSpecialBuilder.cs: current custom Dance implementation and grant to replace conditionally.

## PF1 primary references

| Reference | Source | Application |
| --- | --- | --- |
| R-01 Mephits | [Official legacy PRD — Mephit](https://legacy.aonprd.com/bestiary/mephit.html) | All ten breath damage types and the contextual dice/sickness table in MISSION.md. Water uses acid; air/dust/salt are slashing, earth bludgeoning, ice cold, fire/magma/steam fire, ooze acid. Avoid 3.5 edition drift. |
| R-02 Acadamae Graduate | [Archives of Nethys — feat](https://aonprd.com/FeatDisplay.aspx?ItemName=Acadamae%20Graduate) | Casting-time reduction and caster fatigue save. AoN identifies its 3.5 origin; preserve the project's established adaptation rather than reopening eligibility. |
| R-03 Summon Monster | [Official legacy PRD — Summon Monster](https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html) | Immediate activity when the creature appears on the caster's turn. |
| R-04 Surprise rounds | [Official legacy PRD — Combat](https://legacy.aonprd.com/coreRulebook/combat.html#surprise) | Separate awareness/participation and restricted actions from ordinary-round summon timing. General text does not resolve all engine arrival edge cases. |
| R-05 Ice Devil | [Archives of Nethys — Ice Devil](https://aonprd.com/MonsterDisplay.aspx?ItemName=Ice%20Devil%20(Gelugon)) | Printed 10-foot fear aura, DC 22, is a lead matching the screenshot. Not proof of the runtime source. |
| R-06 Fear | [Archives of Nethys — Fear universal rule](https://aonprd.com/UMR.aspx?ItemName=Fear) | Does not supply a blanket 24-hour successful-save immunity for every PF1 fear aura. Do not import that rule from a different edition or from frightful presence. |
| R-07 Mastodon | [Official legacy PRD — Elephant / Mastodon](https://legacy.aonprd.com/bestiary/elephant.html) | Mastodon explicitly has Trample, with its own values. |
| R-08 Trample | [Official legacy PRD — Universal monster rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html#trample) | Appropriate movement/targeting and once-per-target-per-round behavior. |
| R-09 Pixie | [Official legacy PRD — Pixie](https://legacy.aonprd.com/bestiary/pixie.html) | Printed daily SLAs and Lesser Confusion differ from the owner's four requested at-will spells. Implement the explicit owner adaptation; Dance frequency is separate. |

## Unresolved facts to establish during implementation

- Actual installed/tested binary and mod profile on DATA.
- Exact provider/GUID of the usable Obscuring Mist and Irresistible Dance spells.
- Actual owner/GUID of the blank blast action.
- Source and recurrence mechanism of the Frightened saves.
- Cause of different first-action delays and correct surprise-round integration.
- Effective Water Mephit damage packet, separately from its visual.
- Meaning of Griffon's “two charges.”
- Cause of Lantern's blue protrusion and source of the persistent generic summon sheen.

Do not turn any of these leads into a claimed diagnosis before observing the relevant code/runtime behavior.
