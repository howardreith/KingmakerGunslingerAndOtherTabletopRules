# Expanded Summoning final-live inventory

Status: final and release-qualified. The early discovery observations retained
below are historical evidence; the authoritative selected-donor result is the
55-GUID catalog audit and final structural run recorded under "Complete frozen
donor graph audit" and "Final release qualification". No donor selection
remains pending.

Evidence source: guarded fresh-process run
`20260811T1727529145302Z-observe-expanded-summoning-inventory`, status PASS,
version 0.0.77, save-free. The installed final-live profile included Call of the
Wild, so broad summon-name matching returned 523 abilities and must never be
used as a publication selector.

## Canonical Summon Monster parents

| Tier | GUID | Blueprint name |
|---:|---|---|
| I | `8fd74eddd9b6c224693d9ab241f25e84` | `SummonMonsterISingle` |
| II | `1724061e89c667045a6891179ee2e8e7` | `SummonMonsterIIBase` |
| III | `5d61dde0020bbf54ba1521f7ca0229dc` | `SummonMonsterIIIBase` |
| IV | `7ed74a3ec8c458d4fb50b192fd7be6ef` | `SummonMonsterIVBase` |
| V | `630c8b85d9f07a64f917d79cb5905741` | `SummonMonsterVBase` |
| VI | `e740afbab0147944dab35d83faa0ae1c` | `SummonMonsterVIBase` |
| VII | `ab167fd8203c1314bac6568932f1752f` | `SummonMonsterVIIBase` |
| VIII | `d3ac756a229830243a72e84f3ab050d0` | `SummonMonsterVIIIBase` |
| IX | `52b5df2a97df18242aec67610616ded0` | `SummonMonsterIXBase` |

Tier I is itself the one-creature spell and has no `AbilityVariants` parent in
the observed final-live graph. Tiers II-IX use `AbilityVariants` and contain the
native one/1d3/(where eligible) 1d4+1 children.

## Canonical Summon Nature's Ally parents

| Tier | GUID | Blueprint name |
|---:|---|---|
| I | `c6147854641924442a3bb736080cfeb6` | `SummonNaturesAllyI` |
| II | `298148133cdc3fd42889b99c82711986` | `SummonNaturesAllyII` |
| III | `fdcf7e57ec44f704591f11b45f4acf61` | `SummonNaturesAllyIII` |
| IV | `c83db50513abdf74ca103651931fac4b` | `SummonNaturesAllyIV` |
| V | `8f98a22f35ca6684a983363d32e51bfe` | `SummonNaturesAllyV` |
| VI | `55bbce9b3e76d4a4a8c8e0698d29002c` | `SummonNaturesAllyVI` |
| VII | `051b979e7d7f8ec41b9fa35d04746b33` | `SummonNaturesAllyVII` |
| VIII | `ea78c04f0bd13d049a1cce5daf8d83e0` | `SummonNaturesAllyVIII` |
| IX | `a7469ef84ba50ac4cbf3d145e3173f8e` | `SummonNaturesAllyIX` |

Tier I is a direct ability. Tiers II-IX expose `AbilityVariants`.

## Confirmed native/optional mechanics

- Native children use `AbilityEffectRunAction`, caster-level rank, Conjuration
  `SpellComponent`, descriptor component, close range, and standard action in
  the observed final-live graph.
- Call of the Wild supplies `SuperiorSummoning` GUID
  `0477936c0f74841498b5c8753a8062a3` and quantity children calculate
  `ProjectilesCount` from it; KMG should clone/preserve this native semantic
  shape rather than globally patch spawn counts.
- Final-live parents have many appended optional spell-list components. KMG must
  mutate only the exact `AbilityVariants` collection and preserve every other
  component/reference.
- Optional summon-class surfaces (Summoner, Master Summoner, Monster Tactician,
  Feral Hunter, Fey Caller, spell-kenning/wish/shadow clones) are distinct
  abilities. They require exact structural signatures and must not be selected
  by substring.

At this discovery stage donor-unit and sanitizer rows remained in progress;
broad roster-term matching found 533 candidates and was evidence for narrowing,
not selection authority. The later frozen donor graph supersedes this stage.

## Dedicated summon donors confirmed in composed final-live graph

These were the initial donor candidates. The later frozen donor graph and final
structural/visual runs record the approved selections, full sanitizer audit,
source-nonmutation proof, and instantiated-view contract.

