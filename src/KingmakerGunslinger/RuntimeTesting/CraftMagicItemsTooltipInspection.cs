using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.Tooltip;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class CraftMagicItemsTooltipInspectionResult
    {
        internal CraftMagicItemsTooltipInspectionResult(bool passed,
            IEnumerable<string> diagnostics)
        {
            Passed = passed;
            Diagnostics = (diagnostics ?? new string[0]).ToArray();
        }

        internal bool Passed { get; private set; }
        internal string[] Diagnostics { get; private set; }
    }

    /// <summary>
    /// Save-free inspection of the exact native tooltip graph for CMI-created
    /// and CMI-upgraded firearms. The observer never adds an item to inventory.
    /// </summary>
    internal static class CraftMagicItemsTooltipInspection
    {
        private const string AnarchicGuid =
            "57315bc1e1f62a741be0efde688087e9";
        private const string EnhancementFiveGuid =
            "bdba267e951851449af552aa9f9e3992";
        private const BindingFlags Static = BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic;

        internal static CraftMagicItemsTooltipInspectionResult Capture(
            bool expectInternalMarkersHidden)
        {
            var diagnostics = new List<string>();
            var items = new List<ItemEntityWeapon>();
            UnitEntityData owner = null;
            bool rebuildRequired = false;
            int suppressedBefore = FirearmInternalEnchantmentPresentation
                .SuppressedCount;
            try
            {
                CraftMagicItemsRegistrationCatalog catalog =
                    CraftMagicItemsReflectionBridge.Catalog;
                if (catalog == null || !CraftMagicItemsReflectionBridge
                        .IsFinalized)
                    throw new InvalidOperationException(
                        "The finalized CMI compatibility graph is unavailable.");
                BlueprintItemWeapon pistol = catalog.FirearmCreationBases
                    .Single(value => string.Equals(value.AssetGuid,
                        "a303d71d244640959827e9464df5a867",
                        StringComparison.Ordinal));
                BlueprintWeaponEnchantment anarchic = ResourcesLibrary
                    .TryGetBlueprint<BlueprintWeaponEnchantment>(
                        AnarchicGuid);
                BlueprintWeaponEnchantment enhancementFive = ResourcesLibrary
                    .TryGetBlueprint<BlueprintWeaponEnchantment>(
                        EnhancementFiveGuid);
                if (anarchic == null || enhancementFive == null)
                    throw new InvalidOperationException(
                        "The exact native Anarchic/+5 enchantments are unavailable.");

                BlueprintItemWeapon clone = CraftMagicItemsReflectionBridge
                    .BuildQualificationClone(pistol, new[] { anarchic,
                        enhancementFive });
                rebuildRequired = clone != null;
                if (clone == null)
                    throw new InvalidOperationException(
                        "CMI did not resolve the +5/Anarchic Pistol clone.");

                owner = new ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                if (owner == null || owner.Descriptor == null)
                    throw new InvalidOperationException(
                        "The request-local battered-origin owner is unavailable.");

                ItemEntityWeapon basePistol = Create(pistol, items);
                ItemEntityWeapon craftedEmpty = Create(clone, items);
                ItemEntityWeapon craftedLoaded = Create(clone, items);
                SetState(craftedLoaded, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal);

                ItemEntityWeapon batteredSource = Create(pistol, items);
                BatteredFirearmOriginRuntime.Bind(batteredSource, owner);
                SetState(batteredSource, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal);
                ItemEntityWeapon upgradedBattered = Create(clone, items);
                CraftMagicItemsReflectionBridge.TransferOwnedFirearmState(
                    upgradedBattered, batteredSource);

                ItemEntityWeapon broken = Create(clone, items);
                SetState(broken, 0, null, FirearmCondition.Broken);
                ItemEntityWeapon wrecked = Create(clone, items);
                SetState(wrecked, 0, null, FirearmCondition.Wrecked);

                ItemInspection[] observations =
                {
                    Inspect("base-empty-normal", basePistol),
                    Inspect("cmi-created-empty-normal", craftedEmpty),
                    Inspect("cmi-created-loaded-normal", craftedLoaded),
                    Inspect("cmi-upgraded-loaded-battered", upgradedBattered),
                    Inspect("cmi-created-empty-broken", broken),
                    Inspect("cmi-created-empty-wrecked", wrecked)
                };
                diagnostics.Add("tooltipEnchantments=" + string.Join("|",
                    observations.Select(value => value.Description).ToArray()));
                foreach (ItemInspection observation in observations)
                    diagnostics.AddRange(observation.EnchantmentDiagnostics);

                ItemInspection baseObservation = observations[0];
                ItemInspection emptyObservation = observations[1];
                ItemInspection loadedObservation = observations[2];
                ItemInspection upgradedObservation = observations[3];
                bool underlyingExact = baseObservation.StateMarkers == 0 &&
                    baseObservation.OriginMarkers == 0 &&
                    emptyObservation.StateMarkers == 0 &&
                    emptyObservation.OriginMarkers == 0 &&
                    loadedObservation.StateMarkers == 1 &&
                    loadedObservation.OriginMarkers == 0 &&
                    upgradedObservation.StateMarkers == 1 &&
                    upgradedObservation.OriginMarkers == 1 &&
                    observations[4].StateMarkers == 1 &&
                    observations[5].StateMarkers == 1;
                bool realQualitiesExact = observations.Skip(1).All(value =>
                    value.AnarchicCount == 1 &&
                    value.EnhancementFiveCount == 1 &&
                    value.RealQualityTextPresent);
                bool presentationExact = expectInternalMarkersHidden
                    ? observations.All(value => value.NullCount == 0)
                    : upgradedObservation.NullCount == 10 &&
                        loadedObservation.NullCount == 5 &&
                        observations[4].NullCount == 5 &&
                        observations[5].NullCount == 5 &&
                        baseObservation.NullCount == 0 &&
                        emptyObservation.NullCount == 0;
                int suppressed = FirearmInternalEnchantmentPresentation
                    .SuppressedCount - suppressedBefore;
                bool suppressionExact = !expectInternalMarkersHidden ||
                    suppressed == 20;
                diagnostics.Add("tooltipInspectionPolicy=expectHidden=" +
                    expectInternalMarkersHidden + ";underlyingExact=" +
                    underlyingExact + ";realQualitiesExact=" +
                    realQualitiesExact + ";presentationExact=" +
                    presentationExact + ";suppressedMarkerEnumerations=" +
                    suppressed);
                return new CraftMagicItemsTooltipInspectionResult(
                    underlyingExact && realQualitiesExact &&
                        presentationExact && suppressionExact, diagnostics);
            }
            catch (Exception exception)
            {
                diagnostics.Add("tooltipInspectionException=" + exception);
                return new CraftMagicItemsTooltipInspectionResult(false,
                    diagnostics);
            }
            finally
            {
                foreach (ItemEntityWeapon item in items)
                {
                    if (item == null) continue;
                    try { FirearmRuntimeState.Service.Forget(item); }
                    catch { }
                    item.Dispose();
                }
                if (owner != null) owner.Dispose();
                if (rebuildRequired)
                    CraftMagicItemsOptionalExtensionCoordinator
                        .RebuildCompleteGraphForQualification();
            }
        }

        internal static BlueprintItemWeapon BuildPersistentFixtureBlueprint()
        {
            CraftMagicItemsRegistrationCatalog catalog =
                CraftMagicItemsReflectionBridge.Catalog;
            if (catalog == null || !CraftMagicItemsReflectionBridge.IsFinalized)
                throw new InvalidOperationException(
                    "The finalized CMI compatibility graph is unavailable.");
            BlueprintItemWeapon pistol = catalog.FirearmCreationBases.Single(
                value => string.Equals(value.AssetGuid,
                    "a303d71d244640959827e9464df5a867",
                    StringComparison.Ordinal));
            BlueprintWeaponEnchantment anarchic = ResourcesLibrary
                .TryGetBlueprint<BlueprintWeaponEnchantment>(AnarchicGuid);
            BlueprintWeaponEnchantment enhancementFive = ResourcesLibrary
                .TryGetBlueprint<BlueprintWeaponEnchantment>(
                    EnhancementFiveGuid);
            if (anarchic == null || enhancementFive == null ||
                catalog.Reliable == null)
                throw new InvalidOperationException(
                    "The exact persistent Anarchic/+5/Reliable fixture is unavailable.");
            BlueprintItemWeapon clone = CraftMagicItemsReflectionBridge
                .BuildQualificationClone(pistol, new[] { anarchic,
                    enhancementFive, catalog.Reliable });
            if (clone == null)
                throw new InvalidOperationException(
                    "CMI did not resolve the persistent Anarchic/+5/Reliable Pistol blueprint.");
            return clone;
        }

        internal static CraftMagicItemsTooltipInspectionResult
            CapturePersistent(ItemEntityWeapon item, UnitEntityData owner)
        {
            var diagnostics = new List<string>();
            try
            {
                if (item == null || owner == null)
                    throw new ArgumentNullException(
                        "persistent tooltip fixture input");
                BlueprintItemWeapon expected =
                    BuildPersistentFixtureBlueprint();
                CraftMagicItemsRegistrationCatalog catalog =
                    CraftMagicItemsReflectionBridge.Catalog;
                ItemInspection observation = Inspect(
                    "persistent-cmi-upgraded-loaded-battered", item);
                FirearmItemStateSnapshot snapshot = FirearmRuntimeState
                    .Service.GetOrCreate(item);
                var expectedState = new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal);
                UnitEntityData persistedOwner;
                bool origin = BatteredFirearmOriginRuntime.TryGetOwner(item,
                    out persistedOwner) && ReferenceEquals(persistedOwner,
                        owner);
                FirearmDefinitionComponent[] markers =
                    (item.Blueprint.Type.ComponentsArray ??
                        new BlueprintComponent[0])
                    .OfType<FirearmDefinitionComponent>().ToArray();
                int reliable = item.Enchantments.Count(value => value != null &&
                    ReferenceEquals(value.Blueprint, catalog.Reliable));
                int hidden = item.Enchantments.Count(value => value != null &&
                    FirearmInternalEnchantmentPresentation.IsInternalMarker(
                        value.Blueprint));
                bool blueprint = ReferenceEquals(item.Blueprint, expected) &&
                    string.Equals(item.Blueprint.AssetGuid,
                        expected.AssetGuid, StringComparison.Ordinal);
                bool mechanics = markers.Length == 1 &&
                    expectedState.Equals(snapshot.Repository.State) &&
                    observation.StateMarkers == 1 &&
                    observation.OriginMarkers == 1 && origin;
                bool qualities = observation.AnarchicCount == 1 &&
                    observation.EnhancementFiveCount == 1 && reliable == 1 &&
                    observation.RealQualityTextPresent &&
                    observation.NativeText.IndexOf("Reliable",
                        StringComparison.Ordinal) >= 0;
                bool presentation = observation.NullCount == 0 && hidden == 2 &&
                    observation.NativeText.IndexOf("KMG_StateToken",
                        StringComparison.Ordinal) < 0 &&
                    observation.NativeText.IndexOf("KMG_BatteredFirearm_Origin",
                        StringComparison.Ordinal) < 0 &&
                    FirearmConditionPresentation.Describe(
                        snapshot.Repository.State.Condition).IndexOf(
                            "Normal", StringComparison.Ordinal) >= 0;
                diagnostics.Add("persistentTooltip=blueprint=" + blueprint +
                    ";guid=" + item.Blueprint.AssetGuid + ";markers=" +
                    markers.Length + ";state=" + snapshot.Repository.State +
                    ";stateTokens=" + observation.StateMarkers +
                    ";originTokens=" + observation.OriginMarkers +
                    ";originOwner=" + origin + ";anarchic=" +
                    observation.AnarchicCount + ";plus5=" +
                    observation.EnhancementFiveCount + ";reliable=" +
                    reliable + ";hiddenMarkers=" + hidden + ";null=" +
                    observation.NullCount + ";qualities=" + qualities +
                    ";presentation=" + presentation);
                diagnostics.AddRange(observation.EnchantmentDiagnostics);
                return new CraftMagicItemsTooltipInspectionResult(
                    blueprint && mechanics && qualities && presentation,
                    diagnostics);
            }
            catch (Exception exception)
            {
                diagnostics.Add("persistentTooltipException=" + exception);
                return new CraftMagicItemsTooltipInspectionResult(false,
                    diagnostics);
            }
        }

        private static ItemEntityWeapon Create(BlueprintItemWeapon blueprint,
            ICollection<ItemEntityWeapon> items)
        {
            ItemEntityWeapon value = blueprint == null ? null :
                blueprint.CreateEntity() as ItemEntityWeapon;
            if (value == null) throw new InvalidOperationException(
                "A request-local firearm entity could not be created.");
            value.Identify();
            items.Add(value);
            return value;
        }

        private static void SetState(ItemEntityWeapon item, int rounds,
            AmmunitionId ammunition, FirearmCondition condition)
        {
            FirearmRuntimeState.SeedLegacyTokenForDebug(item,
                new FirearmState(FirearmState.CurrentSchemaVersion, rounds,
                    ammunition, condition));
        }

        private static ItemInspection Inspect(string identity,
            ItemEntityWeapon item)
        {
            string nativeText = NativeEnchantmentText(item);
            int state = item.Enchantments.Count(value => value != null &&
                HasComponent<FirearmStateTokenComponent>(value.Blueprint));
            int origin = item.Enchantments.Count(value => value != null &&
                HasComponent<BatteredFirearmOriginComponent>(value.Blueprint));
            int anarchic = item.Enchantments.Count(value => value != null &&
                string.Equals(value.Blueprint.AssetGuid, AnarchicGuid,
                    StringComparison.Ordinal));
            int plusFive = item.Enchantments.Count(value => value != null &&
                string.Equals(value.Blueprint.AssetGuid,
                    EnhancementFiveGuid, StringComparison.Ordinal));
            int nullCount = Count(nativeText, "<null>");
            bool realText = nativeText.IndexOf("Anarchic",
                    StringComparison.Ordinal) >= 0 &&
                nativeText.IndexOf("Enhancement +5",
                    StringComparison.Ordinal) >= 0;
            var diagnostics = new List<string>();
            foreach (BlueprintItemEnchantment enchantment in
                item.Blueprint.Enchantments as
                    IEnumerable<BlueprintItemEnchantment> ??
                    Enumerable.Empty<BlueprintItemEnchantment>())
                diagnostics.Add(DescribeEnchantment(identity, "blueprint",
                    enchantment, item));
            foreach (ItemEnchantment enchantment in item.Enchantments)
                diagnostics.Add(DescribeEnchantment(identity, "entity",
                    enchantment == null ? null : enchantment.Blueprint,
                    item));
            return new ItemInspection(identity, state, origin, anarchic,
                plusFive, nullCount, realText, nativeText, diagnostics);
        }

        private static string NativeEnchantmentText(ItemEntityWeapon item)
        {
            TooltipData data = UIUtilityItem.FillTooltipData(item,
                new TooltipData());
            if (data == null || data.Texts == null)
                throw new InvalidOperationException(
                    "Kingmaker produced no native TooltipData text graph.");
            string graph = string.Join("\n", data.Texts.OrderBy(value =>
                    value.Key.ToString(), StringComparer.Ordinal)
                .Select(value => value.Key + "=" +
                    (value.Value ?? string.Empty)).ToArray());
            MethodInfo method = typeof(UIUtilityItem).GetMethod(
                "FillEnchantmentDescription", Static, null,
                new[] { typeof(ItemEntity), typeof(TooltipData) }, null);
            if (method == null)
                throw new MissingMethodException(typeof(UIUtilityItem)
                    .FullName, "FillEnchantmentDescription");
            try
            {
                string detail = method.Invoke(null, new object[] { item,
                    data }) as string ?? string.Empty;
                return graph + "\nFillEnchantmentDescription=" + detail;
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        private static string DescribeEnchantment(string itemIdentity,
            string location, BlueprintItemEnchantment enchantment,
            ItemEntityWeapon item)
        {
            if (enchantment == null)
                return "tooltipEnchantment=" + itemIdentity + ":" +
                    location + ":<null>";
            string[] components = (enchantment.ComponentsArray ??
                    new BlueprintComponent[0])
                .Where(value => value != null)
                .Select(value => value.GetType().FullName)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            bool blueprintOwned = (item.Blueprint.Enchantments as
                    IEnumerable<BlueprintItemEnchantment> ??
                    Enumerable.Empty<BlueprintItemEnchantment>())
                .Any(value => ReferenceEquals(value, enchantment));
            string provenance = HasComponent<FirearmStateTokenComponent>(
                    enchantment) ? "state-owned" :
                HasComponent<BatteredFirearmOriginComponent>(enchantment)
                    ? "origin-owned" : blueprintOwned ? "cmi-blueprint" :
                    "item-owned";
            return "tooltipEnchantment=" + itemIdentity + ":" + location +
                ":guid=" + enchantment.AssetGuid + ":internal=" +
                (enchantment.name ?? string.Empty) + ":name=" +
                (enchantment.Name ?? string.Empty) + ":description=" +
                (enchantment.Description ?? string.Empty) +
                ":components=" + string.Join(",", components) +
                ":provenance=" + provenance;
        }

        private static bool HasComponent<T>(
            BlueprintItemEnchantment enchantment)
            where T : BlueprintComponent
        {
            return enchantment != null &&
                (enchantment.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<T>().Any();
        }

        private static int Count(string value, string token)
        {
            int count = 0;
            for (int index = 0; value != null && token != null &&
                (index = value.IndexOf(token, index,
                    StringComparison.Ordinal)) >= 0; index += token.Length)
                count++;
            return count;
        }

        private sealed class ItemInspection
        {
            internal ItemInspection(string identity, int stateMarkers,
                int originMarkers, int anarchicCount,
                int enhancementFiveCount, int nullCount,
                bool realQualityTextPresent, string nativeText,
                IEnumerable<string> enchantmentDiagnostics)
            {
                Identity = identity;
                StateMarkers = stateMarkers;
                OriginMarkers = originMarkers;
                AnarchicCount = anarchicCount;
                EnhancementFiveCount = enhancementFiveCount;
                NullCount = nullCount;
                RealQualityTextPresent = realQualityTextPresent;
                NativeText = nativeText ?? string.Empty;
                EnchantmentDiagnostics = (enchantmentDiagnostics ??
                    new string[0]).ToArray();
            }

            internal string Identity { get; private set; }
            internal int StateMarkers { get; private set; }
            internal int OriginMarkers { get; private set; }
            internal int AnarchicCount { get; private set; }
            internal int EnhancementFiveCount { get; private set; }
            internal int NullCount { get; private set; }
            internal bool RealQualityTextPresent { get; private set; }
            internal string NativeText { get; private set; }
            internal string[] EnchantmentDiagnostics { get; private set; }
            internal string Description
            {
                get
                {
                    return Identity + ":state=" + StateMarkers +
                        ":origin=" + OriginMarkers + ":anarchic=" +
                        AnarchicCount + ":plus5=" +
                        EnhancementFiveCount + ":null=" + NullCount +
                        ":text=" + NativeText.Replace('\r', ' ')
                            .Replace('\n', ' ');
                }
            }
        }
    }
}
