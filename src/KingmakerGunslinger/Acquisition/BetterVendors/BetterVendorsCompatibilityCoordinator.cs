using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    /// <summary>
    /// Optional Better Vendors adapter. It never adds a compile-time or UMM
    /// dependency: the live UMM entry is discovered by id, the progression
    /// contract is verified by reflection and method-body fingerprints, and
    /// only then are four narrow hooks installed:
    ///
    ///   ProgressionLogic.AddStock          postfix  closes the pass scope
    ///   ProgressionLogic.AddMilitaryStock  prefix/postfix  classifies the
    ///                                      call and mirrors its ordinary tier
    ///   ProgressionLogic.GetFilterWeapons  postfix  read-only observation of
    ///                                      Better Vendors' own selection
    ///   VendorLogic.BeginTrading           postfix  one-time catch-up for the
    ///                                      shared Military destination
    ///
    /// Absent, disabled, progression-off or structurally different Better
    /// Vendors leaves the rest of this mod untouched. Resolution is retried at
    /// package load, on the first UMM update and lazily at trading, so an early
    /// check never permanently disables the integration.
    /// </summary>
    internal static class BetterVendorsCompatibilityCoordinator
    {
        internal const string HarmonyOwner =
            "KingmakerGunslinger.better-vendors-progression";
        internal const string TradingHarmonyOwner =
            "KingmakerGunslinger.better-vendors-progression.trading";
        private const BindingFlags AnyStatic = BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly object Gate = new object();
        private static ModContext _context;
        private static UnityModManager.ModEntry _entry;
        private static ResolvedContract _contract;
        private static BetterVendorsContractObservation _observation;
        private static bool _installed;
        private static bool _firstUpdateAttached;
        private static bool _tradingHookInstalled;
        private static bool _patched;
        private static bool _terminal;

        internal static bool IsPatched { get { lock (Gate) return _patched; } }

        internal static bool IsTradingHookInstalled
        {
            get { lock (Gate) return _tradingHookInstalled; }
        }

        internal static BetterVendorsContractObservation Observation
        {
            get { lock (Gate) return _observation; }
        }

        internal static void Install(ModContext context)
        {
            if (context == null) return;
            lock (Gate)
            {
                if (_installed) return;
                _installed = true;
                _context = context;
            }
            TryResolve("package-load");
            AttachFirstUpdate(context);
        }

        /// <summary>Retries resolution unless a terminal state was reached.</summary>
        internal static void EnsureResolved(string checkpoint)
        {
            lock (Gate)
            {
                if (!_installed || _terminal) return;
            }
            TryResolve(checkpoint);
        }

        /// <summary>
        /// True only while the verified hooks are installed and Better Vendors
        /// is loaded, active, enabled and has vendor progression switched on.
        /// Every value is read live, so toggling Better Vendors takes effect at
        /// its next stock event without any cached decision.
        /// </summary>
        internal static bool TryGetActiveProgression(out string reason)
        {
            UnityModManager.ModEntry entry;
            ResolvedContract contract;
            bool patched;
            lock (Gate)
            {
                entry = _entry;
                contract = _contract;
                patched = _patched;
            }
            if (!patched || entry == null || contract == null)
            {
                reason = "integration-not-ready";
                return false;
            }
            if (!entry.Loaded || !entry.Active || entry.ErrorOnLoading)
            {
                reason = "better-vendors-inactive";
                return false;
            }
            if (!contract.ReadModEnabled())
            {
                reason = "better-vendors-disabled";
                return false;
            }
            if (!contract.ReadVendorProgression())
            {
                reason = "vendor-progression-off";
                return false;
            }
            reason = string.Empty;
            return true;
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

        private static void FirstUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            lock (Gate) context = _context;
            if (context != null) context.ModEntry.OnUpdate -= FirstUpdate;
            EnsureResolved("first-update-after-umm-load");
        }

        private static void TryResolve(string checkpoint)
        {
            ModContext context;
            lock (Gate) context = _context;
            if (context == null) return;
            try
            {
                UnityModManager.ModEntry[] matches = ReadEntries(context.ModEntry)
                    .Where(value => value.Info != null && string.Equals(
                        value.Info.Id, BetterVendorsContract.ModId,
                        StringComparison.Ordinal)).ToArray();
                if (matches.Length == 0)
                {
                    Terminal(BetterVendorsIntegrationAvailability.NotInstalled,
                        "Better Vendors is not installed; the optional progression integration is inactive.",
                        checkpoint);
                    return;
                }
                if (matches.Length != 1)
                {
                    Terminal(BetterVendorsIntegrationAvailability.Incompatible,
                        "Better Vendors UMM entry is ambiguous; count=" +
                        matches.Length.ToString(CultureInfo.InvariantCulture),
                        checkpoint);
                    return;
                }
                UnityModManager.ModEntry live = matches[0];
                lock (Gate) _entry = live;
                InstallTradingHook(context);
                Assembly assembly = live.Assembly;
                if (!live.HasAssembly || assembly == null)
                {
                    Report(BetterVendorsIntegrationAvailability.InstalledInactive,
                        "Better Vendors is installed without a loaded assembly; progression integration waits for it.",
                        checkpoint);
                    return;
                }

                ResolvedContract contract;
                BetterVendorsContractObservation observation = Observe(live,
                    assembly, out contract);
                BetterVendorsContractDecision decision =
                    BetterVendorsContract.Evaluate(observation);
                lock (Gate) _observation = observation;
                if (!decision.IsCompatible)
                {
                    Terminal(BetterVendorsIntegrationAvailability.Incompatible,
                        "Better Vendors " + Describe(observation) +
                        " does not match the verified " +
                        BetterVendorsContract.VerifiedModVersion +
                        " progression contract at " + decision.FailedCheck +
                        " (observed " + decision.Detail + "); Gunslinger progression stock is disabled. Better Vendors itself and every other feature of this mod are unaffected.",
                        checkpoint);
                    return;
                }
                InstallContractPatches(contract);
                lock (Gate)
                {
                    _contract = contract;
                    _patched = true;
                    _terminal = true;
                }
                string state;
                TryGetActiveProgression(out state);
                Report(BetterVendorsIntegrationAvailability.Ready,
                    "Verified Better Vendors " + Describe(observation) +
                    "; hooks=AddStock,AddMilitaryStock,GetFilterWeapons,BeginTrading;progressionNow=" +
                    (state.Length == 0 ? "active" : state), checkpoint);
            }
            catch (Exception exception)
            {
                lock (Gate) _terminal = true;
                Report(BetterVendorsIntegrationAvailability.Faulted,
                    "The optional Better Vendors integration failed closed: " +
                    exception.GetType().Name + ": " + exception.Message,
                    checkpoint, exception);
            }
        }

        private static void InstallTradingHook(ModContext context)
        {
            lock (Gate)
            {
                if (_tradingHookInstalled) return;
            }
            MethodInfo beginTrading = typeof(VendorLogic).GetMethod(
                "BeginTrading", new[] { typeof(UnitEntityData) });
            if (beginTrading == null || beginTrading.ReturnType != typeof(void))
                throw new MissingMethodException(typeof(VendorLogic).FullName,
                    "BeginTrading(UnitEntityData)");
            HarmonyInstance harmony = HarmonyInstance.Create(TradingHarmonyOwner);
            try
            {
                harmony.Patch(beginTrading, null, new HarmonyMethod(
                    Callback("BeginTradingPostfix")), null);
            }
            catch
            {
                harmony.UnpatchAll(TradingHarmonyOwner);
                throw;
            }
            lock (Gate) _tradingHookInstalled = true;
        }

        private static void InstallContractPatches(ResolvedContract contract)
        {
            HarmonyInstance harmony = HarmonyInstance.Create(HarmonyOwner);
            try
            {
                harmony.Patch(contract.AddStock, null,
                    new HarmonyMethod(Callback("AddStockPostfix")), null);
                harmony.Patch(contract.AddMilitaryStock,
                    new HarmonyMethod(Callback("AddMilitaryStockPrefix")),
                    new HarmonyMethod(Callback("AddMilitaryStockPostfix")), null);
                harmony.Patch(contract.GetFilterWeapons, null,
                    new HarmonyMethod(Callback("GetFilterWeaponsPostfix")), null);
            }
            catch
            {
                harmony.UnpatchAll(HarmonyOwner);
                throw;
            }
        }

        private static MethodInfo Callback(string name)
        {
            MethodInfo value = typeof(BetterVendorsStockRuntime).GetMethod(name,
                AnyStatic);
            if (value == null)
                throw new MissingMethodException(
                    typeof(BetterVendorsStockRuntime).FullName, name);
            return value;
        }

        private static BetterVendorsContractObservation Observe(
            UnityModManager.ModEntry entry, Assembly assembly,
            out ResolvedContract contract)
        {
            contract = null;
            var observed = new BetterVendorsContractObservation();
            AssemblyName name = assembly.GetName();
            observed.AssemblyName = name.Name;
            observed.AssemblyVersion = name.Version == null ? string.Empty :
                name.Version.ToString();
            observed.ModVersion = entry.Info == null ? string.Empty :
                entry.Info.Version;
            observed.ModuleVersionId = assembly.ManifestModule.ModuleVersionId
                .ToString();
            observed.FileSha256 = FileSha256(assembly.Location);

            Type logic = assembly.GetType(
                BetterVendorsContract.ProgressionLogicTypeName, false, false);
            Type loadPatch = assembly.GetType(
                BetterVendorsContract.LoadPatchTypeName, false, false);
            Type improvePatch = assembly.GetType(
                BetterVendorsContract.ImproveStatPatchTypeName, false, false);
            Type settings = assembly.GetType(
                BetterVendorsContract.SettingsWrapperTypeName, false, false);
            Type main = assembly.GetType(BetterVendorsContract.MainTypeName,
                false, false);
            if (logic == null || loadPatch == null || improvePatch == null ||
                settings == null || main == null)
            {
                observed.MissingMember = "required-types";
                return observed;
            }

            MethodInfo addStock = logic.GetMethod("AddStock", AnyStatic, null,
                Type.EmptyTypes, null);
            MethodInfo addMilitary = logic.GetMethod("AddMilitaryStock",
                AnyStatic, null, new[] { typeof(int) }, null);
            MethodInfo getFilter = logic.GetMethod("GetFilterWeapons", AnyStatic,
                null, new[] { typeof(List<string>), typeof(List<string>),
                    typeof(List<string>), typeof(WeaponCategory), typeof(bool) },
                null);
            MethodInfo loadPostfix = loadPatch.GetMethod("Postfix", AnyStatic);
            MethodInfo improvePostfix = improvePatch.GetMethod("Postfix",
                AnyStatic);
            FieldInfo vendorTables = logic.GetField("VendorTableIds", AnyStatic);
            FieldInfo levels = logic.GetField("WeaponEnhancementLevels", AnyStatic);
            PropertyInfo toggle = settings.GetProperty("ToggleVendorProgression",
                AnyStatic);
            FieldInfo mod = main.GetField("Mod", AnyStatic);
            PropertyInfo enabled = mod == null ? null :
                mod.FieldType.GetProperty("Enabled", BindingFlags.Instance |
                    BindingFlags.Public);
            string missing =
                addStock == null || addStock.ReturnType != typeof(void) ||
                    !addStock.IsPublic ? "ProgressionLogic.AddStock()" :
                addMilitary == null || addMilitary.ReturnType != typeof(void)
                    ? "ProgressionLogic.AddMilitaryStock(int)" :
                getFilter == null ||
                    getFilter.ReturnType != typeof(List<BlueprintItemWeapon>)
                    ? "ProgressionLogic.GetFilterWeapons" :
                loadPostfix == null ? "GameLoadGamePatch.Postfix" :
                improvePostfix == null ? "KingdomActionsImproveStatPatch.Postfix" :
                vendorTables == null ||
                    vendorTables.FieldType != typeof(Dictionary<string, string>)
                    ? "ProgressionLogic.VendorTableIds" :
                levels == null ||
                    levels.FieldType != typeof(Dictionary<int, string>)
                    ? "ProgressionLogic.WeaponEnhancementLevels" :
                toggle == null || toggle.PropertyType != typeof(bool) ||
                    toggle.GetGetMethod() == null
                    ? "SettingsWrapper.ToggleVendorProgression" :
                mod == null || enabled == null || enabled.PropertyType != typeof(bool)
                    ? "Main.Mod.Enabled" : null;
            if (missing != null)
            {
                observed.MissingMember = missing;
                return observed;
            }

            observed.MethodIlSha256[BetterVendorsContract.AddStockKey] =
                IlSha256(addStock);
            observed.MethodIlSha256[BetterVendorsContract.AddMilitaryStockKey] =
                IlSha256(addMilitary);
            observed.MethodIlSha256[BetterVendorsContract.GetFilterWeaponsKey] =
                IlSha256(getFilter);
            observed.MethodIlSha256[BetterVendorsContract.LoadPatchKey] =
                IlSha256(loadPostfix);
            observed.MethodIlSha256[BetterVendorsContract.ImproveStatPatchKey] =
                IlSha256(improvePostfix);
            observed.LoadPatchTarget = HarmonyPatchTarget(loadPatch);
            observed.ImproveStatPatchTarget = HarmonyPatchTarget(improvePatch);

            // Reading these static tables runs only Better Vendors' own type
            // initializer, which allocates its constant collections.
            var tables = vendorTables.GetValue(null) as Dictionary<string, string>;
            string destination;
            observed.MilitaryDestinationGuid = tables != null &&
                tables.TryGetValue(BetterVendorsContract.MilitaryDestinationKey,
                    out destination) ? destination : null;
            var tiers = levels.GetValue(null) as Dictionary<int, string>;
            observed.EnhancementLevelGuids = tiers == null ? null :
                Enumerable.Range(1, ProgressionWeaponCatalog.MaximumEnhancement)
                    .Select(tier =>
                    {
                        string guid;
                        return tiers.TryGetValue(tier, out guid) ? guid : null;
                    }).ToArray();
            if (tiers != null && tiers.Count != ProgressionWeaponCatalog.MaximumEnhancement)
                observed.EnhancementLevelGuids = null;

            contract = new ResolvedContract(addStock, addMilitary, getFilter,
                toggle, mod, enabled);
            return observed;
        }

        private static string HarmonyPatchTarget(Type patchType)
        {
            foreach (CustomAttributeData attribute in
                CustomAttributeData.GetCustomAttributes(patchType))
            {
                if (attribute.AttributeType.FullName != "HarmonyLib.HarmonyPatch" ||
                    attribute.ConstructorArguments.Count != 2)
                    continue;
                Type declaring = attribute.ConstructorArguments[0].Value as Type;
                string method = attribute.ConstructorArguments[1].Value as string;
                if (declaring != null && method != null)
                    return declaring.FullName + "." + method;
            }
            return null;
        }

        private static string IlSha256(MethodInfo method)
        {
            MethodBody body = method.GetMethodBody();
            byte[] il = body == null ? null : body.GetILAsByteArray();
            if (il == null) return null;
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(il))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string FileSha256(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return "unavailable";
                using (SHA256 sha = SHA256.Create())
                using (FileStream stream = File.OpenRead(path))
                    return BitConverter.ToString(sha.ComputeHash(stream))
                        .Replace("-", string.Empty).ToLowerInvariant();
            }
            catch (Exception)
            {
                return "unavailable";
            }
        }

        private static string Describe(BetterVendorsContractObservation observed)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "mod={0};assembly={1} {2};mvid={3};sha256={4}",
                observed.ModVersion, observed.AssemblyName,
                observed.AssemblyVersion, observed.ModuleVersionId,
                observed.FileSha256);
        }

        private static void Terminal(BetterVendorsIntegrationAvailability state,
            string detail, string checkpoint)
        {
            lock (Gate) _terminal = true;
            Report(state, detail, checkpoint);
        }

        private static void Report(BetterVendorsIntegrationAvailability state,
            string detail, string checkpoint, Exception exception = null)
        {
            if (!BetterVendorsIntegrationStatusRegistry.Update(
                    new BetterVendorsIntegrationStatus(state, detail)))
                return;
            ModContext context;
            lock (Gate) context = _context;
            if (context == null) return;
            string message = "checkpoint=" + checkpoint + ";status=" + state +
                ";" + detail;
            if (exception != null)
                context.Logger.Failure("better-vendors", "compatibility.faulted",
                    message, exception);
            else if (state == BetterVendorsIntegrationAvailability.Incompatible)
                context.Logger.Warning("better-vendors",
                    "compatibility.incompatible", message);
            else
                context.Logger.Info("better-vendors",
                    "compatibility." + state.ToString().ToLowerInvariant(), message);
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

        /// <summary>Live reflection handles for the verified contract.</summary>
        internal sealed class ResolvedContract
        {
            private readonly PropertyInfo _toggle;
            private readonly FieldInfo _mod;
            private readonly PropertyInfo _enabled;

            internal ResolvedContract(MethodInfo addStock,
                MethodInfo addMilitaryStock, MethodInfo getFilterWeapons,
                PropertyInfo toggle, FieldInfo mod, PropertyInfo enabled)
            {
                AddStock = addStock;
                AddMilitaryStock = addMilitaryStock;
                GetFilterWeapons = getFilterWeapons;
                _toggle = toggle;
                _mod = mod;
                _enabled = enabled;
            }

            internal MethodInfo AddStock { get; private set; }
            internal MethodInfo AddMilitaryStock { get; private set; }
            internal MethodInfo GetFilterWeapons { get; private set; }

            internal bool ReadModEnabled()
            {
                try
                {
                    object manager = _mod.GetValue(null);
                    return manager != null && (bool)_enabled.GetValue(manager, null);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            // Better Vendors' getter dereferences its settings object, which is
            // null while the mod is disabled; any failure reads as "off".
            internal bool ReadVendorProgression()
            {
                try
                {
                    return (bool)_toggle.GetValue(null, null);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
