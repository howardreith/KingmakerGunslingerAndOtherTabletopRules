using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.FeatureModules;
using UnityModManagerNet;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurOptionalExtensionCoordinator
    {
        private const string CotwModId = "CallOfTheWild";
        private const string ArcanistTypeName = "CallOfTheWild.Arcanist";
        private const string CreationMethodName = "createArcanistClass";
        private static readonly object Gate = new object();
        private static ModContext _context;
        private static UnityModManager.ModEntry _cotwEntry;
        private static CotwArcanistResolution _current;
        private static BlueprintRegistry _registry;
        private static BrownFurBlueprintSet _blueprints;
        private static bool _installed;
        private static bool _firstUpdateAttached;
        private static bool _reconciling;
        private static int _successfulReconciliations;

        internal static CotwArcanistResolution Current
        { get { lock (Gate) return _current; } }

        internal static BrownFurBlueprintSet Blueprints
        { get { lock (Gate) return _blueprints; } }

        internal static int SuccessfulReconciliations
        { get { lock (Gate) return _successfulReconciliations; } }

        internal static void Install(ModContext context)
        {
            if (context == null) return;
            try
            {
                lock (Gate)
                {
                    _context = context;
                    if (_installed) return;
                    _installed = true;
                }

                UnityModManager.ModEntry[] matches = ReadEntries(context.ModEntry)
                    .Where(value => value.Info != null && string.Equals(
                        value.Info.Id, CotwModId, StringComparison.Ordinal)).ToArray();
                if (matches.Length == 0)
                {
                    BrownFurFeatureStatusRegistry.Update(new BrownFurFeatureStatus(
                        BrownFurDependencyAvailability.Unavailable, false,
                        "Call of the Wild UMM entry was not detected."));
                    BrownFurDiagnostics.Info(context, "dependency.unavailable",
                        "Call of the Wild was not detected; Brown-Fur was not registered or published. Independent modules remain active.");
                    return;
                }
                if (matches.Length != 1)
                {
                    Block(context, "cotw-mod-entry-ambiguous",
                        "Expected one Call of the Wild UMM entry but found " +
                        matches.Length + ".", null);
                    return;
                }
                _cotwEntry = matches[0];
                Assembly assembly = _cotwEntry.Assembly;
                if (!_cotwEntry.Loaded || !_cotwEntry.Active ||
                    !_cotwEntry.HasAssembly || _cotwEntry.ErrorOnLoading ||
                    assembly == null)
                {
                    Block(context, "cotw-not-active",
                        "Call of the Wild is installed but its live UMM entry is not loaded, active, assembly-backed, and error-free.", null);
                    return;
                }

                Type arcanist = assembly.GetType(ArcanistTypeName, false, false);
                MethodInfo creation = arcanist == null ? null : arcanist.GetMethod(
                    CreationMethodName, BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (creation == null || creation.ReturnType != typeof(void) ||
                    creation.IsGenericMethod)
                {
                    Block(context, "arcanist-creation-signature",
                        "CotW Arcanist creation method did not match the exact static void zero-argument contract.", null);
                    return;
                }

                MethodInfo postfix = typeof(BrownFurOptionalExtensionCoordinator)
                    .GetMethod("AfterCotwArcanistCreation", BindingFlags.Static |
                        BindingFlags.NonPublic);
                if (postfix == null)
                {
                    Block(context, "coordinator-postfix-missing",
                        "The Brown-Fur lifecycle postfix was unavailable.", null);
                    return;
                }
                context.Harmony.Patch(creation, null, new HarmonyMethod(postfix), null);
                BrownFurDiagnostics.Info(context, "lifecycle.patch-installed",
                    DescribePatchOrder(context, creation));

                if (CotwArcanistResolver.HasConstructedArcanist(assembly))
                    TryReconcile("install-after-cotw");
                else
                    BrownFurDiagnostics.Info(context, "lifecycle.awaiting-cotw",
                        "CotW Arcanist is not constructed yet; the exact creation postfix is awaiting it.");
                AttachFirstUpdate(context);
            }
            catch (Exception exception)
            {
                Block(context, "coordinator-install-exception",
                    "Brown-Fur optional coordination failed without failing the package bootstrap.",
                    exception);
            }
        }

        private static void AfterCotwArcanistCreation()
        {
            TryReconcile("cotw-create-postfix");
        }

        private static void FirstUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            lock (Gate)
            {
                context = _context;
                _firstUpdateAttached = false;
            }
            if (context != null) context.ModEntry.OnUpdate -= FirstUpdate;
            TryReconcile("first-update-fallback");
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

        private static void TryReconcile(string checkpoint)
        {
            ModContext context;
            UnityModManager.ModEntry cotw;
            lock (Gate)
            {
                if (_reconciling) return;
                _reconciling = true;
                context = _context;
                cotw = _cotwEntry;
            }
            try
            {
                CotwArcanistResolution resolution =
                    CotwArcanistResolver.Resolve(cotw);
                lock (Gate) _current = resolution;
                if (!resolution.Decision.IsCompatible)
                {
                    Block(context, resolution.Decision.FailedCheck,
                        "CotW structural contract failed at " + checkpoint +
                        "; Brown-Fur publication remains blocked.", null);
                    return;
                }

                if (!EnsureBlueprintsRegistered(context, resolution.Contract,
                    checkpoint)) return;

                lock (Gate) _successfulReconciliations++;
                BrownFurFeatureStatusRegistry.Update(new BrownFurFeatureStatus(
                    BrownFurDependencyAvailability.Available, false,
                    "Compatible CotW contract resolved and 19 stable Brown-Fur identities registered; archetype publication remains gated pending focused mechanics qualification."));
                BrownFurDiagnostics.Info(context, "contract.compatible",
                    "checkpoint=" + checkpoint + ";activeSetting=" +
                    context.FeatureModules.Active.BrownFurTransmuter + ";" +
                    resolution.Contract.Fingerprint);
            }
            catch (Exception exception)
            {
                Block(context, "reconcile-exception",
                    "Brown-Fur reconciliation failed closed at " + checkpoint +
                    " without disabling unrelated modules.", exception);
            }
            finally
            {
                lock (Gate) _reconciling = false;
            }
        }

        private static bool EnsureBlueprintsRegistered(ModContext context,
            CotwArcanistContract contract, string checkpoint)
        {
            BrownFurBlueprintSet existing;
            lock (Gate) existing = _blueprints;
            if (existing != null)
            {
                BrownFurBlueprints.Validate(existing, contract);
                BrownFurDiagnostics.Info(context, "registration.idempotent",
                    "checkpoint=" + checkpoint + ";count=" + existing.Count);
                return true;
            }

            var library = BlueprintBootstrap.Library;
            if (library == null)
            {
                BrownFurFeatureStatusRegistry.Update(new BrownFurFeatureStatus(
                    BrownFurDependencyAvailability.Available, false,
                    "Compatible CotW contract resolved; awaiting the package blueprint lifecycle before optional Brown-Fur identity registration."));
                BrownFurDiagnostics.Info(context, "registration.deferred",
                    "checkpoint=" + checkpoint +
                    ";reason=package-blueprint-library-not-ready");
                return false;
            }

            string assemblyLocation = context == null || context.Assembly == null ?
                string.Empty : context.Assembly.Location;
            string modDirectory = string.IsNullOrWhiteSpace(assemblyLocation) ?
                string.Empty : Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrWhiteSpace(modDirectory))
                throw new InvalidOperationException(
                    "The installed package directory was unavailable for Brown-Fur manifest registration.");

            var registry = new BlueprintRegistry(library,
                BlueprintManifest.Load(modDirectory), context.Logger);
            try
            {
                BrownFurBlueprintSet registered = BrownFurBlueprints.Register(
                    registry, contract);
                if (registered.Count != BrownFurIdentityCatalog.IdentityCount ||
                    registry.RegisteredCount != BrownFurIdentityCatalog.IdentityCount)
                    throw new InvalidOperationException(
                        "Brown-Fur optional registration did not create exactly " +
                        BrownFurIdentityCatalog.IdentityCount + " identities.");
                lock (Gate)
                {
                    _registry = registry;
                    _blueprints = registered;
                }
                BrownFurDiagnostics.Info(context, "registration.complete",
                    "checkpoint=" + checkpoint + ";count=" +
                    registry.RegisteredCount + ";published=false");
                return true;
            }
            catch
            {
                try { registry.RollbackAll(); }
                catch (Exception rollbackFailure)
                {
                    BrownFurDiagnostics.Failure(context,
                        "registration.rollback-failed",
                        "Brown-Fur identity registration failed and its owned blueprint rollback was incomplete.",
                        rollbackFailure);
                }
                throw;
            }
        }

        private static void Block(ModContext context, string failedCheck,
            string message, Exception exception)
        {
            BrownFurFeatureStatusRegistry.Update(new BrownFurFeatureStatus(
                BrownFurDependencyAvailability.Blocked, false,
                failedCheck + ": " + message));
            if (exception == null)
                BrownFurDiagnostics.Warning(context, "contract.blocked",
                    "failedCheck=" + failedCheck + ";" + message);
            else
                BrownFurDiagnostics.Failure(context, "contract.blocked",
                    "failedCheck=" + failedCheck + ";" + message, exception);
        }

        private static UnityModManager.ModEntry[] ReadEntries(
            UnityModManager.ModEntry current)
        {
            Type manager = current == null ? null : current.GetType().DeclaringType;
            FieldInfo field = manager == null ? null : manager.GetField("modEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            IEnumerable values = field == null ? null : field.GetValue(null) as IEnumerable;
            if (values == null) throw new InvalidOperationException(
                "The live UMM modEntries collection was unavailable.");
            return values.Cast<object>().Select(value =>
                value as UnityModManager.ModEntry).Where(value => value != null).ToArray();
        }

        private static string DescribePatchOrder(ModContext context,
            MethodInfo target)
        {
            Patches patches = context.Harmony.GetPatchInfo(target);
            if (patches == null) return "target=" + target.DeclaringType.FullName +
                "." + target.Name + ";patches=<unavailable>";
            return "target=" + target.DeclaringType.FullName + "." + target.Name +
                ";prefixes=" + Describe(patches.Prefixes) +
                ";postfixes=" + Describe(patches.Postfixes) +
                ";transpilers=" + Describe(patches.Transpilers);
        }

        private static string Describe(System.Collections.Generic.IEnumerable<Patch> values)
        {
            return string.Join("|", values.Select((value, index) => index + ":" +
                value.owner + "/" + value.priority + "/" +
                (value.patch == null ? "<missing>" :
                    value.patch.DeclaringType.FullName + "." + value.patch.Name))
                .ToArray());
        }
    }
}
