using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.AidAnotherCompatibility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using UnityModManagerNet;

namespace KingmakerGunslinger.EasternWeapons
{
    /// <summary>
    /// Defers only the native broad-martial array mutation until UMM's first
    /// update, which is after the complete LoadDictionary postfix chain.
    /// Foreign blueprint builders can safely enumerate the native category
    /// arrays before KMG adds its runtime-only Nodachi category.
    /// </summary>
    internal static class EasternWeaponLatePublicationCoordinator
    {
        private static readonly object Gate = new object();
        private static ModContext _context;
        private static EasternWeaponMartialPublication _publication;
        private static HelpfulPublicationTransaction _heirloomPublication;
        private static TweakOrTreatHeirloomContract _tweakOrTreat;
        private static bool _attached;
        private static bool _attempted;
        private static int _earlyNodachiCount = -1;
        private static string _failedCheck = string.Empty;
        private static string _heirloomFailedCheck = string.Empty;

        internal static bool Published
        { get { lock (Gate) return _publication != null; } }

        internal static bool Attempted
        { get { lock (Gate) return _attempted; } }

        internal static int EarlyNodachiCount
        { get { lock (Gate) return _earlyNodachiCount; } }

        internal static string FailedCheck
        { get { lock (Gate) return _failedCheck; } }

        internal static bool HeirloomPublished
        { get { lock (Gate) return _heirloomPublication != null; } }

        internal static string HeirloomFailedCheck
        { get { lock (Gate) return _heirloomFailedCheck; } }

        internal static TweakOrTreatHeirloomContract TweakOrTreatContract
        { get { lock (Gate) return _tweakOrTreat; } }

        internal static EasternWeaponMartialPublication Publication
        { get { lock (Gate) return _publication; } }

        internal static void AttachFirstUpdate(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            lock (Gate)
            {
                _context = context;
                if (_attached || _publication != null) return;
                _attached = true;
            }
            context.ModEntry.OnUpdate += FirstUpdate;
            context.Logger.Info("eastern-weapons-compatibility",
                "late-publication.attached",
                "phase=first-umm-update-after-LoadDictionary-postfix-chain;native-array-observation=deferred-with-publication");
        }

        private static void FirstUpdate(UnityModManager.ModEntry entry,
            float delta)
        {
            ModContext context;
            lock (Gate)
            {
                context = _context;
                _attached = false;
            }
            if (context != null) context.ModEntry.OnUpdate -= FirstUpdate;
            TryPublish("first-update-after-load-dictionary");
        }

        internal static bool TryPublish(string checkpoint)
        {
            ModContext context;
            EasternWeaponMartialPublication existing;
            EasternWeaponMartialPublication publication = null;
            lock (Gate)
            {
                context = _context;
                existing = _publication;
                _attempted = true;
            }
            try
            {
                if (existing != null)
                {
                    publication = existing;
                    existing.Validate();
                    LogInfo(context, "late-publication.idempotent",
                        "checkpoint=" + checkpoint + ";broadFacts=" +
                        existing.BroadFacts.Length +
                        ";nativeAuthority=" +
                        existing.NativeCategoryCountBeforeNodachi);
                    TryPublishHeirloom(context, checkpoint);
                    return true;
                }
                var library = BlueprintBootstrap.Library;
                if (library == null) throw new InvalidOperationException(
                    "KMG blueprint library was unavailable in the late publication phase.");
                int early = EasternWeaponMartialPublication
                    .CountNodachiOnNative(library);
                lock (Gate) _earlyNodachiCount = early;
                if (early != 0) throw new InvalidOperationException(
                    "Nodachi was present in native broad-martial proficiency before KMG's late publication transaction.");
                publication = EasternWeaponMartialPublication.Publish(library);
                lock (Gate)
                {
                    _publication = publication;
                    _failedCheck = string.Empty;
                }
                LogInfo(context, "late-publication.complete",
                    "checkpoint=" + checkpoint + ";earlyNativeNodachiCount=" +
                    EarlyNodachiCount + ";nativeNodachiCount=" +
                    EasternWeaponMartialPublication.CountNodachiOnNative(
                        library) + ";broadFacts=" +
                    publication.BroadFacts.Length + ";mutatedFacts=" +
                    publication.MutatedFactCount + ";nativeAuthority=" +
                    publication.NativeCategoryCountBeforeNodachi);
                TryPublishHeirloom(context, checkpoint);
                return true;
            }
            catch (Exception exception)
            {
                EasternWeaponMartialPublication rollback = publication;
                lock (Gate)
                {
                    if (ReferenceEquals(_publication, publication))
                        _publication = null;
                    _failedCheck = "late-martial-publication:" +
                        exception.GetType().Name;
                }
                if (rollback != null)
                    try { rollback.Rollback(); }
                    catch (Exception rollbackException)
                    {
                        exception = new AggregateException(exception,
                            rollbackException);
                    }
                if (context != null)
                    context.Logger.Failure("eastern-weapons-compatibility",
                        "late-publication.blocked",
                        "checkpoint=" + checkpoint + ";failedCheck=" +
                        FailedCheck +
                        ";Nodachi broad-martial compatibility remained fail-closed; unrelated KMG modules remain active.",
                        exception);
                return false;
            }
        }

