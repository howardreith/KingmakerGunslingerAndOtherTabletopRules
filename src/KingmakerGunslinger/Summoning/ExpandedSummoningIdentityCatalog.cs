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
        internal const int UnitCount = 80;
        internal const int LogicalAbilityCount = 807;
        internal const int TemplatedPlacementCount = 199;
        internal const int TemplateExecutionAbilityCount = TemplatedPlacementCount * 2;
        internal const int TemplateBuffCount = 8;
        internal const int SpecialIdentityCount = 94;
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
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite1d4", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite1d3", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Tail1d12", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Tail3d6", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Bite2d8", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Natural.Talon2d6", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Subtype.Extraplanar", "BlueprintFeature"));
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
