using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.ElementalRaces
{
    // Dormant graph glue, not a main-bootstrap hook or an automatic race grant.
    // Buffs need an explicit holder context: native AddFacts passes null context
    // and cannot supply Stoic's attached area's exact MaybeCaster linkage.
    // Only the recorded owned fact is removed; no remove-by-blueprint sweep.
    [Serializable]
    public sealed class ElementalCharacterTraitOwnedGrant : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintUnitFact GrantedFact;
        [JsonProperty] private Fact _ownedFact;

        internal static void VerifyContract()
        {
            if (typeof(BlueprintUnitFact).GetMethod("GetTargetCollection",new[] { typeof(UnitDescriptor) }) == null ||
                typeof(FactCollection).GetProperty("RawFacts") == null ||
                typeof(Fact).GetProperty("IsDisposed")?.PropertyType != typeof(bool) ||
                typeof(MechanicsContext).GetConstructor(new[] { typeof(Kingmaker.EntitySystem.Entities.UnitEntityData),
                    typeof(UnitDescriptor),typeof(BlueprintScriptableObject),typeof(MechanicsContext),typeof(TargetWrapper) }) == null)
                throw new InvalidOperationException("Exact native owned-grant/context lifecycle absent.");
        }
        public override void OnFactActivate() { Reconcile(); }
        public override void OnTurnOn() { Reconcile(); }
        public override void OnTurnOff() { RemoveOwned(); }
        public override void OnFactDeactivate() { RemoveOwned(); }

        private void Reconcile()
        {
            ModContext context;
            bool enabled=ModContext.TryGet(out context) && context.IsReady && !context.IsFailed &&
                context.ModEntry.Active && context.FeatureModules?.Active?.ElementalRaces == true;
            if (Owner == null || Fact == null || GrantedFact == null) return;
            if (!ElementalCharacterTraitPersistenceContract.MayRebuildOwnedProvider(true,true,enabled))
            { RemoveOwned(); return; }
            var collection=GrantedFact.GetTargetCollection(Owner);
            bool present=_ownedFact != null && collection != null &&
                collection.RawFacts.Any(f => ReferenceEquals(f,_ownedFact));
            if (ElementalCharacterTraitPersistenceContract.MayReuseOwnedFact(_ownedFact,_ownedFact?.Blueprint,
                GrantedFact,_ownedFact == null || _ownedFact.IsDisposed,present)) return;
            RemoveOwned();
            var buff=GrantedFact as BlueprintBuff;
            if (buff != null)
            {
                var source=new MechanicsContext(Owner.Unit,Owner,Fact.Blueprint,null,new TargetWrapper(Owner.Unit));
                _ownedFact=Owner.Buffs.AddBuff(buff,source,null);
            }
            else _ownedFact=Owner.AddFact(GrantedFact);
            if (_ownedFact == null || !ReferenceEquals(_ownedFact.Blueprint,GrantedFact))
                throw new InvalidOperationException("Native engine rejected the exact owned trait grant.");
        }
        private void RemoveOwned()
        {
            var owned=_ownedFact;
            if (owned == null) return;
            if (!owned.IsDisposed && Owner != null) Owner.RemoveFact(owned);
            _ownedFact=null;
        }
    }
}
