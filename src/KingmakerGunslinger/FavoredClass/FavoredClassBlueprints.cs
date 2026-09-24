using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Properties;
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
        private readonly IDictionary<string, BlueprintScriptableObject> _auxiliary;

        internal FavoredClassBlueprintSet(IList<FavoredClassLeafPair> pairs,
            string gunslingerClassGuid)
            : this(pairs, gunslingerClassGuid, null)
        {
        }

        internal FavoredClassBlueprintSet(IList<FavoredClassLeafPair> pairs,
            string gunslingerClassGuid, IDictionary<string, BlueprintScriptableObject> auxiliary)
        {
            Pairs = pairs ?? throw new ArgumentNullException("pairs");
            GunslingerClassGuid = gunslingerClassGuid ??
                throw new ArgumentNullException("gunslingerClassGuid");
            _auxiliary = new Dictionary<string, BlueprintScriptableObject>(
                auxiliary ?? new Dictionary<string, BlueprintScriptableObject>(), StringComparer.Ordinal);
        }

        /// <summary>Owned helper identities (aura steps property, pet features) by symbol.</summary>
        internal IEnumerable<KeyValuePair<string, BlueprintScriptableObject>> Auxiliary
        {
            get { return _auxiliary; }
        }

        /// <summary>O06: the earned-steps property the native aura buffs read, or null.</summary>
        internal BlueprintUnitProperty AuraStepsProperty
        {
            get
            {
                BlueprintScriptableObject value;
                return _auxiliary.TryGetValue(FavoredClassBlueprints.AuraStepsPropertySymbol, out value)
                    ? value as BlueprintUnitProperty : null;
            }
        }

        /// <summary>O07/O08: the hidden pet feature of a pet-armor effect, or null.</summary>
        internal BlueprintFeature PetFeature(string effectId)
        {
            string symbol = FavoredClassBlueprints.PetFeatureSymbol(effectId);
            BlueprintScriptableObject value;
            return symbol != null && _auxiliary.TryGetValue(symbol, out value) ? value as BlueprintFeature : null;
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

        // Exact native host classes of the scheduled non-Gunslinger families
        // (the Favored Class host's own class identities).
        private static readonly Dictionary<string, string> NativeClassGuids =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { FavoredClassCatalog.Alchemist, "0937bec61c0dabc468428f496580c721" },
                { FavoredClassCatalog.Inquisitor, "f1a70d9e1b0b41e49874e1fa9052a1ce" },
                { FavoredClassCatalog.Rogue, "299aa766dee3cbf4790da4efb8c72484" },
                { FavoredClassCatalog.Fighter, "48ac8db94d5de7645906c7d0ad3bcfbd" },
                { FavoredClassCatalog.Monk, "e8f21e5b58e0569468e420ebea456124" },
                { FavoredClassCatalog.Cleric, "67819271767a9dd4fbfd4ae700befea0" },
                { FavoredClassCatalog.Paladin, "bfa11238e7ae3544bbeb4d0b92e897ec" },
                { FavoredClassCatalog.Ranger, "cda0615668a6df14eb36ba19ee881af6" },
            };

        // Classes of an optional provider (Call of the Wild), which may be
        // created after KMG registers: only their identities are used at
        // registration; publication skips a class the host did not scan.
        private static readonly Dictionary<string, string> OptionalProviderClassGuids =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { FavoredClassCatalog.Summoner, "0f4c4ada51334b43a802350c5c0b85f5" },
            };

        // O06/O07/O08 helper identities and native features.
        internal const string AuraStepsPropertySymbol = "KMG.FavoredClass.Paladin.AuraAllyBonus.StepsProperty";
        internal const string CompanionPetFeatureSymbol = "KMG.FavoredClass.Ranger.CompanionNaturalArmor.PetFeature";
        internal const string EidolonPetFeatureSymbol = "KMG.FavoredClass.Summoner.EidolonNaturalArmor.PetFeature";
        internal const string AuraOfCourageFeatureGuid = "e45ab30f49215054e83b4ea12165409f";
        internal const string AuraOfResolveFeatureGuid = "a28693b24cc412c478b8b85877f2dad2";
        internal const string HuntersBondSelectionGuid = "b705c5184a96a84428eeb35ae2517a14";
        // Call of the Wild's eidolon class (optional provider; resolved by identity at use).
        internal const string EidolonClassGuid = "e3b3ad6decb14cdba2e7e14982d90035";

        internal static string PetFeatureSymbol(string effectId)
        {
            switch (effectId)
            {
                case FavoredClassCatalog.EffectCompanionArmor: return CompanionPetFeatureSymbol;
                case FavoredClassCatalog.EffectEidolonArmor: return EidolonPetFeatureSymbol;
                default: return null;
            }
        }

        // Native blueprints the Phase 3 mechanics read.
        internal const string FastBombsBuffGuid = "c42ae8f9652bbc14eb13b31d12d20f8a";
        internal const string SubtypeFireGuid = "23dc7b90d148b9d439f48e015a520a9c";
        internal const string SubtypeWaterGuid = "bf7ee56ec9e43c14fa17727997e91993";
        internal const string SubtypeAquaticGuid = "03ce447c6147ecd46940dbef87f6eed7";
        internal const string StunningFistResourceGuid = "d2bae584db4bf4f4f86dd9d15ae56558";
        private const string VivisectionistArchetypeGuid = "68cbcd9fbf1fb1d489562f829bb97e38";
        private const string ToxicantArchetypeGuid = "ad9d36a0e5d7499498c6cc59f43b3afe";

        // Icons of the exact native features each counter improves.
        private static readonly Dictionary<string, string> NativeIconSources =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { FavoredClassCatalog.EffectBombDamage, "c59b2f256f5a70a4d896568658315b7d" },
                { FavoredClassCatalog.EffectFireIntimidate, "7d2233c3b7a0b984ba058a83b736e6ac" },
                { FavoredClassCatalog.EffectDemoralize, "7d2233c3b7a0b984ba058a83b736e6ac" },
                { FavoredClassCatalog.EffectBullRushDragDefense, "b3614622866fe7046b787a548bbd7f59" },
                { FavoredClassCatalog.EffectUnarmedConfirmation, "7812ad3672a4b9a4fb894ea402095167" },
                { FavoredClassCatalog.EffectAquaticPenetration, "ee7dc126939e4d9438357fbd5980d459" },
                { FavoredClassCatalog.EffectGrappleStunning, "a29a582c3daa4c24bb0e991c596ccb28" },
                { FavoredClassCatalog.EffectPaladinAuras, AuraOfCourageFeatureGuid },
                { FavoredClassCatalog.EffectCompanionArmor, "ee63330662126374e8785cc901941ac7" },
            };

        internal static FavoredClassBlueprintSet Register(BlueprintRegistry registry,
            LibraryScriptableObject library, GunslingerClassBlueprintSet gunslinger,
            ProductionFirearmBlueprintCatalog firearms)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (library == null) throw new ArgumentNullException("library");
            if (gunslinger == null) throw new ArgumentNullException("gunslinger");
            if (firearms == null) throw new ArgumentNullException("firearms");
            var pairs = new List<FavoredClassLeafPair>();
            var auxiliary = new Dictionary<string, BlueprintScriptableObject>(StringComparer.Ordinal);
            foreach (string effectId in FavoredClassLeafCatalog.ImplementedEffects)
            {
                FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
                IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.LeavesFor(effectId);
                foreach (string targetKey in FavoredClassLeafCatalog.TargetKeys(effectId))
                {
                    Sprite icon = IconFor(effect, targetKey, library, gunslinger, firearms);
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
                    RegisterAuxiliary(effect, full, registry, auxiliary);
                    AttachPrerequisites(effect, full, partial, library, gunslinger);
                    AttachMechanics(effect, full, partial, library, gunslinger, auxiliary);
                    pairs.Add(new FavoredClassLeafPair(effect, targetKey,
                        HostClassGuidFor(effect, gunslinger), full, partial));
                }
            }
            FavoredClassBlueprintSet set = new FavoredClassBlueprintSet(pairs.AsReadOnly(),
                gunslinger.CharacterClass.AssetGuid, auxiliary);
            Validate(set);
            return set;
        }

        private static string HostClassGuidFor(FavoredClassEffectSpec effect,
            GunslingerClassBlueprintSet gunslinger)
        {
            if (effect.ClassFamily == FavoredClassCatalog.Gunslinger)
                return gunslinger.CharacterClass.AssetGuid;
            string guid;
            if (NativeClassGuids.TryGetValue(effect.ClassFamily, out guid))
                return guid;
            if (OptionalProviderClassGuids.TryGetValue(effect.ClassFamily, out guid))
                return guid;
            throw new InvalidOperationException("No verified host class for " + effect.Id);
        }

        /// <summary>
        /// Owned helper identities of the aura and pet counters, registered
        /// with the leaves so their saved facts always resolve.
        /// </summary>
        private static void RegisterAuxiliary(FavoredClassEffectSpec effect, BlueprintFeature full,
            BlueprintRegistry registry, IDictionary<string, BlueprintScriptableObject> auxiliary)
        {
            switch (effect.Id)
            {
                case FavoredClassCatalog.EffectPaladinAuras:
                    auxiliary[AuraStepsPropertySymbol] = registry.Register<BlueprintUnitProperty>(
                        AuraStepsPropertySymbol, () => CreateStepsProperty(effect, full));
                    break;
                case FavoredClassCatalog.EffectCompanionArmor:
                {
                    BlueprintCharacterClass companion = BlueprintRoot.Instance == null ||
                        BlueprintRoot.Instance.Progression == null ? null :
                        BlueprintRoot.Instance.Progression.AnimalCompanion;
                    if (companion == null)
                        throw new InvalidOperationException("The native animal companion class is unavailable.");
                    auxiliary[CompanionPetFeatureSymbol] = registry.Register<BlueprintFeature>(
                        CompanionPetFeatureSymbol, () => CreatePetFeature(CompanionPetFeatureSymbol, effect,
                            full, companion.AssetGuid, "Animal Companion Armor (Favored Class)"));
                    break;
                }
                case FavoredClassCatalog.EffectEidolonArmor:
                    auxiliary[EidolonPetFeatureSymbol] = registry.Register<BlueprintFeature>(
                        EidolonPetFeatureSymbol, () => CreatePetFeature(EidolonPetFeatureSymbol, effect,
                            full, EidolonClassGuid, "Eidolon Armor (Favored Class)"));
                    break;
            }
        }

        private static BlueprintUnitProperty CreateStepsProperty(FavoredClassEffectSpec effect,
            BlueprintFeature full)
        {
            var property = ScriptableObject.CreateInstance<BlueprintUnitProperty>();
            property.name = "KMG_" + AuraStepsPropertySymbol.Substring(
                FavoredClassLeafCatalog.SymbolPrefix.Length).Replace('.', '_');
            var getter = ScriptableObject.CreateInstance<FavoredClassEarnedStepsProperty>();
            getter.name = "$" + property.name + "_Getter";
            getter.Feature = full;
            getter.Divisor = effect.Rate.Divisor;
            getter.CapSteps = effect.Rate.CapSteps ?? 0;
            property.ComponentsArray = new BlueprintComponent[] { getter };
            return property;
        }

        private static BlueprintFeature CreatePetFeature(string symbol, FavoredClassEffectSpec effect,
            BlueprintFeature full, string petClassGuid, string title)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + symbol.Substring(FavoredClassLeafCatalog.SymbolPrefix.Length)
                .Replace('.', '_');
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.HideInCharacterSheetAndLevelUp = true;
            feature.Groups = new FeatureGroup[0];
            var armor = ScriptableObject.CreateInstance<FavoredClassPetNaturalArmor>();
            armor.name = "$" + feature.name + "_NaturalArmor";
            armor.MasterFeature = full;
            armor.Divisor = effect.Rate.Divisor;
            armor.CapSteps = effect.Rate.CapSteps ?? 0;
            armor.PetClassGuid = petClassGuid;
            feature.ComponentsArray = new BlueprintComponent[] { armor };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    "Natural armor bonus from the master's favored class investment."),
                null);
            return feature;
        }

        /// <summary>
        /// The class features a counter improves, when an archetype can remove
        /// all of them: the counter is then not offered (dormant investment in
        /// a feature gained later stays legal).
        /// </summary>
        private static BlueprintFeature[] ImprovedFeatures(FavoredClassEffectSpec effect,
            LibraryScriptableObject library)
        {
            switch (effect.Id)
            {
                case FavoredClassCatalog.EffectPaladinAuras:
                    return new[]
                    {
                        BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                            AuraOfCourageFeatureGuid, "native Aura of Courage"),
                        BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                            AuraOfResolveFeatureGuid, "native Aura of Resolve")
                    };
                case FavoredClassCatalog.EffectCompanionArmor:
                    return new BlueprintFeature[]
                    {
                        BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(library,
                            HuntersBondSelectionGuid, "native Hunter's Bond selection")
                    };
                default:
                    return new BlueprintFeature[0];
            }
        }

        /// <summary>
        /// Each leaf shows the icon of the exact KMG feature or firearm type it
        /// improves (the host's own favored-class leaves likewise reuse related
        /// feature art); null renders the native monogram. Read after the
        /// project icon stage has assigned those icons.
        /// </summary>
        private static Sprite IconFor(FavoredClassEffectSpec effect, string targetKey,
            LibraryScriptableObject library, GunslingerClassBlueprintSet gunslinger,
            ProductionFirearmBlueprintCatalog firearms)
        {
            string nativeSource;
            if (NativeIconSources.TryGetValue(effect.Id, out nativeSource))
                return NativeIconSource(library, nativeSource, effect.Id).Icon;
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

        // An icon source is a native feature or ability; presentation only,
        // but still resolved exactly by identity and failing closed.
        private static BlueprintUnitFact NativeIconSource(LibraryScriptableObject library, string guid,
            string effectId)
        {
            BlueprintScriptableObject blueprint;
            if (library.BlueprintsByAssetId == null ||
                !library.BlueprintsByAssetId.TryGetValue(guid, out blueprint) ||
                !(blueprint is BlueprintFeature || blueprint is BlueprintAbility))
                throw new InvalidOperationException("Native icon source for " + effectId +
                    " is missing or not a feature/ability: " + guid);
            return (BlueprintUnitFact)blueprint;
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
                    spec.EffectId == FavoredClassCatalog.EffectDrowNimble ||
                    spec.EffectId == FavoredClassCatalog.EffectCompanionArmor ||
                    spec.EffectId == FavoredClassCatalog.EffectEidolonArmor);
            feature.Groups = new FeatureGroup[0];
            feature.ComponentsArray = new BlueprintComponent[0];
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(spec.Symbol + ".Name", spec.Name),
                LocalizationService.Create(spec.Symbol + ".Description", spec.Description),
                icon);
            return feature;
        }

        private static void AttachPrerequisites(FavoredClassEffectSpec effect,
            BlueprintFeature full, BlueprintFeature partial, LibraryScriptableObject library,
            GunslingerClassBlueprintSet gunslinger)
        {
            IList<BlueprintArchetype> replacing = ReplacingArchetypes(effect, library, gunslinger);
            BlueprintFeature[] improved = ImprovedFeatures(effect, library);
            // An optional provider's class may not exist yet; it is read only
            // when an archetype rule needs it.
            BlueprintCharacterClass hostClass = replacing.Count == 0 && improved.Length == 0 ? null :
                BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                    library, HostClassGuidFor(effect, gunslinger), "host class of " + effect.Id);
            foreach (BlueprintFeature leaf in new[] { partial, full })
            {
                if (leaf == null)
                    continue;
                var components = new List<BlueprintComponent>
                {
                    Investment(effect, full, partial, ReferenceEquals(leaf, partial)),
                    Ancestry(effect, leaf)
                };
                for (int index = 0; index < replacing.Count; index++)
                    components.Add(NoArchetype(leaf, hostClass, replacing[index], index));
                if (improved.Length > 0)
                {
                    var available = ScriptableObject.CreateInstance<PrerequisiteFavoredClassFeatureAvailable>();
                    available.name = "$" + leaf.name + "_FeatureAvailable";
                    available.CharacterClass = hostClass;
                    available.Features = improved;
                    available.Group = Prerequisite.GroupType.All;
                    components.Add(available);
                }
                leaf.ComponentsArray = leaf.ComponentsArray.Concat(components).ToArray();
            }
        }

        /// <summary>
        /// The supported archetype that permanently replaces the improved
        /// feature, so its favored-class investment would be a broken no-op.
        /// Earlier (dormant) investment is otherwise allowed.
        /// </summary>
        internal static IList<BlueprintArchetype> ReplacingArchetypes(FavoredClassEffectSpec effect,
            LibraryScriptableObject library, GunslingerClassBlueprintSet gunslinger)
        {
            var result = new List<BlueprintArchetype>();
            switch (effect.Id)
            {
                case FavoredClassCatalog.EffectHalflingNimble:
                case FavoredClassCatalog.EffectDrowNimble:
                    if (gunslinger.MysteriousStranger != null)
                        result.Add(gunslinger.MysteriousStranger.Archetype);
                    break;
                case FavoredClassCatalog.EffectHalflingDodge:
                    if (gunslinger.MusketMaster != null)
                        result.Add(gunslinger.MusketMaster.Archetype);
                    break;
                case FavoredClassCatalog.EffectBombDamage:
                    // Vivisectionist is native; Toxicant exists only with Call
                    // of the Wild, and its absence removes only this exclusion.
                    result.Add(BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                        VivisectionistArchetypeGuid, "native Vivisectionist"));
                    BlueprintScriptableObject toxicant;
                    if (library.BlueprintsByAssetId.TryGetValue(ToxicantArchetypeGuid, out toxicant) &&
                        toxicant is BlueprintArchetype)
                        result.Add((BlueprintArchetype)toxicant);
                    break;
            }
            return result;
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
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype, int index)
        {
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteNoArchetype>();
            prerequisite.name = "$" + leaf.name + "_NoArchetype" + (index == 0 ? string.Empty :
                index.ToString(System.Globalization.CultureInfo.InvariantCulture));
            prerequisite.CharacterClass = characterClass;
            prerequisite.Archetype = archetype;
            prerequisite.Group = Prerequisite.GroupType.All;
            return prerequisite;
        }

        private static void AttachMechanics(FavoredClassEffectSpec effect, BlueprintFeature full,
            BlueprintFeature partial, LibraryScriptableObject library, GunslingerClassBlueprintSet gunslinger,
            IDictionary<string, BlueprintScriptableObject> auxiliary)
        {
            BlueprintComponent mechanics = CreateMechanics(effect, full, library, gunslinger, auxiliary);
            if (mechanics != null)
                full.ComponentsArray = full.ComponentsArray.Concat(
                    new[] { mechanics }).ToArray();
            // U04's grapple CMD is immediate: every investment, full or
            // partial, adds +1, so both leaves carry the per-rank defense.
            if (effect.Id == FavoredClassCatalog.EffectGrappleStunning)
                foreach (BlueprintFeature leaf in new[] { partial, full })
                    if (leaf != null)
                        leaf.ComponentsArray = leaf.ComponentsArray.Concat(new BlueprintComponent[]
                            { ManeuverDefense(leaf, CombatManeuver.Grapple) }).ToArray();
        }

        private static FavoredClassManeuverDefenseBonus ManeuverDefense(BlueprintFeature leaf,
            CombatManeuver maneuver)
        {
            var defense = ScriptableObject.CreateInstance<FavoredClassManeuverDefenseBonus>();
            defense.name = "$" + leaf.name + "_" + maneuver + "Defense";
            defense.Maneuver = maneuver;
            return defense;
        }

        /// <summary>
        /// The owned component on the full leaf, or null where the effect is
        /// applied inside an existing authoritative calculation that reads the
        /// full rank through <see cref="FavoredClassEarnedSteps"/> (misfire
        /// threshold, Gunslinger's Dodge, Gunslinger Initiative).
        /// </summary>
        private static BlueprintComponent CreateMechanics(FavoredClassEffectSpec effect,
            BlueprintFeature full, LibraryScriptableObject library,
            GunslingerClassBlueprintSet gunslinger, IDictionary<string, BlueprintScriptableObject> auxiliary)
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
                case FavoredClassCatalog.EffectBombDamage:
                {
                    var bombs = ScriptableObject.CreateInstance<FavoredClassBombDamageBonus>();
                    bombs.name = "$" + full.name + "_BombDamage";
                    bombs.Divisor = divisor;
                    bombs.CapSteps = cap;
                    FastBombs lineage = BlueprintLibraryLookup
                        .RequireExact<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(library,
                            FastBombsBuffGuid, "native Fast Bombs").ComponentsArray.OfType<FastBombs>()
                        .Single();
                    bombs.Bombs = lineage.Abilities.Where(value => value != null).ToArray();
                    if (bombs.Bombs.Length == 0)
                        throw new InvalidOperationException("The native bomb lineage is empty.");
                    return bombs;
                }
                case FavoredClassCatalog.EffectFireIntimidate:
                case FavoredClassCatalog.EffectDemoralize:
                {
                    var intimidate = ScriptableObject.CreateInstance<FavoredClassIntimidateBonus>();
                    intimidate.name = "$" + full.name + "_Intimidate";
                    intimidate.Divisor = divisor;
                    intimidate.CapSteps = cap;
                    if (effect.Id == FavoredClassCatalog.EffectFireIntimidate)
                        intimidate.RequiredTargetSubtype = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                            library, SubtypeFireGuid, "native fire subtype");
                    else
                        intimidate.RequireDemoralize = true;
                    return intimidate;
                }
                case FavoredClassCatalog.EffectBullRushDragDefense:
                    return ManeuverDefense(full, CombatManeuver.BullRush);
                case FavoredClassCatalog.EffectUnarmedConfirmation:
                {
                    var unarmed = ScriptableObject.CreateInstance<FavoredClassUnarmedConfirmationBonus>();
                    unarmed.name = "$" + full.name + "_Confirmation";
                    unarmed.Divisor = divisor;
                    unarmed.CapSteps = cap;
                    unarmed.CriticalFocus = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                        library, CriticalFocusGuid, "native Critical Focus");
                    return unarmed;
                }
                case FavoredClassCatalog.EffectAquaticPenetration:
                {
                    var penetration = ScriptableObject.CreateInstance<FavoredClassSpellPenetrationBonus>();
                    penetration.name = "$" + full.name + "_SpellPenetration";
                    penetration.TargetSubtypes = new[]
                    {
                        BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SubtypeAquaticGuid,
                            "native aquatic subtype"),
                        BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SubtypeWaterGuid,
                            "native water subtype")
                    };
                    return penetration;
                }
                case FavoredClassCatalog.EffectGrappleStunning:
                {
                    var stunning = ScriptableObject.CreateInstance<FavoredClassResourceBonus>();
                    stunning.name = "$" + full.name + "_StunningFist";
                    stunning.Divisor = divisor;
                    stunning.CapSteps = cap;
                    stunning.Resource = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(
                        library, StunningFistResourceGuid, "native Stunning Fist resource");
                    return stunning;
                }
                case FavoredClassCatalog.EffectCompanionArmor:
                case FavoredClassCatalog.EffectEidolonArmor:
                {
                    var projection = ScriptableObject.CreateInstance<FavoredClassPetArmorProjection>();
                    projection.name = "$" + full.name + "_PetArmor";
                    projection.PetFeature = (BlueprintFeature)auxiliary[PetFeatureSymbol(effect.Id)];
                    projection.PetClassGuid = projection.PetFeature.ComponentsArray
                        .OfType<FavoredClassPetNaturalArmor>().Single().PetClassGuid;
                    return projection;
                }
                case FavoredClassCatalog.EffectMisfire:
                case FavoredClassCatalog.EffectHalflingDodge:
                case FavoredClassCatalog.EffectInitiative:
                case FavoredClassCatalog.EffectPaladinAuras:
                    // O06 is read inside the native aura buffs (FavoredClassAuraPublication).
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
                // Only a mixed-rate bundle's immediate portion may sit on a
                // partial leaf (U04's per-investment grapple defense).
                if (pair.Partial != null && pair.Partial.ComponentsArray.Any(component =>
                        !(component is Prerequisite) &&
                        !(pair.Effect.Id == FavoredClassCatalog.EffectGrappleStunning &&
                            component is FavoredClassManeuverDefenseBonus)))
                    throw new InvalidOperationException("A partial favored-class leaf carries mechanics: " +
                        pair.Partial.name);
            }
            if (set.Pairs.Select(pair => pair.Effect.Id + "|" + pair.TargetKey)
                    .Distinct(StringComparer.Ordinal).Count() != set.Pairs.Count)
                throw new InvalidOperationException("Favored-class counters are not unique.");
            foreach (string effectId in new[] { FavoredClassCatalog.EffectCompanionArmor,
                FavoredClassCatalog.EffectEidolonArmor })
            {
                FavoredClassLeafPair pair = set.Pair(effectId, null);
                if (pair == null)
                    continue;
                BlueprintFeature pet = set.PetFeature(effectId);
                FavoredClassPetArmorProjection[] projection = pair.Full.ComponentsArray
                    .OfType<FavoredClassPetArmorProjection>().ToArray();
                if (pet == null || !pet.HideInUI || projection.Length != 1 ||
                    !ReferenceEquals(projection[0].PetFeature, pet) ||
                    pet.ComponentsArray.OfType<FavoredClassPetNaturalArmor>().Count(armor =>
                        ReferenceEquals(armor.MasterFeature, pair.Full)) != 1)
                    throw new InvalidOperationException("Pet-armor graph is malformed: " + effectId);
            }
            if (set.Pair(FavoredClassCatalog.EffectPaladinAuras, null) != null &&
                (set.AuraStepsProperty == null || set.AuraStepsProperty.ComponentsArray
                    .OfType<FavoredClassEarnedStepsProperty>().Count() != 1))
                throw new InvalidOperationException("The paladin aura steps property is malformed.");
        }
    }
}
