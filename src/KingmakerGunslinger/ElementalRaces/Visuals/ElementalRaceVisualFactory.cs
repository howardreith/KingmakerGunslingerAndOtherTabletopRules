using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    internal static class ElementalRaceVisualFactory
    {
        private const string AasimarStandardPresetGuid =
            "640e57f7890fa044ea78914930ddac5b";
        private const string AasimarHeavyPresetGuid =
            "d529cb3def52a584f93a4aff5e20316a";
        private const string AasimarSlenderPresetGuid =
            "00fa5240ec151e8419cb60c34fb96e0e";
        private static readonly string[] AasimarPresetGuids =
        {
            AasimarStandardPresetGuid,
            AasimarHeavyPresetGuid,
            AasimarSlenderPresetGuid
        };

        private static readonly FieldInfo PrimaryRampsField = RequireField(
            typeof(EquipmentEntity), "m_PrimaryRamps",
            typeof(List<Texture2D>));
        private static readonly FieldInfo SecondaryRampsField = RequireField(
            typeof(EquipmentEntity), "m_SecondaryRamps",
            typeof(List<Texture2D>));
        private static readonly FieldInfo MaleArrayField = RequireField(
            typeof(KingmakerEquipmentEntity), "m_MaleArray",
            typeof(EquipmentEntityLink[]));
        private static readonly FieldInfo FemaleArrayField = RequireField(
            typeof(KingmakerEquipmentEntity), "m_FemaleArray",
            typeof(EquipmentEntityLink[]));
        private static readonly FieldInfo RaceDependentField = RequireField(
            typeof(KingmakerEquipmentEntity), "m_RaceDependent",
            typeof(bool));
        private static readonly FieldInfo RaceDependentArraysField =
            RequireArrayField(typeof(KingmakerEquipmentEntity),
                "m_RaceDependentArrays");

        internal static ElementalRaceVisualSet Register(
            LibraryScriptableObject library, BlueprintManifest manifest,
            BlueprintRegistry blueprintRegistry, ModLogger logger,
            BlueprintRace aasimar)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (manifest == null) throw new ArgumentNullException("manifest");
            if (blueprintRegistry == null)
                throw new ArgumentNullException("blueprintRegistry");
            if (logger == null) throw new ArgumentNullException("logger");
            if (aasimar == null) throw new ArgumentNullException("aasimar");
            ElementalRaceVisualCatalog.Validate();
            BlueprintRaceVisualPreset[] donors = RequirePresetDonors(
                library, aasimar);
            ElementalRaceVisualDefinition[] definitions =
                ElementalRaceVisualCatalog.Ordered().ToArray();
            ResolvedPlan[] plans = definitions.Select(value =>
                ResolvePlan(value, aasimar, logger)).ToArray();
            var resourceRegistry = new
                ElementalRaceVisualResourceRegistry(manifest, logger);
            resourceRegistry.EnsureAvailable(definitions.SelectMany(value =>
                value.Proxies()));

            try
            {
                var results = new List<ElementalRaceVisualBlueprints>();
                foreach (ResolvedPlan plan in plans)
                {
                    var registrations = new List<
                        ElementalRaceVisualResourceRegistration>();
                    foreach (ElementalRaceVisualProxySpec spec in
                        plan.Definition.Proxies())
                    {
                        ResolvedDonor donor = plan.RequireDonor(spec.Symbol);
                        EquipmentEntity proxy = CreateProxy(spec, donor.Resource,
                            plan.SkinPalette);
                        registrations.Add(resourceRegistry.Register(spec, proxy,
                            donor.UsedFallback));
                    }

                    KingmakerEquipmentEntity body = blueprintRegistry.Register<
                        KingmakerEquipmentEntity>(
                            plan.Definition.BodyBlueprintSymbol,
                            () => CreateBodyWrapper(aasimar, plan,
                                resourceRegistry));
                    string[] presetSymbols = plan.Definition.PresetSymbols;
                    var presets = new BlueprintRaceVisualPreset[3];
                    for (int index = 0; index < presets.Length; index++)
                    {
                        int captured = index;
                        presets[index] = blueprintRegistry.Register<
                            BlueprintRaceVisualPreset>(presetSymbols[index],
                                () => CreatePreset(donors[captured], body,
                                    presetSymbols[captured]));
                    }

                    CustomizationOptions male = CreateOptions(plan, true,
                        resourceRegistry);
                    CustomizationOptions female = CreateOptions(plan, false,
                        resourceRegistry);
                    var visuals = new ElementalRaceVisualBlueprints(
                        plan.Definition, body, presets, male, female,
                        registrations, plan.UsedFallback);
                    Validate(visuals, aasimar, donors, resourceRegistry);
                    results.Add(visuals);
                    logger.Info("elemental-races", "visuals.registered",
                        string.Format(CultureInfo.InvariantCulture,
                            "Registered {0} visuals; resources={1}; fallback={2}; reason={3}.",
                            plan.Definition.Kind, registrations.Count,
                            plan.UsedFallback, plan.FallbackReason));
                }
                return new ElementalRaceVisualSet(results, resourceRegistry);
            }
            catch
            {
                resourceRegistry.RollbackAll();
                throw;
            }
        }

        private static EquipmentEntity CreateProxy(
            ElementalRaceVisualProxySpec spec, EquipmentEntity donor,
            IReadOnlyList<Texture2D> skinPalette)
        {
            EquipmentEntity proxy = UnityEngine.Object.Instantiate(donor);
            if (proxy == null || ReferenceEquals(proxy, donor))
                throw new InvalidOperationException(
                    "Unity failed to clone visual donor " + donor.name + ".");
            proxy.name = spec.Symbol.Replace('.', '_');
            proxy.hideFlags = HideFlags.DontUnloadUnusedAsset;
            if (spec.UsesSkinPalette)
            {
                if (skinPalette == null ||
                    skinPalette.Count != ElementalRaceVisualCatalog.SkinRampCount ||
                    skinPalette.Any(value => value == null))
                    throw new InvalidOperationException(
                        "A complete seven-ramp skin palette is required for " +
                        spec.Symbol + ".");
                proxy.ColorsProfile = null;
                PrimaryRampsField.SetValue(proxy,
                    new List<Texture2D>(skinPalette));
                SecondaryRampsField.SetValue(proxy, new List<Texture2D>());
            }
            if (string.IsNullOrWhiteSpace(proxy.name) ||
                proxy.BodyParts == null || proxy.OutfitParts == null)
                throw new InvalidOperationException(
                    "Visual proxy clone is incomplete for " + spec.Symbol + ".");
            return proxy;
        }

        private static KingmakerEquipmentEntity CreateBodyWrapper(
            BlueprintRace aasimar, ResolvedPlan plan,
            ElementalRaceVisualResourceRegistry resources)
        {
            BlueprintRaceVisualPreset donorPreset = aasimar.Presets[0];
            if (donorPreset == null || donorPreset.Skin == null)
                throw new InvalidOperationException(
                    "Aasimar's complete body wrapper fallback is unavailable.");
            KingmakerEquipmentEntity wrapper = BlueprintCloneService.Clone(
                donorPreset.Skin,
                plan.Definition.BodyBlueprintSymbol.Replace('.', '_'));
            wrapper.ComponentsArray = Array.Empty<BlueprintComponent>();
            RaceDependentField.SetValue(wrapper, false);
            MaleArrayField.SetValue(wrapper, new[]
            {
                Link(resources.Require(plan.Definition.Male.Body.Symbol).AssetId)
            });
            FemaleArrayField.SetValue(wrapper, new[]
            {
                Link(resources.Require(plan.Definition.Female.Body.Symbol).AssetId)
            });
            RaceDependentArraysField.SetValue(wrapper, Array.CreateInstance(
                RaceDependentArraysField.FieldType.GetElementType(), 0));
            return wrapper;
        }

        private static BlueprintRaceVisualPreset CreatePreset(
            BlueprintRaceVisualPreset donor, KingmakerEquipmentEntity body,
            string symbol)
        {
            BlueprintRaceVisualPreset preset = BlueprintCloneService.Clone(
                donor, symbol.Replace('.', '_'));
            // Native Aasimar is an outsider race whose visual presets retain
            // the Human doll/equipment RaceId. Preserve that exact split.
            preset.RaceId = donor.RaceId;
            preset.Skin = body;
            if (preset.MaleSkeleton == null || preset.FemaleSkeleton == null)
                throw new InvalidOperationException(
                    "A visual preset lost a required Human-compatible skeleton.");
            return preset;
        }

        private static CustomizationOptions CreateOptions(ResolvedPlan plan,
            bool male, ElementalRaceVisualResourceRegistry resources)
        {
            ElementalRaceSexVisualDefinition definition = male ?
                plan.Definition.Male : plan.Definition.Female;
            ResolvedOptions options = male ? plan.Male : plan.Female;
            EquipmentEntityLink[] horns = definition.Horns.Length == 0 ?
                Array.Empty<EquipmentEntityLink>() :
                new[] { Link(ElementalRaceVisualCatalog.EmptyAssetId) }.Concat(
                    definition.Horns.Select(value => Link(resources.Require(
                        value.Symbol).AssetId))).ToArray();
            return new CustomizationOptions
            {
                Heads = definition.Heads.Select(value => Link(resources.Require(
                    value.Symbol).AssetId)).ToArray(),
                Hair = Links(options.Hair),
                Eyebrows = Links(options.Eyebrows),
                Beards = Links(options.Beards),
                Horns = horns,
                TailSkinColors = Array.Empty<EquipmentEntityLink>()
            };
        }

        private static void Validate(ElementalRaceVisualBlueprints visuals,
            BlueprintRace aasimar, BlueprintRaceVisualPreset[] donors,
            ElementalRaceVisualResourceRegistry resources)
        {
            EquipmentEntityLink[] maleBody = visuals.Body.GetLinks(Gender.Male,
                aasimar.RaceId);
            EquipmentEntityLink[] femaleBody = visuals.Body.GetLinks(
                Gender.Female, aasimar.RaceId);
            if (maleBody == null || maleBody.Length != 1 ||
                femaleBody == null || femaleBody.Length != 1 ||
                !string.Equals(maleBody[0].AssetId,
                    resources.Require(visuals.Definition.Male.Body.Symbol)
                        .AssetId, StringComparison.Ordinal) ||
                !string.Equals(femaleBody[0].AssetId,
                    resources.Require(visuals.Definition.Female.Body.Symbol)
                        .AssetId, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    visuals.Definition.Kind + " body wrapper is invalid.");
            BlueprintRaceVisualPreset[] presets = visuals.Presets;
            if (donors == null || donors.Length != presets.Length)
                throw new InvalidOperationException(
                    "A complete native preset donor set is required.");
            for (int index = 0; index < presets.Length; index++)
                if (presets[index] == null || presets[index].Skin == null ||
                    !ReferenceEquals(presets[index].Skin, visuals.Body) ||
                    presets[index].RaceId != donors[index].RaceId ||
                    !ReferenceEquals(presets[index].MaleSkeleton,
                        donors[index].MaleSkeleton) ||
                    !ReferenceEquals(presets[index].FemaleSkeleton,
                        donors[index].FemaleSkeleton))
                    throw new InvalidOperationException(
                        visuals.Definition.Kind +
                        " visual presets are invalid at index " + index + ".");
            ValidateOptions(visuals.MaleOptions, true,
                visuals.Definition.Kind);
            ValidateOptions(visuals.FemaleOptions, false,
                visuals.Definition.Kind);
            foreach (ElementalRaceVisualResourceRegistration registration in
                visuals.Resources)
            {
                EquipmentEntity resolved = ResourcesLibrary.TryGetResource<
                    EquipmentEntity>(registration.AssetId, true);
                if (!ReferenceEquals(resolved, registration.Resource))
                    throw new InvalidOperationException(
                        "Visual proxy resolution changed during validation.");
                if (registration.Spec.UsesSkinPalette &&
                    (resolved.PrimaryRamps == null || resolved.PrimaryRamps.Count !=
                        ElementalRaceVisualCatalog.SkinRampCount ||
                    resolved.PrimaryRamps.Any(value => value == null)))
                    throw new InvalidOperationException(
                        "Visual proxy palette validation failed for " +
                        registration.Spec.Symbol + ".");
            }
        }

        private static void ValidateOptions(CustomizationOptions options,
            bool male, ElementalRaceKind kind)
        {
            if (options == null || options.Heads == null ||
                options.Heads.Length < 2 || options.Hair == null ||
                options.Hair.Length < 4 || options.Eyebrows == null ||
                options.Eyebrows.Length < 1 || options.Beards == null ||
                options.Horns == null || options.TailSkinColors == null)
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1} customization breadth is incomplete.", kind,
                    male ? "male" : "female"));
            foreach (EquipmentEntityLink link in options.Heads.Concat(
                options.Hair).Concat(options.Eyebrows).Concat(options.Beards)
                .Concat(options.Horns))
                if (link == null || string.IsNullOrWhiteSpace(link.AssetId) ||
                    ResourcesLibrary.TryGetResource<EquipmentEntity>(
                        link.AssetId, true) == null)
                    throw new InvalidOperationException(
                        "A customization link failed exact resolution.");
        }

        private static EquipmentEntityLink[] Links(IEnumerable<string> ids)
        {
            return ids.Select(Link).ToArray();
        }

        private static EquipmentEntityLink Link(string id)
        {
            return new EquipmentEntityLink { AssetId = id };
        }

        private static ResolvedPlan ResolvePlan(
            ElementalRaceVisualDefinition definition, BlueprintRace aasimar,
            ModLogger logger)
        {
            var reasons = new List<string>();
            var donors = new Dictionary<string, ResolvedDonor>(
                StringComparer.Ordinal);
            foreach (ElementalRaceVisualProxySpec spec in definition.Proxies())
            {
                EquipmentEntity resource;
                string failure;
                bool fallback = !TryResolveExact(spec.Donor, out resource,
                    out failure);
                if (fallback)
                {
                    reasons.Add(spec.Symbol + ":" + failure);
                    resource = RequireExact(spec.Fallback,
                        "visual proxy fallback for " + spec.Symbol);
                }
                donors.Add(spec.Symbol, new ResolvedDonor
                {
                    Resource = resource,
                    UsedFallback = fallback
                });
            }

            List<Texture2D> palette;
            string paletteFailure;
            if (!TryResolvePalette(definition.SkinPalette, out palette,
                out paletteFailure))
            {
                reasons.Add("skin-palette:" + paletteFailure);
                EquipmentEntity fallbackHead = RequireExact(
                    definition.Male.Heads[0].Fallback,
                    "stable Aasimar skin-palette fallback");
                palette = NormalizeFallbackPalette(fallbackHead.PrimaryRamps,
                    ElementalRaceVisualCatalog.SkinRampCount);
            }

            ResolvedOptions male;
            string maleFailure;
            if (!TryResolveOptions(definition.Male, out male,
                out maleFailure))
            {
                reasons.Add("male-options:" + maleFailure);
                male = ResolveFallbackOptions(aasimar.MaleOptions, true);
            }
            ResolvedOptions female;
            string femaleFailure;
            if (!TryResolveOptions(definition.Female, out female,
                out femaleFailure))
            {
                reasons.Add("female-options:" + femaleFailure);
                female = ResolveFallbackOptions(aasimar.FemaleOptions, false);
            }

            bool usedFallback = reasons.Count > 0 ||
                donors.Values.Any(value => value.UsedFallback);
            string reason = reasons.Count == 0 ? "none" :
                string.Join(" | ", reasons.ToArray());
            if (usedFallback)
                logger.Warning("elemental-races", "visuals.fallback-selected",
                    definition.Kind + " selected complete native fallback visuals before mutation: " +
                    reason);
            return new ResolvedPlan
            {
                Definition = definition,
                SkinPalette = palette,
                Donors = donors,
                Male = male,
                Female = female,
                UsedFallback = usedFallback,
                FallbackReason = reason
            };
        }

        private static bool TryResolvePalette(
            IEnumerable<ElementalRaceRampReference> references,
            out List<Texture2D> palette, out string failure)
        {
            palette = new List<Texture2D>();
            failure = string.Empty;
            foreach (ElementalRaceRampReference reference in references)
            {
                EquipmentEntity source;
                if (!TryResolveExact(reference.Source, out source, out failure))
                {
                    palette.Clear();
                    return false;
                }
                if (source.ColorsProfile == null ||
                    !string.Equals(source.ColorsProfile.name,
                        reference.ExpectedProfile, StringComparison.Ordinal))
                {
                    failure = reference.Source.AssetId +
                        " color-profile mismatch";
                    palette.Clear();
                    return false;
                }
                Texture2D[] matches = (source.PrimaryRamps ??
                    new List<Texture2D>()).Where(value => value != null &&
                    string.Equals(value.name, reference.TextureName,
                        StringComparison.Ordinal)).ToArray();
                if (matches.Length != 1 || !IsNativeRamp(matches[0]))
                {
                    failure = reference.TextureName +
                        " missing, duplicate, or incompatible";
                    palette.Clear();
                    return false;
                }
                palette.Add(matches[0]);
            }
            if (palette.Count != ElementalRaceVisualCatalog.SkinRampCount ||
                palette.Distinct().Count() != palette.Count)
            {
                failure = "palette count or texture identity drift";
                palette.Clear();
                return false;
            }
            return true;
        }

        private static List<Texture2D> NormalizeFallbackPalette(
            IList<Texture2D> available, int required)
        {
            Texture2D[] valid = available == null ? Array.Empty<Texture2D>() :
                available.Where(IsNativeRamp).ToArray();
            if (valid.Length == 0)
                throw new InvalidOperationException(
                    "The native Aasimar fallback has no compatible skin ramps.");
            var result = new List<Texture2D>(required);
            for (int index = 0; index < required; index++)
                result.Add(valid[index % valid.Length]);
            return result;
        }

        private static bool TryResolveOptions(
            ElementalRaceSexVisualDefinition definition,
            out ResolvedOptions options, out string failure)
        {
            options = null;
            failure = string.Empty;
            string[] hair;
            string[] eyebrows;
            string[] beards;
            if (!TryResolveAssets(definition.Hair, out hair, out failure) ||
                hair.Count(value => !string.Equals(value,
                    ElementalRaceVisualCatalog.EmptyAssetId,
                    StringComparison.Ordinal)) < 4 ||
                !TryResolveAssets(definition.Eyebrows, out eyebrows,
                    out failure) ||
                !TryResolveAssets(definition.Beards, out beards, out failure))
                return false;
            options = new ResolvedOptions
            {
                Hair = hair,
                Eyebrows = eyebrows,
                Beards = beards
            };
            return true;
        }

        private static bool TryResolveAssets(
            IEnumerable<ElementalRaceNativeVisualAsset> assets,
            out string[] ids, out string failure)
        {
            var result = new List<string>();
            failure = string.Empty;
            foreach (ElementalRaceNativeVisualAsset asset in assets)
            {
                EquipmentEntity ignored;
                if (!TryResolveExact(asset, out ignored, out failure))
                {
                    ids = Array.Empty<string>();
                    return false;
                }
                result.Add(asset.AssetId);
            }
            ids = result.ToArray();
            return true;
        }

        private static ResolvedOptions ResolveFallbackOptions(
            CustomizationOptions options, bool male)
        {
            if (options == null)
                throw new InvalidOperationException(
                    "Aasimar customization fallback is unavailable.");
            string[] hair = ResolveFallbackLinks(options.Hair, 4,
                "Aasimar hair");
            string[] eyebrows = ResolveFallbackLinks(options.Eyebrows, 1,
                "Aasimar eyebrows");
            string[] beards = male ? ResolveFallbackLinks(options.Beards, 0,
                "Aasimar beards") : Array.Empty<string>();
            return new ResolvedOptions
            {
                Hair = hair,
                Eyebrows = eyebrows,
                Beards = beards
            };
        }

        private static string[] ResolveFallbackLinks(
            IEnumerable<EquipmentEntityLink> links, int required,
            string role)
        {
            var result = new List<string>();
            foreach (EquipmentEntityLink link in links ??
                Array.Empty<EquipmentEntityLink>())
            {
                if (link == null || string.IsNullOrWhiteSpace(link.AssetId))
                    continue;
                EquipmentEntity resource = ResourcesLibrary.TryGetResource<
                    EquipmentEntity>(link.AssetId, false);
                if (resource != null) result.Add(link.AssetId);
            }
            if (result.Count < required)
                throw new InvalidOperationException(role +
                    " fallback does not meet required breadth.");
            return result.ToArray();
        }

        private static EquipmentEntity RequireExact(
            ElementalRaceNativeVisualAsset asset, string role)
        {
            EquipmentEntity result;
            string failure;
            if (!TryResolveExact(asset, out result, out failure))
                throw new InvalidOperationException(role + " failed: " +
                    failure + ".");
            return result;
        }

        private static bool TryResolveExact(
            ElementalRaceNativeVisualAsset asset,
            out EquipmentEntity result, out string failure)
        {
            result = null;
            failure = string.Empty;
            try
            {
                result = ResourcesLibrary.TryGetResource<EquipmentEntity>(
                    asset.AssetId, true);
            }
            catch (Exception exception)
            {
                failure = asset.AssetId + " threw " +
                    exception.GetType().Name;
                return false;
            }
            if (result == null)
            {
                failure = asset.AssetId + " did not resolve";
                return false;
            }
            if (!string.Equals(result.name, asset.ExpectedName,
                StringComparison.Ordinal))
            {
                failure = asset.AssetId + " resolved as " + result.name +
                    " instead of " + asset.ExpectedName;
                result = null;
                return false;
            }
            return true;
        }

        private static bool IsNativeRamp(Texture2D texture)
        {
            return texture != null && texture.width == 256 &&
                texture.height == 1 && texture.format == TextureFormat.RGB24 &&
                texture.filterMode == FilterMode.Bilinear &&
                texture.wrapMode == TextureWrapMode.Clamp;
        }

        private static BlueprintRaceVisualPreset[] RequirePresetDonors(
            LibraryScriptableObject library, BlueprintRace aasimar)
        {
            var result = new BlueprintRaceVisualPreset[AasimarPresetGuids.Length];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = BlueprintLibraryLookup.RequireExact<
                    BlueprintRaceVisualPreset>(library,
                        AasimarPresetGuids[index],
                        "native Aasimar Human-compatible visual preset");
                if (result[index].Skin == null ||
                    result[index].MaleSkeleton == null ||
                    result[index].FemaleSkeleton == null ||
                    !(aasimar.Presets ??
                        Array.Empty<BlueprintRaceVisualPreset>()).Any(value =>
                            ReferenceEquals(value, result[index])))
                    throw new InvalidOperationException(
                        "Installed Aasimar visual preset contract changed for " +
                        AasimarPresetGuids[index] + ".");
            }
            if (result.Select(value => value.RaceId).Distinct().Count() != 1)
                throw new InvalidOperationException(
                    "Installed Aasimar visual presets no longer share one doll RaceId.");
            return result;
        }

        private static FieldInfo RequireField(Type owner, string name,
            Type fieldType)
        {
            FieldInfo field = owner.GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != fieldType)
                throw new MissingFieldException(owner.FullName, name);
            return field;
        }

        private static FieldInfo RequireArrayField(Type owner, string name)
        {
            FieldInfo field = owner.GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || !field.FieldType.IsArray)
                throw new MissingFieldException(owner.FullName, name);
            return field;
        }

        private sealed class ResolvedDonor
        {
            internal EquipmentEntity Resource;
            internal bool UsedFallback;
        }

        private sealed class ResolvedOptions
        {
            internal string[] Hair;
            internal string[] Eyebrows;
            internal string[] Beards;
        }

        private sealed class ResolvedPlan
        {
            internal ElementalRaceVisualDefinition Definition;
            internal List<Texture2D> SkinPalette;
            internal Dictionary<string, ResolvedDonor> Donors;
            internal ResolvedOptions Male;
            internal ResolvedOptions Female;
            internal bool UsedFallback;
            internal string FallbackReason;

            internal ResolvedDonor RequireDonor(string symbol)
            {
                ResolvedDonor result;
                if (Donors == null || !Donors.TryGetValue(symbol, out result))
                    throw new InvalidOperationException(
                        "No resolved visual donor exists for " + symbol + ".");
                return result;
            }
        }
    }
}
