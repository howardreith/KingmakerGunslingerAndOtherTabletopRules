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
            Assertions.Equal(1158, first.Count, "Foundation identity count changed.");
            Assertions.Equal(67, first.Count(value => value.PlannedType == "BlueprintUnit"), "Unit identity count changed.");
            Assertions.Equal(1050, first.Count(value => value.PlannedType == "BlueprintAbility"), "Ability identity count changed.");
            Assertions.Equal(2, first.Count(value => value.Symbol.StartsWith(
                "KMG.Summoning.Native.", StringComparison.Ordinal)),
                "Native tier-one preservation identity count changed.");
            Assertions.Equal(18, first.Count(value => value.PlannedType == "BlueprintBuff"), "Buff identity count changed.");
            Assertions.Equal(3, first.Count(value => value.PlannedType == "BlueprintAiCastSpell"), "AI identity count changed.");
            Assertions.Equal(3, first.Count(value => value.PlannedType == "BlueprintBrain"), "Brain identity count changed.");
            Assertions.Equal(10, first.Count(value => value.PlannedType == "BlueprintItemWeapon"), "Weapon identity count changed.");
            Assertions.Equal(2, first.Count(value => value.PlannedType == "BlueprintWeaponType"), "Weapon-type identity count changed.");
            Assertions.Equal(2, first.Count(value => value.PlannedType == "BlueprintAbilityResource"), "Resource identity count changed.");
            Assertions.Equal(2, first.Count(value => value.PlannedType == "BlueprintFeature"), "Feature identity count changed.");
            Assertions.Equal(1, first.Count(value => value.PlannedType ==
                "BlueprintActivatableAbility"),
                "Neutral alignment-mode toggle identity count changed.");
            Assertions.Equal(string.Join("|", first.Select(value => value.Symbol)),
                string.Join("|", second.Select(value => value.Symbol)), "Identity output is not deterministic.");
        }

        internal static void PlayerPathHarnessUsesRealSpellbookParents()
        {
            string root = Environment.CurrentDirectory;
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRequest.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            foreach (string token in new[] {
                "disposable-expanded-summoning-player-path",
                "RunDisposableExpandedSummoningPlayerPath",
                "PrepareExpandedSummoningPlayerPathSpell",
                "new AbilityData(slot.Spell, selected)",
                "result.ParamSpellSlot = slot",
                "CountAvailableExpandedSummoningSlots",
                "Game.Instance.EntityCreator.Tick()",
                "ContainsReference(sceneEntities, value)",
                "ContainsReference(allUnits, value)",
                "ExpandedSummoningPlayerPathRuleCastPostfix",
                "ExpandedSummoningPlayerPathSpawnPrefix",
                "kmg-dog-celestial-direct",
                "kmg-sna1-dog-logical",
                "kmg-sm6-erinyes",
                "kmg-sm1-dog-neutral-default",
                "kmg-sm1-dog-neutral-fiendish-mode",
                "kmg-sm7-roc", "kmg-sm6-dire-tiger",
                "kmg-sm3-dog-1d4plus1",
                "native-sm8-movanic-deva",
                "native-sm8-frost-giant",
                "evidence.TemplateContract",
                "evidence.RenderableContract",
                "expanded-summoning-all-logical-player-paths",
                "ExerciseRejectedExpandedSummoningPlayerPathCase",
                "expanded-summoning-invalid-cast-slot-preservation",
                "kmg-sm1-dog-cancelled-before-range",
                "canonical Summon Monster I parent for Acadamae fixture",
                "ExpandedSummoningIdentityCatalog.AbilitySymbol(dog)",
                "broadCases.Count == 681",
                "result.QuantityContract" })
                Assertions.True(runner.Contains(token) || catalog.Contains(token) ||
                    request.Contains(token) || automation.Contains(token),
                    "Player-path acceptance contract is missing: " + token);
            Assertions.True(request.Contains(
                    "RuntimeTestScenarioCatalog.DisposableExpandedSummoningPlayerPath") &&
                automation.Contains("RequiresSaveName = $true") &&
                automation.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'"),
                "Player-path scenario must use only the guarded working save.");
            Assertions.True(runner.Contains(
                    "evidence.SlotsAfter == evidence.SlotsBefore - 1") &&
                runner.Contains("evidence.SlotsAfter == evidence.SlotsBefore"),
                "Player-path evidence must distinguish spellbook slot spend from direct-child controls.");
        }

        internal static void LowTierNaturalProfilesAreExact()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            Assertions.Equal(26, ExpandedSummoningNaturalProfiles.All.Count,
                "Tier I-VII natural reconstruction count changed.");
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
                "new DiceFormula(rolls, dice)",
                "facts.Add(extraplanar)",
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
                "ExpandedSummoningNaturalBuilder.Configure(library, registered,"),
                "The natural reconstruction builder is not registered.");
            foreach (string token in new[] {
                "KMG.Summoning.Subtype.Extraplanar",
                "ConfigureExtraplanar(extraplanar)",
                "feature.HideInUI = true",
                "ApplyExtraplanarMarker(registered, extraplanar)",
                "ExpandedSummoningCatalog.All" })
                Assertions.True(registration.Contains(token),
                    "The local extraplanar marker contract is missing: " + token);
        }

        internal static void TierThreeFourNaturalProfilesAreExact()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            string[] expected = { "boar", "leopard", "monitor-lizard",
                "cheetah", "crocodile", "dire-bat", "wolverine",
                "dire-boar", "dire-wolf", "grizzly-bear", "lion",
                "pteranodon" };
            Assertions.Equal(12, expected.Count(key =>
                ExpandedSummoningNaturalProfiles.All.Any(value =>
                    value.Key == key)),
                "Tier III-IV natural profile coverage changed.");
            NaturalSummonProfile leopard =
                ExpandedSummoningNaturalProfiles.For("leopard");
            Assertions.Equal(4, leopard.AdditionalWeapons.Count,
                "Leopard rake limb representation changed.");
            Assertions.True(leopard.Facts.Contains("Pounce"),
                "Leopard lost native pounce.");
            NaturalSummonProfile monitor =
                ExpandedSummoningNaturalProfiles.For("monitor-lizard");
            Assertions.Equal("Bite1d8", monitor.PrimaryWeapon,
                "Monitor Lizard bite changed.");
            Assertions.True(monitor.Facts.Contains("MonitorLizardPoison"),
                "Monitor Lizard lost its native poison graph.");
            NaturalSummonProfile crocodile =
                ExpandedSummoningNaturalProfiles.For("crocodile");
            Assertions.Equal(1, crocodile.AdditionalSecondaryWeapons.Count,
                "Crocodile secondary tail count changed.");
            Assertions.Equal("Tail1d12",
                crocodile.AdditionalSecondaryWeapons.Single(),
                "Crocodile tail dice contract changed.");
            Assertions.True(crocodile.Deviations.Any(value =>
                value.Contains("death roll")),
                "Crocodile death-roll deviation is not explicit.");
            NaturalSummonProfile wolverine =
                ExpandedSummoningNaturalProfiles.For("wolverine");
            Assertions.Equal(1, wolverine.AdditionalWeapons.Count,
                "Wolverine second claw changed.");
            Assertions.Equal("Bite1d4",
                wolverine.AdditionalSecondaryWeapons.Single(),
                "Wolverine secondary bite changed.");
            Assertions.True(wolverine.Deviations.Any(value =>
                value.Contains("rage")),
                "Wolverine rage deviation is not explicit.");
            NaturalSummonProfile direBoar =
                ExpandedSummoningNaturalProfiles.For("dire-boar");
            Assertions.Equal(5, direBoar.HitDice, "Dire Boar HD changed.");
            Assertions.Equal(23, direBoar.Strength,
                "Dire Boar Strength changed.");
            Assertions.True(direBoar.Facts.Contains("Ferocity"),
                "Dire Boar lost ferocity.");
            NaturalSummonProfile direWolf =
                ExpandedSummoningNaturalProfiles.For("dire-wolf");
            Assertions.Equal(19, direWolf.Strength,
                "Dire Wolf Strength changed.");
            Assertions.True(direWolf.Facts.Contains("TrippingBite"),
                "Dire Wolf lost trip.");
            NaturalSummonProfile lion =
                ExpandedSummoningNaturalProfiles.For("lion");
            Assertions.Equal(5, lion.HitDice, "Lion HD changed.");
            Assertions.Equal(4, lion.AdditionalWeapons.Count,
                "Lion rake limb representation changed.");
            NaturalSummonProfile pteranodon =
                ExpandedSummoningNaturalProfiles.For("pteranodon");
            Assertions.Equal(5, pteranodon.HitDice,
                "Pteranodon HD changed.");
            Assertions.Equal(50, pteranodon.SpeedFeet,
                "Pteranodon fly speed changed.");
            Assertions.Equal("Bite2d6", pteranodon.PrimaryWeapon,
                "Pteranodon bite changed.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            foreach (string token in new[] {
                "KMG.Summoning.Natural.Tail1d12",
                "AdditionalSecondaryLimbs = secondary",
                "c988aa874d11ff84d873508ddc9b928f",
                "d2f99947db522e24293a7ec4eded453f",
                "73ed4e955295e62469fe471f1d49d9ef",
                "d1f80b5c5c73cc84db7854774850b08c",
                "d88236a83413baa45ae9c8e5ddce5a6c",
                "955e356c813de1743a98ab3485d5bc69",
                "1a8149c09e0bdfc48a305ee6ac3729a8",
                "BaseUnitFactKeys.Contains(fact)" })
                Assertions.True(builder.Contains(token),
                    "Tier III-IV natural builder contract is missing: " +
                    token);
        }

        internal static void TierFiveSevenNaturalProfilesAreExact()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            string[] expected = { "dire-lion", "ankylosaurus", "dire-bear",
                "dire-tiger", "elephant", "mastodon", "roc" };
            Assertions.Equal(expected.Length, expected.Count(key =>
                ExpandedSummoningNaturalProfiles.All.Any(value =>
                    value.Key == key)),
                "Tier V-VII natural profile coverage changed.");
            NaturalSummonProfile direLion =
                ExpandedSummoningNaturalProfiles.For("dire-lion");
            Assertions.Equal(8, direLion.HitDice, "Dire Lion HD changed.");
            Assertions.Equal(2, direLion.AdditionalWeapons.Count,
                "Dire Lion primary claw count changed.");
            Assertions.Equal(2, direLion.AdditionalSecondaryWeapons.Count,
                "Dire Lion rake count changed.");
            NaturalSummonProfile ankylosaurus =
                ExpandedSummoningNaturalProfiles.For("ankylosaurus");
            Assertions.Equal("Huge", ankylosaurus.Size,
                "Ankylosaurus size changed.");
            Assertions.Equal(14, ankylosaurus.NaturalArmor,
                "Ankylosaurus natural armor changed.");
            Assertions.Equal("Tail3d6", ankylosaurus.PrimaryWeapon,
                "Ankylosaurus tail profile changed.");
            Assertions.True(ankylosaurus.Deviations.Any(value =>
                value.Contains("daze/stun")),
                "Ankylosaurus control-rider deviation is not explicit.");
            NaturalSummonProfile direBear =
                ExpandedSummoningNaturalProfiles.For("dire-bear");
            Assertions.Equal(10, direBear.HitDice, "Dire Bear HD changed.");
            Assertions.Equal(21, direBear.Constitution,
                "Dire Bear Constitution changed.");
            NaturalSummonProfile direTiger =
                ExpandedSummoningNaturalProfiles.For("dire-tiger");
            Assertions.Equal(14, direTiger.HitDice, "Dire Tiger HD changed.");
            Assertions.Equal("BiteLarge2d6", direTiger.PrimaryWeapon,
                "Dire Tiger bite profile changed.");
            Assertions.Equal(2, direTiger.AdditionalSecondaryWeapons.Count,
                "Dire Tiger rake count changed.");
            NaturalSummonProfile elephant =
                ExpandedSummoningNaturalProfiles.For("elephant");
            Assertions.Equal(11, elephant.HitDice, "Elephant HD changed.");
            Assertions.Equal("Slam2d6",
                elephant.AdditionalSecondaryWeapons.Single(),
                "Elephant slam profile changed.");
            Assertions.True(elephant.Deviations.Any(value =>
                value.Contains("Trample")),
                "Elephant trample deviation is not explicit.");
            NaturalSummonProfile mastodon =
                ExpandedSummoningNaturalProfiles.For("mastodon");
            Assertions.Equal(14, mastodon.HitDice, "Mastodon HD changed.");
            Assertions.Equal(34, mastodon.Strength,
                "Mastodon Strength changed.");
            NaturalSummonProfile roc = ExpandedSummoningNaturalProfiles.For("roc");
            Assertions.Equal("Gargantuan", roc.Size, "Roc size changed.");
            Assertions.Equal(80, roc.SpeedFeet, "Roc flight speed changed.");
            Assertions.Equal("Bite2d8", roc.PrimaryWeapon,
                "Roc bite profile changed.");
            Assertions.True(roc.AdditionalWeapons.All(value =>
                value == "Talon2d6"), "Roc talons changed.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            foreach (string token in new[] {
                "KMG.Summoning.Natural.Tail3d6",
                "KMG.Summoning.Natural.Bite2d8",
                "KMG.Summoning.Natural.Talon2d6",
                "de42c58801037b84c9d992634ddd7220",
                "c2ce7bc3559b2024ea91ddf5bb321f0a",
                "209a2920891b580418b4e5e80466e134",
                "153937f44fcd42a429a286a10babd82d",
                "76a335b7d69691c4e8376f9379338778" })
                Assertions.True(builder.Contains(token),
                    "Tier V-VII natural builder contract is missing: " +
                    token);
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
            Assertions.Equal(SummonAlignmentMode.Celestial,
                SummonTemplateSelectionPolicy.Select(10, true),
                "Good casters must ignore the neutral mode.");
            Assertions.Equal(SummonAlignmentMode.Fiendish,
                SummonTemplateSelectionPolicy.Select(12, false),
                "Evil casters must ignore the neutral mode.");
            Assertions.Equal(SummonAlignmentMode.Celestial,
                SummonTemplateSelectionPolicy.Select(1, false),
                "Neutral mode must deterministically default to celestial.");
            Assertions.Equal(SummonAlignmentMode.Fiendish,
                SummonTemplateSelectionPolicy.Select(1, true),
                "Neutral casters must be able to select fiendish.");

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
            Assertions.Equal("676f8b7d0a170674cb6e504e0e30b4f0",
                ExpandedSummoningDonorCatalog.For("invisible-stalker").Guid,
                "Invisible Stalker must use the Medium Air Elemental view.");
            Assertions.Equal("2e24256e459468743b91fbb9aa85e1ab",
                ExpandedSummoningDonorCatalog.For("huge-air-elemental").Guid,
                "The real Huge Air Elemental donor must remain unchanged.");
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
            Assertions.Equal(9, ExpandedSummoningSpecialProfiles.ErinyesHitDice,
                "Erinyes outsider HD changed.");
            Assertions.Equal(23, ExpandedSummoningSpecialProfiles
                .ErinyesDexterity, "Erinyes Dexterity changed.");
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
                "OptionalExact<BlueprintBuff>",
                "if (optionalAura != null)",
                "SavingThrowBonusAgainstDescriptor",
                "ArmorClassBonusAgainstAlignment", "unit.Body = new",
                "unit.Alignment = Alignment.LawfulGood",
                "brain.Actions = new BlueprintAiAction[] { ai }",
                "RequireExact<BlueprintFeature>",
                "RequireExact<BlueprintUnitFact>(library",
                "RequireExact<BlueprintBuff>(library",
                "ConfigureInvisibleStalker", "NaturalInvisibilityGuid",
                "ConfigureErinyes", "ErinyesHitDice",
                "ranged == null || !ranged.IsRanged",
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
                "NativeSleepingBuffGuid", "PixieDanceStateSymbol",
                "UnitCondition.CantAct", "reflex.Value = -10",
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
            foreach (string token in new[] { "SummonDisplayOrderPolicy.Order",
                "OriginalComponents", "PublishedComponents",
                "rollback refused after unrelated mutation",
                "nativePreservation", "new[] { nativePreservation }",
                "Direct summon publication requires exactly one frozen native-preservation child",
                "SummonNativeOptionCatalog.Find",
                "Native duplicate map did not resolve exactly one KMG option",
                "preservedOriginals.Any(original =>" })
                Assertions.True(source.Contains(token),
                    "Runtime publication contract is missing: " + token);
            Assertions.False(source.Contains("SummonElemental"),
                "Expanded Summoning publication must not target the standalone Summon Elemental spell.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningAbilityBuilder.cs"));
            foreach (string token in new[] {
                "ConfigureNativeTierOnePreservation",
                "NativeMonsterTierOneSymbol", "NativeNaturesAllyTierOneSymbol",
                "Native tier-one preservation requires a direct ability",
                ".Select(DeepCloneComponent).ToArray()" })
                Assertions.True(builder.Contains(token),
                    "Native tier-one preservation builder contract is missing: " +
                    token);
        }

        internal static void RuntimeUnitComponentsAreReferenceIsolated()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningBlueprints.cs"));
            foreach (string token in new[] { "DeepCloneUnitComponent",
                "FormatterServices.GetUninitializedObject",
                "ScriptableObject.CreateInstance(type)",
                "source is BlueprintScriptableObject", "ReferenceComparer.Instance",
                "result.FxOnStart = new Kingmaker.ResourceLinks.PrefabLink()",
                "result.FxOnRemove = new Kingmaker.ResourceLinks.PrefabLink()" })
                Assertions.True(source.Contains(token),
                    "Unit component isolation contract is missing: " + token);
            string special = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(special.Contains(
                    "UnitAnimationActionCastSpell.CastAnimationStyle.Immediate"),
                "Lantern Archon must use the native animationless ray fallback.");
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
                "expanded-summoning-native-ability-contracts",
                "expanded-summoning-menu-reconciliation",
                "expanded-summoning-menu-order-and-icons",
                "expanded-summoning-menu-counts",
                "expanded-summoning-high-tier-native-choices",
                "AuditExpandedSummoningMenus",
                "VisibleMappedNativeDuplicates",
                "CategoryIconsDistinct",
                "expanded-summoning-standalone-summon-elemental",
                "standaloneElementalCandidates",
                "expectedContractNodes == 681",
                "ExpandedSummoningTemplateByCasterCount(value)",
                "SummonNativeOptionCatalog.All.Single",
                "acadamaeClassifiedNodes == expectedContractNodes",
                "metamagicContractNodes == expectedContractNodes",
                "ExpandedSummoningNativeAbilityContractExact",
                "expanded-summoning-exact-donor-inventory",
                "expanded-summoning-special-mechanic-candidates",
                "expanded-summoning-invisible-stalker",
                "expanded-summoning-erinyes",
                "expanded-summoning-shadow-demon",
                "expanded-summoning-salamander",
                "expanded-summoning-succubus",
                "expanded-summoning-bebelith",
                "expanded-summoning-pixie",
                "expanded-summoning-visual-instances",
                "expanded-summoning-renderable-geometry",
                "expanded-summoning-bounded-footprints",
                "expanded-summoning-selection-navigation",
                "expanded-summoning-locomotion-events",
                "expanded-summoning-attack-animations",
                "expanded-summoning-hit-and-death",
                "expanded-summoning-projectile-origins",
                "expanded-summoning-view-cleanup",
                "WorkingSaveExpandedSummoningPrepare",
                "WorkingSaveExpandedSummoningVerifyCleanup",
                "WorkingSaveExpandedSummoningVerifyAbsent",
                "ExpandedSummoningPersistentUnits",
                "RequiredBasePublicationIsExact",
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
                "acid splash", "PolishCandidateTerms",
                "polish-unit-candidate=", "polish-icon-candidate=",
                "polish-candidate-summary=", "value.PortraitSafe.SmallPortrait",
                "ObjectGraph(FieldValue(value, \"Visual\"), 8)" })
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
            foreach (string token in new[] { "ConfigureDynamicTemplate",
                "AbilityCasterAlignment", "(AlignmentMaskType)63",
                "(AlignmentMaskType)504", "CreateInstance<",
                "ContextActionApplySummonBuff>();",
                "ContextActionApplySummonTemplateByCaster>();",
                "NativeMonsterTemplateBuffs",
                "ReplacedNativeTemplateBuffs",
                "SpellDescriptor.Good", "SpellDescriptor.Evil" })
                Assertions.True(abilities.Contains(token),
                    "Template execution contract is missing: " + token);
            Assertions.False(abilities.Contains(
                    "choices.Variants = new[] { celestial, fiendish }"),
                "Player-facing logical roots must not retain nested template variants.");
            foreach (string token in new[] { "SummonTemplateBandPolicy.Select",
                "KMG.Summoning.Template.Celestial.\" + band",
                "KMG.Summoning.Template.Fiendish.\" + band" })
                Assertions.True(abilities.Contains(token),
                    "Template HD-band contract is missing: " + token);
            foreach (string token in new[] {
                "KMG.Summoning.Smite.Celestial.Available",
                "KMG.Summoning.Smite.Fiendish.Available",
                "AppendTemplateBuff(target.ComponentsArray, smiteBuff," })
                Assertions.True(abilities.Contains(token),
                    "Bounded template smite publication is missing: " + token);
        }
    }
}
