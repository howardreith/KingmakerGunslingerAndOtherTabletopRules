using System;
using System.Collections.Generic;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Process-lifetime composition root for the Sprint 19 item-owned enchantment-token
    /// persistence candidate. Kingmaker 2.1.7b exposes no ItemEntityWeapon.UniqueId,
    /// so the rejected identity-vault carrier is deliberately bypassed. The exact
    /// firearm item carries zero or one inert state-token enchantment.
    /// </summary>
    internal static class FirearmRuntimeState
    {
        private static readonly object Gate = new object();
        private static IFirearmRuntimeItemResolver _resolver;
        private static IFirearmStateRepository _repository;
        private static FirearmItemStateService _service;
        private static IFirearmStateTokenStore _tokenStore;
        private static FirearmStateTokenCatalog _catalog;

        internal static bool IsConfigured
        {
            get
            {
                lock (Gate)
                {
                    return _service != null;
                }
            }
        }

        internal static string CarrierDescription
        {
            get
            {
                return "item-owned inert BlueprintWeaponEnchantment state token";
            }
        }

        internal static IFirearmStateRepository Repository
        {
            get
            {
                lock (Gate)
                {
                    RequireConfigured();
                    return _repository;
                }
            }
        }

        internal static FirearmItemStateService Service
        {
            get
            {
                lock (Gate)
                {
                    RequireConfigured();
                    return _service;
                }
            }
        }

        internal static FirearmStateMigrationSnapshot TokenMigrationSnapshot
        {
            get
            {
                return new FirearmStateMigrationSnapshot(0, 0, 0, 0, 0, 0);
            }
        }

        internal static FirearmStateMigrationSnapshot MigrationSnapshot
        {
            get { return TokenMigrationSnapshot; }
        }

        internal static FirearmStateIdentityMigrationSnapshot IdentityMigrationSnapshot
        {
            get
            {
                return new FirearmStateIdentityMigrationSnapshot(0, 0, 0, 0, 0, 0, 0);
            }
        }

        internal static int VaultRecordCount
        {
            get { return 0; }
        }

        internal static int IdentityVaultRecordCount
        {
            get { return 0; }
        }

        internal static int LegacyReferenceRecordCount
        {
            get { return 0; }
        }

        /// <summary>
        /// Compatibility name retained for existing diagnostics. In Sprint 19 this
        /// answers whether the exact item carries a non-default state token.
        /// </summary>
        internal static bool HasIdentityVaultRecord(object candidate)
        {
            lock (Gate)
            {
                RequireConfigured();
                ResolvedFirearmItem resolved = RequireResolved(candidate);
                return _tokenStore.ReadTokenIds(resolved.ItemInstance).Count != 0;
            }
        }

        internal static IReadOnlyList<string> ReadStateTokenIds(object candidate)
        {
            lock (Gate)
            {
                RequireConfigured();
                if (candidate == null)
                {
                    throw new ArgumentNullException("candidate");
                }

                return _tokenStore.ReadTokenIds(candidate);
            }
        }

        internal static void RestoreMissingStateToken(
            object candidate,
            string tokenId)
        {
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                throw new ArgumentException(
                    "A known state-token ID is required for restoration.",
                    "tokenId");
            }

            lock (Gate)
            {
                RequireConfigured();
                IReadOnlyList<string> current = _tokenStore.ReadTokenIds(candidate);
                if (current.Count != 0)
                {
                    throw new InvalidOperationException(
                        "A missing state token can only be restored when the exact item currently has no known state token.");
                }

                _tokenStore.ReplaceToken(candidate, null, tokenId);
            }
        }

        internal static void Configure(FirearmStateTokenBlueprintSet tokenBlueprints)
        {
            if (tokenBlueprints == null)
            {
                throw new ArgumentNullException("tokenBlueprints");
            }

            lock (Gate)
            {
                if (_service != null || _repository != null)
                {
                    throw new InvalidOperationException(
                        "Firearm runtime persistence was already configured in this process.");
                }

                _resolver = new KingmakerFirearmRuntimeItemResolver();
                _catalog = tokenBlueprints.Catalog;
                _tokenStore = new KingmakerFirearmStateTokenStore(tokenBlueprints);
                _repository = new TokenBackedFirearmStateRepository(
                    _tokenStore,
                    _catalog);
                _service = new FirearmItemStateService(_resolver, _repository);
            }
        }

        /// <summary>
        /// Development-only compatibility fixture. State tokens are now the active
        /// carrier, so this writes the requested canonical state directly.
        /// </summary>
        internal static FirearmState SeedLegacyTokenForDebug(
            object candidate,
            FirearmState legacyState)
        {
            if (legacyState == null)
            {
                throw new ArgumentNullException("legacyState");
            }

            lock (Gate)
            {
                RequireConfigured();
                ResolvedFirearmItem resolved = RequireResolved(candidate);
                _repository.Set(resolved.ItemInstance, legacyState);
                return legacyState;
            }
        }

        internal static FirearmState SeedLegacyReferenceForDebug(
            object candidate,
            FirearmState legacyState)
        {
            throw new NotSupportedException(
                "The Sprint 13 direct-reference migration fixture is retired because Kingmaker 2.1.7b exposes no stable firearm item identity contract.");
        }

        private static ResolvedFirearmItem RequireResolved(object candidate)
        {
            ResolvedFirearmItem resolved;
            string reason;
            if (!_resolver.TryResolve(candidate, out resolved, out reason))
            {
                throw new InvalidOperationException(reason);
            }

            return resolved;
        }

        private static void RequireConfigured()
        {
            if (_service == null ||
                _repository == null ||
                _resolver == null ||
                _tokenStore == null ||
                _catalog == null)
            {
                throw new InvalidOperationException(
                    "Firearm runtime persistence has not been configured by blueprint bootstrap.");
            }
        }
    }
}
