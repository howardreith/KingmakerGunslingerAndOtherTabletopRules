using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Recovery;

namespace KingmakerGunslinger.Development
{
    /// <summary>
    /// Development-only adapter for the currently loaded Kingmaker campaign. It uses
    /// guarded reflection because the exact public shape of selection and inventory
    /// services differs across Kingmaker builds. Every mutating operation verifies its
    /// result and fails closed when the expected runtime contract is absent.
    /// </summary>
    internal sealed partial class KingmakerDevelopmentBridge
    {
        private static readonly string[] SelectedUnitPaths =
        {
            "UI.SelectionManager.SingleSelectedUnit",
            "UI.SelectionManagerPC.SingleSelectedUnit",
            "UI.SelectionManager.FirstSelectUnit",
            "UI.SelectionManagerPC.FirstSelectUnit",
            "UI.SelectionManager.SelectedUnits",
            "UI.SelectionManagerPC.SelectedUnits",
            "UI.SelectionManager.SelectedUnit",
            "UI.SelectionManager.SelectedCharacter",
            "UI.SelectionManager.CurrentUnit",
            "UI.SelectionManager.SelectedCharacters"
        };

        private static readonly string[] MainCharacterMembers =
        {
            "MainCharacterEntity",
            "MainCharacter",
            "MainCharacterUnit"
        };

        private static readonly string[] DescriptorMembers =
        {
            "Descriptor",
            "UnitDescriptor"
        };

        private static readonly string[] InventoryMembers =
        {
            "Inventory",
            "SharedInventory",
            "SharedStash"
        };

        private static readonly string[] InventoryItemsMembers =
        {
            "Items",
            "RawItems",
            "m_Items"
        };

        private static readonly string[] ItemBlueprintMembers =
        {
            "Blueprint",
            "m_Blueprint",
            "BlueprintItem",
            "ItemBlueprint"
        };

        private static readonly string[] EquippedWeaponPaths =
        {
            "Body.PrimaryHand.MaybeWeapon",
            "Body.PrimaryHand.Weapon",
            "Body.PrimaryHand.MaybeItem",
            "Body.PrimaryHand.Item",
            "Body.SecondaryHand.MaybeWeapon",
            "Body.SecondaryHand.Weapon",
            "Body.SecondaryHand.MaybeItem",
            "Body.SecondaryHand.Item",
            "Descriptor.Body.PrimaryHand.MaybeWeapon",
            "Descriptor.Body.PrimaryHand.Weapon",
            "Descriptor.Body.PrimaryHand.MaybeItem",
            "Descriptor.Body.SecondaryHand.MaybeWeapon",
            "Descriptor.Body.SecondaryHand.Weapon",
            "Descriptor.Body.SecondaryHand.MaybeItem",
            "Body.CurrentHandEquipmentSet.PrimaryHand.MaybeWeapon",
            "Body.CurrentHandEquipmentSet.PrimaryHand.Weapon",
            "Body.CurrentHandEquipmentSet.SecondaryHand.MaybeWeapon",
            "Body.CurrentHandEquipmentSet.SecondaryHand.Weapon"
        };

        private static readonly AmmunitionId DebugLeadBall =
            FirearmStateTokenCatalog.DiagnosticLeadBall;

        private readonly BlueprintFeature _firearmProficiency;
        private readonly BlueprintAbility _reloadAbility;
        private readonly BlueprintAbility _overhaulAbility;
        private readonly BlueprintAbility _repairAbility;
        private readonly BlueprintItemWeapon _testMusketItem;
        private readonly BlueprintItem _blackPowderItem;
        private readonly BlueprintItem _leadBallItem;
        private readonly BlueprintItem _repairKitItem;
        private readonly FirearmItemStateService _stateService;
        private readonly IFirearmItemIdentityProvider _identityProvider;

        internal KingmakerDevelopmentBridge(
            BlueprintFeature firearmProficiency,
            BlueprintAbility reloadAbility,
            BlueprintAbility overhaulAbility,
            BlueprintAbility repairAbility,
            BlueprintItemWeapon testMusketItem,
            BlueprintItem blackPowderItem,
            BlueprintItem leadBallItem,
            BlueprintItem repairKitItem)
        {
            _firearmProficiency = firearmProficiency ??
                throw new ArgumentNullException("firearmProficiency");
            _reloadAbility = reloadAbility ??
                throw new ArgumentNullException("reloadAbility");
            _overhaulAbility = overhaulAbility ??
                throw new ArgumentNullException("overhaulAbility");
            _repairAbility = repairAbility ??
                throw new ArgumentNullException("repairAbility");
            _testMusketItem = testMusketItem ??
                throw new ArgumentNullException("testMusketItem");
            _blackPowderItem = blackPowderItem ??
                throw new ArgumentNullException("blackPowderItem");
            _leadBallItem = leadBallItem ??
                throw new ArgumentNullException("leadBallItem");
            _repairKitItem = repairKitItem ??
                throw new ArgumentNullException("repairKitItem");
            if (ReferenceEquals(_blackPowderItem, _leadBallItem) ||
                ReferenceEquals(_blackPowderItem, _repairKitItem) ||
                ReferenceEquals(_leadBallItem, _repairKitItem))
            {
                throw new ArgumentException(
                    "Black Powder Charge, Lead Ball, and Firearm Repair Kit must use distinct item blueprints.");
            }

            _stateService = FirearmRuntimeState.Service;
            _identityProvider = new KingmakerFirearmItemIdentityProvider();
        }