        private static void TryPublishHeirloom(ModContext context,
            string checkpoint)
        {
            try
            {
                FavoredClassTraitContract favored =
                    AidAnotherOptionalExtensionCoordinator
                        .FavoredClassContract;
                if (favored == null)
                {
                    lock (Gate)
                    {
                        _tweakOrTreat = null;
                        _heirloomFailedCheck =
                            "favored-class-compatible-contract-absent";
                    }
                    LogInfo(context, "heirloom.not-published",
                        "checkpoint=" + checkpoint +
                        ";reason=favored-class-compatible-contract-absent");
                    return;
                }

                UnityModManager.ModEntry tweakEntry = Single(ReadEntries(
                    context == null ? null : context.ModEntry),
                    TweakOrTreatHeirloomResolver.ModId);
                AidAnotherContractResolution<TweakOrTreatHeirloomContract>
                    tweak = TweakOrTreatHeirloomResolver.Resolve(tweakEntry,
                        favored);
                if (tweak.Availability !=
                        OptionalAidAnotherAvailability.Absent &&
                    !tweak.IsCompatible)
                {
                    lock (Gate)
                    {
                        _tweakOrTreat = null;
                        _heirloomFailedCheck = tweak.FailedCheck;
                    }
                    if (context != null)
                        context.Logger.Warning(
                            "eastern-weapons-compatibility",
                            "heirloom.blocked",
                            "checkpoint=" + checkpoint + ";failedCheck=" +
                            tweak.FailedCheck +
                            ";essential late martial publication remains active.");
                    return;
                }
                lock (Gate) _tweakOrTreat = tweak.Contract;

                EasternWeaponBlueprintSet eastern =
                    BlueprintBootstrap.EasternWeapons;
                if (eastern == null || eastern.HeirloomNodachi == null)
                    throw new InvalidOperationException(
                        "The registered Heirloom Weapon (Nodachi) identities were unavailable.");
                bool requested = favored.TraitsEnabled && context != null &&
                    context.FeatureModules.Active.EasternWeapons;
                if (!requested)
                {
                    ValidateHeirloomCount(favored, eastern.HeirloomNodachi,
                        false);
                    lock (Gate) _heirloomFailedCheck = favored.TraitsEnabled ?
                        "eastern-weapons-module-off" :
                        "favored-class-traits-disabled";
                    LogInfo(context, "heirloom.not-published",
                        "checkpoint=" + checkpoint + ";traitsEnabled=" +
                        favored.TraitsEnabled + ";easternModule=" +
                        (context != null && context.FeatureModules.Active
                            .EasternWeapons));
                    return;
                }

                HelpfulPublicationTransaction existing;
                lock (Gate) existing = _heirloomPublication;
                if (existing == null)
                {
                    existing = new HelpfulPublicationTransaction().Append(
                        "favored-equipment-traits-heirloom-nodachi",
                        () => favored.EquipmentTraits.AllFeatures,
                        values => favored.EquipmentTraits.AllFeatures = values,
                        eastern.HeirloomNodachi.Selection,
                        value => value.AssetGuid, false);
                    existing.Commit();
                    lock (Gate) _heirloomPublication = existing;
                }
                else existing.Commit();
                ValidateHeirloomCount(favored, eastern.HeirloomNodachi, true);
                lock (Gate) _heirloomFailedCheck = string.Empty;
                LogInfo(context, "heirloom.published",
                    "checkpoint=" + checkpoint + ";equipmentSelection=" +
                    favored.EquipmentTraits.AssetGuid + ";nodachiSelection=" +
                    eastern.HeirloomNodachi.Selection.AssetGuid +
                    ";count=1;tweakOrTreat=" +
                    (tweak.IsCompatible ? tweak.Contract.Fingerprint :
                        "absent"));
            }
            catch (Exception exception)
            {
                HelpfulPublicationTransaction publication;
                lock (Gate)
                {
                    publication = _heirloomPublication;
                    _heirloomPublication = null;
                    _heirloomFailedCheck = "heirloom-publication:" +
                        exception.GetType().Name;
                }
                if (publication != null)
                    try { publication.Rollback(); }
                    catch (Exception rollback)
                    { exception = new AggregateException(exception, rollback); }
                if (context != null)
                    context.Logger.Failure(
                        "eastern-weapons-compatibility",
                        "heirloom.blocked",
                        "checkpoint=" + checkpoint + ";failedCheck=" +
                        HeirloomFailedCheck +
                        ";essential late martial publication remains active.",
                        exception);
            }
        }

