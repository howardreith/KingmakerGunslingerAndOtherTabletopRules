# Turn Undead — technical implementation contract

This document accompanies the mission's entry point, development-process instructions, icon integration notes, and acceptance matrix. Implement **Turn Undead only**. The earlier investigation of Command Undead does not authorize implementing that feat, Control Undead, permanent servants, a necromancy overhaul, or a general channel-system replacement.

## 1. Evidence and authority

The owner selected Turn Undead and requested two original images: a feat image and a separate execution/action-bar image. They prefer the execution action in the positive-channel submenu when the loaded game provides one. The word “probably” makes this a placement preference; it is not authority to restructure every existing channel action.

The reviewed TRE baseline is `howardreith/KingmakerGunslingerAndOtherTabletopRules`, `master`, commit `52755b617c79328a972ae5b74bc05784422e332d`. Refresh the actual remote, worktree, and instructions at intake; do not reset another worktree to this research snapshot. Read current `AGENTS.md`, `CLAUDE.md` when present, `docs/VALIDATION-POLICY.md`, the relevant guarded-runtime documentation, and the art files named in the icon notes.

The native identifiers and graph observations below come from the community-published **Kingmaker 2.1.4** blueprint archive. They are discovery leads, not proof of the installed **2.1.7b** engine or the owner's loaded mod graph. Confirm exact contracts against the authorized installation before relying on them. Record the inspected game version and assembly identity. Do not distribute proprietary assemblies, raw game assets, saves, or the blueprint archive in commits or public handoffs.

Useful primary sources:

