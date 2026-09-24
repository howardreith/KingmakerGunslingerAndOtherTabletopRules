using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.FavoredClass.Mechanics;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>The full and partial leaves of one target counter.</summary>
    internal sealed class FavoredClassLeafPair
    {
        internal FavoredClassLeafPair(FavoredClassEffectSpec effect, string targetKey,
            string hostClassGuid, BlueprintFeature full, BlueprintFeature partial)
        {
            Effect = effect;
            TargetKey = targetKey;
            HostClassGuid = hostClassGuid;
            Full = full;
            Partial = partial;
        }

        internal FavoredClassEffectSpec Effect { get; private set; }
        internal string TargetKey { get; private set; }

        /// <summary>The class whose host bonus selection receives this pair.</summary>
        internal string HostClassGuid { get; private set; }

        internal BlueprintFeature Full { get; private set; }

        /// <summary>Null for divisor-one effects.</summary>
        internal BlueprintFeature Partial { get; private set; }

        internal IEnumerable<BlueprintFeature> Leaves
        {
            get
            {
                if (Partial != null)
                    yield return Partial;
                yield return Full;
            }
        }
    }

    internal sealed class FavoredClassBlueprintSet
    {
        internal FavoredClassBlueprintSet(IList<FavoredClassLeafPair> pairs,
            string gunslingerClassGuid)
        {
            Pairs = pairs ?? throw new ArgumentNullException("pairs");
            GunslingerClassGuid = gunslingerClassGuid ??
                throw new ArgumentNullException("gunslingerClassGuid");
        }

        internal IList<FavoredClassLeafPair> Pairs { get; private set; }

        /// <summary>The KMG Gunslinger class identity the host scan must contain.</summary>
        internal string GunslingerClassGuid { get; private set; }

        internal int LeafCount
        {
            get { return Pairs.Sum(pair => pair.Leaves.Count()); }
        }
    }

    /// <summary>
    /// Registers every KMG-owned favored-class leaf with its committed
    /// manifest identity. Registration is unconditional and independent of
    /// the host, so saved investments always resolve; publication into the
    /// host's menus is a separate, gated first-update transaction.
    /// </summary>
    internal static class FavoredClassBlueprints
    {
        internal static FavoredClassBlueprintSet Register(BlueprintRegistry registry,
            GunslingerClassBlueprintSet gunslinger)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (gunslinger == null) throw new ArgumentNullException("gunslinger");
            var pairs = new List<FavoredClassLeafPair>();
            foreach (string effectId in FavoredClassLeafCatalog.ImplementedEffects)
            {
                FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
                IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.LeavesFor(effectId);
                FavoredClassLeafSpec fullSpec = leaves.Single(
                    leaf => leaf.Role == FavoredClassInvestmentRole.Full);
                FavoredClassLeafSpec partialSpec = leaves.SingleOrDefault(
                    leaf => leaf.Role == FavoredClassInvestmentRole.Partial);
                Sprite icon = IconFor(effect, gunslinger);
                BlueprintFeature partial = partialSpec == null ? null :
                    registry.Register<BlueprintFeature>(partialSpec.Symbol,
                        () => CreateLeaf(partialSpec, icon));
                BlueprintFeature full = registry.Register<BlueprintFeature>(fullSpec.Symbol,
                    () => CreateLeaf(fullSpec, icon));
                AttachPrerequisites(effect, full, partial);
                AttachMechanics(effect, full, partial, gunslinger);
                pairs.Add(new FavoredClassLeafPair(effect, null,
                    HostClassGuidFor(effect, gunslinger), full, partial));
            }
            FavoredClassBlueprintSet set = new FavoredClassBlueprintSet(pairs.AsReadOnly(),
                gunslinger.CharacterClass.AssetGuid);
            Validate(set);
            return set;
        }

        private static string HostClassGuidFor(FavoredClassEffectSpec effect,
            GunslingerClassBlueprintSet gunslinger)
        {
            if (effect.ClassFamily == FavoredClassCatalog.Gunslinger)
                return gunslinger.CharacterClass.AssetGuid;
            throw new InvalidOperationException("No verified host class for " + effect.Id);
        }

        private static Sprite IconFor(FavoredClassEffectSpec effect, GunslingerClassBlueprintSet gunslinger)
        {
            if (effect.Id == FavoredClassCatalog.EffectGrit)
                return gunslinger.Grit.Feature.Icon;
            return null;
        }

        private static BlueprintFeature CreateLeaf(FavoredClassLeafSpec spec, Sprite icon)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + spec.Symbol.Substring(FavoredClassLeafCatalog.SymbolPrefix.Length)
                .Replace('.', '_');
            feature.Ranks = spec.Ranks;
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.HideNotAvailibleInUI = true;
            feature.ReapplyOnLevelUp = false;
            feature.Groups = new FeatureGroup[0];
            feature.ComponentsArray = new BlueprintComponent[0];
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(spec.Symbol + ".Name", spec.Name),
                LocalizationService.Create(spec.Symbol + ".Description", spec.Description),
                icon);
            return feature;
        }

        private static void AttachPrerequisites(FavoredClassEffectSpec effect,
            BlueprintFeature full, BlueprintFeature partial)
        {
            full.ComponentsArray = full.ComponentsArray.Concat(new BlueprintComponent[]
            {
                Investment(effect, full, partial, false),
                Ancestry(effect, full)
            }).ToArray();
            if (partial != null)
                partial.ComponentsArray = partial.ComponentsArray.Concat(new BlueprintComponent[]
                {
                    Investment(effect, full, partial, true),
                    Ancestry(effect, partial)
                }).ToArray();
        }

        private static PrerequisiteFavoredClassInvestment Investment(FavoredClassEffectSpec effect,
            BlueprintFeature full, BlueprintFeature partial, bool partialRole)
        {
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteFavoredClassInvestment>();
            prerequisite.name = "$" + (partialRole ? partial : full).name + "_Investment";
            prerequisite.Full = full;
            prerequisite.Partial = partial;
            prerequisite.Divisor = effect.Rate.Divisor;
            prerequisite.CapSteps = effect.Rate.CapSteps ?? 0;
            prerequisite.PartialRole = partialRole;
            prerequisite.Group = Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite.GroupType.All;
            return prerequisite;
        }

        private static PrerequisiteFavoredClassAncestry Ancestry(FavoredClassEffectSpec effect,
            BlueprintFeature leaf)
        {
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteFavoredClassAncestry>();
            prerequisite.name = "$" + leaf.name + "_Ancestry";
            prerequisite.EffectId = effect.Id;
            prerequisite.Group = Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite.GroupType.All;
            return prerequisite;
        }

        private static void AttachMechanics(FavoredClassEffectSpec effect, BlueprintFeature full,
            BlueprintFeature partial, GunslingerClassBlueprintSet gunslinger)
        {
            if (effect.Id == FavoredClassCatalog.EffectGrit)
            {
                var grit = ScriptableObject.CreateInstance<FavoredClassGritResourceBonus>();
                grit.name = "$" + full.name + "_GritMaximum";
                grit.Resource = gunslinger.Grit.Resource;
                grit.Divisor = effect.Rate.Divisor;
                grit.CapSteps = effect.Rate.CapSteps ?? 0;
                full.ComponentsArray = full.ComponentsArray.Concat(
                    new BlueprintComponent[] { grit }).ToArray();
                return;
            }
            throw new InvalidOperationException("No mechanics are implemented for " + effect.Id);
        }

        private static void Validate(FavoredClassBlueprintSet set)
        {
            foreach (FavoredClassLeafPair pair in set.Pairs)
            {
                foreach (BlueprintFeature leaf in pair.Leaves)
                {
                    PrerequisiteFavoredClassInvestment[] investment = leaf.ComponentsArray
                        .OfType<PrerequisiteFavoredClassInvestment>().ToArray();
                    PrerequisiteFavoredClassAncestry[] ancestry = leaf.ComponentsArray
                        .OfType<PrerequisiteFavoredClassAncestry>().ToArray();
                    if (investment.Length != 1 || ancestry.Length != 1 ||
                        investment[0].Full != pair.Full || investment[0].Partial != pair.Partial ||
                        ancestry[0].EffectId != pair.Effect.Id || !leaf.HideNotAvailibleInUI ||
                        leaf.Ranks < 1)
                        throw new InvalidOperationException("Favored-class leaf graph is malformed: " +
                            leaf.name);
                }
            }
        }
    }
}
