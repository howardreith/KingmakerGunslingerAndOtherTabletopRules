using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Visual.LocalMap;
using Kingmaker.Visual.WeatherSystem;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Enchantments;
using KingmakerGunslinger.RuntimeTesting;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class WhiteoutNativeContract
    {
        internal const string NativeMvid="07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";
        internal static bool WeatherValid()
        {
            try
            {
                return typeof(Player).Module.ModuleVersionId==new Guid(NativeMvid) &&
                    typeof(Player).GetField("Weather")?.FieldType==typeof(Player.WeatherData) &&
                    typeof(Player.WeatherData).GetProperty("ActualWeather")?.PropertyType==typeof(InclemencyType) &&
                    typeof(WeatherSystemBehaviour).GetField("WeatherType")?.FieldType==typeof(WeatherType) &&
                    typeof(WeatherSystemBehaviour).GetField("RainIntensity")?.FieldType==typeof(float) &&
                    typeof(WeatherSystemBehaviour).GetField("SnowIntensity")?.FieldType==typeof(float) &&
                    typeof(LocalMapArea).GetProperty("AreaPart")?.PropertyType==typeof(BlueprintAreaPart) &&
                    typeof(LocalMapArea).GetMethod("GetClosest",new[]{typeof(Vector3)})?.ReturnType==typeof(LocalMapArea) &&
                    typeof(LocalMapArea).GetMethod("IsIndoor",new[]{typeof(Vector3)})?.ReturnType==typeof(bool);
            }
            catch(Exception) { return false; }
        }
        internal static MethodInfo AttackMethod(Type ruleType)
        {
            try
            {
                if(ruleType!=typeof(RuleAttackRoll) || ruleType.Module.ModuleVersionId!=new Guid(NativeMvid) || !WeatherValid()) return null;
                var method=ruleType.GetMethod("TryOvercomeTargetConcealmentAndMissChance",BindingFlags.Instance|BindingFlags.NonPublic,null,Type.EmptyTypes,null);
                var resolver=typeof(SeekingExactItemResolver).GetMethod("IsAuthorized",BindingFlags.Static|BindingFlags.NonPublic,null,new[]{typeof(ItemEntityWeapon)},null);
                if(method==null || method.DeclaringType!=ruleType || method.IsStatic || method.ReturnType!=typeof(bool) ||
                    method.GetParameters().Length!=0 || method.GetMethodBody()?.GetILAsByteArray()?.Length!=149 ||
                    ruleType.GetField("Initiator")?.FieldType!=typeof(UnitEntityData) ||
                    ruleType.GetField("Target")?.FieldType!=typeof(UnitEntityData) ||
                    ruleType.GetProperty("Weapon")?.PropertyType!=typeof(ItemEntityWeapon) ||
                    ruleType.GetProperty("ConcealmentCheck")?.PropertyType!=typeof(RuleConcealmentCheck) ||
                    ruleType.GetProperty("IgnoreConcealment")?.PropertyType!=typeof(bool) ||
                    ruleType.GetProperty("MissChance")?.PropertyType!=typeof(int) || resolver?.ReturnType!=typeof(bool)) return null;
                return method;
            }
            catch(Exception) { return null; }
        }
        internal static bool ModuleEnabled()
        {
            ModContext context;
            return ModContext.TryGet(out context) && context.IsReady && !context.IsFailed &&
                context.ModEntry.Active && context.FeatureModules?.Active?.ElementalRaces==true;
        }
    }

    // The only construction caller is the closed guarded fixture. Random identity,
    // unregistered, hidden, no icon/localization or ordinary acquisition.
    internal static class UnpublishedWhiteoutFoundationFactory
    {
        internal static BlueprintBuff Create()
        {
            if(!WhiteoutNativeContract.WeatherValid()) throw new InvalidOperationException("Exact Whiteout native weather contract absent.");
            var flags=typeof(BlueprintBuff).GetField("m_Flags",BindingFlags.Instance|BindingFlags.NonPublic);
            var guid=typeof(BlueprintScriptableObject).GetField("m_AssetGuid",BindingFlags.Instance|BindingFlags.NonPublic);
            if(flags==null || flags.FieldType.FullName!="Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff+Flags" ||
                !Enum.IsDefined(flags.FieldType,"HiddenInUi") || guid?.FieldType!=typeof(string))
                throw new InvalidOperationException("Exact hidden transient blueprint contract absent.");
            var provider=ScriptableObject.CreateInstance<BlueprintBuff>();
            provider.name="KMG_Unpublished_Whiteout_Provider";
            guid.SetValue(provider,Guid.NewGuid().ToString("N"));
            flags.SetValue(provider,Enum.Parse(flags.FieldType,"HiddenInUi",false));
            provider.Stacking=StackingType.Stack; provider.IsClassFeature=true;
            provider.FxOnStart=new PrefabLink(); provider.FxOnRemove=new PrefabLink();
            provider.ResourceAssetIds=Array.Empty<string>();
            var component=ScriptableObject.CreateInstance<WhiteoutWeatherProvider>();
            component.name="$UnpublishedWhiteoutWeather"; component.ProviderIdentity=provider;
            provider.ComponentsArray=new BlueprintComponent[]{component};
            return provider;
        }
    }

    [Serializable]
    public sealed class WhiteoutWeatherProvider : OwnedGameLogicComponent<UnitDescriptor>, IWeatherChangeHandler, ISceneHandler
    {
        public BlueprintBuff ProviderIdentity;
        [NonSerialized] private bool _enabled;
        [NonSerialized] private WhiteoutWeatherState _state;
        [NonSerialized] private bool _areaUnloading;
        [NonSerialized] internal int WeatherEventCount;
        internal WhiteoutWeatherState State { get { return _state ?? (_state=new WhiteoutWeatherState()); } }
        public override void OnTurnOn() { _enabled=true; _areaUnloading=false; Reconcile(true); }
        public override void OnTurnOff() { _enabled=false; State.OnMarkerRemoved(); }
        public override void OnFactDeactivate() { _enabled=false; State.OnMarkerRemoved(); }
        public void OnWeatherChange() { WeatherEventCount++; Reconcile(); }
        public void OnAreaBeginUnloading() { _areaUnloading=true; State.OnAreaUnloading(); }
        public void OnAreaDidLoad() { _areaUnloading=false; Reconcile(); }
        internal bool HasExactIdentity()
        {
            var buff=Fact as Buff;
            return _enabled && Owner?.Unit!=null && buff!=null && ProviderIdentity!=null &&
                ReferenceEquals(buff.Blueprint,ProviderIdentity) && ProviderIdentity.ComponentsArray?.Length==1 &&
                ProviderIdentity.GetComponent<WhiteoutWeatherProvider>()?.ProviderIdentity==ProviderIdentity;
        }
        internal bool Reconcile(bool activating=false)
        {
            try
            {
                var game=Game.Instance; var visual=WeatherSystemBehaviour.Instance; var buff=Fact as Buff;
                bool identity=HasExactIdentity() && (activating || buff.Active) && !buff.IsSuppressed;
                var unit=Owner?.Unit;
                bool area=!_areaUnloading && game?.CurrentlyLoadedArea!=null && unit!=null && unit.IsInGame &&
                    !Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance.IsLoadingInProcess;
                var map=area ? LocalMapArea.GetClosest(unit.Position) : null;
                bool known=map?.AreaPart!=null;
                bool indoors=!known || map.AreaPart.IsIndoor;
                if(known && LocalMapArea.IsIndoor(unit.Position)!=indoors) known=false;
                var precipitation=visual==null ? WhiteoutPrecipitation.Normal :
                    visual.WeatherType==WeatherType.Rain ? WhiteoutPrecipitation.Rain :
                    visual.WeatherType==WeatherType.Snow ? WhiteoutPrecipitation.Snow : WhiteoutPrecipitation.Normal;
                int actual=game?.Player?.Weather==null ? -1 : (int)game.Player.Weather.ActualWeather;
                bool valid=WhiteoutFoundationPolicy.EnvironmentActive(identity,known,indoors,precipitation,actual,
                    WhiteoutNativeContract.ModuleEnabled(),WhiteoutNativeContract.WeatherValid() && WhiteoutAttackStagePatch.ContractValid,area);
                State.Reconcile(valid,precipitation,actual,indoors,true,area);
                return State.Active;
            }
            catch(Exception) { State.OnAreaUnloading(); return false; }
        }
    }

    internal static class WhiteoutAttackRuntime
    {
        // Weak attack keys, never a static unit dictionary. Values hold only Whiteout
        // booleans/rolls, not a native result, target, owner, weapon or context.
        private static readonly ConditionalWeakTable<RuleAttackRoll,WhiteoutNativeStageDecision> Decisions=new ConditionalWeakTable<RuleAttackRoll,WhiteoutNativeStageDecision>();
        internal static void Forget(RuleAttackRoll attack) { if(attack!=null) Decisions.Remove(attack); }
        internal static bool HasDecision(RuleAttackRoll attack) { WhiteoutNativeStageDecision value; return attack!=null && Decisions.TryGetValue(attack,out value); }
        internal static void AfterNative(RuleAttackRoll attack,ref bool result)
        {
            bool native=result;
            try
            {
                bool valid=WhiteoutAttackStagePatch.ContractValid;
                var provider=attack?.Target?.Buffs?.Enumerable.Where(b=>b.Active && !b.IsSuppressed)
                    .SelectMany(b=>b.SelectComponents<WhiteoutWeatherProvider>())
                    .FirstOrDefault(p=>p.HasExactIdentity());
                bool active=provider!=null && provider.Reconcile();
                bool ignore=attack!=null && attack.IgnoreConcealment;
                bool seeking=attack!=null && SeekingExactItemResolver.IsAuthorized(attack.Weapon);
                bool eligible=WhiteoutPolicy.ShouldRoll(active,true,native,ignore,seeking,
                    WhiteoutNativeContract.ModuleEnabled(),valid) && attack?.Initiator!=null && attack.Target!=null;
                WhiteoutNativeStageDecision decision=null;
                bool resolved=native;
                // Native false is never cached/replaced. Unmarked ordinary attacks
                // allocate no Whiteout state and use no extra RNG.
                if(native && provider!=null && valid && attack.Initiator!=null)
                {
                    decision=Decisions.GetValue(attack,key=>new WhiteoutNativeStageDecision());
                    resolved=decision.AfterNative(native,eligible,()=>WhiteoutGuardedDiagnostics.RollOrNative(attack));
                }
                WhiteoutGuardedDiagnostics.Record(attack,native,resolved,active,ignore,seeking,decision,null);
                result=resolved;
            }
            catch(Exception error)
            {
                result=native;
                WhiteoutGuardedDiagnostics.RecordFailure(attack,native,error);
            }
        }
    }
    [HarmonyPatch]
    internal static class WhiteoutAttackStagePatch
    {
        private static MethodInfo _method;
        internal static bool ContractValid { get { return _method!=null; } }
        private static bool Prepare() { _method=WhiteoutNativeContract.AttackMethod(typeof(RuleAttackRoll)); return _method!=null; }
        private static MethodBase TargetMethod() { return _method; }
        private static void Postfix(RuleAttackRoll __instance,ref bool __result) { WhiteoutAttackRuntime.AfterNative(__instance,ref __result); }
    }
}