| Intended/proxy | GUID | Blueprint name |
|---|---|---|
| Giant frog | `1ed9a630f0d9d7f44855d3d1d1b2cdf2` | `GiantFrogSummoned` |
| Dire wolf | `03dd28e92faf2e44eb9564a6ba01fdd0` | `DireWolfSummon` |
| Giant spider | `9e120b5e0ad3c794491c049aa24b9fde` | `GiantSpiderSummoned` |
| Leopard | `768275c9885dd954fb3c84ba69ac4281` | `LeopardSummoned` |
| Monitor lizard | `4109b40f6bbb49640840644cc84ada67` | `MonitorLizardSummoned` |
| Dire boar | `6ec9c63c41a1e754ea4dcd85557625b4` | `DireBoarSummoned` |
| Air elemental, small | `04944455200bc224d955a8e9bbd64f3f` | `SummonedAirElementalSmall` |
| Air elemental, large | `3764b43791a00e1468257adbca43ce9b` | `SummonedAirElementalLarge` |
| Air elemental, huge | `2e24256e459468743b91fbb9aa85e1ab` | `SummonedAirElementalHuge` |
| Air elemental, elder | `33bb90ffd13c87b4c8e45d920313752a` | `SummonedAirElementalElder` |
| Air mephit | `50782bc4eb36aac4287023e20ee00808` | `MephitAirSummoned` |
| Earth mephit | `46779f56cab2cb0438161fec0129790d` | `MephitEarthSummoned` |
| Fire mephit | `10a820de0a417f345866f794324205ad` | `MephitFireSummoned` |
| Water mephit | `4615328295cd7e84bb2ef09d3dba8403` | `MephitWaterSummoned` |
| Hell hound | `ece348345859351439e1263115f5fdb9` | `HellhoundSummoned` |
| Bralani | `58574e8d1d4dc464c976f396d9115b1a` | `AzataBralaniSummoned` |
| Smilodon | `beae4985629a6f64eb98081e3171e4c1` | `SmilodonSummoned` |
| Mastodon | `028cc6f46e7998f46855a33ffde89567` | `MastodonSummon` |
| Soul eater proxy | `1832be68f9814254dbbdab6df7fd5d0b` | `SoulEaterSummoned` |

The composed graph also confirms plausible non-summon visual donors including
standard Worg `313a17cbd273d1f40bd1654ee2ae186e`, Hodag
`c3524f96954a1d94f8525b86e7626633`, Erinyes
`6ea3a75279bab234aa723989e30cb15a`, Nymph
`0cc7a2526e4557945b1d8eb277d1fb3a`, Ankou
`58ed91a92b8d70248aa884d303954469`, and Nixie
`394610e32cfbc4f43a0efaab16faae49`. These are visual/mechanical clues only;
campaign units will never be summoned directly.

## Exact donor structure audit

Guarded fresh-process run
`20260811T1741299016346Z-observe-expanded-summoning-inventory` inspected the
exact 25 GUID allowlist on source commit
`df65c391365ce52367f05b457e6f2bc6a61a3a09`. All 25 were found; no save was
selected, loaded, or written.

The audit proves that a `Summoned` name is not a safety contract. Every
inspected dedicated donor except Giant Spider retains an `Experience`
component. Dire Wolf, Mastodon, and Smilodon also retain `AddLoot`; the latter
two therefore require the same stripping as ordinary campaign donors. Native
dedicated donors already use faction `Summoned`
(`1b08d9ed04518ec46a9b3e4e23cb5105`) and are preferred for body, prefab,
animation, and combat-profile clues only. KMG clones must create independent
component/fact arrays, remove XP/loot and other forbidden surfaces, and prove
the source blueprint unchanged.

The Worg, Hodag, Erinyes, Nymph, Ankou, and Nixie candidates use campaign
factions and may include `AddLoot`, `AddTags`, `MobCaster`, or post-load fixer
components. They are visual donors only. No campaign unit is eligible for
direct spawning.

### Dedicated elemental summon identities

