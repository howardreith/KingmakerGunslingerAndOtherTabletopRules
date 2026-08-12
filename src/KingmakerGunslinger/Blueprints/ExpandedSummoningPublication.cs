using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class ExpandedSummoningPublication
    {
        private readonly List<Record> _records;
        internal ExpandedSummoningPublication(List<Record> records)
        { _records = records; }

        internal void Rollback()
        {
            for (int index = _records.Count - 1; index >= 0; index--)
            {
                Record record = _records[index];
                if (!ReferenceEquals(record.Parent.ComponentsArray,
                    record.PublishedComponents))
                    throw new InvalidOperationException(
                        "Expanded Summoning rollback refused after unrelated mutation: " +
                        record.Parent.AssetGuid);
                record.Parent.ComponentsArray = record.OriginalComponents;
            }
        }

        internal sealed class Record
        {
            internal BlueprintAbility Parent;
            internal BlueprintComponent[] OriginalComponents;
            internal BlueprintComponent[] PublishedComponents;
        }
    }

    internal static class ExpandedSummoningPublisher
    {
        private static readonly string[] MonsterParents = {
            "8fd74eddd9b6c224693d9ab241f25e84", "1724061e89c667045a6891179ee2e8e7",
            "5d61dde0020bbf54ba1521f7ca0229dc", "7ed74a3ec8c458d4fb50b192fd7be6ef",
            "630c8b85d9f07a64f917d79cb5905741", "e740afbab0147944dab35d83faa0ae1c",
            "ab167fd8203c1314bac6568932f1752f", "d3ac756a229830243a72e84f3ab050d0",
            "52b5df2a97df18242aec67610616ded0" };
        private static readonly string[] AllyParents = {
            "c6147854641924442a3bb736080cfeb6", "298148133cdc3fd42889b99c82711986",
            "fdcf7e57ec44f704591f11b45f4acf61", "c83db50513abdf74ca103651931fac4b",
            "8f98a22f35ca6684a983363d32e51bfe", "55bbce9b3e76d4a4a8c8e0698d29002c",
            "051b979e7d7f8ec41b9fa35d04746b33", "ea78c04f0bd13d049a1cce5daf8d83e0",
            "a7469ef84ba50ac4cbf3d145e3173f8e" };

        internal static ExpandedSummoningPublication Publish(
            LibraryScriptableObject library, ExpandedSummoningBlueprintSet set)
        {
            SummonNativeOptionCatalog.Validate();
            var records = new List<ExpandedSummoningPublication.Record>();
            try
            {
                foreach (SummonFamily family in new[] { SummonFamily.Monster,
                    SummonFamily.NaturesAlly })
                for (int tier = 1; tier <= 9; tier++)
                {
                    string guid = (family == SummonFamily.Monster ?
                        MonsterParents : AllyParents)[tier - 1];
                    BlueprintAbility parent = BlueprintLibraryLookup
                        .RequireExact<BlueprintAbility>(library, guid,
                            "required native summon parent");
                    SummonVariantSpec[] specs = ExpandedSummoningCatalog
                        .GenerateVariants(family).Where(value =>
                            value.ParentTier == tier).ToArray();
                    BlueprintAbility[] additions = specs.Select(value =>
                                (BlueprintAbility)set.BySymbol[
                                    ExpandedSummoningIdentityCatalog.AbilitySymbol(value)])
                        .ToArray();
                    BlueprintAbility nativePreservation = tier != 1 ? null :
                        (BlueprintAbility)set.BySymbol[family == SummonFamily.Monster ?
                            ExpandedSummoningIdentityCatalog
                                .NativeMonsterTierOneSymbol :
                            ExpandedSummoningIdentityCatalog
                                .NativeNaturesAllyTierOneSymbol];
                    PublishParent(parent, family, tier, nativePreservation,
                        additions, specs, records);
                }
                return new ExpandedSummoningPublication(records);
            }
            catch
            {
                new ExpandedSummoningPublication(records).Rollback();
                throw;
            }
        }

        internal static bool RequiredBasePublicationIsExact(
            LibraryScriptableObject library, bool expectedEnabled,
            out int referenceCount)
        {
            if (library == null) throw new ArgumentNullException("library");
            referenceCount = 0;
            bool exact = true;
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            for (int tier = 1; tier <= 9; tier++)
            {
                string guid = (family == SummonFamily.Monster ?
                    MonsterParents : AllyParents)[tier - 1];
                BlueprintAbility parent = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(library, guid,
                        "required native summon publication probe parent");
                AbilityVariants variants = (parent.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<AbilityVariants>()
                    .SingleOrDefault();
                BlueprintAbility[] live = variants == null ?
                    Array.Empty<BlueprintAbility>() : variants.Variants ??
                        Array.Empty<BlueprintAbility>();
                foreach (SummonVariantSpec variant in ExpandedSummoningCatalog
                    .GenerateVariants(family).Where(value =>
                        value.ParentTier == tier))
                {
                    string expectedName = ExpandedSummoningIdentityCatalog
                        .AbilitySymbol(variant).Replace('.', '_')
                        .Replace('-', '_');
                    int count = live.Count(value => value != null &&
                        string.Equals(value.name, expectedName,
                            StringComparison.Ordinal));
                    referenceCount += count;
                    if (count != (expectedEnabled ? 1 : 0)) exact = false;
                }
            }
            return exact;
        }

        private static void PublishParent(BlueprintAbility parent,
            SummonFamily family, int tier,
            BlueprintAbility nativePreservation, BlueprintAbility[] additions,
            SummonVariantSpec[] specs,
            List<ExpandedSummoningPublication.Record> records)
        {
            BlueprintComponent[] before = parent.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            AbilityVariants existing = before.OfType<AbilityVariants>()
                .SingleOrDefault();
            if ((existing == null) != (nativePreservation != null))
                throw new InvalidOperationException(
                    "Direct summon publication requires exactly one frozen native-preservation child: " +
                    parent.AssetGuid);
            BlueprintAbility[] originals = existing == null ?
                new[] { nativePreservation } : existing.Variants ??
                    Array.Empty<BlueprintAbility>();
            if (originals.Any(value => value == null) ||
                additions.Any(value => value == null) ||
                originals.Concat(additions).GroupBy(value => value.AssetGuid,
                    StringComparer.Ordinal).Any(group => group.Count() > 1))
                throw new InvalidOperationException(
                    "Expanded Summoning parent contains a null or duplicate GUID: " +
                    parent.AssetGuid);
            var additionSpecs = additions.Select((ability, index) => new {
                Ability = ability, Spec = specs[index] }).ToDictionary(
                    value => value.Ability.AssetGuid, value => value.Spec,
                    StringComparer.Ordinal);
            BlueprintAbility[] preservedOriginals = originals.Where(original =>
            {
                SummonNativeOptionSpec native = SummonNativeOptionCatalog.Find(
                    family, tier, original.AssetGuid);
                if (native == null || !native.IsSemanticDuplicate) return true;
                int matches = specs.Count(value => value.Creature.Key ==
                    native.EquivalentCreatureKey && value.Multiplicity ==
                    native.Multiplicity);
                if (matches != 1) throw new InvalidOperationException(
                    "Native duplicate map did not resolve exactly one KMG option: " +
                    native.Guid);
                return false;
            }).ToArray();
            IReadOnlyList<BlueprintAbility> merged = SummonDisplayOrderPolicy.Order(
                preservedOriginals, additions,
                value => {
                    SummonNativeOptionSpec native = SummonNativeOptionCatalog.Find(
                        family, tier, value.AssetGuid);
                    return native == null ? (SummonMultiplicity?)null :
                        native.Multiplicity;
                }, value => additionSpecs[value.AssetGuid].Multiplicity);
            var variants = ScriptableObject.CreateInstance<AbilityVariants>();
            variants.name = "$KMG_ExpandedSummoning_Variants";
            variants.Variants = merged.ToArray();
            BlueprintComponent[] after = existing == null ?
                before.Concat(new BlueprintComponent[] { variants }).ToArray() :
                before.Select(value => ReferenceEquals(value, existing) ?
                    (BlueprintComponent)variants : value).ToArray();
            records.Add(new ExpandedSummoningPublication.Record {
                Parent = parent, OriginalComponents = before,
                PublishedComponents = after });
            parent.ComponentsArray = after;
            if (!ReferenceEquals(parent.ComponentsArray, after) ||
                additions.Any(addition => variants.Variants.Count(value =>
                    value.AssetGuid == addition.AssetGuid) != 1) ||
                preservedOriginals.Any(original =>
                    !variants.Variants.Contains(original)) ||
                originals.Where(original => !preservedOriginals.Contains(original))
                    .Any(original => variants.Variants.Contains(original)))
                throw new InvalidOperationException(
                    "Expanded Summoning parent publication validation failed: " +
                    parent.AssetGuid);
        }
    }
}
