using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummoningIdentitySpec
    {
        internal SummoningIdentitySpec(string symbol, string plannedType)
        {
            if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("An identity symbol is required.", "symbol");
            if (string.IsNullOrWhiteSpace(plannedType)) throw new ArgumentException("A planned type is required.", "plannedType");
            Symbol = symbol;
            PlannedType = plannedType;
        }
        internal string Symbol { get; private set; }
        internal string PlannedType { get; private set; }
    }

    internal static class ExpandedSummoningIdentityCatalog
    {
        internal const int UnitCount = 95;
        internal const int LogicalAbilityCount = 976;
        internal const int TemplatedPlacementCount = 271;
        internal const int TemplateExecutionAbilityCount = TemplatedPlacementCount * 2;
        internal const int TemplateBuffCount = 8;
        // Sprint 14 adds eight: the soldier's sting, its poison and venom,
        // the ants' racial Perception, a unit type for the beetle and one
        // shared by all the ant castes, the beetle's luminescence, and the
        // soldier's grab traits carrier. Sprint 15 adds three: the Drone's own
        // grab traits carrier, the Giant Stag Beetle's trample and its unit
        // type. The Drone needs no poison graph of its own, because the DC is
        // derived live from the caster's Constitution. Sprint 16's
        // registration adds two, both weapons: no native blueprint carries a
        // 3d6 bite or a 4d8 tail slap, which is the Dire Crocodile's printed
        // routine. Its death roll and swallow whole will add their own when
        // they are implemented.
        internal const int SpecialIdentityCount = 192;
        internal const int NativePreservationIdentityCount = 2;
        internal const int AlignmentModeIdentityCount = 3;
        internal const int NativeExpandedOptionIdentityCount = 29;
        internal const int FoundationIdentityCount = UnitCount + LogicalAbilityCount +
            TemplateExecutionAbilityCount + TemplateBuffCount + SpecialIdentityCount +
            NativePreservationIdentityCount + AlignmentModeIdentityCount +
            NativeExpandedOptionIdentityCount;

        internal const string NativeMonsterTierOneSymbol =
            "KMG.Summoning.Native.SM.Tier1";
        internal const string NativeNaturesAllyTierOneSymbol =
            "KMG.Summoning.Native.SNA.Tier1";

        internal static IReadOnlyList<SummoningIdentitySpec> Build()
        {
            var result = new List<SummoningIdentitySpec>();
            foreach (SummonCreatureSpec creature in ExpandedSummoningCatalog.All)
                result.Add(new SummoningIdentitySpec("KMG.Summoning.Unit." + Token(creature.Key), "BlueprintUnit"));
            result.Add(new SummoningIdentitySpec(NativeMonsterTierOneSymbol,
                "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec(NativeNaturesAllyTierOneSymbol,
                "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec(
                "KMG.Summoning.AlignmentMode.Feature", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec(
                "KMG.Summoning.AlignmentMode.FiendishMarker", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec(
                "KMG.Summoning.AlignmentMode.Toggle",
                "BlueprintActivatableAbility"));
            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family))
            {
                string symbol = AbilitySymbol(variant);
                result.Add(new SummoningIdentitySpec(symbol, "BlueprintAbility"));
                if (family == SummonFamily.Monster && variant.Creature.MonsterTemplated)
                {
                    result.Add(new SummoningIdentitySpec(symbol + ".Celestial", "BlueprintAbility"));
                    result.Add(new SummoningIdentitySpec(symbol + ".Fiendish", "BlueprintAbility"));
                }
            }
            foreach (SummonNativeExpansionSpec native in
                SummonNativeExpansionCatalog.All)
                result.Add(new SummoningIdentitySpec(native.Symbol,
                    "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Template.Celestial.Low", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Template.Celestial.Mid", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Template.Celestial.High", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Template.Fiendish.Low", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Template.Fiendish.Mid", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Template.Fiendish.High", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Smite.Celestial.Available", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Smite.Fiendish.Available", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.LanternArchon.LightRay", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.LanternArchon.LightRayAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.LanternArchon.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.LanternArchon.Defenses", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShadowDemon.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Salamander.SpearType", "BlueprintWeaponType"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Salamander.Spear", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Salamander.Tail", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Salamander.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Succubus.Dominate", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Succubus.Domination", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Succubus.DominateAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Succubus.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Succubus.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Bebelith.Claw", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Bebelith.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Bebelith.DismantledArmor", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.SleepBowType", "BlueprintWeaponType"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.SleepBow", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDance", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDanceState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDanceResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.SleepArrowResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDanceAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cyclops.FlashOfInsight", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cyclops.FlashOfInsightState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cyclops.FlashOfInsightResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cyclops.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cyclops.FlashOfInsightAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cyclops.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Grapple.Hold", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Grapple.Grappled", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Owlbear.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShamblingMound.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantFlytrap.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.PurpleWorm.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.PurpleWorm.Swallowed", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.Breath", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.BreathAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.SpellLikeOne", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.SpellLikeOneResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.SpellLikeOneAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.Breath", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.BreathAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.SpellLikeOne", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.SpellLikeOneResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.SpellLikeOneAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.Breath", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.BreathAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.Breath", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.BreathAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.SpellLikeOne", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.SpellLikeOneResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.SpellLikeOneAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.SpellLikeTwo", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.SpellLikeTwoResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.Breath", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.BreathAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.SpellLikeOne", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.SpellLikeOneResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.SpellLikeOneAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.SpellLikeTwo", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.SpellLikeTwoResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SaltMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.Breath", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.BreathAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.SpellLikeOne", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.SpellLikeOneResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.SpellLikeOneAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.SpellLikeTwo", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.SpellLikeTwoResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.SteamMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MonitorLizard.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GrizzlyBear.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireBear.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantSpider.Web", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantSpider.WebResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantSpider.WebAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantSpider.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantSpider.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Leopard.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Lion.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireLion.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireTiger.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Tiger.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cheetah.Sprint", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cheetah.SprintResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cheetah.SprintState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cheetah.SprintAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cheetah.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cheetah.CombatTraits", "BlueprintBuff"));
            // Correction order (2026-09-25): the multi-link hold, its held state and the Flytrap's engulfed state.
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Grapple.MultiHold", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Grapple.MultiHeld", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantFlytrap.Engulfed", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Cyclops.HideArmor", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pony.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Horse.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.SpellLikeTwo", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.SpellLikeTwoResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.WindWallArea", "BlueprintAbilityAreaEffect"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DustMephit.WindWallState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.SpellLikeTwo", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.SpellLikeTwoResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.IceMephit.ChillMetalState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.SpellLikeOne", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.SpellLikeOneResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.SpellLikeOneAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.SpellLikeTwo", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.SpellLikeTwoResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.MagmaFormState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.MagmaMephit.PyrotechnicsBlindedState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.OozeMephit.StinkingCloudArea", "BlueprintAbilityAreaEffect"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite1d4", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.DireBat.Blindsense", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite1d3", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Tail1d12", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Tail3d6", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite2d8", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Talon2d6", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Claw1d8", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Subtype.Extraplanar", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.WaspSting1d8", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantWasp.Poison", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantWasp.Venom", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantWasp.UnitType", "BlueprintUnitType"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.StirgeTouch", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Stirge.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Stirge.Hold", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Rhinoceros.PowerfulCharge", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.WoollyRhinoceros.PowerfulCharge", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Aurochs.Trample", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Bison.Trample", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.WoollyRhinoceros.Trample", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Stirge.Remove", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.DireRat.Disease", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GoblinDog.Traits", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GoblinDog.AllergicReaction", "BlueprintBuff"));
            // Sprint 13
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite1", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Wolverine.Rage", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Wolverine.RageOnset", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Wolverine.RageState", "BlueprintBuff"));
            // Sprint 13 Shadow Mastiff. Printed: bite +10 (1d8+4 plus trip),
            // tail slap +5 (1d6+2). The tail slap's 1d6 has no project-owned
            // identity yet; bay and shadow blend are its two printed Su
            // abilities, and the per-mastiff bay immunity is the printed
            // rule's own 24-hour bound.
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Tail1d6", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShadowMastiff.Traits", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShadowMastiff.Bay", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShadowMastiff.BayPanic", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShadowMastiff.BayImmunity", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShadowMastiff.ShadowBlend", "BlueprintActivatableAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.ShadowMastiff.ShadowBlendState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.AntSting1d4", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantAnt.Poison", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantAnt.Venom", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantAnt.RacialSkills", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantAnt.UnitType", "BlueprintUnitType"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.FireBeetle.Luminescence", "BlueprintFeature"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.FireBeetle.UnitType", "BlueprintUnitType"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantAntSoldier.Traits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantAntDrone.Traits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.GiantStagBeetle.Trample", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.GiantStagBeetle.UnitType", "BlueprintUnitType"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite3d6", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Tail4d8", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Crocodile.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireCrocodile.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Crocodile.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Crocodile.Sprint", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Crocodile.SprintAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Crocodile.SprintState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Crocodile.SprintCooldown", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireCrocodile.Brain", "BlueprintBrain"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireCrocodile.Sprint", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireCrocodile.SprintAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireCrocodile.SprintState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireCrocodile.SprintCooldown", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.DireCrocodile.Swallowed", "BlueprintBuff"));
            Validate(result);
            return result.AsReadOnly();
        }

        internal static string AbilitySymbol(SummonVariantSpec variant)
        {
            if (variant == null) throw new ArgumentNullException("variant");
            string family = variant.Family == SummonFamily.Monster ? "SM" : "SNA";
            string count = variant.Multiplicity == SummonMultiplicity.One ? "One" :
                variant.Multiplicity == SummonMultiplicity.OneD3 ? "OneD3" : "OneD4PlusOne";
            return "KMG.Summoning.Ability." + family + ".Tier" + variant.ParentTier + "." +
                Token(variant.Creature.Key) + "." + count;
        }

        internal static string UnitSymbol(SummonCreatureSpec creature)
        {
            if (creature == null) throw new ArgumentNullException("creature");
            return "KMG.Summoning.Unit." + Token(creature.Key);
        }

        internal static void Validate(IEnumerable<SummoningIdentitySpec> identities)
        {
            if (identities == null) throw new ArgumentNullException("identities");
            SummoningIdentitySpec[] values = identities.ToArray();
            if (values.Length != FoundationIdentityCount)
                throw new InvalidOperationException("Expanded Summoning foundation identity count must be " + FoundationIdentityCount + ".");
            if (values.Any(value => value == null)) throw new InvalidOperationException("Identity catalog contains null.");
            if (values.Select(value => value.Symbol).Distinct(StringComparer.Ordinal).Count() != values.Length)
                throw new InvalidOperationException("Identity catalog contains duplicate symbols.");
        }

        private static string Token(string key)
        {
            var result = new StringBuilder();
            bool upper = true;
            foreach (char value in key)
            {
                if (!char.IsLetterOrDigit(value)) { upper = true; continue; }
                result.Append(upper ? char.ToUpperInvariant(value) : value);
                upper = false;
            }
            if (result.Length == 0) throw new ArgumentException("A symbol token cannot be empty.", "key");
            return result.ToString();
        }
    }
}
