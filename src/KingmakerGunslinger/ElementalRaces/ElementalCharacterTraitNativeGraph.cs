using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using KingmakerGunslinger.AidAnotherCompatibility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    // Save hydration without an optional host must never turn into acquisition.
    // This closed prerequisite is replaced atomically with the exact host race
    // prerequisite before any of the four features reaches racial_traits.
    [Serializable]
    public sealed class ElementalCharacterTraitHostUnavailable : Prerequisite
    {
        public BlueprintRace ExpectedRace;
        public override bool Check(FeatureSelectionState selection, UnitDescriptor unit, LevelUpState state) { return false; }
        public override string GetUIText() { return "Character traits are unavailable."; }
    }

    internal static class ElementalCharacterTraitNativeGraph
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static DormantElementalCharacterTraitGraph ResolveCanonicalGraphIfFullyRegistered(
            LibraryScriptableObject library, ElementalRaceBlueprintSet races, BlueprintBuff wings)
        {
            var nodes=ElementalCharacterTraitCatalog.Nodes();
            var assets=CharacterTraitCanonicalAdmission.Resolve<BlueprintScriptableObject>(
                n=>{ BlueprintScriptableObject b; return library.BlueprintsByAssetId.TryGetValue(n.Guid,out b)?b:null; },
                (n,value)=>value.AssetGuid==n.Guid && value.name==n.Symbol.Replace('.','_') &&
                    value.GetType().Name==n.BlueprintType &&
                    ReferenceEquals(ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(n.Guid),value) &&
                    library.GetAllBlueprints().Count(b=>ReferenceEquals(b,value))==1);
            if(assets==null) return null;
            var graph=new DormantElementalCharacterTraitGraph();
            for(int i=0;i<nodes.Length;i++) graph.AdoptCanonical(nodes[i],assets[i]);
            foreach(var d in ElementalCharacterTraitCatalog.All()) graph.AddCopy(d);
            Validate(graph,races.OrderedRaces(),wings);
            return graph;
        }

        internal static void Validate(DormantElementalCharacterTraitGraph graph, BlueprintRace[] races, BlueprintBuff wings)
        {
            if(graph==null || graph.Nodes.Length!=11 || races==null || races.Length!=4)
                throw new InvalidOperationException("Incomplete canonical character-trait graph.");
            AerialObserverFlightContract.VerifyCarrier(wings);
            var definitions=ElementalCharacterTraitCatalog.All();
            for(int i=0;i<4;i++)
            {
                var d=definitions[i];var f=graph.Features[i];
                if(f.AssetGuid!=d.Feature.Guid || f.name!=d.Feature.Symbol.Replace('.','_') || f.Ranks!=1 ||
                    f.HideInUI || f.HideInCharacterSheetAndLevelUp || f.IsClassFeature ||
                    f.Groups==null || !f.Groups.SequenceEqual(new[]{FeatureGroup.Trait}) ||
                    f.ComponentsArray==null || f.ComponentsArray.Length!=3 ||
                    f.ComponentsArray[1]?.GetType()!=typeof(PrerequisiteNoFeature) ||
                    f.ComponentsArray[2]?.GetType()!=typeof(ElementalCharacterTraitOwnedGrant) ||
                    !ReferenceEquals(((PrerequisiteNoFeature)f.ComponentsArray[1]).Feature,f) ||
                    !ReferenceEquals(((ElementalCharacterTraitOwnedGrant)f.ComponentsArray[2]).GrantedFact,graph.Resolve(d.GrantedNode.Symbol)))
                    throw new InvalidOperationException("Foreign or incomplete feature graph: "+d.Name);
                var prerequisite=f.ComponentsArray[0] as Prerequisite;
                if(prerequisite==null || prerequisite.Group!=Prerequisite.GroupType.All ||
                    ((PrerequisiteNoFeature)f.ComponentsArray[1]).Group!=Prerequisite.GroupType.All)
                    throw new InvalidOperationException("Prerequisite conjunction is not exact.");
                var unavailable=prerequisite as ElementalCharacterTraitHostUnavailable;
                if(unavailable!=null)
                {
                    if(!ReferenceEquals(unavailable.ExpectedRace,races[i])) throw new InvalidOperationException("Closed race identity mismatch.");
                }
                else
                {
                    Type t=prerequisite.GetType();
                    if(t.FullName!="ZFavoredClass.NewMechanics.PrerequisiteRace" || t.Assembly.GetName().Name!="ZFavoredClass" ||
                        t.GetField("race",Fields)?.FieldType!=typeof(BlueprintRace) ||
                        !ReferenceEquals(t.GetField("race",Fields).GetValue(prerequisite),races[i]))
                        throw new InvalidOperationException("The canonical exact host race prerequisite is absent.");
                }
                ValidateDisplay(f,d);
            }
            var fiery=definitions[0];
            var buff=(BlueprintBuff)graph.Resolve(fiery.Node(CharacterTraitNodeKind.ActivationBuff).Symbol);
            var toggle=(BlueprintActivatableAbility)graph.Resolve(fiery.Node(CharacterTraitNodeKind.Toggle).Symbol);
            var take=buff.GetComponent<Take10ForSuccess>();
            var command=typeof(BlueprintActivatableAbility).GetField("m_ActivateWithUnitCommand",Fields);
            if(buff.ComponentsArray.Length!=1 || take==null || take.Skill!=Kingmaker.EntitySystem.Stats.StatType.CheckIntimidate ||
                !ReferenceEquals(toggle.Buff,buff) || toggle.IsOnByDefault || toggle.ActivationType!=AbilityActivationType.Immediately ||
                command==null || !Equals(command.GetValue(toggle),Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free) ||
                !toggle.DeactivateImmediately || toggle.OnlyInCombat || toggle.ComponentsArray.Length!=0 ||
                toggle.ResourceAssetIds==null || toggle.ResourceAssetIds.Length!=0)
                throw new InvalidOperationException("Fiery Glare graph contract mismatch.");
            ValidateDisplay(toggle,fiery);
            var stoic=definitions[1];
            var provider=(BlueprintBuff)graph.Resolve(stoic.Node(CharacterTraitNodeKind.ProviderBuff).Symbol);
            var recipient=(BlueprintBuff)graph.Resolve(stoic.Node(CharacterTraitNodeKind.RecipientBuff).Symbol);
            var area=(BlueprintAbilityAreaEffect)graph.Resolve(stoic.Node(CharacterTraitNodeKind.Area).Symbol);
            var self=provider.GetComponent<StoicDignitySaveBonus>();
            var emanation=provider.GetComponent<AddAreaEffect>();
            var ally=recipient.GetComponent<StoicDignitySaveBonus>();
            var delivery=area.GetComponent<AbilityAreaEffectBuff>();
            if(provider.ComponentsArray.Length!=2 || self==null || self.AllyRecipient || emanation==null ||
                !ReferenceEquals(emanation.AreaEffect,area) || recipient.ComponentsArray.Length!=1 || ally==null || !ally.AllyRecipient ||
                area.ComponentsArray.Length!=1 || area.Shape!=AreaEffectShape.Cylinder || !area.Size.Equals(10.Feet()) ||
                area.AffectEnemies || area.AggroEnemies || area.AffectDead || area.SpellResistance ||
                delivery==null || !ReferenceEquals(delivery.Buff,recipient) || delivery.Condition?.Conditions==null ||
                delivery.Condition.Operation!=Kingmaker.ElementsSystem.Operation.And ||
                delivery.Condition.Conditions.Length!=2 || delivery.Condition.Conditions[0].Not || delivery.Condition.Conditions[0]?.GetType()!=typeof(ContextConditionIsAlly) ||
                delivery.Condition.Conditions[1]?.GetType()!=typeof(ContextConditionIsCaster) || !delivery.Condition.Conditions[1].Not)
                throw new InvalidOperationException("Stoic Dignity graph contract mismatch.");
            var aerial=(BlueprintBuff)graph.Resolve(definitions[2].GrantedNode.Symbol);
            if(aerial.ComponentsArray.Length!=1 || aerial.GetComponent<AerialObserverPerceptionBonus>()==null ||
                !ReferenceEquals(aerial.GetComponent<AerialObserverPerceptionBonus>().FlightCarrier,wings))
                throw new InvalidOperationException("Aerial Observer carrier mismatch.");
            var weather=(BlueprintBuff)graph.Resolve(definitions[3].GrantedNode.Symbol);
            if(weather.ComponentsArray.Length!=1 || weather.GetComponent<WhiteoutWeatherProvider>()==null ||
                !ReferenceEquals(weather.GetComponent<WhiteoutWeatherProvider>().ProviderIdentity,weather) ||
                WhiteoutNativeContract.AttackMethod(typeof(Kingmaker.RuleSystem.Rules.RuleAttackRoll))==null)
                throw new InvalidOperationException("Whiteout provider/seam mismatch.");
            foreach(var n in ElementalCharacterTraitCatalog.Nodes().Where(n=>!n.Visible))
            {
                var fact=graph.Resolve(n.Symbol) as BlueprintUnitFact;
                if(fact?.Icon!=null) throw new InvalidOperationException("Hidden node has a visible icon.");
                var hidden=fact as BlueprintBuff;
                if(hidden!=null)
                {
                    var flags=typeof(BlueprintBuff).GetField("m_Flags",Fields);
                    var mask=Convert.ToInt32(Enum.Parse(flags.FieldType,"HiddenInUi",false));
                    if((Convert.ToInt32(flags.GetValue(hidden))&mask)==0 || hidden.Stacking!=StackingType.Stack)
                        throw new InvalidOperationException("Hidden provider lifecycle mismatch.");
                }
            }
        }

        private static void ValidateDisplay(BlueprintUnitFact fact, ElementalCharacterTraitDefinition definition)
        {
            if(fact.Icon==null || fact.Icon.name!="KMG_Icon_"+definition.IconKey || fact.Icon.texture?.width!=128 ||
                fact.Icon.texture.height!=128 ||
                (typeof(BlueprintUnitFact).GetField("m_DisplayName",Fields).GetValue(fact) as LocalizedString)?.Key!=definition.NameKey ||
                (typeof(BlueprintUnitFact).GetField("m_Description",Fields).GetValue(fact) as LocalizedString)?.Key!=definition.DescriptionKey)
                throw new InvalidOperationException("Stable icon/localization contract mismatch.");
        }

        internal static void BindExactRacePrerequisites(DormantElementalCharacterTraitGraph graph,
            BlueprintRace[] races, FavoredClassTraitContract host)
        {
            var type=host.Assembly.GetType("ZFavoredClass.NewMechanics.PrerequisiteRace",false,false);
            var field=type?.GetField("race",Fields);
            if(type==null || !typeof(Prerequisite).IsAssignableFrom(type) || field?.FieldType!=typeof(BlueprintRace))
                throw new InvalidOperationException("Exact host race contract absent.");
            var original=graph.Features.Select(f=>f.ComponentsArray).ToArray();
            var next=graph.Features.Select((f,i)=>{
                if(f.ComponentsArray[0].GetType()==type && ReferenceEquals(field.GetValue(f.ComponentsArray[0]),races[i]))
                    return f.ComponentsArray;
                if(f.ComponentsArray[0].GetType()!=typeof(ElementalCharacterTraitHostUnavailable))
                    throw new InvalidOperationException("Foreign race prerequisite cannot be replaced.");
                var race=(Prerequisite)ScriptableObject.CreateInstance(type);
                race.name="$CharacterTraitExactRace";race.Group=Prerequisite.GroupType.All;field.SetValue(race,races[i]);
                return new BlueprintComponent[]{race,f.ComponentsArray[1],f.ComponentsArray[2]};
            }).ToArray();
            try
            {
                for(int i=0;i<4;i++)
                {
                    if(!ReferenceEquals(graph.Features[i].ComponentsArray,original[i]))
                        throw new InvalidOperationException("Feature graph changed before prerequisite commit.");
                }
                for(int i=0;i<4;i++) graph.Features[i].ComponentsArray=next[i];
            }
            catch
            {
                for(int i=0;i<4;i++) if(ReferenceEquals(graph.Features[i].ComponentsArray,next[i])) graph.Features[i].ComponentsArray=original[i];
                throw;
            }
        }
    }

    // Complete preflight precedes any library/localization mutation. Every
    // rollback targets only an exact owned object/key added by this batch.
    internal sealed class ElementalCharacterTraitNativeRegistration
    {
        private readonly LibraryScriptableObject _library;
        private readonly DormantElementalCharacterTraitGraph _graph;
        private readonly BlueprintManifest _manifest;
        private readonly bool _existing;
        private readonly List<KeyValuePair<string,BlueprintScriptableObject>> _added=new List<KeyValuePair<string,BlueprintScriptableObject>>();
        private readonly List<Tuple<LocalizationPack,string,string>> _text=new List<Tuple<LocalizationPack,string,string>>();
        internal ElementalCharacterTraitNativeRegistration(LibraryScriptableObject library, BlueprintManifest manifest,
            DormantElementalCharacterTraitGraph graph, bool existing)
        { _library=library;_manifest=manifest;_graph=graph;_existing=existing;Preflight(); }
        private LocalizationPack[] Packs()
        {
            var flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
            var packs=new[]{"CurrentPack","CurrentPackFast"}.Select(n=>typeof(LocalizationManager).GetField(n,flags)?.GetValue(null) as LocalizationPack)
                .Where(p=>p!=null).Distinct().ToArray();
            if(packs.Length==0 || packs.Any(p=>p.Strings==null)) throw new InvalidOperationException("Current native localization packs unavailable.");
            return packs;
        }
        internal void Preflight()
        {
            if(_library.BlueprintsByAssetId==null || _library.GetAllBlueprints()==null) throw new InvalidOperationException("Native registry absent.");
            foreach(var n in ElementalCharacterTraitCatalog.Nodes())
            {
                var asset=_graph.Resolve(n.Symbol);var identity=_manifest.ResolveActive(n.Symbol,asset.GetType());
                if(identity.Id.Value!=n.Guid) throw new InvalidOperationException("Manifest stable identity mismatch.");
                BlueprintScriptableObject current;
                bool exists=_library.BlueprintsByAssetId.TryGetValue(n.Guid,out current);
                if(_existing ? !exists || !ReferenceEquals(current,asset) : exists)
                    throw new InvalidOperationException("Foreign/partial registry conflict before mutation.");
            }
            foreach(var pack in Packs()) foreach(var copy in _graph.Copy)
            {
                string value;
                if(pack.Strings.TryGetValue(copy.Key,out value) && value!=copy.Value)
                    throw new InvalidOperationException("Foreign localization conflict: "+copy.Key);
            }
        }
        internal void Apply()
        {
            Preflight();
            try
            {
                if(!_existing) foreach(var pair in _graph.Nodes)
                {
                    _library.BlueprintsByAssetId.Add(pair.Value.AssetGuid,pair.Value);
                    _added.Add(pair);
                    _library.GetAllBlueprints().Add(pair.Value);
                }
                foreach(var pack in Packs()) foreach(var copy in _graph.Copy)
                    if(!pack.Strings.ContainsKey(copy.Key))
                    { pack.Strings.Add(copy.Key,copy.Value);_text.Add(Tuple.Create(pack,copy.Key,copy.Value)); }
            }
            catch { Rollback();throw; }
        }
        internal void Rollback()
        {
            for(int i=_text.Count-1;i>=0;i--)
            {
                var text=_text[i];string live;
                if(!text.Item1.Strings.TryGetValue(text.Item2,out live) || live!=text.Item3)
                    throw new InvalidOperationException("Localization rollback refused a foreign mutation.");
                text.Item1.Strings.Remove(text.Item2);_text.RemoveAt(i);
            }
            for(int i=_added.Count-1;i>=0;i--)
            {
                var pair=_added[i];BlueprintScriptableObject live;
                if(!_library.BlueprintsByAssetId.TryGetValue(pair.Value.AssetGuid,out live) || !ReferenceEquals(live,pair.Value))
                    throw new InvalidOperationException("Registry rollback refused a foreign mutation.");
                _library.GetAllBlueprints().Remove(pair.Value);_library.BlueprintsByAssetId.Remove(pair.Value.AssetGuid);_added.RemoveAt(i);
            }
        }
    }
}