        internal DevelopmentActionResult GrantFirearmProficiency()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = runtime.UnitDescriptor as UnitDescriptor;
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "The selected unit did not expose a concrete Kingmaker UnitDescriptor.");
            }

            bool alreadyHadProficiency = HasFeature(descriptor, _firearmProficiency);
            Feature granted = null;
            if (!alreadyHadProficiency)
            {
                granted = descriptor.Progression.Features.AddFeature(
                    _firearmProficiency,
                    null);
            }

            int rank = descriptor.Progression.Features.GetRank(_firearmProficiency);
            if (rank <= 0 || (!alreadyHadProficiency && granted == null))
            {
                throw new InvalidOperationException(
                    "Kingmaker's typed FeatureCollection.AddFeature call did not retain Firearm Proficiency.");
            }

            Ability reloadAbility = EnsureReloadAbility(descriptor);
            Ability overhaulAbility = EnsureOverhaulAbility(descriptor);
            Ability repairAbility = EnsureRepairAbility(descriptor);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    alreadyHadProficiency
                        ? "{0} already had Firearm Proficiency; verified rank={1}; Reload restored={2}; Overhaul restored={3}; Repair restored={4}; abilityType={5}."
                        : "Granted Firearm Proficiency to {0}; verified rank={1}; Reload restored={2}; Overhaul restored={3}; Repair restored={4}; factType={5}.",
                    runtime.UnitName,
                    rank,
                    reloadAbility != null,
                    overhaulAbility != null,
                    repairAbility != null,
                    alreadyHadProficiency
                        ? reloadAbility.GetType().FullName
                        : granted.GetType().FullName));
        }

        internal DevelopmentActionResult DescribeReloadReadiness()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = runtime.UnitDescriptor as UnitDescriptor;
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "The selected unit did not expose a concrete Kingmaker UnitDescriptor.");
            }

            Ability ability = descriptor.Abilities.GetAbility(_reloadAbility);
            ReloadTestMusketAvailability availability = ReloadTestMusketRuntime.Evaluate(
                descriptor,
                _testMusketItem,
                _blackPowderItem,
                _leadBallItem);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Selected unit={0}; hasReloadAbility={1}; readiness=[{2}]; runtime=[{3}].",
                    runtime.UnitName,
                    ability != null,
                    availability,
                    ReloadRuntimeDiagnostics.Describe()));
        }

        internal DevelopmentActionResult ReloadEquippedTestMusketNowForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = runtime.UnitDescriptor as UnitDescriptor;
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "The selected unit did not expose a concrete Kingmaker UnitDescriptor.");
            }

            EnsureReloadAbility(descriptor);
            ReloadTestMusketAvailability availability = ReloadTestMusketRuntime.Evaluate(
                descriptor,
                _testMusketItem,
                _blackPowderItem,
                _leadBallItem);
            if (!availability.IsAvailable)
            {
                return DevelopmentActionResult.Failure(
                    "Immediate diagnostic reload was rejected without mutation: " +
                    availability.Reason);
            }

            FirearmReloadResult result = ReloadTestMusketRuntime.Execute(
                descriptor,
                _testMusketItem,
                _blackPowderItem,
                _leadBallItem);
            ReloadRuntimeDiagnostics.Record(result);
            return DevelopmentActionResult.Success(
                "Immediate diagnostic reload completed; this bypassed full-round action economy: " +
                result + ".");
        }


        internal DevelopmentActionResult DescribeOverhaulReadiness()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = runtime.UnitDescriptor as UnitDescriptor;
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "The selected unit did not expose a concrete Kingmaker UnitDescriptor.");
            }

            Ability ability = descriptor.Abilities.GetAbility(_overhaulAbility);
            FirearmOverhaulAvailability availability = OverhaulTestMusketRuntime.Evaluate(
                descriptor,
                _testMusketItem,
                _repairKitItem);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Selected unit={0}; hasOverhaulAbility={1}; readiness=[{2}]; runtime=[{3}].",
                    runtime.UnitName,
                    ability != null,
                    availability,
                    OverhaulRuntimeDiagnostics.Describe()));
        }

        internal DevelopmentActionResult OverhaulEquippedTestMusketNowForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = runtime.UnitDescriptor as UnitDescriptor;
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "The selected unit did not expose a concrete Kingmaker UnitDescriptor.");
            }

            EnsureOverhaulAbility(descriptor);
            FirearmOverhaulAvailability availability = OverhaulTestMusketRuntime.Evaluate(
                descriptor,
                _testMusketItem,
                _repairKitItem);
            if (!availability.IsAvailable)
            {
                return DevelopmentActionResult.Failure(
                    "Immediate diagnostic overhaul was rejected without mutation: " +
                    availability.Reason);
            }

            FirearmOverhaulRuntimeResult result = OverhaulTestMusketRuntime.Execute(
                descriptor,
                _testMusketItem,
                _repairKitItem);
            OverhaulRuntimeDiagnostics.Record(result);
            return DevelopmentActionResult.Success(
                "Immediate diagnostic overhaul completed; this bypassed full-round action economy: " +
                result + ".");
        }

        internal DevelopmentActionResult AddTestMusket()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            int before = CountMatchingInventoryItems(inventory, _testMusketItem);

            object result;
            string method;
            object[][] argumentSets =
            {
                new object[] { _testMusketItem, 1, false },
                new object[] { _testMusketItem, 1 },
                new object[] { _testMusketItem }
            };
            if (!ReflectionAccess.TryInvokeAny(
                inventory,
                new[] { "Add", "AddItem", "AddItemSilent" },
                argumentSets,
                out result,
                out method))
            {
                throw new MissingMethodException(
                    "Could not resolve a compatible Kingmaker shared-inventory add method.");
            }

            int after = CountMatchingInventoryItems(inventory, _testMusketItem);
            if (after <= before && !ResultLooksLikeCreatedItem(result, _testMusketItem))
            {
                throw new InvalidOperationException(
                    "The inventory add call completed but no Test Musket could be verified in shared inventory.");
            }

            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Added one Test Musket to shared inventory through {0}; verified count={1}.",
                    method,
                    Math.Max(after, before + 1)));
        }

        internal DevelopmentActionResult AddFirearmRepairKits(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "amount",
                    amount,
                    "The amount to add must be positive.");
            }

            KingmakerRepairKitInventory inventory = ResolveRepairKitInventory();
            RepairKitInventorySnapshot before = RepairKitInventorySnapshot.Capture(inventory);
            inventory.Add(amount);
            RepairKitInventorySnapshot after = RepairKitInventorySnapshot.Capture(inventory);
            if (after.RepairKits != before.RepairKits + amount)
            {
                throw new InvalidOperationException(
                    "Kingmaker did not retain the exact requested Firearm Repair Kit quantity.");
            }

            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Added {0} Firearm Repair Kit item(s); before=[{1}]; after=[{2}].",
                    amount,
                    before,
                    after));
        }

        internal DevelopmentActionResult DescribeFirearmRepairKits()
        {
            KingmakerRepairKitInventory inventory = ResolveRepairKitInventory();
            RepairKitInventorySnapshot snapshot = RepairKitInventorySnapshot.Capture(inventory);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Shared-inventory Firearm Repair Kits: {0}; blueprint={1}/{2}; overhaulRuntime=[{3}]; repairRuntime=[{4}].",
                    snapshot,
                    _repairKitItem.name,
                    _repairKitItem.AssetGuid,
                    OverhaulRuntimeDiagnostics.Describe(),
                    RepairRuntimeDiagnostics.Describe()));
        }

        internal DevelopmentActionResult RemoveAllFirearmRepairKits()
        {
            KingmakerRepairKitInventory inventory = ResolveRepairKitInventory();
            RepairKitInventorySnapshot before = RepairKitInventorySnapshot.Capture(inventory);
            if (before.RepairKits > 0)
            {
                inventory.Remove(before.RepairKits);
            }

            RepairKitInventorySnapshot after = RepairKitInventorySnapshot.Capture(inventory);
            if (after.RepairKits != 0)
            {
                throw new InvalidOperationException(
                    "Some Firearm Repair Kits remained after the remove-all operation.");
            }

            return DevelopmentActionResult.Success(
                "Removed all Firearm Repair Kits from shared inventory; before=[" +
                before + "]; after=[" + after + "].");
        }

        internal DevelopmentActionResult AddBasicAmmunition(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "amount",
                    amount,
                    "The amount to add must be positive.");
            }

            KingmakerBasicAmmunitionInventory inventory =
                ResolveBasicAmmunitionInventory();
            BasicAmmunitionInventorySnapshot before =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            inventory.Add(BasicAmmunitionComponent.BlackPowderCharge, amount);
            inventory.Add(BasicAmmunitionComponent.LeadBall, amount);
            BasicAmmunitionInventorySnapshot after =
                BasicAmmunitionInventorySnapshot.Capture(inventory);

            if (after.BlackPowderCharges != before.BlackPowderCharges + amount ||
                after.LeadBalls != before.LeadBalls + amount)
            {
                throw new InvalidOperationException(
                    "Kingmaker did not retain the exact requested basic-ammunition quantities.");
            }

            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Added {0} Black Powder Charge(s) and {0} Lead Ball(s); before=[{1}]; after=[{2}].",
                    amount,
                    before,
                    after));
        }

        internal DevelopmentActionResult AddBlackPowder(int amount)
        {
            return AddSingleBasicAmmunition(
                BasicAmmunitionComponent.BlackPowderCharge,
                amount);
        }

        internal DevelopmentActionResult AddLeadBalls(int amount)
        {
            return AddSingleBasicAmmunition(
                BasicAmmunitionComponent.LeadBall,
                amount);
        }

        internal DevelopmentActionResult DescribeBasicAmmunition()
        {
            KingmakerBasicAmmunitionInventory inventory =
                ResolveBasicAmmunitionInventory();
            BasicAmmunitionInventorySnapshot snapshot =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Shared-inventory basic ammunition: {0}; powderBlueprint={1}/{2}; leadBallBlueprint={3}/{4}.",
                    snapshot,
                    _blackPowderItem.name,
                    _blackPowderItem.AssetGuid,
                    _leadBallItem.name,
                    _leadBallItem.AssetGuid));
        }

        internal DevelopmentActionResult ConsumeOneBasicAmmunitionLoad()
        {
            KingmakerBasicAmmunitionInventory inventory =
                ResolveBasicAmmunitionInventory();
            var service = new BasicAmmunitionTransactionService();
            BasicAmmunitionTransactionResult result =
                service.TryConsumeOneLoad(inventory);

            return DevelopmentActionResult.Success(
                result.Succeeded
                    ? "Atomically consumed one Black Powder Charge and one Lead Ball; " + result + "."
                    : "No ammunition was consumed because a complete powder-and-ball load was unavailable; " + result + ".");
        }

        internal DevelopmentActionResult RemoveAllBasicAmmunition()
        {
            KingmakerBasicAmmunitionInventory inventory =
                ResolveBasicAmmunitionInventory();
            BasicAmmunitionInventorySnapshot before =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            if (before.BlackPowderCharges > 0)
            {
                inventory.Remove(
                    BasicAmmunitionComponent.BlackPowderCharge,
                    before.BlackPowderCharges);
            }

            if (before.LeadBalls > 0)
            {
                inventory.Remove(
                    BasicAmmunitionComponent.LeadBall,
                    before.LeadBalls);
            }

            BasicAmmunitionInventorySnapshot after =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            if (after.BlackPowderCharges != 0 || after.LeadBalls != 0)
            {
                throw new InvalidOperationException(
                    "Some basic ammunition remained after the remove-all operation.");
            }

            return DevelopmentActionResult.Success(
                "Removed all basic ammunition from shared inventory; before=[" +
                before + "]; after=[" + after + "].");
        }

        private DevelopmentActionResult AddSingleBasicAmmunition(
            BasicAmmunitionComponent component,
            int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "amount",
                    amount,
                    "The amount to add must be positive.");
            }

            KingmakerBasicAmmunitionInventory inventory =
                ResolveBasicAmmunitionInventory();
            BasicAmmunitionInventorySnapshot before =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            inventory.Add(component, amount);
            BasicAmmunitionInventorySnapshot after =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            if (after.Count(component) != before.Count(component) + amount)
            {
                throw new InvalidOperationException(
                    "Kingmaker did not retain the exact requested ammunition amount.");
            }

            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Added {0} {1} item(s); before=[{2}]; after=[{3}].",
                    amount,
                    component,
                    before,
                    after));
        }

        private KingmakerRepairKitInventory ResolveRepairKitInventory()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            Player player = runtime.Player as Player;
            if (player == null || player.Inventory == null)
            {
                throw new InvalidOperationException(
                    "The active Kingmaker player has no typed shared inventory.");
            }

            return new KingmakerRepairKitInventory(
                player.Inventory,
                _repairKitItem);
        }

        private KingmakerBasicAmmunitionInventory ResolveBasicAmmunitionInventory()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            Player player = runtime.Player as Player;
            if (player == null || player.Inventory == null)
            {
                throw new InvalidOperationException(
                    "The active Kingmaker player has no typed shared inventory.");
            }

            return new KingmakerBasicAmmunitionInventory(
                player.Inventory,
                _blackPowderItem,
                _leadBallItem);
        }

        internal DevelopmentActionResult RemoveTestMuskets()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            List<object> matches = EnumerateInventoryItems(inventory)
                .Where(item => ItemUsesBlueprint(item, _testMusketItem))
                .ToList();

            int removed = 0;
            int forgottenStates = 0;
            foreach (object item in matches)
            {
                object ignored;
                string method;
                object[][] argumentSets =
                {
                    new object[] { item, 1, false },
                    new object[] { item, 1 },
                    new object[] { item }
                };
                if (!ReflectionAccess.TryInvokeAny(
                    inventory,
                    new[] { "Remove", "RemoveItem" },
                    argumentSets,
                    out ignored,
                    out method))
                {
                    throw new MissingMethodException(
                        "Could not resolve a compatible Kingmaker shared-inventory remove method.");
                }

                removed++;
                if (_stateService.Forget(item))
                {
                    forgottenStates++;
                }
            }

            int remaining = CountMatchingInventoryItems(inventory, _testMusketItem);
            if (remaining != 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Attempted to remove {0} Test Musket item(s), but {1} remain in shared inventory. Unequip equipped copies before retrying.",
                        removed,
                        remaining));
            }

            return DevelopmentActionResult.Success(
                removed == 0
                    ? "No unequipped Test Muskets were present in shared inventory."
                    : string.Format(
                        CultureInfo.InvariantCulture,
                        "Removed {0} unequipped Test Musket item(s) from shared inventory and cleared {1} item-token/repository state combination(s).",
                        removed,
                        forgottenStates));
        }

        internal DevelopmentActionResult DescribeEquippedFirearms()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            List<FirearmItemStateSnapshot> snapshots = DescribeCandidates(
                CollectEquippedRuntimeWeaponItems(runtime));
            bool hasProficiency = HasFeature(runtime.UnitDescriptor, _firearmProficiency);

            string message = snapshots.Count == 0
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Selected unit={0}; firearmProficiency={1}; equippedFirearms=none detected.",
                    runtime.UnitName,
                    hasProficiency)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "Selected unit={0}; firearmProficiency={1}; equippedFirearms={2}; {3}.",
                    runtime.UnitName,
                    hasProficiency,
                    snapshots.Count,
                    FormatSnapshots(snapshots));
            return DevelopmentActionResult.Success(message);
        }

        internal DevelopmentActionResult DescribeVisibleFirearmStates()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object inventory = RequireInventory(runtime.Player);
            var candidates = new List<object>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            AddReferenceDistinct(
                CollectEquippedRuntimeWeaponItems(runtime),
                candidates,
                seen);
            AddReferenceDistinct(
                EnumerateInventoryItems(inventory),
                candidates,
                seen);

            List<FirearmItemStateSnapshot> snapshots = DescribeCandidates(candidates);
            IFirearmStateRepository repository = _stateService.Repository;
            string message = snapshots.Count == 0
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Selected unit={0}; no exact runtime firearm items were found in equipment or shared inventory; carrier={1}; repositoryCreated={2}; mutations={3}; removals={4}.",
                    runtime.UnitName,
                    FirearmRuntimeState.CarrierDescription,
                    repository.CreatedEntryCount,
                    repository.MutationCount,
                    repository.RemovalCount)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "Selected unit={0}; visibleFirearms={1}; carrier={2}; repositoryCreated={3}; mutations={4}; removals={5}; {6}.",
                    runtime.UnitName,
                    snapshots.Count,
                    FirearmRuntimeState.CarrierDescription,
                    repository.CreatedEntryCount,
                    repository.MutationCount,
                    repository.RemovalCount,
                    FormatSnapshots(snapshots));
            return DevelopmentActionResult.Success(message);
        }

        internal PersistenceEvidenceSnapshotData CapturePersistenceEvidenceSnapshot()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object inventory = RequireInventory(runtime.Player);
            var candidates = new List<object>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            AddReferenceDistinct(
                CollectEquippedRuntimeWeaponItems(runtime),
                candidates,
                seen);
            AddReferenceDistinct(
                EnumerateInventoryItems(inventory),
                candidates,
                seen);

            List<PersistenceFirearmEvidenceData> firearms =
                CaptureStrictEvidenceFirearms(candidates);
            IFirearmStateRepository repository = _stateService.Repository;

            return new PersistenceEvidenceSnapshotData
            {
                SchemaVersion = 1,
                CapturedAtUtc = DateTimeOffset.UtcNow.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                SelectedUnitName = runtime.UnitName,
                IdentityRecordCount = FirearmRuntimeState.IdentityVaultRecordCount,
                LegacyReferenceRecordCount = FirearmRuntimeState.LegacyReferenceRecordCount,
                RepositoryEntriesCreated = repository.CreatedEntryCount,
                RepositoryMutations = repository.MutationCount,
                RepositoryRemovals = repository.RemovalCount,
                IdentityMigration = FirearmRuntimeState.IdentityMigrationSnapshot.ToString(),
                TokenMigration = FirearmRuntimeState.TokenMigrationSnapshot.ToString(),
                Firearms = firearms
            };
        }

        internal DevelopmentActionResult CreatePersistenceFixtureAd()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            int addAttempts = 0;
            while (CountMatchingInventoryItems(inventory, _testMusketItem) < 4 && addAttempts < 4)
            {
                AddTestMusket();
                addAttempts++;
            }

            var exact = new List<object>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            foreach (object item in EnumerateInventoryItems(inventory))
            {
                if (!ItemUsesBlueprint(item, _testMusketItem) || !seen.Add(item))
                {
                    continue;
                }

                FirearmItemStateSnapshot ignored;
                string reason;
                if (_stateService.TryGetOrCreate(item, out ignored, out reason))
                {
                    exact.Add(item);
                }
            }

            if (exact.Count < 4)
            {
                return DevelopmentActionResult.Failure(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The token fixture requires four exact Test Musket item instances; found={0} after addAttempts={1}.",
                        exact.Count,
                        addAttempts));
            }

            object aItem = exact[0];
            object bItem = exact[1];
            object cItem = exact[2];
            object dItem = exact[3];
            FirearmItemStateSnapshot baseline = _stateService.GetOrCreate(aItem);
            FirearmStateRules rules = CreateDebugRules(baseline.Definition);
            FirearmState loadedNormal = FirearmStateMachine.Load(
                FirearmState.CreateEmpty(),
                rules,
                DebugLeadBall,
                1);
            FirearmState brokenEmpty =
                FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty());
            FirearmState brokenLoaded =
                FirearmStateMachine.ApplyMisfireDamage(loadedNormal);

            FirearmItemStateSnapshot a = _stateService.Set(aItem, loadedNormal);
            FirearmItemStateSnapshot b = _stateService.Set(bItem, brokenEmpty);
            FirearmItemStateSnapshot c = _stateService.Set(cItem, brokenLoaded);
            _stateService.Set(dItem, FirearmState.CreateEmpty());
            _stateService.Forget(dItem);
            FirearmItemStateSnapshot d = _stateService.GetOrCreate(dItem);

            if (a.Repository.State != loadedNormal ||
                b.Repository.State != brokenEmpty ||
                c.Repository.State != brokenLoaded ||
                d.Repository.State != FirearmState.CreateEmpty())
            {
                throw new InvalidOperationException(
                    "The A-D item-token fixture states did not verify after assignment.");
            }

            bool aToken = FirearmRuntimeState.HasIdentityVaultRecord(aItem);
            bool bToken = FirearmRuntimeState.HasIdentityVaultRecord(bItem);
            bool cToken = FirearmRuntimeState.HasIdentityVaultRecord(cItem);
            bool dToken = FirearmRuntimeState.HasIdentityVaultRecord(dItem);
            if (!aToken || !bToken || !cToken || dToken)
            {
                throw new InvalidOperationException(
                    "The A-D fixture did not produce one item-owned token on A-C and no token on D.");
            }

            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Created/normalized item-token fixture: A runtime={0} state=[{1}] token=true; B runtime={2} state=[{3}] token=true; C runtime={4} state=[{5}] token=true; D runtime={6} state=[{7}] token=false. Save, exit to desktop, restart, reload, and print visible states to test durability.",
                    a.Repository.RepositoryIdentity,
                    a.Repository.State,
                    b.Repository.RepositoryIdentity,
                    b.Repository.State,
                    c.Repository.RepositoryIdentity,
                    c.Repository.State,
                    d.Repository.RepositoryIdentity,
                    d.Repository.State));
        }

        internal DevelopmentActionResult PrimeIndependentTestMusketStates()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            var candidates = new List<object>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            foreach (object item in EnumerateInventoryItems(inventory))
            {
                if (ItemUsesBlueprint(item, _testMusketItem) && seen.Add(item))
                {
                    candidates.Add(item);
                }
            }

            var exactFirearms = new List<object>();
            foreach (object candidate in candidates)
            {
                FirearmItemStateSnapshot ignored;
                string reason;
                if (_stateService.TryGetOrCreate(candidate, out ignored, out reason))
                {
                    exactFirearms.Add(candidate);
                }
            }

            if (exactFirearms.Count < 2)
            {
                return DevelopmentActionResult.Failure(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Two unequipped Test Musket item instances are required in shared inventory; exact instances found={0}. Add two muskets, then retry.",
                        exactFirearms.Count));
            }

            object firstItem = exactFirearms[0];
            object secondItem = exactFirearms[1];
            FirearmItemStateSnapshot firstBefore = _stateService.GetOrCreate(firstItem);
            FirearmState firstLoaded = FirearmStateMachine.Load(
                FirearmState.CreateEmpty(),
                CreateDebugRules(firstBefore.Definition),
                DebugLeadBall,
                1);
            FirearmItemStateSnapshot firstAfter = _stateService.Set(firstItem, firstLoaded);
            FirearmItemStateSnapshot secondAfter = _stateService.Set(
                secondItem,
                FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty()));
            FirearmItemStateSnapshot firstVerified = _stateService.GetOrCreate(firstItem);

            if (firstAfter.Repository.EntryId == secondAfter.Repository.EntryId)
            {
                throw new InvalidOperationException(
                    "Two distinct Test Musket objects resolved to one repository entry.");
            }

            if (firstVerified.Repository.State != firstLoaded ||
                secondAfter.Repository.State.Condition != FirearmCondition.Broken ||
                !secondAfter.Repository.State.IsEmpty)
            {
                throw new InvalidOperationException(
                    "Independent Test Musket states could not be verified after assignment.");
            }

            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Assigned independent item-owned token states without inventory ammunition: first=[{0}]; second=[{1}]. Save/load and process restart remain unproven until the lifecycle matrix is run.",
                    firstVerified,
                    secondAfter));
        }

        internal DevelopmentActionResult SeedLegacyStateTokenForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object item = CollectEquippedRuntimeWeaponItems(runtime)
                .FirstOrDefault(candidate => ItemUsesBlueprint(candidate, _testMusketItem));
            if (item == null)
            {
                return DevelopmentActionResult.Failure(
                    "Equip a Test Musket on the selected unit before creating the legacy-token migration fixture.");
            }

            FirearmState legacyState = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                1,
                DebugLeadBall,
                FirearmCondition.Broken);
            FirearmRuntimeState.SeedLegacyTokenForDebug(item, legacyState);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Seeded one Sprint 12 broken/loaded legacy token on {0}'s equipped Test Musket. No UnitPart identity record was written. Use a normal state read or the visible-state diagnostic next to exercise verified one-way token migration.",
                    runtime.UnitName));
        }

        internal DevelopmentActionResult SeedLegacyReferenceStateForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object item = CollectEquippedRuntimeWeaponItems(runtime)
                .FirstOrDefault(candidate => ItemUsesBlueprint(candidate, _testMusketItem));
            if (item == null)
            {
                return DevelopmentActionResult.Failure(
                    "Equip a Test Musket on the selected unit before creating the Sprint 13 direct-reference migration fixture.");
            }

            FirearmState legacyState = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Broken);
            FirearmRuntimeState.SeedLegacyReferenceForDebug(item, legacyState);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Seeded one Sprint 13 broken/empty direct-reference vault record on {0}'s equipped Test Musket. No Sprint 14 identity record was written. Use a normal state read next to exercise verified one-way reference-to-identity migration.",
                    runtime.UnitName));
        }

        internal DevelopmentActionResult LoadFirstEquippedFirearmForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object item;
            FirearmItemStateSnapshot current = RequireFirstEquippedFirearm(runtime, out item);
            FirearmStateRules rules = CreateDebugRules(current.Definition);
            FirearmItemStateSnapshot updated = _stateService.Transition(
                item,
                state => FirearmStateMachine.Load(state, rules, DebugLeadBall, 1));
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Loaded one development-only round into {0}'s first equipped firearm without consuming inventory: {1}. The state is now encoded by an item-owned enchantment token.",
                    runtime.UnitName,
                    updated));
        }

        internal DevelopmentActionResult DamageFirstEquippedFirearmForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object item;
            RequireFirstEquippedFirearm(runtime, out item);
            FirearmItemStateSnapshot updated = _stateService.Transition(
                item,
                FirearmStateMachine.ApplyMisfireDamage);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Applied one development-only misfire-damage transition to {0}'s first equipped firearm: {1}.",
                    runtime.UnitName,
                    updated));
        }

        internal DevelopmentActionResult RepairFirstEquippedFirearmForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object item;
            RequireFirstEquippedFirearm(runtime, out item);
            FirearmItemStateSnapshot updated = _stateService.Transition(
                item,
                FirearmStateMachine.Repair);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Applied the ordinary repair transition to {0}'s first equipped firearm: {1}.",
                    runtime.UnitName,
                    updated));
        }

        internal DevelopmentActionResult OverhaulFirstEquippedWreckedFirearmForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object item;
            RequireFirstEquippedFirearm(runtime, out item);

            FirearmItemStateSnapshot before = _stateService.GetOrCreate(item);
            FirearmItemStateSnapshot after = _stateService.Transition(
                item,
                FirearmStateMachine.OverhaulWrecked);

            if (!string.Equals(
                    before.Repository.RepositoryIdentity,
                    after.Repository.RepositoryIdentity,
                    StringComparison.Ordinal) ||
                before.Repository.RuntimeReferenceHash != after.Repository.RuntimeReferenceHash)
            {
                throw new InvalidOperationException(
                    "The same-item overhaul changed repository or runtime-reference identity.");
            }

            if (after.Repository.Revision != before.Repository.Revision + 1 ||
                !after.Repository.State.IsEmpty ||
                after.Repository.State.Condition != FirearmCondition.Broken)
            {
                throw new InvalidOperationException(
                    "The same-item overhaul did not produce exactly one empty/Broken revision.");
            }

            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Applied the development-only same-item Wrecked-to-Broken overhaul to {0}'s first equipped firearm; repositoryIdentity={1}; referenceHash=0x{2:x8}; revision={3}->{4}; stateBefore=[{5}]; stateAfter=[{6}]. The item was not removed, replaced, or silently repaired to Normal.",
                    runtime.UnitName,
                    after.Repository.RepositoryIdentity,
                    after.Repository.RuntimeReferenceHash,
                    before.Repository.Revision,
                    after.Repository.Revision,
                    before.Repository.State,
                    after.Repository.State));
        }

        internal DevelopmentActionResult ResetFirstEquippedFirearmState()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            object item;
            RequireFirstEquippedFirearm(runtime, out item);
            FirearmItemStateSnapshot updated = _stateService.Set(
                item,
                FirearmState.CreateEmpty());
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Reset {0}'s first equipped firearm to empty/normal state by removing its item-owned state token: {1}.",
                    runtime.UnitName,
                    updated));
        }

        private List<PersistenceFirearmEvidenceData> CaptureStrictEvidenceFirearms(
            IEnumerable<object> candidates)
        {
            var evidence = new List<PersistenceFirearmEvidenceData>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            foreach (object candidate in candidates)
            {
                if (candidate == null || !seen.Add(candidate))
                {
                    continue;
                }

                FirearmItemStateSnapshot snapshot;
                string stateReason;
                if (!_stateService.TryGetOrCreate(candidate, out snapshot, out stateReason))
                {
                    continue;
                }

                FirearmItemId identity;
                string identityReason;
                if (!_identityProvider.TryGetIdentity(candidate, out identity, out identityReason))
                {
                    throw new InvalidOperationException(
                        "A visible exact firearm could not supply the strict engine identity required by persistence evidence: " + identityReason);
                }

                evidence.Add(new PersistenceFirearmEvidenceData
                {
                    RepositoryIdentity = snapshot.Repository.RepositoryIdentity,
                    RepositoryRevision = snapshot.Repository.Revision,
                    EngineItemId = identity.Value,
                    RuntimeType = snapshot.Repository.RuntimeTypeName,
                    ItemBlueprintId = snapshot.ItemBlueprintId,
                    WeaponTypeId = snapshot.WeaponTypeId,
                    LoadedRounds = snapshot.Repository.State.LoadedRounds,
                    LoadedAmmunitionId = snapshot.Repository.State.LoadedAmmunition == null
                        ? string.Empty
                        : snapshot.Repository.State.LoadedAmmunition.Value,
                    Condition = snapshot.Repository.State.Condition.ToString()
                });
            }

            return evidence
                .OrderBy(item => item.EngineItemId, StringComparer.Ordinal)
                .ThenBy(item => item.RepositoryIdentity, StringComparer.Ordinal)
                .ToList();
        }

        private List<FirearmItemStateSnapshot> DescribeCandidates(
            IEnumerable<object> candidates)
        {
            var snapshots = new List<FirearmItemStateSnapshot>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            foreach (object candidate in candidates)
            {
                if (candidate == null || !seen.Add(candidate))
                {
                    continue;
                }

                FirearmItemStateSnapshot snapshot;
                string reason;
                if (_stateService.TryGetOrCreate(candidate, out snapshot, out reason))
                {
                    snapshots.Add(snapshot);
                }
            }

            snapshots.Sort(
                (left, right) => left.Repository.EntryId.CompareTo(right.Repository.EntryId));
            return snapshots;
        }

        private FirearmItemStateSnapshot RequireFirstEquippedFirearm(
            RuntimeContext runtime,
            out object item)
        {
            foreach (object candidate in CollectEquippedRuntimeWeaponItems(runtime))
            {
                FirearmItemStateSnapshot snapshot;
                string reason;
                if (_stateService.TryGetOrCreate(candidate, out snapshot, out reason))
                {
                    item = candidate;
                    return snapshot;
                }
            }

            item = null;
            throw new InvalidOperationException(
                "The selected unit has no exact firearm item equipped. Equip a Test Musket and retry.");
        }

        private static FirearmStateRules CreateDebugRules(FirearmDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            return new FirearmStateRules(
                definition.Capacity,
                new[] { DebugLeadBall });
        }

        private static List<object> CollectEquippedRuntimeWeaponItems(
            RuntimeContext runtime)
        {
            var items = new List<object>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            foreach (string path in EquippedWeaponPaths)
            {
                object candidate;
                if (ReflectionAccess.TryGetPath(runtime.UnitEntity, path, out candidate))
                {
                    AddRuntimeItemCandidate(candidate, items, seen);
                }
            }

            object equipmentSets;
            if (ReflectionAccess.TryGetPath(
                runtime.UnitEntity,
                "Body.HandsEquipmentSets",
                out equipmentSets))
            {
                foreach (object set in ReflectionAccess.Enumerate(equipmentSets))
                {
                    AddRuntimeItemFromPath(set, "PrimaryHand.MaybeWeapon", items, seen);
                    AddRuntimeItemFromPath(set, "PrimaryHand.Weapon", items, seen);
                    AddRuntimeItemFromPath(set, "SecondaryHand.MaybeWeapon", items, seen);
                    AddRuntimeItemFromPath(set, "SecondaryHand.Weapon", items, seen);
                }
            }

            return items;
        }

        private static void AddRuntimeItemFromPath(
            object source,
            string path,
            ICollection<object> result,
            ISet<object> seen)
        {
            object candidate;
            if (ReflectionAccess.TryGetPath(source, path, out candidate))
            {
                AddRuntimeItemCandidate(candidate, result, seen);
            }
        }

        private static void AddRuntimeItemCandidate(
            object candidate,
            ICollection<object> result,
            ISet<object> seen)
        {
            if (candidate == null || candidate is BlueprintItemWeapon)
            {
                return;
            }

            if (seen.Add(candidate))
            {
                result.Add(candidate);
            }
        }

        private static void AddReferenceDistinct(
            IEnumerable<object> source,
            ICollection<object> result,
            ISet<object> seen)
        {
            foreach (object value in source)
            {
                if (value != null && seen.Add(value))
                {
                    result.Add(value);
                }
            }
        }

        private static int GetInventorySlotIndex(object item)
        {
            object value;
            if (item != null &&
                ReflectionAccess.TryGetMember(item, "InventorySlotIndex", out value) &&
                value is int)
            {
                return (int)value;
            }

            return int.MaxValue;
        }

        private static string FormatSnapshots(
            IEnumerable<FirearmItemStateSnapshot> snapshots)
        {
            return string.Join(
                " | ",
                snapshots.Select(snapshot => snapshot.ToString()).ToArray());
        }

        private static RuntimeContext ResolveRuntime(bool requireUnit)
        {
            Game game = Game.Instance;
            if (game == null)
            {
                throw new InvalidOperationException(
                    "No active Kingmaker Game.Instance is available. Load a disposable campaign first.");
            }

            Player player = game.Player;
            if (player == null)
            {
                throw new InvalidOperationException(
                    "No active Kingmaker player state is available. Load a disposable campaign first.");
            }

            if (!requireUnit)
            {
                return new RuntimeContext(game, player, null, null, "<not required>");
            }

            UnitEntityData unit = ResolveSelectedOrMainUnit(player);
            UnitDescriptor descriptor = unit.Descriptor;
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "The selected Kingmaker unit has no UnitDescriptor.");
            }

            return new RuntimeContext(
                game,
                player,
                unit,
                descriptor,
                ResolveUnitName(unit, descriptor));
        }

        private static UnitEntityData ResolveSelectedOrMainUnit(Player player)
        {
            SelectionManager selection = SelectionManager.Instance;
            UnitEntityData selected = selection == null
                ? null
                : selection.GetSingleSelectedUnit();
            if (selected == null && selection != null)
            {
                selected = selection.FirstSelectUnit;
            }

            if (selected != null)
            {
                return selected;
            }

            UnitEntityData mainCharacter = player.MainCharacter.Value;
            if (mainCharacter != null)
            {
                return mainCharacter;
            }

            throw new InvalidOperationException(
                "No selected unit or main-character entity could be resolved. Select exactly one party member in a loaded disposable campaign.");
        }

        private static bool CanResolveDescriptor(object unit)
        {
            if (unit == null)
            {
                return false;
            }

            if (string.Equals(
                unit.GetType().FullName,
                "Kingmaker.UnitLogic.UnitDescriptor",
                StringComparison.Ordinal))
            {
                return true;
            }

            object descriptor;
            string member;
            return ReflectionAccess.TryGetFirstNonNullMember(
                unit,
                DescriptorMembers,
                out descriptor,
                out member) &&
                descriptor != null;
        }

        private static object ResolveDescriptor(object unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            if (string.Equals(
                unit.GetType().FullName,
                "Kingmaker.UnitLogic.UnitDescriptor",
                StringComparison.Ordinal))
            {
                return unit;
            }

            object descriptor;
            string member;
            if (!ReflectionAccess.TryGetFirstNonNullMember(
                unit,
                DescriptorMembers,
                out descriptor,
                out member) ||
                descriptor == null)
            {
                throw new MissingMemberException(
                    "The selected Kingmaker unit does not expose a UnitDescriptor.");
            }

            return descriptor;
        }

        private static object RequireInventory(object player)
        {
            object inventory;
            string member;
            if (!ReflectionAccess.TryGetFirstNonNullMember(
                player,
                InventoryMembers,
                out inventory,
                out member) ||
                inventory == null)
            {
                throw new MissingMemberException(
                    "The active Kingmaker player does not expose a shared inventory.");
            }

            return inventory;
        }

        private Ability EnsureReloadAbility(UnitDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            if (descriptor.Abilities == null)
            {
                throw new InvalidOperationException(
                    "The selected unit has no AbilityCollection.");
            }

            Ability ability = descriptor.Abilities.GetAbility(_reloadAbility);
            if (ability == null)
            {
                descriptor.Abilities.AddFact(_reloadAbility, null);
                ability = descriptor.Abilities.GetAbility(_reloadAbility);
            }

            if (ability == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker did not retain the Reload Test Musket ability after the grant.");
            }

            return ability;
        }

        private Ability EnsureOverhaulAbility(UnitDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            if (descriptor.Abilities == null)
            {
                throw new InvalidOperationException(
                    "The selected unit has no AbilityCollection.");
            }

            Ability ability = descriptor.Abilities.GetAbility(_overhaulAbility);
            if (ability == null)
            {
                descriptor.Abilities.AddFact(_overhaulAbility, null);
                ability = descriptor.Abilities.GetAbility(_overhaulAbility);
            }

            if (ability == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker did not retain the Overhaul Test Musket ability after the grant.");
            }

            return ability;
        }

        private static bool HasFeature(object descriptor, BlueprintFeature feature)
        {
            UnitDescriptor unitDescriptor = descriptor as UnitDescriptor;
            if (unitDescriptor == null)
            {
                throw new InvalidOperationException(
                    "A concrete Kingmaker UnitDescriptor is required for feature checks.");
            }

            return unitDescriptor.Progression.Features.GetRank(feature) > 0;
        }

        private static bool FactUsesBlueprint(object fact, BlueprintFeature feature)
        {
            if (ReferenceEquals(fact, feature))
            {
                return true;
            }

            object blueprint;
            string member;
            return ReflectionAccess.TryGetFirstNonNullMember(
                fact,
                ItemBlueprintMembers,
                out blueprint,
                out member) &&
                ReferenceEquals(blueprint, feature);
        }

        private static IEnumerable<object> EnumerateInventoryItems(object inventory)
        {
            if (ReflectionAccess.CanEnumerate(inventory))
            {
                return ReflectionAccess.Enumerate(inventory).ToList();
            }

            object items;
            string member;
            if (ReflectionAccess.TryGetFirstNonNullMember(
                inventory,
                InventoryItemsMembers,
                out items,
                out member) &&
                ReflectionAccess.CanEnumerate(items))
            {
                return ReflectionAccess.Enumerate(items).ToList();
            }

            throw new MissingMemberException(
                "The resolved Kingmaker inventory does not expose an enumerable item collection.");
        }

        private static int CountMatchingInventoryItems(
            object inventory,
            BlueprintItemWeapon blueprint)
        {
            return EnumerateInventoryItems(inventory)
                .Count(item => ItemUsesBlueprint(item, blueprint));
        }

        private static bool ItemUsesBlueprint(
            object item,
            BlueprintItemWeapon blueprint)
        {
            if (ReferenceEquals(item, blueprint))
            {
                return true;
            }

            object itemBlueprint;
            string member;
            return ReflectionAccess.TryGetFirstNonNullMember(
                item,
                ItemBlueprintMembers,
                out itemBlueprint,
                out member) &&
                ReferenceEquals(itemBlueprint, blueprint);
        }

        private static bool ResultLooksLikeCreatedItem(
            object result,
            BlueprintItemWeapon blueprint)
        {
            if (result == null)
            {
                return false;
            }

            if (ItemUsesBlueprint(result, blueprint))
            {
                return true;
            }

            return ReflectionAccess.Enumerate(result)
                .Any(item => ItemUsesBlueprint(item, blueprint));
        }

        private static string ResolveUnitName(object unit, object descriptor)
        {
            object value;
            string member;
            string[] names = { "CharacterName", "Name", "name" };
            if (ReflectionAccess.TryGetFirstNonNullMember(
                unit,
                names,
                out value,
                out member) &&
                value != null &&
                !string.IsNullOrWhiteSpace(value.ToString()))
            {
                return value.ToString();
            }

            if (ReflectionAccess.TryGetFirstNonNullMember(
                descriptor,
                names,
                out value,
                out member) &&
                value != null &&
                !string.IsNullOrWhiteSpace(value.ToString()))
            {
                return value.ToString();
            }

            return unit.GetType().Name;
        }

        private sealed class RuntimeContext
        {
            internal RuntimeContext(
                object game,
                object player,
                object unitEntity,
                object unitDescriptor,
                string unitName)
            {
                Game = game ?? throw new ArgumentNullException("game");
                Player = player ?? throw new ArgumentNullException("player");
                UnitEntity = unitEntity;
                UnitDescriptor = unitDescriptor;
                UnitName = unitName ?? throw new ArgumentNullException("unitName");
            }

            internal object Game { get; private set; }

            internal object Player { get; private set; }

            internal object UnitEntity { get; private set; }

            internal object UnitDescriptor { get; private set; }

            internal string UnitName { get; private set; }
        }
    }

    internal sealed class DevelopmentActionResult
    {
        private DevelopmentActionResult(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Message = message ?? throw new ArgumentNullException("message");
        }

        internal bool Succeeded { get; private set; }

        internal string Message { get; private set; }

        internal static DevelopmentActionResult Success(string message)
        {
            return new DevelopmentActionResult(true, message);
        }

        internal static DevelopmentActionResult Failure(string message)
        {
            return new DevelopmentActionResult(false, message);
        }
    }
}