- [Turn Undead feat, Archives of Nethys](https://aonprd.com/FeatDisplay.aspx?ItemName=Turn%20Undead)
- [Owner-provided Turn Undead page](https://www.d20pfsrd.com/feats/general-feats/turn-undead/)
- [Selective Channeling](https://aonprd.com/FeatDisplay.aspx?ItemName=Selective%20Channeling)
- [Improved Channel](https://aonprd.com/FeatDisplay.aspx?ItemName=Improved%20Channel)
- [Core Rulebook FAQ, including independent channel sources](https://paizo.com/paizo/faq/v5748nruor1fm)
- [Universal monster rules, including channel resistance](https://legacy.aonprd.com/bestiary/universalMonsterRules.html)
- [Native blueprint release](https://github.com/spacehamster/KingmakerDataminer/releases/tag/blueprints)
- [2.1.4 blueprint archive](https://github.com/spacehamster/KingmakerDataminer/releases/download/blueprints/Blueprints2.1.4.zip), research SHA-256 `6ddf33970fb9ed3c127dd8f30b6416d7d23f607303ca61a31aab1972ed43df60`

## 2. Required player-visible behavior

Publish a general feat named **Turn Undead** with the appropriate positive-channel prerequisite. Learning it grants an executable Turn Undead action for each supported positive-channel source the character actually owns. A negative-only channeler, an alignment permission fact alone, a channel-capable item, or a generic channel marker does not independently establish the prerequisite.

One activation uses a standard action and one use of its chosen positive-channel source. It affects eligible undead in a 30-foot radius centered on the caster. Each affected undead receives an initial Will save. A failed save causes one minute of flight. Intelligent undead receive a further Will save each round; mindless undead receive no further saves from this effect. It has no additional healing, damage, automatic destruction, or control effect. It has no Hit Dice ceiling.

For an ordinary cleric the save DC is `10 + floor(cleric level / 2) + Charisma modifier`, plus applicable genuine bonuses. Do not use Wisdom merely because the character casts cleric spells with Wisdom, total character level, the wizard donor's level, or the number of channel damage dice. Source-specific class overrides require the verified mapping in section 3.

Use the game's established fleeing condition to represent the feat's panic behavior. The mission does not require a global fear rewrite, equipment dropping, a new panic engine, changes to enemy loot, or a new disarm system. Document the native flight abstraction honestly in the description/implementation record. Verify the actual movement/action behavior in both combat modes; a buff label alone is not evidence of flight.

Use actual undead creature identity for targeting. A living creature that is healed by negative energy is not automatically undead. In particular, do not inherit CotW's negative-energy-affinity substitution from its damage/healing channels into Turn Undead.

The unmodified rule includes friendly undead. Do not silently make this enemy-only. Inspect the supported Selective Channeling behavior and apply only the supported, documented channel-selection rule. Preserve an explicit test for an allied undead and for a negative-energy-affinity living target. Avoid adding an independent arbitrary “protect party members” rule.

## 3. Channel source binding

### Native sources to investigate and support

The following native sources are in the intended feature coverage. Each must require the actual source feature, not merely the corresponding class or an alignment.

| Source | Native grant/feature lead | Resource and cost observed in 2.1.4 | Required verification |
| --- | --- | --- | --- |
| Positive cleric | `ChannelEnergyFeature` `a79013ff4bcd4864cb669622a29ddafb` | `ChannelEnergyResource` `5e2bba3e07c37be42909a12945c27de7`; cost 1 | Actual positive choice, class level, Charisma DC, existing pool ownership |
| Ordinary paladin | `ChannelEnergyPaladinFeature` `cb6d55dda5ab906459d18a435994a760` | `LayOnHandsResource` `9dedf41d995ff4446a181f143c3db98c`; cost 2 | Channel is gained at the real progression point; ability restrictions and two-use accounting |
| Hospitaler | `ChannelEnergyHospitalerFeature` `a9ab1bbc79ecb174d9a04699986ce8d5` | `ChannelEnergyHospitalerResource` `b0e0c7716ab27c64fb4b131c9845c596`; cost 1 | Separate pool; effective level/DC mapping independent of delayed damage progression |
| Empyreal sorcerer | `ChannelEnergyEmpyrealFeature` `7d49d7f590dc9a948b3bd1c8b7979854` | `ChannelEnergyEmpyrealResource` `f9af9354fb8a79649a6e512569387dc5`; cost 1 | Real feature availability, sorcerer scaling, source-specific Wisdom override and whether it applies to this derived use |

The historical Hospitaler and Empyreal damage abilities have delayed damage progression. Their descriptions and DC overrides cannot safely be reconstructed from the damage rank. Empyreal's grant has `ReplaceAbilityDC` components targeting its original heal/harm actions with Wisdom; merely adding a new ability will not automatically put its GUID into those components. Confirm the exact class rules and installed parameter-calculation path. Write a small source-contract table with effective level, save statistic, resource, cost, restrictions, and evidence before completing these bindings. If printed and installed semantics materially disagree, report the precise conflict and continue independent work; do not silently invent a balance rule.

Relevant native action identities:

| Source | Heal action | Harm-undead action |
| --- | --- | --- |
| Cleric | `ChannelEnergy` `f5fc9a1a2a3c1a946a31b320d1dd31b2` | `ChannelPositiveHarm` `279447a6bf2d3544d93a0a39c3b8e91d` |
| Paladin | `6670f0f21a1d7f04db2b8b115e8e6abf` | `4937473d1cfd7774a979b625fb833b47` |
| Hospitaler | `0c0cf7fcb356d2448b7d57f2c4db3c0c` | `cc17243b2185f814aa909ac6b6599eaa` |
| Empyreal | `574cf074e8b65e84d9b69a8c6f1af27b` | `e1536ee240c5d4141bf9f9485a665128` |

### Binding invariants

1. Grant an alternative use of the existing source. Do not create, duplicate, refill, or reset its daily pool when taking the feat, reloading, rebuilding the action menu, or toggling the module.
2. Consume the correct cost once per executed attempt, not once per target or saving throw. Successful target saves do not refund the attempt. A failed prerequisite/resource check must not debit another source.
3. A multiclass character chooses the source explicitly when more than one is available. Keep source levels, resources, DC parameters, and genuine bonuses separate. Do not silently borrow an undepleted pool or sum class levels unless a specific supported feature grants that progression.
4. Respect the chosen source's existing availability restrictions. The new action must not bypass a disabled or unavailable channel by using a new unconstrained wrapper.
5. Extra uses and modifiers should work through the actual shared source. Do not fix or redesign unrelated Extra Channel behavior in this mission.
6. Preserve the wizard necromancer's original ability and resource. A pure necromancer should not acquire this new general feat merely because its school grants the existing turning power. A qualifying multiclass caster must keep the two sources distinct.

## 4. Native implementation leads and hazards

### Core donor graph

`TurnUndeadNecromancy` (`71b8898b1d26d654b9a3eeac87e3e2f8`) supplies a supernatural standard action, `AbilityTargetsAround` radius 30, actual `UndeadType` condition, initial Will save, one-minute `FrightenedUndead`, and wizard-specific resource/scaling.

`FrightenedUndead` (`1265c0b27ef559c4e980a900dfc68d1e`) contains a `BuffStatusCondition` with `Condition=Frightened`, `SaveEachRound=true`, and `SaveType=Will`. It has no intelligent/mindless conditional and no `SpellDescriptorComponent` in the historical dump.

`UndeadType` is `734a29b693e9ec346ba2951b27987e33`. `UndeadImmunities` is `8a75eb16bfff86949a4ddcb3dd2f83ae`.

Build private assets, deep-copy only the necessary donor components, replace source binding explicitly, and remove irrelevant inherited fields after inspecting their engine meaning. In particular, check donor metamagic flags, spell resistance, dispellability flags, `Parent`, action-bar visibility, targeting, and parameter calculation. A supernatural feat must not accidentally become a spell, provoke because of spell defaults, consume spell slots, or inherit damage/healing actions.

### Immunity handling

Generic Frightened carries descriptors blocked by undead immunities. Use the narrow undead-specific exception represented by the native turning pattern; do not remove immunity facts from creatures or change global fear/compulsion policy. Separate explicit condition immunity or story protection may still matter. Resolve those through actual rules and installed contracts, with a focused test, rather than assuming every protection is bypassed.

### Repeat saves and saved context

Own the intelligent-only repeat-save decision. Initial saves still apply to mindless undead. Use the game's `RuleSavingThrow`/buff machinery so real modifiers and logging work. Preserve the correct ability reason/context; channel resistance and ability-specific effects can depend on identity.

Persist the effect's source identity, initial save context/DC as required by the verified engine contract, remaining duration, and classification needed by the saved effect. Re-entering a scene or loading must not restart the minute, create an immediate extra save, suppress all remaining intelligent saves, or spend new channel uses. Round boundaries must behave correctly in real time and turn-based combat.

The existing [GorgonPetrification.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Summoning/GorgonPetrification.cs) demonstrates a saved recovery DC and controlled use of native `BuffStatusCondition` round-save behavior. It also warns that `BuffCollection.AddBuff` already owns its native application event; do not accidentally issue a second `RuleApplyBuff` around the same application.

Follow the repository's exact saved-component naming and primitive serialization conventions. `ExpandedSummoningSpecialBuilder.ControlEffects32.cs` documents a `Fact.PostLoad` dependency on stable component names. Do not serialize live Unity/game objects or change released component identities.

A successful save against a new attempt must not remove a separate existing fear effect. Removal/expiry must affect the owned application, preserve unrelated effects, and handle multiple casters deterministically according to the supported same-effect stacking rules. Do not mutate the shared wizard buff to obtain this behavior.

## 5. Intelligent versus mindless classification

The historical game data does not encode tabletop mindlessness reliably as a low Intelligence score:

| Observed unit | Blueprint identity | Intelligence | Finding |
| --- | --- | --- | --- |
| Ordinary skeleton | `e8658c2a211043e4b983a666d3c7068e` | 10 | A numeric Intelligence threshold misclassifies it |
| Ordinary zombie | `4c44d7b485cbc8741a1c91affd8a03f3` | 10 | Same problem |
| Greater zombie lord | `a2803487ba242f144a3df7c672cf58ac` | 12 | Shares the ordinary zombie's type and “Dumb Monster Brain” |

Confirm these and the available runtime facts in 2.1.7b. First look for a reliable explicit runtime characteristic. If none exists, implement a small data-backed classification with verified stable identities/template relationships and explicit exceptions. Keep policy separate from native adapters so it can be tested with real classification records. Permit narrow optional-provider registration if the existing project has that convention.

Prohibited shortcuts: name-substring matching as the authoritative rule; `Intelligence > 0`/`> 2`; treating every skeleton/zombie type as mindless; using the AI brain as the discriminator; lowering creature Intelligence to repair the classification.

Create a concise classification inventory for the supported native undead and actually selected compatibility profiles. Record the evidence for each rule/group and its exceptions. Make unknown classification explicit in development diagnostics. Resolve unknowns in supported content before claiming that intelligent-only repeat saves are complete. If arbitrary third-party content cannot be classified, describe that compatibility boundary and the actual behavior; do not quietly claim universal coverage or silently convert all undead to the native repeat-save simplification.

## 6. Action-bar placement and the two image consumers

### What the research actually found

The 2.1.4 native positive-channel features grant separate heal and harm actions; those inspected actions have no `Parent` and no `AbilityVariants` wrapper. By contrast, CotW's `ChannelEnergyEngine.init()` creates wrappers such as `ClericPositiveChannelHarmBase` and replaces the granted actions with those wrappers. Do not assume the owner's submenu exists in vanilla or has the same identity in every profile.

Primary source: [CotW ChannelEnergyEngine.cs](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/ChannelEnergyEngine.cs), especially initialization and channel registration. Wrapper construction: [Common.cs](https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/Common.cs), `createVariantWrapper`.

### Required presentation

Use the supplied **feat artwork** on the new general feat in character creation/level-up and the character's feat record. Use the supplied **action artwork** on the executable Turn Undead ability and any directly pinned execution entry. These are distinct roles; do not swap them or reuse one image for both merely because the names match. Follow `ICON_INTEGRATION_NOTES.md` and the final asset manifest for exact filenames, dimensions, and checksums.

Prefer one feat-gated Turn Undead option under the existing supported positive-channel wrapper for the appropriate source. The positive damage-undead wrapper is the natural candidate when a provider offers separate heal/harm submenus. Confirm the actual loaded graph and existing user-facing labels. Preserve every preexisting variant, its order/identity where possible, its resource behavior, and saved action-bar references.

Add only the new child, idempotently, and remove only that owned contribution when publication requires rollback. Gate both visibility and execution on the feat and the real source. A global `AbilityVariants` list must not make the option usable by every channeler. A single blueprint's `Parent` cannot safely be reassigned among several independent wrappers; use stable source-specific child identities as necessary.

When the loaded profile has no suitable channel submenu, expose a normal Turn Undead action with the correct icon; use an intelligible source selector only for genuine multiple-source cases. The owner's placement preference does not require reparenting all native channel abilities, replacing their grants, or building a new universal action-bar system. Document this profile-specific presentation and prove the chosen action is discoverable, executable, and pinnable in the real UI.

Inspect the native variant menu, empty-resource state, tooltip, cooldown/action-spent feedback, drag-to-hotbar behavior, and save/load of a pinned action. Do not use screenshots alone to claim resource or save mechanics work.

## 7. Channel resistance, bonuses, and CotW compatibility

The historical `ChannelResistance4` (`a9ac84c6f48b491438f91bb237bc9212`) uses `SavingThrowBonusAgainstSpecificSpells` with a four-entry ordinary damage-channel list. The native wizard Turn Undead is absent. New ability identities need intentional integration; the engine will not infer eligibility from the displayed name or the spent resource.

Append the relevant private ability identities through a narrow verified integration, preserving existing entries and exemptions. Confirm that both initial and repeat saves carry the right context, and that genuine bypass features apply only under their applicable rules. Do not repair the original wizard capstone/resistance behavior as an incidental side effect of this mission.

Audit the currently supported DC modifiers, including Improved Channel when provided, and source-specific modifiers or items actually present in selected profiles. Add narrow registrations/adapters for applicable bonuses. Test a non-applicable spell bonus to ensure it is not accidentally inherited. Avoid recalculating a duplicate bonus by hand on top of the native rulebook result.

**Do not register Turn Undead as an ordinary `PositiveHarm` damage channel merely to reuse CotW's `storeChannel`.** That method also creates or attaches Quick Channel, swift channeling, Channel Smite, versatile shapes, and other transformations. Those side effects can alter action cost, targeting, resource use, and scope. Use only the supported registration surfaces actually needed for wrapper placement, DC modifiers, and resistance, or a narrowly scoped compatibility adapter. New quick/smite/cone/line Turn variants require a separately justified rules scope; they are not deliverables of this mission.

CotW remains optional. The vanilla profile must work without the DLL. Preserve existing load-order/version detection and failure handling. Do not add a hard library reference or copy an entire external channel engine. Audit supported positive-channel features present in the selected CotW profile, including source-specific prerequisites/scaling, and record the exact implemented coverage. Unsupported external channel sources must not be advertised as working.

## 8. Module registration and save compatibility

Follow the existing dedicated-feat module pattern. Relevant current code:

- [FeatureModuleConfiguration.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/FeatureModules/FeatureModuleConfiguration.cs)
- [BlueprintBootstrap.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs)
- [AcadamaeFeatCatalogPublication.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/AcadamaeFeatCatalogPublication.cs)
- [BlueprintRegistry.cs](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/52755b617c79328a972ae5b74bc05784422e332d/src/KingmakerGunslinger/Blueprints/BlueprintRegistry.cs)

Allocate stable new identities using the current manifest process. Register save-referenced definitions independently from publication into selections. Publish into the general feat selection, not fighter-only combat selections. Update both selection arrays through the established publication transaction, deduplicate, and preserve other mods' contributions.

Use the repository's existing module-default and saved-settings conventions. Disabling publication must not make learned feats, buffs, or saved hotbar references unresolvable. Specify the behavior of existing owners while the setting is disabled according to the project's established policy. Do not implement an undocumented automatic respec or clear saved facts.

No new dependency is expected. Put rules, source selection, classification, and save-state decisions in small testable policies; keep blueprint construction and native rulebook adaptation at the existing boundaries. Reuse components and conventions, but do not copy a large unrelated summoning subsystem into a new folder.

## 9. Validation and evidence contract

Use `ACCEPTANCE_MATRIX.md` as the finite acceptance inventory. Tests should execute production decisions and actual adapters where practical. Do not prove mechanics by checking source text, the presence of a field, a fabricated event trace, or a DTO that mirrors the implementation. Mock only genuine external boundaries; prefer real rules/data and narrow test seams. Add mutation/failing-baseline checks only where needed to establish that an important test catches the original hazard.

Follow current `docs/VALIDATION-POLICY.md`: focused changed-behavior checks and production compilation first, then affected guarded native scenarios. The ordinary 15-minute planning target and 30-minute ceiling are workflow constraints, not predictions of build speed or authorization to abort safe cleanup. Do not launch an unchanged broad campaign repeatedly. Reuse a verified unchanged candidate, preserve failed evidence, and narrow the next check around a concrete remaining risk.

Runtime requests must use current repository-owned guards, the correct Steam application, an allowed isolated fixture/disposable save, resource leases, and transactional staging/restoration. Existing human installations and unrelated saves remain protected. Record exact product commit, binary/package hash, fixture/observer version, environment profile, observed results, and deferred checks. A test harness change that enters the product changes the candidate; a text-only report does not justify rebuilding it.

Final evidence must establish ordinary feat acquisition, actual ability-menu use, correct resource debit, genuine Will-save outcomes, actual flight, intelligent-only recovery, expiration, and a mid-effect save/load. Include the supplied icons at normal UI sizes and their proper consumers. Build/domain PASS and screenshots alone do not prove these native behaviors.

End at the owner-review handoff required by `DEVELOPMENT_PROCESS.md`. Include the coherent draft PR, tested artifact identity, supported source/profile table, classification inventory, known adaptations, and exact remaining unknowns. Do not merge, tag, publish a release, or leave a development build in the normal-play installation.
