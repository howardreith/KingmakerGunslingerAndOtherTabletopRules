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
        internal const int UnitCount = 67;
        internal const int LogicalAbilityCount = 681;
        internal const int TemplatedPlacementCount = 182;
        internal const int TemplateExecutionAbilityCount = TemplatedPlacementCount * 2;
        internal const int TemplateBuffCount = 8;
        internal const int SpecialIdentityCount = 30;
        internal const int NativePreservationIdentityCount = 2;
        internal const int FoundationIdentityCount = UnitCount + LogicalAbilityCount +
            TemplateExecutionAbilityCount + TemplateBuffCount + SpecialIdentityCount +
            NativePreservationIdentityCount;

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
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.SleepBow", "BlueprintItemWeapon"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDance", "BlueprintAbility"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDanceState", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDanceResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.SleepArrowResource", "BlueprintAbilityResource"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.CombatTraits", "BlueprintBuff"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.IrresistibleDanceAi", "BlueprintAiCastSpell"));
            result.Add(new SummoningIdentitySpec("KMG.Summoning.Special.Pixie.Brain", "BlueprintBrain"));
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
