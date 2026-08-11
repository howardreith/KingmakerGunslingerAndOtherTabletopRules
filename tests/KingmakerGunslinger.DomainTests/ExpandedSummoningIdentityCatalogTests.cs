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
            Assertions.Equal(1144, first.Count, "Foundation identity count changed.");
            Assertions.Equal(67, first.Count(value => value.PlannedType == "BlueprintUnit"), "Unit identity count changed.");
            Assertions.Equal(1048, first.Count(value => value.PlannedType == "BlueprintAbility"), "Ability identity count changed.");
            Assertions.Equal(16, first.Count(value => value.PlannedType == "BlueprintBuff"), "Buff identity count changed.");
            Assertions.Equal(3, first.Count(value => value.PlannedType == "BlueprintAiCastSpell"), "AI identity count changed.");
            Assertions.Equal(3, first.Count(value => value.PlannedType == "BlueprintBrain"), "Brain identity count changed.");
            Assertions.Equal(5, first.Count(value => value.PlannedType == "BlueprintItemWeapon"), "Weapon identity count changed.");
            Assertions.Equal(2, first.Count(value => value.PlannedType == "BlueprintAbilityResource"), "Resource identity count changed.");
            Assertions.Equal(string.Join("|", first.Select(value => value.Symbol)),
                string.Join("|", second.Select(value => value.Symbol)), "Identity output is not deterministic.");
        }

        internal static void LowTierNaturalProfilesAreExact()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            Assertions.Equal(7, ExpandedSummoningNaturalProfiles.All.Count,
                "Low-tier natural reconstruction count changed.");
            NaturalSummonProfile dog = ExpandedSummoningNaturalProfiles.For("dog");
            Assertions.Equal("Small", dog.Size, "Dog size changed.");
            Assertions.Equal(1, dog.HitDice, "Dog HD changed.");
            Assertions.Equal("Bite1d4", dog.PrimaryWeapon,
                "Dog bite profile changed.");
            NaturalSummonProfile eagle = ExpandedSummoningNaturalProfiles.For("eagle");
            Assertions.Equal(80, eagle.SpeedFeet, "Eagle flight speed changed.");
            Assertions.Equal(2, eagle.AdditionalWeapons.Count,
                "Eagle talon count changed.");
            NaturalSummonProfile frog = ExpandedSummoningNaturalProfiles.For(
                "poisonous-frog");
            Assertions.Equal(2, frog.Strength, "Poisonous Frog Strength changed.");
            Assertions.True(frog.Facts.Contains("PoisonFrog"),
                "Poisonous Frog lost its exact native poison graph.");
            NaturalSummonProfile centipede = ExpandedSummoningNaturalProfiles.For(
                "giant-centipede");
            Assertions.Equal("Vermin", centipede.HitDieClass,
                "Giant Centipede type class changed.");
            Assertions.True(centipede.Deviations.Any(value =>
                value.Contains("racial DC bonus")),
                "The conservative Centipede poison DC deviation is not explicit.");
            NaturalSummonProfile spider = ExpandedSummoningNaturalProfiles.For(
                "giant-spider");
            Assertions.Equal(3, spider.HitDice, "Giant Spider HD changed.");
            Assertions.Equal(1, spider.NaturalArmor,
                "Giant Spider natural armor changed.");
            Assertions.True(spider.Facts.Contains("GiantSpiderPoison"),
                "Giant Spider lost its exact native poison graph.");
            Assertions.True(ExpandedSummoningNaturalProfiles.For("goblin-dog")
                .Deviations.Any(value => value.Contains("allergic reaction")),
                "Goblin Dog allergic-reaction omission is not explicit.");
            Assertions.True(ExpandedSummoningNaturalProfiles.For("hyena")
                .Facts.Contains("TrippingBite"),
                "Hyena lost its tripping bite.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            foreach (string token in new[] { "ExpandedSummoningNaturalProfiles.All",
                "unit.ComponentsArray = new BlueprintComponent[] { levels }",
                "unit.Body = new BlueprintUnit.UnitBody",
                "unit.StartingInventory = Array.Empty<BlueprintItem>()",
                "new DiceFormula(1, dice)",
                "RequireExactUnitFactReference<",
                "1a3f2f384bbef804d8f52db1f9aa62d3",
                "6fed981bf0ef27a499969f369f35b5e8",
                "094714bb08f4e1943a8e9d2384ebe573" })
                Assertions.True(builder.Contains(token),
                    "Low-tier natural builder contract is missing: " + token);
            Assertions.False(builder.Contains("CR2_WorgStandart"),
                "The Worg donor must not supply Goblin Dog mechanics.");
            string registration = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningBlueprints.cs"));
            Assertions.True(registration.Contains(
                "ExpandedSummoningNaturalBuilder.Configure(library, registered)"),
                "The natural reconstruction builder is not registered.");
            string lookup = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "BlueprintLibraryLookup.cs"));
            foreach (string token in new[] { "RequireExactUnitFactReference<T>",
                ".OfType<BlueprintUnit>()", "ReferenceEquals(value, fact)",
                "distinct.Count != 1", "value.GetType() != typeof(T)" })
                Assertions.True(lookup.Contains(token),
                    "Referenced unit-fact lookup lost its exact contract: " + token);
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

        internal static void NativeReuseAndLanternProfilesAreExact()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal(24, ExpandedSummoningSpecialProfiles
                .NativeElementalKeys.Count, "Elemental native-reuse count changed.");
            Assertions.Equal(4, ExpandedSummoningSpecialProfiles
                .NativeMephitKeys.Count, "Mephit native-reuse count changed.");
            Assertions.Equal(2, ExpandedSummoningSpecialProfiles.LanternHitDice,
                "Lantern Archon HD changed.");
            Assertions.Equal(30, ExpandedSummoningSpecialProfiles
                .LanternRayRangeFeet, "Lantern light-ray range changed.");
            Assertions.Equal(2, ExpandedSummoningSpecialProfiles
                .LanternRayProjectiles, "Lantern light-ray count changed.");
            Assertions.Equal(6, ExpandedSummoningSpecialProfiles
                .LanternRayDieSides, "Lantern light-ray die changed.");
            Assertions.Equal(7, ExpandedSummoningSpecialProfiles
                .InvisibleStalkerHitDice, "Invisible Stalker HD changed.");
            Assertions.Equal(7, ExpandedSummoningSpecialProfiles
                .ShadowDemonHitDice, "Shadow Demon HD changed.");
            Assertions.Equal(17, ExpandedSummoningSpecialProfiles
                .ShadowDemonSpellResistance, "Shadow Demon SR changed.");
            Assertions.Equal(8, ExpandedSummoningSpecialProfiles
                .SalamanderHitDice, "Salamander HD changed.");
            Assertions.Equal(8, ExpandedSummoningSpecialProfiles
                .SuccubusHitDice, "Succubus HD changed.");
            Assertions.Equal(27, ExpandedSummoningSpecialProfiles
                .SuccubusCharisma, "Succubus Charisma changed.");
            Assertions.Equal(12, ExpandedSummoningSpecialProfiles
                .BebelithHitDice, "Bebelith HD changed.");
            Assertions.Equal(25, ExpandedSummoningSpecialProfiles
                .BebelithDismantleReflexDc, "Bebelith dismantle DC changed.");
            Assertions.True(ExpandedSummoningSpecialProfiles
                .ShouldAttemptBebelithDismantle(true, true, true, 1, false),
                "A second same-round claw hit against armor must attempt dismantle.");
            Assertions.False(ExpandedSummoningSpecialProfiles
                .ShouldAttemptBebelithDismantle(true, true, true, 0, false),
                "A first claw hit must not dismantle armor.");
            Assertions.False(ExpandedSummoningSpecialProfiles
                .ShouldAttemptBebelithDismantle(true, true, false, 1, false),
                "An unarmored target must not receive the adapted AC penalty.");
            Assertions.True(ExpandedSummoningSpecialProfiles
                .IsBebelithDemonHuntingTarget(true, 20),
                "Chaotic evil outsiders must receive the demon-hunting bonus.");
            Assertions.False(ExpandedSummoningSpecialProfiles
                .IsBebelithDemonHuntingTarget(false, 20),
                "A chaotic evil non-outsider must not be treated as a demon.");
            Assertions.Equal(4, ExpandedSummoningSpecialProfiles.PixieHitDice,
                "Pixie HD changed.");
            Assertions.Equal(16, ExpandedSummoningSpecialProfiles
                .PixieSleepArrowUses, "Pixie sleep-arrow uses changed.");
            Assertions.True(ExpandedSummoningSpecialProfiles
                .ShouldSpendPixieSleepArrow(true, true, 1),
                "A successful sleep-bow hit must spend one use.");
            Assertions.False(ExpandedSummoningSpecialProfiles
                .ShouldSpendPixieSleepArrow(true, false, 16),
                "A missed arrow must not spend a sleep-arrow use.");
            Assertions.False(ExpandedSummoningSpecialProfiles
                .ShouldSpendPixieSleepArrow(true, true, 0),
                "An exhausted Pixie must not apply another sleep arrow.");
            Assertions.Equal("24719a49b84c5cd43b894268d22d9c89",
                ExpandedSummoningDonorCatalog.For("lantern-archon").Guid,
                "Lantern visual donor changed.");
            Assertions.False(ExpandedSummoningDonorCatalog.For(
                "lantern-archon").DedicatedSummon,
                "Will-o'-Wisp must remain a visual-only donor.");

            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            foreach (string token in new[] { "LanternRayProjectiles",
                "DamageType.Direct", "DiceType.D6", "AbilityRange.Custom",
                "AuraOfMenaceBuffGuid", "DamageAlignment.Evil",
                "SavingThrowBonusAgainstDescriptor",
                "ArmorClassBonusAgainstAlignment", "unit.Body = new",
                "unit.Alignment = Alignment.LawfulGood",
                "brain.Actions = new BlueprintAiAction[] { ai }",
                "RequireExact<BlueprintFeature>",
                "RequireExact<BlueprintUnitFact>(library",
                "RequireExact<BlueprintBuff>(library",
                "ConfigureInvisibleStalker", "NaturalInvisibilityGuid",
                "ConfigureShadowDemon", "IncorporealGuid",
                "DamageEnergyType.Cold", "PhysicalDamageMaterial.ColdIron",
                "ShadowDemonCombatTraitsSymbol", "ConfigureSalamander",
                "SalamanderConstrictDice", "Feature(library, DrMagic10Guid",
                "ConfigureSuccubus", "Feature(library, AberrationTypeGuid",
                "RemoveBuffIfCasterIsMissing", "EnergyDrainType.Temporary",
                "SuccubusDominateRounds", "OnlyOnFirstHit = true",
                "ConfigureBebelith", "BebelithCombatComponent",
                "DamageAlignment.Good",
                "ConfigurePixie", "PixieSleepArrowComponent",
                "NativeSleepingBuffGuid", "NativeDanceBuffGuid",
                "PixieSleepArrowUses", "AbilityResourceLogic",
                "new DiceFormula(0, DiceType.Zero)" })
                Assertions.True(source.Contains(token),
                    "Lantern reconstruction contract is missing: " + token);
            foreach (string forbidden in new[] { "GreaterTeleport", "Gestalt",
                "WillOWispTouchGuid", "SummonMonster" })
                Assertions.False(source.Contains(forbidden),
                    "Lantern reconstruction retained a forbidden surface: " + forbidden);

            string registration = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningBlueprints.cs"));
            foreach (string token in new[] { "CreateAiCastSpellShell(identity.Symbol)",
                "CreateBrainShell(identity.Symbol)", "CreateWeaponShell(identity.Symbol)",
                "CreateResourceShell(identity.Symbol)",
                "result.name = InternalName(symbol)" })
                Assertions.True(registration.Contains(token),
                    "Special blueprint factory naming contract is missing: " + token);

            string observer = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningInventoryObserver.cs"));
            foreach (string token in new[] { "ExactSpecialMechanicGuids",
                "specialIndex", "special-index=", "Take(500)",
                "specialDetails", "special-detail=", "missing-details:",
                "invisiblestalker", "shadowdemon", "succubus",
                "salamander", "energydrain", "charmmonster", "tailslap",
                "constrict", "subtypedemon", "94b2838e8a492c44ebf89e7fe7a75a62",
                "c4a7f98d743bc784c9d4cf2105852c39",
                "04dcf5776f9d4315b27d1c0c7c2f3c46",
                "efc1e80fb41e06544be46604983806d6",
                "d7cbd2004ce66a042aeab2e95a3c5c61",
                "cce5bb72adc78f944b480e01efd3eaef",
                "c0f4e1c24c9cd334ca988ed1bd9d201f",
                "6cbb040023868574b992677885390f92",
                "0c852a2405dd9f14a8bbcfaf245ff823",
                "1a3f2f384bbef804d8f52db1f9aa62d3",
                "6fed981bf0ef27a499969f369f35b5e8",
                "094714bb08f4e1943a8e9d2384ebe573",
                "d12770f0432d6c94380b056b1e238e33",
                "625363a810f4d884dad551b26b3454d3",
                "56ec8788092b6314e8f3c1c502e8433", "longspear",
                "giantfrogpoisonous", "giantfrogpoison", "centipedepoison",
                "giantspiderpoison" })
                Assertions.True(observer.Contains(token),
                    "Bounded special-mechanic observer is missing: " + token);
            Assertions.False(observer.Contains(
                "foreach (BlueprintScriptableObject value in specialCandidates)"),
                "Broad discovery candidates must not all receive deep graph inspection.");

            string combat = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningSpecialCombatComponents.cs"));
            foreach (string token in new[] { "ITickEachRound",
                "RuleSavingThrow(evt.Target", "SavingThrowType.Reflex",
                "SavingThrowType.Will", "Body.Armor.HasArmor",
                "ReferenceEquals(evt.Weapon.Blueprint, SleepBow)",
                "Owner.Resources.Spend(SleepArrowResource, 1)",
                "TimeSpan.FromSeconds(6d" })
                Assertions.True(combat.Contains(token),
                    "Bebelith/Pixie bounded combat contract is missing: " + token);
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
                "expanded-summoning-starting-inventory",
                "ExpandedSummoningIsForbiddenReference(value)",
                "ExpandedSummoningIdentityCatalog.Build()",
                "value.name.StartsWith(\"KMG_Summoning_Special_\"",
                "KMG_Summoning_Special_LanternArchon_Defenses" })
                Assertions.True(runtime.Contains(token),
                    "Guarded isolation observer is missing: " + token);
            foreach (string token in new[] {
                "expanded-summoning-template-logical-choices",
                "expanded-summoning-celestial-executions",
                "expanded-summoning-fiendish-executions",
                "expanded-summoning-template-buffs",
                "expanded-summoning-smite-markers",
                "expanded-summoning-runtime-alignments",
                "expanded-summoning-native-action-isolation",
                "expanded-summoning-exact-donor-inventory",
                "expanded-summoning-special-mechanic-candidates",
                "expanded-summoning-invisible-stalker",
                "expanded-summoning-shadow-demon",
                "expanded-summoning-salamander",
                "expanded-summoning-succubus",
                "expanded-summoning-bebelith",
                "expanded-summoning-pixie",
                "ExpandedSummoningSpawnActionCount(value)" })
                Assertions.True(runtime.Contains(token),
                    "Guarded template observer is missing: " + token);
            string inventory = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningInventoryObserver.cs"));
            foreach (string token in new[] { "ExpandedSummoningDonorCatalog.All",
                "Distinct(StringComparer.Ordinal)", "component-graph=",
                "body-graph=", "view-graph=", "SpecialMechanicTerms",
                "special-detail=", "special-candidate-summary=",
                "BebelithPixieTerms", "bebelith-pixie-candidate=",
                "bebelith-pixie-candidate-summary=", "longbow",
                "acid splash" })
                Assertions.True(inventory.Contains(token),
                    "Exact donor graph inventory is missing: " + token);
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
