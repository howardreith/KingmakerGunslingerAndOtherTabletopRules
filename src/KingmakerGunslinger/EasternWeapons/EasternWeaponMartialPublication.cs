using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.EasternWeapons
{
    /// <summary>
    /// Transactionally appends Nodachi to every exact broad-martial grant.
    /// This transaction is deliberately invoked only after every mod's
    /// LoadDictionary postfix has returned; foreign trait builders therefore
    /// never observe KMG's runtime-only WeaponCategory while deriving IDs.
    /// </summary>
    internal sealed class EasternWeaponMartialPublication
    {
        private readonly BlueprintFeature _nativeMartial;
        private readonly BlueprintFeature[] _broadFacts;
        private readonly Dictionary<BlueprintFeature, BlueprintComponent[]>
            _originals;
        private readonly int[] _authority;
        private bool _rolledBack;

        private EasternWeaponMartialPublication(BlueprintFeature nativeMartial,
            BlueprintFeature[] broadFacts,
            Dictionary<BlueprintFeature, BlueprintComponent[]> originals,
            int[] authority)
        {
            _nativeMartial = nativeMartial;
            _broadFacts = broadFacts;
            _originals = originals;
            _authority = authority;
        }

        internal BlueprintFeature[] BroadFacts
        { get { return (BlueprintFeature[])_broadFacts.Clone(); } }

        internal int NativeCategoryCountBeforeNodachi
        { get { return _authority.Length; } }

        internal int MutatedFactCount { get { return _originals.Count; } }

        internal static int CountNodachiOnNative(
            LibraryScriptableObject library)
        {
            BlueprintFeature native = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library,
                    EasternWeaponBlueprints.NativeMartialWeaponProficiencyGuid,
                    "native Martial Weapon Proficiency feature");
            AddProficiencies grant = RequireAuthorityGrant(native);
            return (grant.WeaponProficiencies ??
                Array.Empty<WeaponCategory>()).Count(value =>
                    (int)value == EasternWeaponMartialPublicationPolicy
                        .NodachiCategoryValue);
        }

        internal static EasternWeaponMartialPublication Publish(
            LibraryScriptableObject library)
        {
            if (library == null) throw new ArgumentNullException("library");
            BlueprintFeature native = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library,
                    EasternWeaponBlueprints.NativeMartialWeaponProficiencyGuid,
                    "native Martial Weapon Proficiency feature");
            AddProficiencies nativeGrant = RequireAuthorityGrant(native);
            int[] authority = EasternWeaponMartialPublicationPolicy
                .NormalizeAuthority((nativeGrant.WeaponProficiencies ??
                    Array.Empty<WeaponCategory>()).Select(value => (int)value));
            if (authority.Length < EasternWeaponMartialPublicationPolicy
                    .MinimumNativeMartialCategoryCount)
                throw new InvalidOperationException(
                    "Native Martial Weapon Proficiency category authority changed.");

            var originals = new Dictionary<BlueprintFeature,
                BlueprintComponent[]>();
            var broad = new List<BlueprintFeature>();
            try
            {
                foreach (BlueprintFeature feature in library.GetAllBlueprints()
                    .OfType<BlueprintFeature>().Where(value => value != null))
                {
                    BlueprintComponent[] components = feature.ComponentsArray ??
                        Array.Empty<BlueprintComponent>();
                    AddProficiencies[] grants = components.OfType<
                        AddProficiencies>().Where(value =>
                            EasternWeaponMartialPublicationPolicy.IsBroadGrant(
                                authority, (value.WeaponProficiencies ??
                                    Array.Empty<WeaponCategory>()).Select(
                                        category => (int)category))).ToArray();
                    if (grants.Length == 0) continue;
                    broad.Add(feature);
                    BlueprintComponent[] next =
                        (BlueprintComponent[])components.Clone();
                    bool changed = false;
                    foreach (AddProficiencies grant in grants)
                    {
                        int[] categories = (grant.WeaponProficiencies ??
                            Array.Empty<WeaponCategory>()).Select(value =>
                                (int)value).ToArray();
                        int[] normalized = EasternWeaponMartialPublicationPolicy
                            .AppendNodachiExactlyOnce(categories);
                        if (normalized.SequenceEqual(categories)) continue;
                        var replacement = (AddProficiencies)
                            UnityEngine.Object.Instantiate(grant);
                        replacement.WeaponProficiencies = normalized.Select(
                            value => (WeaponCategory)value).ToArray();
                        next[Array.IndexOf(components, grant)] = replacement;
                        changed = true;
                    }
                    if (!changed) continue;
                    originals.Add(feature, components);
                    feature.ComponentsArray = next;
                }

                BlueprintFeature[] facts = broad.Distinct().ToArray();
                if (!facts.Contains(native))
                    throw new InvalidOperationException(
                        "Native Martial Weapon Proficiency was not classified as broad.");
                var publication = new EasternWeaponMartialPublication(native,
                    facts, originals, authority);
                publication.Validate();
                EasternWeaponProficiencyRuntime.Configure(facts);
                return publication;
            }
            catch
            {
                foreach (KeyValuePair<BlueprintFeature, BlueprintComponent[]>
                    entry in originals) entry.Key.ComponentsArray = entry.Value;
                EasternWeaponProficiencyRuntime.Rollback();
                throw;
            }
        }

        internal void Validate()
        {
            if (_rolledBack) throw new InvalidOperationException(
                "A rolled-back martial publication cannot be validated.");
            if (_broadFacts.Length == 0 || !_broadFacts.Contains(_nativeMartial))
                throw new InvalidOperationException(
                    "The broad-martial fact catalog is incomplete.");
            foreach (BlueprintFeature feature in _broadFacts)
            {
                AddProficiencies[] broad = (feature.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<AddProficiencies>()
                    .Where(value => EasternWeaponMartialPublicationPolicy
                        .IsBroadGrant(_authority,
                            (value.WeaponProficiencies ??
                                Array.Empty<WeaponCategory>()).Select(category =>
                                    (int)category))).ToArray();
                if (broad.Length == 0 || broad.Any(value =>
                    (value.WeaponProficiencies ??
                        Array.Empty<WeaponCategory>()).Count(category =>
                            (int)category ==
                            EasternWeaponMartialPublicationPolicy
                                .NodachiCategoryValue) != 1))
                    throw new InvalidOperationException(
                        "A broad martial grant did not retain exactly one Nodachi category.");
            }
        }

        internal void Rollback()
        {
            if (_rolledBack) return;
            foreach (KeyValuePair<BlueprintFeature, BlueprintComponent[]> entry
                in _originals) entry.Key.ComponentsArray = entry.Value;
            EasternWeaponProficiencyRuntime.Rollback();
            _rolledBack = true;
        }

        private static AddProficiencies RequireAuthorityGrant(
            BlueprintFeature native)
        {
            AddProficiencies[] grants = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddProficiencies>()
                .Where(value => EasternWeaponMartialPublicationPolicy
                    .NormalizeAuthority((value.WeaponProficiencies ??
                        Array.Empty<WeaponCategory>()).Select(category =>
                            (int)category)).Length >=
                        EasternWeaponMartialPublicationPolicy
                            .MinimumNativeMartialCategoryCount)
                .OrderByDescending(value => EasternWeaponMartialPublicationPolicy
                    .NormalizeAuthority((value.WeaponProficiencies ??
                        Array.Empty<WeaponCategory>()).Select(category =>
                            (int)category)).Length).ToArray();
            if (grants.Length == 0)
                throw new InvalidOperationException(
                    "Native Martial Weapon Proficiency has no broad weapon grant.");
            int largest = EasternWeaponMartialPublicationPolicy
                .NormalizeAuthority((grants[0].WeaponProficiencies ??
                    Array.Empty<WeaponCategory>()).Select(category =>
                        (int)category)).Length;
            AddProficiencies[] exact = grants.Where(value =>
                EasternWeaponMartialPublicationPolicy.NormalizeAuthority(
                    (value.WeaponProficiencies ??
                        Array.Empty<WeaponCategory>()).Select(category =>
                            (int)category)).Length == largest).ToArray();
            if (exact.Length != 1)
                throw new InvalidOperationException(
                    "Native Martial Weapon Proficiency broad grant is ambiguous.");
            return exact[0];
        }
    }
}
