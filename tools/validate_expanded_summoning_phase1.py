#!/usr/bin/env python3
"""Expanded Summoning Phase 1 (charter Sprints 3-8) static gates.

Phase 1 appends creature identities to the frozen ledger without changing the
release version: Sprint 3 added Pony, Horse, Owlbear and Cyclops, the
Nature's Ally Frost Giant wrappers and the Cyclops Flash of Insight specials;
Sprint 4 added Shambling Mound, Giant Flytrap and Purple Worm and the shared
summon grapple lifecycle specials. This module pins that append exactly -
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
# Sprint 4, in ledger order.
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
    ("KMG.Summoning.Special.PurpleWorm.Swallowed", "8a25b84199cd42319fa77b0b1d1d5bea", "BlueprintBuff")
)

PHASE1 = {
    "uniqueCreatures": 74,
    "summonMonsterEntries": 68,
    "summonMonsterPlacements": 378,
    "summonNaturesAllyEntries": 64,
    "summonNaturesAllyPlacements": 357,
    "registeredLogicalPlacements": 735,
    "publishedLogicalPlacements": 721,
    "templatedPlacements": 199,
    "nativeExpansionWrappers": 29,
    "naturalProfiles": 33,
    "projectIcons": 84,
    "foundationIdentities": 1295,
    "appendedLedgerIdentities": 111,
    "packageFileCountWithSoundBank": 244,
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
        'C("purple-worm","Purple Worm",null,false,8)')
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
    runtime_icons = json.loads((root / "assets/game/icons/expanded-summoning/icon-manifest.json")
                               .read_text(encoding="utf-8-sig"))
    if runtime_icons["count"] != SPRINT3["projectIcons"] or \
            len(runtime_icons["icons"]) != SPRINT3["projectIcons"] or \
            not {"pony", "horse", "owlbear", "cyclops", "shambling-mound", "giant-flytrap",
                 "purple-worm"} <= {row["key"] for row in runtime_icons["icons"]}:
        raise AssertionError("Runtime icon manifest does not carry the Phase 1 icons")
    for key in ("pony", "horse", "owlbear", "cyclops", "shambling-mound", "giant-flytrap",
                "purple-worm"):
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
    print("Expanded Summoning Phase 1 validation PASS: %d appended identities; Sprint 3-4 pins exact."
          % len(APPENDED))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
