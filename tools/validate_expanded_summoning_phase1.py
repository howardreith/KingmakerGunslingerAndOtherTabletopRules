#!/usr/bin/env python3
"""Expanded Summoning Phase 1 (charter Sprints 3-8) static gates.

Phase 1 appends creature identities to the frozen ledger without changing the
release version: Sprint 3 added Pony, Horse, Owlbear and Cyclops, the
Nature's Ally Frost Giant wrappers and the Cyclops Flash of Insight specials;
Sprint 4 added Shambling Mound, Giant Flytrap and Purple Worm and the shared
summon grapple lifecycle specials; Sprint 5 added the Dust, Ice, Magma, Ooze,
Salt and Steam Mephits with their breath, spell-like ability, resource, cast
action, brain and traits specials; Sprint 6 added the Monitor Lizard, Grizzly
Bear and Dire Bear grab carriers and the Giant Spider's web pack; Sprint 7
added the four big-cat grab-and-rake carriers; Sprint 8 added the Tiger with
its placements, the project 1d8 claw, its carrier and the Cheetah's sprint
pack. This module pins that append exactly -
symbol, GUID and planned type, in ledger order, directly after the Better
Vendors progression block - and the current roster figures the catalogs,
icons and package carry. It claims no runtime or visual acceptance; those
live in the Phase 1 state file.
"""
from __future__ import annotations
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import expanded_summoning_manifest

PRESERVED_ENTRIES = 1956  # 1913 preserved + 43 Better Vendors progression
STATIC_KEY = "expandedSummoningPhase1"

