using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class GunslingerLevelTransitionSnapshot
    {
        internal GunslingerLevelTransitionSnapshot(UnitDescriptor descriptor,
            int priorLevel)
        {
            Descriptor = descriptor;
            PriorLevel = priorLevel;
        }

        internal UnitDescriptor Descriptor { get; private set; }
        internal int PriorLevel { get; private set; }
    }

    internal sealed class NativeStartingFirearmObservation
    {
        internal NativeStartingFirearmObservation(bool suppressNative,
            StartingFirearmInventorySnapshot inventorySnapshot,
            bool committedCharacterCreation)
        {
            SuppressNative = suppressNative;
            InventorySnapshot = inventorySnapshot;
            CommittedCharacterCreation = committedCharacterCreation;
        }

        internal bool SuppressNative { get; private set; }
        internal StartingFirearmInventorySnapshot InventorySnapshot
        { get; private set; }
        internal bool CommittedCharacterCreation { get; private set; }
    }

    internal sealed class StartingFirearmInventorySnapshot
    {
        internal StartingFirearmInventorySnapshot(UnitDescriptor receiver,
            ExpectedStartingFirearm expected, ItemsCollection inventory,
            HashSet<object> references, int firearmCount, int powderCount,
            int ballCount, int gunsmithKitCount)
        {
            Receiver = receiver;
            Expected = expected;
            Inventory = inventory;
            References = references;
            FirearmCount = firearmCount;
            PowderCount = powderCount;
            BallCount = ballCount;
            GunsmithKitCount = gunsmithKitCount;
        }

        internal UnitDescriptor Receiver { get; private set; }
        internal ExpectedStartingFirearm Expected { get; private set; }
        internal ItemsCollection Inventory { get; private set; }
        internal HashSet<object> References { get; private set; }
        internal int FirearmCount { get; private set; }
        internal int PowderCount { get; private set; }
        internal int BallCount { get; private set; }
        internal int GunsmithKitCount { get; private set; }
    }

    internal static class GunslingerStartingFirearmGrantTransaction
    {
        internal const int StartingAmmunitionCount = 20;
        internal const int StartingGunsmithKitCount = 1;

        private static readonly KingmakerBatteredFirearmOwnershipPartProvider
            OwnershipParts = new KingmakerBatteredFirearmOwnershipPartProvider();
        private static long _grants;
        private static long _receiptReconciliations;
        private static long _rollbacks;
        [ThreadStatic]
        private static GunslingerLevelTransitionSnapshot _activeFirstLevelCommit;

        internal static long Grants { get { return Interlocked.Read(ref _grants); } }
        internal static long ReceiptReconciliations
        { get { return Interlocked.Read(ref _receiptReconciliations); } }
        internal static long Rollbacks
        { get { return Interlocked.Read(ref _rollbacks); } }

        internal static void BeginCommit(
            GunslingerLevelTransitionSnapshot transition,
            bool committedCharacterCreation)
        {
            if (transition == null || !committedCharacterCreation) return;
            if (_activeFirstLevelCommit != null)
                throw new InvalidOperationException(
                    "A nested first-level character commit is not supported.");
            _activeFirstLevelCommit = transition;
        }

        internal static void EndCommit(
            GunslingerLevelTransitionSnapshot transition)
        {
            if (ReferenceEquals(_activeFirstLevelCommit, transition))
                _activeFirstLevelCommit = null;
        }

        internal static GunslingerLevelTransitionSnapshot CaptureTransition(
            UnitDescriptor descriptor)
        {
            if (!IsModuleEnabled() || descriptor == null ||
                descriptor.Progression == null ||
                BlueprintBootstrap.GunslingerClass == null) return null;
            return new GunslingerLevelTransitionSnapshot(descriptor,
                ExactGunslingerLevel(descriptor));
        }

        internal static bool CompleteTransition(
            GunslingerLevelTransitionSnapshot transition,
            UnitDescriptor finalizedDescriptor)
        {
            if (transition == null || !IsModuleEnabled()) return false;
            if (!ReferenceEquals(transition.Descriptor, finalizedDescriptor))
                throw new InvalidOperationException(
                    "The committed level-up descriptor changed across the exact Commit boundary.");
            RequireStableReceiver(finalizedDescriptor);
            int currentLevel = ExactGunslingerLevel(finalizedDescriptor);
            bool playerControlled = IsPlayerControlled(
                finalizedDescriptor.Unit);
            if (transition.PriorLevel != 0 || currentLevel != 1 ||
                !playerControlled) return false;
            OriginatingUnitId ownerId = OwnerId(finalizedDescriptor);
            UnitPartBatteredFirearmOwnership existingPart;
            bool hasReceipt = OwnershipParts.TryGetExisting(
                    finalizedDescriptor.Unit, out existingPart) &&
                existingPart.HasReceipt(ownerId);
            ItemEntityWeapon[] bound = FindOwnerBoundProductionFirearms(
                RequireInventory(), finalizedDescriptor.Unit);
            StartingFirearmGrantDecision decision = StartingFirearmGrantPolicy.Decide(
                true, playerControlled,
                transition.PriorLevel, currentLevel, hasReceipt, bound.Length);
            if (decision.Disposition == StartingFirearmGrantDisposition.None)
                return false;
            if (decision.Disposition ==
                    StartingFirearmGrantDisposition.ReconcileReceipt)
            {
                ReconcileReceipt(bound[0], finalizedDescriptor.Unit);
                Interlocked.Increment(ref _receiptReconciliations);
                Log("starter.receipt-reconciled", finalizedDescriptor,
                    bound[0].Blueprint, decision.Status);
                return false;
            }

            ExpectedStartingFirearm expected =
                GunslingerStartingFirearmResolver.Resolve(finalizedDescriptor);
            StartingFirearmInventorySnapshot snapshot = Snapshot(
                finalizedDescriptor, expected);
            CompleteGrant(snapshot, true);
            Interlocked.Increment(ref _grants);
            Log("starter.granted", finalizedDescriptor, expected.Item,
                decision.Status);
            return true;
        }

        internal static bool RemoveReceiptForRuntimeTest(
            ItemEntityWeapon firearm, UnitEntityData owner)
        {
            if (firearm == null || owner == null) return false;
            UnitEntityData boundOwner;
            if (!BatteredFirearmOriginRuntime.TryGetOwner(firearm,
                    out boundOwner) || !BatteredFirearmOriginRuntime
                    .SameStableOwner(boundOwner, owner))
                throw new InvalidOperationException(
                    "Runtime cleanup requires the exact owner-bound starter firearm.");
            UnitPartBatteredFirearmOwnership part;
            if (!OwnershipParts.TryGetExisting(owner, out part)) return false;
            bool removed = part.RemoveStarterReceipt(
                new OriginatingUnitId(owner.UniqueId));
            if (removed && part.Count == 0)
                OwnershipParts.RemoveIfEmpty(part, owner);
            return removed;
        }

        internal static NativeStartingFirearmObservation BeginNativeGrant(
            UnitDescriptor receiver)
        {
            if (!IsModuleEnabled() || receiver == null ||
                receiver.Progression == null ||
                BlueprintBootstrap.GunslingerClass == null ||
                ExactGunslingerLevel(receiver) != 1) return null;
            RequireStableReceiver(receiver);
            ExpectedStartingFirearm expected =
                GunslingerStartingFirearmResolver.Resolve(receiver);
            ItemsCollection inventory = RequireInventory();
            OriginatingUnitId ownerId = OwnerId(receiver);
            UnitPartBatteredFirearmOwnership existingPart;
            bool committedCharacterCreation = IsCommittedCharacterCreation(
                receiver);
            if (OwnershipParts.TryGetExisting(receiver.Unit,
                    out existingPart) &&
                existingPart.HasReceipt(ownerId))
                return new NativeStartingFirearmObservation(true, null,
                    committedCharacterCreation);

            ItemEntityWeapon[] bound = FindOwnerBoundProductionFirearms(
                inventory, receiver.Unit);
            if (bound.Length > 1)
                throw new InvalidOperationException(
                    "The native starter receiver has multiple owner-bound production firearms and no receipt.");
            if (bound.Length == 1)
            {
                ReconcileReceipt(bound[0], receiver.Unit);
                Interlocked.Increment(ref _receiptReconciliations);
                return new NativeStartingFirearmObservation(true, null,
                    committedCharacterCreation);
            }
            return new NativeStartingFirearmObservation(false,
                Snapshot(receiver, expected), committedCharacterCreation);
        }

        internal static bool CompleteNativeGrant(
            NativeStartingFirearmObservation observation)
        {
            if (observation == null || observation.SuppressNative ||
                observation.InventorySnapshot == null || !IsModuleEnabled())
                return false;
            StartingFirearmInventorySnapshot snapshot =
                observation.InventorySnapshot;
            ExpectedStartingFirearm finalized =
                GunslingerStartingFirearmResolver.Resolve(snapshot.Receiver);
            if (!ReferenceEquals(finalized.Item, snapshot.Expected.Item))
            {
                Exception rollbackFailure = RollbackInventory(snapshot, null,
                    false);
                if (rollbackFailure != null)
                    throw new InvalidOperationException(
                        "The finalized Gunslinger archetype changed and the exact native starter rollback failed.",
                        rollbackFailure);
                throw new InvalidOperationException(
                    "The finalized Gunslinger archetype changed during the native starting-item transaction.");
            }
            ItemEntityWeapon[] added = NewProductionFirearms(snapshot);
            // Detached CharGen evaluation may invoke the native method without
            // touching the shared inventory. Observing that absence is not a grant.
            if (added.Length == 0)
            {
                if (!HasInventoryMutation(snapshot))
                {
                    if (!observation.CommittedCharacterCreation) return false;
                    CompleteGrant(snapshot, true);
                    Interlocked.Increment(ref _grants);
                    Log("starter.committed-creation-fallback", snapshot.Receiver,
                        snapshot.Expected.Item, snapshot.Expected.Source);
                    return true;
                }
                Exception rollbackFailure = RollbackInventory(snapshot, null,
                    false);
                if (rollbackFailure != null)
                    throw new InvalidOperationException(
                        "The native starting-item flow partially mutated supplies without a firearm and rollback failed.",
                        rollbackFailure);
                throw new InvalidOperationException(
                    "The native starting-item flow partially mutated supplies without creating its firearm.");
            }
            CompleteGrant(snapshot, false);
            Interlocked.Increment(ref _grants);
            Log("starter.native-grant-completed", snapshot.Receiver,
                snapshot.Expected.Item, snapshot.Expected.Source);
            return true;
        }

        private static void CompleteGrant(StartingFirearmInventorySnapshot snapshot,
            bool createFirearm)
        {
            ItemEntityWeapon firearm = null;
            UnitPartBatteredFirearmOwnership part = null;
            bool partExisted = false;
            bool receiptAdded = false;
            try
            {
                ItemEntityWeapon[] current = NewProductionFirearms(snapshot);
                if (createFirearm)
                {
                    if (current.Length != 0)
                        throw new InvalidOperationException(
                            "The direct starter transaction began with an unexpected new firearm.");
                    firearm = snapshot.Inventory.Add(snapshot.Expected.Item) as
                        ItemEntityWeapon;
                    if (firearm == null)
                        throw new InvalidOperationException(
                            "Kingmaker did not create the expected firearm item entity.");
                }
                else
                {
                    if (current.Length != 1 || !ReferenceEquals(
                            current[0].Blueprint, snapshot.Expected.Item))
                        throw new InvalidOperationException(
                            "The native grant did not add exactly one expected production firearm.");
                    firearm = current[0];
                }

                AddToTarget(snapshot.Inventory,
                    BlueprintBootstrap.BasicAmmunition.BlackPowder,
                    snapshot.PowderCount, StartingAmmunitionCount,
                    "Black Powder");
                AddToTarget(snapshot.Inventory,
                    BlueprintBootstrap.BasicAmmunition.LeadBall,
                    snapshot.BallCount, StartingAmmunitionCount,
                    "Lead Ball");
                AddToTarget(snapshot.Inventory,
                    BlueprintBootstrap.GunsmithingSupplies.GunsmithKit,
                    snapshot.GunsmithKitCount, StartingGunsmithKitCount,
                    "Gunsmith's Kit");

                if (FirearmRuntimeState.ReadStateTokenIds(firearm).Count != 0)
                    throw new InvalidOperationException(
                        "A newly granted starter firearm did not begin in ordinary empty Normal state.");
                BatteredFirearmOriginRuntime.Bind(firearm,
                    snapshot.Receiver.Unit);
                partExisted = OwnershipParts.TryGetExisting(
                    snapshot.Receiver.Unit, out part);
                if (!partExisted) part = OwnershipParts.RequireForWrite(
                    snapshot.Receiver.Unit);
                receiptAdded = part.AddStarterReceipt(
                    OwnerId(snapshot.Receiver));
                Validate(snapshot, firearm, part);
            }
            catch (Exception exception)
            {
                Exception rollbackFailure = RollbackInventory(snapshot, part,
                    receiptAdded);
                Interlocked.Increment(ref _rollbacks);
                if (!partExisted && part != null && part.Count == 0)
                {
                    try { OwnershipParts.RemoveIfEmpty(part,
                        snapshot.Receiver.Unit); }
                    catch (Exception removeFailure)
                    {
                        rollbackFailure = rollbackFailure == null ? removeFailure :
                            new AggregateException(rollbackFailure, removeFailure);
                    }
                }
                if (rollbackFailure != null)
                    throw new InvalidOperationException(
                        "The starter-firearm transaction failed and its exact rollback also failed.",
                        new AggregateException(exception, rollbackFailure));
                throw;
            }
        }

        private static void Validate(StartingFirearmInventorySnapshot snapshot,
            ItemEntityWeapon firearm, UnitPartBatteredFirearmOwnership part)
        {
            ItemEntityWeapon[] added = NewProductionFirearms(snapshot);
            if (added.Length != 1 || !ReferenceEquals(added[0], firearm) ||
                !ReferenceEquals(firearm.Blueprint, snapshot.Expected.Item) ||
                snapshot.Inventory.Count(snapshot.Expected.Item) -
                    snapshot.FirearmCount != 1)
                throw new InvalidOperationException(
                    "The starter transaction did not retain exactly one expected firearm delta.");
            RequireDelta(snapshot.Inventory,
                BlueprintBootstrap.BasicAmmunition.BlackPowder,
                snapshot.PowderCount, StartingAmmunitionCount, "Black Powder");
            RequireDelta(snapshot.Inventory,
                BlueprintBootstrap.BasicAmmunition.LeadBall,
                snapshot.BallCount, StartingAmmunitionCount, "Lead Ball");
            RequireDelta(snapshot.Inventory,
                BlueprintBootstrap.GunsmithingSupplies.GunsmithKit,
                snapshot.GunsmithKitCount, StartingGunsmithKitCount,
                "Gunsmith's Kit");
            UnitEntityData owner;
            if (!BatteredFirearmOriginRuntime.TryGetOwner(firearm, out owner) ||
                !BatteredFirearmOriginRuntime.SameStableOwner(owner,
                    snapshot.Receiver.Unit))
                throw new InvalidOperationException(
                    "The starter firearm did not retain its exact stable owner binding.");
            if (!part.HasReceipt(OwnerId(snapshot.Receiver)))
                throw new InvalidOperationException(
                    "The starter firearm did not retain its durable per-unit receipt.");
            if (FirearmRuntimeState.ReadStateTokenIds(firearm).Count != 0)
                throw new InvalidOperationException(
                    "The starter firearm acquired a non-default condition token.");
        }

        private static Exception RollbackInventory(
            StartingFirearmInventorySnapshot snapshot,
            UnitPartBatteredFirearmOwnership part, bool receiptAdded)
        {
            var failures = new List<Exception>();
            if (receiptAdded && part != null)
            {
                try { part.RemoveStarterReceipt(OwnerId(snapshot.Receiver)); }
                catch (Exception exception) { failures.Add(exception); }
            }
            foreach (ItemEntityWeapon item in NewProductionFirearms(snapshot))
            {
                try
                {
                    snapshot.Inventory.Remove(item);
                    item.Dispose();
                }
                catch (Exception exception) { failures.Add(exception); }
            }
            RollbackCount(snapshot.Inventory,
                BlueprintBootstrap.BasicAmmunition.BlackPowder,
                snapshot.PowderCount, failures);
            RollbackCount(snapshot.Inventory,
                BlueprintBootstrap.BasicAmmunition.LeadBall,
                snapshot.BallCount, failures);
            RollbackCount(snapshot.Inventory,
                BlueprintBootstrap.GunsmithingSupplies.GunsmithKit,
                snapshot.GunsmithKitCount, failures);
            return failures.Count == 0 ? null : new AggregateException(failures);
        }

        private static void ReconcileReceipt(ItemEntityWeapon firearm,
            UnitEntityData owner)
        {
            UnitEntityData boundOwner;
            if (!BatteredFirearmOriginRuntime.TryGetOwner(firearm,
                    out boundOwner) || !BatteredFirearmOriginRuntime
                    .SameStableOwner(boundOwner, owner))
                throw new InvalidOperationException(
                    "Starter receipt reconciliation requires the exact owner-bound firearm.");
            UnitPartBatteredFirearmOwnership part;
            bool existed = OwnershipParts.TryGetExisting(owner, out part);
            if (!existed) part = OwnershipParts.RequireForWrite(owner);
            bool added = false;
            var ownerId = new OriginatingUnitId(owner.UniqueId);
            try
            {
                added = part.AddStarterReceipt(ownerId);
                if (!part.HasReceipt(ownerId))
                    throw new InvalidOperationException(
                        "The reconciled starter receipt was not readable.");
            }
            catch
            {
                if (added)
                    part.RemoveStarterReceipt(ownerId);
                if (!existed && part.Count == 0)
                    OwnershipParts.RemoveIfEmpty(part, owner);
                throw;
            }
        }

        private static StartingFirearmInventorySnapshot Snapshot(
            UnitDescriptor receiver, ExpectedStartingFirearm expected)
        {
            ItemsCollection inventory = RequireInventory();
            RequireCatalogs();
            return new StartingFirearmInventorySnapshot(receiver, expected,
                inventory, Enumerate(inventory), inventory.Count(expected.Item),
                inventory.Count(BlueprintBootstrap.BasicAmmunition.BlackPowder),
                inventory.Count(BlueprintBootstrap.BasicAmmunition.LeadBall),
                inventory.Count(
                    BlueprintBootstrap.GunsmithingSupplies.GunsmithKit));
        }

        private static void AddToTarget(ItemsCollection inventory,
            BlueprintItem item, int before, int targetDelta, string label)
        {
            int currentDelta = inventory.Count(item) - before;
            if (currentDelta < 0 || currentDelta > targetDelta)
                throw new InvalidOperationException(label +
                    " changed outside the starter transaction.");
            if (currentDelta < targetDelta)
                inventory.Add(item, targetDelta - currentDelta);
        }

        private static void RequireDelta(ItemsCollection inventory,
            BlueprintItem item, int before, int expected, string label)
        {
            if (inventory.Count(item) - before != expected)
                throw new InvalidOperationException(label +
                    " did not retain its exact starter quantity.");
        }

        private static void RollbackCount(ItemsCollection inventory,
            BlueprintItem item, int before, IList<Exception> failures)
        {
            try
            {
                int delta = inventory.Count(item) - before;
                if (delta > 0) inventory.Remove(item, delta);
                if (inventory.Count(item) != before)
                    throw new InvalidOperationException(
                        "A starter supply count did not roll back exactly.");
            }
            catch (Exception exception) { failures.Add(exception); }
        }

        private static ItemEntityWeapon[] NewProductionFirearms(
            StartingFirearmInventorySnapshot snapshot)
        {
            return Enumerate(snapshot.Inventory)
                .Where(item => !snapshot.References.Contains(item))
                .OfType<ItemEntityWeapon>()
                .Where(item => IsProductionFirearm(item.Blueprint)).ToArray();
        }

        private static bool HasInventoryMutation(
            StartingFirearmInventorySnapshot snapshot)
        {
            return snapshot.Inventory.Count(snapshot.Expected.Item) !=
                    snapshot.FirearmCount ||
                snapshot.Inventory.Count(
                    BlueprintBootstrap.BasicAmmunition.BlackPowder) !=
                    snapshot.PowderCount ||
                snapshot.Inventory.Count(
                    BlueprintBootstrap.BasicAmmunition.LeadBall) !=
                    snapshot.BallCount ||
                snapshot.Inventory.Count(
                    BlueprintBootstrap.GunsmithingSupplies.GunsmithKit) !=
                    snapshot.GunsmithKitCount;
        }

        private static ItemEntityWeapon[] FindOwnerBoundProductionFirearms(
            ItemsCollection inventory, UnitEntityData owner)
        {
            return Enumerate(inventory).OfType<ItemEntityWeapon>()
                .Where(item => IsProductionFirearm(item.Blueprint))
                .Where(item =>
                {
                    UnitEntityData origin;
                    return BatteredFirearmOriginRuntime.TryGetOwner(item,
                        out origin) && BatteredFirearmOriginRuntime
                            .SameStableOwner(origin, owner);
                }).ToArray();
        }

        private static bool IsProductionFirearm(BlueprintItemWeapon item)
        {
            return item != null && BlueprintBootstrap.ProductionFirearms != null &&
                BlueprintBootstrap.ProductionFirearms.Entries.Any(value =>
                    value != null && ReferenceEquals(value.Item, item));
        }

        private static bool IsPlayerControlled(UnitEntityData unit)
        {
            if (unit == null || Game.Instance == null ||
                Game.Instance.Player == null) return false;
            Player player = Game.Instance.Player;
            if (player.MainCharacter != null &&
                ReferenceEquals(player.MainCharacter.Value, unit)) return true;
            return player.AllCharacters != null &&
                player.AllCharacters.Any(value => ReferenceEquals(value, unit));
        }

        private static bool IsCommittedCharacterCreation(
            UnitDescriptor receiver)
        {
            GunslingerLevelTransitionSnapshot commit =
                _activeFirstLevelCommit;
            return commit != null && commit.PriorLevel == 0 &&
                ReferenceEquals(commit.Descriptor, receiver) &&
                receiver != null && receiver.Unit != null &&
                receiver.Unit.IsPlayerFaction &&
                ExactGunslingerLevel(receiver) == 1;
        }

        private static int ExactGunslingerLevel(UnitDescriptor descriptor)
        {
            return descriptor.Progression.GetClassLevel(
                BlueprintBootstrap.GunslingerClass.CharacterClass);
        }

        private static OriginatingUnitId OwnerId(UnitDescriptor descriptor)
        {
            RequireStableReceiver(descriptor);
            return new OriginatingUnitId(descriptor.Unit.UniqueId);
        }

        private static void RequireStableReceiver(UnitDescriptor receiver)
        {
            if (receiver == null || receiver.Unit == null ||
                string.IsNullOrWhiteSpace(receiver.Unit.UniqueId))
                throw new InvalidOperationException(
                    "A starter-firearm receiver exposes no exact stable unit identity.");
        }

        private static ItemsCollection RequireInventory()
        {
            if (Game.Instance == null || Game.Instance.Player == null ||
                Game.Instance.Player.Inventory == null)
                throw new InvalidOperationException(
                    "The exact shared player inventory is unavailable.");
            return Game.Instance.Player.Inventory;
        }

        private static void RequireCatalogs()
        {
            if (BlueprintBootstrap.BasicAmmunition == null ||
                BlueprintBootstrap.GunsmithingSupplies == null ||
                BlueprintBootstrap.ProductionFirearms == null)
                throw new InvalidOperationException(
                    "The production firearm starter catalogs are unavailable.");
        }

        private static bool IsModuleEnabled()
        {
            ModContext context;
            return ModContext.TryGet(out context) &&
                context.FeatureModules != null &&
                context.FeatureModules.Active.Gunslinger;
        }

        private static HashSet<object> Enumerate(ItemsCollection inventory)
        {
            if (!ReflectionAccess.CanEnumerate(inventory))
                throw new MissingMemberException(
                    "The exact shared inventory is not enumerable.");
            return new HashSet<object>(ReflectionAccess.Enumerate(inventory),
                ReferenceIdentityComparer.Instance);
        }

        private static void Log(string eventName, UnitDescriptor receiver,
            BlueprintItemWeapon item, string status)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.Logger.Info("gunsmithing", eventName, string.Format(
                CultureInfo.InvariantCulture,
                "owner={0};firearm={1}:{2};status={3};powder={4};ball={5};kit={6}",
                receiver.Unit.UniqueId, item.name, item.AssetGuid, status,
                StartingAmmunitionCount, StartingAmmunitionCount,
                StartingGunsmithKitCount));
        }
    }
}
