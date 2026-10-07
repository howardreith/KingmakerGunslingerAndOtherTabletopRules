using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.AidAnotherCompatibility;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class DormantElementalCharacterTraitGraph : IDisposable
    {
        private readonly Dictionary<string,BlueprintScriptableObject> _nodes =
            new Dictionary<string,BlueprintScriptableObject>(StringComparer.Ordinal);
        private readonly Dictionary<string,string> _copy = new Dictionary<string,string>(StringComparer.Ordinal);
        internal BlueprintScriptableObject Resolve(string symbol) { return _nodes[symbol]; }
        internal KeyValuePair<string,BlueprintScriptableObject>[] Nodes { get { return _nodes.ToArray(); } }
        internal KeyValuePair<string,string>[] Copy { get { return _copy.ToArray(); } }
        internal BlueprintFeature[] Features
        { get { return ElementalCharacterTraitCatalog.All().Select(d => (BlueprintFeature)Resolve(d.Feature.Symbol)).ToArray(); } }
        internal void Add(CharacterTraitNode node, BlueprintScriptableObject asset)
        {
            if (node == null || asset == null || asset.GetType().Name != node.BlueprintType)
                throw new InvalidOperationException("Exact dormant node type mismatch.");
            FieldInfo guid=typeof(BlueprintScriptableObject).GetField("m_AssetGuid",BindingFlags.Instance|BindingFlags.NonPublic);
            if (guid == null || guid.FieldType != typeof(string)) throw new MissingFieldException("BlueprintScriptableObject.m_AssetGuid");
            asset.name=node.Symbol.Replace('.','_');
            guid.SetValue(asset,node.Guid);
            if (asset.AssetGuid != node.Guid) throw new InvalidOperationException("Stable dormant GUID assignment failed.");
            _nodes.Add(node.Symbol,asset);
        }
        internal void AdoptCanonical(CharacterTraitNode node, BlueprintScriptableObject asset)
        {
            if(asset == null || asset.AssetGuid != node.Guid || asset.name != node.Symbol.Replace('.','_') ||
                asset.GetType().Name != node.BlueprintType) throw new InvalidOperationException("Canonical identity mismatch.");
            _nodes.Add(node.Symbol,asset);
        }
        internal void AddCopy(ElementalCharacterTraitDefinition definition)
        {
            _copy.Add(definition.NameKey,definition.Name);
            _copy.Add(definition.DescriptionKey,definition.Description +
                (definition.Supplemental.Length == 0 ? "" : "\n\n" + definition.Supplemental));
        }
        public void Dispose()
        {
            // Detached output only. Never destroy an identity after another stage
            // has registered it; rollback must remove exact registry entries first.
            foreach (var pair in _nodes)
                if (ReferenceEquals(ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(pair.Value.AssetGuid),pair.Value))
                    throw new InvalidOperationException("Registered graph cannot be disposed as detached output.");
            var owned=new HashSet<UnityEngine.Object>();
            foreach (var value in _nodes.Values)
            {
                foreach (var component in value.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                {
                    if (component == null) continue;
                    owned.Add(component);
                    var delivery=component as Kingmaker.UnitLogic.Abilities.Components.AreaEffects.AbilityAreaEffectBuff;
                    if (delivery?.Condition?.Conditions != null)
                        foreach (var condition in delivery.Condition.Conditions) if (condition != null) owned.Add(condition);
                }
                owned.Add(value);
            }
            foreach (var value in owned) UnityEngine.Object.DestroyImmediate(value);
            _nodes.Clear(); _copy.Clear();
        }
    }

    // There is no bootstrap caller and no Register method. All-four ORIGINAL
    // icon admission is first, before native access, Unity allocation or text
    // creation. This source does not register even a localization string.
    internal static class DormantElementalCharacterTraitFactory
    {
        private const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
        internal static DormantElementalCharacterTraitGraph CreateDetached(
            LibraryScriptableObject library, ElementalRaceBlueprintSet races,
            BlueprintBuff canonicalWingsOfAir,
            AidAnotherContractResolution<FavoredClassTraitContract> host,
            bool moduleEnabled, Func<string,Sprite> resolveQualifiedIcon)
        {
            ElementalCharacterTraitAssetGate.Evaluate(ElementalCharacterTraitAssetCatalog.Current()).RequireReady();
            ElementalCharacterTraitCatalog.Validate();
            // Stable identities must hydrate even with acquisition/module/host disabled.
            // A closed native prerequisite prevents acquisition until the exact host is available.
            bool exactHost=host != null && host.IsCompatible;
            if (library?.BlueprintsByAssetId == null || races == null || resolveQualifiedIcon == null)
                throw new ArgumentNullException("canonicalGraphInputs");
            foreach (var node in ElementalCharacterTraitCatalog.Nodes())
                if (library.BlueprintsByAssetId.ContainsKey(node.Guid))
                    throw new InvalidOperationException("Dormant graph GUID conflicts before construction: " + node.Symbol);
            var canonicalRaces=races.OrderedRaces();
            var definitions=ElementalCharacterTraitCatalog.All();
            for (int i=0;i<definitions.Length;i++)
                if (canonicalRaces[i]?.AssetGuid != definitions[i].RaceGuid ||
                    !ReferenceEquals(canonicalRaces[i],ResourcesLibrary.TryGetBlueprint<BlueprintRace>(definitions[i].RaceGuid)))
                    throw new InvalidOperationException("PrerequisiteRace requires the canonical KMG race object.");
            // Kingmaker has no PrerequisiteRace class. The supported Favored Class
            // class derives from native Prerequisite and checks its exact race field.
            Type racePrerequisite=exactHost ? host.Contract.Assembly.GetType("ZFavoredClass.NewMechanics.PrerequisiteRace",false,false) : typeof(ElementalCharacterTraitHostUnavailable);
            FieldInfo raceField=racePrerequisite?.GetField(exactHost ? "race" : "ExpectedRace",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            if (racePrerequisite == null || !typeof(Prerequisite).IsAssignableFrom(racePrerequisite) ||
                racePrerequisite.IsAbstract || raceField?.FieldType != typeof(BlueprintRace))
                throw new InvalidOperationException("Exact Favored Class PrerequisiteRace contract absent.");
            UnpublishedRaceTraitFoundationFactory.VerifyFieryContract(typeof(Kingmaker.Designers.Mechanics.Facts.Take10ForSuccess));
            StoicDignityRuntime.VerifyContract();
            AerialObserverFlightContract.VerifyCarrier(canonicalWingsOfAir);
            if (WhiteoutNativeContract.AttackMethod(typeof(Kingmaker.RuleSystem.Rules.RuleAttackRoll)) == null)
                throw new InvalidOperationException("Qualified native Whiteout seam unavailable.");
            ElementalCharacterTraitOwnedGrant.VerifyContract();
            var icons=definitions.Select(d => resolveQualifiedIcon(d.IconKey)).ToArray();
            for (int i=0;i<icons.Length;i++)
                if (icons[i] == null || icons[i].name != "KMG_Icon_" + definitions[i].IconKey ||
                    icons[i].texture == null || icons[i].texture.width != 128 || icons[i].texture.height != 128)
                    throw new InvalidOperationException("Qualified concept sprite is missing or mismatched.");
            var graph=new DormantElementalCharacterTraitGraph();
            try
            {
                var fiery=UnpublishedRaceTraitFoundationFactory.CreateFieryGlare();
                var fieryDef=definitions[0];
                graph.Add(fieryDef.Node(CharacterTraitNodeKind.ActivationBuff),fiery.Buff);
                graph.Add(fieryDef.Node(CharacterTraitNodeKind.Toggle),fiery.Toggle);
                fiery.Toggle.ActionBarAutoFillIgnored=false;
                Display(fiery.Toggle,fieryDef,icons[0]);
                var stoic=UnpublishedRaceTraitFoundationFactory.CreateStoicDignity();
                graph.Add(definitions[1].Node(CharacterTraitNodeKind.RecipientBuff),stoic.Recipient);
                graph.Add(definitions[1].Node(CharacterTraitNodeKind.Area),stoic.Area);
                graph.Add(definitions[1].Node(CharacterTraitNodeKind.ProviderBuff),stoic.Provider);
                graph.Add(definitions[2].Node(CharacterTraitNodeKind.ProviderBuff),
                    UnpublishedAerialObserverFoundationFactory.Create(canonicalWingsOfAir));
                CreateStableWhiteoutProvider(graph,definitions[3].Node(CharacterTraitNodeKind.ProviderBuff));
                for (int i=0;i<definitions.Length;i++)
                {
                    var d=definitions[i];
                    var feature=ScriptableObject.CreateInstance<BlueprintFeature>();
                    graph.Add(d.Feature,feature);
                    feature.Ranks=1; feature.HideInUI=false; feature.HideInCharacterSheetAndLevelUp=false;
                    feature.IsClassFeature=false; feature.Groups=new[] { FeatureGroup.Trait };
                    var race=(Prerequisite)ScriptableObject.CreateInstance(racePrerequisite);
                    race.name="$CharacterTraitExactRace"; race.Group=Prerequisite.GroupType.All;
                    raceField.SetValue(race,canonicalRaces[i]);
                    if (!ReferenceEquals(raceField.GetValue(race),canonicalRaces[i]))
                        throw new InvalidOperationException("Race prerequisite object identity did not survive assignment.");
                    var noFeature=ScriptableObject.CreateInstance<PrerequisiteNoFeature>();
                    noFeature.name="$CharacterTraitNoDuplicate"; noFeature.Group=Prerequisite.GroupType.All;
                    noFeature.Feature=feature;
                    var grant=ScriptableObject.CreateInstance<ElementalCharacterTraitOwnedGrant>();
                    grant.name="$CharacterTraitOwnedGrant";
                    grant.GrantedFact=(BlueprintUnitFact)graph.Resolve(d.GrantedNode.Symbol);
                    feature.ComponentsArray=new BlueprintComponent[] { race,noFeature,grant };
                    Display(feature,d,icons[i]); graph.AddCopy(d);
                }
                foreach (var pair in graph.Nodes)
                    ElementalComponentIdentity.Prepare(pair.Value);
                ValidateDetached(graph,canonicalRaces,raceField);
                ElementalCharacterTraitNativeGraph.Validate(graph,canonicalRaces,canonicalWingsOfAir);
                return graph;
            }
            catch { graph.Dispose(); throw; }
        }
        private static void CreateStableWhiteoutProvider(DormantElementalCharacterTraitGraph graph, CharacterTraitNode node)
        {
            // The guarded random-GUID factory remains fixture-only. A future
            // persistent provider receives its stable identity before any use.
            var flags=typeof(BlueprintBuff).GetField("m_Flags",PrivateInstance);
            if (!WhiteoutNativeContract.WeatherValid() ||
                flags?.FieldType.FullName != "Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff+Flags" ||
                !Enum.IsDefined(flags.FieldType,"HiddenInUi"))
                throw new InvalidOperationException("Exact hidden weather provider contract absent.");
            var provider=ScriptableObject.CreateInstance<BlueprintBuff>();
            graph.Add(node,provider);
            flags.SetValue(provider,Enum.Parse(flags.FieldType,"HiddenInUi",false));
            provider.Stacking=StackingType.Stack; provider.IsClassFeature=true;
            provider.FxOnStart=new PrefabLink(); provider.FxOnRemove=new PrefabLink();
            provider.ResourceAssetIds=Array.Empty<string>();
            var component=ScriptableObject.CreateInstance<WhiteoutWeatherProvider>();
            component.name="$CharacterTraitWhiteoutWeather"; component.ProviderIdentity=provider;
            provider.ComponentsArray=new BlueprintComponent[] { component };
        }
        private static void Display(BlueprintUnitFact fact, ElementalCharacterTraitDefinition d, Sprite icon)
        {
            var name=new LocalizedString(); var description=new LocalizedString();
            Set(typeof(LocalizedString),name,"m_Key",typeof(string),d.NameKey);
            Set(typeof(LocalizedString),description,"m_Key",typeof(string),d.DescriptionKey);
            Set(typeof(LocalizedString),name,"m_ShouldProcess",typeof(bool),false);
            Set(typeof(LocalizedString),description,"m_ShouldProcess",typeof(bool),false);
            Set(typeof(BlueprintUnitFact),fact,"m_DisplayName",typeof(LocalizedString),name);
            Set(typeof(BlueprintUnitFact),fact,"m_Description",typeof(LocalizedString),description);
            Set(typeof(BlueprintUnitFact),fact,"m_Icon",typeof(Sprite),icon);
            if (!ReferenceEquals(fact.Icon,icon)) throw new InvalidOperationException("Exact dormant icon assignment failed.");
            // Copy is staged separately. No LocalizationManager/Service mutation.
        }
        private static void Set(Type declaring, object value, string member, Type type, object setting)
        {
            var field=declaring.GetField(member,PrivateInstance);
            if (field?.FieldType != type) throw new MissingFieldException(declaring.FullName,member);
            field.SetValue(value,setting);
        }
        internal static void ValidateDetached(DormantElementalCharacterTraitGraph graph,
            BlueprintRace[] races, FieldInfo raceField)
        {
            if (graph.Nodes.Length != 11 || graph.Features.Length != 4)
                throw new InvalidOperationException("All four complete stable graphs are required.");
            var definitions=ElementalCharacterTraitCatalog.All();
            for (int i=0;i<definitions.Length;i++)
            {
                var feature=graph.Features[i]; var d=definitions[i];
                var parts=feature.ComponentsArray;
                if (feature.AssetGuid != d.Feature.Guid || feature.Icon == null || feature.HideInUI ||
                    feature.Ranks != 1 || feature.Groups.Length != 1 || feature.Groups[0] != FeatureGroup.Trait ||
                    parts.Length != 3 || !ReferenceEquals(raceField.GetValue(parts[0]),races[i]) ||
                    !ReferenceEquals(((PrerequisiteNoFeature)parts[1]).Feature,feature) ||
                    !ReferenceEquals(((ElementalCharacterTraitOwnedGrant)parts[2]).GrantedFact,graph.Resolve(d.GrantedNode.Symbol)))
                    throw new InvalidOperationException("Detached feature graph/prerequisites are not exact.");
            }
            var provider=(BlueprintBuff)graph.Resolve(definitions[3].GrantedNode.Symbol);
            if (provider.ComponentsArray.Length != 1 ||
                !ReferenceEquals(provider.GetComponent<WhiteoutWeatherProvider>().ProviderIdentity,provider))
                throw new InvalidOperationException("Stable Whiteout provider must retain the qualified exact backlink.");
        }
    }
}
