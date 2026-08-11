using System;
using System.Linq;
using System.IO;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningIdentityCatalogTests
    {
        internal static void FoundationLedgerIsExactAndDeterministic()
        {
            var first = ExpandedSummoningIdentityCatalog.Build();
            var second = ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(1120, first.Count, "Foundation identity count changed.");
            Assertions.Equal(67, first.Count(value => value.PlannedType == "BlueprintUnit"), "Unit identity count changed.");
            Assertions.Equal(1045, first.Count(value => value.PlannedType == "BlueprintAbility"), "Ability identity count changed.");
            Assertions.Equal(8, first.Count(value => value.PlannedType == "BlueprintBuff"), "Template buff identity count changed.");
            Assertions.Equal(string.Join("|", first.Select(value => value.Symbol)),
                string.Join("|", second.Select(value => value.Symbol)), "Identity output is not deterministic.");
        }

        internal static void TemplateExecutionsAreFamilyScoped()
        {
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(182, identities.Count(value => value.Symbol.EndsWith(".Celestial", StringComparison.Ordinal)),
                "Celestial execution count changed.");
            Assertions.Equal(182, identities.Count(value => value.Symbol.EndsWith(".Fiendish", StringComparison.Ordinal)),
                "Fiendish execution count changed.");
            Assertions.True(!identities.Any(value => value.Symbol.Contains(".SNA.") &&
                (value.Symbol.EndsWith(".Celestial", StringComparison.Ordinal) ||
                 value.Symbol.EndsWith(".Fiendish", StringComparison.Ordinal))),
                "SNA must not receive celestial or fiendish execution identities.");
        }

        internal static void TemplateHitDiceBandsAreExact()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                SummonTemplateBandPolicy.Select(-1), "Negative HD must fail closed.");
            Assertions.Equal(SummonTemplateBand.Low,
                SummonTemplateBandPolicy.Select(0), "Zero HD band changed.");
            Assertions.Equal(SummonTemplateBand.Low,
                SummonTemplateBandPolicy.Select(4), "Four HD band changed.");
            Assertions.Equal(SummonTemplateBand.Mid,
                SummonTemplateBandPolicy.Select(5), "Five HD band changed.");
            Assertions.Equal(SummonTemplateBand.Mid,
                SummonTemplateBandPolicy.Select(10), "Ten HD band changed.");
            Assertions.Equal(SummonTemplateBand.High,
                SummonTemplateBandPolicy.Select(11), "Eleven HD band changed.");
            Assertions.Equal(5, SummonTemplateBandPolicy.ResistanceValue(
                SummonTemplateBand.Mid), "Mid resistance changed.");
            Assertions.Equal(10, SummonTemplateBandPolicy.ResistanceValue(
                SummonTemplateBand.High), "High resistance changed.");
            Assertions.False(SummonTemplateBandPolicy.GrantsSpellResistance(
                SummonTemplateBand.Low), "Low template must omit SR below 5 HD.");
            Assertions.True(SummonTemplateBandPolicy.GrantsSpellResistance(
                SummonTemplateBand.Mid), "Mid template must grant SR at 5 HD.");
        }

        internal static void TemplateSmitePolicyIsBoundedAndOpposed()
        {
            Assertions.True(SummonTemplateSmitePolicy.IsEligible(true, 5),
                "Celestial smite must recognize neutral evil.");
            Assertions.True(SummonTemplateSmitePolicy.IsEligible(true, 20),
                "Celestial smite must recognize chaotic evil.");
            Assertions.False(SummonTemplateSmitePolicy.IsEligible(true, 18),
                "Celestial smite must reject chaotic good.");
            Assertions.True(SummonTemplateSmitePolicy.IsEligible(false, 3),
                "Fiendish smite must recognize neutral good.");
            Assertions.False(SummonTemplateSmitePolicy.IsEligible(false, 12),
                "Fiendish smite must reject lawful evil.");
            Assertions.Equal(0, SummonTemplateSmitePolicy.AttackBonus(-2),
                "Smite must not convert a negative Charisma modifier into a penalty.");
            Assertions.Equal(11, SummonTemplateSmitePolicy.DamageBonus(11),
                "Smite damage must equal hit dice.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                SummonTemplateSmitePolicy.DamageBonus(-1),
                "Negative hit dice must fail closed.");

            string component = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningSmiteComponent.cs"));
            foreach (string token in new[] { "RuleAttackRoll",
                "RuleCalculateWeaponStats", "RuleAttackWithWeapon",
                "evt.AddBonusDamage(bonus)", "evt.AttackRoll.IsHit",
                "Owner.Buffs.RemoveFact(Fact)" })
                Assertions.True(component.Contains(token),
                    "Bounded smite component contract is missing: " + token);
            Assertions.False(component.Contains("ContextActionApplyBuff"),
                "Bounded smite must not create target buffs.");
        }

        internal static void RuntimeAlignmentPolicyIsFamilyScopedAndExact()
        {
            int resolved;
            Assertions.True(SummonAlignmentRuntimePolicy.TryResolve(
                SummonAlignmentMode.Celestial, 9, null, out resolved),
                "Lawful-neutral celestial resolution failed.");
            Assertions.Equal(10, resolved,
                "Celestial must preserve law and replace the moral axis.");
            Assertions.True(SummonAlignmentRuntimePolicy.TryResolve(
                SummonAlignmentMode.Fiendish, 17, null, out resolved),
                "Chaotic-neutral fiendish resolution failed.");
            Assertions.Equal(20, resolved,
                "Fiendish must preserve chaos and replace the moral axis.");
            Assertions.True(SummonAlignmentRuntimePolicy.TryResolve(
                SummonAlignmentMode.Caster, 1, 12, out resolved),
                "Nature's Ally caster alignment resolution failed.");
            Assertions.Equal(12, resolved,
                "Nature's Ally must receive the caster's exact alignment.");
            Assertions.False(SummonAlignmentRuntimePolicy.TryResolve(
                SummonAlignmentMode.Caster, 1, null, out resolved),
                "Missing caster alignment must fail closed.");
            Assertions.False(SummonAlignmentRuntimePolicy.TryResolve(
                (SummonAlignmentMode)99, 1, null, out resolved),
                "Unknown alignment mode must fail closed.");
            Assertions.False(SummonAlignmentRuntimePolicy.TryResolve(
                SummonAlignmentMode.Celestial, 511, null, out resolved),
                "Non-exact owner alignment must fail closed.");

            string action = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ContextActionSetSummonAlignment.cs"));
            foreach (string token in new[] { "Context.MaybeCaster",
                "target.Descriptor.Alignment.Set", "Target.Unit",
                "SummonAlignmentRuntimePolicy.TryResolve" })
                Assertions.True(action.Contains(token),
                    "Spawn alignment action is missing: " + token);
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningAbilityBuilder.cs"));
            foreach (string token in new[] { "SummonAlignmentMode.Celestial",
                "SummonAlignmentMode.Fiendish", "SummonAlignmentMode.Caster",
                "AppendAlignmentAction(target.ComponentsArray" })
                Assertions.True(builder.Contains(token),
                    "Family-scoped alignment publication is missing: " + token);
        }

        internal static void SymbolsEncodeEveryLogicalPlacement()
        {
            var identities = ExpandedSummoningIdentityCatalog.Build();
            int found = 0;
            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family))
            {
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                Assertions.Equal(1, identities.Count(value => value.Symbol == symbol),
                    "Logical placement identity missing or duplicated: " + symbol);
                found++;
            }
            Assertions.Equal(681, found, "Logical placement traversal changed.");
        }

        internal static void DonorsCoverEveryFrozenCreature()
        {
            ExpandedSummoningDonorCatalog.Validate();
            Assertions.Equal(67, ExpandedSummoningDonorCatalog.All.Count,
                "Every unique creature requires exactly one frozen donor decision.");
            Assertions.True(ExpandedSummoningDonorCatalog.All.Any(value =>
                !value.DedicatedSummon),
                "Proxy donors must remain explicit sanitizer obligations.");
        }

        internal static void AbilityBuilderPreservesNativeGraphContracts()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningAbilityBuilder.cs"));
            foreach (string token in new[] { "MonsterParents", "AllyParents",
                "NativeTemplate", "DeepCloneComponent", "ReplaceSpawnUnits",
                "Expected at least one native spawn action", "MaterialComponentData",
                "variant.Multiplicity == SummonMultiplicity.OneD3",
                "type == typeof(ActionList)",
                "(GameAction)DeepClone(action, seen)",
                "!(source is GameAction)",
                "source is BlueprintComponent || source is GameAction" })
                Assertions.True(source.Contains(token),
                    "Ability builder contract is missing: " + token);
        }

        internal static void RuntimePublicationIsAdditiveAndTransactional()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningPublication.cs"));
            foreach (string token in new[] { "SummonVariantMergePolicy.Merge",
                "OriginalComponents", "PublishedComponents",
                "rollback refused after unrelated mutation",
                "originals.Any(original => !variants.Variants.Contains(original))" })
                Assertions.True(source.Contains(token),
                    "Runtime publication contract is missing: " + token);
        }

        internal static void RuntimeUnitComponentsAreReferenceIsolated()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningBlueprints.cs"));
            foreach (string token in new[] { "DeepCloneUnitComponent",
                "FormatterServices.GetUninitializedObject",
                "ScriptableObject.CreateInstance(type)",
                "source is BlueprintScriptableObject", "ReferenceComparer.Instance" })
                Assertions.True(source.Contains(token),
                    "Unit component isolation contract is missing: " + token);
            Assertions.False(source.Contains("!IsForbiddenComponent(component.GetType().Name)).ToArray()"),
                "Retained donor components must not remain shared instances.");
            string runtime = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "expanded-summoning-donor-component-isolation",
                "expanded-summoning-prohibited-references",
                "expanded-summoning-inherited-class-spells",
                "expanded-summoning-starting-inventory" })
                Assertions.True(runtime.Contains(token),
                    "Guarded isolation observer is missing: " + token);
            foreach (string token in new[] {
                "expanded-summoning-template-logical-choices",
                "expanded-summoning-celestial-executions",
                "expanded-summoning-fiendish-executions",
                "expanded-summoning-template-buffs",
                "expanded-summoning-smite-markers",
                "expanded-summoning-runtime-alignments",
                "expanded-summoning-native-action-isolation" })
                Assertions.True(runtime.Contains(token),
                    "Guarded template observer is missing: " + token);
            string inventory = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningInventoryObserver.cs"));
            foreach (string token in new[] { "ExactTemplateMechanicGuids",
                "69f0d7d1077f492f8237952f8219a270",
                "3e33af2ab5974859bdaa92c32987b3e0",
                "bf0882a6d254407bb259356f1aa66392",
                "a432066702694b2590260b58426fee28",
                "f009c072167c4b53a37c1071a2251c3f",
                "320b92730bd54842b9707931a5dbab18",
                "b4274c5bb0bf2ad4190eb7c44859048b",
                "template-mechanic-summary", "TemplateResourceAmount",
                "ObjectGraph(value.ComponentsArray, 12)" })
                Assertions.True(inventory.Contains(token),
                    "Exact template-mechanic inventory is missing: " + token);
        }

        internal static void TemplateBlueprintsUseNativeBoundedMechanics()
        {
            string templates = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningTemplateBuilder.cs"));
            foreach (string token in new[] { "DamageEnergyType.Acid",
                "DamageEnergyType.Cold", "DamageEnergyType.Electricity",
                "DamageEnergyType.Fire", "BypassedByAlignment = true",
                "DamageAlignment.Evil", "DamageAlignment.Good",
                "AddSpellResistance", "resistance.AddCR = true", "Mid" })
                Assertions.True(templates.Contains(token),
                    "Template buff contract is missing: " + token);
            string abilities = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningAbilityBuilder.cs"));
            foreach (string token in new[] { "ConfigureTemplateChoice",
                "AbilityCasterAlignment", "(AlignmentMaskType)63",
                "(AlignmentMaskType)504", "ContextActionApplyBuff",
                "Permanent = true", "IsNotDispelable = true", "AsChild = true",
                "SpellDescriptor.Good", "SpellDescriptor.Evil" })
                Assertions.True(abilities.Contains(token),
                    "Template execution contract is missing: " + token);
            foreach (string token in new[] { "SummonTemplateBandPolicy.Select",
                "KMG.Summoning.Template.Celestial.\" + band",
                "KMG.Summoning.Template.Fiendish.\" + band" })
                Assertions.True(abilities.Contains(token),
                    "Template HD-band contract is missing: " + token);
            foreach (string token in new[] {
                "KMG.Summoning.Smite.Celestial.Available",
                "KMG.Summoning.Smite.Fiendish.Available",
                "AppendTemplateBuff(target.ComponentsArray, smiteBuff)" })
                Assertions.True(abilities.Contains(token),
                    "Bounded template smite publication is missing: " + token);
        }
    }
}