| Element | Small | Medium | Large | Huge | Greater | Elder |
|---|---|---|---|---|---|---|
| Air | `04944455200bc224d955a8e9bbd64f3f` | `676f8b7d0a170674cb6e504e0e30b4f0` | `3764b43791a00e1468257adbca43ce9b` | `2e24256e459468743b91fbb9aa85e1ab` | `e770cfbb96b528c4db258d7d03fe6533` | `33bb90ffd13c87b4c8e45d920313752a` |
| Earth | `651600a51edd20141adb67696986c582` | `812c9a0348e004242ba4e46efa91e38e` | `d3d9ab560534bd948b10ac00abbff083` | `3b86a449e7264174eaccef9b8f02fe20` | `cda7013db24f4c547b79bfc5c617066b` | `6b4cb9b6116f2194192e1e7e379c48d7` |
| Fire | `46cede83b1f34ad4fa46b8776e352b02` | `a0ab0c31b1a92554291a82e598f39ba4` | `ba5026596b06b204eb2efed2b411c5b9` | `640fb7efb7c916945837bbcab995267e` | `b0b4091bdaebb464e903857a95189dea` | `ea0f0bbc6e5e471428d535501b21eb26` |
| Water | `56372b0a2749c224392a5ee74105c534` | `62a3e860e6e72e6499c38bb8b2fe303e` | `680b5b61c80af664daec46af7644486c` | `877c154a296ee8e45be1a00668319923` | `fcc939e3acf355b458ddf9617d8c6c28` | `3bd31a0b4d800f04a8c5b7b1a6d7061e` |

The exact Wolf summon identity is
`76597216769b0d540aafafa07edf0cec` (`WolfSummon`). These identities are donor
inputs, never KMG output identities.

## Canonical child action graph

Guarded run `20260811T1747085341434Z-observe-expanded-summoning-inventory`
on source `ba6500f16b06cf8adb9c5d32149929137e1d98e2` inspected all 18 canonical
parents and 48 final-live direct children.

- Native children use `ContextActionSpawnMonster` with shared pool
  `d94c93e7240f10e41ae41db4c83d1cbe`.
- Duration is extendable caster-rank rounds. Spawn cleanup/faction buffs are
  tier-banded separately for SM and SNA.
- One-creature actions use count 1. Quantity actions use the correct dice plus
  `ProjectilesCount`, preserving Superior Summoning through the child ability's
  rank component.
- Native aligned-outsider choices use `Conditional` plus
  `ContextConditionAlignment` on the caster. This is a safe structural model
  for explicit alignment branches, but the observed base-family natural
  creatures do not receive celestial/fiendish templates.
- CotW supplies template buff candidates, but KMG cannot depend on optional
  assemblies. Standalone template mechanics therefore require KMG-owned,
  frozen identities and native component construction; optional equivalents
  may be reused only after exact structural validation.

Publication abilities are not shared across parent tiers: distinct identities
preserve parent context, UI text, spell level, metamagic, and save stability.
Each templated SM logical placement is represented by one published choice and
two gated execution identities so neutral casters can choose while good/evil
casters remain restricted. This produces 182 templated logical placements and
364 subordinate execution identities.

## Complete frozen donor graph audit

Guarded fresh-process run
`20260811T2055502086857Z-observe-expanded-summoning-inventory` inspected the
exact donor set derived from `ExpandedSummoningDonorCatalog` on committed
source `9e1d851e75cf413f5d0a576484a9f5a8538b2a2b`. The final-live library
contained all 54 distinct selected GUIDs (`54;missing=0`). The run completed
PASS in 103,225 ms with zero warnings, no exception, and no save access.

For every selected donor the retained evidence contains core `BlueprintUnit`
fields plus bounded component, body, and view graphs. The inventory confirms
that visual suitability and summon safety are separate concerns:

- dedicated native summons still commonly carry `Experience`, and some carry
  `AddLoot`; KMG must continue cloning and sanitizing them;
- campaign donors expose `AddLoot`, `AddTags`, `MobCaster`, or comparable
  campaign surfaces and remain visual/body/mechanic references only;
- the 24 elemental donors and four mephit donors are exact dedicated summon
  units and are the first native-mechanic reuse candidates;
- proxy donors such as Wolf, Giant Eagle/Roc, Worg, Leopard, Monitor Lizard,
  Hodag, Mastodon, Nymph, Doomspider, and Nixie cannot establish the intended
  creature's stat block merely by being cloned;
- `AzataGhaelSummoned` is appropriate evidence for Ghaele, but is only a
  visual donor for Lantern Archon; `SummonedAirElementalHuge` is only a visual
  donor for Invisible Stalker.

The same run re-proved the immutable construction contract: registry 1,374;
67 KMG units; 1,045 KMG abilities; 681 live parent placements; all alignment,
template, smite, clone-isolation, prohibited-reference, inherited-spell, and
starting-inventory assertions PASS. The 8 MB raw graph remains machine-local;
this compact interpretation and the exact run ID are the checked-in evidence.

## Lantern Archon exact dependency inventory

