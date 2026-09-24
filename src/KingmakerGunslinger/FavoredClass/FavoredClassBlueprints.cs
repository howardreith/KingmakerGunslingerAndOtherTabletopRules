using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
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

        /// <summary>The counter of an effect and target (null target for untargeted effects).</summary>
        internal FavoredClassLeafPair Pair(string effectId, string targetKey)
        {
            return Pairs.FirstOrDefault(pair =>
                string.Equals(pair.Effect.Id, effectId, StringComparison.Ordinal) &&
                string.Equals(pair.TargetKey, targetKey, StringComparison.Ordinal));
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
        /// <summary>Kingmaker's Critical Focus feat (nonstacking comparison for confirmation).</summary>
        internal const string CriticalFocusGuid = "8ac59959b1b23c347a0361dc97cc786d";

        internal static FavoredClassBlueprintSet Register(BlueprintRegistry registry,
            LibraryScriptableObject library, GunslingerClassBlueprintSet gunslinger,
            ProductionFirearmBlueprintCatalog firearms)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (library == null) throw new ArgumentNullException("library");
            if (gunslinger == null) throw new ArgumentNullException("gunslinger");
            if (firearms == null) throw new ArgumentNullException("firearms");
            var pairs = new List<FavoredClassLeafPair>();
            foreach (string effectId in FavoredClassLeafCatalog.ImplementedEffects)
            {
                FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
                IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.LeavesFor(effectId);
                foreach (string targetKey in FavoredClassLeafCatalog.TargetKeys(effectId))
                {
                    Sprite icon = IconFor(effect, targetKey, gunslinger, firearms);
                    FavoredClassLeafSpec fullSpec = leaves.Single(leaf =>
                        leaf.Role == FavoredClassInvestmentRole.Full &&
                        string.Equals(leaf.TargetKey, targetKey, StringComparison.Ordinal));
                    FavoredClassLeafSpec partialSpec = leaves.SingleOrDefault(leaf =>
                        leaf.Role == FavoredClassInvestmentRole.Partial &&
                        string.Equals(leaf.TargetKey, targetKey, StringComparison.Ordinal));
                    BlueprintFeature partial = partialSpec == null ? null :
                        registry.Register<BlueprintFeature>(partialSpec.Symbol,
                            () => CreateLeaf(partialSpec, icon));
                    BlueprintFeature full = registry.Register<BlueprintFeature>(fullSpec.Symbol,
                        () => CreateLeaf(fullSpec, icon));
                    AttachPrerequisites(effect, full, partial, gunslinger);
                    AttachMechanics(effect, full, library, gunslinger);
                    pairs.Add(new FavoredClassLeafPair(effect, targetKey,
                        HostClassGuidFor(effect, gunslinger), full, partial));
                }
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

        /// <summary>
        /// Each leaf shows the icon of the exact KMG feature or firearm type it
        /// improves (the host's own favored-class leaves likewise reuse related
        /// feature art); null renders the native monogram. Read after the
        /// project icon stage has assigned those icons.
        /// </summary>
        private static Sprite IconFor(FavoredClassEffectSpec effect, string targetKey,
            GunslingerClassBlueprintSet gunslinger, ProductionFirearmBlueprintCatalog firearms)
        {
            switch (effect.Id)
            {
                case FavoredClassCatalog.EffectMisfire:
                    switch (targetKey)
                    {
                        case "Pistol": return firearms.Pistol.Item.Icon;
                        case "Musket": return firearms.Musket.Item.Icon;
                        case "Blunderbuss": return firearms.Blunderbuss.Item.Icon;
                        default: throw new InvalidOperationException("Unknown firearm target " + targetKey);
                    }
                case FavoredClassCatalog.EffectGrit:
                    return gunslinger.Grit.Feature.Icon;
                case FavoredClassCatalog.EffectPistolWhip:
                    return gunslinger.PistolWhip.Feature.Icon;
                case FavoredClassCatalog.EffectHalflingDodge:
                    return gunslinger.Dodge.Feature.Icon;
                case FavoredClassCatalog.EffectInitiative:
                    return gunslinger.Initiative.Icon;
                case FavoredClassCatalog.EffectHalflingNimble:
                case FavoredClassCatalog.EffectDrowNimble:
                    return gunslinger.Nimble.Features[0].Icon;
                default:
                    return null;
            }
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
            // Nimble's bonus depends on Nimble itself, which a later level can
            // grant; reapplying on level-up re-evaluates the owned modifier.
            feature.ReapplyOnLevelUp = spec.Role == FavoredClassInvestmentRole.Full &&
                (spec.EffectId == FavoredClassCatalog.EffectHalflingNimble ||
                    spec.EffectId == FavoredClassCatalog.EffectDrowNimble);
            feature.Groups = new FeatureGroup[0];
            feature.ComponentsArray = new BlueprintComponent[0];
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(spec.Symbol + ".Name", spec.Name),
                LocalizationService.Create(spec.Symbol + ".Description", spec.Description),
                icon);
            return feature;
        }

        private static void AttachPrerequisites(FavoredClassEffectSpec effect,
            BlueprintFeature full, BlueprintFeature partial, GunslingerClassBlueprintSet gunslinger)
        {
            BlueprintArchetype replacing = ReplacingArchetype(effect, gunslinger);
            foreach (BlueprintFeature leaf in new[] { partial, full })
            {
                if (leaf == null)
                    continue;
                var components = new List<BlueprintComponent>
                {
                    Investment(effect, full, partial, ReferenceEquals(leaf, partial)),
                    Ancestry(effect, leaf)
                };
                if (replacing != null)
                    components.Add(NoArchetype(leaf, gunslinger.CharacterClass, replacing));
                leaf.ComponentsArray = leaf.ComponentsArray.Concat(components).ToArray();
            }
        }

        /// <summary>
        /// The supported archetype that permanently replaces the improved
        /// feature, so its favored-class investment would be a broken no-op.
        /// Earlier (dormant) investment is otherwise allowed.
        /// </summary>
        internal static BlueprintArchetype ReplacingArchetype(FavoredClassEffectSpec effect,
            GunslingerClassBlueprintSet gunslinger)
        {
            switch (effect.Id)
            {
                case FavoredClassCatalog.EffectHalflingNimble:
                case FavoredClassCatalog.EffectDrowNimble:
                    return gunslinger.MysteriousStranger == null ? null :
                        gunslinger.MysteriousStranger.Archetype;
                case FavoredClassCatalog.EffectHalflingDodge:
                    return gunslinger.MusketMaster == null ? null :
                        gunslinger.MusketMaster.Archetype;
                default:
                    return null;
            }
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
            prerequisite.Group = Prerequisite.GroupType.All;
            return prerequisite;
        }

        private static PrerequisiteFavoredClassAncestry Ancestry(FavoredClassEffectSpec effect,
            BlueprintFeature leaf)
        {
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteFavoredClassAncestry>();
            prerequisite.name = "$" + leaf.name + "_Ancestry";
            prerequisite.EffectId = effect.Id;
            prerequisite.Group = Prerequisite.GroupType.All;
            return prerequisite;
        }

        private static PrerequisiteNoArchetype NoArchetype(BlueprintFeature leaf,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype)
        {
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteNoArchetype>();
            prerequisite.name = "$" + leaf.name + "_NoArchetype";
            prerequisite.CharacterClass = characterClass;
            prerequisite.Archetype = archetype;
            prerequisite.Group = Prerequisite.GroupType.All;
            return prerequisite;
        }

        private static void AttachMechanics(FavoredClassEffectSpec effect, BlueprintFeature full,
            LibraryScriptableObject library, GunslingerClassBlueprintSet gunslinger)
        {
            BlueprintComponent mechanics = CreateMechanics(effect, full, library, gunslinger);
            if (mechanics != null)
                full.ComponentsArray = full.ComponentsArray.Concat(
                    new[] { mechanics }).ToArray();
        }

        /// <summary>
        /// The owned component on the full leaf, or null where the effect is
        /// applied inside an existing authoritative calculation that reads the
        /// full rank through <see cref="FavoredClassEarnedSteps"/> (misfire
        /// threshold, Gunslinger's Dodge, Gunslinger Initiative).
        /// </summary>
        private static BlueprintComponent CreateMechanics(FavoredClassEffectSpec effect,
            BlueprintFeature full, LibraryScriptableObject library,
            GunslingerClassBlueprintSet gunslinger)
        {
            int divisor = effect.Rate.Divisor;
            int cap = effect.Rate.CapSteps ?? 0;
            switch (effect.Id)
            {
                case FavoredClassCatalog.EffectGrit:
                {
                    var grit = ScriptableObject.CreateInstance<FavoredClassGritResourceBonus>();
                    grit.name = "$" + full.name + "_GritMaximum";
                    grit.Resource = gunslinger.Grit.Resource;
                    grit.Divisor = divisor;
                    grit.CapSteps = cap;
                    return grit;
                }
                case FavoredClassCatalog.EffectFirearmConfirmation:
                {
                    var confirmation = ScriptableObject.CreateInstance<FavoredClassFirearmConfirmationBonus>();
                    confirmation.name = "$" + full.name + "_Confirmation";
                    confirmation.Divisor = divisor;
                    confirmation.CapSteps = cap;
                    confirmation.CriticalFocus = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                        library, CriticalFocusGuid, "native Critical Focus");
                    return confirmation;
                }
                case FavoredClassCatalog.EffectPistolWhip:
                {
                    var pistolWhip = ScriptableObject.CreateInstance<FavoredClassPistolWhipAttackBonus>();
                    pistolWhip.name = "$" + full.name + "_AttackBonus";
                    pistolWhip.Divisor = divisor;
                    pistolWhip.CapSteps = cap;
                    pistolWhip.OneHandedSurrogate = gunslinger.PistolWhip.OneHandedItem;
                    pistolWhip.TwoHandedSurrogate = gunslinger.PistolWhip.TwoHandedItem;
                    return pistolWhip;
                }
                case FavoredClassCatalog.EffectHalflingNimble:
                case FavoredClassCatalog.EffectDrowNimble:
                {
                    var nimble = ScriptableObject.CreateInstance<FavoredClassNimbleArmorClassBonus>();
                    nimble.name = "$" + full.name + "_NimbleArmorClass";
                    nimble.Divisor = divisor;
                    nimble.CapSteps = cap;
                    nimble.Nimble = gunslinger.Nimble.Features[0];
                    return nimble;
                }
                case FavoredClassCatalog.EffectDirtyTrickTrip:
                {
                    var maneuver = ScriptableObject.CreateInstance<FavoredClassManeuverBonus>();
                    maneuver.name = "$" + full.name + "_ManeuverBonus";
                    maneuver.Divisor = divisor;
                    maneuver.CapSteps = cap;
                    return maneuver;
                }
                case FavoredClassCatalog.EffectMisfire:
                case FavoredClassCatalog.EffectHalflingDodge:
                case FavoredClassCatalog.EffectInitiative:
                    return null;
                default:
                    throw new InvalidOperationException("No mechanics are implemented for " + effect.Id);
            }
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
                if (pair.Partial != null && pair.Partial.ComponentsArray.Any(component =>
                        !(component is Prerequisite)))
                    throw new InvalidOperationException("A partial favored-class leaf carries mechanics: " +
                        pair.Partial.name);
            }
            if (set.Pairs.Select(pair => pair.Effect.Id + "|" + pair.TargetKey)
                    .Distinct(StringComparer.Ordinal).Count() != set.Pairs.Count)
                throw new InvalidOperationException("Favored-class counters are not unique.");
        }
    }
}
