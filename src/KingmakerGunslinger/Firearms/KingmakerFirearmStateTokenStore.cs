using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Development;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Reflection-contained adapter over Kingmaker's runtime item-enchantment API.
    /// State-token replacement is add-first, verify, remove-old, verify; failures attempt
    /// to restore the previously observed token set before escaping.
    /// </summary>
    internal sealed class KingmakerFirearmStateTokenStore : IFirearmStateTokenStore
    {
        private static readonly string[] EnchantmentCollectionMembers =
        {
            "Enchantments",
            "m_Enchantments",
            "EnchantmentFacts",
            "m_EnchantmentFacts"
        };

        private static readonly string[] EnchantmentBlueprintMembers =
        {
            "Blueprint",
            "m_Blueprint"
        };

        private static readonly string[] RemoveMethodNames =
        {
            "RemoveEnchantment"
        };

        private readonly FirearmStateTokenBlueprintSet _blueprints;

        internal KingmakerFirearmStateTokenStore(
            FirearmStateTokenBlueprintSet blueprints)
        {
            _blueprints = blueprints ?? throw new ArgumentNullException("blueprints");
        }

        public IReadOnlyList<string> ReadTokenIds(object itemInstance)
        {
            return ReadRecords(itemInstance)
                .Select(record => record.Definition.TokenId)
                .OrderBy(tokenId => tokenId, StringComparer.Ordinal)
                .ToArray();
        }

        public void ReplaceToken(
            object itemInstance,
            string expectedCurrentTokenId,
            string targetTokenId)
        {
            RequireKnownOrNull(expectedCurrentTokenId, "expectedCurrentTokenId");
            RequireKnownOrNull(targetTokenId, "targetTokenId");
            List<RuntimeTokenRecord> before = ReadRecords(itemInstance);
            RequireExpected(before, expectedCurrentTokenId);
            if (string.Equals(
                expectedCurrentTokenId,
                targetTokenId,
                StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                if (targetTokenId != null)
                {
                    BlueprintWeaponEnchantment targetBlueprint =
                        _blueprints.RequireBlueprint(targetTokenId);
                    AddEnchantment(itemInstance, targetBlueprint);
                    RequireSingleRecord(itemInstance, targetTokenId);
                }

                foreach (RuntimeTokenRecord record in before)
                {
                    RemoveEnchantment(itemInstance, record.Instance);
                }

                RequireExpected(ReadRecords(itemInstance), targetTokenId);
            }
            catch
            {
                RestorePreviousBestEffort(
                    itemInstance,
                    before,
                    expectedCurrentTokenId);
                throw;
            }
        }

        public bool ClearTokens(object itemInstance)
        {
            List<RuntimeTokenRecord> before = ReadRecords(itemInstance);
            if (before.Count == 0)
            {
                return false;
            }

            if (before.Count != 1)
            {
                throw new InvalidOperationException(
                    "A corrupt item with multiple firearm-state tokens cannot be cleared implicitly.");
            }

            try
            {
                RemoveEnchantment(itemInstance, before[0].Instance);
                if (ReadRecords(itemInstance).Count != 0)
                {
                    throw new InvalidOperationException(
                        "One or more firearm-state token enchantments remained after clear.");
                }

                return true;
            }
            catch
            {
                RestorePreviousBestEffort(
                    itemInstance,
                    before,
                    before[0].Definition.TokenId);
                throw;
            }
        }

        private List<RuntimeTokenRecord> ReadRecords(object itemInstance)
        {
            RequireReferenceItem(itemInstance);
            object collection;
            string resolvedMember;
            if (!ReflectionAccess.TryGetFirstNonNullMember(
                itemInstance,
                EnchantmentCollectionMembers,
                out collection,
                out resolvedMember) ||
                !ReflectionAccess.CanEnumerate(collection))
            {
                throw new MissingMemberException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Runtime item type '{0}' exposes no readable enumerable enchantment collection.",
                        itemInstance.GetType().FullName));
            }

            var records = new List<RuntimeTokenRecord>();
            foreach (object enchantment in ReflectionAccess.Enumerate(collection))
            {
                object blueprintObject;
                string blueprintMember;
                if (!ReflectionAccess.TryGetFirstNonNullMember(
                    enchantment,
                    EnchantmentBlueprintMembers,
                    out blueprintObject,
                    out blueprintMember))
                {
                    continue;
                }

                BlueprintWeaponEnchantment blueprint =
                    blueprintObject as BlueprintWeaponEnchantment;
                if (blueprint == null)
                {
                    continue;
                }

                FirearmStateTokenComponent[] markers =
                    (blueprint.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<FirearmStateTokenComponent>()
                    .ToArray();
                if (markers.Length == 0)
                {
                    continue;
                }

                if (markers.Length != 1)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Weapon enchantment '{0}' contains {1} firearm-state token markers; exactly one is valid.",
                            blueprint.name,
                            markers.Length));
                }

                FirearmStateTokenDefinition definition = markers[0].Definition;
                if (!_blueprints.Catalog.ContainsToken(definition.TokenId))
                {
                    throw new NotSupportedException(
                        "An item contains a future or foreign firearm-state token '" +
                        definition.TokenId + "'.");
                }

                BlueprintWeaponEnchantment expectedBlueprint =
                    _blueprints.RequireBlueprint(definition.TokenId);
                if (!ReferenceEquals(blueprint, expectedBlueprint))
                {
                    throw new InvalidOperationException(
                        "A firearm-state token ID was carried by an unexpected blueprint instance.");
                }

                if (!definition.Equals(
                    _blueprints.Catalog.RequireDefinition(definition.TokenId)))
                {
                    throw new InvalidOperationException(
                        "A firearm-state token blueprint payload disagrees with the canonical catalog.");
                }

                records.Add(
                    new RuntimeTokenRecord(
                        enchantment,
                        blueprint,
                        definition,
                        resolvedMember,
                        blueprintMember));
            }

            return records;
        }

        private void AddEnchantment(
            object itemInstance,
            BlueprintWeaponEnchantment blueprint)
        {
            ItemEntity item = itemInstance as ItemEntity;
            if (item == null)
            {
                throw new ArgumentException(
                    "A firearm-state token can only be attached to a Kingmaker ItemEntity.",
                    "itemInstance");
            }

            MechanicsContext parentContext = CreateParentContext(item, blueprint);
            ItemEnchantment added = item.AddEnchantment(
                blueprint,
                parentContext,
                null);
            if (added == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker returned no runtime enchantment after adding a firearm-state token.");
            }

            if (parentContext != null && added.ParentContext == null)
            {
                throw new InvalidOperationException(
                    "A firearm-state token was added with a parent context, but Kingmaker did not retain it.");
            }
        }

        private static MechanicsContext CreateParentContext(
            ItemEntity item,
            BlueprintWeaponEnchantment blueprint)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (blueprint == null)
            {
                throw new ArgumentNullException("blueprint");
            }

            UnitDescriptor owner = item.Wielder ?? item.Owner;
            if (owner == null || owner.Unit == null)
            {
                // Inventory-only state tokens remain valid. The ApplyEnchantments
                // Harmony guard restores them if Kingmaker later reconciles the item
                // before a wielder is available.
                return null;
            }

            return new MechanicsContext(
                owner.Unit,
                owner,
                blueprint,
                null,
                new TargetWrapper(owner.Unit));
        }

        private static void RemoveEnchantment(
            object itemInstance,
            object enchantmentInstance)
        {
            object ignored;
            string resolvedMethod;
            if (!ReflectionAccess.TryInvokeAny(
                itemInstance,
                RemoveMethodNames,
                new[] { new[] { enchantmentInstance } },
                out ignored,
                out resolvedMethod))
            {
                throw new MissingMethodException(
                    itemInstance.GetType().FullName,
                    "RemoveEnchantment(ItemEnchantment)");
            }
        }

        private RuntimeTokenRecord RequireSingleRecord(
            object itemInstance,
            string tokenId)
        {
            RuntimeTokenRecord[] matches = ReadRecords(itemInstance)
                .Where(record => string.Equals(
                    record.Definition.TokenId,
                    tokenId,
                    StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Adding firearm-state token '{0}' produced {1} matching runtime enchantments; exactly one is required.",
                        tokenId,
                        matches.Length));
            }

            return matches[0];
        }

        private static void RequireExpected(
            IList<RuntimeTokenRecord> records,
            string expectedTokenId)
        {
            if (expectedTokenId == null)
            {
                if (records.Count != 0)
                {
                    throw new InvalidOperationException(
                        "The item token set changed before replacement; expected no current token.");
                }

                return;
            }

            if (records.Count != 1 ||
                !string.Equals(
                    records[0].Definition.TokenId,
                    expectedTokenId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The item token set changed before replacement or contains duplicate state tokens.");
            }
        }

        private void RestorePreviousBestEffort(
            object itemInstance,
            IList<RuntimeTokenRecord> before,
            string expectedTokenId)
        {
            try
            {
                List<RuntimeTokenRecord> current = ReadRecords(itemInstance);
                RuntimeTokenRecord preferredOriginal = before.FirstOrDefault(
                    record => current.Any(candidate => ReferenceEquals(
                        candidate.Instance,
                        record.Instance)));
                bool keptExpected = false;
                foreach (RuntimeTokenRecord record in current.ToArray())
                {
                    bool isExpected = expectedTokenId != null &&
                        string.Equals(
                            record.Definition.TokenId,
                            expectedTokenId,
                            StringComparison.Ordinal);
                    bool isPreferred = preferredOriginal != null &&
                        ReferenceEquals(record.Instance, preferredOriginal.Instance);
                    bool shouldKeep = isExpected && !keptExpected &&
                        (preferredOriginal == null || isPreferred);
                    if (shouldKeep)
                    {
                        keptExpected = true;
                        continue;
                    }

                    RemoveEnchantment(itemInstance, record.Instance);
                }

                current = ReadRecords(itemInstance);
                if (expectedTokenId != null && current.Count == 0)
                {
                    AddEnchantment(
                        itemInstance,
                        _blueprints.RequireBlueprint(expectedTokenId));
                }

                RequireExpected(ReadRecords(itemInstance), expectedTokenId);
            }
            catch
            {
                // The original failure remains authoritative. Runtime testing must inspect
                // the item after any rollback warning; this adapter never hides the cause.
            }
        }

        private void RequireKnownOrNull(string tokenId, string parameterName)
        {
            if (tokenId != null && !_blueprints.Catalog.ContainsToken(tokenId))
            {
                throw new ArgumentException(
                    "The token ID is not part of the registered Sprint 12 catalog.",
                    parameterName);
            }
        }

        private static void RequireReferenceItem(object itemInstance)
        {
            if (itemInstance == null)
            {
                throw new ArgumentNullException("itemInstance");
            }

            if (itemInstance.GetType().IsValueType)
            {
                throw new ArgumentException(
                    "A runtime item token carrier must be a reference type.",
                    "itemInstance");
            }
        }

        private sealed class RuntimeTokenRecord
        {
            internal RuntimeTokenRecord(
                object instance,
                BlueprintWeaponEnchantment blueprint,
                FirearmStateTokenDefinition definition,
                string collectionMember,
                string blueprintMember)
            {
                Instance = instance ?? throw new ArgumentNullException("instance");
                Blueprint = blueprint ?? throw new ArgumentNullException("blueprint");
                Definition = definition ?? throw new ArgumentNullException("definition");
                CollectionMember = collectionMember;
                BlueprintMember = blueprintMember;
            }

            internal object Instance { get; private set; }

            internal BlueprintWeaponEnchantment Blueprint { get; private set; }

            internal FirearmStateTokenDefinition Definition { get; private set; }

            internal string CollectionMember { get; private set; }

            internal string BlueprintMember { get; private set; }
        }
    }
}
