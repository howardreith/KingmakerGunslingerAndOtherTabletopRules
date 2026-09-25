#!/usr/bin/env python3
"""Validate the 0.0.139 Favored Class integration candidate.

Retains every inherited gate. Mechanical acceptance is the domain suite and
guarded runtime evidence, not these documentation/metadata checks. This is an
unpublished local candidate: the metadata must say so, must keep the exact
qualified host profile, and must never claim native qualification that was not
observed.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_better_vendors138 as baseline
import validate_sprint32

VERSION = "0.0.139"
INFORMATIONAL_VERSION = "0.0.139-favored-class-integration"
PACKAGE = "KingmakerGunslinger-0.0.139-local-runtime.zip"
PACKAGE_SUFFIX = "favored-class-integration"
DETERMINISTIC_TEST_COUNT = 1823
STATIC_KEY = "favoredClassIntegration139"

# Exact ordered (symbol, guid) pairs this candidate appends after the
# Better Vendors block.
APPENDED = (
    ("KMG.FavoredClass.Gunslinger.Grit.Partial", "718289fb8ab945e48880961722a344fd"),
    ("KMG.FavoredClass.Gunslinger.Grit.Full", "cd8674400bea40adbd8489a08b14eeff"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Pistol.Partial", "115ab4b2b0174a708cb93adac7826079"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Pistol.Full", "6d125884d3f94f83a59ba57112a31ec6"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Musket.Partial", "103ecf217c6b4e93865a846a2c584f03"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Musket.Full", "a603feebd92b45c683be87fe17b084c2"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Blunderbuss.Partial", "7627279fc5ae4239b6ccaa51d3f55032"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Blunderbuss.Full", "8bf4dee4bb164d93bf22a3bbff946dd7"),
    ("KMG.FavoredClass.Gunslinger.FirearmConfirmation.Partial", "b07889d80b5f48b285e364885a512815"),
    ("KMG.FavoredClass.Gunslinger.FirearmConfirmation.Full", "9d8e1f609b2b47d2b9c80a3525022e69"),
    ("KMG.FavoredClass.Gunslinger.PistolWhip.Partial", "e24c74437b88443298f1861c1eb2043e"),
    ("KMG.FavoredClass.Gunslinger.PistolWhip.Full", "84639afbb1d14aba83dcf6630eb626c0"),
    ("KMG.FavoredClass.Gunslinger.HalflingNimble.Partial", "40269699043e4fe3a512e5ee28dd93bd"),
    ("KMG.FavoredClass.Gunslinger.HalflingNimble.Full", "29a320ea1af0499d9185fe8321d60cae"),
    ("KMG.FavoredClass.Gunslinger.HalflingDodge.Partial", "b62476699d734b10bb1c124b549f634f"),
    ("KMG.FavoredClass.Gunslinger.HalflingDodge.Full", "5400219fafad4780b440dee91788eb68"),
    ("KMG.FavoredClass.Gunslinger.DrowNimble.Partial", "49359d21155e4741b6a1f438bf0db7ed"),
    ("KMG.FavoredClass.Gunslinger.DrowNimble.Full", "efb83ae0fde04abb96dd1e7cce0211e7"),
    ("KMG.FavoredClass.Gunslinger.Initiative.Partial", "385a62ab48214b32accaa2f5e7c86d66"),
    ("KMG.FavoredClass.Gunslinger.Initiative.Full", "403489a552e5470bb720a8148c2b09ed"),
    ("KMG.FavoredClass.Gunslinger.DirtyTrickTrip.Partial", "422ad9a4bd294b84b2f6230856ecdd10"),
    ("KMG.FavoredClass.Gunslinger.DirtyTrickTrip.Full", "ece977845f1c4b40a6df5183b6caa7e4"),
    ("KMG.FavoredClass.Alchemist.BombDamage.Partial", "6d101f4776294ec78ac07b2bf8b1ace7"),
    ("KMG.FavoredClass.Alchemist.BombDamage.Full", "c98af1a0630a452e8af968684e285183"),
    ("KMG.FavoredClass.Inquisitor.FireIntimidate.Partial", "96c9cb2bc89941b2a37f126aafccf3ca"),
    ("KMG.FavoredClass.Inquisitor.FireIntimidate.Full", "98847fc06c1c4f53a1da0ce0df78f266"),
    ("KMG.FavoredClass.Rogue.Demoralize.Partial", "a9f9782987db4b518ccc6726039f56fc"),
    ("KMG.FavoredClass.Rogue.Demoralize.Full", "bf08007130b24072b50fa165e640ffa1"),
    ("KMG.FavoredClass.Fighter.BullRushDefense.Full", "429de527d5dc46039e9d5a2311901374"),
    ("KMG.FavoredClass.Monk.UnarmedConfirmation.Partial", "8b3453ed61fe489a9f120c773215b68e"),
    ("KMG.FavoredClass.Monk.UnarmedConfirmation.Full", "c76759e1e881420a9248ce8fe74b5004"),
    ("KMG.FavoredClass.Cleric.AquaticPenetration.Full", "189538fd7b67433e8d0cea5e691dd8eb"),
    ("KMG.FavoredClass.Monk.GrappleStunning.Partial", "9c56579186ef4fdba0f07d956b1cb9a5"),
    ("KMG.FavoredClass.Monk.GrappleStunning.Full", "ca51bd6d19b241e48f05e6acf892a5f1"),
    ("KMG.FavoredClass.Paladin.AuraAllyBonus.Partial", "076fb613ebba4c54b3bcd3edbf841fa6"),
    ("KMG.FavoredClass.Paladin.AuraAllyBonus.Full", "4c88771d0d5b4348843d1011d7ae242e"),
    ("KMG.FavoredClass.Paladin.AuraAllyBonus.StepsProperty", "604f0f1bae3041618aa08b6348a088e6"),
    ("KMG.FavoredClass.Ranger.CompanionNaturalArmor.Partial", "a2fa12fa25a848c7b297a515e745865c"),
    ("KMG.FavoredClass.Ranger.CompanionNaturalArmor.Full", "4dda167346da47d7b8ee82244229df75"),
    ("KMG.FavoredClass.Ranger.CompanionNaturalArmor.PetFeature", "5c47cb4deec546289e7302295b4efae2"),
    ("KMG.FavoredClass.Summoner.EidolonNaturalArmor.Partial", "c8d050714c6c4def9e15f49dc9ea52b4"),
    ("KMG.FavoredClass.Summoner.EidolonNaturalArmor.Full", "02c00d3cf652431a93f0248c9a5d9c96"),
    ("KMG.FavoredClass.Summoner.EidolonNaturalArmor.PetFeature", "563c6a25ff824d14a6bfc8012d62e46a"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.FireRay.Partial", "917f52c439a14fa4ab27a38f3ed9c903"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.FireRay.Full", "5a0698585272446aa1d382f248662347"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.FireBlast.Partial", "d24a00e4558f4a8dacc00e30d519f829"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.FireBlast.Full", "53c4743882314561822fe7b188ab5c25"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.AirRay.Partial", "c914960ba2e24de68009307c87b6f3d5"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.AirRay.Full", "4d43fea45ce1455ba6a9bfaa1e9d4524"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.AirBlast.Partial", "2ab5c20220c746cfabb58e768a331590"),
    ("KMG.FavoredClass.Sorcerer.BloodlinePower.AirBlast.Full", "505a86ad1c104a8e890df0adbc48d8c3"),
    ("KMG.FavoredClass.Oracle.Revelation.AgingTouch.Partial", "da61fb425f834e54ad326c29f573d3ba"),
    ("KMG.FavoredClass.Oracle.Revelation.AgingTouch.Full", "0b996d9788144e3895b78b70d4843ee5"),
    ("KMG.FavoredClass.Oracle.Revelation.RewindTime.Partial", "12b27ffdb48b4649b0fbe01299910d37"),
    ("KMG.FavoredClass.Oracle.Revelation.RewindTime.Full", "6917e8ae89194baeb9f556080823bac1"),
    ("KMG.FavoredClass.Oracle.Revelation.SpeedOrSlowTime.Partial", "34f6c38db0414870acab783f4c011b4e"),
    ("KMG.FavoredClass.Oracle.Revelation.SpeedOrSlowTime.Full", "5456da6fa91043b6879f55a59a504541"),
    ("KMG.FavoredClass.Oracle.Revelation.TimeFlicker.Partial", "4d7c8e9b6030446d95eba9d322fcec20"),
    ("KMG.FavoredClass.Oracle.Revelation.TimeFlicker.Full", "1b508b073c3f4cfc936e7277a8073e99"),
    ("KMG.FavoredClass.Oracle.Revelation.TimeHop.Partial", "98f52f51b8cc4aec80700784d233481c"),
    ("KMG.FavoredClass.Oracle.Revelation.TimeHop.Full", "916f5b48b59b4d25bffdfcc8169ad690"),
    ("KMG.FavoredClass.Oracle.Revelation.TimeSight.Partial", "96e6aaf10bb24c0d850ba23bd657ed01"),
    ("KMG.FavoredClass.Oracle.Revelation.TimeSight.Full", "8e4a1d0940574c248b536a6d9d2ab616"),
    ("KMG.FavoredClass.Oracle.Revelation.EraseFromTime.Partial", "b766697324434c86815dbb5594eec193"),
    ("KMG.FavoredClass.Oracle.Revelation.EraseFromTime.Full", "e60cdb45988d44ae96153c6e09507dac"),
    ("KMG.FavoredClass.Oracle.Revelation.BloodOfHeroes.Partial", "41de88824f444c9da877f661311e8fa0"),
    ("KMG.FavoredClass.Oracle.Revelation.BloodOfHeroes.Full", "f23f08ef665c4acebc64ada0cac9a137"),
    ("KMG.FavoredClass.Oracle.Revelation.PhantomTouch.Partial", "55586ffacedd4bd3b860c3d4145b1ffb"),
    ("KMG.FavoredClass.Oracle.Revelation.PhantomTouch.Full", "b9f08028f9c544a49cc896a8d319ad2f"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritOfTheWarrior.Partial", "726fb3420f924ba0b6d2621fbcf7d2bb"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritOfTheWarrior.Full", "1a9ecf494f4d46a2901f58224705681a"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritShield.Partial", "67536ef33847470195881cf24cf1ce11"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritShield.Full", "f532effc462247138866c3ddaeca7212"),
    ("KMG.FavoredClass.Oracle.Revelation.StormOfSouls.Partial", "932ad0ce74ff4e8496ab8ffe48e6e883"),
    ("KMG.FavoredClass.Oracle.Revelation.StormOfSouls.Full", "f0759fce3c7b4d548c67ee32467f2f83"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritWalk.Partial", "6cf0a655c21a421494bbf8d0e97f88c0"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritWalk.Full", "c212127fbd5c4008ba3fa5c4d6ced7eb"),
    ("KMG.FavoredClass.Oracle.Revelation.AncestralWeapon.Partial", "4560550ddd1643cea93f7d57fb7174f8"),
    ("KMG.FavoredClass.Oracle.Revelation.AncestralWeapon.Full", "345f30d0cfb642b9bf2178944b22a4f4"),
    ("KMG.FavoredClass.Oracle.Revelation.FireBreath.Partial", "483578f4eb634baebf4f3e822eda22b6"),
    ("KMG.FavoredClass.Oracle.Revelation.FireBreath.Full", "9376cecc8bc14fd4881ad710189ee55f"),
    ("KMG.FavoredClass.Oracle.Revelation.Firestorm.Partial", "516438e5c3d04319b6b2d147288c7fb3"),
    ("KMG.FavoredClass.Oracle.Revelation.Firestorm.Full", "604214f43f6f4819b8376277a1965e50"),
    ("KMG.FavoredClass.Oracle.Revelation.FormOfFlame.Partial", "a6d373bf198d4139928cc0d2823e6447"),
    ("KMG.FavoredClass.Oracle.Revelation.FormOfFlame.Full", "c52628d882bc4af7b0b8827667a3abc6"),
    ("KMG.FavoredClass.Oracle.Revelation.HeatAura.Partial", "89557af95a6a4fe4bacba626d5f4d98d"),
    ("KMG.FavoredClass.Oracle.Revelation.HeatAura.Full", "1ea651d8ebd1477685d7cb14182b8112"),
    ("KMG.FavoredClass.Oracle.Revelation.TouchOfFlame.Partial", "937f104493004a16a16bf736e7edf337"),
    ("KMG.FavoredClass.Oracle.Revelation.TouchOfFlame.Full", "242d9d95cd7a469390998d2f05ea1158"),
    ("KMG.FavoredClass.Oracle.Revelation.Battlecry.Partial", "d2ec7b55af9849968f22f03f63965b49"),
    ("KMG.FavoredClass.Oracle.Revelation.Battlecry.Full", "9549be18e8f3445a8c06ccc1937449cc"),
    ("KMG.FavoredClass.Oracle.Revelation.BattleCombatHealer.Partial", "ff0fc48afbae465fa7d5f9673bde55d7"),
    ("KMG.FavoredClass.Oracle.Revelation.BattleCombatHealer.Full", "e4b11a41888744c1a056174ec552bcaa"),
    ("KMG.FavoredClass.Oracle.Revelation.IronSkin.Partial", "c69961cfd3454fc98a44535b525de48a"),
    ("KMG.FavoredClass.Oracle.Revelation.IronSkin.Full", "8b75b30d9ce749d49e2500efc3c76e5a"),
    ("KMG.FavoredClass.Oracle.Revelation.SurprisingCharge.Partial", "1e22bebe691042a2918b175903a9916b"),
    ("KMG.FavoredClass.Oracle.Revelation.SurprisingCharge.Full", "077d4309e84344c1963ad931db88e5d8"),
    ("KMG.FavoredClass.Oracle.Revelation.Channel.Partial", "b1ae5152bfd64f0da0a2883a77ed3a8b"),
    ("KMG.FavoredClass.Oracle.Revelation.Channel.Full", "5860a442a5dd422aacd4d7317844cda9"),
    ("KMG.FavoredClass.Oracle.Revelation.LifeCombatHealer.Partial", "67f0b311e06a49d5acc328f689db5b46"),
    ("KMG.FavoredClass.Oracle.Revelation.LifeCombatHealer.Full", "f9bfd6575c5a4f7b934aeb1997ddf150"),
    ("KMG.FavoredClass.Oracle.Revelation.EnergyBody.Partial", "b70d9f10db3c480d85a7781b5ef29752"),
    ("KMG.FavoredClass.Oracle.Revelation.EnergyBody.Full", "069c01b9f5b34f5f8fdf0e369568b266"),
    ("KMG.FavoredClass.Oracle.Revelation.LifeLink.Partial", "cdb582a92eb0409d9ec316b159475a6a"),
    ("KMG.FavoredClass.Oracle.Revelation.LifeLink.Full", "637d3271aab84071aa7a94aadebb5ca0"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritBoost.Partial", "f55ae4403bbb4497b2a99c43c67208f2"),
    ("KMG.FavoredClass.Oracle.Revelation.SpiritBoost.Full", "fe0cece45c02437cacf384aa09f5e0c1"),
    ("KMG.FavoredClass.Oracle.Revelation.AirBarrier.Partial", "8cc771ca74774444aaa0224c3e72b347"),
    ("KMG.FavoredClass.Oracle.Revelation.AirBarrier.Full", "dccb6b08f7c8480ea6a678285ceb7735"),
    ("KMG.FavoredClass.Oracle.Revelation.Invisibility.Partial", "5f718509fd19475488063b62104b1ccd"),
    ("KMG.FavoredClass.Oracle.Revelation.Invisibility.Full", "3f8ed052b7b74753a70e6257dbd63521"),
    ("KMG.FavoredClass.Oracle.Revelation.LightningBreath.Partial", "71a9451be80e468f8f931aa9f9a25097"),
    ("KMG.FavoredClass.Oracle.Revelation.LightningBreath.Full", "9a17180c433e4e8681c5be31550683c4"),
    ("KMG.FavoredClass.Oracle.Revelation.Thunderburst.Partial", "56f5990c98354d5dbb83282d6b2f7d15"),
    ("KMG.FavoredClass.Oracle.Revelation.Thunderburst.Full", "e1370a2d71fd4030967ee0813656a0e1"),
    ("KMG.FavoredClass.Oracle.Revelation.TouchOfElectricity.Partial", "109089c0829e4929b5e227b4e4306719"),
    ("KMG.FavoredClass.Oracle.Revelation.TouchOfElectricity.Full", "4486b7cdda964c0ea4e0dbbf2dfd7d67"),
    ("KMG.FavoredClass.Oracle.Revelation.PresenceOfDragons.Partial", "ff9b24c4e3d84ee7a695f5f5e1848740"),
    ("KMG.FavoredClass.Oracle.Revelation.PresenceOfDragons.Full", "c81fa38e4bb94f0c9b27cd10e81341b4"),
    ("KMG.FavoredClass.Oracle.Revelation.ScaledToughness.Partial", "5dd55cface544fa9a445db4e73369afb"),
    ("KMG.FavoredClass.Oracle.Revelation.ScaledToughness.Full", "0cabc8d4db034fd39a292192f16b9d49"),
    ("KMG.FavoredClass.Oracle.Revelation.BreathWeapon.Partial", "fa6f3516d0364626a8fe4498ad7fd534"),
    ("KMG.FavoredClass.Oracle.Revelation.BreathWeapon.Full", "265727dec0974d6d968f0ab7ba3fca71"),
    ("KMG.FavoredClass.Oracle.Revelation.FormOfTheDragon.Partial", "d2a18903bd884ff4bd873aacf72379fd"),
    ("KMG.FavoredClass.Oracle.Revelation.FormOfTheDragon.Full", "a82b2cd1f16943369a427fce5e259a99"),
    ("KMG.FavoredClass.Oracle.Revelation.Blizzard.Partial", "d3fb2d02cca74b51afac8f07a5f8e525"),
    ("KMG.FavoredClass.Oracle.Revelation.Blizzard.Full", "9c7017ee453f40b9a38ce9014c81829e"),
    ("KMG.FavoredClass.Oracle.Revelation.IceArmor.Partial", "208d15f10a554a6b9c6748e9c0c313f5"),
    ("KMG.FavoredClass.Oracle.Revelation.IceArmor.Full", "e0b6762d7f0f4adda5a6910d98b20160"),
    ("KMG.FavoredClass.Oracle.Revelation.WaterForm.Partial", "24847cbfae544a98b9691c0d29176f32"),
    ("KMG.FavoredClass.Oracle.Revelation.WaterForm.Full", "bec8145f859141169e24096c95e56dcf"),
    ("KMG.FavoredClass.Oracle.Revelation.WintryTouch.Partial", "6df5bb9349574b5c9ae86a9276a3e472"),
    ("KMG.FavoredClass.Oracle.Revelation.WintryTouch.Full", "202d6803d3dc443dbc95750f7fe3c556"),
    ("KMG.FavoredClass.Oracle.Revelation.PunitiveTransformation.Partial", "a19686b71b6c4b0f919ac6f6fe10f9f8"),
    ("KMG.FavoredClass.Oracle.Revelation.PunitiveTransformation.Full", "f4149108117c4ac19fa2dba2638205f6"),
    ("KMG.FavoredClass.Oracle.Revelation.ErosionTouch.Partial", "a251efd7c5924cbeadddb41cb6d4435b"),
    ("KMG.FavoredClass.Oracle.Revelation.ErosionTouch.Full", "37a08629d6bb4768aaa9932f20c224c6"),
    ("KMG.FavoredClass.Oracle.Revelation.LifeLich.Partial", "f268ba990713410c9727fdc424e49667"),
    ("KMG.FavoredClass.Oracle.Revelation.LifeLich.Full", "ab721ae0ca004520a2d169668ed6076b"),
    ("KMG.FavoredClass.Oracle.Revelation.FormOfTheBeast.Partial", "ea065d2104e14751a179e16057a29008"),
    ("KMG.FavoredClass.Oracle.Revelation.FormOfTheBeast.Full", "bc799e6efedb4d229e771aeb87a02752"),
    ("KMG.FavoredClass.Oracle.Revelation.GiftOfClawAndHorn.Partial", "dac34c408ee24555903ee9f993cd44b5"),
    ("KMG.FavoredClass.Oracle.Revelation.GiftOfClawAndHorn.Full", "c99eca1adfa44f2aa259f6c1c1a4a4f2"),
    ("KMG.FavoredClass.Oracle.Revelation.ArmorOfBones.Partial", "c00d6f59ef3b45a1a2edbcbad9463a9d"),
    ("KMG.FavoredClass.Oracle.Revelation.ArmorOfBones.Full", "a7fe7565ae6245a198c946d31cb7bb58"),
    ("KMG.FavoredClass.Oracle.Revelation.BleedingWounds.Partial", "bdb718c477974d5eb805d1131f741e48"),
    ("KMG.FavoredClass.Oracle.Revelation.BleedingWounds.Full", "6c315b3c650243819fe1b976652c4657"),
    ("KMG.FavoredClass.Oracle.Revelation.DeathsTouch.Partial", "f9a85fc32d0d4dedbe34058ab38d1695"),
    ("KMG.FavoredClass.Oracle.Revelation.DeathsTouch.Full", "5d71d81ab3ea42febf5ee222ed75e971"),
    ("KMG.FavoredClass.Oracle.Revelation.RaiseTheDead.Partial", "a17574d2c8e44336a14c1dd9e3c3138d"),
    ("KMG.FavoredClass.Oracle.Revelation.RaiseTheDead.Full", "f8cf74ce9d774a98ae73abe4b78f1ba4"),
    ("KMG.FavoredClass.Oracle.Revelation.SoulSiphon.Partial", "26d6ae83269a4a199fdadf9d2098725c"),
    ("KMG.FavoredClass.Oracle.Revelation.SoulSiphon.Full", "d10cdbc8903e49fe8662355abacaa38b"),
    ("KMG.FavoredClass.Oracle.Revelation.UndeadServitude.Partial", "152cb9741f1f45da8dc82055a09aec4d"),
    ("KMG.FavoredClass.Oracle.Revelation.UndeadServitude.Full", "0b1f716e9e974e70b22a6c04de41cbaa"),
    ("KMG.FavoredClass.Bard.PerformanceRange.InspireCourage.Full", "16205a81e0d04f68b55282e6a13cc97d"),
    ("KMG.FavoredClass.Bard.PerformanceRange.InspireCompetence.Full", "ea9809fdf1a944c3a442028a07d8862c"),
    ("KMG.FavoredClass.Bard.PerformanceRange.Fascinate.Full", "4cd2b01519d946db9c6bf65efaf244cf"),
    ("KMG.FavoredClass.Bard.PerformanceRange.DirgeOfDoom.Full", "613f8dc6bc3c4ec6a8c9e4274de90ca2"),
    ("KMG.FavoredClass.Bard.PerformanceRange.InspireGreatness.Full", "639a9524faf9421482c5e1cbeadc79f2"),
    ("KMG.FavoredClass.Bard.PerformanceRange.FrighteningTune.Full", "40da51168d684fc4ae303cd89daa1280"),
    ("KMG.FavoredClass.Bard.PerformanceRange.InspireHeroics.Full", "aad05815f7eb44f98521ad7abb18743f"),
    ("KMG.FavoredClass.Bard.PerformanceRange.InciteRage.Full", "07a4a190ac4e4a3980578bc37a38d0a5"),
    ("KMG.FavoredClass.Bard.PerformanceRange.StormCall.Full", "43753f2f0c5641ec838fb3f38009c617"),
    ("KMG.FavoredClass.Bard.PerformanceRange.FireDance.Full", "a4709cf1ef224d2ca624715d549b4de8"),
    ("KMG.FavoredClass.Bard.PerformanceRange.SongOfFieryGaze.Full", "5abb7971618c4164a2a0f60e796d0ee0"),
    ("KMG.FavoredClass.Bard.PerformanceRange.Satire.Full", "6d06672bc4d84a34837620e1135320b4"),
    ("KMG.FavoredClass.Bard.PerformanceRange.Mockery.Full", "a29d90d2b0e64569b64e92aac02a61b2"),
    ("KMG.FavoredClass.Bard.PerformanceRange.GloriousEpic.Full", "0f8cf9ae1c874e089bf9b875384ed706"),
    ("KMG.FavoredClass.Bard.PerformanceRange.Scandal.Full", "485193cfb5254e21ad30db05f1d2cdaa"),
    ("KMG.FavoredClass.Bard.PerformanceRange.DanceOfTheDead.Full", "7c7e7e8ded0847de9dfca9c6c74d0aaa"),
    ("KMG.MostlyHuman.Identity", "d71a4b4250914a7aa6d4bec53dfc768f"),
    ("KMG.MostlyHuman.Ifrit.Selection", "c0485fc76e514ad38c09961dbdc6efc7"),
    ("KMG.MostlyHuman.Ifrit.Standard", "7c508ee3415942bb91f584306d70bf70"),
    ("KMG.MostlyHuman.Ifrit.Trait", "8060afe7da3b40bbbc77b315ee23fdd3"),
    ("KMG.MostlyHuman.Oread.Selection", "e5bcb7c34a7b4b048c0d877019edac63"),
    ("KMG.MostlyHuman.Oread.Standard", "5c39e1237c4d4bbd9bfd0f1eaacb18c3"),
    ("KMG.MostlyHuman.Oread.Trait", "65aedd35c5a54273a9a33c2d58683a21"),
    ("KMG.MostlyHuman.Sylph.Selection", "2c93d16297764ce9a0f73622b9825c05"),
    ("KMG.MostlyHuman.Sylph.Standard", "0be790c9af23490e8f3ed46f5a59d00c"),
    ("KMG.MostlyHuman.Sylph.Trait", "21451d96af0843d088b04331ac7a5a99"),
    ("KMG.MostlyHuman.Undine.Selection", "bb0bae8a5720452eb16a1a6629e8fd4d"),
    ("KMG.MostlyHuman.Undine.Standard", "66868270c36a48c89947fbad8e6d979c"),
    ("KMG.MostlyHuman.Undine.Trait", "f3f6f6e074114b8da0a94a49b3cf4b47"),
)

HOST_SHA256 = "dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c"
HOST_MVID = "3efd38e7-8682-4b4d-8d53-e368a3664919"
COTW_SHA256 = "4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915"
COTW_MVID = "8caab254-aacf-4811-8093-44b9184e6e53"


def validate(root: Path) -> None:
    # Favored-class misfire reductions (G01/G18) made the scatter all-roll
    # aggregate use the effective threshold that decided each native roll.
    validate_sprint32.SCATTER_MISFIRE_AGGREGATE_TOKEN = "IsMisfire(misfireThreshold)"
    baseline.AUTHORIZED_APPENDED_AFTER = APPENDED
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    # The host stays optional: no UMM requirement or compile-time reference.
    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    if info.get("Requirements") != [] or info.get("Version") != VERSION:
        raise AssertionError("Info.json must not require Favored Class or Call of the Wild")
    project = root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
    baseline.forbid_tokens(project, "ZFavoredClass.dll", "CallOfTheWild.dll",
        "<Reference Include=\"ZFavoredClass", "<Reference Include=\"CallOfTheWild")
    # The adapter reads the host by reflection and never executes host code:
    # no reflective invocation, no second Core.load(), no runtime identities.
    for source in (root / "src/KingmakerGunslinger/FavoredClass").rglob("*.cs"):
        baseline.forbid_tokens(source, "using ZFavoredClass", "using CallOfTheWild",
            "Core.load();", ".Invoke(", "Guid.NewGuid")
    baseline.require_tokens(
        root / "src/KingmakerGunslinger/FavoredClass/FavoredClassHostContract.cs",
        HOST_SHA256, HOST_MVID, COTW_SHA256, COTW_MVID,
        '"binary-sha256"', '"binary-mvid"', '"dependency-sha256"', '"dependency-mvid"',
        '"core-load-incomplete"', '"gunslinger-not-scanned"')
    baseline.require_tokens(
        root / "src/KingmakerGunslinger/Scatter/ScatterAttackVolleyService.cs",
        "return Evaluate(definition, plan, rolls, definition.MisfireValue);",
        "if (roll.IsMisfire(misfireThreshold)) misfires++;")
    baseline.require_tokens(
        root / "src/KingmakerGunslinger/Misfires/EffectiveFirearmMisfireValuePolicy.cs",
        "Math.Max(FavoredClassFloor, unclamped - favoredClassReduction)",
        "if (unclamped <= MinimumEffectiveValue)")
    baseline.require_tokens(
        root / "docs/FAVORED-CLASS-COMPATIBILITY.md",
        HOST_SHA256, HOST_MVID, COTW_SHA256, COTW_MVID,
        "N = fullRank + partialRank", "unrestricted", "outside")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if static.get("version") != VERSION or static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the 0.0.139 candidate")
    state = static[STATIC_KEY]
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": False,
        "candidateOnly": True,
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        "hostVerifiedVersion": "1.3.1",
        "hostVerifiedFileSha256": HOST_SHA256,
        "hostVerifiedMvid": HOST_MVID,
        "callOfTheWildVerifiedFileSha256": COTW_SHA256,
        "callOfTheWildVerifiedMvid": COTW_MVID,
        "hostRequired": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Favored Class candidate metadata mismatch: {key}")
    if not isinstance(state.get("compatibilityRuntimeQualificationPending"), bool):
        raise AssertionError("The compatibility qualification status must be recorded")
    if not isinstance(state.get("nativeRuntimeQualified"), bool):
        raise AssertionError("Native runtime qualification must be recorded explicitly")
    if state.get("nativeRuntimeQualified") and not state.get("nativeRuntimeEvidence"):
        raise AssertionError("A native qualification claim needs recorded evidence")

    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.139.md",
        INFORMATIONAL_VERSION, "Favored Class", "optional", "candidate",
        "not published", "uninstall")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Favored Class integration {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Favored Class integration {VERSION} validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
