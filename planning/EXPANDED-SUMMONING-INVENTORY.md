# Expanded Summoning final-live inventory

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

Donor-unit and sanitizer rows remain in progress; broad roster-term matching
found 533 candidates and is evidence for narrowing, not selection authority.

## Dedicated summon donors confirmed in composed final-live graph

These are donor candidates, not yet approved clones. Each still requires the
full component/fact/body/view sanitizer audit and source-nonmutation proof.

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