Guarded run
`20260811T2102583998517Z-observe-expanded-summoning-inventory` passed on
`8c576bc2ff741f726493ce347de16f02c7ee02de` and found 75 focused
Will-o'-Wisp, archon, light-ray, and aura candidates.

- visual donor: `24719a49b84c5cd43b894268d22d9c89`
  (`CR6_WillOWispStandart`), Small, prefab asset
  `8a8d7c448ff2c8749adc08eeb223333b`; mechanics are discarded;
- ray graph donor: `33e8997912cf76b4c99dca0445082804`
  (`AzataGhaelLightRay`), the only inspected native two-projectile
  ranged-touch ray; its long range and 2d12 damage are replaced;
- ray AI donor: `dcfc5e9aec5bea540b36caf754989164`
  (`AzataGhaelLightRayAiAction`); only safe targeting considerations are
  copied, with the ability reference replaced by the KMG ray;
- optional Aura of Menace carrier: `1ce4878b5e714f659d0854a12f4b3cf2`
  (present in the installed Call of the Wild reference, absent in standalone
  Kingmaker 2.1.7b; exact-type reuse is optional and standalone omits the aura)
- Call of the Wild-only Irresistible Dance delivery/state:
  `fad6a06a3cb04fabaedf4d358c61880d` /
  `4d283e0b70fb489ba79e69387818c3f3`; neither is a registration dependency.
  Pixie uses a stable KMG touch-range ability plus bounded `CantAct`, -4 AC,
  and -10 Reflex state so standalone and optional-mod profiles share identical
  mechanics.
  (`ArchonSubdomainAreaArchonsAuraEffectBuff`); adding the buff as a unit fact
  activates the native area-effect lifecycle without a toggle resource;
- outsider class: `92ab5f2fe00631b44810deffcc1a97fd`.

The Will-o'-Wisp donor's 9 aberration HD, touch attack, invisibility, spell
immunity, tags, ambush behavior, alignment, and brain are not retained. The
Ghaele donor's 13 HD, weapons, spellcasting, chain lightning, gaze, holy aura,
inventory, and azata facts are not retained.

Selecting the distinct Will-o'-Wisp visual increases the current frozen donor
set from the historical 54-GUID audit to 55 GUIDs; Ghaele remains independently
selected for the actual Ghaele Azata roster entry. The next structural run must
therefore observe `55;missing=0`.

## Salamander, Invisible Stalker, Shadow Demon, and Succubus dependencies

Guarded runs `20260811T2150184037199Z-observe-expanded-summoning-inventory`
and `20260811T2156134219365Z-observe-expanded-summoning-inventory` passed on
committed source and inspected 12 exact candidates in the final-live library.

- `94b2838e8a492c44ebf89e7fe7a75a62` (`NaturalInvisibilityBuff`) has
  `NotDispellAfterOffensiveAction=True`; it is the exact native primitive for
  Invisible Stalker's attack-safe natural invisibility.
- `c4a7f98d743bc784c9d4cf2105852c39` (`Incorporeal`) supplies native
  incorporeal damage handling, critical/precision immunity, airborne, and trip
  immunity for Shadow Demon.
- `84f41b83ef6b8c242a15381045822f94` and
  `ab966bf06859119419989ccb0061ba39` prove that stock energy drain is a
  one-day `SaveOrBecamePermanent` effect. It cannot be granted to a temporary
  summon unchanged; a derived temporary round-bounded action is required.
- `04dcf5776f9d4315b27d1c0c7c2f3c46` (`TailSlapDrakeFeature`) adds exact
  `b21cd5b03fbb0f542815580e66f85915` (`Tail1d6`).
- `efc1e80fb41e06544be46604983806d6` (`ShamblingMoundGrabFeature`) is a
  hit-confirmed grapple graph with explicit caster and target grapple buffs and
  a separate constrict damage action. It is a reconstruction reference, not a
  fact to reuse unchanged.
- `d7cbd2004ce66a042aeab2e95a3c5c61` (`DominatePerson`) is a Will-save,
  round-ranked native domination graph referring to
  `c0f4e1c24c9cd334ca988ed1bd9d201f`; all spell-list and optional-mod additions
  are excluded from the KMG derivative.
- `cce5bb72adc78f944b480e01efd3eaef` (`SoulsCloakVampiricTouch`) proves a
  native touch-delivery damage plus caster-only temporary-hit-point pattern.

Both runs preserved registry 1,378, 67 KMG units, 1,046 KMG abilities, all 681
placements, 55 exact donors, and zero forbidden references, donor aliases,
inherited spells, inventory, or native action contamination. No save was read
or written.
