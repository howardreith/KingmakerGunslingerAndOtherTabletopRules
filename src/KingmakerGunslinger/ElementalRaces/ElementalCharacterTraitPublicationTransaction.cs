using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.AidAnotherCompatibility;

namespace KingmakerGunslinger.ElementalRaces
{
    // A dormant transaction plan over the existing qualified array transaction.
    // No native registration/selection/localization caller is installed here.
    // Native registry and localization batch adapters must supply exact rollback.
    internal sealed class CharacterTraitPublicationStep
    {
        internal CharacterTraitPublicationStep(Action apply, Action rollback)
        { Apply=apply ?? throw new ArgumentNullException("apply"); Rollback=rollback ?? throw new ArgumentNullException("rollback"); }
        internal Action Apply { get; private set; }
        internal Action Rollback { get; private set; }
    }

    internal sealed class ElementalCharacterTraitPublicationTransaction<T> where T : class
    {
        private readonly HelpfulPublicationTransaction _selection=new HelpfulPublicationTransaction();
        private readonly CharacterTraitPublicationStep[] _steps;
        private readonly Action _revalidate;
        private int _attempted=-1;
        private bool _committed, _closed;

        // Every identity and the complete host/asset graph are checked before a
        // step can run. Failure never registers a prefix of the graph.
        internal ElementalCharacterTraitPublicationTransaction(
            ElementalCharacterTraitPublicationConditions conditions,
            Func<T[]> readFeatures, Func<T[]> readAllFeatures, Action<T[]> writeAllFeatures,
            T[] additions, Func<T,string> identity, Action validateAllIdentities,
            Action revalidate, params CharacterTraitPublicationStep[] steps)
        {
            if (conditions == null) throw new ArgumentNullException("conditions");
            conditions.RequireReady();
            ElementalCharacterTraitCatalog.Validate();
            if (readFeatures == null || readAllFeatures == null || writeAllFeatures == null ||
                identity == null || validateAllIdentities == null || revalidate == null)
                throw new ArgumentNullException("transactionSurface");
            T[] features=readFeatures(), current=readAllFeatures();
            if (features == null || features.Length != 0 || current == null || current.Length == 0)
                throw new InvalidOperationException("The exact Favored Class empty-Features / nonempty-AllFeatures contract is absent.");
            var expected=ElementalCharacterTraitCatalog.All().Select(d => d.Feature.Guid).ToArray();
            if (additions == null || additions.Length != 4 || additions.Any(v => v == null) ||
                !additions.Select(identity).SequenceEqual(expected,StringComparer.Ordinal))
                throw new InvalidOperationException("All four canonical trait features are required in catalog order.");
            additions=(T[])additions.Clone();
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach (T value in current)
            {
                string id=value == null ? null : identity(value);
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id))
                    throw new InvalidOperationException("The foreign racial catalog contains an ambiguous identity.");
                int index=Array.IndexOf(expected,id);
                if (index >= 0 && !ReferenceEquals(value,additions[index]))
                    throw new InvalidOperationException("A trait feature identity conflicts with foreign content.");
            }
            validateAllIdentities();
            _revalidate=() =>
            {
                conditions.RequireReady();
                revalidate();
                validateAllIdentities();
                var liveFeatures=readFeatures(); var live=readAllFeatures();
                if (liveFeatures == null || liveFeatures.Length != 0 || live == null || live.Length == 0)
                    throw new InvalidOperationException("The target selection changed before publication.");
                var seen=new HashSet<string>(StringComparer.Ordinal);
                foreach (var value in live)
                {
                    var id=value == null ? null : identity(value);
                    int index=Array.IndexOf(expected,id);
                    if (string.IsNullOrWhiteSpace(id) || !seen.Add(id) ||
                        (index >= 0 && !ReferenceEquals(value,additions[index])))
                        throw new InvalidOperationException("Selection identity conflict before registration.");
                }
            };
            _steps=steps == null ? Array.Empty<CharacterTraitPublicationStep>() : (CharacterTraitPublicationStep[])steps.Clone();
            if (_steps.Any(s => s == null)) throw new ArgumentException("A publication step cannot be null.");
            for (int i=0;i<additions.Length;i++)
                _selection.Append("elemental-character-trait-" + expected[i],readAllFeatures,
                    writeAllFeatures,additions[i],identity,false);
        }

        internal bool IsCommitted { get { return _committed; } }
        internal void Commit()
        {
            if (_committed) return;
            if (_closed) throw new InvalidOperationException("A closed publication transaction cannot be reused.");
            _revalidate(); // remote host/settings/identities may have changed since preparation
            try
            {
                for (int i=0;i<_steps.Length;i++) { _attempted=i; _steps[i].Apply(); }
                _selection.Commit();
                _committed=true;
            }
            catch (Exception failure)
            {
                var errors=Undo();
                _closed=true;
                if (errors.Count != 0)
                {
                    errors.Insert(0,failure);
                    throw new InvalidOperationException("Publication failed with incomplete rollback.",new AggregateException(errors));
                }
                throw;
            }
        }
        internal void WithdrawAcquisition()
        {
            if (_closed) return;
            // Registration is a save contract after successful admission.
            // Settings only remove this transaction's exact selection entries.
            _selection.Rollback();
            _attempted=-1; _closed=true; _committed=false;
        }
        internal void Rollback()
        {
            if (_closed) return;
            var errors=Undo();
            _closed=errors.Count == 0; _committed=false;
            if (errors.Count != 0)
                throw new InvalidOperationException("Publication rollback was incomplete; foreign state was preserved.",new AggregateException(errors));
        }
        private List<Exception> Undo()
        {
            var errors=new List<Exception>();
            try { _selection.Rollback(); } catch (Exception e) { errors.Add(e); }
            // If selector rollback fails, identities/localization must remain
            // resolvable rather than stranding surviving published references.
            if (errors.Count != 0) return errors;
            for (int i=_attempted;i>=0;i--)
                try { _steps[i].Rollback(); } catch (Exception e) { errors.Add(e); }
            if (errors.Count == 0) _attempted=-1;
            return errors;
        }
    }

    internal static class ElementalCharacterTraitPersistenceContract
    {
        internal const string PolicyId="elemental-character-traits-stable-owned-graphs-v1";
        internal static bool CanRestoreIdentity(string guid)
        { return ElementalCharacterTraitCatalog.Nodes().Any(n => n.Guid == guid); }
        internal static bool MayOffer(bool compatibleHost, bool traitsOn, bool moduleOn, bool originalAssets)
        { return compatibleHost && traitsOn && moduleOn && originalAssets; }
        internal static bool MayRebuildOwnedProvider(bool featurePresent, bool featureActive, bool moduleOn)
        { return featurePresent && featureActive && moduleOn; }
        internal static bool MayReuseOwnedFact(object stored, object storedBlueprint, object canonicalBlueprint,
            bool disposed, bool stillInOwnerCollection)
        {
            return stored != null && canonicalBlueprint != null && ReferenceEquals(storedBlueprint,canonicalBlueprint) &&
                !disposed && stillInOwnerCollection;
        }
        // Stable identities survive removal from the acquisition catalog; module
        // OFF does not delete saved facts or rewrite raw saves. New providers are
        // inactive, respec/removal cleans only the exact recorded owned fact.
        // No serialization/write fixture is invoked by this contract.
    }
}
