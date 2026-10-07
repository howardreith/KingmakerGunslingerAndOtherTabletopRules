using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.FeatureModules;
using Kingmaker.Utility;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunPublishedElementalCharacterTraits()
        {
            if(!_request.ExitAfterCompletion || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Exact no-write disposable working-load required.");
            var checks=new List<RuntimeTestAssertion>();
            var game=Game.Instance;var units=game.State.Units.All.ToArray();
            var areas=game.State.AreaEffects.All.ToArray();var party=game.Player.Party.ToArray();
            var beforeFacts=units.ToDictionary(u=>u,u=>u.Buffs.Enumerable.ToArray());
            var clock=game.Player.GameTime;var paused=game.IsPaused;
            var library=BlueprintBootstrap.Library;
            var definitions=ElementalCharacterTraitCatalog.All();
            var settings=_context.FeatureModules;var initial=settings.Pending;
            var host=ElementalCharacterTraitPublicationCoordinator.ResolveHost();
            if(!host.IsCompatible || !host.Contract.TraitsEnabled || !ElementalCharacterTraitPublicationCoordinator.Published)
                throw new InvalidOperationException("All-four publication unavailable: "+ElementalCharacterTraitPublicationCoordinator.Failure);
            var target=host.Contract.RaceTraits;var graph=ElementalCharacterTraitPublicationCoordinator.Graph;
            var hostMain=host.Contract.Assembly.GetType("ZFavoredClass.Main",false,false);
            var settingsField=hostMain?.GetField("settings",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
            var traitSettings=settingsField?.GetValue(null);
            var traitsProperty=traitSettings?.GetType().GetProperty("enable_traits",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            var traitsBacking=traitSettings?.GetType().GetField("<enable_traits>k__BackingField",BindingFlags.NonPublic|BindingFlags.Instance);
            if(traitSettings==null || traitSettings.GetType().FullName!="ZFavoredClass.Main+Settings" ||
                settingsField.IsInitOnly || settingsField.FieldType!=traitSettings.GetType() ||
                traitsProperty==null || traitsProperty.PropertyType!=typeof(bool) || !traitsProperty.CanRead ||
                traitsProperty.CanWrite || traitsProperty.GetIndexParameters().Length!=0 ||
                traitsBacking==null || traitsBacking.FieldType!=typeof(bool) || !traitsBacking.IsInitOnly)
                throw new InvalidOperationException("Exact immutable native host settings contract unavailable.");
            bool originalTraits=(bool)traitsProperty.GetValue(traitSettings,null);
            // The supported host reads immutable settings once in its constructor.
            // No live GUI setter exists. Model the loaded OFF state with a private
            // request-local shallow copy; never mutate the original object/file.
            var memberwise=typeof(object).GetMethod("MemberwiseClone",BindingFlags.Instance|BindingFlags.NonPublic);
            var disabledSettings=memberwise.Invoke(traitSettings,null);
            traitsBacking.SetValue(disabledSettings,false);
            if((bool)traitsProperty.GetValue(disabledSettings,null) ||
                (bool)traitsProperty.GetValue(traitSettings,null)!=originalTraits)
                throw new InvalidOperationException("Request-local native settings copy failed exact admission.");
            Action<bool> traits=enabled=>{
                var live=settingsField.GetValue(null);
                if(!ReferenceEquals(live,traitSettings) && !ReferenceEquals(live,disabledSettings))
                    throw new InvalidOperationException("Foreign host settings replacement during owned fixture.");
                settingsField.SetValue(null,enabled?traitSettings:disabledSettings);
                if(!ElementalCharacterTraitPublicationCoordinator.Reconcile("qualification-exact-loaded-host-setting-"+enabled))
                    throw new InvalidOperationException("Native loaded traits-setting reconciliation failed.");
            };
            var initialEntries=target.AllFeatures.ToArray();
            var foreign=initialEntries.Where(f=>!graph.Features.Contains(f)).ToArray();
            var actors=new List<UnitEntityData>();var prototypes=new List<BlueprintUnit>();
            var detached=new List<UnitEntityData>();
            string failure=null;
            Action<bool> module=enabled=>settings.SetPending(initial.Gunslinger,initial.AcadamaeGraduate,initial.ShieldOther,
                initial.ExpandedSummoning,initial.ElvenBranchedSpears,initial.EasternWeapons,initial.BrownFurTransmuter,
                initial.UrbanBarbarian,initial.BodyguardFeats,initial.ProtectionFromAlignmentControlImmunity,
                enabled,initial.TeleportationSpells,initial.MagicCircleSpells);
            try
            {
                game.IsPaused=true;
                TraitCheck(checks,"published-eleven-canonical",ElementalCharacterTraitCatalog.Nodes().All(n=>
                    ReferenceEquals(graph.Resolve(n.Symbol),library.BlueprintsByAssetId[n.Guid])),"eleven canonical manifest-owned resolutions");
                TraitCheck(checks,"published-four-only-racial-traits",graph.Features.All(f=>target.AllFeatures.Count(x=>ReferenceEquals(x,f))==1) &&
                    ElementalCharacterTraitCatalog.Nodes().Where(n=>!n.Visible).All(n=>!target.AllFeatures.Any(f=>f.AssetGuid==n.Guid)),
                    "correct Favored Class selection; no hidden choice");
                for(int visit=0;visit<4;visit++)
                {
                    var unit=FavoredClassLevelUpHarness.CreateUnit(12);detached.Add(unit);
                    var race=BlueprintBootstrap.ElementalRaces.OrderedRaces()[visit];
                    var rogue=library.GetAllBlueprints().OfType<BlueprintCharacterClass>().Single(c=>c.AssetGuid==FcbRogueClassGuid);
                    LevelUpController controller=null;
                    try
                    {
                        controller=FavoredClassLevelUpHarness.Open(unit.Descriptor,race,rogue,"KMG trait publication qualification");
                        for(int trait=0;trait<4;trait++)
                        {
                            var feature=graph.Features[trait];
                            bool allowed=feature.ComponentsArray.OfType<Prerequisite>().All(p=>p.Check(null,controller.Preview,controller.State));
                            TraitCheck(checks,"published-race-"+visit+"-trait-"+trait,allowed==(visit==trait),"exact native host PrerequisiteRace and native duplicate prerequisite");
                        }
                        var racial=FavoredClassLevelUpHarness.FindOpenState(controller,target.AssetGuid);
                        if(racial==null)
                        {
                            var root=FavoredClassLevelUpHarness.FindOpenState(controller,host.Contract.FirstTrait.AssetGuid) ??
                                FavoredClassLevelUpHarness.FindOpenState(controller,host.Contract.SecondTrait.AssetGuid);
                            if(root!=null && FavoredClassLevelUpHarness.Select(controller,root,target))
                                racial=FavoredClassLevelUpHarness.FindOpenState(controller,target.AssetGuid);
                        }
                        TraitCheck(checks,"published-native-character-selection-"+visit,racial!=null &&
                            FavoredClassLevelUpHarness.CanSelect(controller,racial,graph.Features[visit]) &&
                            FavoredClassLevelUpHarness.Select(controller,racial,graph.Features[visit]),
                            "native level-one trait state and SelectFeature; preview only, no permanent player commit");
                        var duplicate=graph.Features[visit].GetComponent<PrerequisiteNoFeature>();
                        TraitCheck(checks,"published-duplicate-"+visit,!duplicate.Check(null,controller.Preview,controller.State),
                            "selected preview owns the stable feature; native duplicate prerequisite rejects it");
                    }
                    finally { FavoredClassLevelUpHarness.Close(controller);unit.Descriptor.Dispose();detached.Remove(unit); }
                    var featureVisible=graph.Features[visit];
                    TraitCheck(checks,"published-ui-resolution-"+visit,featureVisible.Name==definitions[visit].Name &&
                        !string.IsNullOrWhiteSpace(featureVisible.Description) && featureVisible.Description.Contains(definitions[visit].Description) &&
                        featureVisible.Icon!=null && featureVisible.Icon.name=="KMG_Icon_"+definitions[visit].IconKey &&
                        featureVisible.Icon.texture.width==128 && featureVisible.Icon.texture.height==128,
                        "localized name/description and exact 128 sprite; aesthetic approval not recorded");
                }
                var anchor=party.First(u=>u.IsInGame && u.View!=null);
                var owner=CircleSpawn("PublishedTraitOwnedGrants",anchor.Position,anchor,actors,prototypes);
                var features=definitions.Select(d=>TraitFeatureGrant(owner,d.Id)).ToArray();
                var originalGrants=features.Select(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact).ToArray();
                var aerial=(BlueprintBuff)graph.Resolve(definitions[2].GrantedNode.Symbol);
                var foreignFact=AerialBuff(owner,aerial);
                var originalToggle=(ActivatableAbility)originalGrants[0];
                var fieryBuff=(BlueprintBuff)graph.Resolve(definitions[0].Node(CharacterTraitNodeKind.ActivationBuff).Symbol);
                originalToggle.IsOn=true;
                TraitCheck(checks,"published-active-toggle-control",owner.Buffs.Enumerable.Count(b=>ReferenceEquals(b.Blueprint,fieryBuff))==1,
                    "published toggle owns one exact activation buff before any settings change");
                traits(false);
                TraitCheck(checks,"published-traits-off-acquisition-only",!ElementalCharacterTraitPublicationCoordinator.Published &&
                    target.AllFeatures.SequenceEqual(foreign) &&
                    features.Select(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact).SequenceEqual(originalGrants) &&
                    originalToggle.IsOn && !foreignFact.IsDisposed &&
                    ElementalCharacterTraitCatalog.Nodes().All(n=>ReferenceEquals(graph.Resolve(n.Symbol),library.BlueprintsByAssetId[n.Guid])),
                    "exact immutable native getter on request-local OFF settings copy; no original object/file mutation or GUI/save callback; existing mechanics remain");
                traits(true);
                TraitCheck(checks,"published-traits-on-canonical-reuse",ElementalCharacterTraitPublicationCoordinator.Published &&
                    target.AllFeatures.SequenceEqual(initialEntries) &&
                    features.Select(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact).SequenceEqual(originalGrants),
                    "all-four acquisition restored with exact original canonical graph and grants");
                module(false);
                TraitCheck(checks,"published-module-off-immediate",!ElementalCharacterTraitPublicationCoordinator.Published &&
                    features.All(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact==null) &&
                    originalGrants.All(f=>f.IsDisposed) && !foreignFact.IsDisposed &&
                    !owner.Buffs.Enumerable.Any(b=>ReferenceEquals(b.Blueprint,fieryBuff)) &&
                    foreign.SequenceEqual(target.AllFeatures) && definitions.All(d=>library.BlueprintsByAssetId.ContainsKey(d.Feature.Guid)),
                    "exact notification withdraws only owned acquisition/providers; stable IDs and foreign grant remain");
                module(true);
                TraitCheck(checks,"published-module-on-one",ElementalCharacterTraitPublicationCoordinator.Published &&
                    features.All(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact!=null) &&
                    target.AllFeatures.SequenceEqual(initialEntries) && !foreignFact.IsDisposed &&
                    !((ActivatableAbility)features[0].SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact).IsOn &&
                    !owner.Buffs.Enumerable.Any(b=>ReferenceEquals(b.Blueprint,fieryBuff)),
                    "one rebuilt owned grant per feature; Fiery begins off again; canonical graph reused and foreign facts preserved");
                ElementalCharacterTraitPublicationCoordinator.Reconcile("qualification-repeated-initialization");
                TraitCheck(checks,"published-repeated-initialization",target.AllFeatures.SequenceEqual(initialEntries) &&
                    features.All(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Count()==1),
                    "no repeated registration/grant/selection");
                foreach(var fact in features) owner.Descriptor.RemoveFact(fact);
                TraitCheck(checks,"published-feature-removal-exact",features.All(f=>f.IsDisposed) && !foreignFact.IsDisposed,
                    "remove feature through exact native fact collection; its foreign same-blueprint provider remains");
                foreignFact.Remove();
            }
            catch(Exception error){failure=error.ToString();}
            finally
            {
                try { traits(originalTraits); } catch(Exception cleanup) { failure=(failure??String.Empty)+"\nHost setting restoration: "+cleanup; }
                try { module(initial.ElementalRaces); } catch(Exception cleanup) { failure=(failure??String.Empty)+"\nModule restoration: "+cleanup; }
                foreach(var actor in actors) if(actor!=null && !actor.Destroyed) actor.Destroy();
                game.EntityDestroyer.Tick();game.EntityDestroyer.Tick();
                foreach(var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                foreach(var unit in detached) if(unit!=null) unit.Descriptor.Dispose();
                game.IsPaused=paused;
            }
            TraitCheck(checks,"published-fixture-cleanup",units.SequenceEqual(game.State.Units.All) &&
                areas.SequenceEqual(game.State.AreaEffects.All) && party.SequenceEqual(game.Player.Party) &&
                beforeFacts.All(p=>p.Key.Buffs.Enumerable.SequenceEqual(p.Value)) && clock==game.Player.GameTime &&
                initialEntries.SequenceEqual(target.AllFeatures) && ReferenceEquals(settingsField.GetValue(null),traitSettings) &&
                (bool)traitsProperty.GetValue(traitSettings,null)==originalTraits &&
                !_workingSaveSmoke.WriteObserved,
                "exact preexisting scene/party/facts/time/selection; no save writes; request-local preview and actors removed");
            if(failure!=null) TraitCheck(checks,"published-native-execution",false,failure);
            var result=CreateResult(failure==null && checks.All(c=>c.Status=="PASS")?"PASS":"FAIL",checks,failure);
            result.Diagnostics.Add("FavoredClassEnableTraits=IMMUTABLE_LOADED_SETTING; OFF control=request-local MemberwiseClone with exact getter/backing-field contract; original settings object and file untouched; no live host GUI transition claimed.");
            result.WorkingSaveSmoke=_workingSaveSmoke.Stop();return result;
        }
    }
}