# Exact ordered (symbol, guid, plannedType) triples appended by Sprint 3, then
# Sprint 4, then Sprint 5, then Sprint 6, then Sprint 7, then Sprint 8, in
# ledger order.
APPENDED = (
    ("KMG.Summoning.Unit.Pony", "67a661d7b39e44bc811ad9ff59120110", "BlueprintUnit"),
    ("KMG.Summoning.Unit.Horse", "629fa53bf40d4b4b9828f57cf81194e9", "BlueprintUnit"),
    ("KMG.Summoning.Unit.Owlbear", "7bde6f94222c47f687ed68c752cb24cb", "BlueprintUnit"),
    ("KMG.Summoning.Unit.Cyclops", "a85dfadbca3340f4aad99c1cc36e40b3", "BlueprintUnit"),
    ("KMG.Summoning.Ability.SM.Tier1.Pony.One", "317c59d12326455fb35402b28719dfd3", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier1.Pony.One.Celestial", "e9e45cdcfe774f83805408720e1bdfae", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier1.Pony.One.Fiendish", "7fd5d7464e0045829cf74534f210b59a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier2.Pony.OneD3", "a01faa703d374dd99f7e4cfa88e56527", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier2.Pony.OneD3.Celestial", "d549ae5535ba49c4b62fde0ee343a1dd", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier2.Pony.OneD3.Fiendish", "ac438525685b4b41b1f2ae3b1ea5248d", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier2.Horse.One", "22dd279f838443cfb9d65d3868c45549", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier2.Horse.One.Celestial", "9f552144bd5e4202928c68fbc086f130", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier2.Horse.One.Fiendish", "feb5352357ff47f69b9b0e73b390c6f6", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier3.Pony.OneD4PlusOne", "f438cc45db55419b80be5b819365edda", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier3.Pony.OneD4PlusOne.Celestial", "22ad6ac3f4ef4660a9d6f255b10d7f58", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier3.Pony.OneD4PlusOne.Fiendish", "084ad05505a64d9fae3a6b62955ae86c", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier3.Horse.OneD3", "aabf5cbb375e485799686c3133c21e95", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier3.Horse.OneD3.Celestial", "f417bc58f0fb41dc9a2c2f137d8ea32b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier3.Horse.OneD3.Fiendish", "aab2cdf72eea4ad19319fd58dea59592", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.Pony.OneD4PlusOne", "a6220be2482c490d9750624002859df4", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.Pony.OneD4PlusOne.Celestial", "c3468b2f8bb94d1f9121333b8318e06d", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.Pony.OneD4PlusOne.Fiendish", "3d4d5459966940e4a24a51a28b71efbc", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.Horse.OneD4PlusOne", "dd75f2771e0f4b74b8f4cbfcf32967c7", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.Horse.OneD4PlusOne.Celestial", "62dc549ff82b4f71ae84d008707568a7", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.Horse.OneD4PlusOne.Fiendish", "52efd4d9de7e4d319f4cad9c4accda6c", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.Pony.OneD4PlusOne", "853b19daeb9b4a54bbc193a8d47a36f5", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.Pony.OneD4PlusOne.Celestial", "cccabd42e640418eb6f5dd391b3b00b1", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.Pony.OneD4PlusOne.Fiendish", "912d8c9692ce445da76978d4792d4604", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.Horse.OneD4PlusOne", "2e660045bffa49d0af7ae9f5751b8c3e", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.Horse.OneD4PlusOne.Celestial", "889bf1ab72484f6fa2b9564d970fef12", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.Horse.OneD4PlusOne.Fiendish", "fcc1fcf3fdcc45bb967f19c5ee53b182", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.Pony.OneD4PlusOne", "1513bfc815cd4532b2daecde0cde3efa", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.Pony.OneD4PlusOne.Celestial", "e2b49cd8fae248849473175f60c076b6", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.Pony.OneD4PlusOne.Fiendish", "3fab9cb9a7034749bf2e1ed505e0fa08", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.Horse.OneD4PlusOne", "adf92f970fb646f4809b742bcc972dbc", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.Horse.OneD4PlusOne.Celestial", "dc0e9e800f3d42b3b456ec9dd2cf2c99", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.Horse.OneD4PlusOne.Fiendish", "ad6db42c83c14429b229e02de7cd12d7", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.Pony.OneD4PlusOne", "dc22becb8287449f8e91b3435b27c3dd", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.Pony.OneD4PlusOne.Celestial", "8cfe3437543a466f8e92ef5693cbcb0f", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.Pony.OneD4PlusOne.Fiendish", "79da62c69610434d90aa8692fe7f905b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.Horse.OneD4PlusOne", "e1528209f4ce4c8c9a5ddf61c46b029b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.Horse.OneD4PlusOne.Celestial", "56b4ad7f8d6a44eab16d00fcf685e138", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.Horse.OneD4PlusOne.Fiendish", "b9eab8995263454d84c9332b5755f611", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.Pony.OneD4PlusOne", "acf12709e641494492c1167661afc62b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.Pony.OneD4PlusOne.Celestial", "fd43220257414477bf51e331b1dd8644", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.Pony.OneD4PlusOne.Fiendish", "c6eb24374cc74e25bdc82515eb7b709b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.Horse.OneD4PlusOne", "2c8d8fb226e745f8960e83846fb36e20", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.Horse.OneD4PlusOne.Celestial", "48913b62388a489ea6d22f1e857e679a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.Horse.OneD4PlusOne.Fiendish", "ef27bf81cd6d4706b0ed4ddcaffd7a3b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.Pony.OneD4PlusOne", "3c65c13606a545f29bf4097eb4000154", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.Pony.OneD4PlusOne.Celestial", "05b0f4728ce645f49f957d16df4345f6", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.Pony.OneD4PlusOne.Fiendish", "203af45d7b0144f3a143326a7fdea4b9", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.Horse.OneD4PlusOne", "3aa36d84ec454de388f5a5efd43bc5f1", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.Horse.OneD4PlusOne.Celestial", "c36cc0cae4114c22ac99c3a223acc6fb", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.Horse.OneD4PlusOne.Fiendish", "e26c41e9fcaa42c8b75e8df1cab0c054", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier1.Pony.One", "320582b60f0a4d13a6684cf6c84aef74", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier2.Pony.OneD3", "b747202d130f48cf83a791fa3b6bfa6e", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier2.Horse.One", "1cd82938c8b44a37b918a2a21fd99d49", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier3.Pony.OneD4PlusOne", "1f88acc6c08542bdb0de96a7d90688a0", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier3.Horse.OneD3", "73725890b5104930a71d09f099712dd5", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.Pony.OneD4PlusOne", "3bf254266e2642fc941b7ffe39f4f531", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.Horse.OneD4PlusOne", "e8e57a5023f44e9ca3b532f78f4e124f", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.Owlbear.One", "d8df6662e6074ce4924f514225168596", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.Pony.OneD4PlusOne", "9f7ec7d1bc5c465a90bfae8a554af621", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.Horse.OneD4PlusOne", "d4b5c882c9c348b791db3a336b223a26", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.Owlbear.OneD3", "e5b2aba45afd45f886db2376f5df478b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.Cyclops.One", "73913587f7a34fb7983075e5181a38bf", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.Pony.OneD4PlusOne", "877519a63d744827a2cc4904ec0f0457", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.Horse.OneD4PlusOne", "0b66bf1ba23e479b9dedfefdfb4680f4", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.Owlbear.OneD4PlusOne", "485c218c31d748248fb128381576597a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.Cyclops.OneD3", "503bf184a5d24d41b7c560a0120d8ecb", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.Pony.OneD4PlusOne", "bdb87ce8d66b4df2bcdf806ee205ea70", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.Horse.OneD4PlusOne", "e52fcbb87b664ee097b8a0dc0ce15e60", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.Owlbear.OneD4PlusOne", "4ace3dff5a3b4148bba40252ad227e6a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.Cyclops.OneD4PlusOne", "2f5a8860a880460b86eadfc4b328a34b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.Pony.OneD4PlusOne", "a321f1d48844482da14997dc938de55c", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.Horse.OneD4PlusOne", "1f045f2934784e209d70cb3e87a24cae", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.Owlbear.OneD4PlusOne", "720468f04dc84608990821ba5ef6367a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.Cyclops.OneD4PlusOne", "de12343b979f4767bd3b241eca9021c9", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.Pony.OneD4PlusOne", "19b194447b3c410bb2cd4d3d1c3fac63", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.Horse.OneD4PlusOne", "56330eaf5d9540ba90676918cdd1a28b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.Owlbear.OneD4PlusOne", "d40fa9c2af7f429aa96424b94227f066", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.Cyclops.OneD4PlusOne", "a5f8960539004f11977ca552f1c81b00", "BlueprintAbility"),
    ("KMG.Summoning.NativeOption.SNA.Tier7.FrostGiant.One", "184cfe9d6203442ebe4af4ac717fdf6d", "BlueprintAbility"),
    ("KMG.Summoning.NativeOption.SNA.Tier8.FrostGiant.OneD3", "3afff2c5ea7943569e3d30457ccc6f3a", "BlueprintAbility"),
    ("KMG.Summoning.NativeOption.SNA.Tier9.FrostGiant.OneD4PlusOne", "52c367cec4b342bd86356769193e8f87", "BlueprintAbility"),
    ("KMG.Summoning.Special.Cyclops.FlashOfInsight", "f88d2d8f5a154c4094fec3b01ac1109c", "BlueprintAbility"),
    ("KMG.Summoning.Special.Cyclops.FlashOfInsightState", "1c5ddb4bc9ae4929b61682fca8e7ba6e", "BlueprintBuff"),
    ("KMG.Summoning.Special.Cyclops.FlashOfInsightResource", "a07046042da74c388c312d7ef5d4bb4a", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.Cyclops.CombatTraits", "559b9a9a3c53423b87c7281ecc88fbc0", "BlueprintBuff"),
    ("KMG.Summoning.Special.Cyclops.FlashOfInsightAi", "591b2ddf91244a4da1dbb5586550e1a6", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.Cyclops.Brain", "22dc52d0dfba4c3d8b9c6f49ea6037de", "BlueprintBrain"),
    ("KMG.Summoning.Unit.ShamblingMound", "c0680a67c42f4d17b3b55c011a8f9c56", "BlueprintUnit"),
    ("KMG.Summoning.Unit.GiantFlytrap", "287490e71bed46bd92adef60489ecb4c", "BlueprintUnit"),
    ("KMG.Summoning.Unit.PurpleWorm", "8739fe771e5b4f3fa3acbb10dd969892", "BlueprintUnit"),
    ("KMG.Summoning.Ability.SNA.Tier6.ShamblingMound.One", "d9df6491b93b4b97a15014bbc43dcf36", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.ShamblingMound.OneD3", "cab9fbcc770a4ce39020ecaf544937e4", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.GiantFlytrap.One", "085adbcd4ba540f7b73802bb114c6bbf", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.ShamblingMound.OneD4PlusOne", "42c2f703eeb74d84be9614e6cffcd01c", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.GiantFlytrap.OneD3", "2172cc7b9b0e485fa0fa1a0d8d794c93", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.PurpleWorm.One", "ac4f08e655f34b86865d19b4f86cb45f", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.ShamblingMound.OneD4PlusOne", "121d2ae2dc42456bbb789e7ae6095ff8", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.GiantFlytrap.OneD4PlusOne", "2d15592c70ed4969bbaa5df5e13d082f", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.PurpleWorm.OneD3", "4a8be5f2b72f4cc88b781cfdf51b1e18", "BlueprintAbility"),
    ("KMG.Summoning.Special.Grapple.Hold", "5af4e99c8e5744a6b978412b126d25f0", "BlueprintBuff"),
    ("KMG.Summoning.Special.Grapple.Grappled", "e49fc099e63b4392bb262f1fefa3301b", "BlueprintBuff"),
    ("KMG.Summoning.Special.Owlbear.CombatTraits", "1ab3a4951af443409b9eaa190d55f69d", "BlueprintBuff"),
    ("KMG.Summoning.Special.ShamblingMound.CombatTraits", "4ec446a8591a4f169e7c548c089bf63a", "BlueprintBuff"),
    ("KMG.Summoning.Special.GiantFlytrap.CombatTraits", "ea204a20924045ca856de5af350a7194", "BlueprintBuff"),
    ("KMG.Summoning.Special.PurpleWorm.CombatTraits", "ce031441c4fa4f41a142799bbbef3f36", "BlueprintBuff"),
    ("KMG.Summoning.Special.PurpleWorm.Swallowed", "8a25b84199cd42319fa77b0b1d1d5bea", "BlueprintBuff"),
    ("KMG.Summoning.Unit.DustMephit", "30f6615badf14d21b41d6468d5c93ac3", "BlueprintUnit"),
    ("KMG.Summoning.Unit.IceMephit", "b37cdd78c8cc43be8799e9ffd6953ba2", "BlueprintUnit"),
    ("KMG.Summoning.Unit.MagmaMephit", "ce9551f1413d4fdcb59941734d4546b1", "BlueprintUnit"),
    ("KMG.Summoning.Unit.OozeMephit", "7cbf19a6c83f4b8e849d17b8a54e6d51", "BlueprintUnit"),
    ("KMG.Summoning.Unit.SaltMephit", "8c8fe6eb536b450a9a2c10b8cbef9fbe", "BlueprintUnit"),
    ("KMG.Summoning.Unit.SteamMephit", "047f68ee53554d8ab1702de189ecd85d", "BlueprintUnit"),
    ("KMG.Summoning.Ability.SM.Tier4.DustMephit.One", "eff27d99ab3b476cbbd1d23e933a6391", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.IceMephit.One", "f7fb199319e0401ebe4a6d78f21b66d3", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.MagmaMephit.One", "7dcc9bc37ddd4b86af4fa448ab687376", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.OozeMephit.One", "24e332e41d8f41488096c18c54dcab65", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.SaltMephit.One", "371461a919fe43e79f3f9af1f9353069", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier4.SteamMephit.One", "dbd3825fc6684c9494ef99066acb714b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.DustMephit.OneD3", "2119e7c0505e427f82e4fc83313ca0c2", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.IceMephit.OneD3", "7feeac5c68a14fd2be0fabb5ed0beb6a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.MagmaMephit.OneD3", "428999d099e9440a9b1ae91d8638a720", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.OozeMephit.OneD3", "7eb49ca4d3b248caa57254cc36e19636", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.SaltMephit.OneD3", "3a5765e8bc304ca899d2f75d7347d896", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier5.SteamMephit.OneD3", "ed98e96706a94091a3c20be4919e1215", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.DustMephit.OneD4PlusOne", "aa55b6e8af2d417f827331ac5158eaff", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.IceMephit.OneD4PlusOne", "35ccb17c84614dbc803a3f01c1d8ee0a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.MagmaMephit.OneD4PlusOne", "10f71ecb1ac745a8b281c4b824d494b1", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.OozeMephit.OneD4PlusOne", "238ed4eb12204a5a88b5860c75131363", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.SaltMephit.OneD4PlusOne", "afd91ab9c4824894b39f38b6ba78147b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier6.SteamMephit.OneD4PlusOne", "8a9c2eea91724dbcb2d501306a4fe3b7", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.DustMephit.OneD4PlusOne", "353e8c264ae842338e6d78c58fbccd90", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.IceMephit.OneD4PlusOne", "b323c7e8f61648888903185e8229b043", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.MagmaMephit.OneD4PlusOne", "35a596b1345a444da4c5970cd93ea735", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.OozeMephit.OneD4PlusOne", "d9f924cd282c486d95a83cf149635b39", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.SaltMephit.OneD4PlusOne", "48123b1e26784cb18d4dae1edcc91c2e", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier7.SteamMephit.OneD4PlusOne", "515c1836e8dc419eb934642c2e2e32c8", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.DustMephit.OneD4PlusOne", "e23c829fd8cf426fb9c0c4500d8130d5", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.IceMephit.OneD4PlusOne", "7e847258944749b9b89884b91619ca8c", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.MagmaMephit.OneD4PlusOne", "0ea2d7c472a24fdcbf907bbfb38296b4", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.OozeMephit.OneD4PlusOne", "1b78444899be4226913c494d57c4af76", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.SaltMephit.OneD4PlusOne", "5b5eae63ef0740869438826c21741ab3", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier8.SteamMephit.OneD4PlusOne", "7954908b932c4a0c803d7a000d47861d", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.DustMephit.OneD4PlusOne", "ee0bd5e3a1854a75ac465e57c5cd8f09", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.IceMephit.OneD4PlusOne", "bc7409315f4f462dbef07534663c06c6", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.MagmaMephit.OneD4PlusOne", "f1eab7a23cd64ae39b6a05ee576f82d0", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.OozeMephit.OneD4PlusOne", "8a6a699f872d4442acef6f9b28811cbb", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.SaltMephit.OneD4PlusOne", "c461ccd8ff6a4afc994b0f71758bd4ca", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SM.Tier9.SteamMephit.OneD4PlusOne", "3f41f861e58f4c5b8738bbe10696cae2", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.DustMephit.One", "0124979771724bd8936584b8cc32f7a8", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.IceMephit.One", "09d795dca9c44933a1db09ef40a21e17", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.MagmaMephit.One", "aea3f06b4bcb423a91bd68916d3f6460", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.OozeMephit.One", "99c870fa6cf9489e845c76cbe902dbb1", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.SaltMephit.One", "acf1b770048a4e22a27ac4b2fd716373", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier4.SteamMephit.One", "b5e5a5d9f2f94ccd9c0d8afea6bb43ff", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.DustMephit.OneD3", "5a1ac2544fa44f55b9094832e4de4f97", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.IceMephit.OneD3", "f81a0239a7bb441b84478e56210b6562", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.MagmaMephit.OneD3", "418f04c5b8a74c969283bcbea5fc5691", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.OozeMephit.OneD3", "2ebc599086784fb7926cb0688399b48b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.SaltMephit.OneD3", "eff8f3258b72427da6542b4caaf0a8df", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.SteamMephit.OneD3", "4d02ca7da65741d58732096f115d83a6", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.DustMephit.OneD4PlusOne", "dd061d12989b40908b85c881599592d7", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.IceMephit.OneD4PlusOne", "885de0ddb41e4ba8988c10fdde41f906", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.MagmaMephit.OneD4PlusOne", "3a5e545dda0f4515b5eeb41d813d8bab", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.OozeMephit.OneD4PlusOne", "53a0cfec253a4320963f3673178cc32a", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.SaltMephit.OneD4PlusOne", "9a3a0580c29742809c11447771ad2fb5", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.SteamMephit.OneD4PlusOne", "4c42d019e4af4040938114dd92eac207", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.DustMephit.OneD4PlusOne", "28edfd81307d4f36b36c9e4f5ac4f983", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.IceMephit.OneD4PlusOne", "2d447b0867ca41e98c4e4adb4c4c47d6", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.MagmaMephit.OneD4PlusOne", "efac552327fe4f088edbc29afad06b24", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.OozeMephit.OneD4PlusOne", "22260fe9e5cd4a2aa446403bc39ea33e", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.SaltMephit.OneD4PlusOne", "ce79472133de4c4282e5da261bb90c00", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.SteamMephit.OneD4PlusOne", "07b8c7f4937747348c5fe5accb498258", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.DustMephit.OneD4PlusOne", "15b787300ea34934bb7d05e56bc2eb47", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.IceMephit.OneD4PlusOne", "4f6ba3f16d65430b91c241cc05c03df2", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.MagmaMephit.OneD4PlusOne", "3908d0139371499f8d2da327f87b25bc", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.OozeMephit.OneD4PlusOne", "77906868a4764b248ad4373a28374db1", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.SaltMephit.OneD4PlusOne", "724fff9f684e41f9962cae6dc3df70f9", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.SteamMephit.OneD4PlusOne", "3ae72841e5f34705a5f8d0849c511f2e", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.DustMephit.OneD4PlusOne", "1d8b6f45ce644f52bcf0605d10fec3cc", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.IceMephit.OneD4PlusOne", "fdc107675cf34950961ac85c6691a7b2", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.MagmaMephit.OneD4PlusOne", "868445f8949b4093b5d310927375fe8f", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.OozeMephit.OneD4PlusOne", "b7775e03255849bdbeba34c35717833d", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.SaltMephit.OneD4PlusOne", "3b52c9eb1ae7483cbe16e55fa8fc668b", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.SteamMephit.OneD4PlusOne", "116328ca642d4e0997eac89a96d79ebe", "BlueprintAbility"),
    ("KMG.Summoning.Special.DustMephit.Breath", "9bf56be04a0c452fa628136a7a454b22", "BlueprintAbility"),
    ("KMG.Summoning.Special.DustMephit.BreathAi", "319e9603ece140a78dff1d8483cd6685", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.DustMephit.Brain", "176f06382dd347ba920c6ff482a0db5b", "BlueprintBrain"),
    ("KMG.Summoning.Special.DustMephit.CombatTraits", "1692bbe558154adf8dedbc2e94cf8e7f", "BlueprintBuff"),
    ("KMG.Summoning.Special.DustMephit.SpellLikeOne", "1b2d864de558462a86045a5c667ce296", "BlueprintAbility"),
    ("KMG.Summoning.Special.DustMephit.SpellLikeOneResource", "3e8b23efde2e46bb96b405e45c6553ad", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.DustMephit.SpellLikeOneAi", "5d7472e16a72429f928da3caabc2349f", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.IceMephit.Breath", "fe4016ec0ea04b0da8bc2a08c984818c", "BlueprintAbility"),
    ("KMG.Summoning.Special.IceMephit.BreathAi", "f6b453d55cb54806874f624da2aea1f2", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.IceMephit.Brain", "cf16af70f80f4f968fd5969356b19c0a", "BlueprintBrain"),
    ("KMG.Summoning.Special.IceMephit.CombatTraits", "be63637c7ad646d7b35b52c2929bab5d", "BlueprintBuff"),
    ("KMG.Summoning.Special.IceMephit.SpellLikeOne", "3694bb231deb4603bfe97601e308fe05", "BlueprintAbility"),
    ("KMG.Summoning.Special.IceMephit.SpellLikeOneResource", "e464bb6d092143449c407564935e5bfe", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.IceMephit.SpellLikeOneAi", "2d69b4a35093498784ebdaaeaaa8c0cf", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.MagmaMephit.Breath", "512be3e38f524cc4ba045e4b3167c54c", "BlueprintAbility"),
    ("KMG.Summoning.Special.MagmaMephit.BreathAi", "3708db4d4bfc4541b46618dae76212dd", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.MagmaMephit.Brain", "8963c494751644a2b07d8f2143dbcea0", "BlueprintBrain"),
    ("KMG.Summoning.Special.MagmaMephit.CombatTraits", "cfb1de7a10b041888b8f9b8779e715d2", "BlueprintBuff"),
    ("KMG.Summoning.Special.OozeMephit.Breath", "3d5cd0f7990646eaa6477247b60412dc", "BlueprintAbility"),
    ("KMG.Summoning.Special.OozeMephit.BreathAi", "7da203b6e1a941c2a788a438cb78fdaa", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.OozeMephit.Brain", "1d17e755989b4075870279a9be161ab6", "BlueprintBrain"),
    ("KMG.Summoning.Special.OozeMephit.CombatTraits", "c373f71f640543b094c4a77f8d2a92f4", "BlueprintBuff"),
    ("KMG.Summoning.Special.OozeMephit.SpellLikeOne", "2c51d416aade4648a3b6a91b772aa66c", "BlueprintAbility"),
    ("KMG.Summoning.Special.OozeMephit.SpellLikeOneResource", "e0b376a3992e4a6bac65a4c038afc8ce", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.OozeMephit.SpellLikeOneAi", "17bd5aee21d549b68626b412a9efca8a", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.OozeMephit.SpellLikeTwo", "2b78134358af447f8a874f13b6a25ef6", "BlueprintAbility"),
    ("KMG.Summoning.Special.OozeMephit.SpellLikeTwoResource", "7572abb41b194e67af27c65250a36821", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.OozeMephit.SpellLikeTwoAi", "d8c2e71c889f483bb08cabb670bd2f92", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.SaltMephit.Breath", "0082eea8755740dc8a4de5308d9faff8", "BlueprintAbility"),
    ("KMG.Summoning.Special.SaltMephit.BreathAi", "fc117113465e442fa66edb21bbcb4d84", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.SaltMephit.Brain", "04d2ec99fa334b16a2b116cf0843775e", "BlueprintBrain"),
    ("KMG.Summoning.Special.SaltMephit.CombatTraits", "93880e0d35fb4ea7b5969f7c0a3a7a3d", "BlueprintBuff"),
    ("KMG.Summoning.Special.SaltMephit.SpellLikeOne", "1e83c9cd30764c08b44b3e6e4dfb37fc", "BlueprintAbility"),
    ("KMG.Summoning.Special.SaltMephit.SpellLikeOneResource", "5827a77fd35f4772ba053f7544364b2a", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.SaltMephit.SpellLikeOneAi", "129d7a6fb21b4a71af7de9e30803f68e", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.SaltMephit.SpellLikeTwo", "6db971113edf47e4ae4404ac71ba819e", "BlueprintAbility"),
    ("KMG.Summoning.Special.SaltMephit.SpellLikeTwoResource", "9b4ce3e9fe1c4606a83d5c8864b4aae4", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.SaltMephit.SpellLikeTwoAi", "397c49078c3945858ca355775e72b2c5", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.SteamMephit.Breath", "146c6326b7c7430c9bf33cad288fceef", "BlueprintAbility"),
    ("KMG.Summoning.Special.SteamMephit.BreathAi", "014bb50badae457998e17b708a3c1bfa", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.SteamMephit.Brain", "b72c6534a9234bb5b1a6ae63632b8ace", "BlueprintBrain"),
    ("KMG.Summoning.Special.SteamMephit.CombatTraits", "0ea021251fa244228ca3ec54b36c46e8", "BlueprintBuff"),
    ("KMG.Summoning.Special.SteamMephit.SpellLikeOne", "b4a135660e8e4023b58845b825f44228", "BlueprintAbility"),
    ("KMG.Summoning.Special.SteamMephit.SpellLikeOneResource", "5c8f1dc087fc4e028095e6f21b074bc2", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.SteamMephit.SpellLikeOneAi", "aaa64536cc8345d78cc5109c055e1ae2", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.SteamMephit.SpellLikeTwo", "ab241b72756a49cab443ef5b1acd23de", "BlueprintAbility"),
    ("KMG.Summoning.Special.SteamMephit.SpellLikeTwoResource", "d8bdd93b25bb45f9b5775ed90df5b61c", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.SteamMephit.SpellLikeTwoAi", "3ab58c5239174273892727aa25c6f5cb", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.MonitorLizard.CombatTraits", "45f7552dddaf4322b44a1421cef3a3f2", "BlueprintBuff"),
    ("KMG.Summoning.Special.GrizzlyBear.CombatTraits", "75a7c289e4c643bb93c2155a9f079c34", "BlueprintBuff"),
    ("KMG.Summoning.Special.DireBear.CombatTraits", "74e7cfabb0e14a07b9c0ec9862e8009d", "BlueprintBuff"),
    ("KMG.Summoning.Special.GiantSpider.Web", "ea2845894cb94af19994768602294900", "BlueprintAbility"),
    ("KMG.Summoning.Special.GiantSpider.WebResource", "2f841b1ca74a42d487ad43e1c26f7210", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.GiantSpider.WebAi", "67a101d251444047a6f3c15323bff174", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.GiantSpider.Brain", "1b8106c390e64f048043998da1c13875", "BlueprintBrain"),
    ("KMG.Summoning.Special.GiantSpider.CombatTraits", "c5637ec9346b414087fda9334ec50bf5", "BlueprintBuff"),
    ("KMG.Summoning.Special.Leopard.CombatTraits", "da89f9ecc18849edbbad0ee7772c8cd0", "BlueprintBuff"),
    ("KMG.Summoning.Special.Lion.CombatTraits", "e7d7d41e2c764fa5a56d33ca4556c65c", "BlueprintBuff"),
    ("KMG.Summoning.Special.DireLion.CombatTraits", "0641d94c18b740f4b7772e8d3205ba96", "BlueprintBuff"),
    ("KMG.Summoning.Special.DireTiger.CombatTraits", "4cbd552217734bbfbc4be0c2c8b97849", "BlueprintBuff"),
    ("KMG.Summoning.Unit.Tiger", "5182125341e34ae98491eaf991bc0ef3", "BlueprintUnit"),
    ("KMG.Summoning.Ability.SNA.Tier4.Tiger.One", "11803dae3bbd4aa2b724e198723cf823", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier5.Tiger.OneD3", "78f0199c387545bdb4882ca470758fcb", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier6.Tiger.OneD4PlusOne", "9744d59cb03d431abaaebaaddf01de75", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier7.Tiger.OneD4PlusOne", "a73109affc4b41a6bb763bf0f71be396", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier8.Tiger.OneD4PlusOne", "c01fd035f03f402d81402b8b1f26307f", "BlueprintAbility"),
    ("KMG.Summoning.Ability.SNA.Tier9.Tiger.OneD4PlusOne", "99ac368e5a6c43b9bdb48327984acebe", "BlueprintAbility"),
    ("KMG.Summoning.Special.Tiger.CombatTraits", "87733e5295284048949fbb4b50c533c9", "BlueprintBuff"),
    ("KMG.Summoning.Special.Cheetah.Sprint", "a2ae66e5a8ca4bd2bbb8fe7001bd66cd", "BlueprintAbility"),
    ("KMG.Summoning.Special.Cheetah.SprintResource", "b1db61f007c547bdb0668ffac3e7b960", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.Cheetah.SprintState", "c7a7fd6c4c7e4ef29d9a634ed3de995d", "BlueprintBuff"),
    ("KMG.Summoning.Special.Cheetah.SprintAi", "4c41608d98f945d0897ff01e5a691cdc", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.Cheetah.Brain", "607f8ece3e8943faa13ab95ac611ad54", "BlueprintBrain"),
    ("KMG.Summoning.Special.Cheetah.CombatTraits", "056353ae7a1b4663b4e028e133eb4b8f", "BlueprintBuff"),
    ("KMG.Summoning.Natural.Claw1d8", "422318f491354d37aba69dece0004a11", "BlueprintItemWeapon"),
    # Correction order (2026-09-25): multi-link hold and held states, the Flytrap engulfed
    # state, the Cyclops hide armor, the docile hoof carriers, and the chartered mephit
    # abilities (wind wall, chill metal, pyrotechnics, magma form) with the ally-safe cloud area.
    ("KMG.Summoning.Special.Grapple.MultiHold", "a6ff9b2a7c704889a3adf6c28da683fe", "BlueprintBuff"),
    ("KMG.Summoning.Special.Grapple.MultiHeld", "c6a4dd2e1c3d401ca464178ffb590452", "BlueprintBuff"),
    ("KMG.Summoning.Special.GiantFlytrap.Engulfed", "6f8239e5e0ad4506b76972cd85a01de7", "BlueprintBuff"),
    ("KMG.Summoning.Special.Cyclops.HideArmor", "09b90e2a09ef4bf99c158d816ae8ccba", "BlueprintFeature"),
    ("KMG.Summoning.Special.Pony.CombatTraits", "94945076953646e9827fa7dae965e3b9", "BlueprintBuff"),
    ("KMG.Summoning.Special.Horse.CombatTraits", "824224c39ac440849414f8a78c5e50b7", "BlueprintBuff"),
    ("KMG.Summoning.Special.DustMephit.SpellLikeTwo", "342914575f204309b3ea64da949a347e", "BlueprintAbility"),
    ("KMG.Summoning.Special.DustMephit.SpellLikeTwoResource", "262dd7ae4bf74db8953698cbd5c3ccca", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.DustMephit.SpellLikeTwoAi", "2657bacbca8a4b6cbbb586cbd9546117", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.DustMephit.WindWallArea", "9d08edbf64a84f1aabdce39370774f76", "BlueprintAbilityAreaEffect"),
    ("KMG.Summoning.Special.DustMephit.WindWallState", "334df80385894c0189212aefecb509cd", "BlueprintBuff"),
    ("KMG.Summoning.Special.IceMephit.SpellLikeTwo", "4a2400396c7544039380104e76613816", "BlueprintAbility"),
    ("KMG.Summoning.Special.IceMephit.SpellLikeTwoResource", "757a99a5d9b8455ebdc4f1eac68db267", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.IceMephit.SpellLikeTwoAi", "8dac523325794ed3a767007d2e50eb90", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.IceMephit.ChillMetalState", "fb0e363ae9c440cf8d37d63c9b679e73", "BlueprintBuff"),
    ("KMG.Summoning.Special.MagmaMephit.SpellLikeOne", "bd1ea7fc5dbe4c228a500f757b31c188", "BlueprintAbility"),
    ("KMG.Summoning.Special.MagmaMephit.SpellLikeOneResource", "dbff52927879462d88faaf784e650594", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.MagmaMephit.SpellLikeOneAi", "9595136945f6453bbfe9ec16001da54d", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.MagmaMephit.SpellLikeTwo", "1fbf5c29c1974bdfadbbf9109b023420", "BlueprintAbility"),
    ("KMG.Summoning.Special.MagmaMephit.SpellLikeTwoResource", "249fc0db377e47c98c652af72bb5c4a5", "BlueprintAbilityResource"),
    ("KMG.Summoning.Special.MagmaMephit.SpellLikeTwoAi", "68a29584ba8b4c81a22d29d1782055e0", "BlueprintAiCastSpell"),
    ("KMG.Summoning.Special.MagmaMephit.MagmaFormState", "38ae68b0102a442f8eda8d85018f4ddc", "BlueprintBuff"),
    ("KMG.Summoning.Special.OozeMephit.StinkingCloudArea", "eb0e5daa5ddd408cbf6fb38acb0e8751", "BlueprintAbilityAreaEffect"),
    ("KMG.Summoning.Special.MagmaMephit.PyrotechnicsBlindedState", "a507e36761f9478f956d38d2a4b37d0a", "BlueprintBuff"),
)

PHASE1 = {
    "uniqueCreatures": 81,
    "summonMonsterEntries": 74,
    "summonMonsterPlacements": 414,
    "summonNaturesAllyEntries": 71,
    "summonNaturesAllyPlacements": 399,
    "registeredLogicalPlacements": 813,
    "publishedLogicalPlacements": 799,
    "templatedPlacements": 199,
    "nativeExpansionWrappers": 29,
    "naturalProfiles": 34,
    "projectIcons": 91,
    "foundationIdentities": 1472,
    "appendedLedgerIdentities": 288,
    "packageFileCountWithSoundBank": 251,
}
SPRINT3 = PHASE1  # the pins below read the current figures


def require_tokens(path: Path, *tokens: str) -> None:
    text = path.read_text(encoding="utf-8-sig")
    for token in tokens:
        if token not in text:
            raise AssertionError(f"{path.name} lacks required token: {token}")


def validate(root: Path) -> None:
    if len(APPENDED) != SPRINT3["appendedLedgerIdentities"] or \
            len({guid for _, guid, _ in APPENDED}) != len(APPENDED) or \
            len({symbol for symbol, _, _ in APPENDED}) != len(APPENDED):
        raise AssertionError("The Phase 1 appended identity list is malformed")
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    entries = manifest["entries"]
    tail = entries[PRESERVED_ENTRIES:]
    if [(e["symbol"], e["guid"], e["plannedType"]) for e in tail] != list(APPENDED):
        raise AssertionError("Expanded Summoning Phase 1 identities drifted")
    if any(e["status"] != "active" or e["milestone"] != "Expanded Summoning" for e in tail):
        raise AssertionError("Phase 1 identities must be active Expanded Summoning entries")
    # The append is exactly the manifest plan minus the preserved prefix:
    # nothing planned is missing and nothing unplanned was added. Order is
    # pinned by APPENDED itself (each sprint appends its own block, so the
    # ledger order is sprint order, not the plan's category order).
    plan = expanded_summoning_manifest.planned()
    prefix = {e["symbol"] for e in entries[:PRESERVED_ENTRIES]}
    expected = sorted((symbol, planned_type) for symbol, planned_type in plan
                      if symbol not in prefix)
    if expected != sorted((symbol, planned_type) for symbol, _, planned_type in APPENDED):
        raise AssertionError("Phase 1 append is not the manifest plan minus the preserved prefix")
    if len(plan) != SPRINT3["foundationIdentities"]:
        raise AssertionError("Expanded Summoning foundation identity count changed")
    expanded_summoning_manifest.validate(manifest, plan)

    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
        "Creatures.Length != %d" % SPRINT3["uniqueCreatures"],
        "ValidateFamily(SummonFamily.Monster, %d, %d)" % (
            SPRINT3["summonMonsterEntries"], SPRINT3["summonMonsterPlacements"]),
        "ValidateFamily(SummonFamily.NaturesAlly, %d, %d)" % (
            SPRINT3["summonNaturesAllyEntries"], SPRINT3["summonNaturesAllyPlacements"]),
        'C("pony","Pony",1,true,1)', 'C("horse","Horse",2,true,2)',
        'C("owlbear","Owlbear",null,false,4)', 'C("cyclops","Cyclops",null,false,5)',
        'C("shambling-mound","Shambling Mound",null,false,6)',
        'C("giant-flytrap","Giant Flytrap",null,false,7)',
        'C("purple-worm","Purple Worm",null,false,8)',
        'C("dust-mephit","Dust Mephit",4,false,4)', 'C("ice-mephit","Ice Mephit",4,false,4)',
        'C("magma-mephit","Magma Mephit",4,false,4)', 'C("ooze-mephit","Ooze Mephit",4,false,4)',
        'C("salt-mephit","Salt Mephit",4,false,4)', 'C("steam-mephit","Steam Mephit",4,false,4)',
        'C("tiger","Tiger",null,false,4,"Leopard")')
    require_tokens(root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
        "RegisteredLogicalPlacementCount = %d;" % SPRINT3["registeredLogicalPlacements"],
        "SuppressedLogicalPlacementCount = 14;")
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
        "UnitCount = %d;" % SPRINT3["uniqueCreatures"],
        "LogicalAbilityCount = %d;" % SPRINT3["registeredLogicalPlacements"],
        "TemplatedPlacementCount = %d;" % SPRINT3["templatedPlacements"],
        "NativeExpandedOptionIdentityCount = %d;" % SPRINT3["nativeExpansionWrappers"])
    require_tokens(root / "src/KingmakerGunslinger/Summoning/SummonNativeExpansionCatalog.cs",
        "Values.Length != %d" % SPRINT3["nativeExpansionWrappers"],
        '"6d8d59aa38713be4fa3be76c19107cc0","590cd3d5e76fdc649a5f97bc984cd3c4",true',
        '"256739c1e61e3f64eaf71734d271f4be","590cd3d5e76fdc649a5f97bc984cd3c4",true',
        '"9bd8cb6180842f44e9302c58e47b91f0","590cd3d5e76fdc649a5f97bc984cd3c4",true')
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
        "Values.Length != %d" % SPRINT3["naturalProfiles"],
        '"Animal", "Vermin", "MagicalBeast", "Humanoid", "Plant"')
    require_tokens(root / "src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs",
        "Values.Length != %d" % SPRINT3["projectIcons"])
    # Correction order: the chartered mephit roles and the ally-safe cloud stay in the code.
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs",
        '"Blur", "WindWall"', '"MagicMissile", "ChillMetal"', '"Pyrotechnics", "MagmaForm"',
        "WindWallRounds = 6", "WindWallOtherRangedMissChance = 30", "ChillMetalRounds = 7",
        "MagmaFormDamageReduction = 20", "MagmaFormSpeedFeet = 10", "PyrotechnicsBlindDieSides = 4")
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialCombatComponents.cs",
        "class SummonWindWallComponent", "evt.IncreaseMissChance(", "class SummonChillMetal",
        "class SummonChillMetalTargetChecker", "class SummonChillMetalComponent",
        "class ExpandedSummoningDocileHoovesBodyPatch", "buff.Components.OfType<SummonGrabComponent>()")
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
        "ConfigureWindWall(bySymbol, ability, prefix, token)",
        "ConfigureChillMetal(bySymbol, ability, prefix, token)",
        "ConfigurePyrotechnics(bySymbol, ability, prefix, token)",
        "ConfigureMagmaForm(bySymbol, ability, prefix, token, unit)",
        "MakeMephitCloudAllySafe(bySymbol, ability, prefix)", "MakeGlitterdustEnemyOnly(ability)",
        "UnitCondition.CanNotAttack", "ContextConditionIsAlly")
    # Sprint 5: the mephit pack's charter boundaries stay in the code.
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs",
        "MephitVariants.Length != 6", "MephitVisualTints.Length != 6",
        "MephitSickenedRounds = 3", "MephitSpellLikeUses = 1", "MephitBurstRadiusFeet = 20")
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
        "ConfigureMephitVariants(library, bySymbol)", "ContextConditionIsEnemy",
        "TargetType.Enemy", "4e42460798665fd4cb9173ffa7ada323",
        # Sprint 6: the repaired grabbers and the bounded web
        "MonitorLizardCombatTraitsSymbol, \"MonitorLizard\"", "GrizzlyBearCombatTraitsSymbol, \"GrizzlyBear\"",
        "DireBearCombatTraitsSymbol, \"DireBear\"", "ConfigureGiantSpiderWeb(library, bySymbol)",
        "a719abac0ea0ce346b401060754cc1c0")
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
        '"SpiderWebImmunity"', "shared summon grapple lifecycle (Sprint 6)",
        "rake gate", "shared summon grapple lifecycle (Sprint 7; correction order)")
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialCombatComponents.cs",
        "class SummonRakeComponent", "evt.AutoMiss = true;", "evt.SuspendCombatLog = true;",
        # Correction order: attack identity, target identity, the multi-link hold, the later-turn swallow
        "class SummonLimbs", "class SummonGrappleDamage", "class SummonMultiHoldComponent",
        "class SummonHeldComponent", "class ExpandedSummoningRakeSequencePatch",
        "IsHeldSinceRoundStart", "ShouldSwallowOnMaintain", "IsGrabSizeAllowed", "UnitHelper.TryBreakFree")
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
        "ConfigureGrabber(library, bySymbol, LeopardUnitSymbol", "ConfigureGrabber(library, bySymbol, DireTigerUnitSymbol",
        "ExpandedSummoningSpecialProfiles.LionVisualTint", "GrappleMultiHoldSymbol", "GrappleMultiHeldSymbol",
        "GiantFlytrapEngulfedSymbol", "ConfigureEngulfed(engulfed)",
        # Sprint 8: the tiger's carrier, the cheetah's sprint, both coats
        "ConfigureGrabber(library, bySymbol, TigerUnitSymbol", "ConfigureCheetahSprint(bySymbol)",
        "ExpandedSummoningSpecialProfiles.TigerCoat", "ExpandedSummoningSpecialProfiles.CheetahCoat")
    require_tokens(root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningVisualVariantPatch.cs",
        "class SummonCoatRasterizer", "CoatTextureSize = 512")
    require_tokens(root / "src/KingmakerGunslinger/Summoning/SummonViewScaleCatalog.cs",
        'S("tiger", 1.25f)', 'S("cheetah", 0.92f)')
    for path in (root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
                 root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdealRosterCatalog.cs"):
        if "Lightning" in path.read_text(encoding="utf-8-sig"):
            raise AssertionError("No Lightning Mephit may enter the catalogs: " + path.name)
    runtime_icons = json.loads((root / "assets/game/icons/expanded-summoning/icon-manifest.json")
                               .read_text(encoding="utf-8-sig"))
    if runtime_icons["count"] != SPRINT3["projectIcons"] or \
            len(runtime_icons["icons"]) != SPRINT3["projectIcons"] or \
            not {"pony", "horse", "owlbear", "cyclops", "shambling-mound", "giant-flytrap",
                 "purple-worm", "dust-mephit", "ice-mephit", "magma-mephit", "ooze-mephit",
                 "salt-mephit", "steam-mephit", "tiger"} <= {row["key"] for row in runtime_icons["icons"]}:
        raise AssertionError("Runtime icon manifest does not carry the Phase 1 icons")
    for key in ("pony", "horse", "owlbear", "cyclops", "shambling-mound", "giant-flytrap",
                "purple-worm", "dust-mephit", "ice-mephit", "magma-mephit", "ooze-mephit",
                "salt-mephit", "steam-mephit", "tiger"):
        if not (root / "assets/game/icons/expanded-summoning" / (key + ".png")).is_file() or \
                not (root / "assets-source/original-icons/expanded-summoning/sources" / (key + ".png")).is_file():
            raise AssertionError("Phase 1 icon file missing: " + key)
    require_tokens(root / "scripts/Build-Local.ps1",
        "{ %d } else { %d }" % (SPRINT3["packageFileCountWithSoundBank"],
                                SPRINT3["packageFileCountWithSoundBank"] - 2))
    require_tokens(root / "scripts/package.ps1",
        "{ %d } else { %d }" % (SPRINT3["packageFileCountWithSoundBank"],
                                SPRINT3["packageFileCountWithSoundBank"] - 2))

    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))[STATIC_KEY]
    for key, value in SPRINT3.items():
        if state.get(key) != value:
            raise AssertionError(f"Expanded Summoning Phase 1 metadata mismatch: {key}")
    if state.get("releaseAuthorized") is not False or state.get("draftOnly") is not True:
        raise AssertionError("Phase 1 must stay a draft: no release, deployment or Sprint 9")
    if state.get("humanReview") != "NOT_PERFORMED_NONBLOCKING":
        raise AssertionError("Phase 1 human review status must be recorded as not performed")
    if not isinstance(state.get("sprintsComplete"), list):
        raise AssertionError("Phase 1 must record which sprints are complete")


def main() -> int:
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except AssertionError as error:
        print(f"Expanded Summoning Phase 1 validation failed: {error}", file=sys.stderr)
        return 1
    print("Expanded Summoning Phase 1 validation PASS: %d appended identities; Sprint 3-8 pins exact."
          % len(APPENDED))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
