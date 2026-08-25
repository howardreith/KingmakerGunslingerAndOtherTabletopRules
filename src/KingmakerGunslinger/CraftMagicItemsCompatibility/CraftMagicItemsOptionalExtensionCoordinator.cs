using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    /// <summary>
    /// Discovers the exact live UMM entry and installs late-bound Harmony 2
    /// adapters without adding a production reference to CraftMagicItems.
    /// </summary>
    internal static class CraftMagicItemsOptionalExtensionCoordinator
    {
        private const string HarmonyOwner =
            "KingmakerGunslinger.craft-magic-items-compatibility";
        private const BindingFlags Static = BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object Gate = new object();
        private static ModContext _context;
        private static UnityModManager.ModEntry _entry;
        private static CraftMagicItemsContract _contract;
        private static object _harmony;
        private static bool _installed;
        private static bool _patched;
        private static bool _firstUpdateAttached;
        private static bool _safeUpdateAttached;
        private static bool _rebuilding;
        private static bool _incompatibleLogged;

        internal static CraftMagicItemsContract Contract
        { get { lock (Gate) return _contract; } }

        internal static UnityModManager.ModEntry Entry
        { get { lock (Gate) return _entry; } }

        internal static void Install(ModContext context)
        {
            if (context == null) return;
            lock (Gate)
            {
                _context = context;
                if (_installed) return;
                _installed = true;
            }
            CraftMagicItemsCompatibilityStatusRegistry.Update(
                new CraftMagicItemsCompatibilityStatus(
                    CraftMagicItemsCompatibilityAvailability.Pending,
                    "Awaiting the exact live CraftMagicItems UMM entry.",
                    0, 0, 0));
            AttachSafeUpdate(context);
            TryResolveAndPatch("package-load");
            AttachFirstUpdate(context);
        }

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
            TryResolveAndPatch("first-update-after-umm-load");
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

        private static void AttachSafeUpdate(ModContext context)
        {
            lock (Gate)
            {
                if (_safeUpdateAttached) return;
                _safeUpdateAttached = true;
            }
            context.ModEntry.OnUpdate += SafeUpdate;
        }

        private static void SafeUpdate(UnityModManager.ModEntry entry,
            float delta)
        {
            CraftMagicItemsReflectionBridge.ProcessDeferredUiFailure();
        }

        private static void TryResolveAndPatch(string checkpoint)
        {
            ModContext context;
            lock (Gate) context = _context;
            if (context == null) return;
            try
            {
                UnityModManager.ModEntry[] matches = ReadEntries(
                    context.ModEntry).Where(value => value.Info != null &&
                    string.Equals(value.Info.Id,
                        CraftMagicItemsContractProbe.ModId,
                        StringComparison.Ordinal)).ToArray();
                if (matches.Length == 0)
                {
                    CraftMagicItemsCompatibilityStatusRegistry.Update(
                        new CraftMagicItemsCompatibilityStatus(
                            CraftMagicItemsCompatibilityAvailability
                                .NotInstalled,
                            "CraftMagicItems UMM entry was not detected.",
                            0, 0, 0));
                    context.Logger.Info("craft-magic-items",
                        "dependency.not-installed", "checkpoint=" +
                        checkpoint + ";bridgeActive=false");
                    return;
                }
                if (matches.Length != 1)
                {
                    Incompatible("umm-entry-ambiguous", "checkpoint=" +
                        checkpoint + ";count=" + matches.Length, null);
                    return;
                }

                UnityModManager.ModEntry live = matches[0];
                lock (Gate) _entry = live;
                Assembly assembly = live.Assembly;
                if (!live.HasAssembly || assembly == null)
                {
                    CraftMagicItemsCompatibilityStatusRegistry.Update(
                        new CraftMagicItemsCompatibilityStatus(
                            CraftMagicItemsCompatibilityAvailability
                                .InstalledDisabled,
                            "CraftMagicItems is installed without a live assembly.",
                            0, 0, 0));
                    return;
                }

                CraftMagicItemsContractResolution resolution =
                    CraftMagicItemsContractProbe.Probe(assembly, true);
                if (!resolution.IsCompatible)
                {
                    Incompatible(resolution.FailedCheck, "checkpoint=" +
                        checkpoint + ";" + AssemblyIdentity(assembly), null);
                    return;
                }

                lock (Gate) _contract = resolution.Contract;
                InstallPatches(resolution.Contract);

                bool active = live.Loaded && live.Active &&
                    !live.ErrorOnLoading && (bool)resolution.Contract
                        .EnabledField.GetValue(null);
                if (!active)
                {
                    CraftMagicItemsReflectionBridge.ExternalDisabled();
                    CraftMagicItemsCompatibilityStatusRegistry.Update(
                        new CraftMagicItemsCompatibilityStatus(
                            CraftMagicItemsCompatibilityAvailability
                                .InstalledDisabled,
                            "CraftMagicItems is installed but its live UMM entry is disabled.",
                            0, 0, 0));
                    context.Logger.Info("craft-magic-items",
                        "dependency.disabled", "checkpoint=" + checkpoint +
                        ";" + AssemblyIdentity(assembly));
                    return;
                }

                if (!BlueprintBootstrap.IsInitialized)
                {
                    CraftMagicItemsCompatibilityStatusRegistry.Update(
                        new CraftMagicItemsCompatibilityStatus(
                            CraftMagicItemsCompatibilityAvailability.Pending,
                            "Compatible CMI contract accepted; awaiting finalized Gunslinger blueprints.",
                            0, 0, 0));
                    context.Logger.Info("craft-magic-items",
                        "blueprints.pending", "checkpoint=" + checkpoint +
                        ";contractAccepted=true;graphMutation=false");
                    return;
                }

                CraftMagicItemsRegistrationCatalog catalog =
                    CraftMagicItemsRegistrationCatalog.Create(
                        context.FeatureModules.Active);
                CraftMagicItemsReflectionBridge.Configure(context,
                    resolution.Contract, catalog);

                context.Logger.Info("craft-magic-items",
                    "contract.accepted", "checkpoint=" + checkpoint + ";" +
                    AssemblyIdentity(assembly) +
                    ";lifecycle=first-equipment-index-prefix");
                if (resolution.Contract.ItemDataField.GetValue(null) != null &&
                    !CraftMagicItemsReflectionBridge.IsFinalized)
                    RebuildCompleteGraph("late-attachment");
                else if (!CraftMagicItemsReflectionBridge.IsFinalized)
                    CraftMagicItemsCompatibilityStatusRegistry.Update(
                        new CraftMagicItemsCompatibilityStatus(
                            CraftMagicItemsCompatibilityAvailability.Pending,
                            "Compatible CMI contract accepted; awaiting its finalized data graph.",
                            0, 0, 0));
            }
            catch (Exception exception)
            {
                Incompatible("coordinator-exception", "checkpoint=" +
                    checkpoint, exception);
            }
        }

        private static void InstallPatches(CraftMagicItemsContract contract)
        {
            lock (Gate)
            {
                if (_patched) return;
            }
            Type harmonyType = contract.HarmonyInstanceField.FieldType;
            Type harmonyMethodType = harmonyType == null ? null :
                harmonyType.Assembly.GetType("HarmonyLib.HarmonyMethod", false,
                    false);
            ConstructorInfo harmonyConstructor = harmonyType == null ? null :
                harmonyType.GetConstructor(new[] { typeof(string) });
            ConstructorInfo methodConstructor = harmonyMethodType == null ?
                null : harmonyMethodType.GetConstructor(new[] {
                    typeof(MethodInfo) });
            MethodInfo patch = harmonyType == null ? null : harmonyType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .SingleOrDefault(value => value.Name == "Patch" &&
                    value.GetParameters().Length == 5 &&
                    typeof(MethodBase).IsAssignableFrom(value.GetParameters()[0]
                        .ParameterType));
            MethodInfo unpatchAll = harmonyType == null ? null : harmonyType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .SingleOrDefault(value => value.Name == "UnpatchAll" &&
                    value.ReturnType == typeof(void) &&
                    value.GetParameters().Length == 1 &&
                    value.GetParameters()[0].ParameterType == typeof(string));
            if (harmonyConstructor == null || methodConstructor == null ||
                patch == null || unpatchAll == null)
                throw new MissingMemberException(
                    "The loaded Harmony 2 patch contract is unavailable.");

            object harmony = harmonyConstructor.Invoke(new object[] {
                HarmonyOwner });
            try
            {
                Apply("equipment-index", harmony, patch, methodConstructor,
                    contract.AddItemIdForEnchantment,
                    Callback("BeforeEquipmentIndexPrefix"), null);
                Apply("crafting-feats", harmony, patch, methodConstructor,
                    contract.AddAllCraftingFeats, null,
                    Callback("AddAllCraftingFeatsPostfix"));
                Apply("recipe-category", harmony, patch, methodConstructor,
                    contract.RenderRecipeBased,
                    Callback("RenderRecipeBasedPrefix"),
                    Callback("RenderRecipeBasedPostfix"));
                Apply("enchant-candidate", harmony, patch, methodConstructor,
                    contract.CanEnchant, null, Callback("CanEnchantPostfix"));
                Apply("slot-boundary", harmony, patch, methodConstructor,
                    contract.BlueprintMatchesSlot, null,
                    Callback("BlueprintMatchesSlotPostfix"));
                Apply("authored-target-boundary", harmony, patch,
                    methodConstructor,
                    contract.ItemMatchesEnchantments, null,
                    Callback("ItemMatchesEnchantmentsPostfix"));
                Apply("recipe-applicability", harmony, patch,
                    methodConstructor, contract.RecipeApplies, null,
                    Callback("RecipeAppliesPostfix"));
                Apply("custom-guid-boundary", harmony, patch,
                    methodConstructor, contract.BuildCustomRecipeGuid,
                    Callback("BuildCustomRecipeGuidPrefix"), null);
                Apply("owned-state-transfer", harmony, patch,
                    methodConstructor, contract.CraftItem,
                    Callback("CraftItemPrefix"), null);
                Apply("ammunition-ui-inner-seam", harmony, patch,
                    methodConstructor, contract.RenderMundane, null, null,
                    Callback("RenderMundaneTranspiler"));
                Apply("toggle-rebuild", harmony, patch, methodConstructor,
                    contract.OnToggle, null, Callback("OnTogglePostfix"));
            }
            catch (Exception patchException)
            {
                Exception rollbackException = null;
                try
                {
                    unpatchAll.Invoke(harmony, new object[] { HarmonyOwner });
                }
                catch (Exception exception)
                {
                    rollbackException = exception;
                }
                if (rollbackException != null)
                    throw new InvalidOperationException(
                        "Harmony2 patch transaction and owner rollback failed;" +
                        "patch=" + ExceptionSummary(patchException) +
                        ";rollback=" + ExceptionSummary(rollbackException),
                        new AggregateException(patchException,
                            rollbackException));
                if (_context != null)
                    _context.Logger.Warning("craft-magic-items",
                        "harmony.patch-install-rollback", "owner=" +
                        HarmonyOwner + ";cause=" +
                        ExceptionSummary(patchException));
                throw;
            }

            lock (Gate)
            {
                _harmony = harmony;
                _patched = true;
            }
            _context.Logger.Info("craft-magic-items",
                "harmony.installed",
                "owner=" + HarmonyOwner + ";patches=11;harmony=" +
                harmonyType.Assembly.GetName().Version + ";mundaneUiSeam=" +
                contract.MundaneUiAnchor.Identity);
        }

        private static void Apply(string identity, object harmony,
            MethodInfo patch, ConstructorInfo harmonyMethodConstructor,
            MethodBase target, MethodInfo prefix, MethodInfo postfix,
            MethodInfo transpiler = null)
        {
            if (target == null) throw new ArgumentNullException("target");
            try
            {
                object prefixValue = prefix == null ? null :
                    harmonyMethodConstructor.Invoke(new object[] { prefix });
                object postfixValue = postfix == null ? null :
                    harmonyMethodConstructor.Invoke(new object[] { postfix });
                object transpilerValue = transpiler == null ? null :
                    harmonyMethodConstructor.Invoke(new object[] {
                        transpiler });
                patch.Invoke(harmony, new[] { target, prefixValue,
                    postfixValue, transpilerValue, null });
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Harmony2 patch failed at " + identity + ";target=" +
                    target.DeclaringType.FullName + "." + target.Name +
                    ";cause=" + ExceptionSummary(exception), exception);
            }
        }

        private static string ExceptionSummary(Exception exception)
        {
            var parts = new List<string>();
            Exception current = exception;
            for (int depth = 0; current != null && depth < 5; depth++)
            {
                parts.Add(current.GetType().FullName + ":" +
                    (current.Message ?? string.Empty).Replace('\r', ' ')
                        .Replace('\n', ' '));
                current = current.InnerException;
            }
            return string.Join(" -> ", parts.ToArray());
        }

        private static MethodInfo Callback(string name)
        {
            MethodInfo value = typeof(
                CraftMagicItemsOptionalExtensionCoordinator).GetMethod(name,
                    Static);
            if (value == null) throw new MissingMethodException(
                typeof(CraftMagicItemsOptionalExtensionCoordinator).FullName,
                name);
            return value;
        }

        private static void BeforeEquipmentIndexPrefix()
        {
            CraftMagicItemsReflectionBridge.AfterDataRead();
            CraftMagicItemsReflectionBridge.BeforeEquipmentIndexes();
        }

        private static void AddAllCraftingFeatsPostfix()
        {
            try
            {
                CraftMagicItemsReflectionBridge.ActivateMagicFeatCategories();
            }
            catch (Exception exception)
            {
                CraftMagicItemsReflectionBridge.ReportBoundaryFailure(
                    "magic-category-feat-activation", exception);
            }
        }

        private static void RenderRecipeBasedPrefix(object craftingData,
            out CraftMagicItemsReflectionBridge.CategoryScope __state)
        {
            CraftMagicItemsReflectionBridge.EnterRecipeCategory(craftingData,
                out __state);
        }

        private static void RenderRecipeBasedPostfix(
            CraftMagicItemsReflectionBridge.CategoryScope __state)
        { CraftMagicItemsReflectionBridge.ExitRecipeCategory(__state); }

        private static void CanEnchantPostfix(ItemEntity item,
            ref bool __result)
        {
            if (!__result && CraftMagicItemsReflectionBridge
                    .ShouldAdmitMundaneFirearm(item))
                __result = true;
        }

        private static void BlueprintMatchesSlotPostfix(
            BlueprintItemEquipment blueprint, ref bool __result)
        {
            if (__result) __result = CraftMagicItemsReflectionBridge
                .IsCandidateAllowed(blueprint);
        }

        private static void ItemMatchesEnchantmentsPostfix(object[] __args,
            ref bool __result)
        {
            if (!__result || __args == null || __args.Length < 4) return;
            if (CraftMagicItemsReflectionBridge.ShouldRejectMatchingItem(
                    __args[0] as BlueprintItemEquipment,
                    __args[3] as BlueprintItemEquipment))
                __result = false;
        }

        private static void RecipeAppliesPostfix(object recipe,
            BlueprintItem blueprint, ref bool __result)
        {
            if (CraftMagicItemsReflectionBridge.IsReliableRecipe(recipe))
                __result = __result && CraftMagicItemsReflectionBridge
                    .ReliableAppliesTo(blueprint);
        }

        private static bool BuildCustomRecipeGuidPrefix(string originalGuid,
            IEnumerable<string> enchantments, ref string __result)
        {
            string blocked;
            bool allowed = CraftMagicItemsReflectionBridge
                .GuardCustomRecipeGuid(originalGuid, enchantments,
                    out blocked);
            if (!allowed) __result = blocked;
            return allowed;
        }

        private static bool CraftItemPrefix(object[] __args)
        {
            try
            {
                ItemEntity resultItem = __args != null && __args.Length > 0
                    ? __args[0] as ItemEntity : null;
                ItemEntity upgradeItem = __args != null && __args.Length > 1
                    ? __args[1] as ItemEntity : null;
                CraftMagicItemsReflectionBridge.TransferOwnedFirearmState(
                    resultItem, upgradeItem);
                return true;
            }
            catch (Exception exception)
            {
                CraftMagicItemsReflectionBridge.ReportBoundaryFailure(
                    "owned-firearm-state-transfer", exception);
                return false;
            }
        }

        private static IEnumerable<CodeInstruction> RenderMundaneTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CraftMagicItemsContract contract = Contract;
            if (contract == null) throw new InvalidOperationException(
                "CMI mundane UI contract was unavailable during patching.");
            return CraftMagicItemsMundaneUiTranspiler.Transpile(instructions,
                generator, contract.MundaneUiAnchor,
                contract.RecipeBasedType, contract.GetSelectedCrafter,
                typeof(CraftMagicItemsReflectionBridge).GetMethod(
                    "TryRenderSelectedAmmunition", Static));
        }

        private static void OnTogglePostfix(bool enabled)
        {
            if (!enabled)
            {
                CraftMagicItemsReflectionBridge.ExternalDisabled();
                CraftMagicItemsCompatibilityStatusRegistry.Update(
                    new CraftMagicItemsCompatibilityStatus(
                        CraftMagicItemsCompatibilityAvailability
                            .InstalledDisabled,
                        "CraftMagicItems was disabled through UMM.", 0, 0, 0));
            }
            else if (!CraftMagicItemsReflectionBridge.IsFinalized)
            {
                if (CraftMagicItemsReflectionBridge.IsFailed)
                    CraftMagicItemsCompatibilityStatusRegistry.Update(
                        new CraftMagicItemsCompatibilityStatus(
                            CraftMagicItemsReflectionBridge.IsUiFaulted ?
                                CraftMagicItemsCompatibilityAvailability
                                    .BridgeFaulted :
                                CraftMagicItemsCompatibilityAvailability
                                    .Incompatible,
                            "The KMG compatibility bridge failed closed earlier in this process.",
                            0, 0, 0));
                else
                    CraftMagicItemsCompatibilityStatusRegistry.Update(
                        new CraftMagicItemsCompatibilityStatus(
                            CraftMagicItemsCompatibilityAvailability.Pending,
                            "CraftMagicItems was enabled; awaiting its finalized data graph.",
                            0, 0, 0));
            }
        }

        internal static void RebuildCompleteGraphForQualification()
        { RebuildCompleteGraph("guarded-runtime-qualification"); }

        private static void RebuildCompleteGraph(string checkpoint)
        {
            CraftMagicItemsContract contract;
            UnityModManager.ModEntry entry;
            lock (Gate)
            {
                if (_rebuilding) throw new InvalidOperationException(
                    "A CMI graph rebuild is already active.");
                _rebuilding = true;
                contract = _contract;
                entry = _entry;
            }
            try
            {
                if (contract == null || entry == null)
                    throw new InvalidOperationException(
                        "The exact CMI toggle contract is unavailable.");
                object disabled = contract.OnToggle.Invoke(null,
                    new object[] { entry, false });
                object enabled = contract.OnToggle.Invoke(null,
                    new object[] { entry, true });
                if (!Equals(disabled, true) || !Equals(enabled, true) ||
                    !CraftMagicItemsReflectionBridge.IsFinalized)
                    throw new InvalidOperationException(
                        "CMI did not complete one exact disable/enable graph rebuild.");
                _context.Logger.Info("craft-magic-items",
                    "graph.rebuilt", "checkpoint=" + checkpoint +
                    ";generation=" + CraftMagicItemsReflectionBridge
                        .Snapshot.Generation);
            }
            finally
            {
                lock (Gate) _rebuilding = false;
            }
        }

        private static void Incompatible(string failedCheck, string detail,
            Exception exception)
        {
            ModContext context;
            bool log;
            lock (Gate)
            {
                context = _context;
                log = !_incompatibleLogged;
                _incompatibleLogged = true;
            }
            CraftMagicItemsCompatibilityStatusRegistry.Update(
                new CraftMagicItemsCompatibilityStatus(
                    CraftMagicItemsCompatibilityAvailability.Incompatible,
                    failedCheck, 0, 0, 0));
            if (!log || context == null) return;
            string message = "failedCheck=" + failedCheck + ";" + detail +
                ";the optional bridge was disabled without failing KMG";
            if (exception == null) context.Logger.Warning(
                "craft-magic-items", "contract.incompatible", message);
            else context.Logger.Failure("craft-magic-items",
                "contract.incompatible", message, exception);
        }

        private static string AssemblyIdentity(Assembly assembly)
        {
            string location = assembly == null ? null : assembly.Location;
            string hash = string.IsNullOrWhiteSpace(location) ||
                !File.Exists(location) ? "unavailable" : Sha256(location);
            string fileVersion = string.IsNullOrWhiteSpace(location) ?
                "unavailable" : FileVersionInfo.GetVersionInfo(location)
                    .FileVersion;
            return string.Format(CultureInfo.InvariantCulture,
                "assembly={0};assemblyVersion={1};fileVersion={2};mvid={3};sha256={4}",
                assembly.GetName().Name, assembly.GetName().Version,
                fileVersion, assembly.ManifestModule.ModuleVersionId, hash);
        }

        private static string Sha256(string path)
        {
            using (SHA256 value = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(value.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
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
            return values.Cast<object>().Select(value => value as
                    UnityModManager.ModEntry)
                .Where(value => value != null).ToArray();
        }
    }
}