        private static void ValidateHeirloomCount(
            FavoredClassTraitContract favored,
            HeirloomNodachiBlueprintSet set, bool published)
        {
            BlueprintFeature[] values = favored.EquipmentTraits.AllFeatures ??
                new BlueprintFeature[0];
            int references = values.Count(value => ReferenceEquals(value,
                set.Selection));
            int identities = values.Count(value => value != null &&
                string.Equals(value.AssetGuid, set.Selection.AssetGuid,
                    StringComparison.Ordinal));
            int expected = published ? 1 : 0;
            if (references != expected || identities != expected)
                throw new InvalidOperationException(
                    "Heirloom Weapon (Nodachi) publication count is not exact.");
        }

        private static UnityModManager.ModEntry[] ReadEntries(
            UnityModManager.ModEntry current)
        {
            Type manager = current == null ? null :
                current.GetType().DeclaringType;
            FieldInfo field = manager == null ? null : manager.GetField(
                "modEntries", BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic);
            IEnumerable values = field == null ? null :
                field.GetValue(null) as IEnumerable;
            if (values == null) throw new InvalidOperationException(
                "The live UMM modEntries collection was unavailable.");
            return values.Cast<object>().Select(value =>
                value as UnityModManager.ModEntry).Where(value => value != null)
                .ToArray();
        }

        private static UnityModManager.ModEntry Single(
            UnityModManager.ModEntry[] entries, string id)
        {
            UnityModManager.ModEntry[] matches = entries.Where(value =>
                value.Info != null && string.Equals(value.Info.Id, id,
                    StringComparison.Ordinal)).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException(
                "Expected at most one live UMM entry for " + id +
                " but found " + matches.Length + ".");
            return matches.SingleOrDefault();
        }

        private static void LogInfo(ModContext context, string code,
            string details)
        {
            if (context != null) context.Logger.Info(
                "eastern-weapons-compatibility", code, details);
        }
    }
}
