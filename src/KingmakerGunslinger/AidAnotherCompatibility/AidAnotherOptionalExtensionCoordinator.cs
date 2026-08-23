using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;
using UnityModManagerNet;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    /// <summary>
    /// Late-bound coordinator for CotW's shared Aid Another configuration and
    /// Favored Class's trait catalog. Lifecycle callbacks request reconciliation;
    /// the first UMM update performs the mutation after all LoadDictionary
    /// postfixes have finished, independent of mod load order.
    /// </summary>
    internal static class AidAnotherOptionalExtensionCoordinator
    {
        private static readonly object Gate = new object();
        private static ModContext _context;
        private static bool _installed;
        private static bool _firstUpdateAttached;
        private static bool _reconciling;
        private static bool _cotwLifecyclePatched;
        private static bool _favoredLifecyclePatched;
        private static HelpfulPublicationTransaction _publication;
        private static CotwAidAnotherContract _cotw;
        private static FavoredClassTraitContract _favored;
        private static int _successfulReconciliations;
        private static int _pendingUpdateRetries;
        private const int MaximumPendingUpdateRetries = 2;

        internal static CotwAidAnotherContract CotwContract
        { get { lock (Gate) return _cotw; } }

        internal static FavoredClassTraitContract FavoredClassContract
        { get { lock (Gate) return _favored; } }

        internal static int SuccessfulReconciliations
        { get { lock (Gate) return _successfulReconciliations; } }

        internal static string[] PublicationEvidence
        {
            get
            {
                lock (Gate) return _publication == null ? new string[0] :
                    _publication.Evidence.ToArray();
            }
        }

        internal static void Install(ModContext context)
        {
            if (context == null) return;
            lock (Gate)
            {
                _context = context;
                if (_installed) return;
                _installed = true;
            }
            AidAnotherCompatibilityStatusRegistry.Update(
                new AidAnotherCompatibilityStatus(
                    OptionalAidAnotherAvailability.Pending,
                    OptionalAidAnotherAvailability.Pending, null, false, false,
                    "awaiting optional-mod LoadDictionary completion"));
            try
            {
                InstallAvailableLifecyclePatches(context);
                AttachFirstUpdate(context);
            }
            catch (Exception exception)
            {
                Block(OptionalAidAnotherAvailability.Blocked,
                    OptionalAidAnotherAvailability.Blocked, null,
                    "coordinator-install-exception", exception);
            }
        }

        private static void AfterCotwAidAnotherCreation()
        { RequestReconcile("cotw-create-aid-postfix"); }

        private static void AfterFavoredTraitsLoad()
        { RequestReconcile("favored-traits-load-postfix"); }

        private static void FirstUpdate(UnityModManager.ModEntry entry,
            float delta)
        {
            ModContext context;
            lock (Gate)
            {
                context = _context;
                _firstUpdateAttached = false;
            }
            if (context != null) context.ModEntry.OnUpdate -= FirstUpdate;
            try { InstallAvailableLifecyclePatches(context); }
            catch (Exception exception)
            {
                AidAnotherCompatibilityDiagnostics.Failure(context,
                    "lifecycle.patch-failed",
                    "Optional lifecycle diagnostics could not be attached; first-update reconciliation remains authoritative.",
                    exception);
            }
            TryReconcile("first-update-after-load-dictionary");
        }

        private static void AttachFirstUpdate(ModContext context)
        {
            lock (Gate)
            {
                if (_firstUpdateAttached) return;
                _firstUpdateAttached = true;
            }
            context.ModEntry.OnUpdate += FirstUpdate;
        }

        private static void RequestReconcile(string checkpoint)
        {
            bool waiting;
            ModContext context;
            lock (Gate)
            {
                waiting = _firstUpdateAttached;
                context = _context;
            }
            if (waiting)
            {
                AidAnotherCompatibilityDiagnostics.Info(context,
                    "lifecycle.reconcile-requested",
                    "checkpoint=" + checkpoint +
                    ";deferredUntil=first-update-after-load-dictionary");
                return;
            }
            TryReconcile(checkpoint);
        }

        private static void TryReconcile(string checkpoint)
        {
            ModContext context;
            lock (Gate)
            {
                if (_reconciling) return;
                _reconciling = true;
                context = _context;
            }
            try
            {
                UnityModManager.ModEntry[] entries = ReadEntries(
                    context == null ? null : context.ModEntry);
                UnityModManager.ModEntry cotwEntry = Single(entries,
                    CotwAidAnotherResolver.ModId);
                UnityModManager.ModEntry favoredEntry = Single(entries,
                    FavoredClassTraitResolver.ModId);
                BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
                if (set == null)
                {
                    if (!RetryPendingOrBlock(context, checkpoint,
                            "kmg-helpful-not-registered",
                            OptionalAidAnotherAvailability.Pending,
                            OptionalAidAnotherAvailability.Pending))
                        return;
                    AidAnotherCompatibilityStatusRegistry.Update(
                        new AidAnotherCompatibilityStatus(
                            OptionalAidAnotherAvailability.Pending,
                            OptionalAidAnotherAvailability.Pending, null, false,
                            false, "KMG Helpful identity is awaiting blueprint registration"));
                    AttachFirstUpdate(context);
                    return;
                }

                AidAnotherContractResolution<CotwAidAnotherContract> cotw =
                    CotwAidAnotherResolver.Resolve(cotwEntry);
                AidAnotherContractResolution<FavoredClassTraitContract> favored =
                    FavoredClassTraitResolver.Resolve(favoredEntry,
                        set.HelpfulCombat);
                if (cotw.Availability == OptionalAidAnotherAvailability.Pending ||
                    favored.Availability == OptionalAidAnotherAvailability.Pending)
                {
                    string pendingCheck = cotw.Availability ==
                        OptionalAidAnotherAvailability.Pending ?
                        cotw.FailedCheck : favored.FailedCheck;
                    if (!RetryPendingOrBlock(context, checkpoint, pendingCheck,
                            cotw.Availability, favored.Availability))
                        return;
                    AidAnotherCompatibilityStatusRegistry.Update(
                        new AidAnotherCompatibilityStatus(cotw.Availability,
                            favored.Availability, null, false, false,
                            "checkpoint=" + checkpoint + ";optional blueprints are not ready; reconciliation will retry on the next update"));
                    AttachFirstUpdate(context);
                    return;
                }
                if (cotw.Availability == OptionalAidAnotherAvailability.Absent)
                {
                    lock (Gate) _pendingUpdateRetries = 0;
                    AidAnotherGrantRuntime.Configure(null, null);
                    OptionalAidAnotherAvailability favoredState = favoredEntry ==
                        null ? OptionalAidAnotherAvailability.Absent :
                            OptionalAidAnotherAvailability.Blocked;
                    AidAnotherCompatibilityStatusRegistry.Update(
                        new AidAnotherCompatibilityStatus(
                            OptionalAidAnotherAvailability.Absent, favoredState,
                            null, false, false,
                            "Call of the Wild absent; Bodyguard uses standalone +2 and no Helpful trait is published."));
                    AidAnotherCompatibilityDiagnostics.Info(context,
                        "dependency.cotw-absent",
                        "checkpoint=" + checkpoint +
                        ";standalone Bodyguard remains active; no foreign surface was mutated.");
                    return;
                }
                if (!cotw.IsCompatible)
                {
                    Block(cotw.Availability,
                        favored.Availability, null, cotw.FailedCheck, null);
                    return;
                }
                if (favoredEntry != null && !favored.IsCompatible)
                {
                    Block(OptionalAidAnotherAvailability.Compatible,
                        favored.Availability, null, favored.FailedCheck, null);
                    return;
                }

                EnsurePublished(context, cotw.Contract,
                    favored.IsCompatible ? favored.Contract : null, set,
                    checkpoint);
                lock (Gate)
                {
                    _cotw = cotw.Contract;
                    _favored = favored.IsCompatible ? favored.Contract : null;
                    _successfulReconciliations++;
                    _pendingUpdateRetries = 0;
                }
                AidAnotherGrantRuntime.Configure(cotw.Contract,
                    favored.IsCompatible ? favored.Contract.HalflingHelpful :
                        null);
                EasternWeaponLatePublicationCoordinator.TryPublish(
                    "aid-another-compatible-reconcile");
                bool published = favored.IsCompatible &&
                    favored.Contract.TraitsEnabled &&
                    context.FeatureModules.Active.BodyguardFeats;
                AidAnotherCompatibilityStatusRegistry.Update(
                    new AidAnotherCompatibilityStatus(
                        OptionalAidAnotherAvailability.Compatible,
                        favored.IsCompatible ?
                            OptionalAidAnotherAvailability.Compatible :
                            OptionalAidAnotherAvailability.Absent,
                        favored.IsCompatible ? (bool?)favored.Contract
                            .TraitsEnabled : null, true, published,
                        "checkpoint=" + checkpoint + ";" +
                        cotw.Contract.Fingerprint + ";favored=" +
                        (favored.IsCompatible ? favored.Contract.Fingerprint :
                            "absent") + ";moduleActive=" +
                        context.FeatureModules.Active.BodyguardFeats));
                AidAnotherCompatibilityDiagnostics.Info(context,
                    "contract.compatible",
                    "checkpoint=" + checkpoint + ";helpfulPublished=" +
                    published + ";" + cotw.Contract.Fingerprint +
                    (favored.IsCompatible ? ";" +
                        favored.Contract.Fingerprint :
                        ";favoredClass=absent"));
            }
            catch (Exception exception)
            {
                Block(OptionalAidAnotherAvailability.Blocked,
                    OptionalAidAnotherAvailability.Blocked, null,
                    "reconcile-exception", exception);
            }
            finally
            {
                lock (Gate) _reconciling = false;
            }
        }

        private static bool RetryPendingOrBlock(ModContext context,
            string checkpoint, string failedCheck,
            OptionalAidAnotherAvailability cotwAvailability,
            OptionalAidAnotherAvailability favoredAvailability)
        {
            int attempt;
            lock (Gate) attempt = ++_pendingUpdateRetries;
            if (attempt <= MaximumPendingUpdateRetries) return true;
            Block(cotwAvailability == OptionalAidAnotherAvailability.Pending ?
                    OptionalAidAnotherAvailability.Blocked : cotwAvailability,
                favoredAvailability == OptionalAidAnotherAvailability.Pending ?
                    OptionalAidAnotherAvailability.Blocked : favoredAvailability,
                null, "pending-contract-timeout:" + failedCheck, null);
            AidAnotherCompatibilityDiagnostics.Warning(context,
                "lifecycle.pending-blocked", "checkpoint=" + checkpoint +
                ";attempts=" + attempt +
                ";a later exact lifecycle callback may reconcile safely if the optional mod completes initialization.");
            return false;
        }

        private static void EnsurePublished(ModContext context,
            CotwAidAnotherContract cotw, FavoredClassTraitContract favored,
            BodyguardFeatBlueprintSet set, string checkpoint)
        {
            HelpfulPublicationTransaction existing;
            lock (Gate) existing = _publication;
            if (existing != null)
            {
                existing.Commit();
                ValidatePublication(cotw, favored, set,
                    favored != null && favored.TraitsEnabled &&
                    context.FeatureModules.Active.BodyguardFeats);
                AidAnotherCompatibilityDiagnostics.Info(context,
                    "publication.idempotent", "checkpoint=" + checkpoint);
                return;
            }

            BlueprintFeature[] beforeContributors = cotw.ReadFeatureList();
            if (beforeContributors == null)
                throw new InvalidOperationException(
                    "CotW canonical Aid Another feature list is null.");
            int halflingMultiplicity = beforeContributors.Count(value =>
                value != null && string.Equals(value.AssetGuid,
                    FavoredClassTraitResolver.HalflingHelpfulGuid,
                    StringComparison.Ordinal));
            if (favored == null && halflingMultiplicity != 0 ||
                favored != null && (halflingMultiplicity != 2 ||
                    beforeContributors.Count(value => ReferenceEquals(value,
                        favored.HalflingHelpful)) != 2))
                throw new InvalidOperationException(
                    "Favored Class halfling Helpful does not have its exact canonical multiplicity of two.");

            bool publishTrait = favored != null && favored.TraitsEnabled &&
                context.FeatureModules.Active.BodyguardFeats;
            var transaction = new HelpfulPublicationTransaction()
                .Append("cotw-aid-another-feature-list",
                    cotw.ReadFeatureList, cotw.WriteFeatureList,
                    set.HelpfulCombat, FeatureIdentity, true);
            if (favored != null)
            {
                PrerequisiteNoFeature kmgExclusion = CreateExclusion(
                    favored.HalflingHelpful,
                    "$KMG_HelpfulCombat_NoHalflingHelpful");
                PrerequisiteNoFeature favoredExclusion = CreateExclusion(
                    set.HelpfulCombat,
                    "$KMG_HalflingHelpful_NoCombatHelpful");
                transaction.Append("kmg-helpful-components",
                    () => set.HelpfulCombat.ComponentsArray,
                    value => set.HelpfulCombat.ComponentsArray = value,
                    kmgExclusion, ComponentIdentity, true)
                    .Append("favored-helpful-components",
                        () => favored.HalflingHelpful.ComponentsArray,
                        value => favored.HalflingHelpful.ComponentsArray = value,
                        favoredExclusion, ComponentIdentity, true);
                if (publishTrait)
                {
                    transaction.Append("favored-combat-features",
                        () => favored.CombatTraits.Features,
                        value => favored.CombatTraits.Features = value,
                        set.HelpfulCombat, FeatureIdentity, false)
                        .Append("favored-combat-all-features",
                            () => favored.CombatTraits.AllFeatures,
                            value => favored.CombatTraits.AllFeatures = value,
                            set.HelpfulCombat, FeatureIdentity, false);
                }
            }
            lock (Gate) _publication = transaction;
            try
            {
                transaction.Commit();
                ValidatePublication(cotw, favored, set, publishTrait);
                AidAnotherCompatibilityDiagnostics.Info(context,
                    "publication.complete", "checkpoint=" + checkpoint + ";" +
                    string.Join("|", transaction.Evidence.ToArray()));
            }
            catch
            {
                lock (Gate) _publication = null;
                throw;
            }
        }

        private static void ValidatePublication(CotwAidAnotherContract cotw,
            FavoredClassTraitContract favored, BodyguardFeatBlueprintSet set,
            bool publishTrait)
        {
            BlueprintFeature[] contributors = cotw.ReadFeatureList();
            if (contributors == null || contributors.Count(value =>
                    ReferenceEquals(value, set.HelpfulCombat)) != 1 ||
                contributors.Count(value => value != null && string.Equals(
                    value.AssetGuid, set.HelpfulCombat.AssetGuid,
                    StringComparison.Ordinal)) != 1)
                throw new InvalidOperationException(
                    "KMG combat Helpful is not present exactly once in CotW's canonical Aid Another feature list.");
            if (favored == null) return;
            if (CountExclusions(set.HelpfulCombat.ComponentsArray,
                    favored.HalflingHelpful) != 1 ||
                CountExclusions(favored.HalflingHelpful.ComponentsArray,
                    set.HelpfulCombat) != 1)
                throw new InvalidOperationException(
                    "Helpful reciprocal selection exclusions are not exact.");
            int features = Count(favored.CombatTraits.Features,
                set.HelpfulCombat);
            int allFeatures = Count(favored.CombatTraits.AllFeatures,
                set.HelpfulCombat);
            if (publishTrait ? features != 1 || allFeatures != 1 :
                    features != 0 || allFeatures != 0)
                throw new InvalidOperationException(
                    "Combat Helpful publication does not match traits/module state.");
            if (Count(favored.RaceTraits.AllFeatures,
                    favored.HalflingHelpful) != 1)
                throw new InvalidOperationException(
                    "Foreign halfling Helpful race-trait membership changed.");
        }

        private static PrerequisiteNoFeature CreateExclusion(
            BlueprintFeature excluded, string name)
        {
            var prerequisite = UnityEngine.ScriptableObject.CreateInstance<
                PrerequisiteNoFeature>();
            prerequisite.name = name;
            prerequisite.Feature = excluded;
            prerequisite.Group = Prerequisite.GroupType.All;
            return prerequisite;
        }

        private static int CountExclusions(BlueprintComponent[] values,
            BlueprintFeature feature)
        {
            return (values ?? new BlueprintComponent[0]).OfType<
                PrerequisiteNoFeature>().Count(value => ReferenceEquals(
                    value.Feature, feature));
        }

        private static int Count(BlueprintFeature[] values,
            BlueprintFeature feature)
        {
            return values == null ? 0 : values.Count(value =>
                ReferenceEquals(value, feature) || value != null &&
                string.Equals(value.AssetGuid, feature.AssetGuid,
                    StringComparison.Ordinal));
        }

        private static string FeatureIdentity(BlueprintFeature feature)
        { return feature == null ? string.Empty : feature.AssetGuid; }

        private static string ComponentIdentity(BlueprintComponent component)
        {
            var prerequisite = component as PrerequisiteNoFeature;
            if (prerequisite != null && prerequisite.Feature != null)
                return typeof(PrerequisiteNoFeature).FullName + ":" +
                    prerequisite.Feature.AssetGuid;
            return component == null ? string.Empty :
                component.GetType().FullName + ":ref=" +
                RuntimeHelpers.GetHashCode(component);
        }

        private static void Block(
            OptionalAidAnotherAvailability cotwAvailability,
            OptionalAidAnotherAvailability favoredAvailability,
            bool? traitsEnabled, string failedCheck, Exception exception)
        {
            ModContext context;
            HelpfulPublicationTransaction publication;
            lock (Gate)
            {
                context = _context;
                publication = _publication;
                _publication = null;
                _cotw = null;
                _favored = null;
            }
            if (publication != null)
                try { publication.Rollback(); }
                catch (Exception rollback)
                {
                    exception = exception == null ? rollback :
                        new AggregateException(exception, rollback);
                }
            AidAnotherGrantRuntime.Configure(null, null);
            AidAnotherCompatibilityStatusRegistry.Update(
                new AidAnotherCompatibilityStatus(cotwAvailability,
                    favoredAvailability, traitsEnabled, false, false,
                    failedCheck));
            if (exception == null)
                AidAnotherCompatibilityDiagnostics.Warning(context,
                    "contract.blocked", "failedCheck=" + failedCheck +
                    ";optional extension remained fail-closed; unrelated KMG modules remain active.");
            else
                AidAnotherCompatibilityDiagnostics.Failure(context,
                    "contract.blocked", "failedCheck=" + failedCheck +
                    ";optional extension remained fail-closed; unrelated KMG modules remain active.",
                    exception);
        }

        private static void InstallAvailableLifecyclePatches(
            ModContext context)
        {
            if (context == null) return;
            UnityModManager.ModEntry[] entries = ReadEntries(context.ModEntry);
            UnityModManager.ModEntry cotw = Single(entries,
                CotwAidAnotherResolver.ModId);
            UnityModManager.ModEntry favored = Single(entries,
                FavoredClassTraitResolver.ModId);
            if (!_cotwLifecyclePatched && IsActive(cotw))
            {
                MethodInfo method = FindMethod(cotw.Assembly,
                    CotwAidAnotherResolver.RebalanceTypeName,
                    CotwAidAnotherResolver.CreationMethodName,
                    Type.EmptyTypes);
                Patch(context, method, "AfterCotwAidAnotherCreation");
                _cotwLifecyclePatched = true;
            }
            if (!_favoredLifecyclePatched && IsActive(favored))
            {
                MethodInfo method = FindMethod(favored.Assembly,
                    FavoredClassTraitResolver.TraitsTypeName,
                    FavoredClassTraitResolver.LoadMethodName,
                    new[] { typeof(bool) });
                Patch(context, method, "AfterFavoredTraitsLoad");
                _favoredLifecyclePatched = true;
            }
        }

        private static void Patch(ModContext context, MethodInfo target,
            string postfixName)
        {
            if (target == null || target.ReturnType != typeof(void) ||
                target.IsGenericMethod)
                throw new InvalidOperationException(
                    "Optional lifecycle method did not match its exact static void contract: " +
                    postfixName);
            MethodInfo postfix = typeof(AidAnotherOptionalExtensionCoordinator)
                .GetMethod(postfixName, BindingFlags.Static |
                    BindingFlags.NonPublic);
            if (postfix == null) throw new MissingMethodException(
                typeof(AidAnotherOptionalExtensionCoordinator).FullName,
                postfixName);
            context.Harmony.Patch(target, null, new HarmonyMethod(postfix), null);
            AidAnotherCompatibilityDiagnostics.Info(context,
                "lifecycle.patch-installed", "target=" +
                target.DeclaringType.FullName + "." + target.Name +
                ";postfix=" + postfixName);
        }

        private static MethodInfo FindMethod(Assembly assembly,
            string typeName, string methodName, Type[] parameters)
        {
            Type type = assembly == null ? null : assembly.GetType(typeName,
                false, false);
            return type == null ? null : type.GetMethod(methodName,
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic, null, parameters, null);
        }

        private static bool IsActive(UnityModManager.ModEntry entry)
        {
            return entry != null && entry.Loaded && entry.Active &&
                entry.HasAssembly && !entry.ErrorOnLoading &&
                entry.Assembly != null;
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

        private static UnityModManager.ModEntry[] ReadEntries(
            UnityModManager.ModEntry current)
        {
            Type manager = current == null ? null : current.GetType().DeclaringType;
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
    }
}
