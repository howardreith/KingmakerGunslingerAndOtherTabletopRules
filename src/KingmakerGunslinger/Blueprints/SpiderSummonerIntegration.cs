using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Integrates the Races Unleashed Spider Summoner rows, once per process,
    /// on the coordinator's first update after every LoadDictionary postfix.
    /// Each row is checked against its exact known input, repaired in place
    /// behind private cloned actions so its identity and saved references hold,
    /// and checked again. Generated higher-tier rows are built only from a
    /// source the checks accepted; anything unverified keeps its original
    /// behavior and reports why.
    /// </summary>
    internal static class SpiderSummonerIntegration
    {
        private static bool _attempted;
        private static readonly Dictionary<string, SpiderPlacement> GeneratedRows =
            new Dictionary<string, SpiderPlacement>(StringComparer.Ordinal);

        internal static string Outcome { get; private set; } = "not-run";

        /// <summary>The published generated row's placement, or null. Only rows
        /// this integration built and published can be admitted.</summary>
        internal static SpiderPlacement PlacementOf(BlueprintAbility row)
        {
            SpiderPlacement placement;
            return row != null && GeneratedRows.TryGetValue(row.AssetGuid, out placement) ? placement : null;
        }

        internal static void Integrate(ModContext context, LibraryScriptableObject library, BlueprintManifest manifest,
            ExpandedSummoningBlueprintSet canonical, bool moduleActive)
        {
            if (_attempted) return;
            _attempted = true;
            try
            {
                SpiderSummonerCatalog.Validate();
                BlueprintFeature feature = Find<BlueprintFeature>(library, SummonForeignRowClassifier.SpiderSummonerFeatureGuid);
                if (feature == null) { Outcome = "absent:spider-summoner-feature-not-installed"; return; }
                var registry = new BlueprintRegistry(library, manifest, context.Logger);
                BlueprintBuff marker = registry.Register<BlueprintBuff>(SpiderSummonerCatalog.MarkerBuffSymbol, CreateMarker);
                var shells = new Dictionary<string, BlueprintAbility>(StringComparer.Ordinal);
                foreach (SpiderPlacement placement in SpiderSummonerCatalog.Generated)
                {
                    string symbol = placement.Symbol;
                    shells.Add(symbol, registry.Register<BlueprintAbility>(symbol, () => AlternateSummoningAdapterBuilder.CreateShell(symbol)));
                }
                if (!moduleActive || canonical == null)
                {
                    foreach (SpiderPlacement placement in SpiderSummonerCatalog.Generated)
                        Placeholder(shells[placement.Symbol], placement, "expanded-summoning-inactive");
                    Outcome = "registered-module-off";
                    return;
                }
                SpiderSummonerRuntime.Activate(feature, marker);
                Outcome = Apply(context, library, canonical, feature, shells);
                context.Logger.Info("summoning", "spider-summoner.integrated", Outcome);
            }
            catch (Exception exception)
            {
                Outcome = "failed:" + exception.GetType().Name + ":" + exception.Message;
                context.Logger.Failure("summoning", "spider-summoner.failed", "Spider Summoner integration failed closed; Races Unleashed rows keep their existing behavior.", exception);
            }
            finally
            {
                // A repaired or generated row must never be judged by a
                // classification cached before this pass.
                SummonForeignRowRuntime.InvalidateCache();
            }
        }

        private static string Apply(ModContext context, LibraryScriptableObject library, ExpandedSummoningBlueprintSet canonical,
            BlueprintFeature feature, Dictionary<string, BlueprintAbility> shells)
        {
            var problems = new List<string>();
            int repaired = 0, generated = 0;
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
                foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
                {
                    SpiderPlacement[] placements = SpiderSummonerCatalog.Placements(family).Where(value => value.Species == species && !value.IsOriginal).ToArray();
                    string reason;
                    if (!RepairOriginal(library, canonical, family, species, out reason))
                    {
                        problems.Add(family + ":" + species.Key + ":" + reason);
                        foreach (SpiderPlacement placement in placements) Placeholder(shells[placement.Symbol], placement, reason);
                        continue;
                    }
                    repaired++;
                    BlueprintUnit unit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library, species.UnitGuid, "spider summoner unit");
                    BlueprintAbility original = Find<BlueprintAbility>(library, SpiderSummonerCatalog.RowIdentity(family, species));
                    foreach (SpiderPlacement placement in placements)
                    {
                        BlueprintAbility row = shells[placement.Symbol];
                        try
                        {
                            BuildGenerated(library, canonical, row, placement, unit, feature, original.Icon);
                            ExpandedSummoningPublisher.AddLateOwnedRow(library, placement, row);
                            GeneratedRows[row.AssetGuid] = placement;
                            generated++;
                        }
                        catch (Exception exception)
                        {
                            GeneratedRows.Remove(row.AssetGuid);
                            problems.Add(placement.Symbol + ":" + exception.Message);
                            Placeholder(row, placement, "generation-failed");
                        }
                    }
                }
            return "integrated;repaired=" + repaired + ";generated=" + generated + ";problems=" + string.Join(",", problems.ToArray());
        }

        /// <summary>
        /// Repairs one original row in place when it is exactly the known
        /// input. The row's identity is unchanged; only its own component array
        /// moves to private cloned actions, so nothing shared with a vanilla
        /// donor or another row is touched.
        /// </summary>
        private static bool RepairOriginal(LibraryScriptableObject library, ExpandedSummoningBlueprintSet canonical,
            SummonFamily family, SpiderSpeciesSpec species, out string reason)
        {
            BlueprintAbility row = Find<BlueprintAbility>(library, SpiderSummonerCatalog.RowIdentity(family, species));
            if (row == null) { reason = "row-absent"; return false; }
            SummonForeignRowReading reading = SummonForeignRowRuntime.Read(row);
            SummonForeignRowResult before = SummonForeignRowClassifier.Classify(reading);
            if (before.Reason == "verified-repaired-spider-contract") { reason = null; return true; }
            string input = SummonForeignRowClassifier.LegacyInputProblem(reading);
            if (input != null) { reason = "unsupported-input:" + input; return false; }
            BlueprintUnit unit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library, species.UnitGuid, "spider summoner unit");
            BlueprintComponent[] components = row.ComponentsArray;
            AbilityEffectRunAction original = components.OfType<AbilityEffectRunAction>().Single();
            AbilityEffectRunAction run = (AbilityEffectRunAction)ExpandedSummoningAbilityBuilder.DeepCloneComponent(original);
            Conditional branch = run.Actions.Actions.OfType<Conditional>().SingleOrDefault();
            if (branch != null)
                run.Actions = new ActionList { Actions = new GameAction[] { branch.IfFalse.Actions.OfType<ContextActionSpawnMonster>().Single() } };
            if (ExpandedSummoningAbilityBuilder.ReplaceSpawnUnits(new BlueprintComponent[] { run }, unit) != 1)
                throw new InvalidOperationException("The repaired row must hold exactly one spawn: " + row.AssetGuid);
            ContextActionSpawnMonster spawn = run.Actions.Actions.OfType<ContextActionSpawnMonster>().Single();
            spawn.AfterSpawn.Actions = spawn.AfterSpawn.Actions.Where(value => !IsNativeTemplateStep(value)).ToArray();
            if (family == SummonFamily.Monster) AppendTemplate(new BlueprintComponent[] { run }, canonical, unit, library);
            else ExpandedSummoningAbilityBuilder.AppendAlignmentAction(new BlueprintComponent[] { run }, SummonAlignmentMode.Caster);
            ExpandedSummoningAbilityBuilder.AppendSpiderBenefit(new BlueprintComponent[] { run });
            // A tier I row copied after publication also inherits the parent's
            // own variants component and would present as a menu of its own.
            row.ComponentsArray = components.Where(value => !(value is AbilityVariants))
                .Select(value => ReferenceEquals(value, original) ? run : value).ToArray();
            AlternateSummoningAdapterBuilder.ResetComponentCaches(row);
            SummonForeignRowResult after = SummonForeignRowClassifier.Classify(SummonForeignRowRuntime.Read(row));
            if (after.Reason != "verified-repaired-spider-contract")
            {
                row.ComponentsArray = components;
                AlternateSummoningAdapterBuilder.ResetComponentCaches(row);
                reason = "repaired-shape-rejected:" + after.Reason;
                return false;
            }
            reason = null;
            return true;
        }

        private static bool IsNativeTemplateStep(GameAction action)
        {
            Conditional conditional = action as Conditional;
            if (conditional == null || conditional.GetType() != typeof(Conditional)) return false;
            IEnumerable<GameAction> branches = (conditional.IfTrue == null ? new GameAction[0] : conditional.IfTrue.Actions ?? new GameAction[0])
                .Concat(conditional.IfFalse == null ? new GameAction[0] : conditional.IfFalse.Actions ?? new GameAction[0]);
            return branches.Any() && branches.All(value =>
            {
                ContextActionApplyBuff apply = value as ContextActionApplyBuff;
                return apply != null && apply.Buff != null && ExpandedSummoningAbilityBuilder.NativeTemplateIds.Contains(apply.Buff.AssetGuid);
            });
        }

        private static void AppendTemplate(IEnumerable<BlueprintComponent> components, ExpandedSummoningBlueprintSet canonical, BlueprintUnit unit, LibraryScriptableObject library)
        {
            SummonTemplateBand band = SummonTemplateBandPolicy.Select(ExpandedSummoningAbilityBuilder.HitDiceOf(unit));
            Func<string, BlueprintBuff> buff = symbol => (BlueprintBuff)canonical.BySymbol[symbol];
            BlueprintBuff[] replaced = ExpandedSummoningAbilityBuilder.NativeTemplateIds.Select(guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, guid, "native summon template buff")).ToArray();
            ExpandedSummoningAbilityBuilder.AppendDynamicTemplate(components,
                buff("KMG.Summoning.Template.Celestial." + band), buff("KMG.Summoning.Template.Fiendish." + band),
                buff("KMG.Summoning.Smite.Celestial.Available"), buff("KMG.Summoning.Smite.Fiendish.Available"),
                buff("KMG.Summoning.AlignmentMode.FiendishMarker"), replaced);
        }

        private static void BuildGenerated(LibraryScriptableObject library, ExpandedSummoningBlueprintSet canonical, BlueprintAbility target,
            SpiderPlacement placement, BlueprintUnit unit, BlueprintFeature feature, Sprite icon)
        {
            SummonNativeOptionSpec donorSpec = SummonNativeOptionCatalog.All.Single(value => value.Family == placement.Family && value.Tier == placement.Tier && value.Multiplicity == placement.Quantity);
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, donorSpec.Guid, "native summon quantity donor");
            string symbol = placement.Symbol;
            ExpandedSummoningAbilityBuilder.CopyFields(donor, target);
            target.name = symbol.Replace('.', '_').Replace('-', '_');
            BlueprintComponent[] components = (donor.ComponentsArray ?? new BlueprintComponent[0]).Where(value => !(value is AbilityVariants) && !(value is AbilityCasterAlignment))
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            if (ExpandedSummoningAbilityBuilder.ReplaceSpawnUnits(components, unit) < 1)
                throw new InvalidOperationException("The quantity donor holds no spawn: " + donor.AssetGuid);
            if (placement.Family == SummonFamily.Monster) AppendTemplate(components, canonical, unit, library);
            else ExpandedSummoningAbilityBuilder.AppendAlignmentAction(components, SummonAlignmentMode.Caster);
            ExpandedSummoningAbilityBuilder.AppendSpiderBenefit(components);
            AbilityCasterHasFacts required = ScriptableObject.CreateInstance<AbilityCasterHasFacts>();
            required.Facts = new BlueprintUnitFact[] { feature };
            AbilityShowIfCasterHasFact visible = ScriptableObject.CreateInstance<AbilityShowIfCasterHasFact>();
            visible.UnitFact = feature;
            target.ComponentsArray = components.Concat(new BlueprintComponent[] { required, visible }).ToArray();
            if (placement.Family == SummonFamily.Monster)
                ExpandedSummoningAbilityBuilder.AttachClericAlignmentGate(target, placement.Species.Key, true);
            SpellDescriptorComponent descriptor = target.ComponentsArray.OfType<SpellDescriptorComponent>().FirstOrDefault();
            if (descriptor != null) descriptor.Descriptor &= ~(SpellDescriptor.Good | SpellDescriptor.Evil);
            target.Hidden = false;
            target.ActionBarAutoFillIgnored = false;
            target.Parent = null;
            target.MaterialComponent = donor.MaterialComponent ?? new BlueprintAbility.MaterialComponentData();
            string key = "KMG.Spider." + placement.StableKey;
            BlueprintUnitFactAccess.Resolve().Configure(target,
                LocalizationService.Create(key + ".Name", SummonChoicePresentationPolicy.For(placement.Species.Key, placement.Quantity).Label),
                LocalizationService.Create(key + ".Description", "Summons " + placement.Species.DisplayName + (placement.Quantity == SummonMultiplicity.One ? "" : " in greater numbers") +
                    " through the native summon lifecycle. Requires Spider Summoner."), icon);
            AlternateSummoningAdapterBuilder.ResetComponentCaches(target);
            SummonSpawn spawn = SummonSignatureReader.Read(target).Spawns().Single();
            if (spawn.Unit != placement.Species.UnitGuid || !QuantityMatches(spawn, placement.Quantity))
                throw new InvalidOperationException("Generated spider row failed its quantity contract: " + symbol);
        }

        private static bool QuantityMatches(SummonSpawn spawn, SummonMultiplicity quantity)
        {
            if (spawn.CountDiceCount == null || spawn.CountBonus == null || spawn.CountDiceCount.Kind != "Simple" || spawn.CountBonus.Kind != "Simple") return false;
            if (quantity == SummonMultiplicity.One) return spawn.CountDice == "Zero" && spawn.CountDiceCount.Value == 0 && spawn.CountBonus.Value == 1;
            if (quantity == SummonMultiplicity.OneD3) return spawn.CountDice == "D3" && spawn.CountDiceCount.Value == 1 && spawn.CountBonus.Value == 0;
            return spawn.CountDice == "D4" && spawn.CountDiceCount.Value == 1 && spawn.CountBonus.Value == 1;
        }

        private static BlueprintBuff CreateMarker()
        {
            BlueprintBuff marker = ScriptableObject.CreateInstance<BlueprintBuff>();
            marker.name = "KMG_Summoning_Spider_DcBonus";
            SpiderSummonerDcBonus bonus = ScriptableObject.CreateInstance<SpiderSummonerDcBonus>();
            bonus.name = "$KMG_SpiderSummoner_DcBonus";
            marker.ComponentsArray = new BlueprintComponent[] { bonus };
            marker.FxOnStart = new PrefabLink();
            marker.FxOnRemove = new PrefabLink();
            marker.ResourceAssetIds = new string[0];
            marker.Stacking = StackingType.Replace;
            FieldInfo flags = typeof(BlueprintBuff).GetField("m_Flags", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (flags == null || !flags.FieldType.IsEnum) throw new MissingFieldException(typeof(BlueprintBuff).FullName, "m_Flags");
            flags.SetValue(marker, Enum.Parse(flags.FieldType, "HiddenInUi"));
            BlueprintUnitFactAccess.Resolve().Configure(marker,
                LocalizationService.Create("KMG.Spider.DcBonus.Name", "Spider Summoner"),
                LocalizationService.Create("KMG.Spider.DcBonus.Description", "This summoned spider's poison and web save DCs are increased by 2."), null);
            return marker;
        }

        private static void Placeholder(BlueprintAbility ability, SpiderPlacement placement, string reason)
        {
            AlternateSummoningAdapterBuilder.MakePlaceholder(ability, null,
                LocalizationService.Create("KMG.Spider.Unavailable.Name", "Unavailable spider summon"),
                LocalizationService.Create("KMG.Spider.Unavailable.Description", "This spider summoning option is unavailable in this configuration. It cannot be cast."),
                placement.Symbol + ":" + reason);
        }

        private static T Find<T>(LibraryScriptableObject library, string guid) where T : BlueprintScriptableObject
        {
            BlueprintScriptableObject found;
            return library.BlueprintsByAssetId != null && library.BlueprintsByAssetId.TryGetValue(guid, out found) ? found as T : null;
        }
    }
}
